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
                    card.innerHTML = `
                        <div class="card h-100 text-center shadow-sm">
                            <img src="${producto.imagenUrl || '/images/productos/default_product.png'}" class="card-img-top p-3" alt="${producto.nombre}" style="max-height: 200px; object-fit: contain;">
                            <div class="card-body">
                                <h5 class="card-title fw-bold">${producto.nombre}</h5>
                                <p class="card-text text-muted">${producto.descripcion || 'Sin descripción disponible'}</p>
                                <p class="card-text"><small>1 x ${producto.categoriaNombre || 'N/A'}</small></p>
                                <p class="card-text fs-5 fw-bold text-primary">$${producto.precioDeVenta.toFixed(2)}</p>
                                <div class="d-flex justify-content-center align-items-center mb-3">
                                    <button class="btn btn-outline-secondary btn-sm" onclick="cambiarCantidad(${producto.id}, -1)">-</button>
                                    <input type="number" id="cantidad-${producto.id}" class="form-control text-center mx-2" value="1" min="1" style="width: 60px;" readonly>
                                    <button class="btn btn-outline-secondary btn-sm" onclick="cambiarCantidad(${producto.id}, 1)">+</button>
                                </div>
                                <button class="btn btn-warning text-white fw-bold w-100" onclick="agregarAlCarrito(${producto.id})">Agregar al Carrito</button>
                            </div>
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
    cantidad = Math.max(1, cantidad + cambio);
    inputCantidad.value = cantidad;
}

function agregarAlCarrito(productoId) {
    const cantidad = document.getElementById(`cantidad-${productoId}`).value;
    console.log(`Producto ${productoId} agregado al carrito con cantidad ${cantidad}`);
    // Aquí puedes implementar la lógica para agregar al carrito
}
