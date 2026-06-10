using Bicicleteria.Backend.Data;
using Bicicleteria.Backend.DTOs;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Bicicleteria.Backend.Cache
{
    /// <summary>
    /// Servicio de caché híbrido que almacena productos en MongoDB para acelerar consultas.
    /// Mantiene sincronización automática con la base de datos SQL Server mediante TTL configurable.
    /// </summary>
    public class MongoCacheService : IMongoCacheService
    {
        private readonly IMongoCollection<CachedProducts> _collection;
        private readonly AppDbContext _context;
        private readonly int _ttlMinutos;
        private readonly ILogger<MongoCacheService> _logger;
        private const string CACHE_ID = "global";

        public MongoCacheService(
            IMongoClient mongoClient,
            IConfiguration configuration,
            AppDbContext context,
            ILogger<MongoCacheService> logger)
        {
            _context = context;
            _logger = logger;

            // Obtener configuración de MongoDB
            var mongoDbConfig = configuration.GetSection("MongoDb");
            var databaseName = mongoDbConfig["DatabaseName"] ?? "BicicleteriaMDB";
            _ttlMinutos = int.TryParse(mongoDbConfig["CacheTtlMinutes"], out var ttl) ? ttl : 15;

            // Conectar a la base de datos y colección
            var database = mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<CachedProducts>("productos_cache");

            // Crear índice TTL si no existe
            EnsureIndexAsync().Wait();
        }

        /// <summary>
        /// Obtiene los productos desde la caché si están frescos, o desde SQL Server si han expirado.
        /// </summary>
        public async Task<List<ProductDto>> GetProductsAsync()
        {
            try
            {
                _logger.LogInformation("Intentando obtener productos desde caché de MongoDB");

                // Intentar obtener documento de caché
                var cachedDoc = await _collection.Find(p => p.Id == CACHE_ID).FirstOrDefaultAsync();

                // Si existe y no ha expirado, devolver desde caché
                if (cachedDoc != null && !cachedDoc.EstaExpirado())
                {
                    _logger.LogInformation($"? Caché activa. Devolviendo {cachedDoc.Productos.Count} productos desde MongoDB");
                    return cachedDoc.Productos;
                }

                // Caché expirada o no existe: cargar desde SQL Server
                _logger.LogInformation("Caché expirada o no existe. Cargando desde SQL Server...");
                var productosDelDB = await _context.Products
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        ImageUrl = p.ImageUrl,
                        CategoryId = p.CategoryId,
                        Availability = p.Availability
                    })
                    .ToListAsync();

                // Crear documento de caché
                var newCachedDoc = new CachedProducts
                {
                    Id = CACHE_ID,
                    Productos = productosDelDB,
                    FechaActualizacion = DateTime.UtcNow,
                    TtlMinutos = _ttlMinutos
                };

                // Guardar en MongoDB (reemplazar si existe)
                await _collection.ReplaceOneAsync(
                    p => p.Id == CACHE_ID,
                    newCachedDoc,
                    new ReplaceOptions { IsUpsert = true });

                _logger.LogInformation($"? Caché actualizada. Guardados {productosDelDB.Count} productos en MongoDB");
                return productosDelDB;
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error en MongoCacheService.GetProductsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Invalida la caché borrando el documento de MongoDB.
        /// Fuerza una recarga desde SQL Server en la próxima consulta.
        /// </summary>
        public async Task InvalidateAsync()
        {
            try
            {
                _logger.LogInformation("Invalidando caché de productos...");

                var result = await _collection.DeleteOneAsync(p => p.Id == CACHE_ID);

                if (result.DeletedCount > 0)
                {
                    _logger.LogInformation("? Caché invalidada correctamente");
                }
                else
                {
                    _logger.LogWarning("? No se encontró documento de caché para invalidar");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error invalidando caché: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el documento de caché directamente sin validar TTL.
        /// Útil para propósitos de diagnóstico.
        /// </summary>
        public async Task<CachedProducts?> GetCacheDocumentAsync()
        {
            try
            {
                return await _collection.Find(p => p.Id == CACHE_ID).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error obteniendo documento de caché: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Asegura que exista el índice TTL en la colección de caché.
        /// </summary>
        private async Task EnsureIndexAsync()
        {
            try
            {
                var indexKeysDefinition = Builders<CachedProducts>.IndexKeys.Ascending(p => p.FechaActualizacion);
                var indexModel = new CreateIndexModel<CachedProducts>(indexKeysDefinition);

                await _collection.Indexes.CreateOneAsync(indexModel);
                _logger.LogInformation("? Índice de MongoDB verificado/creado");
            }
            catch (MongoCommandException ex) when (ex.CodeName == "IndexKeySpecsConflict")
            {
                // El índice ya existe, no es un error
                _logger.LogInformation("? Índice de MongoDB ya existe");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"? Error creando índice en MongoDB: {ex.Message}");
            }
        }
    }
}
