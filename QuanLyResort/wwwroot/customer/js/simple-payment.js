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
    let booking = bookings.find(b => String(b.bookingId) === String(bookingId));
    
    // If not found in list, try to fetch from API (for booking-details page)
    if (!booking) {
      console.log("[FRONTEND] " + '🔵 [openSimplePayment] Booking not found in _bookings, fetching from API...');
      try {
        const token = localStorage.getItem('token');
        if (token) {
          const url = `${location.origin}/api/bookings/${bookingId}?_=${Date.now()}`;
          const resp = await fetch(url, {
            cache: 'no-store',
            headers: { 'Authorization': `Bearer ${token}` }
          });
          if (resp.ok) {
            booking = await resp.json();
            // Add to _bookings for future use
            if (!window._bookings) window._bookings = [];
            window._bookings.push(booking);
            console.log("[FRONTEND] " + '✅ [openSimplePayment] Booking fetched from API and added to _bookings');
          }
        }
      } catch (e) {
        console.error("[FRONTEND] " + '❌ [openSimplePayment] Error fetching booking:', e);
      }
    }
    
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
      console.log('[FRONTEND] ✅ [openSimplePayment] Using override amount from booking-details:', amount);
    } else {
      amount = Number(booking.estimatedTotalAmount || booking.totalAmount || booking.amount || booking.totalPrice || 0);
    }
    
    // If amount is 0 or invalid, try to calculate from dates and room price
    if (amount <= 0 && booking.checkInDate && booking.checkOutDate) {
      console.log("[FRONTEND] " + '🔵 [openSimplePayment] Amount is 0, calculating from dates...');
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
        console.log("[FRONTEND] " + `✅ [openSimplePayment] Calculated amount: ${amount} (${nights} nights × ${roomPrice})`);
        // Update booking object
        booking.estimatedTotalAmount = amount;
      }
    }
    
    // If still 0, try to fetch booking detail
    if (amount <= 0) {
      console.log("[FRONTEND] " + '🔵 [openSimplePayment] Amount still 0, fetching booking detail...');
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
              console.log("[FRONTEND] " + `✅ [openSimplePayment] Calculated from detail: ${amount} (${nights} nights × ${roomPrice})`);
            }
          }
          
          // Update booking object
          if (amount > 0) {
            booking.estimatedTotalAmount = amount;
          }
        }
      } catch (e) {
        console.error("[FRONTEND] " + '❌ Error fetching booking detail:', e);
      }
    }
    
    // Trust backend amount - Database đã được sửa về giá đúng (5,000 VND)
    // Không cần correction nữa
    console.log("[FRONTEND] " + '✅ [openSimplePayment] Using amount from backend:', amount);
    
    if (amount <= 0) {
      showSimpleToast('Không thể xác định số tiền thanh toán. Vui lòng liên hệ quản trị viên.', 'danger');
      return;
    }

    // Check payment method - if PayAtHotel or Cash, show hotel payment confirmation instead of QR
    // Payment method can be in booking object, invoice, SpecialRequests (JSON), or need to fetch from API
    let paymentMethod = booking.paymentMethod || booking.PaymentMethod;
    console.log("[FRONTEND] " + '🔍 [openSimplePayment] Step 1 - Direct paymentMethod:', paymentMethod);
    
    // If not found, try to parse from SpecialRequests (JSON string)
    if (!paymentMethod && booking.specialRequests) {
      console.log("[FRONTEND] " + '🔍 [openSimplePayment] Step 2 - Checking SpecialRequests:', booking.specialRequests);
      try {
        const specialRequests = typeof booking.specialRequests === 'string' 
          ? JSON.parse(booking.specialRequests) 
          : booking.specialRequests;
        console.log("[FRONTEND] " + '🔍 [openSimplePayment] Parsed SpecialRequests:', specialRequests);
        if (specialRequests && typeof specialRequests === 'object') {
          paymentMethod = specialRequests.paymentMethod || specialRequests.PaymentMethod;
          console.log("[FRONTEND] " + '🔍 [openSimplePayment] Found paymentMethod in SpecialRequests:', paymentMethod);
        }
      } catch (e) {
        console.warn("[FRONTEND] " + '⚠️ [openSimplePayment] Could not parse SpecialRequests as JSON:', e);
        // If parsing fails, check if it contains payment method as plain text
        const specialRequestsStr = String(booking.specialRequests || '');
        if (specialRequestsStr.includes('PayAtHotel') || specialRequestsStr.includes('"paymentMethod":"PayAtHotel"')) {
          paymentMethod = 'PayAtHotel';
          console.log("[FRONTEND] " + '🔍 [openSimplePayment] Found PayAtHotel in SpecialRequests string');
        } else if (specialRequestsStr.includes('Cash') || specialRequestsStr.includes('"paymentMethod":"Cash"')) {
          paymentMethod = 'Cash';
          console.log("[FRONTEND] " + '🔍 [openSimplePayment] Found Cash in SpecialRequests string');
        }
      }
    }
    
    // If not found in booking, check invoice
    if (!paymentMethod && booking.invoice) {
      paymentMethod = booking.invoice.paymentMethod || booking.invoice.PaymentMethod;
    }
    
    // If still not found, try to fetch booking detail from API
    if (!paymentMethod) {
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
          paymentMethod = bookingDetail.paymentMethod || bookingDetail.PaymentMethod;
          
          // Try to parse from SpecialRequests in detail
          if (!paymentMethod && bookingDetail.specialRequests) {
            try {
              const specialRequests = typeof bookingDetail.specialRequests === 'string' 
                ? JSON.parse(bookingDetail.specialRequests) 
                : bookingDetail.specialRequests;
              if (specialRequests && typeof specialRequests === 'object') {
                paymentMethod = specialRequests.paymentMethod || specialRequests.PaymentMethod;
              }
            } catch (e) {
              console.warn("[FRONTEND] " + '⚠️ Could not parse SpecialRequests:', e);
            }
          }
          
          if (bookingDetail.invoice) {
            paymentMethod = paymentMethod || bookingDetail.invoice.paymentMethod || bookingDetail.invoice.PaymentMethod;
          }
        }
      } catch (e) {
        console.error("[FRONTEND] " + '❌ Error fetching booking detail for payment method:', e);
      }
    }
    
    // Default to QR if not found
    paymentMethod = paymentMethod || 'QR';
    console.log("[FRONTEND] " + '🔍 [openSimplePayment] Payment method:', paymentMethod, 'from booking:', booking);
    
    // Normalize payment method: PayAtHotel -> Cash, BankTransfer -> QR
    if (paymentMethod === 'PayAtHotel' || paymentMethod === 'BankTransfer') {
      if (paymentMethod === 'PayAtHotel') paymentMethod = 'Cash';
      if (paymentMethod === 'BankTransfer') paymentMethod = 'QR';
    }
    
    if (paymentMethod === 'Cash') {
      // Show hotel payment confirmation modal instead of QR
      showHotelPaymentConfirmation(bookingId, booking.bookingCode || `BKG${bookingId}`, amount);
      return;
    }

    // Update modal content for QR payment
    updatePaymentModal(bookingId, booking.bookingCode || `BKG${bookingId}`, amount, booking);

    // Show modal
    const modalElement = document.getElementById('simplePaymentModal');
    if (!modalElement) {
      console.error("[FRONTEND] " + '❌ Modal element not found: simplePaymentModal');
      showSimpleToast('Lỗi: Không tìm thấy modal thanh toán', 'danger');
      return;
    }
    
    // Show modal - compatible with Bootstrap 4 and 5
    try {
      if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
        const modal = new bootstrap.Modal(modalElement);
        modal.show();
      } else if (typeof $ !== 'undefined' && $.fn.modal) {
        // jQuery fallback
        $(modalElement).modal('show');
      } else {
        // Direct show fallback
        modalElement.classList.add('show');
        modalElement.style.display = 'block';
        document.body.classList.add('modal-open');
        // Add backdrop
        const backdrop = document.createElement('div');
        backdrop.className = 'modal-backdrop fade show';
        document.body.appendChild(backdrop);
      }
    } catch (e) {
      console.error("[FRONTEND] " + '❌ Error showing modal:', e);
      // Fallback: direct show
      modalElement.classList.add('show');
      modalElement.style.display = 'block';
      document.body.classList.add('modal-open');
    }

    // Start polling
    startSimplePolling(bookingId);

    window.currentPaymentBookingId = bookingId;
    currentPaymentBookingId = bookingId;

  } catch (error) {
    console.error("[FRONTEND] " + '❌ Error opening payment:', error);
    showSimpleToast('Lỗi mở form thanh toán', 'danger');
  }
}

