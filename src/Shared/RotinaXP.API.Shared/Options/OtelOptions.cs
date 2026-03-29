namespace RotinaXP.API.Options;

public class OtelOptions
{
    public const string SectionName = "OpenTelemetry";

    public string ServiceName { get; set; } = "RotinaXP.API";
    public OtelOtlpOptions Otlp { get; set; } = new();
}

public class OtelOtlpOptions
{
    public string Endpoint { get; set; } = string.Empty;
}
