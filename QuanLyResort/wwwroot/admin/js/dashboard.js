/**
 * Dashboard JavaScript
 * Xử lý logic cho trang dashboard chính
 */

// Kiểm tra đăng nhập
if (!checkAuth()) {
  throw new Error('Not authenticated');
}

// Load dashboard data khi trang được tải
document.addEventListener('DOMContentLoaded', async () => {
  try {
    // Hiển thị tên user
    const user = getUser();
    if (user && user.fullName) {
      document.getElementById('userName').textContent = user.fullName;
    }

    // Load dashboard stats
    await loadDashboardStats();
    
    // Load room status
    await loadRoomStatus();
    
    // Load recent bookings
    await loadRecentBookings();
    
    // Load alerts
    await loadAlerts();
    
    // Load low stock items
    await loadLowStock();

    // Setup logout button
    document.getElementById('logoutBtn').addEventListener('click', (e) => {
      e.preventDefault();
      if (confirm('Bạn có chắc muốn đăng xuất?')) {
        logout();
      }
    });

  } catch (error) {
    console.error('Error loading dashboard:', error);
    showError('Không thể tải dữ liệu dashboard');
  }
});

/**
 * Load dashboard statistics
 */
const loadDashboardStats = async () => {
  try {
    const stats = await getDashboardStats();
    
    if (stats) {
      // Update today's bookings
      document.getElementById('todayBookings').textContent = stats.todayBookings || 0;
      
      // Update total revenue
      document.getElementById('totalRevenue').textContent = formatCurrency(stats.monthlyRevenue || 0);
      
      // Update total bookings
      document.getElementById('totalBookings').textContent = stats.monthlyBookings || 0;
    }
  } catch (error) {
    console.error('Error loading dashboard stats:', error);
    // Set default values
    document.getElementById('todayBookings').textContent = '0';
    document.getElementById('totalRevenue').textContent = '0đ';
    document.getElementById('totalBookings').textContent = '0';
  }
};

/**
 * Load room status
 */
const loadRoomStatus = async () => {
  try {
    const rooms = await getAllRooms();
    
    if (rooms && Array.isArray(rooms)) {
      // Count rooms by status
      const statusCount = {
        available: 0,
        occupied: 0,
        maintenance: 0,
        unavailable: 0
      };

      rooms.forEach(room => {
        switch (room.status) {
          case 'Available':
            statusCount.available++;
            break;
          case 'Occupied':
            statusCount.occupied++;
            break;
          case 'Maintenance':
            statusCount.maintenance++;
            break;
          case 'Unavailable':
            statusCount.unavailable++;
            break;
        }
      });

      // Update UI
      document.getElementById('availableRooms').textContent = statusCount.available;
      document.getElementById('occupiedRooms').textContent = statusCount.occupied;
      document.getElementById('maintenanceRooms').textContent = statusCount.maintenance;
      document.getElementById('unavailableRooms').textContent = statusCount.unavailable;
    }
  } catch (error) {
    console.error('Error loading room status:', error);
    document.getElementById('availableRooms').textContent = '0';
    document.getElementById('occupiedRooms').textContent = '0';
    document.getElementById('maintenanceRooms').textContent = '0';
    document.getElementById('unavailableRooms').textContent = '0';
  }
};

/**
 * Load recent bookings
 */
const loadRecentBookings = async () => {
  try {
    const bookings = await getAllBookings();
    const tbody = document.querySelector('#recentBookingsTable tbody');
    
    if (!bookings || !Array.isArray(bookings) || bookings.length === 0) {
      tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Không có đặt phòng nào</td></tr>';
      return;
    }

    // Sort by created date descending and take first 5
    const recentBookings = bookings
      .sort((a, b) => new Date(b.createdAt || b.checkInDate) - new Date(a.createdAt || a.checkInDate))
      .slice(0, 5);

    // Build table rows
    const rows = recentBookings.map(booking => {
      const statusClass = getStatusBadgeClass(booking.status);
      const statusText = getStatusText(booking.status);
      
      return `
        <tr>
          <td><span class="fw-semibold">#${booking.bookingId || booking.id}</span></td>
          <td>${booking.customerName || 'N/A'}</td>
          <td>${booking.roomNumber || booking.requestedRoomType || 'Chưa phân phòng'}</td>
          <td>${formatDate(booking.checkInDate)}</td>
          <td><span class="badge ${statusClass}">${statusText}</span></td>
        </tr>
      `;
    }).join('');

    tbody.innerHTML = rows;

  } catch (error) {
    console.error('Error loading recent bookings:', error);
    const tbody = document.querySelector('#recentBookingsTable tbody');
    tbody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Không thể tải dữ liệu</td></tr>';
  }
};

/**
 * Load system alerts
 */
const loadAlerts = async () => {
  try {
    const alerts = await getUnreadAlerts();
    const alertsList = document.getElementById('alertsList');
    
    if (!alerts || !Array.isArray(alerts) || alerts.length === 0) {
      alertsList.innerHTML = '<li class="text-center text-muted">Không có cảnh báo mới</li>';
      return;
    }

    // Take first 5 alerts
    const recentAlerts = alerts.slice(0, 5);
    
    const alertItems = recentAlerts.map(alert => {
      const severityIcon = {
        'Critical': 'bx-error text-danger',
        'High': 'bx-error-circle text-warning',
        'Medium': 'bx-info-circle text-info',
        'Low': 'bx-info-circle text-secondary'
      };
      
      const icon = severityIcon[alert.severity] || 'bx-info-circle';
      
      return `
        <li class="d-flex align-items-center mb-3">
          <i class='bx ${icon} fs-4 me-2'></i>
          <div class="flex-grow-1">
            <small class="text-muted">${formatDateTime(alert.createdAt)}</small>
            <p class="mb-0">${alert.message}</p>
          </div>
        </li>
      `;
    }).join('');

    alertsList.innerHTML = alertItems;

  } catch (error) {
    console.error('Error loading alerts:', error);
    document.getElementById('alertsList').innerHTML = '<li class="text-center text-danger">Không thể tải dữ liệu</li>';
  }
};

/**
 * Load low stock items
 */
const loadLowStock = async () => {
  try {
    const items = await getLowStockItems();
    const lowStockList = document.getElementById('lowStockList');
    
    if (!items || !Array.isArray(items) || items.length === 0) {
      lowStockList.innerHTML = '<li class="text-center text-success">Kho đầy đủ</li>';
      return;
    }

    // Take first 5 items
    const topItems = items.slice(0, 5);
    
    const itemsHtml = topItems.map(item => {
      const percentage = Math.round((item.currentStock / item.minStockLevel) * 100);
      const progressClass = percentage < 50 ? 'bg-danger' : 'bg-warning';
      
      return `
        <li class="d-flex align-items-center mb-3">
          <div class="flex-grow-1">
            <div class="d-flex justify-content-between align-items-center mb-1">
              <span class="fw-semibold">${item.itemName}</span>
              <span class="text-muted">${item.currentStock}/${item.minStockLevel}</span>
            </div>
            <div class="progress" style="height: 6px;">
              <div class="progress-bar ${progressClass}" role="progressbar" 
                   style="width: ${percentage}%" aria-valuenow="${percentage}" 
                   aria-valuemin="0" aria-valuemax="100"></div>
            </div>
          </div>
        </li>
      `;
    }).join('');

    lowStockList.innerHTML = itemsHtml;

  } catch (error) {
    console.error('Error loading low stock:', error);
    document.getElementById('lowStockList').innerHTML = '<li class="text-center text-danger">Không thể tải dữ liệu</li>';
  }
};

