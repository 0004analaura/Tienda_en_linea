using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

/// <summary>
/// Controlador para gestionar productos en el sistema
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]
public class ProductoController : ControllerBase
{
    private readonly ProductoService _productoService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductoController(ProductoService productoService, IWebHostEnvironment webHostEnvironment)
    {
        _productoService = productoService;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet("GetAll")]
    public async Task<ActionResult<IEnumerable<ProductoDTO>>> GetProductos()
    {
        var productos = await _productoService.GetAllProductosAsync();
        return Ok(productos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductoDTO>> GetProducto(int id)
    {
        var producto = await _productoService.GetProductoByIdAsync(id);
        if (producto == null)
        {
            return NotFound();
        }

        return Ok(producto);
    }

    [HttpGet("categoria/{id}")]
    public async Task<ActionResult<IEnumerable<ProductoDTO>>> GetProductosByCategoria(int id)
    {
        var productos = await _productoService.GetProductosByCategoriaIdAsync(id);
        return Ok(productos);
    }

    [HttpPost("Create")]
    public async Task<ActionResult<ProductoDTO>> CreateProducto([FromForm] CreateProductoDTO productoDto, IFormFile? Imagen)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Manejar la imagen si existe
        if (Imagen != null && Imagen.Length > 0)
        {
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productos");
            
            // Crear directorio si no existe
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);
            
            // Generar un nombre único para la imagen
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Imagen.FileName);
            var filePath = Path.Combine(uploadPath, fileName);
            
            // Guardar la imagen
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await Imagen.CopyToAsync(stream);
            }
            
            // Guardar la ruta relativa en el DTO
            productoDto.ImagenUrl = $"/images/productos/{fileName}";
        }
        else
        {
            productoDto.ImagenUrl = "/images/productos/default_product.png"; // Ruta por defecto
        }

        var producto = await _productoService.CreateProductoAsync(productoDto);
        return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
    }

    [HttpPut("Edit/{id}")]
    public async Task<IActionResult> UpdateProducto(int id, [FromBody] UpdateProductoDTO updateProductoDTO)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _productoService.UpdateProductoAsync(id, updateProductoDTO);
        if (result == null)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("EditImage/{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProductoWithImage(int id, [FromForm] UpdateProductoDTO updateProductoDTO, IFormFile Imagen)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
    
        var productoExistente = await _productoService.GetProductoByIdAsync(id);
        if (productoExistente == null)
        {
            return NotFound();
        }
    
        // Process the image if one is provided
        if (Imagen != null && Imagen.Length > 0)
        {
            // If replacing an existing image, delete the old image file
            if (!string.IsNullOrEmpty(productoExistente.ImagenUrl))
            {
                var rutaImagenAntigua = Path.Combine(_webHostEnvironment.WebRootPath, productoExistente.ImagenUrl.TrimStart('/'));
                if (System.IO.File.Exists(rutaImagenAntigua))
                {
                    System.IO.File.Delete(rutaImagenAntigua);
                }
            }
    
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productos");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);
    
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Imagen.FileName);
            var filePath = Path.Combine(uploadPath, fileName);
    
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await Imagen.CopyToAsync(stream);
            }
    
            updateProductoDTO.ImagenUrl = $"/images/productos/{fileName}";
        }
        else
        {
            // If no new image provided, retain the existing one
            updateProductoDTO.ImagenUrl = productoExistente.ImagenUrl;
        }
    
        var result = await _productoService.UpdateProductoAsync(id, updateProductoDTO);
        if (result == null)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        var result = await _productoService.DeleteProductoAsync(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{id}/stock/{cantidad}")]
    public async Task<IActionResult> UpdateStock(int id, int cantidad)
    {
        var result = await _productoService.UpdateStockAsync(id, cantidad);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}