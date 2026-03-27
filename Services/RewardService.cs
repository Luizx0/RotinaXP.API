using Microsoft.EntityFrameworkCore;
using RotinaXP.API.Data;
using RotinaXP.API.Models;

namespace RotinaXP.API.Services;

public class RewardService
{
    private readonly ApplicationDbContext _context;

    public RewardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Reward>> GetAllAsync()
    {
        return await _context.Rewards
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Reward?> GetByIdAsync(int id)
    {
        return await _context.Rewards
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Reward>> GetByUserAsync(int userId)
    {
        return await _context.Rewards
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> UserExistsAsync(int userId)
    {
        return await _context.Users.AnyAsync(u => u.Id == userId);
    }

    public async Task<Reward> CreateAsync(Reward reward)
    {
        _context.Rewards.Add(reward);
        await _context.SaveChangesAsync();
        return reward;
    }

    public async Task UpdateAsync(Reward reward)
    {
        _context.Rewards.Update(reward);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Reward reward)
    {
        _context.Rewards.Remove(reward);
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Success, string Message, int PointsRemaining)> RedeemAsync(int rewardId)
    {
        var reward = await _context.Rewards.FirstOrDefaultAsync(r => r.Id == rewardId);
        if (reward == null)
            return (false, "Reward not found", 0);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == reward.UserId);
        if (user == null)
            return (false, "User not found", 0);

        if (user.Points < reward.PointsCost)
            return (false, "Insufficient points balance", user.Points);

        user.Points -= reward.PointsCost;
        _context.Rewards.Remove(reward);
        await _context.SaveChangesAsync();

        return (true, "Reward redeemed successfully", user.Points);
    }
}
