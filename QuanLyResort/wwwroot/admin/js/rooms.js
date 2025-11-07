/**
 * Rooms Management JavaScript
 * Xử lý logic cho trang quản lý phòng
 */

// Kiểm tra đăng nhập — làm cho linh hoạt nếu helper chưa load
let __isAuthenticated = false;
if (typeof checkAuth === 'function') {
  try {
    __isAuthenticated = !!checkAuth();
  } catch (e) {
    console.warn('checkAuth threw an error:', e);
    __isAuthenticated = false;
  }
} else {
  console.warn('checkAuth is not defined — continuing in unauthenticated/dev mode');
  __isAuthenticated = false;
}

// Role helpers: read from localStorage.user or decode JWT token
const getRoles = () => {
  try {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      const u = JSON.parse(userStr);
      // common shapes: u.role or u.roles (array) or u.claims
      if (u.role) return Array.isArray(u.role) ? u.role : [u.role];
      if (u.roles) return Array.isArray(u.roles) ? u.roles : [u.roles];
    }
    const t = localStorage.getItem('token');
    if (t) {
      const parts = t.split('.');
      if (parts.length >= 2) {
        const payload = JSON.parse(atob(parts[1]));
        if (payload.role) return Array.isArray(payload.role) ? payload.role : [payload.role];
        if (payload.roles) return Array.isArray(payload.roles) ? payload.roles : [payload.roles];
        // check common claim names
        const claimRole = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
        if (claimRole) return Array.isArray(claimRole) ? claimRole : [claimRole];
      }
    }
  } catch (e) {
    console.warn('getRoles error', e);
  }
  return [];
};

const hasRole = (roleName) => {
  if (!roleName) return false;
  const roles = getRoles();
  return roles.some(r => String(r).toLowerCase() === String(roleName).toLowerCase());
};

// Permission helpers (use functions so they reflect dynamic login state)
const isAdmin = () => hasRole('Admin');
const isManager = () => hasRole('Manager');
const canCreate = () => isAdmin() || isManager();
const canEdit = () => isAdmin() || isManager();
const canDelete = () => isAdmin();

let allRooms = [];
let currentEditRoomId = null;

// Load rooms khi trang được tải
document.addEventListener('DOMContentLoaded', async () => {
  try {
    // Hiển thị tên user (nếu helper getUser tồn tại)
    if (typeof getUser === 'function') {
      try {
        const user = getUser();
        if (user && (user.fullName || user.fullName === '')) {
          const el = document.getElementById('userName') || document.getElementById('userDisplayName');
          if (el) el.textContent = user.fullName;
        }
      } catch (e) {
        console.warn('getUser threw error:', e);
      }
    }

  // Update auth state (if token exists)
  __isAuthenticated = !!localStorage.getItem('token');

  // Load danh sách phòng
  await loadRooms();

    // Setup event listeners
    setupEventListeners();

      // React to login events (so the page updates permissions live)
      window.addEventListener('userLoggedIn', async (e) => {
        __isAuthenticated = true;
        // show/hide Add button
        const btn = document.getElementById('btnAddRoom');
        if (btn) btn.style.display = canCreate() ? '' : 'none';
        // reload rooms with new permissions
        try { await loadRooms(); } catch (err) { console.warn('reload after login failed', err); }
      });

      // React to logout event if any
      window.addEventListener('userLoggedOut', (e) => {
        __isAuthenticated = false;
        const btn = document.getElementById('btnAddRoom');
        if (btn) btn.style.display = 'none';
      });

    // Setup logout (nếu nút và hàm logout tồn tại)
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn && typeof logout === 'function') {
      logoutBtn.addEventListener('click', (e) => {
        e.preventDefault();
        if (confirm('Bạn có chắc muốn đăng xuất?')) {
          logout();
        }
      });
    }

  } catch (error) {
    console.error('Error initializing rooms page:', error);
    showError('Không thể tải dữ liệu phòng');
  }
});

/**
 * Setup event listeners
 */
