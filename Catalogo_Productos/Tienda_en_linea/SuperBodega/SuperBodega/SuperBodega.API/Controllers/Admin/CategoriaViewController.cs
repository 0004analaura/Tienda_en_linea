using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

[Route("Categorias")]
public class CategoriaViewController : Controller
{
    private readonly CategoriaService _categoriaService;
    
    public CategoriaViewController(CategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
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
        var categoria = await _categoriaService.GetCategoriaByIdAsync(id);
        if (categoria == null)
        {
            return NotFound();
        }
        return View(categoria);
    }
}