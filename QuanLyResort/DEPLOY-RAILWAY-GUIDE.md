# 🚀 Hướng Dẫn Deploy Code Lên Railway

## 📋 Tổng Quan

Railway tự động deploy từ GitHub repository. Chỉ cần push code lên GitHub, Railway sẽ tự động build và deploy.

## 🔄 Quy Trình Deploy

### Bước 1: Commit Code

```bash
# Add các file đã thay đổi
git add QuanLyResort/Controllers/SimplePaymentController.cs
git add QuanLyResort/Services/PayOsService.cs
git add QuanLyResort/test-payos-webhook.sh
git add QuanLyResort/test-sepay-webhook.sh
git add QuanLyResort/verify-payos-webhook.sh

# Commit với message rõ ràng
git commit -m "feat: Add SePay webhook support and update PayOs integration"
```

### Bước 2: Push Lên GitHub

```bash
git push origin main
```

### Bước 3: Railway Tự Động Deploy

1. **Railway tự động detect** push mới từ GitHub
2. **Railway tự động build** Docker image
3. **Railway tự động deploy** service mới
4. **Railway tự động restart** service với code mới

## 🔍 Kiểm Tra Deploy

### Cách 1: Railway Dashboard

1. **Vào Railway Dashboard:** https://railway.app
2. **Chọn service** `quanlyresort`
3. **Tab "Deployments"**
4. **Xem deployment mới nhất:**
   - Status: "Building" → "Deploying" → "Active"
   - Thời gian: Xem khi nào deploy xong

### Cách 2: Railway Logs

1. **Tab "Logs"**
2. **Xem logs deployment:**
   ```
   Building Docker image...
   Deploying service...
   Service started successfully
   ```

### Cách 3: Test Endpoint

```bash
# Test webhook endpoint
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook

# Kết quả mong đợi:
# {"status":"active","endpoint":"/api/simplepayment/webhook",...}
```

## ⏱️ Thời Gian Deploy

- **Build time:** 2-5 phút
- **Deploy time:** 1-2 phút
- **Total:** ~3-7 phút

## ✅ Sau Khi Deploy Thành Công

### 1. Kiểm Tra Logs

Vào Railway Dashboard → Logs và tìm:
```
[PAYOS] ✅ Service initialized with ClientId: 90ad103f
[WEBHOOK] ✅ Webhook endpoint is ready
```

### 2. Test SePay Webhook

```bash
cd QuanLyResort
./test-sepay-webhook.sh
```

**Kết quả mong đợi:**
- ✅ Test 1: Format SePay - Thành công
- ✅ Test 2: Format Simple - Thành công
- ✅ Test 3: Format với description - Thành công (sau khi deploy)
- ✅ Test 4: Restaurant Order - Extract đúng
- ✅ Test 5: Verification Request - Thành công

### 3. Test PayOs Webhook

```bash
cd QuanLyResort
./test-payos-webhook.sh
```

## 🐛 Troubleshooting

### Lỗi: "Build failed"

**Nguyên nhân:**
- Lỗi syntax trong code
- Lỗi Dockerfile
- Lỗi dependencies

**Giải pháp:**
1. Xem logs trong Railway Dashboard
2. Sửa lỗi trong code
3. Commit và push lại

### Lỗi: "Deploy failed"

**Nguyên nhân:**
- Lỗi runtime
- Lỗi environment variables
- Lỗi database connection

**Giải pháp:**
1. Xem logs trong Railway Dashboard
2. Kiểm tra environment variables
3. Kiểm tra database connection string

### Deploy Thành Công Nhưng Service Không Chạy

**Nguyên nhân:**
- Lỗi runtime
- Service crash

**Giải pháp:**
1. Xem logs trong Railway Dashboard
2. Kiểm tra service status
3. Restart service nếu cần

## 📋 Checklist

- [ ] Đã commit code với message rõ ràng
- [ ] Đã push lên GitHub
- [ ] Railway đã detect push mới
- [ ] Railway đã build thành công
- [ ] Railway đã deploy thành công
- [ ] Service đang chạy (status: Active)
- [ ] Đã test webhook endpoint
- [ ] Đã test SePay webhook
- [ ] Đã test PayOs webhook

## 🔗 Links Quan Trọng

- **Railway Dashboard:** https://railway.app
- **GitHub Repository:** (kiểm tra remote URL)
- **Webhook Endpoint:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

## 💡 Lưu Ý

1. **Railway tự động deploy** - Không cần trigger thủ công
2. **Deploy time** - Có thể mất 3-7 phút
3. **Service restart** - Service sẽ restart tự động sau khi deploy
4. **Logs** - Xem logs để biết deploy có thành công không

## 🎯 Kết Quả Mong Đợi

Sau khi deploy thành công:
- ✅ Code mới đã được deploy lên Railway
- ✅ SePay webhook format được hỗ trợ
- ✅ PayOs signature format comment đã được cập nhật
- ✅ Webhook endpoint hoạt động với cả PayOs và SePay format

