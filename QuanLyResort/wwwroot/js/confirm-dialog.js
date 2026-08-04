/**
 * Confirm Dialog System
 * Thay thế confirm() bằng dialog đẹp hơn
 */

(function() {
  'use strict';

  /**
   * Hiển thị confirm dialog
   * @param {string} message - Nội dung câu hỏi
   * @param {string} title - Tiêu đề (optional)
   * @returns {Promise<boolean>} - true nếu user chọn OK, false nếu Cancel
   */
  window.showConfirm = function(message, title = 'Xác nhận') {
    return new Promise((resolve) => {
      // Tạo modal overlay
      const overlay = document.createElement('div');
      overlay.className = 'confirm-dialog-overlay';
      overlay.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0, 0, 0, 0.5);
        z-index: 10000;
        display: flex;
        align-items: center;
        justify-content: center;
        animation: fadeIn 0.2s ease;
      `;

      // Tạo dialog box
      const dialog = document.createElement('div');
      dialog.className = 'confirm-dialog';
      dialog.style.cssText = `
        background: white;
        border-radius: 12px;
        padding: 24px;
        max-width: 400px;
        width: 90%;
        box-shadow: 0 10px 40px rgba(0,0,0,0.2);
        animation: slideUp 0.3s ease;
      `;

      // Title
      const titleEl = document.createElement('h5');
      titleEl.style.cssText = `
        margin: 0 0 16px 0;
        font-size: 18px;
        font-weight: 600;
        color: #1f2937;
      `;
      titleEl.textContent = title;

      // Message
      const messageEl = document.createElement('p');
      messageEl.style.cssText = `
        margin: 0 0 24px 0;
        font-size: 14px;
        color: #6b7280;
        line-height: 1.6;
      `;
      messageEl.textContent = message;

      // Buttons
      const buttonsEl = document.createElement('div');
      buttonsEl.style.cssText = `
        display: flex;
        gap: 12px;
        justify-content: flex-end;
      `;

      const cancelBtn = document.createElement('button');
      cancelBtn.textContent = 'Hủy';
      cancelBtn.style.cssText = `
        padding: 10px 20px;
        border: 1px solid #e5e7eb;
        background: white;
        color: #374151;
        border-radius: 8px;
        cursor: pointer;
        font-size: 14px;
        font-weight: 500;
        transition: all 0.2s;
      `;
      cancelBtn.onmouseover = () => {
        cancelBtn.style.background = '#f9fafb';
        cancelBtn.style.borderColor = '#d1d5db';
      };
      cancelBtn.onmouseout = () => {
        cancelBtn.style.background = 'white';
        cancelBtn.style.borderColor = '#e5e7eb';
      };
      cancelBtn.onclick = () => {
        removeDialog();
        resolve(false);
      };

      const okBtn = document.createElement('button');
      okBtn.textContent = 'Xác nhận';
      okBtn.style.cssText = `
        padding: 10px 20px;
        border: none;
        background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
        color: white;
        border-radius: 8px;
        cursor: pointer;
        font-size: 14px;
        font-weight: 500;
        transition: all 0.2s;
      `;
      okBtn.onmouseover = () => {
        okBtn.style.background = 'linear-gradient(135deg, #dc2626 0%, #b91c1c 100%)';
      };
      okBtn.onmouseout = () => {
        okBtn.style.background = 'linear-gradient(135deg, #ef4444 0%, #dc2626 100%)';
      };
      okBtn.onclick = () => {
        removeDialog();
        resolve(true);
      };

      buttonsEl.appendChild(cancelBtn);
      buttonsEl.appendChild(okBtn);

      dialog.appendChild(titleEl);
      dialog.appendChild(messageEl);
      dialog.appendChild(buttonsEl);
      overlay.appendChild(dialog);
      document.body.appendChild(overlay);

      function removeDialog() {
        overlay.style.animation = 'fadeOut 0.2s ease';
        setTimeout(() => {
          if (overlay.parentElement) {
            overlay.parentElement.removeChild(overlay);
          }
        }, 200);
      }

      // Close on overlay click
      overlay.onclick = (e) => {
        if (e.target === overlay) {
          removeDialog();
          resolve(false);
        }
      };

      // Focus OK button
      setTimeout(() => okBtn.focus(), 100);
    });
  };

  // Thêm CSS animations nếu chưa có
  if (!document.getElementById('confirm-dialog-animations')) {
    const style = document.createElement('style');
    style.id = 'confirm-dialog-animations';
    style.textContent = `
      @keyframes fadeIn {
        from { opacity: 0; }
        to { opacity: 1; }
      }
      @keyframes fadeOut {
        from { opacity: 1; }
        to { opacity: 0; }
      }
      @keyframes slideUp {
        from {
          transform: translateY(20px);
          opacity: 0;
        }
        to {
          transform: translateY(0);
          opacity: 1;
        }
      }
    `;
    document.head.appendChild(style);
  }

  // Thay thế window.confirm nếu cần
  if (typeof window.originalConfirm === 'undefined') {
    window.originalConfirm = window.confirm;
    window.confirm = function(message) {
      console.warn('confirm() called, consider using showConfirm() instead');
      // Return promise-based confirm
      return showConfirm(message, 'Xác nhận');
    };
  }
})();

