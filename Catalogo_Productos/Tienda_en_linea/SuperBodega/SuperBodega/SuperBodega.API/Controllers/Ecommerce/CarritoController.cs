using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Ecommerce;
using SuperBodega.API.Services.Ecommerce;

namespace SuperBodega.API.Controllers.Ecommerce;

[Route("api/[controller]")]
[ApiController]
public class CarritoController : ControllerBase
{
    private readonly CarritoService _carritoService;

    public CarritoController(CarritoService carritoService)
    {
        _carritoService = carritoService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CarritoDTO>>> GetAllCarritos()
    {
        var carritos = await _carritoService.GetAllCarritosAsync();
        return Ok(carritos);
    }

    [HttpGet("cliente/{clienteId}")]
    public async Task<ActionResult<CarritoDTO>> GetCarritoByClienteId(int clienteId)
    {
        var carrito = await _carritoService.GetCarritoByClienteIdAsync(clienteId);
        return Ok(carrito);
    }

    [HttpPost("agregar")]
    public async Task<ActionResult<CarritoDTO>> AgregarAlCarrito([FromBody] AgregarAlCarritoDTO dto)
    {
        try
        {
            var carrito = await _carritoService.AgregarAlCarritoAsync(dto);
            return Ok(carrito);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("actualizar-cantidad")]
    public async Task<ActionResult<CarritoDTO>> ActualizarCantidad([FromBody] ActualizarCarritoItemDTO dto)
    {
        try
        {
            var carrito = await _carritoService.ActualizarCantidadItemAsync(dto);
            return Ok(carrito);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("eliminar-item/{itemId}")]
    public async Task<ActionResult<CarritoDTO>> EliminarItemDelCarrito(int itemId)
    {
        try
        {
            var carrito = await _carritoService.EliminarItemDelCarritoAsync(itemId);
            return Ok(carrito);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("vaciar/{clienteId}")]
    public async Task<IActionResult> VaciarCarrito(int clienteId)
    {
        try
        {
            await _carritoService.VaciarCarritoAsync(clienteId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}