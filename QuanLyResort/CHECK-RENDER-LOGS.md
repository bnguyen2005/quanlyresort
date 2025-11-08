# 🔍 Kiểm Tra Logs Trên Render

## ⚠️ Vấn Đề

Service đã "Live" nhưng tất cả endpoints trả về **404 Not Found**.

## 🔍 Cần Kiểm Tra Logs

### Bước 1: Vào Render Dashboard

1. Vào: https://dashboard.render.com
2. Click service `quanlyresort-api`
3. Tab **"Logs"**

### Bước 2: Tìm Các Dòng Quan Trọng

**✅ Nếu app start thành công, sẽ thấy:**

```
🔧 Checking database connection...
✅ Database ready
📦 Applying X pending migrations...
✅ Migrations applied
🌱 Seeding initial data...
✅ Data seeded successfully
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:10000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**❌ Nếu có lỗi, sẽ thấy:**

```
❌ Error initializing database
SQLite Error 1: 'no such table: Employees'
Unhandled exception...
```

### Bước 3: Kiểm Tra Các Trường Hợp

#### Trường Hợp 1: App Chưa Start

**Triệu chứng:** Không thấy "Application started"

**Nguyên nhân có thể:**
- Database error
- Missing environment variables
- Build failed

**Fix:**
- Xem error message trong logs
- Kiểm tra Environment Variables
- Redeploy

#### Trường Hợp 2: App Start Nhưng Routing Sai

**Triệu chứng:** Có "Application started" nhưng 404

**Nguyên nhân có thể:**
- Render routing configuration
- Base path issue
- Port mismatch

**Fix:**
- Kiểm tra Render Settings → Health Check Path
- Kiểm tra PORT và ASPNETCORE_URLS

#### Trường Hợp 3: App Start Nhưng Crash Ngay

**Triệu chứng:** "Application started" rồi "Exited with status"

**Nguyên nhân có thể:**
- Unhandled exception
- Database connection failed
- Missing dependencies

**Fix:**
- Xem error message ngay sau "Application started"
- Kiểm tra database connection string

## 📋 Checklist

Vui lòng kiểm tra và cho biết:

- [ ] Logs có "Application started"?
- [ ] Logs có "Now listening on: http://0.0.0.0:10000"?
- [ ] Logs có "✅ Data seeded successfully"?
- [ ] Logs có lỗi gì không? (Copy error message)
- [ ] App có crash không? (Có "Exited with status" không?)

## 💡 Quick Test

Nếu logs có "Application started", thử:

```bash
# Test với curl verbose để xem response
curl -v https://quanlyresort-api.onrender.com/api/simplepayment/webhook-status

# Test với header
curl -H "Accept: application/json" https://quanlyresort-api.onrender.com/api/simplepayment/webhook-status
```

## 🎯 Kết Quả Mong Đợi

Nếu tất cả OK, logs sẽ có:
- ✅ Database created
- ✅ Migrations applied
- ✅ Data seeded
- ✅ Application started
- ✅ Now listening on port 10000

Và endpoints sẽ trả về 200 (không phải 404).

