using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;

namespace SuperBodega.API.Services.Admin
{
    public class ProductoService
    {
        private readonly IProductoRepository _productoRepository;

        public ProductoService(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
        }

        public async Task<IEnumerable<ProductoDTO>> GetAllProductosAsync()
        {
            var productos = await _productoRepository.GetAllAsync();
            return productos.Select(p => new ProductoDTO
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                CategoriaId = p.CategoriaId,
                CategoriaNombre = p.Categoria.Nombre,
                Stock = p.Stock,
                PrecioDeCompra = p.PrecioDeCompra,
                PrecioDeVenta = p.PrecioDeVenta,
                Estado = p.Estado,
                ImagenUrl = p.ImagenUrl,
                FechaDeRegistro = p.FechaDeRegistro
            });
        }

        public async Task<ProductoDTO> GetProductoByIdAsync(int id)
        {
            var producto = await _productoRepository.GetByIdAsync(id);
            if (producto == null)
            {
                return null;
            }

            return new ProductoDTO
            {
                Id = producto.Id,
                Codigo = producto.Codigo,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                CategoriaId = producto.CategoriaId,
                CategoriaNombre = producto.Categoria.Nombre,
                Stock = producto.Stock,
                PrecioDeCompra = producto.PrecioDeCompra,
                PrecioDeVenta = producto.PrecioDeVenta,
                Estado = producto.Estado,
                ImagenUrl = producto.ImagenUrl,
                FechaDeRegistro = producto.FechaDeRegistro
            };
        }

        public async Task<IEnumerable<ProductoDTO>> GetProductosByCategoriaIdAsync(int categoriaId)
        {
            var productos = await _productoRepository.GetByCategoriaIdAsync(categoriaId);
            return productos.Select(p => new ProductoDTO
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                CategoriaId = p.CategoriaId,
                CategoriaNombre = p.Categoria.Nombre,
                Stock = p.Stock,
                PrecioDeCompra = p.PrecioDeCompra,
                PrecioDeVenta = p.PrecioDeVenta,
                Estado = p.Estado,
                ImagenUrl = p.ImagenUrl,
                FechaDeRegistro = p.FechaDeRegistro
            });
        }

        public async Task<ProductoDTO> CreateProductoAsync(CreateProductoDTO createProductoDTO)
        {
            var producto = new Producto
            {
                Codigo = createProductoDTO.Codigo,
                Nombre = createProductoDTO.Nombre,
                Descripcion = createProductoDTO.Descripcion,
                CategoriaId = createProductoDTO.CategoriaId,
                Stock = createProductoDTO.Stock,
                PrecioDeCompra = createProductoDTO.PrecioDeCompra,
                PrecioDeVenta = createProductoDTO.PrecioDeVenta,
                Estado = createProductoDTO.Estado,
                ImagenUrl = createProductoDTO.ImagenUrl,
                FechaDeRegistro = createProductoDTO.FechaDeRegistro
            };
            
            var newProducto = await _productoRepository.AddAsync(producto);
            
            // Obtener el nombre de la categoría o manejar el caso null
            string categoriaNombre = null;
            if (newProducto.Categoria != null)
            {
                categoriaNombre = newProducto.Categoria.Nombre;
            }
            else
            {
                // Obtener el nombre de la categoría manualmente
                var productosDeCategoria = await _productoRepository.GetByCategoriaIdAsync(newProducto.CategoriaId);
                var primerProducto = productosDeCategoria.FirstOrDefault();
                    categoriaNombre = primerProducto?.Categoria?.Nombre ?? "Categoría no disponible";            
            }
            
            return new ProductoDTO
            {
                Id = newProducto.Id,
                Codigo = newProducto.Codigo,
                Nombre = newProducto.Nombre,
                Descripcion = newProducto.Descripcion,
                CategoriaId = newProducto.CategoriaId,
                CategoriaNombre = categoriaNombre,
                Stock = newProducto.Stock,
                PrecioDeCompra = newProducto.PrecioDeCompra,
                PrecioDeVenta = newProducto.PrecioDeVenta,
                Estado = newProducto.Estado,
                ImagenUrl = newProducto.ImagenUrl,
                FechaDeRegistro = newProducto.FechaDeRegistro
            };
        }

        public async Task<ProductoDTO> UpdateProductoAsync(int id, UpdateProductoDTO updateProductoDTO)
        {
            var existingProducto = await _productoRepository.GetByIdAsync(id);
            if (existingProducto == null)
            {
                return null;
            }
        
            existingProducto.Codigo = updateProductoDTO.Codigo;
            existingProducto.Nombre = updateProductoDTO.Nombre;
            existingProducto.Descripcion = updateProductoDTO.Descripcion ?? string.Empty;
            existingProducto.CategoriaId = updateProductoDTO.CategoriaId;
            existingProducto.PrecioDeCompra = updateProductoDTO.PrecioDeCompra;
            existingProducto.PrecioDeVenta = updateProductoDTO.PrecioDeVenta;
            existingProducto.Estado = updateProductoDTO.Estado;
            
            // Asegurarse de que ImagenUrl nunca sea null
            existingProducto.ImagenUrl = !string.IsNullOrEmpty(updateProductoDTO.ImagenUrl) 
                ? updateProductoDTO.ImagenUrl 
                : "/images/productos/default_product.png";
            
            var updateProducto = await _productoRepository.UpdateAsync(existingProducto);
            return new ProductoDTO
            {
                Id = updateProducto.Id,
                Codigo = updateProducto.Codigo,
                Nombre = updateProducto.Nombre,
                Descripcion = updateProducto.Descripcion,
                CategoriaId = updateProducto.CategoriaId,
                CategoriaNombre = updateProducto.Categoria?.Nombre ?? "Sin categoría",
                Stock = updateProducto.Stock,
                PrecioDeCompra = updateProducto.PrecioDeCompra,
                PrecioDeVenta = updateProducto.PrecioDeVenta,
                Estado = updateProducto.Estado,
                ImagenUrl = updateProducto.ImagenUrl,
                FechaDeRegistro = updateProducto.FechaDeRegistro
            };
        }

        public async Task<bool> DeleteProductoAsync(int id)
        {
            return await _productoRepository.DeleteAsync(id);
        }

        public async Task<bool> UpdateStockAsync(int productoId, int cantidad)
        {
            return await _productoRepository.UpdateStockAsync(productoId, cantidad);
        }
    }
}