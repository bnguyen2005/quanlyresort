/**
 * Users Management Page JavaScript
 * File riêng để tránh conflict với api.js
 */

// Sử dụng API_BASE_URL từ api.js
const API_BASE = API_BASE_URL;
let dataTable;
let editingUserId = null;

// Check auth on load
document.addEventListener('DOMContentLoaded', function() {
  initUserPage();
  loadUsers();
});

function initUserPage() {
  console.log('🔵 [initUserPage] Checking auth...');
  const token = localStorage.getItem('token');
  const user = JSON.parse(localStorage.getItem('user') || '{}');
  
  console.log('🔵 [initUserPage] Token exists:', !!token);
  console.log('🔵 [initUserPage] User role:', user.role);
  
  if (!token || !user.role) {
    console.log('❌ [initUserPage] No token or role - redirecting to login');
    window.location.href = '/customer/login.html';
    return;
  }
  
  console.log('✅ [initUserPage] Auth OK');

  if (user.role !== 'Admin' && user.role !== 'Manager') {
    alert('Bạn không có quyền truy cập trang này!');
    window.location.href = '/customer/index.html';
    return;
  }

  document.getElementById('userFullName').textContent = user.fullName || user.username;
  
  // Update role display
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

async function loadUsers() {
  console.log('🔵 [loadUsers] Function called');
  const role = document.getElementById('filterRole').value;
  const isActive = document.getElementById('filterStatus').value;
  
  let url = `${API_BASE}/usermanagement?`;
  if (role) url += `role=${role}&`;
  if (isActive !== '') url += `isActive=${isActive}&`;

  console.log('🔵 [loadUsers] API URL:', url);
  console.log('🔵 [loadUsers] Token:', localStorage.getItem('token'));

  try {
    const response = await fetch(url, {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    });

    console.log('🔵 [loadUsers] Response status:', response.status);

    if (!response.ok) throw new Error('Failed to load users');

    const users = await response.json();
    console.log('🔵 [loadUsers] Users loaded:', users.length, users);
    
    if (dataTable) {
      dataTable.destroy();
    }

    const roleNames = {
      'Admin': 'Quản trị viên',
      'Manager': 'Quản lý',
      'Business': 'Kinh doanh',
      'FrontDesk': 'Lễ tân',
      'Cashier': 'Thu ngân',
      'Accounting': 'Kế toán',
      'Inventory': 'Kho',
      'Customer': 'Khách hàng'
    };

    const tbody = document.querySelector('#usersTable tbody');
    tbody.innerHTML = users.map(user => `
      <tr>
        <td>${user.userId}</td>
        <td><strong>${user.username}</strong></td>
        <td>${user.email}</td>
        <td>${user.fullName || '-'}</td>
        <td><span class="badge bg-primary">${roleNames[user.role] || user.role}</span></td>
        <td>${user.isActive ? '<span class="badge bg-success">Hoạt động</span>' : '<span class="badge bg-danger">Đã khóa</span>'}</td>
        <td>${new Date(user.createdAt).toLocaleDateString('vi-VN')}</td>
        <td>
          <div class="dropdown">
            <button type="button" class="btn btn-sm btn-icon dropdown-toggle hide-arrow" data-bs-toggle="dropdown">
              <i class="bx bx-dots-vertical-rounded"></i>
            </button>
            <div class="dropdown-menu">
              <a class="dropdown-item" href="javascript:void(0);" onclick="editUser(${user.userId})">
                <i class="bx bx-edit-alt me-1"></i> Sửa
              </a>
              <a class="dropdown-item" href="javascript:void(0);" onclick="openChangePasswordModal(${user.userId})">
                <i class="bx bx-key me-1"></i> Đổi mật khẩu
              </a>
              <a class="dropdown-item" href="javascript:void(0);" onclick="toggleActive(${user.userId}, ${user.isActive})">
                <i class="bx bx-${user.isActive ? 'lock' : 'lock-open'} me-1"></i> ${user.isActive ? 'Khóa' : 'Mở khóa'}
              </a>
              <div class="dropdown-divider"></div>
              <a class="dropdown-item text-danger" href="javascript:void(0);" onclick="deleteUser(${user.userId})">
                <i class="bx bx-trash me-1"></i> Xóa
              </a>
            </div>
          </div>
        </td>
      </tr>
    `).join('');

    dataTable = $('#usersTable').DataTable({
      language: {
  url: '/admin/local-plugins/datatables/i18n/vi.json'
      },
      pageLength: 25,
      order: [[0, 'desc']]
    });

  } catch (error) {
    console.error('Error loading users:', error);
    alert('Lỗi khi tải danh sách users!');
  }
}

function openCreateModal() {
  editingUserId = null;
  document.getElementById('modalTitle').textContent = 'Tạo User Mới';
  document.getElementById('userForm').reset();
  document.getElementById('userId').value = '';
  document.getElementById('isActive').checked = true;
  document.getElementById('password').required = true;
  document.getElementById('passwordRequired').classList.remove('d-none');
  
  const modal = new bootstrap.Modal(document.getElementById('userModal'));
  modal.show();
}

async function editUser(id) {
  try {
    const response = await fetch(`${API_BASE}/usermanagement/${id}`, {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    });

    if (!response.ok) throw new Error('Failed to load user');

    const user = await response.json();
    
    editingUserId = id;
    document.getElementById('modalTitle').textContent = 'Sửa User';
    document.getElementById('userId').value = user.userId;
    document.getElementById('username').value = user.username;
    document.getElementById('email').value = user.email;
    document.getElementById('fullName').value = user.fullName || '';
    document.getElementById('phoneNumber').value = user.phoneNumber || '';
    document.getElementById('role').value = user.role;
    document.getElementById('isActive').checked = user.isActive;
    document.getElementById('password').value = '';
    document.getElementById('password').required = false;
    document.getElementById('passwordRequired').classList.add('d-none');

    const modal = new bootstrap.Modal(document.getElementById('userModal'));
    modal.show();

  } catch (error) {
    console.error('Error loading user:', error);
    alert('Lỗi khi tải thông tin user!');
  }
}

async function saveUser() {
  console.log('🔵 [saveUser] Starting...');
  const form = document.getElementById('userForm');
  if (!form.checkValidity()) {
    form.reportValidity();
    return;
  }

  const userId = document.getElementById('userId').value;
  console.log('🔵 [saveUser] UserId:', userId || 'NEW');
  
  const data = {
    username: document.getElementById('username').value,
    email: document.getElementById('email').value,
    fullName: document.getElementById('fullName').value,
    phoneNumber: document.getElementById('phoneNumber').value,
    role: document.getElementById('role').value,
    isActive: document.getElementById('isActive').checked
  };
  
  console.log('🔵 [saveUser] Data:', data);

  if (!userId) {
    // Create new user
    const password = document.getElementById('password').value;
    if (!password) {
      alert('Vui lòng nhập mật khẩu!');
      return;
    }
    data.password = password;
  }

  try {
    const url = userId ? `${API_BASE}/usermanagement/${userId}` : `${API_BASE}/usermanagement`;
    const method = userId ? 'PUT' : 'POST';
    
    console.log('🔵 [saveUser] URL:', url);
    console.log('🔵 [saveUser] Method:', method);

    const response = await fetch(url, {
      method: method,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      },
      body: JSON.stringify(data)
    });
    
    console.log('🔵 [saveUser] Response status:', response.status);

    if (!response.ok) {
      const error = await response.json();
      console.log('❌ [saveUser] Error response:', error);
      throw new Error(error.message || 'Failed to save user');
    }
    
    const result = await response.json();
    console.log('✅ [saveUser] Success:', result);

    alert(userId ? 'Cập nhật user thành công!' : 'Tạo user thành công!');
    bootstrap.Modal.getInstance(document.getElementById('userModal')).hide();
    loadUsers();

  } catch (error) {
    console.error('❌ [saveUser] Error:', error);
    alert('Lỗi: ' + error.message);
  }
}

