using RotinaXP.API.Application.Interfaces.Services;
using RotinaXP.API.DTOs;

namespace RotinaXP.API.Features.Users.UseCases;

public class UpdateUserUseCase
{
    private readonly IUserService _service;

    public UpdateUserUseCase(IUserService service)
    {
        _service = service;
    }

    public Task<(bool Success, UserDTO? User, string Message)> ExecuteAsync(int userId, UpdateUserRequest request)
    {
        return _service.UpdateAsync(userId, request);
    }
}
