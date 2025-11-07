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
        private readonly ResortDbContext _context;

        public SuppliersController(ResortDbContext context)
        {
            _context = context;
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
            if (s == null) return NotFound(new { message = "Không tìm thấy nhà cung cấp" });

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
                return BadRequest(new { message = "Tên nhà cung cấp là bắt buộc" });
            }

            dto.SupplierId = 0;
            dto.IsActive = true;
            dto.CreatedAt = DateTime.UtcNow;
            _context.Suppliers.Add(dto);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSupplier), new { id = dto.SupplierId }, new { message = "Tạo nhà cung cấp thành công", supplierId = dto.SupplierId });
        }

        // PUT: api/suppliers/5
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateSupplier(int id, [FromBody] Supplier dto)
        {
            var s = await _context.Suppliers.FindAsync(id);
            if (s == null) return NotFound(new { message = "Không tìm thấy nhà cung cấp" });

            s.SupplierName = dto.SupplierName;
            s.ContactPerson = dto.ContactPerson;
            s.Phone = dto.Phone;
            s.Email = dto.Email;
            s.Address = dto.Address;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật nhà cung cấp thành công" });
        }

        // DELETE (soft): api/suppliers/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> SoftDeleteSupplier(int id)
        {
            var s = await _context.Suppliers.FindAsync(id);
            if (s == null) return NotFound(new { message = "Không tìm thấy nhà cung cấp" });
            if (!s.IsActive) return Ok(new { message = "Nhà cung cấp đã ở trạng thái ẩn" });

            s.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã ẩn nhà cung cấp" });
        }

        // PATCH: api/suppliers/5/toggle-active
        [HttpPatch("{id}/toggle-active")]
        public async Task<ActionResult<object>> ToggleActive(int id)
        {
            var s = await _context.Suppliers.FindAsync(id);
            if (s == null) return NotFound(new { message = "Không tìm thấy nhà cung cấp" });
            s.IsActive = !s.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã cập nhật trạng thái", isActive = s.IsActive });
        }
    }
}


