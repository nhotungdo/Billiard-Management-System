// Staff JavaScript - Billiard Management System

document.addEventListener('DOMContentLoaded', function () {

    // ---- Sidebar Toggle ----
    const wrapper = document.getElementById('staff-wrapper');
    const toggleBtn = document.getElementById('sidebarToggle');
    const overlay = document.getElementById('sidebarOverlay');
    const closeBtn = document.getElementById('sidebarClose');

    if (toggleBtn) {
        toggleBtn.addEventListener('click', function () {
            if (window.innerWidth < 768) {
                const sidebar = document.getElementById('staff-sidebar');
                sidebar?.classList.toggle('open');
                overlay?.classList.toggle('open');
            } else {
                wrapper?.classList.toggle('sidebar-collapsed');
                localStorage.setItem('staffSidebarCollapsed', wrapper?.classList.contains('sidebar-collapsed'));
            }
        });
    }

    if (overlay) {
        overlay.addEventListener('click', function () {
            document.getElementById('staff-sidebar')?.classList.remove('open');
            overlay.classList.remove('open');
        });
    }

    if (closeBtn) {
        closeBtn.addEventListener('click', function () {
            document.getElementById('staff-sidebar')?.classList.remove('open');
            overlay?.classList.remove('open');
        });
    }

    // Restore sidebar state
    const collapsed = localStorage.getItem('staffSidebarCollapsed') === 'true';
    if (collapsed && window.innerWidth >= 768) {
        wrapper?.classList.add('sidebar-collapsed');
    }

    // ---- Loading Overlay ----
    const loadingOverlay = document.getElementById('loading-overlay');

    document.querySelectorAll('.nav-link[href]').forEach(function (link) {
        link.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            if (href && !href.startsWith('#') && !href.startsWith('javascript')) {
                if (loadingOverlay) loadingOverlay.style.display = 'flex';
            }
        });
    });

    document.querySelectorAll('form').forEach(function (form) {
        form.addEventListener('submit', function () {
            if (loadingOverlay) loadingOverlay.style.display = 'flex';
        });
    });

    // ---- Realtime Clock ----
    function updateClock() {
        const el = document.getElementById('clockDisplay');
        if (el) {
            const now = new Date();
            el.textContent = now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
        }
    }
    updateClock();
    setInterval(updateClock, 1000);

    // ---- Auto-dismiss alerts ----
    document.querySelectorAll('.alert').forEach(function (alert) {
        setTimeout(function () {
            alert.style.transition = 'opacity 0.5s';
            alert.style.opacity = '0';
            setTimeout(function () { alert.remove(); }, 500);
        }, 4000);
    });
});
