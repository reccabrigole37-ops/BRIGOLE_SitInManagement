using System.ComponentModel.DataAnnotations;

namespace BRIGOLE_SitInManagement.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string IdNumber { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string MiddleName { get; set; } = "";
        public int CourseLevel { get; set; } = 1;
        public string Course { get; set; } = "BSIT";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public int RemainingSession { get; set; } = 30;
        public int Points { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string FullName => $"{FirstName} {MiddleName} {LastName}".Trim();
    }

    public class SitInRecord
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentIdNumber { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string Course { get; set; } = "";
        public int PcNumber { get; set; }
        public string Purpose { get; set; } = "";
        public DateTime TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Feedback { get; set; }
    }

    public class Announcement
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = "Admin";
    }

    public class Reservation
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentIdNumber { get; set; } = "";
        public string StudentName { get; set; } = "";
        public int PcNumber { get; set; }
        public string Purpose { get; set; } = "";
        public DateTime ReservationDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Denied
        public string? AdminNote { get; set; }
    }

    public class Admin
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string FullName { get; set; } = "Administrator";
    }

    // View Models
    public class LoginViewModel
    {
        [Required(ErrorMessage = "ID Number is required")]
        public string IdNumber { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
        public bool IsAdmin { get; set; } = false;
    }

    public class RegisterViewModel
    {
        [Required] public string IdNumber { get; set; } = "";
        [Required] public string LastName { get; set; } = "";
        [Required] public string FirstName { get; set; } = "";
        public string MiddleName { get; set; } = "";
        [Range(1, 5)] public int CourseLevel { get; set; } = 1;
        [Required] public string Course { get; set; } = "BSIT";
        [Required, EmailAddress] public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        [Required, MinLength(6)] public string Password { get; set; } = "";
        [Required, Compare("Password")] public string RepeatPassword { get; set; } = "";
    }

    public class SitInViewModel
    {
        [Required] public string IdNumber { get; set; } = "";
        [Required, Range(1, 50)] public int PcNumber { get; set; }
        [Required] public string Purpose { get; set; } = "";
    }

    public class DashboardViewModel
    {
        public Student? Student { get; set; }
        public List<SitInRecord> RecentSessions { get; set; } = new();
        public List<Announcement> Announcements { get; set; } = new();
        public List<Reservation> Reservations { get; set; } = new();
        public int TotalSessions { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int CurrentSitIns { get; set; }
        public int TotalSitInsToday { get; set; }
        public List<SitInRecord> CurrentSessions { get; set; } = new();
        public List<Announcement> Announcements { get; set; } = new();
        public List<Reservation> PendingReservations { get; set; } = new();
    }
}