/**
 * Update modal content - Tạo SePay QR code động
 */
async function updatePaymentModal(bookingId, bookingCode, amount, booking) {
  // Booking code
  const codeEl = document.getElementById('spBookingCode');
  if (codeEl) codeEl.textContent = bookingCode;

  // Customer info
  const nameEl = document.getElementById('spCustomerName');
  if (nameEl && booking) {
    nameEl.textContent = booking.customer?.fullName || booking.fullName || '-';
  }
  const phoneEl = document.getElementById('spCustomerPhone');
  if (phoneEl && booking) {
    phoneEl.textContent = booking.customer?.phoneNumber || booking.phoneNumber || '-';
  }
  const emailEl = document.getElementById('spCustomerEmail');
  if (emailEl && booking) {
    emailEl.textContent = booking.customer?.email || booking.email || '-';
  }

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
    // Call SePay API to create dynamic QR code
    const token = localStorage.getItem('token');
    if (!token) {
      throw new Error('Không tìm thấy token đăng nhập');
    }

    console.log("[FRONTEND] " + '🔄 [updatePaymentModal] Creating VietQR QR code for booking:', bookingId);
    
    // Ưu tiên dùng VietQR (miễn phí), nếu không có thì fallback sang SePay
    let response = await fetch(`${location.origin}/api/simplepayment/create-qr-booking-vietqr`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ bookingId: bookingId })
    });

    // Nếu VietQR không có hoặc lỗi, fallback sang SePay
    if (!response.ok) {
      console.log("[FRONTEND] " + '⚠️ [updatePaymentModal] VietQR không khả dụng, fallback sang SePay...');
      response = await fetch(`${location.origin}/api/simplepayment/create-qr-booking`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ bookingId: bookingId })
      });
    }

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Lỗi không xác định' }));
      throw new Error(error.message || `HTTP ${response.status}`);
    }

    const result = await response.json();
    console.log("[FRONTEND] " + '✅ [updatePaymentModal] QR code created:', result);
    console.log("[FRONTEND] " + '🔍 [updatePaymentModal] Full response:', JSON.stringify(result, null, 2));

    // Check if we have QR code - SePay có thể trả về:
    // 1. qrCodeUrl: "https://..."
    // 2. qrCode: Base64 image
    let qrCodeData = result.qrCode || result.qrCodeUrl;
    console.log("[FRONTEND] " + '🔍 [updatePaymentModal] QR Code data type:', typeof qrCodeData);
    console.log("[FRONTEND] " + '🔍 [updatePaymentModal] QR Code data length:', qrCodeData?.length || 0);
    console.log("[FRONTEND] " + '🔍 [updatePaymentModal] QR Code data preview:', qrCodeData?.substring(0, 50) || 'NULL');
    console.log("[FRONTEND] " + '🔍 [updatePaymentModal] Has checkoutUrl:', !!result.checkoutUrl);

    if (!result.success) {
      throw new Error(`QR code API error: ${result.message || 'Unknown error'}`);
    }

    // Display QR code (VietQR hoặc SePay)
    if (qrImg) {
      if (qrCodeData) {
        // Case 1: QR code là URL (https://...) - VietQR hoặc SePay
        if (qrCodeData.startsWith('http://') || qrCodeData.startsWith('https://')) {
          console.log("[FRONTEND] " + '🌐 [updatePaymentModal] QR Code is URL:', qrCodeData);
          qrImg.src = qrCodeData;
          qrImg.style.display = 'block';
          qrImg.alt = `QR Code - ${bookingCode}`;
          
          qrImg.onerror = function(e) {
            console.error("[FRONTEND] " + '❌ [updatePaymentModal] QR URL failed to load:', e);
            console.error("[FRONTEND] " + '❌ [updatePaymentModal] QR URL:', qrCodeData);
            qrImg.style.display = 'none';
            if (waitingEl) {
              waitingEl.textContent = 'Không thể tải QR code. Vui lòng thử lại.';
              waitingEl.className = 'text-center mt-4 text-danger';
            }
          };
          
          qrImg.onload = function() {
            console.log("[FRONTEND] " + '✅ [updatePaymentModal] QR URL loaded successfully');
            qrImg.style.border = '4px solid #e9ecef';
          };
        }
        // Case 2: QR code là Base64 image (PNG/JPEG) - SePay
        else if (qrCodeData.startsWith('iVBORw0KGgo') || qrCodeData.startsWith('/9j/4AAQ') || 
                 qrCodeData.startsWith('data:image/') || 
                 /^[A-Za-z0-9+/=]{100,}$/.test(qrCodeData.trim())) {
          console.log("[FRONTEND] " + '📦 [updatePaymentModal] QR Code is Base64 image (SePay)');
          // Remove any whitespace/newlines from base64 string
          qrCodeData = qrCodeData.trim().replace(/\s/g, '');
          
          // Check if it's already a data URL
          let qrSrc = qrCodeData;
          if (!qrCodeData.startsWith('data:')) {
            // Add data URL prefix if not present
            qrSrc = `data:image/png;base64,${qrCodeData}`;
          }
          
          console.log("[FRONTEND] " + '🖼️ [updatePaymentModal] Setting QR image src (first 100 chars):', qrSrc.substring(0, 100));
          
          qrImg.src = qrSrc;
          qrImg.style.display = 'block';
          qrImg.alt = `QR Code - ${bookingCode}`;
          
          qrImg.onerror = function(e) {
            console.error("[FRONTEND] " + '❌ [updatePaymentModal] QR Base64 failed to load:', e);
            console.error("[FRONTEND] " + '❌ [updatePaymentModal] Failed src (first 200 chars):', qrSrc.substring(0, 200));
            qrImg.style.display = 'none';
            if (waitingEl) {
              waitingEl.textContent = 'Không thể tải QR code từ SePay. Vui lòng thử lại.';
              waitingEl.className = 'text-center mt-4 text-danger';
            }
          };
          
          qrImg.onload = function() {
            console.log("[FRONTEND] " + '✅ [updatePaymentModal] QR Base64 loaded successfully');
            qrImg.style.border = '4px solid #e9ecef';
          };
        }
        // Case 3: Không nhận diện được format
        else {
          console.error("[FRONTEND] " + '❌ [updatePaymentModal] Không nhận diện được format QR code');
          console.error("[FRONTEND] " + '❌ [updatePaymentModal] QR data preview:', qrCodeData?.substring(0, 100) || 'NULL');
          qrImg.style.display = 'none';
          if (waitingEl) {
            waitingEl.textContent = 'Định dạng QR code không hợp lệ. Vui lòng thử lại.';
            waitingEl.className = 'text-center mt-4 text-danger';
          }
        }
      }
      // Case 4: Không có QR code
      else {
        console.warn("[FRONTEND] " + '⚠️ [updatePaymentModal] Không trả về QR code');
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
      console.log("[FRONTEND] " + '✅ [updatePaymentModal] QR section displayed');
    }

    // Update bank info from SePay response
    if (result.accountNumber) {
      const bankAccEl = document.getElementById('spBankAccount');
      if (bankAccEl) {
        bankAccEl.textContent = result.accountNumber;
        console.log("[FRONTEND] " + '✅ [updatePaymentModal] Account Number: ' + result.accountNumber);
      }
    }
    
    if (result.accountName) {
      const bankNameEl = document.getElementById('spBankName');
      if (bankNameEl) {
        bankNameEl.textContent = result.accountName;
        console.log("[FRONTEND] " + '✅ [updatePaymentModal] Account Name: ' + result.accountName);
      }
    }
    
    if (result.bankName) {
      const bankNameEl = document.getElementById('spBankName');
      if (bankNameEl && !result.accountName) {
        bankNameEl.textContent = result.bankName;
        console.log("[FRONTEND] " + '✅ [updatePaymentModal] Bank Name: ' + result.bankName);
      }
    }

    // Update amount from response
    if (result.amount && result.amount > 0) {
      const amountEl = document.getElementById('spAmount');
      if (amountEl) {
        amountEl.textContent = formatCurrency(result.amount);
        console.log("[FRONTEND] " + '✅ [updatePaymentModal] Amount updated:', result.amount);
      }
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

    // Store payment info for later use
    window._currentPaymentLink = {
      orderId: result.orderId,
      orderCode: result.orderCode,
      vaNumber: result.vaNumber
    };

  } catch (error) {
    console.error("[FRONTEND] " + '❌ [updatePaymentModal] Error creating QR code:', error);
    
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
 * Generate QR code from checkoutUrl using QRCode.js library
 * Fallback khi SePay không trả về QR code
 */
function generateQRFromCheckoutUrl(checkoutUrl, container) {
  if (!checkoutUrl) {
    console.error("[FRONTEND] " + '❌ [generateQRFromCheckoutUrl] No checkoutUrl provided');
    return;
  }

  if (!container) {
    console.error("[FRONTEND] " + '❌ [generateQRFromCheckoutUrl] No container provided');
    return;
  }

  console.log("[FRONTEND] " + '🔄 [generateQRFromCheckoutUrl] Generating QR code from checkoutUrl:', checkoutUrl);

  // Check if QRCode.js is loaded
  if (typeof QRCode === 'undefined') {
    console.error("[FRONTEND] " + '❌ [generateQRFromCheckoutUrl] QRCode.js library not loaded');
    // Fallback: show link button
    container.innerHTML = `
      <div class="text-center">
        <a href="${checkoutUrl}" target="_blank" class="btn btn-primary btn-lg">
          <i class="icon-credit-card"></i> Click để thanh toán qua SePay
        </a>
        <p class="mt-2 text-muted">QR code không khả dụng. Vui lòng click nút trên để thanh toán.</p>
      </div>
    `;
    return;
  }

  // Clear container
  container.innerHTML = '';

  // Create QR code container
  const qrContainer = document.createElement('div');
  qrContainer.id = 'qrcode-' + Date.now();
  qrContainer.style.display = 'inline-block';
  qrContainer.style.padding = '15px';
  qrContainer.style.background = 'white';
  qrContainer.style.borderRadius = '15px';
  qrContainer.style.border = '4px solid #e9ecef';
  container.appendChild(qrContainer);

  try {
    // Generate QR code
    new QRCode(qrContainer, {
      text: checkoutUrl,
      width: 300,
      height: 300,
      colorDark: '#000000',
      colorLight: '#ffffff',
      correctLevel: QRCode.CorrectLevel.H
    });

    console.log("[FRONTEND] " + '✅ [generateQRFromCheckoutUrl] QR code generated successfully');

    // Add click handler to open checkout URL
    qrContainer.style.cursor = 'pointer';
    qrContainer.title = 'Click để mở trang thanh toán';
    qrContainer.onclick = function() {
      window.open(checkoutUrl, '_blank');
    };
  } catch (error) {
    console.error("[FRONTEND] " + '❌ [generateQRFromCheckoutUrl] Error generating QR code:', error);
    // Fallback: show link button
    container.innerHTML = `
      <div class="text-center">
        <a href="${checkoutUrl}" target="_blank" class="btn btn-primary btn-lg">
          <i class="icon-credit-card"></i> Click để thanh toán qua SePay
        </a>
        <p class="mt-2 text-muted">QR code không khả dụng. Vui lòng click nút trên để thanh toán.</p>
      </div>
    `;
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
  console.log("[FRONTEND] " + '🔄 [SimplePolling] Starting polling for booking:', bookingId);
  let pollCount = 0;
  const maxPolls = 300; // Poll tối đa 10 phút (300 * 2s = 600s)
  
  window.paymentPollingInterval = setInterval(async () => {
    pollCount++;
    try {
      const token = localStorage.getItem('token');
      if (!token) {
        console.warn("[FRONTEND] " + '⚠️ [SimplePolling] No token found');
        stopSimplePolling();
        return;
      }

      // Timeout sau 10 phút
      if (pollCount > maxPolls) {
        console.log("[FRONTEND] " + '⏰ [SimplePolling] Timeout reached after 10 minutes, stopping polling');
        stopSimplePolling();
        return;
      }

      const response = await fetch(`${location.origin}/api/bookings/${bookingId}?_=${Date.now()}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        },
        cache: 'no-store'
      });

      if (!response.ok) {
        console.warn("[FRONTEND] " + '⚠️ [SimplePolling] Response not OK:', response.status);
        return;
      }

      const booking = await response.json();
      
      // Log mỗi 10 lần poll để không spam console, nhưng luôn log lần đầu
      if (pollCount % 10 === 0 || pollCount === 1) {
        console.log(`[FRONTEND] 🔍 [SimplePolling] Poll #${pollCount} - Status: ${booking.status} (booking ${bookingId})`);
        console.log(`[FRONTEND] 🔍 [SimplePolling] Full booking response:`, JSON.stringify(booking, null, 2));
      }
      
      // Normalize status để check (case-insensitive, trim whitespace)
      const rawStatus = String(booking.status || '').trim();
      const normalizedStatus = rawStatus.toLowerCase();
      
      console.log(`[FRONTEND] 🔍 [SimplePolling] Poll #${pollCount} - Raw status: '${rawStatus}', Normalized: '${normalizedStatus}'`);

      // Check for "Paid" status (case-insensitive, với nhiều variations)
      const isPaid = normalizedStatus === 'paid' || 
                     rawStatus === 'Paid' || 
                     rawStatus === 'PAID' ||
                     normalizedStatus.includes('paid');
      
      console.log(`[FRONTEND] 🔍 [SimplePolling] Poll #${pollCount} - isPaid check: ${isPaid} (normalizedStatus='${normalizedStatus}', rawStatus='${rawStatus}')`);
      
      if (isPaid) {
        console.log('[FRONTEND] ✅✅✅ [SimplePolling] ========== PAYMENT DETECTED ==========');
        console.log('[FRONTEND] ✅ [SimplePolling] Payment detected! Status =', rawStatus, '(normalized:', normalizedStatus + ')');
        console.log('[FRONTEND] ✅ [SimplePolling] Poll count:', pollCount);
        console.log('[FRONTEND] ✅ [SimplePolling] Full booking object:', JSON.stringify(booking, null, 2));
        
        // Stop polling first
        console.log('[FRONTEND] 🔄 [SimplePolling] Stopping polling...');
        stopSimplePolling();
        
        // Show thank you message in modal
        showBookingPaymentThankYou(booking);
        
        // Show toast notification
        console.log('[FRONTEND] 🎉 [SimplePolling] Showing toast notification...');
        showSimpleToast('✅ Thanh toán thành công! Cảm ơn bạn!', 'success');
        
        console.log('[FRONTEND] ✅✅✅ [SimplePolling] ========== PAYMENT PROCESSING COMPLETE ==========');
        
        // Option 2: Đóng modal sau 5 giây (nếu không muốn reload trang)
        // Uncomment dòng dưới và comment Option 1 nếu muốn dùng cách này
        /*
        setTimeout(() => {
          console.log('[FRONTEND] 🔄 [SimplePolling] Closing modal after 5 seconds...');
          hideModalDirectly(document.getElementById('simplePaymentModal'));
        }, 5000);
        */
        
        console.log('[FRONTEND] ✅✅✅ [SimplePolling] ========== PAYMENT PROCESSING COMPLETE ==========');
      } else {
        // Log status mỗi 10 lần poll hoặc mỗi lần để debug
        if (pollCount % 10 === 0 || pollCount <= 5) {
          console.log(`[FRONTEND] ⏳ [SimplePolling] Still waiting... Status: '${rawStatus}' (normalized: '${normalizedStatus}', poll #${pollCount})`);
          console.log(`[FRONTEND] ⏳ [SimplePolling] Booking object keys:`, Object.keys(booking));
          console.log(`[FRONTEND] ⏳ [SimplePolling] Booking.status type:`, typeof booking.status);
          console.log(`[FRONTEND] ⏳ [SimplePolling] Booking.status value:`, booking.status);
        }
      }
    } catch (error) {
      console.error('[FRONTEND] ❌ [SimplePolling] Polling error:', error);
      console.error('[FRONTEND] ❌ [SimplePolling] Error details:', {
        message: error.message,
        stack: error.stack,
        pollCount: pollCount,
        bookingId: bookingId
      });
    }
  }, 1000); // Poll mỗi 1 giây để detect payment nhanh hơn
  
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
 * Show thank you message after successful QR payment for booking
 */
function showBookingPaymentThankYou(booking) {
  console.log("[FRONTEND] 🎉 [showBookingPaymentThankYou] Showing thank you message for booking:", booking);
  
  const modal = document.getElementById('simplePaymentModal');
  if (!modal) {
    console.error("[FRONTEND] ❌ [showBookingPaymentThankYou] Modal simplePaymentModal not found!");
    return;
  }
  
  const tasteModalRight = modal.querySelector('.payment-modal-right');
  const modalBody = modal.querySelector('.modal-body');
  const modalFooter = modal.querySelector('.modal-footer');
  const modalHeader = modal.querySelector('.modal-header');
  
  const bookingCode = booking?.bookingCode || `BKG${booking?.bookingId || ''}`;
  const amount = booking?.estimatedTotalAmount || booking?.totalAmount || 0;

  if (tasteModalRight) {
    // Taste Skill styling
    tasteModalRight.innerHTML = `
      <div style="text-align: center; padding: 20px 0;">
        <div style="font-size: 80px; margin-bottom: 24px;">🎉</div>
        <h3 style="color: #d4af37; margin-bottom: 16px; font-weight: 700; font-family: 'Playfair Display', serif; font-size: 32px">Thanh toán thành công!</h3>
        <p style="color: rgba(255,255,255,0.7); margin-bottom: 24px; font-size: 16px; line-height: 1.6;">
          Cảm ơn bạn. Giao dịch đã được xác nhận thành công.
        </p>
        <div style="background: rgba(50,205,50,0.05); padding: 20px; border-radius: 12px; border: 1px solid rgba(50,205,50,0.2); margin-bottom: 24px; text-align: left">
          <div class="d-flex justify-content-between mb-2">
            <span style="color: rgba(255,255,255,0.5);">Mã đặt phòng:</span>
            <strong style="color: #fff;">${bookingCode}</strong>
          </div>
          <div class="d-flex justify-content-between mb-2">
            <span style="color: rgba(255,255,255,0.5);">Số tiền:</span>
            <strong style="color: #d4af37;">${formatCurrency(amount)}</strong>
          </div>
          <div class="d-flex justify-content-between mb-2">
            <span style="color: rgba(255,255,255,0.5);">Phương thức:</span>
            <strong style="color: #fff;">QR Code</strong>
          </div>
          <div class="d-flex justify-content-between">
            <span style="color: rgba(255,255,255,0.5);">Trạng thái:</span>
            <strong style="color: #32cd32;">Đã thanh toán</strong>
          </div>
        </div>
        <div style="background: rgba(212,175,55,0.05); padding: 16px; border-radius: 8px; border: 1px solid rgba(212,175,55,0.2); margin-bottom: 30px">
          <p style="margin: 0; color: #d4af37; font-size: 14px; line-height: 1.6;">
            <strong>💡 Lưu ý:</strong> Vui lòng mang theo CMND/CCCD hoặc Hộ chiếu khi đến làm thủ tục check-in.
          </p>
        </div>
        <button type="button" class="taste-btn w-100 justify-content-center" data-bs-dismiss="modal" onclick="closeSimplePaymentModal()" style="font-weight: 600;">
          <i class="fas fa-check"></i> Đóng
        </button>
      </div>
    `;
  } else if (modalBody && modalFooter && modalHeader) {
    // Legacy styling
    // Update header
    const headerTitle = modalHeader.querySelector('.modal-title');
    const headerCloseBtn = modalHeader.querySelector('.btn-close');
    if (headerTitle) {
      headerTitle.innerHTML = '✅ Cảm ơn bạn đã thanh toán!';
      headerTitle.style.color = '#059669';
    }
    // Ensure close button in header works
    if (headerCloseBtn) {
      headerCloseBtn.setAttribute('onclick', 'closeSimplePaymentModal()');
      headerCloseBtn.setAttribute('data-bs-dismiss', 'modal');
    }
    
    // Update body with thank you message
    modalBody.innerHTML = `
      <div style="text-align: center; padding: 40px 20px;">
        <div style="font-size: 80px; margin-bottom: 24px;">🎉</div>
        <h3 style="color: #059669; margin-bottom: 16px; font-weight: 700;">Cảm ơn bạn đã thanh toán!</h3>
        <p style="color: #6b7280; margin-bottom: 24px; font-size: 16px; line-height: 1.6;">
          Thanh toán của bạn đã được xác nhận thành công.
        </p>
        <div style="background: #f0fdf4; padding: 20px; border-radius: 12px; border: 2px solid #86efac; margin-bottom: 24px;">
          <div style="margin-bottom: 12px;">
            <strong style="color: #1a1a1a; font-size: 16px;">Mã đặt phòng:</strong>
            <span style="color: #059669; font-size: 18px; font-weight: 700; margin-left: 8px;">${bookingCode}</span>
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
            <strong>💡 Lưu ý:</strong> Vui lòng mang theo CMND/CCCD hoặc Hộ chiếu khi đến làm thủ tục check-in.
          </p>
        </div>
      </div>
    `;
    
    // Update footer - only show close button
    modalFooter.innerHTML = `
      <button type="button" class="btn btn-primary" data-bs-dismiss="modal" onclick="closeSimplePaymentModal()" style="padding: 12px 28px; font-size: 16px; font-weight: 600; border-radius: 10px; background: #c8a97e; border: none; width: 100%;">
        <i class="icon-check"></i> Đóng
      </button>
    `;
  }
}

/**
 * Close simple payment modal
 */
function closeSimplePaymentModal() {
  const modal = document.getElementById('simplePaymentModal');
  if (!modal) {
    console.warn("[FRONTEND] " + '⚠️ [closeSimplePaymentModal] Modal not found');
    return;
  }
  
  console.log("[FRONTEND] " + '🔄 [closeSimplePaymentModal] Closing simple payment modal');
  
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
          console.log("[FRONTEND] " + '✅ [closeSimplePaymentModal] Closed using Bootstrap 5 Modal.getInstance');
        } else {
          const newModal = new bootstrap.Modal(modal);
          newModal.hide();
          closed = true;
          console.log("[FRONTEND] " + '✅ [closeSimplePaymentModal] Closed using Bootstrap 5 new Modal instance');
        }
      }
    } catch (e) {
      console.warn("[FRONTEND] " + '⚠️ [closeSimplePaymentModal] Bootstrap method failed:', e);
    }
  }
  
  // Method 2: jQuery
  if (!closed && typeof $ !== 'undefined' && $.fn.modal) {
    try {
      $(modal).modal('hide');
      closed = true;
      console.log("[FRONTEND] " + '✅ [closeSimplePaymentModal] Closed using jQuery');
    } catch (e) {
      console.warn("[FRONTEND] " + '⚠️ [closeSimplePaymentModal] jQuery method failed:', e);
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
    console.log("[FRONTEND] " + '✅ [closeSimplePaymentModal] Closed using direct DOM manipulation');
  }
  
  // Reload bookings list after modal is closed
  setTimeout(() => {
    if (window.loadBookings) {
      window.loadBookings();
    } else {
      window.location.reload();
    }
  }, 300);
}

/**
 * Show payment success (legacy - kept for compatibility)
 */
function showPaymentSuccess() {
  console.log("[FRONTEND] 🎉🎉🎉 [showPaymentSuccess] ========== STARTING ==========");
  console.log("[FRONTEND] 🎉 [showPaymentSuccess] Showing payment success...");
  
  const modal = document.getElementById('simplePaymentModal');
  if (!modal) {
    console.error("[FRONTEND] ❌ [showPaymentSuccess] Modal simplePaymentModal not found!");
    return;
  }
  console.log("[FRONTEND] ✅ [showPaymentSuccess] Modal found, is visible:", modal.classList.contains('show'));
  
  const waitingEl = document.getElementById('spWaiting');
  const successEl = document.getElementById('spSuccess');
  const qrImg = document.getElementById('spQRImage');
  const qrSection = document.getElementById('spQRSection');

  // Hide waiting message - force với !important
  if (waitingEl) {
    console.log("[FRONTEND] 🔄 [showPaymentSuccess] Hiding waiting message...");
    waitingEl.style.display = 'none';
    waitingEl.style.visibility = 'hidden';
    waitingEl.style.opacity = '0';
    waitingEl.setAttribute('hidden', '');
    waitingEl.classList.add('d-none');
    waitingEl.classList.remove('d-block');
    console.log("[FRONTEND] ✅ [showPaymentSuccess] Hidden waiting message");
    console.log("[FRONTEND]    - computed display:", window.getComputedStyle(waitingEl).display);
  } else {
    console.warn("[FRONTEND] ⚠️ [showPaymentSuccess] spWaiting element not found");
  }
  
  // Show success message - force với nhiều cách
  if (successEl) {
    console.log("[FRONTEND] 🎉 [showPaymentSuccess] Showing success message...");
    // Remove all hiding classes/styles
    successEl.style.display = 'block';
    successEl.style.visibility = 'visible';
    successEl.style.opacity = '1';
    successEl.removeAttribute('hidden');
    successEl.classList.remove('d-none');
    successEl.classList.add('d-block');
    
    // Force với !important qua setAttribute
    successEl.setAttribute('style', 'display: block !important; visibility: visible !important; opacity: 1 !important;');
    
    console.log("[FRONTEND] ✅ [showPaymentSuccess] Showed success message");
    console.log("[FRONTEND]    - inline display:", successEl.style.display);
    console.log("[FRONTEND]    - visibility:", successEl.style.visibility);
    console.log("[FRONTEND]    - computed display:", window.getComputedStyle(successEl).display);
    console.log("[FRONTEND]    - computed visibility:", window.getComputedStyle(successEl).visibility);
    console.log("[FRONTEND]    - has d-none class:", successEl.classList.contains('d-none'));
    console.log("[FRONTEND]    - has d-block class:", successEl.classList.contains('d-block'));
    
    // Verify nó thực sự visible
    const rect = successEl.getBoundingClientRect();
    console.log("[FRONTEND]    - bounding rect:", { width: rect.width, height: rect.height, top: rect.top, left: rect.left });
    console.log("[FRONTEND]    - is visible:", rect.width > 0 && rect.height > 0);
  } else {
    console.error("[FRONTEND] ❌ [showPaymentSuccess] spSuccess element not found!");
    console.error("[FRONTEND] ❌ [showPaymentSuccess] Available elements in modal:", Array.from(modal.querySelectorAll('[id]')).map(el => el.id));
  }
  
  // Hide QR image - force với nhiều cách
  if (qrImg) {
    console.log("[FRONTEND] 🔄 [showPaymentSuccess] Hiding QR image...");
    qrImg.style.display = 'none';
    qrImg.style.visibility = 'hidden';
    qrImg.style.opacity = '0';
    qrImg.setAttribute('hidden', '');
    qrImg.src = ''; // Clear src để đảm bảo không load lại
    qrImg.classList.add('d-none');
    qrImg.classList.remove('d-block');
    console.log("[FRONTEND] ✅ [showPaymentSuccess] Hidden QR image");
    console.log("[FRONTEND]    - computed display:", window.getComputedStyle(qrImg).display);
  } else {
    console.warn("[FRONTEND] ⚠️ [showPaymentSuccess] spQRImage element not found");
  }
  
  // Hide QR section - force với nhiều cách
  if (qrSection) {
    console.log("[FRONTEND] 🔄 [showPaymentSuccess] Hiding QR section...");
    qrSection.style.display = 'none';
    qrSection.style.visibility = 'hidden';
    qrSection.style.opacity = '0';
    qrSection.setAttribute('hidden', '');
    qrSection.classList.add('d-none');
    qrSection.classList.remove('d-block');
    console.log("[FRONTEND] ✅ [showPaymentSuccess] Hidden QR section");
    console.log("[FRONTEND]    - computed display:", window.getComputedStyle(qrSection).display);
  } else {
    console.warn("[FRONTEND] ⚠️ [showPaymentSuccess] spQRSection element not found");
  }
  
  // Force modal to update - trigger reflow
  if (modal.classList.contains('show')) {
    console.log("[FRONTEND] 🔄 [showPaymentSuccess] Modal is visible, forcing UI update...");
    // Force reflow
    void modal.offsetHeight;
    // Trigger repaint
    requestAnimationFrame(() => {
      void modal.offsetHeight;
      // Double-check success element after repaint
      if (successEl) {
        const finalDisplay = window.getComputedStyle(successEl).display;
        console.log("[FRONTEND] 🔍 [showPaymentSuccess] After repaint - computed display:", finalDisplay);
        if (finalDisplay === 'none') {
          console.error("[FRONTEND] ❌ [showPaymentSuccess] WARNING: Success element still hidden after repaint!");
          // Force one more time
          successEl.style.setProperty('display', 'block', 'important');
        }
      }
    });
  }
  
  console.log("[FRONTEND] ✅✅✅ [showPaymentSuccess] ========== COMPLETED ==========");
}

/**
 * Hide modal directly (KHÔNG dùng Bootstrap API - chỉ dùng DOM manipulation)
 */
function hideModalDirectly(modalElement) {
  if (!modalElement) {
    console.warn('[FRONTEND] ⚠️ [hideModalDirectly] Modal element not found');
    return;
  }
  
  try {
    console.log('[FRONTEND] 🔄 [hideModalDirectly] Hiding modal directly (no Bootstrap API)...');
    
    // Method 1: jQuery (if available) - đơn giản nhất
    if (typeof $ !== 'undefined' && $.fn.modal) {
      console.log('[FRONTEND] 🔄 [hideModalDirectly] Using jQuery to hide modal');
      $(modalElement).modal('hide');
      return;
    }
    
    // Method 2: Direct DOM manipulation (KHÔNG cần Bootstrap API)
    console.log('[FRONTEND] 🔄 [hideModalDirectly] Using direct DOM manipulation');
    
    // Remove show class và các attributes
    modalElement.classList.remove('show');
    modalElement.style.display = 'none';
    modalElement.setAttribute('aria-hidden', 'true');
    modalElement.removeAttribute('aria-modal');
    modalElement.removeAttribute('role');
    
    // Remove ALL backdrops (có thể có nhiều)
    const backdrops = document.querySelectorAll('.modal-backdrop');
    backdrops.forEach(backdrop => {
      console.log('[FRONTEND] 🔄 [hideModalDirectly] Removing backdrop');
      backdrop.remove();
    });
    
    // Remove modal-open class from body
    document.body.classList.remove('modal-open');
    document.body.style.overflow = '';
    document.body.style.paddingRight = '';
    
    console.log('[FRONTEND] ✅ [hideModalDirectly] Modal hidden successfully');
  } catch (e) {
    console.error('[FRONTEND] ❌ [hideModalDirectly] Error hiding modal:', e);
    // Last resort: just hide it
    if (modalElement) {
      modalElement.style.display = 'none';
      modalElement.classList.remove('show');
    }
  }
}

/**
 * Format currency (only declare if not already defined)
 */
if (typeof formatCurrency === 'undefined') {
  window.formatCurrency = function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(amount);
  };
}

