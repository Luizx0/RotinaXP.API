using RotinaXP.API.DTOs;
using RotinaXP.API.Application.Interfaces.Services;

namespace RotinaXP.API.Features.Rewards.UseCases;

public class GetRewardByIdUseCase
{
    private readonly IRewardService _rewardService;

    public GetRewardByIdUseCase(IRewardService rewardService)
    {
        _rewardService = rewardService;
    }

    public Task<RewardDTO?> ExecuteAsync(int rewardId, int userId)
    {
        return _rewardService.GetRewardDtoByIdForUserAsync(rewardId, userId);
    }
}
