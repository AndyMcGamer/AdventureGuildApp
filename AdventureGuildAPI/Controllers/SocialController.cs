using AdventureGuildAPI.Data;
using AdventureGuildAPI.Extensions;
using AdventureGuildAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdventureGuildAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "RequireUser")]
public class SocialController : ControllerBase
{
    private const int MaximumPartySize = 4;
    private readonly DataContext _context;

    public SocialController(DataContext context) => _context = context;

    [HttpGet("friends")]
    public async Task<IEnumerable<string>> GetFriends()
    {
        var userId = User.GetUserId();
        var sent = _context.Friendships.Where(x => x.RequestId == userId && x.Confirmed).Select(x => x.AcceptUser.Username);
        var received = _context.Friendships.Where(x => x.AcceptId == userId && x.Confirmed).Select(x => x.RequestUser.Username);
        return await sent.Union(received).ToListAsync();
    }

    [HttpPost("add-friend")]
    public async Task<IActionResult> RequestFriendship(string username)
    {
        var userId = User.GetUserId();
        var friend = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (friend == null) return NotFound("User not found.");
        if (friend.Id == userId) return BadRequest("You cannot add yourself as a friend.");

        var existing = await _context.Friendships.FirstOrDefaultAsync(x =>
            (x.RequestId == userId && x.AcceptId == friend.Id) ||
            (x.RequestId == friend.Id && x.AcceptId == userId));

        if (existing != null)
        {
            if (existing.Confirmed) return BadRequest("Already friends.");
            if (existing.AcceptId != userId) return BadRequest("Request sent already.");

            existing.Confirmed = true;
            await _context.SaveChangesAsync();
            return Ok();
        }

        _context.Friendships.Add(new Friendship { RequestId = userId, AcceptId = friend.Id });
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("accept-friend")]
    public async Task<IActionResult> AcceptFriendship(string username)
    {
        var userId = User.GetUserId();
        var friend = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (friend == null) return NotFound("User not found.");

        var request = await _context.Friendships.FindAsync(friend.Id, userId);
        if (request == null || request.Confirmed) return NotFound("Friend request not found.");

        request.Confirmed = true;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("delete-friend")]
    public async Task<IActionResult> DeleteFriendship(string username)
    {
        var userId = User.GetUserId();
        var friend = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (friend == null) return NotFound("User not found.");

        var friendships = await _context.Friendships.Where(x =>
            (x.RequestId == userId && x.AcceptId == friend.Id) ||
            (x.RequestId == friend.Id && x.AcceptId == userId)).ToListAsync();
        _context.Friendships.RemoveRange(friendships);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("friend-requests")]
    public Task<List<string>> GetFriendRequests() => _context.Friendships
        .Where(x => x.AcceptId == User.GetUserId() && !x.Confirmed)
        .Select(x => x.RequestUser.Username)
        .ToListAsync();

    [HttpPost("create-guild")]
    public async Task<IActionResult> CreateGuild(GuildDto guildRequest)
    {
        var userId = User.GetUserId();
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound("User not found.");
        if (user.GuildId != null) return BadRequest("Leave your current guild before creating another.");
        if (await _context.Guilds.AnyAsync(x => x.Name == guildRequest.Name)) return Conflict("A guild with that name already exists.");

        user.Guild = new Guild
        {
            Name = guildRequest.Name,
            Description = guildRequest.Description,
            IsPrivate = guildRequest.IsPrivate,
            LeaderId = userId
        };
        _context.GuildRequests.RemoveRange(await _context.GuildRequests.Where(x => x.RequestId == userId).ToListAsync());
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("guild")]
    public async Task<IActionResult> GetCurrentGuild()
    {
        var guild = await _context.Users.Where(x => x.Id == User.GetUserId()).Select(x => x.Guild).FirstOrDefaultAsync();
        if (guild == null) return NoContent();

        return Ok(new Guild
        {
            Id = guild.Id,
            Name = guild.Name,
            Description = guild.Description,
            IsPrivate = guild.IsPrivate,
            LeaderId = guild.LeaderId
        });
    }

    [HttpGet("guild/{guildId}/members")]
    public async Task<IActionResult> GetGuildMembers(int guildId)
    {
        if (!await _context.Guilds.AnyAsync(x => x.Id == guildId)) return NotFound("Guild not found.");
        return Ok(await _context.Users.Where(x => x.GuildId == guildId).Select(x => x.Username).ToListAsync());
    }

