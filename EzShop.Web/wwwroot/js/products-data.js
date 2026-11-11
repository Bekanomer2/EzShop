// ==========================================
// EzShop - Product & Brand Data Storage Manager
// ==========================================

const initialProductsData = [
    // Apple (5)
    { id: 1, name: 'iPhone 13', price: 67500, img: 'img/iphone 13.jpg', brand: 'Apple', ram: 4, storage: 128, color: 'Midnight', display: '6.1"', battery: 3240, inStock: false, description: 'Надежный смартфон с мощным процессором A15 Bionic, отличной двойной камерой и длительным временем работы.' },
    { id: 6, name: 'iPhone 15 Pro Max', price: 107910, img: 'img/iPhone 15 pro max.jpg', brand: 'Apple', ram: 8, storage: 256, color: 'Natural Titanium', display: '6.7"', battery: 4441, description: 'Флагман в титановом корпусе с кнопкой Action Button, инновационным чипом A17 Pro и 5x оптическим зумом.' },
    { id: 10, name: 'iPhone 14 Pro', price: 89910, img: 'img/iPhone 14 Pro.jpg', brand: 'Apple', ram: 6, storage: 256, color: 'Space Black', display: '6.1"', battery: 3200, description: 'Модель с интерактивным Dynamic Island, Always-On дисплеем и камерой 48 Мп для потрясающих снимков.' },
    { id: 15, name: 'iPhone 12 Mini', price: 45000, img: 'img/iphone 12 mini.jpg', brand: 'Apple', ram: 4, storage: 64, color: 'Blue', display: '5.4"', battery: 2227, description: 'Компактный и легкий флагман с поддержкой 5G, экраном Super Retina XDR и защитой Ceramic Shield.' },
    { id: 19, name: 'iPhone SE 2022', price: 38610, img: 'img/iphone se 2022.webp', brand: 'Apple', ram: 4, storage: 64, color: 'Starlight', display: '4.7"', battery: 2018, description: 'Классический дизайн с кнопкой Home и быстрым процессором A15 Bionic по доступной цене.' },

    // Samsung (5)
    { id: 2, name: 'Samsung S23', price: 61200, img: 'img/samsung s23.webp', brand: 'Samsung', ram: 8, storage: 256, color: 'Phantom Black', display: '6.1"', battery: 3900, description: 'Компактный флагман от Samsung с чипом Snapdragon 8 Gen 2 for Galaxy и ярким Dynamic AMOLED экраном.' },
    { id: 7, name: 'Samsung S24 Ultra', price: 116910, img: 'img/samsung galaxy s24 ultra.jpg', brand: 'Samsung', ram: 12, storage: 512, color: 'Titanium Gray', display: '6.8"', battery: 5000, description: 'Вершина технологий: титановый корпус, умные функции Galaxy AI, стилус S Pen и камера 200 Мп.' },
    { id: 11, name: 'Samsung Galaxy S22', price: 49500, img: 'img/Samsung-Galaxy-S22.webp', brand: 'Samsung', ram: 8, storage: 128, color: 'Green', display: '6.1"', battery: 3700, description: 'Стильный премиум-смартфон с плавной частотой обновления 120 Гц и продвинутым ночным режимом съемки.' },
    { id: 16, name: 'Samsung Galaxy A54', price: 31500, img: 'img/samsung-galaxy-a54.jpg', brand: 'Samsung', ram: 6, storage: 128, color: 'Awesome Graphite', display: '6.4"', battery: 5000, description: 'Народный хит: влагозащита IP67, емкая батарея на 5000 мАч и стеклянная задняя панель.' },
    { id: 21, name: 'Samsung Galaxy Z Flip 5', price: 89910, img: 'img/samsung flip 5.jpg', brand: 'Samsung', ram: 8, storage: 256, color: 'Mint', display: '6.7"', battery: 3700, description: 'Инновационная раскладушка с увеличенным внешним экраном Flex Window и механизмом Flex Hinge.' },

    // Xiaomi (5)
    { id: 3, name: 'Xiaomi Note 13', price: 28800, img: 'img/xiaomi redmi 13.jpg', brand: 'Xiaomi', ram: 8, storage: 256, color: 'Midnight Black', display: '6.67"', battery: 5000, description: 'Яркий AMOLED экран 120 Гц, камера 108 Мп и емкий аккумулятор с быстрой зарядкой 33W.' },
    { id: 8, name: 'Xiaomi 14 Ultra', price: 98910, img: 'img/xiaomi 14 ultra.jpg', brand: 'Xiaomi', ram: 16, storage: 512, color: 'Black', display: '6.73"', battery: 5000, description: 'Ультимативный камерофон с оптикой Leica Summilux, 1-дюймовым сенсором и чипом Snapdragon 8 Gen 3.' },
    { id: 12, name: 'Xiaomi 13T Pro', price: 58500, img: 'img/xiaomi 13t pro.jpg', brand: 'Xiaomi', ram: 12, storage: 512, color: 'Alpine Blue', display: '6.67"', battery: 5000, description: 'Экран 144 Гц CrystalRes, зарядка HyperCharge 120W (100% за 19 минут) и камеры Leica.' },
    { id: 18, name: 'Redmi Note 12', price: 18000, img: 'img/Redmi-Note-12.jpg', brand: 'Xiaomi', ram: 4, storage: 128, color: 'Ice Blue', display: '6.67"', battery: 5000, description: 'Отличный бюджетный смартфон с AMOLED-экраном 120 Гц и энергоэффективным процессором Snapdragon.' },
    { id: 22, name: 'Xiaomi 12', price: 45000, img: 'img/xiaomi 12.jpg', brand: 'Xiaomi', ram: 8, storage: 256, color: 'Blue', display: '6.28"', battery: 4500, description: 'Эргономичный компактный флагман с беспроводной зарядкой 50W и стереодинамиками Harman Kardon.' },

    // Realme (5)
    { id: 4, name: 'Realme 11 Pro', price: 36000, img: 'img/realme-11.webp', brand: 'Realme', ram: 8, storage: 256, color: 'Sunrise Beige', display: '6.7"', battery: 5000, description: 'Дизайнерский корпус из эко-кожи от экс-дизайнера Gucci, изогнутый дисплей 120 Гц и камера 100 Мп.' },
    { id: 13, name: 'Realme GT 3', price: 52200, img: 'img/realme gt3.jpg', brand: 'Realme', ram: 16, storage: 1024, color: 'Purple', display: '6.74"', battery: 4600, description: 'Рекордная зарядка 240 Вт (заряжается за 9 минут), интерактивная RGB-подсветка Pulse Interface и 1 ТБ памяти.' },
    { id: 23, name: 'Realme C55', price: 18000, img: 'img/realme c55.jpg', brand: 'Realme', ram: 8, storage: 256, color: 'Sunshower', display: '6.72"', battery: 5000, description: 'Стильный бюджетник с функцией мини-капсулы Mini Capsule и камерой 64 Мп.' },
    { id: 24, name: 'Realme 10', price: 20700, img: 'img/realme 10.jpg', brand: 'Realme', ram: 8, storage: 128, color: 'White', display: '6.4"', battery: 5000, description: 'Super AMOLED экран 90 Гц, игровой процессор Helio G99 и ультратонкий корпус 7.95 мм.' },
    { id: 25, name: 'Realme Narzo 60', price: 22500, img: 'img/realme narzo 60.webp', brand: 'Realme', ram: 8, storage: 128, color: 'Mars Orange', display: '6.43"', battery: 5000, description: 'Премиальная отделка под кожу, контрастный Super AMOLED дисплей и подэкранный сканер отпечатков.' },

    // OnePlus (5)
    { id: 5, name: 'OnePlus Nord CE 3', price: 40500, img: 'img/oneplus.png', brand: 'OnePlus', ram: 12, storage: 256, color: 'Gray Shimmer', display: '6.7"', battery: 5000, description: 'Оптимальный баланс: чип Snapdragon 782G, камера Sony IMX890 с оптической стабилизацией.' },
    { id: 14, name: 'OnePlus 11', price: 63000, img: 'img/oneplus 11.webp', brand: 'OnePlus', ram: 16, storage: 256, color: 'Eternal Green', display: '6.7"', battery: 5000, description: 'Флагманские камеры Hasselblad третьего поколения, чип Snapdragon 8 Gen 2 и быстрая зарядка 100W.' },
    { id: 26, name: 'OnePlus 12', price: 72000, img: 'img/oneplus 12.webp', brand: 'OnePlus', ram: 16, storage: 512, color: 'Flowy Emerald', display: '6.82"', battery: 5400, description: 'Яркость дисплея до 4500 нит, аккумулятор 5400 мАч и передовая перископическая камера.' },
    { id: 27, name: 'OnePlus 10 Pro', price: 54000, img: 'img/oneplus-10.jpg', brand: 'OnePlus', ram: 12, storage: 256, color: 'Volcanic Black', display: '6.7"', battery: 5000, description: 'Дисплей Fluid AMOLED с технологией LTPO 2.0, беспроводная зарядка 50W AirVOOC.' },
    { id: 28, name: 'OnePlus Nord 3', price: 40500, img: 'img/oneplus nord 3.jpg', brand: 'OnePlus', ram: 16, storage: 256, color: 'Misty Green', display: '6.74"', battery: 5000, description: 'Флагманский процессор Dimensity 9000, 16 ГБ оперативной памяти и переключатель Alert Slider.' },

    // Google (5)
    { id: 9, name: 'Google Pixel 8 Pro', price: 89910, img: 'img/pixel 8 pro.jpeg', brand: 'Google', ram: 12, storage: 128, color: 'Obsidian', display: '6.7"', battery: 5050, description: 'Искусственный интеллект Google Tensor G3, революционные фотовозможности Best Take и Magic Audio Eraser.' },
    { id: 29, name: 'Google Pixel 7', price: 53910, img: 'img/Pixel7-lemongrass .jpg', brand: 'Google', ram: 8, storage: 128, color: 'Lemongrass', display: '6.3"', battery: 4355, description: 'Чистый Android с регулярными обновлениями, процессор Tensor G2 и алюминиевая панель камеры.' },
    { id: 30, name: 'Google Pixel 7a', price: 44910, img: 'img/Google-Pixel-7a.jpg', brand: 'Google', ram: 8, storage: 128, color: 'Sea', display: '6.1"', battery: 4385, description: 'Лучшая камера в своем классе, беспроводная зарядка и частота обновления экрана 90 Гц.' },
    { id: 31, name: 'Google Pixel 6', price: 36000, img: 'img/google-pixel-6-seafoam-2.avif', brand: 'Google', ram: 8, storage: 128, color: 'Sorta Seafoam', display: '6.4"', battery: 4614, description: 'Оригинальный двухцветный дизайн, первый чип Google Tensor и защита от воды IP68.' },
    { id: 32, name: 'Google Pixel 6a', price: 31500, img: 'img/Pixel-6a.avif', brand: 'Google', ram: 6, storage: 128, color: 'Sage', display: '6.1"', battery: 4410, description: 'Компактный телефон с легендарной вычислительной фотографией Pixel по супер-цене.' },

    // Poco (5)
    { id: 17, name: 'Poco X6 Pro', price: 27000, img: 'img/poco x6 pro.webp', brand: 'Poco', ram: 8, storage: 256, color: 'Yellow', display: '6.67"', battery: 5000, description: 'Игровой монстр на Dimensity 8300-Ultra с системой охлаждения LiquidCool 2.0 и HyperOS.' },
    { id: 33, name: 'Poco F5', price: 36000, img: 'img/poco f5.jpg', brand: 'Poco', ram: 12, storage: 256, color: 'White', display: '6.67"', battery: 5000, description: 'Мощнейший чип Snapdragon 7+ Gen 2, тонкие рамки экрана и вес всего 181 грамм.' },
    { id: 34, name: 'Poco X5', price: 22500, img: 'img/poco x5.webp', brand: 'Poco', ram: 8, storage: 256, color: 'Green', display: '6.67"', battery: 5000, description: 'AMOLED DotDisplay 120 Гц, чип Snapdragon 695 5G и тройная камера с AI.' },
    { id: 35, name: 'Poco M6', price: 13500, img: 'img/poco m6.jpg', brand: 'Poco', ram: 6, storage: 128, color: 'Galactic Black', display: '6.79"', battery: 5000, description: 'Стеклянная задняя крышка, большой экран 90 Гц и камера 108 Мп с сенсорным зумом 3x.' },
    { id: 36, name: 'Poco C65', price: 11700, img: 'img/poco-c65.jpg', brand: 'Poco', ram: 8, storage: 256, color: 'Purple', display: '6.74"', battery: 5000, description: 'Доступный смартфон с экраном 90 Гц, защитой зрения Corning Gorilla Glass и батареей 5000 мАч.' },

    // Honor (5)
    { id: 20, name: 'Honor 90', price: 40500, img: 'img/honor 90.jpg', brand: 'Honor', ram: 12, storage: 512, color: 'Emerald Green', display: '6.7"', battery: 5000, description: 'Камера 200 Мп, безвредный для глаз экран без мерцания (ШИМ 3840 Гц) и тонкий элегантный корпус.' },
    { id: 37, name: 'Honor Magic 6', price: 81000, img: 'img/magic-6-pro.webp', brand: 'Honor', ram: 12, storage: 512, color: 'Green', display: '6.8"', battery: 5600, description: 'Кремний-углеродный аккумулятор 2-го поколения, стекло повышенной прочности NanoCrystal Shield.' },
    { id: 38, name: 'Honor X9b', price: 31500, img: 'img/HONOR-X9b.jpg', brand: 'Honor', ram: 12, storage: 256, color: 'Sunrise Orange', display: '6.78"', battery: 5800, description: 'Сверхпрочный дисплей с защитой от падений с 1.5 метров и огромный аккумулятор 5800 мАч.' },
    { id: 39, name: 'Honor 70', price: 36000, img: 'img/honor-70.jpg', brand: 'Honor', ram: 8, storage: 256, color: 'Crystal Silver', display: '6.67"', battery: 4800, description: 'Двухсторонний изогнутый OLED экран 120 Гц, сенсор Sony IMX800 и режим Solo Cut для видеоблогеров.' },
    { id: 40, name: 'Honor X8', price: 18000, img: 'img/honor x8.jpg', brand: 'Honor', ram: 6, storage: 128, color: 'Ocean Blue', display: '6.7"', battery: 4000, description: 'Ультратонкий корпус 7.45 мм, экран HONOR FullView 90 Гц и технология RAM Turbo.' }
];

