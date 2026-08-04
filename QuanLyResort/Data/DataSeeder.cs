using QuanLyResort.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace QuanLyResort.Data;

public class DataSeeder
{
    private readonly ResortDbContext _context;

    public DataSeeder(ResortDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Seed Employees
        if (!_context.Employees.Any())
        {
            var employees = new List<Employee>
            {
                new Employee { FullName = "Nguyễn Văn Admin", Email = "admin@resort.test", PhoneNumber = "0901234567", Position = "Administrator", Department = "Management", IsActive = true },
                new Employee { FullName = "Trần Thị Business", Email = "business@resort.test", PhoneNumber = "0901234568", Position = "Business Manager", Department = "Business", IsActive = true },
                new Employee { FullName = "Lê Văn FrontDesk", Email = "frontdesk@resort.test", PhoneNumber = "0901234569", Position = "Receptionist", Department = "FrontDesk", IsActive = true },
                new Employee { FullName = "Phạm Thị Cashier", Email = "cashier@resort.test", PhoneNumber = "0901234570", Position = "Cashier", Department = "Finance", IsActive = true },
                new Employee { FullName = "Hoàng Văn Accounting", Email = "accounting@resort.test", PhoneNumber = "0901234571", Position = "Accountant", Department = "Finance", IsActive = true },
                new Employee { FullName = "Võ Thị Inventory", Email = "inventory@resort.test", PhoneNumber = "0901234572", Position = "Inventory Manager", Department = "Operations", IsActive = true },
                new Employee { FullName = "Đặng Văn Manager", Email = "manager@resort.test", PhoneNumber = "0901234573", Position = "General Manager", Department = "Management", IsActive = true }
            };
            await _context.Employees.AddRangeAsync(employees);
            await _context.SaveChangesAsync();
        }

        // Seed Inventory Categories
        if (!_context.InventoryCategories.Any())
        {
            var categories = new List<InventoryCategory>
            {
                new InventoryCategory { CategoryName = "Đồ uống", Description = "Beverages" , IsActive = true},
                new InventoryCategory { CategoryName = "Thực phẩm", Description = "Food" , IsActive = true},
                new InventoryCategory { CategoryName = "Vệ sinh", Description = "Housekeeping supplies" , IsActive = true},
                new InventoryCategory { CategoryName = "Thiết bị", Description = "Equipment" , IsActive = true}
            };
            await _context.InventoryCategories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
        }

        // Seed Suppliers
        if (!_context.Suppliers.Any())
        {
            var suppliers = new List<Supplier>
            {
                new Supplier { SupplierName = "Công ty TNHH ABC", Email = "abc@supplier.test", Phone = "0281234567", Address = "TP.HCM", IsActive = true },
                new Supplier { SupplierName = "Nhà cung cấp XYZ", Email = "xyz@supplier.test", Phone = "0241234567", Address = "Hà Nội", IsActive = true }
            };
            await _context.Suppliers.AddRangeAsync(suppliers);
            await _context.SaveChangesAsync();
        }

        // Seed Inventory Items
        if (!_context.InventoryItems.Any())
        {
            var catDrinks = _context.InventoryCategories.FirstOrDefault(c => c.CategoryName == "Đồ uống");
            var catFood = _context.InventoryCategories.FirstOrDefault(c => c.CategoryName == "Thực phẩm");
            var catHK = _context.InventoryCategories.FirstOrDefault(c => c.CategoryName == "Vệ sinh");
            var sup1 = _context.Suppliers.First();

            var items = new List<InventoryItem>
            {
                new InventoryItem
                {
                    ItemCode = "DU001",
                    ItemName = "Nước suối Aquafina 500ml",
                    Description = "Chai 500ml",
                    CategoryId = catDrinks?.CategoryId,
                    SupplierId = sup1.SupplierId,
                    CurrentStock = 150,
                    MinimumStock = 50,
                    MaximumStock = 500,
                    UnitPrice = 8000,
                    Unit = "chai",
                    Location = "Kệ A1",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                },
                new InventoryItem
                {
                    ItemCode = "TP001",
                    ItemName = "Bánh mì sandwich",
                    Description = "Gói 500g",
                    CategoryId = catFood?.CategoryId,
                    SupplierId = sup1.SupplierId,
                    CurrentStock = 30,
                    MinimumStock = 20,
                    MaximumStock = 200,
                    UnitPrice = 25000,
                    Unit = "gói",
                    Location = "Kệ B2",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                },
                new InventoryItem
                {
                    ItemCode = "VS001",
                    ItemName = "Dầu gội 30ml",
                    Description = "Dành cho phòng",
                    CategoryId = catHK?.CategoryId,
                    SupplierId = sup1.SupplierId,
                    CurrentStock = 500,
                    MinimumStock = 200,
                    MaximumStock = 2000,
                    UnitPrice = 5000,
                    Unit = "chai",
                    Location = "Kho HK",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                }
            };
            await _context.InventoryItems.AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }

        // Seed Users (Staff accounts)
        if (!_context.Users.Any())
        {
            var employees = _context.Employees.ToList();
            var users = new List<User>
            {
                new User { Username = "admin", Email = "admin@resort.test", PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd123"), Role = "Admin", FullName = "Nguyễn Văn Admin", IsActive = true, EmployeeId = employees.FirstOrDefault(e => e.Email == "admin@resort.test")?.EmployeeId },
                new User { Username = "business", Email = "business@resort.test", PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd123"), Role = "Business", FullName = "Trần Thị Business", IsActive = true, EmployeeId = employees.FirstOrDefault(e => e.Email == "business@resort.test")?.EmployeeId },
                new User { Username = "frontdesk", Email = "frontdesk@resort.test", PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd123"), Role = "FrontDesk", FullName = "Lê Văn FrontDesk", IsActive = true, EmployeeId = employees.FirstOrDefault(e => e.Email == "frontdesk@resort.test")?.EmployeeId },
                new User { Username = "cashier", Email = "cashier@resort.test", PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd123"), Role = "Cashier", FullName = "Phạm Thị Cashier", IsActive = true, EmployeeId = employees.FirstOrDefault(e => e.Email == "cashier@resort.test")?.EmployeeId },
                new User { Username = "accounting", Email = "accounting@resort.test", PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd123"), Role = "Accounting", FullName = "Hoàng Văn Accounting", IsActive = true, EmployeeId = employees.FirstOrDefault(e => e.Email == "accounting@resort.test")?.EmployeeId },
                new User { Username = "inventory", Email = "inventory@resort.test", PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd123"), Role = "Inventory", FullName = "Võ Thị Inventory", IsActive = true, EmployeeId = employees.FirstOrDefault(e => e.Email == "inventory@resort.test")?.EmployeeId },
                new User { Username = "manager", Email = "manager@resort.test", PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd123"), Role = "Manager", FullName = "Đặng Văn Manager", IsActive = true, EmployeeId = employees.FirstOrDefault(e => e.Email == "manager@resort.test")?.EmployeeId }
            };
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
        }

        // Seed Customers
        if (!_context.Customers.Any())
        {
            var customers = new List<Customer>
            {
                new Customer { FullName = "Nguyễn Thị Lan", Email = "customer1@guest.test", PhoneNumber = "0912345678", PassportNumber = "N1234567", Nationality = "Vietnam", CustomerType = "Regular" },
                new Customer { FullName = "John Smith", Email = "john.smith@example.com", PhoneNumber = "0987654321", PassportNumber = "US987654", Nationality = "USA", CustomerType = "VIP" },
                new Customer { FullName = "Trần Văn Minh", Email = "tran.minh@example.com", PhoneNumber = "0923456789", IdCardNumber = "001234567890", Nationality = "Vietnam", CustomerType = "Corporate" }
            };
            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();

            // Add customer user accounts
            var customer1 = customers[0];
            var customerUser = new User
            {
                Username = "customer1",
                Email = "customer1@guest.test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Guest@123"),
                Role = "Customer",
                FullName = "Nguyễn Thị Lan",
                IsActive = true,
                CustomerId = customer1.CustomerId
            };
            await _context.Users.AddAsync(customerUser);
            await _context.SaveChangesAsync();
        }

        // Seed RoomTypes
        if (!_context.RoomTypes.Any())
        {
            var roomTypes = new List<RoomType>
            {
                new RoomType 
                { 
                    TypeName = "Standard Room", 
                    TypeCode = "STD", 
                    Description = "Phòng tiêu chuẩn với đầy đủ tiện nghi cơ bản, phù hợp cho 2 người", 
                    BasePrice = 5000, 
                    MaxOccupancy = 2, 
                    StandardOccupancy = 2,
                    RoomSize = 25,
                    BedType = "1 Double Bed hoặc 2 Single Beds",
                    Amenities = "WiFi, TV, Điều hòa, Minibar, Két sắt, Bàn làm việc",
                    MainImageUrl = "images/room-1.jpg",
                    ImageGallery = "[\"images/room-1.jpg\", \"images/room-2.jpg\"]",
                    IsActive = true,
                    DisplayOrder = 1
                },
                new RoomType 
                { 
                    TypeName = "Deluxe Room", 
                    TypeCode = "DLX", 
                    Description = "Phòng sang trọng hơn với view biển, diện tích rộng rãi", 
                    BasePrice = 8000, 
                    MaxOccupancy = 3, 
                    StandardOccupancy = 2,
                    ExtraPersonCharge = 2000,
                    RoomSize = 35,
                    BedType = "1 King Bed hoặc 2 Queen Beds",
                    Amenities = "WiFi, Smart TV, Điều hòa, Minibar, Ban công, Bồn tắm, Két sắt, Sofa",
                    MainImageUrl = "images/room-3.jpg",
                    ImageGallery = "[\"images/room-3.jpg\", \"images/room-4.jpg\"]",
                    IsActive = true,
                    DisplayOrder = 2
                },
                new RoomType 
                { 
                    TypeName = "Suite Room", 
                    TypeCode = "SUT", 
                    Description = "Suite cao cấp với phòng khách riêng biệt và view toàn cảnh biển", 
                    BasePrice = 15000, 
                    MaxOccupancy = 4, 
                    StandardOccupancy = 2,
                    ExtraPersonCharge = 3000,
                    RoomSize = 60,
                    BedType = "1 King Bed + Sofa Bed",
                    Amenities = "WiFi, Smart TV 55', Điều hòa, Minibar, Phòng khách riêng, Jacuzzi, Ban công lớn, Két sắt, Coffee machine",
                    MainImageUrl = "images/room-4.jpg",
                    ImageGallery = "[\"images/room-4.jpg\", \"images/room-5.jpg\"]",
                    IsActive = true,
                    DisplayOrder = 3
                },
                new RoomType 
                { 
                    TypeName = "Villa", 
                    TypeCode = "VIL", 
                    Description = "Villa sang trọng với hồ bơi riêng và khu vườn", 
                    BasePrice = 30000, 
                    MaxOccupancy = 6, 
                    StandardOccupancy = 4,
                    ExtraPersonCharge = 5000,
                    RoomSize = 120,
                    BedType = "2 King Beds + 2 Sofa Beds",
                    Amenities = "WiFi, Multiple Smart TVs, Điều hòa, Bếp đầy đủ, Hồ bơi riêng, Khu vườn, Nhiều phòng ngủ, BBQ, Bồn tắm Jacuzzi",
                    MainImageUrl = "images/room-5.jpg",
                    ImageGallery = "[\"images/room-5.jpg\", \"images/room-6.jpg\"]",
                    IsActive = true,
                    DisplayOrder = 4
                }
            };
            await _context.RoomTypes.AddRangeAsync(roomTypes);
            await _context.SaveChangesAsync();
        }

        // Seed Rooms
        if (!_context.Rooms.Any())
        {
            var roomTypes = _context.RoomTypes.ToList();
            var standardType = roomTypes.FirstOrDefault(rt => rt.TypeCode == "STD");
            var deluxeType = roomTypes.FirstOrDefault(rt => rt.TypeCode == "DLX");
            var suiteType = roomTypes.FirstOrDefault(rt => rt.TypeCode == "SUT");
            var villaType = roomTypes.FirstOrDefault(rt => rt.TypeCode == "VIL");

            var rooms = new List<Room>
            {
                new Room { RoomNumber = "101", RoomType = "Standard", RoomTypeId = standardType?.RoomTypeId, Floor = "1", PricePerNight = 5000, MaxOccupancy = 2, Description = "Standard room with garden view", Amenities = "WiFi, TV, Air Conditioning, Mini Bar", IsAvailable = true, HousekeepingStatus = "Ready" },
                new Room { RoomNumber = "102", RoomType = "Standard", RoomTypeId = standardType?.RoomTypeId, Floor = "1", PricePerNight = 5000, MaxOccupancy = 2, Description = "Standard room with garden view", Amenities = "WiFi, TV, Air Conditioning, Mini Bar", IsAvailable = true, HousekeepingStatus = "Ready" },
                new Room { RoomNumber = "201", RoomType = "Deluxe", RoomTypeId = deluxeType?.RoomTypeId, Floor = "2", PricePerNight = 8000, MaxOccupancy = 3, Description = "Deluxe room with sea view", Amenities = "WiFi, Smart TV, Air Conditioning, Mini Bar, Balcony, Bathtub", IsAvailable = true, HousekeepingStatus = "Ready" },
                new Room { RoomNumber = "301", RoomType = "Suite", RoomTypeId = suiteType?.RoomTypeId, Floor = "3", PricePerNight = 15000, MaxOccupancy = 4, Description = "Suite with panoramic sea view", Amenities = "WiFi, Smart TV, Air Conditioning, Mini Bar, Living Room, Jacuzzi, Balcony", IsAvailable = false, HousekeepingStatus = "Clean" },
                new Room { RoomNumber = "401", RoomType = "Villa", RoomTypeId = villaType?.RoomTypeId, Floor = "Garden", PricePerNight = 30000, MaxOccupancy = 6, Description = "Luxury villa with private pool", Amenities = "WiFi, Smart TV, Air Conditioning, Full Kitchen, Private Pool, Garden, Multiple Bedrooms", IsAvailable = true, HousekeepingStatus = "Ready" }
            };
            await _context.Rooms.AddRangeAsync(rooms);
            await _context.SaveChangesAsync();
        }

        // Seed Services
        var restaurantMenuItems = new List<Service>
        {
            new Service { ServiceName = "Bò nướng kèm khoai tây", ServiceType = "Restaurant", Description = "Thịt bò tuyển chọn, nướng vừa chín, ăn kèm sốt đặc biệt", Price = 320000, Unit = "Phần", IsActive = true },
            new Service { ServiceName = "Mì Ý sốt kem nấm", ServiceType = "Restaurant", Description = "Sốt kem béo ngậy, nấm tươi và phô mai Parmesan", Price = 260000, Unit = "Phần", IsActive = true },
            new Service { ServiceName = "Salad cá hồi", ServiceType = "Restaurant", Description = "Cá hồi áp chảo, rau củ tươi và sốt chanh dây", Price = 280000, Unit = "Phần", IsActive = true },
            new Service { ServiceName = "Gà nướng thảo mộc", ServiceType = "Restaurant", Description = "Gà nướng cùng thảo mộc Địa Trung Hải, hương vị đậm đà", Price = 240000, Unit = "Phần", IsActive = true },
            new Service { ServiceName = "Bò bít tết sốt tiêu", ServiceType = "Restaurant", Description = "Thịt bò mềm mọng nước, sốt tiêu đen cay nhẹ", Price = 420000, Unit = "Phần", IsActive = true },
            new Service { ServiceName = "Hải sản tổng hợp", ServiceType = "Restaurant", Description = "Tôm, mực, nghêu tươi kèm sốt bơ tỏi thơm", Price = 390000, Unit = "Phần", IsActive = true },
            new Service { ServiceName = "Súp bí đỏ kem", ServiceType = "Restaurant", Description = "Súp mịn, béo nhẹ, dùng kèm bánh mì bơ tỏi", Price = 160000, Unit = "Phần", IsActive = true },
            new Service { ServiceName = "Bánh mì kẹp thịt nguội & dứa", ServiceType = "Restaurant", Description = "Hương vị cân bằng, phù hợp bữa nhẹ", Price = 180000, Unit = "Phần", IsActive = true },
            new Service { ServiceName = "Breakfast Buffet", ServiceType = "Restaurant", Description = "Bữa sáng buffet tự chọn đầy đủ", Price = 150000, Unit = "Người", IsActive = true }
        };

        // Seed restaurant menu items if missing
        var existingRestaurantServices = await _context.Services.Where(s => s.ServiceType == "Restaurant").ToListAsync();
        var existingNames = existingRestaurantServices.Select(s => s.ServiceName).ToHashSet();
        
        var newRestaurantItems = restaurantMenuItems
            .Where(item => !existingNames.Contains(item.ServiceName))
            .ToList();

        if (newRestaurantItems.Any())
        {
            await _context.Services.AddRangeAsync(newRestaurantItems);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[DataSeeder] Added {newRestaurantItems.Count} new restaurant menu items");
        }

        // Seed other services if Services table is empty
        if (!_context.Services.Any())
        {
            var otherServices = new List<Service>
            {
                new Service { ServiceName = "Spa Massage (60min)", ServiceType = "Spa", Description = "Traditional Vietnamese massage", Price = 400000, Unit = "Session", IsActive = true },
                new Service { ServiceName = "Airport Transfer", ServiceType = "Transport", Description = "Round-trip airport transfer", Price = 300000, Unit = "Trip", IsActive = true },
                new Service { ServiceName = "Laundry Service", ServiceType = "Laundry", Description = "Express laundry service", Price = 50000, Unit = "Kg", IsActive = true },
                new Service { ServiceName = "Room Service", ServiceType = "RoomService", Description = "24/7 room service", Price = 0, Unit = "Order", IsActive = true }
            };
            await _context.Services.AddRangeAsync(otherServices);
            await _context.SaveChangesAsync();
        }

        // Seed Bookings
        if (!_context.Bookings.Any())
        {
            var customers = _context.Customers.ToList();
            var rooms = _context.Rooms.ToList();

            var bookings = new List<Booking>
            {
                new Booking 
                { 
                    BookingCode = "BKG2025001", 
                    CustomerId = customers[0].CustomerId, 
                    RoomId = rooms[0].RoomId,
                    RequestedRoomType = "Standard", 
                    CheckInDate = DateTime.Today.AddDays(-2), 
                    CheckOutDate = DateTime.Today.AddDays(1), 
                    NumberOfGuests = 2, 
                    Status = "CheckedIn",
                    ActualCheckInTime = DateTime.Today.AddDays(-2).AddHours(14),
                    EstimatedTotalAmount = 15000, // 3 nights × 5000
                    Source = "Direct",
                    CreatedBy = "Customer"
                },
                new Booking 
                { 
                    BookingCode = "BKG2025002", 
                    CustomerId = customers[1].CustomerId, 
                    RoomId = rooms[2].RoomId,
                    RequestedRoomType = "Deluxe", 
                    CheckInDate = DateTime.Today, 
                    CheckOutDate = DateTime.Today.AddDays(3), 
                    NumberOfGuests = 2, 
                    Status = "Assigned",
                    EstimatedTotalAmount = 24000, // 3 nights × 8000
                    Source = "Online",
                    CreatedBy = "frontdesk@resort.test"
                },
                new Booking 
                { 
                    BookingCode = "BKG2025003", 
                    CustomerId = customers[2].CustomerId,
                    RequestedRoomType = "Suite", 
                    CheckInDate = DateTime.Today.AddDays(5), 
                    CheckOutDate = DateTime.Today.AddDays(8), 
                    NumberOfGuests = 4, 
                    Status = "Confirmed",
                    EstimatedTotalAmount = 45000, // 3 nights × 15000
                    Source = "Phone",
                    CreatedBy = "frontdesk@resort.test"
                }
            };
            await _context.Bookings.AddRangeAsync(bookings);
            await _context.SaveChangesAsync();
        }

        // Seed Invoices based on existing bookings and charges
        if (!_context.Invoices.Any())
        {
            var bookings = _context.Bookings
                .Include(b => b.Charges)
                .ToList();
            var customers = _context.Customers.ToList();

            if (bookings.Count > 0 && customers.Count > 0)
            {
                var invoices = new List<Invoice>();

                // Paid invoice for first booking
                var b1 = bookings.First();
                var totalB1 = b1.Charges?.Sum(c => c.TotalAmount) ?? 0;
                invoices.Add(new Invoice
                {
                    InvoiceNumber = "INV2025001",
                    BookingId = b1.BookingId,
                    CustomerId = b1.CustomerId,
                    IssueDate = DateTime.Today,
                    SubTotal = totalB1,
                    TaxAmount = 0,
                    DiscountAmount = 0,
                    TotalAmount = totalB1,
                    PaidAmount = totalB1,
                    BalanceDue = 0,
                    Status = "Paid",
                    PaidDate = DateTime.Today,
                    IssuedBy = "cashier@resort.test"
                });

                // Issued (unpaid) invoice for second booking if exists
                if (bookings.Count > 1)
                {
                    var b2 = bookings[1];
                    var est2 = b2.EstimatedTotalAmount ?? 0;
                    invoices.Add(new Invoice
                    {
                        InvoiceNumber = "INV2025002",
                        BookingId = b2.BookingId,
                        CustomerId = b2.CustomerId,
                        IssueDate = DateTime.Today,
                        SubTotal = est2,
                        TaxAmount = 0,
                        DiscountAmount = 0,
                        TotalAmount = est2,
                        PaidAmount = 0,
                        BalanceDue = est2,
                        Status = "Issued",
                        IssuedBy = "frontdesk@resort.test"
                    });
                }

                await _context.Invoices.AddRangeAsync(invoices);
                await _context.SaveChangesAsync();
            }
        }

        // Seed Inventory Vouchers
        if (!_context.InventoryVouchers.Any())
        {
            var vouchers = new List<InventoryVoucher>
            {
                new InventoryVoucher 
                { 
                    VoucherNumber = "VOUCHER2025001", 
                    VoucherType = "Purchase", 
                    ItemName = "Bed Linen Set", 
                    ItemCode = "LINEN001",
                    Category = "Linen", 
                    Quantity = 50, 
                    Unit = "Set",
                    UnitPrice = 200000, 
                    TotalAmount = 10000000, 
                    Supplier = "Luxury Linens Co.",
                    Department = "Housekeeping",
                    Status = "Approved",
                    CreatedBy = "inventory@resort.test"
                },
                new InventoryVoucher 
                { 
                    VoucherNumber = "VOUCHER2025002", 
                    VoucherType = "Consumption", 
                    ItemName = "Shampoo Bottles", 
                    ItemCode = "TOIL001",
                    Category = "Toiletries", 
                    Quantity = 100, 
                    Unit = "Bottle",
                    UnitPrice = 30000, 
                    TotalAmount = 3000000,
                    Department = "Housekeeping",
                    Status = "Approved",
                    CreatedBy = "inventory@resort.test"
                }
            };
            await _context.InventoryVouchers.AddRangeAsync(vouchers);
            await _context.SaveChangesAsync();
        }

        // Seed sample charges for checked-in booking
        if (!_context.Charges.Any())
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingCode == "BKG2025001");
            var service = _context.Services.FirstOrDefault(s => s.ServiceName == "Breakfast Buffet");
            
            if (booking != null && service != null)
            {
                var charges = new List<Charge>
                {
                    new Charge
                    {
                        BookingId = booking.BookingId,
                        RoomId = booking.RoomId,
                        ChargeType = "RoomCharge",
                        Description = "Room charges for 3 nights",
                        Amount = 5000,  // Fixed: 5,000 VND/đêm
                        Quantity = 3,
                        TotalAmount = 15000,  // Fixed: 15,000 VND (3 nights × 5,000)
                        ChargeDate = DateTime.Today.AddDays(-2),
                        CreatedBy = "frontdesk@resort.test"
                    },
                    new Charge
                    {
                        BookingId = booking.BookingId,
                        ServiceId = service.ServiceId,
                        ChargeType = "ServiceCharge",
                        Description = "Breakfast Buffet",
                        Amount = 1500, // per person
                        Quantity = 4,
                        TotalAmount = 6000, // 4 people × 1500
                        OutletName = "Main Restaurant",
                        ChargeDate = DateTime.Today.AddDays(-1),
                        CreatedBy = "frontdesk@resort.test"
                    }
                };
                await _context.Charges.AddRangeAsync(charges);
                await _context.SaveChangesAsync();
            }
        }

        // Seed sample notifications
        if (!_context.Notifications.Any())
        {
            var notifications = new List<Notification>
            {
                new Notification
                {
                    NotificationType = "Alert",
                    Title = "Booking Reminder",
                    Message = "BKG2025002 check-in today at 14:00",
                    Severity = "Medium",
                    TargetRole = "FrontDesk",
                    RelatedEntity = "Booking",
                    RelatedEntityId = 2
                },
                new Notification
                {
                    NotificationType = "Warning",
                    Title = "Low Inventory",
                    Message = "Shampoo bottles stock below minimum level",
                    Severity = "High",
                    TargetRole = "Inventory",
                    RelatedEntity = "InventoryVoucher"
                }
            };
            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();
        }

        // Seed FAQs
        if (!_context.FAQs.Any())
        {
            var faqs = new List<FAQ>
            {
                // Booking FAQs
                new FAQ
                {
                    Question = "Làm thế nào để đặt phòng?",
                    Answer = "Bạn có thể đặt phòng trực tuyến qua website của chúng tôi, gọi điện đến hotline +84 901 329 227, hoặc đến trực tiếp tại quầy lễ tân. Chúng tôi khuyến nghị đặt phòng trước ít nhất 3 ngày để đảm bảo có phòng trống.",
                    Category = "Booking",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedBy = "admin@resort.test",
                    CreatedAt = DateTime.UtcNow
                },
                new FAQ
                {
                    Question = "Tôi có thể hủy đặt phòng không?",
                    Answer = "Có, bạn có thể hủy đặt phòng. Nếu hủy trước 48 giờ so với thời gian check-in, bạn sẽ được hoàn tiền đầy đủ. Nếu hủy trong vòng 24-48 giờ, phí hủy là 50% giá trị đặt phòng. Hủy trong vòng 24 giờ sẽ không được hoàn tiền.",
                    Category = "Booking",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedBy = "admin@resort.test",
                    CreatedAt = DateTime.UtcNow
                },
                new FAQ
                {
                    Question = "Thời gian check-in và check-out là khi nào?",
                    Answer = "Thời gian check-in là từ 14:00 và check-out là trước 12:00. Nếu bạn cần check-in sớm hoặc check-out muộn, vui lòng liên hệ quầy lễ tân để được hỗ trợ (có thể phát sinh phụ phí).",
                    Category = "Booking",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedBy = "admin@resort.test",
                    CreatedAt = DateTime.UtcNow
                },
                // Payment FAQs
                new FAQ
                {
                    Question = "Các phương thức thanh toán được chấp nhận?",
                    Answer = "Chúng tôi chấp nhận thanh toán bằng tiền mặt, thẻ tín dụng/ghi nợ (Visa, Mastercard), chuyển khoản ngân hàng, QR code, hoặc charge vào phòng (nếu bạn đã có booking).",
                    Category = "Payment",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedBy = "admin@resort.test",
                    CreatedAt = DateTime.UtcNow
                },
                new FAQ
                {
                    Question = "Tôi có thể thanh toán trước khi đến không?",
                    Answer = "Có, bạn có thể thanh toán trước qua website sau khi đặt phòng thành công. Thanh toán trước giúp bạn đảm bảo phòng và có thể nhận được ưu đãi đặc biệt.",
                    Category = "Payment",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedBy = "admin@resort.test",
                    CreatedAt = DateTime.UtcNow
                },
                // Restaurant FAQs
                new FAQ
                {
                    Question = "Nhà hàng phục vụ những loại món ăn nào?",
                    Answer = "Nhà hàng của chúng tôi phục vụ đa dạng các món ăn từ ẩm thực Việt Nam truyền thống đến các món quốc tế. Chúng tôi có thực đơn cho bữa sáng, trưa, tối và đồ uống. Bạn có thể đặt món qua website hoặc gọi điện trực tiếp.",
                    Category = "Restaurant",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedBy = "admin@resort.test",
                    CreatedAt = DateTime.UtcNow
                },
                new FAQ
                {
                    Question = "Tôi có thể đặt món giao đến phòng không?",
                    Answer = "Có, chúng tôi cung cấp dịch vụ room service 24/7. Bạn có thể đặt món qua website, gọi điện đến nhà hàng, hoặc sử dụng dịch vụ đặt món trực tuyến. Phí giao hàng có thể áp dụng tùy theo thời gian và khoảng cách.",
                    Category = "Restaurant",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedBy = "admin@resort.test",
                    CreatedAt = DateTime.UtcNow
                },
                // Services FAQs
                new FAQ
                {
                    Question = "Resort có những tiện ích gì?",
                    Answer = "Resort của chúng tôi có đầy đủ tiện ích như hồ bơi, phòng gym, spa, nhà hàng, quầy bar, WiFi miễn phí, bãi đỗ xe, và nhiều dịch vụ khác. Vui lòng xem chi tiết tại trang Tiện ích trên website.",
                    Category = "Services",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedBy = "admin@resort.test",
                    CreatedAt = DateTime.UtcNow
                },
                new FAQ
                {
                    Question = "Resort có WiFi miễn phí không?",
                    Answer = "Có, chúng tôi cung cấp WiFi miễn phí tốc độ cao cho tất cả khách hàng trong toàn bộ khu vực resort. Thông tin đăng nhập WiFi sẽ được cung cấp khi bạn check-in.",
                    Category = "Services",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedBy = "admin@resort.test",
                    CreatedAt = DateTime.UtcNow
                },
                // General FAQs
                new FAQ
                {
                    Question = "Làm thế nào để liên hệ với resort?",
                    Answer = "Bạn có thể liên hệ với chúng tôi qua: Hotline: +84 901 329 227, Email: support@resortdeluxe.vn, hoặc đến trực tiếp tại địa chỉ: 123 Đường Biển Xanh, Thành phố Biển, Việt Nam. Chúng tôi phục vụ 24/7.",
                    Category = "General",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedBy = "admin@resort.test",
                    CreatedAt = DateTime.UtcNow
                },
                new FAQ
                {
                    Question = "Tôi có thể yêu cầu dịch vụ đặc biệt không?",
                    Answer = "Có, chúng tôi luôn sẵn sàng đáp ứng các yêu cầu đặc biệt của bạn như phòng không hút thuốc, giường phụ, dịch vụ spa, tổ chức sự kiện, v.v. Vui lòng liên hệ trước khi đến để chúng tôi có thể chuẩn bị tốt nhất.",
                    Category = "General",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedBy = "admin@resort.test",
                    CreatedAt = DateTime.UtcNow
                }
            };
            await _context.FAQs.AddRangeAsync(faqs);
            await _context.SaveChangesAsync();
        }
    }
}

