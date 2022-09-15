using AdventureGuildAPI.Data;
using AdventureGuildAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AdventureGuildAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckController : ControllerBase
    {
        private readonly DataContext _context;
        public CheckController(DataContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "RequireUser")]
        [HttpPost("create")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<IActionResult> CreateCheck(CheckDto check)
        {
            var quest = await _context.Quests.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == check.QuestId);
            if (quest == null) return BadRequest();
            var reqUser = quest.User;
            var questCheck = new QuestCheck()
            {
                RequestId = reqUser.Id,
                QuestId = quest.Id,
                PartyId = reqUser.PartyId,
                ImageRef = check.ImageRef
            };

            List<Approval> approvals = new();
            List<User> party = new();
            if (reqUser.PartyId != null)
            {
                party = await _context.Users.Where(x => x.PartyId == reqUser.PartyId).ToListAsync();
            }
            else
            {
                party.Add(quest.User);
            }

            foreach (var user in party)
            {
                var approval = new Approval()
                {
                    ApproverId = user.Id,
                    QuestId = quest.Id,
                    Approved = false
                };
                if (user.Id == questCheck.RequestId) approval.Approved = true;
                approvals.Add(approval);
            }

            await _context.QuestChecks.AddAsync(questCheck);
            await _context.Approvals.AddRangeAsync(approvals);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Policy = "RequireUser")]
        [HttpPost("approve")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<IActionResult> ApproveQuest(int questId)
        {
            var approverId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var approval = await _context.Approvals.FindAsync(approverId , questId);
            if (approval == null) return BadRequest();
            approval.Approved = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Policy = "RequireUser")]
        [HttpGet]
        public async Task<IEnumerable<Approval>> GetApprovals()
        {
            var filteredData = _context.Approvals.AsQueryable();
            filteredData = filteredData.Where(x => x.ApproverId == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) && x.Approved == false && x.Quest.CreatedDateTime >= DateTime.UtcNow.AddDays(-1));
            return await filteredData.ToListAsync();
        }
    }
}
