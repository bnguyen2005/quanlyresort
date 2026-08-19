using QuanLyResort.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;
using System.Security.Claims;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupportTicketsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private ResortDbContext _context => _unitOfWork.Context;

    public SupportTicketsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// T?o ticket m?i (public - khách hàng ho?c khách vãng lai)
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest(new { message = "Tiêu d? và n?i dung không du?c d? tr?ng" });
            }

            // Generate ticket number
            var ticketCount = await _context.SupportTickets.CountAsync();
            var ticketNumber = $"TKT{DateTime.UtcNow:yyyyMM}{(ticketCount + 1):D4}";

            // Get customer ID if logged in
            int? customerId = null;
            string? userEmail = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == userEmail.ToLower());
                    if (user == null)
                    {
                        // Th? tìm b?ng username
                        user = await _context.Users
                            .FirstOrDefaultAsync(u => u.Username != null && u.Username.ToLower() == userEmail.ToLower());
                    }
                    if (user != null && user.CustomerId.HasValue)
                    {
                        customerId = user.CustomerId.Value;
                    }
                }
            }

            var ticket = new SupportTicket
            {
                TicketNumber = ticketNumber,
                CustomerId = customerId,
                Subject = request.Subject.Trim(),
                Description = request.Description.Trim(),
                Category = request.Category ?? "General",
                Status = "Open",
                Priority = request.Priority ?? "Normal",
                ContactName = request.ContactName?.Trim(),
                ContactEmail = request.ContactEmail?.Trim(),
                ContactPhone = request.ContactPhone?.Trim(),
                CreatedBy = userEmail ?? request.ContactEmail,
                CreatedAt = DateTime.UtcNow
            };

            _context.SupportTickets.Add(ticket);
            await _unitOfWork.SaveChangesAsync();

            // Create initial message
            var initialMessage = new TicketMessage
            {
                TicketId = ticket.TicketId,
                Content = request.Description.Trim(),
                SenderType = customerId.HasValue ? "Customer" : "Customer",
                SenderName = request.ContactName ?? "Khách hàng",
                SenderEmail = request.ContactEmail ?? userEmail,
                CreatedAt = DateTime.UtcNow
            };

            _context.TicketMessages.Add(initialMessage);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new
            {
                message = "Ticket dã du?c t?o thành công",
                ticket = new
                {
                    ticket.TicketId,
                    ticket.TicketNumber,
                    ticket.Subject,
                    ticket.Status,
                    ticket.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SupportTicketsController] Error: {ex.Message}");
            return StatusCode(500, new { message = "L?i khi t?o ticket", error = ex.Message });
        }
    }

    /// <summary>
    /// L?y danh sách tickets c?a khách hàng (c?n dang nh?p)
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyTickets()
    {
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            Console.WriteLine($"[SupportTicketsController] GetMyTickets called for email: {userEmail}");
            
            if (string.IsNullOrEmpty(userEmail))
            {
                Console.WriteLine("[SupportTicketsController] No user email found");
                return Unauthorized(new { message = "Không tìm th?y thông tin ngu?i dùng" });
            }

            // Tìm user v?i email (case-insensitive)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == userEmail.ToLower());
            Console.WriteLine($"[SupportTicketsController] User found: {user != null}, CustomerId: {user?.CustomerId}, Email in DB: {user?.Email}");
            
            if (user == null)
            {
                Console.WriteLine($"[SupportTicketsController] User not found in database for email: {userEmail}");
                // Th? tìm v?i username n?u email không tìm th?y
                var userByUsername = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username != null && u.Username.ToLower() == userEmail.ToLower());
                if (userByUsername != null)
                {
                    Console.WriteLine($"[SupportTicketsController] Found user by username instead: {userByUsername.Username}");
                    user = userByUsername;
                }
                else
                {
                    Console.WriteLine("[SupportTicketsController] User not found by email or username");
                    return NotFound(new { message = "Không tìm th?y thông tin ngu?i dùng" });
                }
            }

            if (!user.CustomerId.HasValue)
            {
                Console.WriteLine("[SupportTicketsController] User has no CustomerId - returning empty list");
                // Tr? v? danh sách r?ng thay vì NotFound n?u user chua có CustomerId
                // (có th? là user m?i t?o chua có customer record)
                return Ok(new List<object>());
            }

            var tickets = await _context.SupportTickets
                .Where(t => t.CustomerId == user.CustomerId.Value)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.TicketId,
                    t.TicketNumber,
                    t.Subject,
                    t.Category,
                    t.Status,
                    t.Priority,
                    t.CreatedAt,
                    t.UpdatedAt,
                    t.ResolvedAt,
                    messageCount = t.Messages.Count(m => !m.IsInternal)
                })
                .ToListAsync();

            Console.WriteLine($"[SupportTicketsController] Found {tickets.Count} tickets for customer {user.CustomerId.Value}");
            return Ok(tickets);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SupportTicketsController] Error in GetMyTickets: {ex.Message}");
            Console.WriteLine($"[SupportTicketsController] Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { message = "L?i khi t?i tickets", error = ex.Message });
        }
    }

    /// <summary>
    /// L?y chi ti?t ticket (khách hàng ch? xem du?c ticket c?a mình)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetTicket(int id)
    {
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var query = _context.SupportTickets
                .Include(t => t.Messages.Where(m => !m.IsInternal || userRole == "Admin" || userRole == "Manager"))
                .Include(t => t.Customer)
                .AsQueryable();

            // Customer ch? xem du?c ticket c?a mình
            if (userRole == "Customer")
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == userEmail.ToLower());
                if (user == null && !string.IsNullOrEmpty(userEmail))
                {
                    // Th? tìm b?ng username
                    user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username != null && u.Username.ToLower() == userEmail.ToLower());
                }
                if (user == null || !user.CustomerId.HasValue)
                {
                    return Unauthorized(new { message = "Không tìm th?y thông tin khách hàng" });
                }
                query = query.Where(t => t.CustomerId == user.CustomerId.Value);
            }

            var ticket = await query.FirstOrDefaultAsync(t => t.TicketId == id);
            if (ticket == null)
            {
                return NotFound(new { message = "Ticket không t?n t?i ho?c b?n không có quy?n xem" });
            }

            return Ok(new
            {
                ticket.TicketId,
                ticket.TicketNumber,
                ticket.Subject,
                ticket.Description,
                ticket.Category,
                ticket.Status,
                ticket.Priority,
                ticket.AssignedTo,
                ticket.CreatedAt,
                ticket.UpdatedAt,
                ticket.ResolvedAt,
                ticket.ResolutionNotes,
                customer = ticket.Customer != null ? new
                {
                    ticket.Customer.CustomerId,
                    ticket.Customer.FullName,
                    ticket.Customer.Email,
                    ticket.Customer.PhoneNumber
                } : null,
                contactName = ticket.ContactName,
                contactEmail = ticket.ContactEmail,
                contactPhone = ticket.ContactPhone,
                messages = ticket.Messages.OrderBy(m => m.CreatedAt).Select(m => new
                {
                    m.MessageId,
                    m.Content,
                    m.SenderType,
                    m.SenderName,
                    m.SenderEmail,
                    m.IsInternal,
                    m.CreatedAt,
                    m.AttachmentUrl,
                    m.AttachmentName
                })
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SupportTicketsController] Error: {ex.Message}");
            return StatusCode(500, new { message = "L?i khi t?i ticket", error = ex.Message });
        }
    }

    /// <summary>
    /// Thêm message vào ticket
    /// </summary>
    [HttpPost("{id}/messages")]
    [Authorize]
    public async Task<IActionResult> AddMessage(int id, [FromBody] AddMessageRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(new { message = "N?i dung tin nh?n không du?c d? tr?ng" });
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var ticket = await _context.SupportTickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound(new { message = "Ticket không t?n t?i" });
            }

            // Check permission: Customer ch? có th? reply ticket c?a mình
            if (userRole == "Customer")
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == userEmail.ToLower());
                if (user == null && !string.IsNullOrEmpty(userEmail))
                {
                    // Th? tìm b?ng username
                    user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username != null && u.Username.ToLower() == userEmail.ToLower());
                }
                if (user == null || !user.CustomerId.HasValue || ticket.CustomerId != user.CustomerId.Value)
                {
                    return Forbid("B?n không có quy?n thêm tin nh?n vào ticket này");
                }
            }

            var message = new TicketMessage
            {
                TicketId = id,
                Content = request.Content.Trim(),
                SenderType = userRole == "Customer" ? "Customer" : "Staff",
                SenderName = request.SenderName?.Trim(),
                SenderEmail = userEmail ?? request.SenderEmail,
                IsInternal = request.IsInternal ?? false,
                CreatedAt = DateTime.UtcNow
            };

            _context.TicketMessages.Add(message);

            // Update ticket status
            if (ticket.Status == "Resolved" || ticket.Status == "Closed")
            {
                ticket.Status = "Open"; // Reopen if customer replies
            }
            ticket.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Tin nh?n dã du?c g?i thành công", messageId = message.MessageId });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SupportTicketsController] Error: {ex.Message}");
            return StatusCode(500, new { message = "L?i khi g?i tin nh?n", error = ex.Message });
        }
    }

    // ========== ADMIN/STAFF ENDPOINTS ==========

    /// <summary>
    /// L?y t?t c? tickets (Admin/Staff only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,FrontDesk")]
    public async Task<IActionResult> GetAllTickets(
        [FromQuery] string? status = null,
        [FromQuery] string? category = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? search = null)
    {
        // Reduced logging to avoid Railway rate limit (500 logs/sec)
        // const string logPrefix = "[SupportTicketsController.GetAllTickets]";
        // var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        // Console.WriteLine($"{logPrefix} [{timestamp}] ========== START ==========");
        // Console.WriteLine($"{logPrefix} [{timestamp}] Request params: status={status}, category={category}, priority={priority}, search={search}");
        
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            // Console.WriteLine($"{logPrefix} [{timestamp}] User: {userEmail}, Role: {userRole}");
            
            var query = _context.SupportTickets
                .Include(t => t.Customer)
                .AsQueryable();
            
            // Console.WriteLine($"{logPrefix} [{timestamp}] Initial query count: {await query.CountAsync()}");

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(t => t.Category == category);

            if (!string.IsNullOrEmpty(priority))
                query = query.Where(t => t.Priority == priority);

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(t =>
                    t.TicketNumber.ToLower().Contains(searchLower) ||
                    t.Subject.ToLower().Contains(searchLower) ||
                    (t.Customer != null && t.Customer.FullName.ToLower().Contains(searchLower)) ||
                    (t.ContactName != null && t.ContactName.ToLower().Contains(searchLower)) ||
                    (t.ContactEmail != null && t.ContactEmail.ToLower().Contains(searchLower)));
            }

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.TicketId,
                    t.TicketNumber,
                    t.Subject,
                    t.Category,
                    t.Status,
                    t.Priority,
                    t.AssignedTo,
                    t.CreatedAt,
                    t.UpdatedAt,
                    customer = t.Customer != null ? new
                    {
                        t.Customer.CustomerId,
                        t.Customer.FullName,
                        t.Customer.Email
                    } : null,
                    contactName = t.ContactName,
                    contactEmail = t.ContactEmail,
                    messageCount = t.Messages.Count(m => !m.IsInternal)
                })
                .ToListAsync();

            // Console.WriteLine($"{logPrefix} [{timestamp}] ? Found {tickets.Count} tickets");
            // Console.WriteLine($"{logPrefix} [{timestamp}] ========== END (SUCCESS) ==========");
            return Ok(tickets);
        }
        catch (Exception ex)
        {
            // Only log errors, not verbose debug info
            Console.WriteLine($"[SupportTicketsController.GetAllTickets] ? Error: {ex.Message}");
            return StatusCode(500, new { message = "L?i khi t?i tickets", error = ex.Message });
        }
    }

    /// <summary>
    /// C?p nh?t ticket (Admin/Staff only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager,FrontDesk")]
    public async Task<IActionResult> UpdateTicket(int id, [FromBody] UpdateTicketRequest request)
    {
        try
        {
            var ticket = await _context.SupportTickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound(new { message = "Ticket không t?n t?i" });
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
                ticket.Status = request.Status;

            if (!string.IsNullOrWhiteSpace(request.Priority))
                ticket.Priority = request.Priority;

            if (request.AssignedTo != null)
                ticket.AssignedTo = request.AssignedTo;

            if (!string.IsNullOrWhiteSpace(request.ResolutionNotes))
                ticket.ResolutionNotes = request.ResolutionNotes;

            if (ticket.Status == "Resolved" && ticket.ResolvedAt == null)
                ticket.ResolvedAt = DateTime.UtcNow;

            if (ticket.Status == "Closed" && ticket.ClosedAt == null)
                ticket.ClosedAt = DateTime.UtcNow;

            ticket.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Ticket dã du?c c?p nh?t thành công" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SupportTicketsController] Error: {ex.Message}");
            return StatusCode(500, new { message = "L?i khi c?p nh?t ticket", error = ex.Message });
        }
    }
}

// DTOs
public class CreateTicketRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Priority { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
}

public class AddMessageRequest
{
    public string Content { get; set; } = string.Empty;
    public string? SenderName { get; set; }
    public string? SenderEmail { get; set; }
    public bool? IsInternal { get; set; }
}

public class UpdateTicketRequest
{
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? AssignedTo { get; set; }
    public string? ResolutionNotes { get; set; }
}


