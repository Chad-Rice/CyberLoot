using System.ComponentModel.DataAnnotations;

namespace CyberLoot.Models
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "The First Name field is required"), MaxLength(100)]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "The Last Name field is required"), MaxLength(100)]
        public string LastName { get; set; } = "";

        [Required,  EmailAddress, MaxLength(100)]
        public string Email { get; set; } = "";

        [Phone(ErrorMessage = "The format of the Phone Number is not valid"), MaxLength(20)]
        public string? PhoneNumber { get; set; } = "";

        [Required, MaxLength(100)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "The Confirm Password field is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match, please try again")]
        public string ConfirmPassword { get; set; } = "";
    }
}
