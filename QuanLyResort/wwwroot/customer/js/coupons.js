(function(){
  'use strict';

  const STORAGE_KEY = 'appliedCoupon';

  async function validateCoupon(code) {
    const url = `${location.origin}/api/coupons/validate?code=${encodeURIComponent(code)}&_=${Date.now()}`;
    // Don't send Authorization header for validate endpoint - it's [AllowAnonymous]
    // Sending header with insufficient permissions may cause 403
    const resp = await fetch(url, { cache: 'no-store' });
    if (!resp.ok) {
      let message = 'Mã giảm giá không hợp lệ';
      if (resp.status === 401) {
        message = 'Bạn chưa đăng nhập hoặc phiên đã hết hạn. Vui lòng đăng nhập lại để áp dụng mã.';
      } else if (resp.status === 403) {
        message = 'Tài khoản hiện không có quyền kiểm tra mã. Mã sẽ được hệ thống xác thực khi gửi đặt phòng.';
      }
      try { const data = await resp.json(); message = data.message || message; } catch(_){ }
      const err = new Error(message);
      err.status = resp.status;
      throw err;
    }
    return await resp.json();
  }

  function saveAppliedCoupon(coupon) {
    try { localStorage.setItem(STORAGE_KEY, JSON.stringify(coupon)); } catch(_){}
  }
  function getAppliedCoupon() {
    try { return JSON.parse(localStorage.getItem(STORAGE_KEY) || 'null'); } catch(_){ return null; }
  }
  function clearAppliedCoupon() {
    try { localStorage.removeItem(STORAGE_KEY); } catch(_){ }
  }

  // Apply discount to a base amount in VND
  function calculateDiscountedTotal(baseAmount, coupon) {
    if (!coupon) return { total: baseAmount, discount: 0 };
    const type = (coupon.type || coupon.discountType || '').toLowerCase();
    const value = Number(coupon.value || coupon.discountValue || 0);
    let discount = 0;
    if (type === 'percent' || type === 'percentage') {
      discount = Math.round(baseAmount * (value / 100));
      const max = Number(coupon.maxDiscount || 0);
      if (max > 0) discount = Math.min(discount, max);
    } else if (type === 'amount' || type === 'fixed') {
      discount = Math.round(value);
    }
    if (discount < 0) discount = 0;
    if (discount > baseAmount) discount = baseAmount;
    return { total: baseAmount - discount, discount };
  }

  window.coupons = {
    validateCoupon,
    saveAppliedCoupon,
    getAppliedCoupon,
    clearAppliedCoupon,
    calculateDiscountedTotal
  };
})();


