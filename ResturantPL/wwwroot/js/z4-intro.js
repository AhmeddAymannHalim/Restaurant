// Opening screen: the logo spins in, the welcome line is typed, the short story appears line by line,
// then the screen slides up. It plays once per browser session (add ?intro=1 to the address to see it again).

$(function () {
    var intro = $('#z4Intro');
    if (intro.length === 0) {
        return;
    }

    var finished = false;

    if (!$('html').hasClass('z4-intro-on')) {
        intro.remove();
        return;
    }

    intro.find('.z4i-skip').trigger('focus');
    intro.on('click', '[data-z4-skip]', finish);

    $(document).on('keydown', function (event) {
        if (event.key === 'Escape') {
            finish();
        }
    });

    // the logo spins in for about 2 seconds, then the welcome line starts
    setTimeout(startWelcome, 2000);

    function startWelcome() {
        if (finished) {
            return;
        }

        intro.addClass('is-welcome');
        typeWelcome(intro.data('welcome'), 0);
    }

    function typeWelcome(text, position) {
        if (finished) {
            return;
        }

        $('[data-z4-typed]').text(text.substring(0, position));

        if (position < text.length) {
            setTimeout(function () {
                typeWelcome(text, position + 1);
            }, 48);
        } else {
            setTimeout(function () {
                showStoryLine(0);
            }, 350);
        }
    }

    function showStoryLine(number) {
        if (finished) {
            return;
        }

        var lines = intro.find('.z4i-story p');

        if (number < lines.length) {
            lines.eq(number).addClass('is-in');
            setTimeout(function () {
                showStoryLine(number + 1);
            }, 1100);
        } else {
            intro.addClass('is-ready');
            setTimeout(finish, 3200);
        }
    }

    function finish() {
        if (finished) {
            return;
        }

        finished = true;
        rememberThatItPlayed();
        intro.addClass('is-leaving');

        setTimeout(function () {
            $('html').removeClass('z4-intro-on');
            intro.remove();
        }, 950);
    }

    function rememberThatItPlayed() {
        try {
            sessionStorage.setItem('z4intro', '1');
        } catch (error) {
            // private browsing: the intro will simply play again next time
        }
    }
});
