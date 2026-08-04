// Admin Dashboard API Functions

// Get system stats
const getSystemStats = async () => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/admin/stats`);
        if (!response) return null;
        
        const stats = await response.json();
        return response.ok ? stats : null;
    } catch (error) {
        console.error('Error fetching stats:', error);
        return null;
    }
};

// Get daily occupancy report
const getDailyOccupancy = async (date = null) => {
    try {
        const url = date 
            ? `${API_BASE_URL}/reports/daily-occupancy?date=${date}`
            : `${API_BASE_URL}/reports/daily-occupancy`;
        
        const response = await fetchWithAuth(url);
        if (!response) return null;
        
        const data = await response.json();
        return response.ok ? data : null;
    } catch (error) {
        console.error('Error fetching occupancy:', error);
        return null;
    }
};

// Get daily revenue report
const getDailyRevenue = async (date = null) => {
    try {
        const url = date 
            ? `${API_BASE_URL}/reports/daily-revenue?date=${date}`
            : `${API_BASE_URL}/reports/daily-revenue`;
        
        const response = await fetchWithAuth(url);
        if (!response) return null;
        
        const data = await response.json();
        return response.ok ? data : null;
    } catch (error) {
        console.error('Error fetching revenue:', error);
        return null;
    }
};

// Get service usage report
const getServiceUsage = async (startDate = null, endDate = null) => {
    try {
        let url = `${API_BASE_URL}/reports/service-usage`;
        const params = new URLSearchParams();
        
        if (startDate) params.append('startDate', startDate);
        if (endDate) params.append('endDate', endDate);
        
        if (params.toString()) {
            url += '?' + params.toString();
        }
        
        const response = await fetchWithAuth(url);
        if (!response) return null;
        
        const data = await response.json();
        return response.ok ? data : null;
    } catch (error) {
        console.error('Error fetching service usage:', error);
        return null;
    }
};

// Get daily reconciliation
const getDailyReconciliation = async (date = null) => {
    try {
        const url = date 
            ? `${API_BASE_URL}/audit/daily-reconciliation?date=${date}`
            : `${API_BASE_URL}/audit/daily-reconciliation`;
        
        const response = await fetchWithAuth(url);
        if (!response) return null;
        
        const data = await response.json();
        return response.ok ? data : null;
    } catch (error) {
        console.error('Error fetching reconciliation:', error);
        return null;
    }
};

// Get audit logs
const getAuditLogs = async (filters = {}) => {
    try {
        const params = new URLSearchParams();
        
        if (filters.entityName) params.append('entityName', filters.entityName);
        if (filters.entityId) params.append('entityId', filters.entityId);
        if (filters.startDate) params.append('startDate', filters.startDate);
        if (filters.endDate) params.append('endDate', filters.endDate);
        
        const url = `${API_BASE_URL}/audit/logs?${params.toString()}`;
        
        const response = await fetchWithAuth(url);
        if (!response) return [];
        
        const logs = await response.json();
        return response.ok ? logs : [];
    } catch (error) {
        console.error('Error fetching audit logs:', error);
        return [];
    }
};

// Get all invoices
const getAllInvoices = async () => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/invoices`);
        if (!response) return [];
        
        const invoices = await response.json();
        return response.ok ? invoices : [];
    } catch (error) {
        console.error('Error fetching invoices:', error);
        return [];
    }
};

// Get invoice by ID
const getInvoiceById = async (invoiceId) => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/invoices/${invoiceId}`);
        if (!response) return null;
        
        const invoice = await response.json();
        return response.ok ? invoice : null;
    } catch (error) {
        console.error('Error fetching invoice:', error);
        return null;
    }
};

// Process payment
const processPayment = async (invoiceId, paymentData) => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/invoices/${invoiceId}/pay`, {
            method: 'POST',
            body: JSON.stringify(paymentData)
        });

        if (!response) return { success: false, message: 'Authentication required' };

        const data = await response.json();
        return { success: response.ok, message: data.message };
    } catch (error) {
        console.error('Error processing payment:', error);
        return { success: false, message: 'Network error' };
    }
};

// Get notifications/alerts
const getAlerts = async () => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/alerts`);
        if (!response) return [];
        
        const alerts = await response.json();
        return response.ok ? alerts : [];
    } catch (error) {
        console.error('Error fetching alerts:', error);
        return [];
    }
};

// Mark notification as read
const markAlertAsRead = async (alertId) => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/alerts/${alertId}/mark-read`, {
            method: 'POST'
        });

        if (!response) return { success: false };

        const data = await response.json();
        return { success: response.ok, message: data.message };
    } catch (error) {
        console.error('Error marking alert as read:', error);
        return { success: false };
    }
};

// Seed database (admin only)
const seedDatabase = async () => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/admin/seed`, {
            method: 'POST'
        });

        if (!response) return { success: false, message: 'Authentication required' };

        const data = await response.json();
        return { success: response.ok, message: data.message };
    } catch (error) {
        console.error('Error seeding database:', error);
        return { success: false, message: 'Network error' };
    }
};

