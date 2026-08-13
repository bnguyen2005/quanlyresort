// wwwroot/customer/js/page-transition.js

const PAGE_ORDER = [
  'index.html',
  'rooms.html',
  'restaurant.html',
  'about.html',
  'reviews.html'
];

document.addEventListener('DOMContentLoaded', () => {
  const container = document.getElementById('snapContainer');
  const currentPath = window.location.pathname.split('/').pop() || 'index.html';
  const currentIndex = PAGE_ORDER.findIndex(p => p.startsWith(currentPath)) === -1 ? 0 : PAGE_ORDER.findIndex(p => p.startsWith(currentPath));
  
  // 1. Initial Load Animation
  // Mặc định body sẽ có opacity = 0 (bằng CSS inline hoặc class) để chờ hiệu ứng
  const transitionDirection = sessionStorage.getItem('transitionDirection');
  if (transitionDirection === 'right') {
    document.body.classList.add('slide-in-from-right');
  } else if (transitionDirection === 'left') {
    document.body.classList.add('slide-in-from-left');
  } else {
    document.body.classList.add('fade-in-page');
  }
  sessionStorage.removeItem('transitionDirection');

  // 2. Intercept Wheel Scroll ONLY if container exists
  if (container) {
    let transitionTriggered = false;
    
    container.addEventListener('wheel', (evt) => {
      const isHorizontalScroll = Math.abs(evt.deltaX) > Math.abs(evt.deltaY);
      const delta = isHorizontalScroll ? evt.deltaX : evt.deltaY;
      
      const isScrollableSlide = evt.target.closest('.ts-slide[style*="overflow-y: auto"]');
      if (isScrollableSlide && !isHorizontalScroll) return; 

      if (delta !== 0) {
        const direction = delta > 0 ? 1 : -1;
        const maxScrollLeft = container.scrollWidth - container.clientWidth;
        // Tròn số để tránh sai số thập phân trên một số màn hình
        const currentScrollLeft = Math.ceil(container.scrollLeft);
        
        // Nếu đã ở Slide cuối và tiếp tục cuộn xuống/phải -> Chuyển sang trang kế
        if (direction === 1 && currentScrollLeft >= Math.floor(maxScrollLeft) - 10) {
          if (!window.DISABLE_PAGE_TRANSITION && currentIndex < PAGE_ORDER.length - 1) {
            evt.preventDefault();
            if (transitionTriggered) return;
            transitionTriggered = true;
            navigateToPage(PAGE_ORDER[currentIndex + 1], 'right');
            return;
          }
        }
        
        // Nếu đang ở Slide đầu và tiếp tục cuộn lên/trái -> Trở về trang trước
        if (direction === -1 && currentScrollLeft <= 10) {
          if (!window.DISABLE_PAGE_TRANSITION && currentIndex > 0) {
            evt.preventDefault();
            if (transitionTriggered) return;
            transitionTriggered = true;
            navigateToPage(PAGE_ORDER[currentIndex - 1], 'left');
            return;
          }
        }
        
        // Nếu dùng cuộn chuột dọc thông thường, ta đẩy giá trị deltaY sang scrollLeft
        // Trình duyệt sẽ tự động bắt dính (scroll-snap) cực mượt mà không bị khựng
        if (!isHorizontalScroll) {
          evt.preventDefault();
          container.scrollBy({ left: evt.deltaY, behavior: 'auto' });
        }
      }
    }, { passive: false });
  }

  // 3. Intercept Links (Từ Sidebar, Header, hoặc các nút CTA)
  document.addEventListener('click', (e) => {
    const link = e.target.closest('a');
    if (!link) return;
    
    const href = link.getAttribute('href');
    if (!href || href.startsWith('#') || href.startsWith('javascript') || href.startsWith('http') || link.target === '_blank') return;
    
    // Kiểm tra xem href có nằm trong PAGE_ORDER không
    const targetPath = href.split('/').pop().split('?')[0] || 'index.html';
    const targetIndex = PAGE_ORDER.findIndex(p => p.startsWith(targetPath));
    
    if (targetIndex !== -1) {
      if (targetIndex === currentIndex) {
        // Cùng trang thì không làm gì
        return;
      }
      e.preventDefault();
      const dir = targetIndex > currentIndex ? 'right' : 'left';
      navigateToPage(href, dir);
    }
  });

  // Navigation Logic
  function navigateToPage(url, dir) {
    sessionStorage.setItem('transitionDirection', dir);
    if (dir === 'right') {
      document.body.classList.add('slide-out-to-left');
    } else {
      document.body.classList.add('slide-out-to-right');
    }
    setTimeout(() => {
      window.location.href = url;
    }, 600); 
  }
});
