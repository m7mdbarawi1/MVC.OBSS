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
    public class UsersController : Controller
    {
        private readonly OBSSContext _context;

        public UsersController(OBSSContext context)
        {
            _context = context;
        }

        // GET: Users
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var oBSSContext = _context.Users.Include(u => u.Gender).Include(u => u.UserTypeNavigation);
            return View(await oBSSContext.ToListAsync());
        }

        // GET: Users/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.Include(u => u.Gender).Include(u => u.UserTypeNavigation).FirstOrDefaultAsync(m => m.UserId == id);
            
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["GenderId"] = new SelectList(_context.Genders, "GenderId", "GenderDesc");
            ViewData["UserType"] = new SelectList(_context.UserTypes, "TypeId", "TypeDesc");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,UserType,UserName,Password,FirstName,LastName,Birthdate,GenderId,ContactNumber,Email")] User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
            }

            if (_context.Users.Any(u => u.UserName == user.UserName))
            {
                ModelState.AddModelError("UserName", "This username is already taken.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["GenderId"] = new SelectList(_context.Genders, "GenderId", "GenderId", user.GenderId);
            ViewData["UserType"] = new SelectList(_context.UserTypes, "TypeId", "TypeId", user.UserType);
            return View(user);
        }

        // GET: Users/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }
            ViewData["GenderId"] = new SelectList(_context.Genders, "GenderId", "GenderDesc", user.GenderId);
            ViewData["UserType"] = new SelectList(_context.UserTypes, "TypeId", "TypeDesc", user.UserType);
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,UserType,UserName,Password,FirstName,LastName,Birthdate,GenderId,ContactNumber,Email")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (_context.Users.Any(u => u.Email == user.Email && u.UserId != id))
            {
                ModelState.AddModelError("Email", "This email is already registered by another user.");
            }

            if (_context.Users.Any(u => u.UserName == user.UserName && u.UserId != id))
            {
                ModelState.AddModelError("UserName", "This username is already taken by another user.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
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

            ViewData["GenderId"] = new SelectList(_context.Genders, "GenderId", "GenderId", user.GenderId);
            ViewData["UserType"] = new SelectList(_context.UserTypes, "TypeId", "TypeId", user.UserType);
            return View(user);
        }

        // GET: Users/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.Include(u => u.Gender).Include(u => u.UserTypeNavigation).FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.Include(u => u.Carts).Include(u => u.Rates).Include(u => u.Sales).FirstOrDefaultAsync(u => u.UserId == id);

            if (user != null)
            {
                // Remove related entities first
                if (user.Carts.Any())
                    _context.Carts.RemoveRange(user.Carts);

                if (user.Rates.Any())
                    _context.Rates.RemoveRange(user.Rates);

                if (user.Sales.Any())
                    _context.Sales.RemoveRange(user.Sales);

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

    }
}
