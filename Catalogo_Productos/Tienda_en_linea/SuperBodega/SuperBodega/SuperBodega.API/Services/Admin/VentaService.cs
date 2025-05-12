using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Services.Admin
{
    public class VentaService
    {
        private readonly SuperBodegaContext _context;

        public VentaService(SuperBodegaContext context)
        {
            _context = context;
        }

        // Obtener todas las ventas con sus productos
        public async Task<List<Venta>> GetAllAsync() =>
            await _context.Ventas
                .Include(v => v.Productos)
                .ToListAsync();

        // Obtener una venta por su ID
        public async Task<Venta> GetByIdAsync(int id) =>
            await _context.Ventas
                .Include(v => v.Productos)
                .FirstOrDefaultAsync(v => v.Id == id);

        // Crear una nueva venta junto con sus productos
        public async Task<Venta> CreateAsync(CreateVentaDTO dto)
        {
            // Validar existencia de cliente
            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null)
                throw new ArgumentException("El cliente especificado no existe.");

            var venta = new Venta
            {
                Fecha = dto.Fecha,
                ClienteId = dto.ClienteId,
                Estado = EstadoVenta.Pendiente
            };
            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            // Agregar productos a la venta
            foreach (var p in dto.Productos)
            {
                // Validar existencia de producto
                var producto = await _context.Productos.FindAsync(p.ProductoId);
                if (producto == null)
                    throw new ArgumentException($"El producto ID {p.ProductoId} no existe.");

                var vp = new VentaProducto
                {
                    VentaId = venta.Id,
                    ProductoId = p.ProductoId,
                    Cantidad = p.Cantidad,
                    PrecioUnitario = p.PrecioUnitario
                };
                _context.VentaProductos.Add(vp);
            }
            await _context.SaveChangesAsync();

            // Calcular y actualizar total
            venta.Total = await _context.VentaProductos
                .Where(vp => vp.VentaId == venta.Id)
                .SumAsync(vp => vp.Total);
            _context.Ventas.Update(venta);
            await _context.SaveChangesAsync();

            return venta;
        }

        // Actualizar una venta y sus productos
        public async Task<Venta> UpdateAsync(int id, UpdateVentaDTO dto)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null) return null;

            // Validar existencia de cliente
            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null)
                throw new ArgumentException("El cliente especificado no existe.");

            venta.Fecha = dto.Fecha;
            venta.ClienteId = dto.ClienteId;

            // Reemplazar productos existentes
            var antiguos = _context.VentaProductos.Where(vp => vp.VentaId == id);
            _context.VentaProductos.RemoveRange(antiguos);
            await _context.SaveChangesAsync();

            foreach (var p in dto.Productos)
            {
                var producto = await _context.Productos.FindAsync(p.ProductoId);
                if (producto == null)
                    throw new ArgumentException($"El producto ID {p.ProductoId} no existe.");

                _context.VentaProductos.Add(new VentaProducto
                {
                    VentaId = id,
                    ProductoId = p.ProductoId,
                    Cantidad = p.Cantidad,
                    PrecioUnitario = p.PrecioUnitario
                });
            }
            await _context.SaveChangesAsync();

            // Recalcular total
            venta.Total = await _context.VentaProductos
                .Where(vp => vp.VentaId == id)
                .SumAsync(vp => vp.Total);
            _context.Ventas.Update(venta);
            await _context.SaveChangesAsync();

            return venta;
        }

        // Eliminar una venta y sus productos asociados
        public async Task<bool> DeleteAsync(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null) return false;

            var productos = _context.VentaProductos.Where(vp => vp.VentaId == id);
            _context.VentaProductos.RemoveRange(productos);

            _context.Ventas.Remove(venta);
            await _context.SaveChangesAsync();
            return true;
        }

        // Cambiar el estado de una venta
        public async Task<Venta> ChangeStateAsync(int id, UpdateEstadoVentaDTO dto)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null) return null;

            if (!Enum.TryParse<EstadoVenta>(dto.Estado, out var nuevoEstado))
                throw new ArgumentException("Estado inv�lido");

            venta.Estado = nuevoEstado;
            _context.Ventas.Update(venta);
            await _context.SaveChangesAsync();
            return venta;
        }
    }
}

