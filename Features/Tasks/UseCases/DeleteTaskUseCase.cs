using RotinaXP.API.Services;

namespace RotinaXP.API.Features.Tasks.UseCases;

public class DeleteTaskUseCase
{
    private readonly TaskService _taskService;

    public DeleteTaskUseCase(TaskService taskService)
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
