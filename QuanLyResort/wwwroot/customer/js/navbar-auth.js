/**
 * Navbar Authentication UI Handler
 * Hiển thị email và dropdown menu sau khi đăng nhập
 */

// Thêm CSS cho badge đỏ trong menu (chỉ thêm 1 lần)
if (!document.getElementById('navbar-unpaid-badge-style')) {
  const style = document.createElement('style');
  style.id = 'navbar-unpaid-badge-style';
  style.textContent = `
    .user-dropdown .dropdown-item {
      position: relative;
      padding-right: 60px;
    }
    .unpaid-badge-menu {
      position: absolute;
      top: 50%;
      right: 18px;
      transform: translateY(-50%);
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-width: 20px;
      height: 20px;
      padding: 0 6px;
      background: #dc3545;
      color: #fff;
      border-radius: 10px;
      font-size: 11px;
      font-weight: 700;
      box-shadow: 0 2px 8px rgba(220, 53, 69, 0.4);
      animation: pulse-badge 2s infinite;
      z-index: 10;
    }
    .unpaid-badge-menu::before {
      content: '';
      width: 6px;
      height: 6px;
      background: #fff;
      border-radius: 50%;
      display: inline-block;
      margin-right: 4px;
      animation: blink-badge 1.5s infinite;
    }
    @keyframes pulse-badge {
      0%, 100% { transform: scale(1); }
      50% { transform: scale(1.15); }
    }
    @keyframes blink-badge {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.3; }
    }
  `;
  document.head.appendChild(style);
}

// Update navbar ngay khi page load
document.addEventListener('DOMContentLoaded', () => {
  console.log('[Navbar Auth] DOMContentLoaded triggered');
  updateNavbarAuth();
});

// Update navbar khi window load (backup)
window.addEventListener('load', () => {
  console.log('[Navbar Auth] Window load triggered');
  updateNavbarAuth();
});

// Update navbar khi có thay đổi localStorage
window.addEventListener('storage', (e) => {
  if (e.key === 'token' || e.key === 'user') {
    console.log('[Navbar Auth] Storage change detected');
    updateNavbarAuth();
  }
});

// Update navbar khi có thay đổi localStorage trong cùng tab
const originalSetItem = localStorage.setItem;
localStorage.setItem = function(key, value) {
  originalSetItem.apply(this, arguments);
  if (key === 'token' || key === 'user') {
    console.log('[Navbar Auth] LocalStorage setItem detected:', key);
    setTimeout(() => updateNavbarAuth(), 100);
  }
};

// Update navbar ngay khi script load (immediate)
console.log('[Navbar Auth] Script loaded, updating navbar immediately');
updateNavbarAuth();

// Sử dụng MutationObserver để theo dõi khi navbar được thêm vào DOM
const observer = new MutationObserver((mutations) => {
  const navbarMenu = document.querySelector('#ftco-nav .navbar-nav.ml-auto');
  if (navbarMenu) {
    console.log('[Navbar Auth] Navbar detected via MutationObserver');
    updateNavbarAuth();
    observer.disconnect(); // Ngừng observe sau khi tìm thấy
  }
});

// Bắt đầu observe DOM changes
if (document.body) {
  observer.observe(document.body, { childList: true, subtree: true });
} else {
  // Nếu body chưa có, đợi DOMContentLoaded
  document.addEventListener('DOMContentLoaded', () => {
    observer.observe(document.body, { childList: true, subtree: true });
  });
}

// Listen for custom login event
window.addEventListener('userLoggedIn', (e) => {
  console.log('[Navbar Auth] User logged in event detected');
  updateNavbarAuth();
});

// Listen for header loaded event (from load-header.js)
window.addEventListener('headerLoaded', (e) => {
  console.log('[Navbar Auth] Header loaded event detected');
  setTimeout(() => updateNavbarAuth(), 100);
});

/**
 * Update navbar dựa trên trạng thái đăng nhập
 */
