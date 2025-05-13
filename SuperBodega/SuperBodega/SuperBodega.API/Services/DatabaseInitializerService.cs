using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SuperBodega.API.Services
{
    public class DatabaseInitializerService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DatabaseInitializerService> _logger;
        private readonly IConfiguration _configuration;
        // Control de inicialización
        private static bool _initialized = false;
        private static readonly object _lockObject = new object();

        public DatabaseInitializerService(
            IServiceScopeFactory scopeFactory,
            ILogger<DatabaseInitializerService> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InitializeAsync()
        {
            // Si ya se inicializó, no hacer nada
            if (_initialized)
            {
                _logger.LogInformation("Base de datos ya inicializada previamente. Saltando inicialización.");
                return;
            }

            lock (_lockObject)
            {
                if (_initialized)
                {
                    _logger.LogInformation("Base de datos ya inicializada por otro hilo. Saltando inicialización.");
                    return;
                }

                _logger.LogInformation("Iniciando proceso de inicialización de la base de datos...");
            }

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<SuperBodegaContext>();
                    var connectionString = dbContext.Database.GetConnectionString();
                    _logger.LogInformation($"Usando connection string: {connectionString}");

                    // 1. Verificar si la base de datos existe, si no, crearla
                    _logger.LogInformation("Verificando si la base de datos existe...");
                    bool exists = await dbContext.Database.CanConnectAsync();

                    if (!exists)
                    {
                        _logger.LogInformation("La base de datos no existe, creándola...");
                        await dbContext.Database.EnsureCreatedAsync();
                        _logger.LogInformation("Base de datos creada exitosamente");
                    }
                    else
                    {
                        _logger.LogInformation("La base de datos ya existe");
                    }

                    // Marcar como inicializado
                    _initialized = true;
                    _logger.LogInformation("Inicialización de base de datos completada con éxito!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la inicialización de la base de datos");
            }
        }
    }
}