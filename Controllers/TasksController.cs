using Microsoft.AspNetCore.Mvc;
using RotinaXP.API.DTOs;
using RotinaXP.API.Models;
using RotinaXP.API.Services;
namespace RotinaXP.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly TaskService _service;

    public TasksController(TaskService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<TaskDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var tasks = await _service.GetAllAsync();
        var response = tasks.Select(ToDto).ToList();

        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _service.GetByIdAsync(id);

        if (task == null)
            return NotFound(new { message = "Task not found" });

        return Ok(ToDto(task));
    }

    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(List<TaskDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var userExists = await _service.UserExistsAsync(userId);
        if (!userExists)
            return NotFound(new { message = "User not found" });

        var tasks = await _service.GetByUserAsync(userId);
        var response = tasks.Select(ToDto).ToList();

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Title is required" });

        var userExists = await _service.UserExistsAsync(request.UserId);
        if (!userExists)
            return BadRequest(new { message = "UserId not found" });

        var task = new TaskItem
        {
            Title = request.Title,
            IsCompleted = request.IsCompleted,
            UserId = request.UserId
        };

        await _service.CreateAsync(task);

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, ToDto(task));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto request)
    {
        var result = await _service.UpdateWithGamificationAsync(id, request.Title, request.IsCompleted);
        if (!result.Success)
        {
            if (result.Message == "Task not found")
                return NotFound(new { message = result.Message });

            if (result.Message == "Completed tasks cannot be reopened")
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
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _service.GetByIdAsync(id);
        if (task == null)
            return NotFound(new { message = "Task not found" });

        await _service.DeleteAsync(task);

        return NoContent();
    }

    private static TaskDTO ToDto(TaskItem task)
    {
        return new TaskDTO
        {
            Id = task.Id,
            Title = task.Title,
            IsCompleted = task.IsCompleted,
            UserId = task.UserId
        };
    }
}
