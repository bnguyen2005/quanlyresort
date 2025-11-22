/**
 * API Helper Functions for Resort Management System
 * Hàm hỗ trợ gọi API cho hệ thống quản lý resort
 */

// Auto-detect API base URL (production or development)
const API_BASE_URL = (typeof window !== 'undefined' && window.location) 
  ? `${window.location.origin}/api`
  : 'http://localhost:5130/api';

/**
 * Lấy token từ localStorage
 */
const getToken = () => {
  return localStorage.getItem('token');
};

/**
 * Lấy thông tin user từ localStorage
 */
const getUser = () => {
  const userStr = localStorage.getItem('user');
  return userStr ? JSON.parse(userStr) : null;
};

/**
 * Hàm gọi API chung
 */
const apiCall = async (endpoint, options = {}) => {
  const token = getToken();
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  try {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      ...options,
      headers
    });

    // Helpful debug log for developer
    console.debug('[apiCall] ', options.method || 'GET', `${API_BASE_URL}${endpoint}`, 'status=', response.status);

    if (response.status === 401) {
      // Token hết hạn hoặc không hợp lệ
      console.warn('[apiCall] 401 Unauthorized - clearing token and redirecting to login');
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/customer/login.html';
      return null;
    }

    // Try to parse JSON, but handle non-JSON responses gracefully
    // Clone response để có thể đọc nhiều lần nếu cần
    let data = null;
    try {
      // Clone response để có thể đọc lại nếu cần
      const clonedResponse = response.clone();
      data = await response.json();
    } catch (parseErr) {
      // Nếu JSON parse fail, thử đọc text từ cloned response
      try {
        const clonedResponse = response.clone();
        const text = await clonedResponse.text();
        console.warn('[apiCall] Response not JSON:', text.slice(0, 200));
        data = { message: text };
      } catch (textErr) {
        // Nếu không clone được, thử đọc từ response gốc (có thể đã bị consume)
        console.warn('[apiCall] Could not read response text:', textErr);
        data = { message: `HTTP ${response.status}: ${response.statusText}` };
      }
    }

    if (!response.ok) {
      console.error('[apiCall] API error', response.status, data);
      throw new Error(data.message || `HTTP ${response.status}`);
    }

    return data;
  } catch (error) {
    console.error('API Error:', error);
    throw error;
  }
};

// ==================== AUTH API ====================

/**
 * Đăng nhập admin (universal - works for both admin and customer)
 */
const adminLogin = async (username, password) => {
  try {
    // Try admin login first
    try {
      const adminData = await apiCall('/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email: username, password: password, role: 'Admin' })
      });

      if (adminData && adminData.token) {
        localStorage.setItem('token', adminData.token);
        localStorage.setItem('user', JSON.stringify(adminData.user));
        
        // Dispatch custom event để update navbar ngay lập tức
        window.dispatchEvent(new CustomEvent('userLoggedIn', { detail: adminData.user }));
        
        return adminData;
      }
    } catch (adminError) {
      // If admin login fails, try customer login
      const customerData = await apiCall('/auth/customer-login', {
        method: 'POST',
        body: JSON.stringify({ email: username, password: password })
      });

      if (customerData && customerData.token) {
        localStorage.setItem('token', customerData.token);
        localStorage.setItem('user', JSON.stringify(customerData.user));
        
        // Dispatch custom event để update navbar ngay lập tức
        window.dispatchEvent(new CustomEvent('userLoggedIn', { detail: customerData.user }));
        
        return customerData;
      }
    }
    
    throw new Error('Đăng nhập thất bại');
  } catch (error) {
    throw error;
  }
};

/**
 * Đăng xuất
 */
const logout = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  window.location.href = '/customer/login.html';
};

// ==================== ROOMS API ====================

/**
 * Lấy tất cả phòng
 */
const normalizeRoom = (r) => {
  if (!r) return r;
  return {
    roomId: r.roomId ?? r.RoomId ?? r.id ?? null,
    roomNumber: r.roomNumber ?? r.RoomNumber ?? '',
    roomType: r.roomType ?? r.RoomType ?? '',
    roomTypeId: r.roomTypeId ?? r.RoomTypeId ?? null,
    roomTypeName: r.roomTypeName ?? r.RoomTypeName ?? null,
    floor: r.floor ?? r.Floor ?? '',
    pricePerNight: r.pricePerNight ?? r.PricePerNight ?? r.price ?? null,
    maxOccupancy: r.maxOccupancy ?? r.MaxOccupancy ?? 0,
    description: r.description ?? r.Description ?? '',
    amenities: r.amenities ?? r.Amenities ?? null,
    isAvailable: (r.isAvailable ?? r.IsAvailable) === undefined ? null : (r.isAvailable ?? r.IsAvailable),
    housekeepingStatus: r.housekeepingStatus ?? r.HousekeepingStatus ?? null,
    notes: r.notes ?? r.Notes ?? null,
    createdAt: r.createdAt ?? r.CreatedAt ?? null,
    updatedAt: r.updatedAt ?? r.UpdatedAt ?? null,
    _raw: r
  };
};

