/**
 * Coupons Management Page JavaScript
 */

// Sử dụng API_BASE_URL từ api.js
const API_BASE = API_BASE_URL;
let dataTable;
let editingCouponId = null;

// Check auth on load
document.addEventListener('DOMContentLoaded', function() {
  initCouponsPage();
  loadCoupons();
  
  // Auto uppercase coupon code input
  document.getElementById('code')?.addEventListener('input', function(e) {
    e.target.value = e.target.value.toUpperCase().replace(/\s+/g, '');
  });
  
  // Show/hide maxDiscount based on type
  document.getElementById('type')?.addEventListener('change', function(e) {
    const maxDiscountDiv = document.getElementById('maxDiscount').closest('.mb-3');
    const hint = document.getElementById('valueHint');
    if (e.target.value === 'percent') {
      maxDiscountDiv.style.display = 'block';
      if (hint) hint.textContent = 'Nhập % giảm giá (1-100)';
    } else if (e.target.value === 'amount') {
      maxDiscountDiv.style.display = 'none';
      if (hint) hint.textContent = 'Nhập số tiền giảm (₫)';
    }
  });
});

function initCouponsPage() {
  console.log('🔵 [initCouponsPage] Checking auth...');
  const token = localStorage.getItem('token');
  const user = JSON.parse(localStorage.getItem('user') || '{}');
  
  if (!token || !user.role) {
    console.log('❌ [initCouponsPage] No token or role - redirecting to login');
    window.location.href = '/customer/login.html';
    return;
  }
  
  console.log('✅ [initCouponsPage] Auth OK');

  if (user.role !== 'Admin' && user.role !== 'Manager') {
    if (window.showToast) {
      showToast('Bạn không có quyền truy cập trang này!', 'error');
    } else {
      alert('Bạn không có quyền truy cập trang này!');
    }
    setTimeout(() => {
      window.location.href = '/customer/index.html';
    }, 2000);
    return;
  }

  document.getElementById('userFullName').textContent = user.fullName || user.username;
  
  const roleElement = document.getElementById('userRole');
  if (roleElement && user.role) {
    const roleNames = {
      'Admin': 'Quản trị viên',
      'Manager': 'Quản lý',
      'FrontDesk': 'Lễ tân',
      'Customer': 'Khách hàng'
    };
    roleElement.textContent = roleNames[user.role] || user.role;
  }
}

async function loadCoupons() {
  console.log('🔵 [loadCoupons] Function called');
  const isActive = document.getElementById('filterStatus').value;
  const type = document.getElementById('filterType').value;
  
  let url = `${API_BASE}/coupons?`;
  if (isActive !== '') url += `isActive=${isActive}&`;
  if (type) url += `type=${type}&`;

  console.log('🔵 [loadCoupons] API URL:', url);

  try {
    const response = await fetch(url, {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    });

    console.log('🔵 [loadCoupons] Response status:', response.status);

    if (!response.ok) {
      if (response.status === 404) {
        // API not found - initialize empty table with message
        console.warn('⚠️ [loadCoupons] API endpoint /api/coupons not found (404). Backend API may not be implemented yet.');
        renderCouponsTable([]);
        showToast('API mã giảm giá chưa được triển khai. Vui lòng liên hệ quản trị viên.', 'warning');
        return;
      }
      const errorText = await response.text().catch(() => '');
      throw new Error(`Failed to load coupons: ${response.status} ${errorText}`);
    }

    const coupons = await response.json();
    console.log('🔵 [loadCoupons] Coupons response:', coupons);
    
    // Handle different response formats
    let couponsArray = [];
    if (Array.isArray(coupons)) {
      couponsArray = coupons;
    } else if (coupons && Array.isArray(coupons.items)) {
      couponsArray = coupons.items;
    } else if (coupons && Array.isArray(coupons.data)) {
      couponsArray = coupons.data;
    }
    
    console.log('🔵 [loadCoupons] Coupons array:', couponsArray.length || 0);
    renderCouponsTable(couponsArray);
    
  } catch (error) {
    console.error('❌ [loadCoupons] Error:', error);
    showToast('Lỗi khi tải danh sách mã giảm giá: ' + error.message, 'danger');
    renderCouponsTable([]);
  }
}

