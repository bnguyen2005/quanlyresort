# 🔧 Fix Lỗi Không Đặt Được Phòng

## 🐛 Vấn Đề

Không đặt được phòng từ frontend.

## 🔍 Nguyên Nhân Có Thể

### 1. Chưa Đăng Nhập (Không Có JWT Token)

**API endpoint `/api/bookings` POST yêu cầu:**
- `[Authorize]` - Cần JWT token
- Frontend phải gửi `Authorization: Bearer {token}`

**Triệu chứng:**
- Response: `401 Unauthorized`
- Logs: `[Authorization] ❌ Unauthorized request to: /api/bookings`

**Fix:**
- Đảm bảo user đã đăng nhập
- Kiểm tra token có được lưu trong localStorage không
- Kiểm tra token có hết hạn không

### 2. CustomerId Không Tồn Tại

**API kiểm tra:**
```csharp
var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == request.CustomerId);
if (!customerExists)
{
    return BadRequest(new { message = $"CustomerId {request.CustomerId} không tồn tại trong hệ thống" });
}
```

**Triệu chứng:**
- Response: `400 Bad Request`
- Message: `CustomerId X không tồn tại trong hệ thống`

**Fix:**
- Đảm bảo customer đã được tạo trong database
- Kiểm tra `customerId` trong request có đúng không

### 3. Token Hết Hạn

**Triệu chứng:**
- Response: `401 Unauthorized`
- Logs: `Token validation failed`

**Fix:**
- Đăng nhập lại để lấy token mới
- Kiểm tra token expiration time

### 4. Frontend Không Gửi Token

**Triệu chứng:**
- Response: `401 Unauthorized`
- Request headers không có `Authorization`

**Fix:**
- Kiểm tra frontend code có gửi token không
- Kiểm tra token có được lưu trong localStorage không

## ✅ Cách Kiểm Tra

### Bước 1: Kiểm Tra Logs Railway

**Vào Railway Dashboard → Logs**

**Tìm khi user đặt phòng:**

**Nếu chưa đăng nhập:**
```
[Authorization] ❌ Unauthorized request to: /api/bookings
```

**Nếu CustomerId không tồn tại:**
```
❌ [CreateBooking] CustomerId X does not exist in database
```

**Nếu thành công:**
```
[Authorization] ✅ Allowing authorized request to: /api/bookings
✅ [CreateBooking] Booking created successfully
```

### Bước 2: Test API Trực Tiếp

**1. Đăng nhập để lấy token:**
```bash
curl -X POST "https://quanlyresort-production.up.railway.app/api/auth/customer-login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "customer1@guest.test",
    "password": "Password123!"
  }'
```

**2. Lấy token từ response và test đặt phòng:**
```bash
TOKEN="your-jwt-token-here"

curl -X POST "https://quanlyresort-production.up.railway.app/api/bookings" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "customerId": 1,
    "requestedRoomType": "Standard",
    "checkInDate": "2025-11-20T00:00:00Z",
    "checkOutDate": "2025-11-22T00:00:00Z",
    "numberOfGuests": 2
  }'
```

### Bước 3: Kiểm Tra Frontend

**Mở Browser Console (F12) và tìm:**

**Khi đặt phòng:**
```javascript
🔵 [submitBooking] Submitting: {...}
🔵 [submitBooking] Response status: 200
✅ [submitBooking] Booking created: {...}
```

**Nếu có lỗi:**
```javascript
❌ [submitBooking] API Error: ...
```

## 🔧 Giải Pháp

### Fix 1: Đảm Bảo User Đã Đăng Nhập

**Kiểm tra:**
1. User có đăng nhập không?
2. Token có trong localStorage không?
3. Token có hết hạn không?

**Fix:**
- Yêu cầu user đăng nhập trước khi đặt phòng
- Kiểm tra token trước khi gửi request
- Refresh token nếu hết hạn

### Fix 2: Tạo Customer Nếu Chưa Có

**Nếu CustomerId không tồn tại:**

**Option 1: Tạo customer trước khi đặt phòng**
```javascript
// Tạo customer trước
const customerResp = await fetch('/api/customers', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({
    fullName: fullName,
    email: email,
    phoneNumber: phone
  })
});

const customer = await customerResp.json();
const customerId = customer.customerId;
```

**Option 2: Dùng customer ID từ JWT token**
```javascript
// Lấy customerId từ JWT token
const tokenData = JSON.parse(atob(token.split('.')[1]));
const customerId = tokenData.customerId;
```

### Fix 3: Kiểm Tra Token Trước Khi Gửi Request

**Frontend code:**
```javascript
// Kiểm tra token trước khi đặt phòng
const token = localStorage.getItem('token');
if (!token) {
  showToast('Vui lòng đăng nhập để đặt phòng', 'warning');
  window.location.href = '/customer/login.html';
  return;
}

// Kiểm tra token hết hạn
const tokenData = JSON.parse(atob(token.split('.')[1]));
const expirationTime = tokenData.exp * 1000;
if (Date.now() >= expirationTime) {
  showToast('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại', 'warning');
  localStorage.removeItem('token');
  window.location.href = '/customer/login.html';
  return;
}
```

## 📋 Checklist

- [ ] User đã đăng nhập (có token)
- [ ] Token chưa hết hạn
- [ ] CustomerId tồn tại trong database
- [ ] Frontend gửi token trong Authorization header
- [ ] API endpoint trả về 200 OK
- [ ] Booking được tạo thành công

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Service Logs:** Railway Dashboard → Logs
- **API Endpoint:** `https://quanlyresort-production.up.railway.app/api/bookings`
- **Customer Login:** `https://quanlyresort-production.up.railway.app/customer/login.html`

## 💡 Lưu Ý

1. **Authentication** - API đặt phòng yêu cầu JWT token
2. **CustomerId** - Phải tồn tại trong database
3. **Token expiration** - Token có thể hết hạn, cần refresh
4. **Frontend validation** - Kiểm tra token trước khi gửi request

## 🎯 Kết Luận

**Vấn đề thường gặp:**
- User chưa đăng nhập (không có token)
- CustomerId không tồn tại
- Token hết hạn

**Bước tiếp theo:**
1. Kiểm tra logs Railway để xem lỗi cụ thể
2. Test API trực tiếp với token
3. Fix frontend nếu cần

