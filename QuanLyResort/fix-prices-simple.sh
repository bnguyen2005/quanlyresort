#!/bin/bash
# Script đơn giản để fix giá phòng
# Cách tốt nhất: Xóa .db và restart app

cd "$(dirname "$0")"

echo "🔧 Fix Room Prices - Simple Method"
echo ""
echo "✅ CÁCH TỐT NHẤT: Xóa database và restart app"
echo ""
echo "1. Stop backend (Ctrl+C)"
echo "2. Xóa file database:"
echo "   rm ResortDev.db"
echo "   # hoặc"
echo "   rm bin/Debug/net8.0/*.db"
echo ""
echo "3. Restart backend:"
echo "   dotnet run"
echo ""
echo "✅ Database sẽ tự động được tạo lại với giá đúng (5,000 VND)"
echo ""
echo "⚠️  Nếu muốn giữ dữ liệu, cần kiểm tra tên bảng thực tế trong database"
echo "   sqlite3 ResortDev.db '.tables'"

