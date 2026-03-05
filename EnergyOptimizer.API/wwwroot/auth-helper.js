const AuthHelper = {

    getToken() {
        return localStorage.getItem('token');
    },

    // All API calls go through here - adds Authorization header automatically
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