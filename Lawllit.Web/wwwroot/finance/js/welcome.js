(function () {
    var progressContainer = document.querySelector('[data-current-step]');
    if (!progressContainer) return;

    var currentStep = parseInt(progressContainer.dataset.currentStep, 10);
    var totalSteps = parseInt(progressContainer.dataset.totalSteps, 10);
    var stepLabelTemplate = progressContainer.dataset.stepLabel;

    var backButton = document.getElementById('back-btn');
    var nextButton = document.getElementById('next-btn');
    var nextButtonLabel = document.getElementById('next-btn-label');
    var nextButtonIcon = document.getElementById('next-btn-icon');
    var completeForm = document.getElementById('complete-form');

    function showStep(stepNumber) {
        currentStep = stepNumber;

        document.querySelectorAll('.welcome-dot').forEach(function (dotElement, dotIndex) {
            if (dotIndex < stepNumber) {
                dotElement.classList.add('welcome-dot--active');
            } else {
                dotElement.classList.remove('welcome-dot--active');
            }
        });

        var stepLabelElement = document.getElementById('welcome-step-label');
        if (stepLabelElement) {
            stepLabelElement.textContent = stepLabelTemplate
                .replace('{0}', stepNumber)
                .replace('{1}', totalSteps);
        }

        document.querySelectorAll('.welcome-step').forEach(function (stepElement) {
            if (parseInt(stepElement.dataset.step, 10) === stepNumber) {
                stepElement.classList.remove('welcome-step--hidden');
            } else {
                stepElement.classList.add('welcome-step--hidden');
            }
        });

        if (backButton) {
            if (stepNumber === 1) {
                backButton.classList.add('invisible');
            } else {
                backButton.classList.remove('invisible');
            }
        }

        if (nextButton && nextButtonLabel && nextButtonIcon) {
            if (stepNumber === 4) {
                nextButtonLabel.textContent = nextButton.dataset.labelComplete;
                nextButtonIcon.classList.add('d-none');
            } else {
                nextButtonLabel.textContent = nextButton.dataset.labelNext;
                nextButtonIcon.classList.remove('d-none');
            }
        }
    }

    showStep(currentStep);

    var languageForm = document.getElementById('language-form');
    if (languageForm) {
        var languageToken = languageForm.querySelector('[name="__RequestVerificationToken"]').value;

        document.getElementById('language-options').addEventListener('click', function (event) {
            var card = event.target.closest('[data-language-value]');
            if (!card) return;

            var language = card.dataset.languageValue;

            document.querySelectorAll('[data-language-value]').forEach(function (languageCard) {
                languageCard.classList.remove('welcome-option-card--active');
            });
            card.classList.add('welcome-option-card--active');

            fetch(languageForm.action, {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: '__RequestVerificationToken=' + encodeURIComponent(languageToken) + '&Language=' + encodeURIComponent(language)
            }).then(function () {
                window.location.href = '/Finance/Welcome?step=1';
            });
        });
    }

    var currencyForm = document.getElementById('currency-form');
    if (currencyForm) {
        var currencyToken = currencyForm.querySelector('[name="__RequestVerificationToken"]').value;

        document.getElementById('currency-options').addEventListener('click', function (event) {
            var card = event.target.closest('[data-currency-value]');
            if (!card) return;

            var currency = card.dataset.currencyValue;

            document.querySelectorAll('[data-currency-value]').forEach(function (currencyCard) {
                currencyCard.classList.remove('welcome-option-card--active');
            });
            card.classList.add('welcome-option-card--active');

            fetch(currencyForm.action, {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: '__RequestVerificationToken=' + encodeURIComponent(currencyToken) + '&Currency=' + encodeURIComponent(currency)
            });
        });
    }

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
                sizeCard.classList.remove('welcome-option-card--active');
            });
            card.classList.add('welcome-option-card--active');

            if (fontSize === 'normal') {
                document.documentElement.removeAttribute('data-font-size');
            } else {
                document.documentElement.setAttribute('data-font-size', fontSize);
            }

            fetch(fontSizeForm.action, {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: '__RequestVerificationToken=' + encodeURIComponent(fontSizeToken) + '&FontSize=' + encodeURIComponent(fontSize)
            });
        });
    }

    if (nextButton) {
        nextButton.addEventListener('click', function () {
            if (currentStep === 4) {
                if (completeForm) {
                    completeForm.submit();
                }
                return;
            }

            if (currentStep === 1) {
                window.location.href = '/Finance/Welcome?step=2';
                return;
            }

            showStep(currentStep + 1);
        });
    }

    if (backButton) {
        backButton.addEventListener('click', function () {
            if (currentStep === 2) {
                window.location.href = '/Finance/Welcome?step=1';
                return;
            }

            if (currentStep > 2) {
                showStep(currentStep - 1);
            }
        });
    }

    document.querySelectorAll('[data-complete-onboarding]').forEach(function (button) {
        button.addEventListener('click', function () {
            if (completeForm) {
                completeForm.submit();
            }
        });
    });
})();
