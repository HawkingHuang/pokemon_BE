namespace api.Dtos.Team;

public class TeamDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<int> PokemonIds { get; set; } = new List<int>();
    public List<int> GroupIds { get; set; } = new List<int>();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
