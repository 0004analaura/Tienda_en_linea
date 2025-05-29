document.addEventListener('DOMContentLoaded', function() {
    const currentPath = window.location.pathname;
    const viewMode = getViewMode(currentPath);
    
    switch(viewMode) {
        case 'index':
            initIndexView();
            break;
        case 'create':
            initCreateView();
            break;
        case 'edit':
            initEditView();
            break;
    }
});

function getViewMode(path) {
    if (path.includes('/Categorias/Index')) {
        return 'index';
    } else if (path.includes('/Categorias/Create')) {
        return 'create';
    } else if (path.includes('/Categorias/Edit/')) {
        return 'edit';
    }
    return 'unknown';
}

function initIndexView() {
    if (document.getElementById('categoriasTableBody')) {
        loadCategorias();
    }
}

function initCreateForm() {
    const form = document.getElementById('createCategoriaForm');
    if (!form) return;

    form.addEventListener('submit', function(e) {
        e.preventDefault();

        const nombre = document.getElementById('nombre').value.trim();
        if (!nombre) {
            mostrarAlerta('El nombre de la categoría es obligatorio', 'danger');
            document.getElementById('nombre').focus();
            return;
        }

        if (!form.checkValidity()) {
            e.stopPropagation();
            form.classList.add('was-validated');
            return;
        }

        verificarNombreDuplicado(nombre)
            .then(existeDuplicado => {
                if (existeDuplicado) {
                    mostrarAlerta('Ya existe una categoría con este nombre. Por favor, use un nombre diferente.', 'warning');
                    document.getElementById('nombre').focus();
                    return;
                }

                const categoriaData = {
                    nombre: nombre,
                    descripcion: document.getElementById('descripcion').value,
                    estado: document.getElementById('estado').checked,
                    fechaDeRegistro: document.getElementById('fechaDeRegistro').value
                };

                createCategoria(categoriaData);
            })
            .catch(error => {
                mostrarAlerta('Error al verificar nombres duplicados: ' + error.message, 'danger');
            });
    });
}

function verificarNombreDuplicado(nombre) {
    return new Promise((resolve, reject) => {
        fetch('/api/Categoria')
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error al cargar categorías');
                }
                return response.json();
            })
            .then(categorias => {
                const nombreNormalizado = nombre.toLowerCase().trim();
                const existeDuplicado = categorias.some(cat =>
                    cat.nombre.toLowerCase().trim() === nombreNormalizado
                );
                resolve(existeDuplicado);
            })
            .catch(error => {
                console.error('Error al verificar duplicados:', error);
                reject(error);
            });
    });
}

function createCategoria(categoriaData) {
    fetch('/api/Categoria/Create', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(categoriaData)
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al crear categoría');
            }
            return response.json();
        })
        .then(data => {
            mostrarModalExito('La categoría se ha creado correctamente');
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarAlerta('Ha ocurrido un error al crear la categoría: ' + error.message, 'danger');
        });
}

let categoriaOriginal = null;
function loadCategoriaData(id) {
    fetch(`/api/Categoria/${id}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Categoría no encontrada');
            }
            return response.json();
        })
        .then(categoria => {
            categoriaOriginal = categoria;
            const form = document.getElementById('editCategoriaForm');
            if (form) {
                document.getElementById('categoriaId').value = categoria.id;
                document.getElementById('nombre').value = categoria.nombre;
                document.getElementById('descripcion').value = categoria.descripcion || '';
                document.getElementById('estado').checked = categoria.estado;

                const fecha = new Date(categoria.fechaDeRegistro);
                document.getElementById('fechaDeRegistro').value = fecha.toISOString().slice(0, 16);
                document.getElementById('fechaDeRegistroVisible').value = formatearFechaAmPm(categoria.fechaDeRegistro);

                form.addEventListener('submit', function(e) {
                    e.preventDefault();

                    const nombre = document.getElementById('nombre').value.trim();
                    if (!nombre) {
                        mostrarAlerta('El nombre de la categoría es obligatorio', 'danger');
                        document.getElementById('nombre').focus();
                        return;
                    }

                    if (!form.checkValidity()) {
                        e.stopPropagation();
                        form.classList.add('was-validated');
                        return;
                    }

                    if (nombre !== categoriaOriginal.nombre) {
                        verificarNombreDuplicadoParaEdicion(nombre, id)
                            .then(existeDuplicado => {
                                if (existeDuplicado) {
                                    mostrarAlerta('Ya existe una categoría con este nombre. Por favor, use un nombre diferente.', 'warning');
                                    document.getElementById('nombre').focus();
                                    return;
                                }

                                const categoriaData = {
                                    nombre: nombre,
                                    descripcion: document.getElementById('descripcion').value,
                                    estado: document.getElementById('estado').checked
                                };

                                updateCategoria(id, categoriaData);
                            })
                            .catch(error => {
                                mostrarAlerta('Error al verificar nombres duplicados: ' + error.message, 'danger');
                            });
                    } else {
                        const categoriaData = {
                            nombre: nombre,
                            descripcion: document.getElementById('descripcion').value,
                            estado: document.getElementById('estado').checked
                        };

                        updateCategoria(id, categoriaData);
                    }

                });

            }
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarAlerta(`Error: ${error.message}`, 'danger');
            setTimeout(() => {
                window.location.href = '/Categorias/Index';
            }, 3000);
        });
}

function updateCategoria(id, categoriaData) {
    fetch(`/api/Categoria/Edit/${id}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(categoriaData)
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al actualizar categoría');
            }
            return response.json();
        })
        .then(data => {
            mostrarModalExito('Categoría actualizada con éxito');
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarAlerta(`Error al actualizar: ${error.message}`, 'danger');
        });
}

