(function () {
    document.addEventListener('click', function (e) {
        const link = e.target.closest('a[href^="#"]');
        if (!link) return;
        const target = document.querySelector(link.getAttribute('href'));
        if (!target) return;
        e.preventDefault();
        const nav = document.getElementById('main-nav');
        const offset = nav ? nav.offsetHeight + 8 : 0;
        window.scrollTo({ top: target.getBoundingClientRect().top + window.scrollY - offset, behavior: 'smooth' });

        const navContent = document.getElementById('navContent');
        if (navContent && navContent.classList.contains('show')) {
            const bsCollapse = bootstrap.Collapse.getInstance(navContent);
            if (bsCollapse) bsCollapse.hide();
        }
    });
})();
