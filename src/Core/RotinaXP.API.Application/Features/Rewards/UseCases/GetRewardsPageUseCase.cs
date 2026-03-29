using RotinaXP.API.DTOs;
using RotinaXP.API.Application.Interfaces.Services;

namespace RotinaXP.API.Features.Rewards.UseCases;

public class GetRewardsPageUseCase
{
    private readonly IRewardService _rewardService;

    public GetRewardsPageUseCase(IRewardService rewardService)
    {
        _rewardService = rewardService;
    }

    public Task<PagedResult<RewardDTO>> ExecuteAsync(int userId, int page, int pageSize)
    {
        return _rewardService.GetByUserPagedAsync(userId, page, pageSize);
    }
}
