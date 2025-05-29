document.addEventListener('DOMContentLoaded', function() {
    inicializarCampoFechaRegistro();

    const currentPath = window.location.pathname;

    if (currentPath.includes('/Clientes/Create')) {
        configurarFormularioCreacion();
    }
    else if (currentPath.includes('/Clientes/Edit/')) {
        configurarFormularioEdicion();
    }
    else if (currentPath.includes('/Clientes/Index')) {
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
    horas = horas ? horas : 12;
    const horasStr = horas.toString().padStart(2, '0');

    return `${dia}/${mes}/${año} ${horasStr}:${minutos} ${ampm}`;
}

function cargarClientes() {
    const tablaBody = document.querySelector('#clientesTable tbody');
    if (tablaBody) {
        tablaBody.innerHTML = '<tr><td colspan="9" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></td></tr>';
    }

    fetch('/api/Cliente')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al cargar clientes');
            }
            return response.json();
        })
        .then(data => {
            if (tablaBody) {
                if (data.length === 0) {
                    tablaBody.innerHTML = '<tr><td colspan="9" class="text-center">No hay clientes registrados</td></tr>';
                    return;
                }

                tablaBody.innerHTML = '';

                data.forEach(cliente => {
                    const row = document.createElement('tr');
                    const fechaFormateada = formatearFechaAmPm(cliente.fechaDeRegistro);

                    row.innerHTML = `
                        <td>${cliente.id}</td>
                        <td>${cliente.nombre}</td>
                        <td>${cliente.apellido}</td>
                        <td>${cliente.email}</td>
                        <td>${cliente.telefono}</td>
                        <td>${cliente.direccion}</td>
                        <td>
                            <span class="badge ${cliente.estado ? 'bg-success' : 'bg-danger'}">
                                ${cliente.estado ? 'Activo' : 'Inactivo'}
                            </span>
                        </td>
                        <td>${fechaFormateada}</td>
                        <td>
                            <a href="/Clientes/Edit/${cliente.id}" class="btn btn-sm btn-primary me-1">
                                <i class="fas fa-edit"></i>
                            </a>
                            <button class="btn btn-sm btn-danger btn-eliminar-cliente" data-id="${cliente.id}">
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
    document.querySelectorAll('.btn-eliminar-cliente').forEach(boton => {
        boton.addEventListener('click', function() {
            const id = this.getAttribute('data-id');
            eliminarCliente(id);
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
        if (window.location.pathname.includes('/Clientes/Edit/')) {
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
    const formulario = document.getElementById('formCrearCliente');
    if (formulario) {
        formulario.addEventListener('submit', function(e) {
            e.preventDefault();
            crearCliente();
        });

        configurarCampoTelefono();
    }
}

function verificarDuplicados(nombre, apellido, email, telefono, id = null) {
    return fetch('/api/Cliente')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al verificar datos duplicados');
            }
            return response.json();
        })
        .then(clientes => {
            const clientesFiltrados = id
                ? clientes.filter(c => c.id != id)
                : clientes;

            let duplicados = {
                nombre: clientesFiltrados.some(c => c.nombre.toLowerCase() === nombre.toLowerCase()),
                apellido: clientesFiltrados.some(c => c.apellido && c.apellido.toLowerCase() === apellido.toLowerCase()),
                email: clientesFiltrados.some(c => c.email && c.email.toLowerCase() === email.toLowerCase()),
                telefono: clientesFiltrados.some(c => c.telefono && c.telefono === telefono)
            };

            return duplicados;
        })
        .catch(error => {
            console.error('Error al verificar duplicados:', error);
            return { nombre: false, apellido: false, email: false, telefono: false };
        });
}

function crearCliente() {
    const formulario = document.getElementById('formCrearCliente');
    if (!formulario) return;

    if (!validarFormularioCliente()) {
        return;
    }

    const nombre = document.getElementById('nombre').value.trim();
    const apellido = document.getElementById('apellido').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();

    verificarDuplicados(nombre, apellido, email, telefono)
        .then(duplicados => {
            let hayDuplicados = false;

            if (duplicados.nombre) {
                document.getElementById('nombre').classList.add('is-invalid');
                mostrarAlerta('Ya existe un cliente con este nombre', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('nombre').classList.remove('is-invalid');
            }
            
            if (duplicados.apellido) {
                document.getElementById('apellido').classList.add('is-invalid');
                mostrarAlerta('Ya existe un cliente con este apellido', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('apellido').classList.remove('is-invalid');
            }

            if (duplicados.email) {
                document.getElementById('email').classList.add('is-invalid');
                mostrarAlerta('Este email ya está registrado para otro cliente', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('email').classList.remove('is-invalid');
            }

            if (duplicados.telefono) {
                document.getElementById('telefono').classList.add('is-invalid');
                mostrarAlerta('Este teléfono ya está registrado para otro cliente', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('telefono').classList.remove('is-invalid');
            }

            if (!hayDuplicados) {
                enviarFormularioCliente();
            }
        });
}

function validarFormularioCliente() {
    const nombre = document.getElementById('nombre').value.trim();
    const apellido = document.getElementById('apellido').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();
    const direccion = document.getElementById('direccion').value.trim();
    const estado = document.getElementById('estado').value === 'true';
    const fechaDeRegistro = document.getElementById('fechaDeRegistro').value;

    let isValid = true;

    if (!nombre) {
        isValid = false;
        document.getElementById('nombre').classList.add('is-invalid');
        mostrarAlerta('El nombre del cliente es obligatorio', 'danger');
        return false;
    } else {
        document.getElementById('nombre').classList.remove('is-invalid');
    }
    
    if (!apellido) {
        isValid = false;
        document.getElementById('apellido').classList.add('is-invalid');
        mostrarAlerta('El apellido del cliente es obligatorio', 'danger');
        return false;
    } else {
        document.getElementById('apellido').classList.remove('is-invalid');
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

function enviarFormularioCliente() {
    const formulario = document.getElementById('formCrearCliente');
    const estado = document.getElementById('estado').checked;
    const fechaDeRegistro = document.getElementById('fechaDeRegistro').value;

    const clienteDto = {
        nombre: document.getElementById('nombre').value.trim(),
        apellido: document.getElementById('apellido').value.trim(),
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

    fetch('/api/Cliente/Create', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(clienteDto)
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
                text: 'El cliente ha sido creado correctamente',
                icon: 'success',
                confirmButtonText: 'Aceptar'
            }).then(() => {
                window.onbeforeunload = null;
                window.location.href = '/Clientes/Index';
            });
        })
        .catch(error => {
            console.error('Error:', error);
            let errorMsg = 'Ocurrió un error al crear el cliente';
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
    const formulario = document.getElementById('formEditCliente');
    if (formulario) {
        const id = window.location.pathname.split('/').pop();

        configurarCampoTelefono();

        fetch(`/api/Cliente/${id}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('No se pudo cargar el cliente');
                }
                return response.json();
            })
            .then(cliente => {
                clienteOriginal = {
                    nombre: cliente.nombre,
                    apellido: cliente.apellido,
                    email: cliente.email,
                    telefono: cliente.telefono,
                    direccion: cliente.direccion,
                    estado: cliente.estado,
                    fechaDeRegistro: cliente.fechaDeRegistro
                };

                document.getElementById('idCliente').value = cliente.id;
                document.getElementById('nombre').value = cliente.nombre;
                document.getElementById('apellido').value = cliente.apellido;
                document.getElementById('email').value = cliente.email;
                document.getElementById('telefono').value = cliente.telefono;
                document.getElementById('direccion').value = cliente.direccion;
                document.getElementById('estado').checked = cliente.estado;

                if (cliente.fechaDeRegistro) {
                    document.getElementById('fechaDeRegistro').value = cliente.fechaDeRegistro;
                    document.getElementById('fechaDeRegistroVisible').value = formatearFechaAmPm(cliente.fechaDeRegistro);
                }
                
            })
            .catch(error => {
                console.error('Error al cargar datos originales:', error);
                mostrarAlerta('Error al cargar datos del cliente', 'error');
            });

        formulario.addEventListener('submit', function(e) {
            e.preventDefault();
            actualizarCliente();
        });
    }
}

