const AuthHelper = {
    getToken() {
        return localStorage.getItem('token');
    },

    async checkAuth() {
        const token = localStorage.getItem('token');
        if (!token) {
            window.location.href = 'login.html';
            return false;
        }
        // Verify token is not expired by checking its payload
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            if (payload.exp && payload.exp * 1000 < Date.now()) {
                localStorage.removeItem('token');
                window.location.href = 'login.html';
                return false;
            }
        } catch {
        }
        return true;
    },

    async fetchWithAuth(url, options = {}) {
        const token = this.getToken();
        const response = await fetch(url, {
            ...options,
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json',
                ...(token ? { 'Authorization': 'Bearer ' + token } : {}),
                ...(options.headers || {})
            }
        });
        if (response.status === 401) {
            localStorage.removeItem('token');
            window.location.href = 'login.html';
            return null;
        }
        return response;
    },

    async logout() {
        try {
            await fetch('/api/account/logout', {
                method: 'POST',
                credentials: 'include'
            });
        } catch { }
        localStorage.removeItem('token');
        window.location.href = 'login.html';
    },

    async apiCall(url, options = {}) {
        const response = await this.fetchWithAuth(url, options);
        if (!response) return null;
        const result = await response.json();
        if (response.ok) return result.Details ?? result.details ?? result.data ?? result;
        const errorMsg = result.Message ?? result.message ?? 'An error occurred';
        if (typeof toastr !== 'undefined') toastr.error(errorMsg);
        throw new Error(errorMsg);
    },

    broadcastEvent(name, detail) {
        window.dispatchEvent(new CustomEvent(name, { detail }));
    }
};