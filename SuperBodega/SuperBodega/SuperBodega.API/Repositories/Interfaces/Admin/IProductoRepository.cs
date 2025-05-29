using SuperBodega.API.Models.Admin;

namespace SuperBodega.API.Repositories.Interfaces.Admin;

public interface IProductoRepository : IGenericOperationsRepository<Producto>
{
    Task<IEnumerable<Producto>> GetByCategoriaIdAsync(int categoriaId);
    Task<bool> UpdateStockAsync(int productoId, int cantidad);
}