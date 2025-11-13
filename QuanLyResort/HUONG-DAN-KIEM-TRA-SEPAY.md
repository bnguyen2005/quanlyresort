# 🔍 Hướng Dẫn Kiểm Tra SePay

## 📋 Các Bước Kiểm Tra SePay

### Bước 1: Đăng Ký Tài Khoản SePay

1. **Vào website:** https://sepay.vn
2. **Đăng ký tài khoản** SePay
3. **Xác thực doanh nghiệp/cá nhân** (theo hướng dẫn)

### Bước 2: Vào Webhook Dashboard

1. **Đăng nhập:** https://my.sepay.vn
2. **Vào menu Webhooks:** https://my.sepay.vn/webhooks
3. **Xem giao diện quản lý webhook**

### Bước 3: Thêm Webhook Mới

1. **Click "+ Thêm webhooks"** (góc trên bên phải)
2. **Điền thông tin:**
   - **Đặt tên:** `Resort Payment Webhook`
   - **Chọn sự kiện:** `Có tiền vào` (hoặc `Cả hai`)
   - **Chọn điều kiện:** Chọn tài khoản ngân hàng
   - **Thuộc tính WebHooks:**
     - **URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
     - **Method:** `POST`
   - **Cấu hình chứng thực WebHooks:**
     - Chọn: `API Key` hoặc `OAuth 2.0` hoặc `Không cần chứng thực`
3. **Click "Lưu"**

### Bước 4: Kiểm Tra Verify

1. **Xem trạng thái webhook** trong dashboard
2. **Kiểm tra xem SePay có verify được Railway URL không**
3. **Xem logs** (nếu có) để biết kết quả verify

### Bước 5: Xem Documentation

1. **Vào:** https://docs.sepay.vn
2. **Xem:** https://docs.sepay.vn/tich-hop-webhooks.html
3. **Xem:** https://docs.sepay.vn/lap-trinh-webhooks.html
4. **Kiểm tra:**
   - Webhook format
   - Signature verification
   - Cách xử lý webhook

### Bước 6: Test Webhook

1. **Tạo giao dịch thử nghiệm** (nếu có)
2. **Kiểm tra webhook có được gửi không**
3. **Xem logs trong SePay dashboard**
4. **Kiểm tra Railway logs** xem có nhận được webhook không

## 📊 So Sánh Với PayOs

### SePay Có Thể Tốt Hơn Nếu:

1. ✅ **Hỗ trợ nhiều ngân hàng** (không chỉ MB Bank)
2. ✅ **Dashboard webhook tốt hơn** - Quản lý dễ dàng
3. ✅ **Không gặp vấn đề Railway** - Verify thành công
4. ✅ **Chứng thực linh hoạt** - OAuth 2.0, API Key, hoặc không cần

### PayOs Vẫn Tốt Hơn Nếu:

1. ✅ **Đã tích hợp sẵn** - Có code rồi
2. ✅ **Miễn phí setup** - Không có phí
3. ✅ **API ổn định** - Đã test nhiều
4. ✅ **Documentation đầy đủ** - Đã có kinh nghiệm

## 💡 Khuyến Nghị

### Nên Thử SePay Nếu:

1. ✅ PayOs vẫn không verify được Railway domain
2. ✅ Cần hỗ trợ nhiều ngân hàng
3. ✅ SePay có phí hợp lý
4. ✅ SePay verify Railway thành công

### Không Nên Chuyển Nếu:

1. ❌ PayOs đã hoạt động tốt
2. ❌ SePay phí quá cao
3. ❌ SePay vẫn gặp vấn đề Railway
4. ❌ Webhook format phức tạp

## 🔗 Links Quan Trọng

- **SePay Website:** https://sepay.vn
- **SePay Dashboard:** https://my.sepay.vn
- **Webhook Management:** https://my.sepay.vn/webhooks
- **Documentation:** https://docs.sepay.vn
- **Webhook Integration:** https://docs.sepay.vn/tich-hop-webhooks.html
- **Webhook Programming:** https://docs.sepay.vn/lap-trinh-webhooks.html
- **Support Email:** info@sepay.vn
- **Support Hotline:** 02873.059.589

## 📋 Checklist

- [ ] Đã đăng ký tài khoản SePay
- [ ] Đã vào my.sepay.vn/webhooks
- [ ] Đã thêm webhook với Railway URL
- [ ] Đã kiểm tra SePay có verify được Railway domain không
- [ ] Đã xem documentation
- [ ] Đã so sánh với PayOs
- [ ] Đã quyết định có nên dùng SePay không

