document.addEventListener('DOMContentLoaded', function () {
    cargarProductos();

    function cargarProductos() {
        const listaProductos = document.getElementById('lista-productos');
        if (!listaProductos) return;

        listaProductos.innerHTML = '<div class="text-center w-100"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></div>';

        fetch('/api/Producto/GetAll')
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error al cargar productos');
                }
                return response.json();
            })
            .then(data => {
                listaProductos.innerHTML = '';
                if (data.length === 0) {
                    listaProductos.innerHTML = '<div class="text-center w-100">No se encontraron productos</div>';
                    return;
                }

                data.forEach(producto => {
                    const card = document.createElement('div');
                    card.className = 'col';

                    const outOfStock = producto.stock <= 0;

                    card.innerHTML = `
                        <div class="card h-100 text-center shadow-sm ${outOfStock ? 'opacity-75' : ''}">
                            ${outOfStock ? '<div class="position-absolute top-0 end-0 m-2"><span class="badge bg-danger">Agotado</span></div>' : ''}
                            <img src="${producto.imagenUrl || '/images/productos/default_product.png'}" class="card-img-top p-3" alt="${producto.nombre}" style="max-height: 200px; object-fit: contain;">
                            <div class="card-body">
                                <h5 class="card-title fw-bold">${producto.nombre}</h5>
                                <p class="card-text text-muted">${producto.descripcion || 'Sin descripción disponible'}</p>
                                <p class="card-text"><small>1 x ${producto.categoriaNombre || 'N/A'}</small></p>
                                <p class="card-text fs-5 fw-bold text-primary">Q${producto.precioDeVenta.toFixed(2)}</p>
                                <div class="d-flex justify-content-center align-items-center mb-3">
                                    <button class="btn btn-outline-secondary btn-sm" onclick="cambiarCantidad(${producto.id}, -1)" ${outOfStock ? 'disabled' : ''}>-</button>
                                    <input type="number" id="cantidad-${producto.id}" class="form-control text-center mx-2" value="1" min="1" style="width: 60px;" readonly>
                                    <button class="btn btn-outline-secondary btn-sm" onclick="cambiarCantidad(${producto.id}, 1)" ${outOfStock ? 'disabled' : ''}>+</button>
                                </div>
                                ${outOfStock ?
                        '<button class="btn btn-secondary text-white fw-bold w-100" disabled>Sin existencias</button>' :
                        `<button class="btn btn-warning text-white fw-bold w-100" onclick="agregarAlCarrito(${producto.id})">Agregar al Carrito</button>`
                    }
                            </div>
                            ${outOfStock ? '<div class="card-footer text-muted">Producto temporalmente agotado</div>' : ''}
                        </div>
                    `;
                    listaProductos.appendChild(card);
                });
            })
            .catch(error => {
                console.error('Error:', error);
                listaProductos.innerHTML = `<div class="text-center w-100 text-danger">Error al cargar datos: ${error.message}</div>`;
            });
    }
});

function cambiarCantidad(productoId, cambio) {
    const inputCantidad = document.getElementById(`cantidad-${productoId}`);
    if (!inputCantidad) return;

    let cantidad = parseInt(inputCantidad.value) || 1;
    const nuevaCantidad = Math.max(1, cantidad + cambio);

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
                // Establecer la cantidad al máximo disponible
                inputCantidad.value = producto.stock;
            } else {
                // Stock suficiente, actualizar cantidad
                inputCantidad.value = nuevaCantidad;
            }
        })
        .catch(error => {
            console.error('Error:', error);
            // Si hay un error, permitir el cambio pero mostrar una advertencia
            inputCantidad.value = nuevaCantidad;
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

function agregarAlCarrito(productoId) {
    const cantidad = parseInt(document.getElementById(`cantidad-${productoId}`).value) || 1;

    // Verificar stock antes de mostrar el diálogo de cliente
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
                // Actualizar el input de cantidad con el máximo disponible
                document.getElementById(`cantidad-${productoId}`).value = producto.stock;
                return;
            }

            // Continuar con la selección del cliente y agregar al carrito
            mostrarDialogoSeleccionCliente(productoId, cantidad);
        })
        .catch(error => {
            console.error('Error:', error);
            Swal.fire({
                title: 'Error',
                text: 'No se pudo verificar el stock disponible',
                icon: 'error',
                confirmButtonText: 'Aceptar'
            });
        });
}

function mostrarDialogoSeleccionCliente(productoId, cantidad) {
    // Modal para seleccionar el cliente
    Swal.fire({
        title: 'Seleccionar Cliente',
        html: '<div class="mb-3"><select id="swal-cliente-select" class="form-select"></select></div>',
        showCancelButton: true,
        confirmButtonText: 'Agregar al Carrito',
        cancelButtonText: 'Cancelar',
        didOpen: () => {
            const clienteSelect = document.getElementById('swal-cliente-select');

            // Cargar clientes
            fetch('/api/Cliente')
                .then(response => response.json())
                .then(clientes => {
                    clienteSelect.innerHTML = '<option value="">Selecciona un cliente...</option>';
                    clientes.forEach(cliente => {
                        const option = document.createElement('option');
                        option.value = cliente.id;
                        option.textContent = `${cliente.nombre} ${cliente.apellido}`;
                        clienteSelect.appendChild(option);
                    });
                })
                .catch(error => {
                    console.error('Error al cargar clientes:', error);
                    Swal.showValidationMessage('Error al cargar la lista de clientes');
                });
        },
        preConfirm: () => {
            const clienteId = document.getElementById('swal-cliente-select').value;

            if (!clienteId) {
                Swal.showValidationMessage('Debes seleccionar un cliente');
                return false;
            }

            // Enviar solicitud para agregar al carrito
            return fetch('/api/Carrito/agregar', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    clienteId: parseInt(clienteId),
                    productoId: productoId,
                    cantidad: cantidad
                })
            })
                .then(response => {
                    if (!response.ok) {
                        return response.text().then(text => {
                            throw new Error(text);
                        });
                    }
                    return {
                        response: response.json(),
                        clienteId: clienteId
                    };
                })
                .catch(error => {
                    console.error('Error:', error);
                    Swal.showValidationMessage(`Error: ${error.message}`);
                    throw error;
                });
        }
    }).then((result) => {
        if (result.isConfirmed) {
            const clienteId = result.value.clienteId;

            Swal.fire({
                title: '¡Producto agregado!',
                text: 'El producto se ha agregado al carrito exitosamente',
                icon: 'success',
                confirmButtonText: 'Ver Carrito',
                showCancelButton: true,
                cancelButtonText: 'Seguir comprando'
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = `/Dashboard/Carrito?clienteId=${clienteId}`;
                }
            });
        }
    });
}