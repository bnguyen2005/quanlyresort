# 🔧 Fix SePay Railway Variables

## ⚠️ Vấn Đề Phát Hiện

**Tên biến `SePayMerchantId` không đúng format!**

**Hiện tại:**
```
SePayMerchantId = SP-LIVE-LT39A334
```

**Phải sửa thành:**
```
SePay__MerchantId = SP-LIVE-LT39A334
```

## 📋 Giải Thích

**Trong .NET Configuration:**
- Environment variables với format `SePay__*` sẽ được map vào `SePay:*` trong configuration
- `SePay__MerchantId` → `SePay:MerchantId` ✅
- `SePayMerchantId` → Không được map đúng ❌

**Format đúng:**
- `SePay__AccountId` ✅
- `SePay__ApiToken` ✅
- `SePay__ApiBaseUrl` ✅
- `SePay__BankAccountNumber` ✅
- `SePay__BankCode` ✅
- `SePay__MerchantId` ✅ (phải có 2 dấu gạch dưới!)

## ✅ Cấu Hình Đúng Trên Railway

### Bước 1: Vào Railway Dashboard

1. **Mở Railway:** https://railway.app
2. **Chọn project** `quanlyresort`
3. **Vào tab "Variables"**

### Bước 2: Sửa Tên Biến

**Tìm biến:**
```
SePayMerchantId
```

**Xóa biến cũ và thêm biến mới:**
```
Name:  SePay__MerchantId
Value: SP-LIVE-LT39A334
```

**Lưu ý:** Phải có **2 dấu gạch dưới** (`__`) giữa `SePay` và `MerchantId`!

### Bước 3: Kiểm Tra Tất Cả Các Biến

**Danh sách đầy đủ các biến cần có:**

#### ✅ Đã Đúng:
- `SePay__AccountId` = `5365` ✅
- `SePay__ApiBaseUrl` = `https://pgapi.sepay.vn` ✅
- `SePay__ApiToken` = `spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb` ✅
- `SePay__BankAccountNumber` = `0901329227` ✅
- `SePay__BankCode` = `MB` ✅
- `SEPAY_WEBHOOK_URL` = `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook` ✅

#### ⚠️ Cần Sửa:
- `SePayMerchantId` → **XÓA** và thêm `SePay__MerchantId` = `SP-LIVE-LT39A334` ✅

## 🔍 Kiểm Tra Sau Khi Sửa

### Bước 1: Deploy Code Mới

**Railway sẽ tự động deploy sau khi bạn sửa biến môi trường.**

**Hoặc trigger deploy thủ công:**
- Railway Dashboard → Service → Deployments → Redeploy

### Bước 2: Kiểm Tra Logs

**Railway Dashboard → Service → Logs**

**Tìm các dòng:**
- `[SEPAY] 🔍 Client ID configured: 5365`
- `[SEPAY] 🔍 API Key configured: spsk_live_eofJdy5CA7...`
- `[SEPAY] 🔍 Merchant ID configured: SP-LIVE-LT39A334` ← **Phải có dòng này!**

**Nếu không thấy dòng "Merchant ID configured":**
- Kiểm tra lại tên biến có đúng `SePay__MerchantId` không
- Kiểm tra có 2 dấu gạch dưới không
- Restart service trên Railway

### Bước 3: Test Tạo QR Code

1. **Vào website:** https://quanlyresort-production.up.railway.app
2. **Đăng nhập** với tài khoản customer
3. **Tạo booking mới**
4. **Click "Thanh toán"**
5. **Kiểm tra logs:**
   - `[SEPAY] 🔍 Added merchant_id to request: SP-LIVE-LT39A334` ← **Phải có dòng này!**
   - `[SEPAY] 🔄 Thử endpoint: Production Standard - https://pgapi.sepay.vn/api/v1/orders`
   - `[SEPAY] 🔄 Thử endpoint: Production Merchant - https://pgapi.sepay.vn/api/v1/merchants/SP-LIVE-LT39A334/orders`

## 📊 So Sánh Trước và Sau

### Trước (Sai):
```
SePayMerchantId = SP-LIVE-LT39A334
```
- ❌ Code không đọc được
- ❌ `_merchantId` = null
- ❌ Request body không có `merchant_id`
- ❌ API có thể trả về 404

### Sau (Đúng):
```
SePay__MerchantId = SP-LIVE-LT39A334
```
- ✅ Code đọc được
- ✅ `_merchantId` = "SP-LIVE-LT39A334"
- ✅ Request body có `merchant_id`
- ✅ API có thể hoạt động đúng

## 🔧 Code Sẽ Đọc Như Thế Nào

**Trong SePayService.cs:**
```csharp
_merchantId = _configuration["SePay:MerchantId"];
```

**Environment variable mapping:**
- `SePay__MerchantId` → `SePay:MerchantId` ✅
- `SePayMerchantId` → Không map được ❌

## ✅ Checklist

- [ ] Đã xóa biến `SePayMerchantId` cũ
- [ ] Đã thêm biến `SePay__MerchantId` mới (với 2 dấu gạch dưới)
- [ ] Giá trị = `SP-LIVE-LT39A334`
- [ ] Đã deploy code mới (hoặc restart service)
- [ ] Kiểm tra logs có dòng "Merchant ID configured"
- [ ] Test tạo QR code
- [ ] Kiểm tra logs có dòng "Added merchant_id to request"

## 💡 Lưu Ý

1. **Format biến môi trường:** Phải có 2 dấu gạch dưới (`__`) giữa prefix và tên biến
2. **Case sensitive:** Tên biến phân biệt hoa thường
3. **Restart:** Sau khi sửa biến, cần restart service hoặc deploy lại
4. **Logs:** Luôn kiểm tra logs để xác nhận biến đã được đọc đúng

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Railway Variables:** Railway Dashboard → Variables
- **Railway Logs:** Railway Dashboard → Service → Logs

## 🎯 Kết Luận

**Vấn đề:** Tên biến `SePayMerchantId` không đúng format

**Giải pháp:** Đổi thành `SePay__MerchantId` (với 2 dấu gạch dưới)

**Sau khi sửa:**
- ✅ Code sẽ đọc được merchant_id
- ✅ Request body sẽ có `merchant_id`
- ✅ API có thể hoạt động đúng
- ✅ Production API endpoint có thể hoạt động

