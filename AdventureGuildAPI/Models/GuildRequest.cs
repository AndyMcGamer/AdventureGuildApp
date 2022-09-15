using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureGuildAPI.Models
{
    [Keyless]
    public class GuildRequest
    {
        public int RequestId { get; set; }
        public int GuildId { get; set; }
        [ForeignKey("RequestId")]
        public User User { get; set; }
        [ForeignKey("GuildId")]
        public Guild Guild { get; set; }
    }
}
