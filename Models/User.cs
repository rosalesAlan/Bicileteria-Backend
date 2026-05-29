namespace Bicicleteria.Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NumeroTelefono { get; set; }
        public string Mail { get; set; }
        public string Tipo { get; set; }  // 'admin' o 'cliente'
        public string PasswordHash { get; set; }
    }
}
