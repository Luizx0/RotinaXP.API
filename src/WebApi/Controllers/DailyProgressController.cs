using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RotinaXP.API.DTOs;
using RotinaXP.API.Extensions;
using RotinaXP.API.Features.DailyProgress.UseCases;

namespace RotinaXP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DailyProgressController : ControllerBase
{
    private readonly GetDailyProgressPageUseCase _getDailyProgressPageUseCase;
    private readonly GetDailyProgressByIdUseCase _getDailyProgressByIdUseCase;

    public DailyProgressController(
        GetDailyProgressPageUseCase getDailyProgressPageUseCase,
        GetDailyProgressByIdUseCase getDailyProgressByIdUseCase)
    {
        _getDailyProgressPageUseCase = getDailyProgressPageUseCase;
        _getDailyProgressByIdUseCase = getDailyProgressByIdUseCase;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<DailyProgressDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] int page = PaginationDefaults.DefaultPage, [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
            return Unauthorized();

        var response = await _getDailyProgressPageUseCase.ExecuteAsync(authenticatedUserId.Value, page, pageSize);
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

        var progress = await _getDailyProgressByIdUseCase.ExecuteAsync(id, authenticatedUserId.Value);
        if (progress == null)
            return NotFound(new { message = "Daily progress not found" });

        return Ok(progress);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = "ResourceOwner")]
    [ProducesResponseType(typeof(PagedResult<DailyProgressDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByUser(int userId, [FromQuery] int page = PaginationDefaults.DefaultPage, [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize)
    {
        var response = await _getDailyProgressPageUseCase.ExecuteAsync(userId, page, pageSize);
        return Ok(response);
    }
}
