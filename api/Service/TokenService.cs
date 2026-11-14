using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using api.Interfaces;
using api.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace api.Service;

public class TokenService : ITokenService
{
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromHours(1);

    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _key;

    public TokenService(IConfiguration config)
    {
        _config = config;
        var signingKey = _config["JWT:SigningKey"]
            ?? throw new InvalidOperationException("JWT:SigningKey is not configured. Set it with dotnet user-secrets.");
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
    }

    public string CreateAccessToken(AppUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(AccessTokenLifetime),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512),
            Issuer = _config["JWT:Issuer"],
            Audience = _config["JWT:Audience"]
        };

        return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
    }

    public string CreateRefreshToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    public string HashRefreshToken(string refreshToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
}
