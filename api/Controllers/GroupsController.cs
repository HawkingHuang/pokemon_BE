using api.Dtos.Group;
using api.Extensions;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[Route("api/groups")]
[ApiController]
[Authorize]
public class GroupsController : ControllerBase
{
    private const string DuplicateNameMessage = "You already have a group with this name.";
    private const string GroupNotFoundMessage = "Group not found.";
    private const string TeamNotFoundMessage = "Team not found.";

    private readonly IGroupRepository _groupRepo;
    private readonly ITeamRepository _teamRepo;
    private readonly IGroupTeamRepository _groupTeamRepo;

    public GroupsController(IGroupRepository groupRepo, ITeamRepository teamRepo, IGroupTeamRepository groupTeamRepo)
    {
        _groupRepo = groupRepo;
        _teamRepo = teamRepo;
        _groupTeamRepo = groupTeamRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var groups = await _groupRepo.GetAllByUserAsync(User.GetUserId());
        var groupDtos = groups.Select(g => g.ToGroupDto()).ToList();
        return Ok(groupDtos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var group = await _groupRepo.GetByIdAsync(id, User.GetUserId());
        if (group == null) return NotFound();

        var groupDto = group.ToGroupDto();
        return Ok(groupDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GroupRequestDto request)
    {
        var userId = User.GetUserId();
        var name = request.Name.Trim();
        if (await _groupRepo.NameExistsAsync(userId, name)) return Conflict(DuplicateNameMessage);

        var group = await _groupRepo.CreateAsync(new Group { Name = name, AppUserId = userId });

        var groupDto = group.ToGroupDto();
        return CreatedAtAction(nameof(GetById), new { id = group.Id }, groupDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] GroupRequestDto request)
    {
        var userId = User.GetUserId();
        var group = await _groupRepo.GetByIdAsync(id, userId);
        if (group == null) return NotFound();

        var name = request.Name.Trim();
        if (await _groupRepo.NameExistsAsync(userId, name, excludeGroupId: id)) return Conflict(DuplicateNameMessage);

        await _groupRepo.UpdateAsync(group, name);

        var groupDto = group.ToGroupDto();
        return Ok(groupDto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var group = await _groupRepo.GetByIdAsync(id, User.GetUserId());
        if (group == null) return NotFound();

        await _groupRepo.DeleteAsync(group);
        return NoContent();
    }

    [HttpGet("{groupId:int}/teams")]
    public async Task<IActionResult> GetTeams([FromRoute] int groupId)
    {
        var userId = User.GetUserId();
        var group = await _groupRepo.GetByIdAsync(groupId, userId);
        if (group == null) return NotFound();

        var teams = await _teamRepo.GetAllByGroupAsync(groupId, userId);
        var teamDtos = teams.Select(t => t.ToTeamDto()).ToList();
        return Ok(teamDtos);
    }

    [HttpPost("{groupId:int}/teams/{teamId:int}")]
    public async Task<IActionResult> AddTeam([FromRoute] int groupId, [FromRoute] int teamId)
    {
        var userId = User.GetUserId();
        var group = await _groupRepo.GetByIdAsync(groupId, userId);
        if (group == null) return NotFound(GroupNotFoundMessage);

        var team = await _teamRepo.GetByIdAsync(teamId, userId);
        if (team == null) return NotFound(TeamNotFoundMessage);

        if (await _groupTeamRepo.ExistsAsync(groupId, teamId)) return Conflict("This team is already in the group.");

        await _groupTeamRepo.CreateAsync(new GroupTeam { GroupId = groupId, TeamId = teamId });
        return Created();
    }

    [HttpDelete("{groupId:int}/teams/{teamId:int}")]
    public async Task<IActionResult> RemoveTeam([FromRoute] int groupId, [FromRoute] int teamId)
    {
        var group = await _groupRepo.GetByIdAsync(groupId, User.GetUserId());
        if (group == null) return NotFound(GroupNotFoundMessage);

        // only the user's own teams can be in their group, so finding the link is enough
        var groupTeam = await _groupTeamRepo.DeleteAsync(groupId, teamId);
        if (groupTeam == null) return NotFound("This team is not in the group.");

        return NoContent();
    }
}
