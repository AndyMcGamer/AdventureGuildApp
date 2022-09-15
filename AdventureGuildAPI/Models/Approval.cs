using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureGuildAPI.Models
{
    public class Approval
    {
        public int ApproverId { get; set; }
        public int QuestId { get; set; }
        public bool Approved { get; set; }
        [ForeignKey("ApproverId")]
        public User User { get; set; }
        [ForeignKey("QuestId")]
        public Quest Quest { get; set; }
    }
}
