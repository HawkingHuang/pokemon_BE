using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repository;

public class GroupRepository : IGroupRepository
{
    private readonly ApplicationDBContext _context;

    public GroupRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<List<Group>> GetAllByUserAsync(string appUserId)
    {
        return await _context.Groups
            .Where(g => g.AppUserId == appUserId)
            .OrderBy(g => g.Id)
            .ToListAsync();
    }

    // filtering by owner here means another user's group is simply "not found"
    public async Task<Group?> GetByIdAsync(int id, string appUserId)
    {
        return await _context.Groups.FirstOrDefaultAsync(g => g.Id == id && g.AppUserId == appUserId);
    }

    // true only if every id is one of this user's groups
    public async Task<bool> AllExistAsync(List<int> ids, string appUserId)
    {
        var ownedIds = await _context.Groups
            .Where(g => g.AppUserId == appUserId && ids.Contains(g.Id))
            .Select(g => g.Id)
            .ToListAsync();

        return ids.All(id => ownedIds.Contains(id));
    }

    public async Task<bool> NameExistsAsync(string appUserId, string name, int? excludeGroupId = null)
    {
        return await _context.Groups.AnyAsync(g =>
            g.AppUserId == appUserId && g.Name == name && (excludeGroupId == null || g.Id != excludeGroupId));
    }

    public async Task<Group> CreateAsync(Group group)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task UpdateAsync(Group group, string name)
    {
        group.Name = name;
        await _context.SaveChangesAsync();
    }

    // the group's GroupTeams rows are removed by the database (cascade); the teams stay
    public async Task DeleteAsync(Group group)
    {
        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();
    }
}
