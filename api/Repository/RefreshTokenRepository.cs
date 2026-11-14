using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repository;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDBContext _context;

    public RefreshTokenRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == tokenHash);
    }

    public async Task CreateAsync(RefreshToken refreshToken)
    {
        await RemoveExpiredAsync(refreshToken.AppUserId, refreshToken.CreatedAt);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task RotateAsync(RefreshToken current, RefreshToken replacement)
    {
        current.RevokedAt = replacement.CreatedAt;
        await RemoveExpiredAsync(replacement.AppUserId, replacement.CreatedAt);
        _context.RefreshTokens.Add(replacement);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Remove(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAllAsync(string appUserId)
    {
        var tokens = await _context.RefreshTokens.Where(r => r.AppUserId == appUserId).ToListAsync();
        _context.RefreshTokens.RemoveRange(tokens);
        await _context.SaveChangesAsync();
    }

    // only expired rows are removed; rotated rows stay until expiry so reuse can still be detected
    private async Task RemoveExpiredAsync(string appUserId, DateTime now)
    {
        var expired = await _context.RefreshTokens
            .Where(r => r.AppUserId == appUserId && r.ExpiresAt <= now)
            .ToListAsync();
        _context.RefreshTokens.RemoveRange(expired);
    }
}
