using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

/// <summary>
/// Controlador para gestionar clientes en el sistema
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]
public class ClienteController : ControllerBase
{
    private readonly ClienteService _clienteService;
    
    public ClienteController(ClienteService clienteService)
    {
        _clienteService = clienteService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllClientesAsync()
    {
        var clientes = await _clienteService.GetAllClientesAsync();
        return Ok(clientes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClienteById(int id)
    {
        var cliente = await _clienteService.GetClienteByIdAsync(id);
        if (cliente == null)
        {
            return NotFound();
        }
        return Ok(cliente);
    }
    
    [HttpPost("Create")]
    public async Task<IActionResult> CreateCliente([FromBody] CreateClienteDTO createClienteDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var newCliente = await _clienteService.CreateClienteAsync(createClienteDto);
        return CreatedAtAction(nameof(GetClienteById), new { id = newCliente.Id }, newCliente);
    }
    
    [HttpPut("Edit/{id}")]
    public async Task<IActionResult> UpdateCliente(int id, [FromBody] UpdateClienteDTO updateClienteDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var updatedCliente = await _clienteService.UpdateClienteAsync(id, updateClienteDto);
        if (updatedCliente == null)
        {
            return NotFound();
        }
        
        return Ok(updatedCliente);
    }
    
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> DeleteCliente(int id)
    {
        var result = await _clienteService.DeleteClienteAsync(id);
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
    
}