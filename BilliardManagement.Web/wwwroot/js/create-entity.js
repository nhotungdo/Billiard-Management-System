// Shared create table / product via API + toast + loading
(function () {
    const API_BASE = document.querySelector('meta[name="api-base-url"]')?.content || 'http://localhost:8080/api/';
    const API_TOKEN = document.querySelector('meta[name="api-token"]')?.content || '';
    const HUB_BASE = document.querySelector('meta[name="hub-base-url"]')?.content || 'http://localhost:8080';
    const API_ORIGIN = document.querySelector('meta[name="api-origin"]')?.content || 'http://localhost:8080';

    window.showAppToast = function (message, type) {
        const container = document.getElementById('app-toast-container') || document.getElementById('table-toast-container');
        if (!container) return;
        const id = 'toast-' + Date.now();
        const bg = type === 'success' ? 'text-bg-success' : 'text-bg-danger';
        container.insertAdjacentHTML('beforeend',
            `<div id="${id}" class="toast align-items-center ${bg} border-0" role="alert">
                <div class="d-flex"><div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div></div>`);
        const el = document.getElementById(id);
        const toast = new bootstrap.Toast(el, { delay: 3500 });
        toast.show();
        el.addEventListener('hidden.bs.toast', () => el.remove());
    };

    window.resolveImageUrl = function (url) {
        if (!url) return '';
        if (url.startsWith('http')) return url;
        return API_ORIGIN + (url.startsWith('/') ? url : '/' + url);
    };

    window.createTableViaApi = async function (formData) {
        const res = await fetch(API_BASE + 'tables', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + API_TOKEN
            },
            body: JSON.stringify({
                tableName: formData.tableName,
                tableType: formData.tableType,
                pricePerHour: parseFloat(formData.pricePerHour),
                status: formData.status,
                description: formData.description || ''
            })
        });
        
        let json;
        const contentType = res.headers.get("content-type");
        if (contentType && contentType.indexOf("application/json") !== -1) {
            json = await res.json();
        } else {
            const text = await res.text();
            throw new Error(text || `Lỗi hệ thống (${res.status})`);
        }

        if (!res.ok || !(json.success ?? json.Success))
            throw new Error(json.message || json.Message || 'Không thể tạo dữ liệu');
        return json.data || json.Data;
    };

    window.createProductViaApi = async function (formEl) {
        const fd = new FormData(formEl);
        const res = await fetch(API_BASE + 'products', {
            method: 'POST',
            headers: { 'Authorization': 'Bearer ' + API_TOKEN },
            body: fd
        });

        let json;
        const contentType = res.headers.get("content-type");
        if (contentType && contentType.indexOf("application/json") !== -1) {
            json = await res.json();
        } else {
            const text = await res.text();
            throw new Error(text || `Lỗi hệ thống (${res.status})`);
        }

        if (!res.ok || !(json.success ?? json.Success))
            throw new Error(json.message || json.Message || 'Không thể tạo dữ liệu');
        return json.data || json.Data;
    };

    window.bindCreateTableForm = function (options) {
        const form = document.getElementById(options.formId || 'createTableForm');
        if (!form) return;
        form.addEventListener('submit', async function (e) {
            e.preventDefault();
            const btn = form.querySelector('[type="submit"]');
            const spinner = document.getElementById(options.spinnerId || 'createTableSpinner');
            btn.disabled = true;
            if (spinner) spinner.classList.remove('d-none');
            try {
                const data = {
                    tableName: form.tableName.value.trim(),
                    tableType: form.tableType.value,
                    pricePerHour: form.pricePerHour.value,
                    status: form.status.value,
                    description: form.description?.value?.trim() || ''
                };
                const table = await window.createTableViaApi(data);
                window.showAppToast('Tạo bàn mới thành công', 'success');
                bootstrap.Modal.getInstance(document.getElementById(options.modalId || 'createTableModal'))?.hide();
                form.reset();
                if (form.status) form.status.value = 'Available';
                if (typeof options.onCreated === 'function') options.onCreated(table);
            } catch (err) {
                window.showAppToast(err.message || 'Không thể tạo dữ liệu', 'error');
            } finally {
                btn.disabled = false;
                if (spinner) spinner.classList.add('d-none');
            }
        });
    };

    window.bindCreateProductForm = function (options) {
        const form = document.getElementById(options.formId || 'createProductForm');
        if (!form) return;
        
        const nameInput = form.querySelector('[name="name"]');
        const submitBtn = form.querySelector('[type="submit"]');
        const nameError = document.getElementById('productNameError');

        if (nameInput) {
            nameInput.addEventListener('blur', async () => {
                const name = nameInput.value.trim();
                if (!name) return;
                try {
                    const res = await fetch(API_BASE + 'products/check-name?name=' + encodeURIComponent(name), {
                        headers: { 'Authorization': 'Bearer ' + API_TOKEN }
                    });
                    
                    const contentType = res.headers.get("content-type");
                    if (!res.ok || !contentType || contentType.indexOf("application/json") === -1) {
                        return;
                    }
                    const json = await res.json();
                    if (json.isDuplicate) {
                        nameInput.classList.add('is-invalid');
                        if (nameError) nameError.classList.remove('d-none');
                        submitBtn.disabled = true;
                        window.showAppToast('Tên sản phẩm đã tồn tại', 'error');
                    } else {
                        nameInput.classList.remove('is-invalid');
                        if (nameError) nameError.classList.add('d-none');
                        submitBtn.disabled = false;
                    }
                } catch (e) {
                    console.warn(e);
                }
            });

            nameInput.addEventListener('input', () => {
                nameInput.classList.remove('is-invalid');
                if (nameError) nameError.classList.add('d-none');
                submitBtn.disabled = false;
            });
        }

        form.addEventListener('submit', async function (e) {
            e.preventDefault();
            if (nameInput && nameInput.classList.contains('is-invalid')) {
                window.showAppToast('Vui lòng sửa các lỗi trước khi lưu', 'error');
                return;
            }
            const btn = form.querySelector('[type="submit"]');
            const spinner = document.getElementById(options.spinnerId || 'createProductSpinner');
            btn.disabled = true;
            if (spinner) spinner.classList.remove('d-none');
            try {
                const product = await window.createProductViaApi(form);
                window.showAppToast('Tạo thành công', 'success');
                bootstrap.Modal.getInstance(document.getElementById(options.modalId || 'createProductModal'))?.hide();
                form.reset();
                if (form.isAvailable) form.isAvailable.checked = true;
                if (typeof options.onCreated === 'function') options.onCreated(product);
            } catch (err) {
                window.showAppToast(err.message || 'Không thể tạo dữ liệu', 'error');
            } finally {
                btn.disabled = false;
                if (spinner) spinner.classList.add('d-none');
            }
        });
    };

    window.connectProductHub = function (options) {
        if (!window.signalR) return null;
        const conn = new signalR.HubConnectionBuilder()
            .withUrl(HUB_BASE + '/hubs/product')
            .withAutomaticReconnect()
            .build();
        conn.on('ProductCreated', function (payload) {
            if (options && typeof options.onProductCreated === 'function')
                options.onProductCreated(payload);
        });
        conn.on('ReceiveProductUpdate', function (msg) {
            if (options && typeof options.onMessage === 'function')
                options.onMessage(msg);
        });
        conn.start().catch(function (e) { console.warn('ProductHub:', e); });
        return conn;
    };

    window.appendTableRow = function (table, tbodyId) {
        const tbody = document.getElementById(tbodyId || 'tables-tbody');
        if (!tbody) return;
        const empty = document.getElementById('empty-row');
        if (empty) empty.remove();
        const status = table.status !== undefined ? table.status : table.Status;
        const cfg = window.getTableStatusConfig(status);
        const id = table.id || table.Id;
        const tr = document.createElement('tr');
        tr.id = 'row-' + id;
        tr.dataset.tableId = id;
        tr.dataset.status = status;
        tr.innerHTML = `
            <td class="text-muted row-index">+</td>
            <td><strong class="table-name">${table.tableName || table.TableName}</strong></td>
            <td><span class="badge bg-secondary bg-opacity-75">${table.tableType || table.TableType}</span></td>
            <td class="fw-500">${(table.pricePerHour || table.PricePerHour || 0).toLocaleString('vi-VN')} đ/giờ</td>
            <td><span class="badge bg-${cfg.badge}" id="status-badge-${id}"><i class="fa-solid fa-${cfg.icon} me-1"></i>${cfg.text}</span></td>
            <td class="text-center text-muted small">Mới tạo</td>`;
        tbody.prepend(tr);
        const countEl = document.getElementById('table-count');
        if (countEl) countEl.textContent = tbody.querySelectorAll('tr[id^="row-"]').length;
    };

    window.appendProductCard = function (product, gridId) {
        const grid = document.getElementById(gridId || 'products-grid');
        if (!grid) return;
        const empty = document.getElementById('products-empty');
        if (empty) empty.remove();
        const id = product.id || product.Id;
        const img = window.resolveImageUrl(product.imageUrl || product.ImageUrl);
        const available = product.isAvailable ?? product.IsAvailable;
        const col = document.createElement('div');
        col.className = 'col-6 col-md-4 col-lg-3';
        col.id = 'product-card-' + id;
        col.innerHTML = `
            <div class="card h-100 shadow-sm product-card">
                <div class="product-img-wrap">
                    ${img ? `<img src="${img}" class="card-img-top product-img" alt="">` :
                `<div class="product-img-placeholder"><i class="fa-solid fa-box-open"></i></div>`}
                </div>
                <div class="card-body">
                    <h6 class="card-title mb-1">${product.name || product.Name}</h6>
                    <p class="text-primary fw-bold mb-1">${(product.price || product.Price || 0).toLocaleString('vi-VN')} đ</p>
                    <span class="badge ${available ? 'bg-success' : 'bg-secondary'}">${available ? 'Đang bán' : 'Ngừng bán'}</span>
                    <div class="small text-muted mt-1">${product.category || product.Category || ''}</div>
                </div>
            </div>`;
        grid.prepend(col);
        const countEl = document.getElementById('product-count');
        if (countEl) countEl.textContent = grid.querySelectorAll('[id^="product-card-"]').length;
    };
})();
