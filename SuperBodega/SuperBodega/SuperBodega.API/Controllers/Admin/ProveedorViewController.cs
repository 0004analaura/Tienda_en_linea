using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

[Route("Proveedores")]
public class ProveedorViewController : Controller
{
    private readonly ProveedorService _proveedorService;
    
    public ProveedorViewController(ProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
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
        var proveedor = await _proveedorService.GetProveedorByIdAsync(id);
        if (proveedor == null)
        {
            return NotFound();
        }
        return View(proveedor);
    }
}