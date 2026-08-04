// API Base URL Configuration
const API_BASE_URL = window.location.origin + '/api';

// Get JWT token from localStorage
const getToken = () => {
    return localStorage.getItem('resort_auth_token');
};

// Set JWT token to localStorage
const setToken = (token) => {
    localStorage.setItem('resort_auth_token', token);
};

// Remove JWT token from localStorage
const removeToken = () => {
    localStorage.removeItem('resort_auth_token');
    localStorage.removeItem('resort_user');
};

// Save user info
const setUser = (user) => {
    localStorage.setItem('resort_user', JSON.stringify(user));
};

// Get user info
const getUser = () => {
    const userStr = localStorage.getItem('resort_user');
    return userStr ? JSON.parse(userStr) : null;
};

// Fetch with authentication
const fetchWithAuth = async (url, options = {}) => {
    const token = getToken();
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers
    };

    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    try {
        const response = await fetch(url, {
            ...options,
            headers
        });

        if (response.status === 401) {
            // Unauthorized - redirect to login
            removeToken();
            window.location.href = '/customer/login.html';
            return null;
        }

        return response;
    } catch (error) {
        console.error('Fetch error:', error);
        throw error;
    }
};

// Check if user is logged in
const isAuthenticated = () => {
    return !!getToken();
};

// Logout
const logout = () => {
    removeToken();
    window.location.href = '/customer/index.html';
};

