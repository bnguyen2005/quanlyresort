/**
 * Common Navbar Component for Admin Pages
 * Dùng chung cho tất cả trang admin
 */

// Navbar HTML template
const navbarHTML = `
<nav class="layout-navbar container-xxl navbar navbar-expand-xl navbar-detached align-items-center bg-navbar-theme" id="layout-navbar">
  <div class="layout-menu-toggle navbar-nav align-items-xl-center me-3 me-xl-0 d-xl-none">
    <a class="nav-item nav-link px-0 me-xl-4" href="javascript:void(0)">
      <i class="bx bx-menu bx-sm"></i>
    </a>
  </div>

  <div class="navbar-nav-right d-flex align-items-center" id="navbar-collapse">
    <div class="navbar-nav align-items-center">
      <div class="nav-item d-flex align-items-center">
        <h4 class="mb-0" id="pageTitle">Dashboard</h4>
      </div>
    </div>

    <ul class="navbar-nav flex-row align-items-center ms-auto">
      <!-- User Dropdown -->
      <li class="nav-item navbar-dropdown dropdown-user dropdown">
        <a class="nav-link dropdown-toggle hide-arrow" href="javascript:void(0);" data-bs-toggle="dropdown">
          <div class="avatar avatar-online">
            <img src="../assets/img/avatars/1.png" alt class="w-px-40 h-auto rounded-circle" />
          </div>
        </a>
        <ul class="dropdown-menu dropdown-menu-end">
          <li>
            <a class="dropdown-item" href="#">
              <div class="d-flex">
                <div class="flex-shrink-0 me-3">
                  <div class="avatar avatar-online">
                    <img src="../assets/img/avatars/1.png" alt class="w-px-40 h-auto rounded-circle" />
                  </div>
                </div>
                <div class="flex-grow-1">
                  <span class="fw-semibold d-block" id="userDisplayName">Admin</span>
                  <small class="text-muted" id="userRole">Admin</small>
                </div>
              </div>
            </a>
          </li>
          <li><div class="dropdown-divider"></div></li>
          <li>
            <a class="dropdown-item" href="#">
              <i class="bx bx-user me-2"></i>
              <span class="align-middle">Hồ sơ của tôi</span>
            </a>
          </li>
          <li>
            <a class="dropdown-item" href="#">
              <i class="bx bx-cog me-2"></i>
              <span class="align-middle">Cài đặt</span>
            </a>
          </li>
          <li><div class="dropdown-divider"></div></li>
          <li>
            <a class="dropdown-item" href="javascript:void(0);" onclick="commonLogout()">
              <i class="bx bx-power-off me-2"></i>
              <span class="align-middle">Đăng xuất</span>
            </a>
          </li>
        </ul>
      </li>
    </ul>
  </div>
</nav>
`;

// Initialize navbar
function initCommonNavbar(pageTitle = 'Dashboard') {
  // Set page title
  document.addEventListener('DOMContentLoaded', function() {
    const titleElement = document.getElementById('pageTitle');
    if (titleElement) {
      titleElement.textContent = pageTitle;
    }
    
    // Load user info
    loadNavbarUserInfo();
  });
}

// Load user info from localStorage
function loadNavbarUserInfo() {
  try {
    const user = JSON.parse(localStorage.getItem('user') || '{}');

    // Display name
    const displayNameElement = document.getElementById('userDisplayName');
    if (displayNameElement) {
      displayNameElement.textContent = user.fullName || user.username || 'User';
    }

    // Role: first try user.role, then try decode token claims
    let role = user.role;
    if (!role) {
      const token = localStorage.getItem('token');
      if (token) {
        try {
          const parts = token.split('.');
          if (parts.length >= 2) {
            const payload = JSON.parse(atob(parts[1]));
            role = payload.role || payload.roles || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
            if (Array.isArray(role)) role = role[0];
          }
        } catch (e) {
          // ignore
        }
      }
    }

    const roleElement = document.getElementById('userRole');
    if (roleElement) {
      const label = getRoleDisplayName(role || 'Guest');
      roleElement.innerHTML = `<span class="badge bg-label-secondary">${label}</span>`;
    }
  } catch (error) {
    console.error('Error loading user info:', error);
  }
}

