# 📊 Hướng Dẫn Theo Dõi Deploy Trên Render

## 🎯 Cách Kiểm Tra Trạng Thái Deploy

### 1. Vào Render Dashboard

1. **Vào:** https://dashboard.render.com
2. **Click vào service:** `quanlyresort-api`

### 2. Xem Tab "Events" (Logs)

Trong trang service, bạn sẽ thấy tab **"Events"** hoặc **"Logs"**.

**Các trạng thái:**

#### 🔵 **Deploying** (Đang Deploy)
- Status: **"Deploying"** hoặc **"Building"**
- Có thể thấy:
  - "Cloning from GitHub..."
  - "Building Docker image..."
  - "Deploying..."

#### ✅ **Live** (Đã Deploy Xong)
- Status: **"Live"** (màu xanh lá)
- Service đang chạy
- Có URL: `https://quanlyresort-api.onrender.com`

#### ❌ **Failed** (Deploy Thất Bại)
- Status: **"Failed"** (màu đỏ)
- Có thể thấy lỗi trong logs

#### ⚠️ **Sleep** (Đang Ngủ - Free Tier)
- Status: **"Sleep"** (màu vàng)
- Service đã sleep sau 15 phút không có request
- Lần đầu request sẽ mất ~30 giây để wake up

### 3. Xem Logs Chi Tiết

Click vào **"Logs"** tab để xem:

#### ✅ **Deploy Thành Công:**
```
==> Deploying...
==> Starting service...
==> Service started successfully
```

#### ❌ **Deploy Thất Bại:**
```
==> Deploying...
==> Exited with status 139
==> Error: ...
```

### 4. Kiểm Tra Service Đang Chạy

#### Cách 1: Test URL Trực Tiếp

Mở trình duyệt hoặc dùng curl:
```bash
curl https://quanlyresort-api.onrender.com/api/simplepayment/webhook-status
```

**Kết quả:**
- ✅ **200 OK** → Service đang chạy
- ❌ **503 Service Unavailable** → Service đang sleep hoặc deploy
- ❌ **404 Not Found** → Route không tồn tại
- ❌ **Timeout** → Service chưa sẵn sàng

#### Cách 2: Xem Health Check

Nếu có health check endpoint:
```bash
curl https://quanlyresort-api.onrender.com/health
```

### 5. Các Dấu Hiệu Deploy Thành Công

- ✅ Status: **"Live"** (màu xanh)
- ✅ URL có thể truy cập được
- ✅ Logs không có lỗi
- ✅ API endpoint trả về 200 OK

### 6. Các Dấu Hiệu Deploy Thất Bại

- ❌ Status: **"Failed"** (màu đỏ)
- ❌ Logs có lỗi (ví dụ: "Exited with status 139")
- ❌ URL không truy cập được
- ❌ API endpoint trả về 500/503

## 🔔 Cách Nhận Thông Báo

### Email Notifications

Render sẽ gửi email khi:
- ✅ Deploy thành công
- ❌ Deploy thất bại
- ⚠️ Service có vấn đề

### Render Dashboard

- Status badge ở góc trên bên phải
- Màu xanh = Live
- Màu đỏ = Failed
- Màu vàng = Sleep

## 📋 Checklist Kiểm Tra

Sau khi deploy, kiểm tra:

- [ ] Status = "Live"
- [ ] URL có thể truy cập
- [ ] API endpoint trả về 200 OK
- [ ] Logs không có lỗi
- [ ] Database connection thành công (nếu có)

## 🧪 Test Nhanh

```bash
# Test webhook status
curl https://quanlyresort-api.onrender.com/api/simplepayment/webhook-status

# Test health (nếu có)
curl https://quanlyresort-api.onrender.com/health

# Test API với authentication
curl -H "Authorization: Bearer YOUR_TOKEN" \
  https://quanlyresort-api.onrender.com/api/bookings
```

## ⏱️ Thời Gian Deploy

- **Lần đầu:** 10-15 phút (build Docker image)
- **Các lần sau:** 5-10 phút
- **Redeploy:** 2-5 phút

## 🔄 Manual Deploy

Nếu muốn deploy lại:

1. Vào Render Dashboard → Service
2. Click **"Manual Deploy"**
3. Chọn **"Deploy latest commit"**
4. Đợi deploy xong

## 📖 Xem Chi Tiết Logs

1. Click tab **"Logs"**
2. Scroll xuống để xem logs chi tiết
3. Tìm các dòng:
   - `==> Cloning from GitHub...`
   - `==> Building...`
   - `==> Deploying...`
   - `==> Service started successfully`

## ❓ Troubleshooting

### Service không Live sau khi deploy
→ Kiểm tra logs để tìm lỗi

### Service bị Sleep
→ Đây là bình thường với Free tier. Request đầu tiên sẽ wake up service.

### Deploy mãi không xong
→ Kiểm tra:
- Build logs có lỗi không
- Environment variables đúng chưa
- Dockerfile có vấn đề không

