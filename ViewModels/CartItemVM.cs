using BooksEcommerce.Models;

namespace BooksEcommerce.ViewModels
{
    public class CartItemVM
    {
        public int BookId { get; set; }
        public string? Title { get; set; }
        public string? CategoryName { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }

        public int Stock { get; set; }
        public double TotalAmount => Price * Quantity;
        public bool IsUnavailable { get; set; }
    }
}
