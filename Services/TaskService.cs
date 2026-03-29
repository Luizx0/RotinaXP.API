using Microsoft.EntityFrameworkCore;
using RotinaXP.API.Data;
using RotinaXP.API.DTOs;
using RotinaXP.API.Models;

namespace RotinaXP.API.Services;

public class TaskService
{
    public const int CompletionPoints = 10;
    public const string ConcurrencyConflictMessage = "Concurrency conflict. Please retry operation.";
    private readonly ApplicationDbContext _context;

    public TaskService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await _context.Tasks
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TaskDTO?> GetTaskDtoByIdForUserAsync(int id, int userId)
    {
        return await _context.Tasks
            .AsNoTracking()
            .Where(t => t.Id == id && t.UserId == userId)
            .Select(t => new TaskDTO
            {
                Id = t.Id,
                Title = t.Title,
                IsCompleted = t.IsCompleted,
                UserId = t.UserId
            })
            .FirstOrDefaultAsync();
    }

    public async Task<TaskItem?> GetByIdForUserAsync(int id, int userId)
    {
        return await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    public async Task<List<TaskItem>> GetByUserAsync(int userId)
    {
        return await _context.Tasks
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    public async Task<PagedResult<TaskDTO>> GetByUserPagedAsync(int userId, int page, int pageSize)
    {
        var (normalizedPage, normalizedPageSize) = NormalizePagination(page, pageSize);

        var query = _context.Tasks
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Id)
            .Select(t => new TaskDTO
            {
                Id = t.Id,
                Title = t.Title,
                IsCompleted = t.IsCompleted,
                UserId = t.UserId
            });

        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync();

        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)normalizedPageSize);

        return new PagedResult<TaskDTO>
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

    public async Task<bool> UserExistsAsync(int userId)
    {
        return await _context.Users.AnyAsync(u => u.Id == userId);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task UpdateAsync(TaskItem task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Success, string Message, bool PointsAwarded)> UpdateWithGamificationAsync(
        int taskId,
        int userId,
        string? title,
        bool? isCompleted)
    {
        await using var transaction = _context.Database.IsRelational()
            ? await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted)
            : null;

        try
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

            if (task == null)
                return (false, "Task not found", false);

            if (!string.IsNullOrWhiteSpace(title))
                task.Title = title;

            var pointsAwarded = false;

            if (isCompleted.HasValue && isCompleted.Value != task.IsCompleted)
            {
                if (task.IsCompleted && !isCompleted.Value)
                    return (false, "Completed tasks cannot be reopened", false);

                task.IsCompleted = isCompleted.Value;

                if (task.IsCompleted)
                {
                    var userSnapshot = await _context.Users
                        .AsNoTracking()
                        .Where(u => u.Id == task.UserId)
                        .Select(u => new { u.Id, u.RowVersion })
                        .FirstOrDefaultAsync();

                    if (userSnapshot == null)
                        return (false, "User not found", false);

                    var pointsUpdated = await TryAtomicAddPointsAsync(userSnapshot.Id, userSnapshot.RowVersion, CompletionPoints);
                    if (!pointsUpdated)
                    {
                        if (transaction != null)
                            await transaction.RollbackAsync();

                        return (false, ConcurrencyConflictMessage, false);
                    }

                    pointsAwarded = true;
                    await IncrementDailyProgressAsync(task.UserId, DateTime.UtcNow.Date);
                }
            }

            await _context.SaveChangesAsync();

            if (transaction != null)
                await transaction.CommitAsync();

            var message = pointsAwarded
                ? $"Task completed. {CompletionPoints} points awarded"
                : "Task updated successfully";

            return (true, message, pointsAwarded);
        }
        catch
        {
            if (transaction != null)
                await transaction.RollbackAsync();

            throw;
        }
    }

    private async Task<bool> TryAtomicAddPointsAsync(int userId, long expectedRowVersion, int pointsToAdd)
    {
        if (_context.Database.IsRelational())
        {
            var affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users""
                SET ""Points"" = ""Points"" + {pointsToAdd},
                    ""RowVersion"" = ""RowVersion"" + 1
                WHERE ""Id"" = {userId}
                  AND ""RowVersion"" = {expectedRowVersion};");

            return affectedRows == 1;
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null || user.RowVersion != expectedRowVersion)
            return false;

        user.Points += pointsToAdd;
        user.RowVersion += 1;
        return true;
    }

    private async Task IncrementDailyProgressAsync(int userId, DateTime date)
    {
        if (_context.Database.IsRelational())
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO ""DailyProgresses"" (""UserId"", ""Date"", ""CompletedTasksCount"")
                VALUES ({userId}, {date}, 1)
                ON CONFLICT (""UserId"", ""Date"")
                DO UPDATE SET ""CompletedTasksCount"" = ""DailyProgresses"".""CompletedTasksCount"" + 1;");

            return;
        }

        var progress = await _context.DailyProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Date == date);

        if (progress == null)
        {
            _context.DailyProgresses.Add(new DailyProgress
            {
                UserId = userId,
                Date = date,
                CompletedTasksCount = 1
            });
        }
        else
        {
            progress.CompletedTasksCount += 1;
        }
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
