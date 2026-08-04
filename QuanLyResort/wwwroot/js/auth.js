// Authentication API Functions

// Customer Login
const customerLogin = async (email, password) => {
    try {
        const response = await fetch(`${API_BASE_URL}/auth/customer-login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email, password })
        });

        const data = await response.json();

        if (response.ok) {
            setToken(data.token);
            setUser(data.user);
            
            // Dispatch custom event để update navbar ngay lập tức
            window.dispatchEvent(new CustomEvent('userLoggedIn', { detail: data.user }));
            
            return { success: true, user: data.user };
        } else {
            return { success: false, message: data.message || 'Login failed' };
        }
    } catch (error) {
        console.error('Login error:', error);
        return { success: false, message: 'Network error' };
    }
};

// Admin/Staff Login
const adminLogin = async (email, password) => {
    try {
        const response = await fetch(`${API_BASE_URL}/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email, password })
        });

        const data = await response.json();

        if (response.ok) {
            setToken(data.token);
            setUser(data.user);
            
            // Dispatch custom event để update navbar ngay lập tức
            window.dispatchEvent(new CustomEvent('userLoggedIn', { detail: data.user }));
            
            return { success: true, user: data.user };
        } else {
            return { success: false, message: data.message || 'Login failed' };
        }
    } catch (error) {
        console.error('Login error:', error);
        return { success: false, message: 'Network error' };
    }
};

// Register Customer
const registerCustomer = async (userData) => {
    try {
        const response = await fetch(`${API_BASE_URL}/auth/register-customer`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(userData)
        });

        const data = await response.json();

        if (response.ok) {
            return { success: true, message: 'Registration successful' };
        } else {
            return { success: false, message: data.message || 'Registration failed' };
        }
    } catch (error) {
        console.error('Registration error:', error);
        return { success: false, message: 'Network error' };
    }
};

// Check authentication and redirect if not logged in
const requireAuth = () => {
    if (!isAuthenticated()) {
        window.location.href = '/customer/login.html';
    }
};

// Check if user has required role
const hasRole = (role) => {
    const user = getUser();
    return user && user.role === role;
};

