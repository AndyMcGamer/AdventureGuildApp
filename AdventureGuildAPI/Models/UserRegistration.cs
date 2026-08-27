namespace AdventureGuildAPI.Models
{
    public class UserRegistration
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Username { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.Required]
        public string Password { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.Required]
        public string FirstName { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.Required]
        public string LastName { get; set; } = null!;
    }
}
