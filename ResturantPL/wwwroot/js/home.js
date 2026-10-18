// Home page: fills the small details, loads the menu, starts the chef slider and the photo viewer.

$(function () {
    var language = document.documentElement.lang;
    var menu = $('#menu');
    var dishesBox = $('#dishes');
    var texts = menu.data();

    fillBannerAndPhoto();
    fillWorkingDays();
    hideEmptyParts();
    $('#year').text(new Date().getFullYear());

    startChefSlider();
    startPhotoViewer();

    loadDishes('');

    $('.ws-tab').on('click', function () {
        $('.ws-tab').removeClass('is-on');
        $(this).addClass('is-on');
        loadDishes($(this).data('section'));
    });

    function fillBannerAndPhoto() {
        var heroImage = $('#ws-top').data('hero');
        if (heroImage) {
            $('#ws-top').css('--hero', "url('" + heroImage + "')");
        }

        var photo = $('#welcomeImage');
        if (photo.data('image')) {
            photo.attr('src', photo.data('image')).removeClass('is-art');
        }
    }

    // the opening days are saved as English or Arabic day names; show them in the page language
    function fillWorkingDays() {
        var dayNames = $('#dayNames').data();
        var arabicDays = {
            'السبت': 'saturday', 'الأحد': 'sunday', 'الاحد': 'sunday', 'الاثنين': 'monday', 'الإثنين': 'monday',
            'الثلاثاء': 'tuesday', 'الأربعاء': 'wednesday', 'الاربعاء': 'wednesday', 'الخميس': 'thursday', 'الجمعة': 'friday'
        };

        $('[data-day]').each(function () {
            var saved = String($(this).data('day')).trim();
            var key = arabicDays[saved] || saved.toLowerCase();
            if (dayNames[key]) {
                $(this).text(dayNames[key]);
            }
        });
    }

    // a contact card with nothing to show is hidden, and so are empty sections
    function hideEmptyParts() {
        $('.ff-card').filter(function () {
            return $(this).attr('data-value') === '';
        }).hide();

        $('.ff-play').filter(function () {
            return !$(this).attr('href');
        }).hide();

        if ($('#chefList .ff-chef').length === 0) {
            $('#chefs').hide();
        }

        if ($('#gallery .ff-shot').length === 0) {
            $('#gallery').hide();
        }
    }

    function startChefSlider() {
        var chefs = $('#chefList');
        var count = chefs.children().length;
        if (count === 0) {
            return;
        }

        chefs.owlCarousel({
            loop: count > 4,
            margin: 28,
            autoplay: true,
            autoplayTimeout: 3200,
            autoplayHoverPause: true,
            smartSpeed: 800,
            dots: true,
            nav: false,
            rtl: document.documentElement.dir === 'rtl',
            responsive: { 0: { items: 1 }, 576: { items: Math.min(2, count) }, 992: { items: Math.min(4, count) } }
        });
    }

    function startPhotoViewer() {
        if (window.GLightbox) {
            GLightbox({ selector: '.glightbox' });
        }
    }

    function loadDishes(sectionId) {
        dishesBox.html('<div class="ws-skel"></div><div class="ws-skel"></div><div class="ws-skel"></div>');

        $.getJSON(texts.url, { sectionId: sectionId })
            .done(function (dishes) {
                showDishes(dishes);
            })
            .fail(function () {
                dishesBox.html('<p class="ws-note"></p>').find('p').text(texts.failed);
            });
    }

    function showDishes(dishes) {
        dishesBox.empty();

        if (dishes.length === 0) {
            dishesBox.html('<p class="ws-note"></p>').find('p').text(texts.empty);
            return;
        }

        for (var i = 0; i < dishes.length; i++) {
            dishesBox.append(drawDish(dishes[i], i, dishes));
        }
    }

    function pick(arabicText, englishText) {
        if (language === 'ar') {
            return arabicText || englishText;
        }
        return englishText || arabicText;
    }

    function sizeName(size) {
        if (!size) {
            return '';
        }

        var key = 'size' + size.replace('_', '').replace('SyrianBread', 'SyrianBread');
        var names = {
            Small: texts.sizeSmall,
            Medium: texts.sizeMedium,
            Large: texts.sizeLarge,
            Syrian_Bread: texts.sizeSyrianBread,
            regular: texts.sizeRegular
        };
        return names[size] || size;
    }

    function formatPrice(price) {
        return Number(price).toLocaleString(undefined, { minimumFractionDigits: 0, maximumFractionDigits: 2 });
    }

    // all the sizes of the same dish, so the details panel can offer them
    function findSizes(dish, dishes) {
        var sizes = [];
        for (var i = 0; i < dishes.length; i++) {
            var other = dishes[i];
            if (other.arName === dish.arName && other.enName === dish.enName) {
                sizes.push({ id: other.id, label: sizeName(other.size), price: other.price });
            }
        }
        return sizes;
    }

    function drawDish(dish, index, dishes) {
        var name = pick(dish.arName, dish.enName);
        var description = pick(dish.arDescription, dish.enDescription);
        var ingredients = pick(dish.arIngredients, dish.enIngredients);

        var details = {
            name: name,
            description: description || '',
            ingredients: ingredients || '',
            image: dish.imageUrl || '',
            selected: dish.id,
            sizes: findSizes(dish, dishes)
        };

        var card = $('<article class="ws-dish" tabindex="0"></article>');
        card.attr('data-detail', JSON.stringify(details));
        card.css('--i', index);

        var picture = $('<div class="ws-dish-img"></div>');
        if (dish.imageUrl) {
            picture.append($('<img loading="lazy">').attr('src', dish.imageUrl).attr('alt', name));
        }

        var head = $('<div class="ws-dish-head"></div>');
        head.append($('<h3 class="ws-dish-name"></h3>').text(name));
        head.append($('<span class="ws-dish-price"></span>').text(formatPrice(dish.price) + ' ' + texts.currency));

        var body = $('<div class="ws-dish-body"></div>');
        body.append(head);

        if (dish.size) {
            body.append($('<span class="ws-dish-size"></span>').text(sizeName(dish.size)));
        }
        if (description) {
            body.append($('<p class="ws-dish-desc"></p>').text(description));
        }
        if (ingredients) {
            body.append($('<p class="ws-dish-desc ws-dish-made"></p>').text(texts.madeOf + ' ' + ingredients));
        }

        var addButton = $('<button type="button" class="ws-dish-add"></button>');
        addButton.attr('data-cart-add', dish.id);
        addButton.append('<i class="bi bi-bag-plus" aria-hidden="true"></i> ');
        addButton.append(document.createTextNode(texts.add));
        body.append(addButton);

        return card.append(picture).append(body);
    }
});
