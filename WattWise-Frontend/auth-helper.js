const AuthHelper = {
    API_BASE_URL: 'http://localhost:5167',
    _isRefreshing: false,
    _refreshPromise: null,

    async tryRefreshToken() {
        if (this._isRefreshing) {
            return this._refreshPromise;
        }

        this._isRefreshing = true;
        this._refreshPromise = (async () => {
            try {
                const res = await fetch(this.API_BASE_URL + '/api/v1/account/refresh-token', {
                    method: 'POST',
                    credentials: 'include',
                    headers: { 'Content-Type': 'application/json' }
                });
                return res.ok;
            } catch (e) {
                return false;
            } finally {
                this._isRefreshing = false;
                this._refreshPromise = null;
            }
        })();

        return this._refreshPromise;
    },

    async checkAuth() {
        try {
            const res = await fetch(this.API_BASE_URL + '/api/v1/account/me', {
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' }
            });

            if (res.status === 401) {
                const refreshed = await this.tryRefreshToken();
                if (refreshed) {
                    const retryRes = await fetch(this.API_BASE_URL + '/api/v1/account/me', {
                        credentials: 'include',
                        headers: { 'Content-Type': 'application/json' }
                    });
                    if (retryRes.ok) return true;
                }
                window.location.href = 'login.html';
                return false;
            }

            if (!res.ok) {
                window.location.href = 'login.html';
                return false;
            }
            return true;
        } catch (e) {
            window.location.href = 'login.html';
            return false;
        }
    },

    async fetchWithAuth(url, options = {}, isRetry = false) {
        const absoluteUrl = url.startsWith('/') ? (this.API_BASE_URL + url) : url;
        const response = await fetch(absoluteUrl, {
            ...options,
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json',
                ...(options.headers || {})
            }
        });

        if (response.status === 401 && !isRetry) {
            const refreshed = await this.tryRefreshToken();
            if (refreshed) {
                return await this.fetchWithAuth(url, options, true);
            }
            window.location.href = 'login.html';
            return null;
        }

        if (response.status === 401) {
            window.location.href = 'login.html';
            return null;
        }

        return response;
    },

    async logout() {
        try {
            await fetch(this.API_BASE_URL + '/api/v1/account/logout', {
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
