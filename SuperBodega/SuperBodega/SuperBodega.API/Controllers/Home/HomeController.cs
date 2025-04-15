using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Data;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace SuperBodega.API.Controllers.Home
{
    public class HomeController : Controller
    {
        private readonly SuperBodegaContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(SuperBodegaContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            bool dbConnectionSuccess = false;
            string dbConnectionMessage = "No se ha intentado conectar a la base de datos.";
            
            try
            {
                // Intentar una consulta simple para verificar la conexión
                dbConnectionSuccess = _context.Database.CanConnect();
                dbConnectionMessage = dbConnectionSuccess 
                    ? "Conexión a SQL Server establecida exitosamente." 
                    : "No se pudo establecer conexión con SQL Server.";
                
                _logger.LogInformation(dbConnectionMessage);
            }
            catch (Exception ex)
            {
                dbConnectionSuccess = false;
                dbConnectionMessage = $"Error al conectar con la base de datos: {ex.Message}";
                _logger.LogError(ex, "Error al conectar con la base de datos");
            }

            ViewBag.DbConnectionSuccess = dbConnectionSuccess;
            ViewBag.DbConnectionMessage = dbConnectionMessage;
            
            return View();
        }
    }
}