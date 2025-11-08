# ✅ Test API Sau Khi Deploy Thành Công

## 🎉 Deploy Đã Thành Công!

Service đã **"Live"** trên Render!

## 🔍 Test Các Endpoints

### 1. Test Webhook Status (Public)

```bash
curl https://quanlyresort-api.onrender.com/api/simplepayment/webhook-status
```

**Kết quả mong đợi:**
```json
{
  "status": "ok",
  "message": "Webhook endpoint is ready"
}
```

### 2. Test Webhook Endpoint (POST)

```bash
curl -X POST https://quanlyresort-api.onrender.com/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "data": "BOOKING1",
    "amount": 5000,
    "description": "Test payment"
  }'
```

### 3. Test API Base URL

**Frontend cần cập nhật:**

Trong các file HTML/JS, thay:
```javascript
const API_BASE_URL = 'http://localhost:5130';
```

Thành:
```javascript
const API_BASE_URL = 'https://quanlyresort-api.onrender.com';
```

Hoặc tự động detect:
```javascript
const API_BASE_URL = window.location.origin.includes('localhost') 
  ? 'http://localhost:5130' 
  : 'https://quanlyresort-api.onrender.com';
```

## 📋 Checklist

- [ ] Webhook status endpoint hoạt động
- [ ] Database đã được tạo và seed
- [ ] API endpoints trả về data
- [ ] Frontend có thể kết nối API
- [ ] PayOs webhook có thể gửi request đến server

## 🔧 Cấu Hình PayOs Webhook

Sau khi deploy thành công, cần cấu hình PayOs webhook URL:

```
https://quanlyresort-api.onrender.com/api/simplepayment/webhook
```

**Cách cấu hình:**
1. Vào PayOs Dashboard
2. Tìm mục "Webhook Configuration"
3. Nhập URL: `https://quanlyresort-api.onrender.com/api/simplepayment/webhook`
4. Lưu cấu hình

Hoặc dùng API (nếu PayOs hỗ trợ):

```bash
curl -X POST https://api.payos.vn/v2/webhook-url \
  -H "Content-Type: application/json" \
  -H "x-client-id: YOUR_CLIENT_ID" \
  -H "x-api-key: YOUR_API_KEY" \
  -d '{
    "webhookUrl": "https://quanlyresort-api.onrender.com/api/simplepayment/webhook"
  }'
```

## 🎯 Test Thanh Toán Tự Động

1. **Tạo booking mới** trên frontend
2. **Quét QR code** và thanh toán
3. **Kiểm tra logs** trên Render:
   - Tab "Logs" → tìm dòng có "webhook" hoặc "payment"
4. **Kiểm tra database:**
   - Booking status phải đổi từ "Pending" → "Paid"
   - QR code phải biến mất trên frontend
   - Hiển thị "Thanh toán thành công"

## 📊 Monitor Logs

**Xem logs real-time:**
1. Vào Render Dashboard
2. Click service `quanlyresort-api`
3. Tab **"Logs"**
4. Tìm các dòng:
   - `✅ Database created and migrations applied`
   - `✅ Data seeded successfully`
   - `🔔 Webhook received` (khi có payment)

## ⚠️ Lưu Ý

- **Database:** SQLite file `resort.db` được tạo trong container
- **Persistence:** Nếu container restart, database vẫn giữ nguyên (trừ khi xóa volume)
- **Backup:** Nên backup database định kỳ nếu có data quan trọng

## 🎉 Hoàn Thành!

Nếu tất cả test đều pass → **Deploy thành công!**

