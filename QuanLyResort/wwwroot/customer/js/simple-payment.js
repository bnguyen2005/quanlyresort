/**
 * Hệ thống thanh toán đơn giản
 * Chỉ cần: QR code + polling check booking status
 */

// Constants - Make them global so they can be used in other scripts
window.BANK_CODE = 'MB';
window.BANK_ACCOUNT = '0901329227';
window.BANK_ACCOUNT_NAME = 'Resort Deluxe';

// Also define as const for local use
const BANK_CODE = window.BANK_CODE;
const BANK_ACCOUNT = window.BANK_ACCOUNT;
const BANK_ACCOUNT_NAME = window.BANK_ACCOUNT_NAME;

// Global state - Make them global so they can be used in other scripts
window.currentPaymentBookingId = null;
window.paymentPollingInterval = null;

// Also define as let for local use
let currentPaymentBookingId = window.currentPaymentBookingId;
let paymentPollingInterval = window.paymentPollingInterval;

/**
 * Mở modal thanh toán
 */
async function openSimplePayment(bookingId) {
  try {
    // Get booking từ list
    const bookings = window._bookings || [];
    const booking = bookings.find(b => String(b.bookingId) === String(bookingId));
    
    if (!booking) {
      showSimpleToast('Không tìm thấy booking', 'danger');
      return;
    }

    // Check if already paid
    if (booking.status === 'Paid') {
      showSimpleToast('Đặt phòng này đã được thanh toán!', 'success');
      return;
    }

    // Get amount - prefer override from booking-details page if available
    let amount = 0;
    const overrides = window._bookingAmountOverrides || {};
    const overrideAmount = overrides ? overrides[Number(bookingId)] : undefined;
    if (typeof overrideAmount === 'number' && overrideAmount > 0) {
      amount = Number(overrideAmount);
      console.log('✅ [openSimplePayment] Using override amount from booking-details:', amount);
    } else {
      amount = Number(booking.estimatedTotalAmount || booking.totalAmount || booking.amount || booking.totalPrice || 0);
    }
    
    // If amount is 0 or invalid, try to calculate from dates and room price
    if (amount <= 0 && booking.checkInDate && booking.checkOutDate) {
      console.log('🔵 [openSimplePayment] Amount is 0, calculating from dates...');
      const checkin = new Date(booking.checkInDate);
      const checkout = new Date(booking.checkOutDate);
      const nights = Math.ceil((checkout - checkin) / (1000 * 60 * 60 * 24));
      
      // Try to get room price from various sources
      let roomPrice = 0;
      
      // Priority 1: From booking object
      if (booking.roomPrice) roomPrice = Number(booking.roomPrice);
      else if (booking.pricePerNight) roomPrice = Number(booking.pricePerNight);
      else if (booking.room?.pricePerNight) roomPrice = Number(booking.room.pricePerNight);
      else if (booking.roomTypeNavigation?.basePrice) roomPrice = Number(booking.roomTypeNavigation.basePrice);
      else if (booking.roomTypeNavigation?.pricePerNight) roomPrice = Number(booking.roomTypeNavigation.pricePerNight);
      
      // Priority 2: From room types cache
      if (roomPrice <= 0 && booking.requestedRoomType && window._roomTypesCache && Array.isArray(window._roomTypesCache)) {
        const roomType = window._roomTypesCache.find(rt => 
          rt.typeName?.toLowerCase() === booking.requestedRoomType?.toLowerCase() ||
          rt.roomTypeName?.toLowerCase() === booking.requestedRoomType?.toLowerCase()
        );
        if (roomType?.basePrice) roomPrice = Number(roomType.basePrice);
        else if (roomType?.pricePerNight) roomPrice = Number(roomType.pricePerNight);
      }
      
      if (nights > 0 && roomPrice > 0) {
        amount = nights * roomPrice;
        console.log(`✅ [openSimplePayment] Calculated amount: ${amount} (${nights} nights × ${roomPrice})`);
        // Update booking object
        booking.estimatedTotalAmount = amount;
      }
    }
    
    // If still 0, try to fetch booking detail
    if (amount <= 0) {
      console.log('🔵 [openSimplePayment] Amount still 0, fetching booking detail...');
      try {
        const token = localStorage.getItem('token');
        const detailUrl = `${location.origin}/api/bookings/${bookingId}?_=${Date.now()}`;
        const detailResp = await fetch(detailUrl, {
          cache: 'no-store',
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });
        
        if (detailResp.ok) {
          const bookingDetail = await detailResp.json();
          amount = Number(bookingDetail?.estimatedTotalAmount ?? bookingDetail?.totalAmount ?? 0);
          
          // If still 0, calculate from dates
          if (amount <= 0 && bookingDetail?.checkInDate && bookingDetail?.checkOutDate) {
            const checkin = new Date(bookingDetail.checkInDate);
            const checkout = new Date(bookingDetail.checkOutDate);
            const nights = Math.ceil((checkout - checkin) / (1000 * 60 * 60 * 24));
            
            let roomPrice = 0;
            if (bookingDetail?.room?.pricePerNight) roomPrice = Number(bookingDetail.room.pricePerNight);
            else if (bookingDetail?.room?.roomTypeNavigation?.basePrice) roomPrice = Number(bookingDetail.room.roomTypeNavigation.basePrice);
            else if (bookingDetail?.roomTypeNavigation?.basePrice) roomPrice = Number(bookingDetail.roomTypeNavigation.basePrice);
            
            if (nights > 0 && roomPrice > 0) {
              amount = nights * roomPrice;
              console.log(`✅ [openSimplePayment] Calculated from detail: ${amount} (${nights} nights × ${roomPrice})`);
            }
          }
          
          // Update booking object
          if (amount > 0) {
            booking.estimatedTotalAmount = amount;
          }
        }
      } catch (e) {
        console.error('❌ Error fetching booking detail:', e);
      }
    }
    
    // Trust backend amount - Database đã được sửa về giá đúng (5,000 VND)
    // Không cần correction nữa
    console.log('✅ [openSimplePayment] Using amount from backend:', amount);
    
    if (amount <= 0) {
      showSimpleToast('Không thể xác định số tiền thanh toán. Vui lòng liên hệ quản trị viên.', 'danger');
      return;
    }

    // Update modal content
    updatePaymentModal(bookingId, booking.bookingCode || `BKG${bookingId}`, amount);

    // Show modal
    const modalElement = document.getElementById('simplePaymentModal');
    if (!modalElement) {
      console.error('❌ Modal element not found: simplePaymentModal');
      showSimpleToast('Lỗi: Không tìm thấy modal thanh toán', 'danger');
      return;
    }
    
    const modal = new bootstrap.Modal(modalElement);
    modal.show();

    // Start polling
    startSimplePolling(bookingId);

    window.currentPaymentBookingId = bookingId;
    currentPaymentBookingId = bookingId;

  } catch (error) {
    console.error('❌ Error opening payment:', error);
    showSimpleToast('Lỗi mở form thanh toán', 'danger');
  }
}

