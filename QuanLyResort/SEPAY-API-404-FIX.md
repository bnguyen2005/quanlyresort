# 🔧 Fix SePay API 404 Error

## 📋 Vấn Đề

**Logs cho thấy:**
```
[SEPAY] 🔍 API URL: https://pgapi.sepay.vn/api/v1/orders, AccountId: 5365, BankCode: MB
[SEPAY] 🔍 Request body: {"amount":5000,"order_code":"BOOKING4","duration":86400,"with_qrcode":true}
[SEPAY] ❌ SePay API error: Status=NotFound, Response=
```

**API trả về 404 Not Found** khi gọi `POST https://pgapi.sepay.vn/api/v1/orders`

## 🔍 Nguyên Nhân Có Thể

### 1. **Thiếu merchant_id trong Request Body**

**Code hiện tại:**
```csharp
var prodBody = new Dictionary<string, object>
{
    { "amount", (long)(amount) },
    { "order_code", orderCode },
    { "description", description },
    { "duration", durationSeconds },
    { "with_qrcode", true }
};

// Thêm merchant_id nếu có
if (!string.IsNullOrEmpty(_merchantId))
{
    prodBody["merchant_id"] = _merchantId;
}
```

**Vấn đề:** Nếu `_merchantId` chưa được cấu hình, request body sẽ thiếu `merchant_id`.

### 2. **API Endpoint Không Đúng**

**Có thể SePay yêu cầu:**
- Format khác: `/api/v1/merchants/{merchant_id}/orders`
- Hoặc cần thêm path: `/api/v1/accounts/{account_id}/orders`

### 3. **AccountId Không Đúng**

**AccountId hiện tại:** `5365`
- Có thể đây không phải là CLIENT_ID mà là Account ID khác
- SePay có thể yêu cầu CLIENT_ID khác với Account ID

### 4. **Authorization Header Không Đúng**

**Hiện tại:** `Bearer spsk_live_eofJdy5CA7...`
- Có thể cần format khác
- Hoặc cần thêm headers khác

## ✅ Giải Pháp

### Bước 1: Kiểm Tra Các Biến Môi Trường

**Railway Dashboard → Variables**

**Kiểm tra các biến sau:**

#### ✅ Biến 1: API_KEY (Bắt buộc)
```
Name:  SEPAY_API_KEY
Value: spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb
```
**Hoặc:**
```
Name:  SePay__ApiToken
Value: spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb
```

#### ✅ Biến 2: CLIENT_ID (Bắt buộc)
```
Name:  SEPAY_CLIENT_ID
Value: 5365
```
**Hoặc:**
```
Name:  SePay__AccountId
Value: 5365
```

#### ✅ Biến 3: MERCHANT_ID (Quan trọng cho Production API)
```
Name:  SePay__MerchantId
Value: SP-LIVE-LT39A334
```

**Lưu ý:** MERCHANT_ID có thể BẮT BUỘC cho Production API!

#### ✅ Biến 4: API_BASE_URL
```
Name:  SePay__ApiBaseUrl
Value: https://pgapi.sepay.vn
```

### Bước 2: Kiểm Tra Request Body

**Request body hiện tại:**
```json
{
    "amount": 5000,
    "order_code": "BOOKING4",
    "duration": 86400,
    "with_qrcode": true
}
```

**Request body cần có (nếu có merchant_id):**
```json
{
    "amount": 5000,
    "order_code": "BOOKING4",
    "description": "BOOKING4",
    "duration": 86400,
    "with_qrcode": true,
    "merchant_id": "SP-LIVE-LT39A334"
}
```

### Bước 3: Kiểm Tra API Endpoint

**Có thể SePay yêu cầu endpoint khác:**

#### Option 1: Production API với Merchant ID
```
POST https://pgapi.sepay.vn/api/v1/merchants/{merchant_id}/orders
```

#### Option 2: Production API với Account ID
```
POST https://pgapi.sepay.vn/api/v1/accounts/{account_id}/orders
```

#### Option 3: User API
```
POST https://my.sepay.vn/userapi/{bankCode}/{accountId}/orders
```

### Bước 4: Kiểm Tra SePay Dashboard

1. **Vào SePay Dashboard:** https://my.sepay.vn
2. **Menu:** **API** hoặc **Cài đặt → API**
3. **Xem:**
   - **API Endpoint:** URL chính xác để tạo order
   - **Request Format:** Format request body
   - **Required Fields:** Các trường bắt buộc

## 🔧 Cách Sửa

### Sửa 1: Đảm Bảo merchant_id Được Thêm Vào Request

**Code hiện tại đã có check, nhưng cần đảm bảo:**
- `SePay__MerchantId` đã được set trong Railway
- `_merchantId` không null khi tạo request

### Sửa 2: Thử Endpoint Khác

**Nếu Production API không hoạt động, thử User API:**

```
POST https://my.sepay.vn/userapi/MB/5365/orders
```

**Hoặc:**
```
POST https://my.sepay.vn/userapi/SP-LIVE-LT39A334/orders
```

### Sửa 3: Kiểm Tra Authorization

**Có thể cần format khác:**
- `Authorization: Bearer {token}`
- `X-API-Key: {token}`
- `X-Auth-Token: {token}`

## 📋 Checklist

- [ ] SEPAY_API_KEY đã được cấu hình trong Railway
- [ ] SEPAY_CLIENT_ID đã được cấu hình trong Railway
- [ ] SePay__MerchantId đã được cấu hình trong Railway (QUAN TRỌNG!)
- [ ] SePay__ApiBaseUrl đã được cấu hình (nếu cần)
- [ ] Request body có chứa merchant_id (nếu có)
- [ ] API endpoint đúng theo SePay documentation
- [ ] Authorization header đúng format

## 🔍 Debug

### Kiểm Tra Logs

**Railway Dashboard → Service → Logs**

**Tìm các dòng:**
- `[SEPAY] 🔍 API URL: ...`
- `[SEPAY] 🔍 Request body: ...`
- `[SEPAY] 🔍 Authorization header: ...`
- `[SEPAY] ❌ SePay API error: ...`

### Test Thủ Công

**Dùng curl để test API:**

```bash
curl -X POST https://pgapi.sepay.vn/api/v1/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb" \
  -d '{
    "amount": 5000,
    "order_code": "BOOKING4",
    "description": "BOOKING4",
    "duration": 86400,
    "with_qrcode": true,
    "merchant_id": "SP-LIVE-LT39A334"
  }'
```

**Nếu vẫn 404, thử endpoint khác:**
```bash
curl -X POST https://my.sepay.vn/userapi/MB/5365/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb" \
  -d '{
    "amount": 5000,
    "order_code": "BOOKING4",
    "duration": 86400,
    "with_qrcode": true
  }'
```

## 💡 Lưu Ý

1. **MERCHANT_ID:** Có thể BẮT BUỘC cho Production API
2. **API Endpoint:** Có thể khác tùy theo loại tài khoản SePay
3. **Request Format:** Cần đúng theo SePay documentation
4. **Authorization:** Cần đúng format và token hợp lệ

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn
- **Railway Dashboard:** https://railway.app
- **Railway Variables:** Railway Dashboard → Variables

