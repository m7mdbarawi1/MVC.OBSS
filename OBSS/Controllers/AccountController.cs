using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
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

        [AllowAnonymous] // Anyone can access the login page
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken] // Form submission for new users, CSRF protected
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            var user = await _context.Users.Include(u => u.UserTypeNavigation).FirstOrDefaultAsync(u => u.UserName == username && u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            // Build claims
            var displayName = $"{user.FirstName} {user.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(displayName)) displayName = user.UserName;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, displayName),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim("UserTypeId", user.UserType.ToString())
            };

            if (!string.IsNullOrEmpty(user.UserTypeNavigation?.TypeDesc))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.UserTypeNavigation.TypeDesc));
            }

            var identity = new ClaimsIdentity(claims, "OBSSAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("OBSSAuth", principal, new AuthenticationProperties
            {
                IsPersistent = true
            });

            // Role-based redirection
            if (user.UserTypeNavigation?.TypeDesc == "Admin")
                return RedirectToAction("AdminDashboard", "Dashboard");

            if (user.UserTypeNavigation?.TypeDesc == "Customer")
                return RedirectToAction("CustomerDashboard", "Dashboard");

            // Fallback
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [Authorize] // Only authenticated users can log out
        [HttpPost, ValidateAntiForgeryToken] // CSRF protection
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("OBSSAuth");
            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied() => View();

        // GET: /Account/Register
        [AllowAnonymous] // Anyone can see the registration page
        public IActionResult Register()
        {
            ViewBag.UserTypes = _context.UserTypes.ToList();

            ViewBag.Genders = _context.Genders.ToList();

            return View();
        }

        // POST: /Account/Register
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User model)
        {
            // Username unique
            if (_context.Users.Any(u => u.UserName == model.UserName))
            {
                ModelState.AddModelError("UserName", "Username already taken.");
            }

            // Email unique
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already registered.");
            }

            // Password validation
            if (string.IsNullOrEmpty(model.Password) || model.Password.Length < 8)
            {
                ModelState.AddModelError("Password", "Password must be at least 8 characters long.");
            }

            // Contact number validation (exactly 10 digits)
            if (!string.IsNullOrEmpty(model.ContactNumber))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.ContactNumber, @"^\d{10}$"))
                {
                    ModelState.AddModelError("ContactNumber", "Contact number must be exactly 10 digits.");
                }
            }

            // Age validation: must be at least 18
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (model.Birthdate > today.AddYears(-18))
            {
                ModelState.AddModelError("Birthdate", "You must be at least 18 years old.");
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
                Password = model.Password, // still plain text (only validation required)
                FirstName = model.FirstName,
                LastName = model.LastName,
                Birthdate = model.Birthdate,
                GenderId = model.GenderId,
                ContactNumber = model.ContactNumber,
                Email = model.Email
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Get role name
            var role = await _context.UserTypes
                .Where(t => t.TypeId == user.UserType)
                .Select(t => t.TypeDesc)
                .FirstOrDefaultAsync();

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim("UserTypeId", user.UserType.ToString())
    };

            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "OBSSAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("OBSSAuth", principal, new AuthenticationProperties
            {
                IsPersistent = true
            });

            // Role-based redirection
            if (role == "Admin")
                return RedirectToAction("AdminDashboard", "Dashboard");

            if (role == "Customer")
                return RedirectToAction("CustomerDashboard", "Dashboard");

            return RedirectToAction("Index", "Home");
        }


        [Authorize] // Only logged-in users can view their profile
        public async Task<IActionResult> MyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return RedirectToAction("Login");

            var user = await _context.Users.Include(u => u.Gender).Include(u => u.UserTypeNavigation).FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [AllowAnonymous]
        public IActionResult HomeRedirect()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("AdminDashboard", "Dashboard");

                if (User.IsInRole("Customer"))
                    return RedirectToAction("CustomerDashboard", "Dashboard");
            }

            return RedirectToAction("Welcome", "Home");
        }

        [Authorize] // Only logged-in users can update their profile
        [HttpPost, ValidateAntiForgeryToken] // CSRF protection
        public async Task<IActionResult> MyProfile(User model)
        {
            // Password length check
            if (!string.IsNullOrEmpty(model.Password) && model.Password.Length < 8)
            {
                ModelState.AddModelError("Password", "Password must be at least 8 characters long.");
            }

            // Contact number format check (only digits, exactly 10)
            if (!string.IsNullOrEmpty(model.ContactNumber))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.ContactNumber, @"^\d{10}$"))
                {
                    ModelState.AddModelError("ContactNumber", "Contact number must be exactly 10 digits.");
                }
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login");
            }

            // ✅ Check if username already exists (but ignore current user)
            bool usernameTaken = await _context.Users
                .AnyAsync(u => u.UserName == model.UserName && u.UserId != userId);

            if (usernameTaken)
            {
                ModelState.AddModelError("UserName", "This username is already taken.");
            }

            // ✅ Check if email already exists (optional, like in Register)
            bool emailTaken = await _context.Users
                .AnyAsync(u => u.Email == model.Email && u.UserId != userId);

            if (emailTaken)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Update only allowed fields
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UserName = model.UserName;
            user.Password = model.Password;
            user.Email = model.Email;
            user.ContactNumber = model.ContactNumber;

            _context.Update(user);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Profile updated successfully!";
            return View(user);
        }

        [Authorize] // Only logged-in users can delete their account
        [HttpPost, ValidateAntiForgeryToken] // CSRF protection
        public async Task<IActionResult> DeleteMyAccount()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // Logout after deleting
            await HttpContext.SignOutAsync("OBSSAuth");

            return RedirectToAction("Login", "Account");
        }

    }
}
