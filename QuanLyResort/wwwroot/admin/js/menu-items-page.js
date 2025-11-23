/**
 * Menu Items Management Page
 * Quản lý món ăn (Restaurant Services)
 */

let menuItemsTable;
let isEditMode = false;

// Initialize page
document.addEventListener('DOMContentLoaded', () => {
  const token = localStorage.getItem('token');
  if (!token) {
    window.location.href = '/admin/html/login.html';
    return;
  }

  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const userRole = payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'Unknown';
    document.getElementById('userFullName').textContent = payload.unique_name || payload.name || 'Admin';
    document.getElementById('userRole').textContent = userRole;
    
    // Check if user has required role (Admin or Manager)
    const allowedRoles = ['Admin', 'Manager'];
    if (!allowedRoles.includes(userRole)) {
      alert(`Bạn không có quyền truy cập trang này. Yêu cầu role: ${allowedRoles.join(', ')}`);
      window.location.href = '/admin/html/dashboard.html';
      return;
    }
    
    console.log('[menu-items] User role:', userRole);
  } catch (e) {
    console.error('Error parsing token:', e);
    alert('Token không hợp lệ. Vui lòng đăng nhập lại.');
    localStorage.removeItem('token');
    window.location.href = '/admin/html/login.html';
    return;
  }

  // Load menu items
  loadMenuItems();
  
  // Setup image preview
  document.getElementById('imageFile').addEventListener('change', function(e) {
    const file = e.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = function(e) {
        document.getElementById('previewImg').src = e.target.result;
        document.getElementById('imagePreview').style.display = 'block';
        document.getElementById('currentImage').style.display = 'none';
      };
      reader.readAsDataURL(file);
    }
  });
});

/**
 * Load menu items from API
 */
async function loadMenuItems() {
  try {
    console.log('[loadMenuItems] Starting to load menu items...');
    showPageLoading();
    
    const search = document.getElementById('filterSearch')?.value || '';
    const isActive = document.getElementById('filterStatus')?.value || '';
    const unit = document.getElementById('filterUnit')?.value || '';
    
    let url = '/services?type=Restaurant';
    const params = new URLSearchParams();
    if (search) params.append('search', search);
    if (isActive) params.append('isActive', isActive);
    if (params.toString()) url += '&' + params.toString();
    else url += '&' + params.toString();

    console.log('[loadMenuItems] Request URL:', url);
    const services = await apiGet(url);
    
    console.log('[loadMenuItems] ✅ Success! Received services:', services?.length || 0);
    
    // Filter by unit if specified
    let filteredServices = services || [];
    if (unit) {
      filteredServices = filteredServices.filter(s => s.unit === unit);
    }
    
    const tbody = document.querySelector('#menuItemsTable tbody');
    if (!tbody) {
      console.error('[loadMenuItems] ❌ Table tbody not found!');
      return;
    }
    
    tbody.innerHTML = '';
    
    if (filteredServices && filteredServices.length > 0) {
        filteredServices.forEach(service => {
          const imageUrl = service.imageUrl 
            ? `<img src="${service.imageUrl}" alt="${service.serviceName}" style="width: 60px; height: 60px; object-fit: cover; border-radius: 4px;">`
            : '<span class="text-muted">Chưa có ảnh</span>';
        
          // Badge với indicator rõ ràng hơn
          const statusBadge = service.isActive 
            ? '<span class="badge bg-success"><i class="bx bx-check-circle me-1"></i>Đang hiển thị</span>'
            : '<span class="badge bg-secondary"><i class="bx bx-hide me-1"></i>Đã ẩn</span>';
          
          // Thêm indicator trong tên món nếu đang active
          const serviceNameDisplay = service.isActive 
            ? `<strong>${escapeHtml(service.serviceName)}</strong> <small class="text-success">(Đang hiển thị)</small>`
            : `<strong>${escapeHtml(service.serviceName)}</strong> <small class="text-muted">(Đã ẩn)</small>`;
        
          const row = `
            <tr ${service.isActive ? '' : 'style="opacity: 0.7;"'}>
              <td>${imageUrl}</td>
              <td>${serviceNameDisplay}</td>
              <td>${escapeHtml(service.description || '')}</td>
              <td><strong>${formatVND(service.price)}</strong></td>
              <td>${escapeHtml(service.unit || 'Unit')}</td>
              <td>${statusBadge}</td>
              <td>
                <div class="action-buttons">
                  <button class="action-btn action-btn-edit" onclick="editMenuItem(${service.serviceId})" title="Sửa">
                    <i class="bx bx-edit"></i>
                  </button>
                  <button class="action-btn action-btn-toggle" onclick="toggleMenuItemStatus(${service.serviceId}, ${!service.isActive})" title="${service.isActive ? 'Ẩn khỏi menu' : 'Hiển thị trên menu'}">
                    <i class="bx bx-${service.isActive ? 'hide' : 'show'}"></i>
                  </button>
                  <button class="action-btn action-btn-delete" onclick="deleteMenuItem(${service.serviceId})" title="Xóa">
                    <i class="bx bx-trash"></i>
                  </button>
                </div>
              </td>
            </tr>
          `;
          tbody.innerHTML += row;
        });
    }
    
    // Destroy existing DataTable before re-initializing
    if (menuItemsTable) {
      try {
        menuItemsTable.destroy();
        menuItemsTable = null;
      } catch (e) {
        console.warn('[loadMenuItems] ⚠️ Error destroying DataTable:', e);
      }
    }
    
    // Show empty message if no services
    if (!filteredServices || filteredServices.length === 0) {
      const emptyRow = `
        <tr>
          <td colspan="7" class="text-center py-5 text-muted">
            <i class="bx bx-info-circle me-2"></i>
            Chưa có món ăn nào
          </td>
        </tr>
      `;
      tbody.innerHTML = emptyRow;
      console.log('[loadMenuItems] ℹ️ No menu items found');
      return; // Don't initialize DataTable for empty state
    }
    
    // Re-initialize DataTable AFTER tbody content is set
    setTimeout(() => {
      try {
        if ($.fn.DataTable.isDataTable('#menuItemsTable')) {
          $('#menuItemsTable').DataTable().destroy();
        }
        
        // Verify table has valid rows before initializing
        const rows = tbody.querySelectorAll('tr');
        let hasValidRows = false;
        rows.forEach(row => {
          const cells = row.querySelectorAll('td');
          if (cells.length === 7 && !row.querySelector('td[colspan]')) {
            hasValidRows = true;
          }
        });
        
        if (hasValidRows) {
          menuItemsTable = $('#menuItemsTable').DataTable({
            order: [[1, 'asc']], // Sort by name
            pageLength: 25,
            responsive: true,
            autoWidth: false,
            language: {
              url: '../local-plugins/datatables/i18n/vi.json'
            },
            drawCallback: function() {
              // Khởi tạo lại dropdown sau khi DataTable render
              if (window.initializeDropdowns) {
                window.initializeDropdowns();
              }
            }
          });
          console.log('[loadMenuItems] ✅ DataTable initialized');
          
          // Khởi tạo dropdown sau khi DataTable đã render
          setTimeout(() => {
            if (window.initializeDropdowns) {
              window.initializeDropdowns();
            }
          }, 200);
        }
      } catch (e) {
        console.error('[loadMenuItems] ❌ Error initializing DataTable:', e);
      }
    }, 150);
    
  } catch (error) {
    console.error('[loadMenuItems] ❌ Error loading menu items:', error);
    if (typeof showToast === 'function') {
      showToast('Lỗi khi tải danh sách món ăn: ' + error.message, 'danger');
    } else {
      alert('Lỗi khi tải danh sách món ăn: ' + error.message);
    }
  } finally {
    hidePageLoading();
  }
}

