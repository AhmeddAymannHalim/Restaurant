// Dish details: click a dish card and a panel opens with its picture, ingredients and sizes.
// A card opts in with data-detail='{"name":"","description":"","ingredients":"","image":"","selected":1,"sizes":[{"id":1,"label":"","price":0}]}'

$(function () {
    var dialog = $('#dishDetail');
    if (dialog.length === 0) {
        return;
    }

    var currency = dialog.data('currency');
    var addButton = dialog.find('[data-dd-add]');
    var lastFocus = null;

    $(document).on('click', '.ws-dish[data-detail]', function (event) {
        var clickedAControl = $(event.target).closest('button, select, a, input').length > 0;
        if (!clickedAControl) {
            openDialog($(this));
        }
    });

    $(document).on('keydown', function (event) {
        if (event.key === 'Escape') {
            closeDialog();
        }

        var card = $(event.target);
        var cardPressed = card.is('.ws-dish[data-detail]') && (event.key === 'Enter' || event.key === ' ');
        if (cardPressed) {
            event.preventDefault();
            openDialog(card);
        }
    });

    dialog.on('click', '[data-dd-close]', closeDialog);

    addButton.on('click', function () {
        setTimeout(closeDialog, 450);
    });

    dialog.on('click', '.dd-chip', function () {
        chooseSize($(this).data('id'), $(this).data('price'));
    });

    function openDialog(card) {
        var dish = JSON.parse(card.attr('data-detail'));
        lastFocus = document.activeElement;

        showDish(dish);

        dialog.prop('hidden', false);
        $('body').addClass('dd-lock');
        setTimeout(function () {
            dialog.addClass('is-open');
        }, 20);
        dialog.find('.dd-close').trigger('focus');
    }

    function closeDialog() {
        if (dialog.prop('hidden')) {
            return;
        }

        dialog.removeClass('is-open');
        setTimeout(function () {
            dialog.prop('hidden', true);
            $('body').removeClass('dd-lock');
            if (lastFocus) {
                $(lastFocus).trigger('focus');
            }
        }, 300);
    }

    function showDish(dish) {
        dialog.find('[data-dd-name]').text(dish.name);
        dialog.find('[data-dd-text]').text(dish.description).prop('hidden', !dish.description);

        var image = dialog.find('[data-dd-image]');
        image.empty();
        if (dish.image) {
            image.removeClass('is-empty').append($('<img>').attr('src', dish.image).attr('alt', dish.name));
        } else {
            image.addClass('is-empty');
        }

        showIngredients(dish.ingredients);
        showSizes(dish.sizes, dish.selected);
    }

    function showIngredients(ingredients) {
        var list = dialog.find('[data-dd-made-list]');
        list.empty();

        if (ingredients) {
            var names = ingredients.split(' - ');
            for (var i = 0; i < names.length; i++) {
                list.append($('<li></li>').text(names[i]).css('--i', i));
            }
        }

        dialog.find('[data-dd-made]').prop('hidden', !ingredients);
    }

    function showSizes(sizes, selectedId) {
        var box = dialog.find('[data-dd-sizes]');
        box.empty();

        var firstChoice = sizes[0];
        for (var i = 0; i < sizes.length; i++) {
            var chip = $('<button type="button" class="dd-chip"><strong></strong><small></small></button>');
            chip.data('id', sizes[i].id).data('price', sizes[i].price);
            chip.find('strong').text(sizes[i].label);
            chip.find('small').text(formatPrice(sizes[i].price));
            box.append(chip);

            if (sizes[i].id === selectedId) {
                firstChoice = sizes[i];
            }
        }

        dialog.find('[data-dd-sizes-box]').prop('hidden', sizes.length < 2);
        chooseSize(firstChoice.id, firstChoice.price);
    }

    function chooseSize(id, price) {
        dialog.find('[data-dd-price]').text(formatPrice(price));
        addButton.attr('data-cart-add', id);

        dialog.find('.dd-chip').each(function () {
            $(this).toggleClass('is-on', $(this).data('id') === id);
        });
    }

    function formatPrice(price) {
        return Number(price).toFixed(2) + ' ' + currency;
    }
});
