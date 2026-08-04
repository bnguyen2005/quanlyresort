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

let allUsers = []; // Store all users for stats calculation

async function loadUsers() {
  console.log('🔵 [loadUsers] Function called');
  const role = document.getElementById('filterRole').value;
  const isActive = document.getElementById('filterStatus').value;
  const search = document.getElementById('searchInput')?.value?.trim() || '';
  
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
    allUsers = users; // Store for stats
    console.log('🔵 [loadUsers] Users loaded:', users.length, users);
    
    // Update stats
    updateStats(users);
    
    // Apply search filter
    let filteredUsers = users;
    if (search) {
      const searchLower = search.toLowerCase();
      filteredUsers = users.filter(user => 
        (user.username && user.username.toLowerCase().includes(searchLower)) ||
        (user.email && user.email.toLowerCase().includes(searchLower)) ||
        (user.fullName && user.fullName.toLowerCase().includes(searchLower))
      );
    }
    
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
    tbody.innerHTML = filteredUsers.map(user => `
      <tr>
        <td>${user.userId}</td>
        <td><strong>${user.username}</strong></td>
        <td>${user.email}</td>
        <td>${user.fullName || '-'}</td>
        <td><span class="badge bg-primary">${roleNames[user.role] || user.role}</span></td>
        <td>${user.isActive ? '<span class="badge bg-success">Hoạt động</span>' : '<span class="badge bg-danger">Đã khóa</span>'}</td>
        <td>${new Date(user.createdAt).toLocaleDateString('vi-VN')}</td>
        <td>
          <div class="action-buttons">
            <button class="action-btn action-btn-edit" onclick="editUser(${user.userId})" title="Sửa">
              <i class="bx bx-edit-alt"></i>
            </button>
            <button class="action-btn action-btn-change" onclick="openChangePasswordModal(${user.userId})" title="Đổi mật khẩu">
              <i class="bx bx-key"></i>
            </button>
            <button class="action-btn action-btn-toggle" onclick="toggleActive(${user.userId}, ${user.isActive})" title="${user.isActive ? 'Khóa' : 'Mở khóa'}">
              <i class="bx bx-${user.isActive ? 'lock' : 'lock-open'}"></i>
            </button>
            <button class="action-btn action-btn-delete" onclick="deleteUser(${user.userId})" title="Xóa">
              <i class="bx bx-trash"></i>
            </button>
          </div>
        </td>
      </tr>
    `).join('');

    dataTable = $('#usersTable').DataTable({
      language: {
        url: '/admin/local-plugins/datatables/i18n/vi.json'
      },
      pageLength: 25,
      order: [[0, 'desc']],
      drawCallback: function() {
        // Re-initialize Bootstrap dropdowns after table redraw
        if (window.initializeDropdowns) {
          window.initializeDropdowns();
        }
      }
    });
    
    // Initialize Bootstrap dropdowns after table is ready
    setTimeout(() => {
      if (window.initializeDropdowns) {
        window.initializeDropdowns();
      }
    }, 100);

  } catch (error) {
    console.error('Error loading users:', error);
    if (window.showToast) {
      showToast('Lỗi khi tải danh sách users!', 'error');
    } else {
      alert('Lỗi khi tải danh sách users!');
    }
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
    if (window.showToast) {
      showToast('Lỗi khi tải thông tin user!', 'error');
    } else {
      alert('Lỗi khi tải thông tin user!');
    }
  }
}

