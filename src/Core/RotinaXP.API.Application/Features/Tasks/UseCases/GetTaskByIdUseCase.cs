using RotinaXP.API.DTOs;
using RotinaXP.API.Application.Interfaces.Services;

namespace RotinaXP.API.Features.Tasks.UseCases;

public class GetTaskByIdUseCase
{
    private readonly ITaskService _taskService;

    public GetTaskByIdUseCase(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public Task<TaskDTO?> ExecuteAsync(int taskId, int userId)
    {
        return _taskService.GetTaskDtoByIdForUserAsync(taskId, userId);
    }
}
