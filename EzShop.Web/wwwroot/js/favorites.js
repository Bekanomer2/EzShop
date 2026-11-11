// ==========================================
// EzShop - Wishlist / Favorites System (PostgreSQL DB Powered)
// ==========================================

function escapeHtml(value) {
    return String(value ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');
}
window.escapeHtml = window.escapeHtml || escapeHtml;

function getAvailableProducts() {
    if (typeof window !== 'undefined' && Array.isArray(window.productsData) && window.productsData.length > 0) return window.productsData;
    if (typeof productsData !== 'undefined' && Array.isArray(productsData) && productsData.length > 0) return productsData;
    if (typeof initialProductsData !== 'undefined' && Array.isArray(initialProductsData) && initialProductsData.length > 0) return initialProductsData;
    return [];
}

// Получение уникального ключа хранилища в зависимости от текущего пользователя
function getFavStorageKey() {
    let uid = 0;
    if (typeof window !== 'undefined' && window.currentUserId) {
        uid = Number(window.currentUserId) || 0;
    }
    if (!uid) {
        try {
            const u = JSON.parse(localStorage.getItem('ez_user') || 'null');
            if (u && u.id) uid = Number(u.id) || 0;
        } catch (e) {}
    }
    return (uid > 0) ? ('ez_fav_' + uid) : 'ez_fav_guest';
}

function getFavoriteIds() {
    try {
        const key = getFavStorageKey();
        const raw = localStorage.getItem(key);
        const parsed = raw ? JSON.parse(raw) : [];
        return Array.isArray(parsed) ? parsed.map(Number) : [];
    } catch (e) {
        return [];
    }
}

async function toggleFavorite(id, e) {
    if (e) {
        if (typeof e.stopPropagation === 'function') e.stopPropagation();
        if (typeof e.preventDefault === 'function') e.preventDefault();
    }
    id = Number(id);
    let favs = getFavoriteIds();
    const idx = favs.indexOf(id);
    const prods = getAvailableProducts();
    const prod = prods.find(p => Number(p.id) === id);
    const name = prod ? prod.name : 'Товар';
    const key = getFavStorageKey();

    if (idx > -1) {
        favs.splice(idx, 1);
        if (typeof showToast === 'function') showToast(`${name} удален из избранного`, 'info');
    } else {
        favs.push(id);
        if (typeof showToast === 'function') showToast(`${name} добавлен в избранное ❤️`, 'success');
    }

    try {
        localStorage.setItem(key, JSON.stringify(favs));
    } catch (err) {}
    updateFavoritesUI();

    // Если модальное окно открыто, сразу перерисовываем
    const modal = document.getElementById('favoritesModal');
    if (modal && modal.style.display === 'flex') {
        renderFavoritesModalContent();
    }

    // Сохранение в базу данных PostgreSQL на сервере
    try {
        const resp = await fetch('/Favorites/Toggle/' + id, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });
        if (resp.ok) {
            const data = await resp.json();
            // Если ответ успешен и подтверждает статус
            if (data && data.isAuth && typeof data.isFavorite === 'boolean') {
                let current = getFavoriteIds();
                if (data.isFavorite && !current.includes(id)) {
                    current.push(id);
                } else if (!data.isFavorite && current.includes(id)) {
                    current = current.filter(x => x !== id);
                }
                localStorage.setItem(key, JSON.stringify(current));
                updateFavoritesUI();
            }
        }
    } catch (err) {
        console.warn('DB favorite sync warning:', err);
    }
}

function updateFavoritesUI() {
    const favs = getFavoriteIds();

    // Обновление бейджей счетчиков в шапке
    document.querySelectorAll('.nav-fav-count').forEach(el => {
        el.textContent = favs.length;
    });

    // Обновление иконок сердечек на карточках
    document.querySelectorAll('.btn-fav').forEach(btn => {
        const id = parseInt(btn.getAttribute('data-id'), 10);
        if (favs.includes(id)) {
            btn.classList.add('active');
            btn.innerHTML = (typeof EZ_ICON === 'function') ? EZ_ICON('heart-fill') : '❤️';
        } else {
            btn.classList.remove('active');
            btn.innerHTML = (typeof EZ_ICON === 'function') ? EZ_ICON('heart') : '🤍';
        }
    });
}

