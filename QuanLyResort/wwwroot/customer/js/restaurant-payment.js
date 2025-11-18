/**
 * Hệ thống thanh toán SePay cho Restaurant Orders
 * Tương tự simple-payment.js nhưng cho restaurant orders
 */

// Global state
window.currentRestaurantPaymentOrderId = null;
window.restaurantPaymentPollingInterval = null;

/**
 * Mở modal thanh toán cho restaurant order
 */
async function openRestaurantPayment(orderId) {
  try {
    console.log("[FRONTEND] 🔄 [openRestaurantPayment] Opening payment for order:", orderId);

    // Get order details
    const token = localStorage.getItem('token');
    if (!token) {
      showSimpleToast('Vui lòng đăng nhập để thanh toán', 'warning');
      window.location.href = '/customer/login.html';
      return;
    }

    const response = await fetch(`${location.origin}/api/restaurant-orders/${orderId}`, {
      headers: {
        'Authorization': `Bearer ${token}`
      },
      cache: 'no-store'
    });

    if (!response.ok) {
      throw new Error('Không thể tải thông tin đơn hàng');
    }

    const order = await response.json();
    console.log("[FRONTEND] ✅ [openRestaurantPayment] Order loaded:", order);

    // Check if already paid
    if (order.paymentStatus === 'Paid') {
      showSimpleToast('Đơn hàng này đã được thanh toán!', 'success');
      return;
    }

    // Check if walk-in order
    if (!order.customerId) {
      showSimpleToast('Đơn hàng này là đơn tại quầy, vui lòng thanh toán trực tiếp tại nhà hàng', 'warning');
      return;
    }

    // Get amount
    const amount = order.totalAmount || 0;
    if (amount <= 0) {
      showSimpleToast('Số tiền thanh toán không hợp lệ', 'danger');
      return;
    }

    // Setup payment method change handler FIRST, before updating modal
    const paymentMethodSelect = document.getElementById('rpPaymentMethod');
    const qrSection = document.getElementById('rpQRSection');
    const cashSection = document.getElementById('rpCashSection');
    
    // Initialize sections - hide both first
    if (qrSection) qrSection.style.display = 'none';
    if (cashSection) cashSection.style.display = 'none';
    
    if (paymentMethodSelect) {
      // Remove existing listeners to avoid duplicates
      const newSelect = paymentMethodSelect.cloneNode(true);
      paymentMethodSelect.parentNode.replaceChild(newSelect, paymentMethodSelect);
      
      // Setup new handler
      newSelect.addEventListener('change', function() {
        const method = this.value;
        const qrSectionEl = document.getElementById('rpQRSection');
        const cashSectionEl = document.getElementById('rpCashSection');
        
        if (method === 'QR') {
          if (qrSectionEl) qrSectionEl.style.display = 'block';
          if (cashSectionEl) cashSectionEl.style.display = 'none';
          // Only create QR when QR is selected
          updateRestaurantPaymentModal(orderId, order.orderNumber || `ORD${orderId}`, amount);
        } else if (method === 'Cash') {
          if (qrSectionEl) qrSectionEl.style.display = 'none';
          if (cashSectionEl) cashSectionEl.style.display = 'block';
          
          // Update cash section info
          const cashOrderNumber = document.getElementById('rpCashOrderNumber');
          const cashAmount = document.getElementById('rpCashAmount');
          if (cashOrderNumber) cashOrderNumber.textContent = order.orderNumber || `ORD${orderId}`;
          if (cashAmount) cashAmount.textContent = formatCurrency(amount);
          
          // Hide QR section elements
          const qrImg = document.getElementById('rpQRImage');
          const waitingEl = document.getElementById('rpWaiting');
          if (qrImg) qrImg.style.display = 'none';
          if (waitingEl) waitingEl.style.display = 'none';
        }
      });
      
      // Set default to QR and trigger change to show correct section
      newSelect.value = 'QR';
      newSelect.dispatchEvent(new Event('change'));
    } else {
      // If no payment method select, default to QR
      updateRestaurantPaymentModal(orderId, order.orderNumber || `ORD${orderId}`, amount);
    }

    // Show modal
    const modalElement = document.getElementById('restaurantPaymentModal');
    if (!modalElement) {
      console.error("[FRONTEND] ❌ Modal element not found: restaurantPaymentModal");
      showSimpleToast('Lỗi: Không tìm thấy modal thanh toán', 'danger');
      return;
    }
    
    // Show modal - compatible with Bootstrap 4 and 5
    try {
      if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
        const modal = new bootstrap.Modal(modalElement);
        modal.show();
      } else if (typeof $ !== 'undefined' && $.fn.modal) {
        $(modalElement).modal('show');
      } else {
        modalElement.classList.add('show');
        modalElement.style.display = 'block';
        document.body.classList.add('modal-open');
        const backdrop = document.createElement('div');
        backdrop.className = 'modal-backdrop fade show';
        document.body.appendChild(backdrop);
      }
    } catch (e) {
      console.error("[FRONTEND] ❌ Error showing modal:", e);
      modalElement.classList.add('show');
      modalElement.style.display = 'block';
      document.body.classList.add('modal-open');
    }

    // Start polling
    startRestaurantPaymentPolling(orderId);

    window.currentRestaurantPaymentOrderId = orderId;

  } catch (error) {
    console.error("[FRONTEND] ❌ Error opening restaurant payment:", error);
    showSimpleToast('Lỗi mở form thanh toán', 'danger');
  }
}

