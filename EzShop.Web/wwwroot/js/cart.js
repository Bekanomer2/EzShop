// ==========================================
// EzShop - Cart System (PostgreSQL DB Powered)
// ==========================================

let cart = [];
const MAX_CART_QTY = 10;

function getCartStorageKey() {
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
    return (uid > 0) ? ('cart_' + uid) : 'cart_guest';
}

function updateCartIcon() {
    const count = (cart || []).reduce((s, i) => s + (i.qty || 1), 0);
    document.querySelectorAll('.nav-count.nav-cart-count, #cartCount, #navCartCount').forEach(el => {
        el.textContent = count;
    });
    const floatingBtn = document.getElementById('cartBtn');
    if (floatingBtn) {
        const countEl = floatingBtn.querySelector('#cartCount');
        if (countEl) countEl.textContent = count;
    }
}

function saveLocalCart() {
    const key = getCartStorageKey();
    try {
        localStorage.setItem(key, JSON.stringify(cart));
    } catch (e) {}
    updateCartIcon();
    if (document.getElementById('items')) renderCart();
}

async function loadCart() {
    const key = getCartStorageKey();
    const isAuth = !!(window.currentUserId && Number(window.currentUserId) > 0);

    // 1. Быстро читаем из локального кэша для мгновенного отклика UI
    try {
        cart = JSON.parse(localStorage.getItem(key)) || [];
    } catch (e) {
        cart = [];
    }
    updateCartIcon();
    if (window.location.pathname.toLowerCase().includes('cart')) {
        renderCart();
    }

    // 2. Если пользователь авторизован, синхронизируем с БД (PostgreSQL)
    if (isAuth) {
        try {
            // Если есть гостевая корзина перед логином, отправим ее на сервер для слияния
            const guestCart = JSON.parse(localStorage.getItem('cart_guest') || '[]');
            if (Array.isArray(guestCart) && guestCart.length > 0) {
                const syncPayload = {
                    items: guestCart.map(i => ({ productId: Number(i.id), qty: Number(i.qty || 1) }))
                };
                const syncRes = await fetch('/Cart/SyncGuestCart', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(syncPayload)
                });
                if (syncRes.ok) {
                    const data = await syncRes.json();
                    if (data.items && Array.isArray(data.items)) {
                        cart = data.items;
                        localStorage.removeItem('cart_guest');
                        saveLocalCart();
                        return;
                    }
                }
            }

            // Загружаем актуальную корзину пользователя из PostgreSQL
            const res = await fetch('/Cart/GetMyCart');
            if (res.ok) {
                const data = await res.json();
                if (data.success && Array.isArray(data.items)) {
                    cart = data.items;
                    saveLocalCart();
                }
            }
        } catch (err) {
            console.warn('Не удалось загрузить корзину из БД:', err);
        }
    }
}

async function addToCart(p, qty = 1) {
    if (!p) return;
    if (typeof p === 'number' || typeof p === 'string') {
        const id = parseInt(p, 10);
        const list = (window.productsData && Array.isArray(window.productsData)) 
            ? window.productsData 
            : ((typeof productsData !== 'undefined' && Array.isArray(productsData)) ? productsData : []);
        p = list.find(x => x.id === id);
    }
    if (!p) return;
    if (p.inStock === false) {
        if (typeof showToast === 'function') {
            showToast('К сожалению, этого товара сейчас нет в наличии', 'error');
        }
        return;
    }

    if (!Array.isArray(cart)) cart = [];
    const item = cart.find(i => i.id === p.id);
    if (item) {
        item.qty = Math.min(MAX_CART_QTY, item.qty + qty);
    } else {
        cart.push({ ...p, qty });
    }

    saveLocalCart();
    if (typeof showToast === 'function') {
        showToast(`${p.name} добавлен в корзину (${qty} шт.) 🛒`, 'success');
    }

    const cartBtn = document.getElementById('cartBtn');
    if (cartBtn) {
        cartBtn.classList.remove('bump');
        void cartBtn.offsetWidth;
        cartBtn.classList.add('bump');
    }

    // Сохраняем в PostgreSQL если пользователь вошел
    if (window.currentUserId && Number(window.currentUserId) > 0) {
        fetch('/Cart/AddItem', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId: p.id, qty })
        }).catch(e => console.warn('Ошибка сохранения товара в БД корзины:', e));
    }
}

