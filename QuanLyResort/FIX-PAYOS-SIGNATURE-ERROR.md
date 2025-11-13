# 🔧 Fix Lỗi PayOs Signature Không Hợp Lệ

## ❌ Lỗi Hiện Tại

```
[PAYOS] PayOs API returned error. Code: 201, Desc: Mã kiểm tra(signature) không hợp lệ
```

**Nguyên nhân:**
- ChecksumKey không đúng hoặc không khớp với PayOs Dashboard
- Signature format không đúng theo yêu cầu PayOs
- Environment variables chưa được set đúng trên Railway

## ✅ Giải Pháp

### Bước 1: Kiểm Tra PayOs Dashboard

1. **Vào PayOs Dashboard:** https://payos.vn
2. **Vào Settings** → **API Keys**
3. **Copy các giá trị:**
   - **Client ID**
   - **API Key**
   - **Checksum Key** (quan trọng nhất!)

### Bước 2: Cập Nhật Environment Variables Trên Railway

1. **Vào Railway Dashboard** → Service `quanlyresort`
2. **Tab "Variables"**
3. **Tìm và cập nhật các biến sau:**

```env
# PayOs Settings - QUAN TRỌNG: Phải lấy từ PayOs Dashboard
BankWebhook__PayOs__ClientId=c704495b-5984-4ad3-aa23-b2794a02aa83
BankWebhook__PayOs__ApiKey=f6ea421b-a8b7-46b8-92be-209eb1a9b2fb
BankWebhook__PayOs__ChecksumKey=429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
BankWebhook__PayOs__SecretKey=429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
```

**⚠️ LƯU Ý QUAN TRỌNG:**
- **ChecksumKey** phải lấy từ PayOs Dashboard
- **Không được copy sai** - phải copy chính xác từ PayOs
- **Không có khoảng trắng** ở đầu/cuối

### Bước 3: Kiểm Tra Signature Format

Từ log, signature string hiện tại:
```
amount=5000&cancelUrl=http://quanlyresort-production.up.railway.app/customer/my-bookings.html?payment=cancelled&bookingId=4&description=BOOKING4&orderCode=47711&returnUrl=http://quanlyresort-production.up.railway.app/customer/my-bookings.html?payment=success&bookingId=4
```

**Vấn đề có thể:**
- URL có query parameters (`?payment=cancelled&bookingId=4`) có thể gây lỗi
- PayOs có thể yêu cầu URL encode

### Bước 4: Redeploy Sau Khi Cập Nhật

1. **Save** các environment variables
2. **Vào tab "Deployments"**
3. **Click "Redeploy"**
4. **Chọn "Deploy"**

## 🔍 Kiểm Tra Sau Khi Fix

### 1. Xem Logs

Vào tab **"Logs"** và tìm:

✅ **Thành công:**
```
[PAYOS] ✅ Payment link created successfully
[PAYOS] Payment URL: https://pay.payos.vn/web/...
```

❌ **Vẫn lỗi:**
```
[PAYOS] PayOs API returned error. Code: 201, Desc: Mã kiểm tra(signature) không hợp lệ
```

### 2. Test Tạo Payment Link

Thử tạo payment link lại từ frontend và kiểm tra logs.

## 🐛 Troubleshooting

### Lỗi: "Mã kiểm tra(signature) không hợp lệ"

**Nguyên nhân:**
1. ChecksumKey không đúng
2. ChecksumKey không khớp với PayOs Dashboard
3. Environment variable chưa được load

**Giải pháp:**
1. **Kiểm tra lại ChecksumKey** trong PayOs Dashboard
2. **Copy lại chính xác** vào Railway Variables
3. **Đảm bảo không có khoảng trắng** ở đầu/cuối
4. **Redeploy** để load environment variables mới

### Lỗi: "ClientId không hợp lệ"

**Giải pháp:**
- Kiểm tra `BankWebhook__PayOs__ClientId` đúng chưa
- Lấy từ PayOs Dashboard

### Lỗi: "API Key không hợp lệ"

**Giải pháp:**
- Kiểm tra `BankWebhook__PayOs__ApiKey` đúng chưa
- Lấy từ PayOs Dashboard

## 📋 Checklist

- [ ] Đã lấy ChecksumKey từ PayOs Dashboard
- [ ] Đã cập nhật `BankWebhook__PayOs__ChecksumKey` trên Railway
- [ ] Đã cập nhật `BankWebhook__PayOs__ClientId` trên Railway
- [ ] Đã cập nhật `BankWebhook__PayOs__ApiKey` trên Railway
- [ ] Đã redeploy sau khi cập nhật
- [ ] Đã test lại tạo payment link

## 💡 Lưu Ý

- **ChecksumKey** là quan trọng nhất - phải chính xác 100%
- PayOs signature được tính bằng HMAC-SHA256 của signature string
- Signature string format: `amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}`
- Tất cả giá trị phải lấy từ PayOs Dashboard, không tự tạo

## 🔗 Tài Liệu Tham Khảo

- PayOs API Documentation: https://payos.vn/docs
- PayOs Dashboard: https://payos.vn

