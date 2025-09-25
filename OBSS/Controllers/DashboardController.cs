using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OBSS.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        [Authorize(Roles = "Customer")]
        public IActionResult CustomerDashboard()
        {
            return View();
        }
    }
}