// Get role display name in Vietnamese
function getRoleDisplayName(role) {
  const roleNames = {
    'Admin': 'Quản trị viên',
    'Manager': 'Quản lý',
    'Business': 'Kinh doanh',
    'FrontDesk': 'Lễ tân',
    'Cashier': 'Thu ngân',
    'Accounting': 'Kế toán',
    'Inventory': 'Kho',
    'Housekeeping': 'Dọn phòng',
    'Maintenance': 'Kỹ thuật',
    'Customer': 'Khách hàng'
  };
  return roleNames[role] || role;
}

// Common logout function
function commonLogout() {
  if (confirm('Bạn có chắc muốn đăng xuất?')) {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/customer/login.html';
  }
}

// Common auth check function
function checkCommonAuth(requiredRoles = ['Admin', 'Manager']) {
  const token = localStorage.getItem('token');
  const user = JSON.parse(localStorage.getItem('user') || '{}');
  
  if (!token || !user.role) {
    window.location.href = '/customer/login.html';
    return false;
  }

  if (!requiredRoles.includes(user.role)) {
    alert('Bạn không có quyền truy cập trang này!');
    window.location.href = '/customer/index.html';
    return false;
  }

  return true;
}

// Export for use in other files
if (typeof module !== 'undefined' && module.exports) {
  module.exports = {
    initCommonNavbar,
    loadNavbarUserInfo,
    getRoleDisplayName,
    commonLogout,
    checkCommonAuth
  };
}

// Inject a global Bootstrap modal into document.body for unified error/success display
(function() {
  function ensureAppModal() {
    if (document.getElementById('appModal')) return;
    const modalHtml = `
      <div class="modal fade" id="appModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-md modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title" id="appModalTitle"></h5>
              <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body" id="appModalBody"></div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
            </div>
          </div>
        </div>
      </div>`;
    const div = document.createElement('div');
    div.innerHTML = modalHtml;
    document.body.appendChild(div.firstElementChild);
  }

  // show an app modal. type can be 'error'|'success'|'info' which sets a small class on title
  window.showAppModal = function(title, bodyHtml, options = {}) {
    try {
      ensureAppModal();
      const titleEl = document.getElementById('appModalTitle');
      const bodyEl = document.getElementById('appModalBody');
      titleEl.textContent = title || '';
      if (typeof bodyHtml === 'string') bodyEl.innerHTML = bodyHtml; else if (bodyHtml instanceof Node) { bodyEl.innerHTML = ''; bodyEl.appendChild(bodyHtml); } else bodyEl.textContent = String(bodyHtml || '');

      // apply small styling for error/success
      const modalEl = document.getElementById('appModal');
      const bsModal = new bootstrap.Modal(modalEl);
      bsModal.show();
    } catch (e) {
      console.error('showAppModal error', e);
      alert((title ? title + '\n' : '') + (typeof bodyHtml === 'string' ? bodyHtml : JSON.stringify(bodyHtml)));
    }
  };

  window.showAppError = function(title, message) {
    const body = `<div class="text-danger"><pre style="white-space:pre-wrap">${escapeHtml(message || '')}</pre></div>`;
    window.showAppModal(title || 'Lỗi', body);
  };

  window.showAppSuccess = function(title, message) {
    const body = `<div class="text-success">${escapeHtml(message || '')}</div>`;
    window.showAppModal(title || 'Thành công', body);
  };

  function escapeHtml(s) {
    if (!s) return '';
    return String(s)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }

  // ensure modal exists on DOMContentLoaded (so other scripts can assume it's ready)
  document.addEventListener('DOMContentLoaded', ensureAppModal);
})();

/**
 * Fix overlay blocking all buttons - Auto-run on all admin pages
 */
