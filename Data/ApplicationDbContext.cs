using BooksEcommerce.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BooksEcommerce.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
     : base(options)
        {
        }

        public DbSet<Category> categories { get; set; }
        public DbSet<Book> books { get; set; }

        public DbSet<Order> orders { get; set; }
        public DbSet<OrderDetail> orderdetails { get; set; }

        public DbSet<CartItem> cartitems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.ISBN)
                .IsUnique(); // Ensure ISBN is unique

            base.OnModelCreating(modelBuilder);
        }
    }
}
