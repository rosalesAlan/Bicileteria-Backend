using Bicicleteria.Backend.Data;
using MongoDB.Driver;

namespace Bicicleteria.Backend.Services
{
    public interface IConnectionVerificationService
    {
        Task<ConnectionStatus> VerifyPostgresConnectionAsync();
        Task<ConnectionStatus> VerifyMongoDbConnectionAsync();
        Task<AllConnectionsStatus> VerifyAllConnectionsAsync();
    }

    public class ConnectionStatus
    {
        public bool IsConnected { get; set; }
        public string Message { get; set; } = string.Empty;
        public long ResponseTimeMs { get; set; }
        public Exception? Exception { get; set; }
    }

    public class AllConnectionsStatus
    {
        public ConnectionStatus PostgresStatus { get; set; } = new();
        public ConnectionStatus MongoDbStatus { get; set; } = new();
        public bool AllConnected => PostgresStatus.IsConnected && MongoDbStatus.IsConnected;
    }

    public class ConnectionVerificationService : IConnectionVerificationService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMongoClient _mongoClient;
        private readonly ILogger<ConnectionVerificationService> _logger;

        public ConnectionVerificationService(
            AppDbContext dbContext,
            IMongoClient mongoClient,
            ILogger<ConnectionVerificationService> logger)
        {
            _dbContext = dbContext;
            _mongoClient = mongoClient;
            _logger = logger;
        }

        public async Task<ConnectionStatus> VerifyPostgresConnectionAsync()
        {
            _logger.LogInformation("[POSTGRES] Iniciando verificacion de conexion a PostgreSQL");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var status = new ConnectionStatus();

            try
            {
                _logger.LogInformation("[POSTGRES] Ejecutando prueba de conexion con CanConnectAsync()");

                var canConnect = await _dbContext.Database.CanConnectAsync();

                stopwatch.Stop();
                status.ResponseTimeMs = stopwatch.ElapsedMilliseconds;

                if (canConnect)
                {
                    status.IsConnected = true;
                    status.Message = $"Conectado a PostgreSQL exitosamente. Tiempo de respuesta: {status.ResponseTimeMs}ms";
                    _logger.LogInformation($"[POSTGRES] EXITO: {status.Message}");
                }
                else
                {
                    status.IsConnected = false;
                    status.Message = "No se pudo establecer conexion con PostgreSQL (CanConnectAsync retorno false)";
                    _logger.LogWarning($"[POSTGRES] FALLO: {status.Message}");
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                status.IsConnected = false;
                status.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                status.Exception = ex;
                status.Message = $"Error al conectar con PostgreSQL: {ex.Message}";
                _logger.LogError($"[POSTGRES] ERROR: {status.Message}. Excepcion: {ex}");
            }

            return status;
        }

        public async Task<ConnectionStatus> VerifyMongoDbConnectionAsync()
        {
            _logger.LogInformation("[MONGODB] Iniciando verificacion de conexion a MongoDB");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var status = new ConnectionStatus();

            try
            {
                _logger.LogInformation("[MONGODB] Listando bases de datos disponibles");

                // Intentar obtener lista de bases de datos (prueba de conexion)
                var adminClient = _mongoClient.GetDatabase("admin");
                var databases = await _mongoClient.ListDatabasesAsync();

                stopwatch.Stop();
                status.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                status.IsConnected = true;
                status.Message = $"Conectado a MongoDB exitosamente. Tiempo de respuesta: {status.ResponseTimeMs}ms";
                _logger.LogInformation($"[MONGODB] EXITO: {status.Message}");

                // Log adicional: contar numero de bases de datos
                var dbNames = await databases.ToListAsync();
                _logger.LogInformation($"[MONGODB] Bases de datos disponibles: {dbNames.Count}");
                foreach (var db in dbNames)
                {
                    _logger.LogInformation($"[MONGODB]   - {db["name"]}");
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                status.IsConnected = false;
                status.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                status.Exception = ex;
                status.Message = $"Error al conectar con MongoDB: {ex.Message}";
                _logger.LogError($"[MONGODB] ERROR: {status.Message}. Excepcion: {ex}");
            }

            return status;
        }

        public async Task<AllConnectionsStatus> VerifyAllConnectionsAsync()
        {
            _logger.LogInformation("[SISTEMA] Iniciando verificacion de todas las conexiones");

            var result = new AllConnectionsStatus();

            _logger.LogInformation("[SISTEMA] ========================================");
            _logger.LogInformation("[SISTEMA] VERIFICACION DE CONEXIONES DE BASE DE DATOS");
            _logger.LogInformation("[SISTEMA] ========================================");

            result.PostgresStatus = await VerifyPostgresConnectionAsync();
            result.MongoDbStatus = await VerifyMongoDbConnectionAsync();

            _logger.LogInformation("[SISTEMA] ========================================");
            _logger.LogInformation($"[SISTEMA] Estado General: {(result.AllConnected ? "TODO CONECTADO" : "FALLO EN ALGUNAS CONEXIONES")}");
            _logger.LogInformation($"[SISTEMA]   - PostgreSQL: {(result.PostgresStatus.IsConnected ? "OK" : "FALLO")} ({result.PostgresStatus.ResponseTimeMs}ms)");
            _logger.LogInformation($"[SISTEMA]   - MongoDB: {(result.MongoDbStatus.IsConnected ? "OK" : "FALLO")} ({result.MongoDbStatus.ResponseTimeMs}ms)");
            _logger.LogInformation("[SISTEMA] ========================================");

            return result;
        }
    }
}
