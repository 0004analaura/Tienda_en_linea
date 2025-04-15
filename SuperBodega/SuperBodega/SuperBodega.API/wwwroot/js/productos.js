let productos = [];
document.addEventListener('DOMContentLoaded', function () {
    inicializarFormularioProducto();

    const formCrearProducto = document.getElementById('formCrearProducto');
    if (formCrearProducto) {
        formCrearProducto.addEventListener('submit', function (e) {
            e.preventDefault();
            crearProducto();
        });
    }

    const formEditarProducto = document.getElementById('formEditarProducto');
    if (formEditarProducto) {
        formEditarProducto.addEventListener('submit', function (e) {
            e.preventDefault();
            editarProducto();
        });
    }

    const tablaProductos = document.getElementById('tablaProductos');
    if (tablaProductos) {
        cargarProductos();
    }

    configurarBotonesEliminar();
});

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

function inicializarCampoFechaRegistro() {
    const fechaInput = document.getElementById('fechaDeRegistro');
    const fechaVisibleInput = document.getElementById('fechaDeRegistroVisible');
    
    if (fechaInput && fechaVisibleInput) {
        const formEditarProducto = document.getElementById('formEditarProducto');
        if (formEditarProducto) {
            const fechaProducto = document.querySelector('img.img-thumbnail')?.getAttribute('data-fecha-registro');
            
            if (fechaProducto) {
                fechaInput.value = new Date(fechaProducto).toISOString().slice(0, 16);
                fechaVisibleInput.value = formatearFechaAmPm(fechaProducto);
                return;
            }
        }
        
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

function setupImagePreview() {
    const imagenInput = document.getElementById('imagen');
    if (!imagenInput) return;

    let previewContainer = document.getElementById('image-preview-container');
    if (!previewContainer) {
        previewContainer = document.createElement('div');
        previewContainer.id = 'image-preview-container';
        previewContainer.className = 'preview-container mt-2';
        previewContainer.style.display = 'none';
        previewContainer.style.position = 'relative';
        previewContainer.style.maxWidth = '100%';
        previewContainer.style.height = '150px';
        previewContainer.style.overflow = 'hidden';
        previewContainer.style.borderRadius = '5px';
        previewContainer.style.border = '1px solid #ddd';

        const previewImage = document.createElement('img');
        previewImage.id = 'image-preview';
        previewImage.className = 'preview-image';
        previewImage.alt = 'Vista previa';
        previewImage.style.maxWidth = '100%';
        previewImage.style.maxHeight = '100%';
        previewImage.style.objectFit = 'contain';
        previewImage.style.display = 'block';
        previewImage.style.margin = '0 auto';

        previewContainer.appendChild(previewImage);
        imagenInput.parentNode.parentNode.insertBefore(previewContainer, imagenInput.parentNode.nextSibling);
    }

    imagenInput.addEventListener('change', function() {
        const previewImage = document.getElementById('image-preview');

        if (this.files && this.files[0]) {
            const reader = new FileReader();

            reader.onload = function(e) {
                previewImage.src = e.target.result;
                previewContainer.style.display = 'flex';
                previewContainer.style.alignItems = 'center';
                previewContainer.style.justifyContent = 'center';

                const mantenerImagenCheckbox = document.getElementById('mantenerImagen');
                if (mantenerImagenCheckbox) {
                    mantenerImagenCheckbox.checked = false;
                }
            };

            reader.readAsDataURL(this.files[0]);

            let clearButton = document.getElementById('clear-image-button');
            if (!clearButton) {
                clearButton = document.createElement('button');
                clearButton.id = 'clear-image-button';
                clearButton.type = 'button';
                clearButton.className = 'btn btn-sm btn-danger position-absolute';
                clearButton.style.top = '5px';
                clearButton.style.right = '5px';
                clearButton.style.zIndex = '10';
                clearButton.innerHTML = '<i class="bi bi-x-lg"></i>';
                clearButton.title = 'Quitar imagen seleccionada';

                clearButton.addEventListener('click', function(e) {
                    e.stopPropagation();
                    imagenInput.value = '';
                    previewContainer.style.display = 'none';
                    const mantenerImagenCheckbox = document.getElementById('mantenerImagen');
                    if (mantenerImagenCheckbox) {
                        mantenerImagenCheckbox.checked = true;
                    }
                });

                previewContainer.appendChild(clearButton);
            }
        } else {
            previewContainer.style.display = 'none';
        }
    });
    
    const imagenExistente = document.querySelector('.card-body .img-thumbnail');
    if (imagenExistente && imagenExistente.src) {
        previewContainer.style.display = 'none';
    }
}

function verificarCodigoDuplicado(codigo) {
    return fetch('/api/Producto/GetAll')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al verificar código duplicado');
            }
            return response.json();
        })
        .then(productos => {
            return productos.some(p => p.codigo.toLowerCase() === codigo.toLowerCase());
        })
        .catch(error => {
            console.error('Error al verificar duplicados:', error);
            return false;
        });
}