/**
 * Open create modal
 */
function openCreateModal() {
  isEditMode = false;
  document.getElementById('modalTitle').textContent = 'Thêm món mới';
  document.getElementById('menuItemForm').reset();
  document.getElementById('menuItemId').value = '';
  document.getElementById('imagePreview').style.display = 'none';
  document.getElementById('currentImage').style.display = 'none';
  document.getElementById('imageFile').value = '';
  new bootstrap.Modal(document.getElementById('menuItemModal')).show();
}

/**
 * Edit menu item
 */
async function editMenuItem(id) {
  try {
    console.log('[editMenuItem] Loading service:', id);
    const service = await apiGet(`/services/${id}`);
    
    isEditMode = true;
    document.getElementById('modalTitle').textContent = 'Sửa món ăn';
    document.getElementById('menuItemId').value = service.serviceId;
    document.getElementById('serviceName').value = service.serviceName || '';
    document.getElementById('description').value = service.description || '';
    document.getElementById('price').value = service.price || 0;
    document.getElementById('unit').value = service.unit || 'Phần';
    document.getElementById('isActive').value = service.isActive ? 'true' : 'false';
    
    // Show current image if exists
    if (service.imageUrl) {
      document.getElementById('currentImg').src = service.imageUrl;
      document.getElementById('currentImage').style.display = 'block';
      document.getElementById('imagePreview').style.display = 'none';
    } else {
      document.getElementById('currentImage').style.display = 'none';
      document.getElementById('imagePreview').style.display = 'none';
    }
    
    new bootstrap.Modal(document.getElementById('menuItemModal')).show();
  } catch (error) {
    console.error('[editMenuItem] Error:', error);
    if (typeof showToast === 'function') {
      showToast('Lỗi khi tải thông tin món ăn: ' + error.message, 'danger');
    } else {
      alert('Lỗi khi tải thông tin món ăn: ' + error.message);
    }
  }
}

/**
 * Save menu item (create or update)
 */
