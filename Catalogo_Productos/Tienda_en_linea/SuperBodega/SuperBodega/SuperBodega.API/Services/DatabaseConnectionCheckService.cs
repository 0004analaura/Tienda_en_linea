using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;

namespace SuperBodega.API.Services
{
    public class DatabaseConnectionCheckService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DatabaseConnectionCheckService> _logger;
        private int _connectionAttempt = 0;
        private readonly int _maxAttempts = 30;
        private bool _connectionSuccessful = false;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);
        // Variable estática para controlar si ya se verificó la creación de la base de datos
        private static bool _databaseVerified = false;
        private static readonly object _lockObject = new object();

        public DatabaseConnectionCheckService(
            IServiceScopeFactory scopeFactory,
            ILogger<DatabaseConnectionCheckService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de verificación de base de datos iniciado");
            
            while (!stoppingToken.IsCancellationRequested && !_connectionSuccessful && _connectionAttempt < _maxAttempts)
            {
                _connectionAttempt++;
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<SuperBodegaContext>();
                        
                        _logger.LogInformation("Intento {Attempt}/{MaxAttempts} de conexión a SQL Server", 
                            _connectionAttempt, _maxAttempts);
                        
                        // Intenta conectar a la base de datos
                        bool canConnect = await dbContext.Database.CanConnectAsync(stoppingToken);
                        
                        if (canConnect)
                        {
                            _connectionSuccessful = true;
                            _logger.LogInformation("¡CONEXIÓN EXITOSA! SQL Server está en línea después de {Attempt} intentos", 
                                _connectionAttempt);
                            
                            // Solo verificar si la base de datos existe una vez
                            if (!_databaseVerified)
                            {
                                lock (_lockObject)
                                {
                                    if (!_databaseVerified)
                                    {
                                        _logger.LogInformation("Verificación inicial de la base de datos");
                                        _databaseVerified = true;
                                    }
                                }
                            }
                            
                            break;
                        }
                        else
                        {
                            _logger.LogWarning("Intento fallido de conexión a SQL Server ({Attempt}/{MaxAttempts})", 
                                _connectionAttempt, _maxAttempts);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al intentar conectar a SQL Server (Intento {Attempt}/{MaxAttempts}): {Message}", 
                        _connectionAttempt, _maxAttempts, ex.Message);
                }

                // Esperar antes del siguiente intento
                await Task.Delay(_checkInterval, stoppingToken);
            }

            if (!_connectionSuccessful && _connectionAttempt >= _maxAttempts)
            {
                _logger.LogError("SE AGOTARON LOS INTENTOS. No se pudo conectar a SQL Server después de {MaxAttempts} intentos", 
                    _maxAttempts);
            }
            
            // Continuar verificando la conexión periódicamente SIN recrear tablas
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<SuperBodegaContext>();
                        bool isConnected = await dbContext.Database.CanConnectAsync(stoppingToken);
                        
                        _logger.LogInformation("Estado de la conexión a SQL Server: {Status}", 
                            isConnected ? "CONECTADO" : "DESCONECTADO");
                            
                        if (isConnected != _connectionSuccessful)
                        {
                            if (isConnected)
                            {
                                _logger.LogInformation("La conexión a SQL Server se ha restablecido");
                                _connectionSuccessful = true;
                            }
                            else
                            {
                                _logger.LogWarning("Se ha perdido la conexión a SQL Server");
                                _connectionSuccessful = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al verificar la conexión a SQL Server: {Message}", ex.Message);
                    _connectionSuccessful = false;
                }

                // Verificar cada cierto tiempo
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}