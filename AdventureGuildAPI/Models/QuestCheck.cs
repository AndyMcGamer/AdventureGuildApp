using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureGuildAPI.Models
{
    public class QuestCheck
    {
        public int RequestId { get; set; }
        public int? PartyId { get; set; }
        public int QuestId { get; set; }
        public string? ImageRef { get; set; }

        [ForeignKey("RequestId")]
        public User User { get; set; } = null!;
        [ForeignKey("QuestId")]
        public Quest Quest { get; set; } = null!;
        [ForeignKey("PartyId")]
        public Party? Party { get; set; }

    }
}
