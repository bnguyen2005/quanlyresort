# 🔧 Fix Service Worker API Fetch Errors

## ❌ Lỗi Hiện Tại

```
[Service Worker] network error for API: https://quanlyresort-production.up.railway.app/api/bookings/4
TypeError: Failed to fetch
```

**Nguyên nhân:**
- Service worker đang intercept API calls và xử lý sai
- Service worker đang can thiệp vào network requests gây lỗi CORS/network

## ✅ Giải Pháp

### Đã Sửa Service Worker

Service worker đã được sửa để **KHÔNG intercept API calls** nữa. API calls sẽ được browser xử lý trực tiếp.

### Bước 1: Clear Service Worker Cache

1. **Mở browser DevTools** (F12)
2. **Tab "Application"** → **"Service Workers"**
3. **Click "Unregister"** cho service worker hiện tại
4. **Tab "Storage"** → **"Clear site data"**
5. **Reload page** (Ctrl+Shift+R hoặc Cmd+Shift+R)

### Bước 2: Hoặc Dùng Clear Cache Page

1. **Mở:** `https://quanlyresort-production.up.railway.app/clear-cache.html`
2. **Click "Clear All"**
3. **Reload page**

### Bước 3: Kiểm Tra Service Worker Mới

1. **Mở DevTools** (F12)
2. **Tab "Application"** → **"Service Workers"**
3. **Kiểm tra:**
   - Service worker version: `resort-cache-v35` ✅
   - Status: "activated and is running" ✅

### Bước 4: Test API Calls

Mở browser console và kiểm tra:

✅ **Không còn lỗi:**
```
[Service Worker] network error for API: ...
```

✅ **API calls hoạt động bình thường:**
- Booking list load được
- Payment polling hoạt động
- QR code có thể ẩn sau khi thanh toán

## 🔍 Kiểm Tra Sau Khi Fix

### 1. Test Booking API

Mở browser console và kiểm tra:
- Không còn lỗi "Failed to fetch" từ service worker
- API calls thành công

### 2. Test Payment Polling

1. Tạo payment link
2. Mở browser console
3. Kiểm tra polling logs:
   ```
   [FRONTEND] 🔍 [SimplePolling] Poll #X - Status: ...
   ```
4. Không còn lỗi "Failed to fetch"

### 3. Test QR Code Hide

1. Thanh toán thành công
2. Kiểm tra QR code có ẩn không
3. Kiểm tra booking status có update không

## 🐛 Troubleshooting

### Lỗi: Vẫn Còn "Failed to fetch"

**Giải pháp:**
1. **Hard refresh:** Ctrl+Shift+R (Windows) hoặc Cmd+Shift+R (Mac)
2. **Clear browser cache:** Settings → Clear browsing data
3. **Unregister service worker:** DevTools → Application → Service Workers → Unregister
4. **Reload page**

### Lỗi: Service Worker Không Update

**Giải pháp:**
1. **Unregister service worker cũ**
2. **Close và mở lại browser**
3. **Reload page**

### Lỗi: API Vẫn Không Hoạt Động

**Giải pháp:**
1. **Kiểm tra network tab:** DevTools → Network
2. **Kiểm tra CORS errors:** Có thể cần cấu hình CORS trên backend
3. **Kiểm tra Railway service:** Đảm bảo service đang chạy

## 📋 Checklist

- [ ] Đã unregister service worker cũ
- [ ] Đã clear browser cache
- [ ] Đã reload page
- [ ] Service worker version mới: `resort-cache-v35`
- [ ] Không còn lỗi "Failed to fetch"
- [ ] API calls hoạt động bình thường
- [ ] Payment polling hoạt động
- [ ] QR code có thể ẩn sau khi thanh toán

## 💡 Lưu Ý

- **Service worker không intercept API calls:** API calls được browser xử lý trực tiếp
- **Cache version:** `resort-cache-v35` - force update service worker
- **Hard refresh:** Cần hard refresh để load service worker mới

## 🎯 Kết Quả

Sau khi fix:
- ✅ Không còn lỗi "Failed to fetch" từ service worker
- ✅ API calls hoạt động bình thường
- ✅ Payment polling hoạt động
- ✅ QR code có thể ẩn sau khi thanh toán
- ✅ Booking status được update đúng

