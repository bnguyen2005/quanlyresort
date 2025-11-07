/**
 * API Helper Functions for Resort Management System
 * Cung cấp các hàm tiện ích để gọi API và xử lý authentication
 */

// Auto-detect API base URL based on current origin
// If running on ngrok, use ngrok URL; otherwise use localhost
function getApiBaseUrl() {
  // Check if running in browser (not Node.js)
  if (typeof window === 'undefined') {
    return 'http://localhost:5130/api';
  }
  
  const origin = window.location.origin;
  // Check if running on ngrok
  if (origin.includes('ngrok-free.app') || origin.includes('ngrok.io') || origin.includes('ngrok.app')) {
    // Use ngrok URL for API
    return `${origin}/api`;
  } else if (origin.includes('localhost') || origin.includes('127.0.0.1')) {
    // Use localhost for local development
    return 'http://localhost:5130/api';
  } else {
    // For production or other domains, use same origin
    return `${origin}/api`;
  }
}

// Cấu hình API
const API_BASE = getApiBaseUrl();

/**
 * Format currency to Vietnamese format
 * @param {number} amount - Số tiền cần format
 * @returns {string} - Chuỗi tiền tệ đã format
 */
function formatCurrency(amount) {
  if (!amount || amount === 0) return '0đ';
  return new Intl.NumberFormat('vi-VN').format(amount) + 'đ';
}

/**
 * Format date to Vietnamese format
 * @param {string|Date} date - Ngày cần format
 * @returns {string} - Chuỗi ngày đã format
 */
function formatDate(date) {
  if (!date) return '—';
  const d = new Date(date);
  return d.toLocaleDateString('vi-VN');
}

/**
 * Format datetime to Vietnamese format
 * @param {string|Date} datetime - Ngày giờ cần format
 * @returns {string} - Chuỗi ngày giờ đã format
 */
function formatDateTime(datetime) {
  if (!datetime) return '—';
  const d = new Date(datetime);
  return d.toLocaleString('vi-VN');
}

/**
 * Get authentication headers
 * @returns {Object} - Headers object with Authorization
 */
function getAuthHeaders() {
  const token = localStorage.getItem('token');
  console.log('[getAuthHeaders] Token present:', !!token, token ? `Bearer ${token.substring(0, 20)}...` : 'none');
  return token ? { 'Authorization': `Bearer ${token}` } : {};
}

/**
 * Make authenticated API request
 * @param {string} url - API endpoint
 * @param {Object} options - Fetch options
 * @returns {Promise<Response>} - Fetch response
 */
