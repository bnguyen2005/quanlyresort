# 🔧 Setup Ngrok (Bắt Buộc)

## ⚠️ Ngrok Cần Đăng Ký Tài Khoản

Ngrok yêu cầu đăng ký tài khoản miễn phí để sử dụng.

## 📋 Bước 1: Đăng Ký Ngrok

1. **Vào website:** https://dashboard.ngrok.com/signup
2. **Đăng ký tài khoản** (miễn phí)
3. **Verify email** (nếu cần)

## 📋 Bước 2: Lấy Authtoken

1. **Đăng nhập** vào https://dashboard.ngrok.com
2. **Vào trang:** https://dashboard.ngrok.com/get-started/your-authtoken
3. **Copy authtoken** (dạng: `2abc123def456ghi789jkl012mno345pq_678rst901uvw234xyz567`)

## 📋 Bước 3: Config Authtoken

```bash
# Config authtoken
ngrok config add-authtoken YOUR_AUTHTOKEN_HERE
```

**Ví dụ:**
```bash
ngrok config add-authtoken 2abc123def456ghi789jkl012mno345pq_678rst901uvw234xyz567
```

## 📋 Bước 4: Chạy Ngrok

```bash
# Chạy ngrok
ngrok http 5130
```

**Kết quả:**
```
Forwarding: https://abc123.ngrok.io -> http://localhost:5130
```

**Copy URL:** `https://abc123.ngrok.io`

## 🚀 Quick Setup (Copy & Paste)

```bash
# 1. Đăng ký tại: https://dashboard.ngrok.com/signup
# 2. Lấy authtoken tại: https://dashboard.ngrok.com/get-started/your-authtoken
# 3. Config authtoken:
ngrok config add-authtoken YOUR_AUTHTOKEN

# 4. Chạy ngrok:
ngrok http 5130

# 5. Copy URL từ output
# 6. Config trong PayOs dashboard
```

## 🔄 Alternative: Không Dùng Ngrok

Nếu không muốn dùng ngrok, có các lựa chọn khác:

### Option 1: Deploy Backend Lên Server
- Deploy lên Azure, AWS, Heroku, etc.
- Có URL public cố định
- Không cần ngrok

### Option 2: Dùng LocalTunnel (Không Cần Đăng Ký)
```bash
# Cài localtunnel
npm install -g localtunnel

# Chạy localtunnel
lt --port 5130
```

### Option 3: Dùng Cloudflare Tunnel (Free)
```bash
# Cài cloudflared
brew install cloudflared  # macOS

# Chạy tunnel
cloudflared tunnel --url http://localhost:5130
```

### Option 4: Test Manual (Không Cần Webhook)
- Test bằng manual webhook call
- Không cần expose localhost
- Chỉ để test code, không phải production

## ✅ Checklist

- [ ] Đăng ký tài khoản ngrok
- [ ] Lấy authtoken
- [ ] Config authtoken: `ngrok config add-authtoken YOUR_TOKEN`
- [ ] Chạy ngrok: `ngrok http 5130`
- [ ] Copy URL từ output
- [ ] Config URL trong PayOs dashboard

## 📝 Lưu Ý

⚠️ **Ngrok Free Plan:**
- URL thay đổi mỗi lần restart
- Cần update lại trong PayOs mỗi lần restart ngrok

💡 **Giải pháp:**
- Dùng ngrok paid plan (URL cố định)
- Hoặc deploy backend lên server

