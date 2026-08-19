using QuanLyResort.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;

namespace QuanLyResort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class SuppliersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
    private ResortDbContext _context => _unitOfWork.Context;

        public SuppliersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/suppliers
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetSuppliers([FromQuery] bool includeInactive = false)
        {
            var query = _context.Suppliers.AsNoTracking().AsQueryable();
            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive);
            }

            var list = await query
                .OrderBy(s => s.SupplierName)
                .Select(s => new {
                    s.SupplierId,
                    s.SupplierName,
                    s.ContactPerson,
                    s.Phone,
                    s.Email,
                    s.Address,
                    s.IsActive,
                    ItemCount = _context.InventoryItems.Count(i => i.SupplierId == s.SupplierId && i.IsActive)
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/suppliers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetSupplier(int id)
        {
            var s = await _context.Suppliers.FindAsync(id);
            if (s == null) return NotFound(new { message = "Không tìm th?y nhà cung c?p" });

            return Ok(new {
                s.SupplierId,
                s.SupplierName,
                s.ContactPerson,
                s.Phone,
                s.Email,
                s.Address,
                s.IsActive
            });
        }

        // POST: api/suppliers
        [HttpPost]
        public async Task<ActionResult<object>> CreateSupplier([FromBody] Supplier dto)
        {
            if (string.IsNullOrWhiteSpace(dto.SupplierName))
            {
                return BadRequest(new { message = "Tên nhà cung c?p là b?t bu?c" });
            }

            dto.SupplierId = 0;
            dto.IsActive = true;
            dto.CreatedAt = DateTime.UtcNow;
            _context.Suppliers.Add(dto);
            await _unitOfWork.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSupplier), new { id = dto.SupplierId }, new { message = "T?o nhà cung c?p thành công", supplierId = dto.SupplierId });
        }

        // PUT: api/suppliers/5
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateSupplier(int id, [FromBody] Supplier dto)
        {
            var s = await _context.Suppliers.FindAsync(id);
            if (s == null) return NotFound(new { message = "Không tìm th?y nhà cung c?p" });

            s.SupplierName = dto.SupplierName;
            s.ContactPerson = dto.ContactPerson;
            s.Phone = dto.Phone;
            s.Email = dto.Email;
            s.Address = dto.Address;
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "C?p nh?t nhà cung c?p thành công" });
        }

        // DELETE (soft): api/suppliers/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> SoftDeleteSupplier(int id)
        {
            var s = await _context.Suppliers.FindAsync(id);
            if (s == null) return NotFound(new { message = "Không tìm th?y nhà cung c?p" });
            if (!s.IsActive) return Ok(new { message = "Nhà cung c?p dã ? tr?ng thái ?n" });

            s.IsActive = false;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { message = "Ðã ?n nhà cung c?p" });
        }

        // PATCH: api/suppliers/5/toggle-active
        [HttpPatch("{id}/toggle-active")]
        public async Task<ActionResult<object>> ToggleActive(int id)
        {
            var s = await _context.Suppliers.FindAsync(id);
            if (s == null) return NotFound(new { message = "Không tìm th?y nhà cung c?p" });
            s.IsActive = !s.IsActive;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { message = "Ðã c?p nh?t tr?ng thái", isActive = s.IsActive });
        }
    }
}



