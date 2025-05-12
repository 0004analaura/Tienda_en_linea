using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Ecommerce;

namespace SuperBodega.API.Controllers.Ecommerce;

[Route("api/[controller]")]
[ApiController]
public class RealizarCompraController : ControllerBase
{
    private readonly CarritoService _carritoService;

    public RealizarCompraController(CarritoService carritoService)
    {
        _carritoService = carritoService;
    }

    [HttpPost("confirmar/{clienteId}")]
    public async Task<IActionResult> ConfirmarCompra(int clienteId)
    {
        try
        {
            var result = await _carritoService.RealizarCompraAsync(clienteId);
            return Ok(new { success = result, message = "Compra realizada con éxito" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}