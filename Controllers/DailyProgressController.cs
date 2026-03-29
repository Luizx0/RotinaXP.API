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
public class DailyProgressController : ControllerBase
{
    private readonly DailyProgressService _service;

    public DailyProgressController(DailyProgressService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<DailyProgressDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
            return Unauthorized();

        var progress = await _service.GetByUserAsync(authenticatedUserId.Value);
        var response = progress.Select(ToDto).ToList();
        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DailyProgressDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(int id)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
            return Unauthorized();

        var progress = await _service.GetByIdForUserAsync(id, authenticatedUserId.Value);
        if (progress == null)
            return NotFound(new { message = "Daily progress not found" });

        return Ok(ToDto(progress));
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = "ResourceOwner")]
    [ProducesResponseType(typeof(List<DailyProgressDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var progress = await _service.GetByUserAsync(userId);
        var response = progress.Select(ToDto).ToList();
        return Ok(response);
    }

    private static DailyProgressDTO ToDto(DailyProgress progress)
    {
        return new DailyProgressDTO
        {
            Id = progress.Id,
            Date = progress.Date,
            CompletedTasksCount = progress.CompletedTasksCount,
            UserId = progress.UserId
        };
    }
}
