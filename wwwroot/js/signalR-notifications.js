// signalR-notifications.js
class NotificationManager {
    constructor() {
        this.connection = null;
        this.userId = $('#signalRData').data('user-id');
        this.userType = $('#signalRData').data('user-type');
        this.userName = $('#signalRData').data('user-name');
        this.isConnected = false;
        
        this.initialize();
    }
    
    initialize() {
        this.setupSignalR();
        this.setupEventHandlers();
        this.loadInitialNotifications();
    }
    
    setupSignalR() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .withAutomaticReconnect([0, 2000, 10000, 30000])
            .configureLogging(signalR.LogLevel.Warning)
            .build();
        
        // Handle incoming notifications
        this.connection.on("ReceiveNotification", (notification) => {
            this.handleNotification(notification);
        });
        
        // Handle property notifications
        this.connection.on("ReceivePropertyNotification", (notification) => {
            console.log("Property notification:", notification);
        });
        
        // Connection events
        this.connection.onreconnecting(() => {
            console.log("SignalR reconnecting...");
            $('#notificationLoading').show();
        });
        
        this.connection.onreconnected(() => {
            console.log("SignalR reconnected.");
            this.joinGroups();
            this.loadInitialNotifications();
        });
        
        this.connection.onclose(() => {
            console.log("SignalR disconnected.");
            this.isConnected = false;
            setTimeout(() => this.startConnection(), 5000);
        });
        
