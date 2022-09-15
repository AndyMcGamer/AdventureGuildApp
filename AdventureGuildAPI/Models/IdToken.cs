namespace AdventureGuildAPI.Models
{
    public class IdToken
    {
        public byte[] TokenHash { get; set; }
        public byte[] TokenSalt { get; set; }
    }
}
