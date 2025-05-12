using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;
using SuperBodega.API.Models.Admin;  // Para EstadoVenta

namespace SuperBodega.API.Controllers.Admin
{
    [Route("Ventas")]
    public class VentasViewController : Controller
    {
        private readonly VentaService _ventaService;
        private readonly ClienteService _clienteService;

        public VentasViewController(VentaService ventaService, ClienteService clienteService)
        {
            _ventaService = ventaService;
            _clienteService = clienteService;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var ventas = await _ventaService.GetAllAsync();
            return View(ventas);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var clientes = await _clienteService.GetAllClientesAsync();
            ViewBag.Clientes = new SelectList(clientes, "Id", "Nombre");
            return View();
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var clientes = await _clienteService.GetAllClientesAsync();
            ViewBag.Clientes = new SelectList(clientes, "Id", "Nombre");

            var venta = await _ventaService.GetByIdAsync(id);
            if (venta == null) return NotFound();
            return View(venta);
        }

        // ----- NUEVAS ACCIONES PARA CAMBIO DE ESTADO -----

        // GET: /Ventas/ChangeState/{id}
        [HttpGet("ChangeState/{id}")]
        public async Task<IActionResult> ChangeState(int id)
        {
            var venta = await _ventaService.GetByIdAsync(id);
            if (venta == null) return NotFound();

            // Cargar opciones de estado
            ViewBag.Estados = Enum.GetValues(typeof(EstadoVenta))
                                 .Cast<EstadoVenta>()
                                 .Select(e => new SelectListItem(e.ToString(), e.ToString(), e == venta.Estado));
            return View(venta);
        }

        // POST: /Ventas/ChangeState/{id}
        [HttpPost("ChangeState/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeState(int id, [FromForm] UpdateEstadoVentaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _ventaService.ChangeStateAsync(id, dto);
            if (updated == null) return NotFound();

            return RedirectToAction("Index");
        }
    }
}


