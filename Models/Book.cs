using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using BooksEcommerce.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BooksEcommerce.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required]
        [StringLength(200)]
        public string? Title { get; set; }

        [Required]
        [StringLength(100)]
        public string? Author { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PublicationDate { get; set; }

        [Required]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN must be between 10 and 13 characters.")]
        public string? ISBN { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [ValidateNever]

        public string? ImageUrl { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }
        public ICollection<CartItem>? CartItems { get; set; }


        [Required]
        public int Stock { get; set; }

    }
}
