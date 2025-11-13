# 🚀 Hướng Dẫn Deploy Bằng Script

## ✅ Đã Tạo Script Deploy

**File:** `QuanLyResort/deploy-railway.sh`

## 🧪 Cách Sử Dụng

### Cách 1: Chạy Script Trực Tiếp

```bash
cd QuanLyResort
./deploy-railway.sh
```

**Script sẽ:**
1. Kiểm tra git status
2. Commit thay đổi (nếu có)
3. Tạo empty commit để trigger deploy
4. Push lên GitHub
5. Railway tự động detect và deploy

### Cách 2: Dùng Railway CLI (Nếu Có)

**Nếu đã cài Railway CLI:**
```bash
railway up --detach
```

**Cài Railway CLI:**
```bash
npm i -g @railway/cli
# hoặc
brew install railway
```

## 📋 Script Hoạt Động Như Thế Nào

### Bước 1: Kiểm Tra Git Status

- Kiểm tra có thay đổi chưa commit không
- Tự động commit nếu có

### Bước 2: Tạo Empty Commit

- Tạo commit với message: `trigger: Force Railway deploy - YYYYMMDD-HHMMSS`
- Commit này sẽ trigger Railway auto deploy

### Bước 3: Push Lên GitHub

- Push commit lên branch `main`
- Railway sẽ tự động detect và deploy

## 🔍 Kiểm Tra Deploy

### Sau Khi Chạy Script

**Đợi 2-3 phút, sau đó:**

1. **Vào Railway Dashboard:** https://railway.app
2. **Chọn service `quanlyresort`**
3. **Tab "Deployments"** - Xem deployment mới
4. **Tab "Logs"** - Xem logs deployment

### Test SePay Webhook

**Sau khi deploy xong:**

```bash
curl -X POST "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "BOOKING4",
    "transferAmount": 5000,
    "transferType": "IN"
  }'
```

**Xem logs Railway, tìm:**
```
[WEBHOOK] 🔍 [WEBHOOK-xxx] Simple deserialization result: Content=..., Amount=0, TransferAmount=5000
```

**Nếu thấy `TransferAmount=5000`:**
- ✅ Code mới đã hoạt động
- ✅ SePay webhook sẽ xử lý thành công

## 📋 Checklist

- [ ] Đã chạy script deploy
- [ ] Đã đợi 2-3 phút
- [ ] Đã kiểm tra deployment trong Railway
- [ ] Đã test SePay webhook
- [ ] Đã xem logs (TransferAmount được extract?)

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Service Deployments:** Railway Dashboard → Deployments
- **Service Logs:** Railway Dashboard → Logs
- **Webhook Endpoint:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

## 💡 Lưu Ý

1. **Script tự động** - Tự động commit và push
2. **Empty commit** - Tạo commit mới để trigger deploy
3. **Auto deploy** - Railway tự động detect và deploy
4. **Deploy time** - Railway mất 2-3 phút để deploy

## 🎯 Kết Luận

**Script đã sẵn sàng:**
- ✅ `deploy-railway.sh` - Script deploy tự động
- ✅ Tự động commit và push
- ✅ Railway sẽ tự động detect và deploy

**Cách dùng:**
```bash
cd QuanLyResort
./deploy-railway.sh
```

