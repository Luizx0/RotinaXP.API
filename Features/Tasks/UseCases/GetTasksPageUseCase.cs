using RotinaXP.API.DTOs;
using RotinaXP.API.Services;

namespace RotinaXP.API.Features.Tasks.UseCases;

public class GetTasksPageUseCase
{
    private readonly TaskService _taskService;

    public GetTasksPageUseCase(TaskService taskService)
    {
        _taskService = taskService;
    }

    public Task<PagedResult<TaskDTO>> ExecuteAsync(int userId, int page, int pageSize)
    {
        return _taskService.GetByUserPagedAsync(userId, page, pageSize);
    }
}
