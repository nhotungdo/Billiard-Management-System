/* ==========================================================
   BilliardPro — Unified Application Scripts
   ========================================================== */
(function () {
    'use strict';

    const App = {};

    /* ---------------- Formatting helpers ---------------- */
    App.formatVnd = function (value) {
        const n = Number(value || 0);
        return Math.round(n).toLocaleString('vi-VN') + ' đ';
    };

    App.formatNumber = function (value) {
        return Number(value || 0).toLocaleString('vi-VN');
    };

    App.formatDate = function (value) {
        if (!value) return '—';
        const d = new Date(value);
        if (isNaN(d.getTime())) return '—';
        return d.toLocaleString('vi-VN', {
            day: '2-digit', month: '2-digit', year: 'numeric',
            hour: '2-digit', minute: '2-digit'
        });
    };

    App.formatTime = function (value) {
        if (!value) return '—';
        const d = new Date(value);
        if (isNaN(d.getTime())) return '—';
        return d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    };

    App.formatDuration = function (totalSeconds) {
        const s = Math.max(0, Math.floor(Number(totalSeconds) || 0));
        const h = Math.floor(s / 3600);
        const m = Math.floor((s % 3600) / 60);
        const sec = s % 60;
        return [h, m, sec].map(n => String(n).padStart(2, '0')).join(':');
    };

    App.escapeHtml = function (value) {
        if (value === null || value === undefined) return '';
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    };

    /* ---------------- Toast ---------------- */
    App.toast = function (message, type) {
        const host = document.getElementById('app-toast-host');
        if (!host) { console.log('[toast]', type, message); return; }
        const id = 'app-toast-' + Date.now() + Math.random().toString(36).slice(2, 7);
        const tone = type === 'success' ? 'success' : (type === 'warning' ? 'warning' : 'danger');
        const icon = tone === 'success' ? 'fa-circle-check' : (tone === 'warning' ? 'fa-triangle-exclamation' : 'fa-circle-xmark');
        const el = document.createElement('div');
        el.id = id;
        el.className = 'app-toast app-toast--' + tone;
        el.setAttribute('role', 'alert');
        el.innerHTML =
            '<i class="fa-solid ' + icon + '"></i>' +
            '<div class="app-toast__msg">' + App.escapeHtml(message) + '</div>' +
            '<button type="button" class="app-toast__close" aria-label="Đóng"><i class="fa-solid fa-xmark"></i></button>';
        host.appendChild(el);
        requestAnimationFrame(function () { el.classList.add('is-visible'); });
        const timer = setTimeout(function () { App.dismissToast(el); }, 4200);
        el.querySelector('.app-toast__close').addEventListener('click', function () {
            clearTimeout(timer);
            App.dismissToast(el);
        });
    };

    App.dismissToast = function (el) {
        if (!el) return;
        el.classList.remove('is-visible');
        el.classList.add('is-leaving');
        setTimeout(function () { el.remove(); }, 260);
    };

    window.App = App;
    window.showToast = App.toast;
    window.formatVnd = App.formatVnd;

    /* ---------------- Confirm dialog ---------------- */
    App.confirm = function (options) {
        const opts = options || {};
        const title = opts.title || 'Xác nhận';
        const message = opts.message || 'Bạn có chắc chắn muốn thực hiện thao tác này?';
        const confirmText = opts.confirmText || 'Đồng ý';
        const cancelText = opts.cancelText || 'Hủy';
        const danger = opts.danger !== false;

        return new Promise(function (resolve) {
            const host = document.getElementById('app-confirm-host');
            if (!host) { resolve(window.confirm(message)); return; }

            const backdrop = document.createElement('div');
            backdrop.className = 'app-confirm';
            backdrop.innerHTML =
                '<div class="app-confirm__box" role="alertdialog" aria-modal="true">' +
                '<div class="app-confirm__icon"><i class="fa-solid fa-' + (danger ? 'triangle-exclamation' : 'circle-question') + '"></i></div>' +
                '<h5 class="app-confirm__title"></h5>' +
                '<p class="app-confirm__msg"></p>' +
                '<div class="app-confirm__actions">' +
                '<button type="button" class="btn btn-soft" data-act="cancel"></button>' +
                '<button type="button" class="btn ' + (danger ? 'btn-ghost-danger' : 'btn-accent') + '" data-act="ok"></button>' +
                '</div></div>';
            backdrop.querySelector('.app-confirm__title').textContent = title;
            backdrop.querySelector('.app-confirm__msg').textContent = message;
            backdrop.querySelector('[data-act="cancel"]').textContent = cancelText;
            backdrop.querySelector('[data-act="ok"]').textContent = confirmText;
            host.appendChild(backdrop);

            requestAnimationFrame(function () { backdrop.classList.add('is-visible'); });

            function close(result) {
                backdrop.classList.remove('is-visible');
                setTimeout(function () { backdrop.remove(); }, 200);
                resolve(result);
            }

            backdrop.querySelector('[data-act="cancel"]').addEventListener('click', function () { close(false); });
            backdrop.querySelector('[data-act="ok"]').addEventListener('click', function () { close(true); });
            backdrop.addEventListener('click', function (e) { if (e.target === backdrop) close(false); });
            document.addEventListener('keydown', function onKey(e) {
                if (e.key === 'Escape') { document.removeEventListener('keydown', onKey); close(false); }
            });
        });
    };

    window.confirmAction = App.confirm;

    /* ---------------- Layout behaviour ---------------- */
    document.addEventListener('DOMContentLoaded', function () {
        const shell = document.getElementById('app-shell');
        const toggle = document.getElementById('sidebarToggle');
        const overlay = document.getElementById('sidebarOverlay');
        const closeBtn = document.getElementById('sidebarClose');

        function isMobile() { return window.innerWidth < 993; }

        function openSidebar() {
            shell?.classList.add('sidebar-open');
            overlay?.classList.add('show');
        }

        function closeSidebar() {
            shell?.classList.remove('sidebar-open');
            overlay?.classList.remove('show');
        }

        toggle?.addEventListener('click', function () {
            if (isMobile()) {
                shell?.classList.contains('sidebar-open') ? closeSidebar() : openSidebar();
            } else {
                shell?.classList.toggle('sidebar-collapsed');
                try {
                    localStorage.setItem('appSidebarCollapsed', shell?.classList.contains('sidebar-collapsed') ? 'true' : 'false');
                } catch (e) { /* storage unavailable */ }
            }
        });

        overlay?.addEventListener('click', closeSidebar);
        closeBtn?.addEventListener('click', closeSidebar);

        try {
            if (localStorage.getItem('appSidebarCollapsed') === 'true' && !isMobile()) {
                shell?.classList.add('sidebar-collapsed');
            }
        } catch (e) { /* storage unavailable */ }

        /* Loading overlay */
        const loading = document.getElementById('loading-overlay');
        document.querySelectorAll('a.nav-link[href]').forEach(function (link) {
            link.addEventListener('click', function () {
                const href = link.getAttribute('href') || '';
                if (href.startsWith('#') || href.startsWith('javascript')) return;
                if (loading) loading.style.display = 'flex';
            });
        });

        document.querySelectorAll('form').forEach(function (form) {
            form.addEventListener('submit', function (e) {
                if (e.defaultPrevented) return;
                if (form.dataset.ajax === 'true' || form.closest('.modal') || form.id === 'createTableForm' || form.id === 'createProductForm') return;
                setTimeout(function () {
                    if (!e.defaultPrevented && loading) loading.style.display = 'flex';
                }, 60);
            });
        });

        /* Auto dismiss bootstrap alerts */
        document.querySelectorAll('.alert[data-auto-dismiss]').forEach(function (alert) {
            setTimeout(function () {
                alert.style.transition = 'opacity .45s';
                alert.style.opacity = '0';
                setTimeout(function () { alert.remove(); }, 450);
            }, 5000);
        });

        /* Clock */
        const clock = document.getElementById('clockDisplay');
        if (clock) {
            const tick = function () {
                clock.textContent = new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
            };
            tick();
            setInterval(tick, 1000);
        }
    });
})();
