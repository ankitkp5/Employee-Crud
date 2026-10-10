1. Application ka final structure
Frontend
Razor Views, HTML, CSS, Bootstrap
Employee List, Add, Edit, Details, Delete
Backend
C#, ASP.NET Core MVC Controllers, Entity Framework Core
Validation aur database operations
Database
Microsoft SQL Server
Employees table mein permanent data storage
CRUD features
Operation	Function
Create	Naya employee add karna
Read	Employee list aur details dekhna
Update	Existing employee edit karna
Delete	Employee record delete karna
Validation	Required fields aur valid email check karna
Search	Name ya department se employees search karna
2. Step 1 — Visual Studio mein project create karein
1.	Visual Studio 2022 open karein.
2.	Create a new project select karein.
3.	Search karein: ASP.NET Core Web App (Model-View-Controller).
4.	Project name CrudApplication rakhein.
5.	Framework mein .NET 8.0 (LTS) select karein, agar installed hai. .NET 8 use karne par uske compatible packages install karein.
6.	Authentication Type None rakhein aur HTTPS enable rakhein.
7.	Create par click karein.
Agar MVC template nahi dikh raha, Visual Studio Installer kholkar ASP.NET and web development workload install karein.
Step 2 — SQL Server ready karein
Aapko SQL Server Database Engine aur Visual Studio mein Package Manager Console chahiye. SQL Server Management Studio (SSMS) database check karne ke liye optional but useful hai.
Local SQL Server instance ke liye aam taur par ye connection strings use hoti hain:
SQL Server setup	Server name
SQL Server Express	.\SQLEXPRESS
Default local instance	.
LocalDB	(localdb)\MSSQLLocalDB
Neeche main SQL Server Express (.\SQLEXPRESS) use kar raha hoon. Agar aapke system par instance ka naam alag hai, connection string mein use replace karna hoga.
3. Project ki files
Solution Explorer mein ye structure banayenge. MVC template ki existing files ko delete karne ki zaroorat nahi; relevant files add ya replace karein.
CrudApplication
│
├── Controllers
│   └── EmployeesController.cs
│
├── Data
│   └── ApplicationDbContext.cs
│
├── Models
│   └── Employee.cs
│
├── Views
│   ├── Employees
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   ├── Details.cshtml
│   │   └── Delete.cshtml
│   │
│   └── Shared
│       └── _Layout.cshtml
│
├── appsettings.json
└── Program.cs
Step 3 — Required NuGet packages install karein
Visual Studio mein Tools → NuGet Package Manager → Package Manager Console open karein aur commands run karein.
Install-Package Microsoft.EntityFrameworkCore.SqlServer -Version 8.0.0
Install-Package Microsoft.EntityFrameworkCore.Tools -Version 8.0.0
Ye commands tab use karein jab project target framework .NET 8.0 ho. Production use ke liye same EF Core 8 patch version par compatible packages update rakhna behtar hai.
Step 4 — Employee model banayein
Models folder par right-click → Add → Class → Employee.cs.

using System.ComponentModel.DataAnnotations;

namespace CrudApplication.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Employee name is required")]
        [StringLength(100)]
        [Display(Name = "Employee Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Enter a valid phone number")]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;
    }
}
Is model ka kaam: C# properties ko database columns se map karna aur form inputs ki validation define karna.
Step 5 — Database context banayein
Data naam ka folder add karein. Usmein ApplicationDbContext.cs create karein.

using CrudApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace CrudApplication.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
    }
}
DbSet<Employee> Entity Framework Core ko batata hai ki database mein Employees table manage karni hai.
Step 6 — SQL Server connection configure karein
appsettings.json mein existing configuration ko preserve karte hue ConnectionStrings add karein.

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=CrudApplicationDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
Agar aap default SQL Server instance use kar rahe hain, Server=. rakhein. LocalDB ke liye Server=(localdb)\\MSSQLLocalDB use karein.
TrustServerCertificate=True local development ke liye hai; production mein proper SQL Server TLS certificate configuration use karein.
Step 7 — Program.cs configure karein
Project ke Program.cs mein ye code rakhein:

using CrudApplication.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Employees}/{action=Index}/{id?}");

