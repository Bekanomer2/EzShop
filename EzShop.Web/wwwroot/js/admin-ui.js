// ==========================================
// EzShop - Админка: боковая панель-форма, фильтр наличия
// Надстройка над admin.js: оборачивает существующие функции, не меняя их логику.
// ==========================================

window.__adminStock = 'all';

function openAdminDrawer(which) {
    const id = which === 'brand' ? 'brandDrawer' : 'productDrawer';
    const el = document.getElementById(id);
    if (!el) return;
    document.querySelectorAll('.admin-form-panel').forEach(p => p.classList.toggle('is-open', p === el));
    const scrim = document.getElementById('adminScrim');
    if (scrim) scrim.classList.add('show');
    document.body.classList.add('admin-drawer-open');
    setTimeout(() => {
        const first = el.querySelector('input:not([type="hidden"]), select, textarea');
        if (first) first.focus({ preventScroll: true });
    }, 380);
}

function closeAdminDrawer() {
    document.querySelectorAll('.admin-form-panel').forEach(p => p.classList.remove('is-open'));
    const scrim = document.getElementById('adminScrim');
    if (scrim) scrim.classList.remove('show');
    document.body.classList.remove('admin-drawer-open');
}

function openProductDrawerNew() {
    resetForm();
    openAdminDrawer('product');
}

function openBrandDrawerNew() {
    resetBrandForm();
    openAdminDrawer('brand');
}

function setAdminStockFilter(value) {
    window.__adminStock = value;
    document.querySelectorAll('#adminStockChips .chip').forEach(c => c.classList.toggle('is-on', c.dataset.stock === value));
    if (typeof filterAdminProducts === 'function') filterAdminProducts();
}

(function wrapAdminFunctions() {
    const wrap = (name, fn) => {
        const original = window[name];
        if (typeof original !== 'function') return;
        window[name] = function (...args) { return fn(original, ...args); };
    };

    // Редактирование открывает панель
    wrap('editProductInAdmin', (orig, id) => { orig(id); openAdminDrawer('product'); });
    wrap('editBrandInAdmin', (orig, id) => { orig(id); openAdminDrawer('brand'); });

    // Успешное сохранение (форма очищается) закрывает панель; при ошибке валидации она остаётся открытой
    wrap('handleProductFormSubmit', (orig, e) => {
        orig(e);
        const name = document.getElementById('prodName');
        if (name && !name.value) closeAdminDrawer();
    });
    wrap('handleBrandFormSubmit', (orig, e) => {
        orig(e);
        const name = document.getElementById('brandName');
        if (name && !name.value) closeAdminDrawer();
    });

    wrap('cancelEditing', (orig) => { orig(); closeAdminDrawer(); });
    wrap('cancelBrandEditing', (orig) => { orig(); closeAdminDrawer(); });
})();

document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') closeAdminDrawer();
});
