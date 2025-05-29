using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin
{
    /// <summary>
    /// Controlador para gestionar estados de las ventas en el sistema
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/ventas")]
    public class VentasEstadoController : ControllerBase
    {
        private readonly VentaEstadoService _svc;
        private readonly ILogger<VentasEstadoController> _logger;

        public VentasEstadoController(VentaEstadoService svc, ILogger<VentasEstadoController> logger)
        {
            _svc = svc;
            _logger = logger;
        }

        // PUT api/v2/ventas/42/estado/2
        [HttpPut("{id:int}/estado/{estado:int}")]
        public async Task<IActionResult> Cambiar(int id, int estado)
        {
            try
            {
                if (!Enum.IsDefined(typeof(EstadoVenta), estado))
                {
                    return BadRequest($"Estado '{estado}' no es válido. Estados válidos: 1=Recibido, 2=Despachado, 3=Entregado");
                }

                await _svc.CambiarEstadoAsync(id, (EstadoVenta)estado);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Venta no encontrada: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cambio de estado inválido: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cambiar estado de venta {VentaId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET api/v2/ventas/42/estados-permitidos
        [HttpGet("{id:int}/estados-permitidos")]
        public async Task<IActionResult> ObtenerEstadosPermitidos(int id)
        {
            try
            {
                var venta = await _svc.ObtenerVentaPorIdAsync(id);
                if (venta == null)
                {
                    return NotFound($"Venta {id} no encontrada");
                }

                var estadosPermitidos = VentaEstadoService.ObtenerEstadosPermitidos(venta.Estado);
                
                return Ok(new
                {
                    estadoActual = venta.Estado,
                    estadosPermitidos = estadosPermitidos.Select(e => new
                    {
                        valor = (int)e,
                        nombre = e.ToString()
                    }).ToArray()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estados permitidos para venta {VentaId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}