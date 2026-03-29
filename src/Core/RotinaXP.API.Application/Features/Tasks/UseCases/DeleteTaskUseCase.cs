using RotinaXP.API.Application.Interfaces.Services;

namespace RotinaXP.API.Features.Tasks.UseCases;

public class DeleteTaskUseCase
{
    private readonly ITaskService _taskService;

    public DeleteTaskUseCase(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public async Task<(bool Success, string Message)> ExecuteAsync(int taskId, int userId)
    {
        var task = await _taskService.GetByIdForUserAsync(taskId, userId);
        if (task == null)
            return (false, "Task not found");

        await _taskService.DeleteAsync(task);
        return (true, "Task deleted successfully");
    }
}
