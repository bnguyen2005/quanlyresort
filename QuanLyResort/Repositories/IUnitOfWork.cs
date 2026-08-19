using QuanLyResort.Models;
using QuanLyResort.Data;

namespace QuanLyResort.Repositories;

public interface IUnitOfWork : IDisposable
{
    ResortDbContext Context { get; }
    IRepository<User> Users { get; }
    IRepository<Customer> Customers { get; }
    IRepository<Room> Rooms { get; }
    IRepository<Booking> Bookings { get; }
    IRepository<Service> Services { get; }
    IRepository<Charge> Charges { get; }
    IRepository<Invoice> Invoices { get; }
    IRepository<Employee> Employees { get; }
    IRepository<InventoryVoucher> InventoryVouchers { get; }
    IRepository<AuditLog> AuditLogs { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<RoomType> RoomTypes { get; }
    IRepository<InventoryCategory> InventoryCategories { get; }
    IRepository<Supplier> Suppliers { get; }
    IRepository<InventoryItem> InventoryItems { get; }
    IRepository<StockMovement> StockMovements { get; }
    IRepository<PurchaseOrder> PurchaseOrders { get; }
    IRepository<PurchaseOrderItem> PurchaseOrderItems { get; }
    IRepository<RestaurantOrder> RestaurantOrders { get; }
    IRepository<RestaurantOrderItem> RestaurantOrderItems { get; }
    IRepository<Review> Reviews { get; }
    IRepository<Coupon> Coupons { get; }
    IRepository<FAQ> FAQs { get; }
    IRepository<SupportTicket> SupportTickets { get; }
    IRepository<TicketMessage> TicketMessages { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

