#!/bin/bash

# Script kiểm tra booking 6 status
# Usage: ./check-booking-6-status.sh

echo "🔍 Kiểm Tra Booking 6 Status"
echo "================================"
echo ""

# Note: Cần token để gọi API, nhưng script này chỉ hiển thị hướng dẫn
echo "📝 Hướng dẫn kiểm tra:"
echo ""
echo "1️⃣ Trong Browser Console (F12), chạy:"
echo ""
echo "   const token = localStorage.getItem('token');"
echo "   fetch('/api/bookings/6', {"
echo "     headers: { 'Authorization': \`Bearer \${token}\` },"
echo "     cache: 'no-store'"
echo "   })"
echo "   .then(r => r.json())"
echo "   .then(data => {"
echo "     console.log('📊 Booking 6 Status:', data.status);"
echo "     console.log('📊 Full Booking:', data);"
echo "   });"
echo ""
echo "2️⃣ Kiểm tra trong Console logs:"
echo "   - Tìm '🔍 [SimplePolling] Full booking response'"
echo "   - Xem status value trong object"
echo ""
echo "3️⃣ Nếu status = 'Pending' nhưng backend đã update:"
echo "   - Có thể là Entity Framework caching"
echo "   - Cần restart backend"
echo "   - Hoặc database chưa được update"
echo ""
echo "4️⃣ Test lại webhook:"
echo "   curl -X POST http://localhost:5130/api/simplepayment/webhook \\"
echo "     -H 'Content-Type: application/json' \\"
echo "     -d '{\"content\": \"BOOKING-6\", \"amount\": 5000}'"
echo ""
echo "✅ Hoàn tất!"

