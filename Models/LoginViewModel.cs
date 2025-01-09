using System.ComponentModel.DataAnnotations;

namespace RobustEcommerceApp.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = null!; // Non-nullable as it's required

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!; // Non-nullable as it's required

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; } = false; // Defaults to false
    }
}
