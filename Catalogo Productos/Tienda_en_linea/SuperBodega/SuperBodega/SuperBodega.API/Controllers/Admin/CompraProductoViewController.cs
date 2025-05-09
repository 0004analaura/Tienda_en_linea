using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

[Route("Compras/Producto")]
public class CompraProductoViewController : Controller
{
    private readonly CompraProductoService _compraProductoService;
    private readonly ProductoService _productoService;

    private readonly ProveedorService _proveedorService;

    public CompraProductoViewController
    (
        CompraProductoService compraProductoService,
        ProductoService productoService,
        ProveedorService proveedorService
    )
    {
        _compraProductoService = compraProductoService;
        _productoService = productoService;
        _proveedorService = proveedorService;
    }

    [HttpGet]
    [Route("Index")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create()
    {
        var productos = await _productoService.GetAllProductosAsync();
        ViewBag.Productos = productos.Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text = $"{p.Codigo} - {p.Nombre}",
            Group = new SelectListGroup { Name = p.CategoriaNombre },
            Disabled = !p.Estado
        }).ToList();

        // Cargar todos los proveedores
        ViewBag.Proveedores = await GetProveedoresSelectList();


        return View(new CreateCompraProductoDTO());
    }

    private async Task<List<SelectListItem>> GetProveedoresSelectList()
    {
        // Necesitarás inyectar el ProveedorService en el constructor
        var proveedores = await _proveedorService.GetAllProveedoresAsync();
        return proveedores.Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text = p.Nombre,
            Disabled = !p.Estado
        }).ToList();
    }

    [HttpGet]
    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var compraProducto = await _compraProductoService.GetByIdAsync(id);
        if (compraProducto == null) return NotFound();

        // Cargar todos los productos
        var productos = await _productoService.GetAllProductosAsync();
        ViewBag.Productos = productos.Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text = $"{p.Codigo} - {p.Nombre}",
            Selected = p.Id == compraProducto.ProductoId
        }).ToList();

        // Cargar todos los proveedores
        var proveedores = await _proveedorService.GetAllProveedoresAsync();
        ViewBag.Proveedores = proveedores.Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text = p.Nombre,
            Selected = p.Id == compraProducto.ProveedorId
        }).ToList();

        // Convertir a DTO para la vista
        var dto = new CompraProductoDTO
        {
            Id = compraProducto.Id,
            CompraId = compraProducto.CompraId,
            ProductoId = compraProducto.ProductoId,
            ProveedorId = compraProducto.ProveedorId,
            Cantidad = compraProducto.Cantidad,
            PrecioUnitario = compraProducto.PrecioUnitario,
            PrecioVenta = compraProducto.Producto?.PrecioDeVenta ?? 0,
            FechaDeCompra = compraProducto.FechaDeCompra
        };

        return View(dto);
    }
}