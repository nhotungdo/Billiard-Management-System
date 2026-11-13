// Run through verify-multitenant.ps1: it supplies a disposable database/API/Web.
import assert from 'node:assert/strict';
import { setTimeout as delay } from 'node:timers/promises';
import { verifyAudit } from './verify-audit.mjs';
const base = process.env.VERIFY_API;
const web = process.env.VERIFY_WEB;
assert(base && web, 'Run scripts/verify-multitenant.ps1 to isolate test data');
const sockets = [];
async function request(path, token, method = 'GET', body, store, expected = 200) {
    const headers = {};
    if (token) headers.Authorization = `Bearer ${token}`;
    if (store) headers['X-Store-Id'] = store;
    if (body !== undefined && !(body instanceof FormData)) headers['Content-Type'] = 'application/json';
    const response = await fetch(base + path, { method, headers,
        body: body === undefined ? undefined : body instanceof FormData ? body : JSON.stringify(body) });
    const text = await response.text();
    assert.equal(response.status, expected, `${method} ${path}: ${text}`);
    if (expected !== 200) return;
    const result = JSON.parse(text);
    assert.equal(result.success, true, text);
    return result.data;
}
const login = (username, password = '123456') => request('/api/auth/login', null, 'POST', { username, password });
const tableBody = name => ({ tableName: name, tableType: 'Pool 8 Ball', pricePerHour: 60000, status: 'Available' });
async function hub(token) {
    const response = await fetch(`${base}/hubs/table/negotiate?negotiateVersion=1`, {
        method: 'POST', headers: { Authorization: `Bearer ${token}` } });
    assert.equal(response.status, 200);
    const negotiation = await response.json();
    const ws = new WebSocket(`${base.replace('http', 'ws')}/hubs/table?id=${encodeURIComponent(negotiation.connectionToken)}&access_token=${encodeURIComponent(token)}`);
    sockets.push(ws);
    const events = [];
    await new Promise((resolve, reject) => {
        const timeout = setTimeout(() => reject(new Error('SignalR handshake timeout')), 10000);
        ws.addEventListener('open', () => ws.send(JSON.stringify({ protocol: 'json', version: 1 }) + '\x1e'));
        ws.addEventListener('error', reject);
        let buffer = '';
        ws.addEventListener('message', event => {
            buffer += String(event.data);
            const frames = buffer.split('\x1e'); buffer = frames.pop();
            for (const frame of frames.filter(Boolean)) {
                const message = JSON.parse(frame);
                if (!message.type) { clearTimeout(timeout); assert(!message.error, message.error); resolve(); }
                if (message.type === 1) events.push(message);
            }
        });
    });
    return events;
}
class Browser {
    cookies = new Map();
    async get(path, body) {
        const response = await fetch(web + path, { redirect: 'manual', method: body ? 'POST' : 'GET',
            headers: { Cookie: [...this.cookies].map(([k,v]) => `${k}=${v}`).join('; ') }, body });
        for (const cookie of response.headers.getSetCookie()) {
            const pair = cookie.split(';')[0]; const index = pair.indexOf('=');
            this.cookies.set(pair.slice(0, index), pair.slice(index + 1));
        }
        return { status: response.status, location: response.headers.get('location'), html: await response.text() };
    }
    async form(path, fields) {
        const page = await this.get(path.split('?')[0]);
        const token = page.html.match(/name="__RequestVerificationToken"[^>]*value="([^"]+)"/)?.[1];
        assert(token, `Missing antiforgery token: ${path}`);
        return this.get(path, new URLSearchParams({ ...fields, __RequestVerificationToken: token }));
    }
    async login(username, password = '123456') {
        const result = await this.form('/Auth/Login', { 'LoginData.Username': username, 'LoginData.Password': password });
        assert.equal(result.status, 302, result.html);
    }
}
try {
    const superUser = await login('superadmin', process.env.Seed__SuperAdminPassword);
    const token = superUser.token;
    await request('/api/categories', token, 'POST', { categoryName: 'No store' }, null, 400);
    await request('/api/categories', token, 'POST', { categoryName: 'Bad store' }, 'invalid', 400);
    await request('/api/auth/register', null, 'POST', {}, null, 401);
    const stores = [];
    for (let i = 0; i < 12; i++) {
        const store = await request('/api/stores', token, 'POST', { name: `Verify store ${i}` });
        const user = await request('/api/users', token, 'POST', { fullName: `Admin ${i}`, username: `verify_admin_${i}`, password: '123456', role: 1, storeId: store.id });
        const auth = await login(user.username);
        const tables = [];
        for (let j = 0; j < 10; j++) tables.push(await request('/api/tables', token, 'POST', tableBody(`Shared table ${j}`), store.id));
        stores.push({ ...store, auth, tables });
    }
    const [a,b] = stores;
    const createCustomer = s => request('/api/customers', token, 'POST', { fullName: 'Same phone customer', phoneNumber: '0912345678' }, s.id);
    const ca = await createCustomer(a); const cb = await createCustomer(b);
    assert.notEqual(ca.id, cb.id);
    await request('/api/customers', token, 'POST', { fullName: 'Duplicate', phoneNumber: '0912345678' }, a.id, 400);
    await request(`/api/customers/${ca.id}`, b.auth.token, 'GET', undefined, null, 404);
    await request(`/api/tables/${a.tables[0].id}`, b.auth.token, 'GET', undefined, null, 404);
    await request(`/api/tables/${a.tables[0].id}`, token, 'GET', undefined, b.id, 404);
    assert.equal((await request('/api/tables', a.auth.token)).totalCount, 10);
    await request(`/api/users/${superUser.user.id}`, a.auth.token, 'GET', undefined, null, 404);
    assert((await request('/api/users', a.auth.token)).every(u => u.storeId === a.id));
    const spoof = await request('/api/users', a.auth.token, 'POST', { fullName: 'Spoof test', username: 'verify_staff', password: '123456', role: 2, storeId: b.id });
    assert.equal(spoof.storeId, a.id);
    await request(`/api/users/${spoof.id}/store`, a.auth.token, 'PUT', b.id, null, 403);
    const oldAuth = await login(spoof.username);
    await request(`/api/users/${spoof.id}/store`, token, 'PUT', b.id);
    await request('/api/tables', oldAuth.token, 'GET', undefined, null, 401);
    const moved = await login(spoof.username);
    assert.equal(moved.user.storeId, b.id); assert.equal(moved.user.role, 1);
    console.log('PASS: 12 stores, per-store table names/customer phone uniqueness, isolation, spoofing and Admin reassignment/token revocation');

    for (const s of [a,b]) {
        s.category = await request('/api/categories', token, 'POST', { categoryName: 'Shared category' }, s.id);
        const form = new FormData();
        for (const [key,value] of Object.entries({ Name: 'Shared product', CategoryId: s.category.id, Price: 20000, Stock: 100, IsAvailable: true })) form.set(key, String(value));
        s.product = await request('/api/products', token, 'POST', form, s.id);
        s.combo = await request('/api/combos', token, 'POST', { comboCode: 'SHARED', name: 'Shared combo', playingHours: 1, price: 80000, items: [{ productId: s.product.id, quantity: 1 }] }, s.id);
    }
    await request('/api/combos', token, 'POST', { comboCode: 'CROSS', name: 'Invalid cross-store', price: 80000, playingHours: 1, items: [{ productId: b.product.id, quantity: 1 }] }, a.id, 400);
    await request('/api/table-sessions/start', token, 'POST', { tableId: b.tables[0].id, durationHours: 1, customerName: 'Customer', customerPhone: '0912345678' }, a.id, 404);
    for (const s of stores) {
        s.session = await request('/api/table-sessions/start', token, 'POST', { tableId: s.tables[0].id, durationHours: 1, customerName: 'Customer', customerPhone: '0912345678' }, s.id);
    }
    const order = await request('/api/orders', token, 'POST', { sessionId: a.session.id, items: [{ productId: a.product.id, quantity: 1 }] }, a.id);
    await request('/api/orders', token, 'POST', { sessionId: a.session.id, items: [{ productId: b.product.id, quantity: 1 }] }, a.id, 404);
    assert.equal((await request('/api/orders', b.auth.token)).totalCount, 0);
    const revenueA = await request('/api/revenues/total', a.auth.token);
    const revenueB = await request('/api/revenues/total', b.auth.token);
    const revenueAll = await request('/api/revenues/total', token);
    assert(revenueA.totalRevenue > 0 && revenueB.totalRevenue > 0);
    assert(revenueAll.totalRevenue > revenueA.totalRevenue + revenueB.totalRevenue);
    console.log('PASS: SuperAdmin category/product/combo/session/order writes and cross-store FK rejection; revenue isolation');

    const [eventsA,eventsB,eventsSuper] = await Promise.all([hub(a.auth.token), hub(b.auth.token), hub(token)]);
    await request('/api/tables', token, 'POST', tableBody('Only store A notification'), a.id);
    await delay(500);
    assert(eventsA.some(e => JSON.stringify(e).includes('Only store A notification')));
    assert(!eventsB.some(e => JSON.stringify(e).includes('Only store A notification')));
    assert(eventsSuper.some(e => JSON.stringify(e).includes('Only store A notification')));
    await request('/api/tables', b.auth.token, 'POST', tableBody('Only store B notification'));
    await delay(500);
    assert(eventsB.some(e => JSON.stringify(e).includes('Only store B notification')));
    assert(!eventsA.some(e => JSON.stringify(e).includes('Only store B notification')));
    assert(eventsSuper.some(e => JSON.stringify(e).includes('Only store B notification')));
    assert.equal((await fetch(`${base}/hubs/table/negotiate?negotiateVersion=1`, { method: 'POST' })).status, 401);
    console.log('PASS: Real WebSocket SignalR events isolated both directions; SuperAdmin observer receives both; anonymous denied');

    for (const path of ['/api/tables', '/api/table-sessions']) {
        await request(path, a.auth.token); // warmup
        const times = [];
        for (let i=0;i<30;i++) {
            const start = performance.now(); const data = await request(path, a.auth.token);
            assert.equal(data.totalCount, path === '/api/tables' ? 11 : 1);
            times.push(performance.now()-start);
        }
        times.sort((x,y)=>x-y);
        const median = times[15]; const p95=times[28];
        assert(p95 < 1000, `${path}: local smoke benchmark p95 exceeded 1s`);
        console.log(`BENCH ${path}: 12 added stores / 122 tables / 12 active sessions; 30 requests; median=${median.toFixed(1)}ms p95=${p95.toFixed(1)}ms`);
    }

    const browser = new Browser(); await browser.login('superadmin', process.env.Seed__SuperAdminPassword);
    assert.equal((await browser.get('/Admin/Users/Index')).status, 200);
    const createStore = await browser.form('/Admin/Stores/Index?handler=Create', { 'NewStore.Name': 'Web form store' });
    assert.equal(createStore.status, 302, createStore.html);
    const createdWebStore = (await request('/api/stores', token)).find(s => s.name === 'Web form store');
    assert(createdWebStore, 'Store modal POST must validate only the submitted model');
    const createdAdmin = await browser.form('/Admin/Users/Index?handler=Create', { 'NewAdmin.FullName': 'Web Admin', 'NewAdmin.Username': 'verify_web_admin', 'NewAdmin.Password': '123456', 'NewAdmin.StoreId': a.id });
    assert.equal(createdAdmin.status, 302, createdAdmin.html);
    const webUser = (await request('/api/users', token)).find(u => u.username === 'verify_web_admin');
    assert(webUser);
    assert.equal((await browser.form('/Admin/Users/Index?handler=Assign', { userId: webUser.id, storeId: b.id })).status, 302);
    assert.equal((await login(webUser.username)).user.storeId, b.id);
    for (const path of ['/Admin/Dashboard/Index', '/Admin/Revenue/Index']) assert.equal((await browser.get(path)).status, 200);
    const adminBrowser = new Browser(); await adminBrowser.login(a.auth.user.username);
    const denied = await adminBrowser.get('/Admin/Users/Index'); assert.equal(denied.status, 302); assert(denied.location.includes('AccessDenied'));
    await request('/api/users', a.auth.token, 'POST', { fullName: 'Staff sidebar', username: 'verify_sidebar_staff', password: '123456', role: 2 });
    const staffBrowser = new Browser(); await staffBrowser.login('verify_sidebar_staff');
    const staffPage = await staffBrowser.get('/Staff/Dashboard/Index');
    assert.equal(staffPage.status, 200); assert(staffPage.html.includes(a.name));
    console.log('PASS: Web store creation, Users create/assign forms, SuperAdmin aggregate pages, Admin access denial, Staff store name');

    const today = new Date().toISOString().slice(0, 10);
    const compare = await request(`/api/reports/compare?storeIds=${a.id}&storeIds=${b.id}&from=${today}&to=${today}`, token);
    assert.equal(compare.length, 2);
    assert.equal(compare.find(s => s.storeId === a.id).revenue, revenueA.totalRevenue);
    assert.equal(compare.find(s => s.storeId === b.id).revenue, revenueB.totalRevenue);
    assert(compare.every(s => s.dailyRevenues.length === 1 && s.sessionsToday === 1));
    const overview = await request('/api/reports/system-overview', token);
    assert.equal(overview.totalStores, (await request('/api/stores', token)).length);
    assert.equal(overview.todayRevenue, revenueAll.todayRevenue);
    const scopedOverview = await request('/api/reports/system-overview', token, 'GET', undefined, a.id);
    assert.equal(scopedOverview.totalStores, overview.totalStores);
    assert.equal(scopedOverview.todayRevenue, overview.todayRevenue);
    await request('/api/reports/system-overview', a.auth.token, 'GET', undefined, null, 403);
    await request('/api/reports/compare', a.auth.token, 'GET', undefined, null, 403);
    await request('/api/reports/compare?from=2026-11-10&to=2026-11-01', token, 'GET', undefined, null, 400);
    await request('/api/reports/compare?from=2020-01-01&to=2026-01-01', token, 'GET', undefined, null, 400);
    await request(`/api/reports/compare?storeIds=${crypto.randomUUID()}`, token, 'GET', undefined, null, 404);
    assert.equal((await request('/api/stores/current', a.auth.token)).id, a.id);
    await request('/api/stores', a.auth.token, 'GET', undefined, null, 403);
    const adminProfile = await adminBrowser.get('/Profile/Index');
    assert.equal(adminProfile.status, 200); assert(adminProfile.html.includes(a.name));
    const superProfile = await browser.get('/Profile/Index'); assert.equal(superProfile.status, 200);
    assert(!superProfile.html.includes('Thông tin cửa hàng'));
    const comparisonPage = await browser.get(`/Admin/Reports/Compare?StoreIds=${a.id}&FromDate=${today}&ToDate=${today}`);
    assert.equal(comparisonPage.status, 200); assert(comparisonPage.html.includes('storeComparisonChart'));
    const dashboardPage = await browser.get('/Admin/Dashboard/Index');
    assert(dashboardPage.html.includes('systemTrend')); assert(!dashboardPage.html.includes('Không thể tải dữ liệu Dashboard'));
    assert.equal((await adminBrowser.get('/Admin/Reports/Compare')).status, 302);
    assert.equal((await staffBrowser.get('/SuperAdmin/StoreSelect')).status, 302);
    const selected = await browser.form('/SuperAdmin/StoreSelect?handler=Select', { storeId: a.id });
    assert.equal(selected.status, 302); assert(selected.location.includes('/Admin/Tables'));
    const selectedTables = await browser.get('/Admin/Tables/Index');
    assert.equal(selectedTables.status, 200); assert(selectedTables.html.includes('Only store A notification'));
    assert(!selectedTables.html.includes('Only store B notification'));
    assert((await browser.get('/Admin/Users/Index')).html.includes('verify_admin_11'));
    assert.equal((await browser.form('/SuperAdmin/StoreSelect?handler=Clear', {})).status, 302);
    const needsStore = await browser.get('/Admin/Tables/Index');
    assert.equal(needsStore.status, 302); assert(needsStore.location.includes('/SuperAdmin/StoreSelect'));
    const edited = await browser.form('/Admin/Users/Index?handler=Edit', { userId: webUser.id, 'EditAdmin.FullName': 'Updated Web Admin', 'EditAdmin.Email': 'updated@example.com' });
    assert.equal(edited.status, 302);
    assert.equal((await request(`/api/users/${webUser.id}`, token)).fullName, 'Updated Web Admin');
    await request(`/api/users/${webUser.id}/active`, a.auth.token, 'PUT', false, null, 403);
    assert.equal((await browser.form('/Admin/Users/Index?handler=Toggle', { userId: webUser.id, active: 'false' })).status, 302);
    await request('/api/auth/login', null, 'POST', { username: webUser.username, password: '123456' }, null, 403);
    assert.equal((await browser.form('/Admin/Users/Index?handler=Toggle', { userId: webUser.id, active: 'true' })).status, 302);
    assert.equal((await browser.form('/Admin/Users/Index?handler=ResetPassword', { userId: webUser.id, 'PasswordReset.NewPassword': 'UpdatedPass123!' })).status, 302);
    assert.equal((await login(webUser.username, 'UpdatedPass123!')).user.id, webUser.id);
    console.log('PASS: System report totals, date/store validation, authorization, profiles, comparison UI, switcher isolation, Admin edit/disable/enable/reset');
    await request(`/api/table-sessions/end/${a.session.id}`, token, 'POST', { paymentMethod: 0 }, a.id);
    const spoofedTable = await request('/api/tables', a.auth.token, 'POST', { ...tableBody('Spoofed table'), storeId: b.id }, b.id);
    await request(`/api/tables/${spoofedTable.id}`, b.auth.token, 'GET', undefined, null, 404);
    await request('/api/table-sessions/start', a.auth.token, 'POST', { tableId: a.tables[1].id, durationHours: 0, customerName: 'History guard', customerPhone: '0912345678' });
    await request(`/api/users/${a.auth.user.id}/store`, token, 'PUT', b.id, null, 400);
    console.log(`PASS: Session checkout, spoofed table StoreId/header ignored, historical Admin transfer rejected; order ${order.id}`);
    await verifyAudit({ request, login, Browser, token, superUser, stores, base });
    console.log('ALL MULTI-TENANT AND AUDIT CHECKS PASSED (disposable database is removed by runner)');
} finally {
    for (const socket of sockets) socket.close();
}
