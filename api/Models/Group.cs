namespace api.Models;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AppUserId { get; set; } = string.Empty;
    public AppUser? AppUser { get; set; }
    public List<GroupTeam> GroupTeams { get; set; } = new List<GroupTeam>();
}
