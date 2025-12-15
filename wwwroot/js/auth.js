// Authentication Helper Functions
const AUTH_API_BASE = 'http://localhost:5005/api/auth';

// Token Management
function getToken() {
    return localStorage.getItem('authToken');
}

function setToken(token) {
    localStorage.setItem('authToken', token);
}

function removeToken() {
    localStorage.removeItem('authToken');
    localStorage.removeItem('user');
}

function getUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
}

function setUser(user) {
    localStorage.setItem('user', JSON.stringify(user));
}

function isAuthenticated() {
    return !!getToken();
}

// API Request Helper with Auth
async function authFetch(url, options = {}) {
    const token = getToken();
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers
    };

    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(url, {
        ...options,
        headers
    });

    if (response.status === 401) {
        // Token expired or invalid
        removeToken();
        window.location.href = '/login.html';
        return null;
    }

    return response;
}

// Register Function
async function register(formData) {
    try {
        const response = await fetch(`${AUTH_API_BASE}/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        });

        const data = await response.json();
        return data;
    } catch (error) {
        console.error('Registration error:', error);
        return {
            success: false,
            message: 'Network error. Please try again.'
        };
    }
}

// Login Function
async function login(email, password) {
    try {
        const response = await fetch(`${AUTH_API_BASE}/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email, password })
        });

        const data = await response.json();

        if (data.success && data.token) {
            setToken(data.token);
            setUser(data.user);
        }

        return data;
    } catch (error) {
        console.error('Login error:', error);
        return {
            success: false,
            message: 'Network error. Please try again.'
        };
    }
}

// Google Login Function
async function loginWithGoogle(idToken, userType = 'Client') {
    try {
        const response = await fetch(`${AUTH_API_BASE}/google`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ idToken, userType })
        });

        const data = await response.json();

        if (data.success && data.token) {
            setToken(data.token);
            setUser(data.user);
        }

        return data;
    } catch (error) {
        console.error('Google login error:', error);
        return {
            success: false,
            message: 'Network error. Please try again.'
        };
    }
}

// Logout Function
async function logout() {
    try {
        const token = getToken();
        if (token) {
            await authFetch(`${AUTH_API_BASE}/logout`, {
                method: 'POST'
            });
        }
    } catch (error) {
        console.error('Logout error:', error);
    } finally {
        removeToken();
        window.location.href = '/login.html';
    }
}

// Get Current User
async function getCurrentUser() {
    try {
        const response = await authFetch(`${AUTH_API_BASE}/me`);
        if (!response) return null;

        const data = await response.json();
        if (data) {
            setUser(data);
            return data;
        }
        return null;
    } catch (error) {
        console.error('Get user error:', error);
        return null;
    }
}

// Check if user is authenticated and redirect if not
function requireAuth() {
    if (!isAuthenticated()) {
        window.location.href = '/login.html';
        return false;
    }
    return true;
}

// Update UI based on auth status
function updateAuthUI() {
    const user = getUser();
    const authLinks = document.querySelector('.navbar-nav:last-child');

    if (user && authLinks) {
        authLinks.innerHTML = `
            <li class="nav-item dropdown">
                <a class="nav-link dropdown-toggle" href="#" id="userDropdown" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                    <i class="bi bi-person-circle"></i> ${user.name}
                </a>
                <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="userDropdown">
                    <li><a class="dropdown-item" href="/profile.html"><i class="bi bi-person"></i> Profile</a></li>
                    <li><a class="dropdown-item" href="/appointments.html"><i class="bi bi-calendar"></i> Appointments</a></li>
                    <li><hr class="dropdown-divider"></li>
                    <li><a class="dropdown-item" href="#" onclick="logout(); return false;"><i class="bi bi-box-arrow-right"></i> Logout</a></li>
                </ul>
            </li>
        `;
    }
}

// Initialize auth UI on page load
document.addEventListener('DOMContentLoaded', function() {
    updateAuthUI();
});