async function apiRequest(url, options = {}) {
  const headers = {
    'Content-Type': 'application/json',
    ...getAuthHeaders(),
    ...options.headers
  };

  console.log('[apiRequest] Making request:', {
    url,
    method: options.method || 'GET',
    hasAuth: !!headers.Authorization,
    headers: Object.keys(headers)
  });

  const response = await fetch(url, {
    ...options,
    headers
  });

  console.log('[apiRequest] Response:', {
    status: response.status,
    statusText: response.statusText,
    ok: response.ok,
    url: response.url
  });

  if (!response.ok) {
    if (response.status === 401) {
      // Token expired or invalid
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      // Check if we're on admin page or customer page
      if (window.location.pathname.includes('/admin/')) {
        window.location.href = '/customer/login.html?redirect=' + encodeURIComponent(window.location.pathname);
      } else {
        window.location.href = '/customer/login.html';
      }
      throw new Error('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
    }
    
    if (response.status === 403) {
      // Forbidden - insufficient permissions
      const errorData = await response.json().catch(() => ({}));
      console.error('[apiRequest] 403 Forbidden:', errorData);
      
      // Check if we're on admin page
      if (window.location.pathname.includes('/admin/')) {
        // Show alert and redirect to login
        alert('Bạn không có quyền truy cập tài nguyên này. Vui lòng đăng nhập với tài khoản phù hợp.');
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = '/customer/login.html?redirect=' + encodeURIComponent(window.location.pathname);
      }
      
      throw new Error(errorData.message || 'Bạn không có quyền truy cập tài nguyên này.');
    }
    
    const errorData = await response.json().catch(() => ({}));
    console.error('[apiRequest] Error response:', errorData);
    throw new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
  }

  return response;
}

/**
 * Get data from API endpoint
 * @param {string} endpoint - API endpoint
 * @returns {Promise<any>} - API response data
 */
async function apiGet(endpoint) {
  const fullUrl = `${API_BASE}${endpoint}`;
  console.log('[apiGet] Calling:', fullUrl);
  const response = await apiRequest(fullUrl);
  const data = await response.json();
  console.log('[apiGet] Response data:', data);
  return data;
}

/**
 * Post data to API endpoint
 * @param {string} endpoint - API endpoint
 * @param {Object} data - Data to send
 * @returns {Promise<any>} - API response data
 */
async function apiPost(endpoint, data) {
  const response = await apiRequest(`${API_BASE}${endpoint}`, {
    method: 'POST',
    body: JSON.stringify(data)
  });
  return await response.json();
}

/**
 * Put data to API endpoint
 * @param {string} endpoint - API endpoint
 * @param {Object} data - Data to send
 * @returns {Promise<any>} - API response data
 */
async function apiPut(endpoint, data) {
  const response = await apiRequest(`${API_BASE}${endpoint}`, {
    method: 'PUT',
    body: JSON.stringify(data)
  });
  return await response.json();
}

/**
 * Patch data to API endpoint
 * @param {string} endpoint - API endpoint
 * @param {Object} data - Data to send
 * @returns {Promise<any>} - API response data
 */
async function apiPatch(endpoint, data) {
  const response = await apiRequest(`${API_BASE}${endpoint}`, {
    method: 'PATCH',
    body: JSON.stringify(data)
  });
  return await response.json();
}

/**
 * Delete data from API endpoint
 * @param {string} endpoint - API endpoint
 * @returns {Promise<any>} - API response data
 */
async function apiDelete(endpoint) {
  const response = await apiRequest(`${API_BASE}${endpoint}`, {
    method: 'DELETE'
  });
  return await response.json();
}

/**
 * Post FormData to API endpoint (for file uploads)
 * @param {string} endpoint - API endpoint
 * @param {FormData} formData - FormData to send
 * @returns {Promise<any>} - API response data
 */
async function apiPostFormData(endpoint, formData) {
  const token = localStorage.getItem('token');
  const headers = token ? { 'Authorization': `Bearer ${token}` } : {};
  
  // Don't set Content-Type header - browser will set it with boundary for multipart/form-data
  
  const response = await fetch(`${API_BASE}${endpoint}`, {
    method: 'POST',
    headers: headers,
    body: formData
  });
  
  if (!response.ok) {
    if (response.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      if (window.location.pathname.includes('/admin/')) {
        window.location.href = '/customer/login.html?redirect=' + encodeURIComponent(window.location.pathname);
      } else {
        window.location.href = '/customer/login.html';
      }
      throw new Error('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
    }
    
    const errorData = await response.json().catch(() => ({}));
    throw new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
  }
  
  return await response.json();
}

// Make apiPostFormData globally available
window.apiPostFormData = apiPostFormData;

/**
 * Login user
 * @param {string} email - User email
 * @param {string} password - User password
 * @returns {Promise<Object>} - Login response
 */
async function login(email, password) {
  try {
    const response = await fetch(`${API_BASE}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ email, password })
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Đăng nhập thất bại');
    }

    const data = await response.json();
    
    // Store token and user info
    localStorage.setItem('token', data.token);
    localStorage.setItem('user', JSON.stringify(data.user));
    
    return data;
  } catch (error) {
    console.error('Login error:', error);
    throw error;
  }
}

/**
 * Customer login
 * @param {string} email - Customer email
 * @param {string} password - Customer password
 * @returns {Promise<Object>} - Login response
 */
async function customerLogin(email, password) {
  try {
    const response = await fetch(`${API_BASE}/auth/customer-login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ email, password })
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Đăng nhập thất bại');
    }

    const data = await response.json();
    
    // Store token and user info
    localStorage.setItem('token', data.token);
    localStorage.setItem('user', JSON.stringify(data.user));
    
    return data;
  } catch (error) {
    console.error('Customer login error:', error);
    throw error;
  }
}

/**
 * Logout user
 */
function logout() {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  window.location.href = '/customer/login.html';
}

/**
 * Check if user is authenticated
 * @returns {boolean} - True if authenticated
 */
function isAuthenticated() {
  const token = localStorage.getItem('token');
  const user = localStorage.getItem('user');
  return !!(token && user);
}

/**
 * Get current user info
 * @returns {Object|null} - User object or null
 */
