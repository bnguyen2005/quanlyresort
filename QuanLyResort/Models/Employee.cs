using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyResort.Models;

public class Employee
{
    [Key]
    public int EmployeeId { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    [StringLength(50)]
    public string Position { get; set; } = string.Empty; // FrontDesk, Cashier, Manager, Housekeeping, Accounting, Inventory

    [Required]
    [StringLength(50)]
    public string Department { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Salary { get; set; }

    public DateTime HireDate { get; set; } = DateTime.UtcNow;

    public DateTime? TerminationDate { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(50)]
    public string? IdCardNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