// Загрузка избранного из базы данных PostgreSQL при старте
async function initFavoritesFromServer() {
    let uid = 0;
    if (typeof window !== 'undefined' && window.currentUserId) {
        uid = Number(window.currentUserId) || 0;
    }
    if (!uid) {
        try {
            const u = JSON.parse(localStorage.getItem('ez_user') || 'null');
            if (u && u.id) uid = Number(u.id) || 0;
        } catch (e) {}
    }

    if (uid > 0) {
        // Если у пользователя были товары в гостевом избранном до входа, синхронизируем их в БД
        try {
            const guestRaw = localStorage.getItem('ez_fav_guest');
            const guestIds = guestRaw ? JSON.parse(guestRaw) : [];
            if (Array.isArray(guestIds) && guestIds.length > 0) {
                await fetch('/Favorites/SyncGuestFavorites', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(guestIds)
                });
                localStorage.removeItem('ez_fav_guest');
            }
        } catch (e) {}

        // Запрашиваем актуальный список из PostgreSQL
        try {
            const resp = await fetch('/Favorites/GetMyFavorites');
            if (resp.ok) {
                const data = await resp.json();
                if (data && data.success && Array.isArray(data.ids)) {
                    const key = getFavStorageKey();
                    localStorage.setItem(key, JSON.stringify(data.ids));
                    updateFavoritesUI();
                }
            }
        } catch (err) {
            console.warn('Failed to load favorites from DB:', err);
        }
    } else {
        updateFavoritesUI();
    }
}

function openFavoritesModal() {
    let modal = document.getElementById('favoritesModal');
    if (!modal) {
        createFavoritesModalElement();
        modal = document.getElementById('favoritesModal');
    }

    renderFavoritesModalContent();
    modal.style.display = 'flex';
    document.body.classList.add('modal-open');
}

function closeFavoritesModal() {
    const modal = document.getElementById('favoritesModal');
    if (modal) modal.style.display = 'none';
    document.body.classList.remove('modal-open');
}

function renderFavoritesModalContent() {
    const container = document.getElementById('favoritesModalBody') || document.getElementById('favoritesList');
    if (!container) return;

    try {
        const favIds = getFavoriteIds();
        const prods = getAvailableProducts();
        const items = favIds.map(id => prods.find(p => Number(p.id) === id)).filter(Boolean);

        if (items.length === 0) {
            container.innerHTML = `
                <div style="text-align: center; padding: 50px 20px; color: var(--muted);">
                    <div class="empty-ico" style="font-size: 42px; margin-bottom: 12px;">${(typeof EZ_ICON === 'function') ? EZ_ICON('heart') : '🤍'}</div>
                    <h3 style="color: var(--text-strong); margin-bottom: 8px;">Список избранного пуст</h3>
                    <p style="font-size: 14px;">Нажимайте на сердечко на карточках телефонов, чтобы сохранить понравившиеся модели!</p>
                </div>
            `;
            return;
        }

        container.innerHTML = `
            <div class="favorites-grid">
                ${items.map(p => {
                    let img = p.img ? String(p.img).trim().replace(/^~[\/\\]/, '/') : '';
                    if (!img.startsWith('/') && !img.startsWith('http')) img = '/' + img;
                    const priceFormatted = Number(p.price || 0).toLocaleString();
                    return `
                    <div class="fav-card">
                        <img src="${escapeHtml(img)}" alt="${escapeHtml(p.name)}" class="fav-thumb" onerror="this.src='/img/logo.png'">
                        <div class="fav-info">
                            <h4>${escapeHtml(p.name)}</h4>
                            <div class="fav-specs">${p.ram || 8}/${p.storage || 128} ГБ • ${escapeHtml(p.color || '')} • ${p.inStock !== false ? '<span style="color:var(--success)">В наличии</span>' : '<span style="color:var(--danger)">Нет в наличии</span>'}</div>
                            <div class="fav-price">${priceFormatted} сом</div>
                        </div>
                        <div class="fav-actions">
                            ${p.inStock !== false
                                ? `<button class="auth-btn" style="padding: 8px 14px; font-size: 13px;" onclick="addToCart(getAvailableProducts().find(x => x.id === ${p.id}))">В корзину</button>`
                                : `<button class="auth-btn" style="padding: 8px 14px; font-size: 13px;" disabled>Нет в наличии</button>`
                            }
                            <button class="btn-fav-remove" onclick="toggleFavorite(${p.id})">Убрать</button>
                        </div>
                    </div>`;
                }).join('')}
            </div>
        `;
    } catch (err) {
        console.error('Error rendering favorites modal:', err);
    }
}

function createFavoritesModalElement() {
    if (document.getElementById('favoritesModal')) return;
    const div = document.createElement('div');
    div.className = 'modal-overlay';
    div.id = 'favoritesModal';
    div.style.display = 'none';
    div.innerHTML = `
        <div class="modal-content" style="max-width: 650px;">
            <div class="modal-header">
                <h3>Избранные смартфоны</h3>
                <button class="modal-close" onclick="closeFavoritesModal()">&times;</button>
            </div>
            <div id="favoritesModalBody" style="padding: 24px; max-height: 70vh; overflow-y: auto;"></div>
        </div>
    `;
    document.body.appendChild(div);
}

document.addEventListener('DOMContentLoaded', () => {
    createFavoritesModalElement();
    updateFavoritesUI();
    initFavoritesFromServer();
});
