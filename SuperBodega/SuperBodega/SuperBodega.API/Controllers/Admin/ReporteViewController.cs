using Microsoft.AspNetCore.Mvc;

namespace SuperBodega.API.Controllers.Admin;

/// <summary>
/// Controlador para gestionar las vistas de reportes en el sistema
/// </summary>
[Route("Reportes")]
public class ReporteViewController : Controller
{
    [HttpGet("Index")]
    public IActionResult Index() => View();

    // Las vistas usan JS fetch a los endpoints API para poblar las tablas/gráficas
}
