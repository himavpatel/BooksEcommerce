using System.ComponentModel.DataAnnotations;

namespace BooksEcommerce.ViewModels
{
    public class ForgotPasswordVM
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

    }
}
