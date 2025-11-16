# ✅ Review Cấu Hình SePay Railway Variables

## 📋 Cấu Hình Hiện Tại Của Bạn

**Từ Railway Dashboard:**

| Tên Biến | Giá Trị | Trạng Thái |
|----------|---------|------------|
| `SePay__AccountId` | `5365` | ✅ Đúng |
| `SePay__ApiBaseUrl` | `https://pgapi.sepay.vn` | ✅ Đúng |
| `SePay__ApiToken` | `spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb` | ✅ Đúng |
| `SePay__BankAccountNumber` | `0901329227` | ✅ Đúng |
| `SePay__BankCode` | `MB` | ✅ Đúng |
| `SEPAY_WEBHOOK_URL` | `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook` | ✅ Đúng |
| `SePayMerchantId` | `SP-LIVE-LT39A334` | ⚠️ Format sai (nhưng code đã hỗ trợ) |

## ⚠️ Vấn Đề Phát Hiện

**Tên biến `SePayMerchantId` không đúng format chuẩn!**

**Format đúng:** `SePay__MerchantId` (với **2 dấu gạch dưới**)

**Format hiện tại:** `SePayMerchantId` (không có dấu gạch dưới)

## ✅ Giải Pháp

### Option 1: Sửa Tên Biến (Khuyến Nghị)

**Trong Railway Dashboard → Variables:**

1. **Xóa biến cũ:**
   - Tìm `SePayMerchantId`
   - Click "Delete" hoặc "Remove"

2. **Thêm biến mới:**
   ```
   Name:  SePay__MerchantId
   Value: SP-LIVE-LT39A334
   ```

**Lưu ý:** Phải có **2 dấu gạch dưới** (`__`) giữa `SePay` và `MerchantId`!

### Option 2: Giữ Nguyên (Tạm Thời)

**Code đã được cập nhật để hỗ trợ cả 2 format:**
- ✅ `SePay__MerchantId` (format đúng)
- ✅ `SePayMerchantId` (format sai - fallback)

**Nhưng khuyến nghị:** Nên sửa thành format đúng để nhất quán với các biến khác.

## 📊 Mapping Biến

**Code sẽ đọc như sau:**

```csharp
_merchantId = _configuration["SePay:MerchantId"]      // Từ SePay__MerchantId
           ?? _configuration["SePayMerchantId"];      // Fallback từ SePayMerchantId
```

**Environment variable mapping:**
- `SePay__MerchantId` → `SePay:MerchantId` ✅
- `SePayMerchantId` → `SePayMerchantId` (fallback) ⚠️

## ✅ Checklist Cấu Hình

### Đã Đúng:
- [x] `SePay__AccountId` = `5365`
- [x] `SePay__ApiBaseUrl` = `https://pgapi.sepay.vn`
- [x] `SePay__ApiToken` = `spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb`
- [x] `SePay__BankAccountNumber` = `0901329227`
- [x] `SePay__BankCode` = `MB`
- [x] `SEPAY_WEBHOOK_URL` = `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

### Cần Sửa (Khuyến Nghị):
- [ ] `SePayMerchantId` → Đổi thành `SePay__MerchantId` = `SP-LIVE-LT39A334`

## 🧪 Kiểm Tra Sau Khi Cấu Hình

### Bước 1: Kiểm Tra Logs

**Railway Dashboard → Service → Logs**

**Tìm các dòng:**
- `[SEPAY] 🔍 Client ID configured: 5365`
- `[SEPAY] 🔍 API Key configured: spsk_live_eofJdy5CA7...`
- `[SEPAY] 🔍 Merchant ID configured: SP-LIVE-LT39A334` ← **Phải có dòng này!**

**Nếu không thấy:**
- Kiểm tra lại tên biến
- Restart service trên Railway

### Bước 2: Test Tạo QR Code

1. **Vào website:** https://quanlyresort-production.up.railway.app
2. **Đăng nhập** với tài khoản customer
3. **Tạo booking mới**
4. **Click "Thanh toán"**
5. **Kiểm tra logs:**
   - `[SEPAY] 🔍 Added merchant_id to request: SP-LIVE-LT39A334` ← **Phải có!**
   - `[SEPAY] 🔄 Thử endpoint: Production Standard`
   - `[SEPAY] 🔄 Thử endpoint: Production Merchant`

### Bước 3: Kiểm Tra API Response

**Nếu API trả về 404:**
- Kiểm tra logs xem endpoint nào được thử
- Kiểm tra request body có `merchant_id` không
- Kiểm tra SePay Dashboard để xác định endpoint chính xác

## 🔍 Debugging Tips

### Nếu Merchant ID Không Được Đọc:

**Kiểm tra logs:**
```
[SEPAY] ⚠️ Merchant ID chưa được cấu hình...
```

**Giải pháp:**
1. Kiểm tra tên biến có đúng không
2. Kiểm tra giá trị có đúng không
3. Restart service trên Railway
4. Kiểm tra lại logs sau khi restart

### Nếu API Vẫn Trả Về 404:

**Kiểm tra logs:**
- Endpoint nào được thử?
- Request body có `merchant_id` không?
- Response từ API là gì?

**Giải pháp:**
1. Kiểm tra SePay Dashboard → API → Endpoint chính xác
2. Kiểm tra `merchant_id` có đúng không
3. Thử endpoint khác (code sẽ tự động thử)

## 📋 Tóm Tắt

**Cấu hình của bạn:**
- ✅ Hầu hết các biến đã đúng format
- ⚠️ Chỉ có `SePayMerchantId` cần sửa thành `SePay__MerchantId`

**Code đã hỗ trợ:**
- ✅ Cả 2 format (đúng và sai)
- ✅ Rate limiting (2 requests/second)
- ✅ Multiple endpoint fallback
- ✅ Error handling và retry logic

**Bước tiếp theo:**
1. Sửa tên biến `SePayMerchantId` → `SePay__MerchantId` (khuyến nghị)
2. Hoặc giữ nguyên (code đã hỗ trợ)
3. Test tạo QR code
4. Kiểm tra logs để xem endpoint nào hoạt động

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Railway Variables:** Railway Dashboard → Variables
- **Railway Logs:** Railway Dashboard → Service → Logs
- **Website:** https://quanlyresort-production.up.railway.app

