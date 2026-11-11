// ==========================================
// EzShop - Smartphone Comparison System
// ==========================================

function escapeHtml(value) {
    return String(value ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');
}
window.escapeHtml = window.escapeHtml || escapeHtml;

function getCompareProducts() {
    if (typeof productsData !== 'undefined' && Array.isArray(productsData) && productsData.length > 0) return productsData;
    if (typeof window.productsData !== 'undefined' && Array.isArray(window.productsData) && window.productsData.length > 0) return window.productsData;
    if (typeof initialProductsData !== 'undefined' && Array.isArray(initialProductsData) && initialProductsData.length > 0) return initialProductsData;
    return [];
}

function getCompareIds() {
    try {
        return JSON.parse(localStorage.getItem('ez_compare_ids')) || [];
    } catch (e) {
        return [];
    }
}

function toggleCompare(id, e) {
    if (e) {
        e.stopPropagation();
        if (e.currentTarget && typeof e.currentTarget.blur === 'function') {
            e.currentTarget.blur();
        }
    }
    let comp = getCompareIds();
    const idx = comp.indexOf(id);
    const prods = getCompareProducts();
    const prod = prods.find(p => p.id === id);
    const name = prod ? prod.name : 'Товар';

    if (idx > -1) {
        comp.splice(idx, 1);
        showToast(`${name} убран из сравнения`, 'info');
    } else {
        if (comp.length >= 4) {
            showToast('Максимум 4 модели для одновренного сравнения', 'error');
            return;
        }
        comp.push(id);
        showToast(`${name} добавлен к сравнению ⚖️`, 'success');
    }
    localStorage.setItem('ez_compare_ids', JSON.stringify(comp));
    updateCompareBar();
}

function removeFromCompareModal(id) {
    let comp = getCompareIds();
    const idx = comp.indexOf(id);
    const prods = getCompareProducts();
    const prod = prods.find(p => p.id === id);
    const name = prod ? prod.name : 'Товар';

    if (idx > -1) {
        comp.splice(idx, 1);
        localStorage.setItem('ez_compare_ids', JSON.stringify(comp));
        showToast(`${name} убран из сравнения`, 'info');
    }

    updateCompareBar();

    if (comp.length === 0) {
        const container = document.getElementById('compareTableContainer');
        if (container) container.innerHTML = '';
        closeCompareModal();
    } else {
        openCompareModal();
    }
}

function clearCompare() {
    localStorage.removeItem('ez_compare_ids');
    updateCompareBar();
    const container = document.getElementById('compareTableContainer');
    if (container) container.innerHTML = '';
    closeCompareModal();
    showToast('Список сравнения очищен', 'info');
}

function updateCompareBar() {
    const comp = getCompareIds();
    const prods = getCompareProducts();
    const bar = document.getElementById('compareBar');
    const navCount = document.getElementById('navCompareCount');
    if (navCount) navCount.textContent = comp.length;

    if (bar) {
        if (comp.length > 0) {
            bar.style.display = 'flex';
            const textEl = document.getElementById('compareBarText');
            if (textEl) textEl.textContent = `Выбрано для сравнения: ${comp.length} из 4`;
        } else {
            bar.style.display = 'none';
        }
    }

    document.querySelectorAll('.btn-compare').forEach(btn => {
        const id = parseInt(btn.getAttribute('data-id'));
        if (comp.includes(id)) {
            btn.classList.add('active');
            btn.setAttribute('title', 'Убрать из сравнения');
        } else {
            btn.classList.remove('active');
            btn.setAttribute('title', 'Добавить к сравнению');
            if (document.activeElement === btn) btn.blur();
        }
    });
}

function openCompareModal() {
    const comp = getCompareIds();
    const container = document.getElementById('compareTableContainer');
    const modal = document.getElementById('compareModal');

    if (comp.length === 0) {
        if (container) container.innerHTML = '';
        if (modal && modal.style.display === 'flex') {
            closeCompareModal();
        } else {
            showToast('Добавьте хотя бы один смартфон для сравнения (кнопка с весами на карточке)', 'info');
        }
        return;
    }

    const prods = getCompareProducts();
    const items = comp.map(id => prods.find(p => p.id === id)).filter(Boolean);
    if (items.length === 0) {
        if (container) container.innerHTML = '';
        closeCompareModal();
        return;
    }

    if (!container || !modal) return;

    const E = (v) => escapeHtml(v);
    const bestOf = (vals, dir) => {
        if (vals.length < 2) return new Set();
        const target = dir === 'min' ? Math.min(...vals) : Math.max(...vals);
        if (vals.every(v => v === target)) return new Set();
        return new Set(vals.map((v, i) => (v === target ? i : -1)).filter(i => i >= 0));
    };
    const cells = (arr, best) => arr.map((c, i) => {
        const isBest = best && best.has(i);
        return `<td class="${isBest ? 'is-best' : ''}">${c}${isBest ? '<span class="best-badge">лучшее</span>' : ''}</td>`;
    }).join('');
    const row = (label, arr, best) => `<tr><th scope="row">${label}</th>${cells(arr, best)}</tr>`;
    const inStockOf = (p) => p.inStock !== false;

    const html = `
        <table class="compare-table">
            <thead>
                <tr>
                    <th class="compare-corner" scope="col">Параметр</th>
                    ${items.map(p => `
                        <th scope="col">
                            <div class="compare-th-card">
                                <button type="button" class="btn-compare-remove" onclick="removeFromCompareModal(${p.id})" aria-label="Убрать ${E(p.name)} из сравнения" title="Убрать">&times;</button>
                                <a href="/Catalog/Details/${p.id}"><img src="${E(p.img.startsWith('/') ? p.img : '/' + p.img)}" alt="${E(p.name)}"></a>
                                <h4>${E(p.name)}</h4>
                            </div>
                        </th>
                    `).join('')}
                </tr>
            </thead>
            <tbody>
                ${row('Цена', items.map(p => `<b class="compare-price">${p.price.toLocaleString()} сом</b>`), bestOf(items.map(p => p.price), 'min'))}
                ${row('Наличие', items.map(p => inStockOf(p)
                    ? '<span class="cmp-ok">В наличии</span>'
                    : '<span class="cmp-no">Нет в наличии</span>'))}
                ${row('Бренд', items.map(p => E(p.brand)))}
                ${row('Дисплей', items.map(p => `<b>${E(p.display)}</b>`), bestOf(items.map(p => parseFloat(p.display) || 0), 'max'))}
                ${row('ОЗУ (RAM)', items.map(p => `<b>${p.ram} ГБ</b>`), bestOf(items.map(p => p.ram), 'max'))}
                ${row('Память (ROM)', items.map(p => `<b>${p.storage} ГБ</b>`), bestOf(items.map(p => p.storage), 'max'))}
                ${row('Аккумулятор', items.map(p => `<b>${p.battery} мАч</b>`), bestOf(items.map(p => p.battery), 'max'))}
                ${row('Цвет', items.map(p => E(p.color)))}
                ${row('', items.map(p => inStockOf(p)
                    ? `<button type="button" class="btn-add-cart" onclick="addToCart(getCompareProducts().find(x => x.id === ${p.id}))">В корзину</button>`
                    : `<button type="button" class="btn-add-cart" disabled>Нет в наличии</button>`))}
            </tbody>
        </table>
        <p class="compare-legend"><i></i> Лучшее значение в строке подсвечено</p>
    `;

    container.innerHTML = html;
    modal.style.display = 'flex';
}

function closeCompareModal() {
    const modal = document.getElementById('compareModal');
    if (modal) modal.style.display = 'none';
}

document.addEventListener('DOMContentLoaded', () => {
    updateCompareBar();
});