function verificarNombreDuplicado(nombre) {
    return fetch('/api/Producto/GetAll')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al verificar nombre duplicado');
            }
            return response.json();
        })
        .then(productos => {
            return productos.some(p => p.nombre.toLowerCase() === nombre.toLowerCase());
        })
        .catch(error => {
            console.error('Error al verificar duplicados:', error);
            return false;
        });
}

function verificarDuplicadosEdicion(codigo, nombre, idActual) {
    return fetch('/api/Producto/GetAll')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al cargar productos');
            }
            return response.json();
        })
        .then(productos => {
            const productosFiltrados = productos.filter(p => p.id !== parseInt(idActual));

            const codigoNormalizado = codigo.toLowerCase().trim();
            const nombreNormalizado = nombre.toLowerCase().trim();

            const duplicadoCodigo = productosFiltrados.some(p =>
                p.codigo.toLowerCase().trim() === codigoNormalizado);

            const duplicadoNombre = productosFiltrados.some(p =>
                p.nombre.toLowerCase().trim() === nombreNormalizado);

            return {
                codigoDuplicado: duplicadoCodigo,
                nombreDuplicado: duplicadoNombre
            };
        });
}

function inicializarFormularioProducto() {
    inicializarCampoFechaRegistro();
    
    setupImagePreview();

    configurarVistasPreviasImagenes();

    const formCrearProducto = document.getElementById('formCrearProducto');
    if (formCrearProducto) {
        const campoStock = document.getElementById('Stock');
        const campoPrecioCompra = document.getElementById('PrecioDeCompra');
        const campoPrecioVenta = document.getElementById('PrecioDeVenta');

        if (campoStock) {
            campoStock.disabled = true;
            campoStock.value = '0'; 
        }

        if (campoPrecioCompra) {
            campoPrecioCompra.disabled = true;
            campoPrecioCompra.value = '0.00'; 
        }

        if (campoPrecioVenta) {
            campoPrecioVenta.disabled = true;
            campoPrecioVenta.value = '0.00'; 
        }

    }

    const formEditarProducto = document.getElementById('formEditarProducto');
    if (formEditarProducto) {
        
        const idInput = document.querySelector('input[name="Id"]');
        if (idInput) {
            const id = idInput.value;
            fetch(`/api/Producto/${id}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error('No se pudo cargar el producto');
                    }
                    return response.json();
                })
                .then(producto => {
                    productoOriginal = {
                        codigo: producto.codigo,
                        nombre: producto.nombre,
                        descripcion: producto.descripcion || '',
                        categoriaId: producto.categoriaId,
                        estado: producto.estado
                    };
                    
                })
                .catch(error => {
                    console.error('Error al cargar producto original:', error);
                });
            }
        const campoStock = document.getElementById('Stock');
        const campoPrecioCompra = document.getElementById('PrecioDeCompra');
        const campoPrecioVenta = document.getElementById('PrecioDeVenta');

        
        if (campoPrecioCompra) {
            campoPrecioCompra.setAttribute('data-original-value', campoPrecioCompra.value);
            campoPrecioCompra.disabled = true;
        }

        if (campoPrecioVenta) {
            campoPrecioVenta.setAttribute('data-original-value', campoPrecioVenta.value);
            campoPrecioVenta.disabled = true;
        }

        const mantenerImagenCheckbox = document.getElementById('mantenerImagen');
        const imagenInput = document.getElementById('imagen');

        if (mantenerImagenCheckbox && imagenInput) {
            imagenInput.addEventListener('change', function() {
                if (this.files && this.files.length > 0) {
                    mantenerImagenCheckbox.checked = false;
                }
            });
        }
    }
}

function mostrarImagenAmpliada(src, titulo) {
    if (!src || src === '/images/productos/default_product.png') {
        mostrarAlerta('No hay imagen disponible para este producto', 'info');
        return;
    }
    
    Swal.fire({
        title: titulo || 'Vista previa',
        html: `<div class="text-center"><img src="${src}" class="img-fluid" style="max-height: 70vh;" alt="Imagen ampliada"></div>`,
        showCloseButton: true,
        showConfirmButton: false,
        width: 'auto',
        padding: '1rem',
        background: '#fff',
        backdrop: 'rgba(0,0,0,0.8)'
    });
}

function configurarVistasPreviasImagenes() {
    const tablaProductos = document.getElementById('tablaProductos');
    if (tablaProductos) {
        tablaProductos.addEventListener('click', function(e) {
            const imagen = e.target.closest('img.img-thumbnail');
            if (imagen) {
                e.preventDefault();
                const nombreProducto = imagen.getAttribute('alt') || 'Producto';
                mostrarImagenAmpliada(imagen.src, nombreProducto);
            }
        });
    }
    
    const previewContainer = document.getElementById('image-preview-container');
    if (previewContainer) {
        previewContainer.addEventListener('click', function(e) {
            const imagen = e.target.closest('#image-preview');
            if (imagen && imagen.src) {
                if (!e.target.closest('#clear-image-button')) {
                    mostrarImagenAmpliada(imagen.src, 'Vista previa');
                }
            }
        });
    }
    
    const imagenExistente = document.querySelector('.card-body .img-thumbnail');
    if (imagenExistente) {
        imagenExistente.style.cursor = 'pointer';
        imagenExistente.title = 'Clic para ampliar';
        imagenExistente.addEventListener('click', function() {
            const nombreProducto = this.getAttribute('alt') || 'Producto';
            mostrarImagenAmpliada(this.src, nombreProducto);
        });
    }
}

function verificarCategoriaActiva(categoriaId) {
    return new Promise((resolve, reject) => {
        fetch(`/api/Categoria/${categoriaId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error al verificar la categoría');
                }
                return response.json();
            })
            .then(categoria => {
                resolve(categoria.estado);
            })
            .catch(error => {
                console.error('Error:', error);
                resolve(false);
            });
    });
}

