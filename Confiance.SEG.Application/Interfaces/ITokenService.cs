using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace Confiance.SEG.Application.Interfaces;

public interface ITokenService
{
    SecurityToken GenerateAcessToken(IEnumerable<Claim> claims, IConfiguration configuration);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token, IConfiguration configuration);
}
