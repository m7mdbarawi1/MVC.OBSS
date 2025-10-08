using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OBSS.Data;
using OBSS.Models;

namespace OBSS.Controllers
{
    public class UserTypesController : Controller
    {
        private readonly OBSSContext _context;

        public UserTypesController(OBSSContext context)
        {
            _context = context;
        }

        // GET: UserTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.UserTypes.ToListAsync());
        }

        // GET: UserTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userType = await _context.UserTypes.FirstOrDefaultAsync(m => m.TypeId == id);

            if (userType == null)
            {
                return NotFound();
            }

            return View(userType);
        }

        // GET: UserTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TypeDesc")] UserType userType) // 🚀 Removed TypeId
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(userType);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    // Handle duplicate TypeDesc gracefully (since you made it UNIQUE in DB)
                    ModelState.AddModelError("", "That user type already exists.");
                }
            }
            return View(userType);
        }

        // GET: UserTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userType = await _context.UserTypes.FindAsync(id);

            if (userType == null)
            {
                return NotFound();
            }

            return View(userType);
        }

        // POST: UserTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TypeId,TypeDesc")] UserType userType)
        {
            if (id != userType.TypeId)
                return NotFound();

            if (ModelState.IsValid)
            {
                // ✅ Check for duplicate TypeDesc before saving
                if (_context.UserTypes.Any(u => u.TypeDesc == userType.TypeDesc && u.TypeId != userType.TypeId))
                {
                    ModelState.AddModelError("TypeDesc", "This type description already exists.");
                    return View(userType);
                }

                try
                {
                    _context.Update(userType);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserTypeExists(userType.TypeId))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(userType);
        }

        // GET: UserTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userType = await _context.UserTypes.FirstOrDefaultAsync(m => m.TypeId == id);

            if (userType == null)
            {
                return NotFound();
            }

            return View(userType);
        }

        // POST: UserTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userType = await _context.UserTypes.FindAsync(id);
            if (userType != null)
            {
                _context.UserTypes.Remove(userType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserTypeExists(int id)
        {
            return _context.UserTypes.Any(e => e.TypeId == id);
        }
    }
}
