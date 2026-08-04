// Booking API Functions

// Get all rooms
const getAllRooms = async () => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/rooms`);
        if (!response) return [];
        
        const rooms = await response.json();
        return response.ok ? rooms : [];
    } catch (error) {
        console.error('Error fetching rooms:', error);
        return [];
    }
};

// Get available rooms
const getAvailableRooms = async (roomType = null) => {
    try {
        const url = roomType 
            ? `${API_BASE_URL}/rooms/available?roomType=${encodeURIComponent(roomType)}`
            : `${API_BASE_URL}/rooms/available`;
        
        const response = await fetchWithAuth(url);
        if (!response) return [];
        
        const rooms = await response.json();
        return response.ok ? rooms : [];
    } catch (error) {
        console.error('Error fetching available rooms:', error);
        return [];
    }
};

// Create a new booking
const createBooking = async (bookingData) => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/bookings`, {
            method: 'POST',
            body: JSON.stringify(bookingData)
        });

        if (!response) return { success: false, message: 'Authentication required' };

        const data = await response.json();
        
        if (response.ok) {
            return { success: true, booking: data };
        } else {
            return { success: false, message: data.message || 'Booking failed' };
        }
    } catch (error) {
        console.error('Error creating booking:', error);
        return { success: false, message: 'Network error' };
    }
};

// Get customer bookings
const getCustomerBookings = async (customerId) => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/bookings/customer/${customerId}`);
        if (!response) return [];
        
        const bookings = await response.json();
        return response.ok ? bookings : [];
    } catch (error) {
        console.error('Error fetching bookings:', error);
        return [];
    }
};

// Get all bookings (admin/staff only)
const getAllBookings = async () => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/bookings`);
        if (!response) return [];
        
        const bookings = await response.json();
        return response.ok ? bookings : [];
    } catch (error) {
        console.error('Error fetching all bookings:', error);
        return [];
    }
};

// Get booking by ID
const getBookingById = async (bookingId) => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/bookings/${bookingId}`);
        if (!response) return null;
        
        const booking = await response.json();
        return response.ok ? booking : null;
    } catch (error) {
        console.error('Error fetching booking:', error);
        return null;
    }
};

// Transfer booking to front desk
const transferToFrontDesk = async (bookingId) => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/bookings/${bookingId}/transfer-to-frontdesk`, {
            method: 'POST'
        });

        if (!response) return { success: false, message: 'Authentication required' };

        const data = await response.json();
        return { success: response.ok, message: data.message };
    } catch (error) {
        console.error('Error transferring booking:', error);
        return { success: false, message: 'Network error' };
    }
};

// Assign room to booking (admin/staff only)
const assignRoom = async (bookingId, roomId) => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/bookings/${bookingId}/assign-room`, {
            method: 'POST',
            body: JSON.stringify({ roomId })
        });

        if (!response) return { success: false, message: 'Authentication required' };

        const data = await response.json();
        return { success: response.ok, message: data.message };
    } catch (error) {
        console.error('Error assigning room:', error);
        return { success: false, message: 'Network error' };
    }
};

// Check-in (admin/staff only)
const checkIn = async (bookingId) => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/bookings/${bookingId}/checkin`, {
            method: 'POST'
        });

        if (!response) return { success: false, message: 'Authentication required' };

        const data = await response.json();
        return { success: response.ok, message: data.message };
    } catch (error) {
        console.error('Error checking in:', error);
        return { success: false, message: 'Network error' };
    }
};

// Add charge (admin/staff only)
const addCharge = async (bookingId, chargeData) => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/bookings/${bookingId}/add-charge`, {
            method: 'POST',
            body: JSON.stringify(chargeData)
        });

        if (!response) return { success: false, message: 'Authentication required' };

        const data = await response.json();
        return { success: response.ok, message: data.message };
    } catch (error) {
        console.error('Error adding charge:', error);
        return { success: false, message: 'Network error' };
    }
};

// Check-out (admin/staff only)
const checkOut = async (bookingId) => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/bookings/${bookingId}/checkout`, {
            method: 'POST'
        });

        if (!response) return { success: false, message: 'Authentication required' };

        const data = await response.json();
        
        if (response.ok) {
            return { success: true, invoice: data.invoice, message: data.message };
        } else {
            return { success: false, message: data.message };
        }
    } catch (error) {
        console.error('Error checking out:', error);
        return { success: false, message: 'Network error' };
    }
};

// Cancel booking
const cancelBooking = async (bookingId, reason) => {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/bookings/${bookingId}/cancel`, {
            method: 'POST',
            body: JSON.stringify({ reason })
        });

        if (!response) return { success: false, message: 'Authentication required' };

        const data = await response.json();
        return { success: response.ok, message: data.message };
    } catch (error) {
        console.error('Error cancelling booking:', error);
        return { success: false, message: 'Network error' };
    }
};

