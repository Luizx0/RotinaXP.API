namespace RotinaXP.API.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "RotinaXP.API";
    public string Audience { get; set; } = "RotinaXP.Client";
    public string Key { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 120;
}
