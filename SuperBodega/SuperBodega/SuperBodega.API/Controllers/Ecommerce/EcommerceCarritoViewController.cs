using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Admin;
using SuperBodega.API.Services.Ecommerce;

namespace SuperBodega.API.Controllers.Ecommerce;

[Route("Dashboard")]
public class EcommerceCarritoViewController : Controller
{
    private readonly CarritoService _carritoService;
    private readonly ProductoService _productoService;

    public EcommerceCarritoViewController(CarritoService carritoService, ProductoService productoService)
    {
        _carritoService = carritoService;
        _productoService = productoService;
    }

    [HttpGet("Carrito")]
    public async Task<IActionResult> Index()
    {
        // Obtiene el carrito de compras del usuario
        var carrito = await _carritoService.GetCarritoAsync();
        return View("~/Views/Ecommerce/CarritoView/Index.cshtml" , carrito);
    }
}