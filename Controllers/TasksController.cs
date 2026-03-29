using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RotinaXP.API.DTOs;
using RotinaXP.API.Extensions;
using RotinaXP.API.Models;
using RotinaXP.API.Services;
namespace RotinaXP.API.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly TaskService _service;

    public TasksController(TaskService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] int page = PaginationDefaults.DefaultPage, [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
            return Unauthorized();

        var response = await _service.GetByUserPagedAsync(authenticatedUserId.Value, page, pageSize);

        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(int id)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
            return Unauthorized();

        var task = await _service.GetTaskDtoByIdForUserAsync(id, authenticatedUserId.Value);

        if (task == null)
            return NotFound(new { message = "Task not found" });

        return Ok(task);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = "ResourceOwner")]
    [ProducesResponseType(typeof(PagedResult<TaskDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByUser(int userId, [FromQuery] int page = PaginationDefaults.DefaultPage, [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize)
    {
        var response = await _service.GetByUserPagedAsync(userId, page, pageSize);

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto request)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Title is required" });

        if (request.UserId != authenticatedUserId.Value)
            return Forbid();

        var task = new TaskItem
        {
            Title = request.Title,
            IsCompleted = request.IsCompleted,
            UserId = authenticatedUserId.Value
        };

        await _service.CreateAsync(task);

        var createdTask = await _service.GetTaskDtoByIdForUserAsync(task.Id, authenticatedUserId.Value);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, createdTask);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto request)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
            return Unauthorized();

        var result = await _service.UpdateWithGamificationAsync(id, authenticatedUserId.Value, request.Title, request.IsCompleted);
        if (!result.Success)
        {
            if (result.Message == "Task not found")
                return NotFound(new { message = result.Message });

            if (result.Message == "Completed tasks cannot be reopened" || result.Message == TaskService.ConcurrencyConflictMessage)
                return Conflict(new { message = result.Message });

            return BadRequest(new { message = result.Message });
        }

        return Ok(new
        {
            message = result.Message,
            pointsAwarded = result.PointsAwarded
        });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
            return Unauthorized();

        var task = await _service.GetByIdForUserAsync(id, authenticatedUserId.Value);
        if (task == null)
            return NotFound(new { message = "Task not found" });

        await _service.DeleteAsync(task);

        return NoContent();
    }
}
