# ⚡ Quick Start - Test Nhanh

## 🚀 Deploy lên GitHub

```bash
cd "/Users/vyto/Downloads/QuanLyResort-main (1)/QuanLyResort-main"

# Kiểm tra thay đổi
git status

# Push lên GitHub (nếu cần token)
git remote set-url origin https://YOUR_TOKEN@github.com/bnguyen2005/quanlyresort.git
git push origin main
git remote set-url origin https://github.com/bnguyen2005/quanlyresort.git
```

---

## 🧪 Test Nhanh (5 phút)

### 1. Test Email (1 phút)
1. Đăng nhập → Đặt phòng
2. Kiểm tra email `phamthahlam@gmail.com`
3. ✅ Email xác nhận đặt phòng

### 2. Test 2FA (2 phút)
```bash
# Generate secret
curl -X POST http://localhost:5130/api/auth/2fa/generate \
  -H "Authorization: Bearer YOUR_TOKEN"

# Scan QR code vào Google Authenticator
# Enable với code từ app
curl -X POST http://localhost:5130/api/auth/2fa/enable \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{"code": "123456"}'
```

### 3. Test i18n (1 phút)
```bash
# Get translations
curl http://localhost:5130/api/localization/strings?lang=en

# Change language
curl -X POST http://localhost:5130/api/localization/set-language \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{"language": "en"}'
```

### 4. Test Notifications (1 phút)
```javascript
// Browser console
Notification.requestPermission();
window.notificationService.loadUnreadCount();
```

---

## 📚 Tài liệu chi tiết

- **DEPLOYMENT-GUIDE.md** - Hướng dẫn deploy đầy đủ
- **TEST-GUIDE.md** - Hướng dẫn test từng bước
- **ADVANCED-FEATURES-IMPLEMENTATION.md** - Tài liệu kỹ thuật

---

## ✅ Checklist Nhanh

- [ ] Email notifications hoạt động
- [ ] 2FA generate secret thành công
- [ ] i18n get translations thành công
- [ ] Push notifications request permission thành công

