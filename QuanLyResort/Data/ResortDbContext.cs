using Microsoft.EntityFrameworkCore;
using QuanLyResort.Models;

namespace QuanLyResort.Data;

public class ResortDbContext : DbContext
{
    public ResortDbContext(DbContextOptions<ResortDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<RoomType> RoomTypes { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Charge> Charges { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<InventoryVoucher> InventoryVouchers { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    
    // Inventory Management
    public DbSet<InventoryCategory> InventoryCategories { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    
    // Restaurant Orders
    public DbSet<RestaurantOrder> RestaurantOrders { get; set; }
    public DbSet<RestaurantOrderItem> RestaurantOrderItems { get; set; }
    
    // Reviews & Feedback
    public DbSet<Review> Reviews { get; set; }
    
    // Coupons
    public DbSet<Coupon> Coupons { get; set; }
    
    // Customer Support
    public DbSet<FAQ> FAQs { get; set; }
    public DbSet<SupportTicket> SupportTickets { get; set; }
    public DbSet<TicketMessage> TicketMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SQLite-specific configurations for auto-increment primary keys
        // SQLite requires INTEGER PRIMARY KEY (not int) for auto-increment
        // Note: This is handled automatically by EF Core for SQLite when using [Key] attribute

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ResortDbContext).Assembly);

        // User configurations
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            
            entity.HasOne(u => u.Customer)
                .WithMany()
                .HasForeignKey(u => u.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(u => u.Employee)
                .WithMany()
                .HasForeignKey(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Customer configurations
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // RoomType configurations
        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasIndex(e => e.TypeCode).IsUnique();
        });

        // Room configurations
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasIndex(e => e.RoomNumber).IsUnique();
        });

        // Booking configurations
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasIndex(e => e.BookingCode).IsUnique();
            
            entity.HasOne(b => b.Customer)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Charge configurations
        modelBuilder.Entity<Charge>(entity =>
        {
            entity.HasOne(c => c.Booking)
                .WithMany(b => b.Charges)
                .HasForeignKey(c => c.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Room)
                .WithMany(r => r.Charges)
                .HasForeignKey(c => c.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Service)
                .WithMany(s => s.Charges)
                .HasForeignKey(c => c.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Invoice configurations
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();

            entity.HasOne(i => i.Booking)
                .WithOne(b => b.Invoice)
                .HasForeignKey<Invoice>(i => i.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.Customer)
                .WithMany()
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Employee configurations
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // InventoryVoucher configurations
        modelBuilder.Entity<InventoryVoucher>(entity =>
        {
            entity.HasIndex(e => e.VoucherNumber).IsUnique();
        });

        // AuditLog configurations
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => new { e.EntityName, e.EntityId });
            entity.HasIndex(e => e.Timestamp);
        });

        // Notification configurations
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => new { e.TargetRole, e.IsRead });
            entity.HasIndex(e => new { e.TargetUserId, e.IsRead });
        });

        // Inventory configurations
        modelBuilder.Entity<InventoryCategory>(entity =>
        {
            entity.HasIndex(e => e.CategoryName).IsUnique();
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasIndex(e => e.SupplierName);
            entity.HasIndex(e => e.Email);
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasIndex(e => e.ItemCode).IsUnique();
            entity.HasIndex(e => e.ItemName);

            entity.HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(i => i.Supplier)
                .WithMany(s => s.Items)
                .HasForeignKey(i => i.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasIndex(e => new { e.ItemId, e.MovementDate });
            entity.HasIndex(e => e.MovementDate);

            entity.HasOne(sm => sm.Item)
                .WithMany(i => i.StockMovements)
                .HasForeignKey(sm => sm.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => new { e.SupplierId, e.OrderDate });

            entity.HasOne(po => po.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(po => po.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.HasOne(poi => poi.Order)
                .WithMany(po => po.Items)
                .HasForeignKey(poi => poi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(poi => poi.Item)
                .WithMany(i => i.PurchaseOrderItems)
                .HasForeignKey(poi => poi.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Restaurant Order configurations
        modelBuilder.Entity<RestaurantOrder>(entity =>
        {
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => new { e.CustomerId, e.CreatedAt });
            entity.HasIndex(e => new { e.Status, e.PaymentStatus });
            entity.HasIndex(e => e.CreatedAt);

            // Check constraints
            entity.ToTable(tb => tb.HasCheckConstraint("CK_RestaurantOrder_Status", 
                "Status IN ('Pending', 'Confirmed', 'Preparing', 'Ready', 'Delivered', 'Cancelled')"));
            
            entity.ToTable(tb => tb.HasCheckConstraint("CK_RestaurantOrder_PaymentStatus", 
                "PaymentStatus IN ('Unpaid', 'Paid', 'Refunded')"));
            
            entity.ToTable(tb => tb.HasCheckConstraint("CK_RestaurantOrder_TotalAmount", 
                "TotalAmount >= 0"));
            
            entity.ToTable(tb => tb.HasCheckConstraint("CK_RestaurantOrder_PaymentMethod", 
                "PaymentMethod IS NULL OR PaymentMethod IN ('Cash', 'Card', 'QR', 'RoomCharge', 'BankTransfer')"));

            entity.HasOne(ro => ro.Customer)
                .WithMany()
                .HasForeignKey(ro => ro.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(ro => ro.Booking)
                .WithMany()
                .HasForeignKey(ro => ro.BookingId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RestaurantOrderItem>(entity =>
        {
            entity.HasIndex(e => new { e.OrderId, e.ServiceId });
            entity.HasIndex(e => e.ServiceId);

            // Check constraints
            entity.ToTable(tb => tb.HasCheckConstraint("CK_RestaurantOrderItem_Quantity", 
                "Quantity > 0"));
            
            entity.ToTable(tb => tb.HasCheckConstraint("CK_RestaurantOrderItem_UnitPrice", 
                "UnitPrice >= 0"));
            
            entity.ToTable(tb => tb.HasCheckConstraint("CK_RestaurantOrderItem_SubTotal", 
                "SubTotal >= 0"));
            
            // Business rule: SubTotal should equal UnitPrice * Quantity (enforced in application layer)
            // But we can add a check to ensure SubTotal >= UnitPrice * Quantity (minimum validation)
            entity.ToTable(tb => tb.HasCheckConstraint("CK_RestaurantOrderItem_SubTotalCalc", 
                "SubTotal >= UnitPrice * Quantity"));

            entity.HasOne(roi => roi.Order)
                .WithMany(ro => ro.OrderItems)
                .HasForeignKey(roi => roi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(roi => roi.Service)
                .WithMany()
                .HasForeignKey(roi => roi.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Review configurations
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasIndex(e => new { e.RoomId, e.CreatedAt });
            entity.HasIndex(e => new { e.CustomerId, e.RoomId });
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Room)
                .WithMany()
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(r => r.Booking)
                .WithMany()
                .HasForeignKey(r => r.BookingId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Coupon configurations
        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.IsActive, e.StartDate, e.EndDate });
        });

        // FAQ configurations
        modelBuilder.Entity<FAQ>(entity =>
        {
            entity.HasIndex(e => new { e.Category, e.IsActive, e.DisplayOrder });
            entity.HasIndex(e => e.IsActive);
        });

        // SupportTicket configurations
        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.HasIndex(e => e.TicketNumber).IsUnique();
            entity.HasIndex(e => new { e.CustomerId, e.CreatedAt });
            entity.HasIndex(e => new { e.Status, e.Priority });
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.ContactEmail);

            entity.HasOne(t => t.Customer)
                .WithMany()
                .HasForeignKey(t => t.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // TicketMessage configurations
        modelBuilder.Entity<TicketMessage>(entity =>
        {
            entity.HasIndex(e => new { e.TicketId, e.CreatedAt });
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(m => m.Ticket)
                .WithMany(t => t.Messages)
                .HasForeignKey(m => m.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

