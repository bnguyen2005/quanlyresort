/**
 * Customer Register Logic
 */

document.addEventListener('DOMContentLoaded', () => {
  const form = document.getElementById('registerForm');
  form.addEventListener('submit', handleRegister);
});

/**
 * Handle register form submit
 */
const handleRegister = async (e) => {
  e.preventDefault();
  
  const fullName = document.getElementById('fullName').value.trim();
  const email = document.getElementById('email').value.trim();
  const phoneNumber = document.getElementById('phoneNumber').value.trim();
  const password = document.getElementById('password').value;
  const confirmPassword = document.getElementById('confirmPassword').value;
  const agreeTerms = document.getElementById('agreeTerms').checked;
  
  // Validation
  if (!fullName || !email || !phoneNumber || !password || !confirmPassword) {
    showErrorMessage('Vui lòng điền đầy đủ thông tin bắt buộc');
    return;
  }

  if (password !== confirmPassword) {
    showErrorMessage('Mật khẩu xác nhận không khớp');
    return;
  }

  if (password.length < 6) {
    showErrorMessage('Mật khẩu phải có ít nhất 6 ký tự');
    return;
  }

  if (!agreeTerms) {
    showErrorMessage('Bạn phải đồng ý với các điều khoản và điều kiện');
    return;
  }

  // Show loading
  setLoading(true);
  hideMessages();

  try {
    const registerData = {
      fullName,
      email,
      phoneNumber,
      username: email, // Sử dụng email làm username
      nationality: '', // Optional field
      passportNumber: '', // Optional field
      password
    };

    const result = await customerRegister(registerData);
    
    if (result) {
      showSuccessMessage('Đăng ký thành công! Đang chuyển đến trang đăng nhập...');
      
      // Redirect to login after 2 seconds
      setTimeout(() => {
        window.location.href = '/customer/login.html';
      }, 2000);
    } else {
      showErrorMessage('Đăng ký thất bại');
      setLoading(false);
    }
  } catch (error) {
    console.error('Register error:', error);
    showErrorMessage(error.message || 'Đăng ký thất bại. Vui lòng thử lại');
    setLoading(false);
  }
};

/**
 * Set loading state
 */
const setLoading = (isLoading) => {
  const btn = document.getElementById('btnRegister');
  const btnText = document.getElementById('btnText');
  const btnSpinner = document.getElementById('btnSpinner');
  const inputs = document.querySelectorAll('input');

  if (isLoading) {
    btn.disabled = true;
    btnText.classList.add('d-none');
    btnSpinner.classList.remove('d-none');
    inputs.forEach(input => input.disabled = true);
  } else {
    btn.disabled = false;
    btnText.classList.remove('d-none');
    btnSpinner.classList.add('d-none');
    inputs.forEach(input => input.disabled = false);
  }
};

/**
 * Show error message
 */
const showErrorMessage = (message) => {
  const errorDiv = document.getElementById('errorMessage');
  errorDiv.textContent = message;
  errorDiv.classList.remove('d-none');
  
  // Scroll to top
  window.scrollTo({ top: 0, behavior: 'smooth' });
};

/**
 * Show success message
 */
const showSuccessMessage = (message) => {
  const successDiv = document.getElementById('successMessage');
  successDiv.textContent = message;
  successDiv.classList.remove('d-none');
  
  // Scroll to top
  window.scrollTo({ top: 0, behavior: 'smooth' });
};

/**
 * Hide all messages
 */
const hideMessages = () => {
  document.getElementById('errorMessage').classList.add('d-none');
  document.getElementById('successMessage').classList.add('d-none');
};