/**
 * Show toast notification (use existing showToast if available)
 */
function showSimpleToast(message, type) {
  if (typeof showToast === 'function') {
    showToast(message, type);
  } else {
    console.log("[FRONTEND] " + `[${type.toUpperCase()}] ${message}`);
    // Fallback: simple alert
    alert(message);
  }
}

/**
 * Show hotel payment confirmation modal (for PayAtHotel/Cash bookings)
 */
function showHotelPaymentConfirmation(bookingId, bookingCode, amount) {
  console.log("[FRONTEND] " + '🏨 [showHotelPaymentConfirmation] Showing hotel payment confirmation for booking:', bookingId);
  
  // Check if modal exists, if not create it
  let modal = document.getElementById('hotelPaymentConfirmationModal');
  if (!modal) {
    const modalHTML = `
      <div class="modal fade" id="hotelPaymentConfirmationModal" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered modal-lg">
          <div class="modal-content" style="border-radius: 20px;">
            <div class="modal-header" style="background: linear-gradient(135deg, #c8a97e 0%, #b89968 100%); color: white;">
              <h5 class="modal-title" style="font-size: 24px; font-weight: 700;">💵 Xác Nhận Thanh Toán Tại Khách Sạn</h5>
              <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" onclick="closeHotelPaymentModal()"></button>
            </div>
            <div class="modal-body" style="padding: 30px;">
              <div style="text-align: center; padding: 20px;">
                <div style="font-size: 64px; margin-bottom: 20px;">🏨</div>
                <h4 style="color: #1a1a1a; margin-bottom: 16px;">Thanh toán tại khách sạn</h4>
                <p style="color: #6b7280; margin-bottom: 30px; font-size: 16px; line-height: 1.6;">
                  Bạn có thể thanh toán bằng tiền mặt hoặc thẻ tại quầy lễ tân khi làm thủ tục check-in.
                </p>
                <div style="background: white; padding: 24px; border-radius: 12px; border: 2px solid #c8a97e; margin-bottom: 24px;">
                  <div style="margin-bottom: 12px;">
                    <strong style="color: #1a1a1a; font-size: 16px;">Mã đặt phòng:</strong>
                    <span id="hpcBookingCode" style="color: #059669; font-size: 18px; font-weight: 700; margin-left: 8px;">-</span>
                  </div>
                  <div style="margin-bottom: 12px;">
                    <strong style="color: #1a1a1a; font-size: 16px;">Số tiền cần thanh toán:</strong>
                    <span id="hpcAmount" style="color: #c8a97e; font-size: 24px; font-weight: 700; margin-left: 8px;">0 ₫</span>
                  </div>
                  <div>
                    <strong style="color: #1a1a1a; font-size: 16px;">Địa chỉ:</strong>
                    <span style="color: #6b7280; font-size: 15px; margin-left: 8px;">Huflit Hốc Môn, Hồ Chí Minh</span>
                  </div>
                </div>
                <div style="background: #f0fdf4; padding: 16px; border-radius: 8px; border: 1px solid #86efac;">
                  <p style="margin: 0; color: #059669; font-size: 14px; line-height: 1.6;">
                    <strong>💡 Lưu ý:</strong> Vui lòng mang theo CMND/CCCD hoặc Hộ chiếu khi đến làm thủ tục check-in và thanh toán.
                  </p>
                </div>
              </div>
            </div>
            <div class="modal-footer" style="border-top: 2px solid #f0f0f0; padding: 20px 30px;">
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" style="padding: 12px 28px; font-size: 16px; font-weight: 600; border-radius: 10px;">Đóng</button>
              <button type="button" class="btn btn-primary" onclick="confirmHotelPayment()" id="hpcConfirmBtn" style="padding: 12px 28px; font-size: 16px; font-weight: 600; border-radius: 10px; background: #c8a97e; border: none;">
                <i class="icon-check"></i> Xác nhận đã thanh toán
              </button>
            </div>
          </div>
        </div>
      </div>
    `;
    document.body.insertAdjacentHTML('beforeend', modalHTML);
    modal = document.getElementById('hotelPaymentConfirmationModal');
  }
  
  // Update modal content
  const bookingCodeEl = document.getElementById('hpcBookingCode');
  const amountEl = document.getElementById('hpcAmount');
  if (bookingCodeEl) bookingCodeEl.textContent = bookingCode;
  if (amountEl) {
    const formatVND = (v) => {
      const num = Number(v || 0);
      if (num === 0) return '0 ₫';
      return new Intl.NumberFormat('vi-VN').format(num) + ' ₫';
    };
    amountEl.textContent = formatVND(amount);
  }
  
  // Store booking ID and code for confirmation
  modal.dataset.bookingId = bookingId;
  modal.dataset.bookingCode = bookingCode;
  
  // Show modal
  try {
    if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
      const bsModal = new bootstrap.Modal(modal);
      bsModal.show();
    } else if (typeof $ !== 'undefined' && $.fn.modal) {
      $(modal).modal('show');
    } else {
      modal.classList.add('show');
      modal.style.display = 'block';
      document.body.classList.add('modal-open');
      const backdrop = document.createElement('div');
      backdrop.className = 'modal-backdrop fade show';
      document.body.appendChild(backdrop);
    }
  } catch (e) {
    console.error("[FRONTEND] " + '❌ Error showing hotel payment confirmation modal:', e);
    modal.classList.add('show');
    modal.style.display = 'block';
    document.body.classList.add('modal-open');
  }
}

