using RotinaXP.API.DTOs;

namespace RotinaXP.API.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserDTO?> GetUserByIdAsync(int id);
    Task<PagedResult<UserDTO>> GetAllUsersPagedAsync(int page, int pageSize);
    Task<(bool Success, UserDTO? User, string Message)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, UserDTO? User, string Message)> LoginAsync(LoginRequest request);
    Task<(bool Success, UserDTO? User, string Message)> UpdateAsync(int id, UpdateUserRequest request);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
