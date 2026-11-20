using api.Models;

namespace api.Interfaces;

public interface ITeamRepository
{
    Task<List<Team>> GetAllByUserAsync(string appUserId);
    Task<List<Team>> GetAllByGroupAsync(int groupId, string appUserId);
    Task<Team?> GetByIdAsync(int id, string appUserId);
    Task<bool> NameExistsAsync(string appUserId, string name, int? excludeTeamId = null);
    Task<Team> CreateAsync(Team team);
    Task UpdateAsync(Team team, string name, List<Member> members, List<GroupTeam> groupTeams);
    Task DeleteAsync(Team team);
}
