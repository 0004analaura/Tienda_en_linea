using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Admin;
using SuperBodega.API.Services.Ecommerce;

namespace SuperBodega.API.Controllers.Ecommerce;

/// <summary>
/// Controlador para gestionar las vistas de la realizacion de compras del carrito en el sistema ecommerce
/// </summary>
[Route("Dashboard")]
public class EcommerceRealizarCompraViewController : Controller
{
    private readonly CarritoService _carritoService;
    private readonly ProductoService _productoService;

    public EcommerceRealizarCompraViewController(CarritoService carritoService, ProductoService productoService)
    {
        _carritoService = carritoService;
        _productoService = productoService;
    }

    [HttpGet("RealizarCompra")]
    public async Task<IActionResult> Index()
    {
        // Obtiene el carrito de compras del usuario
        var carrito = await _carritoService.GetCarritoAsync();
        return View("~/Views/Ecommerce/RealizarCompraView/Index.cshtml", carrito);
    }
}