/**
 * Main
 */

'use strict';

let menu, animate;

(function () {
  // Initialize menu
  //-----------------
  console.log('🔵 [main.js] Initializing menu...');
  console.log('🔵 [main.js] PerfectScrollbar available:', typeof PerfectScrollbar !== 'undefined');
  console.log('🔵 [main.js] Menu class available:', typeof Menu !== 'undefined');
  console.log('🔵 [main.js] Helpers available:', typeof window.Helpers !== 'undefined');

  // Function to initialize menu when element is ready
  function initMenuWhenReady() {
    // Try to find menu in common-menu container (loaded via fetch)
    const commonMenu = document.getElementById('common-menu');
    let layoutMenuEl = null;
    
    if (commonMenu) {
      layoutMenuEl = commonMenu.querySelector('#layout-menu');
    }
    
    // If not found in common-menu, try direct query
    if (!layoutMenuEl) {
      layoutMenuEl = document.querySelectorAll('#layout-menu');
    }
    
    if (layoutMenuEl && layoutMenuEl.length > 0) {
      console.log('🔵 [main.js] Found menu elements:', layoutMenuEl.length);
      
  layoutMenuEl.forEach(function (element) {
        console.log('🔵 [main.js] Initializing menu for element:', element);
        menu = new Menu(element, {
          orientation: 'vertical',
          closeChildren: false
        });
        // Change parameter to true if you want scroll animation
        window.Helpers.scrollToActive((animate = false));
        window.Helpers.mainMenu = menu;
        console.log('🔵 [main.js] Menu initialized, scrollbar:', menu._scrollbar ? 'YES' : 'NO');
      });
      return true;
    }
    
    console.log('🔵 [main.js] Menu not ready yet, will retry...');
    return false;
  }

  // Try to initialize immediately
  if (!initMenuWhenReady()) {
    // If not ready, wait a bit and try again (menu is loading via fetch)
    setTimeout(initMenuWhenReady, 500);
    setTimeout(initMenuWhenReady, 1000);
    setTimeout(initMenuWhenReady, 2000);
  }


  // Initialize menu togglers and bind click on each
  let menuToggler = document.querySelectorAll('.layout-menu-toggle');
  menuToggler.forEach(item => {
    item.addEventListener('click', event => {
      event.preventDefault();
      window.Helpers.toggleCollapsed();
    });
  });

  // Display menu toggle (layout-menu-toggle) on hover with delay
  let delay = function (elem, callback) {
    let timeout = null;
    elem.onmouseenter = function () {
      // Set timeout to be a timer which will invoke callback after 300ms (not for small screen)
      if (!Helpers.isSmallScreen()) {
        timeout = setTimeout(callback, 300);
      } else {
        timeout = setTimeout(callback, 0);
      }
    };

    elem.onmouseleave = function () {
      // Clear any timers set to timeout
      document.querySelector('.layout-menu-toggle').classList.remove('d-block');
      clearTimeout(timeout);
    };
  };
  if (document.getElementById('layout-menu')) {
    delay(document.getElementById('layout-menu'), function () {
      // not for small screen
      if (!Helpers.isSmallScreen()) {
        document.querySelector('.layout-menu-toggle').classList.add('d-block');
      }
    });
  }

  // Display in main menu when menu scrolls
  let menuInnerContainer = document.getElementsByClassName('menu-inner'),
    menuInnerShadow = document.getElementsByClassName('menu-inner-shadow')[0];
  if (menuInnerContainer.length > 0 && menuInnerShadow) {
    menuInnerContainer[0].addEventListener('ps-scroll-y', function () {
      if (this.querySelector('.ps__thumb-y').offsetTop) {
        menuInnerShadow.style.display = 'block';
      } else {
        menuInnerShadow.style.display = 'none';
      }
    });
  }

  // Init helpers & misc
  // --------------------

  // Init BS Tooltip
  const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
  tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl);
  });

  // Init BS Dropdown - Khởi tạo tất cả dropdown menu
  function initializeDropdowns() {
    if (typeof bootstrap !== 'undefined' && bootstrap.Dropdown) {
      const dropdownToggles = document.querySelectorAll('.dropdown-toggle[data-bs-toggle="dropdown"]');
      dropdownToggles.forEach(toggle => {
        try {
          // Kiểm tra xem dropdown đã được khởi tạo chưa
          const existing = bootstrap.Dropdown.getInstance(toggle);
          if (!existing) {
            new bootstrap.Dropdown(toggle, {
              boundary: document.body,
              popperConfig: {
                strategy: 'fixed',
                placement: 'bottom-end',
                modifiers: [
                  {
                    name: 'preventOverflow',
                    options: {
                      boundary: document.body,
                      padding: 8
                    }
                  },
                  {
                    name: 'flip',
                    options: {
                      boundary: document.body,
                      padding: 8
                    }
                  },
                  {
                    name: 'offset',
                    options: {
                      offset: [0, 5]
                    }
                  }
                ]
              }
            });
          }
        } catch (e) {
          console.warn('Error initializing dropdown:', e);
        }
      });
    }
  }

  // Khởi tạo dropdown khi DOM ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeDropdowns);
  } else {
    initializeDropdowns();
  }

  // Khởi tạo lại dropdown sau khi có thay đổi trong DOM (cho DataTables và dynamic content)
  // Chỉ quan sát các phần quan trọng để tối ưu performance
  let reinitTimeout;
  const observer = new MutationObserver(function(mutations) {
    let shouldReinit = false;
    mutations.forEach(function(mutation) {
      if (mutation.addedNodes.length > 0) {
        mutation.addedNodes.forEach(function(node) {
          if (node.nodeType === 1) {
            // Kiểm tra nếu node là dropdown-toggle hoặc chứa dropdown-toggle
            if (node.classList?.contains('dropdown-toggle') || 
                node.querySelector?.('.dropdown-toggle') ||
                node.matches?.('tbody') || 
                node.matches?.('tr') ||
                (node.querySelector && (node.querySelector('tbody') || node.querySelector('tr')))) {
              shouldReinit = true;
            }
          }
        });
      }
    });
    if (shouldReinit) {
      // Debounce để tránh gọi quá nhiều lần
      clearTimeout(reinitTimeout);
      reinitTimeout = setTimeout(initializeDropdowns, 150);
    }
  });

  // Quan sát thay đổi trong các table và card (nơi thường có dropdown)
  const tables = document.querySelectorAll('table, .table, .card, .dataTables_wrapper');
  tables.forEach(table => {
    observer.observe(table, {
      childList: true,
      subtree: true
    });
  });

  // Cũng quan sát body nhưng chỉ khi có thay đổi lớn
  observer.observe(document.body, {
    childList: true,
    subtree: false // Chỉ quan sát direct children của body
  });

  // Export function để các trang khác có thể gọi
  window.initializeDropdowns = initializeDropdowns;

  // Tự động khởi tạo dropdown khi DataTables vẽ lại
  // Listen cho sự kiện draw của tất cả DataTables
  if (typeof $ !== 'undefined') {
    $(document).on('draw.dt', 'table', function() {
      // Khởi tạo dropdown sau khi DataTable vẽ lại
      if (window.initializeDropdowns) {
        setTimeout(() => {
          window.initializeDropdowns();
        }, 100);
      }
    });
    
    // Cũng khởi tạo khi DataTable được khởi tạo lần đầu
    $(document).on('init.dt', 'table', function() {
      if (window.initializeDropdowns) {
        setTimeout(() => {
          window.initializeDropdowns();
        }, 200);
      }
    });
  }

  // Accordion active class
  const accordionActiveFunction = function (e) {
    if (e.type == 'show.bs.collapse' || e.type == 'show.bs.collapse') {
      e.target.closest('.accordion-item').classList.add('active');
    } else {
      e.target.closest('.accordion-item').classList.remove('active');
    }
  };

  const accordionTriggerList = [].slice.call(document.querySelectorAll('.accordion'));
  const accordionList = accordionTriggerList.map(function (accordionTriggerEl) {
    accordionTriggerEl.addEventListener('show.bs.collapse', accordionActiveFunction);
    accordionTriggerEl.addEventListener('hide.bs.collapse', accordionActiveFunction);
  });

  // Auto update layout based on screen size
  if (window.Helpers && typeof window.Helpers.setAutoUpdate === 'function') {
    window.Helpers.setAutoUpdate(true);
  } else {
    console.warn('⚠️ Helpers.setAutoUpdate not available');
  }

  // Toggle Password Visibility
  if (window.Helpers && typeof window.Helpers.initPasswordToggle === 'function') {
    window.Helpers.initPasswordToggle();
  }

  // Speech To Text
  if (window.Helpers && typeof window.Helpers.initSpeechToText === 'function') {
    window.Helpers.initSpeechToText();
  }

  // Manage menu expanded/collapsed with templateCustomizer & local storage
  //------------------------------------------------------------------

  // If current layout is horizontal OR current window screen is small (overlay menu) than return from here
  if (window.Helpers && typeof window.Helpers.isSmallScreen === 'function' && window.Helpers.isSmallScreen()) {
    return;
  }

  // If current layout is vertical and current window screen is > small

  // Auto update menu collapsed/expanded based on the themeConfig
  if (window.Helpers && typeof window.Helpers.setCollapsed === 'function') {
    window.Helpers.setCollapsed(true, false);
  }
})();
