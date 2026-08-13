/* js/page-loader.js */

// Đặt gọi hàm này ở: (1) lúc trang load lần đầu, (2) mỗi lần PJAX xong (kể cả trang đích), (3) lúc resize
function syncSidebarWidthVar() {
  const nav = document.querySelector('#ftco-navbar');
  if (!nav) return;
  const realWidth = nav.getBoundingClientRect().width; // SỐ THẬT, không đoán
  document.documentElement.style.setProperty('--ts-sidebar-width', `${realWidth}px`);
}

syncSidebarWidthVar();
window.addEventListener('load', syncSidebarWidthVar); // đợi font/icon load xong mới đo, tránh đo hụt
window.addEventListener('resize', syncSidebarWidthVar);

// Chống wipe DOM do document.write trong quá trình PJAX
if (typeof document !== 'undefined') document.write = function() {};
const PageLoader = {
    cache: {},
    isFetching: false,
    accumulatedDelta: 0,
    ticking: false,
    currentIndex: 0,
    resizeTimeout: null,

    init() {
        // Bắt sự kiện click link nội bộ
        document.addEventListener('click', (e) => {
            const link = e.target.closest('a');
            if (!link) return;

            // Tự động đóng menu nếu click vào bất kỳ link nào
            document.body.classList.remove('menu-open');

            const href = link.getAttribute('href');
            if (this.shouldIntercept(link, href)) {
                e.preventDefault();
                this.load(href, { mode: 'replace', push: true });
            }
        });

        // Lắng nghe sự kiện Back/Forward trình duyệt
        window.addEventListener('popstate', (e) => {
            if (e.state && e.state.url && e.state.url !== location.href) {
                this.load(location.href, { mode: 'replace', push: false });
                return;
            }
            if (e.state && typeof e.state.slideIndex !== 'undefined') {
                const track = document.getElementById('page-content');
                if (track && !document.body.classList.contains('layout-vertical')) {
                    track.scrollTo({ left: e.state.slideIndex * window.innerWidth, behavior: 'smooth' });
                    return;
                }
            }
            this.load(location.href, { mode: 'replace', push: false });
        });

        const currentContent = document.querySelector('#page-content');
        if (currentContent) {
            const layout = currentContent.getAttribute('data-layout') || 'horizontal';
            if (layout === 'vertical') {
                document.body.classList.add('layout-vertical');
                document.documentElement.classList.add('layout-vertical');
            } else {
                document.body.classList.remove('layout-vertical');
                document.documentElement.classList.remove('layout-vertical');
            }
        }

        // Khởi tạo Scroll-Jacking
        this.setupScrollJacking();

        // Khởi tạo Keyboard Navigation
        this.setupKeyboardNavigation();

        // Header scroll và Cookie
        this.setupHeaderScroll();
        this.setupCookieBanner();

        // Tandjung Sari animations (Parallax + Reveal)
        this.setupTandjungAnimations();

        // Khởi tạo push state ban đầu
        history.replaceState({ url: location.href, slideIndex: 0 }, document.title, location.href);

        // Đánh dấu đã khởi tạo cho DOM hiện tại
        this.markInitialized(document);

        // Khởi tạo Lenis Smooth Scroll cho trang
        this.setupLenis();
    },

    shouldIntercept(link, href) {
        if (!href) return false;
        if (href.startsWith('http') || href.startsWith('mailto:') || href.startsWith('tel:') || href.startsWith('#')) return false;
        if (link.getAttribute('target') === '_blank') return false;
        if (link.classList.contains('no-pjax')) return false;
        return true;
    },

    setupScrollJacking() {
        const track = document.getElementById('page-content');
        if (!track) return;
        
        // Remove existing listener if any to prevent duplicates on PJAX load
        if (this._wheelHandler) {
            window.removeEventListener('wheel', this._wheelHandler, { passive: false });
        }
        if (this._zoomRaf) {
            cancelAnimationFrame(this._zoomRaf);
        }
        
        let isAnimating = false;
        let wheelTimeout = null;

        // Lerp Scroll Variables
        let targetScroll = track.scrollLeft;
        let isLerping = false;

        // Lerp Zoom Variables
        let targetZoom = 0;
        let currentZoom = 0;
        let isZoomLerping = false;
        let hasTriggeredPjax = false;

        const renderZoom = () => {
            const zoomImage = document.querySelector('.zoom-transition-slide .zoom-image-container');
            const zoomText = document.querySelector('.zoom-transition-slide .ts-slide-content');
            if (!zoomImage) return;

            currentZoom = currentZoom + (targetZoom - currentZoom) * 0.08;
            
            if (Math.abs(targetZoom - currentZoom) < 0.005) {
                currentZoom = targetZoom;
                isZoomLerping = false;
            } else {
                this._zoomRaf = requestAnimationFrame(renderZoom);
            }

            // --- TANDJUNG SARI STYLE FLIP TRANSITION ---
            // Create a fixed overlay to detach the animation from the DOM layout
            let overlay = document.getElementById('ts-zoom-overlay');
            let clone = document.getElementById('ts-zoom-clone');

            if (currentZoom === 0) {
                if (overlay) overlay.remove();
                if (zoomImage) zoomImage.style.opacity = '1';
            } else {
                if (!overlay) {
                    overlay = document.createElement('div');
                    overlay.id = 'ts-zoom-overlay';
                    overlay.style.position = 'fixed';
                    overlay.style.top = '0';
                    overlay.style.left = 'var(--ts-sidebar-width, 40px)'; // Keep sidebar visible
                    overlay.style.width = 'calc(100vw - var(--ts-sidebar-width, 40px))';
                    overlay.style.height = '100vh';
                    overlay.style.zIndex = '997'; // Under top actions (998) and sidebar (1000)
                    overlay.style.display = 'flex';
                    overlay.style.alignItems = 'center';
                    overlay.style.justifyContent = 'center';
                    overlay.style.pointerEvents = 'none';
                    
                    clone = document.createElement('div');
                    clone.id = 'ts-zoom-clone';
                    clone.style.width = '30vh';
                    clone.style.height = '45vh';
                    clone.style.position = 'relative';
                    clone.style.overflow = 'hidden';
                    clone.style.boxShadow = '0 10px 30px rgba(0,0,0,0.1)';
                    
                    const innerClone = document.createElement('div');
                    innerClone.style.width = '100%';
                    innerClone.style.height = '100%';
                    innerClone.style.backgroundSize = 'cover';
                    innerClone.style.backgroundPosition = 'center';
                    
                    const srcImg = zoomImage.querySelector('.zoom-image');
                    if (srcImg) innerClone.style.backgroundImage = srcImg.style.backgroundImage;
                    innerClone.style.filter = 'brightness(0.7) grayscale(1)';
                    
                    clone.appendChild(innerClone);
                    overlay.appendChild(clone);
                    
                    // Clone the text into the overlay so it sits on top of the FLIP image
                    const srcText = document.querySelector('.zoom-transition-slide .ts-slide-content');
                    if (srcText) {
                        const textClone = srcText.cloneNode(true);
                        textClone.id = 'ts-zoom-text-clone';
                        // Override ALL inherited CSS that could cause misalignment
                        textClone.setAttribute('style', [
                            'display: flex',
                            'align-items: center',
                            'justify-content: center',
                            'position: absolute',
                            'top: 0', 'left: 0', 'right: 0', 'bottom: 0',
                            'width: 100%',
                            'height: 100%',
                            'max-width: none',        // override max-width: 800px
                            'text-align: center',
                            'z-index: 2',
                            'opacity: 0',
                            'pointer-events: none',
                            'animation: none',        // override slideUpFade animation
                            'transform: none',        // override translateY(30px)
                            'padding: 0',
                            'box-sizing: border-box'
                        ].join(';'));
                        clone.appendChild(textClone);
                    }
                    
                    document.body.appendChild(overlay);
                    
                    if (zoomImage) zoomImage.style.opacity = '0'; // Hide original
                }
                
                const progress = Math.min(1, currentZoom / 4.5);
                const startWidth = window.innerHeight * 0.30;
                const startHeight = window.innerHeight * 0.45;
                
                if (progress >= 1) {
                    // Full zoom: clone fills the overlay absolutely
                    clone.style.position = 'absolute';
                    clone.style.top = '0';
                    clone.style.left = '0';
                    clone.style.width = '100%';
                    clone.style.height = '100%';
                } else {
                    clone.style.position = 'relative';
                    clone.style.top = 'auto';
                    clone.style.left = 'auto';
                    clone.style.inset = 'auto';
                    
                    const sidebarWidthPx = window.innerWidth <= 768 
                        ? 0 
                        : parseFloat(getComputedStyle(document.documentElement).getPropertyValue('--ts-sidebar-width')) || 40;
                    const targetWidth = window.innerWidth - sidebarWidthPx;
                    
                    const targetHeight = window.innerHeight;
                    const cw = startWidth + (targetWidth - startWidth) * progress;
                    const ch = startHeight + (targetHeight - startHeight) * progress;
                    clone.style.width = `${cw}px`;
                    clone.style.height = `${ch}px`;
                }
            }

            // Show signal text as we zoom in (using the clone in the overlay)
            const zoomTextClone = document.getElementById('ts-zoom-text-clone');
            if (zoomTextClone) {
                const textOpacity = Math.max(0, Math.min(1, (currentZoom - 1) / 2));
                zoomTextClone.style.opacity = textOpacity;
                const scrollInd = document.querySelector('.zoom-transition-slide .ts-scroll-indicator');
                if (scrollInd) scrollInd.style.opacity = textOpacity;
            }

            // Fade out the outer instructions
            const outerTexts = document.querySelectorAll('.zoom-transition-slide .zoom-outer-text');
            outerTexts.forEach(el => {
                el.style.opacity = Math.max(0, 1 - currentZoom * 1.5);
            });

            // Trigger PJAX when zoomed in fully (max is 5) with a 2s delay
            if (currentZoom >= 4.5 && !hasTriggeredPjax && !this.isFetching) {
                if (!this._zoomPjaxTimeout) {
                    this._zoomPjaxTimeout = setTimeout(() => {
                        const zoomSlide = document.querySelector('.zoom-transition-slide');
                        if (zoomSlide && currentZoom >= 4.5) {
                            const target = zoomSlide.getAttribute('data-target');
                            if (target) {
                                hasTriggeredPjax = true;
                                this.load(target, { mode: 'append', push: true, isZoomTransition: true });
                            }
                        }
                    }, 2000);
                }
            } else if (currentZoom < 4.4) {
                if (this._zoomPjaxTimeout) {
                    clearTimeout(this._zoomPjaxTimeout);
                    this._zoomPjaxTimeout = null;
                }
            }
        };

        const renderScroll = () => {
            if (!track) return;
            const currentScroll = track.scrollLeft;
            const nextScroll = currentScroll + (targetScroll - currentScroll) * 0.08; // 0.08 is smoothness factor
            
            if (Math.abs(targetScroll - nextScroll) < 1) {
                track.scrollLeft = targetScroll;
                isLerping = false;
            } else {
                track.scrollLeft = nextScroll;
                requestAnimationFrame(renderScroll);
            }
        };

        this._wheelHandler = (e) => {
            if (e.target.closest('.owl-carousel')) return;

            const currentTrack = document.getElementById('page-content');
            const isVertical = document.body.classList.contains('layout-vertical') || (currentTrack && currentTrack.getAttribute('data-layout') === 'vertical');
            let isAtEnd = false;

            if (isVertical) {
                // Ensure document is scrolled to bottom
                isAtEnd = Math.ceil(window.scrollY + window.innerHeight) >= document.documentElement.scrollHeight - 10;
                
                // Allow native vertical scroll unless at bottom and scrolling down
                // Or if we are currently zooming in and trying to scroll up (to unzoom)
                if (!isAtEnd || (e.deltaY < 0 && targetZoom <= 0)) {
                    return; 
                }
            } else {
                isAtEnd = Math.ceil(track.scrollLeft + track.clientWidth) >= track.scrollWidth - 10;
            }

            const scrollDelta = Math.abs(e.deltaX) > Math.abs(e.deltaY) ? e.deltaX : e.deltaY;

            // ZOOM TRANSITION LOGIC
            if (isAtEnd) {
                const zoomImage = document.querySelector('.zoom-transition-slide .zoom-image-container');
                if (zoomImage) {
                    // Prevent default so we don't rubber-band on Mac
                    e.preventDefault();

                    const zoomDelta = isVertical ? e.deltaY : scrollDelta;

                    if (zoomDelta > 0 || (zoomDelta < 0 && targetZoom > 0)) {
                        targetZoom += zoomDelta * 0.003;
                        if (targetZoom < 0) targetZoom = 0;
                        if (targetZoom > 5) targetZoom = 5; // Max scale reaches 6

                        if (!isZoomLerping) {
                            isZoomLerping = true;
                            this._zoomRaf = requestAnimationFrame(renderZoom);
                        }
                        return; // Prevent normal scroll
                    }
                }
            }

            if (isVertical) return;

            // Trackpad native horizontal bypass
            if (Math.abs(e.deltaX) > Math.abs(e.deltaY)) {
                return;
            }

            // Continuous Fluid Scroll (Translate vertical wheel to horizontal)
            e.preventDefault();
            
            if (!isLerping) {
                targetScroll = track.scrollLeft;
            }
            
            if (isVertical) {
                targetScroll += e.deltaY;
            } else {
                targetScroll += e.deltaY * 1.5; 
                const maxScroll = track.scrollWidth - track.clientWidth;
                targetScroll = Math.max(0, Math.min(targetScroll, maxScroll));
                
                // If zooming has started, force perfect alignment to avoid gaps!
                if (currentZoom > 1) {
                    targetScroll = track.scrollWidth;
                    track.scrollLeft = track.scrollWidth;
                }
            } 
            if (!isLerping) {
                isLerping = true;
                requestAnimationFrame(renderScroll);
            }
            
        };

        window.addEventListener('wheel', this._wheelHandler, { passive: false });

        // Lắng nghe scroll trên track để lấy currentIndex và PJAX trigger
        track.addEventListener('scroll', () => {
            if (!isLerping) {
                targetScroll = track.scrollLeft;
            }
            this.currentIndex = Math.round(track.scrollLeft / track.clientWidth);

            // Cập nhật Scroll Progress Bar
            const progressBar = document.getElementById('global-progress-bar');
            if (progressBar) {
                const maxScroll = track.scrollWidth - track.clientWidth;
                const scrollProgress = maxScroll > 0 ? (track.scrollLeft / maxScroll) * 100 : 0;
                progressBar.style.width = `${scrollProgress}%`;
            }

            // Debounce pushState
            clearTimeout(this.scrollStateTimeout);
            this.scrollStateTimeout = setTimeout(() => {
                const currentUrl = location.href; // Trong thực tế, có thể tính currentUrl dựa trên data-page của slide
                history.replaceState({ url: currentUrl, slideIndex: this.currentIndex }, document.title, currentUrl);
            }, 500);

            // PJAX Trigger (Nối trang)
            const threshold = 100;
            if (track.scrollLeft + track.clientWidth >= track.scrollWidth - threshold) {
                this.triggerNextPage();
            }
        }, { passive: true });

        // Debounce Resize
        window.addEventListener('resize', () => {
            clearTimeout(this.resizeTimeout);
            this.resizeTimeout = setTimeout(() => {
                track.scrollLeft = this.currentIndex * window.innerWidth;
            }, 150);
        });
    },

    updateScroll() {
        const track = document.getElementById('page-content');
        if (track) {
            track.scrollLeft += this.accumulatedDelta;
        }
        this.accumulatedDelta = 0;
        this.ticking = false;
    },

    setupKeyboardNavigation() {
        if (this._keydownHandler) {
            document.removeEventListener('keydown', this._keydownHandler);
        }

        this._keydownHandler = (e) => {
            // Không can thiệp nếu user đang gõ form
            if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA' || e.target.tagName === 'SELECT') return;

            const track = document.getElementById('page-content');
            if (!track) return;
            // Bỏ qua nếu là giao diện dọc
            if (window.matchMedia('(max-width: 768px)').matches) return;

            if (e.key === 'ArrowRight' || e.key === 'PageDown') {
                e.preventDefault();
                track.scrollBy({ left: window.innerWidth, behavior: 'smooth' });
            } else if (e.key === 'ArrowLeft' || e.key === 'PageUp') {
                e.preventDefault();
                track.scrollBy({ left: -window.innerWidth, behavior: 'smooth' });
            }
        };
        
        document.addEventListener('keydown', this._keydownHandler);
    },

    setupFallbackObserver() {
        const trigger = document.querySelector('.next-page-trigger');
        if (!trigger) return;

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    this.triggerNextPage();
                    observer.unobserve(trigger);
                }
            });
        }, { rootMargin: '0px 0px 300px 0px' });
        observer.observe(trigger);
    },

    triggerNextPage() {
        if (this.isFetching) return;
        const trigger = document.querySelector('.next-page-trigger');
        if (!trigger) return;

        const nextUrl = trigger.getAttribute('data-next');
        if (nextUrl) {
            this.load(nextUrl, { mode: 'append', push: true });
        }
    },

    ensureLoadingEdge(track) {
        let edge = document.getElementById('pjax-loading-edge');
        if (!edge) {
            edge = document.createElement('div');
            edge.id = 'pjax-loading-edge';
            edge.innerHTML = '<div style="color: #c8a97e; font-size: 12px; letter-spacing: 2px; text-transform: uppercase;">Loading...</div>';
            track.appendChild(edge);
        } else {
            track.appendChild(edge); // Move to the end
        }
        // Force reflow
        void edge.offsetWidth;
        edge.classList.add('show');
    },

    removeLoadingEdge() {
        const edge = document.getElementById('pjax-loading-edge');
        if (edge) {
            edge.classList.remove('show');
            setTimeout(() => edge.remove(), 300);
        }
    },

    async load(url, options = { mode: 'replace', push: true }) {
        if (this.isFetching) return;
        this.isFetching = true;

        const track = document.getElementById('page-content');

        try {
            if (options.mode === 'replace') {
                document.body.classList.add('pjax-loading');
                document.documentElement.classList.add('pjax-loading');
            } else if (options.mode === 'append' && track && !window.matchMedia('(max-width: 768px)').matches) {
                this.ensureLoadingEdge(track);
            }

            let html = this.cache[url];
            if (!html) {
                const res = await fetch(url);
                if (!res.ok) throw new Error('Network error');
                html = await res.text();
                this.cache[url] = html;
            }

            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');
            const newContent = doc.querySelector('#page-content');
            const title = doc.querySelector('title') ? doc.querySelector('title').innerText : '';

            if (!newContent) {
                window.location.href = url;
                return;
            }

            // Cleanup any global smooth scroll instances to prevent scroll locking on new page
            if (options.mode === 'replace' && window.__lenisInstance) {
                window.__lenisInstance.destroy();
                window.__lenisInstance = null;
            }

            // 1. Identify new styles and links
            const newStyles = Array.from(doc.querySelectorAll('head style'));
            const newLinks = Array.from(doc.querySelectorAll('head link[rel="stylesheet"]'));

            // 2. Add new styles that don't exist yet
            newStyles.forEach(style => {
                if (!Array.from(document.querySelectorAll('head style')).some(s => s.textContent === style.textContent)) {
                    const clonedStyle = style.cloneNode(true);
                    clonedStyle.setAttribute('data-pjax-style', 'true');
                    document.head.appendChild(clonedStyle);
                }
            });

            // 3. Add new links that don't exist yet
            newLinks.forEach(link => {
                const href = link.getAttribute('href');
                if (href && !document.querySelector(`head link[href="${href}"]`)) {
                    const clonedLink = link.cloneNode(true);
                    clonedLink.setAttribute('data-pjax-link', 'true');
                    document.head.appendChild(clonedLink);
                }
            });

            // 4. Remove old PJAX styles/links that are NOT in the new document
            if (options.mode === 'replace') {
                document.querySelectorAll('style[data-pjax-style="true"]').forEach(oldStyle => {
                    if (!newStyles.some(newStyle => newStyle.textContent === oldStyle.textContent)) {
                        oldStyle.remove();
                    }
                });
                document.querySelectorAll('link[data-pjax-link="true"]').forEach(oldLink => {
                    const oldHref = oldLink.getAttribute('href');
                    if (!newLinks.some(newLink => newLink.getAttribute('href') === oldHref)) {
                        oldLink.remove();
                    }
                });
            }

            // Đảm bảo không trùng ID bằng cách thêm suffix ngẫu nhiên hoặc remove (tuỳ tính chất)
            newContent.querySelectorAll('[id]').forEach(el => {
                if (el.id === 'page-content') return;
                // Nếu ID đã có trong DOM (ví dụ modals), xóa element đó ở newContent để tránh duplicate
                if (document.getElementById(el.id) && options.mode === 'append') {
                    if (el.classList.contains('modal')) {
                        el.remove();
                    } else {
                        el.id = el.id + '_' + Date.now();
                    }
                }
            });

            const currentContainer = document.querySelector('#page-content');
            const layout = newContent.getAttribute('data-layout') || 'horizontal';

            if (layout === 'vertical') {
                document.body.classList.add('layout-vertical');
                document.documentElement.classList.add('layout-vertical');
            } else {
                document.body.classList.remove('layout-vertical');
                document.documentElement.classList.remove('layout-vertical');
            }

            if (options.mode === 'replace') {
                // Destroy old Lenis to prevent it from resetting scroll position
                if (window.__lenisInstance && typeof window.__lenisInstance.destroy === 'function') {
                    window.__lenisInstance.destroy();
                    window.__lenisInstance = null;
                }

                const scripts = Array.from(newContent.querySelectorAll('script'));
                currentContainer.parentNode.replaceChild(newContent, currentContainer);

                // Update body layout class based on new content
                const newLayout = newContent.getAttribute('data-layout');
                if (newLayout === 'vertical') {
                    document.body.classList.add('layout-vertical');
                    document.body.classList.remove('layout-horizontal');
                } else if (newLayout === 'horizontal') {
                    document.body.classList.add('layout-horizontal');
                    document.body.classList.remove('layout-vertical');
                } else {
                    document.body.classList.remove('layout-vertical', 'layout-horizontal');
                }
                window.scrollTo(0, 0);
                document.documentElement.scrollTop = 0;
                document.body.scrollTop = 0;
                if (currentContainer) currentContainer.scrollTop = 0;
                
                if (track && layout !== 'vertical') track.scrollLeft = 0;

                if (title) document.title = title;
                if (options.push) {
                    history.pushState({ url, slideIndex: 0 }, title, url);
                }
                
                if ('scrollRestoration' in history) {
                    history.scrollRestoration = 'manual';
                }

                this.executeCollectedScripts(scripts);
                this.setupScrollJacking(); // Re-bind scroll events to new container
            } else if (options.mode === 'append') {
                const oldTrigger = document.querySelector('.next-page-trigger');
                if (oldTrigger) oldTrigger.remove();
                const zoomTrigger = document.querySelector('.zoom-transition-slide');

                if (layout === 'vertical') {
                    // Remove old slides so vertical flow is clean, but keep zoom trigger temporarily
                    Array.from(currentContainer.children).forEach(child => {
                        if (child !== zoomTrigger) child.remove();
                    });
                }

                this.removeLoadingEdge();

                const scripts = Array.from(newContent.querySelectorAll('script'));

                Array.from(newContent.children).forEach(child => {
                    currentContainer.appendChild(child);
                });

                // Remove the zoom trigger slide instantly since we've transitioned
                const oldZoom = document.querySelector('.zoom-transition-slide');
                if (oldZoom) {
                    oldZoom.remove();
                }
                
                window.scrollTo(0, 0);
                
                // SEAMLESS TRANSITION: Fade the overlay out after new page is ready
                // This creates the illusion that text stayed in place while bg changed
                const overlay = document.getElementById('ts-zoom-overlay');
                if (overlay) {
                    // Small delay to allow new page to paint, then fade
                    setTimeout(() => {
                        overlay.style.transition = 'opacity 0.6s ease';
                        overlay.style.opacity = '0';
                        setTimeout(() => overlay.remove(), 650);
                    }, 80);
                }

                if (title) document.title = title;
                if (options.push) {
                    history.pushState({ url, slideIndex: this.currentIndex }, title, url);
                }

                this.executeCollectedScripts(scripts);
                this.setupScrollJacking(); // Reset zoom state and re-bind listeners
            }

            syncSidebarWidthVar();
            this.setupLenis();
            this.reinitPlugins();

        } catch (e) {
            console.error('PJAX Error', e);
            this.removeLoadingEdge();
            if (options.mode === 'replace') window.location.href = url;
        } finally {
            this.isFetching = false;
            document.body.classList.remove('pjax-loading');
            document.documentElement.classList.remove('pjax-loading');
        }
    },

    setupHeaderScroll() {
        const header = document.getElementById('site-header');
        if (!header) return;
        // Scroll header logic may change slightly for horizontal scroll, but we'll bind to both window and track
        const onScroll = () => {
            const track = document.getElementById('page-content');
            const scrollVal = (track && track.scrollLeft > 50) || window.scrollY > 50;
            if (scrollVal) {
                header.classList.add('scrolled');
            } else {
                header.classList.remove('scrolled');
            }
        };
        window.addEventListener('scroll', onScroll, { passive: true });
        const track = document.getElementById('page-content');
        if (track) track.addEventListener('scroll', onScroll, { passive: true });
    },

    setupCookieBanner() {
        if (localStorage.getItem('cookiesAccepted')) return;
        const banner = document.createElement('div');
        banner.id = 'cookie-banner';
        banner.innerHTML = `
            <p>Chúng tôi sử dụng cookie để mang lại trải nghiệm tốt nhất.</p>
            <button id="accept-cookies">ACCEPT ALL</button>
        `;
        document.body.appendChild(banner);

        setTimeout(() => banner.classList.add('show'), 1000);

        document.getElementById('accept-cookies').addEventListener('click', () => {
            localStorage.setItem('cookiesAccepted', 'true');
            banner.classList.remove('show');
            setTimeout(() => banner.remove(), 500);
        });
    },

    setupTandjungAnimations() {
        const track = document.getElementById('page-content');
        if (!track) return;
        
        const isVertical = document.body.classList.contains('layout-vertical');
        
        // 1. Reveal Text via IntersectionObserver (root: null for viewport)
        const revealElements = document.querySelectorAll('.ts-reveal-text');
        if (revealElements.length > 0) {
            const revealOptions = {
                root: null, // use viewport
                threshold: 0.1,
                rootMargin: '0px 100px -50px 100px' // tolerate horizontal edges
            };
            const revealObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('is-visible');
                        observer.unobserve(entry.target); // Reveal only once
                    }
                });
            }, revealOptions);
            
            revealElements.forEach(el => revealObserver.observe(el));
        }

        // 2. Continuous Parallax for Images
        const parallaxImages = document.querySelectorAll('.ts-parallax-img');
        if (parallaxImages.length > 0) {
            let rafId = null;
            const updateParallax = () => {
                const containerWidth = window.innerWidth;
                const containerHeight = window.innerHeight;
                
                parallaxImages.forEach(img => {
                    const wrapper = img.parentElement;
                    const rect = wrapper.getBoundingClientRect();
                    
                    if (isVertical) {
                        // Check if in vertical viewport
                        if (rect.top < containerHeight && rect.bottom > 0) {
                            const yCenter = rect.top + rect.height / 2;
                            const yOffset = (yCenter - containerHeight / 2) * 0.15; // Parallax speed
                            img.style.transform = `translate3d(0, ${yOffset}px, 0)`;
                        }
                    } else {
                        // Check if in horizontal viewport
                        if (rect.left < containerWidth && rect.right > 0) {
                            const xCenter = rect.left + rect.width / 2;
                            const xOffset = (xCenter - containerWidth / 2) * 0.15; // Parallax speed
                            img.style.transform = `translate3d(${xOffset}px, 0, 0)`;
                        }
                    }
                });
                rafId = requestAnimationFrame(updateParallax);
            };
            
            // Start parallax loop
            rafId = requestAnimationFrame(updateParallax);
            
            // Clean up on page unload/pjax
            const cleanup = () => {
                cancelAnimationFrame(rafId);
                document.removeEventListener('pjax:start', cleanup);
            };
            document.addEventListener('pjax:start', cleanup);
        }
    },

    executeCollectedScripts(scripts) {
        scripts.forEach(s => {
            if (s.hasAttribute('data-executed')) return;
            const newScript = document.createElement('script');
            Array.from(s.attributes).forEach(attr => newScript.setAttribute(attr.name, attr.value));
            newScript.textContent = s.textContent;
            newScript.setAttribute('data-executed', 'true');
            s.replaceWith(newScript);
        });
    },

    markInitialized(container) {
        container.querySelectorAll('.owl-carousel:not([data-initialized="true"])').forEach(el => el.setAttribute('data-initialized', 'true'));
        container.querySelectorAll('.ts-animate-on-scroll:not([data-initialized="true"])').forEach(el => el.setAttribute('data-initialized', 'true'));
    },

    reinitPlugins() {
        if (typeof setupScrollAnimations === 'function') {
            setupScrollAnimations();
        }

        // Fire rooms effects if on rooms page
        if (typeof window.__initRoomsEffects === 'function') {
            window.__initRoomsEffects();
        }

        // Notify page scripts
        document.dispatchEvent(new CustomEvent('pjax:complete'));

        if (typeof jQuery !== 'undefined' && jQuery.fn.owlCarousel) {
            jQuery('.owl-carousel:not([data-initialized="true"])').each(function () {
                jQuery(this).owlCarousel({
                    loop: true,
                    margin: 30,
                    nav: false,
                    dots: true,
                    autoplay: true,
                    responsive: {
                        0: { items: 1 },
                        600: { items: 2 },
                        1000: { items: 3 }
                    }
                });
                jQuery(this).attr('data-initialized', 'true');
            });
        }

        if (typeof jQuery !== 'undefined' && jQuery.fn.Scrollax) {
            jQuery.Scrollax();
        }

        if (document.getElementById('ts-rooms-container') && typeof loadRooms === 'function') {
            if (!document.getElementById('ts-rooms-container').hasAttribute('data-loaded')) {
                loadRooms();
                document.getElementById('ts-rooms-container').setAttribute('data-loaded', 'true');
            }
        }
        if (document.getElementById('restaurantMenuList') && typeof loadRestaurantMenu === 'function') {
            if (!document.getElementById('restaurantMenuList').hasAttribute('data-loaded')) {
                loadRestaurantMenu();
                document.getElementById('restaurantMenuList').setAttribute('data-loaded', 'true');
            }
        }

        this.markInitialized(document);
    },

    setupLenis() {
        if (!document.body.classList.contains('layout-vertical')) {
            if (window.__lenisInstance) {
                window.__lenisInstance.destroy();
                window.__lenisInstance = null;
            }
            return;
        }

        const initLenis = () => {
            if (window.__lenisInstance) {
                window.__lenisInstance.destroy();
            }
            const lenis = new Lenis({
                duration: 1.2,
                easing: (t) => Math.min(1, 1.001 - Math.pow(2, -10 * t)),
                direction: 'vertical',
                gestureDirection: 'vertical',
                smooth: true,
                mouseMultiplier: 1,
                smoothTouch: false,
                touchMultiplier: 2,
                infinite: false,
            });
            window.__lenisInstance = lenis;

            let isLenisStopped = false;

            function raf(time) {
                if (window.__lenisInstance === lenis) {
                    const zoomOverlay = document.getElementById('ts-zoom-overlay');
                    if (zoomOverlay && !isLenisStopped) {
                        lenis.stop();
                        isLenisStopped = true;
                    } else if (!zoomOverlay && isLenisStopped) {
                        lenis.start();
                        isLenisStopped = false;
                    }
                    lenis.raf(time);
                    requestAnimationFrame(raf);
                }
            }
            requestAnimationFrame(raf);
        };

        if (typeof Lenis === 'undefined') {
            const script = document.createElement('script');
            script.src = 'https://cdn.jsdelivr.net/gh/studio-freight/lenis@1.0.29/bundled/lenis.min.js';
            script.onload = initLenis;
            document.head.appendChild(script);
        } else {
            initLenis();
        }
    }
};

document.addEventListener('DOMContentLoaded', () => {
    PageLoader.init();
});

window.PageLoader = PageLoader;

function toggleBookingPanel() {
    const panel = document.getElementById('bookingPanel');
    if (panel) {
        panel.classList.toggle('open');
    }
}
