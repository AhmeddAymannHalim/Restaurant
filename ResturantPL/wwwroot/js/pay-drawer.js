/* POS payment drawer.
   Cash and wallets: confirm the amount. Visa: a card form with a short "processing" step.
   This is a front-end flow only: card details stay in these inputs, are never submitted,
   and are cleared as soon as the order is sent. The server only receives the payment method. */
(function ($) {
    'use strict';

    var $drawer = $('#payDrawer');
    if (!$drawer.length) { return; }

    var $form = $('#orderForm');
    var $stage = $drawer.find('.pay-stage');
    var names = $drawer.data('names') || {};
    var currency = $drawer.data('currency');
    var method = 'Cash';
    var busy = false;
    var holderText = $drawer.find('.pay-card-name').text();
    var approvedText = $drawer.find('.pay-done-msg').text();
    var reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    function wait(ms) { return $.Deferred(function (d) { setTimeout(d.resolve, reduce ? Math.min(ms, 120) : ms); }).promise(); }
    function amount() { return $('#total').text(); }
    function fill(text, values) {
        return text.replace('{0}', values[0]).replace('{1}', values[1] || '');
    }

    function quickMessage() {
        var sum = amount() + ' ' + currency;
        return method === 'Cash'
            ? fill($drawer.data('msg-cash'), [sum])
            : fill($drawer.data('msg-via'), [sum, names[method]]);
    }

    // Slide the current pane up, then slide the next one down; the stage height follows the content.
    function show(pane, prepare) {
        var $cur = $stage.children('.pay-pane:not([hidden])');
        var $next = $stage.children('[data-pane="' + pane + '"]');
        var from = $stage.outerHeight();
        $stage.css('height', from);
        $cur.addClass('is-leaving');

        wait(170).then(function () {
            $cur.prop('hidden', true).removeClass('is-leaving');
            if (prepare) { prepare(); }
            $next.prop('hidden', false).addClass('is-entering');
            $stage.css('height', $next.outerHeight(true));
            wait(320).then(function () {
                $next.removeClass('is-entering');
                $stage.css('height', '');
            });
        });
    }

    function setMethod(next) {
        if (busy || next === method) { return; }
        method = next;
        $drawer.find('.pay-chip').each(function () {
            var on = $(this).data('method') === method;
            $(this).toggleClass('is-on', on).attr('aria-checked', on);
        });
        $('#paymentMethod').val(method);

        if (method === 'Visa') {
            show('card', function () { $drawer.find('.pay-error').text(''); });
            wait(520).then(function () { $('#cardName').trigger('focus'); });
        } else {
            show('quick', function () {
                $drawer.find('.pay-msg').text(quickMessage());
                $drawer.find('.pay-badge i').attr('class', 'fas ' + $drawer.find('.pay-chip.is-on i').attr('class').split(' ')[1]);
            });
        }
    }

    function open() {
        if (!$form[0].reportValidity()) { return; }
        if ($('#cartTable tbody tr').length === 0) {
            if (window.toastr) { toastr.error($('#cartEmpty').text()); }
            return;
        }

        method = 'Cash';
        $('#paymentMethod').val(method);
        $drawer.find('.pay-chip').removeClass('is-on').attr('aria-checked', false)
            .filter('[data-method="Cash"]').addClass('is-on').attr('aria-checked', true);
        $stage.children('.pay-pane').prop('hidden', true);
        $stage.children('[data-pane="quick"]').prop('hidden', false);
        $drawer.find('.pay-amount').text(amount() + ' ' + currency);
        $drawer.find('.pay-msg').text(quickMessage());
        $drawer.find('.pay-badge i').attr('class', 'fas fa-money-bill-wave');
        $drawer.removeClass('is-busy is-closing').prop('hidden', false);
        wait(50).then(function () { $drawer.find('.pay-confirm').trigger('focus'); });
    }

    function close() {
        if (busy) { return; }
        $drawer.addClass('is-closing');
        wait(280).then(function () {
            $drawer.prop('hidden', true).removeClass('is-closing');
            clearCard();
        });
    }

    function clearCard() {
        $('#cardName, #cardNumber, #cardExp, #cardCvv').val('');
        $drawer.find('.pay-card-no').text('•••• •••• •••• ••••');
        $drawer.find('.pay-card-name').text(holderText);
        $drawer.find('.pay-card-exp').text('MM/YY');
    }

    // ---- card form ----
    $('#cardNumber').on('input', function () {
        var digits = this.value.replace(/\D/g, '').slice(0, 16);
        this.value = digits.replace(/(.{4})/g, '$1 ').trim();
        var padded = (digits + '•'.repeat(16)).slice(0, 16);
        $drawer.find('.pay-card-no').text(padded.replace(/(.{4})/g, '$1 ').trim());
    });
    $('#cardExp').on('input', function () {
        var d = this.value.replace(/\D/g, '').slice(0, 4);
        this.value = d.length > 2 ? d.slice(0, 2) + '/' + d.slice(2) : d;
        $drawer.find('.pay-card-exp').text(this.value || 'MM/YY');
    });
    $('#cardCvv').on('input', function () { this.value = this.value.replace(/\D/g, '').slice(0, 3); });
    $('#cardName').on('input', function () {
        $drawer.find('.pay-card-name').text(this.value || holderText);
    });

    function cardError() {
        if ($('#cardName').val().trim().length < 3) { return '#cardName'; }
        if ($('#cardNumber').val().replace(/\D/g, '').length !== 16) { return '#cardNumber'; }
        var m = /^(\d{2})\/(\d{2})$/.exec($('#cardExp').val());
        var now = new Date();
        if (!m || +m[1] < 1 || +m[1] > 12 || (2000 + +m[2]) * 12 + +m[1] < now.getFullYear() * 12 + now.getMonth() + 1) { return '#cardExp'; }
        if ($('#cardCvv').val().length !== 3) { return '#cardCvv'; }
        return null;
    }

    // ---- confirm ----
    function finish() {
        // Leave the page with the same motion the cards use, then send the real order.
        $('.ad-card').addClass('is-leaving');
        $drawer.addClass('is-closing');
        wait(380).then(function () {
            clearCard();
            $('#cash').val('true');
            $('#onAccount').val('false');
            $form[0].requestSubmit();
        });
    }

    function confirm() {
        if (busy) { return; }

        if (method === 'Visa') {
            var bad = cardError();
            if (bad) {
                $drawer.find('.pay-error').text($drawer.data('bad'));
                $(bad).trigger('focus');
                $drawer.find('.pay-pane[data-pane="card"]').removeClass('is-shaking');
                void $drawer[0].offsetWidth;
                $drawer.find('.pay-pane[data-pane="card"]').addClass('is-shaking');
                return;
            }
        }

        busy = true;
        $drawer.addClass('is-busy');

        var run = $.Deferred().resolve().promise();
        if (method === 'Visa') {
            show('busy');
            run = wait(2300);
        }

        run.then(function () {
            show('done', function () {
                $drawer.find('.pay-done-msg').text(method === 'Visa' ? approvedText : quickMessage());
            });
            return wait(1100);
        }).then(finish);
    }

    $drawer.find('.pay-chip').on('click', function () { setMethod($(this).data('method')); });
    $drawer.find('.pay-confirm').on('click', confirm);
    $drawer.find('.pay-cancel, .pay-x').on('click', close);
    $drawer.on('keydown', function (e) {
        if (e.key === 'Escape') { close(); }
        if (e.key === 'Enter' && $(e.target).is('input')) { e.preventDefault(); confirm(); }
    });

    $('#payNow').on('click', function (e) {
        e.preventDefault();
        open();
    });
})(jQuery);
