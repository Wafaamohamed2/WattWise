
const AuthHelper = {

    async checkAuth() {
        try {
            const res = await fetch('/api/account/me', {
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' }
            });
            if (res.status === 401 || res.status === 500) {
                window.location.href = 'login.html';
                return false;
            }
            return true;
        } catch (e) {
            window.location.href = 'login.html';
            return false;
        }
    },

    async fetchWithAuth(url, options = {}) {
        const response = await fetch(url, {
            ...options,
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json',
                ...(options.headers || {})
            }
        });
        if (response.status === 401) {
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
        } catch (e) { }
        window.location.href = 'login.html';
    },

    async apiCall(url, options = {}) {
        const response = await this.fetchWithAuth(url, options);
        if (!response) return null;
        const result = await response.json();
        if (response.ok) return result.details ?? result.Details ?? result.data ?? result;
        const errorMsg = result.message ?? result.Message ?? 'An error occurred';
        if (typeof toastr !== 'undefined') toastr.error(errorMsg);
        throw new Error(errorMsg);
    },

    broadcastEvent(name, detail) {
        window.dispatchEvent(new CustomEvent(name, { detail }));
    }
};