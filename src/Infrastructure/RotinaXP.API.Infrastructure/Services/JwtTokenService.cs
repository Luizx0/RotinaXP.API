using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RotinaXP.API.DTOs;

namespace RotinaXP.API.Services;

public class JwtTokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _secret;
    private readonly string _superUserEmail;
    private readonly int _expiryMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        _issuer = configuration["Jwt:Issuer"] ?? "RotinaXP.API";
        _audience = configuration["Jwt:Audience"] ?? "RotinaXP.Client";
        _secret = configuration["Jwt:Key"]
            ?? Environment.GetEnvironmentVariable("ROTINAXP_JWT_KEY")
            ?? throw new InvalidOperationException("JWT secret key was not configured.");
        _superUserEmail = configuration["SuperUser:Email"]
            ?? Environment.GetEnvironmentVariable("ROTINAXP_SUPERUSER_EMAIL")
            ?? string.Empty;
        _expiryMinutes = configuration.GetValue<int?>("Jwt:ExpiryMinutes") ?? 120;
    }

    public string GenerateToken(UserDTO user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name)
        };

        if (!string.IsNullOrWhiteSpace(_superUserEmail)
            && string.Equals(user.Email, _superUserEmail, StringComparison.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(ClaimTypes.Role, "SUPERUSER"));
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
