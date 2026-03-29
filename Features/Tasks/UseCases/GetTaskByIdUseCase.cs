using RotinaXP.API.DTOs;
using RotinaXP.API.Services;

namespace RotinaXP.API.Features.Tasks.UseCases;

public class GetTaskByIdUseCase
{
    private readonly TaskService _taskService;

    public GetTaskByIdUseCase(TaskService taskService)
    {
        _taskService = taskService;
    }

    public Task<TaskDTO?> ExecuteAsync(int taskId, int userId)
    {
        return _taskService.GetTaskDtoByIdForUserAsync(taskId, userId);
    }
}
