document.addEventListener('DOMContentLoaded', function() {
    const clienteSelect = document.getElementById('clienteSelect');
    const carritoTableBody = document.getElementById('carritoTableBody');
    const subtotalElement = document.getElementById('subtotal');
    const totalElement = document.getElementById('total');
    const btnVaciarCarrito = document.getElementById('btnVaciarCarrito');
    const btnRealizarCompra = document.getElementById('btnRealizarCompra');

    let clienteActual = null;
    let carritoActual = null;

    // Verificar si hay un clienteId en la URL
    const urlParams = new URLSearchParams(window.location.search);
    const clienteIdParam = urlParams.get('clienteId');

    // Cargar la lista de clientes
    cargarClientes().then(() => {
        // Si hay un clienteId en la URL, seleccionarlo
        if (clienteIdParam) {
            clienteSelect.value = clienteIdParam;
            cargarCarritoCliente();
        }
    });

    // Event listeners
    clienteSelect.addEventListener('change', cargarCarritoCliente);
    btnVaciarCarrito.addEventListener('click', vaciarCarrito);
    btnRealizarCompra.addEventListener('click', realizarCompra);

    async function cargarClientes() { 
        try {
            const response = await fetch('/api/Cliente');
            if (!response.ok) {
                throw new Error('Error al cargar los clientes');
            }

            const clientes = await response.json();
            clienteSelect.innerHTML = '<option value="">Selecciona un cliente...</option>';

            clientes.forEach(cliente => {
                const option = document.createElement('option');
                option.value = cliente.id;
                option.textContent = `${cliente.nombre} ${cliente.apellido}`;
                clienteSelect.appendChild(option);
            });
            return clientes;
        } catch (error) {
            console.error('Error:', error);
            mostrarAlerta('Error al cargar los clientes', 'danger');
            return [];
        }
    }

    function cargarCarritoCliente() {
        const clienteId = clienteSelect.value;

        if (!clienteId) {
            limpiarCarrito();
            return;
        }

        clienteActual = clienteId;

        // Cargar información del cliente
        fetch(`/api/Cliente/${clienteId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error al cargar la información del cliente');
                }
                return response.json();
            })
            .then(cliente => {
                mostrarInformacionCliente(cliente);
            })
            .catch(error => {
                console.error('Error:', error);
                mostrarAlerta('Error al cargar la información del cliente', 'danger');
            });

        // Cargar carrito del cliente
        fetch(`/api/Carrito/cliente/${clienteId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error al cargar el carrito');
                }
                return response.json();
            })
            .then(carrito => {
                carritoActual = carrito;
                mostrarCarrito(carrito);
                actualizarBotonesCarrito();
            })
            .catch(error => {
                console.error('Error:', error);
                mostrarAlerta('Error al cargar el carrito', 'danger');
            });
    }

    function mostrarInformacionCliente(cliente) {
        const clienteInfo = document.getElementById('clienteInfo');
        clienteInfo.innerHTML = `
            <div class="row">
                <div class="col-md-6">
                    <p class="mb-1"><strong>Nombre:</strong> ${cliente.nombre} ${cliente.apellido}</p>
                    <p class="mb-1"><strong>Email:</strong> ${cliente.email}</p>
                    <p class="mb-1"><strong>Teléfono:</strong> ${cliente.telefono}</p>
                </div>
                <div class="col-md-6">
                    <p class="mb-1"><strong>Dirección:</strong> ${cliente.direccion}</p>
                    <p class="mb-1"><strong>Estado:</strong> <span class="badge ${cliente.estado ? 'bg-success' : 'bg-danger'}">${cliente.estado ? 'Activo' : 'Inactivo'}</span></p>
                    <p class="mb-1"><strong>Registro:</strong> ${formatearFecha(cliente.fechaDeRegistro)}</p>
                </div>
            </div>
        `;
    }

    function mostrarCarrito(carrito) {
        if (!carrito.items || carrito.items.length === 0) {
            carritoTableBody.innerHTML = '<tr><td colspan="6" class="text-center">No hay productos en el carrito</td></tr>';
            subtotalElement.textContent = 'Q0.00';
            totalElement.textContent = 'Q0.00';
            return;
        }

        carritoTableBody.innerHTML = '';

        carrito.items.forEach(item => {
            const row = document.createElement('tr');
            row.innerHTML = `
            <td>
                <img src="${item.productoImagenUrl || '/images/productos/default_product.png'}" 
                     alt="${item.productoNombre}" 
                     class="img-thumbnail" 
                     style="width: 50px; height: 50px; object-fit: contain;">
            </td>
            <td>${item.productoNombre}</td>
            <td>Q${item.precioUnitario.toFixed(2)}</td>
            <td>
                <div class="input-group input-group-sm" style="width: 120px;">
                    <button class="btn btn-outline-secondary" type="button" onclick="cambiarCantidadCarrito(${item.id}, -1)">-</button>
                    <input type="text" class="form-control text-center" value="${item.cantidad}" 
                           data-item-id="${item.id}" 
                           data-producto-id="${item.productoId}" 
                           onchange="actualizarCantidadCarrito(this)">
                    <button class="btn btn-outline-secondary" type="button" onclick="cambiarCantidadCarrito(${item.id}, 1)">+</button>
                </div>
            </td>
            <td>Q${item.subtotal.toFixed(2)}</td>
            <td>
                <button class="btn btn-outline-danger btn-sm" onclick="eliminarItemCarrito(${item.id})">
                    <i class="bi bi-trash"></i>
                </button>
            </td>
        `;
            carritoTableBody.appendChild(row);
        });

        subtotalElement.textContent = `Q${carrito.total.toFixed(2)}`;
        totalElement.textContent = `Q${carrito.total.toFixed(2)}`;
    }

    function actualizarBotonesCarrito() {
        const hayItems = carritoActual && carritoActual.items && carritoActual.items.length > 0;
        btnVaciarCarrito.disabled = !hayItems;
        btnRealizarCompra.disabled = !hayItems;
    }

    function limpiarCarrito() {
        carritoTableBody.innerHTML = '<tr><td colspan="6" class="text-center">No hay productos en el carrito</td></tr>';
        subtotalElement.textContent = '$0.00';
        totalElement.textContent = '$0.00';
        btnVaciarCarrito.disabled = true;
        btnRealizarCompra.disabled = true;

        document.getElementById('clienteInfo').innerHTML = '<p class="text-muted mb-0">Seleccione un cliente para ver su información</p>';
    }

    function vaciarCarrito() {
        if (!clienteActual) return;

        Swal.fire({
            title: '¿Estás seguro?',
            text: 'Esta acción vaciará el carrito del cliente',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Sí, vaciar',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                // Vaciar el carrito mediante API
                fetch(`/api/Carrito/vaciar/${clienteActual}`, {
                    method: 'DELETE'
                })
                    .then(response => {
                        if (!response.ok) {
                            throw new Error('Error al vaciar el carrito');
                        }

                        // Mostrar alerta de éxito
                        Swal.fire({
                            title: '¡Carrito vaciado!',
                            text: 'El carrito ha sido vaciado exitosamente',
                            icon: 'success',
                            toast: true,
                            position: 'top-end',
                            showConfirmButton: false,
                            timer: 3000,
                            timerProgressBar: true
                        });

                        // Recargar el carrito
                        cargarCarritoCliente();
                    })
                    .catch(error => {
                        console.error('Error:', error);
                        Swal.fire({
                            title: 'Error',
                            text: error.message,
                            icon: 'error',
                            confirmButtonText: 'Aceptar'
                        });
                    });
            }
        });
    }

    function realizarCompra() {
        if (!clienteActual) return;
        window.location.href = `/Dashboard/RealizarCompra?clienteId=${clienteActual}`;
    }

    function mostrarAlerta(mensaje, tipo) {
        const alertContainer = document.createElement('div');
        alertContainer.className = `alert alert-${tipo} alert-dismissible fade show`;
        alertContainer.innerHTML = `
            ${mensaje}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;

        document.querySelector('.container-fluid').insertBefore(alertContainer, document.querySelector('.container-fluid').firstChild);

        setTimeout(() => {
            alertContainer.remove();
        }, 5000);
    }

    function formatearFecha(fechaStr) {
        const fecha = new Date(fechaStr);
        return fecha.toLocaleDateString('es-ES', {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    window.cargarCarritoCliente = cargarCarritoCliente;
});

// Funciones globales para interactuar con el carrito
function cambiarCantidadCarrito(itemId, cambio) {
    const inputCantidad = document.querySelector(`input[data-item-id="${itemId}"]`);
    if (!inputCantidad) return;

    let cantidad = parseInt(inputCantidad.value) || 1;
    const nuevaCantidad = Math.max(1, cantidad + cambio);

    const productoId = inputCantidad.getAttribute('data-producto-id');

    fetch(`/api/Producto/${productoId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al verificar stock');
            }
            return response.json();
        })
        .then(producto => {
            if (nuevaCantidad > producto.stock) {
                Swal.fire({
                    title: 'Stock insuficiente',
                    text: `Solo hay ${producto.stock} unidades disponibles de este producto`,
                    icon: 'warning',
                    confirmButtonText: 'Entendido'
                });
                inputCantidad.value = producto.stock;
                actualizarCantidadCarrito(inputCantidad);
            } else {
                inputCantidad.value = nuevaCantidad;
                actualizarCantidadCarrito(inputCantidad);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            Swal.fire({
                title: 'Advertencia',
                text: 'No se pudo verificar el stock disponible',
                icon: 'warning',
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 3000
            });
        });
}

