#!/bin/bash

TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI4IiwidW5pcXVlX25hbWUiOiJjdXN0b21lcjEiLCJlbWFpbCI6ImN1c3RvbWVyMUBndWVzdC50ZXN0Iiwicm9sZSI6IkN1c3RvbWVyIiwiQ3VzdG9tZXJJZCI6IjEiLCJFbXBsb3llZUlkIjoiIiwibmJmIjoxNzYyMjgxMzc3LCJleHAiOjE3NjIzNjc3NzcsImlhdCI6MTc2MjI4MTM3NywiaXNzIjoiUmVzb3J0TWFuYWdlbWVudEFQSSIsImF1ZCI6IlJlc29ydE1hbmFnZW1lbnRDbGllbnQifQ.ZQftE9b9GVcACupHHVfkFqjKh3sywUpoW-4zOHSAbEc"

BOOKING_ID=${1:-39}

echo "🧪 Testing Payment for Booking ID: $BOOKING_ID"
echo ""

# Test payment
echo "1️⃣  Testing Payment..."
RESPONSE=$(curl -s -X POST "http://localhost:5130/api/payment/test/$BOOKING_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json")

echo "Response: $RESPONSE"
echo ""

# Check database
echo "2️⃣  Checking Database..."
DB_CHECK=$(curl -s -X GET "http://localhost:5130/api/payment/test/db-check?bookingId=$BOOKING_ID" \
  -H "Authorization: Bearer $TOKEN")

echo "Database Check: $DB_CHECK"
echo ""

echo "✅ Test completed!"
