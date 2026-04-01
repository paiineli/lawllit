(function () {
    // Smooth scroll
    document.addEventListener('click', function (e) {
        var link = e.target.closest('a[href^="#"]');
        if (!link) return;
        var target = document.querySelector(link.getAttribute('href'));
        if (!target) return;
        e.preventDefault();
        var nav = document.getElementById('main-nav');
        var offset = nav ? nav.offsetHeight + 8 : 0;
        window.scrollTo({ top: target.getBoundingClientRect().top + window.scrollY - offset, behavior: 'smooth' });
    });

    // Terminal animation
    var lines = document.querySelectorAll('.term-line');
    if (!lines.length) return;

    var maxDelay = 0;

    lines.forEach(function (line) {
        var delay = parseInt(line.dataset.delay || '0', 10);
        if (delay > maxDelay) maxDelay = delay;
        setTimeout(function () {
            line.classList.add('visible');
        }, delay + 300);
    });

    // Blink cursor after last line appears
    var cursor = document.querySelector('.term-cursor');
    if (cursor) {
        setTimeout(function () {
            cursor.style.opacity = '1';
            cursor.style.animation = 'blink 1.1s step-end infinite';
        }, maxDelay + 500);
    }
})();
