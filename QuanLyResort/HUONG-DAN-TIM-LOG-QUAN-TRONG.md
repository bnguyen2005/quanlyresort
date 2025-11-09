# 🔍 Hướng Dẫn Tìm Log Quan Trọng Trong Render

## 📋 Các Log Quan Trọng Cần Tìm

### 1. **Webhook Received** (Quan trọng nhất)
Tìm các dòng có:
```
[WEBHOOK] 📥 [WEBHOOK-{id}] Webhook received
[WEBHOOK]    Raw request JSON: {...}
```

**Ý nghĩa**: Xác nhận webhook đã được nhận từ PayOs

---

### 2. **JSON Deserialization** (Rất quan trọng - vừa fix)
Tìm các dòng có:
```
[WEBHOOK] 🔍 [WEBHOOK-{id}] Attempting to deserialize as PayOs format...
[WEBHOOK] 🔍 [WEBHOOK-{id}] PayOs deserialization result: Code=..., Desc=..., Success=..., Data=...
[WEBHOOK] 🔍 [WEBHOOK-{id}] PayOs request details: Code='...', Desc='...', Success=..., Data is null: ...
```

**Ý nghĩa**: 
- ✅ **Tốt**: `Code='00'`, `Data is null: False` → Deserialize thành công
- ❌ **Lỗi**: `Code=''`, `Data is null: True` → Deserialize thất bại (đã fix bằng JsonPropertyName)

**Cần copy**: Toàn bộ phần này để xem có deserialize được không

---

### 3. **PayOs Format Detection**
Tìm các dòng có:
```
[WEBHOOK] 📋 [WEBHOOK-{id}] ✅ Detected PayOs format
[WEBHOOK]    PayOs - Code: ..., Desc: ...
[WEBHOOK]    PayOs - Description: '...'
[WEBHOOK]    PayOs - OrderCode: ..., Amount: ...
```

**Ý nghĩa**: Xác nhận đã nhận diện đúng format PayOs và extract được data

**Cần copy**: 
- `Description: '...'` (ví dụ: `CS730NG59M1 BOOKING4`)
- `OrderCode: ...`
- `Amount: ...`

---

### 4. **Booking ID Extraction** (Rất quan trọng)
Tìm các dòng có:
```
[WEBHOOK] 🔍 [WEBHOOK-{id}] ========== STARTING BOOKING ID EXTRACTION ==========
[WEBHOOK] 🔍 [WEBHOOK-{id}] Current values: Content='...', Amount=..., OrderCode=...
[WEBHOOK] 🔍 [WEBHOOK-{id}] Content is NOT empty, attempting to extract bookingId from: '...'
[WEBHOOK] ExtractBookingId: Normalized content: '...'
[WEBHOOK] ExtractBookingId: ✅ Matched pattern...
[WEBHOOK] ✅ [WEBHOOK-{id}] ✅✅✅ SUCCESS: Extracted bookingId from description: ...
[WEBHOOK] ✅ [WEBHOOK-{id}] ✅✅✅ FINAL: Extracted booking ID: ...
```

**Ý nghĩa**: 
- ✅ **Tốt**: Có dòng `✅✅✅ SUCCESS` hoặc `✅✅✅ FINAL` → Extract thành công
- ❌ **Lỗi**: Có dòng `❌ FAILED` hoặc `❌❌❌ CRITICAL` → Không extract được booking ID

**Cần copy**: Toàn bộ section này, đặc biệt là:
- `Content='...'` (description từ PayOs)
- Pattern nào được match (pattern1, pattern2, ...)
- Booking ID cuối cùng được extract

---

### 5. **Booking Fetch & Status Check**
Tìm các dòng có:
```
[WEBHOOK] 🔍 [WEBHOOK-{id}] Fetching booking {id}...
[WEBHOOK] ✅ [WEBHOOK-{id}] Booking found: Code=..., Status=..., Amount=...
[WEBHOOK] ✅ [WEBHOOK-{id}] Booking {id} already paid, ignoring duplicate
```

**Ý nghĩa**: 
- ✅ **Tốt**: Booking found với status hiện tại
- ⚠️ **Cảnh báo**: Booking đã paid rồi → Webhook duplicate

**Cần copy**: 
- `Status=...` (trước khi update)
- `Amount=...` (để verify)

---

### 6. **Booking Status Update** (Rất quan trọng)
Tìm các dòng có:
```
[WEBHOOK] 🔄 [WEBHOOK-{id}] ========== STARTING BOOKING STATUS UPDATE ==========
[WEBHOOK] 🔄 [WEBHOOK-{id}] Current booking status BEFORE update: ...
[WEBHOOK] 🔄 [WEBHOOK-{id}] Calling ProcessOnlinePaymentAsync with: BookingId=..., PerformedBy=...
[WEBHOOK] 🔄 [WEBHOOK-{id}] ProcessOnlinePaymentAsync returned: ...
[WEBHOOK] ✅ [WEBHOOK-{id}] Booking status AFTER update: ...
[WEBHOOK] ✅ [WEBHOOK-{id}] ✅✅✅ SUCCESS: Booking status is 'Paid'!
```

**Ý nghĩa**: 
- ✅ **Tốt**: Có dòng `✅✅✅ SUCCESS: Booking status is 'Paid'!` → Update thành công
- ❌ **Lỗi**: `ProcessOnlinePaymentAsync returned: False` → Update thất bại
- ⚠️ **Cảnh báo**: `Status is NOT 'Paid' after update` → Update không thành công

