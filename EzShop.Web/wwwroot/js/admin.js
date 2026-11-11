// ==========================================
// EzShop - Administrator Panel & CRUD System (C# API Powered)
// ==========================================

function escapeHtml(str) {
    if (str == null) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}
window.escapeHtml = escapeHtml;

let adminProductsList = [];
let adminBrandsList = [];

// Switch tabs between Phones, Brands and Reviews
function switchAdminTab(tab) {
    const tabs = ['products', 'brands', 'reviews'];
    tabs.forEach(t => {
        const btn = document.getElementById('tabBtn' + t.charAt(0).toUpperCase() + t.slice(1));
        const content = document.getElementById('tabContent' + t.charAt(0).toUpperCase() + t.slice(1));
        if (btn) btn.classList.toggle('active', t === tab);
        if (content) content.classList.toggle('active', t === tab);
    });

    if (tab === 'brands') {
        renderAdminBrandsList();
    } else if (tab === 'reviews') {
        renderAdminReviews();
    }
}

function renderAdminReviews() {
    const tbody = document.getElementById('adminReviewsTableBody');
    if (!tbody) return;
    const reviews = window.reviewsData || [];
    const countEl = document.getElementById('reviewsListCount');
    if (countEl) countEl.textContent = reviews.length;

    if (reviews.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" style="text-align:center; padding:35px 20px; color:var(--muted);">Отзывов пока нет</td></tr>';
        return;
    }

    tbody.innerHTML = reviews.map(r => {
        const ratingNum = Math.max(1, Math.min(5, Math.round(Number(r.rating) || 5)));
        return `
        <tr id="adminReviewRow_${r.id}">
            <td style="font-weight:700; color:var(--muted);">#${r.id}</td>
            <td><strong style="color:var(--text);">${escapeHtml(r.productName || ('Товар #' + r.productId))}</strong></td>
            <td><span class="admin-badge" style="background:rgba(99,102,241,0.12); color:#6366f1;">${escapeHtml(r.author || 'Гость')}</span></td>
            <td>
                <span style="color:#f59e0b; font-size:14px;">${'★'.repeat(ratingNum)}${'☆'.repeat(5 - ratingNum)}</span>
                <b style="margin-left:4px;">${ratingNum}</b>
            </td>
            <td style="max-width:320px; white-space:normal; font-size:13px; line-height:1.4;">${escapeHtml(r.text || '')}</td>
            <td style="font-size:12px; color:var(--muted); white-space:nowrap;">${r.createdAt || ''}</td>
            <td>
                <button type="button" class="btn-action btn-delete" onclick="deleteReviewFromAdmin(${r.id})" title="Удалить отзыв">
                    🗑️
                </button>
            </td>
        </tr>
        `;
    }).join('');
}