let currentPage = 1;
const pageSize = 10;
let totalPages = 0;
function loadProductos(searchTerm = '') {
    const tableBody = document.getElementById('productosTableBody');
    if (!tableBody) return;

    tableBody.innerHTML = '<tr><td colspan="9" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></td></tr>';

    fetch('/api/Producto/GetAll')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al cargar productos');
            }
            return response.json();
        })
        .then(data => {
            productos = data;

            if (searchTerm) {
                searchTerm = searchTerm.toLowerCase();
                productos = productos.filter(p =>
                    p.codigo.toLowerCase().includes(searchTerm) ||
                    p.nombre.toLowerCase().includes(searchTerm) ||
                    (p.descripcion && p.descripcion.toLowerCase().includes(searchTerm)) ||
                    p.categoriaNombre.toLowerCase().includes(searchTerm)
                );
            }

            totalPages = Math.ceil(productos.length / pageSize);

            renderProductos();
        })
        .catch(error => {
            console.error('Error:', error);
            tableBody.innerHTML = `<tr><td colspan="9" class="text-center text-danger">Error al cargar datos: ${error.message}</td></tr>`;
        });
}

function renderProductos() {
    const tableBody = document.getElementById('productosTableBody');
    if (!tableBody) return;

    tableBody.innerHTML = '';

    const start = (currentPage - 1) * pageSize;
    const end = start + pageSize;
    const paginatedProductos = productos.slice(start, end);

    if (paginatedProductos.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="11" class="text-center">No se encontraron productos</td></tr>';
        return;
    }

    const imagenPorDefecto = '/images/productos/default_product.png'; // Ajusta esta ruta según tu proyecto

    paginatedProductos.forEach(producto => {
        const row = document.createElement('tr');
        const fechaFormateada = formatearFechaAmPm(producto.fechaDeRegistro);

        let imagenUrl = imagenPorDefecto;

        if (producto.imagenUrl && producto.imagenUrl.trim() !== '' &&
            producto.imagenUrl !== null && producto.imagenUrl !== 'null') {
            if (producto.imagenUrl.startsWith('/')) {
                imagenUrl = producto.imagenUrl;
            } else {
                imagenUrl = '/' + producto.imagenUrl;
            }
        }

        row.innerHTML = `
            <td class="text-center">
                <img src="${imagenUrl}" alt="Imagen de ${producto.nombre}" 
                     class="img-thumbnail" style="width: 50px; height: 50px; cursor: pointer;"
                     title="Clic para ampliar"
                     onerror="this.src='${imagenPorDefecto}';">
            </td>
            <td>${producto.codigo}</td>
            <td>${producto.nombre}</td>
            <td>${producto.descripcion}</td>
            <td>${producto.categoriaNombre}</td>
            <td class="${producto.stock <= 5 ? 'text-danger fw-bold' : ''}">${producto.stock}</td>
            <td>${formatearPrecio(producto.precioDeCompra)}</td>
            <td>${formatearPrecio(producto.precioDeVenta)}</td>
            <td>
                <span class="badge ${producto.estado ? 'bg-success' : 'bg-danger'}">
                    ${producto.estado ? 'Activo' : 'Inactivo'}
                </span>
            </td>
            <td>${fechaFormateada}</td>
            <td>
                <a href="/Productos/Edit/${producto.id}" class="btn btn-sm btn-primary me-1">
                    <i class="fas fa-edit"></i>
                </a>
                <button class="btn btn-sm btn-danger btn-eliminar-producto" data-id="${producto.id}" onclick="eliminarProducto(${producto.id})">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        `;

        tableBody.appendChild(row);
    });
}

