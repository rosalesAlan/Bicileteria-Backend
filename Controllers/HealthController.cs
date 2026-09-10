using Bicicleteria.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bicicleteria.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IConnectionVerificationService _connectionService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            IConnectionVerificationService connectionService,
            ILogger<HealthController> logger)
        {
            _connectionService = connectionService;
            _logger = logger;
        }

        [HttpGet("connections")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckConnections()
        {
            _logger.LogInformation("[HEALTH] Endpoint /api/health/connections llamado");
            _logger.LogInformation("[HEALTH] Verificando estado de todas las conexiones");

            try
            {
                var status = await _connectionService.VerifyAllConnectionsAsync();

                _logger.LogInformation($"[HEALTH] Respuesta preparada. PostgreSQL: {status.PostgresStatus.IsConnected}, MongoDB: {status.MongoDbStatus.IsConnected}");

                if (status.AllConnected)
                {
                    _logger.LogInformation("[HEALTH] Todas las conexiones OK. Retornando 200");
                    return Ok(new
                    {
                        status = "healthy",
                        timestamp = DateTime.UtcNow,
                        postgresql = new
                        {
                            connected = status.PostgresStatus.IsConnected,
                            message = status.PostgresStatus.Message,
                            responseTimeMs = status.PostgresStatus.ResponseTimeMs
                        },
                        mongodb = new
                        {
                            connected = status.MongoDbStatus.IsConnected,
                            message = status.MongoDbStatus.Message,
                            responseTimeMs = status.MongoDbStatus.ResponseTimeMs
                        }
                    });
                }
                else
                {
                    _logger.LogWarning("[HEALTH] Una o mas conexiones fallaron. Retornando 503");
                    return StatusCode(503, new
                    {
                        status = "unhealthy",
                        timestamp = DateTime.UtcNow,
                        postgresql = new
                        {
                            connected = status.PostgresStatus.IsConnected,
                            message = status.PostgresStatus.Message,
                            responseTimeMs = status.PostgresStatus.ResponseTimeMs,
                            error = status.PostgresStatus.Exception?.Message
                        },
                        mongodb = new
                        {
                            connected = status.MongoDbStatus.IsConnected,
                            message = status.MongoDbStatus.Message,
                            responseTimeMs = status.MongoDbStatus.ResponseTimeMs,
                            error = status.MongoDbStatus.Exception?.Message
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[HEALTH] Exception en health check: {ex.Message}");
                return StatusCode(500, new
                {
                    status = "error",
                    message = ex.Message
                });
            }
        }

        [HttpGet("postgres")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckPostgres()
        {
            _logger.LogInformation("[HEALTH] Endpoint /api/health/postgres llamado");

            try
            {
                var status = await _connectionService.VerifyPostgresConnectionAsync();

                if (status.IsConnected)
                {
                    _logger.LogInformation("[HEALTH] PostgreSQL OK. Retornando 200");
                    return Ok(new
                    {
                        status = "connected",
                        message = status.Message,
                        responseTimeMs = status.ResponseTimeMs
                    });
                }
                else
                {
                    _logger.LogWarning("[HEALTH] PostgreSQL fallo. Retornando 503");
                    return StatusCode(503, new
                    {
                        status = "disconnected",
                        message = status.Message,
                        error = status.Exception?.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[HEALTH] Exception verificando PostgreSQL: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("mongodb")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckMongoDB()
        {
            _logger.LogInformation("[HEALTH] Endpoint /api/health/mongodb llamado");

            try
            {
                var status = await _connectionService.VerifyMongoDbConnectionAsync();

                if (status.IsConnected)
                {
                    _logger.LogInformation("[HEALTH] MongoDB OK. Retornando 200");
                    return Ok(new
                    {
                        status = "connected",
                        message = status.Message,
                        responseTimeMs = status.ResponseTimeMs
                    });
                }
                else
                {
                    _logger.LogWarning("[HEALTH] MongoDB fallo. Retornando 503");
                    return StatusCode(503, new
                    {
                        status = "disconnected",
                        message = status.Message,
                        error = status.Exception?.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[HEALTH] Exception verificando MongoDB: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
