using RotinaXP.API.DTOs;
using RotinaXP.API.Models;

namespace RotinaXP.API.Application.Interfaces.Services;

public interface IRewardService
{
    Task<PagedResult<RewardDTO>> GetByUserPagedAsync(int userId, int page, int pageSize);
    Task<RewardDTO?> GetRewardDtoByIdForUserAsync(int id, int userId);
    Task<Reward?> GetByIdForUserAsync(int id, int userId);
    Task<Reward> CreateAsync(Reward reward);
    Task UpdateAsync(Reward reward);
    Task DeleteAsync(Reward reward);
    Task<(bool Success, string Message, int PointsRemaining)> RedeemAsync(int rewardId, int userId);
}
