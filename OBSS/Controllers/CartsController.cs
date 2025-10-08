using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OBSS.Data;
using OBSS.Models;

namespace OBSS.Controllers
{
    public class CartsController : Controller
    {
        private readonly OBSSContext _context;

        public CartsController(OBSSContext context)
        {
            _context = context;
        }

        // ------------------- Admin CRUD -------------------

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var oBSSContext = _context.Carts.Include(c => c.User);
            return View(await oBSSContext.ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var cart = await _context.Carts
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CartId == id);

            if (cart == null)
                return NotFound();

            return View(cart);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("CartId,UserId,CreationDate")] Cart cart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", cart.UserId);
            return View(cart);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
                return NotFound();

            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", cart.UserId);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("CartId,UserId,CreationDate")] Cart cart)
        {
            if (id != cart.CartId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartExists(cart.CartId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", cart.UserId);
            return View(cart);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var cart = await _context.Carts
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CartId == id);

            if (cart == null)
                return NotFound();

            return View(cart);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.CartId == id);
        }

        // ------------------- Customer Cart -------------------

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyCart()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return View(new List<CartDetail>());

            var cartDetails = await _context.CartDetails
                .Include(cd => cd.Book)
                .ThenInclude(b => b.Category)
                .Where(cd => cd.CartId == cart.CartId)
                .ToListAsync();

            // Remove expired items (optional)
            var expiredItems = cartDetails.Where(cd => cd.AddedDate < DateTime.Now.AddMinutes(-1)).ToList();
            if (expiredItems.Any())
            {
                _context.CartDetails.RemoveRange(expiredItems);
                await _context.SaveChangesAsync();

                TempData["Info"] = $"{expiredItems.Count} item(s) removed due to expiration.";
                cartDetails = cartDetails.Except(expiredItems).ToList();
            }

            // Adjust quantities for stock limits
            foreach (var item in cartDetails)
            {
                if (item.Book.QuantityInStore < item.Quantity)
                {
                    item.Quantity = item.Book.QuantityInStore;
                }
            }
            await _context.SaveChangesAsync();

            return View(cartDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddToCart(int bookId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var book = await _context.Books.FindAsync(bookId);
            if (book == null || book.QuantityInStore <= 0)
            {
                TempData["Error"] = "This book is out of stock.";
                return RedirectToAction(nameof(MyCart));
            }

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreationDate = DateOnly.FromDateTime(DateTime.Now)
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = await _context.CartDetails
                .FirstOrDefaultAsync(cd => cd.CartId == cart.CartId && cd.BookId == bookId);

            if (existingItem == null)
            {
                _context.CartDetails.Add(new CartDetail
                {
                    CartId = cart.CartId,
                    BookId = bookId,
                    Quantity = 1,
                    AddedDate = DateTime.Now
                });
            }
            else
            {
                if (existingItem.Quantity < book.QuantityInStore)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    TempData["Error"] = "You cannot add more than available stock.";
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyCart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RemoveFromCart(int bookId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
                return RedirectToAction(nameof(MyCart));

            var item = await _context.CartDetails.FirstOrDefaultAsync(cd => cd.CartId == cart.CartId && cd.BookId == bookId);

            if (item != null)
            {
                _context.CartDetails.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyCart));
        }

        // ------------------- Purchase -------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Purchase()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.CartDetails)
                .ThenInclude(cd => cd.Book)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartDetails.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction(nameof(MyCart));
            }

            // Check stock before purchase
            var invalidItems = cart.CartDetails
                .Where(cd => cd.Book.QuantityInStore <= 0 || cd.Quantity > cd.Book.QuantityInStore)
                .ToList();

            if (invalidItems.Any())
            {
                TempData["Error"] = "Some items exceed available stock.";
                return RedirectToAction(nameof(MyCart));
            }

            // Create sale record
            var sale = new Sale
            {
                UserId = userId,
                SaleDate = DateOnly.FromDateTime(DateTime.Now)
            };
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            foreach (var cartItem in cart.CartDetails)
            {
                // Subtract stock
                cartItem.Book.QuantityInStore -= cartItem.Quantity;

                // Add to sales details
                _context.SalesDetails.Add(new SalesDetail
                {
                    SaleId = sale.SaleId,
                    BookId = cartItem.BookId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Book.Price
                });
            }

            // Clear cart after purchase
            _context.CartDetails.RemoveRange(cart.CartDetails);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Purchase completed successfully!";
            return RedirectToAction(nameof(MyCart));
        }

        // ------------------- Quantity Update -------------------

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateQuantity(int bookId, int quantity)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
                return Json(new { success = false });

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
                return Json(new { success = false });

            var item = await _context.CartDetails
                .Include(cd => cd.Book)
                .FirstOrDefaultAsync(cd => cd.CartId == cart.CartId && cd.BookId == bookId);

            if (item == null)
                return Json(new { success = false });

            if (quantity < 1)
                quantity = 1;

            if (quantity > item.Book.QuantityInStore)
                quantity = item.Book.QuantityInStore;

            item.Quantity = quantity;
            await _context.SaveChangesAsync();

            var cartDetails = await _context.CartDetails.Include(cd => cd.Book).Where(cd => cd.CartId == cart.CartId).ToListAsync();
            var itemPrice = item.Book.Price * item.Quantity;
            var totalPrice = cartDetails.Sum(cd => cd.Book.Price * cd.Quantity);

            return Json(new { success = true, itemPrice, totalPrice });
        }
    }
}
