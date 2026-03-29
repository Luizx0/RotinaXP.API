using RotinaXP.API.DTOs;
using RotinaXP.API.Models;
using RotinaXP.API.Services;

namespace RotinaXP.API.Features.Rewards.UseCases;

public class CreateRewardUseCase
{
    private readonly RewardService _rewardService;

    public CreateRewardUseCase(RewardService rewardService)
    {
        _rewardService = rewardService;
    }

    public async Task<(bool Success, string Message, RewardDTO? Reward)> ExecuteAsync(CreateRewardDto request, int authenticatedUserId)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return (false, "Title is required", null);

        if (request.PointsCost <= 0)
            return (false, "PointsCost must be greater than zero", null);

        if (request.UserId != authenticatedUserId)
            return (false, "Forbidden", null);

        var reward = new Reward
        {
            Title = request.Title,
            PointsCost = request.PointsCost,
            UserId = authenticatedUserId
        };

        await _rewardService.CreateAsync(reward);
        var createdReward = await _rewardService.GetRewardDtoByIdForUserAsync(reward.Id, authenticatedUserId);

        return (true, "Reward created successfully", createdReward);
    }
}
