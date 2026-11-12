// ==========================================
// EzShop - UI-слой: тема, навигация, анимации
// (не содержит бизнес-логики магазина)
// ==========================================
(function () {
    'use strict';

    const $ = (s, r = document) => r.querySelector(s);
    const $$ = (s, r = document) => Array.from(r.querySelectorAll(s));
    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const finePointer = window.matchMedia('(hover: hover) and (pointer: fine)').matches;
    const icon = (n, c) => (window.EZ_ICON ? window.EZ_ICON(n, c) : '');

    /* ---------- Тема ---------- */
    function currentTheme() {
        return document.documentElement.getAttribute('data-theme') === 'light' ? 'light' : 'dark';
    }

    function setTheme(theme) {
        const root = document.documentElement;
        root.classList.add('theme-anim');
        root.setAttribute('data-theme', theme);
        try { localStorage.setItem('ez_theme', theme); } catch (e) { /* ignore */ }
        const meta = $('meta[name="theme-color"]');
        if (meta) meta.setAttribute('content', theme === 'light' ? '#f3f5fb' : '#070b1a');
        $$('.theme-toggle').forEach(b => b.setAttribute('aria-pressed', String(theme === 'light')));
        window.dispatchEvent(new CustomEvent('ez:themechange', { detail: { theme } }));
        setTimeout(() => root.classList.remove('theme-anim'), 450);
    }

    function createThemeToggle(extraClass) {
        const b = document.createElement('button');
        b.type = 'button';
        b.className = 'theme-toggle' + (extraClass ? ' ' + extraClass : '');
        b.setAttribute('aria-label', 'Переключить тему: светлая или тёмная');
        b.setAttribute('aria-pressed', String(currentTheme() === 'light'));
        b.title = 'Светлая / тёмная тема';
        b.innerHTML = icon('moon', 'ico-moon') + icon('sun', 'ico-sun');
        b.addEventListener('click', () => setTheme(currentTheme() === 'light' ? 'dark' : 'light'));
        return b;
    }

    function initThemeToggle() {
        const panel = $('.user-panel');
        if (panel) {
            panel.insertBefore(createThemeToggle(), panel.firstChild);
        } else {
            document.body.appendChild(createThemeToggle('floating'));
        }
    }

    /* ---------- Навигация: иконки, активная ссылка, счётчики ---------- */
    function initNavIcons() {
        $$('[data-icon]').forEach(el => {
            if (el.querySelector(':scope > svg')) return;
            const inNav = !!el.closest('nav');
            if (el.tagName === 'A') el.insertAdjacentHTML('afterbegin', icon(el.dataset.icon, inNav ? 'nav-ico' : ''));
            else el.innerHTML = icon(el.dataset.icon);
        });
        const here = (location.pathname.split('/').pop() || 'index.html').toLowerCase();
        $$('nav a[href]').forEach(a => {
            const href = (a.getAttribute('href') || '').split('#')[0].toLowerCase();
            if (!href || href.startsWith('javascript')) return;
            if (href === here || (here === '' && href === 'index.html')) a.classList.add('is-active');
        });
    }

    function watchCounters() {
        const sync = el => {
            const n = parseInt(el.textContent, 10) || 0;
            el.classList.toggle('has-items', n > 0);
        };
        const els = $$('.nav-count');
        els.forEach(el => {
            sync(el);
            new MutationObserver(() => sync(el)).observe(el, { childList: true, characterData: true, subtree: true });
        });
    }

    /* ---------- Нижняя навигация (мобильная) ---------- */
    function initBottomNav() {
        if (!$('#nav') || $('.admin-header') || $('.bottom-nav')) return;
        const here = (location.pathname.split('/').pop() || 'index.html').toLowerCase();
        const items = [
            { href: 'index.html', ico: 'grid', label: 'Каталог' },
            { act: 'fav', ico: 'heart', label: 'Избранное', badge: 'fav' },
            { href: 'cart.html', ico: 'cart', label: 'Корзина', badge: 'cart' },
            { href: 'orders.html', ico: 'package', label: 'Заказы' },
            { act: 'menu', ico: 'menu', label: 'Меню' }
        ];
        const nav = document.createElement('nav');
        nav.className = 'bottom-nav';
        nav.setAttribute('aria-label', 'Основная навигация');
        nav.innerHTML = items.map(it => {
            const inner = icon(it.ico) + '<span>' + it.label + '</span>' +
                (it.badge ? '<i class="bn-badge" data-badge="' + it.badge + '"></i>' : '');
            if (it.href) {
                const active = it.href === here ? ' is-active' : '';
                return '<a href="' + it.href + '" class="' + active.trim() + '">' + inner + '</a>';
            }
            return '<button type="button" data-act="' + it.act + '">' + inner + '</button>';
        }).join('');
        document.body.appendChild(nav);

        nav.addEventListener('click', e => {
            const btn = e.target.closest('button[data-act]');
            if (!btn) return;
            if (btn.dataset.act === 'fav' && typeof window.openFavoritesModal === 'function') window.openFavoritesModal();
            if (btn.dataset.act === 'menu') $('#burger') && $('#burger').click();
        });

        // Бейджи берём из уже существующих счётчиков, чтобы не дублировать логику
        const setBadge = (key, n) => {
            const b = $('.bn-badge[data-badge="' + key + '"]', nav);
            if (!b) return;
            b.textContent = n > 99 ? '99+' : String(n);
            b.classList.toggle('show', n > 0);
        };
        const favEl = $('.nav-fav-count');
        if (favEl) {
            const f = () => setBadge('fav', parseInt(favEl.textContent, 10) || 0);
            f();
            new MutationObserver(f).observe(favEl, { childList: true, characterData: true, subtree: true });
        }
        const cartEl = $('#cartCount');
        if (cartEl) {
            const c = () => setBadge('cart', parseInt(cartEl.textContent, 10) || 0);
            c();
            new MutationObserver(c).observe(cartEl, { childList: true, characterData: true, subtree: true });
        }
    }

    /* ---------- Прогресс скролла ---------- */
    function initScrollProgress() {
        if ($('.scroll-progress')) return;
        const bar = document.createElement('div');
        bar.className = 'scroll-progress';
        document.body.appendChild(bar);
        let ticking = false;
        const update = () => {
            const h = document.documentElement.scrollHeight - window.innerHeight;
            bar.style.transform = 'scaleX(' + (h > 0 ? Math.min(1, window.scrollY / h) : 0) + ')';
            ticking = false;
        };
        window.addEventListener('scroll', () => {
            if (!ticking) { ticking = true; requestAnimationFrame(update); }
        }, { passive: true });
        update();
    }

    /* ---------- Появление при скролле ---------- */
    let revealObserver = null;
    function initReveal() {
        if (!('IntersectionObserver' in window) || reduceMotion) {
            $$('[data-reveal]').forEach(el => el.classList.add('in-view'));
            return;
        }
        revealObserver = new IntersectionObserver(entries => {
            entries.forEach(en => {
                if (en.isIntersecting) {
                    en.target.classList.add('in-view');
                    revealObserver.unobserve(en.target);
                }
            });
        }, { threshold: 0.12, rootMargin: '0px 0px -6% 0px' });
        const scan = () => $$('[data-reveal]:not([data-rv])').forEach(el => {
            el.setAttribute('data-rv', '1');
            revealObserver.observe(el);
        });
        scan();
        new MutationObserver(scan).observe(document.body, { childList: true, subtree: true });
    }

    /* ---------- Наклон карточек и блик за курсором ---------- */
    function initTilt() {
        if (!finePointer || reduceMotion) return;
        document.addEventListener('pointermove', e => {
            const card = e.target.closest && e.target.closest('.products .card');
            if (!card) return;
            const r = card.getBoundingClientRect();
            const x = (e.clientX - r.left) / r.width;
            const y = (e.clientY - r.top) / r.height;
            card.style.setProperty('--mx', (x * 100).toFixed(1) + '%');
            card.style.setProperty('--my', (y * 100).toFixed(1) + '%');
            card.style.setProperty('--ry', ((x - 0.5) * 7).toFixed(2) + 'deg');
            card.style.setProperty('--rx', ((0.5 - y) * 7).toFixed(2) + 'deg');
            card.classList.add('is-tilting');
        }, { passive: true });
        document.addEventListener('pointerout', e => {
            const card = e.target.closest && e.target.closest('.products .card');
            if (!card || card.contains(e.relatedTarget)) return;
            card.classList.remove('is-tilting');
            card.style.setProperty('--rx', '0deg');
            card.style.setProperty('--ry', '0deg');
        });
    }

    /* ---------- Ripple на кнопках ---------- */
    const RIPPLE_SEL = '.btn-primary, .btn-add-cart, .btn-buy-primary, .btn-buy-instant, .auth-btn, .btn-details, .admin-btn';
    function initRipple() {
        if (reduceMotion) return;
        document.addEventListener('pointerdown', e => {
            const btn = e.target.closest && e.target.closest(RIPPLE_SEL);
            if (!btn || btn.disabled) return;
            const r = btn.getBoundingClientRect();
            const size = Math.max(r.width, r.height);
            const span = document.createElement('span');
            span.className = 'ripple';
            span.style.width = span.style.height = size + 'px';
            span.style.left = (e.clientX - r.left - size / 2) + 'px';
            span.style.top = (e.clientY - r.top - size / 2) + 'px';
            if (getComputedStyle(btn).position === 'static') btn.style.position = 'relative';
            btn.style.overflow = 'hidden';
            btn.appendChild(span);
            span.addEventListener('animationend', () => span.remove());
        }, { passive: true });
    }

    /* ---------- Полёт товара в корзину ---------- */
    function cartTarget() {
        const floating = $('.cart-btn');
        if (floating) {
            const r = floating.getBoundingClientRect();
            if (r.width > 0 && r.height > 0 && window.getComputedStyle(floating).display !== 'none') {
                return floating;
            }
        }
        const bottomNavCart = $('.bottom-nav a[href="cart.html"]');
        if (bottomNavCart) {
            const r = bottomNavCart.getBoundingClientRect();
            if (r.width > 0 && r.height > 0 && window.getComputedStyle(bottomNavCart).display !== 'none') {
                return bottomNavCart;
            }
        }
        const navCart = $('.main-nav a[href="cart.html"]');
        if (navCart) {
            const r = navCart.getBoundingClientRect();
            if (r.width > 0 && r.height > 0 && window.getComputedStyle(navCart).display !== 'none') {
                return navCart;
            }
        }
        return floating || document.querySelector('#cartBtn');
    }

    function bump(el) {
        if (!el) return;
        el.classList.remove('bump');
        void el.offsetWidth;
        el.classList.add('bump');
    }

    function initFlyToCart() {
        document.addEventListener('click', e => {
            const btn = e.target.closest && e.target.closest('.btn-add-cart:not(:disabled), .btn-buy-primary:not(:disabled)');
            if (!btn || reduceMotion || typeof Element.prototype.animate !== 'function') return;
            const card = btn.closest('.card');
            const img = card ? $('img', card) : $('#mainProductImg');
            const target = cartTarget();
            if (!img || !target) return;
            const from = img.getBoundingClientRect();
            const to = target.getBoundingClientRect();
            if (to.width === 0 && to.height === 0) return;
            const size = Math.min(from.width, from.height, 140);
            const fly = document.createElement('img');
            fly.src = img.currentSrc || img.src;
            fly.className = 'fly-img';
            Object.assign(fly.style, {
                left: (from.left + from.width / 2 - size / 2) + 'px',
                top: (from.top + from.height / 2 - size / 2) + 'px',
                width: size + 'px',
                height: size + 'px'
            });
            document.body.appendChild(fly);
            const dx = (to.left + to.width / 2) - (from.left + from.width / 2);
            const dy = (to.top + to.height / 2) - (from.top + from.height / 2);
            const anim = fly.animate([
                { transform: 'translate(0,0) scale(1)', opacity: 0.95, offset: 0 },
                { transform: 'translate(' + dx * 0.55 + 'px,' + (dy * 0.55 - 70) + 'px) scale(.62)', opacity: 0.9, offset: 0.55 },
                { transform: 'translate(' + dx + 'px,' + dy + 'px) scale(.08)', opacity: 0.15, offset: 1 }
            ], { duration: 720, easing: 'cubic-bezier(.5,0,.3,1)' });
            anim.onfinish = () => { fly.remove(); bump(target); };
        });
    }

    /* ---------- Мобильное меню: затемнение и блокировка прокрутки ---------- */
    function initNavScrim() {
        const nav = $('#nav');
        const burger = $('#burger');
        if (!nav || !burger) return;
        let scrim = $('.nav-scrim');
        if (!scrim) {
            scrim = document.createElement('div');
            scrim.className = 'nav-scrim';
            document.body.appendChild(scrim);
        }
        const sync = () => {
            const open = nav.classList.contains('active');
            scrim.classList.toggle('show', open);
            document.body.style.overflow = open ? 'hidden' : '';
            burger.setAttribute('aria-expanded', String(open));
        };
        new MutationObserver(sync).observe(nav, { attributes: true, attributeFilter: ['class'] });
        burger.addEventListener('click', e => {
            e.preventDefault();
            e.stopPropagation();
            const isOpen = nav.classList.toggle('active');
            burger.classList.toggle('active', isOpen);
        });
        nav.querySelectorAll('a').forEach(link => {
            link.addEventListener('click', () => {
                nav.classList.remove('active');
                burger.classList.remove('active');
            });
        });
        scrim.addEventListener('click', () => { nav.classList.remove('active'); burger.classList.remove('active'); });
        burger.setAttribute('role', 'button');
        burger.setAttribute('tabindex', '0');
        burger.setAttribute('aria-label', 'Меню');
        burger.addEventListener('keydown', e => {
            if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); burger.click(); }
        });
    }

    /* ---------- Загрузчик ---------- */
    function enhanceLoader() {
        const l = $('#pageLoader');
        if (l && !$('.loader-bar', l)) {
            const bar = document.createElement('div');
            bar.className = 'loader-bar';
            bar.innerHTML = '<span></span>';
            l.appendChild(bar);
        }
    }

    /* ---------- Модальные окна: Esc и клик по фону ---------- */
    function initModals() {
        document.addEventListener('keydown', e => {
            if (e.key !== 'Escape') return;
            $$('.modal-overlay').forEach(m => { if (m.style.display === 'flex') m.style.display = 'none'; });
            document.body.classList.remove('modal-open');
        });
        document.addEventListener('mousedown', e => {
            if (e.target.classList && e.target.classList.contains('modal-overlay')) e.target.style.display = 'none';
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        enhanceLoader();
        initThemeToggle();
        initNavIcons();
        watchCounters();
        initBottomNav();
        initNavScrim();
        initScrollProgress();
        initReveal();
        initTilt();
        initRipple();
        initFlyToCart();
        initModals();
    });

    window.EzUI = { setTheme, currentTheme, bump };
})();
