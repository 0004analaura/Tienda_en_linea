document.addEventListener('DOMContentLoaded', function () {
    cargarProductosCliente();
});

function cargarProductosCliente() {
    const listaProductos = document.getElementById('lista-productos');
    
    if (!listaProductos) {
        console.error('Elemento lista-productos no encontrado');
        return;
    }

    // Mostrar indicador de carga
    listaProductos.innerHTML = `
        <div class="col-12 text-center">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Cargando productos...</span>
            </div>
            <p class="mt-2">Cargando productos disponibles...</p>
        </div>
    `;

    fetch('http://localhost:8080/api/Producto/GetAll')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al cargar productos');
            }
            return response.json();
        })
        .then(productos => {
            listaProductos.innerHTML = '';
            
            if (productos.length === 0) {
                listaProductos.innerHTML = `
                    <div class="col-12 text-center">
                        <div class="alert alert-info">
                            <i class="bi bi-info-circle me-2"></i>
                            No hay productos disponibles en este momento.
                        </div>
                    </div>
                `;
                return;
            }

            // Filtrar solo productos activos y con stock
            const productosDisponibles = productos.filter(p => p.estado && p.stock > 0);
            
            if (productosDisponibles.length === 0) {
                listaProductos.innerHTML = `
                    <div class="col-12 text-center">
                        <div class="alert alert-warning">
                            <i class="bi bi-exclamation-triangle me-2"></i>
                            No hay productos disponibles con stock en este momento.
                        </div>
                    </div>
                `;
                return;
            }

            productosDisponibles.forEach(producto => {
                const card = document.createElement('div');
                card.className = 'col';
                
                const categoria = producto.categoriaNombre || producto.categoria || 'Sin categoría';
                const descripcion = producto.descripcion || 'Sin descripción disponible';
                const imagenUrl = producto.imagenUrl || '/images/productos/default_product.png';
                
                card.innerHTML = `
                    <div class="card h-100 shadow-sm">
                        <div class="position-relative">
                            <img src="${imagenUrl}" 
                                 class="card-img-top" 
                                 alt="${producto.nombre}"
                                 style="height: 200px; object-fit: cover;"
                                 onerror="this.src='/images/productos/default_product.png'">
                            <div class="position-absolute top-0 end-0 m-2">
                                <span class="badge bg-success">Disponible</span>
                            </div>
                        </div>
                        <div class="card-body d-flex flex-column">
                            <h5 class="card-title text-primary">${producto.nombre}</h5>
                            <p class="card-text text-muted small">${descripcion}</p>
                            <div class="mt-auto">
                                <div class="row g-2 mb-3">
                                    <div class="col-6">
                                        <small class="text-muted">Precio:</small>
                                        <div class="fw-bold text-success fs-5">Q${formatearPrecio(producto.precioDeVenta)}</div>
                                    </div>
                                    <div class="col-6">
                                        <small class="text-muted">Stock:</small>
                                        <div class="fw-bold">${producto.stock} unidades</div>
                                    </div>
                                </div>
                                <div class="mb-3">
                                    <small class="text-muted">Categoría:</small>
                                    <div class="badge bg-light text-dark">${categoria}</div>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
                listaProductos.appendChild(card);
            });

            console.log(`Se cargaron ${productosDisponibles.length} productos disponibles`);
        })
        .catch(error => {
            console.error('Error al cargar productos del cliente:', error);
            listaProductos.innerHTML = `
                <div class="col-12 text-center">
                    <div class="alert alert-danger">
                        <i class="bi bi-exclamation-triangle me-2"></i>
                        Error al cargar los productos: ${error.message}
                        <br><small>Por favor, recarga la página o intenta más tarde.</small>
                    </div>
                </div>
            `;
        });
}

// Función auxiliar para formatear precios
function formatearPrecio(precio) {
    if (precio === undefined || precio === null) return '0.00';
    return new Intl.NumberFormat('es-GT', { 
        minimumFractionDigits: 2, 
        maximumFractionDigits: 2 
    }).format(precio);
}
