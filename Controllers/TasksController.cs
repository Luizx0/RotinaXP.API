using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RotinaXP.API.DTOs;
using RotinaXP.API.Extensions;
using RotinaXP.API.Features.Tasks.UseCases;
using RotinaXP.API.Services;
namespace RotinaXP.API.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly GetTasksPageUseCase _getTasksPageUseCase;
    private readonly GetTaskByIdUseCase _getTaskByIdUseCase;
    private readonly CreateTaskUseCase _createTaskUseCase;
    private readonly UpdateTaskUseCase _updateTaskUseCase;
    private readonly DeleteTaskUseCase _deleteTaskUseCase;

    public TasksController(
        GetTasksPageUseCase getTasksPageUseCase,
        GetTaskByIdUseCase getTaskByIdUseCase,
        CreateTaskUseCase createTaskUseCase,
        UpdateTaskUseCase updateTaskUseCase,
        DeleteTaskUseCase deleteTaskUseCase)
    {
        _getTasksPageUseCase = getTasksPageUseCase;
        _getTaskByIdUseCase = getTaskByIdUseCase;
        _createTaskUseCase = createTaskUseCase;
        _updateTaskUseCase = updateTaskUseCase;
        _deleteTaskUseCase = deleteTaskUseCase;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] int page = PaginationDefaults.DefaultPage, [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
            return Unauthorized();

        var response = await _getTasksPageUseCase.ExecuteAsync(authenticatedUserId.Value, page, pageSize);

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

        var task = await _getTaskByIdUseCase.ExecuteAsync(id, authenticatedUserId.Value);

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
        var response = await _getTasksPageUseCase.ExecuteAsync(userId, page, pageSize);

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

        var result = await _createTaskUseCase.ExecuteAsync(request, authenticatedUserId.Value);
        if (!result.Success)
        {
            if (result.Message == "Forbidden")
                return Forbid();

            return BadRequest(new { message = result.Message });
        }

        if (result.Task == null)
            return Forbid();

        return CreatedAtAction(nameof(GetById), new { id = result.Task.Id }, result.Task);
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

        var result = await _updateTaskUseCase.ExecuteAsync(id, authenticatedUserId.Value, request);
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

        var result = await _deleteTaskUseCase.ExecuteAsync(id, authenticatedUserId.Value);
        if (!result.Success)
            return NotFound(new { message = "Task not found" });

        return NoContent();
    }
}
