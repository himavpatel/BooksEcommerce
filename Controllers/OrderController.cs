using BooksEcommerce.Data;
using BooksEcommerce.Models;
using BooksEcommerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BooksEcommerce.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Method for users to see their own orders

        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var orders = await _context.orders
                .Where(o => o.Id == userId)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var viewModel = orders.Select(o => new OrderDetailsVM
            {
                OrderId = o.OrderId,
                OrderDate = o.OrderDate,
                TotalFinalPrice = o.TotalAmount,
                Books = o.OrderDetails.Select(od => new BookCategoryVM
                {
                    BookId = od.BookId,
                    Title = od.Book.Title,
                    Price = od.Book.Price,
                    Quantity = od.Quantity
                }).ToList()
            }).ToList();

            return View(viewModel);
        }

        // Method for admins to see all orders
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> OrdersList()
        {
            var orders = await _context.orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var viewModel = orders.Select(o => new OrderDetailsVM
            {
                OrderId = o.OrderId,
                OrderDate = o.OrderDate,
                TotalFinalPrice = o.TotalAmount,
                Name = o.ApplicationUser.Name,
                Address = o.ApplicationUser.Address,
                PhoneNumber = o.ApplicationUser.PhoneNumber,
                Email = o.ApplicationUser?.Email ?? "Unknown",
                Books = o.OrderDetails.Select(od => new BookCategoryVM
                {
                    BookId = od.BookId,
                    Title = od.Book.Title,
                    Price = od.Book.Price,
                    Quantity = od.Quantity
                }).ToList()
            }).ToList();

            return View(viewModel);
        }
    }
}
