/**
 * Notification UI Handler
 * Manages the notification dropdown UI and integrates with NotificationService
 */

(function() {
    'use strict';

    let notificationService = null;
    let notificationList = null;
    let markAllReadBtn = null;
    let notificationDropdownBtn = null;

    // Initialize notification UI
    function initNotificationUI() {
        // Wait for notification service to be available
        if (typeof window.notificationService === 'undefined') {
            setTimeout(initNotificationUI, 100);
            return;
        }

        notificationService = window.notificationService;

        // Find notification dropdown elements
        notificationList = document.getElementById('notificationList');
        markAllReadBtn = document.getElementById('markAllReadBtn');
        notificationDropdownBtn = document.getElementById('notificationDropdownBtn');

        if (!notificationList) {
            console.warn('[NotificationUI] Notification list element not found');
            return;
        }

        // Load notifications when dropdown is opened
        if (notificationDropdownBtn) {
            $(notificationDropdownBtn).on('show.bs.dropdown', loadNotifications);
        }

        // Mark all as read button
        if (markAllReadBtn) {
            markAllReadBtn.addEventListener('click', async (e) => {
                e.preventDefault();
                e.stopPropagation();
                await notificationService.markAllAsRead();
                await loadNotifications();
            });
        }

        // Load notifications initially
        loadNotifications();

        // Poll for new notifications every 30 seconds
        setInterval(loadNotifications, 30000);
    }

    // Load and display notifications
    async function loadNotifications() {
        if (!notificationService || !notificationList) return;

        try {
            const notifications = await notificationService.getNotifications(false);
            renderNotifications(notifications);
        } catch (error) {
            console.error('[NotificationUI] Error loading notifications:', error);
        }
    }

    // Render notifications list
    function renderNotifications(notifications) {
        if (!notificationList) return;

        if (!notifications || notifications.length === 0) {
            notificationList.innerHTML = `
                <div class="text-center p-3 text-muted">
                    <i class="icon-bell" style="font-size: 2rem; opacity: 0.3;"></i>
                    <p class="mb-0 mt-2">Không có thông báo</p>
                </div>
            `;
            return;
        }

        const html = notifications.map(notif => {
            const severityClass = notif.severity || 'Info';
            const isUnread = !notif.isRead;
            const timeAgo = formatTimeAgo(notif.createdAt);

            return `
                <div class="notification-item ${isUnread ? 'unread' : ''}" data-id="${notif.notificationId}">
                    <div class="d-flex align-items-start">
                        <span class="notification-severity ${severityClass}"></span>
                        <div class="flex-grow-1">
                            <div class="notification-title">${escapeHtml(notif.title)}</div>
                            <div class="notification-message">${escapeHtml(notif.message)}</div>
                            <div class="notification-time">${timeAgo}</div>
                        </div>
                    </div>
                </div>
            `;
        }).join('');

        notificationList.innerHTML = html;

        // Add click handlers
        notificationList.querySelectorAll('.notification-item').forEach(item => {
            item.addEventListener('click', async (e) => {
                const notificationId = parseInt(item.getAttribute('data-id'));
                if (notificationId) {
                    await notificationService.markAsRead(notificationId);
                    item.classList.remove('unread');
                    
                    // Navigate to action URL if available
                    const notification = notifications.find(n => n.notificationId === notificationId);
                    if (notification && notification.actionUrl) {
                        window.location.href = notification.actionUrl;
                    }
                }
            });
        });
    }

    // Format time ago
    function formatTimeAgo(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) return 'Vừa xong';
        if (diffMins < 60) return `${diffMins} phút trước`;
        if (diffHours < 24) return `${diffHours} giờ trước`;
        if (diffDays < 7) return `${diffDays} ngày trước`;
        
        return date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
    }

    // Escape HTML
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // Load notification dropdown component
    async function loadNotificationDropdown() {
        const navbarAuthPlaceholder = document.getElementById('navbar-auth-placeholder');
        if (!navbarAuthPlaceholder) {
            console.warn('[NotificationUI] Navbar auth placeholder not found');
            return;
        }

        try {
            const response = await fetch('components/notification-dropdown.html');
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const html = await response.text();
            const tempDiv = document.createElement('div');
            tempDiv.innerHTML = html;

            // Insert notification dropdown before navbar-auth-placeholder
            const notificationWrapper = tempDiv.querySelector('.notification-dropdown-wrapper');
            if (notificationWrapper) {
                navbarAuthPlaceholder.parentNode.insertBefore(notificationWrapper, navbarAuthPlaceholder);
                
                // Initialize UI after component is loaded
                setTimeout(initNotificationUI, 100);
            }
        } catch (error) {
            console.error('[NotificationUI] Error loading notification dropdown:', error);
        }
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            // Wait for navbar-auth to load first
            setTimeout(loadNotificationDropdown, 500);
        });
    } else {
        setTimeout(loadNotificationDropdown, 500);
    }

    // Listen for token changes
    window.addEventListener('storage', (e) => {
        if (e.key === 'token' && notificationService) {
            notificationService.setToken(e.newValue);
        }
    });

})();

