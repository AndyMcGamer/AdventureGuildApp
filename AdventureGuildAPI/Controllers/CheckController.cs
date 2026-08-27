using AdventureGuildAPI.Data;
using AdventureGuildAPI.Extensions;
using AdventureGuildAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdventureGuildAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CheckController : ControllerBase
{
    private readonly DataContext _context;

    public CheckController(DataContext context) => _context = context;

    [Authorize(Policy = "RequireUser")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateCheck(CheckDto check)
    {
        var userId = User.GetUserId();
        var quest = await _context.Quests.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == check.QuestId);
        if (quest == null) return NotFound("Quest not found.");
        if (quest.UserId != userId) return Forbid();
        if (await _context.QuestChecks.AnyAsync(x => x.QuestId == quest.Id)) return BadRequest("A check already exists for this quest.");

        var partyMembers = quest.User.PartyId is int partyId
            ? await _context.Users.Where(x => x.PartyId == partyId).ToListAsync()
            : [quest.User];

        var questCheck = new QuestCheck
        {
            RequestId = userId,
            QuestId = quest.Id,
            PartyId = quest.User.PartyId,
            ImageRef = check.ImageRef
        };
        var approvals = partyMembers.Select(member => new Approval
        {
            ApproverId = member.Id,
            QuestId = quest.Id,
            Approved = member.Id == userId
        });

        _context.QuestChecks.Add(questCheck);
        _context.Approvals.AddRange(approvals);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [Authorize(Policy = "RequireUser")]
    [HttpPost("approve")]
    public async Task<IActionResult> ApproveQuest(int questId)
    {
        var approval = await _context.Approvals.FindAsync(User.GetUserId(), questId);
        if (approval == null) return NotFound("Approval request not found.");

        approval.Approved = true;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [Authorize(Policy = "RequireUser")]
    [HttpGet]
    public async Task<IEnumerable<Approval>> GetApprovals()
    {
        return await _context.Approvals
            .AsNoTracking()
            .Include(x => x.Quest)
            .Where(x => x.ApproverId == User.GetUserId() && !x.Approved && x.Quest.CreatedDateTime >= DateTime.UtcNow.AddDays(-1))
            .ToListAsync();
    }
}