async function saveMenuItem() {
  try {
    const form = document.getElementById('menuItemForm');
    if (!form.checkValidity()) {
      form.reportValidity();
      return;
    }
    
    const id = document.getElementById('menuItemId').value;
    const serviceName = document.getElementById('serviceName').value.trim();
    const description = document.getElementById('description').value.trim();
    const price = parseFloat(document.getElementById('price').value);
    const unit = document.getElementById('unit').value;
    const isActive = document.getElementById('isActive').value === 'true';
    const imageFile = document.getElementById('imageFile').files[0];
    
    // Validate
    if (!serviceName) {
      alert('Vui lòng nhập tên món');
      return;
    }
    
    if (price < 0) {
      alert('Giá phải lớn hơn hoặc bằng 0');
      return;
    }
    
    const serviceData = {
      serviceId: id ? parseInt(id) : 0,
      serviceName: serviceName,
      serviceType: 'Restaurant', // Always Restaurant for menu items
      description: description || null,
      price: price,
      unit: unit,
      isActive: isActive
    };
    
    let savedService;
    if (isEditMode && id) {
      // Update
      console.log('[saveMenuItem] Updating service:', id);
      savedService = await apiPut(`/services/${id}`, serviceData);
      if (typeof showToast === 'function') {
        showToast('Cập nhật món ăn thành công!', 'success');
      } else {
        alert('Cập nhật món ăn thành công!');
      }
    } else {
      // Create
      console.log('[saveMenuItem] Creating new service');
      savedService = await apiPost('/services', serviceData);
      if (typeof showToast === 'function') {
        showToast('Thêm món ăn thành công!', 'success');
      } else {
        alert('Thêm món ăn thành công!');
      }
    }
    
    // Upload image if provided
    if (imageFile) {
      try {
        const formData = new FormData();
        formData.append('file', imageFile);
        
        const serviceId = savedService?.serviceId || id;
        if (serviceId) {
          await apiPostFormData(`/services/${serviceId}/upload-image`, formData);
          console.log('[saveMenuItem] ✅ Image uploaded');
        }
      } catch (imageError) {
        console.warn('[saveMenuItem] ⚠️ Error uploading image:', imageError);
        // Don't fail the whole operation if image upload fails
      }
    }
    
    // Close modal and reload
    bootstrap.Modal.getInstance(document.getElementById('menuItemModal')).hide();
    loadMenuItems();
    
  } catch (error) {
    console.error('[saveMenuItem] Error:', error);
    if (typeof showToast === 'function') {
      showToast('Lỗi khi lưu món ăn: ' + error.message, 'danger');
    } else {
      alert('Lỗi khi lưu món ăn: ' + error.message);
    }
  }
}

/**
 * Toggle menu item status
 */
async function toggleMenuItemStatus(id, newStatus) {
  const action = newStatus ? 'hiển thị' : 'ẩn';
  const message = newStatus 
    ? 'Món ăn này sẽ xuất hiện trên trang nhà hàng của khách hàng. Bạn có chắc muốn hiển thị món ăn này?'
    : 'Món ăn này sẽ bị ẩn khỏi trang nhà hàng của khách hàng. Bạn có chắc muốn ẩn món ăn này?';
    
  if (!confirm(message)) {
    return;
  }
  
  try {
    await apiPatch(`/services/${id}/toggle-active`);
    if (typeof showToast === 'function') {
      showToast(`Đã ${action} món ăn thành công! ${newStatus ? 'Món ăn đang hiển thị cho khách hàng.' : 'Món ăn đã được ẩn khỏi menu.'}`, 'success');
    } else {
      alert(`Đã ${action} món ăn thành công!`);
    }
    loadMenuItems();
  } catch (error) {
    console.error('[toggleMenuItemStatus] Error:', error);
    if (typeof showToast === 'function') {
      showToast('Lỗi khi thay đổi trạng thái: ' + error.message, 'danger');
    } else {
      alert('Lỗi khi thay đổi trạng thái: ' + error.message);
    }
  }
}

/**
 * Delete menu item
 */
async function deleteMenuItem(id) {
  if (!confirm('Bạn có chắc muốn xóa món ăn này? Hành động này không thể hoàn tác!')) {
    return;
  }
  
  try {
    await apiDelete(`/services/${id}`);
    if (typeof showToast === 'function') {
      showToast('Xóa món ăn thành công!', 'success');
    } else {
      alert('Xóa món ăn thành công!');
    }
    loadMenuItems();
  } catch (error) {
    console.error('[deleteMenuItem] Error:', error);
    if (typeof showToast === 'function') {
      showToast('Lỗi khi xóa món ăn: ' + error.message, 'danger');
    } else {
      alert('Lỗi khi xóa món ăn: ' + error.message);
    }
  }
}

/**
 * Remove image preview
 */
function removeImage() {
  document.getElementById('imageFile').value = '';
  document.getElementById('imagePreview').style.display = 'none';
  document.getElementById('currentImage').style.display = 'none';
}

/**
 * Apply filters
 */
function applyFilters() {
  loadMenuItems();
}

/**
 * Format VND currency
 */
function formatVND(amount) {
  return new Intl.NumberFormat('vi-VN', { 
    style: 'currency', 
    currency: 'VND' 
  }).format(amount || 0);
}

/**
 * Escape HTML to prevent XSS
 */
function escapeHtml(text) {
  if (!text) return '';
  const div = document.createElement('div');
  div.textContent = text;
  return div.innerHTML;
}