async function deleteReviewFromAdmin(id) {
    if (!confirm(`Вы действительно хотите удалить отзыв #${id}?`)) return;

    try {
        const resp = await fetch(`/Admin/DeleteReview/${id}`, {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        const data = await resp.json().catch(() => ({}));

        if (resp.ok && data.success !== false) {
            const row = document.getElementById(`adminReviewRow_${id}`);
            if (row) row.remove();
            window.reviewsData = (window.reviewsData || []).filter(r => r.id !== id);
            const countEl = document.getElementById('reviewsListCount');
            if (countEl) countEl.textContent = window.reviewsData.length;
            if (window.reviewsData.length === 0) {
                renderAdminReviews();
            }
            showToast(`Отзыв #${id} успешно удален`, 'success');
        } else {
            showToast(data.message || 'Не удалось удалить отзыв', 'error');
        }
    } catch (err) {
        console.error(err);
        showToast('Ошибка при удалении отзыва', 'error');
    }
}

async function initAdminDashboard() {
    await loadAdminDataFromApi();
    renderAdminReviews();
    updateLivePreview();
}

// Загрузка товаров и брендов через C# ASP.NET Core Action
async function loadAdminDataFromApi() {
    // 1. Мгновенная инициализация из каталога (0 мс), чтобы таблица и счётчики никогда не были пустыми
    const localProds = (typeof productsData !== 'undefined' && Array.isArray(productsData) && productsData.length > 0)
        ? productsData
        : ((typeof initialProductsData !== 'undefined' && Array.isArray(initialProductsData)) ? initialProductsData : []);

    const localBrands = (typeof brandsData !== 'undefined' && Array.isArray(brandsData) && brandsData.length > 0)
        ? brandsData
        : ((typeof initialBrandsData !== 'undefined' && Array.isArray(initialBrandsData)) ? initialBrandsData : []);

    if (localProds.length > 0) {
        adminProductsList = [...localProds];
        adminBrandsList = [...localBrands];
        window.productsData = adminProductsList;
        window.brandsData = adminBrandsList;
        updateAdminStats();
        populateBrandSelect();
        renderAdminProductsTable();
        renderAdminBrandsList();
    }

    // 2. Синхронизация с сервером через C# ASP.NET Core Action
    try {
        const resp = await fetch('/Admin/GetProducts', { credentials: 'include' });
        if (resp.ok) {
            const data = await resp.json();
            if (data && Array.isArray(data.products) && data.products.length > 0) {
                adminProductsList = data.products;
                adminBrandsList = data.brands || [];

                window.productsData = adminProductsList;
                window.brandsData = adminBrandsList;

                updateAdminStats(data);
                populateBrandSelect();
                renderAdminProductsTable();
                renderAdminBrandsList();
            }
        }
    } catch (err) {
        console.error('Failed to load admin data from C# API:', err);
    }
}

function updateAdminStats(data = null) {
    const totalCountEl = document.getElementById('statTotalCount');
    const brandCountEl = document.getElementById('statBrandCount');
    const inEl = document.getElementById('statInStock');
    const outEl = document.getElementById('statOutStock');

    const total = data ? data.totalCount : adminProductsList.length;
    const brandsCount = data ? data.brandCount : adminBrandsList.length;
    const inCount = data ? data.inStock : adminProductsList.filter(p => p.inStock !== false).length;
    const outCount = data ? data.outStock : adminProductsList.filter(p => p.inStock === false).length;

    if (totalCountEl) totalCountEl.textContent = total;
    if (brandCountEl) brandCountEl.textContent = brandsCount;
    if (inEl) inEl.textContent = inCount;
    if (outEl) outEl.textContent = outCount;
}

// ==========================================
// 📱 PHONES CRUD OPERATIONS (C# API)
// ==========================================

function populateBrandSelect(selectedVal = '') {
    const select = document.getElementById('prodBrand');
    if (!select) return;

    select.innerHTML = adminBrandsList.map(b => `
        <option value="${b.name}" ${b.name === selectedVal ? 'selected' : ''}>
            ${b.name}
        </option>
    `).join('');
}

function renderAdminProductsTable(filterQuery = '') {
    const tbody = document.getElementById('adminProductsTableBody');
    if (!tbody) return;
    tbody.innerHTML = '';

    const q = filterQuery.toLowerCase();
    const stockMode = window.__adminStock || 'all';
    const list = adminProductsList.filter(p => {
        const matches = (p.name || '').toLowerCase().includes(q) || (p.brand || '').toLowerCase().includes(q);
        const inStock = p.inStock !== false;
        const stockOk = stockMode === 'all' || (stockMode === 'in' ? inStock : !inStock);
        return matches && stockOk;
    });

    if (list.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" style="text-align: center; color: var(--muted); padding: 30px;">Смартфоны не найдены</td></tr>';
        return;
    }

    list.forEach(p => {
        const inStock = p.inStock !== false;
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td><img src="${esc(p.img)}" class="admin-table-thumb" alt="${esc(p.name)}" onerror="this.src='/img/logo.png'"></td>
            <td>
                <strong>${esc(p.name)}</strong><br>
                <small class="admin-brand-tag">${esc(p.brand)}</small>
            </td>
            <td>${p.ram} / ${p.storage} ГБ</td>
            <td><b>${p.price.toLocaleString()} сом</b></td>
            <td>
                <button type="button" class="stock-chip ${inStock ? 'in' : 'out'}" onclick="toggleProductStock(${p.id})" title="Нажмите, чтобы переключить наличие">
                    ${inStock ? '● В наличии' : '● Нет в наличии'}
                </button>
            </td>
            <td>
                <div class="admin-table-actions">
                    <button type="button" class="btn-action-edit" onclick="editProductInAdmin(${p.id})">${ico('edit')} Редакт.</button>
                    <button type="button" class="btn-action-delete" onclick="deleteProductInAdmin(${p.id})" aria-label="Удалить ${esc(p.name)}" title="Удалить">${ico('trash')}</button>
                </div>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

// Переключение наличия товара через C# Action
async function toggleProductStock(id) {
    try {
        const resp = await fetch(`/Admin/ToggleStock/${id}`, {
            method: 'POST',
            credentials: 'include'
        });
        if (resp.ok) {
            const data = await resp.json();
            const p = adminProductsList.find(x => x.id === id);
            if (p) {
                p.inStock = data.inStock;
                const statusLabel = data.inStock ? 'В наличии' : 'Нет в наличии';
                if (typeof showToast === 'function') {
                    showToast(`${p.name}: статус изменен на "${statusLabel}"`, 'info');
                }
            }
            updateAdminStats();
            filterAdminProducts();
        }
    } catch (err) {
        console.error('Error toggling product stock:', err);
    }
}

function filterAdminProducts() {
    const q = document.getElementById('adminSearchInput')?.value || '';
    renderAdminProductsTable(q);
}

function choosePresetImage(val) {
    if (val) {
        const input = document.getElementById('prodImg');
        if (input) input.value = val;
        updateLivePreview();
    }
}

// Клиентское сжатие и оптимизация размера изображения (до 900px, WebP/JPEG ~50-120 КБ)
async function compressImageFile(file, maxDimension = 900, quality = 0.82) {
    // GIF / мелкие файлы (< 120 КБ) сжимать не обязательно
    if (file.type === 'image/gif' || file.size <= 120 * 1024) {
        return file;
    }
    return new Promise((resolve) => {
        const img = new Image();
        const objectUrl = URL.createObjectURL(file);
        img.onload = () => {
            URL.revokeObjectURL(objectUrl);
            let { width, height } = img;
            if (width > maxDimension || height > maxDimension) {
                if (width > height) {
                    height = Math.round((height * maxDimension) / width);
                    width = maxDimension;
                } else {
                    width = Math.round((width * maxDimension) / height);
                    height = maxDimension;
                }
            }

            const canvas = document.createElement('canvas');
            canvas.width = width;
            canvas.height = height;
            const ctx = canvas.getContext('2d');
            // Если PNG с прозрачностью, сохраняем в WEBP/PNG, иначе в компактный WEBP/JPEG
            ctx.drawImage(img, 0, 0, width, height);

            const targetType = 'image/webp';
            canvas.toBlob((blob) => {
                if (blob && blob.size < file.size) {
                    const baseName = file.name.replace(/\.[^/.]+$/, '');
                    resolve(new File([blob], `${baseName}.webp`, { type: targetType }));
                } else {
                    resolve(file);
                }
            }, targetType, quality);
        };
        img.onerror = () => {
            URL.revokeObjectURL(objectUrl);
            resolve(file);
        };
        img.src = objectUrl;
    });
}

async function uploadProductImage(fileInput) {
    if (!fileInput || !fileInput.files || !fileInput.files[0]) return;
    const rawFile = fileInput.files[0];

    const statusEl = document.getElementById('uploadStatus');
    if (statusEl) {
        statusEl.textContent = '⏳ Оптимизация и загрузка...';
        statusEl.style.color = 'var(--primary)';
    }

    const file = await compressImageFile(rawFile, 900, 0.82);

    if (file.size > 2 * 1024 * 1024) {
        if (statusEl) {
            statusEl.textContent = '❌ Файл слишком большой (макс. 2 МБ)';
            statusEl.style.color = '#f87171';
        }
        if (typeof showToast === 'function') {
            showToast('Размер изображения после сжатия превышает 2 МБ', 'error');
        }
        return;
    }

    const formData = new FormData();
    formData.append('file', file);

    try {
        const resp = await fetch('/Admin/UploadImage', {
            method: 'POST',
            body: formData
        });

        const data = await resp.json();
        if (resp.ok && data.success && data.url) {
            const prodImg = document.getElementById('prodImg');
            if (prodImg) prodImg.value = data.url;
            updateLivePreview();
            const sizeKb = Math.max(1, Math.round(file.size / 1024));
            const cloudNote = data.storage === 'supabase' ? ' (облако)' : '';
            if (statusEl) {
                statusEl.textContent = `✅ Загружено${cloudNote}: ${sizeKb} КБ`;
                statusEl.style.color = '#34d399';
            }
            if (typeof showToast === 'function') {
                showToast(`Фото "${rawFile.name}" сжато до ${sizeKb} КБ и загружено!`, 'success');
            }
        } else {
            throw new Error(data.message || 'Ошибка загрузки');
        }
    } catch (err) {
        if (statusEl) {
            statusEl.textContent = '❌ ' + (err.message || 'Ошибка');
            statusEl.style.color = '#f87171';
        }
        if (typeof showToast === 'function') {
            showToast(err.message || 'Не удалось загрузить изображение', 'error');
        }
    }
}

function updateLivePreview() {
    const name = document.getElementById('prodName')?.value || 'Название модели';
    const price = document.getElementById('prodPrice')?.value || 0;
    const ram = document.getElementById('prodRam')?.value || 8;
    const storage = document.getElementById('prodStorage')?.value || 256;
    const display = document.getElementById('prodDisplay')?.value || '6.7"';
    const battery = document.getElementById('prodBattery')?.value || 5000;
    const color = document.getElementById('prodColor')?.value || 'Цвет';
    const img = document.getElementById('prodImg')?.value || '/img/iPhone 15 pro max.jpg';
    const inStock = document.getElementById('prodInStock')?.value !== 'false';

    const prevName = document.getElementById('prevName');
    const prevPrice = document.getElementById('prevPrice');
    const prevSpecs = document.getElementById('prevSpecs');
    const prevImg = document.getElementById('prevImg');
    const prevStock = document.getElementById('prevStock');
    const prevBtn = document.getElementById('prevBtn');

    if (prevName) prevName.textContent = name;
    if (prevPrice) prevPrice.textContent = `${parseInt(price || 0).toLocaleString()} сом`;
    if (prevSpecs) prevSpecs.innerHTML = `${display} • ${ram}/${storage} ГБ<br>${battery} мАч • ${color}`;
    if (prevImg) prevImg.src = img;
    if (prevStock) {
        prevStock.className = `card-stock ${inStock ? 'in-stock' : 'out-of-stock'}`;
        prevStock.textContent = inStock ? '● В наличии' : '● Нет в наличии';
    }
    if (prevBtn) {
        prevBtn.textContent = inStock ? 'В корзину' : 'Нет в наличии';
        prevBtn.disabled = !inStock;
    }
}

// СОЗДАНИЕ И ОБНОВЛЕНИЕ ТЕЛЕФОНА ЧЕРЕЗ C# API
async function handleProductFormSubmit(e) {
    e.preventDefault();

    const editId = document.getElementById('editProductId').value;
    const name = document.getElementById('prodName').value.trim();
    const brand = document.getElementById('prodBrand').value.trim();
    const price = parseInt(document.getElementById('prodPrice').value) || 0;
    const color = document.getElementById('prodColor').value.trim();
    const inStock = document.getElementById('prodInStock') ? document.getElementById('prodInStock').value !== 'false' : true;
    const ram = parseInt(document.getElementById('prodRam').value) || 8;
    const storage = parseInt(document.getElementById('prodStorage').value) || 256;
    const display = document.getElementById('prodDisplay').value.trim() || '6.7"';
    const battery = parseInt(document.getElementById('prodBattery').value) || 5000;
    const img = document.getElementById('prodImg').value.trim() || '/img/iPhone 15 pro max.jpg';
    const description = document.getElementById('prodDescription').value.trim();

    const payload = {
        id: editId ? parseInt(editId) : 0,
        name,
        brand,
        price,
        color,
        inStock,
        ram,
        storage,
        display,
        battery,
        img,
        description
    };

    try {
        const resp = await fetch('/Admin/SaveProduct', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify(payload)
        });

        if (resp.ok) {
            const data = await resp.json();
            const msg = editId ? `Телефон "${name}" успешно обновлен!` : `Смартфон "${name}" успешно добавлен в каталог!`;
            if (typeof showToast === 'function') showToast(msg, 'success');
            try { localStorage.removeItem('ez_products'); } catch(e){}
            resetForm();
            if (typeof closeAdminDrawer === 'function') closeAdminDrawer();
            await loadAdminDataFromApi();
        } else {
            if (typeof showToast === 'function') showToast('Ошибка сохранения телефона на сервере', 'error');
        }
    } catch (err) {
        console.error('Error saving product in C# API:', err);
    }
}

function editProductInAdmin(id) {
    const p = adminProductsList.find(x => x.id === id);
    if (!p) return;

    if (typeof openProductDrawerEdit === 'function') {
        openProductDrawerEdit();
    }

    document.getElementById('editProductId').value = p.id;
    document.getElementById('prodName').value = p.name;
    populateBrandSelect(p.brand);
    document.getElementById('prodPrice').value = p.price;
    document.getElementById('prodColor').value = p.color;
    if (document.getElementById('prodInStock')) {
        document.getElementById('prodInStock').value = p.inStock !== false ? 'true' : 'false';
    }
    document.getElementById('prodRam').value = p.ram;
    document.getElementById('prodStorage').value = p.storage;
    document.getElementById('prodDisplay').value = p.display;
    document.getElementById('prodBattery').value = p.battery;
    document.getElementById('prodImg').value = p.img;
    document.getElementById('prodDescription').value = p.description || '';

    const titleEl = document.getElementById('formTitle');
    if (titleEl) titleEl.textContent = `Редактирование: ${p.name}`;
    const saveBtn = document.getElementById('saveProductBtn');
    if (saveBtn) saveBtn.textContent = 'Сохранить изменения';
    const cancelBtn = document.getElementById('cancelEditBtn');
    if (cancelBtn) cancelBtn.style.display = 'inline-block';

    updateLivePreview();
}

function cancelEditing() {
    resetForm();
    if (typeof closeAdminDrawer === 'function') closeAdminDrawer();
}

function resetForm() {
    const form = document.getElementById('productForm');
    if (form) form.reset();
    document.getElementById('editProductId').value = '';
    if (document.getElementById('prodInStock')) {
        document.getElementById('prodInStock').value = 'true';
    }
    const titleEl = document.getElementById('formTitle');
    if (titleEl) titleEl.textContent = 'Добавить новый телефон';
    const saveBtn = document.getElementById('saveProductBtn');
    if (saveBtn) saveBtn.textContent = 'Сохранить телефон';
    const cancelBtn = document.getElementById('cancelEditBtn');
    if (cancelBtn) cancelBtn.style.display = 'none';
    populateBrandSelect();
    updateLivePreview();
}

// УДАЛЕНИЕ ТЕЛЕФОНА ЧЕРЕЗ C# API
async function deleteProductInAdmin(id) {
    const p = adminProductsList.find(x => x.id === id);
    if (!p) return;

    if (!confirm(`Вы действительно хотите удалить "${p.name}" из каталога?`)) return;

    try {
        const resp = await fetch(`/Admin/DeleteProduct/${id}`, {
            method: 'POST',
            credentials: 'include'
        });
        if (resp.ok) {
            if (typeof showToast === 'function') showToast(`"${p.name}" удален из каталога`, 'info');
            await loadAdminDataFromApi();
        }
    } catch (err) {
        console.error('Error deleting product:', err);
    }
}

// ==========================================
// 🏷️ BRANDS CRUD OPERATIONS (C# API)
// ==========================================

function renderAdminBrandsList() {
    const container = document.getElementById('adminBrandsGrid');
    const countEl = document.getElementById('brandsListCount');
    if (!container) return;

    if (countEl) countEl.textContent = adminBrandsList.length;
    container.innerHTML = '';

    if (adminBrandsList.length === 0) {
        container.innerHTML = '<p style="color: var(--muted); padding: 20px;">Нет добавленных брендов</p>';
        return;
    }

    adminBrandsList.forEach(b => {
        const count = adminProductsList.filter(p => (p.brand || '').toLowerCase() === b.name.toLowerCase()).length;
        const card = document.createElement('div');
        card.className = 'brand-card';
        card.innerHTML = `
            <div class="brand-card-left">
                <div class="brand-card-info">
                    <h4>${esc(b.name)} <span class="brand-count-badge">${count} шт.</span></h4>
                    <span>${esc(b.country || 'Международный бренд')}</span>
                </div>
            </div>
            <div class="brand-actions">
                <button type="button" class="btn-action-delete" onclick="deleteBrandInAdmin(${b.id})" title="Удалить" aria-label="Удалить">${ico('trash')}</button>
            </div>
        `;
        container.appendChild(card);
    });
}

// СОЗДАНИЕ БРЕНДА ЧЕРЕЗ C# API
async function handleBrandFormSubmit(e) {
    e.preventDefault();

    const name = document.getElementById('brandName').value.trim();
    const country = document.getElementById('brandCountry').value.trim();

    if (!name) return;

    try {
        const resp = await fetch('/Admin/SaveBrand', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify({ name, country })
        });

        if (resp.ok) {
            if (typeof showToast === 'function') showToast(`Бренд "${name}" успешно добавлен!`, 'success');
            resetBrandForm();
            await loadAdminDataFromApi();
        }
    } catch (err) {
        console.error('Error saving brand in C# API:', err);
    }
}

function resetBrandForm() {
    const form = document.getElementById('brandForm');
    if (form) form.reset();
}

// УДАЛЕНИЕ БРЕНДА ЧЕРЕЗ C# API
async function deleteBrandInAdmin(id) {
    const b = adminBrandsList.find(x => x.id === id);
    if (!b) return;

    if (!confirm(`Удалить бренд "${b.name}"?`)) return;

    try {
        const resp = await fetch(`/Admin/DeleteBrand/${id}`, {
            method: 'POST',
            credentials: 'include'
        });
        if (resp.ok) {
            if (typeof showToast === 'function') showToast(`Бренд "${b.name}" удален`, 'info');
            await loadAdminDataFromApi();
        }
    } catch (err) {
        console.error('Error deleting brand:', err);
    }
}

// Запуск при загрузке страницы
document.addEventListener('DOMContentLoaded', () => {
    const isOrdersPage = window.location.pathname.includes('admin-orders') || window.location.pathname.includes('Orders');
    if (!isOrdersPage && (document.getElementById('adminProductsTableBody') || window.location.pathname.toLowerCase().includes('admin'))) {
        initAdminDashboard();
    }
});
