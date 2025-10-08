using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBSS.Data; // Namespace where OBSSContext is located
using OBSS.Models;

namespace OBSS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly OBSSContext _context;

        public HomeController(ILogger<HomeController> logger, OBSSContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Welcome()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.Include(b => b.Category).Include(b => b.Rates).Where(b => b.QuantityInStore > 0).OrderByDescending(b => b.Rates.Any() ? b.Rates.Average(r => r.Rate1) : 0).ToListAsync();
            return View(books);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult aboutUs()
        {
            return View();
        }

        public IActionResult Careers()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
