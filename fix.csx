using System;
using System.IO;
using System.Text.RegularExpressions;

string path = @"d:\quanlyresort-main\quanlyresort-main\QuanLyResort\wwwroot\customer\room-detail.html";
string html = File.ReadAllText(path);

// 1. Add simple-payment.js and showToast
string scriptsTarget = "<script src=\"js/customer-api.js\"></script>";
if (!html.Contains("simple-payment.js")) {
    html = html.Replace(scriptsTarget, scriptsTarget + "\n  <script src=\"js/simple-payment.js\"></script>");
}

// 2. Add showToast definition
string initTarget = "const urlParams = new URLSearchParams(window.location.search);";
string showToastDef = @"
    window.showToast = window.showToast || function(message, type = 'info') {
      const bg = type === 'success' ? 'bg-success' : (type === 'danger' ? 'bg-danger' : (type === 'warning' ? 'bg-warning' : 'bg-info'));
      let container = document.getElementById('toast-container');
      if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.style.cssText = 'position: fixed; top: 20px; right: 20px; z-index: 9999;';
        document.body.appendChild(container);
      }
      const toastEl = document.createElement('div');
      toastEl.className = `toast align-items-center text-white ${bg} border-0 show`;
      toastEl.setAttribute('role', 'alert');
      toastEl.innerHTML = `<div class=""d-flex""><div class=""toast-body"">${message}</div><button type=""button"" class=""btn-close btn-close-white me-2 m-auto"" onclick=""this.parentElement.parentElement.remove()""></button></div>`;
      container.appendChild(toastEl);
      setTimeout(() => toastEl.remove(), 3000);
    };

    ";
if (!html.Contains("toast-container")) {
    html = html.Replace(initTarget, showToastDef + initTarget);
}

// 3. Fix mojibake and update submitBooking
// We will replace the entire submitBooking function using regex
string submitBookingPattern = @"async function submitBooking\(room\) \{.*?hidePageLoading\(\);\s*\}\s*\}";
string submitBookingReplacement = @"async function submitBooking(room) {
      const checkin = document.getElementById('checkinDate');
      const checkout = document.getElementById('checkoutDate');
      const numGuests = document.getElementById('numGuests');
      
      const fp = checkin._flatpickr;
      // Validation
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
        showToast(`Số khách vượt quá sức chứa (tối đa ${room.maxOccupancy || 4} khách)`, 'warning');
        return;
      }

      if (guests <= 0) {
        showToast('Số khách không hợp lệ', 'warning');
        return;
      }
      
      // Get applied coupon
      const appliedCoupon = window.coupons?.getAppliedCoupon ? window.coupons.getAppliedCoupon() : null;

      // Kiểm tra trạng thái phòng còn khả dụng
      try {
        const recheck = await fetch(`${API_ROOMS}/${room.roomId}?_=${Date.now()}`, {cache:'no-store'});
        if (recheck.ok) {
          const latest = await recheck.json();
          if (latest && latest.isAvailable === false) {
            showToast('Phòng hiện không còn khả dụng, vui lòng chọn phòng khác', 'warning');
            return;
          }
        }
      } catch(_) { /* bỏ qua nếu lỗi mạng nhẹ */ }
      
      // Check authentication
      const token = localStorage.getItem('token');
      const userStr = localStorage.getItem('user');
      if (!token || !userStr) {
        showToast('Vui lòng đăng nhập để đặt phòng', 'warning');
        // Lưu URL hiện tại để redirect lại sau khi login
        const currentUrl = window.location.href;
        setTimeout(() => {
          window.location.href = `login.html?redirect=${encodeURIComponent(currentUrl)}`;
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

      // Lấy CustomerId ưu tiên từ JWT claims, fallback sang user object
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
      
      let confirmMessage = `Xác nhận đặt phòng?\n\n` +
        `📅 Ngày nhận: ${checkinDate.toLocaleDateString('vi-VN')}\n` +
        `📅 Ngày trả: ${checkoutDate.toLocaleDateString('vi-VN')}\n` +
        `🌙 Số đêm: ${nights} đêm\n` +
        `👥 Số khách: ${guests} khách\n`;
      
      if (appliedCoupon) {
        const couponCode = appliedCoupon.code || appliedCoupon.Code || appliedCoupon.code;
        if (appliedCoupon.pending) {
          confirmMessage += `🎟️ Mã giảm giá: ${couponCode} (Sẽ được kiểm tra khi đặt phòng)\n`;
        } else if (discount > 0) {
          confirmMessage += `🎟️ Mã giảm giá: ${couponCode}\n` +
            `💸 Giảm: -${new Intl.NumberFormat('vi-VN', {style:'currency', currency:'VND'}).format(discount)}\n`;
        }
      }
      
      confirmMessage += `💰 Tổng tiền: ${new Intl.NumberFormat('vi-VN', {style:'currency', currency:'VND'}).format(totalPrice)}`;
      
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
          specialRequestsObj.note = (specialRequestsObj.note ? specialRequestsObj.note + '\n' : '') + note;
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
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify(bookingData)
        });
        
        if (!bookingResp.ok) {
          let errorMessage = `HTTP ${bookingResp.status}: ${bookingResp.statusText}`;
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
              window.location.href = `booking-success.html?bookingId=${bookingIdOrCode}`;
            }, 2000);
        }
        
      } catch(e) {
        showToast('Lỗi đặt phòng: ' + e.message, 'danger');
      } finally {
        hidePageLoading();
      }
    }';
