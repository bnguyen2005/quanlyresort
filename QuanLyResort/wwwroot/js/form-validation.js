/**
 * Form Validation Utilities
 * Hệ thống validation chung cho tất cả form
 */

(function() {
  'use strict';

  const Validation = {
    /**
     * Validate một input field
     */
    validateInput: function(input) {
      if (!input) return { valid: false, message: 'Input không tồn tại' };
      
      const label = input.getAttribute('data-label') || 
                   input.getAttribute('aria-label') ||
                   (input.labels && input.labels[0] ? input.labels[0].textContent : '') ||
                   input.placeholder ||
                   input.name ||
                   'Trường này';
      
      const value = input.type === 'checkbox' ? input.checked : (input.value || '').trim();
      const type = (input.type || '').toLowerCase();
      const required = input.hasAttribute('required');
      
      // Required check
      if (required) {
        if (type === 'checkbox' && !value) {
          return { valid: false, message: `${label} là bắt buộc` };
        }
        if (type !== 'checkbox' && !value) {
          return { valid: false, message: `${label} là bắt buộc` };
        }
      }
      
      // Skip further validation if empty and not required
      if (!value && !required) {
        return { valid: true, message: '' };
      }
      
      // Email validation
      if (type === 'email' && value) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(value)) {
          return { valid: false, message: `${label} không đúng định dạng email` };
        }
      }
      
      // Phone validation
      if ((type === 'tel' || input.name?.toLowerCase().includes('phone')) && value) {
        const phoneRegex = /^[\d\s\-\+\(\)]+$/;
        if (!phoneRegex.test(value) || value.replace(/\D/g, '').length < 10) {
          return { valid: false, message: `${label} phải có ít nhất 10 chữ số` };
        }
      }
      
      // Min length
      const minLength = input.getAttribute('minlength');
      if (minLength && value.length < parseInt(minLength)) {
        return { valid: false, message: `${label} phải có ít nhất ${minLength} ký tự` };
      }
      
      // Max length
      const maxLength = input.getAttribute('maxlength');
      if (maxLength && value.length > parseInt(maxLength)) {
        return { valid: false, message: `${label} không được vượt quá ${maxLength} ký tự` };
      }
      
      // Pattern validation
      const pattern = input.getAttribute('pattern');
      if (pattern && value) {
        try {
          const regex = new RegExp(pattern);
          if (!regex.test(value)) {
            const patternTitle = input.getAttribute('data-pattern-message') || 'không đúng định dạng';
            return { valid: false, message: `${label} ${patternTitle}` };
          }
        } catch (e) {
          console.warn('Invalid pattern:', pattern);
        }
      }
      
      // Number validation
      if ((type === 'number' || type === 'range') && value) {
        const num = parseFloat(value);
        const min = input.getAttribute('min');
        const max = input.getAttribute('max');
        
        if (isNaN(num)) {
          return { valid: false, message: `${label} phải là số` };
        }
        if (min !== null && num < parseFloat(min)) {
          return { valid: false, message: `${label} phải lớn hơn hoặc bằng ${min}` };
        }
        if (max !== null && num > parseFloat(max)) {
          return { valid: false, message: `${label} phải nhỏ hơn hoặc bằng ${max}` };
        }
      }
      
      // Date validation
      if (type === 'date' && value) {
        const date = new Date(value);
        const min = input.getAttribute('min');
        const max = input.getAttribute('max');
        
        if (isNaN(date.getTime())) {
          return { valid: false, message: `${label} không hợp lệ` };
        }
        if (min && date < new Date(min)) {
          return { valid: false, message: `${label} không được trước ${new Date(min).toLocaleDateString('vi-VN')}` };
        }
        if (max && date > new Date(max)) {
          return { valid: false, message: `${label} không được sau ${new Date(max).toLocaleDateString('vi-VN')}` };
        }
      }
      
      // URL validation
      if (type === 'url' && value) {
        try {
          new URL(value);
        } catch (e) {
          return { valid: false, message: `${label} không phải là URL hợp lệ` };
        }
      }
      
      return { valid: true, message: '' };
    },
    
    /**
     * Validate toàn bộ form
     */
    validateForm: function(form) {
      if (!form) return { valid: false, errors: [] };
      
      const inputs = form.querySelectorAll('input, textarea, select');
      const errors = [];
      
      inputs.forEach(input => {
        // Skip hidden, disabled, và submit buttons
        if (input.type === 'hidden' || input.disabled || input.type === 'submit' || input.type === 'button') {
          return;
        }
        
        const result = this.validateInput(input);
        if (!result.valid) {
          errors.push({ input, message: result.message });
          this.showError(input, result.message);
        } else {
          this.clearError(input);
        }
      });
      
      return {
        valid: errors.length === 0,
        errors: errors
      };
    },
    
    /**
     * Hiển thị lỗi cho input
     */
    showError: function(input, message) {
      if (!input) return;
      
      input.classList.add('is-invalid');
      input.classList.remove('is-valid');
      
      // Tìm hoặc tạo error message element
      let errorEl = input.parentElement?.querySelector('.invalid-feedback');
      if (!errorEl) {
        errorEl = document.createElement('div');
        errorEl.className = 'invalid-feedback';
        const parent = input.parentElement || input;
        parent.appendChild(errorEl);
      }
      
      errorEl.textContent = message;
      errorEl.style.display = 'block';
    },
    
    /**
     * Xóa lỗi của input
     */
    clearError: function(input) {
      if (!input) return;
      
      input.classList.remove('is-invalid');
      input.classList.add('is-valid');
      
      const errorEl = input.parentElement?.querySelector('.invalid-feedback');
      if (errorEl) {
        errorEl.textContent = '';
        errorEl.style.display = 'none';
      }
    },
    
    /**
     * Clear tất cả errors trong form
     */
    clearFormErrors: function(form) {
      if (!form) return;
      const inputs = form.querySelectorAll('.is-invalid');
      inputs.forEach(input => this.clearError(input));
    },
    
    /**
     * Setup real-time validation cho form
     */
    setupRealTimeValidation: function(form) {
      if (!form) return;
      
      const inputs = form.querySelectorAll('input, textarea, select');
      inputs.forEach(input => {
        // Validate on blur
        input.addEventListener('blur', () => {
          this.validateInput(input);
        });
        
        // Clear error on input
        input.addEventListener('input', () => {
          if (input.classList.contains('is-invalid')) {
            const result = this.validateInput(input);
            if (result.valid) {
              this.clearError(input);
            }
          }
        });
      });
    }
  };
  
  // Export to window
  window.Validation = Validation;
  
  // Auto-setup cho tất cả form có class 'needs-validation'
  document.addEventListener('DOMContentLoaded', function() {
    const forms = document.querySelectorAll('form.needs-validation');
    forms.forEach(form => {
      Validation.setupRealTimeValidation(form);
      
      form.addEventListener('submit', function(e) {
        const result = Validation.validateForm(form);
        if (!result.valid) {
          e.preventDefault();
          e.stopPropagation();
          
          // Show first error
          if (result.errors.length > 0) {
            const firstError = result.errors[0];
            firstError.input.focus();
            if (window.showToast) {
              showToast(result.errors[0].message, 'error');
            }
          }
          
          form.classList.add('was-validated');
          return false;
        }
      });
    });
  });
})();

