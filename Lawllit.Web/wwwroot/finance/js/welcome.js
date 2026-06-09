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

    function applyTheme(value) {
        document.documentElement.setAttribute('data-bs-theme', value === 'high-contrast' ? 'dark' : value);
        document.documentElement.setAttribute('data-theme', value);
    }

    function applyFontSize(value) {
        if (value === 'normal') document.documentElement.removeAttribute('data-font-size');
        else document.documentElement.setAttribute('data-font-size', value);
    }

    bindPreference({ containerId: 'language-options', attr: 'language-value', datasetKey: 'languageValue', activeClass: 'welcome-option-card--active', key: 'language', afterSave: function () { window.location.href = '/Finance/Welcome?step=1'; } });
    bindPreference({ containerId: 'currency-options', attr: 'currency-value', datasetKey: 'currencyValue', activeClass: 'welcome-option-card--active', key: 'currency' });
    bindPreference({ containerId: 'theme-options', attr: 'theme-value', datasetKey: 'themeValue', activeClass: 'theme-card--active', key: 'theme', onSelect: applyTheme });
    bindPreference({ containerId: 'font-size-options', attr: 'font-size-value', datasetKey: 'fontSizeValue', activeClass: 'welcome-option-card--active', key: 'fontSize', onSelect: applyFontSize });

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