/**
 * Confirm hotel payment (mark booking as paid)
 */
async function confirmHotelPayment() {
  const modal = document.getElementById('hotelPaymentConfirmationModal');
  if (!modal) {
    showSimpleToast('Lỗi: Không tìm thấy modal', 'danger');
    return;
  }
  
  const bookingId = modal.dataset.bookingId;
  if (!bookingId) {
    showSimpleToast('Lỗi: Không tìm thấy mã đặt phòng', 'danger');
    return;
  }
  
  const confirmBtn = document.getElementById('hpcConfirmBtn');
  if (confirmBtn) {
    confirmBtn.disabled = true;
    confirmBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Đang xử lý...';
  }
  
  try {
    const token = localStorage.getItem('token');
    if (!token) {
      throw new Error('Vui lòng đăng nhập để xác nhận thanh toán');
    }
    
    // Use request-cash-payment endpoint để yêu cầu thanh toán tiền mặt (chờ admin xác nhận)
    const response = await fetch(`${location.origin}/api/bookings/${bookingId}/request-cash-payment`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      }
    });
    
    if (!response.ok) {
      const errorData = await response.json().catch(() => ({ message: 'Lỗi không xác định' }));
      throw new Error(errorData.message || `HTTP ${response.status}`);
    }
    
    const result = await response.json();
    console.log("[FRONTEND] " + '✅ [confirmHotelPayment] Cash payment requested:', result);
    
    // Show waiting message - yêu cầu đã được gửi, chờ admin xác nhận
    const modalBody = modal.querySelector('.modal-body');
    const modalFooter = modal.querySelector('.modal-footer');
    const modalHeader = modal.querySelector('.modal-header');
    
    if (modalBody && modalFooter && modalHeader) {
      // Update header
      const headerTitle = modalHeader.querySelector('.modal-title');
      const headerCloseBtn = modalHeader.querySelector('.btn-close');
      if (headerTitle) {
        headerTitle.innerHTML = '⏳ Yêu cầu đã được gửi';
        headerTitle.style.color = '#f59e0b';
      }
      // Ensure close button in header works
      if (headerCloseBtn) {
        headerCloseBtn.setAttribute('onclick', 'closeHotelPaymentModal()');
        headerCloseBtn.setAttribute('data-bs-dismiss', 'modal');
      }
      
      // Update body with waiting message
      modalBody.innerHTML = `
        <div style="text-align: center; padding: 40px 20px;">
          <div style="font-size: 80px; margin-bottom: 24px;">⏳</div>
          <h3 style="color: #f59e0b; margin-bottom: 16px; font-weight: 700;">Yêu cầu thanh toán đã được gửi</h3>
          <p style="color: #6b7280; margin-bottom: 24px; font-size: 16px; line-height: 1.6;">
            Yêu cầu thanh toán tiền mặt của bạn đã được gửi thành công. Vui lòng chờ admin xác nhận.
          </p>
          <div style="background: #fef3c7; padding: 20px; border-radius: 12px; border: 2px solid #fbbf24; margin-bottom: 24px;">
            <div style="margin-bottom: 12px;">
              <strong style="color: #1a1a1a; font-size: 16px;">Mã đặt phòng:</strong>
              <span id="hpcThankYouBookingCode" style="color: #f59e0b; font-size: 18px; font-weight: 700; margin-left: 8px;">${modal.dataset.bookingCode || '-'}</span>
            </div>
            <div>
              <strong style="color: #1a1a1a; font-size: 16px;">Trạng thái:</strong>
              <span style="color: #f59e0b; font-size: 18px; font-weight: 700; margin-left: 8px;">Chờ xác nhận</span>
            </div>
          </div>
          <div style="background: #eff6ff; padding: 16px; border-radius: 8px; border: 1px solid #93c5fd;">
            <p style="margin: 0; color: #1e40af; font-size: 14px; line-height: 1.6;">
              <strong>💡 Lưu ý:</strong> Admin sẽ xác nhận thanh toán của bạn trong thời gian sớm nhất. Bạn sẽ nhận được thông báo khi thanh toán được xác nhận.
            </p>
          </div>
        </div>
      `;
      
      // Update footer - only show close button
      modalFooter.innerHTML = `
        <button type="button" class="btn btn-primary" data-bs-dismiss="modal" onclick="closeHotelPaymentModal()" style="padding: 12px 28px; font-size: 16px; font-weight: 600; border-radius: 10px; background: #c8a97e; border: none; width: 100%;">
          <i class="icon-check"></i> Đóng
        </button>
      `;
      
      // Store booking code for thank you message
      modal.dataset.bookingCode = modal.dataset.bookingCode || bookingId;
    }
    
    // Show toast notification
    showSimpleToast('Yêu cầu thanh toán tiền mặt đã được gửi. Vui lòng chờ admin xác nhận.', 'info');
    
    // Reload bookings list after a delay
    setTimeout(() => {
      if (window.loadBookings) {
        window.loadBookings();
      } else {
        // Don't auto-reload, let user close manually
      }
    }, 1000);
    
  } catch (error) {
    console.error("[FRONTEND] " + '❌ [confirmHotelPayment] Error:', error);
    showSimpleToast(error.message || 'Lỗi xác nhận thanh toán', 'danger');
    if (confirmBtn) {
      confirmBtn.disabled = false;
      confirmBtn.innerHTML = '<i class="icon-check"></i> Xác nhận đã thanh toán';
    }
  }
}

