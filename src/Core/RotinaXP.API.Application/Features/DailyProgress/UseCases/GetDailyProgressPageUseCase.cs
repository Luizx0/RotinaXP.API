using RotinaXP.API.Application.Interfaces.Services;
using RotinaXP.API.DTOs;

namespace RotinaXP.API.Features.DailyProgress.UseCases;

public class GetDailyProgressPageUseCase
{
    private readonly IDailyProgressService _service;

    public GetDailyProgressPageUseCase(IDailyProgressService service)
    {
        _service = service;
    }

    public Task<PagedResult<DailyProgressDTO>> ExecuteAsync(int userId, int page, int pageSize)
    {
        return _service.GetByUserPagedAsync(userId, page, pageSize);
    }
}
