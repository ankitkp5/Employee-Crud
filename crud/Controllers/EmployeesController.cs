
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
