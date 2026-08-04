// PWA Initialization and Registration for Resort Management System

const ENABLE_PWA = true; // Set to false to disable PWA

// Register Service Worker
const registerServiceWorker = async () => {
  if (!ENABLE_PWA) {
    console.log('[PWA] PWA is disabled');
    return;
  }

  if ('serviceWorker' in navigator) {
    try {
      const registration = await navigator.serviceWorker.register('/service-worker.js', {
        scope: '/'
      });

      console.log('[PWA] Service Worker registered successfully:', registration.scope);

      // Check for updates
      registration.addEventListener('updatefound', () => {
        const newWorker = registration.installing;
        console.log('[PWA] New Service Worker found, installing...');

        newWorker.addEventListener('statechange', () => {
          if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
            // New update available
            console.log('[PWA] New version available! Refresh to update.');
            showUpdateNotification();
          }
        });
      });

    } catch (error) {
      console.error('[PWA] Service Worker registration failed:', error);
    }
  } else {
    console.warn('[PWA] Service Workers are not supported in this browser');
  }
};

// Show update notification
const showUpdateNotification = () => {
  if (confirm('Có phiên bản mới! Bạn có muốn cập nhật không?')) {
    window.location.reload();
  }
};

// Install prompt
let deferredPrompt;

window.addEventListener('beforeinstallprompt', (e) => {
  console.log('[PWA] beforeinstallprompt event fired');
  
  // Prevent the mini-infobar from appearing on mobile
  e.preventDefault();
  
  // Stash the event so it can be triggered later
  deferredPrompt = e;
  
  // Show install button/banner
  showInstallPromotion();
});

window.addEventListener('appinstalled', () => {
  console.log('[PWA] App was installed');
  deferredPrompt = null;
  hideInstallPromotion();
});

// Show install promotion
const showInstallPromotion = () => {
  const installBanner = document.getElementById('install-banner');
  if (installBanner) {
    installBanner.style.display = 'block';
  } else {
    // Create install banner dynamically
    createInstallBanner();
  }
};

// Hide install promotion
const hideInstallPromotion = () => {
  const installBanner = document.getElementById('install-banner');
  if (installBanner) {
    installBanner.style.display = 'none';
  }
};

// Create install banner
const createInstallBanner = () => {
  const banner = document.createElement('div');
  banner.id = 'install-banner';
  banner.style.cssText = `
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    background: #c8a97e;
    color: white;
    padding: 15px;
    text-align: center;
    z-index: 9999;
    box-shadow: 0 -2px 10px rgba(0,0,0,0.1);
  `;
  
  banner.innerHTML = `
    <div style="max-width: 800px; margin: 0 auto; display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap;">
      <div style="flex: 1; margin-right: 15px; text-align: left;">
        <strong>Cài đặt Resort App</strong>
        <p style="margin: 5px 0 0 0; font-size: 14px;">Cài đặt ứng dụng để truy cập nhanh hơn!</p>
      </div>
      <div>
        <button id="install-app-button" style="
          background: white;
          color: #c8a97e;
          border: none;
          padding: 10px 20px;
          border-radius: 5px;
          font-weight: 600;
          cursor: pointer;
          margin-right: 10px;
        ">Cài Đặt</button>
        <button id="close-banner-button" style="
          background: transparent;
          color: white;
          border: 1px solid white;
          padding: 10px 20px;
          border-radius: 5px;
          cursor: pointer;
        ">Đóng</button>
      </div>
    </div>
  `;
  
  document.body.appendChild(banner);
  
  // Add event listeners
  document.getElementById('install-app-button').addEventListener('click', installApp);
  document.getElementById('close-banner-button').addEventListener('click', () => {
    banner.style.display = 'none';
  });
};

// Install app
const installApp = async () => {
  if (!deferredPrompt) {
    console.log('[PWA] Install prompt not available');
    return;
  }

  // Show the install prompt
  deferredPrompt.prompt();

  // Wait for the user to respond to the prompt
  const { outcome } = await deferredPrompt.userChoice;
  console.log(`[PWA] User response to the install prompt: ${outcome}`);

  // Clear the deferred prompt
  deferredPrompt = null;
  hideInstallPromotion();
};

// Check if running as PWA
const isRunningAsPWA = () => {
  return window.matchMedia('(display-mode: standalone)').matches ||
         window.navigator.standalone === true;
};

// Initialize PWA
const initPWA = () => {
  if (!ENABLE_PWA) {
    return;
  }

  // Register service worker
  registerServiceWorker();

  // Check if already installed
  if (isRunningAsPWA()) {
    console.log('[PWA] Running as installed PWA');
    document.body.classList.add('pwa-installed');
  }

  // Add manifest link if not present
  if (!document.querySelector('link[rel="manifest"]')) {
    const manifestLink = document.createElement('link');
    manifestLink.rel = 'manifest';
    manifestLink.href = '/manifest.json';
    document.head.appendChild(manifestLink);
  }

  // Add theme-color meta if not present
  if (!document.querySelector('meta[name="theme-color"]')) {
    const themeColorMeta = document.createElement('meta');
    themeColorMeta.name = 'theme-color';
    themeColorMeta.content = '#c8a97e';
    document.head.appendChild(themeColorMeta);
  }

  // Add viewport meta if not present
  if (!document.querySelector('meta[name="viewport"]')) {
    const viewportMeta = document.createElement('meta');
    viewportMeta.name = 'viewport';
    viewportMeta.content = 'width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no';
    document.head.appendChild(viewportMeta);
  }
};

// Auto-init on page load
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', initPWA);
} else {
  initPWA();
}

