const AuthHelper = {
    getToken: () => localStorage.getItem('token'),

    isAuthenticated() { return !!this.getToken(); },

    logout() {
        localStorage.removeItem('token');
        window.location.href = 'login.html';
    },

    checkAuth() {
        if (!this.isAuthenticated() && !window.location.pathname.includes('login.html')) {
            window.location.href = 'login.html';
            return false;
        }
        return true;
    },

    async fetchWithAuth(url, options = {}) {
        const token = this.getToken();
        const headers = {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
            ...(options.headers || {})
        };
        const response = await fetch(url, { ...options, headers });
        if (response.status === 401) this.logout();
        return response;
    }
};
AuthHelper.checkAuth();