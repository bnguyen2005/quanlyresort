# 🔧 Fix: SePay API 404 - Cập Nhật Endpoint

## ❌ Vấn Đề

SePay API trả về 404 với URL:
```
POST https://my.sepay.vn/userapi/MB/5365/orders
```

## ✅ Giải Pháp

SePay có 2 loại API:
1. **Production API:** `https://pgapi.sepay.vn/api/v1/orders`
2. **User API:** `https://my.sepay.vn/userapi/{bankCode}/{accountId}/orders`

Code đã được cập nhật để tự động detect và dùng đúng endpoint.

## 🔧 Cập Nhật Railway Variables

### Option 1: Dùng Production API (Khuyến Nghị)

1. **Vào Railway Dashboard** → **Variables**
2. **Cập nhật `SePay__ApiBaseUrl`:**
   ```
   Name:  SePay__ApiBaseUrl
   Value: https://pgapi.sepay.vn
   ```
   **Hoặc xóa biến này** (code sẽ dùng mặc định `https://pgapi.sepay.vn`)

3. **Các biến khác giữ nguyên:**
   ```
   SePay__ApiToken = spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb
   SePay__AccountId = 5365
   SePay__BankCode = MB
   ```

### Option 2: Dùng User API (Nếu Production API không hoạt động)

1. **Vào Railway Dashboard** → **Variables**
2. **Cập nhật `SePay__ApiBaseUrl`:**
   ```
   Name:  SePay__ApiBaseUrl
   Value: https://my.sepay.vn/userapi
   ```

## 📋 Format URL Sau Khi Cập Nhật

### Production API:
```
POST https://pgapi.sepay.vn/api/v1/orders
```

### User API:
```
POST https://my.sepay.vn/userapi/MB/5365/orders
```

## 🧪 Test Sau Khi Cập Nhật

1. **Cập nhật `SePay__ApiBaseUrl`** trong Railway
2. **Redeploy service**
3. **Kiểm tra logs:**
   ```
   [SEPAY] 🔍 API URL: https://pgapi.sepay.vn/api/v1/orders
   ```
4. **Test tạo QR code:**
   - Tạo booking mới
   - Click "Thanh toán"
   - Kiểm tra không còn lỗi 404

## 🔍 Kiểm Tra Logs

Sau khi deploy, kiểm tra logs sẽ thấy:
```
[SEPAY] 🔍 API URL: https://pgapi.sepay.vn/api/v1/orders, AccountId: 5365, BankCode: MB, ApiBaseUrl: https://pgapi.sepay.vn
[SEPAY] 🔍 Request body: {"amount":5000,"order_code":"BOOKING4","duration":86400,"with_qrcode":true}
[SEPAY] 🔍 Authorization header: Bearer spsk_live_eofJdy5...
```

## 🐛 Nếu Vẫn Lỗi 404

1. **Kiểm tra SePay Dashboard:**
   - Vào https://my.sepay.vn
   - Kiểm tra **API Documentation**
   - Xem endpoint chính xác

2. **Thử Basic Auth:**
   - SePay có thể yêu cầu Basic Auth thay vì Bearer token
   - Format: `base64(merchant_id:secret_key)`
   - Cần cập nhật code nếu cần

3. **Liên hệ SePay Support:**
   - Email: support@sepay.vn
   - Hoặc qua SePay Dashboard

## 📝 Lưu Ý

1. **Production API** (`pgapi.sepay.vn`) là API chính thức cho production
2. **User API** (`my.sepay.vn/userapi`) có thể là API cũ hoặc cho user management
3. Code tự động detect format URL dựa trên `ApiBaseUrl`
4. Nếu không set `ApiBaseUrl`, mặc định sẽ dùng Production API

