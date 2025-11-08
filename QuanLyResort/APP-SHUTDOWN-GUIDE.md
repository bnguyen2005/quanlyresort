# ⚠️ App Đang Shutdown - Hướng Dẫn

## 🔍 Nguyên Nhân Có Thể

1. **App đang restart** sau khi deploy code mới (BÌNH THƯỜNG)
2. **App crash** do lỗi trong code
3. **Render đang redeploy** tự động

## ✅ Các Bước Kiểm Tra

### Bước 1: Đợi App Restart

**Thời gian:** 1-2 phút

App sẽ tự động restart sau khi shutdown. Đợi và xem logs tiếp theo.

### Bước 2: Kiểm Tra Logs Sau Khi Restart

**Tìm các dòng sau (theo thứ tự):**

1. **App đang start:**
   ```
   info: Microsoft.Hosting.Lifetime[14]
         Now listening on: http://0.0.0.0:10000
   ```

2. **Database initialization:**
   ```
   🔧 Checking database connection...
      Database can connect: true/false
      Total migrations: X
      Applied migrations: X
      Pending migrations: X
   ```

3. **Apply migrations:**
   ```
   📦 Creating/updating database and applying migrations...
   ✅ Database created/updated and migrations applied
   ```

4. **Seed data:**
   ```
   🌱 Seeding initial data...
   ✅ Data seeded successfully
   ```

5. **App ready:**
   ```
   info: Microsoft.Hosting.Lifetime[0]
         Application started. Press Ctrl+C to shut down.
   ```

### Bước 3: Nếu Có Lỗi

**Tìm các dòng lỗi:**

```
❌ Error initializing database
SQLite Error 1: 'no such table: ...'
Exception: ...
```

**Nếu có lỗi:**
- Copy toàn bộ error message
- Gửi để phân tích

## 🎯 Kết Quả Mong Đợi

Sau khi restart, logs sẽ có:
- ✅ App started
- ✅ Database created/updated
- ✅ Migrations applied
- ✅ Data seeded
- ✅ Service running on port 10000

## ⏱️ Timeline

- **Shutdown:** ~5 giây
- **Restart:** ~10-30 giây
- **Database init:** ~5-10 giây
- **Total:** ~20-45 giây

## 💡 Lưu Ý

- **Shutdown là bình thường** khi deploy code mới
- **Đợi app restart** trước khi test endpoints
- **Kiểm tra logs** để đảm bảo database đã được tạo

