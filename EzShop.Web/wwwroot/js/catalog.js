// ==========================================
// EzShop - Catalog & Dynamic Filtering System
// ==========================================

const productsEl = document.getElementById('products');

function getActiveProducts() {
    if (window.productsData && Array.isArray(window.productsData) && window.productsData.length > 0) {
        return window.productsData;
    }
    return (typeof productsData !== 'undefined' && Array.isArray(productsData)) ? productsData : [];
}

function getActiveBrands() {
    if (window.brandsData && Array.isArray(window.brandsData) && window.brandsData.length > 0) {
        return window.brandsData;
    }
    return (typeof brandsData !== 'undefined' && Array.isArray(brandsData)) ? brandsData : [];
}

function renderProducts(list) {
    if (!productsEl) return;

    const countEl = document.getElementById('productsCount');
    if (countEl) countEl.textContent = list.length;
    const applyCount = document.getElementById('filtersApplyCount');
    if (applyCount) applyCount.textContent = list.length;

    if (list.length === 0) {
        productsEl.innerHTML = `
            <div class="empty-state">
                <h3>Ничего не найдено</h3>
                <p>Попробуйте изменить параметры фильтрации или поисковый запрос</p>
                <button type="button" class="btn btn-primary" style="margin-top: 18px;" onclick="resetAllFilters()">Сбросить фильтры</button>
            </div>
        `;
        return;
    }

    const fragment = document.createDocumentFragment();

    list.forEach((p, idx) => fragment.appendChild(buildProductCard(p, idx)));

    productsEl.replaceChildren(fragment);

    if (typeof updateFavoritesUI === 'function') updateFavoritesUI();
    if (typeof updateCompareBar === 'function') updateCompareBar();
}

function renderFilters() {
    const prods = getActiveProducts();
    const brands = getActiveBrands();

    // Preserve any currently checked values before re-rendering
    const prevBrands = Array.from(document.querySelectorAll('#brandFilters input:checked')).map(cb => cb.value);
    const prevRam = Array.from(document.querySelectorAll('#ramFilters input:checked')).map(cb => cb.value);
    const prevStorage = Array.from(document.querySelectorAll('#storageFilters input:checked')).map(cb => cb.value);

    // Dynamic brands from brandsData + any extra from productsData
    const allBrandNames = [...new Set([
        ...brands.map(b => b.name),
        ...prods.map(p => p.brand)
    ])].sort();

    const rams = [...new Set(prods.map(p => p.ram))].sort((a, b) => a - b);
    const storages = [...new Set(prods.map(p => p.storage))].sort((a, b) => a - b);

    const brandContainer = document.getElementById('brandFilters');
    const ramContainer = document.getElementById('ramFilters');
    const storageContainer = document.getElementById('storageFilters');

    const countBy = (key, value) => prods.filter(p => p[key] === value).length;

    if (brandContainer) {
        brandContainer.innerHTML = allBrandNames.map(b => `
            <label class="chip">
                <input type="checkbox" value="${esc(b)}" ${prevBrands.includes(b) ? 'checked' : ''} onchange="applyFilters(true)">
                ${esc(b)} <span class="chip-count">${prods.filter(p => p.brand === b).length}</span>
            </label>
        `).join('');
    }

    if (ramContainer) {
        ramContainer.innerHTML = rams.map(r => `
            <label class="chip">
                <input type="checkbox" value="${r}" ${prevRam.includes(String(r)) ? 'checked' : ''} onchange="applyFilters(true)">
                ${r} ГБ <span class="chip-count">${countBy('ram', r)}</span>
            </label>
        `).join('');
    }

    if (storageContainer) {
        storageContainer.innerHTML = storages.map(s => `
            <label class="chip">
                <input type="checkbox" value="${s}" ${prevStorage.includes(String(s)) ? 'checked' : ''} onchange="applyFilters(true)">
                ${s} ГБ <span class="chip-count">${countBy('storage', s)}</span>
            </label>
        `).join('');
    }

    updateHeroStats();
}

function updateHeroStats() {
    const prods = getActiveProducts();
    const models = document.getElementById('heroModels');
    const brands = document.getElementById('heroBrands');
    if (models) models.textContent = prods.length;
    if (brands) brands.textContent = new Set(prods.map(p => p.brand)).size;
}

// Быстрые диапазоны цены
function setPricePreset(btn) {
    const minPrice = document.getElementById('minPrice');
    const maxPrice = document.getElementById('maxPrice');
    if (!minPrice || !maxPrice) return;
    const same = minPrice.value === btn.dataset.min && maxPrice.value === btn.dataset.max;
    minPrice.value = same ? '' : btn.dataset.min;
    maxPrice.value = same ? '' : btn.dataset.max;
    applyFilters(true);
}