function cargarProductos() {
    loadProductos();
    inicializarCampoFechaRegistro();
}

function formatearPrecio(precio) {
    if (precio === undefined || precio === null) return '-';
    return new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'USD' }).format(precio);
}

function crearProducto(event) {
    if (!validarFormularioProducto()) {
        return false;
    }

    const codigo = document.getElementById('Codigo').value.trim();
    const nombre = document.getElementById('Nombre').value.trim();
    const categoriaId = document.getElementById('CategoriaId').value;

    // Primero verificar si hay duplicados
    Promise.all([
        verificarCodigoDuplicado(codigo),
        verificarNombreDuplicado(nombre),
        verificarCategoriaActiva(categoriaId)
    ])
        .then(([codigoDuplicado, nombreDuplicado, categoriaActiva]) => {
            if (codigoDuplicado) {
                mostrarAlerta('Ya existe un producto con este código', 'danger');
                document.getElementById('Codigo').focus();
                return;
            }

            if (nombreDuplicado) {
                mostrarAlerta('Ya existe un producto con este nombre', 'danger');
                document.getElementById('Nombre').focus();
                return;
            }

            if (!categoriaActiva) {
                mostrarAlerta('La categoría seleccionada no está activa', 'warning');
                document.getElementById('CategoriaId').focus();
                return;
            }

            // Si no hay duplicados, continuar con la creación
            enviarFormularioCreacion();
        });
}

function enviarFormularioCreacion() {
    const formData = new FormData(document.getElementById('formCrearProducto'));
    const btnSubmit = document.querySelector('#formCrearProducto button[type="submit"]');
    const textoOriginal = btnSubmit.innerHTML;

    const imagenInput = document.getElementById('imagen');
    if (!imagenInput.files || imagenInput.files.length === 0) {
        formData.append('UseDefaultImage', 'true');
    }
    
    btnSubmit.disabled = true;
    btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Guardando...';

    fetch('/api/Producto/Create', {
        method: 'POST',
        body: formData
    })
        .then(async response => {
            if (!response.ok) {
                const text = await response.text();
                throw new Error(text);
            }
            return response.json();
        })
        .then(data => {
            Swal.fire({
                title: '¡Éxito!',
                text: 'El producto ha sido creado correctamente',
                icon: 'success',
                confirmButtonText: 'Aceptar'
            }).then(() => {
                window.onbeforeunload = null;
                window.location.href = '/Productos/Index';
            });
        })
        .catch(error => {
            console.error('Error:', error.message);
            Swal.fire({
                title: 'Error',
                text: error.message || 'Hubo un problema al crear el producto.',
                icon: 'error',
                confirmButtonText: 'Aceptar'
            });
        })
        .finally(() => {
            btnSubmit.disabled = false;
            btnSubmit.innerHTML = textoOriginal;
        });
}

