using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Ecommerce;

/// <summary>
/// Controlador para gestionar las vistas de productos en el sistema ecommerce
/// </summary>
[Route("Cliente")]
public class EcommerceProductosViewController : Controller
{
    private readonly ProductoService _productoService;
    private readonly CategoriaService _categoriaService;

    public EcommerceProductosViewController(ProductoService productoService, CategoriaService categoriaService)
    {
        _productoService = productoService;
        _categoriaService = categoriaService;
    }

    [HttpGet("Productos")]
    public async Task<IActionResult> Index()
    {
        // Obtiene todos los productos para mostrarlos en la vista
        var productos = await _productoService.GetAllProductosAsync();
        return View("~/Views/Ecommerce/Productos/Productos.cshtml", productos);
    }

    [HttpGet("FiltrarPorCategoria")]
    public async Task<IActionResult> FiltrarPorCategoria()
    {
        // Obtiene todas las categorías para el filtro
        var categorias = await _categoriaService.GetAllCategoriasAsync();
        ViewBag.Categorias = categorias.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Nombre
        }).ToList();

        return View("~/Views/Ecommerce/Productos/FiltrarPorCategoria.cshtml");
    }
}
