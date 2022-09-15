namespace AdventureGuildAPI.Models
{
    public class ResetPasswordRequest
    {
        public byte[] Token { get; set; }
        public string Password { get; set; }
    }
}
