# 🔍 Kiểm Tra Railway Đang Connect Với Repository Nào

## ✅ Cách 1: Railway Dashboard (Dễ Nhất)

**Railway Dashboard → Service → Settings → Source**

**Xem:**
- **Source Repo:** `Lamm123435469898/quanlyresort` hoặc `Lamm123435469898/quanlyresortt`
- **Branch:** `main`

**Đây là repository mà Railway đang connect!**

## ✅ Cách 2: GitHub Webhooks

**GitHub Repository → Settings → Webhooks**

**Tìm Railway webhook:**
- URL: `https://railway.app/webhook/...`
- Recent deliveries: Xem delivery mới nhất

**Nếu thấy webhook:**
- ✅ Railway đã connect với repository này
- ✅ Webhook đang hoạt động

**Nếu không thấy webhook:**
- ❌ Railway chưa connect hoặc đã disconnect
- ❌ Cần connect lại trong Railway Settings

## ✅ Cách 3: Railway Deployments

**Railway Dashboard → Deployments**

**Xem deployment mới nhất:**
- Commit message sẽ cho biết repository nào
- Commit hash sẽ khớp với GitHub repository

**Nếu thấy deployment:**
- ✅ Railway đã connect và đang deploy từ repository này

## ✅ Cách 4: Sử Dụng Script

**Chạy script:**
```bash
./QuanLyResort/check-railway-repo.sh
```

**Script sẽ:**
- ✅ Hiển thị Git remote repository
- ✅ Hướng dẫn cách kiểm tra Railway repository
- ✅ So sánh repository names

## 🔍 So Sánh Repository Names

**Git Remote (từ terminal):**
- `Lamm123435469898/quanlyresortt` (2 chữ "t")

**Railway Repo (từ Dashboard):**
- `Lamm123435469898/quanlyresort` (1 chữ "t") - CẦN KIỂM TRA

**Nếu không khớp:**
- ❌ Railway sẽ không detect commit mới
- ❌ Auto Deploy sẽ không hoạt động
- ✅ Cần disconnect và connect lại với repository đúng

## 🔧 Fix Nếu Repository Name Không Khớp

**Railway Dashboard → Settings → Source**

1. **Click "Disconnect"**
2. **Click "Connect" lại**
3. **Chọn repository:** `Lamm123435469898/quanlyresortt` (2 chữ "t")
4. **Chọn branch:** `main`
5. **Railway sẽ tự động:**
   - Tạo webhook mới
   - Connect với repository đúng
   - Bật Auto Deploy

## 📋 Checklist

- [ ] Kiểm tra Railway Dashboard → Settings → Source
- [ ] Xem "Source Repo" field
- [ ] So sánh với Git remote repository
- [ ] Kiểm tra GitHub Webhooks
- [ ] Xác nhận repository names khớp
- [ ] Nếu không khớp → Disconnect và connect lại

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Service Settings:** Railway Dashboard → Settings → Source
- **GitHub Webhooks:** GitHub Repository → Settings → Webhooks
- **Railway Deployments:** Railway Dashboard → Deployments

## 💡 Lưu Ý

1. **Repository name** - Phải khớp chính xác với Git remote
2. **Case sensitive** - Repository name phân biệt chữ hoa/thường
3. **Webhook** - Railway tự động tạo khi connect repository
4. **Auto Deploy** - Chỉ hoạt động nếu repository name khớp

## 🎯 Kết Luận

**Cách nhanh nhất:**
1. ✅ Vào Railway Dashboard → Settings → Source
2. ✅ Xem "Source Repo" field
3. ✅ So sánh với Git remote: `Lamm123435469898/quanlyresortt`

**Nếu khớp:**
- ✅ Railway đang connect đúng repository
- ✅ Auto Deploy sẽ hoạt động

**Nếu không khớp:**
- ❌ Cần disconnect và connect lại
- ❌ Railway sẽ không detect commit mới

