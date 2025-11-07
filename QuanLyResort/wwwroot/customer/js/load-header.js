/**
 * Unified Header Loader
 * Loads and injects header component into all customer pages
 */

(function() {
  'use strict';

  // Get current page identifier from pathname
  function getCurrentPage() {
    const path = window.location.pathname;
    const filename = path.split('/').pop() || 'index.html';
    
    // Map filenames to page identifiers
    const pageMap = {
      'index.html': 'index',
      'rooms.html': 'rooms',
      'room-detail.html': 'rooms',
      'restaurant.html': 'restaurant',
      'reviews.html': 'reviews',
      'about.html': 'about',
      'blog.html': 'blog',
      'contact.html': 'contact',
      'my-bookings.html': 'index',
      'booking-details.html': 'index',
      'booking-success.html': 'index',
      'order-details.html': 'restaurant',
      'account.html': 'index'
    };
    
    return pageMap[filename] || 'index';
  }

  // Set active nav item based on current page
  function setActiveNav(currentPage) {
    const navItems = document.querySelectorAll('#ftco-navbar [data-nav]');
    navItems.forEach(item => {
      if (item.getAttribute('data-nav') === currentPage) {
        item.classList.add('active');
      } else {
        item.classList.remove('active');
      }
    });
  }

  // Load header component
  async function loadHeader(forceReload = false) {
    const headerContainer = document.getElementById('header-placeholder');
    
    if (!headerContainer) {
      console.warn('[LoadHeader] Header placeholder not found');
      return;
    }

    try {
      // Always use timestamp for cache busting
      const timestamp = Date.now();
      const url = `components/header.html?_=${timestamp}`;
      
      // Force bypass cache completely
      const response = await fetch(url, { 
        cache: 'no-store',
        method: 'GET',
        headers: {
          'Cache-Control': 'no-cache, no-store, must-revalidate, max-age=0',
          'Pragma': 'no-cache',
          'Expires': '0',
          'X-Requested-With': 'XMLHttpRequest'
        }
      });
      
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      const html = await response.text();
      headerContainer.innerHTML = html;

      // Set active nav item after header is loaded
      const currentPage = getCurrentPage();
      setTimeout(() => {
        setActiveNav(currentPage);
        
        // Trigger navbar-auth update after header is loaded
        // navbar-auth.js uses MutationObserver, but we can also trigger it directly
        if (typeof updateNavbarAuth === 'function') {
          updateNavbarAuth();
        } else {
          // Dispatch a custom event that navbar-auth might listen to
          const event = new CustomEvent('headerLoaded', { detail: { page: currentPage } });
          window.dispatchEvent(event);
        }
      }, 150);

      console.log('[LoadHeader] Header loaded successfully for page:', currentPage);
    } catch (error) {
      console.error('[LoadHeader] Error loading header:', error);
      // Fallback: show basic header structure
      headerContainer.innerHTML = `
        <nav class="navbar navbar-expand-lg navbar-dark ftco_navbar bg-dark ftco-navbar-light" id="ftco-navbar">
          <div class="container">
            <a class="navbar-brand" href="index.html">RESORT DELUXE</a>
            <button class="navbar-toggler" type="button" data-toggle="collapse" data-target="#ftco-nav" aria-controls="ftco-nav" aria-expanded="false" aria-label="Toggle navigation">
              <span class="oi oi-menu"></span> Menu
            </button>
            <div class="collapse navbar-collapse" id="ftco-nav">
              <ul class="navbar-nav ml-auto">
                <li class="nav-item"><a href="index.html" class="nav-link">Trang chủ</a></li>
                <li class="nav-item"><a href="rooms.html" class="nav-link">Phòng</a></li>
                <li class="nav-item"><a href="restaurant.html" class="nav-link">Nhà hàng</a></li>
                <li class="nav-item"><a href="reviews.html" class="nav-link">Đánh giá</a></li>
                <li class="nav-item"><a href="about.html" class="nav-link">Giới thiệu</a></li>
                <li class="nav-item"><a href="blog.html" class="nav-link">Blog</a></li>
                <li class="nav-item"><a href="contact.html" class="nav-link">Liên hệ</a></li>
              </ul>
              <div class="ml-auto d-flex align-items-center">
                <div id="navbar-auth-placeholder"></div>
              </div>
            </div>
          </div>
        </nav>
      `;
      setActiveNav(getCurrentPage());
      
      // Trigger navbar-auth update for fallback header too
      setTimeout(() => {
        if (typeof updateNavbarAuth === 'function') {
          updateNavbarAuth();
        } else {
          window.dispatchEvent(new CustomEvent('headerLoaded'));
        }
      }, 150);
    }
  }

  // Track if header is already loaded to avoid duplicate loads
  let headerLoaded = false;

  // Auto-load header when DOM is ready
  function initializeHeader() {
    if (headerLoaded) return;
    
    if (document.readyState === 'loading') {
      document.addEventListener('DOMContentLoaded', () => {
        loadHeader(true);
        headerLoaded = true;
      });
    } else {
      // DOM already loaded
      loadHeader(true);
      headerLoaded = true;
    }
  }

  initializeHeader();

  // Re-load on pageshow (browser back/forward navigation)
  window.addEventListener('pageshow', function(event) {
    if (document.getElementById('header-placeholder')) {
      console.log('[LoadHeader] Pageshow event - reloading header');
      headerLoaded = false; // Reset flag to allow reload
      loadHeader(true); // Force reload with new timestamp
      headerLoaded = true;
    }
  });
  
  // Re-load on focus (when user returns to tab)
  window.addEventListener('focus', function() {
    if (document.getElementById('header-placeholder')) {
      console.log('[LoadHeader] Window focus - reloading header');
      loadHeader(true);
    }
  });

  // Re-load on hashchange (SPA-like navigation)
  window.addEventListener('hashchange', function() {
    if (document.getElementById('header-placeholder')) {
      console.log('[LoadHeader] Hashchange event - reloading header');
      loadHeader(true);
    }
  });

  // Re-load when page becomes visible (user switches tabs)
  document.addEventListener('visibilitychange', function() {
    if (!document.hidden && document.getElementById('header-placeholder')) {
      console.log('[LoadHeader] Page visible - reloading header');
      setTimeout(() => loadHeader(true), 100);
    }
  });

  // Expose reload function globally for manual refresh if needed
  window.reloadHeader = function() {
    console.log('[LoadHeader] Manual reload triggered');
    loadHeader(true);
  };

})();

