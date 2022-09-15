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
    [Authorize(Policy = "RequireUser")]
    public class SocialController : ControllerBase
    {
        private readonly DataContext _context;
        public SocialController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("friends")]
        public async Task<IEnumerable<string>> GetFriends()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var query1 = from p in _context.Users join f in _context.Friendships on p.Id equals f.RequestId where f.AcceptId == userId && f.Confirmed == true select p.Username;
            var query2 = from p in _context.Users join f in _context.Friendships on p.Id equals f.AcceptId where f.RequestId == userId && f.Confirmed == true select p.Username;
            var friendList = await query1.Union(query2).ToListAsync();
            return friendList;
        }

        [HttpPost("add-friend")]
        [ProducesResponseType(200), ProducesResponseType(404), ProducesResponseType(400)]
        public async Task<IActionResult> RequestFriendship(string username)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (username == User.FindFirstValue(ClaimTypes.Name)) return BadRequest();
            var friend = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
            if (friend == null) return NotFound();
            var query = from f in _context.Friendships where (f.RequestId == userId && f.AcceptId == friend.Id) || (f.RequestId == userId && f.AcceptId == friend.Id) select f;
            var existing = query.Any() ? await query.FirstAsync() : null;
            if(existing != null)
            {
                if (existing.RequestId == userId) return BadRequest("Request sent already");
                if (existing.Confirmed == true) return BadRequest("Already friends");
                await _context.Database.ExecuteSqlInterpolatedAsync($@"UPDATE Friendships SET Confirmed = TRUE WHERE AcceptId = {userId} AND RequestId = {friend.Id}");
            }
            await _context.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO Friendships (RequestId, AcceptId, Confirmed) VALUES ({userId}, {friend.Id}, FALSE)");
            return Ok();
        }

        [HttpPost("accept-friend")]
        [ProducesResponseType(200), ProducesResponseType(404)]
        public async Task<IActionResult> AcceptFriendship(string username)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var friend = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
            if (friend == null) return NotFound();

            await _context.Database.ExecuteSqlInterpolatedAsync($@"UPDATE Friendships SET Confirmed = TRUE WHERE AcceptId = {userId} AND RequestId = {friend.Id}");

            return Ok();

        }

        [HttpPost("delete-friend")]
        [ProducesResponseType(200), ProducesResponseType(404)]
        public async Task<IActionResult> DeleteFriendship(string username)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var friend = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
            if (friend == null) return NotFound();

            await _context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM Friendships WHERE (AcceptId = {userId} AND RequestId = {friend.Id}) OR (AcceptId = {friend.Id} AND RequestId = {userId})");

            return Ok();

        }

        [HttpGet("friend-requests")]
        public async Task<IEnumerable<string>> GetFriendRequests()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var query = from u in _context.Users join f in _context.Friendships on u.Id equals f.AcceptId where (u.Id == userId && f.Confirmed == false) select u.Username;
            if(!query.Any()) return Enumerable.Empty<string>();
            var requestNames = await query.ToListAsync();
            return requestNames;
        }

        [HttpPost("create-guild")]
        [ProducesResponseType(200), ProducesResponseType(409)]
        public async Task<IActionResult> CreateGuild(GuildDto guildReq)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return BadRequest();
            if (user.GuildId != null) return BadRequest();
            var foundGuild = await _context.Guilds.FirstOrDefaultAsync();
            if (foundGuild != null) return StatusCode(409);
            var guild = new Guild()
            {
                Name = guildReq.Name,
                Description = guildReq.Description,
                IsPrivate = guildReq.IsPrivate,
                LeaderId = userId
            };

            await _context.Guilds.AddAsync(guild);
            await _context.SaveChangesAsync();

            user.GuildId = guild.Id;
            await _context.SaveChangesAsync();

            await _context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM GuildRequests g WHERE g.RequestId = {userId}");

            return Ok();
        }

        [HttpGet("guild")]
        [ProducesResponseType(204), ProducesResponseType(typeof(Guild), 200)]
        public async Task<IActionResult> GetCurrentGuild()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var query = from g in _context.Guilds join u in _context.Users on g.Id equals u.GuildId where u.Id == userId select g;
            if (!query.Any()) return NoContent();
            var result = await query.FirstAsync();
            var guild = new Guild()
            {
                Id = result.Id,
                Name = result.Name,
                Description = result.Description,
                IsPrivate = result.IsPrivate,
                LeaderId = result.LeaderId
            };
            return Ok(guild);
        }

        [HttpGet("guild/{guildId}/members")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200), ProducesResponseType(404)]
        public async Task<IActionResult> GetGuildMembers(int guildId)
        {
            var query = from u in _context.Users join g in _context.Guilds on u.GuildId equals g.Id where u.GuildId == guildId select u.Username;
            if (!query.Any()) return NotFound("Guild not found");
            var members = await query.ToListAsync();
            return Ok(members);
        }

        [HttpPost("guild/request")]
        [ProducesResponseType(200), ProducesResponseType(404)]
        public async Task<IActionResult> RequestGuild(string guildName)
        {
            var foundGuild = await _context.Guilds.AsNoTracking().FirstOrDefaultAsync(x => x.Name == guildName);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (foundGuild == null) return NotFound();
            if (foundGuild.IsPrivate)
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO GuildRequests (RequestId, GuildId) VALUES ({userId}, {foundGuild.Id})");
            }
            else
            {
                User user = new()
                {
                    Id = userId,
                    GuildId = foundGuild.Id
                };
                _context.Entry(user).Property("GuildId").IsModified = true;
                await _context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM GuildRequests gr WHERE gr.RequestId = {userId}");
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpGet("guild/{guildId}/get-requests")]
        [ProducesResponseType(204), ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public async Task<IEnumerable<string>> GetGuildRequests(int guildId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var query = from u in _context.Users join gr in _context.GuildRequests on u.Id equals gr.RequestId where gr.GuildId == guildId select u.Username;
            if(!query.Any()) return Enumerable.Empty<string>();
            var requests = await query.ToListAsync();
            return requests;
        }

        [HttpPost("guild/{guildId}/accept")]
        [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(404)]
        public async Task<IActionResult> AcceptGuildRequest(int guildId, string username)
        {
            var approverId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if(user == null) return NotFound();
            var guild = await _context.Guilds.AsNoTracking().FirstOrDefaultAsync(g => g.Id == guildId);
            if (guild == null) return NotFound();
            if (guild.LeaderId != approverId) return BadRequest();
            user.GuildId = guildId;
            await _context.SaveChangesAsync();
            await _context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM GuildRequests g WHERE g.RequestId = {user.Id}");
            return Ok();
        }

        [HttpPost("guild/{guildId}/reject")]
        [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(404)]
        public async Task<IActionResult> RejectGuildRequest(int guildId, string username)
        {
            var approverId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound();
            var guild = await _context.Guilds.AsNoTracking().FirstOrDefaultAsync(g => g.Id == guildId);
            if (guild == null) return NotFound();
            if (guild.LeaderId != approverId) return BadRequest();
            await _context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM GuildRequests gr WHERE gr.RequestId = {user.Id} AND gr.GuildId = {guild.Id}");
            return Ok();
        }

        [HttpPost("guild/{guildId}/change-leader")]
        [ProducesResponseType(200), ProducesResponseType(404)]
        public async Task<IActionResult> ChangeGuildLeader(int guildId, string username)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);
            var guild = await _context.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
            if(guild == null || user == null) return NotFound();
            guild.LeaderId = user.Id;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("leave-guild")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<IActionResult> LeaveGuild()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var query = _context.Users.Include(x => x.Guild).Where(x => x.Id == userId && x.GuildId != null);
            if(!query.Any()) return BadRequest();
            var user = await query.FirstAsync();
            if (user.Guild?.LeaderId == userId) return BadRequest();
            user.GuildId = null;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("guild/disband")]
        [ProducesResponseType(204), ProducesResponseType(400)]
        public async Task<IActionResult> DisbandGuild()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.AsNoTracking().Include(x => x.Guild).FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null || user.Guild?.LeaderId != user.Id) return BadRequest();
            var guildId = user.GuildId;
            
            //await _context.Database.ExecuteSqlInterpolatedAsync($@"UPDATE Users U SET GuildId = NULL WHERE U.GuildId = {guildId}");

            await _context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM Guilds WHERE Guilds.Id = {guildId}");

            return NoContent();
        }

        [HttpPost("guild/{guildId}/set-privacy")]
        [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(404)]
        public async Task<IActionResult> SetPrivacy(int guildId, bool privacy)
        {
            var guild = await _context.Guilds.FindAsync(guildId);
            if (guild == null) return NotFound();
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if(guild.LeaderId != userId) return BadRequest();
            guild.IsPrivate = privacy;
            await _context.SaveChangesAsync();
            return Ok();
        }
        

        [HttpGet("party")]
        [ProducesResponseType(typeof(Party), 200), ProducesResponseType(204)]
        public async Task<IActionResult> GetCurrentParty()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var query = from p in _context.Parties join u in _context.Users on p.Id equals u.PartyId where u.Id == userId select p;
            if (!query.Any()) return NoContent();
            var result = await query.FirstAsync();
            Party party = new()
            {
                Id = result.Id,
                Name = result.Name ?? "Unnamed Party"
            };
            return Ok(party);
        }

        [HttpGet("party/{partyId}/members")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200), ProducesResponseType(404)]
        public async Task<IActionResult> GetPartyMembers(int partyId)
        {
            var query = from u in _context.Users join p in _context.Parties on u.PartyId equals p.Id where u.PartyId == partyId select u.Username;
            if (!query.Any()) return NotFound("Party not found");
            var members = await query.ToListAsync();
            return Ok(members);
        }

        [HttpPost("create-party")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<IActionResult> CreateParty(string? name)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return BadRequest();
            if (user.PartyId != null) return BadRequest();
            var party = new Party()
            {
                Name = name
            };
            await _context.Parties.AddAsync(party);
            await _context.SaveChangesAsync();

            user.PartyId = party.Id;
            await _context.SaveChangesAsync();

            await _context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM PartyInvites p WHERE p.AcceptId = {userId}");

            return Ok();
        }

        [HttpPost("party/invite")]
        [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(404)]
        public async Task<IActionResult> InvitePartyMember(string username)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var inviter = await _context.Users.FindAsync(userId);
            if (inviter == null || inviter.PartyId == null) return BadRequest();
            var invitee = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
            if (invitee == null) return NotFound();
            if (inviter.Id == invitee.Id) return BadRequest();
            var members = await _context.Users.Where(x => x.PartyId == inviter.PartyId).ToListAsync();
            var memberCount = members.Count;
            if (memberCount == 4) return BadRequest("Full party");

            await _context.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO PartyInvites (PartyId, InviteName, AcceptId) VALUES ({inviter.PartyId}, {inviter.Username}, {invitee.Id})");

            return Ok();
        }

        [HttpGet("party/get-invites")]
        public async Task<IEnumerable<string>> GetPartyInvites()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var query = from pi in _context.PartyInvites where pi.AcceptId == userId select pi.InviteName;
            if (!query.Any()) return Enumerable.Empty<string>();
            var invites = await query.ToListAsync();
            return invites;
        }

        [HttpPost("party/accept-invite")]
        [ProducesResponseType(200), ProducesResponseType(404), ProducesResponseType(400)]
        public async Task<IActionResult> AcceptInvite(string username)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);
            var inviteUser = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
            if (inviteUser == null) return NotFound();
            if (user == null) return NotFound();
            if(user.PartyId != null) return BadRequest();
            user.PartyId = inviteUser.PartyId;
            await _context.SaveChangesAsync();

            await _context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM PartyInvites p WHERE p.AcceptId = {userId}");

            return Ok();
        }

        [HttpPost("party/reject-invite")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RejectPartyInvite(string username)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));        
            await _context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM PartyInvites P WHERE P.AcceptId = {userId} AND P.InviteName = {username}");
            return Ok();
        }

        [HttpPost("party/leave")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<IActionResult> LeaveParty()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return BadRequest("No user found");
            var partyId = user.PartyId;
            if (partyId == null) return BadRequest("Not in party");
            user.PartyId = null;
            await _context.SaveChangesAsync();
            var query = from u in _context.Users where u.PartyId == partyId select u;
            if (!query.Any())
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM Parties P WHERE P.Id = {partyId}");
                await _context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM PartyInvites P WHERE P.PartyId = {partyId}");
            }
            else
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM PartyInvites P WHERE P.PartyId = {partyId} AND P.InviteName = {user.Username}");
            }
            return Ok();
        }
    }
}
