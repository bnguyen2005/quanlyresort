# 📋 SePay Webhook Setup - Tóm Tắt Nhanh

## 🎯 3 Bước Chính

### Bước 1: Vào SePay Dashboard
1. **Đăng nhập:** https://my.sepay.vn
2. **Vào Webhooks:** https://my.sepay.vn/webhooks
3. **Click:** "Thêm Webhook"

### Bước 2: Điền Form

**Copy-paste các giá trị này:**

| Trường | Giá Trị |
|--------|---------|
| **Đặt tên** | `ResortDeluxe` |
| **Bắn WebHooks khi** | `Có tiền vào` ✅ |
| **Khi tài khoản ngân hàng là** | `0901329227` (hoặc để trống) |
| **Bỏ qua nếu không có Code thanh toán?** | `Có` ⭐ |
| **Gọi đến URL** | `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook` ⭐ |
| **Là WebHooks xác thực thanh toán?** | `Có` ⭐ |
| **Gọi lại Webhooks khi?** | ☑ **Check** checkbox |
| **Kiểu chứng thực** | `Không cần chứng thực` (test) |
| **Request Content type** | `application/json` ✅ |
| **Trạng thái** | `Kích hoạt` ✅ |

### Bước 3: Click "Thêm"

Sau khi điền xong, **click nút "Thêm"** để tạo webhook.

---

## ⚠️ 3 Điểm Quan Trọng Nhất

### 1. URL Phải Chính Xác ⭐
```
https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```
- ✅ Copy-paste để tránh lỗi typo
- ✅ Phải là HTTPS
- ✅ Không có khoảng trắng

### 2. Chọn "Có" Cho 2 Trường ⭐
- **"Bỏ qua nếu không có Code thanh toán?"** → Chọn `Có`
- **"Là WebHooks xác thực thanh toán?"** → Chọn `Có`

### 3. Code Thanh Toán Format
- Format: `BOOKING{id}` (ví dụ: `BOOKING4`)
- Khi khách hàng thanh toán, họ cần ghi nội dung: `BOOKING4`

---

## 🧪 Test Sau Khi Setup

### Test 1: Kiểm Tra Webhook Trong Dashboard
- Vào danh sách webhook
- Xem trạng thái: "Kích hoạt"
- Xem URL verification: Thành công

### Test 2: Test Với Script
```bash
./QuanLyResort/test-sepay-webhook-production.sh
```

### Test 3: Test Với Giao Dịch Thật
1. Tạo booking mới trên website
2. Thanh toán với nội dung: `BOOKING{id}`
3. Kiểm tra Railway logs
4. Kiểm tra booking status tự động update

---

## 🔍 Kiểm Tra Logs

**Railway Dashboard → Service → Logs**

**Tìm:**
- `[WEBHOOK] 📥 Webhook received`
- `[WEBHOOK] 📋 Detected Simple/SePay format`
- `[WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId`

---

## 🐛 Troubleshooting Nhanh

**Webhook không được gửi:**
- ✅ Kiểm tra URL có đúng không
- ✅ Kiểm tra code thanh toán format: `BOOKING{id}`
- ✅ Kiểm tra webhook có được kích hoạt không

**Webhook được gửi nhưng không xử lý:**
- ✅ Kiểm tra Railway logs
- ✅ Xem có lỗi gì trong logs không
- ✅ Kiểm tra booking ID có được extract không

---

## 📋 Checklist Nhanh

- [ ] Đã vào SePay dashboard
- [ ] Đã click "Thêm Webhook"
- [ ] Đã điền URL: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- [ ] Đã chọn "Có" cho "Bỏ qua nếu không có Code thanh toán"
- [ ] Đã chọn "Có" cho "Là WebHooks xác thực thanh toán"
- [ ] Đã click "Thêm"
- [ ] Đã kiểm tra webhook trong dashboard
- [ ] Đã test với script
- [ ] Đã test với giao dịch thật

---

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn/webhooks
- **Railway Dashboard:** https://railway.app
- **Hướng dẫn chi tiết:** `SEPAY-SETUP-CHI-TIET.md`

---

## 💡 Lưu Ý

1. **URL:** Copy-paste để tránh lỗi
2. **Code thanh toán:** Format `BOOKING{id}`
3. **Test:** Test với script trước khi test với giao dịch thật
4. **Logs:** Luôn kiểm tra Railway logs để debug

---

## 🎯 Kết Luận

**Sau khi setup xong:**
- ✅ SePay sẽ tự động gửi webhook khi có giao dịch
- ✅ Railway sẽ tự động nhận và xử lý webhook
- ✅ Booking sẽ tự động update thành "Paid"
- ✅ Invoice sẽ tự động được tạo

**Không cần làm gì thêm!** 🎉

