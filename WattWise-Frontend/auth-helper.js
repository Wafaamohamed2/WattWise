const AuthHelper = {
    API_BASE_URL: 'https://localhost:7083',

    async checkAuth() {
        try {
            const response = await this.fetchWithAuth('/api/v1/account/me', { method: 'GET' });

            if (response && response.ok) {
                const user = await response.json();

                const isAuthPage = window.location.pathname.endsWith('login.html') ||
                    window.location.pathname.endsWith('register.html') ||
                    window.location.pathname.endsWith('forgot-password.html') ||
                    window.location.pathname.endsWith('reset-password.html');

                if (isAuthPage) {
                    const buildings = await this.checkOnboardingStatus(true);
                    if (buildings) {
                        window.location.href = 'index.html';
                    }
                    return true;
                }

                if (!window.location.pathname.endsWith('onboarding.html')) {
                    await this.checkOnboardingStatus(true);
                }

                return true;
            } else {
                this.handleUnauthenticated();
                return false;
            }
        } catch (error) {
            console.error('Auth Check Error:', error);
            this.handleUnauthenticated();
            return false;
        }
    },

    handleUnauthenticated() {
        const isAuthPage = window.location.pathname.endsWith('login.html') ||
            window.location.pathname.endsWith('register.html') ||
            window.location.pathname.endsWith('forgot-password.html') ||
            window.location.pathname.endsWith('reset-password.html');

        if (!isAuthPage) {
            window.location.href = 'login.html';
        }
    },

    async checkOnboardingStatus(redirectIfIncomplete = true) {
        try {
            const response = await this.fetchWithAuth('/api/v1/buildings');
            if (!response || !response.ok) return false;

            const result = await response.json();
            const buildings = result.data ?? result.details ?? (Array.isArray(result) ? result : []);

            if (!buildings || buildings.length === 0) {
                if (redirectIfIncomplete && !window.location.pathname.endsWith('onboarding.html')) {
                    window.location.href = 'onboarding.html';
                }
                return false;
            }

            const currentActiveId = this.getActiveBuildingId();
            let activeBuilding = buildings.find(b => b.id == currentActiveId);

            if (!activeBuilding) {
                activeBuilding = buildings[0];
            }

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
        wrapper.style.cssText = 'display:inline-flex; align-items:center; gap:8px; vertical-align:middle;';

        const select = document.createElement('select');
        select.id = 'globalBuildingSelect';
        select.style.cssText = `
            padding: 7px 12px;
            border-radius: 10px;
            background: #0f172a;
            color: #f8fafc;
            border: 1px solid rgba(255, 255, 255, 0.15);
            font-size: 0.85em;
            font-weight: 600;
            font-family: inherit;
            cursor: pointer;
            outline: none;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
            transition: all 0.2s ease;
        `;

        buildings.forEach(b => {
            const opt = document.createElement('option');
            opt.value = b.id;
            opt.textContent = `${b.name || 'Building ' + b.id} ${b.id == activeId ? '✓' : ''}`;
            if (b.id == activeId) opt.selected = true;
            select.appendChild(opt);
        });

        select.onchange = (e) => {
            const selectedBuildingId = e.target.value;
            this.setActiveBuildingId(selectedBuildingId);
            window.location.reload();
        };

        const newBtn = document.createElement('a');
        newBtn.href = 'onboarding.html';
        newBtn.style.cssText = `
            padding: 7px 12px;
            border-radius: 10px;
            background: linear-gradient(135deg, #22c55e, #14b8a6);
            color: #0f172a;
            text-decoration: none;
            font-size: 0.82em;
            font-weight: 700;
            font-family: inherit;
            display: inline-flex;
            align-items: center;
            gap: 4px;
            box-shadow: 0 4px 12px rgba(34, 197, 94, 0.2);
            transition: all 0.2s ease;
            white-space: nowrap;
        `;
        newBtn.onmouseover = () => { newBtn.style.transform = 'translateY(-1px)'; newBtn.style.opacity = '0.9'; };
        newBtn.onmouseout = () => { newBtn.style.transform = 'none'; newBtn.style.opacity = '1'; };
        newBtn.innerHTML = '<span style="font-size: 1.1em; font-weight: bold;">+</span> Building';

        wrapper.appendChild(select);
        wrapper.appendChild(newBtn);
        container.appendChild(wrapper);
    },

    async fetchWithAuth(url, options = {}, isRetry = false) {
        const absoluteUrl = url.startsWith('/') ? (this.API_BASE_URL + url) : url;

        const defaultHeaders = {
            'Content-Type': 'application/json',
        };

        const config = {
            ...options,
            credentials: 'include',
            headers: {
                ...defaultHeaders,
                ...options.headers
            }
        };

        try {
            let response;
            try {
                response = await fetch(absoluteUrl, config);
            } catch (err) {
                // Automatic HTTP fallback if HTTPS fails locally
                if (this.API_BASE_URL.startsWith('https://localhost:7083')) {
                    this.API_BASE_URL = 'http://localhost:5167';
                    const fallbackUrl = url.startsWith('/') ? (this.API_BASE_URL + url) : url;
                    response = await fetch(fallbackUrl, config);
                } else {
                    throw err;
                }
            }

            const AUTH_ENDPOINTS = [
                '/account/login',
                '/account/register',
                '/account/refresh-token',
                '/account/forgot-password',
                '/account/reset-password',
                '/account/resend-confirmation-email'
            ];
            const isAuthEndpoint = AUTH_ENDPOINTS.some(e => url.includes(e));

            if (response.status === 401 && !isRetry && !isAuthEndpoint) {
                console.warn('401 Unauthorized. Attempting cookie refresh...');

                const refreshResponse = await fetch(this.API_BASE_URL + '/api/v1/account/refresh-token', {
                    method: 'POST',
                    credentials: 'include',
                    headers: { 'Content-Type': 'application/json' }
                });

                if (refreshResponse.ok) {
                    console.log('Token refreshed via Cookie successfully. Retrying request...');
                    return this.fetchWithAuth(url, options, true);
                } else {
                    console.error('Refresh token failed or expired.');
                    this.logout();
                    return response;
                }
            }

            return response;
        } catch (error) {
            console.error('API Fetch Error:', error);
            throw error;
        }
    },

    async logout() {
        try {
            await fetch(this.API_BASE_URL + '/api/v1/account/logout', {
                method: 'POST',
                credentials: 'include'
            });
        } catch (e) {
            console.error('Logout error:', e);
        } finally {
            localStorage.removeItem('activeBuildingId');
            window.location.href = 'login.html';
        }
    },

    broadcastEvent(eventName, detail) {
        const event = new CustomEvent(eventName, { detail });
        window.dispatchEvent(event);
    }
};
