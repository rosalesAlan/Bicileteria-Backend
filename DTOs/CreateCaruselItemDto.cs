namespace Bicicleteria.Backend.DTOs
{
    public class CreateCaruselItemDto
    {
        public string Name { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public int Range { get; set; }
    }
}

/*
 DTO PARA CREAR UN CARRUSEL: el proposito es permitirle al usuario crear una entidad sin el IDENTIFICADOR
(Id) Así el sistema se encarga de generar el Id automáticamente, 
evitando que el usuario tenga esa responsabilida y asegurando la integridad de los datos.
 */