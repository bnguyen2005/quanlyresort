# 🚀 Hướng Dẫn Đầy Đủ: Push Code Lên GitHub & Deploy

## 📋 Tổng Quan

Quy trình gồm 3 bước chính:
1. ✅ Tạo repository trên GitHub
2. ✅ Push code lên GitHub
3. ✅ Deploy lên Render

---

## 🎯 BƯỚC 1: Tạo Repository Trên GitHub

### 1.1. Vào Trang Tạo Repository

**Link:** https://github.com/new

Hoặc:
- Click nút **"+"** ở góc trên bên phải GitHub
- Chọn **"New repository"**

### 1.2. Điền Thông Tin

- **Repository name:** `quanlyresort`
- **Description:** `Quan Ly Resort Management System` (tùy chọn)
- **Visibility:** 
  - Chọn **Private** (chỉ bạn thấy) hoặc **Public** (mọi người thấy)

### 1.3. ⚠️ QUAN TRỌNG: KHÔNG Tích Các Mục Sau

- ❌ **KHÔNG** tích "Add a README file"
- ❌ **KHÔNG** tích "Add .gitignore"  
- ❌ **KHÔNG** tích "Choose a license"

*(Vì bạn đã có code sẵn rồi, không cần khởi tạo)*

### 1.4. Tạo Repository

Click nút **"Create repository"** (màu xanh lá)

---

## 🔐 BƯỚC 2: Tạo Personal Access Token (Nếu Chưa Có)

### 2.1. Vào Trang Tokens

**Link:** https://github.com/settings/tokens

### 2.2. Tạo Token Mới

1. Click **"Generate new token"** → **"Generate new token (classic)"**
2. **Token name:** `quanlyresort-deploy`
3. **Expiration:** Chọn "No expiration" hoặc thời gian cụ thể
4. **Select scopes:** ✅ Chọn **"repo"** (full control)
5. Click **"Generate token"**
6. **⚠️ Copy token ngay** (chỉ hiện 1 lần!)

**Token của bạn:** `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`

---

## 📤 BƯỚC 3: Push Code Lên GitHub

### 3.1. Mở Terminal

```bash
cd "/Users/vyto/Downloads/QuanLyResort-main (1)/QuanLyResort-main"
```

### 3.2. Kiểm Tra Trạng Thái

```bash
git status
```

**Kết quả mong đợi:** `nothing to commit, working tree clean`

### 3.3. Kiểm Tra Remote

```bash
git remote -v
```

**Kết quả mong đợi:**
```
origin  https://github.com/Lamm123435469898/quanlyresort.git (fetch)
origin  https://github.com/Lamm123435469898/quanlyresort.git (push)
```

### 3.4. Push Code

```bash
git push -u origin main
```

**Khi được hỏi:**
- **Username:** `Lamm123435469898`
- **Password:** `YOUR_GITHUB_PERSONAL_ACCESS_TOKEN_HERE`

### 3.5. Kết Quả Thành Công

Bạn sẽ thấy:
```
Enumerating objects: X, done.
Counting objects: 100% (X/X), done.
Writing objects: 100% (X/X), done.
To https://github.com/Lamm123435469898/quanlyresort.git
 * [new branch]      main -> main
Branch 'main' set up to track remote branch 'main' from 'origin'.
```

### 3.6. Kiểm Tra Trên GitHub

Vào: https://github.com/Lamm123435469898/quanlyresort

Bạn sẽ thấy code đã được push lên!

---

## 🚀 BƯỚC 4: Deploy Lên Render

### 4.1. Vào Render Dashboard

**Link:** https://dashboard.render.com

### 4.2. Tạo Web Service

1. Click **"New +"** → **"Web Service"**
2. **Connect GitHub** → Chọn repository `quanlyresort`
3. **Cấu hình:**
   - **Name:** `quanlyresort-api`
   - **Environment:** `.NET`
   - **Build Command:** `dotnet publish -c Release -o ./publish`
   - **Start Command:** `dotnet ./publish/QuanLyResort.dll`
   - **Instance Type:** Free

### 4.3. Environment Variables

Thêm các biến sau:

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
ConnectionStrings__DefaultConnection=<YOUR_DB_CONNECTION>
JwtSettings__SecretKey=YourSuperSecretKeyForJWTTokenGeneration2025!@#$
BankWebhook__PayOs__ClientId=c704495b-5984-4ad3-aa23-b2794a02aa83
BankWebhook__PayOs__ApiKey=f6ea421b-a8b7-46b8-92be-209eb1a9b2fb
BankWebhook__PayOs__ChecksumKey=429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
BankWebhook__PayOs__SecretKey=429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
```

### 4.4. Deploy

Click **"Create Web Service"**

Render sẽ tự động:
- Build project
- Deploy lên server
- Tạo HTTPS URL

### 4.5. Lấy URL

Sau khi deploy xong, bạn sẽ có URL:
```
https://quanlyresort-api.onrender.com
```

---

## ✅ BƯỚC 5: Config PayOs Webhook

### 5.1. Chạy Script

```bash
cd QuanLyResort
./config-payos-after-deploy.sh https://quanlyresort-api.onrender.com
```

### 5.2. Kết Quả Thành Công

```json
{
  "code": 0,
  "desc": "success"
}
```

---

## 🎉 Hoàn Thành!

Bây giờ:
- ✅ Code đã lên GitHub
- ✅ Backend đã deploy lên Render
- ✅ PayOs webhook đã config
- ✅ Payment tự động 100%!

---

## 📚 Tài Liệu Tham Khảo

- **Push code:** `PUSH-CODE-TO-GITHUB.md`
- **Tạo token:** `TAO-PAT-TOKEN.md`
- **Deploy Render:** `QUICK-DEPLOY-RENDER.md`
- **Config PayOs:** `HUONG-DAN-CONFIG-PAYOS-API.md`

---

## ❓ Troubleshooting

### Lỗi: "Repository not found"
→ Repository chưa được tạo trên GitHub. Xem Bước 1.

### Lỗi: "Authentication failed"
→ Token không đúng hoặc hết hạn. Tạo token mới ở Bước 2.

### Lỗi: "Permission denied"
→ Token không có scope `repo`. Tạo lại token với scope `repo`.

### Lỗi: "Could not read Username"
→ Cần nhập username và password (token) khi push.

