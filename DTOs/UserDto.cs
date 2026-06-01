namespace Bicicleteria.Backend.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}
