# 🔧 Fix PayOs Webhook Timeout Error

## ❌ Lỗi Hiện Tại

Từ PayOs webhook logs:
- **HTTP 500**
- **Timeout:** "timeout of 10000ms exceeded" (10 giây)
- **Webhook URL:** `https://quanlyresort.onrender.com/api/simplepayment/webhook`
- **Thời gian phản hồi:** 10009ms (>10 giây)

**Webhook data:**
```json
{
  "code": "00",
  "desc": "success",
  "data": {
    "description": "VQRIO123",  // ❌ Không phải booking ID format
    "amount": 3000,
    "orderCode": 123
  }
}
```

## 🔍 Phân Tích

### Vấn Đề 1: Timeout (>10 giây)

**Nguyên nhân:**
1. **Render service đã dừng** hoặc đang sleep (free tier)
2. **Render service chậm** khi xử lý webhook
3. **Webhook endpoint xử lý quá lâu** (>10 giây)

**PayOs timeout:** 10 giây (10000ms)
**Webhook response time:** 10009ms → Vượt quá timeout!

### Vấn Đề 2: Description Không Đúng Format

- **Description:** `VQRIO123` ❌
- **Cần:** `BOOKING4` hoặc `BOOKING-4` ✅

Webhook không thể extract booking ID từ `VQRIO123`.

### Vấn Đề 3: HTTP 500

Có thể do:
- Render service không phản hồi kịp
- Webhook endpoint có lỗi khi xử lý
- Render service đã dừng

## ✅ Giải Pháp

### Giải Pháp 1: Restart Render Service

1. **Vào Render Dashboard:** https://dashboard.render.com
2. **Tìm service** `quanlyresort` hoặc tương tự
3. **Click "Restart"** hoặc **"Manual Deploy"**
4. **Đợi service start** (1-2 phút)

### Giải Pháp 2: Chuyển Sang Railway (Khuyến Nghị)

Vì Render có thể chậm hoặc dừng (free tier), chuyển sang Railway:

#### Bước 1: Config Webhook URL Sang Railway

```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: 90ad103f-aa49-4c33-9692-76d739a68b1b" \
  -H "x-api-key: acb138f1-a0f0-4a1f-9692-16d54332a580" \
  -d '{"webhookUrl": "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"}'
```

**Nếu vẫn báo 404:**
- PayOs có vấn đề với Railway domain
- Dùng giải pháp khác

#### Bước 2: Cập Nhật Railway Variables

1. **Railway Dashboard** → Service `quanlyresort`
2. **Tab "Variables"**
3. **Cập nhật:**
   ```env
   BankWebhook__PayOs__WebhookUrl=https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
   ```

### Giải Pháp 3: Fix Description Format

Khi tạo payment link, đảm bảo description là:
- `BOOKING4` ✅
- `BOOKING-4` ✅
- Không phải `VQRIO123` ❌

**Kiểm tra code tạo payment link:**
- Description phải là `BOOKING{id}` hoặc `BOOKING-{id}`
- Không dùng `VQRIO123` hoặc mã khác

### Giải Pháp 4: Tối Ưu Webhook Endpoint

Nếu webhook xử lý quá lâu (>10 giây):

1. **Kiểm tra database queries** - Có thể chậm
2. **Kiểm tra external API calls** - Có thể timeout
3. **Tối ưu code** để xử lý nhanh hơn

## 🔍 Kiểm Tra

### 1. Kiểm Tra Render Service

```bash
# Test Render endpoint
curl -w "\nTime: %{time_total}s\n" https://quanlyresort.onrender.com/api/simplepayment/webhook --max-time 5

# Test Render health
curl https://quanlyresort.onrender.com/api/health
```

**Nếu timeout hoặc không phản hồi:**
- Render service đã dừng
- Cần restart Render service

### 2. Kiểm Tra Railway Service

```bash
# Test Railway endpoint (nhanh hơn)
curl -w "\nTime: %{time_total}s\n" https://quanlyresort-production.up.railway.app/api/simplepayment/webhook --max-time 5
```

**Railway thường nhanh hơn Render** (không có sleep mode).

### 3. Kiểm Tra Webhook Logs

**Trên Railway/Render logs, tìm:**
- Webhook có nhận được không
- Webhook xử lý trong bao lâu
- Có lỗi gì không

## 🐛 Troubleshooting

### Lỗi: Timeout 10000ms

**Nguyên nhân:**
- Render service chậm hoặc đã dừng
- Webhook xử lý quá lâu

**Giải pháp:**
1. Restart Render service
2. Chuyển sang Railway (nhanh hơn)
3. Tối ưu webhook endpoint

### Lỗi: Description Không Đúng Format

**Nguyên nhân:**
- Description là `VQRIO123` thay vì `BOOKING4`

**Giải pháp:**
- Fix code tạo payment link
- Đảm bảo description là `BOOKING{id}`

### Lỗi: HTTP 500

**Nguyên nhân:**
- Render service không phản hồi
- Webhook endpoint có lỗi

**Giải pháp:**
1. Kiểm tra Render service
2. Kiểm tra Railway/Render logs
3. Fix lỗi trong webhook endpoint

## 📋 Checklist

- [ ] Đã kiểm tra Render service (có đang chạy không)
- [ ] Đã test Render endpoint (có phản hồi nhanh không)
- [ ] Đã config webhook URL sang Railway (nếu có thể)
- [ ] Đã fix description format (BOOKING{id})
- [ ] Đã tối ưu webhook endpoint (nếu xử lý quá lâu)
- [ ] Đã test lại thanh toán để verify

## 💡 Khuyến Nghị

**Hiện tại:**
- PayOs đã gửi webhook đến Render
- Render timeout (>10 giây)
- Description không đúng format

**Giải pháp tốt nhất:**
1. **Chuyển sang Railway** (nhanh hơn, không có sleep mode)
2. **Fix description format** (BOOKING{id})
3. **Tối ưu webhook endpoint** (xử lý nhanh hơn)

## 🎯 Kết Quả Mong Đợi

Sau khi fix:
- ✅ Webhook response time < 5 giây
- ✅ Description đúng format (BOOKING{id})
- ✅ Webhook xử lý thành công
- ✅ Booking status được update thành "Paid"
- ✅ QR code tự động ẩn

## 🔗 URLs Quan Trọng

- **Railway Webhook:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- **Render Webhook:** `https://quanlyresort.onrender.com/api/simplepayment/webhook`
- **PayOs Dashboard:** https://payos.vn
- **Render Dashboard:** https://dashboard.render.com

