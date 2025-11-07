# 🔑 Hướng Dẫn Tạo Personal Access Token (PAT)

## 📋 Bước 1: Click "Tạo mã thông báo mới"

Trên trang GitHub Settings, click nút **"Tạo mã thông báo mới"** (Generate new token) ở góc phải.

## 📋 Bước 2: Chọn "Generate new token (classic)"

Trong dropdown menu, chọn **"Generate new token (classic)"**.

## 📋 Bước 3: Đặt Tên Token

- **Note:** `quanlyresort-deploy` (hoặc tên bạn muốn)
- Mô tả: Token để deploy QuanLyResort lên Render

## 📋 Bước 4: Chọn Expiration

- **No expiration** (không hết hạn) - cho development
- Hoặc chọn thời gian cụ thể (ví dụ: 90 days)

## 📋 Bước 5: Chọn Scopes (Quan Trọng!)

✅ **BẮT BUỘC:** Chọn scope `repo` (full control)
- Đánh dấu checkbox **"repo"**
- Điều này cho phép token push/pull code

**Các scope khác (tùy chọn):**
- `workflow` - nếu dùng GitHub Actions
- `write:packages` - nếu publish packages

## 📋 Bước 6: Generate Token

1. Scroll xuống cuối trang
2. Click **"Generate token"**
3. **⚠️ QUAN TRỌNG:** Copy token ngay lập tức!
   - Token chỉ hiện 1 lần
   - Nếu đóng trang, bạn sẽ không thấy lại được
   - Phải tạo token mới

## 📋 Bước 7: Lưu Token An Toàn

- Lưu token vào password manager (1Password, LastPass, etc.)
- Hoặc lưu tạm vào file text (xóa sau khi dùng xong)
- **KHÔNG** commit token vào Git!

## 🚀 Bước 8: Push Code

Sau khi có token, chạy:

```bash
cd "/Users/vyto/Downloads/QuanLyResort-main (1)/QuanLyResort-main"
git push -u origin main
```

**Khi được hỏi:**
- **Username:** `Lamm123435469898`
- **Password:** [Dán PAT token vừa tạo]

## 💡 Lưu Ý

- Token "Con trỏ" (Cursor) hiện có có thể dùng được nếu có scope `repo`
- Nhưng nên tạo token riêng cho project này để dễ quản lý
- Token "Con trỏ" hết hạn vào 7/12/2025

## ✅ Sau Khi Push Thành Công

Bạn sẽ thấy:
```
Enumerating objects: X, done.
Counting objects: 100% (X/X), done.
Writing objects: 100% (X/X), done.
To https://github.com/Lamm123435469898/quanlyresort.git
 * [new branch]      main -> main
```

Sau đó có thể deploy lên Render! 🎉

