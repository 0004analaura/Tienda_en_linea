using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

[Route("Productos")]
public class ProductoViewController : Controller
{
    private readonly ProductoService _productoService;
    private readonly CategoriaService _categoriaService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    
    public ProductoViewController(ProductoService productoService, CategoriaService categoriaService, IWebHostEnvironment webHostEnvironment)
    {
        _productoService = productoService;
        _categoriaService = categoriaService;
        _webHostEnvironment = webHostEnvironment;
    }
    
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var productos = await _productoService.GetAllProductosAsync();
        return View(productos);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var categorias = await _categoriaService.GetAllCategoriasAsync();
        ViewBag.Categorias = categorias.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Nombre
        }).ToList();

        return View(new CreateProductoDTO());
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var producto = await _productoService.GetProductoByIdAsync(id);
        if (producto == null)
        {
            return NotFound();
        }

        var categorias = await _categoriaService.GetAllCategoriasAsync();
        ViewBag.Categorias = categorias.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Nombre
        }).ToList();

        var updateProductoDTO = new UpdateProductoDTO
        {
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            CategoriaId = producto.CategoriaId,
            PrecioDeCompra = producto.PrecioDeCompra,
            PrecioDeVenta = producto.PrecioDeVenta,
            Estado = producto.Estado,
            ImagenUrl = producto.ImagenUrl
        };

        ViewBag.Producto = producto;
        ViewBag.ProductoId = id;

        return View(updateProductoDTO);
    }

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _productoService.GetProductoByIdAsync(id);
        if (producto == null)
        {
            return NotFound();
        }

        // Eliminar la imagen si existe
        if (!string.IsNullOrEmpty(producto.ImagenUrl))
        {
            var rutaImagen = Path.Combine(_webHostEnvironment.WebRootPath, producto.ImagenUrl.TrimStart('/'));
            if (System.IO.File.Exists(rutaImagen))
            {
                System.IO.File.Delete(rutaImagen);
            }
        }

        var result = await _productoService.DeleteProductoAsync(id);
        if (!result)
        {
            return StatusCode(500, new { success = false, message = "No se pudo eliminar el producto" });
        }

        return Json(new { success = true, message = "Producto eliminado correctamente" });
    }
    
    
}