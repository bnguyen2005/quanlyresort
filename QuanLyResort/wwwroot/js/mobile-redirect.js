// Mobile Detection and Redirect Logic

const isMobileDevice = () => {
    // Check user agent
    const userAgent = navigator.userAgent || navigator.vendor || window.opera;
    const mobileRegex = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i;
    
    // Check viewport width
    const isSmallScreen = window.innerWidth < 768;
    
    return mobileRegex.test(userAgent) || isSmallScreen;
};

const getMobilePagePath = (currentPath) => {
    // Convert desktop path to mobile path
    // Example: /customer/index.html -> /customer/mobile/index.html
    // Example: /admin/dashboard.html -> /admin/mobile/dashboard.html
    
    const pathParts = currentPath.split('/');
    const lastPart = pathParts[pathParts.length - 1];
    
    if (currentPath.includes('/customer/')) {
        return currentPath.replace('/customer/', '/customer/mobile/');
    } else if (currentPath.includes('/admin/')) {
        return currentPath.replace('/admin/', '/admin/mobile/');
    }
    
    return currentPath;
};

const redirectToMobileIfNeeded = () => {
    const currentPath = window.location.pathname;
    
    // Skip if already on mobile path
    if (currentPath.includes('/mobile/')) {
        return;
    }
    
    // Skip if explicitly opted out (cookie/localStorage)
    if (localStorage.getItem('force_desktop_view') === 'true') {
        return;
    }
    
    if (isMobileDevice()) {
        const mobilePath = getMobilePagePath(currentPath);
        
        // Check if mobile version exists (basic check)
        if (mobilePath !== currentPath) {
            // Redirect to mobile version
            window.location.href = mobilePath;
        }
    }
};

const toggleDesktopView = (forceDesktop) => {
    if (forceDesktop) {
        localStorage.setItem('force_desktop_view', 'true');
    } else {
        localStorage.removeItem('force_desktop_view');
    }
    window.location.reload();
};

// Auto-run on page load
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', redirectToMobileIfNeeded);
} else {
    redirectToMobileIfNeeded();
}

