using SuperBodega.API.Data;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;

namespace SuperBodega.API.Services.Admin;

public class VentaEstadoService
{
    private readonly SuperBodegaContext _db;
    private readonly IEmailService _email;
    private readonly ILogger<VentaEstadoService> _log;

    public VentaEstadoService(SuperBodegaContext db, IEmailService email,
                              ILogger<VentaEstadoService> log)
    {
        _db = db; _email = email; _log = log;
    }

    public async Task CambiarEstadoAsync(int ventaId, EstadoVenta nuevo,
                                         CancellationToken ct = default)
    {
        var venta = await _db.Ventas
                             .Include(v => v.Cliente)
                             .FirstOrDefaultAsync(v => v.Id == ventaId, ct)
                    ?? throw new KeyNotFoundException($"Venta {ventaId} no existe");

        // Validar que el cambio de estado sea válido
        ValidarCambioEstado(venta.Estado, nuevo);

        if (venta.Estado == nuevo)
        {
            _log.LogInformation("Venta {Id} ya está en estado {Estado}", ventaId, nuevo);
            return;
        }

        var estadoAnterior = venta.Estado;
        venta.Estado = nuevo;
        await _db.SaveChangesAsync(ct);

        await _email.EnviarActualizacionEstadoAsync(ventaId, nuevo.ToString(), ct);

        _log.LogInformation("Venta {Id} cambió de {EstadoAnterior} a {EstadoNuevo}",
            ventaId, estadoAnterior, nuevo);
    }

    private static void ValidarCambioEstado(EstadoVenta estadoActual, EstadoVenta estadoNuevo)
    {
        // Matriz de transiciones válidas
        var transicionesValidas = new Dictionary<EstadoVenta, EstadoVenta[]>
        {
            [EstadoVenta.Recibido] = new[] { EstadoVenta.Despachado },
            [EstadoVenta.Despachado] = new[] { EstadoVenta.Entregado },
            [EstadoVenta.Entregado] = new EstadoVenta[] { } // Estado final, no se puede cambiar
        };

        // Verificar si la transición es válida
        if (!transicionesValidas.ContainsKey(estadoActual))
        {
            throw new InvalidOperationException($"Estado actual '{estadoActual}' no es válido");
        }

        var estadosPermitidos = transicionesValidas[estadoActual];

        if (!estadosPermitidos.Contains(estadoNuevo))
        {
            var mensaje = estadoActual switch
            {
                EstadoVenta.Recibido => "Una venta recibida solo puede pasar a estado 'Despachado'",
                EstadoVenta.Despachado => "Una venta despachada solo puede pasar a estado 'Entregado'",
                EstadoVenta.Entregado => "Una venta entregada no puede cambiar de estado",
                _ => $"No se puede cambiar de '{estadoActual}' a '{estadoNuevo}'"
            };

            throw new InvalidOperationException(mensaje);
        }
    }

    public static EstadoVenta[] ObtenerEstadosPermitidos(EstadoVenta estadoActual)
    {
        return estadoActual switch
        {
            EstadoVenta.Recibido => new[] { EstadoVenta.Despachado },
            EstadoVenta.Despachado => new[] { EstadoVenta.Entregado },
            EstadoVenta.Entregado => new EstadoVenta[] { }, // No se puede cambiar
            _ => new EstadoVenta[] { }
        };
    }
    
    public async Task<Venta?> ObtenerVentaPorIdAsync(int ventaId, CancellationToken ct = default)
    {
        return await _db.Ventas
                        .Include(v => v.Cliente)
                        .FirstOrDefaultAsync(v => v.Id == ventaId, ct);
    }
}