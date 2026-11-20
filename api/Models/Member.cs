namespace api.Models;

public class Member
{
    public int Id { get; set; }
    public int PokemonId { get; set; }
    public int Slot { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
}