const setupEventListeners = () => {
  // Filter listeners (guard missing elements)
  const statusEl = document.getElementById('statusFilter');
  if (statusEl) statusEl.addEventListener('change', filterRooms);
  const typeEl = document.getElementById('typeFilter');
  if (typeEl) typeEl.addEventListener('change', filterRooms);

  // Search listener
  const searchEl = document.getElementById('searchInput');
  if (searchEl) searchEl.addEventListener('input', filterRooms);

  // Add room button
  const btnAddRoom = document.getElementById('btnAddRoom');
  if (btnAddRoom) {
    // Hide Add button if user cannot create
    btnAddRoom.style.display = canCreate() ? '' : 'none';
    btnAddRoom.addEventListener('click', () => {
      if (!__isAuthenticated || !canCreate()) {
        alert('Bạn cần đăng nhập với quyền Admin/Manager để thực hiện thao tác này.');
        return;
      }
      currentEditRoomId = null;
      resetRoomForm();
      document.getElementById('roomModalTitle').textContent = 'Thêm Phòng Mới';
    });
  }

  // Form submit
  document.getElementById('roomForm').addEventListener('submit', handleRoomFormSubmit);
};

/**
 * Load tất cả phòng từ API
 */
const loadRooms = async () => {
  try {
    const rooms = await getAllRooms();
    
    if (rooms && Array.isArray(rooms)) {
      allRooms = rooms;
      displayRooms(allRooms);
    } else {
      allRooms = [];
      displayRooms([]);
    }
  } catch (error) {
    console.error('Error loading rooms:', error);
    const tbody = document.getElementById('roomsTableBody');
    tbody.innerHTML = '<tr><td colspan="6" class="text-center text-danger">Không thể tải dữ liệu phòng</td></tr>';
  }
};

/**
 * Hiển thị danh sách phòng
 */
const displayRooms = (rooms) => {
  const tbody = document.getElementById('roomsTableBody');
  
  if (!rooms || rooms.length === 0) {
    tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Không có phòng nào</td></tr>';
    return;
  }

  const rows = rooms.map(room => {
    // derive status from backend fields (isAvailable / housekeepingStatus)
    const status = (function() {
      try {
        const hk = (room.housekeepingStatus || '').toString().toLowerCase();
        if (hk.includes('maintenance') || hk.includes('bảo trì')) return 'Maintenance';
        if (room.isAvailable === true) return 'Available';
        return 'Occupied';
      } catch (e) { return null; }
    })();

    const statusClass = getStatusBadgeClass(status);
    const statusText = getStatusText(status);
    // If not authenticated, disable action links to prevent API calls from unauthed users
  const disabledAttr = __isAuthenticated ? '' : ' aria-disabled="true" onclick="alert(\'Yêu cầu đăng nhập/ quyền Admin\')"';

  // Build action links according to permissions
  const viewLink = `<a class="dropdown-item" href="javascript:void(0);" ${disabledAttr} onclick="handleViewRoom(${room.roomId})"><i class="bx bx-show me-1"></i> Xem Chi Tiết</a>`;
  const editLink = canEdit() ? `<a class="dropdown-item" href="javascript:void(0);" ${disabledAttr} onclick="handleEditRoom(${room.roomId})"><i class="bx bx-edit-alt me-1"></i> Chỉnh Sửa</a>` : '';
  const deleteLink = canDelete() ? `<a class="dropdown-item text-danger" href="javascript:void(0);" ${disabledAttr} onclick="handleDeleteRoom(${room.roomId})"><i class="bx bx-trash me-1"></i> Xóa</a>` : '';

    return `
      <tr>
        <td><strong>${room.roomNumber}</strong></td>
        <td>${room.roomType}</td>
        <td>${formatCurrency(room.pricePerNight)}</td>
        <td><span class="badge bg-label-info">${room.maxOccupancy} người</span></td>
        <td><span class="badge ${statusClass}">${statusText}</span></td>
        <td>
          <div class="dropdown">
            <button type="button" class="btn p-0 dropdown-toggle hide-arrow" data-bs-toggle="dropdown">
              <i class="bx bx-dots-vertical-rounded"></i>
            </button>
            <div class="dropdown-menu">
              ${viewLink}
              ${editLink}
              ${deleteLink}
            </div>
          </div>
        </td>
      </tr>
    `;
  }).join('');

  tbody.innerHTML = rows;
};

/**
 * Filter phòng theo status, type và search
 */
