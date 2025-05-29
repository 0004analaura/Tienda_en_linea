
using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Data;
using System.Linq;

namespace SuperBodega.API.Controllers.Ecommerce
{
    /// <summary>
    /// Controlador para gestionar productos en el sistema ecommerce
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class ProductoApiController : ControllerBase
    {
        private readonly SuperBodegaContext _context;

        public ProductoApiController(SuperBodegaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var productos = _context.Productos
                .Select(p => new {
                    p.Id,
                    p.Nombre,
                    p.PrecioDeVenta,
                    p.Stock,
                    Categoria = p.Categoria.Nombre,
                    p.ImagenUrl
                }).ToList();

            return Ok(productos);
        }
    }
}
