using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AdventureGuildAPI.Models
{
    public class ClientUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string EmailAddress { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public int Money { get; set; }
        public string? GuildName { get; set; }
    }
}
