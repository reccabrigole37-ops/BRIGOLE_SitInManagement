using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BRIGOLE_SitInManagement.Data;
using BRIGOLE_SitInManagement.Models;

namespace BRIGOLE_SitInManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseService _db;

        public AccountController(DatabaseService db) => _db = db;

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToDashboard();

            return View(new LoginViewModel()); // ← make sure this has "new"
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.IsAdmin)
            {
                var admin = _db.GetAdminByUsername(model.IdNumber);
                if (admin == null || !BCrypt.Net.BCrypt.Verify(model.Password, admin.PasswordHash))
                {
                    ModelState.AddModelError("", "Invalid admin credentials.");
                    return View(model);
                }

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                    new(ClaimTypes.Name, admin.FullName),
                    new(ClaimTypes.Role, "Admin"),
                    new("Username", admin.Username)
                };
                await SignIn(claims, model.RememberMe);
                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                var student = _db.GetStudentByIdNumber(model.IdNumber);
                if (student == null || !BCrypt.Net.BCrypt.Verify(model.Password, student.PasswordHash))
                {
                    ModelState.AddModelError("", "Invalid ID Number or password.");
                    return View(model);
                }

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, student.Id.ToString()),
                    new(ClaimTypes.Name, student.FullName),
                    new(ClaimTypes.Role, "Student"),
                    new("IdNumber", student.IdNumber)
                };
                await SignIn(claims, model.RememberMe);
                return RedirectToAction("Dashboard", "Student");
            }
        }

        private async Task SignIn(List<Claim> claims, bool persistent)
        {
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var props = new AuthenticationProperties { IsPersistent = persistent };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (_db.IdNumberExists(model.IdNumber))
            {
                ModelState.AddModelError("IdNumber", "ID Number already registered.");
                return View(model);
            }

            var student = new Student
            {
                IdNumber = model.IdNumber,
                LastName = model.LastName,
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                CourseLevel = model.CourseLevel,
                Course = model.Course,
                Email = model.Email,
                Address = model.Address,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };
            _db.CreateStudent(student);
            TempData["Success"] = "Registration successful! You can now log in.";
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();

        private IActionResult RedirectToDashboard()
        {
            if (User.IsInRole("Admin")) return RedirectToAction("Dashboard", "Admin");
            return RedirectToAction("Dashboard", "Student");
        }
    }
}
