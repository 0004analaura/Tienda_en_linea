document.addEventListener('DOMContentLoaded', function() {
    const clienteSelect = document.getElementById('clienteSelect');
    const compraTableBody = document.getElementById('compraTableBody');
    const subtotalElement = document.getElementById('subtotal');
    const totalElement = document.getElementById('total');
    const btnConfirmarCompra = document.getElementById('btnConfirmarCompra');
    const direccionEntregaInput = document.getElementById('direccionEntrega');

    const btnVolverCarrito = document.querySelector('a[href="/Carrito/Index"], a[href*="Dashboard/Carrito"]');

    let clienteActual = null;
    let carritoActual = null;

    // Verificar si hay un clienteId en la URL
    const urlParams = new URLSearchParams(window.location.search);
    const clienteIdParam = urlParams.get('clienteId');

    if (btnVolverCarrito) {
        btnVolverCarrito.addEventListener('click', function(e) {
            if (clienteActual) {
                e.preventDefault();
                window.location.href = `/Dashboard/Carrito?clienteId=${clienteActual}`;
            }
        });
    }

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
    btnConfirmarCompra.addEventListener('click', confirmarCompra);

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

        } catch (error) {
            console.error('Error:', error);
            mostrarAlerta('Error al cargar los clientes', 'danger');
        }
    }

    function cargarCarritoCliente() {
        const clienteId = clienteSelect.value;

        if (!clienteId) {
            limpiarDatos();
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
                if (direccionEntregaInput) {
                    direccionEntregaInput.value = cliente.direccion || '';
                }
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
                mostrarProductosCompra(carrito);
                actualizarBotonesCompra();
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

    function mostrarProductosCompra(carrito) {
        if (!carrito.items || carrito.items.length === 0) {
            compraTableBody.innerHTML = '<tr><td colspan="4" class="text-center">No hay productos en el carrito</td></tr>';
            subtotalElement.textContent = 'Q0.00';
            totalElement.textContent = 'Q0.00';
            return;
        }

        compraTableBody.innerHTML = '';

        carrito.items.forEach(item => {
            const row = document.createElement('tr');
            row.innerHTML = `
            <td>
                <div class="d-flex align-items-center">
                    <img src="${item.productoImagenUrl || '/images/productos/default_product.png'}" 
                         alt="${item.productoNombre}" 
                         class="img-thumbnail me-3" 
                         style="width: 50px; height: 50px; object-fit: contain;">
                    <span>${item.productoNombre}</span>
                </div>
            </td>
            <td>Q${item.precioUnitario.toFixed(2)}</td>
            <td>${item.cantidad}</td>
            <td>Q${item.subtotal.toFixed(2)}</td>
        `;
            compraTableBody.appendChild(row);
        });

        subtotalElement.textContent = `Q${carrito.total.toFixed(2)}`;
        totalElement.textContent = `Q${carrito.total.toFixed(2)}`;
    }

    function actualizarBotonesCompra() {
        const hayItems = carritoActual && carritoActual.items && carritoActual.items.length > 0;
        btnConfirmarCompra.disabled = !hayItems;
    }

    function limpiarDatos() {
        compraTableBody.innerHTML = '<tr><td colspan="5" class="text-center">No hay productos seleccionados</td></tr>';
        subtotalElement.textContent = '$0.00';
        totalElement.textContent = '$0.00';
        btnConfirmarCompra.disabled = true;

        if (direccionEntregaInput) {
            direccionEntregaInput.value = '';
        }

        document.getElementById('clienteInfo').innerHTML = '<p class="text-muted mb-0">Seleccione un cliente para ver su información</p>';
    }

    function confirmarCompra() {
        if (!clienteActual) return;

        Swal.fire({
            title: '¿Confirmar compra?',
            text: 'Esta acción completará la compra y actualizará el inventario',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#dc3545',
            confirmButtonText: 'Sí, confirmar',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                // Mostrar loader mientras se procesa la compra
                Swal.fire({
                    title: 'Procesando...',
                    text: 'Por favor espere mientras procesamos su compra',
                    allowOutsideClick: false,
                    didOpen: () => {
                        Swal.showLoading();
                    }
                });

                //fetch(`/api/RealizarCompra/confirmar/${clienteActual}`, {
                //    method: 'POST',
                //    headers: {
                //        'Content-Type': 'application/json',
                //    },
                //    body: JSON.stringify({
                //        notasAdicionales: document.getElementById('notasAdicionales').value
                //    })
                //})
                //    .then(response => {
                //        if (!response.ok) {
                //            return response.json().then(data => {
                //                throw new Error(data.message || 'Error al procesar la compra');
                //            });
                //        }
                //        return response.json();
                //    })


                const payload = {
                    clienteId: parseInt(clienteActual),
                    items: carritoActual.items.map(i => ({
                        productoId: i.productoId,
                        cantidad: i.cantidad
                    })),
                    notasAdicionales: document.getElementById('notasAdicionales').value
                };
                fetch('/api/v2/CarritoV/checkout', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                })
                    .then(response => {
                        if (response.status !== 202) {
                            return response.text().then(t => {
                                throw new Error(t || 'La API no aceptó la compra');
                            });
                        }
                        return 202;
                    })
                    //.then(data => {
                    //    Swal.fire({
                    //        title: '¡Compra realizada!',
                    //        text: 'La compra se ha procesado correctamente',
                    //        icon: 'success',
                    //        confirmButtonColor: '#28a745'
                    //    }).then(() => {
                    //        window.location.href = '/Dashboard/Productos';
                    //    });
                    //})
                    .then(() => {
                        Swal.fire({
                            title: '¡Pedido recibido!',
                            text: 'Te enviaremos un correo con el detalle en unos instantes',
                            icon: 'success',
                            confirmButtonColor: '#28a745'
                        }).then(() => {
                            window.location.href = '/Cliente/Productos';
                        });
                    })
                    .catch(error => {
                        console.error('Error:', error);
                        Swal.fire({
                            title: 'Error',
                            text: error.message,
                            icon: 'error',
                            confirmButtonColor: '#dc3545'
                        });
                    });
            }
        });
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
});