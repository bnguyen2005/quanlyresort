# 🔄 Railway Redeploy - Cách Hoạt Động

## ✅ Câu Trả Lời Ngắn Gọn

**CÓ!** Khi Redeploy, Railway sẽ deploy từ **commit mới nhất** trên branch đã kết nối (thường là `main`).

## 🔍 Cách Redeploy Hoạt Động

### Khi Bạn Click "Redeploy"

1. **Railway lấy commit mới nhất** từ branch đã kết nối (ví dụ: `main`)
2. **Build lại Docker image** từ commit đó
3. **Deploy service mới** với code từ commit mới nhất
4. **Service restart** với code mới

### Ví Dụ

**GitHub có các commit:**
```
1377047 trigger: Force Railway redeploy - 20251114-001719
42e8ab3 fix: Add JsonPropertyName attributes for SePay fields...
3ff013a trigger: Force Railway redeploy - 20251113-233520
```

**Khi Redeploy:**
- Railway sẽ deploy từ commit `1377047` (commit mới nhất)
- Bao gồm tất cả thay đổi từ commit `42e8ab3` (fix SePay)
- Service sẽ có code mới nhất

## 📋 Các Loại Redeploy

### 1. Redeploy từ Deployment (Khuyên Dùng)

**Cách làm:**
1. Railway Dashboard → Deployments
2. Click "Redeploy" trên deployment bất kỳ
3. Railway sẽ deploy từ **commit của deployment đó**

**Lưu ý:**
- Nếu redeploy deployment cũ → Deploy code cũ
- Nếu redeploy deployment mới → Deploy code mới nhất

### 2. Redeploy Latest Commit

**Cách làm:**
1. Railway Dashboard → Command Palette (CMD + K hoặc CTRL + K)
2. Gõ "Deploy Latest Commit"
3. Railway sẽ deploy từ **commit mới nhất trên branch đã kết nối**

**Lưu ý:**
- Luôn deploy code mới nhất
- Không phụ thuộc vào deployment nào

### 3. Auto Deploy (Tự Động)

**Cách hoạt động:**
- Railway tự động detect commit mới trên GitHub
- Tự động trigger deploy
- Deploy từ commit mới nhất

## 🔍 Kiểm Tra Commit Được Deploy

### Bước 1: Xem Deployment

**Railway Dashboard → Deployments**

**Mỗi deployment hiển thị:**
- Commit hash (ví dụ: `1377047`)
- Commit message (ví dụ: "trigger: Force Railway redeploy...")
- Timestamp

### Bước 2: Xem Build Logs

**Railway Dashboard → Logs**

**Tìm:**
```
Building Docker image...
Step 1/10 : FROM mcr.microsoft.com/dotnet/sdk:8.0
...
Successfully built ...
```

**Logs sẽ hiển thị commit được build**

## ✅ Đảm Bảo Deploy Code Mới

### Cách 1: Redeploy Latest Commit

**Railway Dashboard → Command Palette (CMD + K)**
- Gõ "Deploy Latest Commit"
- Railway sẽ deploy từ commit mới nhất

### Cách 2: Kiểm Tra Deployment

**Railway Dashboard → Deployments**
- Xem deployment mới nhất
- Kiểm tra commit hash có phải commit mới nhất không
- Nếu không → Redeploy deployment mới nhất

### Cách 3: Push Commit Mới

**Nếu commit chưa được deploy:**
```bash
git commit --allow-empty -m "trigger: Force deploy"
git push origin main
```

**Railway sẽ tự động detect và deploy**

## 📋 Checklist

- [ ] Đã kiểm tra commit mới nhất trên GitHub
- [ ] Đã kiểm tra deployment mới nhất trong Railway
- [ ] Đã redeploy từ deployment mới nhất
- [ ] Đã đợi 2-3 phút
- [ ] Đã kiểm tra logs (code mới đã được deploy?)
- [ ] Đã test SePay webhook (TransferAmount được extract?)

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Service Deployments:** Railway Dashboard → Deployments
- **Service Logs:** Railway Dashboard → Logs
- **GitHub Repository:** https://github.com/Lamm123435469898/quanlyresortt

## 💡 Lưu Ý

1. **Redeploy** - Deploy từ commit của deployment đó
2. **Deploy Latest Commit** - Luôn deploy code mới nhất
3. **Auto Deploy** - Tự động deploy khi có commit mới
4. **Commit mới nhất** - Railway luôn deploy từ commit mới nhất trên branch đã kết nối

## 🎯 Kết Luận

**Khi Redeploy:**
- ✅ Railway sẽ deploy từ commit mới nhất trên branch đã kết nối
- ✅ Bao gồm tất cả thay đổi đã commit lên GitHub
- ✅ Service sẽ có code mới nhất

**Để đảm bảo deploy code mới:**
1. Kiểm tra commit mới nhất trên GitHub
2. Redeploy từ deployment mới nhất
3. Hoặc dùng "Deploy Latest Commit" từ Command Palette