        // Start connection
        this.startConnection();
    }
    
    async startConnection() {
        try {
            await this.connection.start();
            console.log("SignalR connected.");
            this.isConnected = true;
            await this.joinGroups();
        } catch (err) {
            console.error("SignalR connection error:", err);
            setTimeout(() => this.startConnection(), 5000);
        }
    }
    
    async joinGroups() {
        if (!this.userId) return;
        
        try {
            await this.connection.invoke("JoinUserGroup", this.userId);
            console.log("Joined user group:", this.userId);
            
            // Join property groups if needed
            const propertyId = this.getCurrentPropertyId();
            if (propertyId) {
                await this.connection.invoke("JoinPropertyGroup", propertyId);
            }
        } catch (err) {
            console.error("Error joining groups:", err);
        }
    }
    
    getCurrentPropertyId() {
        const path = window.location.pathname;
        const match = path.match(/\/Properties\/([^\/]+)/);
        return match ? match[1] : null;
    }
    
    handleNotification(notification) {
        console.log("New notification received:", notification);
        
        // Update badge count
        this.updateBadgeCount(1);
        
        // Show toast notification
        this.showToastNotification(notification);
        
        // Add to dropdown list
        this.addToDropdown(notification);
        
        // Update specific page content
        this.updatePageContent(notification);
        
        // Update request count badge if landlord
        if (this.userType === 'Landlord' && notification.type === 'LeaseRequestCreated') {
            this.updateRequestCount(1);
        }
    }
    
    updateBadgeCount(increment = 1) {
        const badge = $('#notificationBadge');
        let count = parseInt(badge.text()) || 0;
        count += increment;
        
        if (count > 0) {
            badge.text(count).show();
        } else {
            badge.hide();
        }
    }
    
    showToastNotification(notification) {
        const toastId = 'live-toast-' + Date.now();
        const typeClass = this.getNotificationTypeClass(notification.type);
        const icon = this.getNotificationIcon(notification.type);
        
        const toastHtml = `
            <div id="${toastId}" class="toast show mb-2" role="alert" aria-live="assertive" aria-atomic="true" data-bs-autohide="true" data-bs-delay="5000">
                <div class="toast-header ${typeClass} text-white">
                    <i class="bi ${icon} me-2"></i>
                    <strong class="me-auto">${notification.title}</strong>
                    <small class="text-white-50">${this.formatTime(notification.timestamp)}</small>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
                <div class="toast-body">
                    ${notification.message}
                    ${this.getActionButtons(notification)}
                </div>
            </div>
        `;
        
        $('#notificationToastContainer').append(toastHtml);
        
        // Auto-remove after hide
        $('#' + toastId).on('hidden.bs.toast', function() {
            $(this).remove();
        });
    }
    
    addToDropdown(notification) {
        const list = $('#notificationList');
        const emptyState = $('#notificationEmpty');
        const loading = $('#notificationLoading');
        
        // Hide loading/empty states if this is first notification
        if (list.find('.notification-item').length === 0) {
            loading.hide();
            emptyState.hide();
            list.show();
        }
        
        const itemHtml = this.createNotificationItem(notification);
        list.prepend(itemHtml);
        
        // Limit to 10 items
        if (list.find('.notification-item').length > 10) {
            list.find('.notification-item').last().remove();
        }
    }
    
    createNotificationItem(notification) {
        const typeClass = this.getNotificationTypeClass(notification.type);
        const icon = this.getNotificationIcon(notification.type);
        const isUnread = true; // New notifications are unread
        
        return `
            <div class="list-group-item notification-item ${isUnread ? 'unread bg-light' : ''} border-start-0 border-end-0" 
                 data-type="${notification.type}" data-id="${notification.leaseRequestId}">
                <div class="d-flex align-items-start">
                    <div class="flex-shrink-0 me-2">
                        <div class="rounded-circle ${typeClass} p-2 text-white">
                            <i class="bi ${icon}"></i>
                        </div>
                    </div>
                    <div class="flex-grow-1">
                        <div class="d-flex justify-content-between align-items-start">
                            <h6 class="mb-1 fw-bold">${notification.title}</h6>
                            <small class="text-muted">${this.formatTime(notification.timestamp)}</small>
                        </div>
                        <p class="mb-1 small">${notification.message}</p>
                        ${this.getDropdownActionButtons(notification)}
                    </div>
                </div>
            </div>
        `;
    }
    
    getNotificationTypeClass(type) {
        switch(type) {
            case 'LeaseRequestCreated': return 'bg-warning';
            case 'LeaseRequestApproved': return 'bg-success';
            case 'LeaseRequestRejected': return 'bg-danger';
            default: return 'bg-primary';
        }
    }
    
    getNotificationIcon(type) {
        switch(type) {
            case 'LeaseRequestCreated': return 'bi-envelope-plus';
            case 'LeaseRequestApproved': return 'bi-check-circle';
            case 'LeaseRequestRejected': return 'bi-x-circle';
            default: return 'bi-bell';
        }
    }
    
    getActionButtons(notification) {
        if (notification.type === 'LeaseRequestCreated') {
            return `
                <div class="mt-2 pt-2 border-top">
                    <a href="/Lease/Requests" class="btn btn-sm btn-outline-light me-1">View Request</a>
                    <button onclick="notificationManager.markAsRead('${notification.leaseRequestId}')" class="btn btn-sm btn-outline-light">Dismiss</button>
                </div>
            `;
        }
        return '';
    }
    
    getDropdownActionButtons(notification) {
        let buttons = '';
        
        if (notification.type === 'LeaseRequestCreated') {
            buttons = `
                <div class="mt-2">
                    <a href="/Lease/Requests" class="btn btn-sm btn-outline-primary btn-sm">View</a>
                    <button onclick="notificationManager.markAsRead('${notification.leaseRequestId}')" class="btn btn-sm btn-outline-secondary btn-sm">Mark Read</button>
                </div>
            `;
        } else if (notification.type === 'LeaseRequestApproved') {
            buttons = `
                <div class="mt-2">
                    <a href="/Lease/Index" class="btn btn-sm btn-outline-success btn-sm">View Lease</a>
                </div>
            `;
        }
        
        return buttons;
    }
    
    formatTime(timestamp) {
        const date = new Date(timestamp);
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        
        if (diffMins < 1) return 'Just now';
        if (diffMins < 60) return `${diffMins}m ago`;
        if (diffMins < 1440) return `${Math.floor(diffMins / 60)}h ago`;
        return date.toLocaleDateString();
    }
    
    updatePageContent(notification) {
        const currentPath = window.location.pathname;
        
        // Refresh landlord requests page
        if (notification.type === 'LeaseRequestCreated' && 
            currentPath.includes('/Lease/Requests')) {
            this.refreshRequestsTable();
        }
        
        // Refresh tenant leases page
        if ((notification.type === 'LeaseRequestApproved' || notification.type === 'LeaseRequestRejected') &&
            currentPath.includes('/Lease/Index')) {
            this.refreshTenantLeases();
        }
    }
    
    refreshRequestsTable() {
        $.ajax({
            url: '/Lease/GetRequestsPartial',
            type: 'GET',
            success: (html) => {
                $('#requestsTableContainer').html(html);
                console.log('Requests table refreshed via AJAX');
            },
            error: (xhr) => {
                console.error('Error refreshing requests:', xhr.responseText);
            }
        });
    }
    
    refreshTenantLeases() {
        $.ajax({
            url: '/Lease/GetTenantLeasesPartial',
            type: 'GET',
            success: (html) => {
                $('#leasesTableContainer').html(html);
                console.log('Tenant leases refreshed via AJAX');
            },
            error: (xhr) => {
                console.error('Error refreshing leases:', xhr.responseText);
            }
        });
    }
    
    updateRequestCount(increment = 1) {
        const badge = $('#requestCountBadge');
        let count = parseInt(badge.text()) || 0;
        count += increment;
        
        if (count > 0) {
            badge.text(count).show();
        } else {
            badge.hide();
        }
    }
    
    async markAsRead(notificationId) {
        try {
            await $.ajax({
                url: '/Notification/MarkAsRead',
                type: 'POST',
                data: { notificationId: notificationId }
            });
            
            // Remove from UI
            $(`.notification-item[data-id="${notificationId}"]`).removeClass('unread bg-light');
            
            // Update badge count
            this.updateBadgeCount(-1);
            
        } catch (err) {
            console.error('Error marking notification as read:', err);
        }
    }
    
    loadInitialNotifications() {
        // Load existing notifications via AJAX
        $.ajax({
            url: '/Notification/GetRecent',
            type: 'GET',
            success: (notifications) => {
                this.displayInitialNotifications(notifications);
            },
            error: () => {
                $('#notificationLoading').hide();
                $('#notificationEmpty').show();
            }
        });
    }
    
    displayInitialNotifications(notifications) {
        const list = $('#notificationList');
        const emptyState = $('#notificationEmpty');
        const loading = $('#notificationLoading');
        
        loading.hide();
        
        if (!notifications || notifications.length === 0) {
            emptyState.show();
            return;
        }
        
        list.show();
        emptyState.hide();
        
        // Clear existing items
        list.empty();
        
        // Add notifications
        notifications.forEach(notification => {
            const itemHtml = this.createNotificationItem(notification);
            list.append(itemHtml);
        });
    }
    
    setupEventHandlers() {
        // Handle notification item clicks
        $(document).on('click', '.notification-item', function(e) {
            if (!$(e.target).is('button, a, .btn')) {
                const notificationId = $(this).data('id');
                notificationManager.markAsRead(notificationId);
            }
        });
    }
}

// Initialize when document is ready
$(document).ready(function() {
    window.notificationManager = new NotificationManager();
});