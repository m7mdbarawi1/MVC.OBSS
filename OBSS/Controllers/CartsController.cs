using System;
using System.Collections.Generic;
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
    public class CartsController : Controller
    {
        private readonly OBSSContext _context;

        public CartsController(OBSSContext context)
        {
            _context = context;
        }

        // ===========================
        // Scaffolded CRUD
        // ===========================
        public async Task<IActionResult> Index()
        {
            var oBSSContext = _context.Carts.Include(c => c.User);
            return View(await oBSSContext.ToListAsync());
        }

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

        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // ===========================
        // Custom Cart Actions
        // ===========================

        // My Cart for logged-in user
        public async Task<IActionResult> MyCart()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return View(new List<CartDetail>());

            var cartDetails = await _context.CartDetails
                .Include(cd => cd.Book)
                    .ThenInclude(b => b.Category)
                .Where(cd => cd.CartId == cart.CartId)
                .ToListAsync();
            var expiredItems = cartDetails
                .Where(cd => cd.AddedDate < DateTime.Now.AddMinutes(-1)) // ✅ expire after 1 minute
                .ToList();
            if (expiredItems.Any())
            {
                _context.CartDetails.RemoveRange(expiredItems); // like pressing Remove
                await _context.SaveChangesAsync();

                TempData["Info"] = $"{expiredItems.Count} item(s) were automatically removed because they were older than 1 minute.";
                cartDetails = cartDetails.Except(expiredItems).ToList();
            }

            return View(cartDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int bookId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

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
                existingItem.Quantity++;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyCart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int bookId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return RedirectToAction(nameof(MyCart));

            var item = await _context.CartDetails
                .FirstOrDefaultAsync(cd => cd.CartId == cart.CartId && cd.BookId == bookId);

            if (item != null)
            {
                _context.CartDetails.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyCart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

            // Create sale record
            var sale = new Sale
            {
                UserId = userId,
                SaleDate = DateOnly.FromDateTime(DateTime.Now)
            };
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            // Process each item
            foreach (var cartItem in cart.CartDetails)
            {
                if (cartItem.Book != null)
                {
                    cartItem.Book.QuantityInStore -= cartItem.Quantity;
                    if (cartItem.Book.QuantityInStore < 0)
                        cartItem.Book.QuantityInStore = 0;
                }

                _context.SalesDetails.Add(new SalesDetail
                {
                    SaleId = sale.SaleId,
                    BookId = cartItem.BookId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Book?.Price ?? 0
                });
            }

            // Clear cart
            _context.CartDetails.RemoveRange(cart.CartDetails);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Purchase completed successfully!";
            return RedirectToAction(nameof(MyCart));
        }


        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int bookId, int quantity)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
                return Json(new { success = false });

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
                return Json(new { success = false });

            var item = await _context.CartDetails
                .Include(cd => cd.Book)
                .FirstOrDefaultAsync(cd => cd.CartId == cart.CartId && cd.BookId == bookId);

            if (item == null)
                return Json(new { success = false });

            item.Quantity = quantity > 0 ? quantity : 1;
            await _context.SaveChangesAsync();

            var cartDetails = await _context.CartDetails
                .Include(cd => cd.Book)
                .Where(cd => cd.CartId == cart.CartId)
                .ToListAsync();

            var itemPrice = item.Book.Price * item.Quantity;
            var totalPrice = cartDetails.Sum(cd => cd.Book.Price * cd.Quantity);

            return Json(new
            {
                success = true,
                itemPrice,
                totalPrice
            });
        }
    }
}
