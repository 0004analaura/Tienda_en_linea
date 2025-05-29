using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

/// <summary>
/// Controlador para gestionar reportes en el sistema
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]
public class ReporteController : ControllerBase
{
    private readonly ReporteService _service;
    public ReporteController(ReporteService service) => _service = service;

    [HttpGet("PorPeriodo")]
    public async Task<IActionResult> PorPeriodo(DateTime inicio, DateTime fin)
        => Ok(await _service.VentasPorPeriodoAsync(inicio, fin));

    [HttpGet("PorProducto")]
    public async Task<IActionResult> PorProducto(DateTime? inicio, DateTime? fin)
        => Ok(await _service.VentasPorProductoAsync(inicio, fin));

    [HttpGet("PorCliente")]
    public async Task<IActionResult> PorCliente(DateTime? inicio, DateTime? fin)
        => Ok(await _service.VentasPorClienteAsync(inicio, fin));

    [HttpGet("PorProveedor")]
    public async Task<IActionResult> PorProveedor(DateTime? inicio, DateTime? fin)
        => Ok(await _service.VentasPorProveedorAsync(inicio, fin));
}
