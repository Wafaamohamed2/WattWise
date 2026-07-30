const NotificationHelper = (function () {
    let connection = null;
    let isInitialized = false;

    // Toast Container Creator
    function ensureToastContainer() {
        let container = document.getElementById('wattwise-toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'wattwise-toast-container';
            container.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                z-index: 9999;
                display: flex;
                flex-direction: column;
                gap: 10px;
                max-width: 380px;
                width: 100%;
                pointer-events: none;
            `;
            document.body.appendChild(container);
        }
        return container;
    }

    function showToast(title, message, severity = 'info') {
        const container = ensureToastContainer();
        const toast = document.createElement('div');
        toast.style.cssText = `
            pointer-events: auto;
            background: white;
            border-left: 5px solid ${getSeverityColor(severity)};
            border-radius: 8px;
            padding: 14px 18px;
            box-shadow: 0 10px 25px rgba(0, 0, 0, 0.15);
            display: flex;
            align-items: flex-start;
            justify-content: space-between;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            animation: slideIn 0.3s cubic-bezier(0.16, 1, 0.3, 1);
            transition: opacity 0.3s;
        `;

        const icon = getSeverityIcon(severity);

        toast.innerHTML = `
            <div style="display:flex; align-items:flex-start; gap:12px;">
                <span style="font-size:1.4em;">${icon}</span>
                <div>
                    <strong style="display:block; color:#1f2937; font-size:0.95em; margin-bottom:2px;">${escapeHtml(title)}</strong>
                    <span style="color:#6b7280; font-size:0.85em; line-height:1.4;">${escapeHtml(message)}</span>
                </div>
            </div>
            <button onclick="this.parentElement.remove()" style="background:none; border:none; color:#9ca3af; font-size:1.1em; cursor:pointer; padding:0 0 0 8px;">&times;</button>
        `;

        container.appendChild(toast);

        // Auto remove after 6 seconds
        setTimeout(() => {
            toast.style.opacity = '0';
            setTimeout(() => toast.remove(), 300);
        }, 6000);
    }

    function getSeverityColor(severity) {
        switch (String(severity).toLowerCase()) {
            case 'critical': case 'error': case 'high': return '#ef4444';
            case 'warning': case 'medium': return '#f59e0b';
            case 'info': case 'low': return '#3b82f6';
            default: return '#10b981';
        }
    }

    function getSeverityIcon(severity) {
        switch (String(severity).toLowerCase()) {
            case 'critical': case 'error': case 'high': return '🚨';
            case 'warning': case 'medium': return '⚠️';
            case 'info': case 'low': return 'ℹ️';
            default: return '⚡';
        }
    }

    function escapeHtml(str) {
        if (!str) return '';
        return String(str).replace(/[&<>"']/g, function (m) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' }[m];
        });
    }

    async function init() {
        if (isInitialized) return;
        if (typeof signalR === 'undefined') {
            console.warn('SignalR library not loaded.');
            return;
        }

        const token = localStorage.getItem('token') || (typeof AuthHelper !== 'undefined' ? AuthHelper.getToken() : null);
        if (!token) {
            console.log('SignalR: No auth token found. Skipping connection.');
            return;
        }

        const baseUrl = typeof AuthHelper !== 'undefined' ? AuthHelper.API_BASE_URL : 'http://localhost:5167';

        connection = new signalR.HubConnectionBuilder()
            .withUrl(baseUrl + '/hubs/notifications', {
                accessTokenFactory: () => token
            })
            // Client-Side Resilience: Automatic Reconnect Policy
            .withAutomaticReconnect([0, 2000, 10000, 30000])
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Listen for Real-Time Alert Broadcasts
        connection.on('ReceiveAlert', (alert) => {
            console.log('⚡ Real-Time Alert Received:', alert);
            const title = `${alert.alertType ?? 'Alert'}: ${alert.deviceName ?? 'Device'}`;
            showToast(title, alert.message, alert.severity);

            // Dispatch Event for Page-level updates
            document.dispatchEvent(new CustomEvent('wattwise:alert', { detail: alert }));
        });

        // Listen for Unread Count Updates
        connection.on('UpdateUnreadCount', (count) => {
            console.log('🔔 Unread Badge Update:', count);
            const badge = document.getElementById('unreadAlertsBadge');
            if (badge) {
                badge.innerText = count > 0 ? count : '';
                badge.style.display = count > 0 ? 'inline-block' : 'none';
            }
        });

        // Listen for System Messages
        connection.on('ReceiveSystemMessage', (sysMsg) => {
            showToast(sysMsg.title, sysMsg.message, sysMsg.severity);
        });

        // Lifecycle Events
        connection.onreconnecting((error) => {
            console.warn('SignalR: Network fluctuation detected. Reconnecting in background...', error);
        });

        connection.onreconnected((connectionId) => {
            console.log('SignalR: Reconnected successfully! Connection ID:', connectionId);
        });

        connection.onclose((error) => {
            console.log('SignalR: Connection closed.', error);
        });

        try {
            await connection.start();
            isInitialized = true;
            console.log('🚀 SignalR Real-Time Notifications Connected!');
        } catch (err) {
            console.error('SignalR Connection Error:', err);
        }
    }

    // Initialize automatically when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    return {
        init,
        showToast
    };
})();
