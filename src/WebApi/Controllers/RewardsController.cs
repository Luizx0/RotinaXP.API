using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RotinaXP.API.DTOs;
using RotinaXP.API.Extensions;
using RotinaXP.API.Features.Rewards.UseCases;
using RotinaXP.API.Services;

namespace RotinaXP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RewardsController : ControllerBase
{
    private readonly GetRewardsPageUseCase _getRewardsPageUseCase;
    private readonly GetRewardByIdUseCase _getRewardByIdUseCase;
    private readonly CreateRewardUseCase _createRewardUseCase;
    private readonly UpdateRewardUseCase _updateRewardUseCase;
    private readonly DeleteRewardUseCase _deleteRewardUseCase;
    private readonly RedeemRewardUseCase _redeemRewardUseCase;

    public RewardsController(
        GetRewardsPageUseCase getRewardsPageUseCase,
        GetRewardByIdUseCase getRewardByIdUseCase,
        CreateRewardUseCase createRewardUseCase,
        UpdateRewardUseCase updateRewardUseCase,
        DeleteRewardUseCase deleteRewardUseCase,
        RedeemRewardUseCase redeemRewardUseCase)
    {
        _getRewardsPageUseCase = getRewardsPageUseCase;
        _getRewardByIdUseCase = getRewardByIdUseCase;
        _createRewardUseCase = createRewardUseCase;
        _updateRewardUseCase = updateRewardUseCase;
        _deleteRewardUseCase = deleteRewardUseCase;
        _redeemRewardUseCase = redeemRewardUseCase;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<RewardDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] int page = PaginationDefaults.DefaultPage, [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
            return Unauthorized();

        var response = await _getRewardsPageUseCase.ExecuteAsync(authenticatedUserId.Value, page, pageSize);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RewardDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(int id)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
            return Unauthorized();

        var reward = await _getRewardByIdUseCase.ExecuteAsync(id, authenticatedUserId.Value);
        if (reward == null)
            return NotFound(new { message = "Reward not found" });

        return Ok(reward);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = "ResourceOwner")]
    [ProducesResponseType(typeof(PagedResult<RewardDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByUser(int userId, [FromQuery] int page = PaginationDefaults.DefaultPage, [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize)
    {
        var response = await _getRewardsPageUseCase.ExecuteAsync(userId, page, pageSize);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RewardDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateRewardDto request)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
            return Unauthorized();

        var result = await _createRewardUseCase.ExecuteAsync(request, authenticatedUserId.Value);
        if (!result.Success)
        {
            if (result.Message == "Forbidden")
                return Forbid();

            return BadRequest(new { message = result.Message });
        }

        if (result.Reward == null)
            return Forbid();

        return CreatedAtAction(nameof(GetById), new { id = result.Reward.Id }, result.Reward);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRewardDto request)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
            return Unauthorized();

        var result = await _updateRewardUseCase.ExecuteAsync(id, authenticatedUserId.Value, request);
        if (!result.Success)
        {
            if (result.Message == "Reward not found")
                return NotFound(new { message = result.Message });

            return BadRequest(new { message = result.Message });
        }

        return NoContent();
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

        var result = await _deleteRewardUseCase.ExecuteAsync(id, authenticatedUserId.Value);
        if (!result.Success)
            return NotFound(new { message = "Reward not found" });

        return NoContent();
    }

    [HttpPost("{id}/redeem")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Redeem(int id)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
            return Unauthorized();

        var result = await _redeemRewardUseCase.ExecuteAsync(id, authenticatedUserId.Value);
        if (!result.Success)
        {
            if (result.Message is "Reward not found" or "User not found")
                return NotFound(new { message = result.Message });

            if (result.Message == RewardService.ConcurrencyConflictMessage)
                return Conflict(new { message = result.Message });

            return BadRequest(new { message = result.Message });
        }

        return Ok(new
        {
            message = result.Message,
            pointsRemaining = result.PointsRemaining
        });
    }
}