// Initial Brands Data (без иконок)
const initialBrandsData = [
    { id: 1, name: 'Apple', country: 'США' },
    { id: 2, name: 'Samsung', country: 'Южная Корея' },
    { id: 3, name: 'Xiaomi', country: 'Китай' },
    { id: 4, name: 'Realme', country: 'Китай' },
    { id: 5, name: 'OnePlus', country: 'Китай' },
    { id: 6, name: 'Google', country: 'США' },
    { id: 7, name: 'Poco', country: 'Китай' },
    { id: 8, name: 'Honor', country: 'Китай' }
];

const PRODUCTS_STORAGE_KEY = 'ez_custom_products';
const BRANDS_STORAGE_KEY = 'ez_custom_brands';
const STOCK_STORAGE_KEY = 'ez_stock_overrides';

// Stock Overrides Manager (syncs stock status across devices)
function getStockOverrides() {
    try {
        const stored = localStorage.getItem(STOCK_STORAGE_KEY);
        if (stored) return JSON.parse(stored);
    } catch (e) {}
    return { 1: false }; // iPhone 13 defaults to out of stock as set by user
}

function saveStockOverride(productId, inStock) {
    const overrides = getStockOverrides();
    overrides[productId] = inStock === false || inStock === 'false' ? false : true;
    localStorage.setItem(STOCK_STORAGE_KEY, JSON.stringify(overrides));
    saveStockToSupabase(overrides);
}

