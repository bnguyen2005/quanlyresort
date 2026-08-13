/* 
    Premium FX Javascript
    Handles Magnetic Cursor, Scroll Skew, and Split-Text Reveal
*/

document.addEventListener('DOMContentLoaded', () => {
    
    // 1. MAGNETIC CURSOR
    const cursor = document.getElementById('ts-cursor');
    if (cursor && !('ontouchstart' in window)) {
        let mouseX = window.innerWidth / 2;
        let mouseY = window.innerHeight / 2;
        let cursorX = mouseX;
        let cursorY = mouseY;
        let isHovering = false;
        let isDragging = false;
        let targetElement = null;

        window.addEventListener('mousemove', (e) => {
            mouseX = e.clientX;
            mouseY = e.clientY;
            
            // Check for interactive elements
            const interactive = document.elementsFromPoint(mouseX, mouseY).find(el => 
                el.tagName === 'A' || el.tagName === 'BUTTON' || el.classList.contains('ts-btn') || el.classList.contains('ts-gallery-item')
            );
            
            if (interactive) {
                if (interactive.classList.contains('ts-gallery-item')) {
                    if (!isDragging) {
                        cursor.classList.add('drag-mode');
                        isDragging = true;
                    }
                } else {
                    if (!isHovering) {
                        cursor.classList.add('hovering');
                        isHovering = true;
                        targetElement = interactive;
                    }
                }
            } else {
                if (isHovering) {
                    cursor.classList.remove('hovering');
                    isHovering = false;
                    targetElement = null;
                }
                if (isDragging) {
                    cursor.classList.remove('drag-mode');
                    isDragging = false;
                }
            }
        });

        // Magnetic pull effect
        const renderCursor = () => {
            if (isHovering && targetElement) {
                // Pull cursor towards center of element
                const rect = targetElement.getBoundingClientRect();
                const centerX = rect.left + rect.width / 2;
                const centerY = rect.top + rect.height / 2;
                
                // Add a small magnetic pull
                const pullX = (mouseX - centerX) * 0.1;
                const pullY = (mouseY - centerY) * 0.1;
                
                cursorX += ((centerX + pullX) - cursorX) * 0.2;
                cursorY += ((centerY + pullY) - cursorY) * 0.2;
            } else {
                // Normal follow
                cursorX += (mouseX - cursorX) * 0.2;
                cursorY += (mouseY - cursorY) * 0.2;
            }
            
            cursor.style.transform = `translate(${cursorX}px, ${cursorY}px) translate(-50%, -50%)`;
            requestAnimationFrame(renderCursor);
        };
        requestAnimationFrame(renderCursor);
    }

    // 2. SKEW ON SCROLL
    const track = document.getElementById('page-content');
    const skewElements = document.querySelectorAll('.ts-skew-elem');
    
    if (track && skewElements.length > 0) {
        let lastScroll = track.scrollLeft;
        let currentSkew = 0;
        
        const renderSkew = () => {
            const currentScroll = track.scrollLeft;
            const delta = currentScroll - lastScroll;
            lastScroll = currentScroll;
            
            // Calculate target skew based on scroll velocity
            const targetSkew = 0; // Skew effect disabled per user request
            
            // Lerp skew back to 0 or towards target
            currentSkew += (targetSkew - currentSkew) * 0.1;
            
            // Apply skew if significant
            if (Math.abs(currentSkew) > 0.01) {
                skewElements.forEach(el => {
                    el.style.transform = `skewX(${currentSkew}deg)`;
                });
            } else if (Math.abs(currentSkew) <= 0.01 && Math.abs(currentSkew) > 0) {
                currentSkew = 0;
                skewElements.forEach(el => {
                    el.style.transform = `skewX(0deg)`;
                });
            }
            
            requestAnimationFrame(renderSkew);
        };
        requestAnimationFrame(renderSkew);
    }

    // 3. SPLIT TEXT REVEAL
    const revealElements = document.querySelectorAll('.ts-split-text-reveal');
    if (revealElements.length > 0) {
        // Prepare DOM
        revealElements.forEach(el => {
            const text = el.innerText;
            el.innerHTML = '';
            
            // Split by words
            const words = text.split(' ');
            words.forEach((word, i) => {
                const lineSpan = document.createElement('span');
                lineSpan.className = 'ts-split-line';
                
                const wordSpan = document.createElement('span');
                wordSpan.className = 'ts-split-word';
                wordSpan.innerText = word + (i < words.length - 1 ? '\u00A0' : '');
                
                // Add staggered delay
                wordSpan.style.transitionDelay = `${i * 0.05}s`;
                
                lineSpan.appendChild(wordSpan);
                el.appendChild(lineSpan);
            });
        });

        // Setup Observer
        const revealOptions = {
            root: null,
            threshold: 0.2,
            rootMargin: '0px 100px -50px 100px'
        };
        
        const observer = new IntersectionObserver((entries, obs) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('ts-split-active');
                    obs.unobserve(entry.target);
                }
            });
        }, revealOptions);
        
        revealElements.forEach(el => observer.observe(el));
    }

    // 4. FADE OUT SLIDE 1 TEXT ON SCROLL
    const slide1Text = document.querySelector('.ts-intro-slide .ts-slide-content');
    if (slide1Text && track) {
        let lastFadeScroll = track.scrollLeft;
        const renderFade = () => {
            const currentScroll = track.scrollLeft;
            if (currentScroll !== lastFadeScroll) {
                lastFadeScroll = currentScroll;
                const windowWidth = window.innerWidth;
                
                // Calculate opacity: starts at 1, goes to 0 when scrolled 60% of window width
                let opacity = 1 - (currentScroll / (windowWidth * 0.6));
                opacity = Math.max(0, Math.min(1, opacity));
                // Use setProperty with important to override CSS animations (slideUpFade forwards)
                slide1Text.style.setProperty('opacity', opacity, 'important');
                
                // Add a very subtle horizontal shift to enhance the "push" effect
                slide1Text.style.setProperty('transform', `translateX(-${currentScroll * 0.4}px)`, 'important');
            }
            requestAnimationFrame(renderFade);
        };
        // Initialize immediately
        renderFade();
    }
});
