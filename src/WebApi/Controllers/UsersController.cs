using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RotinaXP.API.Extensions;
using RotinaXP.API.DTOs;
using RotinaXP.API.Application.Interfaces.Services;
using RotinaXP.API.Features.Users.UseCases;
namespace RotinaXP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;
    private readonly GetUsersPageUseCase _getUsersPage;
    private readonly GetUserByIdUseCase _getUserById;
    private readonly UpdateUserUseCase _updateUser;
    private readonly DeleteUserUseCase _deleteUser;
    private const string DuplicateEmailMessage = "Email is already registered in the system";

    public UsersController(
        IUserService service,
        GetUsersPageUseCase getUsersPage,
        GetUserByIdUseCase getUserById,
        UpdateUserUseCase updateUser,
        DeleteUserUseCase deleteUser)
    {
        _service = service;
        _getUsersPage = getUsersPage;
        _getUserById = getUserById;
        _updateUser = updateUser;
        _deleteUser = deleteUser;
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<UserDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] int page = PaginationDefaults.DefaultPage, [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
            return Unauthorized();
        var response = await _getUsersPage.ExecuteAsync(authenticatedUserId.Value, page, pageSize);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ResourceOwner")]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _getUserById.ExecuteAsync(id);
        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(user);
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] RegisterRequest request)
    {
        var result = await _service.RegisterAsync(request);
        if (!result.Success || result.User == null)
        {
            if (result.Message == DuplicateEmailMessage)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Conflict",
                    Detail = result.Message,
                    Status = StatusCodes.Status409Conflict
                });
            }

            return BadRequest(new { message = result.Message });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.User.Id }, result.User);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ResourceOwner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var result = await _updateUser.ExecuteAsync(id, request);
        if (!result.Success)
        {
            if (result.Message == "User not found")
                return NotFound(new { message = result.Message });

            if (result.Message == DuplicateEmailMessage)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Conflict",
                    Detail = result.Message,
                    Status = StatusCodes.Status409Conflict
                });
            }

            return BadRequest(new { message = result.Message });
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ResourceOwner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _deleteUser.ExecuteAsync(id);
        if (!result.Success)
            return NotFound(new { message = result.Message });

        return NoContent();
    }
}
