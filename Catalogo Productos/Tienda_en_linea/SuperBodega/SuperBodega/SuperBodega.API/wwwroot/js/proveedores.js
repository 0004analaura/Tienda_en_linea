let proveedorOriginal = null;

document.addEventListener('DOMContentLoaded', function() {
    inicializarCampoFechaRegistro();
    
    const currentPath = window.location.pathname;

    if (currentPath.includes('/Proveedores/Create')) {
        configurarFormularioCreacion();
    } 
    else if (currentPath.includes('/Proveedores/Edit/')) {
        configurarFormularioEdicion();
    }
    else if (currentPath.includes('/Proveedores/Index')) {
        configurarPaginaListado();
    }
});

function formatearFechaAmPm(fechaStr) {
    if (!fechaStr) return '';

    const fecha = new Date(fechaStr);

    if (isNaN(fecha.getTime())) return 'Fecha inválida';
    
    const dia = fecha.getDate().toString().padStart(2, '0');
    const mes = (fecha.getMonth() + 1).toString().padStart(2, '0');
    const año = fecha.getFullYear();

    let horas = fecha.getHours();
    const minutos = fecha.getMinutes().toString().padStart(2, '0');
    const ampm = horas >= 12 ? 'p.m.' : 'a.m.';
    horas = horas % 12;
    horas = horas ? horas : 12; // La hora '0' debe mostrarse como '12'
    const horasStr = horas.toString().padStart(2, '0');

    return `${dia}/${mes}/${año} ${horasStr}:${minutos} ${ampm}`;
}

function cargarProveedores() {
    const tablaBody = document.querySelector('#proveedoresTable tbody');
    if (tablaBody) {
        tablaBody.innerHTML = '<tr><td colspan="8" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></td></tr>';
    }

    fetch('/api/Proveedor')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al cargar proveedores');
            }
            return response.json();
        })
        .then(data => {
            if (tablaBody) {
                if (data.length === 0) {
                    tablaBody.innerHTML = '<tr><td colspan="8" class="text-center">No hay proveedores registrados</td></tr>';
                    return;
                }

                tablaBody.innerHTML = '';

                data.forEach(proveedor => {
                    const row = document.createElement('tr');
                    const fechaFormateada = formatearFechaAmPm(proveedor.fechaDeRegistro);

                    row.innerHTML = `
                        <td>${proveedor.id}</td>
                        <td>${proveedor.nombre}</td>
                        <td>${proveedor.email}</td>
                        <td>${proveedor.telefono}</td>
                        <td>${proveedor.direccion}</td>
                        <td>
                            <span class="badge ${proveedor.estado ? 'bg-success' : 'bg-danger'}">
                                ${proveedor.estado ? 'Activo' : 'Inactivo'}
                            </span>
                        </td>
                        <td>${fechaFormateada}</td>
                        <td>
                            <a href="/Proveedores/Edit/${proveedor.id}" class="btn btn-sm btn-primary me-1">
                                <i class="fas fa-edit"></i>
                            </a>
                            <button class="btn btn-sm btn-danger btn-eliminar-proveedor" data-id="${proveedor.id}">
                                <i class="fas fa-trash"></i>
                            </button>
                        </td>
                    `;

                    tablaBody.appendChild(row);
                });

                configurarBotonesEliminar();
            }
        })
        .catch(error => {
            console.error('Error:', error);
            if (tablaBody) {
                tablaBody.innerHTML = `<tr><td colspan="8" class="text-center text-danger">Error al cargar datos: ${error.message}</td></tr>`;
            }
        });
}

function configurarBotonesEliminar() {
    document.querySelectorAll('.btn-eliminar-proveedor').forEach(boton => {
        boton.addEventListener('click', function() {
            const id = this.getAttribute('data-id');
            eliminarProveedor(id);
        });
    });
}

function configurarCampoTelefono() {
    const telefonoInput = document.getElementById('telefono');
    if (telefonoInput) {
        telefonoInput.addEventListener('keypress', function(e) {
            const charCode = (e.which) ? e.which : e.keyCode;

            if (charCode > 31 && (charCode < 48 || charCode > 57)) {
                e.preventDefault();
                return false;
            }
            return true;
        });

        telefonoInput.addEventListener('paste', function(e) {
            e.preventDefault();

            const pastedText = (e.clipboardData || window.clipboardData).getData('text');

            const filteredText = pastedText.replace(/[^0-9]/g, '');

            if (document.execCommand) {
                document.execCommand('insertText', false, filteredText);
            } else {
                this.value = this.value.substring(0, this.selectionStart) +
                    filteredText +
                    this.value.substring(this.selectionEnd);
            }
        });

        telefonoInput.addEventListener('input', function() {
            this.value = this.value.replace(/[^0-9]/g, '');
        });
    }
}