/**
 * Update modal content - Tạo SePay QR code động
 */
async function updateRestaurantPaymentModal(orderId, orderNumber, amount) {
  // Check if QR payment method is selected - only create QR if QR is selected
  const paymentMethodSelect = document.getElementById('rpPaymentMethod');
  if (paymentMethodSelect && paymentMethodSelect.value !== 'QR') {
    console.log("[FRONTEND] ⏭️ [updateRestaurantPaymentModal] Payment method is not QR, skipping QR creation");
    return;
  }
  
  // Order number
  const codeEl = document.getElementById('rpOrderNumber');
  if (codeEl) codeEl.textContent = orderNumber;

  // Amount
  const amountEl = document.getElementById('rpAmount');
  if (amountEl) amountEl.textContent = formatCurrency(amount);

    // Show loading
    const qrImg = document.getElementById('rpQRImage');
    const qrSection = document.getElementById('rpQRSection');
    const waitingEl = document.getElementById('rpWaiting');
    const successEl = document.getElementById('rpSuccess');
    
    // Reset UI state - ensure success is hidden and waiting is shown
    if (waitingEl) {
      waitingEl.style.display = 'block';
      waitingEl.style.visibility = 'visible';
      waitingEl.style.opacity = '1';
      waitingEl.removeAttribute('hidden');
      waitingEl.classList.remove('d-none');
      waitingEl.classList.add('d-block');
      waitingEl.textContent = 'Đang tạo mã thanh toán...';
      waitingEl.className = 'text-center mt-4';
    }
    if (successEl) {
      successEl.style.display = 'none';
      successEl.style.visibility = 'hidden';
      successEl.style.opacity = '0';
      successEl.setAttribute('hidden', '');
      successEl.classList.add('d-none');
      successEl.classList.remove('d-block');
    }
    // Don't hide QR section here - let payment method handler control it
    if (qrImg) {
      qrImg.style.display = 'none';
      qrImg.src = '';
    }

  try {
    // Call SePay API to create dynamic QR code
    const token = localStorage.getItem('token');
    if (!token) {
      throw new Error('Không tìm thấy token đăng nhập');
    }

    console.log("[FRONTEND] 🔄 [updateRestaurantPaymentModal] Creating VietQR QR code for order:", orderId);
    
    // Ưu tiên dùng VietQR (miễn phí), nếu không có thì fallback sang SePay
    let response = await fetch(`${location.origin}/api/simplepayment/create-qr-restaurant-vietqr`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ orderId: orderId })
    });

    // Nếu VietQR không có hoặc lỗi, fallback sang SePay
    if (!response.ok) {
      console.log("[FRONTEND] " + '⚠️ [updateRestaurantPaymentModal] VietQR không khả dụng, fallback sang SePay...');
      response = await fetch(`${location.origin}/api/simplepayment/create-qr-restaurant`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ orderId: orderId })
      });
    }

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Lỗi không xác định' }));
      throw new Error(error.message || `HTTP ${response.status}`);
    }

    const result = await response.json();
    console.log("[FRONTEND] ✅ [updateRestaurantPaymentModal] QR code created:", result);

    // Check if we have QR code - SePay có thể trả về:
    // 1. qrCodeUrl: "https://..."
    // 2. qrCode: Base64 image
    let qrCodeData = result.qrCode || result.qrCodeUrl;
    console.log("[FRONTEND] 🔍 [updateRestaurantPaymentModal] QR Code data type:", typeof qrCodeData);
    console.log("[FRONTEND] 🔍 [updateRestaurantPaymentModal] QR Code data preview:", qrCodeData?.substring(0, 50) || 'NULL');

    if (!result.success) {
      throw new Error(`QR code API error: ${result.message || 'Unknown error'}`);
    }

    // Display QR code from SePay
    if (qrImg) {
      if (qrCodeData) {
        // Case 1: QR code là URL
        if (qrCodeData.startsWith('http://') || qrCodeData.startsWith('https://')) {
          console.log("[FRONTEND] 🌐 [updateRestaurantPaymentModal] QR Code is URL:", qrCodeData);
          qrImg.src = qrCodeData;
          qrImg.style.display = 'block';
          qrImg.alt = `SePay QR - ${orderNumber}`;
          
          qrImg.onerror = function(e) {
            console.error("[FRONTEND] ❌ [updateRestaurantPaymentModal] QR URL failed to load:", e);
            qrImg.style.display = 'none';
            if (waitingEl) {
              waitingEl.textContent = 'Không thể tải QR code từ SePay. Vui lòng thử lại.';
              waitingEl.className = 'text-center mt-4 text-danger';
            }
          };
        }
        // Case 2: QR code là Base64 image
        else if (qrCodeData.startsWith('iVBORw0KGgo') || qrCodeData.startsWith('/9j/4AAQ') || 
                 qrCodeData.startsWith('data:image/') || 
                 /^[A-Za-z0-9+/=]{100,}$/.test(qrCodeData.trim())) {
          console.log("[FRONTEND] 📦 [updateRestaurantPaymentModal] QR Code is Base64 image");
          qrCodeData = qrCodeData.trim().replace(/\s/g, '');
          let qrSrc = qrCodeData;
          if (!qrCodeData.startsWith('data:')) {
            qrSrc = `data:image/png;base64,${qrCodeData}`;
          }
          
          qrImg.src = qrSrc;
          qrImg.style.display = 'block';
          qrImg.alt = `SePay QR - ${orderNumber}`;
          
          qrImg.onerror = function(e) {
            console.error("[FRONTEND] ❌ [updateRestaurantPaymentModal] QR Base64 failed to load:", e);
            qrImg.style.display = 'none';
            if (waitingEl) {
              waitingEl.textContent = 'Không thể tải QR code từ SePay. Vui lòng thử lại.';
              waitingEl.className = 'text-center mt-4 text-danger';
            }
          };
        }
        // Case 3: Không nhận diện được format
        else {
          console.error("[FRONTEND] ❌ [updateRestaurantPaymentModal] Không nhận diện được format QR code");
          qrImg.style.display = 'none';
          if (waitingEl) {
            waitingEl.textContent = 'Định dạng QR code không hợp lệ. Vui lòng thử lại.';
            waitingEl.className = 'text-center mt-4 text-danger';
          }
        }
      } 
      else {
        console.warn("[FRONTEND] ⚠️ [updateRestaurantPaymentModal] Không trả về QR code");
        qrImg.style.display = 'none';
        if (waitingEl) {
          waitingEl.textContent = 'Không trả về QR code. Vui lòng thử lại hoặc liên hệ hỗ trợ.';
          waitingEl.className = 'text-center mt-4 text-danger';
        }
      }
    }

    // Show QR section
    if (qrSection) {
      qrSection.style.display = 'block';
      console.log("[FRONTEND] ✅ [updateRestaurantPaymentModal] QR section displayed");
    }

    // Update bank info from SePay response
    if (result.accountNumber) {
      const bankAccEl = document.getElementById('rpBankAccount');
      if (bankAccEl) {
        bankAccEl.textContent = result.accountNumber;
        console.log("[FRONTEND] ✅ [updateRestaurantPaymentModal] Account Number:", result.accountNumber);
      }
    }
    
    if (result.accountName) {
      const bankNameEl = document.getElementById('rpBankName');
      if (bankNameEl) {
        bankNameEl.textContent = result.accountName;
        console.log("[FRONTEND] ✅ [updateRestaurantPaymentModal] Account Name:", result.accountName);
      }
    }
    
    if (result.bankName) {
      const bankNameEl = document.getElementById('rpBankName');
      if (bankNameEl && !result.accountName) {
        bankNameEl.textContent = result.bankName;
        console.log("[FRONTEND] ✅ [updateRestaurantPaymentModal] Bank Name:", result.bankName);
      }
    }

    // Update amount from response
    if (result.amount && result.amount > 0) {
      const amountEl = document.getElementById('rpAmount');
      if (amountEl) {
        amountEl.textContent = formatCurrency(result.amount);
        console.log("[FRONTEND] ✅ [updateRestaurantPaymentModal] Amount updated:", result.amount);
      }
    }

    // Update content
    const contentEl = document.getElementById('rpContent');
    if (contentEl) contentEl.textContent = result.description || `ORDER${orderId}`;

    // Update waiting message - đảm bảo success bị ẩn và waiting được hiển thị
    if (waitingEl) {
      waitingEl.style.display = 'block';
      waitingEl.style.visibility = 'visible';
      waitingEl.style.opacity = '1';
      waitingEl.removeAttribute('hidden');
      waitingEl.classList.remove('d-none');
      waitingEl.classList.add('d-block');
      waitingEl.textContent = 'Vui lòng quét mã QR để thanh toán';
      waitingEl.className = 'text-center mt-4';
    }
    
    // Đảm bảo success message bị ẩn sau khi tạo QR xong
    const successElAfter = document.getElementById('rpSuccess');
    if (successElAfter) {
      successElAfter.style.display = 'none';
      successElAfter.style.visibility = 'hidden';
      successElAfter.style.opacity = '0';
      successElAfter.setAttribute('hidden', '');
      successElAfter.classList.add('d-none');
      successElAfter.classList.remove('d-block');
      console.log("[FRONTEND] ✅ [updateRestaurantPaymentModal] Success message hidden after QR creation");
    }

    // Store payment info for later use
    window._currentRestaurantPaymentLink = {
      orderId: result.orderId,
      orderCode: result.orderCode,
      vaNumber: result.vaNumber
    };

  } catch (error) {
    console.error("[FRONTEND] ❌ [updateRestaurantPaymentModal] Error creating QR code:", error);
    
    if (waitingEl) {
      waitingEl.style.display = 'block';
      waitingEl.textContent = `Lỗi: ${error.message}. Vui lòng thử lại.`;
      waitingEl.className = 'text-center mt-4 text-danger';
    }
    
    showSimpleToast(`Lỗi tạo mã thanh toán: ${error.message}`, 'danger');
  }
}

