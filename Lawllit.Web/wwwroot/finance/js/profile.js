(function () {
    var themeForm = document.getElementById('theme-form');
    if (themeForm) {
        var themeToken = themeForm.querySelector('[name="__RequestVerificationToken"]').value;

        document.getElementById('theme-options').addEventListener('click', function (event) {
            var card = event.target.closest('[data-theme-value]');
            if (!card) return;

            var theme = card.dataset.themeValue;

            document.querySelectorAll('.theme-card').forEach(function (themeCard) {
                themeCard.classList.remove('theme-card--active');
            });
            card.classList.add('theme-card--active');

            document.documentElement.setAttribute('data-bs-theme', theme === 'high-contrast' ? 'dark' : theme);
            document.documentElement.setAttribute('data-theme', theme);

            fetch(themeForm.action, {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: '__RequestVerificationToken=' + encodeURIComponent(themeToken) + '&Theme=' + encodeURIComponent(theme)
            });
        });
    }

    var fontSizeForm = document.getElementById('font-size-form');
    if (fontSizeForm) {
        var fontSizeToken = fontSizeForm.querySelector('[name="__RequestVerificationToken"]').value;

        document.getElementById('font-size-options').addEventListener('click', function (event) {
            var card = event.target.closest('[data-font-size-value]');
            if (!card) return;

            var fontSize = card.dataset.fontSizeValue;

            document.querySelectorAll('[data-font-size-value]').forEach(function (sizeCard) {
                sizeCard.classList.remove('font-size-card--active');
            });
            card.classList.add('font-size-card--active');

            if (fontSize === 'normal') document.documentElement.removeAttribute('data-font-size');
            else document.documentElement.setAttribute('data-font-size', fontSize);

            fetch(fontSizeForm.action, {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: '__RequestVerificationToken=' + encodeURIComponent(fontSizeToken) + '&FontSize=' + encodeURIComponent(fontSize)
            });
        });
    }

    var languageForm = document.getElementById('language-form');
    if (languageForm) {
        var languageToken = languageForm.querySelector('[name="__RequestVerificationToken"]').value;

        document.getElementById('language-options').addEventListener('click', function (event) {
            var card = event.target.closest('[data-language-value]');
            if (!card) return;

            var language = card.dataset.languageValue;

            document.querySelectorAll('[data-language-value]').forEach(function (languageCard) {
                languageCard.classList.remove('font-size-card--active');
            });
            card.classList.add('font-size-card--active');

            fetch(languageForm.action, {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: '__RequestVerificationToken=' + encodeURIComponent(languageToken) + '&Language=' + encodeURIComponent(language)
            }).then(function () {
                location.reload();
            });
        });
    }
})();
