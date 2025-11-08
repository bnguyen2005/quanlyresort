# 🔍 TÓM TẮT NGUYÊN NHÂN WEBHOOK KHÔNG HOẠT ĐỘNG

## 📋 Tình Trạng Hiện Tại
- ✅ PayOs đã hiển thị "Đã thanh toán" (orderCode: 43843, 42347)
- ✅ Description: "CSCOK68MZC1 BOOKING4"
- ❌ Website chưa cập nhật status thành "Paid"
- ❌ QR code chưa biến mất

---

## 🎯 3 NGUYÊN NHÂN CHÍNH

### 1. ❌ PayOs Không Gửi Webhook (Nguyên nhân phổ biến nhất - 80%)

**Triệu chứng:**
- PayOs hiển thị "Đã thanh toán" nhưng backend không nhận được webhook
- Logs trên Render **KHÔNG CÓ** entry `[WEBHOOK-xxx]`

**Nguyên nhân:**
- Webhook URL chưa được config trong PayOs
- PayOs không tự động gửi webhook (cần config thủ công)
- Webhook URL không accessible từ PayOs server

**Giải pháp:**
```bash
# Chạy script config webhook
./config-payos-webhook.sh

# Hoặc config thủ công qua PayOs dashboard
```

**Kiểm tra:**
- Xem logs trên Render: https://dashboard.render.com -> Logs
- Tìm: `[WEBHOOK-xxx]` entries
- Nếu không có → PayOs không gửi webhook

---

### 2. ❌ Webhook Format Không Đúng (15%)

**Triệu chứng:**
- Backend nhận được webhook nhưng không parse được
- Logs có: `⚠️ Cannot extract booking ID`

**Nguyên nhân:**
- Description từ PayOs không có format "BOOKING4"
- PayOs gửi description khác: "CSCOK68MZC1" (không có "BOOKING4")

**Logic Extract:**
- Pattern: `@"BOOKING(\d+)"` → Match "BOOKING4" trong "CSCOK68MZC1 BOOKING4" ✅
- Nếu description = "CSCOK68MZC1" (không có "BOOKING4") → ❌ Không extract được

**Giải pháp:**
- Kiểm tra description trong PayOs có đúng format không
- Update logic extract nếu PayOs gửi format khác

---

### 3. ❌ Backend Không Nhận Được Webhook (5%)

**Triệu chứng:**
- PayOs đã gửi webhook (theo PayOs dashboard)
- Backend logs không có entry

**Nguyên nhân:**
- CORS issue
- Firewall/Network blocking
- Webhook URL không accessible

**Giải pháp:**
- Test webhook endpoint: `curl https://quanlyresort.onrender.com/api/simplepayment/webhook`
- Kiểm tra network/firewall settings

---

## 🧪 CÁCH KIỂM TRA

### Bước 1: Kiểm Tra Logs Trên Render
```
1. Vào: https://dashboard.render.com
2. Chọn service: quanlyresort
3. Click "Logs"
4. Tìm: [WEBHOOK-xxx] hoặc "Webhook received"
```

**Nếu KHÔNG CÓ logs:**
→ **Nguyên nhân #1: PayOs không gửi webhook**

**Nếu CÓ logs nhưng có lỗi:**
→ Xem chi tiết lỗi trong logs

### Bước 2: Test Webhook Thủ Công
```bash
# Chạy script test
./test-payos-webhook.sh 4

# Hoặc test trực tiếp
curl -X POST "https://quanlyresort.onrender.com/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d '{
    "code": "00",
    "desc": "success",
    "data": {
      "orderCode": 43843,
      "amount": 5000,
      "description": "CSCOK68MZC1 BOOKING4",
      "accountNumber": "0901329227"
    }
  }'
```

**Nếu test thành công:**
→ Backend hoạt động đúng, vấn đề là PayOs không gửi webhook

**Nếu test thất bại:**
→ Backend có vấn đề, xem logs để debug

### Bước 3: Kiểm Tra PayOs Dashboard
```
1. Vào PayOs dashboard
2. Xem payment history
3. Kiểm tra webhook logs (nếu có)
4. Xem description có đúng format không
```

---

## 🔧 GIẢI PHÁP

### Giải Pháp 1: Config Webhook Lại (Khuyến nghị)
```bash
# Chạy script config
./config-payos-webhook.sh

# Kiểm tra response có 200 OK không
```

### Giải Pháp 2: Test Webhook Thủ Công
```bash
# Test với booking 4
./test-payos-webhook.sh 4

# Kiểm tra booking 4 có update thành "Paid" không
```

### Giải Pháp 3: Manual Update (Tạm thời)
```bash
# Sử dụng endpoint manual update
curl -X POST "https://quanlyresort.onrender.com/api/simplepayment/manual-update-paid/4" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

---

## 📊 XÁC SUẤT NGUYÊN NHÂN

| Nguyên nhân | Xác suất | Triệu chứng |
|------------|----------|-------------|
| PayOs không gửi webhook | 80% | Không có logs `[WEBHOOK-xxx]` |
| Webhook format không đúng | 15% | Có logs nhưng `⚠️ Cannot extract booking ID` |
| Backend không nhận được | 5% | PayOs đã gửi nhưng backend không nhận |

---

## ✅ CHECKLIST

- [ ] Kiểm tra logs trên Render → Có `[WEBHOOK-xxx]` không?
- [ ] Test webhook thủ công → Có 200 OK không?
- [ ] Config webhook lại → `./config-payos-webhook.sh`
- [ ] Kiểm tra PayOs dashboard → Webhook có được gửi không?
- [ ] Kiểm tra description → Có "BOOKING4" không?

---

## 🎯 KẾT LUẬN

**Nguyên nhân có khả năng cao nhất (80%):**
→ **PayOs không gửi webhook** (webhook URL chưa được config hoặc PayOs không tự động gửi)

**Giải pháp ngay:**
1. Chạy `./config-payos-webhook.sh` để config webhook lại
2. Kiểm tra logs trên Render
3. Test webhook thủ công với `./test-payos-webhook.sh 4`
4. Nếu vẫn không hoạt động, dùng endpoint manual update

