using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

[Route("api/[controller]")]
[ApiController]
public class CompraProductoController : ControllerBase
{
    private readonly CompraProductoService _compraProductoService;

    public CompraProductoController(CompraProductoService compraProductoService)
    {
        _compraProductoService = compraProductoService;
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        var productos = await _compraProductoService.GetAllAsync();
        return Ok(productos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var producto = await _compraProductoService.GetByIdAsync(id);
        if (producto == null)
            return NotFound();

        return Ok(producto);
    }

    [HttpGet("Compra/{compraId}")]
    public async Task<IActionResult> GetProductosByCompraId(int compraId)
    {
        var productos = await _compraProductoService.GetProductosByCompraIdAsync(compraId);
        return Ok(productos);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateCompra([FromBody] CreateCompraProductoDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var newProducto = await _compraProductoService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = newProducto.Id }, newProducto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("Edit/{id}")]
    public async Task<IActionResult> UpdateCompra(int id, [FromBody] UpdateCompraProductoDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedProducto = await _compraProductoService.UpdateAsync(id, dto);
        if (updatedProducto == null)
            return NotFound();

        return Ok(updatedProducto);
    }

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> DeleteCompra(int id)
    {
        var result = await _compraProductoService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}