function inicializarCampoFechaRegistro() {
    const fechaInput = document.getElementById('fechaDeRegistro');
    const fechaVisibleInput = document.getElementById('fechaDeRegistroVisible');
    
    if (fechaInput && fechaVisibleInput) {
        if (window.location.pathname.includes('/Proveedores/Edit/')) {
            if (fechaInput.value) {
                fechaVisibleInput.value = formatearFechaAmPm(fechaInput.value);
            }
        } else {
            const now = new Date();

            const año = now.getFullYear();
            const mes = (now.getMonth() + 1).toString().padStart(2, '0');
            const dia = now.getDate().toString().padStart(2, '0');
            const horas = now.getHours().toString().padStart(2, '0');
            const minutos = now.getMinutes().toString().padStart(2, '0');

            fechaInput.value = `${año}-${mes}-${dia}T${horas}:${minutos}`;

            fechaVisibleInput.value = formatearFechaAmPm(now);
        }
    }
}

function configurarFormularioCreacion() {
    const formulario = document.getElementById('formCrearProveedor');
    if (formulario) {
        formulario.addEventListener('submit', function(e) {
            e.preventDefault();
            crearProveedor();
        });
        
        configurarCampoTelefono();
    }
}

function verificarDuplicados(nombre, email, telefono, id = null) {
    return fetch('/api/Proveedor')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al verificar datos duplicados');
            }
            return response.json();
        })
        .then(proveedores => {
            const proveedoresFiltrados = id
                ? proveedores.filter(p => p.id != id)
                : proveedores;

            let duplicados = {
                nombre: proveedoresFiltrados.some(p => p.nombre.toLowerCase() === nombre.toLowerCase()),
                email: proveedoresFiltrados.some(p => p.email && p.email.toLowerCase() === email.toLowerCase()),
                telefono: proveedoresFiltrados.some(p => p.telefono && p.telefono === telefono)
            };

            return duplicados;
        })
        .catch(error => {
            console.error('Error al verificar duplicados:', error);
            return { nombre: false, email: false, telefono: false };
        });
}

