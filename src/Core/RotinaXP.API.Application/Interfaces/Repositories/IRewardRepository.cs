using RotinaXP.API.DTOs;
using RotinaXP.API.Models;

namespace RotinaXP.API.Application.Interfaces.Repositories;

public interface IRewardRepository
{
    Task<PagedResult<RewardDTO>> GetByUserPagedAsync(int userId, int page, int pageSize);
    Task<Reward?> GetByIdForUserAsync(int id, int userId);
    Task<RewardDTO?> GetDtoByIdForUserAsync(int id, int userId);
    Task<Reward> CreateAsync(Reward reward);
    Task UpdateAsync(Reward reward);
    Task DeleteAsync(Reward reward);
}
