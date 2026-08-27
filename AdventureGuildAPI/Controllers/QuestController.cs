using AdventureGuildAPI.Data;
using AdventureGuildAPI.Extensions;
using AdventureGuildAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AdventureGuildAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QuestController : ControllerBase
{
    private readonly DataContext _context;

    public QuestController(DataContext context) => _context = context;

    [Authorize(Policy = "RequireUser")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateQuest(QuestDto questDto)
    {
        var user = await _context.Users.FindAsync(User.GetUserId());
        if (user == null) return NotFound("User not found.");

        var quest = new Quest
        {
            User = user,
            Name = questDto.Name,
            Description = questDto.Description,
            Priority = questDto.Priority,
            CreatedDateTime = DateTime.UtcNow
        };
        _context.Quests.Add(quest);
        await _context.SaveChangesAsync();

        questDto.Id = quest.Id;
        return Ok(questDto);
    }

    [Authorize(Policy = "RequireUser")]
    [HttpGet]
    public async Task<IActionResult> GetUserQuests([FromQuery] string? time)
    {
        if (!DateTimeOffset.TryParse(time, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var requestedTime))
        {
            return BadRequest("A valid ISO 8601 time is required.");
        }

        var startTime = requestedTime.UtcDateTime.Date;
        var endTime = startTime.AddDays(1);
        var quests = await _context.Quests
            .AsNoTracking()
            .Where(x => x.UserId == User.GetUserId() && x.CreatedDateTime >= startTime && x.CreatedDateTime < endTime)
            .ToListAsync();
        return Ok(quests);
    }

    [Authorize(Policy = "RequireUser")]
    [HttpPatch("update")]
    public async Task<IActionResult> UpdateQuest(QuestDto questDto)
    {
        var quest = await _context.Quests.FindAsync(questDto.Id);
        if (quest == null) return NotFound();
        if (quest.UserId != User.GetUserId()) return Forbid();

        quest.Name = questDto.Name;
        quest.Description = questDto.Description;
        quest.Priority = questDto.Priority;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Policy = "RequireUser")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuest(int id)
    {
        var quest = await _context.Quests.FindAsync(id);
        if (quest == null) return NotFound();
        if (quest.UserId != User.GetUserId()) return Forbid();

        _context.Quests.Remove(quest);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