function validarFormularioProducto() {
    const codigo = document.getElementById('Codigo').value.trim();
    const nombre = document.getElementById('Nombre').value.trim();
    const descripcion = document.getElementById('Descripcion').value.trim();
    const categoriaId = document.getElementById('CategoriaId').value;

    let isValid = true;

    if (!codigo) {
        isValid = false;
        document.getElementById('Codigo').classList.add('is-invalid');
        mostrarAlerta('El código del producto es obligatorio', 'danger');
        return false;
    } else {
        document.getElementById('Codigo').classList.remove('is-invalid');
    }

    if (!nombre) {
        isValid = false;
        document.getElementById('Nombre').classList.add('is-invalid');
        mostrarAlerta('El nombre del producto es obligatorio', 'danger');
        return false;
    } else {
        document.getElementById('Nombre').classList.remove('is-invalid');
    }
    
    if (descripcion.length > 200) 
    {
        isValid = false;
        document.getElementById('Descripcion').classList.add('is-invalid');
        mostrarAlerta('La descripción no puede exceder los 200 caracteres', 'danger');
        return false;
    }
    else if (!descripcion) 
    {
        isValid = false;
        document.getElementById('Descripcion').classList.add('is-invalid');
        mostrarAlerta('La descripción del producto es obligatoria', 'danger');
        return false;
    }

    if (!categoriaId) {
        isValid = false;
        document.getElementById('CategoriaId').classList.add('is-invalid');
        mostrarAlerta('Debe seleccionar una categoría', 'danger');
        return false;
    } else {
        document.getElementById('CategoriaId').classList.remove('is-invalid');
    }

    return isValid;
}

function editarProducto() {
    const form = document.getElementById('formEditarProducto');
    const idActual = form.querySelector('input[name="Id"]').value;
    const codigo = document.getElementById('Codigo').value.trim();
    const nombre = document.getElementById('Nombre').value.trim();
    const categoriaId = document.getElementById('CategoriaId').value;


    if (!validarFormularioProducto()) {
        return;
    }

    verificarDuplicadosEdicion(codigo, nombre, idActual)
        .then(resultado => {
            if (resultado.codigoDuplicado) {
                mostrarAlerta('Ya existe un producto con este código', 'warning');
                return;
            }

            if (resultado.nombreDuplicado) {
                mostrarAlerta('Ya existe un producto con este nombre', 'warning');
                return;
            }

            if (parseInt(categoriaId) !== productoOriginal.categoriaId) {
                verificarCategoriaActiva(categoriaId)
                    .then(categoriaActiva => {
                        if (!categoriaActiva) {
                            mostrarAlerta('La categoría seleccionada no está activa', 'warning');
                            document.getElementById('CategoriaId').focus();
                            return;
                        }
                        enviarFormularioEdicion();
                    });
            } else {
                enviarFormularioEdicion();
            }
        })
        .catch(error => {
            mostrarAlerta('Error al verificar datos: ' + error.message, 'danger');
        });
}

