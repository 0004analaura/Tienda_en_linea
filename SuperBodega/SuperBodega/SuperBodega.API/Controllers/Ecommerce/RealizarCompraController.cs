using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Ecommerce;
using SuperBodega.API.Infrastructure.Email;

namespace SuperBodega.API.Controllers.Ecommerce;

/// <summary>
/// Controlador para gestionar la realizacion de la compra del carrito en el sistema
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]
public class RealizarCompraController : ControllerBase
{
    private readonly CarritoService _carritoService;
    private readonly IEmailService _emailService;
    private readonly ILogger<RealizarCompraController> _logger;

    public RealizarCompraController(
        CarritoService carritoService, 
        IEmailService emailService, 
        ILogger<RealizarCompraController> logger)
    {
        _carritoService = carritoService;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("confirmar/{clienteId}")]
    public async Task<IActionResult> ConfirmarCompra(int clienteId, [FromBody] ConfirmarCompraRequest? request = null)
    {
        try
        {
            _logger.LogInformation("Iniciando checkout para cliente {ClienteId}", clienteId);

            // 1. Realizar la compra primero
            var ventaId = await _carritoService.RealizarCompraAsync(clienteId);
            
            if (ventaId == 0)
            {
                _logger.LogWarning("RealizarCompraAsync retornó 0 para cliente {ClienteId}", clienteId);
                return BadRequest(new { success = false, message = "No se pudo realizar la compra" });
            }

            _logger.LogInformation("Venta {VentaId} creada exitosamente para cliente {ClienteId}", ventaId, clienteId);

            // 2. Enviar email de manera síncrona pero sin bloquear la respuesta al usuario
            try
            {
                _logger.LogInformation("Enviando email de confirmación para venta {VentaId}", ventaId);
                
                // Enviar email de manera asíncrona DESPUÉS de la transacción
                await _emailService.EnviarPedidoRecibidoAsync(clienteId);
                
                _logger.LogInformation("Email de confirmación enviado para cliente {ClienteId}, venta {VentaId}", clienteId, ventaId);
            }
            catch (Exception emailEx)
            {
                // Log del error pero no fallar toda la operación
                _logger.LogError(emailEx, "Error enviando email para cliente {ClienteId}, venta {VentaId}. La compra SÍ se completó.", clienteId, ventaId);
            }

            return Ok(new { 
                success = true, 
                ventaId = ventaId,
                message = "Compra realizada con éxito",
                nota = "Se ha enviado un email de confirmación"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error de validación en checkout para cliente {ClienteId}: {Message}", clienteId, ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado realizando checkout para cliente {ClienteId}", clienteId);
            return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        }
    }
}

public class ConfirmarCompraRequest
{
    public string? NotasAdicionales { get; set; }
}