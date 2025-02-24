using System.ComponentModel.DataAnnotations;

namespace CyberLoot.Models
{
    public class PasswordResetDto
    {
        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = "";

        [Required, MaxLength(100)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "The Confirm Password field is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match, please try again")]
        public string ConfirmPassword { get; set; } = "";
    }
}
