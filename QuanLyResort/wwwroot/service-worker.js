// Service Worker for Resort Management System
const CACHE_NAME = 'resort-cache-v34'; // Force update - Fix CORS by clearing old cached JS files
const urlsToCache = [
  '/customer/index.html',
  // REMOVED: '/customer/login.html' - KHÔNG cache trang login!
  // REMOVED: '/customer/register.html' - có dữ liệu động
  // REMOVED: '/customer/rooms.html' - có dữ liệu động từ API
  '/customer/css/style.css',
      '/customer/js/main.js'
      // REMOVED: customer-api.js, customer-login.js, customer-register.js - để chúng luôn fresh
];

// URLs that should NEVER be cached (always fetch fresh)
const NEVER_CACHE_URLS = [
  '/api/restaurant-orders',
  '/customer/login.html',
  '/customer/register.html',
  '/customer/rooms.html',      // Có dữ liệu động từ API
  '/customer/room-detail.html', // Có dữ liệu động từ API
  '/customer/my-bookings.html', // Có dữ liệu động từ API
  '/customer/account.html',     // Có dữ liệu động từ API
  '/customer/booking-details.html', // Có dữ liệu động từ API
  '/customer/booking-success.html',
  '/customer/blog.html',  // Blog page với dynamic footer fixes
  '/portal.html',
  '/test-cache.html',
  '/customer/js/customer-api.js',  // NEVER cache - có API URL động
      '/customer/js/customer-register.js',  // NEVER cache - có API URL động
      '/customer/js/navbar-auth.js',
  '/customer/js/load-header.js',  // Header loader script
  '/customer/components/header.html',  // Header component - NEVER cache
  '/admin/',  // KHÔNG cache TẤT CẢ admin pages
  '/admin/html/',  // KHÔNG cache admin HTML pages
  '/admin/html/rooms.html',  // KHÔNG cache rooms.html specifically
  'layout-menu.html'  // KHÔNG cache menu component
];

// Install event - cache resources
self.addEventListener('install', (event) => {
  console.log('[Service Worker] Installing...');
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then((cache) => {
        console.log('[Service Worker] Caching app shell');
        return cache.addAll(urlsToCache);
      })
      .catch((error) => {
        console.error('[Service Worker] Cache failed:', error);
      })
  );
  self.skipWaiting();
});

// Activate event - clean up old caches
self.addEventListener('activate', (event) => {
  console.log('[Service Worker] Activating...');
  event.waitUntil(
    caches.keys().then((cacheNames) => {
      return Promise.all(
        cacheNames.map((cacheName) => {
          if (cacheName !== CACHE_NAME) {
            console.log('[Service Worker] Deleting old cache:', cacheName);
            return caches.delete(cacheName);
          }
        })
      );
    })
  );
  self.clients.claim();
});

// Fetch event - serve from cache, fallback to network
self.addEventListener('fetch', event => {
  const url = new URL(event.request.url);

  // Skip cross-origin requests (CDNs, analytics, third-party) - handle them via network-only
  if (url.origin !== location.origin) {
    console.log('[Service Worker] cross-origin request - network-only:', url.href);
    event.respondWith(
      fetch(event.request).catch(err => {
        console.error('[Service Worker] cross-origin fetch failed:', url.href, err);
        return caches.match('/offline.html').then(fallback => fallback || new Response('Offline', { status: 503 }));
      })
    );
    return;
  }

  // Bypass API calls: trả trực tiếp network (không cache) và bắt lỗi
  if (url.pathname.startsWith('/api/')) {
    event.respondWith(
      fetch(event.request)
        .then(response => {
          return response;
        })
        .catch(async err => {
          console.error('[Service Worker] network error for API:', url.href, err);
          // trả cache nếu có (GET), ngược lại trả JSON lỗi để client xử lý
          try {
            const cached = await caches.match(event.request);
            if (cached) return cached;
          } catch (e) {
            // ignore cache errors
          }
          return new Response(JSON.stringify({ error: 'Service Worker: network error' }), {
            status: 503,
            headers: { 'Content-Type': 'application/json' }
          });
        })
    );
    return; // dừng xử lý tiếp cho request này
  }

  // Skip non-GET requests
  if (event.request.method !== 'GET') {
    return;
  }

  // Skip ALL admin pages - always fetch fresh
  if (event.request.url.includes('/admin/')) {
    console.log('[Service Worker] ADMIN PAGE - fetching fresh:', event.request.url);
    event.respondWith(fetch(event.request));
    return;
  }

  // Check if URL should NEVER be cached
  const shouldNeverCache = NEVER_CACHE_URLS.some(path => url.pathname.includes(path)) ||
                           url.pathname.includes('/components/') || // Never cache any component files
                           url.searchParams.has('_'); // Never cache URLs with cache-busting timestamp
  
  if (shouldNeverCache) {
    console.log('[Service Worker] NEVER CACHE - fetching fresh:', url.pathname);
    // Create a new request without cache to force fresh fetch
    const freshRequest = new Request(event.request, {
      cache: 'no-store',
      headers: {
        'Cache-Control': 'no-cache, no-store, must-revalidate, max-age=0',
        'Pragma': 'no-cache'
      }
    });
    event.respondWith(
      fetch(freshRequest)
        .catch((error) => {
          console.error('[Service Worker] Fetch failed for never-cache URL:', error);
          throw error;
        })
    );
    return;
  }

  // giữ nguyên logic cache-first / network-first của bạn, nhưng thêm catch để không ném
  event.respondWith(
    caches.match(event.request).then(cached => {
      if (cached) return cached;
      return fetch(event.request).then(resp => {
        // optional: cache static assets here
        return resp;
      }).catch(err => {
        console.error('[Service Worker] fetch failed (assets):', event.request.url, err);
        // trả fallback hoặc Response.error() hoặc cached fallback nếu có
        return caches.match('/offline.html').then(fallback => fallback || new Response('Offline', { status: 503 }));
      });
    })
  );
});

// Listen for messages from clients
self.addEventListener('message', (event) => {
  if (event.data && event.data.type === 'SKIP_WAITING') {
    self.skipWaiting();
  }
});

