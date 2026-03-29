namespace RotinaXP.API.Options;

public class AppCorsOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } =
    [
        "http://localhost:3000",
        "http://localhost:5173"
    ];
}
