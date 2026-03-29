using RotinaXP.API.DTOs;
using RotinaXP.API.Services;

namespace RotinaXP.API.Features.Tasks.UseCases;

public class UpdateTaskUseCase
{
    private readonly TaskService _taskService;

    public UpdateTaskUseCase(TaskService taskService)
    {
        _taskService = taskService;
    }

    public Task<(bool Success, string Message, bool PointsAwarded)> ExecuteAsync(int taskId, int userId, UpdateTaskDto request)
    {
        return _taskService.UpdateWithGamificationAsync(taskId, userId, request.Title, request.IsCompleted);
    }
}