function applyStockOverrides(list) {
    if (!Array.isArray(list)) return [];
    const overrides = getStockOverrides();
    return list.map(p => ({
        ...p,
        inStock: overrides[p.id] !== undefined ? overrides[p.id] : (p.inStock === false || p.inStock === 'false' ? false : true)
    }));
}

async function saveStockToSupabase(overrides) {
    // Управление складскими остатками осуществляется через C# контроллер /Admin/ToggleStock
}

// Products Storage
function normalizeProducts(list) {
    return list.map(p => ({
        ...p,
        inStock: p.inStock === false || p.inStock === 'false' ? false : true
    }));
}

function getProductsData() {
    try {
        const stored = localStorage.getItem(PRODUCTS_STORAGE_KEY);
        if (stored) {
            const parsed = JSON.parse(stored);
            if (Array.isArray(parsed) && parsed.length > 0) {
                return applyStockOverrides(normalizeProducts(parsed));
            }
        }
    } catch (e) {
        console.warn('Error reading products from storage:', e);
    }
    return applyStockOverrides(normalizeProducts([...initialProductsData]));
}

function saveProductsData(products, syncCloud = true) {
    try {
        const normalized = applyStockOverrides(normalizeProducts(products));
        localStorage.setItem(PRODUCTS_STORAGE_KEY, JSON.stringify(normalized));
        productsData = normalized;
        if (syncCloud) {
            saveCatalogToSupabase();
        }
    } catch (e) {
        console.error('Error saving products:', e);
    }
}

