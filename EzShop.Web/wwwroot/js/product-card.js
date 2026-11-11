// ==========================================
// EzShop - Общая карточка товара (каталог и «Похожие модели»)
// ==========================================


function buildProductCard(p, idx = 0) {
    const inStock = p.inStock !== false;
    const name = esc(p.name);
    let imgUrl = p.img || '/img/logo.png';
    if (!imgUrl.startsWith('/') && !imgUrl.startsWith('http')) {
        imgUrl = '/' + imgUrl;
    }
    const c = document.createElement('div');
    c.className = 'card';
    // Задержка появления ограничена, чтобы длинные списки не «доезжали» по секунде
    c.style.setProperty('--i', Math.min(idx, 12));
    c.innerHTML = `
        <a href="/Catalog/Details/${p.id}" class="card-img-link" aria-label="${name}">
            <img src="${esc(imgUrl)}" alt="${name}" loading="lazy" ${!inStock ? 'style="opacity: 0.6; filter: grayscale(.5);"' : ''}>
        </a>
        <div class="card-badges">
            <button type="button" class="btn-compare" data-id="${p.id}" onclick="toggleCompare(${p.id}, event)" title="Добавить к сравнению" aria-label="Сравнить ${name}">${ico('scale')}</button>
            <button type="button" class="btn-fav" data-id="${p.id}" onclick="toggleFavorite(${p.id}, event)" title="В избранное" aria-label="В избранное ${name}">${ico('heart')}</button>
        </div>
        <div class="card-body">
            <div class="card-meta">
                <span class="card-brand">${esc(p.brand)}</span>
                <span class="card-stock ${inStock ? 'in-stock' : 'out-of-stock'}">
                    ${inStock ? '● В наличии' : '● Нет в наличии'}
                </span>
            </div>
            <a href="/Catalog/Details/${p.id}" class="card-title-link">
                <h3>${name}</h3>
            </a>
            <div class="specs">
                <span class="spec-pill">${esc(p.display)}</span>
                <span class="spec-pill">${p.ram}/${p.storage} ГБ</span>
                <span class="spec-pill">${p.battery} мАч</span>
            </div>
            <div class="price">${p.price.toLocaleString()}<small>сом</small></div>
        </div>
        <div class="card-actions">
            ${inStock
                ? `<button type="button" class="btn-add-cart" onclick="addToCart(${p.id})">В корзину</button>`
                : `<button type="button" class="btn-add-cart" disabled>Нет в наличии</button>`
            }
            <a href="/Catalog/Details/${p.id}" class="btn-details" title="Подробнее">Инфо</a>
        </div>
    `;
    return c;
}
