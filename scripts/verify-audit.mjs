import assert from 'node:assert/strict';

// Called by verify-multitenant.mjs inside the same disposable database lifecycle.
export async function verifyAudit({ request, login, Browser, token, superUser, stores, base }) {
    const [a, b] = stores;
    for (const path of ['/api/stores', '/api/users', '/api/tables', '/api/products', '/api/categories',
        '/api/combos', '/api/customers', '/api/orders', '/api/bills', '/api/sessions',
        '/api/table-sessions', '/api/shifts/today', '/api/reports/dashboard', '/api/revenues/total']) {
        await request(path, null, 'GET', undefined, null, 401);
    }
    for (const hub of ['table', 'product', 'order', 'notification']) {
        const response = await fetch(`${base}/hubs/${hub}/negotiate?negotiateVersion=1`, { method: 'POST' });
        assert.equal(response.status, 401);
    }

    async function staff(store, suffix) {
        const user = await request('/api/users', store.auth.token, 'POST', {
            fullName: `Audit staff ${suffix}`, username: `audit_staff_${suffix}`, password: 'AuditStaff123!', role: 2
        });
        return login(user.username, 'AuditStaff123!');
    }
    const [one, two, other] = await Promise.all([staff(a, 'a1'), staff(a, 'a2'), staff(b, 'b')]);
    const s1 = one.token, s2 = two.token, sb = other.token;
    for (const path of ['/api/tables', '/api/products', '/api/categories', '/api/combos', '/api/users', '/api/stores']) {
        await request(path, s1, 'POST', {}, null, 403);
    }
    await request('/api/revenues/total', s1, 'GET', undefined, null, 403);
    await request('/api/reports/system-overview', s1, 'GET', undefined, null, 403);
    await request(`/api/users/${two.user.id}/reset-password`, s1, 'POST', { newPassword: 'Denied123!' }, null, 403);
    await request(`/api/users/${other.user.id}/store`, a.auth.token, 'PUT', a.id, null, 403);
    await request(`/api/revenues/staff/${other.user.id}`, a.auth.token, 'GET', undefined, null, 404);
    await request(`/api/revenues/staff/${superUser.user.id}`, a.auth.token, 'GET', undefined, null, 404);
    await request(`/api/tables/${b.tables[0].id}`, s1, 'GET', undefined, null, 404);
    await request(`/api/products/${b.product.id}`, s1, 'GET', undefined, null, 404);
    await request(`/api/categories/${b.category.id}`, s1, 'GET', undefined, null, 404);
    await request(`/api/combos/${b.combo.id}`, s1, 'GET', undefined, null, 404);
    assert((await request('/api/tables', s1, 'GET', undefined, b.id)).items.every(t => t.id !== b.tables[0].id));

    const firstShift = await request('/api/shifts/checkin', s1, 'POST');
    const secondShift = await request('/api/shifts/checkin', s2, 'POST');
    await request('/api/shifts/checkin', s1, 'POST', undefined, null, 400);
    await request(`/api/shifts/checkout/${firstShift.id}`, sb, 'POST', undefined, null, 404);
    await request(`/api/shifts/checkout/${firstShift.id}`, s2, 'POST', undefined, null, 403);
    const start = (table, hours) => ({ tableId: table.id, durationHours: hours, customerName: 'Audit customer', customerPhone: '0987654321', paymentMethod: 0 });
    await request('/api/table-sessions/start', s1, 'POST', start(b.tables[4], 1), null, 404);
    await request(`/api/sessions/start/${b.tables[4].id}?customerPhone=0987654321&customerName=Audit`, s1, 'POST', undefined, null, 404);
    const sessionOne = await request('/api/table-sessions/start', s1, 'POST', start(a.tables[4], 1));
    const sessionTwo = await request('/api/table-sessions/start', s2, 'POST', start(a.tables[5], 2));
    const ownBills = await request(`/api/bills?staffId=${two.user.id}`, s1);
    assert.equal(ownBills.totalCount, 1);
    assert.equal(ownBills.items[0].sessionId, sessionOne.id);
    const secondBill = (await request('/api/bills', s2)).items[0];
    await request(`/api/bills/${secondBill.id}`, s1, 'GET', undefined, null, 403);
    await request(`/api/bills/${secondBill.id}`, sb, 'GET', undefined, null, 404);
    await request(`/api/bills/pay/${secondBill.id}`, sb, 'POST', undefined, null, 404);
    await request(`/api/bills/generate/${sessionOne.id}`, sb, 'POST', { paymentMethod: 0 }, null, 404);
    await request(`/api/table-sessions/extend/${sessionOne.id}`, sb, 'POST', { additionalMinutes: 30 }, null, 404);
    await request(`/api/table-sessions/end/${sessionOne.id}`, sb, 'POST', { paymentMethod: 0 }, null, 404);
    const closed = await request(`/api/shifts/checkout/${firstShift.id}`, s1, 'POST');
    assert.equal(closed.revenue, ownBills.items[0].total);
    assert(closed.revenue > 0 && closed.revenue < ownBills.items[0].total + secondBill.total);
    assert(closed.checkOut);
    await request(`/api/shifts/checkout/${firstShift.id}`, s1, 'POST', undefined, null, 400);
    assert.equal((await request(`/api/shifts/checkout/${secondShift.id}`, a.auth.token, 'POST')).revenue, secondBill.total);
    const personal = await request(`/api/revenues/personal?staffId=${other.user.id}`, s1);
    assert.equal(personal.staffId, one.user.id);
    assert.equal(personal.totalRevenue, ownBills.items[0].total);
    console.log('PASS AUDIT: anonymous/Staff role matrix, cross-tenant IDs, bill ownership, shift ownership and per-user shift revenue');

    await request(`/api/stores/${a.id}`, token, 'PUT', { name: a.name, isActive: false }, null, 400);
    await request(`/api/stores/${a.id}`, token, 'DELETE', undefined, null, 400);
    const disabledStore = await request('/api/stores', token, 'POST', { name: 'Audit disabled store' });
    const disabledUser = await request('/api/users', token, 'POST', {
        fullName: 'Audit disabled admin', username: 'audit_disabled_admin', password: 'AuditAdmin123!', role: 1, storeId: disabledStore.id
    });
    const beforeDisable = await login(disabledUser.username, 'AuditAdmin123!');
    await request(`/api/stores/${disabledStore.id}`, token, 'PUT', { name: disabledStore.name, isActive: false });
    await request('/api/auth/login', null, 'POST', { username: disabledUser.username, password: 'AuditAdmin123!' }, null, 403);
    await request('/api/tables', beforeDisable.token, 'GET', undefined, null, 403);
    await request(`/api/stores/${disabledStore.id}`, token, 'PUT', { name: disabledStore.name, isActive: true });
    assert.equal((await login(disabledUser.username, 'AuditAdmin123!')).user.storeId, disabledStore.id);
    console.log('PASS AUDIT: PUT/DELETE store deactivation guard, disabled-store login and existing-token rejection, reactivation');

    const anon = new Browser();
    await anon.form('/Auth/ForgotPassword', { UsernameOrPhone: one.user.username, FullName: 'Audit request A', Note: 'private-reset-note-A' });
    await anon.form('/Auth/ForgotPassword', { UsernameOrPhone: other.user.username, FullName: 'Audit request B', Note: 'private-reset-note-B' });
    const adminA = new Browser(), adminB = new Browser();
    await adminA.login(a.auth.user.username); await adminB.login(b.auth.user.username);
    const pageA = await adminA.get('/Admin/Staff/Index');
    const pageB = await adminB.get('/Admin/Staff/Index');
    assert(pageA.html.includes('private-reset-note-A') && !pageA.html.includes('private-reset-note-B'));
    assert(pageB.html.includes('private-reset-note-B') && !pageB.html.includes('private-reset-note-A'));
    const rowB = [...pageB.html.matchAll(/<tr>[\s\S]*?<\/tr>/g)].map(m => m[0]).find(row => row.includes('private-reset-note-B'));
    const requestId = rowB?.match(/requestId=([a-f\d-]+)/i)?.[1];
    assert(requestId, 'Missing reset request action');
    assert.equal((await adminA.form('/Admin/Staff/Index?handler=ResolvePasswordReset', { requestId })).status, 404);
    assert((await adminB.get('/Admin/Staff/Index')).html.includes('private-reset-note-B'));
    assert.equal((await adminB.form('/Admin/Staff/Index?handler=ResolvePasswordReset', { requestId })).status, 302);
    assert(!(await adminB.get('/Admin/Staff/Index')).html.includes('private-reset-note-B'));
    console.log('PASS AUDIT: password-reset requests are visible/resolvable only by the target Staff account\'s store');
    // The runner restarts the API after this test and verifies this legitimate account survives seeding.
    await request('/api/users', a.auth.token, 'POST', {
        fullName: 'Existing account named staff', username: 'staff', password: 'KeepAccount123!', role: 2
    });
}
