using Microsoft.EntityFrameworkCore;
using RotinaXP.API.Data;
using RotinaXP.API.DTOs;
using RotinaXP.API.Models;
using System.Security.Cryptography;
using System.Text;
namespace RotinaXP.API.Services;
public class UserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, UserDTO? User, string Message)> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser != null)
            return (false, null, "Email is already registered in the system");

        if (string.IsNullOrWhiteSpace(request.Name))
            return (false, null, "Name is required");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return (false, null, "Password must be at least 6 characters");

        try
        {
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                Points = 0
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDTO = MapToDto(user);
            return (true, userDTO, "User registered successfully");
        }
        catch (Exception ex)
        {
            return (false, null, $"Error registering user: {ex.Message}");
        }
    }

    public async Task<(bool Success, UserDTO? User, string Message)> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return (false, null, "Email and password are required");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return (false, null, "User not found");

        if (!VerifyPassword(request.Password, user.PasswordHash))
            return (false, null, "Invalid password");

        var userDTO = MapToDto(user);
        return (true, userDTO, "Login successful");
    }

    public async Task<UserDTO?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDTO?> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        return user == null ? null : MapToDto(user);
    }

    public async Task<(bool Success, UserDTO? User, string Message)> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return (false, null, "User not found");

        if (!string.IsNullOrWhiteSpace(request.Name))
            user.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
        {
            var existingEmail = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingEmail != null)
                return (false, null, "Email is already registered in the system");

            user.Email = request.Email;
        }

        try
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var userDTO = MapToDto(user);
            return (true, userDTO, "User updated successfully");
        }
        catch (Exception ex)
        {
            return (false, null, $"Error updating user: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return (false, "User not found");

        try
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return (true, "User deleted successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error deleting user: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> AddPointsAsync(int id, int points)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return (false, "User not found");

        if (points < 0)
            return (false, "Points amount cannot be negative");

        try
        {
            user.Points += points;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return (true, $"Added {points} points to user");
        }
        catch (Exception ex)
        {
            return (false, $"Error adding points: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeductPointsAsync(int id, int points)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return (false, "User not found");

        if (points < 0)
            return (false, "Points amount cannot be negative");

        if (user.Points < points)
            return (false, "Insufficient points balance");

        try
        {
            user.Points -= points;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return (true, $"Deducted {points} points from user");
        }
        catch (Exception ex)
        {
            return (false, $"Error deducting points: {ex.Message}");
        }
    }

    private static UserDTO MapToDto(User user)
    {
        return new UserDTO
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Points = user.Points
        };
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password).Equals(hash, StringComparison.Ordinal);
    }
}
