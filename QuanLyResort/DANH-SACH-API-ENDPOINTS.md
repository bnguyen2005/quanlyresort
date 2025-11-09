# 📋 Danh Sách API Endpoints Đang Sử Dụng

## ✅ Web ĐANG SỬ DỤNG Backend API

Web frontend đang gọi rất nhiều Backend API endpoints. Dưới đây là danh sách các API chính:

---

## 🔐 Authentication APIs

### Customer Authentication
- `POST /api/auth/customer-login` - Đăng nhập khách hàng
- `POST /api/auth/register-customer` - Đăng ký khách hàng mới
- `POST /api/auth/login` - Đăng nhập admin/staff (universal login)

**File sử dụng:**
- `customer-api.js`
- `customer/login.html`
- `customer/register.html`

---

## 📦 Booking APIs

### Booking Management
- `GET /api/bookings/my` - Lấy danh sách booking của customer hiện tại
- `GET /api/bookings/{id}` - Lấy chi tiết booking theo ID
- `POST /api/bookings` - Tạo booking mới
- `POST /api/bookings/{id}/cancel` - Hủy booking
- `POST /api/bookings/{id}/pay-online` - Thanh toán online

**File sử dụng:**
- `my-bookings.html`
- `booking-details.html`
- `room-detail.html`
- `customer-api.js`

---

## 💳 Payment APIs

### Simple Payment (PayOs)
- `POST /api/simplepayment/create-link` - Tạo PayOs payment link và QR code
- `POST /api/simplepayment/webhook` - Webhook nhận thông báo thanh toán từ PayOs
- `GET /api/simplepayment/webhook-status` - Kiểm tra trạng thái webhook

**File sử dụng:**
- `simple-payment.js` ⭐ (Quan trọng nhất)

### Payment Session
- `POST /api/payment/session/create` - Tạo payment session
- `GET /api/payment/status/{sessionId}` - Kiểm tra trạng thái payment
- `POST /api/payment/test/{bookingId}` - Test payment

**File sử dụng:**
- `payment-websocket.js`
- `my-bookings.html`

---

## 🏨 Room & Room Types APIs

### Room Types
- `GET /api/room-types` - Lấy danh sách loại phòng
- `GET /api/room-types/{id}` - Lấy chi tiết loại phòng

**File sử dụng:**
- `rooms.html`
- `room-detail.html`
- `my-bookings.html`

### Rooms
- `GET /api/rooms` - Lấy danh sách phòng (có filter theo roomTypeId)
- `GET /api/rooms/available` - Lấy danh sách phòng trống

**File sử dụng:**
- `rooms.html`
- `room-detail.html`
- `my-bookings.html`

---

## 🍽️ Restaurant APIs

### Restaurant Menu
- `GET /api/services/restaurant/menu` - Lấy menu nhà hàng

**File sử dụng:**
- `restaurant.html`

---

## 📊 Cấu Trúc API

### Base URL
```javascript
// Tự động detect từ current origin
const API_BASE_URL = `${window.location.origin}/api`;

// Ví dụ:
// - Local: http://localhost:5130/api
// - Production: https://quanlyresort.onrender.com/api
```

### Authentication
Tất cả API (trừ một số public endpoints) yêu cầu JWT token:
```javascript
headers: {
  'Authorization': `Bearer ${token}`,
  'Content-Type': 'application/json'
}
```

Token được lưu trong `localStorage.getItem('token')`

---

## 🔍 Các File JavaScript Chính Sử Dụng API

### 1. `customer-api.js`
- Helper functions cho tất cả API calls
- Wrapper `apiCall()` để xử lý authentication và errors
- Functions: `customerLogin()`, `customerRegister()`, `createBooking()`, etc.

### 2. `simple-payment.js` ⭐
- **Quan trọng nhất** - Xử lý thanh toán PayOs
- Gọi `POST /api/simplepayment/create-link` để tạo QR code
- Polling `GET /api/bookings/{id}` để check payment status

### 3. `payment-websocket.js`
- Xử lý payment session và WebSocket
- Gọi `POST /api/payment/session/create`

### 4. Các file HTML
- `my-bookings.html` - Gọi nhiều API để load bookings, rooms, room-types
- `booking-details.html` - Gọi API để load booking details
- `room-detail.html` - Gọi API để load room types và tạo booking

---

## 📝 Ví Dụ Sử Dụng API

### Ví dụ 1: Tạo PayOs Payment Link
```javascript
// File: simple-payment.js
const response = await fetch(`${location.origin}/api/simplepayment/create-link`, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({ bookingId: bookingId })
});
```

### Ví dụ 2: Load Bookings
```javascript
// File: my-bookings.html
const resp = await fetch(`${location.origin}/api/bookings/my`, {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
```

### Ví dụ 3: Polling Booking Status
```javascript
// File: simple-payment.js
const response = await fetch(`${location.origin}/api/bookings/${bookingId}`, {
  headers: {
    'Authorization': `Bearer ${token}`
  },
  cache: 'no-store'
});
```

---

## ⚠️ Lưu Ý Quan Trọng

1. **CORS Policy**: Backend phải cho phép CORS từ frontend domain
2. **Authentication**: Hầu hết API yêu cầu JWT token
3. **Error Handling**: API trả về 401 nếu token expired → redirect to login
4. **Cache**: Một số API calls dùng `cache: 'no-store'` để tránh cache

---

## 🎯 Kết Luận

**Web ĐANG SỬ DỤNG Backend API rất nhiều!**

- ✅ Frontend là **SPA (Single Page Application)** với static HTML/JS
- ✅ Tất cả data đều lấy từ Backend API
- ✅ Backend là **.NET Core Web API** (RESTful API)
- ✅ Communication: Frontend ↔ Backend qua HTTP/HTTPS

**Kiến trúc:**
```
Frontend (HTML/JS) 
    ↓ HTTP/HTTPS
Backend API (.NET Core)
    ↓
Database (SQLite/SQL Server)
```

