// Admin JavaScript - Billiard Management System

document.addEventListener('DOMContentLoaded', function () {

    // ---- Sidebar Toggle ----
    const wrapper = document.getElementById('admin-wrapper');
    const toggleBtn = document.getElementById('sidebarToggle');
    const overlay = document.getElementById('sidebarOverlay');
    const closeBtn = document.getElementById('sidebarClose');

    if (toggleBtn) {
        toggleBtn.addEventListener('click', function () {
            if (window.innerWidth < 768) {
                const sidebar = document.getElementById('admin-sidebar');
                sidebar?.classList.toggle('open');
                overlay?.classList.toggle('open');
            } else {
                wrapper?.classList.toggle('sidebar-collapsed');
                localStorage.setItem('adminSidebarCollapsed', wrapper?.classList.contains('sidebar-collapsed'));
            }
        });
    }

    if (overlay) {
        overlay.addEventListener('click', function () {
            document.getElementById('admin-sidebar')?.classList.remove('open');
            overlay.classList.remove('open');
        });
    }

    if (closeBtn) {
        closeBtn.addEventListener('click', function () {
            document.getElementById('admin-sidebar')?.classList.remove('open');
            overlay?.classList.remove('open');
        });
    }

    // Restore sidebar state
    const collapsed = localStorage.getItem('adminSidebarCollapsed') === 'true';
    if (collapsed && window.innerWidth >= 768) {
        wrapper?.classList.add('sidebar-collapsed');
    }

    // ---- Loading Overlay on nav/form ----
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
        form.addEventListener('submit', function (e) {
            if (e.defaultPrevented) {
                return;
            }
            if (form.closest('.modal') || form.getAttribute('data-ajax') === 'true' || form.id === 'createTableForm' || form.id === 'createProductForm') {
                return;
            }
            setTimeout(function () {
                if (!e.defaultPrevented) {
                    if (loadingOverlay) loadingOverlay.style.display = 'flex';
                }
            }, 50);
        });
    });

    // ---- Auto-dismiss alerts ----
    document.querySelectorAll('.alert').forEach(function (alert) {
        setTimeout(function () {
            alert.style.transition = 'opacity 0.5s';
            alert.style.opacity = '0';
            setTimeout(function () { alert.remove(); }, 500);
        }, 4000);
    });
});
