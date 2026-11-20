using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repository;

// adds and removes single links; the controller checks that both the group and the team are the user's
public class GroupTeamRepository : IGroupTeamRepository
{
    private readonly ApplicationDBContext _context;

    public GroupTeamRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(int groupId, int teamId)
    {
        return await _context.GroupTeams.AnyAsync(gt => gt.GroupId == groupId && gt.TeamId == teamId);
    }

    public async Task CreateAsync(GroupTeam groupTeam)
    {
        _context.GroupTeams.Add(groupTeam);
        await _context.SaveChangesAsync();
    }

    public async Task<GroupTeam?> DeleteAsync(int groupId, int teamId)
    {
        var groupTeam = await _context.GroupTeams.FirstOrDefaultAsync(gt => gt.GroupId == groupId && gt.TeamId == teamId);
        if (groupTeam == null) return null;

        _context.GroupTeams.Remove(groupTeam);
        await _context.SaveChangesAsync();
        return groupTeam;
    }
}
