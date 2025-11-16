# 🔧 Fix SePay API Token Trên Railway

## ⚠️ Vấn Đề

**API Token trên Railway đang cấu hình SAI!**

**Token hiện tại (SAI):**
```
spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb
```

**Token đúng (từ SePay Dashboard):**
```
PWGH9OZC4OEMDYNDIIGLWRMTQQQZNA49JU3FFY5LXI8STESEJA6EIBYCP7BOQXFH
```

## ✅ Cách Sửa

### Bước 1: Vào Railway Dashboard

1. **Mở Railway:** https://railway.app
2. **Chọn project** `quanlyresort`
3. **Vào tab "Variables"**

### Bước 2: Tìm và Sửa Biến API Token

**Tìm biến:**
```
SePay__ApiToken
```

**Hoặc:**
```
SEPAY_API_KEY
```

### Bước 3: Cập Nhật Giá Trị

**Click vào biến để edit, sau đó thay đổi giá trị:**

**Từ:**
```
spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb
```

**Thành:**
```
PWGH9OZC4OEMDYNDIIGLWRMTQQQZNA49JU3FFY5LXI8STESEJA6EIBYCP7BOQXFH
```

**Lưu:** Click "Save" hoặc "Update"

## 📋 Cấu Hình Đúng Sau Khi Sửa

**Danh sách đầy đủ các biến cần có:**

| Tên Biến | Giá Trị | Trạng Thái |
|----------|---------|------------|
| `SePay__AccountId` | `5365` | ✅ |
| `SePay__ApiBaseUrl` | `https://pgapi.sepay.vn` | ✅ |
| `SePay__ApiToken` | `PWGH9OZC4OEMDYNDIIGLWRMTQQQZNA49JU3FFY5LXI8STESEJA6EIBYCP7BOQXFH` | ✅ **ĐÃ SỬA** |
| `SePay__BankAccountNumber` | `0901329227` | ✅ |
| `SePay__BankCode` | `MB` | ✅ |
| `SEPAY_WEBHOOK_URL` | `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook` | ✅ |
| `SePayMerchantId` hoặc `SePay__MerchantId` | `SP-LIVE-LT39A334` | ✅ |

## 🔍 Kiểm Tra Sau Khi Sửa

### Bước 1: Deploy/Restart Service

**Sau khi sửa biến môi trường:**
- Railway sẽ tự động restart service
- Hoặc bạn có thể restart thủ công: Railway Dashboard → Service → Restart

### Bước 2: Kiểm Tra Logs

**Railway Dashboard → Service → Logs**

**Tìm các dòng:**
- `[SEPAY] 🔍 API Key configured: PWGH9OZC...` ← **Phải có dòng này với token mới!**
- `[SEPAY] 🔍 Client ID configured: 5365`
- `[SEPAY] 🔍 Merchant ID configured: SP-LIVE-LT39A334`

### Bước 3: Test Tạo QR Code

1. **Vào website:** https://quanlyresort-production.up.railway.app
2. **Đăng nhập** với tài khoản customer
3. **Tạo booking mới**
4. **Click "Thanh toán"**
5. **Kiểm tra logs:**
   - `[SEPAY] 🔍 Authorization header: Bearer PWGH9OZC...` ← **Phải có token mới!**
   - `[SEPAY] 🔄 Thử endpoint: Production Standard`
   - `[SEPAY] ✅ Đơn hàng tạo thành công` hoặc
   - `[SEPAY] ⚠️ SePay API không hoạt động, fallback sang static QR code`

## 🔍 So Sánh Token

### Token Cũ (SAI):
```
spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb
```
- Format: `spsk_live_...` (Production token format)
- Độ dài: Ngắn hơn
- **Không phải token từ SePay Dashboard của bạn**

### Token Mới (ĐÚNG):
```
PWGH9OZC4OEMDYNDIIGLWRMTQQQZNA49JU3FFY5LXI8STESEJA6EIBYCP7BOQXFH
```
- Format: Alphanumeric string
- Độ dài: 64 ký tự
- **Token từ SePay Dashboard (ID: 5365, Tên: ResortDeluxe)**

## 💡 Lưu Ý

1. **Token Format:** Token mới có format khác (`PWGH9OZC...` thay vì `spsk_live_...`)
2. **Code Hỗ Trợ:** Code đã được cập nhật để hỗ trợ cả 2 format token
3. **Authorization:** Code sẽ luôn dùng `Bearer {token}` format
4. **Sau Khi Sửa:** Railway sẽ tự động restart service, không cần deploy lại code

## ✅ Checklist

- [ ] Đã tìm thấy biến `SePay__ApiToken` hoặc `SEPAY_API_KEY` trên Railway
- [ ] Đã cập nhật giá trị từ token cũ sang token mới
- [ ] Đã lưu thay đổi
- [ ] Railway đã restart service (tự động hoặc thủ công)
- [ ] Kiểm tra logs có token mới không
- [ ] Test tạo QR code
- [ ] Kiểm tra API có hoạt động không

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Railway Variables:** Railway Dashboard → Variables
- **Railway Logs:** Railway Dashboard → Service → Logs
- **Website:** https://quanlyresort-production.up.railway.app

## 🎯 Kết Luận

**Vấn đề:** API Token trên Railway đang cấu hình sai

**Giải pháp:** Cập nhật `SePay__ApiToken` thành token đúng từ SePay Dashboard

**Sau khi sửa:**
- ✅ API sẽ dùng token đúng
- ✅ Có thể tạo QR code thành công
- ✅ API có thể hoạt động đúng

**Bước tiếp theo:**
1. Cập nhật token trên Railway
2. Đợi Railway restart service
3. Test tạo QR code
4. Kiểm tra logs để xem API có hoạt động không

