# 🔑 Hướng Dẫn Tạo Classic Token (Để Push Code)

## ⚠️ Lưu Ý Quan Trọng

Bạn đang ở trang **"Fine-grained tokens"** - loại token này phức tạp hơn.

**Để push code, nên dùng "Tokens (classic)"** - đơn giản và đủ dùng!

## 🔄 Cách Chuyển Sang Classic Token

### Bước 1: Quay Lại Trang Trước

1. Click vào **"Personal access tokens"** ở sidebar bên trái
2. Hoặc click **"Cancel"** ở form hiện tại

### Bước 2: Chọn "Tokens (classic)"

Trong sidebar, dưới "Personal access tokens", click:
- **"Tokens (classic)"** (không phải "Fine-grained tokens")

### Bước 3: Tạo Classic Token

1. Click nút **"Generate new token"** → **"Generate new token (classic)"**
2. **Token name:** `quanlyresort-deploy`
3. **Expiration:** Chọn "No expiration" hoặc thời gian cụ thể
4. **Select scopes:** ✅ Chọn **"repo"** (full control)
5. Click **"Generate token"**
6. **Copy token ngay** (chỉ hiện 1 lần!)

## 🚀 Sau Khi Có Token

```bash
cd "/Users/vyto/Downloads/QuanLyResort-main (1)/QuanLyResort-main"
git push -u origin main
```

**Khi được hỏi:**
- **Username:** `Lamm123435469898`
- **Password:** [Dán PAT token vừa tạo]

## 💡 Tại Sao Dùng Classic Token?

- ✅ **Đơn giản hơn:** Chỉ cần chọn scope `repo`
- ✅ **Đủ dùng:** Push/pull code không cần cấu hình phức tạp
- ✅ **Tương thích tốt:** Hoạt động với mọi Git client

## 🔧 Nếu Muốn Dùng Fine-Grained Token

Nếu bạn muốn tiếp tục với Fine-grained token:

1. **Token name:** `quanlyresort-deploy`
2. **Resource owner:** `Lamm123435469898` (đã chọn đúng)
3. **Repository access:** Chọn **"All repositories"** (không phải "Public repositories")
4. **Permissions:** 
   - Click **"+ Add permissions"**
   - Chọn **"Repository permissions"**
   - Chọn:
     - ✅ **Contents** (Read and write)
     - ✅ **Metadata** (Read-only)
5. **Expiration:** Chọn thời gian (ví dụ: 90 days)
6. Click **"Generate token"**

**Lưu ý:** Fine-grained token phức tạp hơn và có thể cần thêm permissions tùy theo nhu cầu.

