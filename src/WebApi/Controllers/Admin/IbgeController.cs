using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RotinaXP.API.Application.Interfaces.Services;
using RotinaXP.API.DTOs;

namespace RotinaXP.API.Controllers;

[ApiController]
[Route("admin/ibge")]
[Authorize(Policy = "RequireAdmin")]
public class IbgeController : ControllerBase
{
    private readonly IIbgeService _service;

    public IbgeController(IIbgeService service)
    {
        _service = service;
    }

    [HttpGet("estados")]
    [ProducesResponseType(typeof(IEnumerable<IbgeStateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstados()
    {
        var states = await _service.GetStatesAsync();
        return Ok(states);
    }

    [HttpGet("indicadores")]
    [ProducesResponseType(typeof(IbgeIndicatorDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIndicador([FromQuery] string indicadorId, [FromQuery] int ano, [FromQuery] string? uf = null)
    {
        if (string.IsNullOrWhiteSpace(indicadorId))
            return BadRequest(new { message = "indicadorId is required" });

        var dto = await _service.GetIndicatorAsync(indicadorId, ano, uf);
        if (dto == null)
            return NotFound(new { message = "Indicator not found or remote API error" });

        return Ok(dto);
    }
}