/**
 * Update modal content - Tạo PayOs payment link
 */
async function updatePaymentModal(bookingId, bookingCode, amount) {
  // Booking code
  const codeEl = document.getElementById('spBookingCode');
  if (codeEl) codeEl.textContent = bookingCode;

  // Amount
  const amountEl = document.getElementById('spAmount');
  if (amountEl) amountEl.textContent = formatCurrency(amount);

  // Show loading
  const qrImg = document.getElementById('spQRImage');
  const qrSection = document.getElementById('spQRSection');
  const waitingEl = document.getElementById('spWaiting');
  const successEl = document.getElementById('spSuccess');
  
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

    console.log('🔄 [updatePaymentModal] Creating PayOs payment link for booking:', bookingId);
    
    const response = await fetch(`${location.origin}/api/simplepayment/create-link`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ bookingId: bookingId })
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Lỗi không xác định' }));
      throw new Error(error.message || `HTTP ${response.status}`);
    }

    const result = await response.json();
    console.log('✅ [updatePaymentModal] PayOs payment link created:', result);

    if (!result.success || !result.qrCode) {
      throw new Error('Không thể tạo mã thanh toán từ PayOs');
    }

    // Display QR code from PayOs (base64)
    if (qrImg) {
      // PayOs returns base64 QR code image
      qrImg.src = `data:image/png;base64,${result.qrCode}`;
      qrImg.style.display = 'block';
      qrImg.alt = `PayOs QR - ${bookingCode}`;
      console.log('✅ [updatePaymentModal] QR image set from PayOs');
    }

    // Show QR section
    if (qrSection) {
      qrSection.style.display = 'block';
      console.log('✅ [updatePaymentModal] QR section displayed');
    }

    // Update bank info if available
    if (result.accountNumber) {
      const bankAccEl = document.getElementById('spBankAccount');
      if (bankAccEl) bankAccEl.textContent = result.accountNumber;
    }
    if (result.accountName) {
      const bankNameEl = document.getElementById('spBankName');
      if (bankNameEl) bankNameEl.textContent = result.accountName;
    }

    // Update content
    const contentEl = document.getElementById('spContent');
    if (contentEl) contentEl.textContent = result.description || `BOOKING${bookingId}`;

    // Update waiting message
    if (waitingEl) {
      waitingEl.style.display = 'block';
      waitingEl.textContent = 'Vui lòng quét mã QR để thanh toán';
      waitingEl.className = 'text-center mt-4';
    }

    // Store payment link info for later use
    window._currentPaymentLink = {
      paymentLinkId: result.paymentLinkId,
      orderCode: result.orderCode,
      checkoutUrl: result.checkoutUrl
    };

  } catch (error) {
    console.error('❌ [updatePaymentModal] Error creating PayOs payment link:', error);
    
    // Show error message
    if (waitingEl) {
      waitingEl.style.display = 'block';
      waitingEl.textContent = `Lỗi: ${error.message}. Vui lòng thử lại.`;
      waitingEl.className = 'text-center mt-4 text-danger';
    }
    
    showSimpleToast(`Lỗi tạo mã thanh toán: ${error.message}`, 'danger');
  }
}