    [HttpPost("guild/request")]
    public async Task<IActionResult> RequestGuild(string guildName)
    {
        var userId = User.GetUserId();
        var user = await _context.Users.FindAsync(userId);
        var guild = await _context.Guilds.FirstOrDefaultAsync(x => x.Name == guildName);
        if (guild == null) return NotFound("Guild not found.");
        if (user == null) return NotFound("User not found.");
        if (user.GuildId != null) return BadRequest("Leave your current guild before joining another.");

        if (guild.IsPrivate)
        {
            if (await _context.GuildRequests.AnyAsync(x => x.RequestId == userId && x.GuildId == guild.Id))
            {
                return BadRequest("Guild request already sent.");
            }

            _context.GuildRequests.Add(new GuildRequest { RequestId = userId, GuildId = guild.Id });
        }
        else
        {
            user.GuildId = guild.Id;
            _context.GuildRequests.RemoveRange(await _context.GuildRequests.Where(x => x.RequestId == userId).ToListAsync());
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("guild/{guildId}/get-requests")]
    public async Task<IActionResult> GetGuildRequests(int guildId)
    {
        var guild = await _context.Guilds.FindAsync(guildId);
        if (guild == null) return NotFound("Guild not found.");
        if (guild.LeaderId != User.GetUserId()) return Forbid();

        return Ok(await _context.GuildRequests.Where(x => x.GuildId == guildId).Select(x => x.User.Username).ToListAsync());
    }

    [HttpPost("guild/{guildId}/accept")]
    public async Task<IActionResult> AcceptGuildRequest(int guildId, string username)
    {
        var guild = await _context.Guilds.FindAsync(guildId);
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (guild == null || user == null) return NotFound();
        if (guild.LeaderId != User.GetUserId()) return Forbid();
        if (user.GuildId != null) return BadRequest("User already belongs to a guild.");

        var request = await _context.GuildRequests.FindAsync(user.Id, guildId);
        if (request == null) return NotFound("Guild request not found.");

        user.GuildId = guildId;
        _context.GuildRequests.RemoveRange(await _context.GuildRequests.Where(x => x.RequestId == user.Id).ToListAsync());
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("guild/{guildId}/reject")]
    public async Task<IActionResult> RejectGuildRequest(int guildId, string username)
    {
        var guild = await _context.Guilds.FindAsync(guildId);
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (guild == null || user == null) return NotFound();
        if (guild.LeaderId != User.GetUserId()) return Forbid();

        var request = await _context.GuildRequests.FindAsync(user.Id, guildId);
        if (request == null) return NotFound("Guild request not found.");

        _context.GuildRequests.Remove(request);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("guild/{guildId}/change-leader")]
    public async Task<IActionResult> ChangeGuildLeader(int guildId, string username)
    {
        var guild = await _context.Guilds.FindAsync(guildId);
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (guild == null || user == null) return NotFound();
        if (guild.LeaderId != User.GetUserId()) return Forbid();
        if (user.GuildId != guildId) return BadRequest("The new leader must be a guild member.");

        guild.LeaderId = user.Id;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("leave-guild")]
    public async Task<IActionResult> LeaveGuild()
    {
        var userId = User.GetUserId();
        var user = await _context.Users.Include(x => x.Guild).FirstOrDefaultAsync(x => x.Id == userId);
        if (user?.Guild == null) return BadRequest("Not in a guild.");
        if (user.Guild.LeaderId == userId) return BadRequest("Transfer leadership or disband the guild before leaving.");

        user.GuildId = null;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("guild/disband")]
    public async Task<IActionResult> DisbandGuild()
    {
        var userId = User.GetUserId();
        var user = await _context.Users.Include(x => x.Guild).FirstOrDefaultAsync(x => x.Id == userId);
        if (user?.Guild == null || user.Guild.LeaderId != userId) return BadRequest("Only the guild leader can disband it.");

        _context.Guilds.Remove(user.Guild);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("guild/{guildId}/set-privacy")]
    public async Task<IActionResult> SetPrivacy(int guildId, bool privacy)
    {
        var guild = await _context.Guilds.FindAsync(guildId);
        if (guild == null) return NotFound();
        if (guild.LeaderId != User.GetUserId()) return Forbid();

        guild.IsPrivate = privacy;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("party")]
    public async Task<IActionResult> GetCurrentParty()
    {
        var party = await _context.Users.Where(x => x.Id == User.GetUserId()).Select(x => x.Party).FirstOrDefaultAsync();
        if (party == null) return NoContent();
        return Ok(new Party { Id = party.Id, Name = party.Name ?? "Unnamed Party" });
    }

    [HttpGet("party/{partyId}/members")]
    public async Task<IActionResult> GetPartyMembers(int partyId)
    {
        if (!await _context.Parties.AnyAsync(x => x.Id == partyId)) return NotFound("Party not found.");
        return Ok(await _context.Users.Where(x => x.PartyId == partyId).Select(x => x.Username).ToListAsync());
    }

    [HttpPost("create-party")]
    public async Task<IActionResult> CreateParty(string? name)
    {
        var userId = User.GetUserId();
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound("User not found.");
        if (user.PartyId != null) return BadRequest("Leave your current party before creating another.");

        user.Party = new Party { Name = name };
        _context.PartyInvites.RemoveRange(await _context.PartyInvites.Where(x => x.AcceptId == userId).ToListAsync());
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("party/invite")]
    public async Task<IActionResult> InvitePartyMember(string username)
    {
        var inviterId = User.GetUserId();
        var inviter = await _context.Users.FindAsync(inviterId);
        var invitee = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (inviter?.PartyId == null) return BadRequest("You must belong to a party to send invites.");
        if (invitee == null) return NotFound("User not found.");
        if (invitee.Id == inviterId || invitee.PartyId != null) return BadRequest("User cannot be invited to this party.");

        var partyId = inviter.PartyId.Value;
        if (await _context.Users.CountAsync(x => x.PartyId == partyId) >= MaximumPartySize) return BadRequest("Full party.");
        if (await _context.PartyInvites.FindAsync(partyId, invitee.Id) != null) return BadRequest("Party invite already sent.");

        _context.PartyInvites.Add(new PartyInvite { PartyId = partyId, InviterId = inviterId, AcceptId = invitee.Id });
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("party/get-invites")]
    public Task<List<string>> GetPartyInvites() => _context.PartyInvites
        .Where(x => x.AcceptId == User.GetUserId())
        .Select(x => x.Inviter.Username)
        .ToListAsync();

    [HttpPost("party/accept-invite")]
    public async Task<IActionResult> AcceptInvite(string username)
    {
        var userId = User.GetUserId();
        var user = await _context.Users.FindAsync(userId);
        var inviter = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (user == null || inviter == null) return NotFound();
        if (user.PartyId != null) return BadRequest("Leave your current party before accepting an invite.");

        var invite = await _context.PartyInvites.FirstOrDefaultAsync(x => x.AcceptId == userId && x.InviterId == inviter.Id);
        if (invite == null) return NotFound("Party invite not found.");
        if (inviter.PartyId != invite.PartyId || await _context.Users.CountAsync(x => x.PartyId == invite.PartyId) >= MaximumPartySize)
        {
            _context.PartyInvites.Remove(invite);
            await _context.SaveChangesAsync();
            return BadRequest("Party invite is no longer valid.");
        }

        user.PartyId = invite.PartyId;
        _context.PartyInvites.RemoveRange(await _context.PartyInvites.Where(x => x.AcceptId == userId).ToListAsync());
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("party/reject-invite")]
    public async Task<IActionResult> RejectPartyInvite(string username)
    {
        var userId = User.GetUserId();
        var inviter = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (inviter == null) return NotFound("User not found.");

        var invite = await _context.PartyInvites.FirstOrDefaultAsync(x => x.AcceptId == userId && x.InviterId == inviter.Id);
        if (invite == null) return NotFound("Party invite not found.");

        _context.PartyInvites.Remove(invite);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("party/leave")]
    public async Task<IActionResult> LeaveParty()
    {
        var userId = User.GetUserId();
        var user = await _context.Users.FindAsync(userId);
        if (user?.PartyId is not int partyId) return BadRequest("Not in a party.");

        user.PartyId = null;
        await _context.SaveChangesAsync();

        if (!await _context.Users.AnyAsync(x => x.PartyId == partyId))
        {
            var party = await _context.Parties.FindAsync(partyId);
            if (party != null) _context.Parties.Remove(party);
        }
        else
        {
            _context.PartyInvites.RemoveRange(await _context.PartyInvites
                .Where(x => x.PartyId == partyId && x.InviterId == userId)
                .ToListAsync());
        }

        await _context.SaveChangesAsync();
        return Ok();
    }
}
