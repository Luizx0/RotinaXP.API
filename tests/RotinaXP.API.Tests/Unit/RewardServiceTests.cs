using Microsoft.EntityFrameworkCore;
using RotinaXP.API.Data;
using RotinaXP.API.Models;
using RotinaXP.API.Services;

namespace RotinaXP.API.Tests.Unit;

public class RewardServiceTests
{
    [Fact]
    public async Task RedeemAsync_WithEnoughPoints_DeductsPointsAndDeletesReward()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"RewardServiceTests-{Guid.NewGuid()}")
            .Options;

        await using var context = new ApplicationDbContext(options);
        var user = new User
        {
            Name = "Reward User",
            Email = "reward-user@example.com",
            PasswordHash = "hash",
            Points = 50
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var reward = new Reward
        {
            Title = "Reward",
            PointsCost = 20,
            UserId = user.Id
        };
        context.Rewards.Add(reward);
        await context.SaveChangesAsync();

        var service = new RewardService(context);
        var result = await service.RedeemAsync(reward.Id, user.Id);

        Assert.True(result.Success);
        Assert.Equal(30, result.PointsRemaining);
        Assert.Equal(30, context.Users.Single(u => u.Id == user.Id).Points);
        Assert.False(context.Rewards.Any(r => r.Id == reward.Id));
    }
}
