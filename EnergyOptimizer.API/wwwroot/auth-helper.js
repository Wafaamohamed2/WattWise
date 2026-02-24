const AuthHelper = {
    getToken: () => localStorage.getItem('token'),

    isAuthenticated() { return !!this.getToken(); },

    logout() {
        localStorage.removeItem('token');
        localStorage.removeItem('user'); 
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
        if (response.status === 401) {
            console.warn("Token expired or invalid. Logging out...");
            this.logout();
            return null;
        }
        return response;
    },
    async handleLoginResponse(response) {
        const result = await response.json();

        if (response.ok) {
            localStorage.setItem('token', result.details?.token || result.token);
            localStorage.setItem('user', JSON.stringify(result.details?.user || result.user));
            return { success: true, message: result.message };
        } else {
            return { success: false, message: result.message || "Login failed" };
        }
    },
    async apiCall(url, options = {}) {
        const response = await this.fetchWithAuth(url, options);
        if (!response) return null;

        const result = await response.json();

        if (response.ok) {
            return result.data || result.details || result;
        } else {
            const errorMsg = result.message || "An error occurred";
            toastr.error(errorMsg);
            throw new Error(errorMsg);
        }
    },
    broadcastEvent(name, detail) {
        window.dispatchEvent(new CustomEvent(name, { detail }));
    }
};
AuthHelper.checkAuth();