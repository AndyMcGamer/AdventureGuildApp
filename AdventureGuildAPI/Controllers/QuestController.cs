using AdventureGuildAPI.Data;
using AdventureGuildAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace AdventureGuildAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestController : ControllerBase
    {
        private readonly DataContext _context;

        public QuestController(DataContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "RequireUser")]
        [HttpPost("create")]
        [ProducesResponseType(typeof(QuestDto),200), ProducesResponseType(400), ProducesResponseType(401)]
        public async Task<IActionResult> CreateQuest(QuestDto questDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var foundUser = await _context.Users.FindAsync(userId);
            if (foundUser == null) return BadRequest();
            Quest quest = new()
            {
                User = foundUser,
                Name = questDto.Name,
                Description = questDto.Description,
                Priority = questDto.Priority,
                CreatedDateTime = DateTime.UtcNow
            };
            await _context.Quests.AddAsync(quest);
            await _context.SaveChangesAsync();

            questDto.Id = quest.Id;

            return Ok(questDto);
        }

        [Authorize(Policy = "RequireUser")]
        [HttpGet]
        public async Task<IEnumerable<Quest>> GetUserQuests(string time)
        {
            int requestId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var startTime = DateTime.ParseExact(time, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            var endTime = startTime.AddDays(1);
            var filteredData = _context.Quests.AsQueryable();
            filteredData = filteredData.Where(x => x.UserId == requestId).Where(x => x.CreatedDateTime >= startTime && x.CreatedDateTime < endTime);
            return await filteredData.ToListAsync();
        }

        [Authorize(Policy = "RequireUser")]
        [HttpPatch("update")]
        [ProducesResponseType(204), ProducesResponseType(401), ProducesResponseType(400)]
        public async Task<IActionResult> UpdateQuest(QuestDto quest)
        {
            var toBeUpdated = new Quest
            {
                Id = quest.Id,
                Name = quest.Name,
                Description = quest.Description,
                Priority = quest.Priority
            };
            _context.Attach(toBeUpdated);
            _context.Entry(toBeUpdated).Property("Name").IsModified = true;
            _context.Entry(toBeUpdated).Property("Description").IsModified = true;
            _context.Entry(toBeUpdated).Property("Priority").IsModified = true;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Policy = "RequireUser")]
        [HttpDelete("{id}")]
        [ProducesResponseType(204), ProducesResponseType(404), ProducesResponseType(401)]
        public async Task<IActionResult> DeleteQuest(int id)
        {
            var quest = await _context.Quests.FindAsync(id);
            if (quest == null) return NotFound();
            if (quest.UserId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))) return Unauthorized();
            _context.Quests.Remove(quest);
            await _context.SaveChangesAsync();

            return NoContent();
        }


    }
}
