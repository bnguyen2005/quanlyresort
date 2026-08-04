/**
 * Common Validation and Toast Loader
 * Tự động load toast và validation cho tất cả các trang
 */

(function() {
  'use strict';
  
  // Load toast notification
  function loadToast() {
    if (window.showToast) return; // Already loaded
    
    const script = document.createElement('script');
    script.src = '/js/toast-notification.js';
    script.onerror = () => console.warn('Failed to load toast-notification.js');
    document.head.appendChild(script);
  }
  
  // Load form validation
  function loadValidation() {
    if (window.Validation) return; // Already loaded
    
    const script = document.createElement('script');
    script.src = '/js/form-validation.js';
    script.onerror = () => console.warn('Failed to load form-validation.js');
    document.head.appendChild(script);
  }
  
  // Load confirm dialog
  function loadConfirm() {
    if (window.showConfirm) return; // Already loaded
    
    const script = document.createElement('script');
    script.src = '/js/confirm-dialog.js';
    script.onerror = () => console.warn('Failed to load confirm-dialog.js');
    document.head.appendChild(script);
  }
  
  // Auto-load on DOM ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
      loadToast();
      loadValidation();
      loadConfirm();
    });
  } else {
    loadToast();
    loadValidation();
    loadConfirm();
  }
  
  // Helper to replace alert globally (optional, can be disabled)
  window.replaceAlerts = function() {
    if (window.originalAlert) return; // Already replaced
    
    window.originalAlert = window.alert;
    window.alert = function(message) {
      console.warn('alert() called, using toast instead');
      if (window.showToast) {
        showToast(message, 'info', 4000);
      } else {
        window.originalAlert(message);
      }
    };
  };
  
  // Helper to replace confirm globally (optional)
  window.replaceConfirms = function() {
    if (window.originalConfirm) return; // Already replaced
    
    window.originalConfirm = window.confirm;
    window.confirm = function(message) {
      console.warn('confirm() called, using showConfirm instead');
      if (window.showConfirm) {
        return showConfirm(message, 'Xác nhận');
      } else {
        return window.originalConfirm(message);
      }
    };
  };
})();

