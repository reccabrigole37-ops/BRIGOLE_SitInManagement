using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BRIGOLE_SitInManagement.Data;
using BRIGOLE_SitInManagement.Models;

namespace BRIGOLE_SitInManagement.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly DatabaseService _db;

        public StudentController(DatabaseService db) => _db = db;

        private int GetStudentId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string GetIdNumber() => User.FindFirstValue("IdNumber")!;

        public IActionResult Dashboard()
        {
            var student = _db.GetStudentById(GetStudentId());
            var vm = new DashboardViewModel
            {
                Student = student,
                RecentSessions = _db.GetStudentSitInHistory(GetStudentId()).Take(5).ToList(),
                Announcements = _db.GetAnnouncements().Take(5).ToList(),
                Reservations = _db.GetStudentReservations(GetStudentId()).Take(3).ToList(),
                TotalSessions = _db.GetStudentSitInHistory(GetStudentId()).Count
            };
            return View(vm);
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            var s = _db.GetStudentById(GetStudentId())!;
            return View(s);
        }

        [HttpPost]
        public IActionResult EditProfile(Student model)
        {
            model.Id = GetStudentId();
            _db.UpdateStudent(model);
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Dashboard");
        }

        public IActionResult SitInHistory()
        {
            var records = _db.GetStudentSitInHistory(GetStudentId());
            return View(records);
        }

        [HttpGet]
        public IActionResult SubmitFeedback(int id)
        {
            var records = _db.GetStudentSitInHistory(GetStudentId());
            var record = records.FirstOrDefault(r => r.Id == id);
            if (record == null) return NotFound();
            return View(record);
        }

        [HttpPost]
        public IActionResult SubmitFeedback(int id, string feedback)
        {
            _db.EndSitIn(id, feedback);
            TempData["Success"] = "Feedback submitted!";
            return RedirectToAction("SitInHistory");
        }

        [HttpGet]
        public IActionResult Reservation() => View(new Reservation());

        [HttpPost]
        public IActionResult Reservation(Reservation model)
        {
            var student = _db.GetStudentById(GetStudentId())!;
            model.StudentId = student.Id;
            model.StudentIdNumber = student.IdNumber;
            model.StudentName = student.FullName;
            _db.CreateReservation(model);
            TempData["Success"] = "Reservation submitted for approval.";
            return RedirectToAction("MyReservations");
        }

        public IActionResult MyReservations()
        {
            var list = _db.GetStudentReservations(GetStudentId());
            return View(list);
        }

        public IActionResult Rewards()
        {
            var student = _db.GetStudentById(GetStudentId())!;
            var leaderboard = _db.GetLeaderboard();
            ViewBag.Leaderboard = leaderboard;
            return View(student);
        }

        public IActionResult Rules() => View();

        public IActionResult Notifications()
        {
            var announcements = _db.GetAnnouncements();
            return View(announcements);
        }
    }
}
