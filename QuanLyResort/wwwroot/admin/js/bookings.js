/**
 * Bookings Management JavaScript
 * Xử lý logic cho trang quản lý đặt phòng
 */

// Kiểm tra đăng nhập
if (!checkAuth()) {
  throw new Error('Not authenticated');
}

let allBookings = [];

// Load bookings khi trang được tải
document.addEventListener('DOMContentLoaded', async () => {
  try {
    // Hiển thị tên user
    const user = getUser();
    if (user && user.fullName) {
      document.getElementById('userName').textContent = user.fullName;
    }

    // Load danh sách đặt phòng
    await loadBookings();

    // Setup event listeners
    setupEventListeners();

    // Setup logout
    document.getElementById('logoutBtn').addEventListener('click', (e) => {
      e.preventDefault();
      if (confirm('Bạn có chắc muốn đăng xuất?')) {
        logout();
      }
    });

  } catch (error) {
    console.error('Error initializing bookings page:', error);
    showError('Không thể tải dữ liệu đặt phòng');
  }
});

/**
 * Setup event listeners
 */
const setupEventListeners = () => {
  // Filter listeners
  document.getElementById('statusFilter').addEventListener('change', filterBookings);
  document.getElementById('dateFilter').addEventListener('change', filterBookings);
  
  // Search listener
  document.getElementById('searchInput').addEventListener('input', filterBookings);

  // Confirm booking form
  document.getElementById('confirmBookingForm').addEventListener('submit', handleConfirmBookingSubmit);
};

/**
 * Load tất cả đặt phòng từ API
 */
const loadBookings = async () => {
  try {
    const bookings = await getAllBookings();
    
    if (bookings && Array.isArray(bookings)) {
      allBookings = bookings;
      displayBookings(allBookings);
    } else {
      allBookings = [];
      displayBookings([]);
    }
  } catch (error) {
    console.error('Error loading bookings:', error);
    const tbody = document.getElementById('bookingsTableBody');
    tbody.innerHTML = '<tr><td colspan="7" class="text-center text-danger">Không thể tải dữ liệu đặt phòng</td></tr>';
  }
};

/**
 * Hiển thị danh sách đặt phòng
 */
const displayBookings = (bookings) => {
  const tbody = document.getElementById('bookingsTableBody');
  
  if (!bookings || bookings.length === 0) {
    tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">Không có đặt phòng nào</td></tr>';
    return;
  }

  // Sort by created date descending
  const sortedBookings = bookings.sort((a, b) => 
    new Date(b.createdAt || b.checkInDate) - new Date(a.createdAt || a.checkInDate)
  );

  const rows = sortedBookings.map(booking => {
    const statusClass = getStatusBadgeClass(booking.status);
    const statusText = getStatusText(booking.status);
    
    return `
      <tr>
        <td><strong>#${booking.bookingId}</strong></td>
        <td>${booking.customerName || 'N/A'}</td>
        <td>${booking.roomNumber || booking.requestedRoomType || 'Chưa phân phòng'}</td>
        <td>${formatDate(booking.checkInDate)}</td>
        <td>${formatDate(booking.checkOutDate)}</td>
        <td><span class="badge ${statusClass}">${statusText}</span></td>
        <td>
          <div class="dropdown">
            <button type="button" class="btn p-0 dropdown-toggle hide-arrow" data-bs-toggle="dropdown">
              <i class="bx bx-dots-vertical-rounded"></i>
            </button>
            <div class="dropdown-menu">
              <a class="dropdown-item" href="javascript:void(0);" onclick="handleViewBooking(${booking.bookingId})">
                <i class="bx bx-show me-1"></i> Xem Chi Tiết
              </a>
              ${booking.status === 'Pending' ? `
                <a class="dropdown-item" href="javascript:void(0);" onclick="handleShowConfirmBooking(${booking.bookingId})">
                  <i class="bx bx-check me-1"></i> Xác Nhận
                </a>
              ` : ''}
              ${booking.status === 'Confirmed' ? `
                <a class="dropdown-item" href="javascript:void(0);" onclick="handleCheckIn(${booking.bookingId})">
                  <i class="bx bx-log-in me-1"></i> Check-in
                </a>
              ` : ''}
              ${booking.status === 'CheckedIn' ? `
                <a class="dropdown-item" href="javascript:void(0);" onclick="handleCheckOut(${booking.bookingId})">
                  <i class="bx bx-log-out me-1"></i> Check-out
                </a>
              ` : ''}
              ${['Pending', 'Confirmed'].includes(booking.status) ? `
                <a class="dropdown-item text-danger" href="javascript:void(0);" onclick="handleCancelBooking(${booking.bookingId})">
                  <i class="bx bx-x me-1"></i> Hủy Đặt Phòng
                </a>
              ` : ''}
            </div>
          </div>
        </td>
      </tr>
    `;
  }).join('');

  tbody.innerHTML = rows;
};

