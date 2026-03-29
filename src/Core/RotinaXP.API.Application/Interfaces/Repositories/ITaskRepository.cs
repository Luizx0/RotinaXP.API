using RotinaXP.API.DTOs;
using RotinaXP.API.Models;

namespace RotinaXP.API.Application.Interfaces.Repositories;

public interface ITaskRepository
{
    Task<PagedResult<TaskDTO>> GetByUserPagedAsync(int userId, int page, int pageSize);
    Task<TaskItem?> GetByIdForUserAsync(int id, int userId);
    Task<TaskDTO?> GetDtoByIdForUserAsync(int id, int userId);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);
}
