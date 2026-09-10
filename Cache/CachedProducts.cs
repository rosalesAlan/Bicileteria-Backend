using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Bicicleteria.Backend.DTOs;

namespace Bicicleteria.Backend.Cache
{
    [BsonIgnoreExtraElements]
    public class CachedProducts
    {
        [BsonId]
        public string Id { get; set; } = "global";

        [BsonElement("productos")]
        public List<ProductDto> Productos { get; set; } = new();

        [BsonElement("fecha_actualizacion")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        [BsonElement("ttl_minutos")]
        public int TtlMinutos { get; set; } = 15;

        public bool EstaExpirado()
        {
            return DateTime.UtcNow > FechaActualizacion.AddMinutes(TtlMinutos);
        }
    }
}
