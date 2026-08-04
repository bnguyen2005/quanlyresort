/**
 * Admin Login JavaScript
 * Xử lý logic đăng nhập cho admin
 */

document.addEventListener('DOMContentLoaded', () => {
  console.log('[Admin Login] Page loaded');

  // Setup form submit handler
  const form = document.getElementById('formAuthentication');
  if (!form) {
    console.error('[Admin Login] Form not found!');
    return;
  }
  form.addEventListener('submit', handleLogin);

  // Setup password toggle
  const togglePassword = document.getElementById('togglePassword');
  const passwordInput = document.getElementById('password');
  
  if (togglePassword && passwordInput) {
    togglePassword.addEventListener('click', () => {
      const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
      passwordInput.setAttribute('type', type);
      
      const icon = togglePassword.querySelector('i');
      if (type === 'password') {
        icon.classList.remove('bx-show');
        icon.classList.add('bx-hide');
      } else {
        icon.classList.remove('bx-hide');
        icon.classList.add('bx-show');
      }
    });
  }
});

/**
 * Xử lý đăng nhập
 */
const handleLogin = async (e) => {
  e.preventDefault();
  
  console.log('[Admin Login] Form submitted');
  
  const username = document.getElementById('username').value.trim();
  const password = document.getElementById('password').value;
  
  console.log('[Admin Login] Username:', username);
  console.log('[Admin Login] Password length:', password.length);
  
  // Validation
  if (!username || !password) {
    console.log('[Admin Login] Validation failed - empty fields');
    showErrorMessage('Vui lòng nhập tên đăng nhập và mật khẩu');
    return;
  }

  // Show loading
  setLoading(true);
  hideErrorMessage();

  try {
    console.log('[Admin Login] Calling adminLogin API...');
    const result = await adminLogin(username, password);
    
    console.log('[Admin Login] API result:', result);
    
    if (result && result.token) {
      // Đăng nhập thành công
      console.log('[Admin Login] Login successful!');
      console.log('[Admin Login] User role:', result.user?.role);
      
      showSuccessMessage('Đăng nhập thành công! Đang chuyển hướng...');
      
      // Cập nhật navbar ngay lập tức trước khi redirect
      if (typeof updateNavbarAuth === 'function') {
        console.log('[Admin Login] Updating navbar immediately');
        updateNavbarAuth();
      }
      
      // Redirect ngay lập tức
      const redirectUrl = (result.user && result.user.role === 'Admin') 
        ? '/admin/html/index.html' 
        : '/customer/index.html';
      
      console.log('[Admin Login] Redirecting to:', redirectUrl);
      
      setTimeout(() => {
        window.location.href = redirectUrl;
      }, 500);
    } else {
      // Đăng nhập thất bại
      console.log('[Admin Login] Login failed - no token');
      showErrorMessage('Đăng nhập thất bại. Vui lòng kiểm tra lại tên đăng nhập và mật khẩu.');
      setLoading(false);
    }
  } catch (error) {
    console.error('[Admin Login] Error:', error);
    showErrorMessage('Đã có lỗi xảy ra: ' + (error.message || 'Không thể kết nối đến server'));
    setLoading(false);
  }
};

/**
 * Hiển thị loading state
 */
const setLoading = (isLoading) => {
  const btnLogin = document.getElementById('btnLogin');
  const btnLoginText = document.getElementById('btnLoginText');
  const btnLoginSpinner = document.getElementById('btnLoginSpinner');
  const usernameInput = document.getElementById('username');
  const passwordInput = document.getElementById('password');

  if (isLoading) {
    btnLogin.disabled = true;
    btnLoginText.classList.add('d-none');
    btnLoginSpinner.classList.remove('d-none');
    usernameInput.disabled = true;
    passwordInput.disabled = true;
  } else {
    btnLogin.disabled = false;
    btnLoginText.classList.remove('d-none');
    btnLoginSpinner.classList.add('d-none');
    usernameInput.disabled = false;
    passwordInput.disabled = false;
  }
};

/**
 * Hiển thị thông báo lỗi
 */
const showErrorMessage = (message) => {
  const errorDiv = document.getElementById('errorMessage');
  errorDiv.textContent = message;
  errorDiv.classList.remove('d-none');
  errorDiv.classList.remove('alert-success');
  errorDiv.classList.add('alert-danger');
};

/**
 * Hiển thị thông báo thành công
 */
const showSuccessMessage = (message) => {
  const errorDiv = document.getElementById('errorMessage');
  errorDiv.textContent = message;
  errorDiv.classList.remove('d-none');
  errorDiv.classList.remove('alert-danger');
  errorDiv.classList.add('alert-success');
};

/**
 * Hiển thị thông báo thông tin
 */
const showInfoMessage = (message) => {
  const errorDiv = document.getElementById('errorMessage');
  errorDiv.textContent = message;
  errorDiv.classList.remove('d-none');
  errorDiv.classList.remove('alert-danger');
  errorDiv.classList.remove('alert-success');
  errorDiv.classList.add('alert-info');
};

/**
 * Ẩn thông báo lỗi
 */
const hideErrorMessage = () => {
  const errorDiv = document.getElementById('errorMessage');
  errorDiv.classList.add('d-none');
};

