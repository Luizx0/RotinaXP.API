using System.Text.Json.Serialization;

namespace RotinaXP.API.DTOs;

public class IbgeStateDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("sigla")]
    public string Sigla { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;
}

public class IbgeIndicatorValueDto
{
    public string? Period { get; set; }
    public decimal? Value { get; set; }
}

public class IbgeIndicatorDto
{
    public string IndicatorId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int Year { get; set; }
    public string? Uf { get; set; }
    public List<IbgeIndicatorValueDto> Values { get; set; } = new();
}
