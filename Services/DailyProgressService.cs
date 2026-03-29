using Microsoft.EntityFrameworkCore;
using RotinaXP.API.Data;
using RotinaXP.API.Models;

namespace RotinaXP.API.Services;

public class DailyProgressService
{
    private readonly ApplicationDbContext _context;

    public DailyProgressService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DailyProgress>> GetAllAsync()
    {
        return await _context.DailyProgresses
            .AsNoTracking()
            .OrderByDescending(p => p.Date)
            .ToListAsync();
    }

    public async Task<DailyProgress?> GetByIdAsync(int id)
    {
        return await _context.DailyProgresses
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<DailyProgress?> GetByIdForUserAsync(int id, int userId)
    {
        return await _context.DailyProgresses
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
    }

    public async Task<List<DailyProgress>> GetByUserAsync(int userId)
    {
        return await _context.DailyProgresses
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.Date)
            .ToListAsync();
    }

    public async Task<bool> UserExistsAsync(int userId)
    {
        return await _context.Users.AnyAsync(u => u.Id == userId);
    }
}
