using Microsoft.AspNetCore.Mvc;

namespace SuperBodega.API.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }
        
        public IActionResult AdminDashboard()
        {
            _logger.LogInformation("Accediendo al dashboard de administrador");
            return View();
        }
        
        public IActionResult ClienteDashboard()
        {
            _logger.LogInformation("Accediendo al dashboard de cliente");
            return View();
        }
    }
}