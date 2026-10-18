// Runs before the page is drawn: decides whether the opening screen plays.
// It plays once per browser session. Add ?intro=1 to the address to see it again.

(function () {
    var reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var asked = location.search.indexOf('intro') >= 0;

    var alreadySeen = false;
    try {
        alreadySeen = sessionStorage.getItem('z4intro') === '1';
    } catch (error) {
        alreadySeen = true;
    }

    if (!reducedMotion && (asked || !alreadySeen)) {
        document.documentElement.classList.add('z4-intro-on');
    }
})();
