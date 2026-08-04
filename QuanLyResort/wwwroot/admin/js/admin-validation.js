/**
 * Admin Validation Utility
 * Hệ thống validation chung cho tất cả form trong admin
 */

(function() {
  'use strict';

  const AdminValidation = {
    /**
     * Validate email
     */
    validateEmail: function(email) {
      if (!email) return { valid: false, message: 'Email là bắt buộc' };
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(email)) {
        return { valid: false, message: 'Email không đúng định dạng' };
      }
      return { valid: true, message: '' };
    },

    /**
     * Validate phone number
     */
    validatePhone: function(phone) {
      if (!phone) return { valid: true, message: '' }; // Phone is optional
      const phoneRegex = /^[\d\s\-\+\(\)]+$/;
      const digits = phone.replace(/\D/g, '');
      if (digits.length < 10 || digits.length > 15) {
        return { valid: false, message: 'Số điện thoại phải có từ 10 đến 15 chữ số' };
      }
      return { valid: true, message: '' };
    },

    /**
     * Validate password
     */
    validatePassword: function(password, isRequired = true) {
      if (!password) {
        if (isRequired) {
          return { valid: false, message: 'Mật khẩu là bắt buộc' };
        }
        return { valid: true, message: '' };
      }
      if (password.length < 6) {
        return { valid: false, message: 'Mật khẩu phải có ít nhất 6 ký tự' };
      }
      if (password.length > 100) {
        return { valid: false, message: 'Mật khẩu không được vượt quá 100 ký tự' };
      }
      return { valid: true, message: '' };
    },

    /**
     * Validate required field
     */
    validateRequired: function(value, fieldName) {
      if (!value || (typeof value === 'string' && !value.trim())) {
        return { valid: false, message: `${fieldName} là bắt buộc` };
      }
      return { valid: true, message: '' };
    },

    /**
     * Validate number
     */
    validateNumber: function(value, fieldName, min = null, max = null) {
      if (value === null || value === undefined || value === '') {
        return { valid: false, message: `${fieldName} là bắt buộc` };
      }
      const num = parseFloat(value);
      if (isNaN(num)) {
        return { valid: false, message: `${fieldName} phải là số` };
      }
      if (min !== null && num < min) {
        return { valid: false, message: `${fieldName} phải lớn hơn hoặc bằng ${min}` };
      }
      if (max !== null && num > max) {
        return { valid: false, message: `${fieldName} phải nhỏ hơn hoặc bằng ${max}` };
      }
      return { valid: true, message: '' };
    },

    /**
     * Validate integer
     */
    validateInteger: function(value, fieldName, min = null, max = null) {
      if (value === null || value === undefined || value === '') {
        return { valid: false, message: `${fieldName} là bắt buộc` };
      }
      const num = parseInt(value);
      if (isNaN(num)) {
        return { valid: false, message: `${fieldName} phải là số nguyên` };
      }
      if (min !== null && num < min) {
        return { valid: false, message: `${fieldName} phải lớn hơn hoặc bằng ${min}` };
      }
      if (max !== null && num > max) {
        return { valid: false, message: `${fieldName} phải nhỏ hơn hoặc bằng ${max}` };
      }
      return { valid: true, message: '' };
    },

    /**
     * Validate date
     */
    validateDate: function(date, fieldName, minDate = null, maxDate = null) {
      if (!date) {
        return { valid: false, message: `${fieldName} là bắt buộc` };
      }
      const d = new Date(date);
      if (isNaN(d.getTime())) {
        return { valid: false, message: `${fieldName} không hợp lệ` };
      }
      if (minDate && d < new Date(minDate)) {
        return { valid: false, message: `${fieldName} không được trước ${new Date(minDate).toLocaleDateString('vi-VN')}` };
      }
      if (maxDate && d > new Date(maxDate)) {
        return { valid: false, message: `${fieldName} không được sau ${new Date(maxDate).toLocaleDateString('vi-VN')}` };
      }
      return { valid: true, message: '' };
    },

    /**
     * Validate date range
     */
    validateDateRange: function(startDate, endDate, startFieldName = 'Ngày bắt đầu', endFieldName = 'Ngày kết thúc') {
      const startResult = this.validateDate(startDate, startFieldName);
      if (!startResult.valid) return startResult;
      
      const endResult = this.validateDate(endDate, endFieldName);
      if (!endResult.valid) return endResult;

      if (new Date(endDate) <= new Date(startDate)) {
        return { valid: false, message: `${endFieldName} phải sau ${startFieldName}` };
      }
      return { valid: true, message: '' };
    },

    /**
     * Validate string length
     */
    validateLength: function(value, fieldName, minLength = null, maxLength = null) {
      if (!value && minLength) {
        return { valid: false, message: `${fieldName} là bắt buộc` };
      }
      const str = String(value || '');
      if (minLength !== null && str.length < minLength) {
        return { valid: false, message: `${fieldName} phải có ít nhất ${minLength} ký tự` };
      }
      if (maxLength !== null && str.length > maxLength) {
        return { valid: false, message: `${fieldName} không được vượt quá ${maxLength} ký tự` };
      }
      return { valid: true, message: '' };
    },

    /**
     * Validate URL
     */
    validateURL: function(url, fieldName = 'URL') {
      if (!url) return { valid: true, message: '' }; // URL is usually optional
      try {
        new URL(url);
        return { valid: true, message: '' };
      } catch (e) {
        return { valid: false, message: `${fieldName} không phải là URL hợp lệ` };
      }
    },

    /**
     * Validate form
     */
    validateForm: function(form, rules = {}) {
      if (!form) return { valid: false, errors: [] };

      const errors = [];
      const inputs = form.querySelectorAll('input, textarea, select');

      inputs.forEach(input => {
        // Skip hidden, disabled, và submit buttons
        if (input.type === 'hidden' || input.disabled || input.type === 'submit' || input.type === 'button') {
          return;
        }

        const fieldName = input.getAttribute('data-label') || 
                         (input.labels && input.labels[0] ? input.labels[0].textContent : '') ||
                         input.placeholder ||
                         input.name ||
                         'Trường này';

        const value = input.type === 'checkbox' ? input.checked : (input.value || '').trim();
        const required = input.hasAttribute('required');
        const type = (input.type || '').toLowerCase();

        // Check required
        if (required) {
          if (type === 'checkbox' && !value) {
            errors.push({ input, message: `${fieldName} là bắt buộc` });
            this.showError(input, `${fieldName} là bắt buộc`);
            return;
          }
          if (type !== 'checkbox' && !value) {
            errors.push({ input, message: `${fieldName} là bắt buộc` });
            this.showError(input, `${fieldName} là bắt buộc`);
            return;
          }
        }

        // Custom rules
        if (rules[input.name || input.id]) {
          const rule = rules[input.name || input.id];
          let result = { valid: true, message: '' };

          if (rule.required && !value) {
            result = { valid: false, message: `${fieldName} là bắt buộc` };
          } else if (value) {
            if (rule.email) {
              result = this.validateEmail(value);
            } else if (rule.phone) {
              result = this.validatePhone(value);
            } else if (rule.password) {
              result = this.validatePassword(value, rule.required);
            } else if (rule.number) {
              result = this.validateNumber(value, fieldName, rule.min, rule.max);
            } else if (rule.integer) {
              result = this.validateInteger(value, fieldName, rule.min, rule.max);
            } else if (rule.date) {
              result = this.validateDate(value, fieldName, rule.minDate, rule.maxDate);
            } else if (rule.length) {
              result = this.validateLength(value, fieldName, rule.minLength, rule.maxLength);
            } else if (rule.url) {
              result = this.validateURL(value, fieldName);
            }
          }

          if (!result.valid) {
            errors.push({ input, message: result.message });
            this.showError(input, result.message);
          } else {
            this.clearError(input);
          }
        } else {
          // Default validation based on input type
          if (type === 'email' && value) {
            const result = this.validateEmail(value);
            if (!result.valid) {
              errors.push({ input, message: result.message });
              this.showError(input, result.message);
            } else {
              this.clearError(input);
            }
          } else if ((type === 'tel' || input.name?.toLowerCase().includes('phone')) && value) {
            const result = this.validatePhone(value);
            if (!result.valid) {
              errors.push({ input, message: result.message });
              this.showError(input, result.message);
            } else {
              this.clearError(input);
            }
          } else {
            this.clearError(input);
          }
        }
      });

      return {
        valid: errors.length === 0,
        errors: errors
      };
    },

    /**
     * Show error for input
     */
    showError: function(input, message) {
      input.classList.add('is-invalid');
      
      // Remove existing error message
      const existingError = input.parentElement.querySelector('.invalid-feedback');
      if (existingError) {
        existingError.remove();
      }

      // Add error message
      const errorDiv = document.createElement('div');
      errorDiv.className = 'invalid-feedback';
      errorDiv.textContent = message;
      input.parentElement.appendChild(errorDiv);
    },

    /**
     * Clear error for input
     */
    clearError: function(input) {
      input.classList.remove('is-invalid');
      const errorDiv = input.parentElement.querySelector('.invalid-feedback');
      if (errorDiv) {
        errorDiv.remove();
      }
    },

    /**
     * Clear all errors in form
     */
    clearAllErrors: function(form) {
      if (!form) return;
      const inputs = form.querySelectorAll('input, textarea, select');
      inputs.forEach(input => this.clearError(input));
    },

    /**
     * Confirm delete action
     */
    confirmDelete: function(itemName, callback) {
      if (typeof Swal !== 'undefined') {
        // Use SweetAlert2 if available
        Swal.fire({
          title: 'Xác nhận xóa',
          text: `Bạn có chắc chắn muốn xóa "${itemName}"? Hành động này không thể hoàn tác!`,
          icon: 'warning',
          showCancelButton: true,
          confirmButtonColor: '#d33',
          cancelButtonColor: '#3085d6',
          confirmButtonText: 'Xóa',
          cancelButtonText: 'Hủy',
          reverseButtons: true
        }).then((result) => {
          if (result.isConfirmed && callback) {
            callback();
          }
        });
      } else {
        // Fallback to native confirm
        if (confirm(`Bạn có chắc chắn muốn xóa "${itemName}"? Hành động này không thể hoàn tác!`)) {
          if (callback) callback();
        }
      }
    },

    /**
     * Show success message
     */
    showSuccess: function(message) {
      if (typeof showToast === 'function') {
        showToast(message, 'success');
      } else if (typeof Swal !== 'undefined') {
        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: message,
          timer: 2000,
          showConfirmButton: false
        });
      } else {
        alert(message);
      }
    },

    /**
     * Show error message
     */
    showError: function(message) {
      if (typeof showToast === 'function') {
        showToast(message, 'error');
      } else if (typeof Swal !== 'undefined') {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: message
        });
      } else {
        alert(message);
      }
    }
  };

  // Export to global scope
  window.AdminValidation = AdminValidation;
})();

