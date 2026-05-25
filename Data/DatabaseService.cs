using Npgsql;
using Dapper;
using BRIGOLE_SitInManagement.Models;

namespace BRIGOLE_SitInManagement.Data
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new Exception("No connection string found!");
        }

        public NpgsqlConnection GetConnection() =>
     new NpgsqlConnection(_connectionString);

        public void InitializeDatabase()
        {
            using var conn = GetConnection();
            conn.Open();

            conn.Execute(@"
                CREATE TABLE IF NOT EXISTS Admins (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    FullName TEXT NOT NULL DEFAULT 'Administrator'
                );

                CREATE TABLE IF NOT EXISTS Students (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    IdNumber TEXT NOT NULL UNIQUE,
                    LastName TEXT NOT NULL,
                    FirstName TEXT NOT NULL,
                    MiddleName TEXT,
                    CourseLevel INTEGER DEFAULT 1,
                    Course TEXT DEFAULT 'BSIT',
                    Email TEXT,
                    Address TEXT,
                    PasswordHash TEXT NOT NULL,
                    RemainingSession INTEGER DEFAULT 30,
                    Points INTEGER DEFAULT 0,
                    CreatedAt TEXT DEFAULT (datetime('now'))
                );

                CREATE TABLE IF NOT EXISTS SitInRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId INTEGER NOT NULL,
                    StudentIdNumber TEXT NOT NULL,
                    StudentName TEXT NOT NULL,
                    Course TEXT,
                    PcNumber INTEGER NOT NULL,
                    Purpose TEXT NOT NULL,
                    TimeIn TEXT NOT NULL,
                    TimeOut TEXT,
                    IsActive INTEGER DEFAULT 1,
                    Feedback TEXT,
                    FOREIGN KEY (StudentId) REFERENCES Students(Id)
                );

                CREATE TABLE IF NOT EXISTS Announcements (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Content TEXT NOT NULL,
                    CreatedAt TEXT DEFAULT (datetime('now')),
                    CreatedBy TEXT DEFAULT 'Admin'
                );

                CREATE TABLE IF NOT EXISTS Reservations (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId INTEGER NOT NULL,
                    StudentIdNumber TEXT NOT NULL,
                    StudentName TEXT NOT NULL,
                    PcNumber INTEGER NOT NULL,
                    Purpose TEXT NOT NULL,
                    ReservationDate TEXT NOT NULL,
                    Status TEXT DEFAULT 'Pending',
                    AdminNote TEXT,
                    FOREIGN KEY (StudentId) REFERENCES Students(Id)
                );
            ");

            // Seed default admin
            var adminExists = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Admins WHERE Username='admin'");
            if (adminExists == 0)
            {
                var hash = BCrypt.Net.BCrypt.HashPassword("admin123");
                conn.Execute("INSERT INTO Admins (Username, PasswordHash, FullName) VALUES ('admin', @Hash, 'System Administrator')", new { Hash = hash });
            }
        }

        // Student methods
        public Student? GetStudentByIdNumber(string idNumber)
        {
            using var conn = GetConnection();
            return conn.QueryFirstOrDefault<Student>("SELECT * FROM Students WHERE IdNumber=@IdNumber", new { IdNumber = idNumber });
        }

        public Student? GetStudentById(int id)
        {
            using var conn = GetConnection();
            return conn.QueryFirstOrDefault<Student>("SELECT * FROM Students WHERE Id=@Id", new { Id = id });
        }

        public List<Student> GetAllStudents()
        {
            using var conn = GetConnection();
            return conn.Query<Student>("SELECT * FROM Students ORDER BY LastName").ToList();
        }

        public bool IdNumberExists(string idNumber)
        {
            using var conn = GetConnection();
            return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Students WHERE IdNumber=@Id", new { Id = idNumber }) > 0;
        }

        public void CreateStudent(Student s)
        {
            using var conn = GetConnection();
            conn.Execute(@"INSERT INTO Students (IdNumber,LastName,FirstName,MiddleName,CourseLevel,Course,Email,Address,PasswordHash)
                VALUES (@IdNumber,@LastName,@FirstName,@MiddleName,@CourseLevel,@Course,@Email,@Address,@PasswordHash)", s);
        }

        public void UpdateStudent(Student s)
        {
            using var conn = GetConnection();
            conn.Execute(@"UPDATE Students SET LastName=@LastName,FirstName=@FirstName,MiddleName=@MiddleName,
                CourseLevel=@CourseLevel,Course=@Course,Email=@Email,Address=@Address WHERE Id=@Id", s);
        }

        public void AddPoints(int studentId, int points)
        {
            using var conn = GetConnection();
            conn.Execute("UPDATE Students SET Points=Points+@Points WHERE Id=@Id", new { Points = points, Id = studentId });
        }

        public void SetPoints(int studentId, int points)
        {
            using var conn = GetConnection();
            conn.Execute("UPDATE Students SET Points=@Points WHERE Id=@Id", new { Points = points, Id = studentId });
        }

        // SitIn methods
        public SitInRecord? GetActiveSitIn(string idNumber)
        {
            using var conn = GetConnection();
            return conn.QueryFirstOrDefault<SitInRecord>("SELECT * FROM SitInRecords WHERE StudentIdNumber=@Id AND IsActive=1", new { Id = idNumber });
        }

        public List<SitInRecord> GetCurrentSitIns()
        {
            using var conn = GetConnection();
            return conn.Query<SitInRecord>("SELECT * FROM SitInRecords WHERE IsActive=1 ORDER BY TimeIn DESC").ToList();
        }

        public List<SitInRecord> GetStudentSitInHistory(int studentId)
        {
            using var conn = GetConnection();
            return conn.Query<SitInRecord>("SELECT * FROM SitInRecords WHERE StudentId=@Id ORDER BY TimeIn DESC", new { Id = studentId }).ToList();
        }

        public List<SitInRecord> GetAllSitInRecords()
        {
            using var conn = GetConnection();
            return conn.Query<SitInRecord>("SELECT * FROM SitInRecords ORDER BY TimeIn DESC").ToList();
        }

        public void CreateSitIn(SitInRecord r)
        {
            using var conn = GetConnection();
            conn.Execute(@"INSERT INTO SitInRecords (StudentId,StudentIdNumber,StudentName,Course,PcNumber,Purpose,TimeIn,IsActive)
                VALUES (@StudentId,@StudentIdNumber,@StudentName,@Course,@PcNumber,@Purpose,@TimeIn,1)", r);
                    conn.Execute(
              "UPDATE Students SET RemainingSession = RemainingSession - 1 WHERE Id = @Id",
              new { Id = student.Id }
);
        }

        public void EndSitIn(int recordId, string? feedback = null)
        {
            using var conn = GetConnection();
            conn.Execute("UPDATE SitInRecords SET TimeOut=@TimeOut, IsActive=0, Feedback=@Feedback WHERE Id=@Id",
                new { TimeOut = DateTime.Now.ToString("o"), Feedback = feedback, Id = recordId });
        }

        public void EndSitInByStudentId(string idNumber, string? feedback = null)
        {
            using var conn = GetConnection();
            conn.Execute("UPDATE SitInRecords SET TimeOut=@TimeOut, IsActive=0, Feedback=@Feedback WHERE StudentIdNumber=@Id AND IsActive=1",
                new { TimeOut = DateTime.Now.ToString("o"), Feedback = feedback, Id = idNumber });
        }

        public int GetTotalSitInsToday()
        {
            using var conn = GetConnection();
            return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM SitInRecords WHERE date(TimeIn)=date('now')");
        }

        // Announcement methods
        public List<Announcement> GetAnnouncements()
        {
            using var conn = GetConnection();
            return conn.Query<Announcement>("SELECT * FROM Announcements ORDER BY CreatedAt DESC").ToList();
        }

        public void CreateAnnouncement(Announcement a)
        {
            using var conn = GetConnection();
            conn.Execute("INSERT INTO Announcements (Title,Content,CreatedBy) VALUES (@Title,@Content,@CreatedBy)", a);
        }

        public void DeleteAnnouncement(int id)
        {
            using var conn = GetConnection();
            conn.Execute("DELETE FROM Announcements WHERE Id=@Id", new { Id = id });
        }

        // Reservation methods
        public List<Reservation> GetStudentReservations(int studentId)
        {
            using var conn = GetConnection();
            return conn.Query<Reservation>("SELECT * FROM Reservations WHERE StudentId=@Id ORDER BY ReservationDate DESC", new { Id = studentId }).ToList();
        }

        public List<Reservation> GetAllReservations()
        {
            using var conn = GetConnection();
            return conn.Query<Reservation>("SELECT * FROM Reservations ORDER BY ReservationDate DESC").ToList();
        }

        public List<Reservation> GetPendingReservations()
        {
            using var conn = GetConnection();
            return conn.Query<Reservation>("SELECT * FROM Reservations WHERE Status='Pending' ORDER BY ReservationDate").ToList();
        }

        public void CreateReservation(Reservation r)
        {
            using var conn = GetConnection();
            conn.Execute(@"INSERT INTO Reservations (StudentId,StudentIdNumber,StudentName,PcNumber,Purpose,ReservationDate,Status)
                VALUES (@StudentId,@StudentIdNumber,@StudentName,@PcNumber,@Purpose,@ReservationDate,'Pending')", r);
        }

        public void UpdateReservationStatus(int id, string status, string? note = null)
        {
            using var conn = GetConnection();
            conn.Execute("UPDATE Reservations SET Status=@Status, AdminNote=@Note WHERE Id=@Id", new { Status = status, Note = note, Id = id });
        }

        // Admin methods
        public Admin? GetAdminByUsername(string username)
        {
            using var conn = GetConnection();
            return conn.QueryFirstOrDefault<Admin>("SELECT * FROM Admins WHERE Username=@Username", new { Username = username });
        }

        // Leaderboard
        public List<Student> GetLeaderboard()
        {
            using var conn = GetConnection();
            return conn.Query<Student>("SELECT * FROM Students ORDER BY Points DESC LIMIT 10").ToList();
        }
    }
}