function verificarNombreDuplicadoParaEdicion(nombre, idActual) {
    return new Promise((resolve, reject) => {
        fetch('/api/Categoria')
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error al cargar categorías');
                }
                return response.json();
            })
            .then(categorias => {
                const nombreNormalizado = nombre.toLowerCase().trim();
                const existeDuplicado = categorias.some(cat =>
                    cat.id !== parseInt(idActual) &&
                    cat.nombre.toLowerCase().trim() === nombreNormalizado
                );
                resolve(existeDuplicado);
            })
            .catch(error => {
                console.error('Error al verificar duplicados:', error);
                reject(error);
            });
    });
}

function confirmDelete(id) {
    Swal.fire({
        title: '¿Está seguro?',
        text: '¿Desea eliminar esta categoría? Esta acción no se puede deshacer.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6'
    }).then((result) => {
        if (result.isConfirmed) {
            deleteCategoria(id);
        }
    });
}

function deleteCategoria(id) {
    fetch(`/api/Categoria/Delete/${id}`, {
        method: 'DELETE'
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al eliminar categoría');
            }

            mostrarModalExitoConAccion('Categoría eliminada con éxito', function() {
                loadCategorias();
                const modalExito = document.getElementById('modalExito');
                const bootstrapModal = bootstrap.Modal.getInstance(modalExito);
                if (bootstrapModal) {
                    bootstrapModal.hide();
                }
            });
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarAlerta(`Error al eliminar categoría: ${error.message}`, 'danger');
        });
}

function initCreateView() {
    const estadoInput = document.getElementById('estado');
    if (estadoInput) {
        estadoInput.checked = true;
    }
    
    const now = new Date();
    const fechaInput = document.getElementById('fechaDeRegistro');

    if (fechaInput) {
        fechaInput.type = 'hidden';

        const año = now.getFullYear();
        const mes = (now.getMonth() + 1).toString().padStart(2, '0');
        const dia = now.getDate().toString().padStart(2, '0');
        const horas = now.getHours().toString().padStart(2, '0');
        const minutos = now.getMinutes().toString().padStart(2, '0');

        fechaInput.value = `${año}-${mes}-${dia}T${horas}:${minutos}`;

        if (!document.getElementById('fechaDeRegistroVisible')) {
            const fechaVisible = document.createElement('input');
            fechaVisible.type = 'text';
            fechaVisible.id = 'fechaDeRegistroVisible';
            fechaVisible.className = 'form-control';
            fechaVisible.readOnly = true;
            fechaVisible.value = formatearFechaAmPm(now);
            fechaInput.parentNode.insertBefore(fechaVisible, fechaInput.nextSibling);
        }
    }

    initCreateForm();

    const btnVolver = document.getElementById('btnVolver');
    if (btnVolver) {
        btnVolver.addEventListener('click', function(e) {
            e.preventDefault();
            window.location.href = '/Categorias/Index';
        });
    }
}

function initEditView() {
    // Obtener el ID de la categoría de la URL
    const pathname = window.location.pathname;
    const categoriaId = pathname.split('/').pop();

    if (categoriaId) {
        loadCategoriaData(categoriaId);

        const btnVolver = document.getElementById('btnVolver');
        if (btnVolver) {
            btnVolver.addEventListener('click', function(e) {
                e.preventDefault();
                window.location.href = '/Categorias/Index';
            });
        }
    } else {
        window.location.href = '/Categorias/Index';
    }
}


