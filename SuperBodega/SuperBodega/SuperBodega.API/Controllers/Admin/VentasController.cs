using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin
{
    /// <summary>
    /// Controlador para gestionar ventas en el sistema
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class VentasController : ControllerBase
    {
        private readonly VentaService _service;
        public VentasController(VentaService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var v = await _service.GetByIdAsync(id);
            return v == null ? NotFound() : Ok(v);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateVentaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var v = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = v.Id }, v);
        }

        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVentaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var v = await _service.UpdateAsync(id, dto);
            return v == null ? NotFound() : Ok(v);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id) =>
            (await _service.DeleteAsync(id)) ? NoContent() : NotFound();

        [HttpPatch("ChangeState/{id}")]
        public async Task<IActionResult> ChangeState(int id, [FromBody] UpdateEstadoVentaDTO dto)
        {
            var v = await _service.ChangeStateAsync(id, dto);
            return v == null ? NotFound() : Ok(v);
        }
    }
}
