using RotinaXP.API.DTOs;

namespace RotinaXP.API.Application.Interfaces.Services;

public interface IIbgeService
{
    Task<IEnumerable<IbgeStateDto>> GetStatesAsync();
    Task<IbgeIndicatorDto?> GetIndicatorAsync(string indicadorId, int ano, string? uf = null);
}