const getAllRooms = async () => {
  const data = await apiCall('/rooms');
  if (!data) return [];
  if (Array.isArray(data)) return data.map(normalizeRoom);
  if (data.items && Array.isArray(data.items)) return data.items.map(normalizeRoom);
  return [];
};

/**
 * Lấy chi tiết một phòng
 */
const getRoomById = async (id) => {
  const data = await apiCall(`/rooms/${id}`);
  return normalizeRoom(data);
};

/**
 * Tạo phòng mới
 */
const createRoom = async (roomData) => {
  // derive availability and housekeeping status from roomData.status if provided
  let isAvailable = roomData.isAvailable ?? true;
  let housekeeping = roomData.housekeepingStatus ?? 'Ready';
  if (roomData.status) {
    const s = roomData.status.toString().toLowerCase();
    if (s === 'available' || s === 'phòng trống' || s === 'trống') {
      isAvailable = true; housekeeping = 'Ready';
    } else if (s === 'occupied' || s === 'đang dùng') {
      isAvailable = false; housekeeping = 'Clean';
    } else if (s === 'maintenance' || s === 'bảo trì') {
      isAvailable = false; housekeeping = 'Maintenance';
    } else if (s === 'unavailable') {
      isAvailable = false; housekeeping = 'Maintenance';
    }
  }

  const payload = {
    RoomNumber: roomData.roomNumber,
    RoomType: roomData.roomType,
    RoomTypeId: roomData.roomTypeId ?? null,
    Floor: roomData.floor?.toString() ?? null,
    PricePerNight: roomData.pricePerNight,
    MaxOccupancy: roomData.maxOccupancy,
    Description: roomData.description,
    IsAvailable: isAvailable,
    HousekeepingStatus: housekeeping
  };
  const res = await apiCall('/rooms', {
    method: 'POST',
    body: JSON.stringify(payload)
  });
  return normalizeRoom(res);
};

/**
 * Cập nhật phòng
 */