/**
 * Filter đặt phòng theo status, date và search
 */
const filterBookings = () => {
  const statusFilter = document.getElementById('statusFilter').value;
  const dateFilter = document.getElementById('dateFilter').value;
  const searchQuery = document.getElementById('searchInput').value.toLowerCase();

  let filteredBookings = allBookings;

  // Filter by status
  if (statusFilter) {
    filteredBookings = filteredBookings.filter(booking => booking.status === statusFilter);
  }

  // Filter by date
  if (dateFilter) {
    filteredBookings = filteredBookings.filter(booking => {
      const checkInDate = new Date(booking.checkInDate).toISOString().split('T')[0];
      return checkInDate === dateFilter;
    });
  }

  // Filter by search query
  if (searchQuery) {
    filteredBookings = filteredBookings.filter(booking => 
      booking.bookingId.toString().includes(searchQuery) ||
      (booking.customerName && booking.customerName.toLowerCase().includes(searchQuery)) ||
      (booking.roomNumber && booking.roomNumber.toLowerCase().includes(searchQuery)) ||
      (booking.requestedRoomType && booking.requestedRoomType.toLowerCase().includes(searchQuery))
    );
  }

  displayBookings(filteredBookings);
};

/**
 * Xử lý xem chi tiết đặt phòng
 */
const handleViewBooking = async (bookingId) => {
  try {
    const booking = await getBookingById(bookingId);
    
    if (booking) {
      const content = `
        <div class="row">
          <div class="col-md-6 mb-3">
            <strong>Mã Đặt Phòng:</strong> #${booking.bookingId}
          </div>
          <div class="col-md-6 mb-3">
            <strong>Trạng Thái:</strong> 
            <span class="badge ${getStatusBadgeClass(booking.status)}">${getStatusText(booking.status)}</span>
          </div>
        </div>
        <div class="row">
          <div class="col-md-6 mb-3">
            <strong>Khách Hàng:</strong> ${booking.customerName || 'N/A'}
          </div>
          <div class="col-md-6 mb-3">
            <strong>Email:</strong> ${booking.customerEmail || 'N/A'}
          </div>
        </div>
        <div class="row">
          <div class="col-md-6 mb-3">
            <strong>Số Điện Thoại:</strong> ${booking.customerPhone || 'N/A'}
          </div>
          <div class="col-md-6 mb-3">
            <strong>Số Khách:</strong> ${booking.numberOfGuests} người
          </div>
        </div>
        <div class="row">
          <div class="col-md-6 mb-3">
            <strong>Loại Phòng:</strong> ${booking.requestedRoomType}
          </div>
          <div class="col-md-6 mb-3">
            <strong>Số Phòng:</strong> ${booking.roomNumber || 'Chưa phân phòng'}
          </div>
        </div>
        <div class="row">
          <div class="col-md-6 mb-3">
            <strong>Check-in:</strong> ${formatDate(booking.checkInDate)}
          </div>
          <div class="col-md-6 mb-3">
            <strong>Check-out:</strong> ${formatDate(booking.checkOutDate)}
          </div>
        </div>
        <div class="row">
          <div class="col-md-6 mb-3">
            <strong>Tổng Tiền:</strong> ${formatCurrency(booking.totalAmount || 0)}
          </div>
          <div class="col-md-6 mb-3">
            <strong>Ngày Đặt:</strong> ${formatDateTime(booking.createdAt)}
          </div>
        </div>
        ${booking.specialRequests ? `
          <div class="row">
            <div class="col-12 mb-3">
              <strong>Yêu Cầu Đặc Biệt:</strong>
              <p class="mt-2">${booking.specialRequests}</p>
            </div>
          </div>
        ` : ''}
      `;
      
      document.getElementById('bookingDetailContent').innerHTML = content;
      
      // Show modal
      const modal = new bootstrap.Modal(document.getElementById('bookingDetailModal'));
      modal.show();
    }
  } catch (error) {
    console.error('Error viewing booking:', error);
    showError('Không thể tải thông tin đặt phòng');
  }
};

