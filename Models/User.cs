namespace Bicicleteria.Backend.Models;



public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public string PasswordHash { get; set; } = string.Empty;
}