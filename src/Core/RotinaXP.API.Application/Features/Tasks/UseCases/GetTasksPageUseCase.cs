using RotinaXP.API.DTOs;
using RotinaXP.API.Application.Interfaces.Services;

namespace RotinaXP.API.Features.Tasks.UseCases;

public class GetTasksPageUseCase
{
    private readonly ITaskService _taskService;

    public GetTasksPageUseCase(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public Task<PagedResult<TaskDTO>> ExecuteAsync(int userId, int page, int pageSize)
    {
        return _taskService.GetByUserPagedAsync(userId, page, pageSize);
    }
}
