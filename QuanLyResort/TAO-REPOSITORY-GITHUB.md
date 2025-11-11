# 📦 Hướng Dẫn Tạo Repository Trên GitHub

## ❌ Vấn Đề

Repository `quanlyresort` chưa tồn tại trên GitHub. Cần tạo trước khi push code.

## 🚀 Cách 1: Tạo Trên GitHub Website (Khuyến Nghị)

### Bước 1: Vào Trang Tạo Repository

1. **Vào:** https://github.com/new
2. Hoặc click nút **"+"** ở góc trên bên phải → **"New repository"**

### Bước 2: Điền Thông Tin

- **Repository name:** `quanlyresort`
- **Description:** `Quan Ly Resort Management System` (tùy chọn)
- **Visibility:**
  - **Private** - Chỉ bạn mới thấy (khuyến nghị cho project cá nhân)
  - **Public** - Mọi người đều thấy

### Bước 3: Cấu Hình Repository

⚠️ **QUAN TRỌNG:** 
- ❌ **KHÔNG** tích "Add a README file"
- ❌ **KHÔNG** tích "Add .gitignore"
- ❌ **KHÔNG** tích "Choose a license"

(Vì bạn đã có code sẵn rồi, không cần khởi tạo)

### Bước 4: Tạo Repository

Click nút **"Create repository"** (màu xanh lá)

### Bước 5: Push Code

Sau khi tạo xong, quay lại terminal và chạy:

```bash
cd "/Users/vyto/Downloads/QuanLyResort-main (1)/QuanLyResort-main"
git push -u origin main
```

**Khi được hỏi:**
- **Username:** `Lamm123435469898`
- **Password:** `YOUR_GITHUB_PERSONAL_ACCESS_TOKEN_HERE`

## 🚀 Cách 2: Tạo Bằng GitHub CLI (Nếu Đã Cài)

```bash
# Cài GitHub CLI (nếu chưa có)
brew install gh

# Login GitHub
gh auth login

# Tạo repository và push code
cd "/Users/vyto/Downloads/QuanLyResort-main (1)/QuanLyResort-main"
gh repo create quanlyresort --private --source=. --remote=origin --push
```

## ✅ Sau Khi Push Thành Công

Bạn sẽ thấy:
```
Enumerating objects: X, done.
Counting objects: 100% (X/X), done.
Writing objects: 100% (X/X), done.
To https://github.com/Lamm123435469898/quanlyresort.git
 * [new branch]      main -> main
Branch 'main' set up to track remote branch 'main' from 'origin'.
```

## 🎯 Tiếp Theo: Deploy Lên Render

Sau khi push thành công, bạn có thể deploy ngay:

1. **Vào:** https://dashboard.render.com
2. **"New +" → "Web Service"**
3. **Connect GitHub** → Chọn repo `quanlyresort`
4. **Deploy theo:** `QUICK-DEPLOY-RENDER.md`

## 🔐 Lưu Ý Bảo Mật

- ✅ Token đã được lưu trong remote URL (tạm thời)
- ⚠️ Sau khi push xong, nên xóa token khỏi remote URL:
  ```bash
  git remote set-url origin https://github.com/Lamm123435469898/quanlyresort.git
  ```
- 🔒 Token vẫn cần để push/pull, nhưng không lộ trong URL

