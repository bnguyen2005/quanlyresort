/**
 * Payment WebSocket/SSE Manager với fallback polling
 */
class PaymentWebSocketManager {
  constructor() {
    this.connection = null;
    this.currentSessionId = null;
    this.pollingInterval = null;
    this.timerInterval = null;
    this.expiresAt = null;
    this.isWebSocketAvailable = false;
    this.onStatusChange = null;
    this.onError = null;
  }

  /**
   * Tạo payment session và kết nối WebSocket
   */
  async createSessionAndConnect(bookingId, amount, onStatusChange, onError) {
    try {
      this.onStatusChange = onStatusChange;
      this.onError = onError;

      // Tạo payment session
      const token = localStorage.getItem('token');
      if (!token) {
        throw new Error('Unauthorized. Please login to access this resource.');
      }
      
      const response = await fetch(`${location.origin}/api/payment/session/create`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          bookingId: bookingId,
          amount: amount,
          expiryMinutes: 15
        })
      });

      if (!response.ok) {
        let errorMessage = 'Lỗi tạo payment session';
        if (response.status === 401) {
          errorMessage = 'Unauthorized. Please login to access this resource.';
        } else {
          const error = await response.json().catch(() => ({ message: errorMessage }));
          errorMessage = error.message || errorMessage;
        }
        throw new Error(errorMessage);
      }

      const data = await response.json();
      this.currentSessionId = data.sessionId;
      this.expiresAt = data.expiresAt ? new Date(data.expiresAt) : null;

      console.log('✅ [PaymentWebSocket] Session created:', this.currentSessionId);

      // Thử kết nối WebSocket
      await this.connectWebSocket(data.sessionId, token);

      // Bắt đầu timer đếm ngược
      if (this.expiresAt) {
        this.startTimer();
      }

      return data.sessionId;
    } catch (error) {
      console.error('❌ [PaymentWebSocket] Error creating session:', error);
      if (this.onError) this.onError(error);
      throw error;
    }
  }

  /**
   * Kết nối WebSocket với fallback polling
   */
  async connectWebSocket(sessionId, token) {
    try {
      // Thử kết nối SignalR
      if (typeof signalR !== 'undefined') {
        this.connection = new signalR.HubConnectionBuilder()
          .withUrl(`${location.origin}/ws/payment`, {
            accessTokenFactory: () => token,
            skipNegotiation: false,
            transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
          })
          .withAutomaticReconnect({
            nextRetryDelayInMilliseconds: retryContext => {
              if (retryContext.elapsedMilliseconds < 60000) {
                return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
              }
              return null; // Stop reconnecting after 60s
            }
          })
          .configureLogging(signalR.LogLevel.Warning)
          .build();

        // Xử lý events
        this.connection.on('PaymentStatusChanged', (data) => {
          console.log('📨 [PaymentWebSocket] Received PaymentStatusChanged:', data);
          if (this.onStatusChange) {
            this.onStatusChange(data);
          }
        });

        // Listen for BookingStatusChanged (fallback khi không có payment session)
        this.connection.on('BookingStatusChanged', (data) => {
          console.log('📨 [PaymentWebSocket] Received BookingStatusChanged:', data);
          if (this.onStatusChange && data.status === 'Paid') {
            // Convert BookingStatusChanged to PaymentStatusChanged format
            this.onStatusChange({
              sessionId: this.currentSessionId || 'unknown',
              bookingId: data.bookingId,
              status: 'paid',
              transactionId: data.transactionId,
              paidAt: data.paidAt
            });
          }
        });

        this.connection.on('Joined', (sessionId) => {
          console.log('✅ [PaymentWebSocket] Joined session:', sessionId);
          this.isWebSocketAvailable = true;
        });

        this.connection.on('JoinedBooking', (bookingId) => {
          console.log('✅ [PaymentWebSocket] Joined booking group:', bookingId);
          this.isWebSocketAvailable = true;
        });

        this.connection.on('Error', (error) => {
          console.error('❌ [PaymentWebSocket] Error:', error);
          if (this.onError) this.onError(new Error(error));
        });

        this.connection.onclose((error) => {
          console.warn('⚠️ [PaymentWebSocket] Connection closed:', error);
          this.isWebSocketAvailable = false;
          // Fallback to polling
          if (this.currentSessionId) {
            this.startPolling();
          }
        });

        // Bắt đầu kết nối
        await this.connection.start();
        console.log('✅ [PaymentWebSocket] Connected');

        // Join payment session group
        await this.connection.invoke('JoinPaymentSession', sessionId);
        
        // Also join booking group as fallback (bookingId được extract từ session hoặc truyền vào)
        // Note: bookingId sẽ được truyền vào createSessionAndConnect nếu cần
        this.isWebSocketAvailable = true;
      } else {
        throw new Error('SignalR not available');
      }
    } catch (error) {
      console.warn('⚠️ [PaymentWebSocket] WebSocket not available, falling back to polling:', error);
      this.isWebSocketAvailable = false;
      this.startPolling();
    }
  }

  /**
   * Fallback polling nếu WebSocket không khả dụng
   */
  startPolling() {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
    }

    let pollCount = 0;
    const maxPolls = 40; // 2 phút (40 * 3s)

    console.log('🔄 [PaymentWebSocket] Starting fallback polling');

    this.pollingInterval = setInterval(async () => {
      pollCount++;
      
      if (pollCount > maxPolls) {
        console.log('⏱️ [PaymentWebSocket] Max polls reached, stopping');
        this.stopPolling();
        if (this.onError) {
          this.onError(new Error('Đã hết thời gian chờ thanh toán'));
        }
        return;
      }

      try {
        const token = localStorage.getItem('token');
        const response = await fetch(`${location.origin}/api/payment/status/${this.currentSessionId}?_=${Date.now()}`, {
          headers: {
            'Authorization': `Bearer ${token}`
          },
          cache: 'no-store'
        });

        if (response.ok) {
          const data = await response.json();
          
          if (data.status !== 'pending' && data.status !== 'processing') {
            // Status changed, notify
            if (this.onStatusChange) {
              this.onStatusChange({
                sessionId: data.sessionId,
                status: data.status,
                transactionId: data.transactionId,
                invoiceNumber: data.invoiceNumber,
                paidAt: data.paidAt,
                errorMessage: data.errorMessage
              });
            }
            
            // Stop polling if final status
            if (['paid', 'failed', 'cancelled', 'expired'].includes(data.status)) {
              this.stopPolling();
            }
          }
        }
      } catch (error) {
        console.error('❌ [PaymentWebSocket] Polling error:', error);
      }
    }, 3000); // Poll mỗi 3 giây
  }

  /**
   * Bắt đầu timer đếm ngược
   */
  startTimer() {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }

    const updateTimer = () => {
      if (!this.expiresAt) return;

      const now = new Date();
      const diff = this.expiresAt - now;

      if (diff <= 0) {
        // Expired
        const timerEl = document.getElementById('plTimerValue');
        if (timerEl) {
          timerEl.textContent = '00:00';
          timerEl.style.color = '#dc3545';
        }
        this.stopTimer();
        
        // Notify expired
        if (this.onStatusChange) {
          this.onStatusChange({
            sessionId: this.currentSessionId,
            status: 'expired'
          });
        }
        return;
      }

      const minutes = Math.floor(diff / 60000);
      const seconds = Math.floor((diff % 60000) / 1000);
      const timeStr = `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;

      const timerEl = document.getElementById('plTimerValue');
      if (timerEl) {
        timerEl.textContent = timeStr;
        
        // Đổi màu khi còn < 2 phút
        if (minutes < 2) {
          timerEl.style.color = '#dc3545';
        } else if (minutes < 5) {
          timerEl.style.color = '#ffc107';
        } else {
          timerEl.style.color = '#856404';
        }
      }
    };

    updateTimer();
    this.timerInterval = setInterval(updateTimer, 1000);
  }

  /**
   * Dừng timer
   */
  stopTimer() {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
  }

  /**
   * Dừng polling
   */
  stopPolling() {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
      this.pollingInterval = null;
    }
  }

  /**
   * Đóng kết nối và cleanup
   */
  disconnect() {
    console.log('🔌 [PaymentWebSocket] Disconnecting...');
    
    this.stopPolling();
    this.stopTimer();

    if (this.connection) {
      if (this.currentSessionId) {
        this.connection.invoke('LeavePaymentSession', this.currentSessionId).catch(console.error);
      }
      this.connection.stop().catch(console.error);
      this.connection = null;
    }

    this.currentSessionId = null;
    this.expiresAt = null;
    this.isWebSocketAvailable = false;
  }
}

// Export for use in other files
if (typeof window !== 'undefined') {
  window.PaymentWebSocketManager = PaymentWebSocketManager;
}

