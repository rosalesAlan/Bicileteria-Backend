namespace Bicicleteria.Backend.DTOs
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
    }
}
