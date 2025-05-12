using SuperBodega.API.Models.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperBodega.API.Repositories.Interfaces.Admin
{
    public interface IVentaRepository
    {
        Task<IEnumerable<Venta>> GetAllVentasAsync();
        Task<Venta> GetVentaByIdAsync(int id);
        Task AddVentaAsync(Venta venta);
        Task UpdateVentaAsync(Venta venta);
        Task DeleteVentaAsync(int id);
        Task ChangeStateAsync(int id, EstadoVenta nuevoEstado);
    }
}
