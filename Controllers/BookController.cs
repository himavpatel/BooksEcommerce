using BooksEcommerce.Data;
using BooksEcommerce.Models;
using BooksEcommerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BooksEcommerce.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BookController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }


        //[AllowAnonymous]
        //public IActionResult BookList(int id)
        //{
        //    var books = _context.books
        //        .Where(b => b.CategoryId == id)
        //        .Select(b => new BookCategoryVM
        //        {
        //            BookId = b.BookId,
        //            Title = b.Title,
        //            Author = b.Author,
        //            PublicationDate = b.PublicationDate,
        //            ISBN = b.ISBN,
        //            Description = b.Description,
        //            Price = b.Price,
        //            ImageUrl = b.ImageUrl,
        //        })
        //        .ToList();

        //    var category = _context.categories.FirstOrDefault(c => c.CategoryId == id);
        //    ViewBag.CategoryName = category?.CategoryName;

        //    return View(books);
        //}


        // GET: Books
        public async Task<IActionResult> BookIndex()
        {
            var data = _context.books.Include(p => p.Category).OrderBy(i => i.BookId).ToList();
            return View(data);

        }


        // GET: Book/Create
        public IActionResult BookCreate()
        {
            var model = new BookCategoryVM
            {
                Categories = _context.categories.ToList()
            };
            return View(model);
        }


        // POST: Book/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookCreate(BookCategoryVM book, IFormFile ImageFile)
        {
            if (ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Please upload an image.");
            }

            //  if (ModelState.IsValid)
            if (!ModelState.IsValid)
            {
                book.Categories = _context.categories.ToList();
                return View(book);
            }

                try
                {
                    // Prevent duplicate book entry based on ISBN
                    var existingBook = await _context.books
                        .AsNoTracking()
                        .FirstOrDefaultAsync(b => b.BookId == book.BookId);

                    if (existingBook != null)
                    {
                        if (existingBook.BookId == book.BookId)
                        {
                            ModelState.AddModelError("", "A book with this Book ID already exists.");
                        }

                        if (existingBook.ISBN == book.ISBN)
                        {
                            ModelState.AddModelError("", "A book with this ISBN already exists.");
                        }

                        ViewData["CategoryId"] = new SelectList(_context.categories, "CategoryId", "CategoryName", book.CategoryId);
                        return View(book);
                    }

                    string stringFileName = UploadImage(ImageFile);
                    var data = new Book()
                    {
                        Title = book.Title,
                        ISBN = book.ISBN,
                        Price = book.Price,
                        Stock = book.Stock,
                        PublicationDate = book.PublicationDate,
                        Description = book.Description,
                        Author = book.Author,
                        ImageUrl = stringFileName,
                        CategoryId = book.CategoryId
                    };
                    _context.Add(data);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Book added successfully!";
                    TempData["RedirectUrl"] = Url.Action("BookIndex", "Book");

                    return RedirectToAction(nameof(BookIndex));

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred: " + ex.Message);
                    book.Categories = _context.categories.ToList();
                    return View(book);
                }


        }

        // Helper method to handle file upload
        //private string UploadImage(IFormFile ImageFile)
        //{
        //    string fileName = null;
        //    if (ImageFile != null)
        //    {
        //        string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
        //        Directory.CreateDirectory(uploadDir); // Ensure the directory exists
        //        fileName = Guid.NewGuid().ToString() + "-" + Path.GetFileName(ImageFile.FileName);
        //        string filePath = Path.Combine(uploadDir, fileName);
        //        using (var fileStream = new FileStream(filePath, FileMode.Create))
        //        {
        //            ImageFile.CopyTo(fileStream);
        //        }
        //    }
        //    return fileName;
        //}

        //private bool BooksExists(int id)
        //{
        //    return _context.books.Any(e => e.BookId == id);
        //}

        private string UploadImage(IFormFile ImageFile)
        {
            string fileName = null;

            if (ImageFile != null)
            {

                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                Directory.CreateDirectory(uploadDir); 

                string originalFileName = Path.GetFileName(ImageFile.FileName);
                string filePath = Path.Combine(uploadDir, originalFileName);

                if (System.IO.File.Exists(filePath))
                {
                    fileName = originalFileName;
                }
                else
                {
                    fileName = originalFileName;
                    string uniqueFilePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(uniqueFilePath, FileMode.Create))
                    {
                        ImageFile.CopyTo(fileStream);
                    }
                }
            }
            return fileName;
        }

        private void DeleteImage(string imagePath)
        {
            if (imagePath != null)
            {
                string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
        }


        public async Task<IActionResult> BookDelete(int? id)
        {
            if (id == null || _context.books == null)
            {
                return NotFound();
            }
            var data = await _context.books
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (data == null)
            {
                return NotFound();
            }
            return View(data);
        }

        // POST: Books/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookDeleteConfirmed(int BookId)
        {
            if (_context.books == null)
            {
                return Problem("Entity set 'ApplicationDbContext.books' is null.");
            }

            var data = await _context.books.FindAsync(BookId);
            if (data != null)
            {
                _context.books.Remove(data);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Book deleted successfully!";
                TempData["RedirectUrl"] = Url.Action("BookIndex", "Book");
            }
            return RedirectToAction(nameof(BookIndex));
        }


        public IActionResult BookEdit(int id)
        {
            var book = _context.books.Find(id);
            if (book == null)
                return NotFound();
            
            var categories = _context.categories.ToList();

            var model = new BookCategoryVM
            {
                BookId = book.BookId,
                Title = book.Title,
                Author = book.Author,
                Stock = book.Stock,
                Description = book.Description,
                ISBN = book.ISBN,
                PublicationDate = book.PublicationDate,
                Price = book.Price,
                CategoryId = book.CategoryId,
                ImageUrl = book.ImageUrl,
                Categories = categories
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BookEdit(BookCategoryVM model)
        {
            if (ModelState.IsValid)
            {
                var book = _context.books.Find(model.BookId);
                if (book == null)
                    return NotFound();

                string imagePath = book.ImageUrl;

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    imagePath = UploadImage(model.ImageFile);
                    if (!string.IsNullOrEmpty(book.ImageUrl))
                    {
                        DeleteImage(book.ImageUrl);
                    }
                }

                book.BookId = model.BookId;
                book.Title = model.Title;
                book.Author = model.Author;
                book.ISBN = model.ISBN;
                book.Stock = model.Stock;
                book.PublicationDate = model.PublicationDate;
                book.Price = model.Price;
                book.CategoryId = model.CategoryId;
                book.Description = model.Description;
                book.ImageUrl = imagePath;

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Book updated successfully!";
                TempData["RedirectUrl"] = Url.Action("BookIndex", "Book");

                return RedirectToAction(nameof(BookIndex));
            }
            model.Categories = _context.categories.ToList();
            return View(model);
        }


    }
}