(function() {
  'use strict';

  function fixOverlayBlocking() {
    // Get all overlays
    const overlays = document.querySelectorAll('.layout-overlay');
    
    overlays.forEach(overlay => {
      // Check if menu is expanded
      const isMenuExpanded = document.body.classList.contains('layout-menu-expanded');
      const isMobile = window.innerWidth < 1200;
      
      if (!isMenuExpanded && !isMobile) {
        // Desktop and menu not expanded - overlay should not block
        overlay.style.pointerEvents = 'none';
        overlay.style.display = 'none';
        overlay.style.zIndex = '-1';
      } else if (isMenuExpanded && isMobile) {
        // Mobile with menu expanded - overlay can block background but not buttons
        overlay.style.pointerEvents = 'auto';
        overlay.style.display = 'block';
      } else {
        // Other cases - disable overlay blocking
        overlay.style.pointerEvents = 'none';
      }
    });

    // Force all buttons to be clickable
    const allClickable = document.querySelectorAll(
      'button:not(:disabled), .btn:not(:disabled), a.btn, .dropdown-toggle, .dropdown-item, [onclick], [data-bs-toggle], table .btn, .card .btn, .modal .btn'
    );
    
    allClickable.forEach(el => {
      const computed = window.getComputedStyle(el);
      if (computed.pointerEvents === 'none') {
        el.style.pointerEvents = 'auto';
      }
      const zIndex = parseInt(computed.zIndex) || 0;
      if (zIndex < 100) {
        el.style.zIndex = '100';
      }
      if (computed.cursor === 'default' || computed.cursor === 'auto') {
        el.style.cursor = 'pointer';
      }
    });

    // SPECIAL FIX: Force logout buttons to be clickable
    // Find by ID
    const logoutById = document.getElementById('logoutBtn');
    // Find by onclick attribute
    const logoutByOnclick = document.querySelectorAll('[onclick*="commonLogout"], [onclick*="logout"]');
    // Find by icon (bx-power-off)
    const logoutIcons = document.querySelectorAll('.dropdown-item i.bx-power-off');
    
    // Collect all logout buttons
    const logoutButtons = [];
    if (logoutById) logoutButtons.push(logoutById);
    logoutByOnclick.forEach(btn => logoutButtons.push(btn));
    logoutIcons.forEach(icon => {
      const parentLink = icon.closest('.dropdown-item');
      if (parentLink) logoutButtons.push(parentLink);
    });
    
    // Remove duplicates
    const uniqueLogoutButtons = [...new Set(logoutButtons)];
    
    uniqueLogoutButtons.forEach(btn => {
      if (!btn) return;
      
      // Force styles
      btn.style.pointerEvents = 'auto';
      btn.style.cursor = 'pointer';
      btn.style.zIndex = '10003';
      btn.style.position = 'relative';
      
      // Add class for CSS targeting
      btn.classList.add('logout-button');
      
      // Ensure click handler is attached
      const hasOnclick = btn.hasAttribute('onclick');
      const hasClickHandler = btn.onclick !== null;
      
      if (!hasOnclick && !hasClickHandler) {
        // Attach commonLogout if available
        if (typeof commonLogout === 'function') {
          // Remove old listeners first
          const newBtn = btn.cloneNode(true);
          btn.parentNode.replaceChild(newBtn, btn);
          
          newBtn.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            console.log('🔄 [Logout] Button clicked, calling commonLogout');
            commonLogout();
            return false;
          });
        } else if (typeof logout === 'function') {
          // Fallback to logout function
          const newBtn = btn.cloneNode(true);
          btn.parentNode.replaceChild(newBtn, btn);
          
          newBtn.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            console.log('🔄 [Logout] Button clicked, calling logout');
            logout();
            return false;
          });
        }
      } else if (hasOnclick && typeof commonLogout === 'function') {
        // Ensure onclick attribute works
        btn.setAttribute('onclick', 'commonLogout(); return false;');
      }
      
      // Remove any blocking styles/attributes
      btn.classList.remove('disabled');
      btn.removeAttribute('disabled');
      btn.removeAttribute('aria-disabled');
      
      // Debug log
      console.log('✅ [Logout Fix] Fixed logout button:', btn.id || btn.className, 'onclick:', btn.onclick ? 'YES' : 'NO');
    });

    // Fix dropdown menu container
    const dropdownMenus = document.querySelectorAll('.dropdown-menu');
    dropdownMenus.forEach(menu => {
      menu.style.pointerEvents = 'auto';
      menu.style.zIndex = '10001';
    });
  }

  // Run immediately and on DOMContentLoaded
  fixOverlayBlocking();
  
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', fixOverlayBlocking);
  }
  
  // Monitor body class changes for menu toggle
  const observer = new MutationObserver(function() {
    setTimeout(fixOverlayBlocking, 50);
  });

  if (document.body) {
    observer.observe(document.body, {
      attributes: true,
      attributeFilter: ['class']
    });
  }

  // Re-run periodically to catch dynamically added buttons
  setInterval(fixOverlayBlocking, 1000);
})();

