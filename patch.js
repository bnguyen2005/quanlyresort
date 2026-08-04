const fs = require('fs');
const path = 'd:/quanlyresort-main/quanlyresort-main/QuanLyResort/wwwroot/customer/room-detail.html';
let html = fs.readFileSync(path, 'utf8');

// Replace everything between async function submitBooking(room) { and // Navbar scroll effect
const startMarker = 'async function submitBooking(room) {';
const endMarker = '// Navbar scroll effect';
const startIndex = html.indexOf(startMarker);
const endIndex = html.indexOf(endMarker);

const submitBookingNew = `async function submitBooking(room) {
      const checkin = document.getElementById('checkinDate');
      const checkout = document.getElementById('checkoutDate');
      const numGuests = document.getElementById('numGuests');
      
      const fp = checkin._flatpickr;
      if (!fp || fp.selectedDates.length < 2) {
        showToast('Vui lòng chọn đầy đủ ngày nhận và trả phòng', 'warning');
        checkin.focus();
        return;
      }
      
      const checkinDate = fp.selectedDates[0];
      const checkoutDate = fp.selectedDates[1];
      const today = new Date();
      today.setHours(0,0,0,0);
      
      if (checkinDate < today) {
        showToast('Ngày nhận phòng không thể là quá khứ', 'warning');
        checkin.focus();
        return;
      }
      
      if (checkoutDate <= checkinDate) {
        showToast('Ngày trả phòng phải sau ngày nhận phòng', 'warning');
        checkin.focus();
        return;
      }
      
      const nights = Math.ceil((checkoutDate - checkinDate) / (1000 * 60 * 60 * 24));
      if (nights < 1) {
        showToast('Phải đặt tối thiểu 1 đêm', 'warning');
        return;
      }
      
      const guests = parseInt(numGuests.value) || 1;
      if (guests > (room.maxOccupancy || 4)) {
        showToast(\`Số khách vượt quá sức chứa (tối đa \${room.maxOccupancy || 4} khách)\`, 'warning');
        return;
      }

      if (guests <= 0) {
        showToast('Số khách không hợp lệ', 'warning');
        return;
      }
      
      const appliedCoupon = window.coupons?.getAppliedCoupon ? window.coupons.getAppliedCoupon() : null;

      try {
        const recheck = await fetch(\`\${API_ROOMS}/\${room.roomId}?_=\${Date.now()}\`, {cache:'no-store'});
        if (recheck.ok) {
          const latest = await recheck.json();
          if (latest && latest.isAvailable === false) {
            showToast('Phòng hiện không còn khả dụng, vui lòng chọn phòng khác', 'warning');
            return;
          }
        }
      } catch(_) { }
      
      const token = localStorage.getItem('token');
      const userStr = localStorage.getItem('user');
      if (!token || !userStr) {
        showToast('Vui lòng đăng nhập để đặt phòng', 'warning');
        const currentUrl = window.location.href;
        setTimeout(() => {
          window.location.href = \`login.html?redirect=\${encodeURIComponent(currentUrl)}\`;
        }, 1500);
        return;
      }
      
      let user;
      try {
        user = JSON.parse(userStr);
        if (!user || !user.customerId) {
          throw new Error('Invalid user data');
        }
      } catch(e) {
        showToast('Thông tin người dùng không hợp lệ. Vui lòng đăng nhập lại', 'danger');
        setTimeout(() => {
          window.location.href = 'login.html';
        }, 1500);
        return;
      }

      function getCustomerIdFromToken(tok){
        try{ const parts = tok.split('.'); if(parts.length>=2){ const payload = JSON.parse(atob(parts[1])); return payload.CustomerId || payload.customerId || null; } }catch(_){ }
        return null;
      }

      const cidFromToken = getCustomerIdFromToken(token);
      const finalCustomerId = cidFromToken || user.customerId || user.CustomerId || user.userId;

      if (!finalCustomerId) {
        showToast('Không tìm thấy thông tin khách hàng. Vui lòng đăng nhập lại', 'warning');
        setTimeout(() => window.location.href = '../admin/html/login.html', 2000);
        return;
      }
      
      const basePrice = (room.pricePerNight || room.basePrice || 0) * nights;
      const { total: totalPrice, discount } = window.coupons?.calculateDiscountedTotal 
        ? window.coupons.calculateDiscountedTotal(basePrice, appliedCoupon) 
        : { total: basePrice, discount: 0 };
      
      let confirmMessage = \`Xác nhận đặt phòng?\\n\\n\` +
        \`📅 Ngày nhận: \${checkinDate.toLocaleDateString('vi-VN')}\\n\` +
        \`📅 Ngày trả: \${checkoutDate.toLocaleDateString('vi-VN')}\\n\` +
        \`🌙 Số đêm: \${nights} đêm\\n\` +
        \`👥 Số khách: \${guests} khách\\n\`;
      
      if (appliedCoupon) {
        const couponCode = appliedCoupon.code || appliedCoupon.Code || appliedCoupon.code;
        if (appliedCoupon.pending) {
          confirmMessage += \`🎟️ Mã giảm giá: \${couponCode} (Sẽ được kiểm tra khi đặt phòng)\\n\`;
        } else if (discount > 0) {
          confirmMessage += \`🎟️ Mã giảm giá: \${couponCode}\\n\` +
            \`💸 Giảm: -\${new Intl.NumberFormat('vi-VN', {style:'currency', currency:'VND'}).format(discount)}\\n\`;
        }
      }
      
      confirmMessage += \`💰 Tổng tiền: \${new Intl.NumberFormat('vi-VN', {style:'currency', currency:'VND'}).format(totalPrice)}\`;
      
      const ok = await showConfirm('Xác nhận đặt phòng', confirmMessage, 'Đặt ngay', 'Hủy');
      if (!ok) return;
      
      try {
        showPageLoading('Đang xử lý đặt phòng...');

        let specialRequestsObj = {};
        const specialRequestsText = document.getElementById('specialRequests')?.value || '';
        
        if (specialRequestsText) {
          try {
            const parsed = JSON.parse(specialRequestsText);
            if (typeof parsed === 'object' && parsed !== null) {
              specialRequestsObj = parsed;
            } else {
              specialRequestsObj.note = specialRequestsText;
            }
          } catch(_) {
            specialRequestsObj.note = specialRequestsText;
          }
        }
        
        const formFields = {
          guestName: document.getElementById('bf_fullName')?.value || '',
          guestEmail: document.getElementById('bf_email')?.value || '',
          guestPhone: document.getElementById('bf_phone')?.value || '',
          nationality: document.getElementById('bf_nationality')?.value || '',
          idCard: document.getElementById('bf_idCard')?.value || '',
          address: document.getElementById('bf_address')?.value || '',
          arrivalTime: document.getElementById('bf_eta')?.value || '',
          paymentMethod: document.getElementById('bf_payment')?.value || '',
        };
        Object.assign(specialRequestsObj, formFields);
        
        const note = document.getElementById('bf_notes')?.value || '';
        if (note) {
          specialRequestsObj.note = (specialRequestsObj.note ? specialRequestsObj.note + '\\n' : '') + note;
        }
        
        if (appliedCoupon) {
          const couponCode = appliedCoupon.code || appliedCoupon.Code || appliedCoupon.code;
          if (couponCode) {
            specialRequestsObj.couponCode = couponCode;
          }
        }
        
        const bookingData = {
          customerId: finalCustomerId,
          requestedRoomType: room.roomTypeName || room.roomType || 'Standard',
          checkInDate: checkinDate.toISOString(),
          checkOutDate: checkoutDate.toISOString(),
          numberOfGuests: guests,
          specialRequests: JSON.stringify(specialRequestsObj),
          source: 'Website'
        };
        
        const API_BOOKINGS = location.origin + '/api/bookings';
        const bookingResp = await fetch(API_BOOKINGS, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': \`Bearer \${token}\`
          },
          body: JSON.stringify(bookingData)
        });
        
        if (!bookingResp.ok) {
          let errorMessage = \`HTTP \${bookingResp.status}: \${bookingResp.statusText}\`;
          try {
            const errorData = await bookingResp.json();
            errorMessage = errorData.message || errorMessage;
          } catch(e) {
            const errorText = await bookingResp.text();
            errorMessage = errorText || errorMessage;
          }
          throw new Error(errorMessage);
        }
        
        const bookingResult = await bookingResp.json();
        const bookingIdOrCode = bookingResult.bookingCode || bookingResult.bookingId;
        
        if (bookingData.paymentMethod === 'QR' && typeof openSimplePayment === 'function') {
            openSimplePayment(bookingIdOrCode, bookingResult.bookingCode, totalPrice);
        } else {
            showToast('Đặt phòng thành công! Mã đặt phòng: ' + bookingIdOrCode, 'success');
            setTimeout(() => {
              window.location.href = \`booking-success.html?bookingId=\${bookingIdOrCode}\`;
            }, 2000);
        }
        
      } catch(e) {
        showToast('Lỗi đặt phòng: ' + e.message, 'danger');
      } finally {
        hidePageLoading();
      }
    }
    
    `;
    
html = html.substring(0, startIndex) + submitBookingNew + html.substring(endIndex);

// Also fix lines 983, 988 in the click listener if they have mojibake
html = html.replace(/showToast\([^)]*H.*t.n, Email v.*SDT[^)]*\)/g, "showToast('Vui lòng nhập Họ tên, Email và SĐT', 'warning')");
html = html.replace(/showToast\([^)]*đi.*u kho.*n & ch.*nh s.*ch[^)]*\)/g, "showToast('Vui lòng đồng ý điều khoản & chính sách', 'warning')");

fs.writeFileSync(path, html, 'utf8');
