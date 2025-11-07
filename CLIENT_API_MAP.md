# Client-API Mapping Guide

## Tổng quan

Document này mô tả chi tiết mapping giữa các trang Frontend (Customer & Admin) với API endpoints.

---

## Customer Frontend Mapping (`/customer`)

### 1. Homepage (`index.html`)

**Purpose:** Landing page, hiển thị thông tin resort, rooms preview

**API Calls:**
- `GET /api/rooms/available` → Hiển thị available rooms

**JS Integration:**
```javascript
// Load available rooms
const rooms = await getAvailableRooms();
displayRooms(rooms);
```

---

### 2. Rooms Page (`rooms.html`)

**Purpose:** Browse all rooms với filter theo room type

**API Calls:**
- `GET /api/rooms/available?roomType={type}` → Filter rooms
- `GET /api/rooms` → Get all rooms

**JS Integration:**
```javascript
// Load rooms by type
document.getElementById('roomTypeFilter').addEventListener('change', async (e) => {
    const rooms = await getAvailableRooms(e.target.value);
    displayRoomCards(rooms);
});
```

---

### 3. Room Detail (`rooms-single.html`)

**Purpose:** Xem chi tiết room và create booking

**API Calls:**
- `GET /api/rooms/{id}` → Get room details
- `POST /api/bookings` → Create booking

**JS Integration:**
```javascript
// Create booking form submit
document.getElementById('bookingForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const bookingData = {
        customerId: getUser().customerId,
        requestedRoomType: roomType,
        checkInDate: document.getElementById('checkIn').value,
        checkOutDate: document.getElementById('checkOut').value,
        numberOfGuests: document.getElementById('guests').value,
        specialRequests: document.getElementById('requests').value
    };
    
    const result = await createBooking(bookingData);
    if (result.success) {
        alert('Booking created successfully!');
        window.location.href = 'my-bookings.html';
    }
});
```

---

### 4. Login Page (`login.html`)

**Purpose:** Customer login

**API Calls:**
- `POST /api/auth/customer-login`

**JS Integration:**
```javascript
document.getElementById('loginForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    
    const result = await customerLogin(email, password);
    if (result.success) {
        window.location.href = 'my-bookings.html';
    } else {
        showError(result.message);
    }
});
```

---

### 5. Register Page (`register.html`)

**Purpose:** Customer registration

**API Calls:**
- `POST /api/auth/register-customer`

**JS Integration:**
```javascript
document.getElementById('registerForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const userData = {
        username: document.getElementById('username').value,
        email: document.getElementById('email').value,
        password: document.getElementById('password').value,
        fullName: document.getElementById('fullName').value,
        phoneNumber: document.getElementById('phone').value,
        passportNumber: document.getElementById('passport').value,
        nationality: document.getElementById('nationality').value
    };
    
    const result = await registerCustomer(userData);
    if (result.success) {
        alert('Registration successful! Please login.');
        window.location.href = 'login.html';
    }
});
```

---

### 6. My Bookings (`my-bookings.html`)

**Purpose:** View customer's booking history

**API Calls:**
- `GET /api/bookings/customer/{customerId}` → Get customer bookings
- `POST /api/bookings/{id}/transfer-to-frontdesk` → Confirm booking
- `POST /api/bookings/{id}/cancel` → Cancel booking

**JS Integration:**
```javascript
// Load bookings on page load
async function loadMyBookings() {
    const user = getUser();
    const bookings = await getCustomerBookings(user.customerId);
    displayBookingsTable(bookings);
}

// Transfer to front desk
async function handleTransfer(bookingId) {
    const result = await transferToFrontDesk(bookingId);
    if (result.success) {
        alert('Booking confirmed and sent to front desk!');
        loadMyBookings(); // Reload
    }
}

// Cancel booking
async function handleCancel(bookingId) {
    const reason = prompt('Cancellation reason:');
    if (reason) {
        const result = await cancelBooking(bookingId, reason);
        if (result.success) {
            alert('Booking cancelled');
            loadMyBookings();
        }
    }
}
```

---

## Admin Frontend Mapping (`/admin`)

### 1. Dashboard (`index.html` / `html/index.html`)

**Purpose:** Overview statistics & quick actions

**API Calls:**
- `GET /api/admin/stats` → System statistics
- `GET /api/reports/daily-occupancy` → Today's occupancy
- `GET /api/reports/daily-revenue` → Today's revenue
- `GET /api/alerts` → Unread notifications

**JS Integration:**
```javascript
async function loadDashboard() {
    // Load stats
    const stats = await getSystemStats();
    document.getElementById('totalBookings').textContent = stats.totalBookings;
    document.getElementById('checkedIn').textContent = stats.checkedInBookings;
    document.getElementById('availableRooms').textContent = stats.availableRooms;
    
    // Load occupancy
    const occupancy = await getDailyOccupancy();
    updateOccupancyChart(occupancy);
    
    // Load alerts
    const alerts = await getAlerts();
    displayAlerts(alerts);
}
```

