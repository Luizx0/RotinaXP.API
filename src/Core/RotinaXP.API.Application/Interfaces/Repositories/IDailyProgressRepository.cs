using RotinaXP.API.DTOs;

namespace RotinaXP.API.Application.Interfaces.Repositories;

public interface IDailyProgressRepository
{
    Task<PagedResult<DailyProgressDTO>> GetByUserPagedAsync(int userId, int page, int pageSize);
    Task<DailyProgressDTO?> GetDtoByIdForUserAsync(int id, int userId);
}
