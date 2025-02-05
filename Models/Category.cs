using System.ComponentModel.DataAnnotations;

namespace BooksEcommerce.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string? CategoryName { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }
        public List<Book>? Books { get; set; }
    }
}

