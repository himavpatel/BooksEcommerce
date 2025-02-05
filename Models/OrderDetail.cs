using System.ComponentModel.DataAnnotations.Schema;

namespace BooksEcommerce.Models
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; } // FK to Order

        [ForeignKey("Book")]
        public int BookId { get; set; } // FK to Book
        
        public int Quantity { get; set; }
        public double Price { get; set; }

        public Order? Order { get; set; }
        public Book? Book { get; set; }

    }
}
