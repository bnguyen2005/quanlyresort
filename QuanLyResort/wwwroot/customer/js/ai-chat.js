/**
 * AI Chat Widget - Floating chat button and modal
 */

(function() {
  'use strict';

  const CHAT_WIDGET_ID = 'aiChatWidget';
  const CHAT_MODAL_ID = 'aiChatModal';
  const API_BASE = window.location.origin + '/api/aichat';

  // Tạo chat widget nếu chưa có
  function createChatWidget() {
    if (document.getElementById(CHAT_WIDGET_ID)) {
      return; // Đã tồn tại
    }

    // Floating button
    const chatButton = document.createElement('div');
    chatButton.id = CHAT_WIDGET_ID;
    chatButton.innerHTML = `
      <button type="button" class="ai-chat-button" aria-label="Mở chat AI" title="Chat với AI">
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M20 2H4C2.9 2 2 2.9 2 4V22L6 18H20C21.1 18 22 17.1 22 16V4C22 2.9 21.1 2 20 2Z" fill="currentColor"/>
        </svg>
      </button>
    `;
    chatButton.style.cssText = `
      position: fixed;
      bottom: 20px;
      right: 20px;
      z-index: 1000;
      cursor: pointer;
    `;

    const button = chatButton.querySelector('.ai-chat-button');
    button.style.cssText = `
      width: 64px;
      height: 64px;
      border-radius: 50%;
      background: linear-gradient(135deg, #C8A97E 0%, #B89968 50%, #A68B5A 100%);
      border: 3px solid rgba(255, 255, 255, 0.3);
      color: white;
      box-shadow: 0 8px 24px rgba(200, 169, 126, 0.5), 0 0 0 0 rgba(200, 169, 126, 0.4);
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.4s cubic-bezier(0.68, -0.55, 0.265, 1.55);
      position: relative;
      overflow: hidden;
    `;

    // Thêm hiệu ứng pulse
    const pulse = document.createElement('div');
    pulse.style.cssText = `
      position: absolute;
      top: 50%;
      left: 50%;
      width: 0;
      height: 0;
      border-radius: 50%;
      background: rgba(255, 255, 255, 0.3);
      transform: translate(-50%, -50%);
      animation: pulse 2s infinite;
    `;
    button.appendChild(pulse);

    button.addEventListener('mouseenter', () => {
      button.style.transform = 'scale(1.15) rotate(5deg)';
      button.style.boxShadow = '0 12px 32px rgba(200, 169, 126, 0.7), 0 0 0 8px rgba(200, 169, 126, 0.2)';
    });

    button.addEventListener('mouseleave', () => {
      button.style.transform = 'scale(1) rotate(0deg)';
      button.style.boxShadow = '0 8px 24px rgba(200, 169, 126, 0.5), 0 0 0 0 rgba(200, 169, 126, 0.4)';
    });

    button.addEventListener('click', () => {
      showChatModal();
    });

    document.body.appendChild(chatButton);
  }

  // Tạo chat modal
  function createChatModal() {
    if (document.getElementById(CHAT_MODAL_ID)) {
      return; // Đã tồn tại
    }

    const modal = document.createElement('div');
    modal.id = CHAT_MODAL_ID;
    modal.className = 'ai-chat-modal';
    modal.innerHTML = `
      <div class="ai-chat-modal-content">
        <div class="ai-chat-header">
          <div class="ai-chat-header-info">
            <div class="ai-chat-avatar">🤖</div>
            <div>
              <h4 style="margin: 0; font-size: 17px; font-weight: 700; letter-spacing: 0.3px;">Trợ lý AI</h4>
              <small style="color: rgba(255, 255, 255, 0.9); font-size: 12px; font-weight: 500;">Resort Deluxe • Luôn sẵn sàng hỗ trợ</small>
            </div>
          </div>
          <button type="button" class="ai-chat-close" aria-label="Đóng chat">×</button>
        </div>
        <div class="ai-chat-messages" id="aiChatMessages">
          <div class="ai-chat-message ai-message">
            <div class="ai-chat-avatar-small">🤖</div>
            <div class="ai-chat-bubble">
              Xin chào! Tôi là trợ lý AI của Resort Deluxe. Tôi có thể giúp bạn với các câu hỏi về đặt phòng, dịch vụ, thanh toán và nhiều hơn nữa. Bạn cần hỗ trợ gì?
            </div>
          </div>
        </div>
        <div class="ai-chat-input-container">
          <input type="text" id="aiChatInput" class="ai-chat-input" placeholder="Nhập câu hỏi của bạn..." />
          <button type="button" id="aiChatSend" class="ai-chat-send-btn">Gửi</button>
        </div>
      </div>
    `;

    // Styles
    const style = document.createElement('style');
    style.textContent = `
      @keyframes pulse {
        0% {
          width: 0;
          height: 0;
          opacity: 1;
        }
        100% {
          width: 100px;
          height: 100px;
          opacity: 0;
        }
      }
      @keyframes slideUp {
        from {
          opacity: 0;
          transform: translateY(20px) scale(0.95);
        }
        to {
          opacity: 1;
          transform: translateY(0) scale(1);
        }
      }
      .ai-chat-modal {
        position: fixed;
        bottom: 90px;
        right: 20px;
        width: 400px;
        max-width: calc(100vw - 40px);
        height: 650px;
        max-height: calc(100vh - 120px);
        background: linear-gradient(135deg, #ffffff 0%, #f9fafb 100%);
        border-radius: 24px;
        box-shadow: 0 20px 60px rgba(0, 0, 0, 0.2), 0 0 0 1px rgba(200, 169, 126, 0.1);
        z-index: 1001;
        display: none;
        flex-direction: column;
        overflow: hidden;
        animation: slideUp 0.3s cubic-bezier(0.68, -0.55, 0.265, 1.55);
      }
      .ai-chat-modal.show {
        display: flex;
      }
      @media (max-width: 768px) {
        .ai-chat-modal {
          width: calc(100vw - 20px);
          right: 10px;
          bottom: 80px;
          height: calc(100vh - 100px);
        }
      }
      .ai-chat-modal-content {
        display: flex;
        flex-direction: column;
        height: 100%;
      }
      .ai-chat-header {
        padding: 20px;
        border-bottom: none;
        display: flex;
        justify-content: space-between;
        align-items: center;
        background: linear-gradient(135deg, #C8A97E 0%, #B89968 50%, #A68B5A 100%);
        color: white;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        position: relative;
        overflow: hidden;
      }
      .ai-chat-header::before {
        content: '';
        position: absolute;
        top: -50%;
        right: -50%;
        width: 200%;
        height: 200%;
        background: radial-gradient(circle, rgba(255, 255, 255, 0.1) 0%, transparent 70%);
        animation: rotate 20s linear infinite;
      }
      @keyframes rotate {
        from { transform: rotate(0deg); }
        to { transform: rotate(360deg); }
      }
      .ai-chat-header-info {
        display: flex;
        align-items: center;
        gap: 14px;
        position: relative;
        z-index: 1;
      }
      .ai-chat-avatar {
        width: 48px;
        height: 48px;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.25);
        backdrop-filter: blur(10px);
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 24px;
        border: 2px solid rgba(255, 255, 255, 0.3);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      }
      .ai-chat-close {
        background: rgba(255, 255, 255, 0.15);
        backdrop-filter: blur(10px);
        border: none;
        color: white;
        font-size: 24px;
        cursor: pointer;
        width: 36px;
        height: 36px;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 50%;
        transition: all 0.3s ease;
        position: relative;
        z-index: 1;
        font-weight: 300;
        line-height: 1;
      }
      .ai-chat-close:hover {
        background: rgba(255, 255, 255, 0.3);
        transform: rotate(90deg) scale(1.1);
      }
      .ai-chat-messages {
        flex: 1;
        overflow-y: auto;
        padding: 20px;
        display: flex;
        flex-direction: column;
        gap: 16px;
        background: linear-gradient(180deg, #f9fafb 0%, #ffffff 100%);
        scroll-behavior: smooth;
      }
      .ai-chat-messages::-webkit-scrollbar {
        width: 6px;
      }
      .ai-chat-messages::-webkit-scrollbar-track {
        background: transparent;
      }
      .ai-chat-messages::-webkit-scrollbar-thumb {
        background: #C8A97E;
        border-radius: 3px;
      }
      .ai-chat-messages::-webkit-scrollbar-thumb:hover {
        background: #B89968;
      }
      .ai-chat-message {
        display: flex;
        gap: 8px;
        animation: fadeIn 0.3s ease;
      }
      @keyframes fadeIn {
        from { opacity: 0; transform: translateY(10px); }
        to { opacity: 1; transform: translateY(0); }
      }
      .ai-chat-message.user-message {
        flex-direction: row-reverse;
      }
      .ai-chat-avatar-small {
        width: 32px;
        height: 32px;
        border-radius: 50%;
        background: #E5E7EB;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 16px;
        flex-shrink: 0;
      }
      .user-message .ai-chat-avatar-small {
        background: #C8A97E;
        color: white;
      }
      .ai-chat-bubble {
        max-width: 75%;
        padding: 14px 18px;
        border-radius: 18px;
        background: white;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08), 0 2px 4px rgba(0, 0, 0, 0.04);
        word-wrap: break-word;
        line-height: 1.6;
        font-size: 14px;
        color: #1F2937;
        position: relative;
      }
      .ai-message .ai-chat-bubble {
        border-top-left-radius: 4px;
      }
      .user-message .ai-chat-bubble {
        background: linear-gradient(135deg, #C8A97E 0%, #B89968 100%);
        color: white;
        border-top-right-radius: 4px;
        box-shadow: 0 4px 12px rgba(200, 169, 126, 0.3);
      }
      .ai-chat-input-container {
        padding: 20px;
        border-top: 1px solid rgba(200, 169, 126, 0.1);
        display: flex;
        gap: 12px;
        background: white;
        box-shadow: 0 -4px 12px rgba(0, 0, 0, 0.05);
      }
      .ai-chat-input {
        flex: 1;
        padding: 14px 20px;
        border: 2px solid #E5E7EB;
        border-radius: 28px;
        font-size: 14px;
        outline: none;
        transition: all 0.3s ease;
        background: #F9FAFB;
      }
      .ai-chat-input:focus {
        border-color: #C8A97E;
        background: white;
        box-shadow: 0 0 0 4px rgba(200, 169, 126, 0.1);
      }
      .ai-chat-send-btn {
        padding: 14px 28px;
        background: linear-gradient(135deg, #C8A97E 0%, #B89968 100%);
        color: white;
        border: none;
        border-radius: 28px;
        font-weight: 600;
        cursor: pointer;
        transition: all 0.3s ease;
        box-shadow: 0 4px 12px rgba(200, 169, 126, 0.3);
        min-width: 80px;
      }
      .ai-chat-send-btn:hover {
        background: linear-gradient(135deg, #B89968 0%, #A68B5A 100%);
        transform: translateY(-2px);
        box-shadow: 0 6px 16px rgba(200, 169, 126, 0.4);
      }
      .ai-chat-send-btn:active {
        transform: translateY(0);
      }
      .ai-chat-send-btn:disabled {
        background: #D1D5DB;
        cursor: not-allowed;
        transform: none;
        box-shadow: none;
      }
      .ai-chat-typing {
        display: flex;
        gap: 4px;
        padding: 12px 16px;
      }
      .ai-chat-typing span {
        width: 8px;
        height: 8px;
        border-radius: 50%;
        background: #C8A97E;
        animation: typing 1.4s infinite;
      }
      .ai-chat-typing span:nth-child(2) {
        animation-delay: 0.2s;
      }
      .ai-chat-typing span:nth-child(3) {
        animation-delay: 0.4s;
      }
      @keyframes typing {
        0%, 60%, 100% { transform: translateY(0); opacity: 0.7; }
        30% { transform: translateY(-10px); opacity: 1; }
      }
    `;
    document.head.appendChild(style);

    // Event listeners
    const closeBtn = modal.querySelector('.ai-chat-close');
    closeBtn.addEventListener('click', hideChatModal);

    const input = modal.querySelector('#aiChatInput');
    const sendBtn = modal.querySelector('#aiChatSend');

    const sendMessage = () => {
      const message = input.value.trim();
      if (!message) return;

      addMessage(message, 'user');
      input.value = '';
      sendBtn.disabled = true;

      // Gửi đến API
      fetch(API_BASE + '/send', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ message: message })
      })
      .then(async res => {
        if (!res.ok) {
          const errorText = await res.text();
          console.error('[AI Chat] API Error:', res.status, errorText);
          
          if (res.status === 401) {
            throw new Error('API key không hợp lệ. Vui lòng liên hệ quản trị viên.');
          } else if (res.status === 429) {
            throw new Error('Hệ thống đang quá tải. Vui lòng thử lại sau vài phút.');
          } else if (res.status >= 500) {
            throw new Error('Lỗi server. Vui lòng thử lại sau.');
          } else {
            throw new Error(`Lỗi ${res.status}: ${errorText}`);
          }
        }
        return res.json();
      })
      .then(data => {
        sendBtn.disabled = false;
        if (data.success) {
          addMessage(data.message, 'ai');
        } else {
          addMessage(data.error || 'Xin lỗi, đã xảy ra lỗi. Vui lòng thử lại sau.', 'ai');
        }
      })
      .catch(err => {
        console.error('[AI Chat] Error:', err);
        sendBtn.disabled = false;
        addMessage(err.message || 'Xin lỗi, không thể kết nối đến server. Vui lòng thử lại sau.', 'ai');
      });
    };

    sendBtn.addEventListener('click', sendMessage);
    input.addEventListener('keypress', (e) => {
      if (e.key === 'Enter') {
        sendMessage();
      }
    });

    document.body.appendChild(modal);
  }

  function showChatModal() {
    const modal = document.getElementById(CHAT_MODAL_ID);
    if (modal) {
      modal.classList.add('show');
      const input = modal.querySelector('#aiChatInput');
      if (input) input.focus();
    }
  }

  function hideChatModal() {
    const modal = document.getElementById(CHAT_MODAL_ID);
    if (modal) {
      modal.classList.remove('show');
    }
  }

  function addMessage(text, type) {
    const messagesContainer = document.getElementById('aiChatMessages');
    if (!messagesContainer) return;

    // Xóa typing indicator nếu có
    const typing = messagesContainer.querySelector('.ai-chat-typing');
    if (typing) typing.remove();

    const messageDiv = document.createElement('div');
    messageDiv.className = `ai-chat-message ${type}-message`;

    if (type === 'ai') {
      messageDiv.innerHTML = `
        <div class="ai-chat-avatar-small">🤖</div>
        <div class="ai-chat-bubble">${escapeHtml(text)}</div>
      `;
    } else {
      messageDiv.innerHTML = `
        <div class="ai-chat-avatar-small">👤</div>
        <div class="ai-chat-bubble">${escapeHtml(text)}</div>
      `;
    }

    messagesContainer.appendChild(messageDiv);
    messagesContainer.scrollTop = messagesContainer.scrollHeight;

    // Hiển thị typing indicator cho AI response
    if (type === 'user') {
      showTypingIndicator();
    }
  }

  function showTypingIndicator() {
    const messagesContainer = document.getElementById('aiChatMessages');
    if (!messagesContainer) return;

    const typing = document.createElement('div');
    typing.className = 'ai-chat-message ai-message';
    typing.innerHTML = `
      <div class="ai-chat-avatar-small">🤖</div>
      <div class="ai-chat-typing">
        <span></span>
        <span></span>
        <span></span>
      </div>
    `;
    messagesContainer.appendChild(typing);
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
  }

  function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
  }

  // Khởi tạo khi DOM ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
      createChatWidget();
      createChatModal();
    });
  } else {
    createChatWidget();
    createChatModal();
  }

  // Export functions để có thể gọi từ bên ngoài
  window.AIChat = {
    show: showChatModal,
    hide: hideChatModal
  };

  console.log('✅ AI Chat widget loaded');
})();

