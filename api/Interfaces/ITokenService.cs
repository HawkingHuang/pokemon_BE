using api.Models;

namespace api.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(AppUser user);
    string CreateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
