using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureGuildAPI.Models
{
    public class GuildRequest
    {
        public int RequestId { get; set; }
        public int GuildId { get; set; }
        [ForeignKey("RequestId")]
        public User User { get; set; } = null!;
        [ForeignKey("GuildId")]
        public Guild Guild { get; set; } = null!;
    }
}
