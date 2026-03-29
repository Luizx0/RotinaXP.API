using RotinaXP.API.Application.Interfaces.Services;

namespace RotinaXP.API.Features.Rewards.UseCases;

public class RedeemRewardUseCase
{
    private readonly IRewardService _rewardService;

    public RedeemRewardUseCase(IRewardService rewardService)
    {
        _rewardService = rewardService;
    }

    public Task<(bool Success, string Message, int PointsRemaining)> ExecuteAsync(int rewardId, int userId)
    {
        return _rewardService.RedeemAsync(rewardId, userId);
    }
}
