using BooksEcommerce.Data;
using BooksEcommerce.Models;
using BooksEcommerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BooksEcommerce.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext context;
        public CategoryController(ApplicationDbContext _context)
        {
            this.context = _context;
        }

        //[AllowAnonymous]
        //public IActionResult Show()
        //{
        //    var categoryBookList = context.categories.Include(c => c.Books).Select(c => new BookCategoryVM
        //    {
        //        CategoryId = c.CategoryId,
        //        CategoryName = c.CategoryName,
        //        Description = c.Description,
        //    }).ToList();

        //    return View(categoryBookList);
        //}

        [AllowAnonymous]
        public IActionResult AllBooks(int? categoryId)
        {
            var booksQuery = context.books.AsQueryable();

            if (categoryId.HasValue)
            {
                booksQuery = booksQuery.Where(b => b.CategoryId == categoryId);
            }
            var books = booksQuery.Select(b => new BookCategoryVM
            {
                BookId = b.BookId,
                Title = b.Title,
                Author = b.Author,
                PublicationDate = b.PublicationDate,
                ISBN = b.ISBN,
                Description = b.Description,
                Price = b.Price,
                ImageUrl = b.ImageUrl,
                CategoryId = b.CategoryId
            }).ToList();

            var categories = context.categories.Select(c => new BookCategoryVM
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName
            }).ToList();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;
            return View(books);
        }


        // GET: Category/ViewDetails
        public async Task<IActionResult> ViewDetails()
        {
            var categories = await context.categories.ToListAsync();

            return View(categories);
        }


        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if the category already exists to prevent duplicate entries
                    var existingCategory = await context.categories
                       .AsNoTracking()
                       .FirstOrDefaultAsync(c => c.CategoryName == category.CategoryName);

                    if (existingCategory != null)
                    {
                        ModelState.AddModelError("", "This category already exists.");
                        return View(category);
                    }

                    await context.AddAsync(category);
                    await context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Category added successfully!";
                    TempData["RedirectUrl"] = Url.Action("ViewDetails", "Category");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving the data: " + ex.Message);
                }
            }
            return View(category);
        }


        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var category = await context.categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,CategoryName,Description")] Category category)
        {
            if (id != category.CategoryId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(category);
                    await context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Category updated successfully!";
                    TempData["RedirectUrl"] = Url.Action("ViewDetails", "Category");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
                        return NotFound();

                    throw;
                }
               
            }
            return View(category);
        }


        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var category = await context.categories.FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await context.categories.FindAsync(id);
            if (category != null)
            {
                context.categories.Remove(category);
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category deleted successfully!";
                TempData["RedirectUrl"] = Url.Action("ViewDetails", "Category");
            }
            return RedirectToAction("ViewDetails", "Category");
        }

        private bool CategoryExists(int id)
        {
            return context.categories.Any(e => e.CategoryId == id);
        }


    }
}
