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
            .Include(t => t.GroupTeams)
            .Where(t => t.AppUserId == appUserId)
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync();
    }

    public async Task<List<Team>> GetAllByGroupAsync(int groupId, string appUserId)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .Include(t => t.GroupTeams)
            .Where(t => t.AppUserId == appUserId && t.GroupTeams.Any(gt => gt.GroupId == groupId))
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync();
    }

    // filtering by owner here means another user's team is simply "not found"
    public async Task<Team?> GetByIdAsync(int id, string appUserId)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .Include(t => t.GroupTeams)
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

    // replaces the whole team; old members and group links are deleted and new ones inserted in one SaveChanges.
    // a group link that stays is deleted and re-added with the same key; EF Core merges the two and sends nothing for it
    public async Task UpdateAsync(Team team, string name, List<Member> members, List<GroupTeam> groupTeams)
    {
        _context.Members.RemoveRange(team.Members);
        _context.GroupTeams.RemoveRange(team.GroupTeams);
        team.Name = name;
        team.Members = members;
        team.GroupTeams = groupTeams;
        team.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Team team)
    {
        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
    }
}
