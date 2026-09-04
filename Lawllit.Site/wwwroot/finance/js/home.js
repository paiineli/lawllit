(function () {
    var line1Element = document.getElementById('typewriter-line1');
    var line2Element = document.getElementById('typewriter-line2');
    if (!line1Element || !line2Element) return;

    var line1Text = line1Element.dataset.text;
    var line2Text = line2Element.dataset.text;
    var charDelay = 60;

    function typeText(element, text, onComplete) {
        var index = 0;
        var interval = setInterval(function () {
            element.textContent += text[index];
            index++;
            if (index >= text.length) {
                clearInterval(interval);
                if (onComplete) onComplete();
            }
        }, charDelay);
    }

    typeText(line1Element, line1Text, function () {
        setTimeout(function () {
            typeText(line2Element, line2Text);
        }, 150);
    });
})();
