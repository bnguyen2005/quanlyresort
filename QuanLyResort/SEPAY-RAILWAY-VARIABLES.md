# 🚂 Cấu Hình SePay Variables Trong Railway

## 📋 Thông Tin SePay Production

- **Tên đơn vị:** Lam Thanh
- **Mã đơn vị:** `SP-LIVE-LT39A334`
- **Secret Key:** `spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb`

## 🔧 Các Biến Môi Trường Cần Thêm Trong Railway

### Bước 1: Vào Railway Dashboard

1. Mở: https://railway.app
2. Chọn project `quanlyresort`
3. Vào tab **"Variables"**

### Bước 2: Thêm Các Biến Sau

Click **"New Variable"** và thêm từng biến:

#### ✅ Biến 1: API Token (Secret Key)
```
Name:  SePay__ApiToken
Value: spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb
```

#### ✅ Biến 2: Account ID (Mã đơn vị)
```
Name:  SePay__AccountId
Value: SP-LIVE-LT39A334
```

#### ✅ Biến 3: Bank Code (Optional - mặc định MB)
```
Name:  SePay__BankCode
Value: MB
```

#### ✅ Biến 4: API Base URL (Optional - mặc định)
```
Name:  SePay__ApiBaseUrl
Value: https://my.sepay.vn/userapi
```

## 📝 Lưu Ý Quan Trọng

1. **Tên biến phải có `__` (2 dấu gạch dưới):**
   - ✅ Đúng: `SePay__ApiToken`
   - ❌ Sai: `SePay_ApiToken` hoặc `SePay-ApiToken`

2. **Copy chính xác giá trị, không có khoảng trắng:**
   - ✅ Đúng: `SP-LIVE-LT39A334`
   - ❌ Sai: ` SP-LIVE-LT39A334 ` (có khoảng trắng)

3. **Sau khi thêm, Railway sẽ tự động redeploy**

## ✅ Checklist

- [ ] Đã thêm `SePay__ApiToken` = `spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb`
- [ ] Đã thêm `SePay__AccountId` = `SP-LIVE-LT39A334`
- [ ] Đã thêm `SePay__BankCode` = `MB` (optional)
- [ ] Đã thêm `SePay__ApiBaseUrl` = `https://my.sepay.vn/userapi` (optional)
- [ ] Railway đã redeploy thành công
- [ ] Kiểm tra logs không còn warning về SePay

## 🧪 Test Sau Khi Cấu Hình

1. **Tạo booking mới** → Click "Thanh toán"
2. **Kiểm tra QR code hiển thị**
3. **Kiểm tra logs:** `[SEPAY] ✅ Đơn hàng tạo thành công`

## 🔗 Xem Thêm

- **Hướng dẫn chi tiết:** `SEPAY-PRODUCTION-CONFIG.md`
- **Troubleshooting:** `SEPAY-API-SETUP.md`