function enviarFormularioEdicion() {
    const form = document.getElementById('formEditarProducto');
    const formData = new FormData(form);
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token) {
        formData.append('__RequestVerificationToken', token);
    }
    const id = formData.get('Id');
    const btnSubmit = document.querySelector('#formEditarProducto button[type="submit"]');
    const textoOriginal = btnSubmit.innerHTML;

    btnSubmit.disabled = true;
    btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Actualizando...';

    const imagenInput = document.getElementById('imagen');
    const mantenerImagenCheckbox = document.getElementById('mantenerImagen');
    const mantenerImagen = mantenerImagenCheckbox ? mantenerImagenCheckbox.checked : false;
    const hayNuevaImagen = imagenInput && imagenInput.files && imagenInput.files.length > 0;

    const precioCompra = document.getElementById('PrecioDeCompra').getAttribute('data-original-value') || document.getElementById('PrecioDeCompra').value;
    const precioVenta = document.getElementById('PrecioDeVenta').getAttribute('data-original-value') || document.getElementById('PrecioDeVenta').value;

    const estadoElem = document.querySelector('input[name="Estado"]');
    const estadoValor = estadoElem ? estadoElem.checked : false;

    if (hayNuevaImagen) {
        formData.set('PrecioDeCompra', precioCompra);
        formData.set('PrecioDeVenta', precioVenta);
        formData.set('Estado', estadoValor ? 'true' : 'false');

        fetch(`/api/Producto/EditImage/${id}`, {
            method: 'POST',
            body: formData
        })
            .then(async response => {
                if (!response.ok) {
                    const errorText = await response.text();
                    throw new Error(errorText || 'Error al actualizar el producto con la imagen');
                }
                // Verificar si hay contenido en la respuesta
                const contentType = response.headers.get('content-type');
                if (contentType && contentType.includes('application/json') && parseInt(response.headers.get('content-length') || '0') > 0) {
                    return response.json();
                } else {
                    // Si no hay JSON, retornar un objeto vacío
                    return {};
                }
                // return response.json();
            })
            .then(() => {
                mostrarMensajeExito();
            })
            .catch(error => {
                mostrarMensajeError(error);
            })
            .finally(() => {
                btnSubmit.disabled = false;
                btnSubmit.innerHTML = textoOriginal;
            });
    } else {
        const productoData = {
            Codigo: formData.get('Codigo'),
            Nombre: formData.get('Nombre'),
            Descripcion: formData.get('Descripcion'),
            CategoriaId: parseInt(formData.get('CategoriaId')),
            PrecioDeCompra: parseFloat(precioCompra),
            PrecioDeVenta: parseFloat(precioVenta),
            Estado: estadoValor,
            ImagenUrl: mantenerImagen ? document.querySelector('img.img-thumbnail')?.getAttribute('src') : '/images/productos/default_product.png'
        };

        fetch(`/api/Producto/Edit/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token || ''
            },
            body: JSON.stringify(productoData)
        })
            .then(async response => {
                if (!response.ok) {
                    const errorText = await response.text();
                    throw new Error(errorText || 'Error al actualizar el producto');
                }
                if (response.status === 204) {
                    return null;
                }
                if (response.headers.get('content-length') > 0) {
                    return response.json();
                }
                return null;
            })
            .then(() => {
                mostrarMensajeExito();
            })
            .catch(error => {
                mostrarMensajeError(error);
            })
            .finally(() => {
                btnSubmit.disabled = false;
                btnSubmit.innerHTML = textoOriginal;
            });
    }
}
function mostrarMensajeExito() {
    Swal.fire({
        title: '¡Éxito!',
        text: 'El producto ha sido actualizado correctamente',
        icon: 'success',
        confirmButtonText: 'Aceptar'
    }).then(() => {
        window.onbeforeunload = null;
        window.location.href = '/Productos/Index';
    });
}

function mostrarMensajeError(error) {
    console.error('Error:', error);
    Swal.fire({
        title: 'Error',
        text: 'Hubo un problema al actualizar el producto.',
        icon: 'error',
        confirmButtonText: 'Aceptar'
    });
}

function configurarBotonesEliminar() {
    const botonesEliminar = document.querySelectorAll('.btn-eliminar-producto, .btn-eliminar');

    botonesEliminar.forEach(boton => {
        boton.addEventListener('click', function() {
            const id = this.getAttribute('data-id');
            eliminarProducto(id);
        });
    });
}

function eliminarProducto(id) {
    Swal.fire({
        title: '¿Está seguro?',
        text: '¿Desea eliminar este producto? Esta acción no se puede deshacer.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6'
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/api/Producto/Delete/${id}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Hubo un problema al eliminar el producto.');
                }
                return response.ok ? Promise.resolve() : response.json();
            })
            .then(() => {
                mostrarModal('Éxito', 'Producto eliminado correctamente.', 'success');
                
                loadProductos(document.getElementById('searchInput')?.value || '');
            })
            .catch(error => {
                console.error('Error:', error);
                mostrarModal('Error', 'No se pudo eliminar el producto. Intente nuevamente.', 'error');
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

function mostrarModal(titulo, mensaje, tipo) {
    Swal.fire({
        title: titulo,
        text: mensaje,
        icon: tipo,
        confirmButtonText: 'Aceptar'
    });
}