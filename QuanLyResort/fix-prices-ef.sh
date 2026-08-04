#!/bin/bash
# Script để update giá phòng bằng Entity Framework
# Chạy: ./fix-prices-ef.sh

cd "$(dirname "$0")"

echo "🔧 Updating room prices using Entity Framework..."
echo ""

# Tạo file C# script tạm
cat > /tmp/update-prices.cs << 'EOF'
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using System.Linq;

var optionsBuilder = new DbContextOptionsBuilder<ResortDbContext>();
optionsBuilder.UseSqlite("Data Source=ResortDev.db");

using var context = new ResortDbContext(optionsBuilder.Options);

Console.WriteLine("🔧 Updating room prices...");

// Update RoomTypes
var standardRoom = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.TypeCode == "STD");
if (standardRoom != null) {
    Console.WriteLine($"Standard Room: {standardRoom.BasePrice} → 5000");
    standardRoom.BasePrice = 5000;
}

var deluxeRoom = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.TypeCode == "DLX");
if (deluxeRoom != null) {
    Console.WriteLine($"Deluxe Room: {deluxeRoom.BasePrice} → 8000");
    deluxeRoom.BasePrice = 8000;
    deluxeRoom.ExtraPersonCharge = 2000;
}

var suiteRoom = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.TypeCode == "SUT");
if (suiteRoom != null) {
    Console.WriteLine($"Suite Room: {suiteRoom.BasePrice} → 15000");
    suiteRoom.BasePrice = 15000;
    suiteRoom.ExtraPersonCharge = 3000;
}

var villaRoom = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.TypeCode == "VIL");
if (villaRoom != null) {
    Console.WriteLine($"Villa: {villaRoom.BasePrice} → 30000");
    villaRoom.BasePrice = 30000;
    villaRoom.ExtraPersonCharge = 5000;
}

// Update bookings
var bookings = await context.Bookings.Where(b => b.EstimatedTotalAmount >= 1000000).ToListAsync();
foreach (var b in bookings) {
    b.EstimatedTotalAmount = b.EstimatedTotalAmount / 100;
}

var changes = await context.SaveChangesAsync();
Console.WriteLine($"✅ Updated {changes} records!");
EOF

echo "⚠️  Cách đơn giản hơn: Xóa file .db và restart app để seed lại với giá mới"
echo ""
echo "Hoặc chạy SQL trực tiếp với tên bảng đúng:"
echo ""
echo "sqlite3 ResortDev.db \"UPDATE RoomTypes SET BasePrice = 5000 WHERE TypeCode = 'STD';\""

