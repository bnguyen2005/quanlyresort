# 📤 Hướng dẫn Push lên GitHub

## Bước 1: Kiểm tra commits

```bash
cd "/Users/vyto/Downloads/QuanLyResort-main (1)/QuanLyResort-main"
git log --oneline -5
```

Bạn sẽ thấy:
- `Add advanced features: i18n, 2FA, email/SMS notifications, push notifications`
- `Update controllers and config for notifications integration`
- `Add quick start test guide`

## Bước 2: Push lên GitHub

### Option 1: Dùng Personal Access Token (Khuyến nghị)

```bash
# Thay YOUR_TOKEN bằng token của bạn
git remote set-url origin https://YOUR_TOKEN@github.com/bnguyen2005/quanlyresort.git
git push origin main
git remote set-url origin https://github.com/bnguyen2005/quanlyresort.git
```

### Option 2: Push trực tiếp (nếu đã cấu hình SSH)

```bash
git push origin main
```

## Bước 3: Kiểm tra trên GitHub

1. Vào https://github.com/bnguyen2005/quanlyresort
2. Kiểm tra commits mới nhất
3. Kiểm tra các file mới:
   - `DEPLOYMENT-GUIDE.md`
   - `TEST-GUIDE.md`
   - `QUICK-START-TEST.md`
   - `ADVANCED-FEATURES-IMPLEMENTATION.md`

## ⚠️ Lưu ý

- File `appsettings.json` có chứa email password - nên dùng Environment Variables trên production
- Database files (`.db`) đã được ignore bởi `.gitignore`
- Build files (`bin/`, `obj/`) đã được ignore

## ✅ Sau khi push

1. Render/Railway sẽ tự động deploy
2. Kiểm tra logs trên cloud platform
3. Test các tính năng trên production URL