---

### 2. Bookings Management (`bookings.html`)

**Purpose:** Manage all bookings (view, assign room, check-in, checkout)

**API Calls:**
- `GET /api/bookings` → Get all bookings
- `GET /api/bookings/{id}` → Get booking details
- `POST /api/bookings/{id}/assign-room` → Assign room
- `POST /api/bookings/{id}/checkin` → Check-in
- `POST /api/bookings/{id}/checkout` → Checkout
- `POST /api/bookings/{id}/add-charge` → Add charge

**JS Integration:**
```javascript
// Load all bookings
async function loadBookings() {
    const bookings = await getAllBookings();
    displayBookingsDataTable(bookings);
}

// Assign room modal
async function showAssignRoomModal(bookingId) {
    const rooms = await getAvailableRooms();
    showModal('assignRoomModal', rooms);
    
    document.getElementById('assignRoomBtn').onclick = async () => {
        const roomId = document.getElementById('roomSelect').value;
        const result = await assignRoom(bookingId, roomId);
        if (result.success) {
            alert('Room assigned!');
            loadBookings();
            hideModal();
        }
    };
}

// Check-in
async function handleCheckIn(bookingId) {
    if (confirm('Confirm check-in?')) {
        const result = await checkIn(bookingId);
        if (result.success) {
            alert('Check-in successful!');
            loadBookings();
        }
    }
}

// Checkout
async function handleCheckout(bookingId) {
    if (confirm('Confirm checkout? Invoice will be generated.')) {
        const result = await checkOut(bookingId);
        if (result.success) {
            alert(`Checkout successful! Invoice: ${result.invoice.invoiceNumber}`);
            window.location.href = `invoice-details.html?id=${result.invoice.invoiceId}`;
        }
    }
}

// Add charge
async function showAddChargeModal(bookingId) {
    showModal('addChargeModal');
    
    document.getElementById('addChargeForm').onsubmit = async (e) => {
        e.preventDefault();
        const chargeData = {
            serviceId: document.getElementById('serviceSelect').value,
            chargeType: 'ServiceCharge',
            description: document.getElementById('chargeDesc').value,
            amount: parseFloat(document.getElementById('amount').value),
            quantity: parseInt(document.getElementById('quantity').value),
            outletName: document.getElementById('outlet').value
        };
        
        const result = await addCharge(bookingId, chargeData);
        if (result.success) {
            alert('Charge added!');
            hideModal();
            loadBookingDetails(bookingId);
        }
    };
}
```

---

### 3. Rooms Management (`rooms.html`)

**Purpose:** Manage rooms (view, update status, add/edit)

**API Calls:**
- `GET /api/rooms` → Get all rooms
- `PUT /api/rooms/{id}/status` → Update room status
- `POST /api/rooms` → Create room (Admin)
- `PUT /api/rooms/{id}` → Update room (Admin)

**JS Integration:**
```javascript
// Load rooms
async function loadRooms() {
    const rooms = await getAllRooms();
    displayRoomsGrid(rooms);
}

// Update room status (housekeeping)
async function updateRoomStatus(roomId, status) {
    const statusData = {
        isAvailable: status === 'Ready',
        housekeepingStatus: status // 'Clean', 'Dirty', 'InProgress', 'Ready'
    };
    
    const result = await fetch(`${API_BASE_URL}/rooms/${roomId}/status`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${getToken()}`
        },
        body: JSON.stringify(statusData)
    });
    
    if (result.ok) {
        alert('Room status updated!');
        loadRooms();
    }
}
```

---

### 4. Invoices & Payment (`invoices.html`)

**Purpose:** View invoices and process payments

**API Calls:**
- `GET /api/invoices` → Get all invoices
- `GET /api/invoices/{id}` → Get invoice details
- `POST /api/invoices/{id}/pay` → Process payment

**JS Integration:**
```javascript
// Load invoices
async function loadInvoices() {
    const invoices = await getAllInvoices();
    displayInvoicesTable(invoices);
}

// Process payment
async function showPaymentModal(invoiceId) {
    const invoice = await getInvoiceById(invoiceId);
    displayInvoiceDetails(invoice);
    
    document.getElementById('paymentForm').onsubmit = async (e) => {
        e.preventDefault();
        const paymentData = {
            amount: parseFloat(document.getElementById('payAmount').value),
            paymentMethod: document.getElementById('paymentMethod').value,
            paymentReference: document.getElementById('reference').value
        };
        
        const result = await processPayment(invoiceId, paymentData);
        if (result.success) {
            alert('Payment processed successfully!');
            hideModal();
            loadInvoices();
        }
    };
}
```

---

### 5. Reports (`reports.html`)

**Purpose:** View daily reports (occupancy, revenue, reconciliation)

**API Calls:**
- `GET /api/reports/daily-occupancy?date={date}`
- `GET /api/reports/daily-revenue?date={date}`
- `GET /api/reports/service-usage?startDate={start}&endDate={end}`
- `GET /api/audit/daily-reconciliation?date={date}`

**JS Integration:**
```javascript
// Load reports
async function loadDailyReports() {
    const date = document.getElementById('reportDate').value || new Date().toISOString().split('T')[0];
    
    // Occupancy
    const occupancy = await getDailyOccupancy(date);
    displayOccupancyReport(occupancy);
    
    // Revenue
    const revenue = await getDailyRevenue(date);
    displayRevenueReport(revenue);
    
    // Reconciliation
    const reconciliation = await getDailyReconciliation(date);
    displayReconciliationReport(reconciliation);
}

