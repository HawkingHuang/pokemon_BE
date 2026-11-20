using api.Dtos.Team;
using api.Extensions;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[Route("api/teams")]
[ApiController]
[Authorize]
public class TeamsController : ControllerBase
{
    private const string DuplicateNameMessage = "You already have a team with this name.";
    private const string GroupsNotFoundMessage = "One or more groups were not found.";

    private readonly ITeamRepository _teamRepo;
    private readonly IGroupRepository _groupRepo;

    public TeamsController(ITeamRepository teamRepo, IGroupRepository groupRepo)
    {
        _teamRepo = teamRepo;
        _groupRepo = groupRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var teams = await _teamRepo.GetAllByUserAsync(User.GetUserId());
        var teamDtos = teams.Select(t => t.ToTeamDto()).ToList();
        return Ok(teamDtos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var team = await _teamRepo.GetByIdAsync(id, User.GetUserId());
        if (team == null) return NotFound();

        var teamDto = team.ToTeamDto();
        return Ok(teamDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TeamRequestDto request)
    {
        var userId = User.GetUserId();
        var name = request.Name.Trim();
        if (await _teamRepo.NameExistsAsync(userId, name)) return Conflict(DuplicateNameMessage);

        var groupIds = request.GroupIds.Distinct().ToList();
        if (!await _groupRepo.AllExistAsync(groupIds, userId)) return NotFound(GroupsNotFoundMessage);

        var team = await _teamRepo.CreateAsync(new Team
        {
            Name = name,
            AppUserId = userId,
            Members = request.PokemonIds.ToMembers(),
            GroupTeams = groupIds.ToGroupTeams()
        });

        var teamDto = team.ToTeamDto();
        return CreatedAtAction(nameof(GetById), new { id = team.Id }, teamDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] TeamRequestDto request)
    {
        var userId = User.GetUserId();
        var team = await _teamRepo.GetByIdAsync(id, userId);
        if (team == null) return NotFound();

        var name = request.Name.Trim();
        if (await _teamRepo.NameExistsAsync(userId, name, excludeTeamId: id)) return Conflict(DuplicateNameMessage);

        var groupIds = request.GroupIds.Distinct().ToList();
        if (!await _groupRepo.AllExistAsync(groupIds, userId)) return NotFound(GroupsNotFoundMessage);

        await _teamRepo.UpdateAsync(team, name, request.PokemonIds.ToMembers(), groupIds.ToGroupTeams());

        var teamDto = team.ToTeamDto();
        return Ok(teamDto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var team = await _teamRepo.GetByIdAsync(id, User.GetUserId());
        if (team == null) return NotFound();

        await _teamRepo.DeleteAsync(team);
        return NoContent();
    }
}
