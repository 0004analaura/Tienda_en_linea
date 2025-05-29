using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Events;
using SuperBodega.API.Infrastructure.Messaging;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Services.Admin
{
    public class VentaService
    {
        private readonly SuperBodegaContext _context;
        private readonly IServiceProvider _sp;
        public VentaService(SuperBodegaContext context, IServiceProvider sp)
        {
            _context = context;
            _sp = sp;
        }

        public async Task<IEnumerable<VentaViewDTO>> GetAllDtoAsync()
        {
            return await _context.Ventas
                .Include(v => v.Cliente)
                .Select(static v => new VentaViewDTO
                {
                    Id = v.Id,
                    Fecha = v.Fecha,
                    ClienteId = v.ClienteId,
                    Cliente = v.Cliente,
                    Total = v.Total,
                    Estado = v.Estado,
                    Productos = v.Productos
                        .Select(p => new VentaProducto
                        {
                            Id = p.Id,
                            ProductoId = p.ProductoId,
                            Cantidad = p.Cantidad,
                            PrecioUnitario = p.PrecioUnitario
                        })
                        .ToList()
                })
                .ToListAsync();
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

            // Manejar fecha correctamente
            DateTime fechaVenta;
            if (dto.Fecha.Kind == DateTimeKind.Unspecified)
            {
                // Si viene sin especificar, asumimos que es hora local de Guatemala
                fechaVenta = dto.Fecha;
            }
            else
            {
                // Convertir a zona horaria de Guatemala
                var guatemalaZone = TimeZoneInfo.CreateCustomTimeZone("Guatemala", new TimeSpan(-6, 0, 0), "Guatemala", "GT");
                fechaVenta = dto.Fecha.Kind == DateTimeKind.Utc 
                    ? TimeZoneInfo.ConvertTimeFromUtc(dto.Fecha, guatemalaZone)
                    : dto.Fecha;
                fechaVenta = DateTime.SpecifyKind(fechaVenta, DateTimeKind.Unspecified);
            }

            var venta = new Venta
            {
                Fecha = fechaVenta,
                ClienteId = dto.ClienteId,
                Estado = EstadoVenta.Recibido
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

            await _sp.GetRequiredService<IKafkaProducerService>()
                .PublishAsync("ventas",
                new VentaCreadaEvent(venta.Id, venta.Fecha,
                venta.Productos.Select(p => new VentaLineaEvent(p.ProductoId, p.Cantidad, p.PrecioUnitario ?? 0))));


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
                throw new ArgumentException("Estado invalido");

            venta.Estado = nuevoEstado;
            _context.Ventas.Update(venta);
            await _context.SaveChangesAsync();
            return venta;
        }
    }
}

