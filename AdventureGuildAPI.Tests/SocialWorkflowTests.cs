using AdventureGuildAPI.Controllers;
using AdventureGuildAPI.Data;
using AdventureGuildAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Xunit;

namespace AdventureGuildAPI.Tests;

public class SocialWorkflowTests
{
    [Fact]
    public void SocialRelationshipTablesUseCompositeKeys()
    {
        using var context = CreateContext();

        Assert.Equal(["RequestId", "AcceptId"], GetKeyProperties<Friendship>(context));
        Assert.Equal(["RequestId", "GuildId"], GetKeyProperties<GuildRequest>(context));
        Assert.Equal(["PartyId", "AcceptId"], GetKeyProperties<PartyInvite>(context));
    }

    [Fact]
    public async Task CrossedFriendRequestConfirmsTheExistingRequest()
    {
        using var context = CreateContext();
        context.Users.AddRange(CreateUser(1, "alpha"), CreateUser(2, "bravo"));
        await context.SaveChangesAsync();

        var firstController = CreateController(context, 1);
        Assert.IsType<OkResult>(await firstController.RequestFriendship("bravo"));

        context.ChangeTracker.Clear();
        var secondController = CreateController(context, 2);
        Assert.IsType<OkResult>(await secondController.RequestFriendship("alpha"));

        var friendship = await context.Friendships.SingleAsync();
        Assert.Equal(1, friendship.RequestId);
        Assert.Equal(2, friendship.AcceptId);
        Assert.True(friendship.Confirmed);
    }

    private static DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DataContext(options);
    }

    private static IReadOnlyList<string> GetKeyProperties<TEntity>(DataContext context) where TEntity : class
    {
        return context.Model.FindEntityType(typeof(TEntity))!.FindPrimaryKey()!.Properties.Select(x => x.Name).ToList();
    }

    private static SocialController CreateController(DataContext context, int userId)
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Test");
        return new SocialController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            }
        };
    }

    private static User CreateUser(int id, string username)
    {
        return new User
        {
            Id = id,
            Username = username,
            EmailAddress = $"{username}@example.test",
            Password = [1],
            PasswordSalt = [1],
            FirstName = username,
            LastName = "User",
            Role = "User",
            VerificationToken = [(byte)id],
            Verified = true
        };
    }
}
