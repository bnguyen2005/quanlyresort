---
name: tandjung-sari-zoom-transition
description: Perfect FLIP-like zoom animation across pages using PJAX. Automatically calculates sidebar carving to prevent pixel-shifting.
---

# Tandjung Sari Zoom Transition (FLIP PJAX)

When the user asks for the "Tandjung Sari Zoom Transition" or "hiệu ứng chuyển mượt mà Tandjung Sari" or similar, implement this exact architecture to ensure a pixel-perfect, seamless page transition.

## 1. Dynamic Sidebar Carving (`syncSidebarWidthVar`)
**CRITICAL:** Never hardcode sidebar widths (e.g., `40px`). Always measure dynamically to avoid pixel-shifting when the clone lands on the new page layout.
```javascript
function syncSidebarWidthVar() {
  const nav = document.querySelector('#ftco-navbar'); // Modify selector as needed
  if (!nav) return;
  const realWidth = nav.getBoundingClientRect().width;
  document.documentElement.style.setProperty('--ts-sidebar-width', `${realWidth}px`);
}
// Execute on: window load, window resize, and immediately after PJAX content replacement.
```

## 2. Animation Logic (e.g. in `page-loader.js`)
Calculate the target dimensions using the dynamic CSS variable:
```javascript
const sidebarWidthPx = window.innerWidth <= 768 
    ? 0 
    : parseFloat(getComputedStyle(document.documentElement).getPropertyValue('--ts-sidebar-width')) || 40;
const targetWidth = window.innerWidth - sidebarWidthPx;
```
Create a fixed overlay that respects the sidebar width:
```javascript
overlay.style.left = 'var(--ts-sidebar-width, 40px)';
overlay.style.width = 'calc(100vw - var(--ts-sidebar-width, 40px))';
```

## 3. The Target Page (e.g. `room-detail.html`)
When PJAX navigation is complete, the incoming HTML must execute JS to gracefully receive the animation.
- The new page creates a clone matching the end-state of the zoom:
```javascript
clone.style.top = '0';
clone.style.left = 'var(--ts-sidebar-width, 40px)';
clone.style.width = 'calc(100% - var(--ts-sidebar-width, 40px))';
clone.style.height = '100%';
```
- **Text Alignment Fix:** Any text placed inside the clone MUST have EXACTLY the same CSS as the target HTML text (especially `margin: 0`, `font-family`, `letter-spacing`, etc.) to prevent baseline shifting. Use `textClone.style.margin = '0';` and ensure the target HTML `.room-title` also has `margin: 0`.

## 4. Global CSS
Apply the layout constraints globally for the main content:
```css
body.layout-vertical #page-content {
  width: calc(100% - var(--ts-sidebar-width, 40px));
  margin-left: var(--ts-sidebar-width, 40px);
}
```
