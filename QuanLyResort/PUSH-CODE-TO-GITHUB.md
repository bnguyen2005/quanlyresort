# 🚀 Hướng Dẫn Push Code Lên GitHub

## ✅ Trạng Thái Hiện Tại

- ✅ **Code đã commit:** Có commit "Initial commit"
- ✅ **Remote đã config:** `https://github.com/Lamm123435469898/quanlyresort.git`
- ❌ **Chưa push:** Cần authentication

## 🔐 Cách 1: Dùng Personal Access Token (PAT) - Khuyến Nghị

### Bước 1: Tạo Personal Access Token

1. **Vào:** https://github.com/settings/tokens
2. **Click:** "Generate new token" → "Generate new token (classic)"
3. **Đặt tên:** `quanlyresort-deploy`
4. **Chọn scope:** ✅ `repo` (full control)
5. **Click:** "Generate token"
6. **Copy token** (chỉ hiện 1 lần! Lưu lại ngay)

### Bước 2: Push Code

```bash
cd "/Users/vyto/Downloads/QuanLyResort-main (1)/QuanLyResort-main"
git push -u origin main
```

**Khi được hỏi:**
- **Username:** `Lamm123435469898`
- **Password:** [Dán PAT token của bạn] (KHÔNG phải password GitHub)

## 🔑 Cách 2: Dùng SSH (Không Cần Nhập Token Mỗi Lần)

### Bước 1: Tạo SSH Key

```bash
# Tạo SSH key
ssh-keygen -t ed25519 -C "your_email@example.com"

# Copy public key
cat ~/.ssh/id_ed25519.pub
```

### Bước 2: Thêm SSH Key Vào GitHub

1. **Vào:** https://github.com/settings/keys
2. **Click:** "New SSH key"
3. **Paste** public key vào
4. **Click:** "Add SSH key"

### Bước 3: Đổi Remote Sang SSH

```bash
cd "/Users/vyto/Downloads/QuanLyResort-main (1)/QuanLyResort-main"
git remote set-url origin git@github.com:Lamm123435469898/quanlyresort.git
git push -u origin main
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

Sau khi push xong, bạn có thể deploy ngay:

1. **Vào:** https://dashboard.render.com
2. **"New +" → "Web Service"**
3. **Connect GitHub** → Chọn repo `quanlyresort`
4. **Deploy theo:** `QUICK-DEPLOY-RENDER.md`

## 💡 Tips

- **PAT Token:** Có thể lưu trong macOS Keychain để không cần nhập lại
- **SSH:** Tiện hơn cho development, không cần nhập token mỗi lần
- **Kiểm tra push:** Vào https://github.com/Lamm123435469898/quanlyresort để xem code

