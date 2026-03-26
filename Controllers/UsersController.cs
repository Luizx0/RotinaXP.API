using Microsoft.AspNetCore.Mvc;
using RotinaXP.API.Models;
using RotinaXP.API.Services;
namespace RotinaXP.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _service;

    public UsersController(UserService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<User>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _service.GetAllAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _service.GetByIdAsync(id);

        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(user);
    }

    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Name and Email are required" });

        var existingUser = await _service.GetByEmailAsync(request.Email);

        if (existingUser != null)
            return BadRequest(new { message = "Email is already registered" });

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = request.PasswordHash,
            Points = request.Points
        };

        await _service.CreateAsync(user);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await _service.GetByIdAsync(id);
        if (user == null)
            return NotFound(new { message = "User not found" });

        if (!string.IsNullOrWhiteSpace(request.Name))
            user.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email;

        if (request.Points.HasValue)
            user.Points = request.Points.Value;

        await _service.UpdateAsync(user);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _service.GetByIdAsync(id);
        if (user == null)
            return NotFound(new { message = "User not found" });

        await _service.DeleteAsync(user);

        return NoContent();
    }
}

public record CreateUserRequest(string Name, string Email, string PasswordHash, int Points = 0);
public record UpdateUserRequest(string? Name, string? Email, int? Points);
