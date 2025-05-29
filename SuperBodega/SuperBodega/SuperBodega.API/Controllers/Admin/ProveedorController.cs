using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

/// <summary>
/// Controlador para gestionar proveedores en el sistema
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]public class ProveedorController : ControllerBase
{
    private readonly ProveedorService _proveedorService;
    
    public ProveedorController(ProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllProveedoresAsync()
    {
        var proveedores = await _proveedorService.GetAllProveedoresAsync();
        return Ok(proveedores);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProveedorById(int id)
    {
        var proveedor = await _proveedorService.GetProveedorByIdAsync(id);
        if (proveedor == null)
        {
            return NotFound();
        }
        return Ok(proveedor);
    }
    
    [HttpPost("Create")]
    public async Task<IActionResult> CreateProveedor([FromBody] CreateProveedorDTO proveedorDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var newProveedor = await _proveedorService.CreateProveedorAsync(proveedorDto);
        return CreatedAtAction(nameof(GetProveedorById), new { id = newProveedor.Id }, newProveedor);
    }
    
    [HttpPut("Edit/{id}")]
    public async Task<IActionResult> UpdateProveedor(int id, [FromBody] UpdateProveedorDTO proveedorDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var updatedProveedor = await _proveedorService.UpdateProveedorAsync(id, proveedorDto);
        if (updatedProveedor == null)
        {
            return NotFound();
        }
        
        return Ok(updatedProveedor);
    }
    
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> DeleteProveedor(int id)
    {
        var result = await _proveedorService.DeleteProveedorAsync(id);
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
    
}