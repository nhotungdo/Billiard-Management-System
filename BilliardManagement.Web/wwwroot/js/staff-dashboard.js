(function () {
    'use strict';

    const STATUS_TEXT = { 1: 'Trống bàn', 2: 'Đang chơi', 3: 'Đặt trước', 4: 'Bảo trì' };
    const STATUS_CLASS = { 1: 'status-available', 2: 'status-playing', 3: 'status-reserved', 4: 'status-maintenance' };

    let pendingTableId = null;
    let pendingSessionId = null;
    let pendingOrderSessionId = null;
    let pendingOrderTableId = null;
    const alertedSessions = new Set();

    const startModal = document.getElementById('startSessionModal') ? new bootstrap.Modal(document.getElementById('startSessionModal')) : null;
    const extendModal = document.getElementById('extendSessionModal') ? new bootstrap.Modal(document.getElementById('extendSessionModal')) : null;
    const endModal = document.getElementById('endSessionModal') ? new bootstrap.Modal(document.getElementById('endSessionModal')) : null;
    const orderModal = document.getElementById('orderModal') ? new bootstrap.Modal(document.getElementById('orderModal')) : null;

    function apiSuccess(data) {
        return data && (data.success === true || data.Success === true);
    }

    function apiData(data) {
        return data?.data ?? data?.Data;
    }

    function apiMessage(data) {
        return data?.message ?? data?.Message ?? 'Có lỗi xảy ra';
    }

    function apiFetch(url, method, body) {
        const cfg = window.STAFF_DASHBOARD || {};
        const headers = { 'Content-Type': 'application/json' };
        if (cfg.token) headers['Authorization'] = 'Bearer ' + cfg.token;
        return fetch((cfg.apiBase || 'http://localhost:8080/api/') + url, {
            method: method,
            headers: headers,
            body: body ? JSON.stringify(body) : undefined
        }).then(function (r) {
            const contentType = r.headers.get("content-type");
            if (contentType && contentType.indexOf("application/json") !== -1) {
                return r.json().then(function (j) { 
                    return { ok: r.ok, data: j }; 
                }).catch(function () {
                    return { ok: false, data: { success: false, message: "Lỗi phân tích cú pháp dữ liệu (JSON lỗi)" } };
                });
            } else {
                return r.text().then(function (t) {
                    let errMsg = "Lỗi hệ thống (" + r.status + ")";
                    if (t && t.trim().startsWith("{")) {
                        try {
                            const parsed = JSON.parse(t);
                            errMsg = parsed.message || parsed.Message || errMsg;
                        } catch(e) {}
                    }
                    return { ok: r.ok, data: { success: false, message: errMsg } };
                });
            }
        });
    }

    function showToast(msg, type) {
        const container = document.getElementById('session-alert-container');
        if (!container) return;
        const el = document.createElement('div');
        el.className = 'alert alert-' + (type || 'info') + ' alert-dismissible fade show';
        el.innerHTML = msg + '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>';
        container.appendChild(el);
        setTimeout(function () { el.remove(); }, 6000);
    }

    function parseEndUtc(str) {
        if (!str) return null;
        if (str.endsWith('Z') || str.includes('+') || str.includes('-', 10)) {
            const d = new Date(str);
            return isNaN(d.getTime()) ? null : d;
        }
        const d = new Date(str + 'Z');
        return isNaN(d.getTime()) ? null : d;
    }

    function formatTimer(totalSeconds) {
        const s = Math.max(0, totalSeconds);
        const h = Math.floor(s / 3600);
        const m = Math.floor((s % 3600) / 60);
        const sec = s % 60;
        return [h, m, sec].map(function (v) { return v.toString().padStart(2, '0'); }).join(':');
    }

    function getTimerLevel(remainingSeconds) {
        if (remainingSeconds <= 0) return 'timer-expired';
        if (remainingSeconds < 15 * 60) return 'timer-warning';
        return 'timer-ok';
    }

    function getRemainingSeconds(card, timerEl) {
        const endStr = timerEl?.getAttribute('data-end') || card?.dataset.endTime;
        const end = parseEndUtc(endStr);
        if (end) return Math.max(0, Math.floor((end - new Date()) / 1000));
        const rem = parseInt(card?.dataset.remainingSeconds || timerEl?.getAttribute('data-remaining') || '0', 10);
        return isNaN(rem) ? 0 : rem;
    }

    function formatMoney(n) {
        return Number(n || 0).toLocaleString('vi-VN') + ' đ';
    }

    function renderOrderLines(tableId, lines) {
        const container = document.getElementById('orders-list-' + tableId);
        if (!container) return;
        if (!lines || !lines.length) {
            container.className = 'tsc-orders mt-2 text-muted small';
            container.innerHTML = 'Chưa có order';
            return;
        }
        container.className = 'tsc-orders mt-2';
        let html = '<div class="tsc-label mb-1">Đồ uống / snack</div><ul class="tsc-order-items list-unstyled mb-0 small">';
        lines.forEach(function (line) {
            const name = line.productName || line.ProductName;
            const qty = line.quantity || line.Quantity;
            html += '<li>' + name + ' <span class="text-muted">x' + qty + '</span></li>';
        });
        html += '</ul>';
        container.innerHTML = html;
    }

    function updateCardFromSession(tableId, payload) {
        const card = document.getElementById('table-card-' + tableId);
        if (!card) return;

        const sessionId = payload.sessionId || payload.SessionId;
        let endTime = payload.endTime || payload.EndTime;
        const tableStatus = payload.tableStatus !== undefined ? payload.tableStatus : payload.TableStatus;
        const durationHours = payload.durationHours ?? payload.DurationHours;
        const totalPrice = payload.totalPrice ?? payload.TotalPrice;
        const ordersTotal = payload.ordersTotal ?? payload.OrdersTotal;
        const currentTotal = payload.currentTotal ?? payload.CurrentTotal;
        const isExpired = payload.isExpired || payload.IsExpired;
        const remainingSeconds = payload.remainingSeconds ?? payload.RemainingSeconds;
        const orderLines = payload.orderLines || payload.OrderLines;

        if (sessionId) card.dataset.sessionId = sessionId;
        if (durationHours) card.dataset.durationHours = durationHours;

        if (endTime) {
            if (typeof endTime === 'string' && !endTime.endsWith('Z') && endTime.indexOf('+') < 0)
                endTime = endTime.replace(/\.\d+$/, '') + 'Z';
            card.dataset.endTime = endTime;
        }

        if (tableStatus !== undefined) {
            card.dataset.status = tableStatus;
            card.className = 'table-session-card ' + (STATUS_CLASS[tableStatus] || '');
            const badge = document.getElementById('status-badge-' + tableId);
            if (badge) {
                badge.textContent = STATUS_TEXT[tableStatus] || '—';
                badge.className = 'tsc-status-badge badge bg-' + (tableStatus === 2 ? 'danger' : tableStatus === 1 ? 'success' : tableStatus === 3 ? 'warning' : 'secondary');
            }
        }

        const durationEl = document.getElementById('duration-' + tableId);
        if (durationEl && durationHours) durationEl.textContent = durationHours + ' giờ';

        const timerEl = document.getElementById('timer-' + tableId);
        if (timerEl) {
            if (endTime) timerEl.setAttribute('data-end', endTime);
            let rem = remainingSeconds;
            if (rem === undefined || rem === null) rem = getRemainingSeconds(card, timerEl);
            timerEl.setAttribute('data-remaining', rem);
            card.dataset.remainingSeconds = rem;
            timerEl.textContent = formatTimer(rem);
            timerEl.className = 'session-timer ' + getTimerLevel(rem);
            if (rem <= 0 || isExpired) timerEl.classList.add('timer-blink');
            else timerEl.classList.remove('timer-blink');
        }

        const tablePriceEl = document.getElementById('table-price-' + tableId);
        const ordersPriceEl = document.getElementById('orders-price-' + tableId);
        const priceEl = document.getElementById('price-' + tableId);
        if (tablePriceEl && totalPrice !== undefined) tablePriceEl.textContent = formatMoney(totalPrice);
        if (ordersPriceEl && ordersTotal !== undefined) ordersPriceEl.textContent = formatMoney(ordersTotal);
        if (priceEl && currentTotal !== undefined) priceEl.textContent = formatMoney(currentTotal);

        if (orderLines) renderOrderLines(tableId, orderLines);

        if (isExpired && sessionId && !alertedSessions.has(sessionId)) {
            alertedSessions.add(sessionId);
            showToast('<strong>Bàn đã hết giờ chơi!</strong> ' + (payload.tableName || payload.TableName || ''), 'danger');
        }

        recalcKpis();
    }

    function tickAllTimers() {
        document.querySelectorAll('.table-session-card').forEach(function (card) {
            const tableId = card.dataset.tableId;
            const timerEl = document.getElementById('timer-' + tableId);
            if (!timerEl || !card.dataset.sessionId) return;

            const rem = getRemainingSeconds(card, timerEl);
            card.dataset.remainingSeconds = rem;
            timerEl.setAttribute('data-remaining', rem);
            timerEl.textContent = formatTimer(rem);
            timerEl.className = 'session-timer ' + getTimerLevel(rem);

            if (rem <= 0) {
                timerEl.classList.add('timer-blink');
                const sid = card.dataset.sessionId;
                if (sid && !alertedSessions.has(sid)) {
                    alertedSessions.add(sid);
                    const name = card.querySelector('.tsc-name')?.textContent || 'Bàn';
                    showToast('<strong>Bàn sắp hết / đã hết giờ:</strong> ' + name, 'warning');
                }
            } else {
                timerEl.classList.remove('timer-blink');
            }
        });
    }

    function recalcKpis() {
        const cards = document.querySelectorAll('.table-session-card');
        let available = 0, playing = 0;
        cards.forEach(function (c) {
            const s = parseInt(c.dataset.status, 10);
            if (s === 1) available++;
            if (s === 2) playing++;
        });
        const kpiPlaying = document.getElementById('kpi-playing');
        const kpiAvailable = document.getElementById('kpi-available');
        if (kpiPlaying) kpiPlaying.textContent = playing;
        if (kpiAvailable) kpiAvailable.textContent = available;
    }

    document.querySelectorAll('.btn-start-session').forEach(function (btn) {
        btn.addEventListener('click', function () {
            pendingTableId = btn.getAttribute('data-table-id');
            document.getElementById('startModalTableName').textContent = btn.getAttribute('data-table-name') || '';
            if (startModal) startModal.show();
        });
    });

    document.querySelectorAll('.btn-hour-option').forEach(function (btn) {
        btn.addEventListener('click', function () {
            doStartSession(pendingTableId, parseInt(btn.getAttribute('data-hours'), 10));
        });
    });

    const btnCustomStart = document.getElementById('btnCustomStart');
    if (btnCustomStart) {
        btnCustomStart.addEventListener('click', function () {
            const hours = parseInt(document.getElementById('customHoursInput')?.value || '1', 10);
            doStartSession(pendingTableId, hours);
        });
    }

    function doStartSession(tableId, hours) {
        if (!tableId) return;
        apiFetch('table-sessions/start', 'POST', { tableId: tableId, durationHours: hours })
            .then(function (res) {
                if (!res.ok || !apiSuccess(res.data)) {
                    showToast(apiMessage(res.data) || 'Không thể bắt đầu phiên', 'danger');
                    return;
                }
                if (startModal) startModal.hide();
                showToast('Đã bắt đầu phiên ' + hours + ' giờ', 'success');
                location.reload();
            })
            .catch(function (e) { showToast(e.message, 'danger'); });
    }

    document.querySelectorAll('.btn-extend-session').forEach(function (btn) {
        btn.addEventListener('click', function () {
            pendingSessionId = btn.getAttribute('data-session-id');
            document.getElementById('extendModalTableName').textContent = btn.getAttribute('data-table-name') || '';
            if (extendModal) extendModal.show();
        });
    });

    document.querySelectorAll('.btn-extend-option').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const minutes = parseInt(btn.getAttribute('data-minutes'), 10);
            if (!pendingSessionId) return;
            apiFetch('table-sessions/extend/' + pendingSessionId, 'POST', { additionalMinutes: minutes })
                .then(function (res) {
                    if (!res.ok || !apiSuccess(res.data)) {
                        showToast(apiMessage(res.data), 'danger');
                        return;
                    }
                    if (extendModal) extendModal.hide();
                    showToast('Đã gia hạn thêm ' + minutes + ' phút', 'success');
                    const s = apiData(res.data);
                    if (s) updateCardFromSession(s.tableId || s.TableId, s);
                });
        });
    });

    document.querySelectorAll('.btn-end-session').forEach(function (btn) {
        btn.addEventListener('click', function () {
            pendingSessionId = btn.getAttribute('data-session-id');
            document.getElementById('endModalTableName').textContent = btn.getAttribute('data-table-name') || '';
            if (endModal) endModal.show();
        });
    });

    document.getElementById('btnConfirmEnd')?.addEventListener('click', function () {
        if (!pendingSessionId) return;
        const discount = parseFloat(document.getElementById('endDiscountInput')?.value || '0');
        const paymentMethod = parseInt(document.getElementById('endPaymentSelect')?.value || '0', 10);
        apiFetch('table-sessions/end/' + pendingSessionId, 'POST', { discount: discount, paymentMethod: paymentMethod })
            .then(function (res) {
                if (!res.ok || !apiSuccess(res.data)) {
                    showToast(apiMessage(res.data), 'danger');
                    return;
                }
                if (endModal) endModal.hide();
                showToast('Đã kết thúc phiên và tạo hóa đơn', 'success');
                location.reload();
            });
    });

    document.querySelectorAll('.btn-set-status').forEach(function (a) {
        a.addEventListener('click', function (e) {
            e.preventDefault();
            const tableId = a.getAttribute('data-table-id');
            const status = a.getAttribute('data-status');
            apiFetch('tables/' + tableId + '/status', 'PUT', { status: status })
                .then(function (res) {
                    if (!res.ok || !apiSuccess(res.data)) {
                        showToast(apiMessage(res.data), 'danger');
                        return;
                    }
                    showToast('Đã cập nhật trạng thái bàn', 'success');
                    location.reload();
                });
        });
    });

    document.querySelectorAll('.btn-order-drinks').forEach(function (btn) {
        btn.addEventListener('click', function () {
            pendingOrderSessionId = btn.getAttribute('data-session-id');
            pendingOrderTableId = btn.getAttribute('data-table-id');
            document.getElementById('orderModalTableName').textContent = btn.getAttribute('data-table-name') || '';
            document.querySelectorAll('#orderProductList .order-qty').forEach(function (i) { i.value = 0; });
            if (orderModal) orderModal.show();
        });
    });

    document.querySelectorAll('#orderProductList .btn-qty-plus').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const input = btn.closest('.order-product-item')?.querySelector('.order-qty');
            if (input) input.value = Math.min(parseInt(input.max || '99', 10), parseInt(input.value || '0', 10) + 1);
        });
    });

    document.querySelectorAll('#orderProductList .btn-qty-minus').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const input = btn.closest('.order-product-item')?.querySelector('.order-qty');
            if (input) input.value = Math.max(0, parseInt(input.value || '0', 10) - 1);
        });
    });

    document.getElementById('btnSubmitOrder')?.addEventListener('click', function () {
        if (!pendingOrderSessionId) {
            showToast('Không có phiên chơi', 'danger');
            return;
        }
        const items = [];
        document.querySelectorAll('#orderProductList .order-product-item').forEach(function (el) {
            const qty = parseInt(el.querySelector('.order-qty')?.value || '0', 10);
            if (qty > 0) {
                items.push({ productId: el.getAttribute('data-product-id'), quantity: qty });
            }
        });
        if (!items.length) {
            showToast('Chọn ít nhất một món', 'warning');
            return;
        }
        apiFetch('orders', 'POST', { tableSessionId: pendingOrderSessionId, items: items })
            .then(function (res) {
                if (!res.ok || !apiSuccess(res.data)) {
                    showToast(apiMessage(res.data) || 'Không thể order', 'danger');
                    return;
                }
                if (orderModal) orderModal.hide();
                showToast('Order thành công', 'success');
                return apiFetch('table-sessions/active', 'GET');
            })
            .then(function (res) {
                if (!res || !apiSuccess(res.data)) return;
                const sessions = apiData(res.data) || [];
                const s = sessions.find(function (x) {
                    return (x.id || x.Id) === pendingOrderSessionId;
                });
                if (s && pendingOrderTableId) updateCardFromSession(pendingOrderTableId, s);
            })
            .catch(function (e) { showToast(e.message, 'danger'); });
    });

    tickAllTimers();
    setInterval(tickAllTimers, 1000);

    if (typeof connectTableStatusHub === 'function') {
        connectTableStatusHub({
            onStatusChanged: function (payload) {
                const tableId = payload.tableId || payload.TableId;
                const status = payload.status !== undefined ? payload.status : payload.Status;
                updateCardFromSession(tableId, { tableStatus: status });
            },
            onSessionUpdated: function (payload) {
                const tableId = payload.tableId || payload.TableId;
                updateCardFromSession(tableId, payload);
            }
        });
    }
})();
