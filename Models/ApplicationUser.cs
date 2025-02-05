using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BooksEcommerce.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string? Name { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        public List<Order>? Orders { get; set; }
    }
}
