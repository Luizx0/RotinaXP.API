using Microsoft.EntityFrameworkCore;
using RotinaXP.API.Data;
using RotinaXP.API.Models;

namespace RotinaXP.API.Services;

public class TaskService
{
    public const int CompletionPoints = 10;
    private readonly ApplicationDbContext _context;

    public TaskService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await _context.Tasks
            .Include(t => t.User)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _context.Tasks
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TaskItem?> GetByIdForUserAsync(int id, int userId)
    {
        return await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    public async Task<List<TaskItem>> GetByUserAsync(int userId)
    {
        return await _context.Tasks
            .Include(t => t.User)
            .Where(t => t.UserId == userId)
            .ToListAsync();
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
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == task.UserId);
                if (user == null)
                    return (false, "User not found", false);

                user.Points += CompletionPoints;
                pointsAwarded = true;

                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var progress = await _context.DailyProgresses
                    .FirstOrDefaultAsync(p => p.UserId == task.UserId && p.Date >= today && p.Date < tomorrow);

                if (progress == null)
                {
                    _context.DailyProgresses.Add(new DailyProgress
                    {
                        UserId = task.UserId,
                        Date = today,
                        CompletedTasksCount = 1
                    });
                }
                else
                {
                    progress.CompletedTasksCount += 1;
                }
            }
        }

        await _context.SaveChangesAsync();

        var message = pointsAwarded
            ? $"Task completed. {CompletionPoints} points awarded"
            : "Task updated successfully";

        return (true, message, pointsAwarded);
    }
}
