/**
 * Script para gestionar la funcionalidad de productos en compras
 */
document.addEventListener('DOMContentLoaded', function () {
    // Inicialización para la vista de creación de productos en compra
    const productoSelector = document.getElementById('productoSelector');
    if (productoSelector) {
        inicializarVistaCreacion();
    }

    // Inicialización para la vista de edición
    const formEditarCompraProducto = document.getElementById('formEditarCompraProducto');
    if (formEditarCompraProducto) {
        inicializarVistaEdicion();
    }

    // Inicialización para la vista de listado
    const compraProductosTable = document.getElementById('compraProductosTable');
    if (compraProductosTable) {
        cargarCompraProductos();
    }
});

/**
 * Inicializa la vista de creación de CompraProducto
 */
function inicializarVistaCreacion() {
    const productoSelector = document.getElementById('productoSelector');
    const cantidadInput = document.getElementById('cantidadInput');
    const precioCompraInput = document.getElementById('precioCompraInput');
    const precioVentaInput = document.getElementById('precioVentaInput');
    const totalCompraInput = document.getElementById('totalCompraInput');
    const detallesProducto = document.getElementById('detallesProducto');
    const btnGuardar = document.getElementById('btnGuardar');

    // Initialize registration date field
    const fechaCompraInput = document.getElementById('fechaDeCompra');
    const fechaCompraVisibleInput = document.getElementById('fechaDeCompraVisible');

    if (fechaCompraInput && fechaCompraVisibleInput) {
        const now = new Date();

        const ano = now.getFullYear();
        const mes = (now.getMonth() + 1).toString().padStart(2, '0');
        const dia = now.getDate().toString().padStart(2, '0');
        const horas = now.getHours().toString().padStart(2, '0');
        const minutos = now.getMinutes().toString().padStart(2, '0');

        fechaCompraInput.value = `${ano}-${mes}-${dia}T${horas}:${minutos}`;
        fechaCompraVisibleInput.value = formatearFechaAmPm(now);
    }

    // Inicializar con valores por defecto
    if (cantidadInput) cantidadInput.value = "1";
    calcularTotal();

    // Evento al seleccionar un producto
    if (productoSelector) {
        productoSelector.addEventListener('change', function () {
            if (this.value) {
                fetchDetallesProducto(this.value);
            } else {
                if (detallesProducto) detallesProducto.style.display = 'none';
                if (precioCompraInput) precioCompraInput.value = "";
                if (precioVentaInput) precioVentaInput.value = "";
                calcularTotal();
            }
        });
    }

    // Recalcular cuando cambie la cantidad o el precio
    if (cantidadInput) cantidadInput.addEventListener('input', calcularTotal);
    if (precioCompraInput) precioCompraInput.addEventListener('input', calcularTotal);

    // Evento para el botón guardar
    if (btnGuardar) {
        btnGuardar.addEventListener('click', function () {
            guardarCompraProducto();
        });
    }
}

/**
 * Inicializa la vista de edición de CompraProducto
 */
function inicializarVistaEdicion() {
    const cantidadInput = document.getElementById('cantidadInput');
    const precioCompraInput = document.getElementById('precioCompraInput');
    const precioVentaInput = document.getElementById('precioVentaInput');
    const totalCompraInput = document.getElementById('totalCompraInput');
    const btnUpdateCompra = document.getElementById('btnUpdateCompra');

    // Recalcular cuando cambie la cantidad o el precio
    if (cantidadInput) cantidadInput.addEventListener('input', calcularTotal);
    if (precioCompraInput) precioCompraInput.addEventListener('input', calcularTotal);

    // Check if we have the update button (Edit view)
    if (btnUpdateCompra) {
        console.log("Update button found, adding event listener");
        btnUpdateCompra.addEventListener('click', function (e) {
            e.preventDefault();
            console.log("Update button clicked");
            actualizarCompraProducto();
        });
    }
}

function formatearFechaAmPm(fechaStr) {
    if (!fechaStr) return '';

    const fecha = new Date(fechaStr);

    if (isNaN(fecha.getTime())) return 'Fecha invalida';

    // Create date in Guatemala timezone (UTC-6)
    const options = {
        timeZone: 'America/Guatemala',
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
    };

    return new Intl.DateTimeFormat('es-GT', options).format(fecha);
}

/**
 * Carga los productos de compras para la vista de listado
 */
