/**
 * Universal Login Logic - Hỗ trợ cả Admin và Customer
 */

console.log('🔵 [customer-login.js] Script loaded at:', new Date().toLocaleTimeString());
console.log('🔵 [customer-login.js] Current URL:', window.location.href);

document.addEventListener('DOMContentLoaded', () => {
  console.log('🟢 [Universal Login] DOMContentLoaded fired');
  console.log('🟢 [Universal Login] Page loaded at:', new Date().toLocaleTimeString());

  const form = document.getElementById('loginForm');
  const btnLogin = document.getElementById('btnLogin');
  
  if (!form) {
    console.error('🔴 [Universal Login] Form #loginForm NOT FOUND!');
    console.log('🔴 [Universal Login] Available forms:', document.querySelectorAll('form'));
    return;
  }
  
  if (!btnLogin) {
    console.error('🔴 [Universal Login] Button #btnLogin NOT FOUND!');
    return;
  }
  
  console.log('✅ [Universal Login] Form found:', form);
  console.log('✅ [Universal Login] Button found:', btnLogin);
  
  // Method 1: Attach to button click (PREFERRED)
  btnLogin.addEventListener('click', (e) => {
    console.log('🔵 [Universal Login] Button clicked!');
    e.preventDefault();
    e.stopPropagation();
    handleLogin(e);
  });
  console.log('✅ [Universal Login] Button click handler attached');
  
  // Method 2: Attach to form submit (BACKUP)
  form.addEventListener('submit', (e) => {
    console.log('🔵 [Universal Login] Form submit event!');
    e.preventDefault();
    e.stopPropagation();
    handleLogin(e);
  }, false);
  console.log('✅ [Universal Login] Form submit handler attached');
  
  // Method 3: Backup handler
  form.onsubmit = (e) => {
    console.log('⚠️ [Universal Login] Backup onsubmit triggered');
    e.preventDefault();
    return false;
  };
});

/**
 * Handle login form submit
 */
