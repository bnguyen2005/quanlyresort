/**
 * Toast Notification System
 * Thay thế alert() bằng toast notification đẹp hơn
 */

(function() {
  'use strict';

  // Tạo container cho toast nếu chưa có
  function ensureToastContainer() {
    let container = document.getElementById('toast-container-global');
    if (!container) {
      container = document.createElement('div');
      container.id = 'toast-container-global';
      container.setAttribute('aria-live', 'polite');
      container.setAttribute('aria-atomic', 'true');
      container.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        max-width: 400px;
        width: 100%;
        pointer-events: none;
      `;
      document.body.appendChild(container);
    }
    return container;
  }

  /**
   * Hiển thị toast notification
   * @param {string} message - Nội dung thông báo
   * @param {string} type - Loại: 'success', 'error', 'warning', 'info'
   * @param {number} duration - Thời gian hiển thị (ms), mặc định 3000
   */
  window.showToast = function(message, type = 'info', duration = 3000) {
    const container = ensureToastContainer();
    
    // Tạo toast element
    const toast = document.createElement('div');
    toast.className = 'toast-notification';
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    
    // Màu sắc và icon theo type
    const config = {
      success: {
        bg: '#10b981',
        icon: '✓',
        title: 'Thành công'
      },
      error: {
        bg: '#ef4444',
        icon: '✕',
        title: 'Lỗi'
      },
      warning: {
        bg: '#f59e0b',
        icon: '⚠',
        title: 'Cảnh báo'
      },
      info: {
        bg: '#3b82f6',
        icon: 'ℹ',
        title: 'Thông tin'
      }
    };
    
    const cfg = config[type] || config.info;
    
    toast.style.cssText = `
      background: white;
      border-left: 4px solid ${cfg.bg};
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
      padding: 16px 20px;
      margin-bottom: 12px;
      display: flex;
      align-items: flex-start;
      gap: 12px;
      animation: slideInRight 0.3s ease-out;
      pointer-events: auto;
      max-width: 100%;
    `;
    
    // Icon
    const iconEl = document.createElement('div');
    iconEl.style.cssText = `
      width: 24px;
      height: 24px;
      border-radius: 50%;
      background: ${cfg.bg};
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 14px;
      font-weight: bold;
      flex-shrink: 0;
    `;
    iconEl.textContent = cfg.icon;
    
    // Content
    const contentEl = document.createElement('div');
    contentEl.style.cssText = `
      flex: 1;
      min-width: 0;
    `;
    
    const titleEl = document.createElement('div');
    titleEl.style.cssText = `
      font-weight: 600;
      font-size: 14px;
      color: #1f2937;
      margin-bottom: 4px;
    `;
    titleEl.textContent = cfg.title;
    
    const messageEl = document.createElement('div');
    messageEl.style.cssText = `
      font-size: 13px;
      color: #6b7280;
      line-height: 1.5;
      word-wrap: break-word;
    `;
    messageEl.textContent = message;
    
    contentEl.appendChild(titleEl);
    contentEl.appendChild(messageEl);
    
    // Close button
    const closeBtn = document.createElement('button');
    closeBtn.innerHTML = '×';
    closeBtn.style.cssText = `
      background: none;
      border: none;
      font-size: 20px;
      color: #9ca3af;
      cursor: pointer;
      padding: 0;
      width: 20px;
      height: 20px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      transition: color 0.2s;
    `;
    closeBtn.onmouseover = () => closeBtn.style.color = '#374151';
    closeBtn.onmouseout = () => closeBtn.style.color = '#9ca3af';
    closeBtn.onclick = () => removeToast(toast);
    
    toast.appendChild(iconEl);
    toast.appendChild(contentEl);
    toast.appendChild(closeBtn);
    
    container.appendChild(toast);
    
    // Auto remove sau duration
    if (duration > 0) {
      setTimeout(() => {
        removeToast(toast);
      }, duration);
    }
    
    return toast;
  };

  function removeToast(toast) {
    if (!toast || !toast.parentElement) return;
    toast.style.animation = 'slideOutRight 0.3s ease-out';
    setTimeout(() => {
      if (toast.parentElement) {
        toast.parentElement.removeChild(toast);
      }
    }, 300);
  }

  // Thêm CSS animation nếu chưa có
  if (!document.getElementById('toast-animations')) {
    const style = document.createElement('style');
    style.id = 'toast-animations';
    style.textContent = `
      @keyframes slideInRight {
        from {
          transform: translateX(100%);
          opacity: 0;
        }
        to {
          transform: translateX(0);
          opacity: 1;
        }
      }
      @keyframes slideOutRight {
        from {
          transform: translateX(0);
          opacity: 1;
        }
        to {
          transform: translateX(100%);
          opacity: 0;
        }
      }
    `;
    document.head.appendChild(style);
  }

  // Thay thế window.alert nếu cần
  if (typeof window.originalAlert === 'undefined') {
    window.originalAlert = window.alert;
    window.alert = function(message) {
      console.warn('alert() called, consider using showToast() instead');
      showToast(message, 'info', 4000);
    };
  }
})();