// Export to Excel (client-side)
function exportToExcel() {
    // Use library like SheetJS to export table data
    const table = document.getElementById('reportTable');
    const wb = XLSX.utils.table_to_book(table);
    XLSX.writeFile(wb, `Report_${new Date().toISOString()}.xlsx`);
}
```

---

### 6. Audit Logs (`audit.html`)

**Purpose:** View system audit logs

**API Calls:**
- `GET /api/audit/logs?entityName={entity}&startDate={start}&endDate={end}`

**JS Integration:**
```javascript
async function loadAuditLogs() {
    const filters = {
        entityName: document.getElementById('entityFilter').value,
        startDate: document.getElementById('startDate').value,
        endDate: document.getElementById('endDate').value
    };
    
    const logs = await getAuditLogs(filters);
    displayAuditLogsTable(logs);
}
```

---

### 7. Inventory (`inventory.html`)

**Purpose:** Manage inventory vouchers

**API Calls:**
- `GET /api/inventory/vouchers?voucherType={type}`
- `POST /api/inventory/voucher`

**JS Integration:**
```javascript
async function loadVouchers() {
    const type = document.getElementById('typeFilter').value;
    const vouchers = await fetch(`${API_BASE_URL}/inventory/vouchers${type ? '?voucherType=' + type : ''}`, {
        headers: { 'Authorization': `Bearer ${getToken()}` }
    }).then(r => r.json());
    
    displayVouchersTable(vouchers);
}

async function createVoucher() {
    const voucherData = {
        voucherType: document.getElementById('voucherType').value,
        itemName: document.getElementById('itemName').value,
        itemCode: document.getElementById('itemCode').value,
        category: document.getElementById('category').value,
        quantity: parseInt(document.getElementById('quantity').value),
        unit: document.getElementById('unit').value,
        unitPrice: parseFloat(document.getElementById('unitPrice').value),
        supplier: document.getElementById('supplier').value,
        department: document.getElementById('department').value,
        notes: document.getElementById('notes').value
    };
    
    const result = await fetch(`${API_BASE_URL}/inventory/voucher`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${getToken()}`
        },
        body: JSON.stringify(voucherData)
    });
    
    if (result.ok) {
        alert('Voucher created!');
        loadVouchers();
    }
}
```

---

## Mobile Pages Mapping

Mobile pages (`/customer/mobile/`, `/admin/mobile/`) sử dụng **cùng API endpoints** như desktop, chỉ khác giao diện.

**Example: Mobile Booking**

```javascript
// Same API, different UI
const rooms = await getAvailableRooms();
displayMobileRoomCards(rooms); // Mobile-optimized cards
```

---

## Common Patterns

### Authentication Check

Tất cả trang admin/customer (except login/register) cần check auth:

```javascript
// Add to top of each protected page
if (!isAuthenticated()) {
    window.location.href = '/customer/login.html'; // or /admin/login.html
}
```

### Error Handling

```javascript
try {
    const result = await apiCall();
    if (result.success) {
        // Handle success
    } else {
        showErrorToast(result.message);
    }
} catch (error) {
    console.error('API Error:', error);
    showErrorToast('Network error. Please try again.');
}
```

### Loading States

```javascript
async function loadData() {
    showLoading();
    try {
        const data = await fetchData();
        displayData(data);
    } catch (error) {
        showError(error);
    } finally {
        hideLoading();
    }
}
```

---

## File References

**JS Helpers:**
- `/js/api-helpers.js` → Base functions (fetchWithAuth, token management)
- `/js/auth.js` → Authentication functions
- `/js/booking.js` → Booking-related API calls
- `/js/admin-dashboard.js` → Admin/reports API calls

**Include in HTML:**

```html
<!-- Customer pages -->
<script src="/js/api-helpers.js"></script>
<script src="/js/auth.js"></script>
<script src="/js/booking.js"></script>
<script src="/js/mobile-redirect.js"></script>

<!-- Admin pages -->
<script src="/js/api-helpers.js"></script>
<script src="/js/auth.js"></script>
<script src="/js/booking.js"></script>
<script src="/js/admin-dashboard.js"></script>
```

---

**Tất cả mapping được documented ở đây. Happy coding! 🚀**

