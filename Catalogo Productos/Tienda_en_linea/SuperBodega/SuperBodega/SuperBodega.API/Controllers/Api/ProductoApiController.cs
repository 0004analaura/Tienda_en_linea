
using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Data;
using System.Linq;

namespace SuperBodega.API.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductoApiController : ControllerBase
    {
        private readonly SuperBodegaContext _context;

        public ProductoApiController(SuperBodegaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetProductos()
        {
            var productos = _context.Productos
                .Select(p => new {
                    p.Id,
                    p.Nombre,
                    p.PrecioDeVenta,
                    p.Stock,
                    p.Categoria,
                    p.ImagenUrl
                }).ToList();

            return Ok(productos);
        }
    }
}