function getCurrentUser() {
  const userStr = localStorage.getItem('user');
  return userStr ? JSON.parse(userStr) : null;
}

/**
 * Check if user has required role
 * @param {string|Array<string>} roles - Required role(s)
 * @returns {boolean} - True if user has required role
 */
function hasRole(roles) {
  const user = getCurrentUser();
  if (!user || !user.role) return false;
  
  const requiredRoles = Array.isArray(roles) ? roles : [roles];
  return requiredRoles.includes(user.role);
}

/**
 * Show loading state
 * @param {string} elementId - Element ID to show loading
 * @param {string} message - Loading message
 */
function showLoading(elementId, message = 'Đang tải...') {
  const element = document.getElementById(elementId);
  if (element) {
    element.innerHTML = `<div class="text-center"><div class="spinner-border" role="status"><span class="visually-hidden">${message}</span></div><div class="mt-2">${message}</div></div>`;
  }
}

/**
 * Show error message
 * @param {string} message - Error message
 * @param {string} type - Alert type (danger, warning, info, success)
 */
function showAlert(message, type = 'danger') {
  const alertHtml = `
    <div class="alert alert-${type} alert-dismissible fade show" role="alert">
      ${message}
      <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </div>
  `;
  
  // Try to find alert container
  let container = document.getElementById('alert-container');
  if (!container) {
    container = document.createElement('div');
    container.id = 'alert-container';
    container.className = 'position-fixed top-0 end-0 p-3';
    container.style.zIndex = '9999';
    document.body.appendChild(container);
  }
  
  container.insertAdjacentHTML('beforeend', alertHtml);
  
  // Auto remove after 5 seconds
  setTimeout(() => {
    const alerts = container.querySelectorAll('.alert');
    if (alerts.length > 0) {
      alerts[alerts.length - 1].remove();
    }
  }, 5000);
}

/**
 * Toast (Bootstrap) nhất quán toàn hệ thống
 */
function ensureToastContainer() {
  let container = document.getElementById('toastContainer');
  if (!container) {
    container = document.createElement('div');
    container.id = 'toastContainer';
    container.className = 'position-fixed top-0 end-0 p-3';
    container.style.zIndex = '1080';
    container.setAttribute('aria-live', 'polite');
    container.setAttribute('aria-atomic', 'true');
    document.body.appendChild(container);
  }
  return container;
}

function showToast(message, type = 'success', delay = 2500) {
  const container = ensureToastContainer();
  const el = document.createElement('div');
  const bg = type === 'success' ? 'success' : type === 'warning' ? 'warning' : 'danger';
  el.className = `toast align-items-center text-bg-${bg} border-0`;
  el.setAttribute('role', 'status');
  el.setAttribute('aria-live', 'polite');
  el.setAttribute('aria-atomic', 'true');
  el.innerHTML = `
    <div class="d-flex">
      <div class="toast-body">${message}</div>
      <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
    </div>`;
  container.appendChild(el);
  try {
    const toast = new bootstrap.Toast(el, { delay });
    toast.show();
    el.addEventListener('hidden.bs.toast', () => el.remove());
  } catch(_) {
    // Fallback nếu Bootstrap chưa sẵn sàng
    setTimeout(() => el.remove(), delay);
  }
}

/**
 * Loading helpers
 */
function setButtonLoading(buttonEl, loading, loadingText = 'Đang xử lý...') {
  if (!buttonEl) return;
  if (loading) {
    buttonEl.dataset._oldText = buttonEl.textContent;
    buttonEl.disabled = true;
    buttonEl.textContent = loadingText;
  } else {
    buttonEl.disabled = false;
    if (buttonEl.dataset._oldText) buttonEl.textContent = buttonEl.dataset._oldText;
  }
}

function showPageLoading(message = 'Đang tải...') {
  let overlay = document.getElementById('globalLoadingOverlay');
  if (!overlay) {
    overlay = document.createElement('div');
    overlay.id = 'globalLoadingOverlay';
    overlay.style.position = 'fixed';
    overlay.style.inset = '0';
    overlay.style.background = 'rgba(255,255,255,0.6)';
    overlay.style.zIndex = '1070';
    overlay.style.display = 'flex';
    overlay.style.alignItems = 'center';
    overlay.style.justifyContent = 'center';
    overlay.innerHTML = `<div class="text-center">
        <div class="spinner-border" role="status"></div>
        <div class="mt-2">${message}</div>
      </div>`;
    document.body.appendChild(overlay);
  } else {
    overlay.style.display = 'flex';
  }
}

