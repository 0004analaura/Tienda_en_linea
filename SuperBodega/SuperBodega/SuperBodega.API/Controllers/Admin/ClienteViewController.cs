using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

/// <summary>
/// Controlador para gestionar las vistas de clientes en el sistema
/// </summary>
[Route("Clientes")]
public class ClienteViewController : Controller
{
    private readonly ClienteService _clienteService;
    
    public ClienteViewController(ClienteService clienteService)
    {
        _clienteService = clienteService;
    }
    
    [HttpGet("Index")]
    public IActionResult Index()
    {
        return View();
    }
    
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _clienteService.GetClienteByIdAsync(id);
        if (cliente == null)
        {
            return NotFound();
        }
        return View(cliente);
    }
}