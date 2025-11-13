# 🔧 Fix: SePay API 404 Error

## ❌ Lỗi Hiện Tại

```
POST https://my.sepay.vn/userapi/MB/SP-LIVE-LT39A334/orders
Status: 404 Not Found
```

## 🔍 Nguyên Nhân

URL đang dùng **MERCHANT ID** (`SP-LIVE-LT39A334`) trong path, nhưng SePay API có thể yêu cầu **Account ID** thực tế (số) thay vì MERCHANT ID.

## ✅ Giải Pháp

### Option 1: Dùng Account ID Thực Tế (Khuyến Nghị)

**MERCHANT ID** (`SP-LIVE-LT39A334`) và **Account ID** có thể khác nhau:
- **MERCHANT ID:** Dùng để xác định merchant (có prefix `SP-LIVE-`)
- **Account ID:** Dùng trong API URL path (thường là số, ví dụ: `5365`)

**Cập nhật Railway Variables:**

1. **Vào Railway Dashboard** → **Variables**
2. **Kiểm tra Account ID thực tế:**
   - Vào SePay Dashboard: https://my.sepay.vn
   - Tìm **Account ID** hoặc **User ID** (thường là số)
3. **Cập nhật biến:**
   ```
   Name:  SePay__AccountId
   Value: 5365  (hoặc Account ID thực tế từ SePay Dashboard)
   ```
   **KHÔNG dùng:** `SP-LIVE-LT39A334` (đây là MERCHANT ID, không phải Account ID)

### Option 2: Kiểm Tra SePay API Documentation

1. **Vào SePay Dashboard:** https://my.sepay.vn
2. **Vào phần API Documentation**
3. **Kiểm tra format URL:**
   - Có thể là: `/userapi/{bankCode}/{merchantId}/orders`
   - Hoặc: `/userapi/{bankCode}/{accountId}/orders`
   - Hoặc format khác

### Option 3: Thử URL Không Có Bank Code

Có thể SePay API không cần bank code trong URL:

```
POST https://my.sepay.vn/userapi/{accountId}/orders
```

Hoặc:

```
POST https://api.sepay.vn/v1/orders
```

## 🧪 Test Sau Khi Sửa

1. **Cập nhật `SePay__AccountId`** trong Railway
2. **Redeploy service**
3. **Kiểm tra logs:**
   ```
   [SEPAY] 🔍 API URL: https://my.sepay.vn/userapi/MB/5365/orders
   ```
4. **Test tạo QR code:**
   - Tạo booking mới
   - Click "Thanh toán"
   - Kiểm tra không còn lỗi 404

## 📝 Lưu Ý

1. **MERCHANT ID** (`SP-LIVE-LT39A334`) ≠ **Account ID** (`5365`)
2. **MERCHANT ID** dùng để xác định merchant
3. **Account ID** dùng trong API URL path
4. **Secret Key** (`spsk_live_...`) dùng cho Authorization header

## 🔗 Thông Tin Cần Kiểm Tra

1. **SePay Dashboard:** https://my.sepay.vn
   - Tìm **Account ID** hoặc **User ID**
   - Kiểm tra **API Documentation** để xem format URL đúng

2. **Railway Variables:**
   - `SePay__AccountId` = Account ID thực tế (số, không phải MERCHANT ID)
   - `SePay__ApiToken` = Secret Key (`spsk_live_...`)
   - `SePay__BankCode` = `MB` (hoặc bank code khác)

## 🐛 Nếu Vẫn Lỗi 404

1. **Liên hệ SePay Support:**
   - Email: support@sepay.vn
   - Hoặc qua SePay Dashboard

2. **Kiểm tra API Base URL:**
   - Có thể không phải `https://my.sepay.vn/userapi`
   - Có thể là `https://api.sepay.vn` hoặc URL khác

3. **Kiểm tra Bank Code:**
   - Có thể không phải `MB`
   - Có thể cần dùng bank code khác hoặc không cần

