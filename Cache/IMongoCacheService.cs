using Bicicleteria.Backend.DTOs;

namespace Bicicleteria.Backend.Cache
{
    public interface IMongoCacheService
    {
        /// <summary>
        /// Obtiene los productos desde la caché de MongoDB si están frescos (no han expirado).
        /// Si la caché no existe o ha expirado, consulta SQL Server, guarda en MongoDB y devuelve los datos.
        /// </summary>
        Task<List<ProductDto>> GetProductsAsync();

        /// <summary>
        /// Invalida la caché de productos, forzando una recarga en la próxima consulta.
        /// </summary>
        Task InvalidateAsync();

        /// <summary>
        /// Obtiene el documento de caché directamente de MongoDB sin validar TTL.
        /// Útil para diagnóstico y depuración.
        /// </summary>
        Task<CachedProducts?> GetCacheDocumentAsync();
    }
}
