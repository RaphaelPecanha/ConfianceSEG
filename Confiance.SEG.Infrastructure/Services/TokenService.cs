using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Confiance.SEG.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Confiance.SEG.Infrastructure.Services;

public class TokenService : ITokenService
{
    public SecurityToken GenerateAcessToken(IEnumerable<Claim> claims, IConfiguration configuration)
    {
        var secretKey = configuration["JWT:SecretKey"] ?? throw new ArgumentException("Invalid secret key");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["JWT:ValidIssuer"],
            audience: configuration["JWT:ValidAudiences"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(int.TryParse(configuration["JWT:TokenValidityInMinutes"], out var v) ? v : 30),
            signingCredentials: creds
        );

        return token;
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token, IConfiguration configuration)
    {
        var secretKey = configuration["JWT:SecretKey"] ?? throw new ArgumentException("Invalid secret key");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is JwtSecurityToken jwt && jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return principal;
            }
        }
        catch
        {
            // ignore and return null
        }

        return null;
    }
}
