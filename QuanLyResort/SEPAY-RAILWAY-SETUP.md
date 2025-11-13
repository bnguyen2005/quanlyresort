# 🚂 Cấu Hình SePay Trong Railway

## 📋 Các Bước Cấu Hình

### Bước 1: Lấy Thông Tin Từ SePay Dashboard

1. **Đăng nhập SePay:** https://my.sepay.vn
2. **Vào phần API Settings** hoặc **Developer Settings**
3. **Lấy các thông tin sau:**
   - **API Token** (Bearer token)
   - **Account ID** (ID tài khoản SePay)
   - **Bank Code** (Mã ngân hàng: `MB`, `BIDV`, `VCB`, etc.)

### Bước 2: Thêm Environment Variables Trong Railway

1. **Vào Railway Dashboard:** https://railway.app
2. **Chọn project** `quanlyresort`
3. **Vào tab "Variables"**
4. **Thêm các biến sau:**

```env
# SePay Configuration
SePay__ApiBaseUrl=https://my.sepay.vn/userapi
SePay__ApiToken=YOUR_API_TOKEN_HERE
SePay__AccountId=YOUR_ACCOUNT_ID_HERE
SePay__BankCode=MB
```

**Ví dụ cụ thể:**
```env
SePay__ApiBaseUrl=https://my.sepay.vn/userapi
SePay__ApiToken=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
SePay__AccountId=123456
SePay__BankCode=MB
```

### Bước 3: Redeploy Service

Sau khi thêm variables:
1. Railway sẽ tự động redeploy
2. Hoặc click **"Redeploy"** trong tab **"Deployments"**

### Bước 4: Kiểm Tra Logs

1. **Vào tab "Logs"** trong Railway
2. **Tìm dòng log:**
   ```
   [SEPAY] ✅ Service initialized with ApiToken: ...
   ```
3. **Nếu thấy warning:**
   ```
   [SEPAY] ⚠️ SePay API Token chưa được cấu hình
   ```
   → Kiểm tra lại tên biến và giá trị

## ✅ Checklist

- [ ] Đã lấy API Token từ SePay Dashboard
- [ ] Đã lấy Account ID từ SePay Dashboard
- [ ] Đã xác định Bank Code (MB, BIDV, VCB, etc.)
- [ ] Đã thêm `SePay__ApiToken` vào Railway Variables
- [ ] Đã thêm `SePay__AccountId` vào Railway Variables
- [ ] Đã thêm `SePay__BankCode` vào Railway Variables (optional, default: MB)
- [ ] Đã thêm `SePay__ApiBaseUrl` vào Railway Variables (optional, default: https://my.sepay.vn/userapi)
- [ ] Railway đã redeploy thành công
- [ ] Không còn warning trong logs về SePay configuration

## 🐛 Troubleshooting

### Lỗi: "SePay service chưa được cấu hình"

**Nguyên nhân:**
- Environment variables chưa được set
- Tên biến không đúng format

**Giải pháp:**
1. Kiểm tra tên biến phải đúng format: `SePay__ApiToken` (2 dấu gạch dưới `__`)
2. Đảm bảo giá trị không có khoảng trắng ở đầu/cuối
3. Redeploy service sau khi thêm variables

### Lỗi: "SePay API error: Status=401"

**Nguyên nhân:**
- API Token không đúng hoặc đã hết hạn

**Giải pháp:**
1. Kiểm tra lại API Token trong SePay Dashboard
2. Tạo token mới nếu cần
3. Update `SePay__ApiToken` trong Railway

### Lỗi: "SePay API error: Status=404"

**Nguyên nhân:**
- Account ID không đúng
- Bank Code không đúng

**Giải pháp:**
1. Kiểm tra Account ID trong SePay Dashboard
2. Kiểm tra Bank Code (MB, BIDV, VCB, etc.)
3. Update các biến trong Railway

## 📝 Lưu Ý

1. **API Token:** Cần bảo mật, không commit vào git
2. **Tên biến:** Phải dùng `__` (2 dấu gạch dưới) để phân tách nested config
3. **Bank Code:** Mặc định là `MB` nếu không set
4. **ApiBaseUrl:** Mặc định là `https://my.sepay.vn/userapi` nếu không set

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn
- **Railway Dashboard:** https://railway.app
- **Hướng dẫn chi tiết:** Xem file `SEPAY-API-SETUP.md`

