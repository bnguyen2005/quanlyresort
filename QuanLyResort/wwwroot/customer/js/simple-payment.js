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
    console.log('🔍 [updatePaymentModal] Full PayOs response:', JSON.stringify(result, null, 2));

    // Check if we have QR code - PayOs có thể trả về:
    // 1. URL QR: "https://img.vietqr.io/image/..."
    // 2. Base64: "iVBORw0KGgoAAAANSUhEUgAA..."
    // 3. Không có QR code, chỉ có checkoutUrl
    let qrCodeData = result.qrCode || result.data?.qrCode || result.qrCodeBase64;
    console.log('🔍 [updatePaymentModal] QR Code data type:', typeof qrCodeData);
    console.log('🔍 [updatePaymentModal] QR Code data length:', qrCodeData?.length || 0);
    console.log('🔍 [updatePaymentModal] QR Code data preview:', qrCodeData?.substring(0, 50) || 'NULL');
    console.log('🔍 [updatePaymentModal] Has checkoutUrl:', !!result.checkoutUrl);

    if (!result.success) {
      throw new Error(`PayOs API error: ${result.desc || result.message || 'Unknown error'}`);
    }

    // Display QR code from PayOs
    if (qrImg) {
      if (qrCodeData) {
        // Case 1: QR code là URL (https://...)
        if (qrCodeData.startsWith('http://') || qrCodeData.startsWith('https://')) {
          console.log('🌐 [updatePaymentModal] QR Code is URL:', qrCodeData);
          qrImg.src = qrCodeData;
          qrImg.style.display = 'block';
          qrImg.alt = `PayOs QR - ${bookingCode}`;
          
          qrImg.onerror = function(e) {
            console.error('❌ [updatePaymentModal] QR URL failed to load:', e);
            console.error('❌ [updatePaymentModal] QR URL:', qrCodeData);
            qrImg.style.display = 'none';
            if (waitingEl) {
              waitingEl.textContent = 'Không thể tải QR code từ PayOs. Vui lòng thử lại.';
              waitingEl.className = 'text-center mt-4 text-danger';
            }
          };
          
          qrImg.onload = function() {
            console.log('✅ [updatePaymentModal] QR URL loaded successfully');
            qrImg.style.border = '4px solid #e9ecef';
          };
        }
        // Case 2: QR code là Base64 image (PNG/JPEG)
        else if (qrCodeData.startsWith('iVBORw0KGgo') || qrCodeData.startsWith('/9j/4AAQ') || 
                 qrCodeData.startsWith('data:image/') || 
                 /^[A-Za-z0-9+/=]{100,}$/.test(qrCodeData.trim())) {
          console.log('📦 [updatePaymentModal] QR Code is Base64 image');
          // Remove any whitespace/newlines from base64 string
          qrCodeData = qrCodeData.trim().replace(/\s/g, '');
          
          // Check if it's already a data URL
          let qrSrc = qrCodeData;
          if (!qrCodeData.startsWith('data:')) {
            // Add data URL prefix if not present
            qrSrc = `data:image/png;base64,${qrCodeData}`;
          }
          
          console.log('🖼️ [updatePaymentModal] Setting QR image src (first 100 chars):', qrSrc.substring(0, 100));
          
          qrImg.src = qrSrc;
          qrImg.style.display = 'block';
          qrImg.alt = `PayOs QR - ${bookingCode}`;
          
          qrImg.onerror = function(e) {
            console.error('❌ [updatePaymentModal] QR Base64 failed to load:', e);
            console.error('❌ [updatePaymentModal] Failed src (first 200 chars):', qrSrc.substring(0, 200));
            qrImg.style.display = 'none';
            if (waitingEl) {
              waitingEl.textContent = 'Không thể tải QR code từ PayOs. Vui lòng thử lại.';
              waitingEl.className = 'text-center mt-4 text-danger';
            }
          };
          
          qrImg.onload = function() {
            console.log('✅ [updatePaymentModal] QR Base64 loaded successfully');
            qrImg.style.border = '4px solid #e9ecef';
          };
        }
        // Case 3: QR code là QR data string (EMV QR format - bắt đầu bằng số)
        // PayOs QR data có thể chứa space, nên cần remove space trước khi test
        else if (/^[0-9A-Za-z\s]+$/.test(qrCodeData.trim()) && qrCodeData.trim().length > 50 && qrCodeData.trim().startsWith('000201')) {
          console.log('📱 [updatePaymentModal] QR Code is QR data string (EMV format)');
          console.log('📱 [updatePaymentModal] QR data string length:', qrCodeData.length);
          
          // Generate QR code image từ QR data string bằng QRCode.js
          // Tạo container tạm để generate QR
          const tempContainer = document.createElement('div');
          tempContainer.style.position = 'absolute';
          tempContainer.style.left = '-9999px';
          tempContainer.style.width = '256px';
          tempContainer.style.height = '256px';
          document.body.appendChild(tempContainer);
          
          try {
            // Clear container trước khi generate
            tempContainer.innerHTML = '';
            
            // Sử dụng QR data string trực tiếp từ PayOs (không remove space)
            // PayOs trả về EMV QR format, space có thể là một phần của format hoặc description
            // Việc remove space có thể làm hỏng format và ngân hàng không nhận diện được
            const qrDataToUse = qrCodeData.trim();
            console.log('📱 [updatePaymentModal] Using QR data from PayOs (preserving format):', qrDataToUse.substring(0, 100) + '...');
            console.log('📱 [updatePaymentModal] QR data length:', qrDataToUse.length);
            console.log('📱 [updatePaymentModal] QR data has spaces:', qrDataToUse.includes(' '));
            
            // Generate QR code từ QR data string (giữ nguyên format từ PayOs)
            const qr = new QRCode(tempContainer, {
              text: qrDataToUse,
              width: 256,
              height: 256,
              colorDark: '#000000',
              colorLight: '#ffffff',
              correctLevel: QRCode.CorrectLevel.H
            });
            
            // Lấy canvas từ QRCode.js
            const canvas = tempContainer.querySelector('canvas');
            if (canvas) {
              // Convert canvas to data URL
              const dataUrl = canvas.toDataURL('image/png');
              qrImg.src = dataUrl;
              qrImg.style.display = 'block';
              qrImg.alt = `PayOs QR - ${bookingCode}`;
              qrImg.style.border = '4px solid #e9ecef';
              
              console.log('✅ [updatePaymentModal] QR code generated from QR data string');
              
              // Remove temp container
              document.body.removeChild(tempContainer);
            } else {
              throw new Error('QRCode.js không tạo được canvas');
            }
          } catch (error) {
            console.error('❌ [updatePaymentModal] Error generating QR from data string:', error);
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
        // Case 4: Không nhận diện được format
        else {
          console.error('❌ [updatePaymentModal] Không nhận diện được format QR code');
          console.error('❌ [updatePaymentModal] QR data preview:', qrCodeData.substring(0, 100));
          qrImg.style.display = 'none';
          if (waitingEl) {
            waitingEl.textContent = 'Định dạng QR code không hợp lệ từ PayOs. Vui lòng thử lại.';
            waitingEl.className = 'text-center mt-4 text-danger';
          }
        }
      } 
      // Case 5: Không có QR code từ PayOs - thử dùng checkoutUrl
      else {
        console.warn('⚠️ [updatePaymentModal] PayOs không trả về QR code');
        console.warn('⚠️ [updatePaymentModal] PaymentLinkId:', result.paymentLinkId);
        console.warn('⚠️ [updatePaymentModal] CheckoutUrl:', result.checkoutUrl);
        
        // Thử generate QR từ checkoutUrl nếu có
        if (result.checkoutUrl) {
          console.log('🔄 [updatePaymentModal] Generating QR from checkoutUrl...');
          const tempContainer = document.createElement('div');
          tempContainer.style.position = 'absolute';
          tempContainer.style.left = '-9999px';
          tempContainer.style.width = '256px';
          tempContainer.style.height = '256px';
          document.body.appendChild(tempContainer);
          
          try {
            tempContainer.innerHTML = '';
            const qr = new QRCode(tempContainer, {
              text: result.checkoutUrl,
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
              qrImg.alt = `PayOs QR - ${bookingCode}`;
              qrImg.style.border = '4px solid #e9ecef';
              console.log('✅ [updatePaymentModal] QR code generated from checkoutUrl');
              document.body.removeChild(tempContainer);
            } else {
              throw new Error('QRCode.js không tạo được canvas');
            }
          } catch (error) {
            console.error('❌ [updatePaymentModal] Error generating QR from checkoutUrl:', error);
            if (tempContainer.parentNode) {
              document.body.removeChild(tempContainer);
            }
            qrImg.style.display = 'none';
            if (waitingEl) {
              waitingEl.textContent = 'Không thể tạo QR code. Vui lòng thử lại hoặc liên hệ hỗ trợ.';
              waitingEl.className = 'text-center mt-4 text-danger';
            }
          }
        } else {
          qrImg.style.display = 'none';
          if (waitingEl) {
            waitingEl.textContent = 'PayOs không trả về QR code. Vui lòng thử lại hoặc liên hệ hỗ trợ.';
            waitingEl.className = 'text-center mt-4 text-danger';
          }
        }
      }
    }

    // Show QR section
    if (qrSection) {
      qrSection.style.display = 'block';
      console.log('✅ [updatePaymentModal] QR section displayed');
    }

    // Update bank info if available - đảm bảo hiển thị đúng tài khoản MB Bank
    const expectedAccountNumber = '0901329227';
    if (result.accountNumber) {
      const bankAccEl = document.getElementById('spBankAccount');
      if (bankAccEl) {
        bankAccEl.textContent = result.accountNumber;
        // Validate account number
        if (result.accountNumber !== expectedAccountNumber) {
          console.warn('⚠️ [updatePaymentModal] Account Number mismatch! Expected: ' + expectedAccountNumber + ', Got: ' + result.accountNumber);
        } else {
          console.log('✅ [updatePaymentModal] Account Number verified: ' + result.accountNumber + ' (MB Bank)');
        }
      }
    } else {
      // Fallback to default if PayOs doesn't return account number
      const bankAccEl = document.getElementById('spBankAccount');
      if (bankAccEl) {
        bankAccEl.textContent = expectedAccountNumber;
        console.warn('⚠️ [updatePaymentModal] PayOs did not return accountNumber, using default: ' + expectedAccountNumber);
      }
    }
    
    if (result.accountName) {
      const bankNameEl = document.getElementById('spBankName');
      if (bankNameEl) {
        bankNameEl.textContent = result.accountName;
        console.log('✅ [updatePaymentModal] Account Name: ' + result.accountName);
      }
    } else {
      // Fallback to default if PayOs doesn't return account name
      const bankNameEl = document.getElementById('spBankName');
      if (bankNameEl) {
        bankNameEl.textContent = 'MB Bank';
        console.warn('⚠️ [updatePaymentModal] PayOs did not return accountName, using default: MB Bank');
      }
    }

    // Update amount from PayOs response (to ensure accuracy)
    if (result.amount && result.amount > 0) {
      const amountEl = document.getElementById('spAmount');
      if (amountEl) {
        // PayOs returns amount in VND (integer)
        amountEl.textContent = formatCurrency(result.amount);
        console.log('✅ [updatePaymentModal] Amount updated from PayOs:', result.amount);
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
 * Generate QR code from checkoutUrl using QRCode.js library
 * Fallback khi PayOs không trả về QR code
 */
function generateQRFromCheckoutUrl(checkoutUrl, container) {
  if (!checkoutUrl) {
    console.error('❌ [generateQRFromCheckoutUrl] No checkoutUrl provided');
    return;
  }

  if (!container) {
    console.error('❌ [generateQRFromCheckoutUrl] No container provided');
    return;
  }

  console.log('🔄 [generateQRFromCheckoutUrl] Generating QR code from checkoutUrl:', checkoutUrl);

  // Check if QRCode.js is loaded
  if (typeof QRCode === 'undefined') {
    console.error('❌ [generateQRFromCheckoutUrl] QRCode.js library not loaded');
    // Fallback: show link button
    container.innerHTML = `
      <div class="text-center">
        <a href="${checkoutUrl}" target="_blank" class="btn btn-primary btn-lg">
          <i class="icon-credit-card"></i> Click để thanh toán qua PayOs
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

    console.log('✅ [generateQRFromCheckoutUrl] QR code generated successfully');

    // Add click handler to open checkout URL
    qrContainer.style.cursor = 'pointer';
    qrContainer.title = 'Click để mở trang thanh toán';
    qrContainer.onclick = function() {
      window.open(checkoutUrl, '_blank');
    };
  } catch (error) {
    console.error('❌ [generateQRFromCheckoutUrl] Error generating QR code:', error);
    // Fallback: show link button
    container.innerHTML = `
      <div class="text-center">
        <a href="${checkoutUrl}" target="_blank" class="btn btn-primary btn-lg">
          <i class="icon-credit-card"></i> Click để thanh toán qua PayOs
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
  console.log('🔄 [SimplePolling] Starting polling for booking:', bookingId);
  let pollCount = 0;
  const maxPolls = 300; // Poll tối đa 10 phút (300 * 2s = 600s)
  
  window.paymentPollingInterval = setInterval(async () => {
    pollCount++;
    try {
      const token = localStorage.getItem('token');
      if (!token) {
        console.warn('⚠️ [SimplePolling] No token found');
        stopSimplePolling();
        return;
      }

      // Timeout sau 10 phút
      if (pollCount > maxPolls) {
        console.log('⏰ [SimplePolling] Timeout reached after 10 minutes, stopping polling');
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
        console.warn('⚠️ [SimplePolling] Response not OK:', response.status);
        return;
      }

      const booking = await response.json();
      
      // Log mỗi 10 lần poll để không spam console
      if (pollCount % 10 === 0 || pollCount === 1) {
        console.log(`🔍 [SimplePolling] Poll #${pollCount} - Status: ${booking.status} (booking ${bookingId})`);
      }
      
      // Normalize status để check (case-insensitive, trim whitespace)
      const rawStatus = String(booking.status || '').trim();
      const normalizedStatus = rawStatus.toLowerCase();

      // Check for "Paid" status (case-insensitive, với nhiều variations)
      const isPaid = normalizedStatus === 'paid' || 
                     rawStatus === 'Paid' || 
                     rawStatus === 'PAID' ||
                     normalizedStatus.includes('paid');
      
      if (isPaid) {
        console.log('✅ [SimplePolling] Payment detected! Status =', rawStatus, '(normalized:', normalizedStatus + ')');
        console.log('✅ [SimplePolling] Poll count:', pollCount);
        console.log('✅ [SimplePolling] Full booking object:', booking);
        
        // Stop polling first
        stopSimplePolling();
        
        // Small delay để đảm bảo polling đã dừng
        await new Promise(resolve => setTimeout(resolve, 100));
        
        // Show success UI immediately (force update)
        showPaymentSuccess();
        
        // Double-check và force update lại sau 200ms
        setTimeout(() => {
          showPaymentSuccess();
        }, 200);
        
        // Show toast notification
        showSimpleToast('✅ Thanh toán thành công!', 'success');
        
        // Force UI update - trigger reflow
        const modal = document.getElementById('simplePaymentModal');
        if (modal) {
          // Trigger a reflow to ensure CSS updates
          void modal.offsetHeight;
          // Force repaint
          modal.style.display = 'block';
          setTimeout(() => {
            modal.style.display = '';
          }, 0);
        }
        
        // Reload bookings list to update status
        if (window.loadBookings) {
          setTimeout(() => {
            window.loadBookings();
          }, 500);
        }
        
        // Close modal after 3 seconds
        setTimeout(() => {
          const modalInstance = bootstrap.Modal.getInstance(document.getElementById('simplePaymentModal'));
          if (modalInstance) {
            modalInstance.hide();
          }
        }, 3000);
      } else {
        // Log status mỗi 10 lần poll
        if (pollCount % 10 === 0) {
          console.log(`⏳ [SimplePolling] Still waiting... Status: ${rawStatus} (normalized: ${normalizedStatus}, poll #${pollCount})`);
        }
      }
    } catch (error) {
      console.error('❌ [SimplePolling] Polling error:', error);
    }
  }, 2000); // Poll mỗi 2 giây
  
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

  // Hide waiting message - force với !important
  if (waitingEl) {
    waitingEl.style.display = 'none';
    waitingEl.style.visibility = 'hidden';
    waitingEl.style.opacity = '0';
    waitingEl.setAttribute('hidden', '');
    console.log('✅ [showPaymentSuccess] Hidden waiting message');
  } else {
    console.warn('⚠️ [showPaymentSuccess] spWaiting element not found');
  }
  
  // Show success message - force với nhiều cách
  if (successEl) {
    successEl.style.display = 'block';
    successEl.style.visibility = 'visible';
    successEl.style.opacity = '1';
    successEl.removeAttribute('hidden');
    successEl.classList.remove('d-none');
    successEl.classList.add('d-block');
    console.log('✅ [showPaymentSuccess] Showed success message');
    console.log('   - display:', successEl.style.display);
    console.log('   - visibility:', successEl.style.visibility);
    console.log('   - computed display:', window.getComputedStyle(successEl).display);
  } else {
    console.warn('⚠️ [showPaymentSuccess] spSuccess element not found');
  }
  
  // Hide QR image - force với nhiều cách
  if (qrImg) {
    qrImg.style.display = 'none';
    qrImg.style.visibility = 'hidden';
    qrImg.style.opacity = '0';
    qrImg.setAttribute('hidden', '');
    qrImg.src = ''; // Clear src để đảm bảo không load lại
    console.log('✅ [showPaymentSuccess] Hidden QR image');
    console.log('   - display:', qrImg.style.display);
    console.log('   - visibility:', qrImg.style.visibility);
    console.log('   - computed display:', window.getComputedStyle(qrImg).display);
  } else {
    console.warn('⚠️ [showPaymentSuccess] spQRImage element not found');
  }
  
  // Hide QR section - force với nhiều cách
  if (qrSection) {
    qrSection.style.display = 'none';
    qrSection.style.visibility = 'hidden';
    qrSection.style.opacity = '0';
    qrSection.setAttribute('hidden', '');
    qrSection.classList.add('d-none');
    qrSection.classList.remove('d-block');
    console.log('✅ [showPaymentSuccess] Hidden QR section');
    console.log('   - display:', qrSection.style.display);
    console.log('   - visibility:', qrSection.style.visibility);
    console.log('   - computed display:', window.getComputedStyle(qrSection).display);
  } else {
    console.warn('⚠️ [showPaymentSuccess] spQRSection element not found');
  }
  
  // Force modal to update - trigger reflow
  if (modal.classList.contains('show')) {
    console.log('✅ [showPaymentSuccess] Modal is visible, forcing UI update...');
    // Force reflow
    void modal.offsetHeight;
    // Trigger repaint
    requestAnimationFrame(() => {
      void modal.offsetHeight;
    });
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

