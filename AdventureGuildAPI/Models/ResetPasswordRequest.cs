namespace AdventureGuildAPI.Models
{
    public class ResetPasswordRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        public byte[] Token { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.Required]
        public string Password { get; set; } = null!;
    }
}
