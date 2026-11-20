using api.Dtos.Team;
using api.Models;

namespace api.Mappers;

public static class TeamMappers
{
    public static TeamDto ToTeamDto(this Team team) => new()
    {
        Id = team.Id,
        Name = team.Name,
        PokemonIds = team.Members.OrderBy(m => m.Slot).Select(m => m.PokemonId).ToList(),
        CreatedAt = team.CreatedAt,
        UpdatedAt = team.UpdatedAt
    };

    public static List<Member> ToMembers(this List<int> pokemonIds) =>
        pokemonIds.Select((pokemonId, index) => new Member { PokemonId = pokemonId, Slot = index + 1 }).ToList();
}
