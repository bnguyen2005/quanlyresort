using QuanLyResort.Models;

namespace QuanLyResort.Repositories;

public interface IUnitOfWork : IDisposable
{
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

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