function quickBuy(p, qty = 1) {
    if (!p) return;
    if (typeof p === 'number' || typeof p === 'string') {
        const id = parseInt(p, 10);
        const list = (typeof productsData !== 'undefined' && Array.isArray(productsData)) ? productsData : [];
        p = list.find(x => x.id === id);
    }
    if (!p) return;
    if (p.inStock === false) {
        if (typeof showToast === 'function') {
            showToast('К сожалению, этого товара сейчас нет в наличии', 'error');
        }
        return;
    }
    addToCart(p, qty);
    setTimeout(() => {
        window.location.href = '/Cart';
    }, 400);
}

// Cart Page Rendering
function renderCart() {
    const itemsEl = document.getElementById('items');
    const totalEl = document.getElementById('total');
    if (!itemsEl || !totalEl) return;

    itemsEl.innerHTML = '';
    let total = 0;

    if (cart.length === 0) {
        const cartIconSvg = typeof ico === 'function' ? ico('cart') : '<svg class="ico" viewBox="0 0 24 24"><path d="M2.05 2.05h2l2.66 12.42a2 2 0 0 0 2 1.58h9.78a2 2 0 0 0 1.95-1.57l1.65-7.43H5.12"/></svg>';
        itemsEl.innerHTML = `
            <div class="cart-empty">
                <div class="empty-ico">${cartIconSvg}</div>
                <h3>Ваша корзина пуста</h3>
                <p>Добавьте смартфоны из каталога — они появятся здесь.</p>
                <a href="/" class="btn btn-primary">Перейти в каталог</a>
            </div>`;
        totalEl.innerHTML = '<span>Итого:</span><b>0 сом</b>';
        const checkoutBtn = document.getElementById('checkout');
        if (checkoutBtn) checkoutBtn.style.display = 'none';
        const orderBlock = document.getElementById('order');
        if (orderBlock) orderBlock.style.display = 'none';
        return;
    }

    const orderBlock = document.getElementById('order');
    const isOrderOpen = orderBlock && (orderBlock.style.display === 'block' || orderBlock.style.display === 'flex');
    const checkoutBtn = document.getElementById('checkout');
    if (checkoutBtn) checkoutBtn.style.display = isOrderOpen ? 'none' : 'inline-flex';

    cart.forEach(item => {
        total += item.price * item.qty;
        const row = document.createElement('div');
        row.className = 'cart-item';
        row.style.setProperty('--i', Math.min(cart.indexOf(item), 8));
        const trashSvg = typeof ico === 'function' ? ico('trash') : '✕';
        row.innerHTML = `
            <img class="cart-thumb" src="${item.img}" alt="${item.name}">
            <div class="cart-item-info">
                <h4>${item.name}</h4>
                <small>${item.price.toLocaleString()} сом</small>
            </div>
            <div class="cart-qty-ctrl">
                <button type="button" onclick="updateQty(${item.id}, -1)" aria-label="Уменьшить количество">−</button>
                <span>${item.qty}</span>
                <button type="button" onclick="updateQty(${item.id}, 1)" aria-label="Увеличить количество">+</button>
            </div>
            <div class="cart-line-total">${(item.price * item.qty).toLocaleString()} сом</div>
            <button type="button" onclick="removeFromCart(${item.id})" class="btn-remove-item" aria-label="Удалить из корзины" title="Удалить">${trashSvg}</button>
        `;
        itemsEl.appendChild(row);
    });

    totalEl.innerHTML = `<span>Итого:</span><b>${total.toLocaleString()} сом</b>`;
}

function updateQty(id, delta) {
    const item = cart.find(i => i.id === id);
    if (!item) return;
    item.qty = Math.min(MAX_CART_QTY, item.qty + delta);
    if (item.qty <= 0) {
        cart = cart.filter(i => i.id !== id);
    }
    saveLocalCart();
    renderCart();

    if (window.currentUserId && Number(window.currentUserId) > 0) {
        fetch('/Cart/SetQty', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId: id, qty: Math.max(0, item.qty) })
        }).catch(e => console.warn(e));
    }
}

function removeFromCart(id) {
    cart = cart.filter(i => i.id !== id);
    saveLocalCart();
    renderCart();
    if (typeof showToast === 'function') {
        showToast('Товар удален из корзины', 'info');
    }

    if (window.currentUserId && Number(window.currentUserId) > 0) {
        fetch('/Cart/RemoveItem', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId: id })
        }).catch(e => console.warn(e));
    }
}

