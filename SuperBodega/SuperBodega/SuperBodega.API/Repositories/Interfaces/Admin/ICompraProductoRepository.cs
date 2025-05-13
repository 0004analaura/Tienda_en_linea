using SuperBodega.API.Models.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperBodega.API.Repositories.Interfaces.Admin;

namespace SuperBodega.API.Repositories.Interfaces.Admin
{
    public interface ICompraProductoRepository
    {
        Task<IEnumerable<CompraProducto>> GetByCompraIdAsync(int compraId);
        Task<CompraProducto> GetByIdAsync(int id);
        Task AddAsync(CompraProducto cp);
        Task UpdateAsync(CompraProducto cp);
        Task DeleteAsync(int id);
    }
}