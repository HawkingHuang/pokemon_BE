using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repository;

public class TeamRepository : ITeamRepository
{
    private readonly ApplicationDBContext _context;

    public TeamRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<List<Team>> GetAllByUserAsync(string appUserId)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .Where(t => t.AppUserId == appUserId)
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync();
    }

    // filtering by owner here means another user's team is simply "not found"
    public async Task<Team?> GetByIdAsync(int id, string appUserId)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id && t.AppUserId == appUserId);
    }

    public async Task<bool> NameExistsAsync(string appUserId, string name, int? excludeTeamId = null)
    {
        return await _context.Teams.AnyAsync(t =>
            t.AppUserId == appUserId && t.Name == name && (excludeTeamId == null || t.Id != excludeTeamId));
    }

    public async Task<Team> CreateAsync(Team team)
    {
        team.CreatedAt = team.UpdatedAt = DateTime.UtcNow;
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();
        return team;
    }

    // replaces the whole team; old members are deleted and new ones inserted in one SaveChanges
    public async Task UpdateAsync(Team team, string name, List<Member> members)
    {
        _context.Members.RemoveRange(team.Members);
        team.Name = name;
        team.Members = members;
        team.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Team team)
    {
        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
    }
}
