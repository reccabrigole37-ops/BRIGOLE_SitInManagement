using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using BRIGOLE_SitInManagement.Data;
using BRIGOLE_SitInManagement.Models;

namespace BRIGOLE_SitInManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly DatabaseService _db;

        public AdminController(DatabaseService db) => _db = db;

        public IActionResult Dashboard()
        {
            var vm = new AdminDashboardViewModel
            {
                TotalStudents = _db.GetAllStudents().Count,
                CurrentSitIns = _db.GetCurrentSitIns().Count,
                TotalSitInsToday = _db.GetTotalSitInsToday(),
                CurrentSessions = _db.GetCurrentSitIns(),
                Announcements = _db.GetAnnouncements().Take(5).ToList(),
                PendingReservations = _db.GetPendingReservations()
            };
            return View(vm);
        }

        public IActionResult StudentList(string? search)
        {
            var students = string.IsNullOrWhiteSpace(search)
                ? _db.GetAllStudents()
                : _db.GetAllStudents().Where(s =>
                    s.IdNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.Course.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.Search = search;
            return View(students);
        }

        public IActionResult StudentInfo(int id)
        {
            var student = _db.GetStudentById(id);
            if (student == null) return NotFound();
            var history = _db.GetStudentSitInHistory(id);
            ViewBag.History = history;
            return View(student);
        }

        [HttpPost]
        public IActionResult SitInStudent(SitInViewModel model)
        {
            var student = _db.GetStudentByIdNumber(model.IdNumber);
            if (student == null)
            {
                TempData["Error"] = "Student not found.";
                return RedirectToAction("Dashboard");
            }
            if (student.RemainingSession <= 0)
            {
                TempData["Error"] = "Student has no remaining sessions.";
                return RedirectToAction("Dashboard");
            }
            var existing = _db.GetActiveSitIn(model.IdNumber);
            if (existing != null)
            {
                TempData["Error"] = "Student is already sitting in.";
                return RedirectToAction("Dashboard");
            }

            var record = new SitInRecord
            {
                StudentId = student.Id,
                StudentIdNumber = student.IdNumber,
                StudentName = student.FullName,
                Course = student.Course,
                PcNumber = model.PcNumber,
                Purpose = model.Purpose,
                TimeIn = DateTime.Now
            };
            _db.CreateSitIn(record);
            TempData["Success"] = $"{student.FullName} checked in to PC #{model.PcNumber}.";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public IActionResult EndSitIn(int id)
        {
            _db.EndSitIn(id);
            TempData["Success"] = "Sit-in session ended.";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public IActionResult EndAllSessions()
        {
            var active = _db.GetCurrentSitIns();
            foreach (var s in active) _db.EndSitIn(s.Id);
            TempData["Success"] = $"Ended {active.Count} active session(s).";
            return RedirectToAction("Dashboard");
        }

        public IActionResult SitInRecords(string? search)
        {
            var records = _db.GetAllSitInRecords();
            if (!string.IsNullOrWhiteSpace(search))
                records = records.Where(r => r.StudentIdNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || r.StudentName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.Search = search;
            return View(records);
        }

        [HttpGet]
        public IActionResult Announcements() => View(_db.GetAnnouncements());

        [HttpPost]
        public IActionResult CreateAnnouncement(string title, string content)
        {
            _db.CreateAnnouncement(new Announcement { Title = title, Content = content });
            TempData["Success"] = "Announcement posted!";
            return RedirectToAction("Announcements");
        }

        [HttpPost]
        public IActionResult DeleteAnnouncement(int id)
        {
            _db.DeleteAnnouncement(id);
            TempData["Success"] = "Announcement deleted.";
            return RedirectToAction("Announcements");
        }

        public IActionResult Reservations() => View(_db.GetAllReservations());

        [HttpPost]
        public IActionResult ApproveReservation(int id)
        {
            _db.UpdateReservationStatus(id, "Approved");
            TempData["Success"] = "Reservation approved.";
            return RedirectToAction("Reservations");
        }

        [HttpPost]
        public IActionResult DenyReservation(int id, string? note)
        {
            _db.UpdateReservationStatus(id, "Denied", note);
            TempData["Success"] = "Reservation denied.";
            return RedirectToAction("Reservations");
        }

        [HttpPost]
        public IActionResult AddPoints(int studentId, int points)
        {
            _db.AddPoints(studentId, points);
            TempData["Success"] = "Points updated.";
            return RedirectToAction("StudentInfo", new { id = studentId });
        }

        public IActionResult Leaderboard() => View(_db.GetLeaderboard());

        public IActionResult Analytics()
        {
            var records = _db.GetAllSitInRecords();
            ViewBag.TotalRecords = records.Count;
            ViewBag.TodayRecords = records.Count(r => r.TimeIn.Date == DateTime.Today);
            ViewBag.WeekRecords = records.Count(r => r.TimeIn >= DateTime.Today.AddDays(-7));
            var byPurpose = records.GroupBy(r => r.Purpose)
                .Select(g => new { Purpose = g.Key, Count = g.Count() }).ToList();
            ViewBag.ByPurpose = byPurpose;
            var students = _db.GetAllStudents();
            ViewBag.TotalStudents = students.Count;
            return View(records);
        }

        public IActionResult ExportCsv()
        {
            var records = _db.GetAllSitInRecords();
            var sb = new StringBuilder();
            sb.AppendLine("ID,StudentIDNumber,StudentName,Course,PC,Purpose,TimeIn,TimeOut");
            foreach (var r in records)
                sb.AppendLine($"{r.Id},{r.StudentIdNumber},{r.StudentName},{r.Course},{r.PcNumber},{r.Purpose},{r.TimeIn:yyyy-MM-dd HH:mm},{r.TimeOut?.ToString("yyyy-MM-dd HH:mm") ?? "Active"}");
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "sitin_records.csv");
        }

        public IActionResult Feedback()
        {
            var records = _db.GetAllSitInRecords().Where(r => !string.IsNullOrEmpty(r.Feedback)).ToList();
            return View(records);
        }
    }
}
