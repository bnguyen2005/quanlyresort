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
      border-radius: 0;
      background: #000;
      border: 1px solid #C8A97E;
      color: #C8A97E;
      box-shadow: 4px 4px 0px rgba(200, 169, 126, 0.2);
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.3s ease;
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
      border-radius: 0;
      background: rgba(200, 169, 126, 0.1);
      transform: translate(-50%, -50%);
      animation: pulse 2s infinite;
    `;
    button.appendChild(pulse);

    button.addEventListener('mouseenter', () => {
      button.style.transform = 'translate(-2px, -2px)';
      button.style.boxShadow = '6px 6px 0px rgba(200, 169, 126, 0.4)';
      button.style.background = '#C8A97E';
      button.style.color = '#000';
    });

    button.addEventListener('mouseleave', () => {
      button.style.transform = 'translate(0, 0)';
      button.style.boxShadow = '4px 4px 0px rgba(200, 169, 126, 0.2)';
      button.style.background = '#000';
      button.style.color = '#C8A97E';
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
            <div class="ai-chat-avatar">AI</div>
            <div>
              <h4 style="margin: 0; font-size: 14px; font-weight: 300; letter-spacing: 4px; font-family: 'Inter'; text-transform: uppercase; color: var(--gold);">A I   A S S I S T A N T</h4>
              <small style="color: rgba(255, 255, 255, 0.5); font-size: 10px; font-weight: 300; letter-spacing: 1px; text-transform: uppercase;">Resort Deluxe</small>
            </div>
          </div>
          <button type="button" class="ai-chat-close" aria-label="Đóng chat">✕</button>
        </div>
        <div class="ai-chat-messages" id="aiChatMessages">
          <div class="ai-chat-message ai-message">
            <div class="ai-chat-avatar-small">AI</div>
            <div class="ai-chat-bubble">
              Xin chào! Tôi là trợ lý AI của Resort Deluxe. Tôi có thể giúp bạn với các câu hỏi về đặt phòng, dịch vụ, thanh toán và nhiều hơn nữa. Bạn cần hỗ trợ gì?
            </div>
          </div>
        </div>
        <div class="ai-chat-input-container">
          <input type="text" id="aiChatInput" class="ai-chat-input" placeholder="NHẬP CÂU HỎI..." />
          <button type="button" id="aiChatSend" class="ai-chat-send-btn">GỬI</button>
        </div>
      </div>
    `;

    // Styles
    const style = document.createElement('style');
    style.textContent = `
      @keyframes slideUp {
        from { opacity: 0; transform: translateY(20px); }
        to { opacity: 1; transform: translateY(0); }
      }
      .ai-chat-modal {
        position: fixed;
        bottom: 90px;
        right: 20px;
        width: 400px;
        max-width: calc(100vw - 40px);
        height: 650px;
        max-height: calc(100vh - 120px);
        background: rgba(5, 5, 5, 0.95);
        backdrop-filter: blur(20px);
        border: 1px solid rgba(255,255,255,0.1);
        border-radius: 0;
        z-index: 1001;
        display: none;
        flex-direction: column;
        overflow: hidden;
        animation: slideUp 0.4s cubic-bezier(0.16, 1, 0.3, 1);
        box-shadow: 0 30px 60px rgba(0,0,0,0.5);
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
        border-bottom: 1px solid rgba(255,255,255,0.1);
        display: flex;
        justify-content: space-between;
        align-items: center;
        background: #000;
        color: white;
        position: relative;
      }
      .ai-chat-header-info {
        display: flex;
        align-items: center;
        gap: 14px;
        position: relative;
        z-index: 1;
      }
      .ai-chat-avatar {
        width: 40px;
        height: 40px;
        border-radius: 0;
        background: transparent;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 14px;
        border: 1px solid var(--gold, #C8A97E);
        color: var(--gold, #C8A97E);
        font-family: 'Inter';
        font-weight: 300;
      }
      .ai-chat-close {
        background: transparent;
        border: none;
        color: rgba(255,255,255,0.5);
        font-size: 20px;
        cursor: pointer;
        width: 36px;
        height: 36px;
        display: flex;
        align-items: center;
        justify-content: center;
        transition: all 0.3s ease;
      }
      .ai-chat-close:hover {
        color: var(--gold, #C8A97E);
        transform: rotate(90deg);
      }
      .ai-chat-messages {
        flex: 1;
        overflow-y: auto;
        padding: 25px 20px;
        display: flex;
        flex-direction: column;
        gap: 20px;
        background: transparent;
        scroll-behavior: smooth;
      }
      .ai-chat-messages::-webkit-scrollbar { width: 4px; }
      .ai-chat-messages::-webkit-scrollbar-track { background: transparent; }
      .ai-chat-messages::-webkit-scrollbar-thumb { background: rgba(255,255,255,0.1); }
      .ai-chat-message {
        display: flex;
        gap: 12px;
        animation: fadeIn 0.4s cubic-bezier(0.16, 1, 0.3, 1);
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
        border-radius: 0;
        background: rgba(255,255,255,0.05);
        border: 1px solid rgba(255,255,255,0.1);
        color: rgba(255,255,255,0.5);
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 10px;
        flex-shrink: 0;
        font-family: 'Inter';
      }
      .user-message .ai-chat-avatar-small {
        background: transparent;
        border-color: var(--gold, #C8A97E);
        color: var(--gold, #C8A97E);
      }
      .ai-chat-bubble {
        max-width: 80%;
        padding: 15px 20px;
        border-radius: 0;
        word-wrap: break-word;
        line-height: 1.6;
        font-size: 14px;
        font-family: 'Inter';
        font-weight: 300;
        position: relative;
      }
      .ai-message .ai-chat-bubble {
        background: rgba(255,255,255,0.03);
        color: rgba(255,255,255,0.9);
        border-left: 2px solid var(--gold, #C8A97E);
      }
      .user-message .ai-chat-bubble {
        background: transparent;
        color: white;
        border: 1px solid rgba(255,255,255,0.15);
      }
      .ai-chat-input-container {
        padding: 20px;
        border-top: 1px solid rgba(255,255,255,0.1);
        display: flex;
        gap: 15px;
        background: #000;
      }
      .ai-chat-input {
        flex: 1;
        padding: 10px 0;
        border: none;
        border-bottom: 1px solid rgba(255,255,255,0.2);
        border-radius: 0;
        font-size: 13px;
        font-family: 'Inter';
        font-weight: 300;
        outline: none;
        transition: all 0.3s ease;
        background: transparent;
        color: #fff;
        letter-spacing: 1px;
      }
      .ai-chat-input:focus {
        border-bottom-color: var(--gold, #C8A97E);
      }
      .ai-chat-send-btn {
        padding: 0 20px;
        background: transparent;
        color: var(--gold, #C8A97E);
        border: 1px solid var(--gold, #C8A97E);
        border-radius: 0;
        font-weight: 300;
        font-family: 'Inter';
        font-size: 12px;
        letter-spacing: 2px;
        cursor: pointer;
        transition: all 0.4s var(--easing, cubic-bezier(0.16, 1, 0.3, 1));
      }
      .ai-chat-send-btn:hover {
        background: var(--gold, #C8A97E);
        color: #000;
      }
      .ai-chat-send-btn:disabled {
        border-color: rgba(255,255,255,0.2);
        color: rgba(255,255,255,0.2);
        cursor: not-allowed;
      }
      .ai-chat-typing {
        display: flex;
        gap: 4px;
        padding: 12px 16px;
      }
      .ai-chat-typing span {
        width: 4px;
        height: 4px;
        border-radius: 0;
        background: var(--gold, #C8A97E);
        animation: typing 1.4s infinite;
      }
      .ai-chat-typing span:nth-child(2) { animation-delay: 0.2s; }
      .ai-chat-typing span:nth-child(3) { animation-delay: 0.4s; }
      @keyframes typing {
        0%, 60%, 100% { transform: translateY(0); opacity: 0.3; }
        30% { transform: translateY(-5px); opacity: 1; }
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
        <div class="ai-chat-avatar-small">AI</div>
        <div class="ai-chat-bubble">${escapeHtml(text)}</div>
      `;
    } else {
      messageDiv.innerHTML = `
        <div class="ai-chat-avatar-small">YOU</div>
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
      <div class="ai-chat-avatar-small">AI</div>
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

