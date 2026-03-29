using RotinaXP.API.Application.Interfaces.Services;
using RotinaXP.API.DTOs;

namespace RotinaXP.API.Features.DailyProgress.UseCases;

public class GetDailyProgressByIdUseCase
{
    private readonly IDailyProgressService _service;

    public GetDailyProgressByIdUseCase(IDailyProgressService service)
    {
        _service = service;
    }

    public Task<DailyProgressDTO?> ExecuteAsync(int progressId, int userId)
    {
        return _service.GetDailyProgressDtoByIdForUserAsync(progressId, userId);
    }
}
