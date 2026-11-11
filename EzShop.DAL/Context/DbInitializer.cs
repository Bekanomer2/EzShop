using EzShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EzShop.DAL.Context;

public static class DbInitializer
{
    public static async Task SeedProductsAndBrandsAsync(EzShopDbContext context)
    {
        // 0. Настройка каскадного удаления и создание таблицы избранного
        try
        {
            await context.Database.ExecuteSqlRawAsync(@"
                DELETE FROM reviews WHERE product_id NOT IN (SELECT id FROM products);
                ALTER TABLE reviews DROP CONSTRAINT IF EXISTS fk_reviews_products;
                ALTER TABLE reviews ADD CONSTRAINT fk_reviews_products FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE;

                CREATE TABLE IF NOT EXISTS user_favorites (
                    id SERIAL PRIMARY KEY,
                    user_id INT NOT NULL,
                    product_id INT NOT NULL,
                    created_at TIMESTAMPTZ DEFAULT NOW(),
                    CONSTRAINT uq_user_favorites UNIQUE (user_id, product_id),
                    CONSTRAINT fk_user_favorites_products FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS cart_items (
                    id SERIAL PRIMARY KEY,
                    user_id INT NOT NULL,
                    product_id INT NOT NULL,
                    qty INT NOT NULL DEFAULT 1,
                    created_at TIMESTAMPTZ DEFAULT NOW(),
                    updated_at TIMESTAMPTZ DEFAULT NOW(),
                    CONSTRAINT uq_cart_items UNIQUE (user_id, product_id),
                    CONSTRAINT fk_cart_items_products FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
                );
            ");
        }
        catch { }

        // 1. Инициализация брендов (если таблица пуста)
        if (!await context.Brands.AnyAsync())
        {
            var brands = GetInitialBrands();
            await context.Brands.AddRangeAsync(brands);
            await context.SaveChangesAsync();
        }

        // 2. Инициализация всех 40 смартфонов каталога (если таблица пуста)
        if (!await context.Products.AnyAsync())
        {
            var products = GetInitialProducts();
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }

    public static List<Brand> GetInitialBrands() => new()
    {
        new Brand { Id = 1, Name = "Apple", Country = "США" },
        new Brand { Id = 2, Name = "Samsung", Country = "Южная Корея" },
        new Brand { Id = 3, Name = "Xiaomi", Country = "Китай" },
        new Brand { Id = 4, Name = "Realme", Country = "Китай" },
        new Brand { Id = 5, Name = "OnePlus", Country = "Китай" },
        new Brand { Id = 6, Name = "Google", Country = "США" },
        new Brand { Id = 7, Name = "Poco", Country = "Китай" },
        new Brand { Id = 8, Name = "Honor", Country = "Китай" }
    };

    public static List<Product> GetInitialProducts() => new()
    {
        // Apple (5)
        new Product { Id = 1, Name = "iPhone 13", Brand = "Apple", Price = 67500, Img = "/img/iphone 13.jpg", Ram = 4, Storage = 128, Color = "Midnight", Display = "6.1\"", Battery = 3240, InStock = false, Description = "Надежный смартфон с мощным процессором A15 Bionic, отличной двойной камерой и длительным временем работы." },
        new Product { Id = 6, Name = "iPhone 15 Pro Max", Brand = "Apple", Price = 107910, Img = "/img/iPhone 15 pro max.jpg", Ram = 8, Storage = 256, Color = "Natural Titanium", Display = "6.7\"", Battery = 4441, InStock = true, Description = "Флагман в титановом корпусе с кнопкой Action Button, инновационным чипом A17 Pro и 5x оптическим зумом." },
        new Product { Id = 10, Name = "iPhone 14 Pro", Brand = "Apple", Price = 89910, Img = "/img/iPhone 14 Pro.jpg", Ram = 6, Storage = 256, Color = "Space Black", Display = "6.1\"", Battery = 3200, InStock = true, Description = "Модель с интерактивным Dynamic Island, Always-On дисплеем и камерой 48 Мп для потрясающих снимков." },
        new Product { Id = 15, Name = "iPhone 12 Mini", Brand = "Apple", Price = 45000, Img = "/img/iphone 12 mini.jpg", Ram = 4, Storage = 64, Color = "Blue", Display = "5.4\"", Battery = 2227, InStock = true, Description = "Компактный и легкий флагман с поддержкой 5G, экраном Super Retina XDR и защитой Ceramic Shield." },
        new Product { Id = 19, Name = "iPhone SE 2022", Brand = "Apple", Price = 38610, Img = "/img/iphone se 2022.webp", Ram = 4, Storage = 64, Color = "Starlight", Display = "4.7\"", Battery = 2018, InStock = true, Description = "Классический дизайн с кнопкой Home и быстрым процессором A15 Bionic по доступной цене." },

        // Samsung (5)
        new Product { Id = 2, Name = "Samsung S23", Brand = "Samsung", Price = 61200, Img = "/img/samsung s23.webp", Ram = 8, Storage = 256, Color = "Phantom Black", Display = "6.1\"", Battery = 3900, InStock = true, Description = "Компактный флагман от Samsung с чипом Snapdragon 8 Gen 2 for Galaxy и ярким Dynamic AMOLED экраном." },
        new Product { Id = 7, Name = "Samsung S24 Ultra", Brand = "Samsung", Price = 116910, Img = "/img/samsung galaxy s24 ultra.jpg", Ram = 12, Storage = 512, Color = "Titanium Gray", Display = "6.8\"", Battery = 5000, InStock = true, Description = "Вершина технологий: титановый корпус, умные функции Galaxy AI, стилус S Pen и камера 200 Мп." },
        new Product { Id = 11, Name = "Samsung Galaxy S22", Brand = "Samsung", Price = 49500, Img = "/img/Samsung-Galaxy-S22.webp", Ram = 8, Storage = 128, Color = "Green", Display = "6.1\"", Battery = 3700, InStock = true, Description = "Стильный премиум-смартфон с плавной частотой обновления 120 Гц и продвинутым ночным режимом съемки." },
        new Product { Id = 16, Name = "Samsung Galaxy A54", Brand = "Samsung", Price = 31500, Img = "/img/samsung-galaxy-a54.jpg", Ram = 6, Storage = 128, Color = "Awesome Graphite", Display = "6.4\"", Battery = 5000, InStock = true, Description = "Народный хит: влагозащита IP67, емкая батарея на 5000 мАч и стеклянная задняя панель." },
        new Product { Id = 21, Name = "Samsung Galaxy Z Flip 5", Brand = "Samsung", Price = 89910, Img = "/img/samsung flip 5.jpg", Ram = 8, Storage = 256, Color = "Mint", Display = "6.7\"", Battery = 3700, InStock = true, Description = "Инновационная раскладушка с увеличенным внешним экраном Flex Window и механизмом Flex Hinge." },

        // Xiaomi (5)
        new Product { Id = 3, Name = "Xiaomi Note 13", Brand = "Xiaomi", Price = 28800, Img = "/img/xiaomi redmi 13.jpg", Ram = 8, Storage = 256, Color = "Midnight Black", Display = "6.67\"", Battery = 5000, InStock = true, Description = "Яркий AMOLED экран 120 Гц, камера 108 Мп и емкий аккумулятор с быстрой зарядкой 33W." },
        new Product { Id = 8, Name = "Xiaomi 14 Ultra", Brand = "Xiaomi", Price = 98910, Img = "/img/xiaomi 14 ultra.jpg", Ram = 16, Storage = 512, Color = "Black", Display = "6.73\"", Battery = 5000, InStock = true, Description = "Ультимативный камерофон с оптикой Leica Summilux, 1-дюймовым сенсором и чипом Snapdragon 8 Gen 3." },
        new Product { Id = 12, Name = "Xiaomi 13T Pro", Brand = "Xiaomi", Price = 58500, Img = "/img/xiaomi 13t pro.jpg", Ram = 12, Storage = 512, Color = "Alpine Blue", Display = "6.67\"", Battery = 5000, InStock = true, Description = "Экран 144 Гц CrystalRes, зарядка HyperCharge 120W (100% за 19 минут) и камеры Leica." },
        new Product { Id = 18, Name = "Redmi Note 12", Brand = "Xiaomi", Price = 18000, Img = "/img/Redmi-Note-12.jpg", Ram = 4, Storage = 128, Color = "Ice Blue", Display = "6.67\"", Battery = 5000, InStock = true, Description = "Отличный бюджетный смартфон с AMOLED-экраном 120 Гц и энергоэффективным процессором Snapdragon." },
        new Product { Id = 22, Name = "Xiaomi 12", Brand = "Xiaomi", Price = 45000, Img = "/img/xiaomi 12.jpg", Ram = 8, Storage = 256, Color = "Blue", Display = "6.28\"", Battery = 4500, InStock = true, Description = "Эргономичный компактный флагман с беспроводной зарядкой 50W и стереодинамиками Harman Kardon." },

        // Realme (5)
        new Product { Id = 4, Name = "Realme 11 Pro", Brand = "Realme", Price = 36000, Img = "/img/realme-11.webp", Ram = 8, Storage = 256, Color = "Sunrise Beige", Display = "6.7\"", Battery = 5000, InStock = true, Description = "Дизайнерский корпус из эко-кожи от экс-дизайнера Gucci, изогнутый дисплей 120 Гц и камера 100 Мп." },
        new Product { Id = 13, Name = "Realme GT 3", Brand = "Realme", Price = 52200, Img = "/img/realme gt3.jpg", Ram = 16, Storage = 1024, Color = "Purple", Display = "6.74\"", Battery = 4600, InStock = true, Description = "Рекордная зарядка 240 Вт (заряжается за 9 минут), интерактивная RGB-подсветка Pulse Interface и 1 ТБ памяти." },
        new Product { Id = 23, Name = "Realme C55", Brand = "Realme", Price = 18000, Img = "/img/realme c55.jpg", Ram = 8, Storage = 256, Color = "Sunshower", Display = "6.72\"", Battery = 5000, InStock = true, Description = "Стильный бюджетник с функцией мини-капсулы Mini Capsule и камерой 64 Мп." },
        new Product { Id = 24, Name = "Realme 10", Brand = "Realme", Price = 20700, Img = "/img/realme 10.jpg", Ram = 8, Storage = 128, Color = "White", Display = "6.4\"", Battery = 5000, InStock = true, Description = "Super AMOLED экран 90 Гц, игровой процессор Helio G99 и ультратонкий корпус 7.95 мм." },
        new Product { Id = 25, Name = "Realme Narzo 60", Brand = "Realme", Price = 22500, Img = "/img/realme narzo 60.webp", Ram = 8, Storage = 128, Color = "Mars Orange", Display = "6.43\"", Battery = 5000, InStock = true, Description = "Премиальная отделка под кожу, контрастный Super AMOLED дисплей и подэкранный сканер отпечатков." },

        // OnePlus (5)
        new Product { Id = 5, Name = "OnePlus Nord CE 3", Brand = "OnePlus", Price = 40500, Img = "/img/oneplus.png", Ram = 12, Storage = 256, Color = "Gray Shimmer", Display = "6.7\"", Battery = 5000, InStock = true, Description = "Оптимальный баланс: чип Snapdragon 782G, камера Sony IMX890 с оптической стабилизацией." },
        new Product { Id = 14, Name = "OnePlus 11", Brand = "OnePlus", Price = 63000, Img = "/img/oneplus 11.webp", Ram = 16, Storage = 256, Color = "Eternal Green", Display = "6.7\"", Battery = 5000, InStock = true, Description = "Флагманские камеры Hasselblad третьего поколения, чип Snapdragon 8 Gen 2 и быстрая зарядка 100W." },
        new Product { Id = 26, Name = "OnePlus 12", Brand = "OnePlus", Price = 72000, Img = "/img/oneplus 12.webp", Ram = 16, Storage = 512, Color = "Flowy Emerald", Display = "6.82\"", Battery = 5400, InStock = true, Description = "Яркость дисплея до 4500 нит, аккумулятор 5400 мАч и передовая перископическая камера." },
        new Product { Id = 27, Name = "OnePlus 10 Pro", Brand = "OnePlus", Price = 54000, Img = "/img/oneplus-10.jpg", Ram = 12, Storage = 256, Color = "Volcanic Black", Display = "6.7\"", Battery = 5000, InStock = true, Description = "Дисплей Fluid AMOLED с технологией LTPO 2.0, беспроводная зарядка 50W AirVOOC." },
        new Product { Id = 28, Name = "OnePlus Nord 3", Brand = "OnePlus", Price = 40500, Img = "/img/oneplus nord 3.jpg", Ram = 16, Storage = 256, Color = "Misty Green", Display = "6.74\"", Battery = 5000, InStock = true, Description = "Флагманский процессор Dimensity 9000, 16 ГБ оперативной памяти и переключатель Alert Slider." },

        // Google (5)
        new Product { Id = 9, Name = "Google Pixel 8 Pro", Brand = "Google", Price = 89910, Img = "/img/pixel 8 pro.jpeg", Ram = 12, Storage = 128, Color = "Obsidian", Display = "6.7\"", Battery = 5050, InStock = true, Description = "Искусственный интеллект Google Tensor G3, революционные фотовозможности Best Take и Magic Audio Eraser." },
        new Product { Id = 29, Name = "Google Pixel 7", Brand = "Google", Price = 53910, Img = "/img/Pixel7-lemongrass .jpg", Ram = 8, Storage = 128, Color = "Lemongrass", Display = "6.3\"", Battery = 4355, InStock = true, Description = "Чистый Android с регулярными обновлениями, процессор Tensor G2 и алюминиевая панель камеры." },
        new Product { Id = 30, Name = "Google Pixel 7a", Brand = "Google", Price = 44910, Img = "/img/Google-Pixel-7a.jpg", Ram = 8, Storage = 128, Color = "Sea", Display = "6.1\"", Battery = 4385, InStock = true, Description = "Лучшая камера в своем классе, беспроводная зарядка и частота обновления экрана 90 Гц." },
        new Product { Id = 31, Name = "Google Pixel 6", Brand = "Google", Price = 36000, Img = "/img/google-pixel-6-seafoam-2.avif", Ram = 8, Storage = 128, Color = "Sorta Seafoam", Display = "6.4\"", Battery = 4614, InStock = true, Description = "Оригинальный двухцветный дизайн, первый чип Google Tensor и защита от воды IP68." },
        new Product { Id = 32, Name = "Google Pixel 6a", Brand = "Google", Price = 31500, Img = "/img/Pixel-6a.avif", Ram = 6, Storage = 128, Color = "Sage", Display = "6.1\"", Battery = 4410, InStock = true, Description = "Компактный телефон с легендарной вычислительной фотографией Pixel по супер-цене." },

        // Poco (5)
        new Product { Id = 17, Name = "Poco X6 Pro", Brand = "Poco", Price = 27000, Img = "/img/poco x6 pro.webp", Ram = 8, Storage = 256, Color = "Yellow", Display = "6.67\"", Battery = 5000, InStock = true, Description = "Игровой монстр на Dimensity 8300-Ultra с системой охлаждения LiquidCool 2.0 и HyperOS." },
        new Product { Id = 33, Name = "Poco F5", Brand = "Poco", Price = 36000, Img = "/img/poco f5.jpg", Ram = 12, Storage = 256, Color = "White", Display = "6.67\"", Battery = 5000, InStock = true, Description = "Мощнейший чип Snapdragon 7+ Gen 2, тонкие рамки экрана и вес всего 181 грамм." },
        new Product { Id = 34, Name = "Poco X5", Brand = "Poco", Price = 22500, Img = "/img/poco x5.webp", Ram = 8, Storage = 256, Color = "Green", Display = "6.67\"", Battery = 5000, InStock = true, Description = "AMOLED DotDisplay 120 Гц, чип Snapdragon 695 5G и тройная камера с AI." },
        new Product { Id = 35, Name = "Poco M6", Brand = "Poco", Price = 13500, Img = "/img/poco m6.jpg", Ram = 6, Storage = 128, Color = "Galactic Black", Display = "6.79\"", Battery = 5000, InStock = true, Description = "Стеклянная задняя крышка, большой экран 90 Гц и камера 108 Мп с сенсорным зумом 3x." },
        new Product { Id = 36, Name = "Poco C65", Brand = "Poco", Price = 11700, Img = "/img/poco-c65.jpg", Ram = 8, Storage = 256, Color = "Purple", Display = "6.74\"", Battery = 5000, InStock = true, Description = "Доступный смартфон с экраном 90 Гц, защитой зрения Corning Gorilla Glass и батареей 5000 мАч." },

        // Honor (5)
        new Product { Id = 20, Name = "Honor 90", Brand = "Honor", Price = 40500, Img = "/img/honor 90.jpg", Ram = 12, Storage = 512, Color = "Emerald Green", Display = "6.7\"", Battery = 5000, InStock = true, Description = "Камера 200 Мп, безвредный для глаз экран без мерцания (ШИМ 3840 Гц) и тонкий элегантный корпус." },
        new Product { Id = 37, Name = "Honor Magic 6", Brand = "Honor", Price = 81000, Img = "/img/magic-6-pro.webp", Ram = 12, Storage = 512, Color = "Green", Display = "6.8\"", Battery = 5600, InStock = true, Description = "Кремний-углеродный аккумулятор 2-го поколения, стекло повышенной прочности NanoCrystal Shield." },
        new Product { Id = 38, Name = "Honor X9b", Brand = "Honor", Price = 31500, Img = "/img/HONOR-X9b.jpg", Ram = 12, Storage = 256, Color = "Sunrise Orange", Display = "6.78\"", Battery = 5800, InStock = true, Description = "Сверхпрочный дисплей с защитой от падений с 1.5 метров и огромный аккумулятор 5800 мАч." },
        new Product { Id = 39, Name = "Honor 70", Brand = "Honor", Price = 36000, Img = "/img/honor-70.jpg", Ram = 8, Storage = 256, Color = "Crystal Silver", Display = "6.67\"", Battery = 4800, InStock = true, Description = "Двухсторонний изогнутый OLED экран 120 Гц, сенсор Sony IMX800 и режим Solo Cut для видеоблогеров." },
        new Product { Id = 40, Name = "Honor X8", Brand = "Honor", Price = 18000, Img = "/img/honor x8.jpg", Ram = 6, Storage = 128, Color = "Ocean Blue", Display = "6.7\"", Battery = 4000, InStock = true, Description = "Ультратонкий корпус 7.45 мм, экран HONOR FullView 90 Гц и технология RAM Turbo." }
    };
}