function renderCouponsTable(coupons) {
  const tbody = document.querySelector('#couponsTable tbody');
  const table = document.querySelector('#couponsTable');
  if (!tbody || !table) return;

  // Destroy existing DataTable properly
  if (dataTable && $.fn.DataTable.isDataTable('#couponsTable')) {
    try {
      dataTable.clear();
      dataTable.destroy();
    } catch (e) {
      console.warn('⚠️ Error destroying DataTable:', e);
      // Try to remove table element and recreate if destroy fails
      const tableWrapper = table.closest('.dataTables_wrapper');
      if (tableWrapper) {
        tableWrapper.remove();
        const cardBody = table.closest('.card-body');
        if (cardBody) {
          const newTable = table.cloneNode(false);
          newTable.innerHTML = table.innerHTML;
          cardBody.appendChild(newTable);
        }
      }
    }
    dataTable = null;
  }

  // Clear tbody
  tbody.innerHTML = '';

  if (!coupons || coupons.length === 0) {
    // Don't initialize DataTable for empty state - just show message
    const emptyRow = document.createElement('tr');
    emptyRow.innerHTML = '<td colspan="9" class="text-center text-muted py-4"><div style="padding:40px"><i class="bx bx-info-circle" style="font-size:48px;opacity:0.3;display:block;margin-bottom:12px"></i><div style="font-size:16px;font-weight:500;margin-bottom:8px">Chưa có mã giảm giá nào</div><div style="font-size:13px;opacity:0.7">API backend có thể chưa được triển khai. Vui lòng liên hệ quản trị viên.</div></div></td>';
    tbody.appendChild(emptyRow);
    // Don't initialize DataTable - it causes column count errors with colspan
    return;
  }

  coupons.forEach(coupon => {
    const typeText = coupon.type === 'percent' ? '%' : '₫';
    const valueDisplay = coupon.type === 'percent' 
      ? `${coupon.value}%` 
      : `${formatVND(coupon.value)}`;
    const maxDiscountText = coupon.maxDiscount ? ` (tối đa ${formatVND(coupon.maxDiscount)})` : '';
    const usesCount = coupon.usesCount || coupon.usedCount || 0;
    const maxUses = coupon.maxUses || coupon.maxUseCount || '∞';
    const usesText = maxUses === '∞' || maxUses === 0 ? `${usesCount} / ∞` : `${usesCount} / ${maxUses}`;
    
    const startDate = coupon.startDate ? new Date(coupon.startDate).toLocaleString('vi-VN') : '-';
    const endDate = coupon.endDate ? new Date(coupon.endDate).toLocaleString('vi-VN') : '-';
    
    const isActive = coupon.isActive !== false;
    const statusBadge = isActive 
      ? '<span class="badge bg-label-success">Hoạt động</span>' 
      : '<span class="badge bg-label-secondary">Đã tắt</span>';

    // Escape HTML to prevent XSS
    const safeCode = String(coupon.code || '-').replace(/"/g, '&quot;');
    const safeDesc = String(coupon.description || '-').replace(/"/g, '&quot;');
    const couponId = String(coupon.id || coupon.couponId || '').replace(/"/g, '&quot;');
    
    const row = document.createElement('tr');
    row.innerHTML = `
      <td><strong class="text-uppercase">${safeCode}</strong></td>
      <td>${safeDesc}</td>
      <td>${coupon.type === 'percent' ? 'Phần trăm' : 'Số tiền'}</td>
      <td>${valueDisplay}${maxDiscountText}</td>
      <td>${startDate}</td>
      <td>${endDate}</td>
      <td>${usesText}</td>
      <td>${statusBadge}</td>
      <td>
        <div class="dropdown">
          <button type="button" class="btn p-0 dropdown-toggle hide-arrow" data-bs-toggle="dropdown">
            <i class="bx bx-dots-vertical-rounded"></i>
          </button>
          <div class="dropdown-menu">
            <a class="dropdown-item" href="javascript:void(0);" onclick="editCoupon('${couponId}')">
              <i class="bx bx-edit-alt me-1"></i> Sửa
            </a>
            <a class="dropdown-item" href="javascript:void(0);" onclick="toggleCouponStatus('${couponId}', ${!isActive})">
              <i class="bx bx-${isActive ? 'x' : 'check'}-circle me-1"></i> ${isActive ? 'Tắt' : 'Bật'}
            </a>
            <div class="dropdown-divider"></div>
            <a class="dropdown-item text-danger" href="javascript:void(0);" onclick="deleteCoupon('${couponId}')">
              <i class="bx bx-trash me-1"></i> Xóa
            </a>
          </div>
        </div>
      </td>
    `;
    tbody.appendChild(row);
  });

  // Initialize DataTable after a small delay to ensure DOM is ready
  setTimeout(() => {
    try {
      if (!$.fn.DataTable.isDataTable('#couponsTable')) {
        dataTable = $('#couponsTable').DataTable({
          language: {
            search: 'Tìm kiếm:',
            lengthMenu: 'Hiển thị _MENU_ bản ghi',
            info: 'Hiển thị _START_ đến _END_ của _TOTAL_ bản ghi',
            paginate: {
              first: 'Đầu',
              last: 'Cuối',
              next: 'Sau',
              previous: 'Trước'
            }
          },
          pageLength: 10,
          order: [[4, 'desc']], // Sort by start date desc
          columnDefs: [
            { orderable: false, targets: [8] } // Disable sorting on actions column
          ]
        });
      }
    } catch (e) {
      console.error('❌ Error initializing DataTable:', e);
      showToast('Lỗi khởi tạo bảng dữ liệu: ' + e.message, 'danger');
    }
  }, 150);
}

function openCreateModal() {
  editingCouponId = null;
  document.getElementById('modalTitle').textContent = 'Tạo Mã giảm giá Mới';
  document.getElementById('couponForm').reset();
  document.getElementById('couponId').value = '';
  document.getElementById('isActive').checked = true;
  
  // Set default dates (now and +30 days)
  const now = new Date();
  const endDate = new Date(now);
  endDate.setDate(endDate.getDate() + 30);
  
  document.getElementById('startDate').value = formatDateTimeLocal(now);
  document.getElementById('endDate').value = formatDateTimeLocal(endDate);
  
  // Hide maxDiscount initially
  document.getElementById('maxDiscount').closest('.mb-3').style.display = 'none';
  
  new bootstrap.Modal(document.getElementById('couponModal')).show();
}

function editCoupon(id) {
  editingCouponId = id;
  document.getElementById('modalTitle').textContent = 'Sửa Mã giảm giá';
  
  const token = localStorage.getItem('token');
  fetch(`${API_BASE}/coupons/${id}`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  })
    .then(response => response.json())
    .then(coupon => {
      document.getElementById('couponId').value = coupon.id || coupon.couponId;
      document.getElementById('code').value = coupon.code || '';
      document.getElementById('description').value = coupon.description || '';
      document.getElementById('type').value = coupon.type || 'percent';
      document.getElementById('value').value = coupon.value || '';
      document.getElementById('maxDiscount').value = coupon.maxDiscount || '';
      document.getElementById('maxUses').value = coupon.maxUses || coupon.maxUseCount || '';
      
      const startDate = coupon.startDate ? new Date(coupon.startDate) : new Date();
      const endDate = coupon.endDate ? new Date(coupon.endDate) : new Date();
      
      document.getElementById('startDate').value = formatDateTimeLocal(startDate);
      document.getElementById('endDate').value = formatDateTimeLocal(endDate);
      document.getElementById('isActive').checked = coupon.isActive !== false;
      
      // Show/hide maxDiscount based on type
      const type = coupon.type || 'percent';
      const maxDiscountDiv = document.getElementById('maxDiscount').closest('.mb-3');
      if (type === 'percent') {
        maxDiscountDiv.style.display = 'block';
      } else {
        maxDiscountDiv.style.display = 'none';
      }
      
      new bootstrap.Modal(document.getElementById('couponModal')).show();
    })
    .catch(error => {
      console.error('Error loading coupon:', error);
      showToast('Lỗi khi tải thông tin mã giảm giá', 'danger');
    });
}

async function saveCoupon() {
  const form = document.getElementById('couponForm');
  
  // Use Validation utility if available
  if (window.Validation) {
    const result = Validation.validateForm(form);
    if (!result.valid) {
      if (result.errors.length > 0) {
        const firstError = result.errors[0];
        firstError.input.focus();
        if (window.showToast) {
          showToast(firstError.message, 'error');
        }
      }
      return;
    }
  } else {
    // Fallback to native validation
    if (!form.checkValidity()) {
      form.reportValidity();
      return;
    }
  }

  const couponData = {
    code: document.getElementById('code').value.trim().toUpperCase(),
    description: document.getElementById('description').value.trim(),
    type: document.getElementById('type').value,
    value: parseFloat(document.getElementById('value').value),
    maxDiscount: document.getElementById('maxDiscount').value ? parseFloat(document.getElementById('maxDiscount').value) : null,
    maxUses: document.getElementById('maxUses').value ? parseInt(document.getElementById('maxUses').value) : 0,
    startDate: document.getElementById('startDate').value,
    endDate: document.getElementById('endDate').value,
    isActive: document.getElementById('isActive').checked
  };

  // Validation
  if (couponData.type === 'percent' && (couponData.value < 1 || couponData.value > 100)) {
    showToast('Giảm giá phần trăm phải từ 1% đến 100%', 'warning');
    return;
  }
  if (couponData.type === 'amount' && couponData.value <= 0) {
    showToast('Số tiền giảm phải lớn hơn 0', 'warning');
    return;
  }
  if (new Date(couponData.endDate) <= new Date(couponData.startDate)) {
    showToast('Ngày kết thúc phải sau ngày bắt đầu', 'warning');
    return;
  }

  const token = localStorage.getItem('token');
  const isEdit = !!editingCouponId;
  const url = isEdit ? `${API_BASE}/coupons/${editingCouponId}` : `${API_BASE}/coupons`;
  const method = isEdit ? 'PUT' : 'POST';

  try {
    const response = await fetch(url, {
      method: method,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(couponData)
    });

    if (!response.ok) {
      let errorMessage = `Failed to ${isEdit ? 'update' : 'create'} coupon`;
      if (response.status === 404) {
        errorMessage = 'API mã giảm giá chưa được triển khai. Vui lòng liên hệ quản trị viên để cài đặt backend API.';
      } else if (response.status === 403) {
        errorMessage = 'Bạn không có quyền thực hiện thao tác này.';
      } else if (response.status === 400) {
        try {
          const errorData = await response.json();
          errorMessage = errorData.message || errorData.errors?.[Object.keys(errorData.errors || {})[0]]?.[0] || errorMessage;
        } catch {
          const errorText = await response.text().catch(() => '');
          errorMessage = errorText || errorMessage;
        }
      } else {
        try {
          const errorData = await response.json().catch(() => ({}));
          errorMessage = errorData.message || errorMessage;
        } catch {
          errorMessage = `HTTP ${response.status}: ${response.statusText}`;
        }
      }
      throw new Error(errorMessage);
    }

    if (window.showToast) {
      showToast(`${isEdit ? 'Cập nhật' : 'Tạo'} mã giảm giá thành công!`, 'success');
    }
    
    // Close modal properly
    const modalEl = document.getElementById('couponModal');
    if (modalEl) {
      const modal = bootstrap.Modal.getInstance(modalEl);
      if (modal) {
        modal.hide();
      } else {
        const newModal = new bootstrap.Modal(modalEl);
        newModal.hide();
      }
    }
    
    // Reset form
    form.reset();
    editingCouponId = null;
    loadCoupons();
  } catch (error) {
    console.error('Error saving coupon:', error);
    showToast(`Lỗi khi ${isEdit ? 'cập nhật' : 'tạo'} mã giảm giá: ${error.message}`, 'danger');
  }
}

async function toggleCouponStatus(id, newStatus) {
  const token = localStorage.getItem('token');
  try {
    const response = await fetch(`${API_BASE}/coupons/${id}`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ isActive: newStatus })
    });

    if (!response.ok) {
      let errorMessage = 'Không thể cập nhật trạng thái';
      if (response.status === 404) {
        errorMessage = 'API mã giảm giá chưa được triển khai. Vui lòng liên hệ quản trị viên.';
      } else if (response.status === 403) {
        errorMessage = 'Bạn không có quyền thực hiện thao tác này.';
      } else {
        try {
          const errorData = await response.json().catch(() => ({}));
          errorMessage = errorData.message || errorMessage;
        } catch {
          errorMessage = `HTTP ${response.status}: ${response.statusText}`;
        }
      }
      throw new Error(errorMessage);
    }

    showToast(`Đã ${newStatus ? 'bật' : 'tắt'} mã giảm giá`, 'success');
    loadCoupons();
  } catch (error) {
    console.error('Error toggling status:', error);
    showToast('Lỗi khi cập nhật trạng thái: ' + error.message, 'danger');
  }
}