// Brands Storage
function getBrandsData() {
    try {
        const stored = localStorage.getItem(BRANDS_STORAGE_KEY);
        if (stored) {
            const parsed = JSON.parse(stored);
            if (Array.isArray(parsed) && parsed.length > 0) {
                return parsed.map(b => ({ id: b.id, name: b.name, country: b.country }));
            }
        }
    } catch (e) {
        console.warn('Error reading brands from storage:', e);
    }
    return [...initialBrandsData];
}

function saveBrandsData(brands, syncCloud = true) {
    try {
        localStorage.setItem(BRANDS_STORAGE_KEY, JSON.stringify(brands));
        brandsData = brands;
        if (syncCloud) {
            saveCatalogToSupabase();
        }
    } catch (e) {
        console.error('Error saving brands:', e);
    }
}

// Каталог и складские остатки синхронизируются через C# ASP.NET Core бэкенд
async function saveCatalogToSupabase() {}

async function syncCatalogWithSupabase() {
    try {
        if (typeof window !== 'undefined' && Array.isArray(window.productsData) && window.productsData.length > 0) {
            productsData = window.productsData;
            return;
        }
        const resp = await fetch('/Admin/GetProducts');
        if (resp.ok) {
            const list = await resp.json();
            if (Array.isArray(list) && list.length > 0) {
                productsData = list;
                window.productsData = list;
            }
        }
    } catch (e) {
        // Fallback to initialProductsData
    }
}

let productsData = (typeof window !== 'undefined' && Array.isArray(window.productsData) && window.productsData.length > 0)
    ? window.productsData
    : getProductsData();
let brandsData = (typeof window !== 'undefined' && Array.isArray(window.brandsData) && window.brandsData.length > 0)
    ? window.brandsData
    : getBrandsData();
if (typeof window !== 'undefined') {
    window.productsData = window.productsData || productsData;
    window.brandsData = window.brandsData || brandsData;
    window.initialProductsData = initialProductsData;
}
