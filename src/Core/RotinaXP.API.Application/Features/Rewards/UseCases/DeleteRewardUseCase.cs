using RotinaXP.API.Application.Interfaces.Services;

namespace RotinaXP.API.Features.Rewards.UseCases;

public class DeleteRewardUseCase
{
    private readonly IRewardService _rewardService;

    public DeleteRewardUseCase(IRewardService rewardService)
    {
        _rewardService = rewardService;
    }

    public async Task<(bool Success, string Message)> ExecuteAsync(int rewardId, int userId)
    {
        var reward = await _rewardService.GetByIdForUserAsync(rewardId, userId);
        if (reward == null)
            return (false, "Reward not found");

        await _rewardService.DeleteAsync(reward);
        return (true, "Reward deleted successfully");
    }
}
