using RotinaXP.API.Services;

namespace RotinaXP.API.Features.Rewards.UseCases;

public class RedeemRewardUseCase
{
    private readonly RewardService _rewardService;

    public RedeemRewardUseCase(RewardService rewardService)
    {
        _rewardService = rewardService;
    }

    public Task<(bool Success, string Message, int PointsRemaining)> ExecuteAsync(int rewardId, int userId)
    {
        return _rewardService.RedeemAsync(rewardId, userId);
    }
}