function updateNavbarAuth(retryCount = 0) {
  const token = localStorage.getItem('token');
  const userStr = localStorage.getItem('user');
  
  console.log('[Navbar Auth] Updating navbar...', { hasToken: !!token, hasUser: !!userStr, retry: retryCount });
  
  // Tìm navbar menu
  const navbarMenu = document.querySelector('#ftco-nav .navbar-nav.ml-auto');
  
  if (!navbarMenu) {
    console.warn('[Navbar Auth] Navbar menu not found, retry:', retryCount);
    
    // Retry sau 100ms nếu navbar chưa render (tối đa 10 lần = 1 giây)
    if (retryCount < 10) {
      setTimeout(() => updateNavbarAuth(retryCount + 1), 100);
    } else {
      console.error('[Navbar Auth] Navbar menu not found after 10 retries');
    }
    return;
  }
  
  // Debug: Đếm số auth items hiện tại
  const currentAuthItems = navbarMenu.querySelectorAll('.nav-item.auth-item, .user-dropdown');
  console.log('[Navbar Auth] Current auth items count:', currentAuthItems.length);
  
  if (token && userStr) {
    // User đã đăng nhập
    try {
      const user = JSON.parse(userStr);
      console.log('[Navbar Auth] User logged in:', user.email);
      showAuthenticatedNav(navbarMenu, user);
    } catch (error) {
      console.error('[Navbar Auth] Error parsing user data:', error);
      showGuestNav(navbarMenu);
    }
  } else {
    // User chưa đăng nhập
    console.log('[Navbar Auth] User not logged in, showing guest nav');
    showGuestNav(navbarMenu);
  }
}

/**
 * Hiển thị nav cho user đã đăng nhập
 */
function showAuthenticatedNav(navbarMenu, user) {
  // XÓA TẤT CẢ auth items cũ (tránh duplicate)
  navbarMenu.querySelectorAll('.nav-item.auth-item, .user-dropdown').forEach(item => item.remove());
  
  // Tìm và xóa các menu item login/register
  const loginItem = navbarMenu.querySelector('a[href="/customer/login.html"], a[href="login.html"]')?.parentElement;
  const registerItem = navbarMenu.querySelector('a[href="/customer/register.html"], a[href="register.html"]')?.parentElement;
  
  if (loginItem) loginItem.remove();
  if (registerItem) registerItem.remove();
  
  // Tạo user dropdown
  const userDropdown = createUserDropdown(user);
  navbarMenu.appendChild(userDropdown);
  
  // Force navbar to refresh/repaint
  forceNavbarRefresh();
}

/**
 * Hiển thị nav cho guest (chưa đăng nhập)
 */
function showGuestNav(navbarMenu) {
  // XÓA TẤT CẢ auth items cũ (tránh duplicate)
  navbarMenu.querySelectorAll('.nav-item.auth-item, .user-dropdown').forEach(item => item.remove());
  
  // Đảm bảo có nút login/register
  const hasLogin = navbarMenu.querySelector('a[href="/customer/login.html"], a[href="login.html"]');
  const hasRegister = navbarMenu.querySelector('a[href="/customer/register.html"], a[href="register.html"]');
  
  if (!hasLogin) {
    const loginItem = document.createElement('li');
    loginItem.className = 'nav-item auth-item';
    loginItem.innerHTML = '<a href="/customer/login.html" class="nav-link">Đăng Nhập</a>';
    navbarMenu.appendChild(loginItem);
  }
  
  if (!hasRegister) {
    const registerItem = document.createElement('li');
    registerItem.className = 'nav-item auth-item';
    registerItem.innerHTML = '<a href="/customer/register.html" class="nav-link">Đăng Ký</a>';
    navbarMenu.appendChild(registerItem);
  }
}

/**
 * Kiểm tra booking có thể thanh toán (chưa thanh toán)
 * Logic giống với canPayBooking trong my-bookings.html
 */
function canPayBooking(booking) {
  return (booking.status === 'Pending' || booking.status === 'Confirmed') && booking.status !== 'Paid';
}

/**
 * Đếm số booking chưa thanh toán
 */
async function getUnpaidBookingsCount() {
  try {
    const token = localStorage.getItem('token');
    if (!token) return 0;
    
    const url = `${location.origin}/api/bookings/my?_=${Date.now()}`;
    const response = await fetch(url, {
      cache: 'no-store',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });
    
    if (!response.ok) {
      console.warn('[Navbar Auth] Cannot fetch unpaid bookings, status:', response.status);
      return 0;
    }
    
    const data = await response.json();
    const bookings = Array.isArray(data) ? data : (data.items || []);
    
    // Lưu cache toàn cục để dùng lại
    window._bookings = bookings;
    
    // Đếm số booking chưa thanh toán (sử dụng cùng logic với canPayBooking)
    const unpaidCount = bookings.filter(booking => canPayBooking(booking)).length;
    
    return unpaidCount;
  } catch (error) {
    console.error('[Navbar Auth] Error fetching unpaid bookings count:', error);
    return 0;
  }
}

/**
 * Cập nhật badge đỏ trong menu
 */
