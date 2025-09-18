using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OBSS.Data;
using OBSS.Models;

namespace OBSS.Controllers
{
    public class RatesController : Controller
    {
        private readonly OBSSContext _context;

        public RatesController(OBSSContext context)
        {
            _context = context;
        }

        // GET: Rates
        public async Task<IActionResult> Index()
        {
            var rates = _context.Rates
                .Include(r => r.Book)
                .Include(r => r.User);
            return View(await rates.ToListAsync());
        }

        // GET: Rates/Details
        public async Task<IActionResult> Details(int bookId, int userId)
        {
            var rate = await _context.Rates
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.BookId == bookId && m.UserId == userId);

            if (rate == null) return NotFound();
            return View(rate);
        }

        // GET: Rates/Create
        public IActionResult Create()
        {
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookTitle");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserName");
            return View();
        }

        // POST: Rates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,UserId,Rate1")] Rate rate)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookTitle", rate.BookId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserName", rate.UserId);
            return View(rate);
        }

        // GET: Rates/Edit
        public async Task<IActionResult> Edit(int bookId, int userId)
        {
            var rate = await _context.Rates
                .FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == userId);
            if (rate == null) return NotFound();

            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookTitle", rate.BookId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserName", rate.UserId);
            return View(rate);
        }

        // POST: Rates/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int bookId, int userId, [Bind("BookId,UserId,Rate1")] Rate rate)
        {
            if (bookId != rate.BookId || userId != rate.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RateExists(bookId, userId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(rate);
        }

        // GET: Rates/Delete
        public async Task<IActionResult> Delete(int bookId, int userId)
        {
            var rate = await _context.Rates
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.BookId == bookId && m.UserId == userId);

            if (rate == null) return NotFound();

            return View(rate);
        }

        // POST: Rates/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int bookId, int userId)
        {
            var rate = await _context.Rates
                .FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == userId);

            if (rate != null)
            {
                _context.Rates.Remove(rate);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult RateBook(int bookId, int rating)
        {
            if (!(User.Identity?.IsAuthenticated ?? false))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var existingRate = _context.Rates
                .FirstOrDefault(r => r.BookId == bookId && r.UserId == userId);

            if (rating == 0)
            {
                if (existingRate != null)
                {
                    _context.Rates.Remove(existingRate);
                    _context.SaveChanges();
                }
            }
            else
            {
                if (existingRate == null)
                {
                    _context.Rates.Add(new Rate { BookId = bookId, UserId = userId, Rate1 = rating });
                }
                else
                {
                    existingRate.Rate1 = rating;
                }
                _context.SaveChanges();
            }

            // ✅ Force client evaluation for Avg
            var avgRating = _context.Rates
                .Where(r => r.BookId == bookId)
                .AsEnumerable()
                .Select(r => r.Rate1)
                .DefaultIfEmpty(0)
                .Average();

            return Json(new { success = true, rating, avgRating = Math.Round(avgRating, 1) });
        }


        private bool RateExists(int bookId, int userId)
        {
            return _context.Rates.Any(e => e.BookId == bookId && e.UserId == userId);
        }
    }
}
