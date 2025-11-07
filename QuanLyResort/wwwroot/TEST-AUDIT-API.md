# 🧪 HƯỚNG DẪN TEST AUDIT LOG API

## 📋 Thông tin API

**Base URL**: `http://localhost:5130/api/audit-log`

**Phân quyền**: Chỉ Admin, Manager, Accounting mới có quyền truy cập

## 🔑 Tính năng nâng cấp

1. ✅ **Tự động capture IP Address** - Lấy từ `HttpContext.Connection.RemoteIpAddress`
2. ✅ **Tự động capture User Agent** - Lấy từ `HttpContext.Request.Headers["User-Agent"]`
3. ✅ **Tự động lấy Username** - Từ JWT Claims nếu không truyền vào
4. ✅ **Route đúng yêu cầu** - `/api/audit-log` (không phải `/api/audit`)

## 🧪 Các API Endpoints

### 1. **GET /api/audit-log** - Lấy danh sách audit logs

```javascript
// Test trong Console (F12)
const token = localStorage.getItem('token');

fetch('http://localhost:5130/api/audit-log', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
})
.then(r => r.json())
.then(data => {
  console.log('📊 Total logs:', data.pagination.totalCount);
  console.log('📄 Logs:', data.logs);
  
  // Kiểm tra xem có IP và UserAgent không
  if (data.logs.length > 0) {
    const firstLog = data.logs[0];
    console.log('✅ Sample log:');
    console.log('  - Performed By:', firstLog.performedBy);
    console.log('  - IP Address:', firstLog.ipAddress);
    console.log('  - User Agent:', firstLog.userAgent);
    console.log('  - Timestamp:', firstLog.timestamp);
  }
});
```

**Query Parameters**:
- `entityName` - Lọc theo entity (User, Employee, Booking, ...)
- `entityId` - Lọc theo ID của entity
- `action` - Lọc theo action (Create, Update, Delete, Login, ...)
- `performedBy` - Lọc theo username
- `startDate` - Từ ngày (format: YYYY-MM-DD)
- `endDate` - Đến ngày (format: YYYY-MM-DD)
- `page` - Trang số (default: 1)
- `pageSize` - Số records mỗi trang (default: 50)

