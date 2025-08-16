using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBSS.Data;
using OBSS.Models;

namespace OBSS.Controllers
{
    public class AccountController : Controller
    {
        private readonly OBSSContext _context;

        public AccountController(OBSSContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet, AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            var user = await _context.Users
                .Include(u => u.UserTypeNavigation)
                .FirstOrDefaultAsync(u => u.UserName == username && u.Password == password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View();
            }

            var displayName = $"{user.FirstName} {user.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(displayName)) displayName = user.UserName;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, displayName),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim("UserTypeId", user.UserType.ToString())
            };

            if (user.UserTypeNavigation != null && !string.IsNullOrWhiteSpace(user.UserTypeNavigation.TypeDesc))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.UserTypeNavigation.TypeDesc));
            }

            var identity = new ClaimsIdentity(claims, "OBSSAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("OBSSAuth", principal, new AuthenticationProperties
            {
                IsPersistent = true
            });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("OBSSAuth");
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        // GET: /Account/Register
        [HttpGet, AllowAnonymous]
        public IActionResult Register()
        {
            ViewBag.UserTypes = _context.UserTypes
                .Where(t => t.TypeId != 1)
                .ToList();

            ViewBag.Genders = _context.Genders.ToList();

            return View();
        }

        // POST: /Account/Register
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User model)
        {
            if (_context.Users.Any(u => u.UserName == model.UserName))
            {
                ModelState.AddModelError("UserName", "Username already taken.");
            }
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already registered.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.UserTypes = _context.UserTypes.Where(t => t.TypeId != 1).ToList();
                ViewBag.Genders = _context.Genders.ToList();
                return View(model);
            }

            var user = new User
            {
                UserType = model.UserType,
                UserName = model.UserName,
                Password = model.Password,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Birthdate = model.Birthdate,
                GenderId = model.GenderId,
                ContactNumber = model.ContactNumber,
                Email = model.Email
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim("UserTypeId", user.UserType.ToString())
            };

            var identity = new ClaimsIdentity(claims, "OBSSAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("OBSSAuth", principal, new AuthenticationProperties
            {
                IsPersistent = true
            });

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/MyProfile
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return RedirectToAction("Login");

            var user = await _context.Users
                .Include(u => u.Gender)
                .Include(u => u.UserTypeNavigation)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound();

            return View(user);
        }
    }
}
