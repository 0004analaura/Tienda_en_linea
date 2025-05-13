using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperBodega.API.Repositories.Implementations.Admin
{
    public class CompraProductoRepository : ICompraProductoRepository
    {
        private readonly SuperBodegaContext _context;

        public CompraProductoRepository(SuperBodegaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CompraProducto>> GetByCompraIdAsync(int compraId) =>
            await _context.CompraProductos
                          .Where(cp => cp.CompraId == compraId)
                          .Include(cp => cp.Producto)
                          .ToListAsync();

        public async Task<CompraProducto> GetByIdAsync(int id) =>
            await _context.CompraProductos
                          .Include(cp => cp.Producto)
                          .FirstOrDefaultAsync(cp => cp.Id == id);

        public async Task AddAsync(CompraProducto cp)
        {
            _context.CompraProductos.Add(cp);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CompraProducto cp)
        {
            _context.CompraProductos.Update(cp);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var cp = await _context.CompraProductos.FindAsync(id);
            if (cp != null)
            {
                _context.CompraProductos.Remove(cp);
                await _context.SaveChangesAsync();
            }
        }
    }
}


