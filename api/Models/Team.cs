namespace api.Models;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AppUserId { get; set; } = string.Empty;
    public AppUser? AppUser { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<Member> Members { get; set; } = new List<Member>();
}
