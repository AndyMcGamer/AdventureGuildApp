using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureGuildAPI.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Guild
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsPrivate { get; set; }
        public int LeaderId { get; set; }

        [ForeignKey("LeaderId")]
        public User Leader { get; set; }
    }
}