const filterRooms = () => {
  const statusFilter = document.getElementById('statusFilter').value;
  const typeFilter = document.getElementById('typeFilter').value;
  const searchQuery = document.getElementById('searchInput').value.toLowerCase();

  let filteredRooms = allRooms;

  // Filter by status
  if (statusFilter) {
    filteredRooms = filteredRooms.filter(room => room.status === statusFilter);
  }

  // Filter by type
  if (typeFilter) {
    filteredRooms = filteredRooms.filter(room => room.roomType === typeFilter);
  }

  // Filter by search query
  if (searchQuery) {
    filteredRooms = filteredRooms.filter(room => 
      room.roomNumber.toLowerCase().includes(searchQuery) ||
      room.roomType.toLowerCase().includes(searchQuery) ||
      (room.description && room.description.toLowerCase().includes(searchQuery))
    );
  }

  displayRooms(filteredRooms);
};

/**
 * Xử lý submit form thêm/sửa phòng
 */
const handleRoomFormSubmit = async (e) => {
  e.preventDefault();
  
  try {
    const roomData = {
      roomNumber: document.getElementById('roomNumber').value,
      roomType: document.getElementById('roomType').value,
      pricePerNight: parseFloat(document.getElementById('pricePerNight').value),
      maxOccupancy: parseInt(document.getElementById('maxOccupancy').value),
      floor: parseInt(document.getElementById('floor').value) || 1,
      status: document.getElementById('status').value,
      description: document.getElementById('description').value
    };

    let result;
    if (currentEditRoomId) {
      // Update existing room
      result = await updateRoom(currentEditRoomId, roomData);
      showSuccess('Cập nhật phòng thành công!');
    } else {
      // Create new room
      result = await createRoom(roomData);
      showSuccess('Thêm phòng mới thành công!');
    }

    // Close modal
    const modal = bootstrap.Modal.getInstance(document.getElementById('roomModal'));
    modal.hide();

    // Reload rooms
    await loadRooms();

  } catch (error) {
    console.error('Error saving room:', error);
    showError('Không thể lưu phòng: ' + error.message);
  }
};

/**
 * Xử lý xem chi tiết phòng
 */
const handleViewRoom = async (roomId) => {
  try {
    const room = await getRoomById(roomId);
    
    if (room) {
      const message = `
        Số Phòng: ${room.roomNumber}
        Loại Phòng: ${room.roomType}
        Giá/Đêm: ${formatCurrency(room.pricePerNight)}
        Sức Chứa: ${room.maxOccupancy} người
        Tầng: ${room.floor}
        Trạng Thái: ${getStatusText(room.status)}
        ${room.description ? '\nMô Tả: ' + room.description : ''}
      `;
      alert(message);
    }
  } catch (error) {
    console.error('Error viewing room:', error);
    showError('Không thể tải thông tin phòng');
  }
};

/**
 * Xử lý chỉnh sửa phòng
 */
const handleEditRoom = async (roomId) => {
  try {
    const room = await getRoomById(roomId);
    
    if (room) {
      currentEditRoomId = roomId;
      
      // Điền dữ liệu vào form
      document.getElementById('roomId').value = room.roomId;
      document.getElementById('roomNumber').value = room.roomNumber;
      document.getElementById('roomType').value = room.roomType;
      document.getElementById('pricePerNight').value = room.pricePerNight;
      document.getElementById('maxOccupancy').value = room.maxOccupancy;
      document.getElementById('floor').value = room.floor;
      document.getElementById('status').value = room.status;
      document.getElementById('description').value = room.description || '';
      
      // Cập nhật tiêu đề modal
      document.getElementById('roomModalTitle').textContent = 'Chỉnh Sửa Phòng';
      
      // Hiển thị modal
      const modal = new bootstrap.Modal(document.getElementById('roomModal'));
      modal.show();
    }
  } catch (error) {
    console.error('Error editing room:', error);
    showError('Không thể tải thông tin phòng');
  }
};

/**
 * Xử lý xóa phòng
 */
const handleDeleteRoom = async (roomId) => {
  if (!confirm('Bạn có chắc muốn xóa phòng này?')) {
    return;
  }

  try {
    await deleteRoom(roomId);
    showSuccess('Xóa phòng thành công!');
    await loadRooms();
  } catch (error) {
    console.error('Error deleting room:', error);
    showError('Không thể xóa phòng: ' + error.message);
  }
};

/**
 * Reset form phòng
 */
const resetRoomForm = () => {
  document.getElementById('roomForm').reset();
  document.getElementById('roomId').value = '';
  currentEditRoomId = null;
};

