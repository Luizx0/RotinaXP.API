using RotinaXP.API.DTOs;
using RotinaXP.API.Services;

namespace RotinaXP.API.Features.Rewards.UseCases;

public class GetRewardsPageUseCase
{
    private readonly RewardService _rewardService;

    public GetRewardsPageUseCase(RewardService rewardService)
    {
        _rewardService = rewardService;
    }

    public Task<PagedResult<RewardDTO>> ExecuteAsync(int userId, int page, int pageSize)
    {
        return _rewardService.GetByUserPagedAsync(userId, page, pageSize);
    }
}