async function deleteCoupon(id) {
  if (!confirm('Bạn có chắc chắn muốn xóa mã giảm giá này?')) return;

  const token = localStorage.getItem('token');
  try {
    const response = await fetch(`${API_BASE}/coupons/${id}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });

    if (!response.ok) {
      let errorMessage = 'Không thể xóa mã giảm giá';
      if (response.status === 404) {
        errorMessage = 'API mã giảm giá chưa được triển khai. Vui lòng liên hệ quản trị viên.';
      } else if (response.status === 403) {
        errorMessage = 'Bạn không có quyền thực hiện thao tác này.';
      } else {
        try {
          const errorData = await response.json().catch(() => ({}));
          errorMessage = errorData.message || errorMessage;
        } catch {
          errorMessage = `HTTP ${response.status}: ${response.statusText}`;
        }
      }
      throw new Error(errorMessage);
    }

    showToast('Đã xóa mã giảm giá thành công!', 'success');
    loadCoupons();
  } catch (error) {
    console.error('Error deleting coupon:', error);
    showToast('Lỗi khi xóa mã giảm giá: ' + error.message, 'danger');
  }
}

function formatVND(amount) {
  return new Intl.NumberFormat('vi-VN', { 
    style: 'currency', 
    currency: 'VND',
    minimumFractionDigits: 0
  }).format(amount || 0);
}

function formatDateTimeLocal(date) {
  const d = new Date(date);
  d.setMinutes(d.getMinutes() - d.getTimezoneOffset());
  return d.toISOString().slice(0, 16);
}

function showToast(message, type = 'info') {
  // Simple toast implementation
  const toast = document.createElement('div');
  toast.className = `alert alert-${type === 'success' ? 'success' : type === 'danger' ? 'danger' : type === 'warning' ? 'warning' : 'info'} alert-dismissible fade show position-fixed`;
  toast.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
  toast.innerHTML = `
    ${message}
    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
  `;
  document.body.appendChild(toast);
  setTimeout(() => toast.remove(), 5000);
}