function cargarCompraProductos() {
    const tableBody = document.getElementById('compraProductosTableBody');
    if (!tableBody) return;

    // Mostrar indicador de carga
    tableBody.innerHTML = '<tr><td colspan="9" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></td></tr>';

    // Llamada a la API para obtener todos los productos de compras
    fetch('/api/CompraProducto/GetAll')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al cargar productos de compras');
            }
            return response.json();
        })
        .then(data => {
            if (data.length === 0) {
                tableBody.innerHTML = '<tr><td colspan="9" class="text-center">No hay productos en compras registrados</td></tr>';
                return;
            }

            // Limpiar la tabla
            tableBody.innerHTML = '';

            // Rellenar la tabla con los datos
            data.forEach(item => {
                const row = document.createElement('tr');

                // Formatear fecha
                const fechaFormateada = item.fechaDeCompra ? formatearFechaAmPm(item.fechaDeCompra) : '-';

                // Formatear precio y total
                const precioUnitarioFormateado = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(item.precioUnitario);
                const precioVentaFormateado = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(item.precioVenta);
                const totalFormateado = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(item.total);

                row.innerHTML = `
                    <td>${item.id}</td>
                    <td>${item.nombreProveedor || '-'}</td>
                    <td>${item.nombreProducto || '-'}</td>
                    <td>${item.cantidad}</td>
                    <td>${precioUnitarioFormateado}</td>
                    <td>${precioVentaFormateado}</td>
                    <td>${totalFormateado}</td>
                    <td>${fechaFormateada}</td>
                    <td>
                        <div class="d-flex gap-2">
                            <a href="/Compras/Producto/Edit/${item.id}" class="btn btn-primary btn-sm">
                                <i class="bi bi-pencil-square"></i>
                            </a>
                            <button onclick="eliminarCompraProducto(${item.id})" class="btn btn-danger btn-sm">
                                <i class="bi bi-trash"></i>
                            </button>
                        </div>
                    </td>
                `;

                tableBody.appendChild(row);
            });
        })
        .catch(error => {
            console.error('Error:', error);
            tableBody.innerHTML = `<tr><td colspan="9" class="text-center text-danger">Error al cargar los datos: ${error.message}</td></tr>`;
            mostrarAlerta('Error al cargar los productos de compras', 'danger');
        });
}

/**
 * Obtiene detalles del producto seleccionado
 * @param {string} productoId - ID del producto a consultar
 */