let currentPage = 1;
const pageSize = 10;
let totalPages = 0;
function loadCategorias(searchTerm = '') {
    const tableBody = document.getElementById('categoriasTableBody');
    if (!tableBody) return;

    tableBody.innerHTML = '<tr><td colspan="6" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></td></tr>';

    fetch('/api/Categoria')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al cargar categorías');
            }
            return response.json();
        })
        .then(data => {
            categorias = data;

            if (searchTerm) {
                searchTerm = searchTerm.toLowerCase();
                categorias = categorias.filter(c =>
                    c.nombre.toLowerCase().includes(searchTerm) ||
                    (c.descripcion && c.descripcion.toLowerCase().includes(searchTerm))
                );
            }

            totalPages = Math.ceil(categorias.length / pageSize);

            renderCategorias();
        })
        .catch(error => {
            console.error('Error:', error);
            tableBody.innerHTML = `<tr><td colspan="6" class="text-center text-danger">Error al cargar datos: ${error.message}</td></tr>`;
        });
}

function renderCategorias() {
    const tableBody = document.getElementById('categoriasTableBody');
    if (!tableBody) return;
    
    tableBody.innerHTML = '';

    const start = (currentPage - 1) * pageSize;
    const end = start + pageSize;
    const paginatedCategorias = categorias.slice(start, end);

    if (paginatedCategorias.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="6" class="text-center">No se encontraron categorías</td></tr>';
        return;
    }

    paginatedCategorias.forEach(categoria => {
        const row = document.createElement('tr');
        const fechaFormateada = formatearFechaAmPm(categoria.fechaDeRegistro);

        row.innerHTML = `
            <td>${categoria.id}</td>
            <td>${categoria.nombre}</td>
            <td>${categoria.descripcion || '-'}</td>
            <td>
                <span class="badge ${categoria.estado ? 'bg-success' : 'bg-danger'}">
                    ${categoria.estado ? 'Activo' : 'Inactivo'}
                </span>
            </td>
            <td>${fechaFormateada}</td>
            <td>
                <a href="/Categorias/Edit/${categoria.id}" class="btn btn-sm btn-primary me-1">
                    <i class="fas fa-edit"></i>
                </a>
                <button class="btn btn-sm btn-danger" onclick="confirmDelete(${categoria.id})">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        `;

        tableBody.appendChild(row);
    });
}

function formatearFechaAmPm(fechaStr) {
    if (!fechaStr) return '';

    const fecha = new Date(fechaStr);

    if (isNaN(fecha.getTime())) return 'Fecha inválida';
    
    const fechaLocal = new Date(fecha.getTime());

    const dia = fechaLocal.getDate().toString().padStart(2, '0');
    const mes = (fechaLocal.getMonth() + 1).toString().padStart(2, '0');
    const año = fechaLocal.getFullYear();
    
    let horas = fechaLocal.getHours();
    const minutos = fechaLocal.getMinutes().toString().padStart(2, '0');
    const ampm = horas >= 12 ? 'p.m.' : 'a.m.';
    horas = horas % 12;
    horas = horas ? horas : 12;
    const horasStr = horas.toString().padStart(2, '0');

    return `${dia}/${mes}/${año} ${horasStr}:${minutos} ${ampm}`;
}

function mostrarAlerta(mensaje, tipo, duracion = 5000) {
    let icon;
    switch (tipo) {
        case 'success': icon = 'success'; break;
        case 'danger': icon = 'error'; break;
        case 'warning': icon = 'warning'; break;
        case 'info': icon = 'info'; break;
        default: icon = 'info';
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

function mostrarModalExito(mensaje) {
    window.onbeforeunload = null;
    formHasChanges = false;
    
    Swal.fire({
        title: '¡Éxito!',
        text: mensaje,
        icon: 'success',
        confirmButtonText: 'Aceptar',
        confirmButtonColor: '#198754'
    }).then(() => {
        window.location.href = '/Categorias/Index';
    });
}

function mostrarModalExitoConAccion(mensaje, accion = null) {
    window.onbeforeunload = null;
    formHasChanges = false;
    
    Swal.fire({
        title: '¡Éxito!',
        text: mensaje,
        icon: 'success',
        confirmButtonText: 'Aceptar',
        confirmButtonColor: '#198754'
    }).then(() => {
        if (accion) {
            accion();
        } else {
            window.location.href = '/Categorias/Index';
        }
    });
}