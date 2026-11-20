namespace api.Models;

// one row means one team is in one group
public class GroupTeam
{
    public int GroupId { get; set; }
    public Group? Group { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
}
