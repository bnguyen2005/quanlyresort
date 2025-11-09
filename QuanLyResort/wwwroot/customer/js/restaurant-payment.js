/**
 * Hệ thống thanh toán PayOs cho Restaurant Orders
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

    // Update modal content
    updateRestaurantPaymentModal(orderId, order.orderNumber || `ORD${orderId}`, amount);

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
 * Update modal content - Tạo PayOs payment link
 */
async function updateRestaurantPaymentModal(orderId, orderNumber, amount) {
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
  
  if (waitingEl) {
    waitingEl.style.display = 'block';
    waitingEl.textContent = 'Đang tạo mã thanh toán...';
    waitingEl.className = 'text-center mt-4';
  }
  if (successEl) successEl.style.display = 'none';
  if (qrSection) qrSection.style.display = 'none';

  try {
    // Call PayOs API to create payment link
    const token = localStorage.getItem('token');
    if (!token) {
      throw new Error('Không tìm thấy token đăng nhập');
    }

    console.log("[FRONTEND] 🔄 [updateRestaurantPaymentModal] Creating PayOs payment link for order:", orderId);
    
    const response = await fetch(`${location.origin}/api/simplepayment/create-link-restaurant`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ orderId: orderId })
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Lỗi không xác định' }));
      throw new Error(error.message || `HTTP ${response.status}`);
    }

    const result = await response.json();
    console.log("[FRONTEND] ✅ [updateRestaurantPaymentModal] PayOs payment link created:", result);

    // Check if we have QR code
    let qrCodeData = result.qrCode || result.data?.qrCode || result.qrCodeBase64;
    console.log("[FRONTEND] 🔍 [updateRestaurantPaymentModal] QR Code data type:", typeof qrCodeData);
    console.log("[FRONTEND] 🔍 [updateRestaurantPaymentModal] QR Code data preview:", qrCodeData?.substring(0, 50) || 'NULL');

    if (!result.success) {
      throw new Error(`PayOs API error: ${result.desc || result.message || 'Unknown error'}`);
    }

    // Display QR code from PayOs (tương tự simple-payment.js)
    if (qrImg) {
      if (qrCodeData) {
        // Case 1: QR code là URL
        if (qrCodeData.startsWith('http://') || qrCodeData.startsWith('https://')) {
          console.log("[FRONTEND] 🌐 [updateRestaurantPaymentModal] QR Code is URL:", qrCodeData);
          qrImg.src = qrCodeData;
          qrImg.style.display = 'block';
          qrImg.alt = `PayOs QR - ${orderNumber}`;
          
          qrImg.onerror = function(e) {
            console.error("[FRONTEND] ❌ [updateRestaurantPaymentModal] QR URL failed to load:", e);
            qrImg.style.display = 'none';
            if (waitingEl) {
              waitingEl.textContent = 'Không thể tải QR code từ PayOs. Vui lòng thử lại.';
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
          qrImg.alt = `PayOs QR - ${orderNumber}`;
          
          qrImg.onerror = function(e) {
            console.error("[FRONTEND] ❌ [updateRestaurantPaymentModal] QR Base64 failed to load:", e);
            qrImg.style.display = 'none';
            if (waitingEl) {
              waitingEl.textContent = 'Không thể tải QR code từ PayOs. Vui lòng thử lại.';
              waitingEl.className = 'text-center mt-4 text-danger';
            }
          };
        }
        // Case 3: QR code là QR data string (EMV format)
        else if (/^[0-9A-Za-z\s]+$/.test(qrCodeData.trim()) && qrCodeData.trim().length > 50 && qrCodeData.trim().startsWith('000201')) {
          console.log("[FRONTEND] 📱 [updateRestaurantPaymentModal] QR Code is QR data string (EMV format)");
          
          // Generate QR code image từ QR data string bằng QRCode.js
          const tempContainer = document.createElement('div');
          tempContainer.style.position = 'absolute';
          tempContainer.style.left = '-9999px';
          tempContainer.style.width = '256px';
          tempContainer.style.height = '256px';
          document.body.appendChild(tempContainer);
          
          try {
            tempContainer.innerHTML = '';
            const qrDataToUse = qrCodeData.trim();
            console.log("[FRONTEND] 📱 [updateRestaurantPaymentModal] Using QR data from PayOs:", qrDataToUse.substring(0, 100) + '...');
            
            const qr = new QRCode(tempContainer, {
              text: qrDataToUse,
              width: 256,
              height: 256,
              colorDark: '#000000',
              colorLight: '#ffffff',
              correctLevel: QRCode.CorrectLevel.H
            });
            
            const canvas = tempContainer.querySelector('canvas');
            if (canvas) {
              const dataUrl = canvas.toDataURL('image/png');
              qrImg.src = dataUrl;
              qrImg.style.display = 'block';
              qrImg.alt = `PayOs QR - ${orderNumber}`;
              qrImg.style.border = '4px solid #e9ecef';
              
              console.log("[FRONTEND] ✅ [updateRestaurantPaymentModal] QR code generated from QR data string");
              document.body.removeChild(tempContainer);
            } else {
              throw new Error('QRCode.js không tạo được canvas');
            }
          } catch (error) {
            console.error("[FRONTEND] ❌ [updateRestaurantPaymentModal] Error generating QR from data string:", error);
            if (tempContainer.parentNode) {
              document.body.removeChild(tempContainer);
            }
            qrImg.style.display = 'none';
            if (waitingEl) {
              waitingEl.textContent = 'Không thể tạo QR code từ dữ liệu PayOs. Vui lòng thử lại.';
              waitingEl.className = 'text-center mt-4 text-danger';
            }
          }
        }
        else {
          console.error("[FRONTEND] ❌ [updateRestaurantPaymentModal] Không nhận diện được format QR code");
          qrImg.style.display = 'none';
          if (waitingEl) {
            waitingEl.textContent = 'Định dạng QR code không hợp lệ từ PayOs. Vui lòng thử lại.';
            waitingEl.className = 'text-center mt-4 text-danger';
          }
        }
      } 
      else {
        console.warn("[FRONTEND] ⚠️ [updateRestaurantPaymentModal] PayOs không trả về QR code");
        qrImg.style.display = 'none';
        if (waitingEl) {
          waitingEl.textContent = 'PayOs không trả về QR code. Vui lòng thử lại hoặc liên hệ hỗ trợ.';
          waitingEl.className = 'text-center mt-4 text-danger';
        }
      }
    }

    // Show QR section
    if (qrSection) {
      qrSection.style.display = 'block';
      console.log("[FRONTEND] ✅ [updateRestaurantPaymentModal] QR section displayed");
    }

    // Update bank info
    const expectedAccountNumber = '0901329227';
    if (result.accountNumber) {
      const bankAccEl = document.getElementById('rpBankAccount');
      if (bankAccEl) {
        bankAccEl.textContent = result.accountNumber;
        if (result.accountNumber !== expectedAccountNumber) {
          console.warn("[FRONTEND] ⚠️ [updateRestaurantPaymentModal] Account Number mismatch!");
        } else {
          console.log("[FRONTEND] ✅ [updateRestaurantPaymentModal] Account Number verified:", result.accountNumber);
        }
      }
    } else {
      const bankAccEl = document.getElementById('rpBankAccount');
      if (bankAccEl) {
        bankAccEl.textContent = expectedAccountNumber;
      }
    }
    
    if (result.accountName) {
      const bankNameEl = document.getElementById('rpBankName');
      if (bankNameEl) {
        bankNameEl.textContent = result.accountName;
      }
    } else {
      const bankNameEl = document.getElementById('rpBankName');
      if (bankNameEl) {
        bankNameEl.textContent = 'MB Bank';
      }
    }

    // Update amount from PayOs response
    if (result.amount && result.amount > 0) {
      const amountEl = document.getElementById('rpAmount');
      if (amountEl) {
        amountEl.textContent = formatCurrency(result.amount);
        console.log("[FRONTEND] ✅ [updateRestaurantPaymentModal] Amount updated from PayOs:", result.amount);
      }
    }

    // Update content
    const contentEl = document.getElementById('rpContent');
    if (contentEl) contentEl.textContent = result.description || `ORDER${orderId}`;

    // Update waiting message
    if (waitingEl) {
      waitingEl.style.display = 'block';
      waitingEl.textContent = 'Vui lòng quét mã QR để thanh toán';
      waitingEl.className = 'text-center mt-4';
    }

    // Store payment link info
    window._currentRestaurantPaymentLink = {
      paymentLinkId: result.paymentLinkId,
      orderCode: result.orderCode,
      checkoutUrl: result.checkoutUrl
    };

  } catch (error) {
    console.error("[FRONTEND] ❌ [updateRestaurantPaymentModal] Error creating PayOs payment link:", error);
    
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
      
      // Normalize status
      const rawStatus = String(order.paymentStatus || '').trim();
      const normalizedStatus = rawStatus.toLowerCase();
      
      console.log(`[FRONTEND] 🔍 [RestaurantPaymentPolling] Poll #${pollCount} - Raw status: '${rawStatus}', Normalized: '${normalizedStatus}'`);

      // Check for "Paid" status
      const isPaid = normalizedStatus === 'paid' || 
                       rawStatus === 'Paid' || 
                       rawStatus === 'PAID' ||
                       normalizedStatus.includes('paid');
      
      if (isPaid) {
        console.log('[FRONTEND] ✅✅✅ [RestaurantPaymentPolling] ========== PAYMENT DETECTED ==========');
        console.log('[FRONTEND] ✅ [RestaurantPaymentPolling] Payment detected! Status =', rawStatus);
        
        // Stop polling
        stopRestaurantPaymentPolling();
        
        // Show success UI immediately
        showRestaurantPaymentSuccess();
        
        // Force update lại sau 100ms và 300ms
        setTimeout(() => {
          showRestaurantPaymentSuccess();
        }, 100);
        
        setTimeout(() => {
          showRestaurantPaymentSuccess();
        }, 300);
        
        // Show toast notification
        showSimpleToast('✅ Thanh toán thành công!', 'success');
        
        // Reload order details after 2 seconds
        setTimeout(() => {
          console.log('[FRONTEND] 🔄 [RestaurantPaymentPolling] Reloading page to show updated status...');
          window.location.reload();
        }, 2000);
        
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
 * Show payment success
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