const updateRoom = async (id, roomData) => {
  // map status to IsAvailable/HousekeepingStatus if provided
  let isAvailable = roomData.isAvailable ?? true;
  let housekeeping = roomData.housekeepingStatus ?? 'Ready';
  if (roomData.status) {
    const s = roomData.status.toString().toLowerCase();
    if (s === 'available' || s === 'phòng trống' || s === 'trống') {
      isAvailable = true; housekeeping = 'Ready';
    } else if (s === 'occupied' || s === 'đang dùng') {
      isAvailable = false; housekeeping = 'Clean';
    } else if (s === 'maintenance' || s === 'bảo trì') {
      isAvailable = false; housekeeping = 'Maintenance';
    } else if (s === 'unavailable') {
      isAvailable = false; housekeeping = 'Maintenance';
    }
  }

  const payload = {
    RoomId: id,
    RoomNumber: roomData.roomNumber,
    RoomType: roomData.roomType,
    RoomTypeId: roomData.roomTypeId ?? null,
    Floor: roomData.floor?.toString() ?? null,
    PricePerNight: roomData.pricePerNight,
    MaxOccupancy: roomData.maxOccupancy,
    Description: roomData.description,
    IsAvailable: isAvailable,
    HousekeepingStatus: housekeeping
  };
  const res = await apiCall(`/rooms/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload)
  });
  return normalizeRoom(res?.room ?? res);
};

/**
 * Xóa phòng
 */
const deleteRoom = async (id) => {
  return await apiCall(`/rooms/${id}`, {
    method: 'DELETE'
  });
};

/**
 * Lấy phòng available
 */
const getAvailableRooms = async (roomType = null) => {
  const endpoint = roomType ? `/rooms/available?roomType=${roomType}` : '/rooms/available';
  return await apiCall(endpoint);
};

// ==================== BOOKINGS API ====================

/**
 * Lấy tất cả đặt phòng
 */
const getAllBookings = async () => {
  return await apiCall('/bookings');
};

/**
 * Lấy chi tiết đặt phòng
 */
const getBookingById = async (id) => {
  return await apiCall(`/bookings/${id}`);
};

/**
 * Tạo đặt phòng mới
 */
const createBooking = async (bookingData) => {
  return await apiCall('/bookings', {
    method: 'POST',
    body: JSON.stringify(bookingData)
  });
};

/**
 * Cập nhật đặt phòng
 */
const updateBooking = async (id, bookingData) => {
  return await apiCall(`/bookings/${id}`, {
    method: 'PUT',
    body: JSON.stringify(bookingData)
  });
};

/**
 * Hủy đặt phòng
 */
const cancelBooking = async (id) => {
  return await apiCall(`/bookings/${id}/cancel`, {
    method: 'POST'
  });
};

/**
 * Xác nhận đặt phòng
 */
const confirmBooking = async (id, roomId) => {
  return await apiCall(`/bookings/${id}/confirm`, {
    method: 'POST',
    body: JSON.stringify({ roomId })
  });
};

/**
 * Check-in
 */
const checkIn = async (id) => {
  return await apiCall(`/bookings/${id}/check-in`, {
    method: 'POST'
  });
};

/**
 * Check-out
 */
const checkOut = async (id) => {
  return await apiCall(`/bookings/${id}/check-out`, {
    method: 'POST'
  });
};

// ==================== INVOICES API ====================

/**
 * Lấy tất cả hóa đơn
 */
const getAllInvoices = async () => {
  return await apiCall('/invoices');
};

/**
 * Lấy chi tiết hóa đơn
 */
const getInvoiceById = async (id) => {
  return await apiCall(`/invoices/${id}`);
};

/**
 * Thanh toán hóa đơn
 */
const payInvoice = async (id, paymentData) => {
  return await apiCall(`/invoices/${id}/pay`, {
    method: 'POST',
    body: JSON.stringify(paymentData)
  });
};

// ==================== INVENTORY API ====================

/**
 * Lấy tất cả items trong kho
 */
const getAllInventoryItems = async () => {
  return await apiCall('/inventory');
};

/**
 * Lấy items sắp hết
 */
const getLowStockItems = async () => {
  return await apiCall('/inventory/low-stock');
};

/**
 * Cập nhật inventory item
 */
const updateInventoryItem = async (id, itemData) => {
  return await apiCall(`/inventory/${id}`, {
    method: 'PUT',
    body: JSON.stringify(itemData)
  });
};

/**
 * Sử dụng item
 */
const useInventoryItem = async (id, quantity, roomId = null) => {
  return await apiCall(`/inventory/${id}/use`, {
    method: 'POST',
    body: JSON.stringify({ quantity, roomId })
  });
};

/**
 * Restock item
 */
const restockInventoryItem = async (id, quantity) => {
  return await apiCall(`/inventory/${id}/restock`, {
    method: 'POST',
    body: JSON.stringify({ quantity })
  });
};

// ==================== REPORTS API ====================

/**
 * Lấy báo cáo doanh thu
 */
const getRevenueReport = async (startDate, endDate) => {
  return await apiCall(`/reports/revenue?startDate=${startDate}&endDate=${endDate}`);
};

/**
 * Lấy báo cáo tỷ lệ lấp đầy
 */
const getOccupancyReport = async (startDate, endDate) => {
  return await apiCall(`/reports/occupancy?startDate=${startDate}&endDate=${endDate}`);
};

/**
 * Lấy báo cáo sử dụng kho
 */
const getInventoryUsageReport = async (startDate, endDate) => {
  return await apiCall(`/reports/inventory-usage?startDate=${startDate}&endDate=${endDate}`);
};

// ==================== ALERTS API ====================

/**
 * Lấy tất cả alerts
 */
const getAllAlerts = async () => {
  return await apiCall('/alerts');
};

/**
 * Lấy alerts chưa đọc
 */
const getUnreadAlerts = async () => {
  return await apiCall('/alerts/unread');
};

/**
 * Đánh dấu alert đã đọc
 */
const markAlertAsRead = async (id) => {
  return await apiCall(`/alerts/${id}/read`, {
    method: 'POST'
  });
};

// ==================== AUDIT API ====================

/**
 * Lấy audit logs
 */
const getAuditLogs = async (page = 1, pageSize = 50) => {
  return await apiCall(`/audit?page=${page}&pageSize=${pageSize}`);
};

// ==================== ADMIN API ====================

/**
 * Lấy dashboard stats
 */
const getDashboardStats = async () => {
  return await apiCall('/admin/dashboard-stats');
};

// ==================== ADMIN API ====================

/**
 * Admin API helper - wrapper for apiCall với error handling tốt hơn
 */
const adminAPI = async (endpoint, method = 'GET', data = null) => {
  const options = {
    method: method
  };

  if (data) {
    options.body = JSON.stringify(data);
  }

  try {
    const response = await apiCall(endpoint, options);
    return response;
  } catch (error) {
    console.error(`[adminAPI] Error ${method} ${endpoint}:`, error);
    throw error;
  }
};

// Make adminAPI globally available
window.adminAPI = adminAPI;

// ==================== UTILITY FUNCTIONS ====================

/**
 * Format tiền VNĐ
 */
const formatCurrency = (amount) => {
  return new Intl.NumberFormat('vi-VN', { 
    style: 'currency', 
    currency: 'VND' 
  }).format(amount);
};

/**
 * Format ngày
 */
const formatDate = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return new Intl.DateTimeFormat('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  }).format(date);
};

/**
 * Format ngày giờ
 */
const formatDateTime = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return new Intl.DateTimeFormat('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(date);
};

/**
 * Lấy badge class theo trạng thái
 */
const getStatusBadgeClass = (status) => {
  const statusMap = {
    // Booking statuses
    'Pending': 'bg-label-warning',
    'Confirmed': 'bg-label-info',
    'CheckedIn': 'bg-label-primary',
    'CheckedOut': 'bg-label-success',
    'Cancelled': 'bg-label-danger',
    // Room statuses
    'Available': 'bg-label-success',
    'Occupied': 'bg-label-primary',
    'Maintenance': 'bg-label-warning',
    'Unavailable': 'bg-label-danger',
    // Invoice statuses
    'Unpaid': 'bg-label-warning',
    'Paid': 'bg-label-success',
    'Overdue': 'bg-label-danger'
  };
  return statusMap[status] || 'bg-label-secondary';
};

/**
 * Lấy text hiển thị theo trạng thái
 */
const getStatusText = (status) => {
  const statusTextMap = {
    // Booking statuses
    'Pending': 'Chờ xác nhận',
    'Confirmed': 'Đã xác nhận',
    'CheckedIn': 'Đã nhận phòng',
    'CheckedOut': 'Đã trả phòng',
    'Cancelled': 'Đã hủy',
    // Room statuses
    'Available': 'Trống',
    'Occupied': 'Đang sử dụng',
    'Maintenance': 'Bảo trì',
    'Unavailable': 'Không khả dụng',
    // Invoice statuses
    'Unpaid': 'Chưa thanh toán',
    'Paid': 'Đã thanh toán',
    'Overdue': 'Quá hạn'
  };
  return statusTextMap[status] || status;
};

/**
 * Hiển thị thông báo lỗi (modal nếu có, fallback alert)
 */
const showError = (message, title = 'Lỗi') => {
  try {
    if (window.showAppError) {
      window.showAppError(title, (message && message.message) ? message.message : (message || 'Đã có lỗi xảy ra'));
      return;
    }
  } catch (e) {
    console.warn('showError modal failed', e);
  }
  alert('Lỗi: ' + (message && message.message ? message.message : (message || 'Đã có lỗi xảy ra')));
};

/**
 * Hiển thị thông báo thành công (modal nếu có, fallback alert)
 */
const showSuccess = (message, title = 'Thành công') => {
  try {
    if (window.showAppSuccess) {
      window.showAppSuccess(title, message || 'Hoàn tất');
      return;
    }
  } catch (e) {
    console.warn('showSuccess modal failed', e);
  }
  alert(title + ': ' + (message || 'Hoàn tất'));
};

/**
 * Kiểm tra đã đăng nhập chưa
 */
const checkAuth = () => {
  const token = getToken();
  if (!token) {
    window.location.href = '/customer/login.html';
    return false;
  }
  return true;
};

// ==================== API HELPER FUNCTIONS ====================

/**
 * GET request helper
 */
const apiGet = async (endpoint) => {
  return await apiCall(endpoint, { method: 'GET' });
};

/**
 * POST request helper
 */
const apiPost = async (endpoint, data) => {
  return await apiCall(endpoint, {
    method: 'POST',
    body: JSON.stringify(data)
  });
};

/**
 * PUT request helper
 */
const apiPut = async (endpoint, data) => {
  return await apiCall(endpoint, {
    method: 'PUT',
    body: JSON.stringify(data)
  });
};

/**
 * DELETE request helper
 */
const apiDelete = async (endpoint) => {
  return await apiCall(endpoint, { method: 'DELETE' });
};