html = Regex.Replace(html, submitBookingPattern, submitBookingReplacement, RegexOptions.Singleline);

// 4. Inject simplePaymentModal HTML before </body>
string modalHtml = @"
  <!-- Modal Thanh toán (SimplePayment) -->
  <div class=""modal fade"" id=""simplePaymentModal"" tabindex=""-1"">
    <div class=""modal-dialog modal-dialog-centered modal-lg"">
      <div class=""modal-content"" style=""border-radius: 20px;"">
        <div class=""modal-header"" style=""background: linear-gradient(135deg, #c8a97e 0%, #b89968 100%); color: white;"">
          <h5 class=""modal-title"" style=""font-size: 24px; font-weight: 700;"">💳 Thanh Toán</h5>
          <button type=""button"" class=""btn-close btn-close-white"" data-bs-dismiss=""modal""></button>
        </div>
        <div class=""modal-body"" style=""padding: 30px;"">
          <div class=""text-center mb-4"">
            <h6>Mã đặt phòng: <strong id=""spBookingCode"">-</strong></h6>
            <h4 class=""text-primary"">Số tiền: <span id=""spAmount"">0 ₫</span></h4>
          </div>

          <div id=""spQRSection"">
            <p class=""text-center mb-3"">
              <strong>Nội dung chuyển khoản:</strong><br>
              <code id=""spContent"" style=""background: #f8f9fa; padding: 8px 12px; border-radius: 8px; font-size: 16px; font-weight: 600;"">BOOKING-</code>
            </p>
            <div class=""text-center mb-4"">
              <img id=""spQRImage"" alt=""QR Code"" style=""max-width: 300px; border: 4px solid #e9ecef; border-radius: 15px; padding: 15px; display: none;"">
            </div>
            <div class=""card"" style=""background: #f8f9fa; padding: 20px; border-radius: 12px;"">
              <p class=""mb-2""><strong>Ngân hàng:</strong> MBBank</p>
              <p class=""mb-2""><strong>Số tài khoản:</strong> <span id=""spBankAccount"">0901329227</span></p>
              <p class=""mb-0""><strong>Chủ tài khoản:</strong> <span id=""spBankName"">Resort Deluxe</span></p>
            </div>
          </div>

          <div id=""spWaiting"" class=""text-center mt-4"" style=""display: block;"">
            <div class=""spinner-border text-primary"" role=""status""></div>
            <p class=""mt-2"">Đang chờ thanh toán...</p>
          </div>

          <div id=""spSuccess"" class=""text-center mt-4"" style=""display: none;"">
            <div class=""alert alert-success"">
              <h5>✅ Thanh toán thành công!</h5>
              <p>Đang cập nhật thông tin...</p>
            </div>
          </div>
        </div>
        <div class=""modal-footer"">
          <button type=""button"" class=""btn btn-secondary"" data-bs-dismiss=""modal"">Đóng</button>
        </div>
      </div>
    </div>
  </div>
";
if (!html.Contains("simplePaymentModal")) {
    html = html.Replace("</body>", modalHtml + "</body>");
}

File.WriteAllText(path, html);
