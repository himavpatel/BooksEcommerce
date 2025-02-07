using BooksEcommerce.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BooksEcommerce.ViewModels
{
    public class BookCategoryVM
    {
        public int BookId { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Author { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PublicationDate { get; set; }

        [Required]
        public string? ISBN { get; set; }

        [Required]
        [StringLength(500)]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

      
        public IFormFile? ImageFile { get; set; }

        public int Quantity { get; set; }

        [Required]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public List<Category>? Categories { get; set; }

        public List<Book>? Books { get; set; }

        [Required]
        public int Stock { get; set; }
    }
}
