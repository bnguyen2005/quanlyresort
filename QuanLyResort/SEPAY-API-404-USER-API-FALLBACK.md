# 🔧 SePay API 404 - User API Fallback

## 📋 Vấn Đề Từ Logs

**Tất cả Production API endpoints đều trả về 404:**

```
[SEPAY] ⚠️ Endpoint Production Standard trả về 404, thử endpoint tiếp theo
[SEPAY] ⚠️ Endpoint Production Merchant trả về 404, thử endpoint tiếp theo
[SEPAY] ⚠️ Endpoint Production Account trả về 404, thử endpoint tiếp theo
```

**Các endpoints đã thử:**
1. ❌ `https://pgapi.sepay.vn/api/v1/orders` → 404
2. ❌ `https://pgapi.sepay.vn/api/v1/merchants/SP-LIVE-LT39A334/orders` → 404
3. ❌ `https://pgapi.sepay.vn/api/v1/accounts/5365/orders` → 404

## ✅ Giải Pháp Đã Implement

**Code đã được cập nhật để tự động thử User API endpoints khi Production API không hoạt động:**

### Thứ Tự Thử Endpoints:

1. **Production Standard:** `https://pgapi.sepay.vn/api/v1/orders`
2. **Production Merchant:** `https://pgapi.sepay.vn/api/v1/merchants/SP-LIVE-LT39A334/orders`
3. **Production Account:** `https://pgapi.sepay.vn/api/v1/accounts/5365/orders`
4. **User API Bank+Account (Fallback):** `https://my.sepay.vn/userapi/MB/5365/orders` ← **MỚI**
5. **User API Merchant (Fallback):** `https://my.sepay.vn/userapi/SP-LIVE-LT39A334/orders` ← **MỚI**
6. **User API Account (Fallback):** `https://my.sepay.vn/userapi/5365/orders` ← **MỚI**

## 🔍 Kiểm Tra Sau Khi Deploy

### Bước 1: Đợi Railway Deploy Code Mới

**Code đã được commit và push. Railway sẽ tự động deploy.**

**Hoặc trigger deploy thủ công:**
- Railway Dashboard → Service → Deployments → Redeploy

### Bước 2: Test Tạo QR Code

1. **Vào website:** https://quanlyresort-production.up.railway.app
2. **Đăng nhập** với tài khoản customer
3. **Tạo booking mới**
4. **Click "Thanh toán"**

### Bước 3: Kiểm Tra Logs

**Railway Dashboard → Service → Logs**

**Tìm các dòng:**
- `[SEPAY] 🔄 Thử endpoint: Production Standard` → 404
- `[SEPAY] 🔄 Thử endpoint: Production Merchant` → 404
- `[SEPAY] 🔄 Thử endpoint: Production Account` → 404
- `[SEPAY] 🔄 Thử endpoint: User API Bank+Account (Fallback)` ← **MỚI - Phải có!**
- `[SEPAY] ✅ Đơn hàng tạo thành công với endpoint User API...` ← **Nếu thành công**

## 📊 User API Endpoints Sẽ Được Thử

### Endpoint 1: User API Bank+Account
```
POST https://my.sepay.vn/userapi/MB/5365/orders
```

**Request body:**
```json
{
    "amount": 5000,
    "order_code": "BOOKING4",
    "duration": 86400,
    "with_qrcode": true
}
```

**Lưu ý:** User API không cần `description` và `merchant_id` trong request body.

### Endpoint 2: User API Merchant
```
POST https://my.sepay.vn/userapi/SP-LIVE-LT39A334/orders
```

**Request body:**
```json
{
    "amount": 5000,
    "order_code": "BOOKING4",
    "duration": 86400,
    "with_qrcode": true
}
```

### Endpoint 3: User API Account
```
POST https://my.sepay.vn/userapi/5365/orders
```

**Request body:**
```json
{
    "amount": 5000,
    "order_code": "BOOKING4",
    "duration": 86400,
    "with_qrcode": true
}
```

## 🔍 So Sánh Production API vs User API

### Production API:
- **Base URL:** `https://pgapi.sepay.vn`
- **Endpoint:** `/api/v1/orders`
- **Request body:** Cần `description` và `merchant_id`
- **Status:** ❌ Trả về 404

### User API:
- **Base URL:** `https://my.sepay.vn`
- **Endpoint:** `/userapi/{bankCode}/{accountId}/orders`
- **Request body:** Không cần `description` và `merchant_id`
- **Status:** ✅ Có thể hoạt động

## 💡 Lưu Ý

1. **User API có thể hoạt động:** Ngay cả khi Production API trả về 404
2. **Request body khác:** User API không cần `description` và `merchant_id`
3. **Tự động fallback:** Code sẽ tự động thử User API nếu Production API không hoạt động
4. **Rate limiting:** Vẫn áp dụng (2 requests/second)

## ✅ Checklist

- [ ] Code đã được deploy lên Railway
- [ ] Test tạo QR code
- [ ] Kiểm tra logs có thử User API endpoints không
- [ ] Kiểm tra User API có hoạt động không
- [ ] Nếu User API thành công → QR code sẽ được tạo
- [ ] Nếu tất cả endpoints đều thất bại → Fallback sang static QR code

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Railway Logs:** Railway Dashboard → Service → Logs
- **Website:** https://quanlyresort-production.up.railway.app

## 🎯 Kết Luận

**Vấn đề:** Production API trả về 404 cho tất cả endpoints

**Giải pháp:** Code đã được cập nhật để tự động thử User API endpoints

**Sau khi deploy:**
- ✅ Code sẽ tự động thử User API nếu Production API không hoạt động
- ✅ User API có thể hoạt động với token và account ID của bạn
- ✅ Nếu User API thành công → QR code sẽ được tạo
- ✅ Nếu tất cả endpoints đều thất bại → Fallback sang static QR code

**Bước tiếp theo:**
1. Đợi Railway deploy code mới
2. Test tạo QR code
3. Kiểm tra logs để xem endpoint nào hoạt động
4. Nếu User API thành công → Vấn đề đã được giải quyết!

