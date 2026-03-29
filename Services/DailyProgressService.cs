using Microsoft.EntityFrameworkCore;
using RotinaXP.API.Data;
using RotinaXP.API.DTOs;
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

    public async Task<PagedResult<DailyProgressDTO>> GetByUserPagedAsync(int userId, int page, int pageSize)
    {
        var (normalizedPage, normalizedPageSize) = NormalizePagination(page, pageSize);

        var query = _context.DailyProgresses
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.Date)
            .ThenByDescending(p => p.Id)
            .Select(p => new DailyProgressDTO
            {
                Id = p.Id,
                Date = p.Date,
                CompletedTasksCount = p.CompletedTasksCount,
                UserId = p.UserId
            });

        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync();

        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)normalizedPageSize);

        return new PagedResult<DailyProgressDTO>
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasNext = totalPages > 0 && normalizedPage < totalPages,
            HasPrevious = normalizedPage > PaginationDefaults.DefaultPage && totalPages > 0,
            Items = items
        };
    }

    public async Task<DailyProgressDTO?> GetDailyProgressDtoByIdForUserAsync(int id, int userId)
    {
        return await _context.DailyProgresses
            .AsNoTracking()
            .Where(p => p.Id == id && p.UserId == userId)
            .Select(p => new DailyProgressDTO
            {
                Id = p.Id,
                Date = p.Date,
                CompletedTasksCount = p.CompletedTasksCount,
                UserId = p.UserId
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UserExistsAsync(int userId)
    {
        return await _context.Users.AnyAsync(u => u.Id == userId);
    }

    private static (int Page, int PageSize) NormalizePagination(int page, int pageSize)
    {
        var normalizedPage = page < PaginationDefaults.DefaultPage
            ? PaginationDefaults.DefaultPage
            : page;

        var normalizedPageSize = pageSize < 1
            ? PaginationDefaults.DefaultPageSize
            : Math.Min(pageSize, PaginationDefaults.MaxPageSize);

        return (normalizedPage, normalizedPageSize);
    }
}
