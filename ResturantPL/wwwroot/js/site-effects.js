// Public site effects: the header shadow, the loading circle and the scroll animations.

$(function () {
    var reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    $(window).on('scroll', function () {
        $('.site-nav').toggleClass('is-top', window.scrollY < 8);
    }).trigger('scroll');

    $(window).on('load', function () {
        var loader = $('.ff-preloader');
        loader.addClass('is-done');
        setTimeout(function () {
            loader.remove();
        }, 600);

        if (window.WOW && !reducedMotion) {
            new WOW({ offset: 60 }).init();
        } else {
            $('.wow').css('visibility', 'visible');
        }
    });
});