/**
 * Close hotel payment modal
 */
function closeHotelPaymentModal() {
  const modal = document.getElementById('hotelPaymentConfirmationModal');
  if (!modal) {
    console.warn("[FRONTEND] " + '⚠️ [closeHotelPaymentModal] Modal not found');
    return;
  }
  
  console.log("[FRONTEND] " + '🔄 [closeHotelPaymentModal] Closing hotel payment modal');
  
  // Try multiple methods to close modal
  let closed = false;
  
  // Method 1: Bootstrap 5 - try getInstance first
  if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
    try {
      // Check if getInstance exists (Bootstrap 5)
      if (typeof bootstrap.Modal.getInstance === 'function') {
        const bsModal = bootstrap.Modal.getInstance(modal);
        if (bsModal) {
          bsModal.hide();
          closed = true;
          console.log("[FRONTEND] " + '✅ [closeHotelPaymentModal] Closed using Bootstrap 5 Modal.getInstance');
        } else {
          // Try creating new instance and hiding
          const newModal = new bootstrap.Modal(modal);
          newModal.hide();
          closed = true;
          console.log("[FRONTEND] " + '✅ [closeHotelPaymentModal] Closed using Bootstrap 5 new Modal instance');
        }
      } else {
        // Bootstrap 4 or older - use jQuery or direct method
        console.log("[FRONTEND] " + '⚠️ [closeHotelPaymentModal] Bootstrap.Modal.getInstance not available, trying jQuery');
      }
    } catch (e) {
      console.warn("[FRONTEND] " + '⚠️ [closeHotelPaymentModal] Bootstrap method failed:', e);
    }
  }
  
  // Method 2: jQuery (if Bootstrap method didn't work)
  if (!closed && typeof $ !== 'undefined' && $.fn.modal) {
    try {
      $(modal).modal('hide');
      closed = true;
      console.log("[FRONTEND] " + '✅ [closeHotelPaymentModal] Closed using jQuery');
    } catch (e) {
      console.warn("[FRONTEND] " + '⚠️ [closeHotelPaymentModal] jQuery method failed:', e);
    }
  }
  
  // Method 3: Direct DOM manipulation (fallback)
  if (!closed) {
    hideModalDirectly(modal);
    closed = true;
    console.log("[FRONTEND] " + '✅ [closeHotelPaymentModal] Closed using direct DOM manipulation');
  }
  
  // Reload bookings list after modal is closed
  setTimeout(() => {
    if (window.loadBookings) {
      window.loadBookings();
    } else {
      window.location.reload();
    }
  }, 300);
}

// Make functions globally available
window.showHotelPaymentConfirmation = showHotelPaymentConfirmation;
window.confirmHotelPayment = confirmHotelPayment;
window.closeHotelPaymentModal = closeHotelPaymentModal;
window.closeSimplePaymentModal = closeSimplePaymentModal;
window.openSimplePayment = openSimplePayment;

// Stop polling when modal is closed
document.addEventListener('DOMContentLoaded', () => {
  const modal = document.getElementById('simplePaymentModal');
  if (modal) {
    modal.addEventListener('hidden.bs.modal', () => {
      stopSimplePolling();
    });
  }
});

