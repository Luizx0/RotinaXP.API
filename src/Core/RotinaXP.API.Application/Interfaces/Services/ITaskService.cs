using RotinaXP.API.DTOs;
using RotinaXP.API.Models;

namespace RotinaXP.API.Application.Interfaces.Services;

public interface ITaskService
{
    Task<PagedResult<TaskDTO>> GetByUserPagedAsync(int userId, int page, int pageSize);
    Task<TaskDTO?> GetTaskDtoByIdForUserAsync(int id, int userId);
    Task<TaskItem?> GetByIdForUserAsync(int id, int userId);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);
    Task<(bool Success, string Message, bool PointsAwarded)> UpdateWithGamificationAsync(int taskId, int userId, string? title, bool? isCompleted);
}
