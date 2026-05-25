# BRIGOLE_SitInManagement
## CCS Sit-In Monitoring System — ASP.NET Core MVC (.NET 10)

---

## 📋 Project Overview

A web-based sit-in monitoring system for the **College of Computer Studies** built with ASP.NET Core MVC 10, SQLite, and Dapper.

---

## 🚀 Setup Instructions (Visual Studio 2022)

### Prerequisites
- Visual Studio 2022 (v17.12+)
- .NET 10 SDK
- No SQL Server required (uses SQLite)

### Steps

1. **Open in Visual Studio 2022**
   - Open Visual Studio 2022
   - File → Open → Project/Solution
   - Navigate to `BRIGOLE_SitInManagement.csproj`

2. **Restore NuGet Packages**
   - Right-click the solution → Restore NuGet Packages
   - Or: Tools → NuGet Package Manager → Package Manager Console → `dotnet restore`

3. **Run the Project**
   - Press `F5` or `Ctrl+F5`
   - The database (`sitin.db`) is auto-created on first run in the project root
   - Navigate to `https://localhost:{port}`

---

## 🔑 Default Login Credentials

### Admin
- **Username:** `admin`
- **Password:** `admin123`
- Check "Admin" toggle on the login page

### Students
- Register via the Register page
- Use your chosen ID Number and password

---

## 📁 Project Structure

```
BRIGOLE_SitInManagement/
├── Controllers/
│   ├── HomeController.cs          # Public pages
│   ├── AccountController.cs       # Login, Register, Logout
│   ├── StudentController.cs       # Student dashboard & features
│   └── AdminController.cs         # Admin dashboard & management
├── Models/
│   └── Models.cs                  # All data models & ViewModels
├── Data/
│   └── DatabaseService.cs         # SQLite + Dapper data access
├── Views/
│   ├── Shared/_Layout.cshtml      # Main layout with navbar
│   ├── Home/                      # Index, Community, About
│   ├── Account/                   # Login, Register
│   ├── Student/                   # All student views
│   └── Admin/                     # All admin views
├── wwwroot/
│   ├── css/site.css               # CCS purple/gold theme
│   ├── js/site.js
│   └── images/                    # Place ccs-logo.png here
├── Program.cs                     # App entry, DI, middleware
└── sitin.db                       # Auto-generated SQLite database
```

---

## ✨ Features

### Student Features
| Feature | Description |
|---|---|
| Dashboard | View sessions remaining, total sit-ins, points |
| Edit Profile | Update personal info |
| Sit-in History | Full session log with feedback option |
| Reservation | Reserve a PC with date/purpose |
| My Reservations | View reservation status |
| Rewards & Points | View points, leaderboard ranking |
| Rules & Regulations | Lab policies page |
| Announcements | View admin announcements |

### Admin Features
| Feature | Description |
|---|---|
| Dashboard | Live stats, quick sit-in form, current sessions |
| Sit-in Records | Full searchable session history |
| Student List | Searchable student directory with profile view |
| Student Info | Per-student history + add points |
| Announcements | Create/delete announcements |
| Reservations | Approve/deny student reservations |
| Leaderboard | Top students by points |
| Analytics | Usage stats by purpose with progress bars |
| Feedback | View submitted student feedback |
| Export CSV | Download all sit-in records |

---

## 🎨 Design

- **Colors:** CCS Purple `#4a1a7c` + Gold `#f0a500`
- **Framework:** Bootstrap 5.3
- **Icons:** Font Awesome 6.5
- **Layout:** Sidebar navigation for dashboards
- **Database:** SQLite (file-based, zero config)

---

## 📦 NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| Microsoft.Data.Sqlite | 8.0.0 | SQLite database |
| Dapper | 2.1.35 | Micro-ORM |
| BCrypt.Net-Next | 4.0.3 | Password hashing |
| Microsoft.AspNetCore.Authentication.Cookies | 2.2.0 | Cookie auth |

---

## 💡 Optional: Add CCS Logo

Place a `ccs-logo.png` file in `wwwroot/images/` to display the official CCS logo in the navbar and login/register pages.

---

## 📝 Notes

- Sessions deduct automatically when Admin checks in a student
- Admin default password should be changed in production
- The `sitin.db` file is created in the project working directory
- No migrations needed — schema is created on startup via `InitializeDatabase()`
