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
public class RewardsController : ControllerBase
{
    private readonly RewardService _service;

    public RewardsController(RewardService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<RewardDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
            return Unauthorized();

        var rewards = await _service.GetByUserAsync(authenticatedUserId.Value);
        var response = rewards.Select(ToDto).ToList();
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

        var reward = await _service.GetByIdForUserAsync(id, authenticatedUserId.Value);
        if (reward == null)
            return NotFound(new { message = "Reward not found" });

        return Ok(ToDto(reward));
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = "ResourceOwner")]
    [ProducesResponseType(typeof(List<RewardDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var rewards = await _service.GetByUserAsync(userId);
        var response = rewards.Select(ToDto).ToList();
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

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Title is required" });

        if (request.PointsCost <= 0)
            return BadRequest(new { message = "PointsCost must be greater than zero" });

        if (request.UserId != authenticatedUserId.Value)
            return Forbid();

        var reward = new Reward
        {
            Title = request.Title,
            PointsCost = request.PointsCost,
            UserId = authenticatedUserId.Value
        };

        await _service.CreateAsync(reward);

    return CreatedAtAction(nameof(GetById), new { id = reward.Id }, ToDto(reward));
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

        var reward = await _service.GetByIdForUserAsync(id, authenticatedUserId.Value);
        if (reward == null)
            return NotFound(new { message = "Reward not found" });

        if (!string.IsNullOrWhiteSpace(request.Title))
            reward.Title = request.Title;

        if (request.PointsCost.HasValue)
        {
            if (request.PointsCost <= 0)
                return BadRequest(new { message = "PointsCost must be greater than zero" });

            reward.PointsCost = request.PointsCost.Value;
        }

        await _service.UpdateAsync(reward);

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

        var reward = await _service.GetByIdForUserAsync(id, authenticatedUserId.Value);
        if (reward == null)
            return NotFound(new { message = "Reward not found" });

        await _service.DeleteAsync(reward);

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

        var result = await _service.RedeemAsync(id, authenticatedUserId.Value);
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

    private static RewardDTO ToDto(Reward reward)
    {
        return new RewardDTO
        {
            Id = reward.Id,
            Title = reward.Title,
            PointsCost = reward.PointsCost,
            UserId = reward.UserId
        };
    }
}
