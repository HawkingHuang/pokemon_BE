using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Group;

public class GroupRequestDto
{
    [Required]
    [MaxLength(30)]
    public string Name { get; set; } = string.Empty;
}
