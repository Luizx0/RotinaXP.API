using RotinaXP.API.DTOs;
using RotinaXP.API.Models;

namespace RotinaXP.API.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<PagedResult<UserDTO>> GetPagedAsync(int page, int pageSize);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
}
