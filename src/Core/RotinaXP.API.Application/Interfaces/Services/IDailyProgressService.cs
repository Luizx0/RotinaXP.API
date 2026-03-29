using RotinaXP.API.DTOs;

namespace RotinaXP.API.Application.Interfaces.Services;

public interface IDailyProgressService
{
    Task<PagedResult<DailyProgressDTO>> GetByUserPagedAsync(int userId, int page, int pageSize);
    Task<DailyProgressDTO?> GetDailyProgressDtoByIdForUserAsync(int id, int userId);
    Task<bool> UserExistsAsync(int userId);
}