function crearProveedor() {
    const formulario = document.getElementById('formCrearProveedor');
    if (!formulario) return;

    if (!validarFormularioProveedor()) {
        return;
    }

    const nombre = document.getElementById('nombre').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();

    verificarDuplicados(nombre, email, telefono)
        .then(duplicados => {
            let hayDuplicados = false;

            if (duplicados.nombre) {
                document.getElementById('nombre').classList.add('is-invalid');
                mostrarAlerta('Ya existe un proveedor con este nombre', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('nombre').classList.remove('is-invalid');
            }

            if (duplicados.email) {
                document.getElementById('email').classList.add('is-invalid');
                mostrarAlerta('Este email ya está registrado para otro proveedor', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('email').classList.remove('is-invalid');
            }

            if (duplicados.telefono) {
                document.getElementById('telefono').classList.add('is-invalid');
                mostrarAlerta('Este teléfono ya está registrado para otro proveedor', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('telefono').classList.remove('is-invalid');
            }

            if (!hayDuplicados) {
                enviarFormularioProveedor();
            }
        });
}

function validarFormularioProveedor() {
    const nombre = document.getElementById('nombre').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();
    const direccion = document.getElementById('direccion').value.trim();
    const estado = document.getElementById('estado').value === 'true';
    const fechaDeRegistro = document.getElementById('fechaDeRegistro').value;
    
    let isValid = true;
    
    if (!nombre) {
        isValid = false;
        document.getElementById('nombre').classList.add('is-invalid');
        mostrarAlerta('El nombre del proveedor es obligatorio', 'danger');
        return false;
    } else {
        document.getElementById('nombre').classList.remove('is-invalid');
    }

    if (!email) {
        isValid = false;
        document.getElementById('email').classList.add('is-invalid');
        mostrarAlerta('El email es obligatorio', 'danger');
        return false;
    } else {
        document.getElementById('email').classList.remove('is-invalid');
    }

    if (!telefono) {
        isValid = false;
        document.getElementById('telefono').classList.add('is-invalid');
        mostrarAlerta('El teléfono es obligatorio', 'danger');
        return false;
    } else {
        document.getElementById('telefono').classList.remove('is-invalid');
    }

    if (!direccion) {
        isValid = false;
        document.getElementById('direccion').classList.add('is-invalid');
        mostrarAlerta('La dirección es obligatoria', 'danger');
        return false;
    } else {
        document.getElementById('direccion').classList.remove('is-invalid');
    }

    return isValid;
}

function enviarFormularioProveedor() {
    const formulario = document.getElementById('formCrearProveedor');
    const estado = document.getElementById('estado').checked;
    const fechaDeRegistro = document.getElementById('fechaDeRegistro').value;

    const proveedorDto = {
        nombre: document.getElementById('nombre').value.trim(),
        email: document.getElementById('email').value.trim(),
        telefono: document.getElementById('telefono').value.trim(),
        direccion: document.getElementById('direccion').value.trim(),
        estado: estado,
        fechaDeRegistro: fechaDeRegistro
    };

    const btnSubmit = formulario.querySelector('button[type="submit"]');
    const btnText = btnSubmit.textContent;
    btnSubmit.disabled = true;
    btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Guardando...';

    fetch('/api/Proveedor/Create', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(proveedorDto)
    })
    .then(response => {
        if (!response.ok) {
            return response.text().then(text => {
                throw new Error(text);
            });
        }
        return response.json();
    })
    .then(data => {
        formHasChanges = false;
        Swal.fire({
            title: '¡Éxito!',
            text: 'El proveedor ha sido creado correctamente',
            icon: 'success',
            confirmButtonText: 'Aceptar'
        }).then(() => {
            window.onbeforeunload = null;
            window.location.href = '/Proveedores/Index';
        });
    })
    .catch(error => {
        console.error('Error:', error);
        let errorMsg = 'Ocurrió un error al crear el proveedor';
        try {
            const errorObj = JSON.parse(error.message);
            if (errorObj.errors) {
                errorMsg = Object.values(errorObj.errors).flat().join('\n');
            } else if (errorObj.title) {
                errorMsg = errorObj.title;
            }
        } catch (e) {
            errorMsg = error.message;
        }

        Swal.fire({
            title: 'Error',
            text: errorMsg,
            icon: 'error',
            confirmButtonText: 'Aceptar'
        });
    })
    .finally(() => {
        btnSubmit.disabled = false;
        btnSubmit.textContent = btnText;
    });
}

function configurarFormularioEdicion() {
    const formulario = document.getElementById('formEditProveedor');
    if (formulario) {
        const id = window.location.pathname.split('/').pop();
        
        configurarCampoTelefono();
        
        fetch(`/api/Proveedor/${id}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('No se pudo cargar el proveedor');
                }
                return response.json();
            })
            .then(proveedor => {
                proveedorOriginal = {
                    nombre: proveedor.nombre,
                    email: proveedor.email,
                    telefono: proveedor.telefono,
                    direccion: proveedor.direccion,
                    estado: proveedor.estado,
                    fechaDeRegistro: proveedor.fechaDeRegistro
                };

                document.getElementById('idProveedor').value = proveedor.id;
                document.getElementById('nombre').value = proveedor.nombre;
                document.getElementById('email').value = proveedor.email;
                document.getElementById('telefono').value = proveedor.telefono;
                document.getElementById('direccion').value = proveedor.direccion;
                document.getElementById('estado').checked = proveedor.estado;

                if (proveedor.fechaDeRegistro) {
                    document.getElementById('fechaDeRegistro').value = proveedor.fechaDeRegistro;
                    document.getElementById('fechaDeRegistroVisible').value = formatearFechaAmPm(proveedor.fechaDeRegistro);
                }
            })
            .catch(error => {
                console.error('Error al cargar datos originales:', error);
                mostrarAlerta('Error al cargar datos del proveedor', 'error');
            });
        
        formulario.addEventListener('submit', function(e) {
            e.preventDefault();
            actualizarProveedor();
        });
    }
}

function actualizarProveedor() {
    const formulario = document.getElementById('formEditProveedor');
    if (!formulario) return;

    if (!validarFormularioProveedor()) {
        return;
    }

    const id = document.getElementById('idProveedor').value;
    const nombre = document.getElementById('nombre').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();

    verificarDuplicados(nombre, email, telefono, id)
        .then(duplicados => {
            let hayDuplicados = false;

            if (duplicados.nombre) {
                document.getElementById('nombre').classList.add('is-invalid');
                mostrarAlerta('Ya existe un proveedor con este nombre', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('nombre').classList.remove('is-invalid');
            }

            if (duplicados.email) {
                document.getElementById('email').classList.add('is-invalid');
                mostrarAlerta('Este email ya está registrado para otro proveedor', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('email').classList.remove('is-invalid');
            }

            if (duplicados.telefono) {
                document.getElementById('telefono').classList.add('is-invalid');
                mostrarAlerta('Este teléfono ya está registrado para otro proveedor', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('telefono').classList.remove('is-invalid');
            }

            if (!hayDuplicados) {
                enviarFormularioEdicion(id);
            }
        });
}

function enviarFormularioEdicion(id){
    const formulario = document.getElementById('formEditProveedor');
    const nombre = document.getElementById('nombre').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();
    const direccion = document.getElementById('direccion').value.trim();
    const estado = document.getElementById('estado').checked;

    const proveedorDto = {
        id: id,
        nombre: nombre,
        email: email,
        telefono: telefono,
        direccion: direccion,
        estado: estado
    };

    const btnSubmit = formulario.querySelector('button[type="submit"]');
    const btnText = btnSubmit.textContent;
    btnSubmit.disabled = true;
    btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Actualizando...';

    fetch(`/api/Proveedor/Edit/${id}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(proveedorDto)
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => {
                    throw new Error(text);
                });
            }
            return response.json();
        })
        .then(data => {
            formHasChanges = false;
            Swal.fire({
                title: '¡Éxito!',
                text: 'El proveedor ha sido actualizado correctamente',
                icon: 'success',
                confirmButtonText: 'Aceptar'
            }).then(() => {
                window.onbeforeunload = null;
                window.location.href = '/Proveedores/Index';
            });
        })
        .catch(error => {
            console.error('Error:', error);
            let errorMsg = 'Ocurrió un error al actualizar el proveedor';
            try {
                const errorObj = JSON.parse(error.message);
                if (errorObj.errors) {
                    errorMsg = Object.values(errorObj.errors).flat().join('\n');
                } else if (errorObj.title) {
                    errorMsg = errorObj.title;
                }
            } catch (e) {
                errorMsg = error.message;
            }

            Swal.fire({
                title: 'Error',
                text: errorMsg,
                icon: 'error',
                confirmButtonText: 'Aceptar'
            });
        })
        .finally(() => {
            btnSubmit.disabled = false;
            btnSubmit.textContent = btnText;
        });
}

