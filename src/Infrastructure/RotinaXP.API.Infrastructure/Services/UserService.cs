using Microsoft.EntityFrameworkCore;
using Npgsql;
using RotinaXP.API.Data;
using RotinaXP.API.DTOs;
using RotinaXP.API.Models;
using RotinaXP.API.Application.Interfaces.Services;
using RotinaXP.API.Security;
namespace RotinaXP.API.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<UserDTO>> GetAllUsersAsync()
    {
        var pagedResult = await GetAllUsersPagedAsync(PaginationDefaults.DefaultPage, PaginationDefaults.DefaultPageSize);
        return pagedResult.Items;
    }

    public async Task<PagedResult<UserDTO>> GetAllUsersPagedAsync(int page, int pageSize)
    {
        var (normalizedPage, normalizedPageSize) = NormalizePagination(page, pageSize);

        var query = _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .Select(u => new UserDTO
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Points = u.Points
            });

        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync();

        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)normalizedPageSize);

        return new PagedResult<UserDTO>
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasNext = totalPages > 0 && normalizedPage < totalPages,
            HasPrevious = normalizedPage > PaginationDefaults.DefaultPage && totalPages > 0,
            Items = items
        };
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
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
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            return (false, null, "Email is already registered in the system");
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
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserDTO
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Points = u.Points
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UserDTO?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Email == email)
            .Select(u => new UserDTO
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Points = u.Points
            })
            .FirstOrDefaultAsync();
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
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            return (false, null, "Email is already registered in the system");
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
        return _passwordHasher.Hash(password);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return _passwordHasher.Verify(password, hash);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException postgresEx
            && postgresEx.SqlState == PostgresErrorCodes.UniqueViolation;
    }

    private static (int Page, int PageSize) NormalizePagination(int page, int pageSize)
    {
        var normalizedPage = page < PaginationDefaults.DefaultPage
            ? PaginationDefaults.DefaultPage
            : page;

        var normalizedPageSize = pageSize < 1
            ? PaginationDefaults.DefaultPageSize
            : Math.Min(pageSize, PaginationDefaults.MaxPageSize);

        return (normalizedPage, normalizedPageSize);
    }
}
