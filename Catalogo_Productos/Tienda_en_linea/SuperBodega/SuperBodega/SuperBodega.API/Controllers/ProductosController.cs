
using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Data;
using System.Linq;

namespace SuperBodega.API.Controllers
{
    [ApiController]
    [Route("api/productos")]
    public class ProductosController : ControllerBase
    {
        private readonly SuperBodegaContext _context;

        public ProductosController(SuperBodegaContext context)
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
