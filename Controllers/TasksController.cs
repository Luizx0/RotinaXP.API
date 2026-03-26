using Microsoft.AspNetCore.Mvc;
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
    [ProducesResponseType(typeof(List<TaskItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var tasks = await _service.GetAllAsync();

        return Ok(tasks);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _service.GetByIdAsync(id);

        if (task == null)
            return NotFound(new { message = "Task not found" });

        return Ok(task);
    }

    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(List<TaskItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var userExists = await _service.UserExistsAsync(userId);
        if (!userExists)
            return NotFound(new { message = "User not found" });

        var tasks = await _service.GetByUserAsync(userId);

        return Ok(tasks);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
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

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request)
    {
        var task = await _service.GetByIdAsync(id);
        if (task == null)
            return NotFound(new { message = "Task not found" });

        if (!string.IsNullOrWhiteSpace(request.Title))
            task.Title = request.Title;

        if (request.IsCompleted.HasValue)
            task.IsCompleted = request.IsCompleted.Value;

        await _service.UpdateAsync(task);

        return NoContent();
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
}

public record CreateTaskRequest(string Title, bool IsCompleted, int UserId);
public record UpdateTaskRequest(string? Title, bool? IsCompleted);
