// ==========================================
// EzShop - Global Client Utilities & Loader Controller
// ==========================================

// Глобальные функции экранирования текста
window.escapeHtml = function (value) {
    return String(value ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');
};
window.esc = window.escapeHtml;

// Управление анимацией загрузки с логотипом
const loaderStartTime = Date.now();
let loaderDismissed = false;

function dismissLoader() {
    if (loaderDismissed) return;
    const loader = document.getElementById('pageLoader');
    if (!loader) {
        loaderDismissed = true;
        document.body.classList.remove('auth-loading');
        return;
    }

    const elapsed = Date.now() - loaderStartTime;
    const minDisplayMs = 700; // Минимальная приятная длительность показа логотипа и прогресс-бара
    const remaining = Math.max(0, minDisplayMs - elapsed);

    setTimeout(() => {
        loader.classList.add('hidden');
        document.body.classList.remove('auth-loading');
        loaderDismissed = true;
    }, remaining);
}

// Завершаем лоадер после полной загрузки страницы или по безопасному таймауту
window.addEventListener('load', dismissLoader);
setTimeout(dismissLoader, 1400);

// Глобальные Toast-уведомления (1:1 как в vibe-main)
let activeToastTimer = null;
window.showToast = function (msg, type = 'success') {
    let container = document.getElementById('toastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toastContainer';
        container.className = 'toast-container';
        document.body.appendChild(container);
    }

    if (activeToastTimer) {
        clearTimeout(activeToastTimer);
        activeToastTimer = null;
    }
    container.innerHTML = '';

    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    const icon = type === 'success' ? '✅' : (type === 'info' ? 'ℹ️' : '⚠️');
    toast.innerHTML = `${icon} <span>${msg}</span>`;
    container.appendChild(toast);

    activeToastTimer = setTimeout(() => {
        toast.classList.add('is-hidden');
        toast.addEventListener('animationend', () => toast.remove());
    }, 3000);
};

// Переключение видимости пароля
window.togglePassword = function (inputId) {
    const input = document.getElementById(inputId);
    if (!input) return;
    const btn = input.parentElement ? input.parentElement.querySelector('.toggle-password') : null;
    const isPass = input.type === 'password';
    input.type = isPass ? 'text' : 'password';
    if (btn && typeof window.EZ_ICON === 'function') {
        btn.innerHTML = window.EZ_ICON(isPass ? 'eye-off' : 'eye');
        btn.setAttribute('aria-label', isPass ? 'Скрыть пароль' : 'Показать пароль');
    }
};