/**
 * Start polling để check restaurant order payment status
 */
function startRestaurantPaymentPolling(orderId) {
  // Stop previous polling
  if (window.restaurantPaymentPollingInterval) {
    clearInterval(window.restaurantPaymentPollingInterval);
  }

  console.log("[FRONTEND] 🔄 [RestaurantPaymentPolling] Starting polling for order:", orderId);
  let pollCount = 0;
  const maxPolls = 300; // Poll tối đa 10 phút
  
  // Delay polling lần đầu 3 giây để tránh check ngay khi mở modal
  // (đảm bảo QR code đã được tạo và hiển thị trước khi bắt đầu check)
  setTimeout(() => {
    console.log("[FRONTEND] 🔄 [RestaurantPaymentPolling] Starting first poll after 3s delay...");
    
    window.restaurantPaymentPollingInterval = setInterval(async () => {
      pollCount++;
      try {
        const token = localStorage.getItem('token');
        if (!token) {
          console.warn("[FRONTEND] ⚠️ [RestaurantPaymentPolling] No token found");
          stopRestaurantPaymentPolling();
          return;
        }

        if (pollCount > maxPolls) {
          console.log("[FRONTEND] ⏰ [RestaurantPaymentPolling] Timeout reached after 10 minutes, stopping polling");
          stopRestaurantPaymentPolling();
          return;
        }

        const response = await fetch(`${location.origin}/api/restaurant-orders/${orderId}?_=${Date.now()}`, {
          headers: {
            'Authorization': `Bearer ${token}`
          },
          cache: 'no-store'
        });

        if (!response.ok) {
          console.warn("[FRONTEND] ⚠️ [RestaurantPaymentPolling] Response not OK:", response.status);
          return;
        }

        const order = await response.json();
      
      if (pollCount % 10 === 0 || pollCount === 1) {
        console.log(`[FRONTEND] 🔍 [RestaurantPaymentPolling] Poll #${pollCount} - PaymentStatus: ${order.paymentStatus} (order ${orderId})`);
      }
      
      // Normalize status - chỉ check paymentStatus, không check status
      const rawStatus = String(order.paymentStatus || '').trim();
      const normalizedStatus = rawStatus.toLowerCase();
      
      console.log(`[FRONTEND] 🔍 [RestaurantPaymentPolling] Poll #${pollCount} - Raw paymentStatus: '${rawStatus}', Normalized: '${normalizedStatus}'`);
      console.log(`[FRONTEND] 🔍 [RestaurantPaymentPolling] Full order object:`, {
        orderId: order.orderId,
        paymentStatus: order.paymentStatus,
        status: order.status,
        totalAmount: order.totalAmount
      });

      // Check for "Paid" status - CHỈ chấp nhận chính xác "Paid", không dùng includes()
      // Tránh false positive với "Unpaid" hoặc các status khác
      const isPaid = normalizedStatus === 'paid' || 
                       rawStatus === 'Paid' || 
                       rawStatus === 'PAID';
      
      // Log để debug
      console.log(`[FRONTEND] 🔍 [RestaurantPaymentPolling] isPaid check: ${isPaid} (rawStatus='${rawStatus}', normalizedStatus='${normalizedStatus}')`);
      
      if (isPaid) {
        console.log('[FRONTEND] ✅✅✅ [RestaurantPaymentPolling] ========== PAYMENT DETECTED ==========');
        console.log('[FRONTEND] ✅ [RestaurantPaymentPolling] Payment detected! Status =', rawStatus);
        
        // Stop polling
        stopRestaurantPaymentPolling();
        
        // Show thank you message in modal
        showRestaurantPaymentThankYou(order);
        
        // Show toast notification
        showSimpleToast('✅ Thanh toán thành công! Cảm ơn bạn!', 'success');
        
        console.log('[FRONTEND] ✅✅✅ [RestaurantPaymentPolling] ========== PAYMENT PROCESSING COMPLETE ==========');
      } else {
        if (pollCount % 10 === 0 || pollCount <= 5) {
          console.log(`[FRONTEND] ⏳ [RestaurantPaymentPolling] Still waiting... Status: '${rawStatus}' (poll #${pollCount})`);
        }
      }
    } catch (error) {
      console.error('[FRONTEND] ❌ [RestaurantPaymentPolling] Polling error:', error);
    }
    }, 2000); // Poll mỗi 2 giây
  }, 3000); // Delay 3 giây trước khi bắt đầu polling
  
  window.currentRestaurantPaymentOrderId = orderId;
}