function configurarPaginaListado() {
    cargarProveedores();
    document.querySelectorAll('.btn-eliminar-proveedor').forEach(boton => {
        boton.addEventListener('click', function() {
            const id = this.getAttribute('data-id');
            eliminarProveedor(id);
        });
    });
}

function eliminarProveedor(id) {
    Swal.fire({
        title: '¿Está seguro?',
        text: '¿Desea eliminar este proveedor? Esta acción no se puede deshacer.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6'
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/api/Proveedor/Delete/${id}`, {
                method: 'DELETE'
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error('No se pudo eliminar el proveedor');
                }
                
                Swal.fire({
                    title: '¡Eliminado!',
                    text: 'El proveedor ha sido eliminado correctamente',
                    icon: 'success',
                    confirmButtonText: 'Aceptar'
                }).then(() => {
                    const fila = document.querySelector(`tr[data-id="${id}"]`);
                    if (fila) {
                        fila.remove();
                    } else {
                        window.location.reload();
                    }
                });
            })
            .catch(error => {
                console.error('Error:', error);
                Swal.fire({
                    title: 'Error',
                    text: 'Ocurrió un error al eliminar el proveedor',
                    icon: 'error',
                    confirmButtonText: 'Aceptar'
                });
            });
        }
    });
}

function mostrarAlerta(mensaje, tipo, duracion = 5000) {
    let icon;
    switch (tipo) {
        case 'success': icon = 'success'; break;
        case 'danger': icon = 'error'; break;
        case 'warning': icon = 'warning'; break;
        default: icon = 'info'; break;
    }
    
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: duracion,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });
    
    Toast.fire({
        icon: icon,
        title: mensaje
    });
}