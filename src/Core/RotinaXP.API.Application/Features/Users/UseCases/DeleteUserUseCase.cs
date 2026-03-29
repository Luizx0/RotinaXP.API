using RotinaXP.API.Application.Interfaces.Services;

namespace RotinaXP.API.Features.Users.UseCases;

public class DeleteUserUseCase
{
    private readonly IUserService _service;

    public DeleteUserUseCase(IUserService service)
    {
        _service = service;
    }

    public Task<(bool Success, string Message)> ExecuteAsync(int userId)
    {
        return _service.DeleteAsync(userId);
    }
}
