using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureGuildAPI.Models
{
    [Index(nameof(Username), IsUnique = true), Index(nameof(EmailAddress), IsUnique = true), Index(nameof(VerificationToken), IsUnique = true), Index(nameof(ResetPasswordToken), IsUnique = true)]
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public byte[] Password { get; set; }
        [Required]
        public byte[] PasswordSalt { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Money { get; set; } = 0;
        public int? GuildId { get; set; }
        public int? PartyId { get; set; }
        [ForeignKey("GuildId")]
        public Guild? Guild { get; set; }
        [ForeignKey("PartyId")]
        public Party? Party { get; set; }
        [Required]
        public string Role { get; set; }
        [Required]
        public bool Verified { get; set; } = false;
        public string? RefreshToken { get; set; }
        public byte[] VerificationToken { get; set; }
        public byte[]? ResetPasswordToken { get; set; }
        public DateTime? ResetPassExpires { get; set; }
    }

}
