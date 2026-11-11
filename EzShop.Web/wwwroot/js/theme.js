// Применяет сохранённую тему ДО первой отрисовки (подключается в <head>, без defer),
// чтобы не было вспышки неправильной темы.
(function () {
    var theme = 'dark';
    try {
        var saved = localStorage.getItem('ez_theme');
        if (saved === 'light' || saved === 'dark') theme = saved;
    } catch (e) { /* localStorage недоступен — остаёмся на тёмной */ }
    document.documentElement.setAttribute('data-theme', theme);
    var meta = document.querySelector('meta[name="theme-color"]');
    if (meta) meta.setAttribute('content', theme === 'light' ? '#f3f5fb' : '#070b1a');
})();