function openChangePasswordModal(id) {
  document.getElementById('changePasswordUserId').value = id;
  document.getElementById('newPassword').value = '';
  document.getElementById('confirmPassword').value = '';
  
  const modal = new bootstrap.Modal(document.getElementById('changePasswordModal'));
  modal.show();
}

async function changePassword() {
  console.log('🔵 [changePassword] Starting...');
  const userId = document.getElementById('changePasswordUserId').value;
  const newPassword = document.getElementById('newPassword').value;
  const confirmPassword = document.getElementById('confirmPassword').value;
  
  console.log('🔵 [changePassword] UserId:', userId);

  if (!newPassword || !confirmPassword) {
    alert('Vui lòng nhập đầy đủ thông tin!');
    return;
  }

  if (newPassword !== confirmPassword) {
    alert('Mật khẩu xác nhận không khớp!');
    return;
  }

  try {
    const url = `${API_BASE}/usermanagement/${userId}/change-password`;
    console.log('🔵 [changePassword] URL:', url);
    console.log('🔵 [changePassword] New password length:', newPassword.length);
    
    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      },
      body: JSON.stringify({ newPassword })
    });
    
    console.log('🔵 [changePassword] Response status:', response.status);

    if (!response.ok) {
      const error = await response.json();
      console.log('❌ [changePassword] Error response:', error);
      throw new Error(error.message || 'Failed to change password');
    }
    
    const result = await response.json();
    console.log('✅ [changePassword] Success:', result);

    alert('Đổi mật khẩu thành công!');
    bootstrap.Modal.getInstance(document.getElementById('changePasswordModal')).hide();

  } catch (error) {
    console.error('❌ [changePassword] Error:', error);
    alert('Lỗi khi đổi mật khẩu: ' + error.message);
  }
}

async function toggleActive(id, currentStatus) {
  if (!confirm(`Bạn có chắc muốn ${currentStatus ? 'khóa' : 'mở khóa'} user này?`)) return;

  try {
    const response = await fetch(`${API_BASE}/usermanagement/${id}/toggle-active`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    });

    if (!response.ok) throw new Error('Failed to toggle status');

    alert(`${currentStatus ? 'Khóa' : 'Mở khóa'} user thành công!`);
    loadUsers();

  } catch (error) {
    console.error('Error toggling status:', error);
    alert('Lỗi khi thay đổi trạng thái!');
  }
}

async function deleteUser(id) {
  if (!confirm('Bạn có chắc muốn xóa user này? Thao tác này không thể hoàn tác!')) return;

  try {
    const response = await fetch(`${API_BASE}/usermanagement/${id}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    });

    if (!response.ok) throw new Error('Failed to delete user');

    alert('Xóa user thành công!');
    loadUsers();

  } catch (error) {
    console.error('Error deleting user:', error);
    alert('Lỗi khi xóa user!');
  }
}

// Không cần khai báo logout() ở đây vì api.js đã có
// Navbar sẽ dùng commonLogout() từ common-navbar.js hoặc logout() từ api.js