/**
 * Start polling để check booking status
 */
function startSimplePolling(bookingId) {
  // Stop previous polling
  if (window.paymentPollingInterval) {
    clearInterval(window.paymentPollingInterval);
  }

  // Poll every 2 seconds (tăng tần suất để detect nhanh hơn)
  console.log('🔄 [SimplePolling] Starting polling for booking:', bookingId);
  window.paymentPollingInterval = setInterval(async () => {
    try {
      const token = localStorage.getItem('token');
      if (!token) {
        console.warn('⚠️ [SimplePolling] No token found');
        return;
      }

      const response = await fetch(`${location.origin}/api/bookings/${bookingId}?_=${Date.now()}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        },
        cache: 'no-store'
      });

      if (!response.ok) {
        console.warn('⚠️ [SimplePolling] Response not OK:', response.status);
        return;
      }

      const booking = await response.json();
      
      // Log full booking object for debugging
      console.log('🔍 [SimplePolling] Full booking response:', booking);
      console.log('🔍 [SimplePolling] Booking status (raw):', booking.status, 'Type:', typeof booking.status);
      console.log('🔍 [SimplePolling] Booking status (trimmed):', String(booking.status || '').trim());
      
      const currentStatus = String(booking.status || '').trim().toLowerCase();
      console.log('🔍 [SimplePolling] Booking status (lowercase):', currentStatus, 'for booking:', bookingId);
      
      // Check status (case-insensitive and handle different formats)
      // Also check for "Paid" with capital P
      if (currentStatus === 'paid' || booking.status === 'Paid' || booking.status === 'PAID') {
        console.log('✅ [SimplePolling] Payment detected! Status = Paid, stopping polling...');
        console.log('✅ [SimplePolling] Full booking object:', booking);
        
        // Stop polling first
        stopSimplePolling();
        
        // Show success UI immediately
        showPaymentSuccess();
        
        // Show toast notification
        showSimpleToast('✅ Thanh toán thành công!', 'success');
        
        // Force UI update
        const modal = document.getElementById('simplePaymentModal');
        if (modal) {
          // Trigger a reflow to ensure CSS updates
          modal.offsetHeight;
        }
        
        // Reload bookings and close modal after 2 seconds
        setTimeout(() => {
          if (window.loadBookings) {
            window.loadBookings();
          }
          const modalInstance = bootstrap.Modal.getInstance(document.getElementById('simplePaymentModal'));
          if (modalInstance) {
            modalInstance.hide();
          }
        }, 2000);
      } else {
        // Log status for debugging
        console.log('⏳ [SimplePolling] Still waiting... Status:', booking.status);
      }
    } catch (error) {
      console.error('❌ [SimplePolling] Polling error:', error);
    }
  }, 2000); // 2 seconds - tăng tần suất để detect payment nhanh hơn
  
  // Update local variable
  paymentPollingInterval = window.paymentPollingInterval;
}

/**
 * Stop polling
 */
function stopSimplePolling() {
  if (window.paymentPollingInterval) {
    clearInterval(window.paymentPollingInterval);
    window.paymentPollingInterval = null;
  }
  if (paymentPollingInterval) {
    clearInterval(paymentPollingInterval);
    paymentPollingInterval = null;
  }
  window.currentPaymentBookingId = null;
  currentPaymentBookingId = null;
}

/**
 * Show payment success
 */
function showPaymentSuccess() {
  console.log('🎉 [showPaymentSuccess] Showing payment success...');
  
  const modal = document.getElementById('simplePaymentModal');
  if (!modal) {
    console.error('❌ [showPaymentSuccess] Modal simplePaymentModal not found!');
    return;
  }
  
  const waitingEl = document.getElementById('spWaiting');
  const successEl = document.getElementById('spSuccess');
  const qrImg = document.getElementById('spQRImage');
  const qrSection = document.getElementById('spQRSection');

  // Hide waiting message
  if (waitingEl) {
    waitingEl.style.display = 'none';
    console.log('✅ [showPaymentSuccess] Hidden waiting message');
  } else {
    console.warn('⚠️ [showPaymentSuccess] spWaiting element not found');
  }
  
  // Show success message
  if (successEl) {
    successEl.style.display = 'block';
    successEl.style.visibility = 'visible';
    successEl.style.opacity = '1';
    console.log('✅ [showPaymentSuccess] Showed success message');
  } else {
    console.warn('⚠️ [showPaymentSuccess] spSuccess element not found');
  }
  
  // Hide QR image
  if (qrImg) {
    qrImg.style.display = 'none';
    qrImg.style.visibility = 'hidden';
    console.log('✅ [showPaymentSuccess] Hidden QR image');
  } else {
    console.warn('⚠️ [showPaymentSuccess] spQRImage element not found');
  }
  
  // Hide QR section
  if (qrSection) {
    qrSection.style.display = 'none';
    qrSection.style.visibility = 'hidden';
    console.log('✅ [showPaymentSuccess] Hidden QR section');
  } else {
    console.warn('⚠️ [showPaymentSuccess] spQRSection element not found');
  }
  
  // Force modal to update
  if (modal.classList.contains('show')) {
    console.log('✅ [showPaymentSuccess] Modal is visible, UI should update immediately');
  }
  
  console.log('✅ [showPaymentSuccess] Completed');
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
 * Show toast notification (use existing showToast if available)
 */
function showSimpleToast(message, type) {
  if (typeof showToast === 'function') {
    showToast(message, type);
  } else {
    console.log(`[${type.toUpperCase()}] ${message}`);
    // Fallback: simple alert
    alert(message);
  }
}

// Stop polling when modal is closed
document.addEventListener('DOMContentLoaded', () => {
  const modal = document.getElementById('simplePaymentModal');
  if (modal) {
    modal.addEventListener('hidden.bs.modal', () => {
      stopSimplePolling();
    });
  }
});

// Export for global use
window.openSimplePayment = openSimplePayment;

