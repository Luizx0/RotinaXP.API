using RotinaXP.API.Services;

namespace RotinaXP.API.Features.Rewards.UseCases;

public class DeleteRewardUseCase
{
    private readonly RewardService _rewardService;

    public DeleteRewardUseCase(RewardService rewardService)
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
