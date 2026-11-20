using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Team;

public class TeamRequestDto : IValidatableObject
{
    public const int MaxPokemonId = 1025;

    [Required]
    [MaxLength(30)]
    public string Name { get; set; } = string.Empty;

    // ordered list: the index becomes the slot (1-6)
    [Required]
    [Length(1, 6)]
    public List<int> PokemonIds { get; set; } = new List<int>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PokemonIds.Any(id => id < 1 || id > MaxPokemonId))
        {
            yield return new ValidationResult(
                $"Each Pokémon id must be between 1 and {MaxPokemonId}.",
                new[] { nameof(PokemonIds) });
        }
    }
}
