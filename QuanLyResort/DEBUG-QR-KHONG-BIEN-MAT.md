# 🔍 DEBUG: QR Không Biến Mất và Không Hiện "Thanh Toán Thành Công"

## 📋 Vấn Đề
- ✅ PayOs đã hiển thị "Đã thanh toán"
- ❌ QR code không biến mất
- ❌ Không hiển thị "Thanh toán thành công"

## 🔍 Các Nguyên Nhân Có Thể

### 1. ❌ Webhook Không Được Gửi Từ PayOs (Nguyên nhân phổ biến nhất)

**Triệu chứng:**
- PayOs hiển thị "Đã thanh toán" nhưng backend không nhận được webhook
- Booking status vẫn là "Pending" (không đổi thành "Paid")
- Logs trên Render **KHÔNG CÓ** entry `[WEBHOOK-xxx]`

**Kiểm tra:**
```bash
# 1. Xem logs trên Render
# https://dashboard.render.com -> Logs
# Tìm: [WEBHOOK-xxx] hoặc "Webhook received"

# 2. Test webhook thủ công
./test-payos-webhook.sh 4

# 3. Kiểm tra booking status
curl -H "Authorization: Bearer YOUR_TOKEN" \
  https://quanlyresort.onrender.com/api/bookings/4
```

**Giải pháp:**
- Chạy lại script config webhook: `./config-payos-webhook.sh`
- Kiểm tra PayOs dashboard xem webhook có được gửi không

---

### 2. ❌ Webhook Được Gửi Nhưng Không Parse Được

**Triệu chứng:**
- Logs có: `[WEBHOOK-xxx] Webhook received`
- Nhưng có lỗi: `⚠️ Cannot extract booking ID` hoặc `⚠️ PayOs webhook failed`

**Kiểm tra logs:**
```
📥 [WEBHOOK-xxx] Webhook received
   PayOs - Description: CSCOK68MZC1 BOOKING4
⚠️ Cannot extract booking ID
```

**Nguyên nhân:**
- Description không có format "BOOKING4"
- PayOs gửi format khác

**Giải pháp:**
- Kiểm tra description trong logs
- Update logic extract nếu cần

---

### 3. ❌ Booking Status Không Được Update

**Triệu chứng:**
- Webhook được xử lý thành công
- Logs có: `✅ Booking updated to Paid`
- Nhưng khi query lại, status vẫn là "Pending"

**Kiểm tra:**
```bash
# Query booking sau khi webhook xử lý
curl -H "Authorization: Bearer YOUR_TOKEN" \
  https://quanlyresort.onrender.com/api/bookings/4
```

**Nguyên nhân:**
- Database transaction rollback
- Cache issue

**Giải pháp:**
- Kiểm tra database logs
- Clear cache nếu có

---

### 4. ❌ Frontend Polling Không Hoạt Động

**Triệu chứng:**
- Booking status đã đổi thành "Paid" trong database
- Nhưng frontend không detect được
- Console không có logs: `[SimplePolling]`

**Kiểm tra:**
1. Mở browser console (F12)
2. Tìm logs: `[SimplePolling]` hoặc `[showPaymentSuccess]`
3. Kiểm tra xem polling có chạy không

**Nguyên nhân:**
- Polling không được start
- Polling bị stop sớm
- API call bị lỗi

**Giải pháp:**
- Kiểm tra console logs
- Đảm bảo `startSimplePolling(bookingId)` được gọi

---

### 5. ❌ showPaymentSuccess() Không Tìm Được Elements

**Triệu chứng:**
- Polling detect được "Paid" status
- Console có: `✅ [SimplePolling] Payment detected!`
- Nhưng có warnings: `⚠️ [showPaymentSuccess] spQRImage element not found`

**Kiểm tra:**
- Console logs có warnings về missing elements
- HTML có đúng IDs không: `spQRImage`, `spSuccess`, `spQRSection`

**Giải pháp:**
- Kiểm tra HTML modal có đúng IDs
- Update IDs nếu cần

---

## 🧪 CÁCH KIỂM TRA TỪNG BƯỚC

### Bước 1: Kiểm Tra Webhook Có Được Gửi Không

