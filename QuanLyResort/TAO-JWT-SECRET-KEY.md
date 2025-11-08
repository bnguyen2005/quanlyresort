# 🔐 Hướng Dẫn Tạo JWT Secret Key

## ❓ JWT Secret Key Là Gì?

**JWT Secret Key** là một chuỗi bí mật dùng để:
- ✅ **Ký (sign)** JWT tokens khi tạo
- ✅ **Xác thực (verify)** JWT tokens khi nhận
- ✅ Đảm bảo tokens không bị giả mạo

## ⚠️ Yêu Cầu

- **Độ dài:** Tối thiểu 32 ký tự (khuyến nghị 64+)
- **Tính ngẫu nhiên:** Phải là chuỗi ngẫu nhiên, không đoán được
- **Bảo mật:** KHÔNG được commit vào Git, phải giữ bí mật

## 🔧 Cách 1: Tạo Bằng Python (Khuyến Nghị)

```bash
python3 -c "import secrets; import string; chars = string.ascii_letters + string.digits + '!@#$%^&*()_+-=[]{}|;:,.<>?'; print(''.join(secrets.choice(chars) for _ in range(64)))"
```

## 🔧 Cách 2: Tạo Bằng OpenSSL

```bash
openssl rand -base64 48
```

## 🔧 Cách 3: Tạo Bằng Online Tool

1. Vào: https://randomkeygen.com/
2. Chọn "CodeIgniter Encryption Keys"
3. Copy một key (64 ký tự)

## 🔧 Cách 4: Tạo Thủ Công

Tạo chuỗi ngẫu nhiên 64 ký tự gồm:
- Chữ cái (a-z, A-Z)
- Số (0-9)
- Ký tự đặc biệt (!@#$%^&*()_+-=[]{}|;:,.<>?)

**Ví dụ:**
```
aB3$kL9#mN2@qR7!wT5&yU8*pI0^oP4+eA6-rS1=tD9[uF3]vG7{hJ2}jK5|lZ8;xC1:zV4<bN6>mM9?
```

## 📋 Sử Dụng Trong Render

Sau khi tạo key, thêm vào Environment Variables:

```
JwtSettings__SecretKey = [KEY_VỪA_TẠO]
```

**Ví dụ:**
```
JwtSettings__SecretKey = aB3$kL9#mN2@qR7!wT5&yU8*pI0^oP4+eA6-rS1=tD9[uF3]vG7{hJ2}jK5|lZ8;xC1:zV4<bN6>mM9?
```

## 🔒 Lưu Ý Bảo Mật

- ✅ **Lưu key vào password manager** (1Password, LastPass, etc.)
- ✅ **KHÔNG commit key vào Git**
- ✅ **KHÔNG chia sẻ key công khai**
- ✅ **Dùng key khác nhau cho môi trường khác nhau** (dev, staging, production)

## 🔄 Thay Đổi Key

Nếu cần thay đổi key:
1. Tạo key mới
2. Cập nhật trong Render Environment Variables
3. **Lưu ý:** Tất cả tokens cũ sẽ không còn hợp lệ
4. Users cần đăng nhập lại

## 💡 Key Mẫu (Chỉ Dùng Cho Development)

Nếu chỉ test local, có thể dùng:
```
YourSuperSecretKeyForJWTTokenGeneration2025!@#$
```

**⚠️ KHÔNG dùng key này cho production!**

## ✅ Checklist

- [ ] Key có độ dài ≥ 32 ký tự
- [ ] Key là ngẫu nhiên, không đoán được
- [ ] Key đã được lưu an toàn
- [ ] Key KHÔNG có trong Git
- [ ] Key đã được thêm vào Render Environment Variables

