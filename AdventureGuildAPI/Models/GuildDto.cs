namespace AdventureGuildAPI.Models
{
    public class GuildDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsPrivate { get; set; }
    }
}