/**
 * Hiển thị modal xác nhận đặt phòng
 */
const handleShowConfirmBooking = async (bookingId) => {
  try {
    const booking = await getBookingById(bookingId);
    
    if (booking) {
      // Load available rooms
      const rooms = await getAvailableRooms(booking.requestedRoomType);
      
      const roomSelect = document.getElementById('assignedRoomId');
      roomSelect.innerHTML = '<option value="">Chọn phòng...</option>';
      
      if (rooms && Array.isArray(rooms) && rooms.length > 0) {
        rooms.forEach(room => {
          roomSelect.innerHTML += `<option value="${room.roomId}">${room.roomNumber} - ${room.roomType} (${formatCurrency(room.pricePerNight)}/đêm)</option>`;
        });
      } else {
        roomSelect.innerHTML = '<option value="">Không có phòng khả dụng</option>';
      }
      
      document.getElementById('confirmBookingId').value = bookingId;
      
      // Show modal
      const modal = new bootstrap.Modal(document.getElementById('confirmBookingModal'));
      modal.show();
    }
  } catch (error) {
    console.error('Error loading confirm booking modal:', error);
    showError('Không thể tải danh sách phòng');
  }
};

/**
 * Xử lý submit form xác nhận đặt phòng
 */
const handleConfirmBookingSubmit = async (e) => {
  e.preventDefault();
  
  try {
    const bookingId = document.getElementById('confirmBookingId').value;
    const roomId = document.getElementById('assignedRoomId').value;
    
    if (!roomId) {
      showError('Vui lòng chọn phòng');
      return;
    }
    
    await confirmBooking(bookingId, roomId);
    showSuccess('Xác nhận đặt phòng thành công!');
    
    // Close modal
    const modal = bootstrap.Modal.getInstance(document.getElementById('confirmBookingModal'));
    modal.hide();
    
    // Reload bookings
    await loadBookings();
    
  } catch (error) {
    console.error('Error confirming booking:', error);
    showError('Không thể xác nhận đặt phòng: ' + error.message);
  }
};

/**
 * Xử lý check-in
 */
const handleCheckIn = async (bookingId) => {
  if (!confirm('Xác nhận khách hàng đã check-in?')) {
    return;
  }
  
  try {
    await checkIn(bookingId);
    showSuccess('Check-in thành công!');
    await loadBookings();
  } catch (error) {
    console.error('Error checking in:', error);
    showError('Không thể check-in: ' + error.message);
  }
};

/**
 * Xử lý check-out
 */
const handleCheckOut = async (bookingId) => {
  if (!confirm('Xác nhận khách hàng đã check-out?')) {
    return;
  }
  
  try {
    await checkOut(bookingId);
    showSuccess('Check-out thành công!');
    await loadBookings();
  } catch (error) {
    console.error('Error checking out:', error);
    showError('Không thể check-out: ' + error.message);
  }
};

/**
 * Xử lý hủy đặt phòng
 */
const handleCancelBooking = async (bookingId) => {
  if (!confirm('Bạn có chắc muốn hủy đặt phòng này?')) {
    return;
  }
  
  try {
    await cancelBooking(bookingId);
    showSuccess('Hủy đặt phòng thành công!');
    await loadBookings();
  } catch (error) {
    console.error('Error cancelling booking:', error);
    showError('Không thể hủy đặt phòng: ' + error.message);
  }
};