/**
 * Stop polling
 */
function stopRestaurantPaymentPolling() {
  if (window.restaurantPaymentPollingInterval) {
    clearInterval(window.restaurantPaymentPollingInterval);
    window.restaurantPaymentPollingInterval = null;
  }
  window.currentRestaurantPaymentOrderId = null;
}

/**
 * Show payment success (legacy - kept for compatibility)
 */
function showRestaurantPaymentSuccess() {
  console.log("[FRONTEND] 🎉🎉🎉 [showRestaurantPaymentSuccess] ========== STARTING ==========");
  
  const modal = document.getElementById('restaurantPaymentModal');
  if (!modal) {
    console.error("[FRONTEND] ❌ [showRestaurantPaymentSuccess] Modal restaurantPaymentModal not found!");
    return;
  }
  
  const waitingEl = document.getElementById('rpWaiting');
  const successEl = document.getElementById('rpSuccess');
  const qrImg = document.getElementById('rpQRImage');
  const qrSection = document.getElementById('rpQRSection');

  // Hide waiting message
  if (waitingEl) {
    waitingEl.style.display = 'none';
    waitingEl.style.visibility = 'hidden';
    waitingEl.style.opacity = '0';
    waitingEl.setAttribute('hidden', '');
    waitingEl.classList.add('d-none');
    waitingEl.classList.remove('d-block');
  }
  
  // Show success message
  if (successEl) {
    successEl.style.display = 'block';
    successEl.style.visibility = 'visible';
    successEl.style.opacity = '1';
    successEl.removeAttribute('hidden');
    successEl.classList.remove('d-none');
    successEl.classList.add('d-block');
    successEl.setAttribute('style', 'display: block !important; visibility: visible !important; opacity: 1 !important;');
  }
  
  // Hide QR image
  if (qrImg) {
    qrImg.style.display = 'none';
    qrImg.style.visibility = 'hidden';
    qrImg.style.opacity = '0';
    qrImg.setAttribute('hidden', '');
    qrImg.src = '';
    qrImg.classList.add('d-none');
    qrImg.classList.remove('d-block');
  }
  
  // Hide QR section
  if (qrSection) {
    qrSection.style.display = 'none';
    qrSection.style.visibility = 'hidden';
    qrSection.style.opacity = '0';
    qrSection.setAttribute('hidden', '');
    qrSection.classList.add('d-none');
    qrSection.classList.remove('d-block');
  }
  
  console.log("[FRONTEND] ✅✅✅ [showRestaurantPaymentSuccess] ========== COMPLETED ==========");
}

