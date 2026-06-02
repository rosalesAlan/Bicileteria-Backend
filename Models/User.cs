namespace Bicicleteria.Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NumeroTelefono { get; set; }
        public string Mail { get; set; }
        public string Rol { get; set; }  // 'admin', 'vendedor' o 'cliente'
        public string PasswordHash { get; set; }
    }
}
