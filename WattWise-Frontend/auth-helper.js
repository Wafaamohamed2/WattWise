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
                    if (retryRes.ok) {
                        await this.checkOnboardingStatus();
                        return true;
                    }
                }
                window.location.href = 'login.html';
                return false;
            }

            if (!res.ok) {
                window.location.href = 'login.html';
                return false;
            }

            // Verify Onboarding Status
            await this.checkOnboardingStatus();
            return true;
        } catch (e) {
            window.location.href = 'login.html';
            return false;
        }
    },

    async checkOnboardingStatus(redirectIfIncomplete = true) {
        try {
            const buildings = await this.apiCall('/api/v1/buildings');
            if (!buildings || !Array.isArray(buildings) || buildings.length === 0) {
                if (redirectIfIncomplete && !window.location.pathname.endsWith('onboarding.html')) {
                    window.location.href = 'onboarding.html';
                }
                return false;
            }

            const currentActiveId = this.getActiveBuildingId();
            let activeBuilding = buildings.find(b => b.id == currentActiveId) || buildings[0];

            if (!activeBuilding.isOnboardingComplete) {
                if (redirectIfIncomplete && !window.location.pathname.endsWith('onboarding.html')) {
                    window.location.href = `onboarding.html?buildingId=${activeBuilding.id}`;
                }
                return false;
            }

            this.setActiveBuildingId(activeBuilding.id);
            return buildings;
        } catch (e) {
            return false;
        }
    },

    getActiveBuildingId() {
        return localStorage.getItem('activeBuildingId');
    },

    setActiveBuildingId(id) {
        localStorage.setItem('activeBuildingId', id);
        this.broadcastEvent('buildingChanged', { buildingId: id });
    },

    async renderBuildingSelector(containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const buildings = await this.checkOnboardingStatus(false);
        if (!buildings || !Array.isArray(buildings) || buildings.length === 0) return;

        const activeId = this.getActiveBuildingId();

        container.innerHTML = '';

        const wrapper = document.createElement('div');
        wrapper.className = 'building-selector-wrapper';
        wrapper.style.cssText = 'display:inline-flex; align-items:center; gap:10px; vertical-align:middle;';

        const select = document.createElement('select');
        select.id = 'globalBuildingSelect';
        select.style.cssText = `
            padding: 10px 16px;
            border-radius: 12px;
            background: rgba(102, 126, 234, 0.08);
            color: #4c51bf;
            border: 1.5px solid rgba(102, 126, 234, 0.3);
            font-weight: 600;
            font-size: 0.95em;
            font-family: inherit;
            cursor: pointer;
            outline: none;
            transition: all 0.3s ease;
            box-shadow: 0 2px 8px rgba(102, 126, 234, 0.08);
        `;
        select.onmouseover = () => { select.style.borderColor = '#667eea'; select.style.background = 'rgba(102, 126, 234, 0.14)'; };
        select.onmouseout = () => { select.style.borderColor = 'rgba(102, 126, 234, 0.3)'; select.style.background = 'rgba(102, 126, 234, 0.08)'; };
        select.onchange = (e) => {
            AuthHelper.setActiveBuildingId(e.target.value);
            window.location.reload();
        };

        buildings.forEach(b => {
            const opt = document.createElement('option');
            opt.value = b.id;
            opt.textContent = `🏢 ${b.name} (${b.type === 1 ? 'Home' : 'Company'})`;
            opt.style.cssText = 'background: white; color: #374151; font-weight: 500; padding: 6px;';
            if (b.id == activeId) opt.selected = true;
            select.appendChild(opt);
        });

        const newBtn = document.createElement('a');
        newBtn.href = 'onboarding.html';
        newBtn.title = 'Add New Building';
        newBtn.style.cssText = `
            padding: 10px 16px;
            border-radius: 12px;
            background: linear-gradient(135deg, #667eea, #764ba2);
            color: white;
            text-decoration: none;
            font-size: 0.9em;
            font-weight: 600;
            font-family: inherit;
            display: inline-flex;
            align-items: center;
            gap: 6px;
            box-shadow: 0 4px 14px rgba(102, 126, 234, 0.35);
            transition: all 0.3s ease;
            white-space: nowrap;
        `;
        newBtn.onmouseover = () => { newBtn.style.transform = 'translateY(-2px)'; newBtn.style.boxShadow = '0 6px 20px rgba(102, 126, 234, 0.45)'; };
        newBtn.onmouseout = () => { newBtn.style.transform = 'none'; newBtn.style.boxShadow = '0 4px 14px rgba(102, 126, 234, 0.35)'; };
        newBtn.innerHTML = '<span style="font-size: 1.1em; font-weight: bold;">+</span> Add Building';

        wrapper.appendChild(select);
        wrapper.appendChild(newBtn);
        container.appendChild(wrapper);
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
        localStorage.removeItem('activeBuildingId');
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
