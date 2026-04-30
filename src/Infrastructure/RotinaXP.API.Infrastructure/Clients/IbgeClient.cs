using System.Net.Http.Json;
using System.Text.Json;
using RotinaXP.API.DTOs;

namespace RotinaXP.API.Infrastructure.Clients;

public class IbgeClient
{
    private readonly HttpClient _http;

    public IbgeClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<IbgeStateDto>> GetStatesAsync(CancellationToken ct = default)
    {
        var res = await _http.GetAsync("/api/v1/localidades/estados", ct);
        res.EnsureSuccessStatusCode();
        var list = await res.Content.ReadFromJsonAsync<List<IbgeStateDto>>(cancellationToken: ct);
        return list ?? new List<IbgeStateDto>();
    }

    public async Task<JsonDocument?> GetRawAsync(string relativeUrl, CancellationToken ct = default)
    {
        var res = await _http.GetAsync(relativeUrl, ct);
        if (!res.IsSuccessStatusCode)
            return null;

        var stream = await res.Content.ReadAsStreamAsync(ct);
        return await JsonDocument.ParseAsync(stream, cancellationToken: ct);
    }
}
