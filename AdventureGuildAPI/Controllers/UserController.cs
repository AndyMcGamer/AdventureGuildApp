using AdventureGuildAPI.Data;
using AdventureGuildAPI.Extensions;
using AdventureGuildAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdventureGuildAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly DataContext _context;

    public UserController(DataContext context) => _context = context;

    [Authorize(Policy = "RequireUser")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        if (User.GetUserId() != id) return Forbid();

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound("No user found.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize("RequireUser")]
    [HttpGet("current-user")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _context.Users
            .Include(x => x.Guild)
            .FirstOrDefaultAsync(x => x.Id == User.GetUserId());
        if (user == null) return NotFound("No user found.");

        return Ok(new ClientUser
        {
            Id = user.Id,
            Username = user.Username,
            EmailAddress = user.EmailAddress,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Money = user.Money,
            GuildName = user.Guild?.Name
        });
    }
}