/**
 * Show thank you message after successful QR payment
 */
function showRestaurantPaymentThankYou(order) {
  console.log("[FRONTEND] 🎉 [showRestaurantPaymentThankYou] Showing thank you message for order:", order);
  
  const modal = document.getElementById('restaurantPaymentModal');
  if (!modal) {
    console.error("[FRONTEND] ❌ [showRestaurantPaymentThankYou] Modal restaurantPaymentModal not found!");
    return;
  }
  
  const modalBody = modal.querySelector('.modal-body');
  const modalFooter = modal.querySelector('.modal-footer');
  const modalHeader = modal.querySelector('.modal-header');
  
  if (modalBody && modalFooter && modalHeader) {
    // Update header
    const headerTitle = modalHeader.querySelector('.modal-title');
    const headerCloseBtn = modalHeader.querySelector('.btn-close');
    if (headerTitle) {
      headerTitle.innerHTML = '✅ Cảm ơn bạn đã thanh toán!';
      headerTitle.style.color = '#059669';
    }
    // Ensure close button in header works
    if (headerCloseBtn) {
      headerCloseBtn.setAttribute('onclick', 'closeRestaurantPaymentModal()');
      headerCloseBtn.setAttribute('data-bs-dismiss', 'modal');
    }
    
    // Update body with thank you message
    const orderNumber = order?.orderNumber || `ORD${order?.orderId || ''}`;
    const amount = order?.totalAmount || 0;
    modalBody.innerHTML = `
      <div style="text-align: center; padding: 40px 20px;">
        <div style="font-size: 80px; margin-bottom: 24px;">🎉</div>
        <h3 style="color: #059669; margin-bottom: 16px; font-weight: 700;">Cảm ơn bạn đã thanh toán!</h3>
        <p style="color: #6b7280; margin-bottom: 24px; font-size: 16px; line-height: 1.6;">
          Thanh toán của bạn đã được xác nhận thành công.
        </p>
        <div style="background: #f0fdf4; padding: 20px; border-radius: 12px; border: 2px solid #86efac; margin-bottom: 24px;">
          <div style="margin-bottom: 12px;">
            <strong style="color: #1a1a1a; font-size: 16px;">Mã đơn hàng:</strong>
            <span style="color: #059669; font-size: 18px; font-weight: 700; margin-left: 8px;">${orderNumber}</span>
          </div>
          <div style="margin-bottom: 12px;">
            <strong style="color: #1a1a1a; font-size: 16px;">Số tiền:</strong>
            <span style="color: #059669; font-size: 18px; font-weight: 700; margin-left: 8px;">${formatCurrency(amount)}</span>
          </div>
          <div style="margin-bottom: 12px;">
            <strong style="color: #1a1a1a; font-size: 16px;">Phương thức thanh toán:</strong>
            <span style="color: #059669; font-size: 18px; font-weight: 700; margin-left: 8px;">💳 QR Code</span>
          </div>
          <div>
            <strong style="color: #1a1a1a; font-size: 16px;">Trạng thái:</strong>
            <span style="color: #059669; font-size: 18px; font-weight: 700; margin-left: 8px;">Đã thanh toán</span>
          </div>
        </div>
        <div style="background: #fef3c7; padding: 16px; border-radius: 8px; border: 1px solid #fbbf24;">
          <p style="margin: 0; color: #92400e; font-size: 14px; line-height: 1.6;">
            <strong>💡 Lưu ý:</strong> Đơn hàng của bạn sẽ được chuẩn bị và giao đến địa chỉ đã đăng ký.
          </p>
        </div>
      </div>
    `;
    
    // Update footer - only show close button
    modalFooter.innerHTML = `
      <button type="button" class="btn btn-primary" data-bs-dismiss="modal" onclick="closeRestaurantPaymentModal()" style="padding: 12px 28px; font-size: 16px; font-weight: 600; border-radius: 10px; background: #c8a97e; border: none; width: 100%;">
        <i class="icon-check"></i> Đóng
      </button>
    `;
  }
}

