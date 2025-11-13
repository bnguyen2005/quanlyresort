# 🔧 Fix: Vẫn Còn Tạo QR Code Bằng PayOs

## 🔍 Nguyên Nhân

Frontend đã được cập nhật để dùng SePay, nhưng có thể vẫn còn cache cũ trong browser hoặc service worker.

## ✅ Giải Pháp

### Bước 1: Clear Browser Cache

1. **Mở Developer Tools** (F12 hoặc Ctrl+Shift+I)
2. **Vào tab "Application"** (Chrome) hoặc **"Storage"** (Firefox)
3. **Click "Clear storage"** hoặc **"Clear site data"**
4. **Chọn tất cả** và click **"Clear site data"**
5. **Reload trang** (Ctrl+Shift+R hoặc Cmd+Shift+R)

### Bước 2: Unregister Service Worker

1. **Vào tab "Application"** → **"Service Workers"**
2. **Click "Unregister"** cho service worker hiện tại
3. **Reload trang**

### Bước 3: Hard Refresh

- **Windows/Linux:** `Ctrl + Shift + R` hoặc `Ctrl + F5`
- **Mac:** `Cmd + Shift + R`

### Bước 4: Kiểm Tra Network Tab

1. **Mở Developer Tools** → **Tab "Network"**
2. **Tạo booking mới** và click "Thanh toán"
3. **Kiểm tra request** trong Network tab:
   - ✅ **Đúng:** `POST /api/simplepayment/create-qr-booking`
   - ❌ **Sai:** `POST /api/simplepayment/create-link`

### Bước 5: Kiểm Tra Console Logs

1. **Mở Developer Tools** → **Tab "Console"**
2. **Tạo booking mới** và click "Thanh toán"
3. **Tìm log:**
   - ✅ **Đúng:** `[FRONTEND] 🔄 [updatePaymentModal] Creating SePay QR code for booking:`
   - ❌ **Sai:** `[FRONTEND] 🔄 [updatePaymentModal] Creating PayOs payment link for booking:`

## 🔍 Kiểm Tra Code

### Frontend Files (Đã Đúng)

- ✅ `simple-payment.js` → Gọi `/api/simplepayment/create-qr-booking`
- ✅ `restaurant-payment.js` → Gọi `/api/simplepayment/create-qr-restaurant`

### Backend Endpoints

- ✅ **SePay (Mới):**
  - `POST /api/simplepayment/create-qr-booking`
  - `POST /api/simplepayment/create-qr-restaurant`

- ⚠️ **PayOs (Cũ - Vẫn còn nhưng không dùng):**
  - `POST /api/simplepayment/create-link` (Có thể xóa sau)
  - `POST /api/simplepayment/create-link-restaurant` (Có thể xóa sau)

## 🧪 Test Sau Khi Clear Cache

1. **Tạo booking mới**
2. **Click "Thanh toán"**
3. **Kiểm tra Network tab:**
   - Request phải là: `POST /api/simplepayment/create-qr-booking`
4. **Kiểm tra Console:**
   - Log phải có: `Creating SePay QR code`
5. **Kiểm tra QR code:**
   - QR code phải hiển thị từ SePay response

## 🐛 Nếu Vẫn Còn Lỗi

### Kiểm Tra Backend Logs

1. **Vào Railway Dashboard** → **Tab "Logs"**
2. **Tìm log khi tạo QR code:**
   - ✅ **Đúng:** `[SEPAY] 🔄 Tạo đơn hàng SePay`
   - ❌ **Sai:** `[PAYOS] 🔄 Creating PayOs payment link`

### Kiểm Tra Environment Variables

1. **Vào Railway Dashboard** → **Tab "Variables"**
2. **Đảm bảo có:**
   - `SePay__ApiToken` = `spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb`
   - `SePay__AccountId` = `SP-LIVE-LT39A334`
   - `SePay__BankCode` = `MB`

### Kiểm Tra Service Worker

1. **Mở Developer Tools** → **Tab "Application"** → **"Service Workers"**
2. **Kiểm tra service worker version:**
   - Phải là version mới nhất (không cache code cũ)
3. **Nếu cần, unregister và reload**

## 📝 Lưu Ý

1. **Browser cache** có thể giữ code JavaScript cũ
2. **Service worker** có thể cache API responses
3. **Hard refresh** sẽ force browser tải lại tất cả files
4. **Incognito mode** có thể test nhanh (không có cache)

## ✅ Kết Quả Mong Đợi

Sau khi clear cache:
- ✅ Frontend gọi endpoint SePay mới
- ✅ Backend tạo QR code qua SePay API
- ✅ QR code hiển thị từ SePay response
- ✅ Console logs hiển thị "SePay" thay vì "PayOs"