app.Run();
Ab application start hone par default page Employees ki list hogi.
Step 8 — EmployeesController banayein
Controllers folder mein EmployeesController.cs create karein.
Is controller mein Create, Read, Update, Delete aur Search ke actions hain.

using CrudApplication.Data;
using CrudApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudApplication.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // READ + SEARCH
        public async Task<IActionResult> Index(string? searchString)
        {
            var employees = _context.Employees.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                employees = employees.Where(e =>
                    e.Name.Contains(searchString) ||
                    e.Department.Contains(searchString));
            }

            ViewData["SearchString"] = searchString;

            return View(await employees
                .OrderBy(e => e.Name)
                .ToListAsync());
        }

        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        // CREATE - GET
        public IActionResult Create()
        {
            return View();
        }

        // CREATE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (!ModelState.IsValid)
                return View(employee);

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Employee added successfully.";

            return RedirectToAction(nameof(Index));
        }

        // EDIT - GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .FindAsync(id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        // EDIT - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, Employee employee)
        {
            if (id != employee.EmployeeId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(employee);

            var existingEmployee = await _context.Employees
                .FindAsync(id);

            if (existingEmployee == null)
                return NotFound();

            existingEmployee.Name = employee.Name;
            existingEmployee.Email = employee.Email;
            existingEmployee.Phone = employee.Phone;
            existingEmployee.Department = employee.Department;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Employee updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // DELETE - GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        // DELETE - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null) return NotFound();

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Employee deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
Important: Edit action existing employee ko fetch karke sirf allowed fields update karta hai. Isse form se aane wale EmployeeId ke alawa unintended fields ko overwrite karne ka risk kam hota hai.
4. Frontend — Bootstrap UI
Views/Shared/_Layout.cshtml open karein aur uska content replace karein. Isse sabhi pages par common navigation aur Bootstrap styling milegi.

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>@ViewData["Title"] - Employee Management</title>

    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"
          rel="stylesheet" />

    <style>
        body {
            background: #f4f7fb;
            color: #172033;
        }

        .navbar {
            background: #172554;
        }

        .page-card {
            background: white;
            border: 0;
            border-radius: 14px;
            box-shadow: 0 5px 24px rgba(20, 40, 80, .07);
        }

        .btn {
            border-radius: 7px;
        }

        .table thead th {
            background: #eef2ff;
            color: #24345c;
            white-space: nowrap;
        }

        .form-control {
            padding: 10px 12px;
        }
    </style>
</head>
<body>
    <nav class="navbar navbar-dark navbar-expand-lg">
        <div class="container">
            <a class="navbar-brand fw-bold"
               asp-controller="Employees" asp-action="Index">
                Employee Management
            </a>
            <a class="btn btn-light btn-sm"
               asp-controller="Employees" asp-action="Create">
                + Add Employee
            </a>
        </div>
    </nav>

    <main class="container py-4 py-md-5">
        @RenderBody()
    </main>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
Step 9 — Employee list aur search page
Views/Employees folder create karein. Usmein Index.cshtml add karein.

@model IEnumerable<CrudApplication.Models.Employee>

@{
    ViewData["Title"] = "Employees";
}

<div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
    <div>
        <h2 class="fw-bold mb-1">Employees</h2>
        <p class="text-secondary mb-0">Manage your employee records</p>
    </div>
    <a asp-action="Create" class="btn btn-primary px-4">
        + Add Employee
    </a>
</div>

@if (TempData["Success"] != null)
{
    <div class="alert alert-success alert-dismissible fade show">
        @TempData["Success"]
        <button type="button" class="btn-close"
                data-bs-dismiss="alert"></button>
    </div>
}

<div class="page-card p-3 p-md-4">
    <form asp-action="Index" method="get"
          class="row g-2 mb-4">
        <div class="col-md-9">
            <input type="text" name="searchString"
                   value="@ViewData["SearchString"]"
                   class="form-control"
                   placeholder="Search by name or department..." />
        </div>
        <div class="col-md-3 d-flex gap-2">
            <button class="btn btn-primary flex-fill" type="submit">
                Search
            </button>
            <a asp-action="Index"
               class="btn btn-outline-secondary">Reset</a>
        </div>
    </form>

    <div class="table-responsive">
        <table class="table table-hover align-middle">
            <thead>
                <tr>
                    <th>ID</th>
                    <th>Name</th>
                    <th>Email</th>
                    <th>Phone</th>
                    <th>Department</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody>
            @foreach (var item in Model)
            {
                <tr>
                    <td>@item.EmployeeId</td>
                    <td class="fw-semibold">@item.Name</td>
                    <td>@item.Email</td>
                    <td>@(item.Phone ?? "-")</td>
                    <td>
                        <span class="badge text-bg-light">
                            @item.Department
                        </span>
                    </td>
                    <td>
                        <div class="d-flex gap-1">
                            <a asp-action="Details"
                               asp-route-id="@item.EmployeeId"
                               class="btn btn-sm btn-outline-info">View</a>
                            <a asp-action="Edit"
                               asp-route-id="@item.EmployeeId"
                               class="btn btn-sm btn-outline-primary">Edit</a>
                            <a asp-action="Delete"
                               asp-route-id="@item.EmployeeId"
                               class="btn btn-sm btn-outline-danger">Delete</a>
                        </div>
                    </td>
                </tr>
            }
            @if (!Model.Any())
            {
                <tr>
                    <td colspan="6" class="text-center py-5 text-secondary">
                        No employees found. Add your first employee.
                    </td>
                </tr>
            }
            </tbody>
        </table>
    </div>

    <div class="text-secondary small">
        Total employees displayed: @Model.Count()
    </div>
</div>
Step 10 — Create Employee page
Views/Employees/Create.cshtml file banayein.

@model CrudApplication.Models.Employee

@{
    ViewData["Title"] = "Add Employee";
}

<div class="row justify-content-center">
    <div class="col-lg-7">
        <div class="page-card p-4 p-md-5">
            <h2 class="fw-bold">Add Employee</h2>
            <p class="text-secondary">Enter the employee details below.</p>

            <form asp-action="Create" method="post">
                <div asp-validation-summary="ModelOnly"
                     class="text-danger mb-3"></div>

                <div class="mb-3">
                    <label asp-for="Name" class="form-label"></label>
                    <input asp-for="Name" class="form-control"
                           placeholder="Enter full name" />
                    <span asp-validation-for="Name"
                          class="text-danger"></span>
                </div>

                <div class="mb-3">
                    <label asp-for="Email" class="form-label"></label>
                    <input asp-for="Email" class="form-control"
                           placeholder="name@example.com" />
                    <span asp-validation-for="Email"
                          class="text-danger"></span>
                </div>

                <div class="mb-3">
                    <label asp-for="Phone" class="form-label"></label>
                    <input asp-for="Phone" class="form-control"
                           placeholder="Phone number" />
                    <span asp-validation-for="Phone"
                          class="text-danger"></span>
                </div>

                <div class="mb-4">
                    <label asp-for="Department" class="form-label"></label>
                    <input asp-for="Department" class="form-control"
                           placeholder="e.g. IT, HR, Sales" />
                    <span asp-validation-for="Department"
                          class="text-danger"></span>
                </div>

                <button type="submit" class="btn btn-primary px-4">
                    Save Employee
                </button>
                <a asp-action="Index"
                   class="btn btn-outline-secondary ms-2">Cancel</a>
            </form>
        </div>
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
Step 11 — Edit Employee page
Views/Employees/Edit.cshtml file banayein. EmployeeId hidden field ke through submit hoga.

@model CrudApplication.Models.Employee

@{
    ViewData["Title"] = "Edit Employee";
}

<div class="row justify-content-center">
    <div class="col-lg-7">
        <div class="page-card p-4 p-md-5">
            <h2 class="fw-bold">Edit Employee</h2>
            <p class="text-secondary">Update the employee information.</p>

            <form asp-action="Edit" asp-route-id="@Model.EmployeeId"
                  method="post">
                <input type="hidden" asp-for="EmployeeId" />

                <div asp-validation-summary="ModelOnly"
                     class="text-danger mb-3"></div>

                <div class="mb-3">
                    <label asp-for="Name" class="form-label"></label>
                    <input asp-for="Name" class="form-control" />
                    <span asp-validation-for="Name"
                          class="text-danger"></span>
                </div>

                <div class="mb-3">
                    <label asp-for="Email" class="form-label"></label>
                    <input asp-for="Email" class="form-control" />
                    <span asp-validation-for="Email"
                          class="text-danger"></span>
                </div>

                <div class="mb-3">
                    <label asp-for="Phone" class="form-label"></label>
                    <input asp-for="Phone" class="form-control" />
                    <span asp-validation-for="Phone"
                          class="text-danger"></span>
                </div>

                <div class="mb-4">
                    <label asp-for="Department" class="form-label"></label>
                    <input asp-for="Department" class="form-control" />
                    <span asp-validation-for="Department"
                          class="text-danger"></span>
                </div>

                <button type="submit" class="btn btn-primary px-4">
                    Update Employee
                </button>
                <a asp-action="Index"
                   class="btn btn-outline-secondary ms-2">Cancel</a>
            </form>
        </div>
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
Step 12 — Employee Details page
Views/Employees/Details.cshtml file banayein.

@model CrudApplication.Models.Employee

@{
    ViewData["Title"] = "Employee Details";
}

<div class="row justify-content-center">
    <div class="col-lg-7">
        <div class="page-card p-4 p-md-5">
            <h2 class="fw-bold mb-4">Employee Details</h2>

            <dl class="row">
                <dt class="col-sm-4 mb-3">Employee ID</dt>
                <dd class="col-sm-8">@Model.EmployeeId</dd>

                <dt class="col-sm-4 mb-3">Name</dt>
                <dd class="col-sm-8">@Model.Name</dd>

                <dt class="col-sm-4 mb-3">Email</dt>
                <dd class="col-sm-8">@Model.Email</dd>

                <dt class="col-sm-4 mb-3">Phone</dt>
                <dd class="col-sm-8">@(Model.Phone ?? "-")</dd>

                <dt class="col-sm-4 mb-3">Department</dt>
                <dd class="col-sm-8">@Model.Department</dd>
            </dl>

            <a asp-action="Edit" asp-route-id="@Model.EmployeeId"
               class="btn btn-primary">Edit Employee</a>
            <a asp-action="Index"
               class="btn btn-outline-secondary ms-2">Back</a>
        </div>
    </div>
</div>
Step 13 — Delete confirmation page
Views/Employees/Delete.cshtml file banayein. Record direct link click karne par delete nahi hoga; pehle confirmation page khulega.

@model CrudApplication.Models.Employee

@{
    ViewData["Title"] = "Delete Employee";
}

<div class="row justify-content-center">
    <div class="col-lg-7">
        <div class="page-card p-4 p-md-5">
            <h2 class="fw-bold text-danger">Delete Employee</h2>
            <p class="text-secondary">
                Are you sure you want to delete this employee?
            </p>

            <div class="alert alert-warning">
                This action cannot be undone.
            </div>

            <dl class="row">
                <dt class="col-sm-4 mb-2">Name</dt>
                <dd class="col-sm-8">@Model.Name</dd>

                <dt class="col-sm-4 mb-2">Email</dt>
                <dd class="col-sm-8">@Model.Email</dd>

                <dt class="col-sm-4 mb-2">Department</dt>
                <dd class="col-sm-8">@Model.Department</dd>
            </dl>

            <form asp-action="Delete"
                  asp-route-id="@Model.EmployeeId" method="post">
                <button type="submit" class="btn btn-danger">
                    Confirm Delete
                </button>
                <a asp-action="Index"
                   class="btn btn-outline-secondary ms-2">Cancel</a>
            </form>
        </div>
    </div>
</div>
5. Database create karein aur application run karein
Abhi tak humne model, database context, controller aur views banaye hain. Ab Entity Framework Core ki migrations se actual SQL Server database aur Employees table create karenge.
Step 14 — Migration commands run karein
Visual Studio mein Tools → NuGet Package Manager → Package Manager Console open karein.
Pehle confirm karein ki Package Manager Console ka Default project CrudApplication hai. Phir ye commands ek-ek karke run karein:

Add-Migration InitialCreate
Update-Database
Inka kaam:
•	Add-Migration InitialCreate — model ke basis par database schema changes generate karta hai.
•	Update-Database — connection string mein configured SQL Server par database aur tables create/update karta hai.
Agar command successfully complete ho jaati hai, SQL Server mein CrudApplicationDB database ke andar Employees table milni chahiye.