async function updateUnpaidBadge() {
  const unpaidCount = await getUnpaidBookingsCount();
  const bookingMenuItem = document.querySelector('.user-dropdown .dropdown-item[href="my-bookings.html"]');
  
  if (bookingMenuItem) {
    // Xóa badge cũ nếu có
    const oldBadge = bookingMenuItem.querySelector('.unpaid-badge-menu');
    if (oldBadge) oldBadge.remove();
    
    // Thêm badge mới nếu có booking chưa thanh toán
    if (unpaidCount > 0) {
      const badge = document.createElement('span');
      badge.className = 'unpaid-badge-menu';
      badge.textContent = unpaidCount > 99 ? '99+' : unpaidCount;
      bookingMenuItem.appendChild(badge);
    }
  }
}

/**
 * Tạo user dropdown menu
 */
function createUserDropdown(user) {
  const li = document.createElement('li');
  li.className = 'nav-item dropdown user-dropdown';
  
  // Force inline styles to ensure visibility
  li.style.display = 'block';
  li.style.visibility = 'visible';
  li.style.opacity = '1';
  
  const displayName = user.name || user.fullName || user.email || 'User';
  const email = user.email || '';
  
  li.innerHTML = `
    <a class="nav-link dropdown-toggle" href="#" id="userDropdown" role="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
      <i class="icon-user"></i>
      <span class="user-email">${email}</span>
    </a>
    <div class="dropdown-menu dropdown-menu-right user-dropdown-menu" aria-labelledby="userDropdown">
      <div class="dropdown-header">
        <strong>${displayName}</strong>
        <small class="d-block text-muted">${email}</small>
      </div>
      <div class="dropdown-divider"></div>
      <a class="dropdown-item" href="account.html">
        <i class="icon-user"></i> Thông Tin Cá Nhân
      </a>
      <a class="dropdown-item" href="my-bookings.html">
        <i class="icon-calendar"></i> Đặt Phòng Của Tôi
      </a>
      <a class="dropdown-item" href="my-restaurant-orders.html">
        <i class="icon-restaurant"></i> Các Món Ăn Đã Đặt
      </a>
      <a class="dropdown-item" href="my-tickets.html">
        <i class="icon-support"></i> Tickets Hỗ Trợ
      </a>
      <div class="dropdown-divider"></div>
      <a class="dropdown-item text-danger" href="#" onclick="handleLogout(event)">
        <i class="icon-power"></i> Đăng Xuất
      </a>
    </div>
  `;
  
  console.log('[Navbar Auth] User dropdown created with email:', email);
  
  // Cập nhật badge sau khi dropdown được tạo
  setTimeout(() => {
    updateUnpaidBadge();
  }, 500);
  
  return li;
}

/**
 * Force navbar refresh/repaint
 */
function forceNavbarRefresh() {
  console.log('[Navbar Auth] Forcing navbar refresh...');
  
  try {
    // Method 1: Force reflow của navbar menu
    const navbarMenu = document.querySelector('#ftco-nav .navbar-nav.ml-auto');
    if (navbarMenu) {
      // Force browser to repaint
      navbarMenu.style.opacity = '0.99';
      setTimeout(() => {
        navbarMenu.style.opacity = '';
      }, 50);
      console.log('[Navbar Auth] Navbar menu repainted');
    }
    
    // Method 2: Re-initialize Bootstrap dropdowns nếu jQuery available
    if (typeof $ !== 'undefined' && $.fn && $.fn.dropdown) {
      setTimeout(() => {
        try {
          $('.user-dropdown .dropdown-toggle').dropdown();
          console.log('[Navbar Auth] Bootstrap dropdown initialized');
        } catch (e) {
          console.log('[Navbar Auth] Dropdown already initialized');
        }
      }, 100);
    }
    
    // Method 3: Trigger window resize để force re-render
    setTimeout(() => {
      window.dispatchEvent(new Event('resize'));
    }, 10);
    
  } catch (error) {
    console.error('[Navbar Auth] Error in forceNavbarRefresh:', error);
  }
}

/**
 * Xử lý đăng xuất
 */
function handleLogout(event) {
  event.preventDefault();
  
  if (confirm('Bạn có chắc muốn đăng xuất?')) {
    // Xóa thông tin đăng nhập
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    
    // Thông báo
    alert('Đăng xuất thành công!');
    
    // Reload trang
    window.location.reload();
  }
}

// Export functions để có thể gọi từ nơi khác
window.updateNavbarAuth = updateNavbarAuth;
window.handleLogout = handleLogout;
window.updateUnpaidBadge = updateUnpaidBadge;

// Cập nhật badge định kỳ mỗi 30 giây
setInterval(() => {
  const token = localStorage.getItem('token');
  if (token) {
    updateUnpaidBadge();
  }
}, 30000);

