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
      width: 60px;
      height: 60px;
      border-radius: 50%;
      background: linear-gradient(135deg, #C8A97E 0%, #B89968 100%);
      border: none;
      color: white;
      box-shadow: 0 4px 12px rgba(200, 169, 126, 0.4);
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.3s ease;
    `;

    button.addEventListener('mouseenter', () => {
      button.style.transform = 'scale(1.1)';
      button.style.boxShadow = '0 6px 16px rgba(200, 169, 126, 0.6)';
    });

    button.addEventListener('mouseleave', () => {
      button.style.transform = 'scale(1)';
      button.style.boxShadow = '0 4px 12px rgba(200, 169, 126, 0.4)';
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
              <h4 style="margin: 0; font-size: 16px; font-weight: 600;">Trợ lý AI</h4>
              <small style="color: #6B7280; font-size: 12px;">Resort Deluxe</small>
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
      .ai-chat-modal {
        position: fixed;
        bottom: 90px;
        right: 20px;
        width: 380px;
        max-width: calc(100vw - 40px);
        height: 600px;
        max-height: calc(100vh - 120px);
        background: white;
        border-radius: 16px;
        box-shadow: 0 8px 32px rgba(0, 0, 0, 0.15);
        z-index: 1001;
        display: none;
        flex-direction: column;
        overflow: hidden;
      }
      .ai-chat-modal.show {
        display: flex;
      }
      .ai-chat-modal-content {
        display: flex;
        flex-direction: column;
        height: 100%;
      }
      .ai-chat-header {
        padding: 16px;
        border-bottom: 1px solid #E5E7EB;
        display: flex;
        justify-content: space-between;
        align-items: center;
        background: linear-gradient(135deg, #C8A97E 0%, #B89968 100%);
        color: white;
      }
      .ai-chat-header-info {
        display: flex;
        align-items: center;
        gap: 12px;
      }
      .ai-chat-avatar {
        width: 40px;
        height: 40px;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.2);
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 20px;
      }
      .ai-chat-close {
        background: none;
        border: none;
        color: white;
        font-size: 28px;
        cursor: pointer;
        width: 32px;
        height: 32px;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 50%;
        transition: background 0.2s;
      }
      .ai-chat-close:hover {
        background: rgba(255, 255, 255, 0.2);
      }
      .ai-chat-messages {
        flex: 1;
        overflow-y: auto;
        padding: 16px;
        display: flex;
        flex-direction: column;
        gap: 12px;
        background: #F9FAFB;
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
        padding: 12px 16px;
        border-radius: 12px;
        background: white;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        word-wrap: break-word;
        line-height: 1.5;
        font-size: 14px;
        color: #1F2937;
      }
      .user-message .ai-chat-bubble {
        background: #C8A97E;
        color: white;
      }
      .ai-chat-input-container {
        padding: 16px;
        border-top: 1px solid #E5E7EB;
        display: flex;
        gap: 8px;
        background: white;
      }
      .ai-chat-input {
        flex: 1;
        padding: 12px 16px;
        border: 1px solid #E5E7EB;
        border-radius: 24px;
        font-size: 14px;
        outline: none;
        transition: border-color 0.2s;
      }
      .ai-chat-input:focus {
        border-color: #C8A97E;
      }
      .ai-chat-send-btn {
        padding: 12px 24px;
        background: #C8A97E;
        color: white;
        border: none;
        border-radius: 24px;
        font-weight: 600;
        cursor: pointer;
        transition: background 0.2s;
      }
      .ai-chat-send-btn:hover {
        background: #B89968;
      }
      .ai-chat-send-btn:disabled {
        background: #D1D5DB;
        cursor: not-allowed;
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

