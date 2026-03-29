using System.ComponentModel.DataAnnotations;

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
    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}
public class LoginResponse
{
    public string Message { get; set; } = string.Empty;
    public UserDTO User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
}
public class RegisterRequest
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}
public class UpdateUserRequest
{
    [StringLength(120, MinimumLength = 2)]
    public string? Name { get; set; }

    [EmailAddress]
    [MaxLength(254)]
    public string? Email { get; set; }
}
