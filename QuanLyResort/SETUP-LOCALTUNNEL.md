# 🔧 Alternative: Dùng LocalTunnel (Không Cần Đăng Ký)

## ✅ LocalTunnel - Không Cần Đăng Ký!

LocalTunnel là alternative miễn phí, không cần đăng ký tài khoản.

## 📋 Bước 1: Cài Đặt LocalTunnel

```bash
# Cài đặt qua npm
npm install -g localtunnel
```

**Nếu chưa có Node.js:**
```bash
# Cài Node.js trước
brew install node  # macOS
```

## 📋 Bước 2: Chạy LocalTunnel

```bash
# Chạy localtunnel
lt --port 5130
```

**Kết quả:**
```
your url is: https://random-name.loca.lt
```

**Copy URL:** `https://random-name.loca.lt`

## 📋 Bước 3: Config PayOs

1. **Vào PayOs dashboard**
2. **Config Webhook URL:**
   ```
   https://random-name.loca.lt/api/simplepayment/webhook
   ```
3. **Save**

## 📋 Bước 4: Test

```bash
# Test webhook qua localtunnel
curl -X POST https://random-name.loca.lt/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content": "BOOKING-6", "amount": 5000}'
```

## ⚠️ Lưu Ý

- URL thay đổi mỗi lần restart localtunnel
- Cần giữ terminal chạy localtunnel mở
- Có thể cần chấp nhận warning từ browser lần đầu

## ✅ So Sánh

| Feature | Ngrok | LocalTunnel |
|---------|-------|-------------|
| Cần đăng ký | ✅ Có | ❌ Không |
| URL cố định | ✅ (Paid) | ❌ |
| Miễn phí | ✅ | ✅ |
| Dễ dùng | ✅ | ✅ |

## 🚀 Quick Start

```bash
# 1. Cài localtunnel
npm install -g localtunnel

# 2. Chạy
lt --port 5130

# 3. Copy URL và config trong PayOs
```