**Xem logs trên Render:**
```
1. Vào: https://dashboard.render.com
2. Chọn service: quanlyresort
3. Click "Logs"
4. Tìm: [WEBHOOK-xxx] hoặc "Webhook received"
```

**Nếu KHÔNG CÓ logs:**
→ **Nguyên nhân #1: PayOs không gửi webhook**

**Nếu CÓ logs:**
→ Xem bước 2

---

### Bước 2: Kiểm Tra Webhook Có Parse Được Không

**Xem logs:**
```
📥 [WEBHOOK-xxx] Webhook received
   PayOs - Description: CSCOK68MZC1 BOOKING4
✅ Extracted booking ID: 4
✅ Booking 4 updated to Paid
```

**Nếu có lỗi:**
```
⚠️ Cannot extract booking ID
```
→ **Nguyên nhân #2: Webhook không parse được**

**Nếu thành công:**
→ Xem bước 3

---

### Bước 3: Kiểm Tra Booking Status Có Đổi Không

**Query booking:**
```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  https://quanlyresort.onrender.com/api/bookings/4
```

**Kiểm tra:**
- `status` có phải `"Paid"` không?

**Nếu vẫn là "Pending":**
→ **Nguyên nhân #3: Booking status không được update**

**Nếu đã là "Paid":**
→ Xem bước 4

---

### Bước 4: Kiểm Tra Frontend Polling

**Mở browser console (F12):**
- Tìm logs: `[SimplePolling]`
- Kiểm tra xem có polling không

**Nếu KHÔNG CÓ logs:**
→ **Nguyên nhân #4: Polling không hoạt động**

**Nếu CÓ logs nhưng không detect:**
```
⏳ [SimplePolling] Still waiting... Status: Pending
```
→ Kiểm tra xem status có đúng không

**Nếu detect được:**
```
✅ [SimplePolling] Payment detected! Status = Paid
```
→ Xem bước 5

---

### Bước 5: Kiểm Tra showPaymentSuccess()

**Xem console logs:**
```
🎉 [showPaymentSuccess] Showing payment success...
✅ [showPaymentSuccess] Hidden QR image
✅ [showPaymentSuccess] Showed success message
```

**Nếu có warnings:**
```
⚠️ [showPaymentSuccess] spQRImage element not found
```
→ **Nguyên nhân #5: Elements không tìm được**

**Giải pháp:**
- Kiểm tra HTML modal có đúng IDs
- Update IDs nếu cần

---

## 🔧 GIẢI PHÁP TỪNG TRƯỜNG HỢP

### Trường Hợp 1: PayOs Không Gửi Webhook

```bash
# Config webhook lại
./config-payos-webhook.sh

# Test webhook thủ công
./test-payos-webhook.sh 4
```

### Trường Hợp 2: Webhook Không Parse Được

- Xem logs để biết description format
- Update logic extract nếu cần
- Test lại với format mới

### Trường Hợp 3: Booking Status Không Update

- Kiểm tra database logs
- Kiểm tra transaction có commit không
- Test manual update: `POST /api/simplepayment/manual-update-paid/4`

### Trường Hợp 4: Polling Không Hoạt Động

- Kiểm tra console logs
- Đảm bảo `startSimplePolling(bookingId)` được gọi
- Kiểm tra API call có lỗi không

### Trường Hợp 5: Elements Không Tìm Được

- Kiểm tra HTML modal
- Update IDs nếu cần
- Test lại

---

## 📊 CHECKLIST DEBUG

- [ ] Logs trên Render có `[WEBHOOK-xxx]` không?
- [ ] Webhook có parse được booking ID không?
- [ ] Booking status có đổi thành "Paid" không?
- [ ] Frontend polling có chạy không?
- [ ] Console có logs `[SimplePolling]` không?
- [ ] `showPaymentSuccess()` có tìm được elements không?
- [ ] HTML modal có đúng IDs không?

---

## 🎯 KẾT LUẬN

**Nguyên nhân phổ biến nhất:**
1. ❌ **PayOs không gửi webhook** (80%)
2. ❌ **Webhook không parse được** (10%)
3. ❌ **Frontend polling không hoạt động** (5%)
4. ❌ **Elements không tìm được** (5%)

**Cần logs từ Render để xác định chính xác nguyên nhân!**
