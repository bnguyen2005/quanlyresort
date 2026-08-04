/**
 * Notification Service for Admin - Handles browser push notifications and in-app notifications
 */
class AdminNotificationService {
    constructor() {
        this.apiBase = '/api/notifications';
        this.token = localStorage.getItem('token');
        this.unreadCount = 0;
        this.pollInterval = null;
        this.notificationPermission = null;
        this.init();
    }

    async init() {
        // Request notification permission
        if ('Notification' in window) {
            this.notificationPermission = Notification.permission;
        }

        // Start polling for notifications if user is logged in
        if (this.token) {
            await this.loadUnreadCount();
            this.startPolling();
        }

        // Listen for visibility changes to refresh notifications
        document.addEventListener('visibilitychange', () => {
            if (!document.hidden && this.token) {
                this.loadUnreadCount();
            }
        });
    }

    async requestPermission() {
        if (!('Notification' in window)) {
            console.warn('[AdminNotificationService] Browser does not support notifications');
            return false;
        }

        if (Notification.permission === 'granted') {
            return true;
        }

        if (Notification.permission === 'denied') {
            console.warn('[AdminNotificationService] Notification permission denied');
            return false;
        }

        const permission = await Notification.requestPermission();
        this.notificationPermission = permission;
        return permission === 'granted';
    }

    async loadUnreadCount() {
        if (!this.token) return;

        try {
            const response = await fetch(`${this.apiBase}/unread-count`, {
                headers: {
                    'Authorization': `Bearer ${this.token}`
                }
            });

            if (response.ok) {
                const data = await response.json();
                this.unreadCount = data.count || 0;
                this.updateBadge();
            }
        } catch (error) {
            console.error('[AdminNotificationService] Error loading unread count:', error);
        }
    }

    async getNotifications(unreadOnly = false) {
        if (!this.token) return [];

        try {
            const response = await fetch(`${this.apiBase}?unreadOnly=${unreadOnly}`, {
                headers: {
                    'Authorization': `Bearer ${this.token}`
                }
            });

            if (response.ok) {
                return await response.json();
            }
        } catch (error) {
            console.error('[AdminNotificationService] Error getting notifications:', error);
        }

        return [];
    }

    async markAsRead(notificationId) {
        if (!this.token) return false;

        try {
            const response = await fetch(`${this.apiBase}/${notificationId}/read`, {
                method: 'PATCH',
                headers: {
                    'Authorization': `Bearer ${this.token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                this.unreadCount = Math.max(0, this.unreadCount - 1);
                this.updateBadge();
                return true;
            }
        } catch (error) {
            console.error('[AdminNotificationService] Error marking as read:', error);
        }

        return false;
    }

    async markAllAsRead() {
        if (!this.token) return false;

        try {
            const response = await fetch(`${this.apiBase}/read-all`, {
                method: 'PATCH',
                headers: {
                    'Authorization': `Bearer ${this.token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                this.unreadCount = 0;
                this.updateBadge();
                return true;
            }
        } catch (error) {
            console.error('[AdminNotificationService] Error marking all as read:', error);
        }

        return false;
    }

    showBrowserNotification(title, options = {}) {
        if (!('Notification' in window) || Notification.permission !== 'granted') {
            return;
        }

        const notification = new Notification(title, {
            icon: options.icon || '/admin/assets/images/logo.png',
            badge: '/admin/assets/images/logo.png',
            body: options.body || '',
            tag: options.tag || 'default',
            requireInteraction: options.requireInteraction || false,
            data: options.data || {}
        });

        notification.onclick = () => {
            window.focus();
            if (options.onClick) {
                options.onClick();
            }
            notification.close();
        };

        // Auto close after 5 seconds
        setTimeout(() => {
            notification.close();
        }, options.duration || 5000);

        return notification;
    }

    updateBadge() {
        // Update badge in UI
        const badgeElements = document.querySelectorAll('.notification-badge, .notif-count');
        badgeElements.forEach(badge => {
            if (this.unreadCount > 0) {
                badge.textContent = this.unreadCount > 99 ? '99+' : this.unreadCount;
                badge.style.display = 'inline-block';
            } else {
                badge.style.display = 'none';
            }
        });

        // Update page title
        if (this.unreadCount > 0) {
            const originalTitle = document.title.replace(/^\(\d+\)\s*/, '');
            document.title = `(${this.unreadCount}) ${originalTitle}`;
        } else {
            document.title = document.title.replace(/^\(\d+\)\s*/, '');
        }
    }

    startPolling(interval = 30000) {
        // Poll every 30 seconds for new notifications
        if (this.pollInterval) {
            clearInterval(this.pollInterval);
        }

        this.pollInterval = setInterval(() => {
            if (this.token && !document.hidden) {
                this.loadUnreadCount();
            }
        }, interval);
    }

    stopPolling() {
        if (this.pollInterval) {
            clearInterval(this.pollInterval);
            this.pollInterval = null;
        }
    }

    setToken(token) {
        this.token = token;
        if (token) {
            this.loadUnreadCount();
            this.startPolling();
        } else {
            this.stopPolling();
            this.unreadCount = 0;
            this.updateBadge();
        }
    }
}

// Create global instance
window.adminNotificationService = new AdminNotificationService();