// Checkout Logic
function openCheckoutForm() {
    const checkoutBtn = document.getElementById('checkout');
    const orderBlock = document.getElementById('order');
    if (typeof cart !== 'undefined' && cart.length === 0) return;
    if (typeof currentUser === 'undefined' || !currentUser) {
        if (typeof showToast === 'function') {
            showToast('Для оформления заказа необходимо авторизоваться', 'error');
        }
        setTimeout(() => {
            window.location.href = '/Account/Login?returnUrl=/Cart';
        }, 600);
        return;
    }
    if (checkoutBtn) {
        checkoutBtn.style.display = 'none';
    }
    if (orderBlock) {
        orderBlock.style.display = 'flex';
        orderBlock.scrollIntoView({ behavior: 'smooth' });
    }
}
window.openCheckoutForm = openCheckoutForm;

document.addEventListener('DOMContentLoaded', () => {
    loadCart();

    const checkoutBtn = document.getElementById('checkout');
    if (checkoutBtn) {
        checkoutBtn.onclick = openCheckoutForm;
    }

    const orderBlock = document.getElementById('order');
    const sendBtn = document.getElementById('send');

    if (sendBtn) {
        sendBtn.onclick = async () => {
            const name = document.getElementById('name').value.trim();
            const phone = document.getElementById('phone').value.trim();
            const address = document.getElementById('address').value.trim();

            if (name.length < 2) {
                showToast('Пожалуйста, укажите корректное имя (не менее 2 букв)', 'error');
                return;
            }

            const cleanPhone = phone.replace(/[\s\-\(\)\.]/g, '');
            const isKgPlus = /^\+996[23579]\d{8}$/.test(cleanPhone) || /^\+996\d{9}$/.test(cleanPhone);
            const isKgWithoutPlus = /^996[23579]\d{8}$/.test(cleanPhone) || /^996\d{9}$/.test(cleanPhone);
            const isKgLocal = /^0[23579]\d{8}$/.test(cleanPhone) || /^0\d{9}$/.test(cleanPhone);

            if (!isKgPlus && !isKgWithoutPlus && !isKgLocal) {
                showToast('Номер телефона должен быть в формате Кыргызстана (+996XXXXXXXXX или 0XXXXXXXXX)', 'error');
                document.getElementById('phone')?.focus();
                return;
            }

            const hasLetters = /[a-zA-Zа-яА-ЯёЁ]/.test(address);
            const hasDigits = /\d/.test(address);

            if (!hasLetters || !hasDigits) {
                showToast('Пожалуйста, укажите полный адрес (улицу и номер дома)', 'error');
                return;
            }

            sendBtn.disabled = true;
            sendBtn.textContent = 'Оформляем...';

            try {
                // 1. Отправка заказа на C# ASP.NET Core бэкенд
                const response = await fetch('/Orders/CreateFromCart', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                    },
                    body: JSON.stringify({
                        customerName: name,
                        customerPhone: phone,
                        customerAddress: address,
                        items: cart.map(i => ({ id: i.id, name: i.name, price: i.price, qty: i.qty }))
                    })
                });

                if (response.ok) {
                    const data = await response.json();
                    showToast(`Заказ #${data.orderId || ''} успешно оформлен!`, 'success');
                    
                    // Очищаем корзину в локальном стейте и на сервере
                    cart = [];
                    saveLocalCart();
                    if (window.currentUserId && Number(window.currentUserId) > 0) {
                        fetch('/Cart/Clear', { method: 'POST' }).catch(() => {});
                    }

                    if (orderBlock) orderBlock.style.display = 'none';

                    setTimeout(() => {
                        window.location.href = '/Orders';
                    }, 1000);
                } else {
                    const err = await response.json().catch(() => ({}));
                    showToast(err.message || 'Ошибка оформления заказа', 'error');
                    sendBtn.disabled = false;
                    sendBtn.textContent = 'Подтвердить заказ';
                }
            } catch (err) {
                console.error('Order creation error:', err);
                showToast('Ошибка оформления заказа: ' + err.message, 'error');
                sendBtn.disabled = false;
                sendBtn.textContent = 'Подтвердить заказ';
            }
        };
    }
});
