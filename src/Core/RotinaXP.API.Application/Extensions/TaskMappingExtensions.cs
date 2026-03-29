using RotinaXP.API.DTOs;
using RotinaXP.API.Models;

namespace RotinaXP.API.Application.Extensions;

public static class TaskMappingExtensions
{
    public static TaskDTO ToDto(this TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        IsCompleted = task.IsCompleted,
        UserId = task.UserId
    };
}
