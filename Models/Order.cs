using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BooksEcommerce.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public double TotalAmount { get; set; }
        public int OrderStatus { get; set; }

        [ForeignKey("ApplicationUser")]
        public string? Id { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        public List<OrderDetail>? OrderDetails { get; set; }
    }
}
