document.addEventListener('DOMContentLoaded', function () {
    cargarCategorias();
    const categoriaSelect = document.getElementById('categoriaSelect');
    categoriaSelect.addEventListener('change', filtrarProductosPorCategoria);

    function cargarCategorias() {
        fetch('/api/Categoria')
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error al cargar categorías');
                }
                return response.json();
            })
            .then(categorias => {
                const select = document.getElementById('categoriaSelect');
                categorias.forEach(categoria => {
                    const option = document.createElement('option');
                    option.value = categoria.id;
                    option.textContent = categoria.nombre;
                    select.appendChild(option);
                });
            })
            .catch(error => {
                console.error('Error al cargar categorías:', error);
            });
    }

    function filtrarProductosPorCategoria() {
        const categoriaId = categoriaSelect.value;
        const tableBody = document.getElementById('productosFiltradosTableBody');
        if (!categoriaId) {
            tableBody.innerHTML = '<tr><td colspan="6" class="text-center">Seleccione una categoría</td></tr>';
            return;
        }

        tableBody.innerHTML = '<tr><td colspan="6" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></td></tr>';

        fetch(`/api/producto/categoria/${categoriaId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error al cargar productos');
                }
                return response.json();
            })
            .then(data => {
                tableBody.innerHTML = '';
                if (data.length === 0) {
                    tableBody.innerHTML = '<tr><td colspan="6" class="text-center">No se encontraron productos</td></tr>';
                    return;
                }

                data.forEach(producto => {
                    const row = document.createElement('tr');
                    row.innerHTML = `
                        <td><img src="${producto.imagenUrl || '/images/productos/default_product.png'}" class="img-thumbnail" style="width: 50px; height: 50px;"></td>
                        <td>${producto.codigo}</td>
                        <td>${producto.nombre}</td>
                        <td>${producto.descripcion}</td>
                        <td>${producto.stock}</td>
                        <td>${producto.precioDeVenta}</td>
                    `;
                    tableBody.appendChild(row);
                });
            })
            .catch(error => {
                console.error('Error:', error);
                tableBody.innerHTML = `<tr><td colspan="6" class="text-center text-danger">Error al cargar datos: ${error.message}</td></tr>`;
            });
    }
});
