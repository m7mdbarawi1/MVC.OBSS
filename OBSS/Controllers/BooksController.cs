using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OBSS.Data;
using OBSS.Models;
using System.Text;

namespace OBSS.Controllers
{
    public class BooksController : Controller
    {
        private readonly OBSSContext _context;
        private readonly IWebHostEnvironment _env;

        public BooksController(OBSSContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Rates)   // 👈 For average rating
                .ToListAsync();

            return View(books);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null) return NotFound();

            return View(book);
        }

        // GET: Books/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "CategoryDesc");
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book, IFormFile? coverImage)
        {
            if (ModelState.IsValid)
            {
                if (coverImage != null && coverImage.Length > 0)
                {
                    string uploadDir = Path.Combine(_env.WebRootPath, "images");
                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    string fileName = Guid.NewGuid() + Path.GetExtension(coverImage.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await coverImage.CopyToAsync(fileStream);
                    }

                    book.CoverImageUrl = "/images/" + fileName;
                }

                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "CategoryDesc", book.CategoryId);
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            var categories = await _context.Categories.ToListAsync();
            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "CategoryDesc", book.CategoryId);
            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book updatedBook, IFormFile? coverImage)
        {
            if (id != updatedBook.BookId) return NotFound();

            if (ModelState.IsValid)
            {
                var existingBook = await _context.Books.FindAsync(id);
                if (existingBook == null) return NotFound();

                // Update scalar fields
                existingBook.BookTitle = updatedBook.BookTitle;
                existingBook.Author = updatedBook.Author;
                existingBook.CategoryId = updatedBook.CategoryId;
                existingBook.Price = updatedBook.Price;
                existingBook.QuantityInStore = updatedBook.QuantityInStore;
                existingBook.Subject = updatedBook.Subject;
                existingBook.PublishingHouse = updatedBook.PublishingHouse;
                existingBook.Description = updatedBook.Description;

                // Handle cover image if provided
                if (coverImage != null && coverImage.Length > 0)
                {
                    string uploadDir = Path.Combine(_env.WebRootPath, "images");
                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    string fileName = Guid.NewGuid() + Path.GetExtension(coverImage.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await coverImage.CopyToAsync(fileStream);
                    }

                    existingBook.CoverImageUrl = "/images/" + fileName;
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(updatedBook.BookId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "CategoryDesc", updatedBook.CategoryId);
            return View(updatedBook);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books
                .Include(b => b.Rates)
                .Include(b => b.CartDetails)
                .Include(b => b.SalesDetails)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book != null)
            {
                // Clean up related data first to avoid FK constraint errors
                if (book.Rates.Any()) _context.Rates.RemoveRange(book.Rates);
                if (book.CartDetails.Any()) _context.CartDetails.RemoveRange(book.CartDetails);
                if (book.SalesDetails.Any()) _context.SalesDetails.RemoveRange(book.SalesDetails);

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Books with zero stock
        public async Task<IActionResult> RequiredBooks()
        {
            var requiredBooks = await _context.Books
                .Include(b => b.Category)
                .Where(b => b.QuantityInStore == 0)
                .ToListAsync();

            return View(requiredBooks);
        }

        // Download report for required books
        public async Task<IActionResult> DownloadRequiredBooksReport()
        {
            var requiredBooks = await _context.Books
                .Include(b => b.Category)
                .Where(b => b.QuantityInStore == 0)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("BookId,Title,Author,Category,Quantity");
            foreach (var book in requiredBooks)
            {
                sb.AppendLine($"{book.BookId},\"{book.BookTitle}\",\"{book.Author}\",{book.Category?.CategoryDesc},0");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "RequiredBooksReport.csv");
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }
    }
}
