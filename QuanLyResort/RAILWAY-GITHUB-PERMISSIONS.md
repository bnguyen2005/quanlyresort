# 🔐 Railway GitHub App Permissions

## ✅ Quyền Hiện Tại

Từ thông tin bạn cung cấp:
- ✅ **Read access to metadata** - Đọc thông tin repository
- ✅ **Read and write access to:**
  - ✅ **actions** - GitHub Actions
  - ✅ **administration** - Quản lý repository
  - ✅ **checks** - Status checks
  - ✅ **code** - Đọc code (quan trọng!)
  - ✅ **commit statuses** - Commit status
  - ✅ **deployments** - Deployments (quan trọng!)
  - ✅ **pull requests** - Pull requests
  - ✅ **workflows** - GitHub Actions workflows
- ✅ **Repository access:** All repositories

## ✅ Đã Đủ Quyền

**Các quyền này đã đủ để Railway:**
- ✅ Đọc code từ GitHub
- ✅ Detect commit mới
- ✅ Trigger deployment
- ✅ Tạo deployment status
- ✅ Tạo webhook để nhận thông báo từ GitHub

## 🔍 Quyền Quan Trọng Cho Auto Deploy

### 1. Code (Read) ✅

**Cần thiết:**
- Railway cần đọc code để build Docker image
- Railway cần đọc Dockerfile để build

**Hiện tại:** ✅ Có quyền "Read and write access to code"

### 2. Deployments (Write) ✅

**Cần thiết:**
- Railway cần tạo deployment khi có commit mới
- Railway cần update deployment status

**Hiện tại:** ✅ Có quyền "Read and write access to deployments"

### 3. Commit Statuses (Write) ✅

**Cần thiết:**
- Railway cần tạo commit status (success/failure)
- Hiển thị deployment status trên GitHub

**Hiện tại:** ✅ Có quyền "Read and write access to commit statuses"

## ⚠️ Nếu Railway Không Tự Động Deploy

**Nguyên nhân có thể:**
1. **Auto Deploy chưa được bật** - Kiểm tra Settings → Source
2. **GitHub webhook chưa hoạt động** - Kiểm tra Settings → Webhooks
3. **Railway đang xử lý deployment khác** - Đợi deployment hiện tại hoàn tất

**Không phải do permissions** - Quyền đã đủ!

## 🔧 Kiểm Tra Auto Deploy

### Bước 1: Kiểm Tra Settings

**Railway Dashboard → Settings → Source**

**Kiểm tra:**
- ✅ **Auto Deploy:** Enabled
- ✅ **Branch:** `main`
- ✅ **Repository:** `Lamm123435469898/quanlyresortt`

### Bước 2: Kiểm Tra GitHub Webhook

**GitHub Repository → Settings → Webhooks**

**Tìm Railway webhook:**
- URL: `https://railway.app/webhook/...`
- Events: `push`, `deployment`, etc.
- Recent deliveries: Không có lỗi

## 📋 Checklist

- [x] Railway App đã được cài đặt
- [x] Quyền đã đủ (code, deployments, commit statuses)
- [ ] Auto Deploy được bật trong Railway Settings
- [ ] GitHub webhook hoạt động
- [ ] Railway tự động detect commit mới

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **GitHub App Settings:** https://github.com/settings/installations
- **Service Settings:** Railway Dashboard → Settings
- **GitHub Webhooks:** GitHub Repository → Settings → Webhooks

## 💡 Lưu Ý

1. **Permissions** - Đã đủ, không cần thay đổi
2. **Auto Deploy** - Phụ thuộc vào Settings, không phụ thuộc vào permissions
3. **Webhook** - Railway tự động tạo webhook khi connect repository
4. **Repository access** - "All repositories" là đủ

## 🎯 Kết Luận

**Quyền hiện tại:**
- ✅ **Đã đủ** - Railway có đủ quyền để auto deploy
- ✅ **Không cần thay đổi** - Giữ nguyên permissions

**Nếu Railway không tự động deploy:**
- ⚠️ Không phải do permissions
- ⚠️ Có thể do Auto Deploy chưa được bật
- ⚠️ Có thể do GitHub webhook chưa hoạt động

**Bước tiếp theo:**
1. Kiểm tra Auto Deploy setting trong Railway
2. Kiểm tra GitHub webhook
3. Hoặc trigger deploy thủ công từ Railway Dashboard

