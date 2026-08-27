using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureGuildAPI.Models
{
    public class Quest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Priority Priority { get; set; }
        public DateTime CreatedDateTime { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }

    public enum Priority
    {
        Low,
        Medium,
        High,
        Urgent
    }
}