function fetchDetallesProducto(productoId) {
    const detallesProducto = document.getElementById('detallesProducto');
    const precioCompraInput = document.getElementById('precioCompraInput');
    const precioVentaInput = document.getElementById('precioVentaInput');

    fetch(`/api/Producto/${productoId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al cargar detalles del producto');
            }
            return response.json();
        })
        .then(producto => {
            // Mostrar detalles del producto
            if (document.getElementById('nombreProducto')) {
                document.getElementById('nombreProducto').textContent = producto.nombre;
            }
            if (document.getElementById('codigoProducto')) {
                document.getElementById('codigoProducto').textContent = producto.codigo;
            }
            if (document.getElementById('categoriaProducto')) {
                document.getElementById('categoriaProducto').textContent = producto.categoriaNombre;
            }
            if (document.getElementById('stockProducto')) {
                document.getElementById('stockProducto').textContent = producto.stock;
            }

            // Establecer precios
            if (precioCompraInput) {
                precioCompraInput.value = producto.precioDeCompra || "";
            }
            if (precioVentaInput) {
                precioVentaInput.value = producto.precioDeVenta || "";
            }

            // Mostrar panel de detalles
            if (detallesProducto) {
                detallesProducto.style.display = 'block';
            }

            // Calcular total
            calcularTotal();
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarAlerta('Error al cargar los detalles del producto', 'danger');
        });
}

/**
 * Calcula el total basado en precio unitario y cantidad
 */
function calcularTotal() {
    const cantidadInput = document.getElementById('cantidadInput');
    const precioCompraInput = document.getElementById('precioCompraInput');
    const totalCompraInput = document.getElementById('totalCompraInput');

    if (cantidadInput && precioCompraInput && totalCompraInput) {
        const cantidad = parseFloat(cantidadInput.value) || 0;
        const precioCompra = parseFloat(precioCompraInput.value) || 0;
        const total = cantidad * precioCompra;
        totalCompraInput.value = total.toFixed(2);
    }
}

/**
 * Envía los datos para crear un nuevo producto en una compra
 */
function guardarCompraProducto() {
    if (!validarFormulario()) {
        return false;
    }

    const productoId = document.getElementById('productoSelector').value;
    const proveedorId = document.getElementById('ProveedorId').value;
    const cantidad = document.getElementById('cantidadInput').value;
    const precioUnitario = document.getElementById('precioCompraInput').value;
    const precioVenta = document.getElementById('precioVentaInput').value;
    const fechaDeCompra = document.getElementById('fechaDeCompra').value;

    // Disable the button and show loading state
    const btnGuardar = document.getElementById('btnGuardar');
    const originalText = btnGuardar.innerHTML;
    btnGuardar.disabled = true;
    btnGuardar.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Guardando...';

    // Datos para la API
    const compraProductoData = {
        productoId: parseInt(productoId),
        proveedorId: parseInt(proveedorId),
        cantidad: parseInt(cantidad),
        precioUnitario: parseFloat(precioUnitario),
        precioVenta: parseFloat(precioVenta),
        fechaDeCompra: fechaDeCompra
    };

    // Mostrar datos que se enviarán (solo para depuración)
    console.log("Enviando datos:", compraProductoData);

    fetch('/api/CompraProducto/Create', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(compraProductoData)
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => {
                    throw new Error(text || 'Error al guardar el producto en la compra');
                });
            }
            return response.json();
        })
        .then(data => {
            // Reset the button state
            btnGuardar.disabled = false;
            btnGuardar.innerHTML = originalText;

            // Show success alert and wait for user confirmation before redirecting
            Swal.fire({
                title: '¡Exito!',
                text: 'Producto agregado a la compra correctamente',
                icon: 'success',
                confirmButtonText: 'Aceptar',
                allowOutsideClick: false
            }).then((result) => {
                // Only redirect after the user clicks the confirm button
                if (result.isConfirmed) {
                    window.location.href = '/Compras/Producto/Index';
                }
            });
        })
        .catch(error => {
            console.error('Error:', error);

            // Reset the button state
            btnGuardar.disabled = false;
            btnGuardar.innerHTML = originalText;

            Swal.fire({
                title: 'Error',
                text: 'Error al guardar: ' + error.message,
                icon: 'error',
                confirmButtonText: 'Aceptar'
            });
        });
}

/**
 * Actualiza un producto de compra existente
 */
function actualizarCompraProducto() {
    console.log("Starting update process");
    if (!validarFormulario()) {
        console.log("Form validation failed");
        return false;
    }

    const id = document.querySelector('input[name="Id"]').value;
    const compraId = document.querySelector('input[name="CompraId"]').value;
    const productoId = document.getElementById('ProductoId').value;
    const proveedorId = document.getElementById('ProveedorId').value;
    const cantidad = document.getElementById('cantidadInput').value;
    const precioUnitario = document.getElementById('precioCompraInput').value;
    const precioVenta = document.getElementById('precioVentaInput').value;

    // Get the date from the input field
    const fechaDeCompraInput = document.querySelector('input[name="FechaDeCompra"]');
    let fechaDeCompra = null;

    if (fechaDeCompraInput && fechaDeCompraInput.value) {
        // Convert to ISO format that ASP.NET Core can understand
        const fecha = new Date(fechaDeCompraInput.value);
        if (!isNaN(fecha.getTime())) {
            fechaDeCompra = fecha.toISOString();
        }
    }

    console.log("Form data collected:", { id, compraId, productoId, proveedorId, cantidad, precioUnitario, precioVenta, fechaDeCompra });

    // Disable the button and show loading state
    const btnUpdateCompra = document.getElementById('btnUpdateCompra');
    const originalText = btnUpdateCompra.innerHTML;
    btnUpdateCompra.disabled = true;
    btnUpdateCompra.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Actualizando...';

    // This is the dto object expected by the API
    const dto = {
        compraId: parseInt(compraId),
        productoId: parseInt(productoId),
        proveedorId: parseInt(proveedorId),
        cantidad: parseInt(cantidad),
        precioUnitario: parseFloat(precioUnitario),
        precioVenta: parseFloat(precioVenta),
        fechaDeCompra: fechaDeCompra
    };

    console.log("Sending update data:", dto);

    fetch(`/api/CompraProducto/Edit/${id}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(dto)
    })
        .then(response => {
            console.log("Response received:", response);
            if (!response.ok) {
                return response.text().then(text => {
                    console.error("Error response text:", text);
                    throw new Error(text || 'Error al actualizar el producto en la compra');
                });
            }
            if (response.status === 204) {
                return null;
            }
            return response.json();
        })
        .then((data) => {
            console.log("Update successful, response data:", data);

            // Reset the button state
            btnUpdateCompra.disabled = false;
            btnUpdateCompra.innerHTML = originalText;

            // Show success alert and wait for user confirmation before redirecting
            Swal.fire({
                title: '¡Exito!',
                text: 'Producto actualizado correctamente',
                icon: 'success',
                confirmButtonText: 'Aceptar',
                allowOutsideClick: false
            }).then((result) => {
                // Only redirect after the user clicks the confirm button
                if (result.isConfirmed) {
                    window.location.href = '/Compras/Producto/Index';
                }
            });
        })
        .catch(error => {
            console.error('Error in update:', error);

            // Reset the button state
            btnUpdateCompra.disabled = false;
            btnUpdateCompra.innerHTML = originalText;

            Swal.fire({
                title: 'Error',
                text: 'Error al actualizar: ' + error.message,
                icon: 'error',
                confirmButtonText: 'Aceptar'
            });
        });
}