const handleLogin = async (e) => {
  // Chặn form submit NGAY LẬP TỨC
  if (e) {
    e.preventDefault();
    e.stopPropagation();
  }
  
  console.log('🟡 [Universal Login] ===== FORM SUBMITTED =====');
  console.log('🟡 [Universal Login] Event:', e);
  console.log('🟡 [Universal Login] Time:', new Date().toLocaleTimeString());
  console.log('🟡 [Universal Login] preventDefault called!');
  
  const emailInput = document.getElementById('email');
  const passwordInput = document.getElementById('password');
  
  console.log('🟡 [Universal Login] Email input element:', emailInput);
  console.log('🟡 [Universal Login] Password input element:', passwordInput);
  
  if (!emailInput || !passwordInput) {
    console.error('🔴 [Universal Login] Input fields not found!');
    showErrorMessage('Lỗi: Không tìm thấy trường nhập liệu');
    return;
  }
  
  const email = emailInput.value.trim();
  const password = passwordInput.value;
  
  console.log('🟡 [Universal Login] Email/Username:', email);
  console.log('🟡 [Universal Login] Password length:', password.length);
  console.log('🟡 [Universal Login] Password first 3 chars:', password.substring(0, 3) + '...');
  
  // Validation
  if (!email || !password) {
    console.error('🔴 [Universal Login] Validation failed - empty fields');
    showErrorMessage('Vui lòng nhập đầy đủ thông tin');
    return;
  }

  // Show loading
  console.log('🟡 [Universal Login] Setting loading state...');
  setLoading(true);
  hideMessages();

  try {
    console.log('🟢 [Universal Login] Calling universalLogin API...');
    console.log('🟢 [Universal Login] Checking universalLogin function:', typeof universalLogin);
    
    if (typeof universalLogin !== 'function') {
      throw new Error('universalLogin function not defined!');
    }
    
    const result = await universalLogin(email, password);
    console.log('🟢 [Universal Login] universalLogin returned:', result);
    
    console.log('✅ [Universal Login] API result received');
    console.log('✅ [Universal Login] Result object:', JSON.stringify(result, null, 2));
    console.log('✅ [Universal Login] Has token:', !!result?.token);
    console.log('✅ [Universal Login] Has user:', !!result?.user);
    
    if (result && result.token) {
      console.log('✅ [Universal Login] Login successful!');
      console.log('✅ [Universal Login] Token:', result.token.substring(0, 20) + '...');
      console.log('✅ [Universal Login] User object:', result.user);
      console.log('✅ [Universal Login] User role:', result.user?.role);
      console.log('✅ [Universal Login] Role type:', typeof result.user?.role);
      console.log('✅ [Universal Login] Role comparison Admin:', result.user?.role === 'Admin');
      
      // Hiển thị thông báo phù hợp với role
      if (result.user && result.user.role === 'Admin') {
        console.log('✅ [Universal Login] Admin login successful!');
        showSuccessMessage('Đăng nhập Admin thành công! Đang chuyển hướng...');
      } else {
        console.log('✅ [Universal Login] Customer login successful!');
        showSuccessMessage('Đăng nhập thành công! Đang chuyển hướng...');
      }
      
      // Cập nhật navbar ngay lập tức trước khi redirect
      if (typeof updateNavbarAuth === 'function') {
        console.log('✅ [Universal Login] Updating navbar immediately');
        updateNavbarAuth();
      } else {
        console.warn('⚠️ [Universal Login] updateNavbarAuth function not found');
      }
      
      // Redirect based on role
      const redirectUrl = (result.user && result.user.role === 'Admin') 
        ? '/admin/html/index.html' 
        : 'index.html';
      
      console.log('✅ [Universal Login] Redirect URL determined:', redirectUrl);
      console.log('✅ [Universal Login] Will redirect in 1 second...');
      
      setTimeout(() => {
        console.log('✅ [Universal Login] Executing redirect NOW to:', redirectUrl);
        window.location.href = redirectUrl;
      }, 1000);
    } else {
      console.error('🔴 [Universal Login] Login failed - no token in result');
      console.error('🔴 [Universal Login] Result was:', result);
      showErrorMessage('Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.');
      setLoading(false);
    }
  } catch (error) {
    console.error('🔴 [Universal Login] EXCEPTION caught:', error);
    console.error('🔴 [Universal Login] Error name:', error.name);
    console.error('🔴 [Universal Login] Error message:', error.message);
    console.error('🔴 [Universal Login] Error stack:', error.stack);
    showErrorMessage(error.message || 'Email hoặc mật khẩu không đúng');
    setLoading(false);
  }
};

/**
 * Set loading state
 */
const setLoading = (isLoading) => {
  const btn = document.getElementById('btnLogin');
  const btnText = document.getElementById('btnText');
  const btnSpinner = document.getElementById('btnSpinner');
  const emailInput = document.getElementById('email');
  const passwordInput = document.getElementById('password');

  if (isLoading) {
    btn.disabled = true;
    btnText.classList.add('d-none');
    btnSpinner.classList.remove('d-none');
    emailInput.disabled = true;
    passwordInput.disabled = true;
  } else {
    btn.disabled = false;
    btnText.classList.remove('d-none');
    btnSpinner.classList.add('d-none');
    emailInput.disabled = false;
    passwordInput.disabled = false;
  }
};

/**
 * Show error message
 */
const showErrorMessage = (message) => {
  const errorDiv = document.getElementById('errorMessage');
  errorDiv.textContent = message;
  errorDiv.classList.remove('d-none');
};

/**
 * Show success message
 */
const showSuccessMessage = (message) => {
  const successDiv = document.getElementById('successMessage');
  successDiv.textContent = message;
  successDiv.classList.remove('d-none');
};

/**
 * Show info message
 */
const showInfoMessage = (message) => {
  const successDiv = document.getElementById('successMessage');
  successDiv.textContent = message;
  successDiv.classList.remove('d-none');
  successDiv.style.background = '#d1ecf1';
  successDiv.style.color = '#0c5460';
  successDiv.style.borderColor = '#bee5eb';
};

/**
 * Hide all messages
 */
const hideMessages = () => {
  document.getElementById('errorMessage').classList.add('d-none');
  document.getElementById('successMessage').classList.add('d-none');
  // Reset success message style
  const successDiv = document.getElementById('successMessage');
  successDiv.style.background = '';
  successDiv.style.color = '';
  successDiv.style.borderColor = '';
};