// Количество активных фильтров + «чипы» применённых условий над сеткой
function renderActiveFilters() {
    const box = document.getElementById('activeFilters');
    const minPrice = document.getElementById('minPrice');
    const maxPrice = document.getElementById('maxPrice');
    const chips = [];

    if (minPrice && minPrice.value) chips.push({ label: `от ${Number(minPrice.value).toLocaleString()} сом`, clear: () => { minPrice.value = ''; } });
    if (maxPrice && maxPrice.value) chips.push({ label: `до ${Number(maxPrice.value).toLocaleString()} сом`, clear: () => { maxPrice.value = ''; } });

    document.querySelectorAll('#brandFilters input:checked').forEach(cb => chips.push({ label: cb.value, clear: () => { cb.checked = false; } }));
    document.querySelectorAll('#ramFilters input:checked').forEach(cb => chips.push({ label: `ОЗУ ${cb.value} ГБ`, clear: () => { cb.checked = false; } }));
    document.querySelectorAll('#storageFilters input:checked').forEach(cb => chips.push({ label: `Память ${cb.value} ГБ`, clear: () => { cb.checked = false; } }));

    const stock = document.getElementById('onlyInStockCheck');
    if (stock && stock.checked) chips.push({ label: 'В наличии', clear: () => { stock.checked = false; } });
    const fav = document.getElementById('onlyFavoritesCheck');
    if (fav && fav.checked) chips.push({ label: 'Избранные', clear: () => { fav.checked = false; } });

    window.__activeFilterClears = chips.map(c => c.clear);

    if (box) {
        box.innerHTML = chips.map((c, i) => `
            <span class="active-chip">${esc(c.label)}<button type="button" aria-label="Убрать фильтр" onclick="removeActiveFilter(${i})">&times;</button></span>
        `).join('');
    }

    ['filterCount', 'filterCountBtn'].forEach(id => {
        const el = document.getElementById(id);
        if (!el) return;
        el.textContent = chips.length;
        el.classList.toggle('show', chips.length > 0);
    });

    // подсветка активных пресетов цены
    document.querySelectorAll('.price-presets .chip').forEach(b => {
        b.classList.toggle('is-on',
            !!minPrice && !!maxPrice && minPrice.value === b.dataset.min && maxPrice.value === b.dataset.max
            && (b.dataset.min !== '' || b.dataset.max !== ''));
    });
}

function removeActiveFilter(i) {
    const fn = (window.__activeFilterClears || [])[i];
    if (fn) fn();
    applyFilters(false);
}

function applyFilters(shouldScroll = true) {
    const search = document.getElementById('search');
    const sort = document.getElementById('sort');
    const minPrice = document.getElementById('minPrice');
    const maxPrice = document.getElementById('maxPrice');

    if (!search || !sort) return;

    const selectedBrands = Array.from(document.querySelectorAll('#brandFilters input:checked')).map(cb => cb.value.toLowerCase());
    const selectedRam = Array.from(document.querySelectorAll('#ramFilters input:checked')).map(cb => parseInt(cb.value));
    const selectedStorage = Array.from(document.querySelectorAll('#storageFilters input:checked')).map(cb => parseInt(cb.value));

    let list = [...getActiveProducts()];
    const min = minPrice && minPrice.value ? parseFloat(minPrice.value) : 0;
    const max = maxPrice && maxPrice.value ? parseFloat(maxPrice.value) : Infinity;
    const q = search.value.trim().toLowerCase();

    if (selectedBrands.length > 0) {
        list = list.filter(p => selectedBrands.includes(p.brand.toLowerCase()));
    }
    if (selectedRam.length > 0) {
        list = list.filter(p => selectedRam.includes(p.ram));
    }
    if (selectedStorage.length > 0) {
        list = list.filter(p => selectedStorage.includes(p.storage));
    }

    // Filter only in stock
    const onlyInStock = document.getElementById('onlyInStockCheck')?.checked;
    if (onlyInStock) {
        list = list.filter(p => p.inStock !== false);
    }

    // Filter only favorites
    const onlyFav = document.getElementById('onlyFavoritesCheck')?.checked;
    if (onlyFav) {
        const favIds = getFavoriteIds();
        list = list.filter(p => favIds.includes(p.id));
    }

    if (sort.value === 'az') list.sort((a, b) => a.name.localeCompare(b.name));
    if (sort.value === 'za') list.sort((a, b) => b.name.localeCompare(a.name));
    if (sort.value === 'price_asc') list.sort((a, b) => a.price - b.price);
    if (sort.value === 'price_desc') list.sort((a, b) => b.price - a.price);

    list = list.filter(p => p.price >= min && p.price <= max && (p.name.toLowerCase().includes(q) || p.brand.toLowerCase().includes(q)));

    // Prevent browser clamping scroll to the bottom footer when filtered list shrinks
    if (shouldScroll && window.innerWidth > 900 && window.scrollY > 160) {
        const top = document.getElementById('catalog');
        const y = top ? Math.max(0, top.getBoundingClientRect().top + window.scrollY - 90) : 0;
        if (window.scrollY > y) window.scrollTo({ top: y, behavior: 'instant' });
    }

    renderActiveFilters();
    renderProducts(list);
}

function resetAllFilters() {
    const search = document.getElementById('search');
    const sort = document.getElementById('sort');
    const minPrice = document.getElementById('minPrice');
    const maxPrice = document.getElementById('maxPrice');

    if (search) search.value = '';
    if (sort) sort.value = 'default';
    if (minPrice) minPrice.value = '';
    if (maxPrice) maxPrice.value = '';

    document.querySelectorAll('.sidebar input[type="checkbox"]').forEach(cb => cb.checked = false);
    applyFilters(true);
    showToast('Фильтры сброшены', 'info');
}

// Мобильная шторка фильтров
function openFilters() {
    const panel = document.getElementById('filtersPanel');
    const back = document.getElementById('filtersBackdrop');
    if (!panel) return;
    panel.classList.add('open');
    if (back) back.classList.add('show');
    document.body.classList.add('sheet-open');
}

function closeFilters() {
    const panel = document.getElementById('filtersPanel');
    const back = document.getElementById('filtersBackdrop');
    if (panel) panel.classList.remove('open');
    if (back) back.classList.remove('show');
    document.body.classList.remove('sheet-open');
}

document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') closeFilters();
});

document.addEventListener('DOMContentLoaded', () => {
    if (productsEl) {
        renderFilters();
        renderProducts(getActiveProducts());
    }
});
