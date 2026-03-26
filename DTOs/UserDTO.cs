namespace RotinaXP.API.DTOs;
public class UserDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Points { get; set; }
}
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDTO User { get; set; } = null!;
}
public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
public class UpdateUserRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
}
