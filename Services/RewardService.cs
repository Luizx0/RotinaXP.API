using Microsoft.EntityFrameworkCore;
using RotinaXP.API.Data;
using RotinaXP.API.Models;

namespace RotinaXP.API.Services;

public class RewardService
{
    public const string ConcurrencyConflictMessage = "Concurrency conflict. Please retry operation.";
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

    public async Task<Reward?> GetByIdForUserAsync(int id, int userId)
    {
        return await _context.Rewards
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
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

    public async Task<(bool Success, string Message, int PointsRemaining)> RedeemAsync(int rewardId, int userId)
    {
        await using var transaction = _context.Database.IsRelational()
            ? await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted)
            : null;

        try
        {
            var reward = await _context.Rewards.FirstOrDefaultAsync(r => r.Id == rewardId && r.UserId == userId);
            if (reward == null)
                return (false, "Reward not found", 0);

            var userSnapshot = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == reward.UserId)
                .Select(u => new { u.Id, u.Points, u.RowVersion })
                .FirstOrDefaultAsync();

            if (userSnapshot == null)
                return (false, "User not found", 0);

            if (userSnapshot.Points < reward.PointsCost)
                return (false, "Insufficient points balance", userSnapshot.Points);

            var pointsUpdated = await TryAtomicDeductPointsAsync(
                userSnapshot.Id,
                userSnapshot.RowVersion,
                reward.PointsCost);

            if (!pointsUpdated)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();

                return (false, ConcurrencyConflictMessage, 0);
            }

            _context.Rewards.Remove(reward);
            await _context.SaveChangesAsync();

            if (transaction != null)
                await transaction.CommitAsync();

            return (true, "Reward redeemed successfully", userSnapshot.Points - reward.PointsCost);
        }
        catch
        {
            if (transaction != null)
                await transaction.RollbackAsync();

            throw;
        }
    }

    private async Task<bool> TryAtomicDeductPointsAsync(int userId, long expectedRowVersion, int pointsToDeduct)
    {
        if (_context.Database.IsRelational())
        {
            var affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users""
                SET ""Points"" = ""Points"" - {pointsToDeduct},
                    ""RowVersion"" = ""RowVersion"" + 1
                WHERE ""Id"" = {userId}
                  AND ""RowVersion"" = {expectedRowVersion}
                  AND ""Points"" >= {pointsToDeduct};");

            return affectedRows == 1;
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null || user.RowVersion != expectedRowVersion || user.Points < pointsToDeduct)
            return false;

        user.Points -= pointsToDeduct;
        user.RowVersion += 1;
        return true;
    }
}
