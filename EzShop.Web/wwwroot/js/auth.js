// ==========================================
// EzShop - Authentication UI & AJAX Sync (C# Backend Powered)
// ==========================================

let currentUser = null;

// Built-in Admin Configuration
const ADMIN_CONFIG = {
    email: 'admin@ezshop.kg',
    username: 'Главный Администратор',
    role: 'admin'
};

// Проверка прав администратора
function isAdmin(user = currentUser) {
    if (!user) return false;
    const emailMatch = user.email && user.email.toLowerCase() === ADMIN_CONFIG.email.toLowerCase();
    const roleMatch = (typeof user.role === 'string' && user.role.toLowerCase() === 'admin') || 
                      (typeof user.user_metadata?.role === 'string' && user.user_metadata.role.toLowerCase() === 'admin');
    const userMatch = user.username && (user.username.toLowerCase() === 'admin' || user.username.toLowerCase() === 'главный администратор');
    return !!(emailMatch || roleMatch || userMatch);
}

// Показ / скрытие элементов администратора в шапке
function updateAdminUI() {
    const adminLinks = document.querySelectorAll('.admin-only-nav');
    const userIsAdmin = isAdmin(currentUser);

    adminLinks.forEach(link => {
        if (userIsAdmin) {
            link.classList.add('visible');
            link.style.display = 'inline-flex';
        } else {
            link.classList.remove('visible');
            link.style.display = 'none';
        }
    });
}

// Инициализация авторизационного состояния на странице через C# API
async function initAuth() {
    // 1. Проверяем контекст, переданный из Razor layout
    if (!currentUser && window.currentUser) {
        currentUser = window.currentUser;
    }
    if (!currentUser) {
        try {
            const saved = localStorage.getItem('ez_user');
            if (saved) currentUser = JSON.parse(saved);
        } catch (e) {}
    }

    // 2. Опрашиваем серверный ASP.NET Core Action /Account/CurrentUser через AJAX
    if (!currentUser) {
        try {
            const resp = await fetch('/Account/CurrentUser', { credentials: 'include' });
            if (resp.ok) {
                const data = await resp.json();
                if (data && data.isAuthenticated && data.user) {
                    currentUser = data.user;
                    window.currentUser = currentUser;
                    try { localStorage.setItem('ez_user', JSON.stringify(currentUser)); } catch (e) {}
                }
            }
        } catch (e) {
            console.warn('API /api/auth/me request failed:', e);
        }
    }

    const path = window.location.pathname;
    const isLoginPage = path.includes('login.html') || path.toLowerCase().includes('/account/login') || path.toLowerCase() === '/login';
    const isAdminPage = path.includes('admin.html') || path.includes('admin-orders.html') || path.toLowerCase().includes('/admin');

    // 3. Защита страниц админки: если пользователь не залогинен, отправляем на форму входа
    if (!currentUser && isAdminPage) {
        window.location.replace('/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname));
        return;
    }

    // 4. Защита роли администратора
    if (isAdminPage && !isAdmin(currentUser)) {
        if (typeof showToast === 'function') {
            showToast('Доступ запрещен. Страница доступна только администратору!', 'error');
        }
        setTimeout(() => {
            window.location.replace('/');
        }, 1200);
        return;
    }

    // Отображаем имя пользователя в шапке
    const usernameEl = document.getElementById('usernameDisplay');
    if (usernameEl && currentUser) {
        const adminBadge = isAdmin(currentUser) ? ' 👑 (Admin)' : '';
        usernameEl.textContent = `${currentUser.username}${adminBadge}`;
    }

    updateAdminUI();

    // Загрузка корзины
    if (typeof loadCart === 'function') loadCart();

    // Избранное и сравнение
    if (typeof updateFavoritesUI === 'function') updateFavoritesUI();
    if (typeof updateCompareBar === 'function') updateCompareBar();

    // Инициализация Admin Dashboard (C# API)
    if ((path.includes('admin.html') || path.toLowerCase().includes('/admin')) && !path.includes('admin-orders') && !path.includes('Orders') && typeof initAdminDashboard === 'function') {
        initAdminDashboard();
    }

    // Инициализация Admin Orders (C# API)
    if (path.includes('admin-orders.html') && typeof initAdminOrdersPage === 'function') {
        initAdminOrdersPage();
    }

    // Инициализация Product Details при открытии карточки товара
    if ((path.includes('product.html') || path.includes('product') || document.getElementById('productDetailContainer')) && typeof initProductPage === 'function') {
        initProductPage();
    }

    // Скрываем лоадер страницы
    const loader = document.getElementById('pageLoader');
    if (loader) loader.classList.add('hidden');
    document.body.classList.remove('auth-loading');
}

// Переключение видимости пароля (скрыть / показать глазком)
function togglePasswordVisibility(fieldId, iconId) {
    const field = document.getElementById(fieldId);
    const icon = document.getElementById(iconId);
    if (!field) return;

    if (field.type === 'password') {
        field.type = 'text';
        if (icon) icon.classList.add('active');
    } else {
        field.type = 'password';
        if (icon) icon.classList.remove('active');
    }
}

// Выход из системы
async function logout() {
    try {
        localStorage.removeItem('ez_user');
    } catch (e) {}
    currentUser = null;

    // Отправляем запрос на завершение сессии в C# Identity
    const logoutForm = document.createElement('form');
    logoutForm.method = 'POST';
    logoutForm.action = '/Account/Logout';
    document.body.appendChild(logoutForm);
    logoutForm.submit();
}

// Синхронизация состояния автозаполнения браузера (анимация плавающих меток)
function syncAutofillStates() {
    document.querySelectorAll('.auth-input').forEach(input => {
        let isFilled = false;
        try {
            isFilled = Boolean(input.value) || input.matches(':-webkit-autofill');
        } catch (_) {
            isFilled = Boolean(input.value);
        }
        if (isFilled) {
            input.setAttribute('data-filled', 'true');
        } else {
            input.removeAttribute('data-filled');
        }
    });
}

if (typeof window !== 'undefined') {
    ['DOMContentLoaded', 'load', 'pageshow'].forEach(evt => {
        window.addEventListener(evt, () => {
            syncAutofillStates();
            setTimeout(syncAutofillStates, 100);
            setTimeout(syncAutofillStates, 400);
            setTimeout(syncAutofillStates, 1000);
        });
    });
    document.addEventListener('input', e => {
        if (e.target && e.target.classList && e.target.classList.contains('auth-input')) {
            syncAutofillStates();
        }
    });
    document.addEventListener('change', e => {
        if (e.target && e.target.classList && e.target.classList.contains('auth-input')) {
            syncAutofillStates();
        }
    });
}

// Запуск инициализации при загрузке документа
document.addEventListener('DOMContentLoaded', () => {
    initAuth();
});
