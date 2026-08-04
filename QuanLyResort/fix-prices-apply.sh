#!/bin/bash

# Script để apply fix prices vào database
# Cách 1: Dùng SQL script (nếu dùng SQL Server)
# Cách 2: Re-seed database (nếu dùng SQLite hoặc muốn reset)

echo "🔧 Fix Room Prices in Database"
echo "=============================="
echo ""

DB_TYPE=${1:-"sqlite"}

if [ "$DB_TYPE" = "sqlite" ]; then
    echo "📋 Option 1: Re-seed database (SQLite)"
    echo ""
    echo "Steps:"
    echo "1. Delete database file:"
    echo "   rm -f QuanLyResort/bin/Debug/net8.0/ResortManagementDb.db"
    echo ""
    echo "2. Restart backend:"
    echo "   dotnet run"
    echo ""
    echo "   Database sẽ tự động được tạo lại với giá đúng (5,000 VND)"
    echo ""
elif [ "$DB_TYPE" = "sqlserver" ]; then
    echo "📋 Option 2: Run SQL script (SQL Server)"
    echo ""
    echo "1. Connect to database:"
    echo "   sqlcmd -S localhost -d ResortManagementDb -U sa -P YourPassword"
    echo ""
    echo "2. Run script:"
    echo "   :r fix-prices-database.sql"
    echo ""
    echo "Hoặc copy nội dung fix-prices-database.sql và chạy trong SQL Server Management Studio"
    echo ""
fi

echo "✅ After fixing prices:"
echo "   - Standard Room: 5,000 VND/đêm"
echo "   - Deluxe Room: 8,000 VND/đêm"
echo "   - Suite Room: 15,000 VND/đêm"
echo "   - Villa: 30,000 VND/đêm"
echo ""
echo "⚠️  Note: Existing bookings will have corrected amounts"
echo "   (divided by 100 if >= 1,000,000 VND)"

