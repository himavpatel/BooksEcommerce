using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BooksEcommerce.Models
{
    public class CartItem
    {
        [Key]
        public int CartId { get; set; }

        [ForeignKey("ApplicationUser")]
        public string? Id { get; set; }
        public ApplicationUser? User { get; set; }

        [ForeignKey("Book")]
        public int BookId { get; set; } 
        public int Quantity { get; set; }

        public Book? Book { get; set; }
    }
}
