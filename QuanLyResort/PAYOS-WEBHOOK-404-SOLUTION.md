# 🔧 Giải Pháp PayOs Webhook 404 Error

## ❌ Kết Quả API Call

PayOs API vẫn trả về lỗi:
```json
{
  "code": "20",
  "desc": "Webhook url invalid",
  "data": "Request failed with status code 404"
}
```

## 🔍 Phân Tích

### Railway Endpoint Hoạt Động ✅

Endpoint Railway đã được test và hoạt động tốt:
- GET request: ✅ Trả về `{"status":"active",...}`
- POST request (empty body): ✅ Trả về `{"status":"active",...}`

### PayOs Vẫn Báo 404 ❌

PayOs đang verify webhook URL nhưng nhận được 404. Có thể:
1. PayOs đang gọi endpoint khác (không phải `/api/simplepayment/webhook`)
2. PayOs đang verify với method/headers khác
3. PayOs có vấn đề với Railway domain (`up.railway.app`)
4. PayOs đang cache kết quả verify cũ

## ✅ Giải Pháp

### Option 1: Giữ Render URL Tạm Thời (Khuyến Nghị)

Vì PayOs vẫn báo 404 với Railway URL:

1. **Giữ Render URL trên PayOs:**
   ```
   https://quanlyresort.onrender.com/api/simplepayment/webhook
   ```

2. **Đảm bảo Render service vẫn chạy** (nếu có)

3. **Webhook sẽ tiếp tục hoạt động** với Render URL

### Option 2: Kiểm Tra Railway Logs

Kiểm tra xem PayOs có gửi verification request đến Railway không:

1. **Vào Railway Dashboard** → Service `quanlyresort`
2. **Tab "Logs"**
3. **Tìm requests từ PayOs:**
   - IP addresses từ PayOs
   - User-Agent có chứa "PayOs"
   - Requests đến `/api/simplepayment/webhook`

**Nếu thấy:**
```
[WEBHOOK-VERIFY] PayOs verification request received
```
→ PayOs đã gửi request nhưng có thể có vấn đề khác

**Nếu không thấy:**
→ PayOs chưa gửi request, có thể đang cache hoặc có vấn đề với domain

### Option 3: Đợi Và Thử Lại

PayOs có thể cần thời gian để verify:

1. **Đợi 30-60 phút**
2. **Thử lại API call:**
   ```bash
   curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
     -H "Content-Type: application/json" \
     -H "x-client-id: c704495b-5984-4ad3-aa23-b2794a02aa83" \
     -H "x-api-key: f6ea421b-a8b7-46b8-92be-209eb1a9b2fb" \
     -d '{"webhookUrl": "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"}'
   ```

### Option 4: Liên Hệ PayOs Support

Nếu vẫn lỗi sau 1 giờ:

1. **Liên hệ PayOs support**
2. **Cung cấp thông tin:**
   - Webhook URL: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
   - Lỗi: "Request failed with status code 404"
   - Test result: Endpoint hoạt động khi test bằng curl
   - Client ID: `c704495b-5984-4ad3-aa23-b2794a02aa83`

3. **Hỏi về:**
   - Cách PayOs verify webhook URL
   - Có vấn đề gì với Railway domain không
   - Có thể dùng Railway URL không

## 📋 Checklist

- [ ] Đã test Railway endpoint (GET và POST) - ✅ Hoạt động
- [ ] Đã gọi PayOs API để cập nhật webhook URL - ❌ Vẫn báo 404
- [ ] Đã kiểm tra Railway logs (PayOs có gửi request không)
- [ ] Đã đợi 30-60 phút và thử lại
- [ ] Đã liên hệ PayOs support (nếu cần)

## 💡 Khuyến Nghị

**Hiện tại:**
- ✅ Railway endpoint hoạt động tốt
- ❌ PayOs vẫn báo 404 khi verify
- ✅ Render URL vẫn hoạt động

**Giải pháp tốt nhất:**
1. **Giữ Render URL tạm thời** để webhook tiếp tục hoạt động
2. **Đợi PayOs fix** hoặc liên hệ PayOs support
3. **Khi PayOs fix xong**, cập nhật lại sang Railway URL

## 🎯 Kết Luận

- Railway endpoint đã hoạt động và sẵn sàng
- PayOs có vấn đề khi verify Railway URL
- Nên giữ Render URL tạm thời để webhook tiếp tục hoạt động
- Liên hệ PayOs support nếu cần hỗ trợ

