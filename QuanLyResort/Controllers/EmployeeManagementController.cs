using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;
using QuanLyResort.Services;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class EmployeeManagementController : ControllerBase
{
    private readonly ResortDbContext _context;
    private readonly IAuditService _auditService;

    public EmployeeManagementController(ResortDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    /// <summary>
    /// Lấy danh sách tất cả nhân viên
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllEmployees(
        [FromQuery] string? department = null,
        [FromQuery] string? position = null,
        [FromQuery] bool? isActive = null)
    {
        var query = _context.Employees.AsQueryable();

        if (!string.IsNullOrEmpty(department))
            query = query.Where(e => e.Department == department);

        if (!string.IsNullOrEmpty(position))
            query = query.Where(e => e.Position == position);

        if (isActive.HasValue)
            query = query.Where(e => e.IsActive == isActive.Value);

        var employees = await query
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new
            {
                e.EmployeeId,
                e.FullName,
                e.Email,
                e.PhoneNumber,
                e.Position,
                e.Department,
                e.Salary,
                e.HireDate,
                e.TerminationDate,
                e.IsActive,
                e.Address,
                e.IdCardNumber,
                e.DateOfBirth,
                e.CreatedAt,
                YearsOfService = (DateTime.UtcNow - e.HireDate).Days / 365.0
            })
            .ToListAsync();

        return Ok(employees);
    }

    /// <summary>
    /// Lấy thông tin nhân viên theo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployee(int id)
    {
        var employee = await _context.Employees
            .Where(e => e.EmployeeId == id)
            .Select(e => new
            {
                e.EmployeeId,
                e.FullName,
                e.Email,
                e.PhoneNumber,
                e.Position,
                e.Department,
                e.Salary,
                e.HireDate,
                e.TerminationDate,
                e.IsActive,
                e.Address,
                e.IdCardNumber,
                e.DateOfBirth,
                e.CreatedAt,
                e.UpdatedAt,
                YearsOfService = (DateTime.UtcNow - e.HireDate).Days / 365.0
            })
            .FirstOrDefaultAsync();

        if (employee == null)
            return NotFound(new { message = "Employee not found" });

        return Ok(employee);
    }

    /// <summary>
    /// Tạo nhân viên mới
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
    {
        // Check if email exists
        if (await _context.Employees.AnyAsync(e => e.Email == request.Email))
            return BadRequest(new { message = "Email already exists" });

        var employee = new Employee
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Position = request.Position,
            Department = request.Department,
            Salary = request.Salary,
            HireDate = request.HireDate ?? DateTime.UtcNow,
            IsActive = true,
            Address = request.Address,
            IdCardNumber = request.IdCardNumber,
            DateOfBirth = request.DateOfBirth
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAsync(
            "Employee",
            employee.EmployeeId,
            "Create",
            User.Identity?.Name ?? "System",
            null,
            Newtonsoft.Json.JsonConvert.SerializeObject(employee),
            $"Created employee: {employee.FullName} - {employee.Position}"
        );

        return CreatedAtAction(nameof(GetEmployee), new { id = employee.EmployeeId }, employee);
    }

    /// <summary>
    /// Cập nhật thông tin nhân viên
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound(new { message = "Employee not found" });

        var oldValues = Newtonsoft.Json.JsonConvert.SerializeObject(employee);

        // Check if email changed and already exists
        if (request.Email != employee.Email && 
            await _context.Employees.AnyAsync(e => e.Email == request.Email && e.EmployeeId != id))
            return BadRequest(new { message = "Email already exists" });

        employee.FullName = request.FullName;
        employee.Email = request.Email;
        employee.PhoneNumber = request.PhoneNumber;
        employee.Position = request.Position;
        employee.Department = request.Department;
        employee.Salary = request.Salary;
        employee.Address = request.Address;
        employee.IdCardNumber = request.IdCardNumber;
        employee.DateOfBirth = request.DateOfBirth;
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAsync(
            "Employee",
            employee.EmployeeId,
            "Update",
            User.Identity?.Name ?? "System",
            oldValues,
            Newtonsoft.Json.JsonConvert.SerializeObject(employee),
            $"Updated employee: {employee.FullName}"
        );

        return Ok(new { message = "Employee updated successfully", employee });
    }

    /// <summary>
    /// Đổi phòng ban/chức vụ
    /// </summary>
    [HttpPost("{id}/transfer")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TransferEmployee(int id, [FromBody] TransferEmployeeRequest request)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound(new { message = "Employee not found" });

        var oldDept = employee.Department;
        var oldPos = employee.Position;

        employee.Department = request.NewDepartment;
        employee.Position = request.NewPosition;
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAsync(
            "Employee",
            employee.EmployeeId,
            "Transfer",
            User.Identity?.Name ?? "System",
            $"{{\"Department\": \"{oldDept}\", \"Position\": \"{oldPos}\"}}",
            $"{{\"Department\": \"{request.NewDepartment}\", \"Position\": \"{request.NewPosition}\"}}",
            $"Transferred {employee.FullName}: {oldDept}/{oldPos} → {request.NewDepartment}/{request.NewPosition}"
        );

        return Ok(new { message = "Employee transferred successfully", employee });
    }

    /// <summary>
    /// Chấm dứt hợp đồng (nghỉ việc)
    /// </summary>
    [HttpPost("{id}/terminate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TerminateEmployee(int id, [FromBody] TerminateEmployeeRequest request)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound(new { message = "Employee not found" });

        employee.IsActive = false;
        employee.TerminationDate = request.TerminationDate ?? DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAsync(
            "Employee",
            employee.EmployeeId,
            "Terminate",
            User.Identity?.Name ?? "System",
            null,
            null,
            $"Terminated employee: {employee.FullName} on {employee.TerminationDate:yyyy-MM-dd}. Reason: {request.Reason}"
        );

        return Ok(new { message = "Employee terminated successfully", employee });
    }

    /// <summary>
    /// Kích hoạt lại nhân viên
    /// </summary>
    [HttpPost("{id}/reactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReactivateEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound(new { message = "Employee not found" });

        employee.IsActive = true;
        employee.TerminationDate = null;
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAsync(
            "Employee",
            employee.EmployeeId,
            "Reactivate",
            User.Identity?.Name ?? "System",
            null,
            null,
            $"Reactivated employee: {employee.FullName}"
        );

        return Ok(new { message = "Employee reactivated successfully", employee });
    }

    /// <summary>
    /// Xóa nhân viên vĩnh viễn
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound(new { message = "Employee not found" });

        var employeeInfo = $"{employee.FullName} ({employee.Email})";

        // Xử lý foreign key constraint: Set EmployeeId = null trong Users trước
        var relatedUsers = await _context.Users
            .Where(u => u.EmployeeId == id)
            .ToListAsync();

        if (relatedUsers.Any())
        {
            foreach (var user in relatedUsers)
            {
                user.EmployeeId = null;
            }
            await _context.SaveChangesAsync();
        }

        // Bây giờ mới xóa Employee
        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAsync(
            "Employee",
            id,
            "Delete",
            User.Identity?.Name ?? "System",
            null,
            null,
            $"Deleted employee: {employeeInfo} (unlinked {relatedUsers.Count} user accounts)"
        );

        return Ok(new { message = "Employee deleted successfully" });
    }

    /// <summary>
    /// Lấy danh sách phòng ban
    /// </summary>
    [HttpGet("departments")]
    public IActionResult GetDepartments()
    {
        var departments = new[]
        {
            new { value = "Management", label = "Ban Giám Đốc", description = "Quản lý cấp cao" },
            new { value = "Business", label = "Kinh Doanh", description = "Kinh doanh & marketing" },
            new { value = "FrontDesk", label = "Lễ Tân", description = "Tiếp đón khách" },
            new { value = "Finance", label = "Tài Chính", description = "Kế toán & thu ngân" },
            new { value = "Operations", label = "Vận Hành", description = "Kho & logistics" },
            new { value = "Housekeeping", label = "Buồng Phòng", description = "Dọn dẹp & vệ sinh" },
            new { value = "Maintenance", label = "Kỹ Thuật", description = "Sửa chữa & bảo trì" },
            new { value = "Kitchen", label = "Bếp", description = "Nhà hàng & ẩm thực" },
            new { value = "Security", label = "Bảo Vệ", description = "An ninh" }
        };

        return Ok(departments);
    }

    /// <summary>
    /// Lấy danh sách chức vụ
    /// </summary>
    [HttpGet("positions")]
    public IActionResult GetPositions()
    {
        var positions = new[]
        {
            "General Manager",
            "Business Manager",
            "Finance Manager",
            "Operations Manager",
            "Receptionist",
            "Front Desk Supervisor",
            "Cashier",
            "Accountant",
            "Inventory Manager",
            "Housekeeping Supervisor",
            "Room Attendant",
            "Maintenance Technician",
            "Chef",
            "Cook",
            "Waiter",
            "Security Guard"
        };

        return Ok(positions);
    }

    /// <summary>
    /// Thống kê nhân viên
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var stats = new
        {
            totalEmployees = await _context.Employees.CountAsync(),
            activeEmployees = await _context.Employees.CountAsync(e => e.IsActive),
            inactiveEmployees = await _context.Employees.CountAsync(e => !e.IsActive),
            byDepartment = await _context.Employees
                .Where(e => e.IsActive)
                .GroupBy(e => e.Department)
                .Select(g => new { department = g.Key, count = g.Count() })
                .ToListAsync(),
            byPosition = await _context.Employees
                .Where(e => e.IsActive)
                .GroupBy(e => e.Position)
                .Select(g => new { position = g.Key, count = g.Count() })
                .ToListAsync(),
            recentHires = await _context.Employees
                .Where(e => e.HireDate >= DateTime.UtcNow.AddMonths(-3))
                .OrderByDescending(e => e.HireDate)
                .Take(5)
                .Select(e => new { e.EmployeeId, e.FullName, e.Position, e.HireDate })
                .ToListAsync()
        };

        return Ok(stats);
    }
}

// DTOs
public class CreateEmployeeRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal? Salary { get; set; }
    public DateTime? HireDate { get; set; }
    public string? Address { get; set; }
    public string? IdCardNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class UpdateEmployeeRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal? Salary { get; set; }
    public string? Address { get; set; }
    public string? IdCardNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class TransferEmployeeRequest
{
    public string NewDepartment { get; set; } = string.Empty;
    public string NewPosition { get; set; } = string.Empty;
}

public class TerminateEmployeeRequest
{
    public DateTime? TerminationDate { get; set; }
    public string? Reason { get; set; }
}

