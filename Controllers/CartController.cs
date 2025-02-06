using BooksEcommerce.Data;
using BooksEcommerce.Models;
using BooksEcommerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BooksEcommerce.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private static List<CartItemVM> CartItems = new List<CartItemVM>();
        private readonly ApplicationDbContext context;
        public CartController(ApplicationDbContext _context)
        {
            this.context = _context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Display Cart Items
        public IActionResult Index()
        {
            var userId = GetUserId();
            var cartItems = context.cartitems.Include(c => c.Book)
                .Where(c => c.Id == userId)
                .Select(c => new CartItemVM
                {
                    BookId = c.BookId,
                    Title = c.Book.Title,
                    Stock = c.Book.Stock,
                    CategoryName = c.Book.Category.CategoryName, // Ensure Category is a valid navigation property
                    Price = c.Book.Price,
                    Quantity = c.Quantity,
                    IsUnavailable = c.Quantity > c.Book.Stock
                }).ToList();

            ViewBag.TotalPrice = cartItems.Sum(item => item.TotalAmount); // Calculate total price dynamically
            return View(cartItems);
        }

        // Add Item to Cart
        [HttpPost]
        public IActionResult AddToCart(int bookId, int quantity)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var book = context.books.FirstOrDefault(b => b.BookId == bookId);
            if (book == null) return NotFound();


            // Ensure cart is user-specific
            var cartItem = context.cartitems.FirstOrDefault(c => c.BookId == bookId && c.Id == userId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                var newCartItem = new CartItem
                {
                    BookId = book.BookId,
                    Id = userId, // Ensure each user has a separate cart
                    Quantity = quantity
                };

                context.cartitems.Add(newCartItem);
            }
            context.SaveChanges();
            return RedirectToAction("Index");
        }

        // Update Quantity of an Item
        [HttpPost]
        public IActionResult UpdateQuantity(int bookId, bool isIncrease)
        {
              var userId = GetUserId();
               var item = context.cartitems.FirstOrDefault(i => i.BookId == bookId && i.Id == userId);
            //var item = context.cartitems.FirstOrDefault(i => i.BookId == bookId );
            if (item != null)
            {
                if (isIncrease)
                    item.Quantity++;
                else if (item.Quantity > 1)
                    item.Quantity--;
            }

            context.SaveChanges();
            return RedirectToAction("Index");
        }

        // Remove Item from Cart
        [HttpPost]
        public IActionResult RemoveFromCart(int bookId)
        {
            var userId = GetUserId();
           var item = context.cartitems.FirstOrDefault(i => i.BookId == bookId && i.Id == userId);
            //var item = context.cartitems.FirstOrDefault(i => i.BookId == bookId );
            if (item != null)
            {
                context.cartitems.Remove(item);
                context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // Checkout - Save Order
        [HttpPost]
        public IActionResult SaveOrder()
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var cartItems = context.cartitems.Include(c => c.Book)
                .Where(c => c.Id == userId)
                .ToList();

            if (!cartItems.Any()) return RedirectToAction("Index");


            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    // Fetch stock from DB with a lock to prevent race conditions
                    var booksToUpdate = context.books
                        .Where(b => cartItems.Select(c => c.BookId).Contains(b.BookId))
                        .ToList();

                    foreach (var item in cartItems)
                    {
                        var book = booksToUpdate.FirstOrDefault(b => b.BookId == item.BookId);
                        if (book == null || book.Stock < item.Quantity)
                        {
                            TempData["ErrorMessage"] = $"Sorry, {item.Book.Title} is out of stock!";
                            return RedirectToAction("Index");
                        }
                    }

                    // Deduct stock and create order
                    foreach (var item in cartItems)
                    {
                        var book = booksToUpdate.FirstOrDefault(b => b.BookId == item.BookId);
                        if (book != null)
                        {
                            book.Stock -= item.Quantity; // Reduce stock after confirming availability
                        }
                    }


                    var order = new Order
                    {
                        Id = userId,
                        OrderDate = DateTime.Now,
                        TotalAmount = cartItems.Sum(item => item.Book.Price * item.Quantity),
                        OrderStatus = 0,
                        OrderDetails = cartItems.Select(c => new OrderDetail
                        {
                            BookId = c.BookId,
                            Quantity = c.Quantity,
                            Price = c.Book.Price
                        }).ToList()
                    };

                    context.orders.Add(order);
                    context.cartitems.RemoveRange(cartItems); // Clear the cart after checkout
                    context.SaveChanges();
                    transaction.Commit();
                    return RedirectToAction("ShowOrderDetails", "Cart", new { orderId = order.OrderId });
                }
                catch
                {
                    transaction.Rollback();
                    TempData["ErrorMessage"] = "An error occurred while processing your order.";
                    return RedirectToAction("Index");
                }
            }
        }

        public IActionResult ShowOrderDetails(int orderId)
        {
            var order = context.orders
                .Include(o => o.ApplicationUser) // Ensure the User entity is loaded
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null) return NotFound("Order not found!");

            var viewModel = new OrderDetailsVM
            {
                Name = order.ApplicationUser.Name,
                Email = order.ApplicationUser.Email,// Ensure safe access to Email
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                TotalFinalPrice = order.TotalAmount,
                Books = order.OrderDetails.Select(od => new BookCategoryVM
                {
                    BookId = od.BookId,
                    Title = od.Book.Title,
                    Price = od.Book.Price,
                    Quantity = od.Quantity
                }).ToList()
            };

            return View(viewModel);
        }


    }
}
