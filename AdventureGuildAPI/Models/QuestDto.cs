using System.ComponentModel.DataAnnotations;

namespace AdventureGuildAPI.Models
{
    public class QuestDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public Priority Priority { get; set; }
    }
}
