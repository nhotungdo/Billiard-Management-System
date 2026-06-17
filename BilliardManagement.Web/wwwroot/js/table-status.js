// Shared table status helpers — enum values match TableStatus (1-4)
window.TableStatusConfig = {
    1: { key: 'Available', text: 'Trống bàn', badge: 'success', icon: 'circle-check' },
    2: { key: 'Playing', text: 'Đang chơi', badge: 'primary', icon: 'circle-play' },
    3: { key: 'Reserved', text: 'Đặt trước', badge: 'warning', icon: 'clock' },
    4: { key: 'Maintenance', text: 'Bảo trì', badge: 'danger', icon: 'wrench' }
};

window.getTableStatusConfig = function (status) {
    return window.TableStatusConfig[status] || { key: 'Unknown', text: 'Không xác định', badge: 'dark', icon: 'question' };
};

window.renderTableStatusBadge = function (status, elementId) {
    const cfg = window.getTableStatusConfig(status);
    const el = document.getElementById(elementId);
    if (!el) return;
    el.className = 'badge bg-' + cfg.badge;
    el.innerHTML = '<i class="fa-solid fa-' + cfg.icon + ' me-1"></i>' + cfg.text;
};

window.updateTableStatusStats = function (tables) {
    const counts = { available: 0, playing: 0, reserved: 0, maintenance: 0 };
    tables.forEach(function (t) {
        const s = t.status !== undefined ? t.status : t.Status;
        if (s === 1) counts.available++;
        else if (s === 2) counts.playing++;
        else if (s === 3) counts.reserved++;
        else if (s === 4) counts.maintenance++;
    });
    ['stat-available', 'stat-playing', 'stat-reserved', 'stat-maintenance'].forEach(function (id) {
        const el = document.getElementById(id);
        if (!el) return;
    });
    const map = {
        'stat-available': counts.available,
        'stat-playing': counts.playing,
        'stat-reserved': counts.reserved,
        'stat-maintenance': counts.maintenance
    };
    Object.keys(map).forEach(function (id) {
        const el = document.getElementById(id);
        if (el) el.textContent = map[id];
    });
};

window.connectTableStatusHub = function (options) {
    const hubUrl = (document.querySelector('meta[name="hub-base-url"]')?.content || 'http://localhost:8080') + '/hubs/table';
    if (!window.signalR) {
        console.warn('SignalR library not loaded');
        return null;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl)
        .withAutomaticReconnect()
        .build();

    connection.on('TableStatusChanged', function (payload) {
        if (options && typeof options.onStatusChanged === 'function') {
            options.onStatusChanged(payload);
        }
    });

    connection.on('ReceiveTableUpdate', function (message) {
        if (options && typeof options.onMessage === 'function') {
            options.onMessage(message);
        }
    });

    connection.on('SessionUpdated', function (payload) {
        if (options && typeof options.onSessionUpdated === 'function') {
            options.onSessionUpdated(payload);
        }
    });

    connection.on('TableCreated', function (payload) {
        if (options && typeof options.onTableCreated === 'function') {
            options.onTableCreated(payload);
        }
    });

    connection.start()
        .then(function () { console.log('TableHub connected'); })
        .catch(function (err) { console.error('TableHub connection failed', err); });

    return connection;
};
