using RotinaXP.API.DTOs;

namespace RotinaXP.API.Application.Interfaces.Auth;

/// <summary>
/// Contrato de autenticação. UserService implementa este contrato.
/// Disponível para injeção em contextos onde apenas register/login são necessários.
/// </summary>
public interface IAuthService
{
    Task<(bool Success, UserDTO? User, string Message)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, UserDTO? User, string Message)> LoginAsync(LoginRequest request);
}
