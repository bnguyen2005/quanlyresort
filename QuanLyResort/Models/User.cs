using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyResort.Models;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = string.Empty; // Admin, FrontDesk, Cashier, Accounting, Inventory, Manager, Customer

    [StringLength(100)]
    public string? FullName { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    // Two-Factor Authentication
    public string? TwoFactorSecret { get; set; }
    public bool TwoFactorEnabled { get; set; } = false;
    public DateTime? TwoFactorEnabledAt { get; set; }
    public string? TwoFactorRecoveryCodes { get; set; }

    // Navigation properties
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
}

