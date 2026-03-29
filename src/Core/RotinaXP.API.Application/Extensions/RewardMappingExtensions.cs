using RotinaXP.API.DTOs;
using RotinaXP.API.Models;

namespace RotinaXP.API.Application.Extensions;

public static class RewardMappingExtensions
{
    public static RewardDTO ToDto(this Reward reward) => new()
    {
        Id = reward.Id,
        Title = reward.Title,
        PointsCost = reward.PointsCost,
        UserId = reward.UserId
    };
}
