/**
 * Customer API Helper
 * API functions for customer portal
 */

// Auto-detect API base URL from current origin
const API_BASE_URL = `${window.location.origin}/api`;

/**
 * API Call wrapper
 */
const apiCall = async (endpoint, options = {}) => {
  const token = localStorage.getItem('token');
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

    if (response.status === 401) {
      // Token expired
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/customer/login.html';
      return null;
    }

    const data = await response.json();
    
    if (!response.ok) {
      throw new Error(data.message || 'Có lỗi xảy ra');
    }

    return data;
  } catch (error) {
    console.error('API Error:', error);
    throw error;
  }
};

/**
 * Get user from localStorage
 */
const getUser = () => {
  const userStr = localStorage.getItem('user');
  return userStr ? JSON.parse(userStr) : null;
};

/**
 * Get token from localStorage
 */
const getToken = () => {
  return localStorage.getItem('token');
};

/**
 * Check if user is logged in
 */
const isLoggedIn = () => {
  return !!getToken();
};

/**
 * Customer Login
 */
const customerLogin = async (email, password) => {
  try {
    const data = await apiCall('/auth/customer-login', {
      method: 'POST',
      body: JSON.stringify({ email, password })
    });

    if (data && data.token) {
      localStorage.setItem('token', data.token);
      localStorage.setItem('user', JSON.stringify(data.user));
      
      // Dispatch custom event để update navbar ngay lập tức
      window.dispatchEvent(new CustomEvent('userLoggedIn', { detail: data.user }));
      
      return data;
    }
    throw new Error('Đăng nhập thất bại');
  } catch (error) {
    throw error;
  }
};

/**
 * Universal Login (checks role and redirects accordingly)
 */
const universalLogin = async (email, password) => {
  console.log('[universalLogin] Attempting login for:', email);
  
  // Try customer login first
  try {
    console.log('[universalLogin] Trying customer login...');
    const data = await apiCall('/auth/customer-login', {
      method: 'POST',
      body: JSON.stringify({ email, password })
    });

    if (data && data.token) {
      console.log('[universalLogin] Customer login successful!');
      localStorage.setItem('token', data.token);
      localStorage.setItem('user', JSON.stringify(data.user));
      
      // Dispatch custom event để update navbar ngay lập tức
      window.dispatchEvent(new CustomEvent('userLoggedIn', { detail: data.user }));
      
      return data;
    }
  } catch (customerError) {
    console.log('[universalLogin] Customer login failed, trying admin login...');
    console.log('[universalLogin] Customer error:', customerError.message);
  }
  
  // If customer login fails, try admin login
  try {
    console.log('[universalLogin] Calling admin login API...');
    const adminData = await apiCall('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password, role: 'Admin' })
    });

    if (adminData && adminData.token) {
      console.log('[universalLogin] Admin login successful!');
      localStorage.setItem('token', adminData.token);
      localStorage.setItem('user', JSON.stringify(adminData.user));
      
      // Dispatch custom event để update navbar ngay lập tức
      window.dispatchEvent(new CustomEvent('userLoggedIn', { detail: adminData.user }));
      
      return adminData;
    }
    
    console.error('[universalLogin] Admin login returned data but no token');
    throw new Error('Đăng nhập thất bại - không nhận được token');
  } catch (adminError) {
    console.error('[universalLogin] Admin login error:', adminError);
    throw new Error('Email hoặc mật khẩu không đúng');
  }
};

/**
 * Customer Register
 */
const customerRegister = async (registerData) => {
  try {
    const data = await apiCall('/auth/register-customer', {
      method: 'POST',
      body: JSON.stringify(registerData)
    });

    return data;
  } catch (error) {
    throw error;
  }
};

/**
 * Logout
 */
const logout = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  window.location.href = 'index.html';
};

/**
 * Get Available Rooms
 */
const getAvailableRooms = async (roomType = null) => {
  const endpoint = roomType ? `/rooms/available?roomType=${roomType}` : '/rooms/available';
  return await apiCall(endpoint);
};

/**
 * Create Booking
 */
const createBooking = async (bookingData) => {
  return await apiCall('/bookings', {
    method: 'POST',
    body: JSON.stringify(bookingData)
  });
};

/**
 * Get Customer Bookings
 */
const getCustomerBookings = async () => {
  return await apiCall('/bookings/customer');
};

/**
 * Format currency VND
 */
const formatCurrency = (amount) => {
  return new Intl.NumberFormat('vi-VN', { 
    style: 'currency', 
    currency: 'VND' 
  }).format(amount);
};

/**
 * Format date
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
 * Show error
 */
const showError = (message) => {
  alert('Lỗi: ' + message);
};

/**
 * Show success
 */
const showSuccess = (message) => {
  alert('Thành công: ' + message);
};

