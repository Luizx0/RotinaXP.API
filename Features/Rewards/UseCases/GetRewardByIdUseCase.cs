using RotinaXP.API.DTOs;
using RotinaXP.API.Services;

namespace RotinaXP.API.Features.Rewards.UseCases;

public class GetRewardByIdUseCase
{
    private readonly RewardService _rewardService;

    public GetRewardByIdUseCase(RewardService rewardService)
    {
        _rewardService = rewardService;
    }

    public Task<RewardDTO?> ExecuteAsync(int rewardId, int userId)
    {
        return _rewardService.GetRewardDtoByIdForUserAsync(rewardId, userId);
    }
}