**Ví dụ với filters**:
```javascript
// Lấy logs của User entity trong hôm nay
const today = new Date().toISOString().split('T')[0];
fetch(`http://localhost:5130/api/audit-log?entityName=User&startDate=${today}`, {
  headers: { 'Authorization': `Bearer ${token}` }
})
.then(r => r.json())
.then(data => console.log('User logs today:', data));
```

---

### 2. **GET /api/audit-log/entity/{entityName}/{entityId}** - Lấy logs theo entity cụ thể

```javascript
// Xem tất cả thay đổi của User có ID = 1
fetch('http://localhost:5130/api/audit-log/entity/User/1', {
  headers: { 'Authorization': `Bearer ${token}` }
})
.then(r => r.json())
.then(logs => {
  console.log('📜 History of User #1:', logs);
  logs.forEach(log => {
    console.log(`  - ${log.timestamp}: ${log.action} by ${log.performedBy}`);
  });
});
```

---

### 3. **GET /api/audit-log/user-activity** - Thống kê hoạt động theo user

```javascript
// Xem user nào hoạt động nhiều nhất
fetch('http://localhost:5130/api/audit-log/user-activity', {
  headers: { 'Authorization': `Bearer ${token}` }
})
.then(r => r.json())
.then(data => {
  console.log('👥 User Activity:', data);
  data.forEach(user => {
    console.log(`\n👤 ${user.user}:`);
    console.log(`  - Total actions: ${user.totalActions}`);
    console.log(`  - Last activity: ${user.lastActivity}`);
    console.log('  - Actions breakdown:');
    user.actionsByType.forEach(a => {
      console.log(`    * ${a.action}: ${a.count}`);
    });
  });
});
```

---

### 4. **GET /api/audit-log/entity-statistics** - Thống kê theo entity

```javascript
// Xem entity nào có nhiều thay đổi nhất
fetch('http://localhost:5130/api/audit-log/entity-statistics', {
  headers: { 'Authorization': `Bearer ${token}` }
})
.then(r => r.json())
.then(data => {
  console.log('📊 Entity Statistics:', data);
  data.forEach(entity => {
    console.log(`\n📦 ${entity.entityName}:`);
    console.log(`  - Creates: ${entity.creates}`);
    console.log(`  - Updates: ${entity.updates}`);
    console.log(`  - Deletes: ${entity.deletes}`);
    console.log(`  - Total: ${entity.totalActions}`);
  });
});
```

---

### 5. **GET /api/audit-log/action-types** - Danh sách action types

```javascript
fetch('http://localhost:5130/api/audit-log/action-types', {
  headers: { 'Authorization': `Bearer ${token}` }
})
.then(r => r.json())
.then(actions => {
  console.log('🎬 Available action types:', actions);
});
```

---

### 6. **GET /api/audit-log/entity-types** - Danh sách entity types

```javascript
fetch('http://localhost:5130/api/audit-log/entity-types', {
  headers: { 'Authorization': `Bearer ${token}` }
})
.then(r => r.json())
.then(entities => {
  console.log('📦 Available entity types:', entities);
});
```

---

### 7. **DELETE /api/audit-log/cleanup** - Xóa logs cũ (Admin only)

```javascript
// Xóa logs cũ hơn 90 ngày
fetch('http://localhost:5130/api/audit-log/cleanup?daysToKeep=90', {
  method: 'DELETE',
  headers: { 'Authorization': `Bearer ${token}` }
})
.then(r => r.json())
.then(data => {
  console.log('🗑️ Cleanup result:', data);
  console.log(`  - Deleted ${data.deletedCount} logs`);
});
```

---

## 🧪 Test Scenarios

### ✅ Test 1: Admin có thể xem audit logs

1. Đăng nhập với Admin: `admin@resort.test` / `Admin@123456`
2. Mở Console (F12)
3. Chạy:
```javascript
fetch('http://localhost:5130/api/audit-log', {
  headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
})
.then(r => r.json())
.then(data => console.log('✅ Admin can access:', data));
```
4. **Kết quả mong đợi**: Thành công, trả về danh sách logs

---

### ❌ Test 2: Customer KHÔNG thể xem audit logs

1. Đăng nhập với Customer account
2. Mở Console (F12)
3. Chạy:
```javascript
fetch('http://localhost:5130/api/audit-log', {
  headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
})
.then(r => r.json())
.then(data => console.log('Response:', data));
```
4. **Kết quả mong đợi**: 403 Forbidden

---

### ✅ Test 3: Kiểm tra tự động capture IP & UserAgent

1. Đăng nhập Admin
2. Thực hiện một action bất kỳ (ví dụ: tạo user mới)
3. Xem audit log mới nhất:
```javascript
fetch('http://localhost:5130/api/audit-log?page=1&pageSize=1', {
  headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
})
.then(r => r.json())
.then(data => {
  const log = data.logs[0];
  console.log('📋 Latest log:');
  console.log('  IP Address:', log.ipAddress);
  console.log('  User Agent:', log.userAgent);
  console.log('  Performed By:', log.performedBy);
});
```
4. **Kết quả mong đợi**: `ipAddress` và `userAgent` có giá trị (không null)

---

### ✅ Test 4: Filter logs theo ngày

```javascript
// Xem logs của hôm nay
const today = new Date().toISOString().split('T')[0];
fetch(`http://localhost:5130/api/audit-log?startDate=${today}`, {
  headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
})
.then(r => r.json())
.then(data => {
  console.log(`📅 Logs today (${today}):`, data.logs.length, 'records');
});
```

---

### ✅ Test 5: Xem lịch sử của một entity cụ thể

```javascript
// Xem tất cả thay đổi của User ID = 1
fetch('http://localhost:5130/api/audit-log/entity/User/1', {
  headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
})
.then(r => r.json())
.then(logs => {
  console.log('📜 User #1 history:');
  logs.forEach(log => {
    console.log(`  ${log.timestamp}: ${log.action} by ${log.performedBy}`);
    if (log.description) console.log(`    ${log.description}`);
  });
});
```

---

## 📊 Expected Data Format

### Audit Log Object:
```json
{
  "logId": 123,
  "entityName": "User",
  "entityId": 10,
  "action": "Update",
  "performedBy": "admin",
  "timestamp": "2025-10-21T10:30:00Z",
  "oldValues": "{\"email\":\"old@example.com\"}",
  "newValues": "{\"email\":\"new@example.com\"}",
  "description": "Updated user email",
  "ipAddress": "::1",
  "userAgent": "Mozilla/5.0 ..."
}
```

---

## ✅ Checklist Test Hoàn Chỉnh

- [ ] Admin có thể truy cập `/api/audit-log`
- [ ] Manager có thể truy cập `/api/audit-log`
- [ ] Accounting có thể truy cập `/api/audit-log`
- [ ] Customer KHÔNG thể truy cập (403 Forbidden)
- [ ] FrontDesk KHÔNG thể truy cập (403 Forbidden)
- [ ] Logs tự động có `ipAddress`
- [ ] Logs tự động có `userAgent`
- [ ] Logs tự động có `performedBy` (username)
- [ ] Filter theo `entityName` hoạt động
- [ ] Filter theo `date` hoạt động
- [ ] Endpoint `/entity/{name}/{id}` hoạt động
- [ ] Endpoint `/user-activity` hoạt động
- [ ] Endpoint `/entity-statistics` hoạt động
- [ ] Cleanup API hoạt động (Admin only)

---

## 🔧 Troubleshooting

1. **403 Forbidden khi Admin truy cập**:
   - Kiểm tra JWT token hợp lệ
   - Kiểm tra role trong token: `jwt.io` để decode token

2. **IP Address = null**:
   - Kiểm tra `IHttpContextAccessor` đã được inject vào `AuditService`
   - Kiểm tra `builder.Services.AddHttpContextAccessor()` trong `Program.cs`

3. **performedBy = "System"**:
   - User chưa authenticate
   - JWT claims không có `ClaimTypes.Name` hoặc `Username`

---

## 📝 Ghi chú

- Mọi action CREATE, UPDATE, DELETE trên entities quan trọng (User, Employee, Booking, Invoice, ...) đều được tự động log
- Logs được lưu vĩnh viễn trừ khi Admin chủ động cleanup
- IP Address và User Agent được capture tự động, không cần truyền vào khi gọi `AuditService.LogAsync()`

