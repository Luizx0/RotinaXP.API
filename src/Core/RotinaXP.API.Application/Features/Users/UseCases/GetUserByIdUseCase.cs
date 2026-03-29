using RotinaXP.API.Application.Interfaces.Services;
using RotinaXP.API.DTOs;

namespace RotinaXP.API.Features.Users.UseCases;

public class GetUserByIdUseCase
{
    private readonly IUserService _service;

    public GetUserByIdUseCase(IUserService service)
    {
        _service = service;
    }

    public Task<UserDTO?> ExecuteAsync(int userId)
    {
        return _service.GetUserByIdAsync(userId);
    }
}