async function saveUser() {
  console.log('🔵 [saveUser] Starting...');
  const form = document.getElementById('userForm');
  
  // Validate form using AdminValidation
  if (window.AdminValidation) {
    const validationRules = {
      username: { required: true, length: { minLength: 3, maxLength: 50 } },
      email: { required: true, email: true },
      fullName: { required: true, length: { minLength: 2, maxLength: 100 } },
      phoneNumber: { phone: true },
      role: { required: true }
    };
    
    const result = AdminValidation.validateForm(form, validationRules);
    if (!result.valid) {
      if (result.errors.length > 0) {
        const firstError = result.errors[0];
        firstError.input.focus();
        if (window.showToast) {
          showToast(firstError.message, 'error');
        } else {
          alert(firstError.message);
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

  const userId = document.getElementById('userId').value;
  console.log('🔵 [saveUser] UserId:', userId || 'NEW');
  
  const data = {
    username: document.getElementById('username').value.trim(),
    email: document.getElementById('email').value.trim(),
    fullName: document.getElementById('fullName').value.trim(),
    phoneNumber: document.getElementById('phoneNumber').value.trim() || null,
    role: document.getElementById('role').value,
    isActive: document.getElementById('isActive').checked
  };
  
  console.log('🔵 [saveUser] Data:', data);

  // Validate email
  if (window.AdminValidation) {
    const emailResult = AdminValidation.validateEmail(data.email);
    if (!emailResult.valid) {
      document.getElementById('email').focus();
      if (window.showToast) {
        showToast(emailResult.message, 'error');
      } else {
        alert(emailResult.message);
      }
      return;
    }
    
    // Validate phone if provided
    if (data.phoneNumber) {
      const phoneResult = AdminValidation.validatePhone(data.phoneNumber);
      if (!phoneResult.valid) {
        document.getElementById('phoneNumber').focus();
        if (window.showToast) {
          showToast(phoneResult.message, 'error');
        } else {
          alert(phoneResult.message);
        }
        return;
      }
    }
  }

  if (!userId) {
    // Create new user - password is required
    const password = document.getElementById('password').value;
    const passwordResult = window.AdminValidation 
      ? AdminValidation.validatePassword(password, true)
      : { valid: password && password.length >= 6, message: password ? '' : 'Mật khẩu là bắt buộc' };
    
    if (!passwordResult.valid) {
      document.getElementById('password').focus();
      if (window.showToast) {
        showToast(passwordResult.message || 'Mật khẩu phải có ít nhất 6 ký tự!', 'error');
      } else {
        alert(passwordResult.message || 'Mật khẩu phải có ít nhất 6 ký tự!');
      }
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

    if (window.showToast) {
      showToast(userId ? 'Cập nhật user thành công!' : 'Tạo user thành công!', 'success');
    } else {
      alert(userId ? 'Cập nhật user thành công!' : 'Tạo user thành công!');
    }
    
    // Close modal properly
    const modalEl = document.getElementById('userModal');
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
    document.getElementById('userId').value = '';
    loadUsers();

  } catch (error) {
    console.error('❌ [saveUser] Error:', error);
    if (window.showToast) {
      showToast('Lỗi: ' + error.message, 'error');
    } else {
      alert('Lỗi: ' + error.message);
    }
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

  // Validate using AdminValidation
  if (window.AdminValidation) {
    const passwordResult = AdminValidation.validatePassword(newPassword, true);
    if (!passwordResult.valid) {
      document.getElementById('newPassword').focus();
      if (window.showToast) {
        showToast(passwordResult.message, 'error');
      } else {
        alert(passwordResult.message);
      }
      return;
    }
    
    // Validate password match
    if (newPassword !== confirmPassword) {
      document.getElementById('confirmPassword').focus();
      if (window.showToast) {
        showToast('Mật khẩu xác nhận không khớp!', 'error');
      } else {
        alert('Mật khẩu xác nhận không khớp!');
      }
      return;
    }
  } else {
    // Fallback validation
    if (!newPassword || !confirmPassword) {
      if (window.showToast) {
        showToast('Vui lòng nhập đầy đủ thông tin!', 'warning');
      } else {
        alert('Vui lòng nhập đầy đủ thông tin!');
      }
      return;
    }
    
    if (newPassword.length < 6) {
      document.getElementById('newPassword').focus();
      if (window.showToast) {
        showToast('Mật khẩu phải có ít nhất 6 ký tự!', 'warning');
      } else {
        alert('Mật khẩu phải có ít nhất 6 ký tự!');
      }
      return;
    }
    
    if (newPassword !== confirmPassword) {
      document.getElementById('confirmPassword').focus();
      if (window.showToast) {
        showToast('Mật khẩu xác nhận không khớp!', 'error');
      } else {
        alert('Mật khẩu xác nhận không khớp!');
      }
      return;
    }
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

    if (window.showToast) {
      showToast('Đổi mật khẩu thành công!', 'success');
    } else {
      alert('Đổi mật khẩu thành công!');
    }
    
    // Close modal properly
    const modalEl = document.getElementById('changePasswordModal');
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
    document.getElementById('newPassword').value = '';
    document.getElementById('confirmPassword').value = '';

  } catch (error) {
    console.error('❌ [changePassword] Error:', error);
    if (window.showToast) {
      showToast('Lỗi khi đổi mật khẩu: ' + error.message, 'error');
    } else {
      alert('Lỗi khi đổi mật khẩu: ' + error.message);
    }
  }
}

async function toggleActive(id, currentStatus) {
  const confirmed = window.showConfirm 
    ? await showConfirm(`Bạn có chắc muốn ${currentStatus ? 'khóa' : 'mở khóa'} user này?`, 'Xác nhận thay đổi trạng thái')
    : confirm(`Bạn có chắc muốn ${currentStatus ? 'khóa' : 'mở khóa'} user này?`);
  if (!confirmed) return;

  try {
    const response = await fetch(`${API_BASE}/usermanagement/${id}/toggle-active`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    });

    if (!response.ok) throw new Error('Failed to toggle status');

    if (window.showToast) {
      showToast(`${currentStatus ? 'Khóa' : 'Mở khóa'} user thành công!`, 'success');
    } else {
      alert(`${currentStatus ? 'Khóa' : 'Mở khóa'} user thành công!`);
    }
    loadUsers();

  } catch (error) {
    console.error('Error toggling status:', error);
    if (window.showToast) {
      showToast('Lỗi khi thay đổi trạng thái!', 'error');
    } else {
      alert('Lỗi khi thay đổi trạng thái!');
    }
  }
}

async function deleteUser(id) {
  // Find user name for confirmation
  const user = allUsers.find(u => u.userId === id);
  const userName = user ? (user.fullName || user.username || user.email) : `User #${id}`;
  
  // Confirm delete
  if (window.AdminValidation) {
    AdminValidation.confirmDelete(userName, async () => {
      await performDeleteUser(id);
    });
  } else {
    if (confirm(`Bạn có chắc chắn muốn xóa "${userName}"? Hành động này không thể hoàn tác!`)) {
      await performDeleteUser(id);
    }
  }
}

async function performDeleteUser(id) {
  try {
    const response = await fetch(`${API_BASE}/usermanagement/${id}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    });

    if (!response.ok) throw new Error('Failed to delete user');

    if (window.showToast) {
      showToast('Xóa user thành công!', 'success');
    } else {
      alert('Xóa user thành công!');
    }
    loadUsers();

  } catch (error) {
    console.error('Error deleting user:', error);
    if (window.showToast) {
      showToast('Lỗi khi xóa user!', 'error');
    } else {
      alert('Lỗi khi xóa user!');
    }
  }
}

// Update stats cards
function updateStats(users) {
  const total = users.length;
  const active = users.filter(u => u.isActive).length;
  const inactive = users.filter(u => !u.isActive).length;
  const admin = users.filter(u => u.role === 'Admin').length;
  
  const totalEl = document.getElementById('totalUsers');
  const activeEl = document.getElementById('activeUsers');
  const inactiveEl = document.getElementById('inactiveUsers');
  const adminEl = document.getElementById('adminUsers');
  
  if (totalEl) totalEl.textContent = total;
  if (activeEl) activeEl.textContent = active;
  if (inactiveEl) inactiveEl.textContent = inactive;
  if (adminEl) adminEl.textContent = admin;
}

// Add search on Enter key
document.addEventListener('DOMContentLoaded', function() {
  const searchInput = document.getElementById('searchInput');
  if (searchInput) {
    searchInput.addEventListener('keypress', function(e) {
      if (e.key === 'Enter') {
        loadUsers();
      }
    });
  }
});

// Không cần khai báo logout() ở đây vì api.js đã có
// Navbar sẽ dùng commonLogout() từ common-navbar.js hoặc logout() từ api.js

