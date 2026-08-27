using System.ComponentModel.DataAnnotations;

namespace AdventureGuildAPI.Models
{
    public class QuestDto
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Priority Priority { get; set; }
    }
}
