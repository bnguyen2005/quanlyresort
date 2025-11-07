using Microsoft.EntityFrameworkCore.Storage;
using QuanLyResort.Data;
using QuanLyResort.Models;

namespace QuanLyResort.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ResortDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ResortDbContext context)
    {
        _context = context;
        Users = new Repository<User>(_context);
        Customers = new Repository<Customer>(_context);
        Rooms = new Repository<Room>(_context);
        Bookings = new Repository<Booking>(_context);
        Services = new Repository<Service>(_context);
        Charges = new Repository<Charge>(_context);
        Invoices = new Repository<Invoice>(_context);
        Employees = new Repository<Employee>(_context);
        InventoryVouchers = new Repository<InventoryVoucher>(_context);
        AuditLogs = new Repository<AuditLog>(_context);
        Notifications = new Repository<Notification>(_context);
    }

    public IRepository<User> Users { get; private set; }
    public IRepository<Customer> Customers { get; private set; }
    public IRepository<Room> Rooms { get; private set; }
    public IRepository<Booking> Bookings { get; private set; }
    public IRepository<Service> Services { get; private set; }
    public IRepository<Charge> Charges { get; private set; }
    public IRepository<Invoice> Invoices { get; private set; }
    public IRepository<Employee> Employees { get; private set; }
    public IRepository<InventoryVoucher> InventoryVouchers { get; private set; }
    public IRepository<AuditLog> AuditLogs { get; private set; }
    public IRepository<Notification> Notifications { get; private set; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