function hidePageLoading() {
  const overlay = document.getElementById('globalLoadingOverlay');
  if (overlay) overlay.style.display = 'none';
}

/**
 * Modal xác nhận đẹp (Bootstrap) - trả về Promise<boolean>
 */
function showConfirm(title = 'Xác nhận', message = 'Bạn có chắc?', confirmText = 'Xác nhận', cancelText = 'Hủy') {
  return new Promise((resolve) => {
    let modal = document.getElementById('globalConfirmModal');
    if (!modal) {
      const wrapper = document.createElement('div');
      wrapper.innerHTML = `
        <div class="modal fade" id="globalConfirmModal" tabindex="-1" aria-hidden="true">
          <div class="modal-dialog">
            <div class="modal-content">
              <div class="modal-header">
                <h5 class="modal-title">${title}</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
              </div>
              <div class="modal-body"><div id="globalConfirmMessage">${message}</div></div>
              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" id="globalConfirmCancelBtn">${cancelText}</button>
                <button type="button" class="btn btn-primary" id="globalConfirmOkBtn">${confirmText}</button>
              </div>
            </div>
          </div>
        </div>`;
      document.body.appendChild(wrapper);
    } else {
      modal.querySelector('.modal-title').textContent = title;
      const msg = document.getElementById('globalConfirmMessage');
      if (msg) msg.textContent = message;
      const ok = document.getElementById('globalConfirmOkBtn');
      if (ok) ok.textContent = confirmText;
      const cancel = document.getElementById('globalConfirmCancelBtn');
      if (cancel) cancel.textContent = cancelText;
    }

    modal = document.getElementById('globalConfirmModal');
    const bs = new bootstrap.Modal(modal);
    bs.show();

    const cleanup = () => {
      okBtn?.removeEventListener('click', onOk);
      cancelBtn?.removeEventListener('click', onCancel);
      modal.removeEventListener('hidden.bs.modal', onCancel);
    };

    const okBtn = document.getElementById('globalConfirmOkBtn');
    const cancelBtn = document.getElementById('globalConfirmCancelBtn');

    const onOk = () => { cleanup(); bs.hide(); resolve(true); };
    const onCancel = () => { cleanup(); resolve(false); };

    okBtn?.addEventListener('click', onOk);
    cancelBtn?.addEventListener('click', onCancel);
    modal.addEventListener('hidden.bs.modal', onCancel, { once: true });
  });
}

/**
 * Validate email format
 * @param {string} email - Email to validate
 * @returns {boolean} - True if valid email
 */
function isValidEmail(email) {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}

/**
 * Validate phone number (Vietnamese format)
 * @param {string} phone - Phone to validate
 * @returns {boolean} - True if valid phone
 */
function isValidPhone(phone) {
  const phoneRegex = /^(\+84|84|0)[1-9][0-9]{8,9}$/;
  return phoneRegex.test(phone.replace(/\s/g, ''));
}

/**
 * Debounce function
 * @param {Function} func - Function to debounce
 * @param {number} wait - Wait time in ms
 * @returns {Function} - Debounced function
 */
function debounce(func, wait) {
  let timeout;
  return function executedFunction(...args) {
    const later = () => {
      clearTimeout(timeout);
      func(...args);
    };
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
  };
}

/**
 * Throttle function
 * @param {Function} func - Function to throttle
 * @param {number} limit - Time limit in ms
 * @returns {Function} - Throttled function
 */
function throttle(func, limit) {
  let inThrottle;
  return function(...args) {
    if (!inThrottle) {
      func.apply(this, args);
      inThrottle = true;
      setTimeout(() => inThrottle = false, limit);
    }
  };
}

// Export functions for use in other scripts
window.apiHelpers = {
  formatCurrency,
  formatDate,
  formatDateTime,
  getAuthHeaders,
  apiRequest,
  apiGet,
  apiPost,
  apiPut,
  apiPatch,
  apiDelete,
  apiPostFormData,
  login,
  customerLogin,
  logout,
  isAuthenticated,
  getCurrentUser,
  hasRole,
  showLoading,
  showAlert,
  showToast,
  setButtonLoading,
  showPageLoading,
  hidePageLoading,
  showConfirm,
  isValidEmail,
  isValidPhone,
  debounce,
  throttle
};
