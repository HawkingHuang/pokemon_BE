using api.Models;

namespace api.Interfaces;

public interface IGroupTeamRepository
{
    Task<bool> ExistsAsync(int groupId, int teamId);
    Task CreateAsync(GroupTeam groupTeam);
    Task<GroupTeam?> DeleteAsync(int groupId, int teamId);
}
