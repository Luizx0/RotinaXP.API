namespace RotinaXP.API.DTOs;
public class UserDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Pontos { get; set; }
}
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDTO Usuario { get; set; } = null!;
}
public class RegisterRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
public class UpdateUserRequest
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
}
