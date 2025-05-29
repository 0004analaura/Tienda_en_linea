using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.DTOs.Admin;

namespace SuperBodega.API.Services.Admin;

public class ReporteService
{
    private readonly SuperBodegaContext _ctx;
    public ReporteService(SuperBodegaContext ctx) => _ctx = ctx;

    // a) Ventas por período
    public async Task<IEnumerable<ReporteVentaPeriodoDTO>> VentasPorPeriodoAsync(
        DateTime inicio, DateTime fin)
    {
        return await _ctx.Ventas
            .Where(v => v.Fecha >= inicio && v.Fecha <= fin)
            .GroupBy(v => v.Fecha.Date)
            .Select(g => new ReporteVentaPeriodoDTO
            {
                Fecha = g.Key,
                TotalTransacciones = g.Count(),
                MontoTotal = (decimal)g.Sum(v => v.Total)
            })
            .OrderBy(r => r.Fecha)
            .ToListAsync();
    }

    // b) Ventas por producto
    public async Task<IEnumerable<ReporteVentaProductoDTO>> VentasPorProductoAsync(
        DateTime? inicio = null, DateTime? fin = null)
    {
        var query = _ctx.VentaProductos
                        .Include(vp => vp.Producto)
                        .Include(vp => vp.Venta)
                        .AsQueryable();

        if (inicio.HasValue && fin.HasValue)
            query = query.Where(vp => vp.Venta.Fecha >= inicio && vp.Venta.Fecha <= fin);

        return await query
            .GroupBy(vp => new { vp.ProductoId, vp.Producto.Nombre })
            .Select(g => new ReporteVentaProductoDTO
            {
                ProductoId = g.Key.ProductoId,
                Producto = g.Key.Nombre,
                CantidadVendida = g.Sum(x => x.Cantidad),
                MontoTotal = (decimal)g.Sum(x => x.Cantidad * x.PrecioUnitario)
            }).OrderByDescending(r => r.MontoTotal)
              .ToListAsync();
    }

    // c) Ventas por cliente
    public async Task<IEnumerable<ReporteVentaClienteDTO>> VentasPorClienteAsync(
        DateTime? inicio = null, DateTime? fin = null)
    {
        var query = _ctx.Ventas.Include(v => v.Cliente).AsQueryable();
        if (inicio.HasValue && fin.HasValue)
            query = query.Where(v => v.Fecha >= inicio && v.Fecha <= fin);

        return await query
            .GroupBy(v => new { v.ClienteId, v.Cliente.Nombre })
            .Select(g => new ReporteVentaClienteDTO
            {
                ClienteId = g.Key.ClienteId,
                Cliente = g.Key.Nombre,
                TotalTransacciones = g.Count(),
                MontoTotal = (decimal)g.Sum(v => v.Total)
            }).OrderByDescending(r => r.MontoTotal)
              .ToListAsync();
    }

    // d) Ventas por proveedor
    public async Task<IEnumerable<ReporteVentaProveedorDTO>> VentasPorProveedorAsync(
        DateTime? inicio = null, DateTime? fin = null)
    {
        var query = from vp in _ctx.VentaProductos
                    join p in _ctx.Productos on vp.ProductoId equals p.Id
                    join prov in _ctx.Proveedores on p.ProveedorId equals prov.Id
                    join v in _ctx.Ventas on vp.VentaId equals v.Id
                    select new { vp, p, prov, v };

        if (inicio.HasValue && fin.HasValue)
            query = query.Where(x => x.v.Fecha >= inicio && x.v.Fecha <= fin);

        return await query
            .GroupBy(x => new { x.prov.Id, x.prov.Nombre })
            .Select(g => new ReporteVentaProveedorDTO
            {
                ProveedorId = g.Key.Id,
                Proveedor = g.Key.Nombre,
                CantidadProductosVendidos = g.Sum(x => x.vp.Cantidad),
                MontoTotal = g.Sum(x =>
                    x.vp.Cantidad * (x.vp.PrecioUnitario ?? 0))
            })
            .OrderByDescending(r => r.MontoTotal)
            .ToListAsync();
    }


}
