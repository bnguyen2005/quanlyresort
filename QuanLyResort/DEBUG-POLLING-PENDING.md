# 🐛 Debug: Polling Thấy "Pending" Nhưng Backend Đã "Paid"

## ❌ Vấn Đề

Backend đã update booking thành "Paid" (từ webhook logs), nhưng frontend polling vẫn thấy "Pending":

```
🔍 [SimplePolling] Booking status: Pending for booking: 6
```

## 🔍 Nguyên Nhân Có Thể

### 1. API Response Cache
- Browser hoặc API có thể cache response
- Cache buster `?_=${Date.now()}` có thể không đủ

### 2. Database Transaction Chưa Commit
- Webhook update nhưng transaction chưa commit
- API query trước khi commit

### 3. Case Sensitivity
- Status có thể là "Paid" (capital P) nhưng code check "paid" (lowercase)

### 4. Response Format
- API có thể trả về status dạng khác (string, enum, etc.)

## ✅ Đã Sửa

1. **Thêm logging chi tiết:**
   - Log full booking object
   - Log status type và raw value
   - Log trimmed và lowercase version

2. **Check multiple formats:**
   - `'paid'` (lowercase)
   - `'Paid'` (capital P)
   - `'PAID'` (uppercase)

## 🧪 Cách Test

### Bước 1: Refresh Browser
- Nhấn `Ctrl+F5` (hoặc `Cmd+Shift+R` trên Mac) để hard refresh
- Clear cache nếu cần

### Bước 2: Mở Payment Modal
- Mở payment modal cho booking 6
- Mở Console (F12)

### Bước 3: Kiểm Tra Logs
Console sẽ hiển thị:
```
🔍 [SimplePolling] Full booking response: { ... }
🔍 [SimplePolling] Booking status (raw): Paid Type: string
🔍 [SimplePolling] Booking status (trimmed): Paid
🔍 [SimplePolling] Booking status (lowercase): paid for booking: 6
```

### Bước 4: Kiểm Tra API Response
Trong Console, chạy:
```javascript
const token = localStorage.getItem('token');
fetch('/api/bookings/6', {
  headers: { 'Authorization': `Bearer ${token}` },
  cache: 'no-store'
})
.then(r => r.json())
.then(data => {
  console.log('API Response:', data);
  console.log('Status:', data.status);
  console.log('Status Type:', typeof data.status);
});
```

## 🐛 Nếu Vẫn Thấy "Pending"

### Option 1: Kiểm Tra Database
```sql
SELECT BookingId, BookingCode, Status 
FROM Bookings 
WHERE BookingId = 6;
```

Nếu Status = "Paid" trong database nhưng API trả về "Pending":
- ❌ Có thể có issue với Entity Framework caching
- ✅ Cần restart backend

### Option 2: Kiểm Tra API Controller
Xem `BookingController.GetBookingById` có filter hoặc transform status không.

### Option 3: Force Refresh
Thử manual trigger webhook lại:
```bash
curl -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content": "BOOKING-6", "amount": 5000}'
```

Sau đó chờ 5 giây và kiểm tra polling logs.

## 📝 Checklist

- [ ] Browser đã refresh (hard refresh với Ctrl+F5)
- [ ] Console logs hiển thị full booking object
- [ ] API response có status = "Paid" không?
- [ ] Database có Status = "Paid" không?
- [ ] Backend đã restart sau khi update code?

## ✅ Kết Luận

Nếu API response vẫn là "Pending" sau khi backend đã update:
- Có thể là Entity Framework caching issue
- Cần restart backend
- Hoặc có vấn đề với database transaction

Nếu API response là "Paid" nhưng polling vẫn không detect:
- Có thể là case sensitivity issue
- Hoặc status comparison logic có vấn đề
- Đã fix bằng cách check multiple formats

