using RotinaXP.API.DTOs;
using RotinaXP.API.Models;
using RotinaXP.API.Application.Interfaces.Services;

namespace RotinaXP.API.Features.Tasks.UseCases;

public class CreateTaskUseCase
{
    private readonly ITaskService _taskService;

    public CreateTaskUseCase(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public async Task<(bool Success, string Message, TaskDTO? Task)> ExecuteAsync(CreateTaskDto request, int authenticatedUserId)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return (false, "Title is required", null);

        if (request.UserId != authenticatedUserId)
            return (false, "Forbidden", null);

        var task = new TaskItem
        {
            Title = request.Title,
            IsCompleted = request.IsCompleted,
            UserId = authenticatedUserId
        };

        await _taskService.CreateAsync(task);
        var createdTask = await _taskService.GetTaskDtoByIdForUserAsync(task.Id, authenticatedUserId);

        return (true, "Task created successfully", createdTask);
    }
}
