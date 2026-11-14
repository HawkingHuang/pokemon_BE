using api.Models;

namespace api.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash);
    Task CreateAsync(RefreshToken refreshToken);
    Task RotateAsync(RefreshToken current, RefreshToken replacement);
    Task DeleteAsync(RefreshToken refreshToken);
    Task DeleteAllAsync(string appUserId);
}
