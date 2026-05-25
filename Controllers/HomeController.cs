using Microsoft.AspNetCore.Mvc;
using BRIGOLE_SitInManagement.Data;

namespace BRIGOLE_SitInManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseService _db;
        public HomeController(DatabaseService db) => _db = db;

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin")) return RedirectToAction("Dashboard", "Admin");
                return RedirectToAction("Dashboard", "Student");
            }
            ViewBag.Announcements = _db.GetAnnouncements().Take(3).ToList();
            return View();
        }

        public IActionResult Community()
        {
            ViewBag.Leaderboard = _db.GetLeaderboard();
            ViewBag.Announcements = _db.GetAnnouncements();
            return View();
        }

        public IActionResult About() => View();

        public IActionResult Error() => View();
    }
}
