using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoEscola.Api.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace AutoEscola.Api.Infrastructure.Security;

public class JwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public (string token, int expiresInSeconds) GenerateAccessToken(User user, Guid? profileId)
    {
        var jwtSection = _config.GetSection("JwtSettings");
        var secret = jwtSection["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured.");
        var expirationMinutes = int.Parse(jwtSection["AccessTokenExpirationMinutes"] ?? "30");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("profileId", profileId?.ToString() ?? string.Empty)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expirationMinutes * 60);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
