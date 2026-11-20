using api.Dtos.Group;
using api.Models;

namespace api.Mappers;

public static class GroupMappers
{
    public static GroupDto ToGroupDto(this Group group) => new()
    {
        Id = group.Id,
        Name = group.Name
    };
}
