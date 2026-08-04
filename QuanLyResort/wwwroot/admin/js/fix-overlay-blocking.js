/**
 * Fix overlay blocking all buttons in admin pages
 * This script ensures overlay doesn't block clickable elements
 */

(function() {
  'use strict';

  function fixOverlayBlocking() {
    // Get all overlays
    const overlays = document.querySelectorAll('.layout-overlay');
    
    overlays.forEach(overlay => {
      // Check if menu is expanded
      const isMenuExpanded = document.body.classList.contains('layout-menu-expanded');
      
      if (!isMenuExpanded) {
        // Menu not expanded - overlay should not block
        overlay.style.pointerEvents = 'none';
        overlay.style.display = 'none';
      } else {
        // Menu expanded - overlay should only block background, not buttons
        overlay.style.pointerEvents = 'auto';
        
        // Ensure all buttons are still clickable even when overlay is active
        const allButtons = document.querySelectorAll(
          'button:not(:disabled), .btn:not(:disabled), a.btn, .dropdown-toggle, .dropdown-item, [onclick], [data-bs-toggle]'
        );
        
        allButtons.forEach(btn => {
          // Force pointer events and z-index
          if (window.getComputedStyle(btn).pointerEvents === 'none') {
            btn.style.pointerEvents = 'auto';
          }
          const zIndex = parseInt(window.getComputedStyle(btn).zIndex) || 0;
          if (zIndex < 100) {
            btn.style.zIndex = '100';
          }
        });
      }
    });

    // Monitor body class changes
    const observer = new MutationObserver(function(mutations) {
      mutations.forEach(function(mutation) {
        if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
          // Body class changed, re-check overlay
          setTimeout(fixOverlayBlocking, 100);
        }
      });
    });

    observer.observe(document.body, {
      attributes: true,
      attributeFilter: ['class']
    });
  }

  // Run on DOMContentLoaded
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', fixOverlayBlocking);
  } else {
    fixOverlayBlocking();
  }

  // Also run after a short delay to catch dynamically loaded content
  setTimeout(fixOverlayBlocking, 500);
  setTimeout(fixOverlayBlocking, 1000);
})();

