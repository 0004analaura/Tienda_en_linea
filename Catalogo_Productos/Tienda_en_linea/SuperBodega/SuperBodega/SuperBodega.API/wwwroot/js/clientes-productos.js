
document.addEventListener('DOMContentLoaded', function () {
    fetch('http://localhost:8080/api/productos')
        .then(res => res.json())
        .then(productos => {
            const lista = document.getElementById('lista-productos');
            lista.innerHTML = '';
            productos.forEach(p => {
                const card = document.createElement('div');
                card.className = 'col';
                card.innerHTML = `
                    <div class="card h-100">
                        <div class="card-body">
                            <h5 class="card-title">${p.nombre}</h5>
                            <p class="card-text">Precio: Q${p.precioDeVenta}</p>
                            <p class="card-text">Stock: ${p.stock}</p>
                            <p class="card-text">Categoría: ${p.categoria || 'Sin categoría'}</p>
                        </div>
                    </div>
                `;
                lista.appendChild(card);
            });
        })
        .catch(err => {
            console.error('Error al cargar productos del cliente:', err);
        });
});
