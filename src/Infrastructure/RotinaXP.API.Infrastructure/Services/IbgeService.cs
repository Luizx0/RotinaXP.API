using RotinaXP.API.Application.Interfaces.Services;
using RotinaXP.API.DTOs;
using RotinaXP.API.Infrastructure.Clients;
using System.Text.Json;

namespace RotinaXP.API.Services;

public class IbgeService : IIbgeService
{
    private readonly IbgeClient _client;

    public IbgeService(IbgeClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<IbgeStateDto>> GetStatesAsync()
    {
        return await _client.GetStatesAsync();
    }

    public async Task<IbgeIndicatorDto?> GetIndicatorAsync(string indicadorId, int ano, string? uf = null)
    {
        // Simple wrapper that calls an example SIDRA endpoint. For simplicity, try a basic values endpoint.
        // Note: indicadorId should be a valid SIDRA table id / resource for production.

        var url = $"/values/{indicadorId}?formato=json&periodo={ano}";
        if (!string.IsNullOrWhiteSpace(uf))
            url += $"&localidade={uf}";

        var doc = await _client.GetRawAsync(url);
        if (doc == null)
            return null;

        // Very small parsing to extract some values if available.
        try
        {
            var root = doc.RootElement;
            var dto = new IbgeIndicatorDto
            {
                IndicatorId = indicadorId,
                Year = ano,
                Uf = uf
            };

            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() >= 2)
            {
                // The first element usually contains metadata
                var metadata = root[0];
                if (metadata.TryGetProperty("D1N", out var nameProp))
                {
                    dto.Name = nameProp.GetString();
                }

                var valuesArray = root[1];
                if (valuesArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var v in valuesArray.EnumerateArray())
                    {
                        var period = v.TryGetProperty("V", out var valueProp) ? valueProp.ToString() : null;
                        // Many SIDRA structures are complex; here we try to parse a numeric field
                        decimal? number = null;
                        if (v.TryGetProperty("V", out var numericProp) && numericProp.ValueKind == JsonValueKind.Number)
                        {
                            if (numericProp.TryGetDecimal(out var d))
                                number = d;
                        }

                        dto.Values.Add(new IbgeIndicatorValueDto
                        {
                            Period = period,
                            Value = number
                        });
                    }
                }
            }

            return dto;
        }
        catch
        {
            return null;
        }
    }
}
