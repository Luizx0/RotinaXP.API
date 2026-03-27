using Microsoft.AspNetCore.Mvc;
using RotinaXP.API.Models;
using RotinaXP.API.Services;

namespace RotinaXP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DailyProgressController : ControllerBase
{
    private readonly DailyProgressService _service;

    public DailyProgressController(DailyProgressService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<DailyProgress>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var progress = await _service.GetAllAsync();
        return Ok(progress);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DailyProgress), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var progress = await _service.GetByIdAsync(id);
        if (progress == null)
            return NotFound(new { message = "Daily progress not found" });

        return Ok(progress);
    }

    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(List<DailyProgress>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var userExists = await _service.UserExistsAsync(userId);
        if (!userExists)
            return NotFound(new { message = "User not found" });

        var progress = await _service.GetByUserAsync(userId);
        return Ok(progress);
    }
}
