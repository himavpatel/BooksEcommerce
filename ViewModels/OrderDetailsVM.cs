using BooksEcommerce.Models;

namespace BooksEcommerce.ViewModels
{
    public class OrderDetailsVM
    {
        public string? Id { get; set; }

        public string? Name { get; set; }
        public string? Email { get; set; }
        public int OrderId { get; set; }
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
        public double TotalFinalPrice { get; set; }
        public List<BookCategoryVM>? Books { get; set; }
    }
}
