namespace AdventureGuildAPI.Models
{
    public class IdToken
    {
        public byte[] TokenHash { get; set; } = null!;
        public byte[] TokenSalt { get; set; } = null!;
    }
}
