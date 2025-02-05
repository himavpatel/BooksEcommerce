using System.ComponentModel.DataAnnotations;

namespace BooksEcommerce.ViewModels
{
    public class ResetPasswordVM
    {

        public string? UserId { get; set; }
        public string? Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string? NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }

    }
}