**Cần copy**: 
- Status BEFORE và AFTER
- Return value của `ProcessOnlinePaymentAsync`
- Bất kỳ warning nào về status

---

### 7. **Error Logs** (Quan trọng khi có lỗi)
Tìm các dòng có:
```
[WEBHOOK] ❌ [WEBHOOK-{id}] Error processing webhook
[WEBHOOK] ❌ [WEBHOOK-{id}] Error message: ...
[WEBHOOK] ❌ [WEBHOOK-{id}] Stack trace: ...
```

**Ý nghĩa**: Có exception xảy ra trong quá trình xử lý

**Cần copy**: Toàn bộ error message và stack trace

---

## 🔎 Cách Tìm Log Trong Render

### Bước 1: Vào Render Dashboard
1. Truy cập: https://dashboard.render.com
2. Chọn service `quanlyresort` (hoặc tên service của bạn)
3. Click tab **"Logs"**

### Bước 2: Filter Log
Trong Render logs, bạn có thể:

**Option 1: Tìm theo keyword**
- Tìm: `[WEBHOOK]` → Tất cả webhook logs
- Tìm: `WEBHOOK-` → Tất cả webhook với ID cụ thể
- Tìm: `Deserialization` → Logs về deserialization
- Tìm: `ExtractBookingId` → Logs về booking ID extraction
- Tìm: `ProcessOnlinePaymentAsync` → Logs về status update

**Option 2: Tìm theo thời gian**
- Tìm logs gần thời điểm bạn test thanh toán
- Ví dụ: Nếu test lúc 11:25, tìm logs từ 11:24-11:26

**Option 3: Copy toàn bộ logs**
- Copy tất cả logs từ khi webhook được nhận đến khi kết thúc
- Tìm các dòng có `═══════════════════════════════════════════════════════════` (đây là separator)

---

## 📝 Template Để Gửi Log Cho Tôi

Khi tìm được logs, hãy copy theo format này:

```
=== WEBHOOK RECEIVED ===
[WEBHOOK] 📥 [WEBHOOK-xxxxx] Webhook received at ...
[WEBHOOK]    Raw request JSON: {...}

=== DESERIALIZATION ===
[WEBHOOK] 🔍 [WEBHOOK-xxxxx] Attempting to deserialize...
[WEBHOOK] 🔍 [WEBHOOK-xxxxx] PayOs deserialization result: Code=..., Desc=..., Success=..., Data=...
[WEBHOOK] 🔍 [WEBHOOK-xxxxx] PayOs request details: Code='...', Desc='...', Success=..., Data is null: ...

=== FORMAT DETECTION ===
[WEBHOOK] 📋 [WEBHOOK-xxxxx] ✅ Detected PayOs format
[WEBHOOK]    PayOs - Description: '...'
[WEBHOOK]    PayOs - OrderCode: ..., Amount: ...

=== BOOKING ID EXTRACTION ===
[WEBHOOK] 🔍 [WEBHOOK-xxxxx] ========== STARTING BOOKING ID EXTRACTION ==========
[WEBHOOK] 🔍 [WEBHOOK-xxxxx] Current values: Content='...', Amount=..., OrderCode=...
[WEBHOOK] ExtractBookingId: Normalized content: '...'
[WEBHOOK] ExtractBookingId: ✅ Matched pattern...
[WEBHOOK] ✅ [WEBHOOK-xxxxx] ✅✅✅ FINAL: Extracted booking ID: ...

=== BOOKING STATUS UPDATE ===
[WEBHOOK] 🔄 [WEBHOOK-xxxxx] ========== STARTING BOOKING STATUS UPDATE ==========
[WEBHOOK] 🔄 [WEBHOOK-xxxxx] Current booking status BEFORE update: ...
[WEBHOOK] 🔄 [WEBHOOK-xxxxx] ProcessOnlinePaymentAsync returned: ...
[WEBHOOK] ✅ [WEBHOOK-xxxxx] Booking status AFTER update: ...
[WEBHOOK] ✅ [WEBHOOK-xxxxx] ✅✅✅ SUCCESS: Booking status is 'Paid'!
```

---

## 🎯 Checklist Khi Test

Sau khi test thanh toán, kiểm tra logs có đủ các phần sau:

- [ ] ✅ Webhook received (có raw JSON)
- [ ] ✅ Deserialization thành công (Code='00', Data is null: False)
- [ ] ✅ PayOs format detected (có Description, OrderCode, Amount)
- [ ] ✅ Booking ID extracted (có FINAL booking ID)
- [ ] ✅ Booking found (có Status và Amount)
- [ ] ✅ Status update thành công (ProcessOnlinePaymentAsync returned: True)
- [ ] ✅ Status verified (Status AFTER update = 'Paid')

Nếu thiếu bất kỳ phần nào → Đó là điểm lỗi cần fix!

---

## 💡 Tips

1. **Tìm webhook ID**: Mỗi webhook có ID unique (ví dụ: `WEBHOOK-c4bab7d1`). Dùng ID này để filter tất cả logs liên quan.

2. **Timeline**: Logs được sắp xếp theo thời gian. Tìm từ trên xuống dưới để theo dõi flow.

3. **Error patterns**: Nếu thấy `❌`, `⚠️`, `CRITICAL`, `FAILED` → Đó là điểm cần chú ý.

4. **Success patterns**: Nếu thấy `✅✅✅ SUCCESS` → Phần đó đã hoạt động đúng.

5. **Separator**: Dòng `═══════════════════════════════════════════════════════════` đánh dấu bắt đầu và kết thúc của một webhook request.

