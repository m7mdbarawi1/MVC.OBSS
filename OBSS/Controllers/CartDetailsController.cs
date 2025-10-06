using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OBSS.Data;
using OBSS.Models;

namespace OBSS.Controllers
{
    public class CartDetailsController : Controller
    {
        private readonly OBSSContext _context;

        public CartDetailsController(OBSSContext context)
        {
            _context = context;
        }

        // GET: CartDetails
        [Authorize(Roles = "Admin")] // Only Admin can see all cart details
        public async Task<IActionResult> Index()
        {
            var oBSSContext = _context.CartDetails.Include(c => c.Book).Include(c => c.Cart);
            return View(await oBSSContext.ToListAsync());
        }

        // GET: CartDetails/Details
        [Authorize(Roles = "Admin")] // Only Admin can view details
        public async Task<IActionResult> Details(int? cartId, int? bookId)
        {
            if (cartId == null || bookId == null)
            {
                return NotFound();
            }

            var cartDetail = await _context.CartDetails.Include(c => c.Book).Include(c => c.Cart).FirstOrDefaultAsync(cd => cd.CartId == cartId && cd.BookId == bookId);

            if (cartDetail == null)
            {
                return NotFound();
            }

            return View(cartDetail);
        }

        // GET: CartDetails/Create
        [Authorize(Roles = "Admin")] // Only Admin can access create page
        public IActionResult Create()
        {
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookId");
            ViewData["CartId"] = new SelectList(_context.Carts, "CartId", "CartId");
            return View();
        }

        // POST: CartDetails/Create
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can create
        public async Task<IActionResult> Create([Bind("CartId,BookId,Quantity")] CartDetail cartDetail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cartDetail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookId", cartDetail.BookId);
            ViewData["CartId"] = new SelectList(_context.Carts, "CartId", "CartId", cartDetail.CartId);
            return View(cartDetail);
        }

        // GET: CartDetails/Edit
        [Authorize(Roles = "Admin")] // Only Admin can access edit page
        public async Task<IActionResult> Edit(int? cartId, int? bookId)
        {
            if (cartId == null || bookId == null)
            {
                return NotFound();
            }

            var cartDetail = await _context.CartDetails.FirstOrDefaultAsync(cd => cd.CartId == cartId && cd.BookId == bookId);

            if (cartDetail == null)
            {
                return NotFound();
            }

            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookId", cartDetail.BookId);
            ViewData["CartId"] = new SelectList(_context.Carts, "CartId", "CartId", cartDetail.CartId);
            return View(cartDetail);
        }

        // POST: CartDetails/Edit
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can edit
        public async Task<IActionResult> Edit(int cartId, int bookId, [Bind("CartId,BookId,Quantity")] CartDetail cartDetail)
        {
            if (cartId != cartDetail.CartId || bookId != cartDetail.BookId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cartDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartDetailExists(cartId, bookId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookId", cartDetail.BookId);
            ViewData["CartId"] = new SelectList(_context.Carts, "CartId", "CartId", cartDetail.CartId);
            return View(cartDetail);
        }

        // GET: CartDetails/Delete
        [Authorize(Roles = "Admin")] // Only Admin can access delete page
        public async Task<IActionResult> Delete(int? cartId, int? bookId)
        {
            if (cartId == null || bookId == null)
            {
                return NotFound();
            }

            var cartDetail = await _context.CartDetails.Include(c => c.Book).Include(c => c.Cart).FirstOrDefaultAsync(cd => cd.CartId == cartId && cd.BookId == bookId);

            if (cartDetail == null)
            {
                return NotFound();
            }

            return View(cartDetail);
        }

        // POST: CartDetails/Delete
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can edit
        public async Task<IActionResult> DeleteConfirmed(int cartId, int bookId)
        {
            var cartDetail = await _context.CartDetails.FirstOrDefaultAsync(cd => cd.CartId == cartId && cd.BookId == bookId);

            if (cartDetail != null)
            {
                _context.CartDetails.Remove(cartDetail);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CartDetailExists(int cartId, int bookId)
        {
            return _context.CartDetails.Any(e => e.CartId == cartId && e.BookId == bookId);
        }
    }
}
