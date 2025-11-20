# 🔒 Hướng Dẫn Xử Lý Groq API Key Bị Lộ

## ⚠️ Tình Trạng
API Key Groq (`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`) đã bị lộ trên GitHub và sẽ bị revoke bởi Groq.

## ✅ Đã Thực Hiện
1. ✅ API key đã được xóa khỏi code (thay bằng placeholder `YOUR_GROQ_API_KEY_HERE`)
2. ✅ Đã thêm `.gitignore` để tránh commit nhầm API keys trong tương lai
3. ✅ Code hiện tại không chứa API key thật

## 🔧 Các Bước Cần Làm Ngay

### 1. Revoke API Key Cũ (Quan Trọng!)
1. Đăng nhập vào https://console.groq.com/
2. Vào **API Keys** → Tìm key `gsk_****O2Tt`
3. Click **Revoke** hoặc **Delete** để vô hiệu hóa key cũ

### 2. Tạo API Key Mới
1. Vào https://console.groq.com/ → **API Keys**
2. Click **Create API Key**
3. Đặt tên: `ResortDeluxe-Production` (hoặc tên khác)
4. Copy API key mới (format: `gsk_...`)

### 3. Cấu Hình Trên Railway
**KHÔNG** thêm API key vào code! Thay vào đó, thêm vào **Environment Variables** trên Railway:

1. Vào Railway Dashboard → Project → Service
2. Click **Variables** tab
3. Thêm biến môi trường:
   ```
   Name:  AIChat__ApiKey
   Value: gsk_your_new_api_key_here
   ```
4. Click **Add** và **Deploy** lại service

### 4. Kiểm Tra Cấu Hình
Sau khi deploy, kiểm tra logs trên Railway:
- Tìm dòng: `[AI Chat] ✅ API Key configured (length: XX, provider: groq)`
- Nếu thấy dòng này → API key đã được cấu hình đúng

## 📋 Checklist Bảo Mật

- [ ] Đã revoke API key cũ trên Groq console
- [ ] Đã tạo API key mới
- [ ] Đã thêm `AIChat__ApiKey` vào Railway Environment Variables
- [ ] Đã deploy lại service trên Railway
- [ ] Đã kiểm tra logs để xác nhận API key hoạt động
- [ ] Đã xác nhận AI Chat hoạt động bình thường

## 🚫 Lưu Ý Quan Trọng

1. **KHÔNG BAO GIỜ** commit API key vào code
2. **KHÔNG BAO GIỜ** commit vào file `appsettings.json`, `appsettings.Production.json`, hoặc bất kỳ file config nào
3. **LUÔN** sử dụng Environment Variables trên Railway/Production
4. Nếu cần test local, tạo file `appsettings.Local.json` và thêm vào `.gitignore`

## 🔍 Cách Kiểm Tra API Key Có Bị Lộ Không

Nếu nghi ngờ API key bị lộ:
1. Kiểm tra git history: `git log --all --full-history -p -S "gsk_"`
2. Nếu thấy API key trong history → Cần revoke và tạo key mới
3. Xóa key khỏi code và commit lại

## 📞 Liên Hệ Hỗ Trợ

Nếu có vấn đề:
- Groq Support: support@groq.com
- Railway Support: https://railway.app/help

