#!/bin/bash
# Script để tìm booking ID thực tế để test webhook

echo "🔍 Finding available booking IDs..."
echo ""

# Thử các booking ID phổ biến
for id in 1 2 3 10 20 30 39 40 41 42 43 44 45; do
    response=$(curl -s http://localhost:5130/api/bookings/$id 2>/dev/null)
    if echo "$response" | grep -q "bookingId\|BookingCode"; then
        status=$(echo "$response" | python3 -c "import sys, json; d=json.load(sys.stdin); print(d.get('status', 'N/A'))" 2>/dev/null || echo "N/A")
        code=$(echo "$response" | python3 -c "import sys, json; d=json.load(sys.stdin); print(d.get('bookingCode', 'N/A'))" 2>/dev/null || echo "N/A")
        echo "✅ Booking ID: $id - Code: $code - Status: $status"
    fi
done

echo ""
echo "💡 Tip: Chọn booking có status 'Pending' hoặc 'Confirmed' để test webhook"

