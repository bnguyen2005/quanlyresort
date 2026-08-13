document.addEventListener('DOMContentLoaded', async () => {
    // Chỉ chạy script này trên trang chủ
    const path = window.location.pathname;
    if (path !== '/' && !path.endsWith('index.html')) return;
    
    const container = document.getElementById('snapContainer');
    if (!container) return;

    // Remove the 'page-transition.js' listener from this page so it doesn't navigate away!
    // We will handle everything in one long scroll.
    // To do this safely, we can just let page-transition run, but since PAGE_ORDER 
    // will be meaningless (we are already on index), page-transition will just scroll smoothly.
    
    // Add loading indicator
    const loadingSlide = document.createElement('section');
    loadingSlide.className = 'ts-slide';
    loadingSlide.id = 'merging-loader';
    loadingSlide.innerHTML = `
      <div style="width: 100%; height: 100vh; display: flex; align-items: center; justify-content: center; flex-direction: column; background: var(--bg-primary);">
         <div class="spinner-border text-light" style="width: 3rem; height: 3rem; margin-bottom: 20px;" role="status"></div>
         <h4 class="playfair" style="color: var(--gold);">Đang tải toàn bộ khu nghỉ dưỡng...</h4>
      </div>
    `;
    container.appendChild(loadingSlide);

    const pagesToMerge = [
        { url: 'rooms.html', id: 'section-rooms' },
        { url: 'restaurant.html', id: 'section-restaurant' },
        { url: 'reviews.html', id: 'section-reviews' }
    ];
    
    const allScriptsToRun = [];
    
    for (const page of pagesToMerge) {
        try {
            const res = await fetch(page.url);
            if (!res.ok) continue;
            const html = await res.text();
            
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');
            
            // 1. Extract Slides using querySelectorAll to avoid malformed HTML nesting issues
            const slides = doc.querySelectorAll('.ts-slide');
            if (slides.length > 0) {
                Array.from(slides).forEach((slide, idx) => {
                    const clone = document.importNode(slide, true);
                    if (idx === 0) clone.id = page.id; 
                    if (clone.id === 'ts-rooms-container') clone.style.display = 'contents'; 
                    container.appendChild(clone);
                });
                
                // If cartFloatingBtn got pushed out of the slide due to malformed HTML, grab it explicitly
                const cartBtn = doc.getElementById('cartFloatingBtn');
                if (cartBtn && !container.querySelector('#cartFloatingBtn')) {
                    container.appendChild(document.importNode(cartBtn, true));
                }
            } else {
                // reviews.html fallback
                const slide = document.createElement('section');
                slide.className = 'ts-slide';
                slide.id = page.id;
                slide.style.overflowY = 'auto';
                slide.style.display = 'block';
                slide.style.padding = '80px 0';
                
                // Safely copy body contents
                const cloneBody = document.importNode(doc.body, true);
                const toRemove = cloneBody.querySelectorAll('script, nav, header, #header-placeholder');
                toRemove.forEach(el => el.remove());
                
                while (cloneBody.firstChild) {
                    slide.appendChild(cloneBody.firstChild);
                }
                container.appendChild(slide);
            }
            
            // 2. Extract outside Modals / Drawers
            const modals = doc.querySelectorAll('.ts-drawer-backdrop, .ts-drawer, .modal');
            modals.forEach(m => {
                document.body.appendChild(document.importNode(m, true));
            });
            
            // 3. Queue Scripts
            const scripts = Array.from(doc.querySelectorAll('script'));
            for (const s of scripts) {
                allScriptsToRun.push(s);
            }
            
        } catch (e) {
            console.error('Failed to merge ' + page.url, e);
        }
    }
    
    // Remove loading slide
    loadingSlide.remove();
    
    // Now that ALL DOM is appended, let the browser settle, then append scripts
    setTimeout(() => {
        for (const s of allScriptsToRun) {
            const src = s.getAttribute('src');
            if (src) {
                // Skip core libraries and duplicates
                const skipList = ['jquery', 'bootstrap', 'main.js', 'page-transition.js', 'load-header', 'navbar-auth.js', 'scrollax', 'popper', 'easing', 'waypoints', 'stellar', 'owl.carousel', 'magnific', 'aos', 'animateNumber', 'datepicker'];
                if (!skipList.some(skip => src.includes(skip))) {
                    const newScript = document.createElement('script');
                    newScript.src = src;
                    document.body.appendChild(newScript);
                }
            } else {
                let code = s.textContent;
                // Protect against multiple executions if needed, but it's isolated by page
                code = code.replace(/DOMContentLoaded/g, 'MergeComplete');
                
                const newScript = document.createElement('script');
                newScript.textContent = code;
                document.body.appendChild(newScript);
            }
        }
        
        // Trigger initialization for merged scripts after a short delay
        setTimeout(() => {
            document.dispatchEvent(new Event('MergeComplete'));
        }, 100);
    }, 100);
    
    // Hack page-transition.js so it doesn't navigate away!
    // By overriding the PAGE_ORDER array or intercepting it
    window.DISABLE_PAGE_TRANSITION = true;
    
    // Intercept all links
    setTimeout(() => {
        const links = document.querySelectorAll('a');
        links.forEach(link => {
            const href = link.getAttribute('href');
            if (href && (href.includes('rooms.html') || href.includes('restaurant.html') || href.includes('reviews.html'))) {
                link.addEventListener('click', (e) => {
                    e.preventDefault();
                    let targetId = '';
                    if (href.includes('rooms.html')) targetId = 'section-rooms';
                    if (href.includes('restaurant.html')) targetId = 'section-restaurant';
                    if (href.includes('reviews.html')) targetId = 'section-reviews';
                    
                    const targetEl = document.getElementById(targetId);
                    if (targetEl) {
                        const closeMenu = document.querySelector('.navbar-toggler');
                        if (closeMenu && !closeMenu.classList.contains('collapsed')) {
                            closeMenu.click();
                        }
                        // Cuộn container đến phần tử đó
                        const leftPos = targetEl.offsetLeft;
                        container.scrollTo({ left: leftPos, behavior: 'smooth' });
                    }
                });
            }
        });
    }, 1500); // Wait for scripts to render and header to load
});
