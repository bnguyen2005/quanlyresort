# 🔧 Fix Lỗi 401 Unauthorized Khi Đăng Nhập Admin

## ❌ Vấn Đề

Khi đăng nhập với:
- **Email:** `admin@resort.test`
- **Password:** `P@ssw0rd123`

Nhận được lỗi: `401 Unauthorized`

## 🔍 Nguyên Nhân

Có thể do:
1. **Database chưa có user admin** - DataSeeder chưa chạy trên Railway
2. **Password hash không khớp** - Có thể password đã bị thay đổi
3. **User bị inactive** - `IsActive = false`

## ✅ Giải Pháp

### Cách 1: Seed Data Tự Động (Khuyến Nghị)

**Gọi endpoint seed data:**
```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/admin/seed
```

Hoặc truy cập trực tiếp trong browser:
```
https://quanlyresort-production.up.railway.app/api/admin/seed
```

Endpoint này sẽ:
- Tạo tất cả users (admin, manager, frontdesk, etc.)
- Tạo rooms, bookings, services, etc.
- Trả về thông tin credentials

### Cách 2: Kiểm Tra User Có Tồn Tại Không

**Gọi endpoint check users:**
```bash
curl https://quanlyresort-production.up.railway.app/api/admin/check-users
```

Hoặc truy cập:
```
https://quanlyresort-production.up.railway.app/api/admin/check-users
```

### Cách 3: Đăng Nhập Với Username Thay Vì Email

Thử đăng nhập với:
- **Username:** `admin` (thay vì email)
- **Password:** `P@ssw0rd123`

Code đã hỗ trợ login bằng email HOẶC username.

## 📋 Credentials Mặc Định

Sau khi seed data, các users sau sẽ được tạo:

| Role | Email | Username | Password |
|------|-------|----------|----------|
| Admin | admin@resort.test | admin | P@ssw0rd123 |
| Manager | manager@resort.test | manager | P@ssw0rd123 |
| Business | business@resort.test | business | P@ssw0rd123 |
| FrontDesk | frontdesk@resort.test | frontdesk | P@ssw0rd123 |
| Cashier | cashier@resort.test | cashier | P@ssw0rd123 |
| Accounting | accounting@resort.test | accounting | P@ssw0rd123 |
| Inventory | inventory@resort.test | inventory | P@ssw0rd123 |

## 🔄 Các Bước Khắc Phục

1. **Gọi endpoint seed data:**
   ```
   POST https://quanlyresort-production.up.railway.app/api/admin/seed
   ```

2. **Đợi vài giây** để data được seed

3. **Thử đăng nhập lại** với:
   - Email: `admin@resort.test`
   - Password: `P@ssw0rd123`

4. **Nếu vẫn lỗi**, thử với username:
   - Username: `admin`
   - Password: `P@ssw0rd123`

## 🎯 Lưu Ý

- Endpoint `/api/admin/seed` là **public** (không cần authentication)
- DataSeeder chỉ tạo data nếu table còn trống (không ghi đè data hiện có)
- Nếu đã có user admin nhưng vẫn lỗi, có thể password đã bị thay đổi

## 🔗 Links

- **Seed Endpoint:** `/api/admin/seed`
- **Check Users Endpoint:** `/api/admin/check-users`
- **DataSeeder:** `QuanLyResort/Data/DataSeeder.cs`

