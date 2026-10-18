// Cashier screen: shows how many portions of each dish are left in stock.
// Every badge carries its number in data-left. A dish with no recipe has no number, so its badge is hidden.

$(function () {
    $('.pos-stock').each(function () {
        var badge = $(this);
        var left = badge.attr('data-left');

        if (left === '') {
            badge.hide();
            return;
        }

        left = Number(left);

        if (left >= 1000) {
            badge.find('span').text('999+');
        } else {
            badge.find('span').text(left);
        }

        if (left <= 0) {
            badge.addClass('is-out');
        } else if (left <= 5) {
            badge.addClass('is-low');
        }
    });
});