function actualizarCantidadCarrito(input) {
    const itemId = input.getAttribute('data-item-id');
    const productoId = input.getAttribute('data-producto-id');
    const cantidad = parseInt(input.value);

    if (isNaN(cantidad) || cantidad < 1) {
        input.value = 1;
        return;
    }

    fetch(`/api/Producto/${productoId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al verificar stock');
            }
            return response.json();
        })
        .then(producto => {
            if (cantidad > producto.stock) {
                Swal.fire({
                    title: 'Stock insuficiente',
                    text: `Solo hay ${producto.stock} unidades disponibles de este producto`,
                    icon: 'warning',
                    confirmButtonText: 'Entendido'
                });
                input.value = producto.stock;
                actualizarCantidadEnServidor(itemId, producto.stock);
            } else {
                actualizarCantidadEnServidor(itemId, cantidad);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            Swal.fire({
                title: 'Advertencia',
                text: 'No se pudo verificar el stock disponible',
                icon: 'warning',
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 3000
            });
        });
}

function actualizarCantidadEnServidor(itemId, cantidad) {
    fetch('/api/Carrito/actualizar-cantidad', {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            carritoItemId: itemId,
            cantidad: cantidad
        })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al actualizar la cantidad');
            }
            return response.json();
        })
        .then(carrito => {
            const subtotalElement = document.getElementById('subtotal');
            const totalElement = document.getElementById('total');

            const itemRow = document.querySelector(`input[data-item-id="${itemId}"]`).closest('tr');
            const subtotalCell = itemRow.querySelector('td:nth-child(5)');
            const item = carrito.items.find(i => i.id === parseInt(itemId));
            if (item && subtotalCell) {
                subtotalCell.textContent = `Q${item.subtotal.toFixed(2)}`;
            }

            subtotalElement.textContent = `Q${carrito.total.toFixed(2)}`;
            totalElement.textContent = `Q${carrito.total.toFixed(2)}`;

            const btnVaciarCarrito = document.getElementById('btnVaciarCarrito');
            const btnRealizarCompra = document.getElementById('btnRealizarCompra');
            const hayItems = carrito.items && carrito.items.length > 0;
            btnVaciarCarrito.disabled = !hayItems;
            btnRealizarCompra.disabled = !hayItems;
        })
        .catch(error => {
            console.error('Error:', error);
            Swal.fire({
                title: 'Error',
                text: error.message || 'Error al actualizar la cantidad',
                icon: 'error',
                confirmButtonText: 'Aceptar'
            });
        });
}

function eliminarItemCarrito(itemId) {
    Swal.fire({
        title: '¿Estás seguro?',
        text: '¿Deseas eliminar este producto del carrito?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/api/Carrito/eliminar-item/${itemId}`, {
                method: 'DELETE'
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Error al eliminar el producto del carrito');
                    }
                    return response.json();
                })
                .then(carrito => {
                    // Mostrar alerta de éxito
                    Swal.fire({
                        title: '¡Eliminado!',
                        text: 'El producto ha sido eliminado del carrito',
                        icon: 'success',
                        toast: true,
                        position: 'top-end',
                        showConfirmButton: false,
                        timer: 3000,
                        timerProgressBar: true
                    });

                    // Recargar el carrito
                    window.cargarCarritoCliente();
                })
                .catch(error => {
                    console.error('Error:', error);
                    Swal.fire({
                        title: 'Error',
                        text: error.message,
                        icon: 'error',
                        confirmButtonText: 'Aceptar'
                    });
                });
        }
    });
}