/**
 * Elimina un producto de compra
 * @param {number} id - ID del CompraProducto a eliminar
 */
function eliminarCompraProducto(id) {
    Swal.fire({
        title: 'Esta seguro?',
        text: 'Desea eliminar este producto de la compra? Esta accion no se puede deshacer.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Si, eliminar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6'
    }).then((result) => {
        if (result.isConfirmed) {
            // Log the endpoint for debugging
            console.log(`Intentando eliminar producto de compra con ID: ${id}`);

            fetch(`/api/CompraProducto/Delete/${id}`, {
                method: 'DELETE'
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Error al eliminar el producto de la compra');
                    }

                    Swal.fire({
                        title: '¡Eliminado!',
                        text: 'Producto eliminado de la compra correctamente',
                        icon: 'success',
                        confirmButtonText: 'Aceptar'
                    }).then(() => {
                        // Recargar la tabla
                        cargarCompraProductos();
                    });
                })
                .catch(error => {
                    console.error('Error:', error);
                    Swal.fire({
                        title: 'Error',
                        text: 'Ocurrio un error al eliminar el producto de la compra',
                        icon: 'error',
                        confirmButtonText: 'Aceptar'
                    });
                });
        }
    });
}

/**
 * Valida el formulario antes de enviarlo
 * @returns {boolean} - Indica si el formulario es válido
 */
function validarFormulario() {
    const productoSelector = document.getElementById('productoSelector');
    const productoId = document.getElementById('ProductoId');
    const proveedorSelector = document.getElementById('ProveedorId');
    const cantidadInput = document.getElementById('cantidadInput');
    const precioCompraInput = document.getElementById('precioCompraInput');
    const precioVentaInput = document.getElementById('precioVentaInput');

    let isValid = true;

    // Check for product - different selectors in Create vs Edit views
    if (productoSelector && !productoSelector.value) {
        mostrarAlerta('Debe seleccionar un producto', 'danger');
        isValid = false;
    }

    // For Edit view, ProductoId is already a hidden field with a value

    // Validar que se haya seleccionado un proveedor (if the field exists)
    if (proveedorSelector && !proveedorSelector.value) {
        mostrarAlerta('Debe seleccionar un proveedor', 'danger');
        isValid = false;
    }

    // Validate quantity
    if (cantidadInput) {
        const cantidad = cantidadInput.value;
        if (!cantidad || cantidad < 1) {
            mostrarAlerta('La cantidad debe ser mayor a 0', 'danger');
            isValid = false;
        }
    }

    // Validate purchase price
    if (precioCompraInput) {
        const precioCompra = precioCompraInput.value;
        if (!precioCompra || precioCompra <= 0) {
            mostrarAlerta('El precio de compra debe ser mayor a 0', 'danger');
            isValid = false;
        }
    }

    // Validate sale price
    if (precioVentaInput) {
        const precioVenta = precioVentaInput.value;
        if (!precioVenta || precioVenta <= 0) {
            mostrarAlerta('El precio de venta debe ser mayor a 0', 'danger');
            isValid = false;
        }

        if (parseFloat(precioVentaInput.value) <= parseFloat(precioCompraInput.value)) {
            mostrarAlerta('El precio de venta debe ser mayor al precio de compra', 'warning');
            isValid = false;
        }
    }

    return isValid;
}

/**
 * Muestra una alerta en el panel de alertas
 * @param {string} mensaje - Mensaje a mostrar 
 * @param {string} tipo - Tipo de alerta (success, danger, warning, info)
 */
function mostrarAlerta(mensaje, tipo) {
    const alertContainer = document.getElementById('alertContainer');
    if (!alertContainer) return;

    alertContainer.style.display = 'block';

    const alerta = document.createElement('div');
    alerta.className = `alert alert-${tipo} alert-dismissible fade show`;
    alerta.innerHTML = `
        <i class="bi bi-exclamation-triangle-fill me-2"></i>
        ${mensaje}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;

    // Limpiar alertas anteriores
    alertContainer.innerHTML = '';
    alertContainer.appendChild(alerta);

    // Auto-eliminar después de 5 segundos
    setTimeout(() => {
        alerta.classList.remove('show');
        setTimeout(() => {
            if (alertContainer.contains(alerta)) {
                alertContainer.removeChild(alerta);
            }
            if (alertContainer.childElementCount === 0) {
                alertContainer.style.display = 'none';
            }
        }, 150);
    }, 5000);
}