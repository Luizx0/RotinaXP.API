using Microsoft.AspNetCore.Mvc;
using RotinaXP.API.DTOs;
using RotinaXP.API.Services;

namespace RotinaXP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _service;
    private const string DuplicateEmailMessage = "Email is already registered in the system";

    public AuthController(UserService service)
    {
        _service = service;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
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

        var response = new LoginResponse
        {
            Message = result.Message,
            User = result.User
        };

        return Created($"/api/users/{result.User.Id}", response);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _service.LoginAsync(request);

        if (!result.Success || result.User == null)
        {
            if (result.Message == "User not found")
                return NotFound(new { message = result.Message });

            return BadRequest(new { message = result.Message });
        }

        var response = new LoginResponse
        {
            Message = result.Message,
            User = result.User
        };

        return Ok(response);
    }
}