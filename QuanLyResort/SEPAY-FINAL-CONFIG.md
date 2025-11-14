# 🚀 Cấu Hình SePay Cuối Cùng - Production

## 📋 Thông Tin SePay Production

- **MERCHANT ID:** `SP-LIVE-LT39A334`
- **Account ID:** `5365`
- **Secret Key:** `spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb`

## 🔧 Các Biến Môi Trường Trong Railway

### Bước 1: Vào Railway Dashboard

1. **Mở Railway:** https://railway.app
2. **Chọn project** `quanlyresort`
3. **Vào tab "Variables"**

### Bước 2: Thêm/Cập Nhật Các Biến

#### ✅ Biến 1: API Base URL (Production API)
```
Name:  SePay__ApiBaseUrl
Value: https://pgapi.sepay.vn
```

#### ✅ Biến 2: API Token (Secret Key)
```
Name:  SePay__ApiToken
Value: spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb
```

#### ✅ Biến 3: Account ID
```
Name:  SePay__AccountId
Value: 5365
```

#### ✅ Biến 4: Merchant ID (Mới - Quan Trọng!)
```
Name:  SePay__MerchantId
Value: SP-LIVE-LT39A334
```

#### ✅ Biến 5: Bank Code (Optional)
```
Name:  SePay__BankCode
Value: MB
```

## 📝 Tổng Hợp Các Biến

Sau khi thêm, bạn sẽ có:

```
SePay__ApiBaseUrl = https://pgapi.sepay.vn
SePay__ApiToken = spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb
SePay__AccountId = 5365
SePay__MerchantId = SP-LIVE-LT39A334
SePay__BankCode = MB
```

## 🔍 API Endpoint Sau Khi Cấu Hình

**URL sẽ là:**
```
POST https://pgapi.sepay.vn/api/v1/orders
```

**Request Body:**
```json
{
  "amount": 5000,
  "order_code": "BOOKING4",
  "description": "Thanh toán đặt phòng 4",
  "duration": 86400,
  "with_qrcode": true,
  "merchant_id": "SP-LIVE-LT39A334"
}
```

**Headers:**
```
Authorization: Bearer spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb
Content-Type: application/json
```

## 🧪 Test Sau Khi Cấu Hình

1. **Cập nhật tất cả biến** trong Railway
2. **Redeploy service**
3. **Kiểm tra logs:**
   ```
   [SEPAY] 🔍 API URL: https://pgapi.sepay.vn/api/v1/orders
   [SEPAY] 🔍 Request body: {"amount":5000,"order_code":"BOOKING4","description":"Thanh toán đặt phòng 4","duration":86400,"with_qrcode":true,"merchant_id":"SP-LIVE-LT39A334"}
   [SEPAY] 🔍 Authorization header: Bearer spsk_live_eofJdy5...
   ```
4. **Test tạo QR code:**
   - Tạo booking mới
   - Click "Thanh toán"
   - Kiểm tra QR code hiển thị

## ✅ Checklist

- [ ] Đã thêm `SePay__ApiBaseUrl` = `https://pgapi.sepay.vn`
- [ ] Đã thêm `SePay__ApiToken` = `spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb`
- [ ] Đã thêm `SePay__AccountId` = `5365`
- [ ] Đã thêm `SePay__MerchantId` = `SP-LIVE-LT39A334` ⭐ **MỚI**
- [ ] Đã thêm `SePay__BankCode` = `MB` (optional)
- [ ] Railway đã redeploy thành công
- [ ] Không còn lỗi 404 trong logs
- [ ] QR code hiển thị thành công

## 🐛 Troubleshooting

### Nếu vẫn lỗi 404:

1. **Kiểm tra SePay Dashboard:**
   - Vào https://my.sepay.vn
   - Kiểm tra **API Documentation**
   - Xem endpoint chính xác

2. **Thử Basic Auth:**
   - SePay có thể yêu cầu Basic Auth
   - Format: `base64(merchant_id:secret_key)`
   - Cần cập nhật code nếu cần

3. **Liên hệ SePay Support:**
   - Email: support@sepay.vn
   - Hoặc qua SePay Dashboard

## 📝 Lưu Ý Quan Trọng

1. **MERCHANT ID** (`SP-LIVE-LT39A334`) ≠ **Account ID** (`5365`)
2. **MERCHANT ID** dùng trong request body
3. **Account ID** có thể dùng trong URL (nếu dùng User API)
4. **Secret Key** (`spsk_live_...`) dùng cho Authorization header
5. **Production API** (`pgapi.sepay.vn`) là API chính thức

## 🎯 Kết Quả Mong Đợi

Sau khi cấu hình đúng:
- ✅ API gọi thành công (không còn 404)
- ✅ QR code được tạo và hiển thị
- ✅ Webhook nhận thông báo thanh toán
- ✅ Trạng thái booking tự động cập nhật

