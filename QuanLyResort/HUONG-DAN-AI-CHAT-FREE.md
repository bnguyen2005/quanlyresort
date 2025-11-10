# 🤖 Hướng Dẫn Sử Dụng AI Chat Miễn Phí

## 📋 Tổng Quan

AI Chat hiện hỗ trợ nhiều provider, bao gồm cả các dịch vụ **MIỄN PHÍ**:

1. **Sample Mode** (Mặc định) - Không cần API key, sử dụng responses mẫu thông minh
2. **Groq** - Free tier rất tốt, nhanh, không cần thẻ tín dụng
3. **Hugging Face** - Free tier, nhiều model miễn phí
4. **Cohere** - Free tier, tốt cho tiếng Việt
5. **OpenAI** - Cần trả phí

---

## 🆓 Option 1: Sample Mode (Khuyến Nghị - Hoàn Toàn Miễn Phí)

**Không cần API key**, hệ thống sẽ trả về responses thông minh dựa trên keywords.

### Cấu hình:
```json
"AIChat": {
  "Provider": "sample",
  "ApiKey": "",
  "ApiUrl": "",
  "Model": ""
}
```

**Ưu điểm:**
- ✅ Hoàn toàn miễn phí
- ✅ Không cần đăng ký
- ✅ Hoạt động ngay lập tức
- ✅ Responses phù hợp với context resort

---

## 🚀 Option 2: Groq (Free Tier - Nhanh Nhất)

Groq cung cấp **free tier rất tốt** với tốc độ cực nhanh.

### Bước 1: Lấy API Key
1. Truy cập: https://console.groq.com/
2. Đăng ký tài khoản (miễn phí)
3. Vào "API Keys" → "Create API Key"
4. Copy API key

### Bước 2: Cấu hình
```json
"AIChat": {
  "Provider": "groq",
  "ApiKey": "gsk_your_groq_api_key_here",
  "ApiUrl": "https://api.groq.com/openai/v1/chat/completions",
  "Model": "llama-3.1-8b-instant"
}
```

**Models miễn phí:**
- `llama-3.1-8b-instant` (nhanh nhất)
- `llama-3.1-70b-versatile` (mạnh hơn)
- `mixtral-8x7b-32768` (tốt cho tiếng Việt)

**Ưu điểm:**
- ✅ Free tier rất hào phóng
- ✅ Tốc độ cực nhanh
- ✅ Không cần thẻ tín dụng
- ✅ Hỗ trợ tốt tiếng Việt

---

## 🎯 Option 3: Hugging Face (Free Tier)

Hugging Face có nhiều model miễn phí.

### Bước 1: Lấy API Key
1. Truy cập: https://huggingface.co/
2. Đăng ký tài khoản
3. Vào Settings → Access Tokens → New Token
4. Copy token

### Bước 2: Cấu hình
```json
"AIChat": {
  "Provider": "huggingface",
  "ApiKey": "hf_your_huggingface_token_here",
  "ApiUrl": "https://api-inference.huggingface.co/models/microsoft/DialoGPT-medium",
  "Model": "microsoft/DialoGPT-medium"
}
```

**Models miễn phí phổ biến:**
- `microsoft/DialoGPT-medium` (chat)
- `facebook/blenderbot-400M-distill` (chat)
- `google/flan-t5-base` (Q&A)

**Ưu điểm:**
- ✅ Free tier
- ✅ Nhiều model miễn phí
- ✅ Không cần thẻ tín dụng

**Nhược điểm:**
- ⚠️ Có thể chậm hơn Groq
- ⚠️ Format response khác, cần parse đặc biệt

---

## 🌐 Option 4: Cohere (Free Tier)

Cohere có free tier tốt cho tiếng Việt.

### Bước 1: Lấy API Key
1. Truy cập: https://cohere.com/
2. Đăng ký tài khoản
3. Vào API Keys → Create API Key
4. Copy API key

### Bước 2: Cấu hình
```json
"AIChat": {
  "Provider": "cohere",
  "ApiKey": "your_cohere_api_key_here",
  "ApiUrl": "https://api.cohere.ai/v1/chat",
  "Model": "command-r-plus"
}
```

**Ưu điểm:**
- ✅ Free tier
- ✅ Tốt cho tiếng Việt
- ✅ API đơn giản

---

## ⚙️ Cấu Hình Trên Render

Thêm vào `render.yaml` hoặc Environment Variables trên Render:

```yaml
# Sample Mode (Miễn phí, không cần API key)
- key: AIChat__Provider
  value: sample

# Hoặc Groq (Free tier)
- key: AIChat__Provider
  value: groq
- key: AIChat__ApiKey
  value: gsk_your_groq_api_key_here
- key: AIChat__ApiUrl
  value: https://api.groq.com/openai/v1/chat/completions
- key: AIChat__Model
  value: llama-3.1-8b-instant
```

---

## 🎯 Khuyến Nghị

### Development/Testing:
- Dùng **Sample Mode** - đơn giản, miễn phí, đủ dùng

### Production (nếu cần AI thật):
- Dùng **Groq** - free tier tốt, nhanh, dễ setup

---

## 📝 Lưu Ý

1. **Sample Mode** đã được tối ưu cho context resort, responses rất phù hợp
2. **Groq** là lựa chọn tốt nhất nếu muốn AI thật mà vẫn miễn phí
3. Tất cả providers đều hỗ trợ tiếng Việt
4. Có thể switch giữa các providers dễ dàng bằng cách thay đổi `Provider` trong config

---

## 🔧 Troubleshooting

### Lỗi 401 Unauthorized:
- Kiểm tra API key có đúng không
- Kiểm tra provider có đúng không
- Kiểm tra API key có còn hiệu lực không

### Lỗi Rate Limit:
- Groq: Có giới hạn requests/phút, đợi vài phút rồi thử lại
- Hugging Face: Có thể chậm khi model đang load, đợi vài giây

### Response không đúng format:
- Kiểm tra logs trên Render để xem response từ API
- Có thể cần điều chỉnh parsing logic cho từng provider

