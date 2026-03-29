using RotinaXP.API.DTOs;
using RotinaXP.API.Application.Interfaces.Services;

namespace RotinaXP.API.Features.Rewards.UseCases;

public class UpdateRewardUseCase
{
    private readonly IRewardService _rewardService;

    public UpdateRewardUseCase(IRewardService rewardService)
    {
        _rewardService = rewardService;
    }

    public async Task<(bool Success, string Message)> ExecuteAsync(int rewardId, int userId, UpdateRewardDto request)
    {
        var reward = await _rewardService.GetByIdForUserAsync(rewardId, userId);
        if (reward == null)
            return (false, "Reward not found");

        if (!string.IsNullOrWhiteSpace(request.Title))
            reward.Title = request.Title;

        if (request.PointsCost.HasValue)
        {
            if (request.PointsCost <= 0)
                return (false, "PointsCost must be greater than zero");

            reward.PointsCost = request.PointsCost.Value;
        }

        await _rewardService.UpdateAsync(reward);
        return (true, "Reward updated successfully");
    }
}
