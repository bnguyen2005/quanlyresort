using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;

namespace QuanLyResort.Scripts;

/// <summary>
/// Script để update giá phòng trong database
/// Chạy: dotnet run --project QuanLyResort -- UpdateRoomPrices
/// </summary>
public class UpdateRoomPrices
{
    public static async Task RunAsync(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ResortDbContext>();
        optionsBuilder.UseSqlite("Data Source=ResortDev.db");
        
        using var context = new ResortDbContext(optionsBuilder.Options);
        
        Console.WriteLine("🔧 Updating room prices in database...");
        
        // Update RoomTypes
        var standardRoom = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.TypeCode == "STD");
        if (standardRoom != null)
        {
            Console.WriteLine($"✅ Found Standard Room: BasePrice = {standardRoom.BasePrice} → 5000");
            standardRoom.BasePrice = 5000;
        }
        
        var deluxeRoom = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.TypeCode == "DLX");
        if (deluxeRoom != null)
        {
            Console.WriteLine($"✅ Found Deluxe Room: BasePrice = {deluxeRoom.BasePrice} → 8000, ExtraPersonCharge = {deluxeRoom.ExtraPersonCharge} → 2000");
            deluxeRoom.BasePrice = 8000;
            deluxeRoom.ExtraPersonCharge = 2000;
        }
        
        var suiteRoom = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.TypeCode == "SUT");
        if (suiteRoom != null)
        {
            Console.WriteLine($"✅ Found Suite Room: BasePrice = {suiteRoom.BasePrice} → 15000, ExtraPersonCharge = {suiteRoom.ExtraPersonCharge} → 3000");
            suiteRoom.BasePrice = 15000;
            suiteRoom.ExtraPersonCharge = 3000;
        }
        
        var villaRoom = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.TypeCode == "VIL");
        if (villaRoom != null)
        {
            Console.WriteLine($"✅ Found Villa: BasePrice = {villaRoom.BasePrice} → 30000, ExtraPersonCharge = {villaRoom.ExtraPersonCharge} → 5000");
            villaRoom.BasePrice = 30000;
            villaRoom.ExtraPersonCharge = 5000;
        }
        
        // Update existing bookings (chia 100 nếu >= 1M)
        var bookingsToUpdate = await context.Bookings
            .Where(b => b.EstimatedTotalAmount >= 1000000)
            .ToListAsync();
        
        if (bookingsToUpdate.Any())
        {
            Console.WriteLine($"✅ Updating {bookingsToUpdate.Count} bookings...");
            foreach (var booking in bookingsToUpdate)
            {
                var oldAmount = booking.EstimatedTotalAmount;
                booking.EstimatedTotalAmount = booking.EstimatedTotalAmount / 100;
                Console.WriteLine($"   Booking {booking.BookingId}: {oldAmount} → {booking.EstimatedTotalAmount}");
            }
        }
        
        // Save changes
        var changes = await context.SaveChangesAsync();
        Console.WriteLine($"✅ Saved {changes} changes to database!");
        
        // Verify
        Console.WriteLine("\n📊 Verification:");
        var roomTypes = await context.RoomTypes.ToListAsync();
        foreach (var rt in roomTypes)
        {
            Console.WriteLine($"   {rt.TypeName} ({rt.TypeCode}): BasePrice = {rt.BasePrice:N0} VND");
        }
    }
}