/**
 * Close restaurant payment modal
 */
function closeRestaurantPaymentModal() {
  const modal = document.getElementById('restaurantPaymentModal');
  if (!modal) {
    console.warn("[FRONTEND] " + '⚠️ [closeRestaurantPaymentModal] Modal not found');
    return;
  }
  
  console.log("[FRONTEND] " + '🔄 [closeRestaurantPaymentModal] Closing restaurant payment modal');
  
  // Try multiple methods to close modal
  let closed = false;
  
  // Method 1: Bootstrap 5 - try getInstance first
  if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
    try {
      if (typeof bootstrap.Modal.getInstance === 'function') {
        const bsModal = bootstrap.Modal.getInstance(modal);
        if (bsModal) {
          bsModal.hide();
          closed = true;
          console.log("[FRONTEND] " + '✅ [closeRestaurantPaymentModal] Closed using Bootstrap 5 Modal.getInstance');
        } else {
          const newModal = new bootstrap.Modal(modal);
          newModal.hide();
          closed = true;
          console.log("[FRONTEND] " + '✅ [closeRestaurantPaymentModal] Closed using Bootstrap 5 new Modal instance');
        }
      }
    } catch (e) {
      console.warn("[FRONTEND] " + '⚠️ [closeRestaurantPaymentModal] Bootstrap method failed:', e);
    }
  }
  
  // Method 2: jQuery
  if (!closed && typeof $ !== 'undefined' && $.fn.modal) {
    try {
      $(modal).modal('hide');
      closed = true;
      console.log("[FRONTEND] " + '✅ [closeRestaurantPaymentModal] Closed using jQuery');
    } catch (e) {
      console.warn("[FRONTEND] " + '⚠️ [closeRestaurantPaymentModal] jQuery method failed:', e);
    }
  }
  
  // Method 3: Direct DOM manipulation
  if (!closed) {
    modal.classList.remove('show');
    modal.style.display = 'none';
    document.body.classList.remove('modal-open');
    const backdrop = document.querySelector('.modal-backdrop');
    if (backdrop) backdrop.remove();
    closed = true;
    console.log("[FRONTEND] " + '✅ [closeRestaurantPaymentModal] Closed using direct DOM manipulation');
  }
  
  // Reload page or order list after modal is closed
  setTimeout(() => {
    if (window.location.pathname.includes('order-details')) {
      // If on order details page, reload order details
      if (window.loadOrderDetails && window.currentOrder?.orderId) {
        window.loadOrderDetails(window.currentOrder.orderId);
      } else {
        window.location.reload();
      }
    } else {
      // If on orders list page, reload page
      window.location.reload();
    }
  }, 300);
}

/**
 * Format currency
 */
function formatCurrency(amount) {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0
  }).format(amount);
}

/**
 * Show toast notification
 */
function showSimpleToast(message, type) {
  if (typeof showToast === 'function') {
    showToast(message, type);
  } else {
    console.log("[FRONTEND] " + `[${type.toUpperCase()}] ${message}`);
    alert(message);
  }
}

// Stop polling when modal is closed
document.addEventListener('DOMContentLoaded', () => {
  const modal = document.getElementById('restaurantPaymentModal');
  if (modal) {
    modal.addEventListener('hidden.bs.modal', () => {
      stopRestaurantPaymentPolling();
    });
  }
});

// Export for global use
window.openRestaurantPayment = openRestaurantPayment;
window.closeRestaurantPaymentModal = closeRestaurantPaymentModal;

