using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperBodega.API.Repositories.Implementations.Admin
{
    public class VentaRepository : IVentaRepository
    {
        private readonly SuperBodegaContext _context;

        public VentaRepository(SuperBodegaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Venta>> GetAllVentasAsync() =>
            await _context.Ventas.Include(v => v.Productos).ToListAsync();

        public async Task<Venta> GetVentaByIdAsync(int id) =>
            await _context.Ventas
                          .Include(v => v.Productos)
                          .FirstOrDefaultAsync(v => v.Id == id);

        public async Task AddVentaAsync(Venta venta)
        {
            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVentaAsync(Venta venta)
        {
            _context.Ventas.Update(venta);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteVentaAsync(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta != null)
            {
                _context.Ventas.Remove(venta);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ChangeStateAsync(int id, EstadoVenta nuevoEstado)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta != null)
            {
                venta.Estado = nuevoEstado;
                await _context.SaveChangesAsync();
            }
        }
    }
}
