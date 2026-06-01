using System.ComponentModel.DataAnnotations;


namespace Bicicleteria.Backend.DTOs
{
    public class RegisterRequest
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [EmailAddress] //Esto valida que el texto tenga formato de email
        public string Email { get; set; } = string.Empty;

        [Required]
        public string NumeroTelefono { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

    }
}