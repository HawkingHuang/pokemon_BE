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

    private readonly ITeamRepository _teamRepo;

    public TeamsController(ITeamRepository teamRepo)
    {
        _teamRepo = teamRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var teams = await _teamRepo.GetAllByUserAsync(User.GetUserId());
        return Ok(teams.Select(t => t.ToTeamDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var team = await _teamRepo.GetByIdAsync(id, User.GetUserId());
        if (team == null) return NotFound();

        return Ok(team.ToTeamDto());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TeamRequestDto teamDto)
    {
        var userId = User.GetUserId();
        var name = teamDto.Name.Trim();
        if (await _teamRepo.NameExistsAsync(userId, name)) return Conflict(DuplicateNameMessage);

        var team = await _teamRepo.CreateAsync(new Team
        {
            Name = name,
            AppUserId = userId,
            Members = teamDto.PokemonIds.ToMembers()
        });

        return CreatedAtAction(nameof(GetById), new { id = team.Id }, team.ToTeamDto());
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] TeamRequestDto teamDto)
    {
        var userId = User.GetUserId();
        var team = await _teamRepo.GetByIdAsync(id, userId);
        if (team == null) return NotFound();

        var name = teamDto.Name.Trim();
        if (await _teamRepo.NameExistsAsync(userId, name, excludeTeamId: id)) return Conflict(DuplicateNameMessage);

        await _teamRepo.UpdateAsync(team, name, teamDto.PokemonIds.ToMembers());
        return Ok(team.ToTeamDto());
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
