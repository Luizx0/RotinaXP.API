using RotinaXP.API.DTOs;
using RotinaXP.API.Application.Interfaces.Services;

namespace RotinaXP.API.Features.Tasks.UseCases;

public class UpdateTaskUseCase
{
    private readonly ITaskService _taskService;

    public UpdateTaskUseCase(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public Task<(bool Success, string Message, bool PointsAwarded)> ExecuteAsync(int taskId, int userId, UpdateTaskDto request)
    {
        return _taskService.UpdateWithGamificationAsync(taskId, userId, request.Title, request.IsCompleted);
    }
}