function actualizarCliente() {
    const formulario = document.getElementById('formEditCliente');
    if (!formulario) return;

    if (!validarFormularioCliente()) {
        return;
    }

    const id = document.getElementById('idCliente').value;
    const nombre = document.getElementById('nombre').value.trim();
    const apellido = document.getElementById('apellido').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();

    verificarDuplicados(nombre, apellido, email, telefono, id)
        .then(duplicados => {
            let hayDuplicados = false;

            if (duplicados.nombre) {
                document.getElementById('nombre').classList.add('is-invalid');
                mostrarAlerta('Ya existe un cliente con este nombre', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('nombre').classList.remove('is-invalid');
            }
            
            if (duplicados.apellido) {
                document.getElementById('apellido').classList.add('is-invalid');
                mostrarAlerta('Ya existe un cliente con este apellido', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('apellido').classList.remove('is-invalid');
            }

            if (duplicados.email) {
                document.getElementById('email').classList.add('is-invalid');
                mostrarAlerta('Este email ya está registrado para otro cliente', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('email').classList.remove('is-invalid');
            }

            if (duplicados.telefono) {
                document.getElementById('telefono').classList.add('is-invalid');
                mostrarAlerta('Este teléfono ya está registrado para otro cliente', 'danger');
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
    const formulario = document.getElementById('formEditCliente');
    const nombre = document.getElementById('nombre').value.trim();
    const apellido = document.getElementById('apellido').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();
    const direccion = document.getElementById('direccion').value.trim();
    const estado = document.getElementById('estado').checked;

    const clienteDto = {
        id: id,
        nombre: nombre,
        apellido: apellido,
        email: email,
        telefono: telefono,
        direccion: direccion,
        estado: estado
    };

    const btnSubmit = formulario.querySelector('button[type="submit"]');
    const btnText = btnSubmit.textContent;
    btnSubmit.disabled = true;
    btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Actualizando...';

    fetch(`/api/Cliente/Edit/${id}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(clienteDto)
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
                text: 'El cliente ha sido actualizado correctamente',
                icon: 'success',
                confirmButtonText: 'Aceptar'
            }).then(() => {
                window.onbeforeunload = null;
                window.location.href = '/Clientes/Index';
            });
        })
        .catch(error => {
            console.error('Error:', error);
            let errorMsg = 'Ocurrió un error al actualizar el cliente';
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
    cargarClientes();
    document.querySelectorAll('.btn-eliminar-cliente').forEach(boton => {
        boton.addEventListener('click', function() {
            const id = this.getAttribute('data-id');
            eliminarCliente(id);
        });
    });
}

function eliminarCliente(id) {
    Swal.fire({
        title: '¿Está seguro?',
        text: '¿Desea eliminar este cliente? Esta acción no se puede deshacer.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6'
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/api/Cliente/Delete/${id}`, {
                method: 'DELETE'
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('No se pudo eliminar el cliente');
                    }

                    Swal.fire({
                        title: '¡Eliminado!',
                        text: 'El cliente ha sido eliminado correctamente',
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
                        text: 'Ocurrió un error al eliminar el cliente',
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