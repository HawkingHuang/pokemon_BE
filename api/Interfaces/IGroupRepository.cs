using api.Models;

namespace api.Interfaces;

public interface IGroupRepository
{
    Task<List<Group>> GetAllByUserAsync(string appUserId);
    Task<Group?> GetByIdAsync(int id, string appUserId);
    Task<bool> AllExistAsync(List<int> ids, string appUserId);
    Task<bool> NameExistsAsync(string appUserId, string name, int? excludeGroupId = null);
    Task<Group> CreateAsync(Group group);
    Task UpdateAsync(Group group, string name);
    Task DeleteAsync(Group group);
}
