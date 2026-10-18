// Booking page: pick a date, time and group size, then a free table, then leave your details.
$(function () {
    var form = $('#rvForm');
    if (form.length === 0) {
        return;
    }

    var texts = form.data();
    var language = document.documentElement.lang;
    var whenField = $('#rvWhen');
    var guestsField = $('#rvGuests');
    var tableField = $('#rvTableId');
    var tablesBox = $('#rvTables');
    var submitButton = $('#rvSubmit');
    var slots = [];
    var lastRequest = 0;

    setStartingValues();
    loadTables();

    $('#rvDate, #rvTime').on('change', loadTables);

    $('[data-guests]').on('click', function () {
        var guests = getGuests() + Number($(this).data('guests'));
        if (guests < 1) {
            guests = 1;
        }
        if (guests > 20) {
            guests = 20;
        }

        guestsField.val(guests);
        loadTables();
    });

    tablesBox.on('change', 'input[name=tableChoice]', function () {
        tableField.val($(this).val());
        showSummary();
    });

    form.on('submit', function (event) {
        whenField.val(getWhen());

        var formIsValid = form[0].checkValidity() && Number(tableField.val()) > 0;
        if (!formIsValid) {
            event.preventDefault();
            form[0].reportValidity();
        }
    });

    function setStartingValues() {
        var saved = whenField.val();

        if (saved) {
            $('#rvDate').val(saved.substring(0, 10));
            $('#rvTime').val(saved.substring(11, 16));
        } else {
            var tomorrow = new Date();
            tomorrow.setDate(tomorrow.getDate() + 1);
            $('#rvDate').val(formatDate(tomorrow));
            $('#rvTime').val('20:00');
        }

        var today = new Date();
        var lastDay = new Date();
        lastDay.setDate(lastDay.getDate() + 60);
        $('#rvDate').attr('min', formatDate(today)).attr('max', formatDate(lastDay));

        if (Number(tableField.val()) === 0) {
            tableField.val('');
        }
    }

    function formatDate(date) {
        var month = ('0' + (date.getMonth() + 1)).slice(-2);
        var day = ('0' + date.getDate()).slice(-2);
        return date.getFullYear() + '-' + month + '-' + day;
    }

    function getWhen() {
        var date = $('#rvDate').val();
        var time = $('#rvTime').val();
        if (date && time) {
            return date + 'T' + time;
        }
        return '';
    }

    function getGuests() {
        return Number(guestsField.val()) || 1;
    }

    function getTableName(slot) {
        if (language === 'ar') {
            return slot.arName;
        }
        return slot.enName;
    }

    function findChosenSlot() {
        for (var i = 0; i < slots.length; i++) {
            if (String(slots[i].id) === String(tableField.val())) {
                return slots[i];
            }
        }
        return null;
    }

    function loadTables() {
        var when = getWhen();
        $('#rvGuestsView').val(getGuests());

        if (when === '') {
            slots = [];
            tablesBox.html('<p class="ws-note"></p>').find('p').text(texts.pick);
            showSummary();
            return;
        }

        lastRequest++;
        var thisRequest = lastRequest;
        tablesBox.addClass('is-loading');

        $.getJSON(texts.url, { when: when, guests: getGuests() })
            .done(function (data) {
                if (thisRequest !== lastRequest) {
                    return;
                }

                slots = data;
                clearChoiceIfNotPossible();
                drawTables();
                showSummary();
            })
            .fail(function () {
                tablesBox.html('<p class="ws-note"></p>').find('p').text(texts.failed);
            })
            .always(function () {
                tablesBox.removeClass('is-loading');
            });
    }

    function clearChoiceIfNotPossible() {
        var chosen = findChosenSlot();
        if (chosen && (chosen.isReserved || chosen.tooSmall)) {
            tableField.val('');
        }
    }

    function drawTables() {
        tablesBox.empty();

        if (slots.length === 0) {
            tablesBox.html('<p class="ws-note"></p>').find('p').text(texts.none);
            return;
        }

        for (var i = 0; i < slots.length; i++) {
            tablesBox.append(drawTable(slots[i]));
        }
    }

    function drawTable(slot) {
        var isBlocked = slot.isReserved || slot.tooSmall;
        var note = slot.numOfPersons + ' ' + texts.seats;
        if (slot.isReserved) {
            note = texts.taken;
        } else if (slot.tooSmall) {
            note = texts.small;
        }

        var label = $('<label class="rv-table"></label>');
        if (isBlocked) {
            label.addClass('is-blocked');
        }
        if (slot.isReserved) {
            label.addClass('is-taken');
        }

        var radio = $('<input type="radio" name="tableChoice">').val(slot.id);
        radio.prop('disabled', isBlocked);
        radio.prop('checked', String(slot.id) === String(tableField.val()));

        var card = $('<span class="rv-card"></span>');
        card.append(drawChairs(slot.numOfPersons));
        card.append($('<strong></strong>').text(getTableName(slot)));
        card.append($('<small></small>').text(note));

        return label.append(radio).append(card);
    }

    // A round table with one dot for every chair.
    function drawChairs(count) {
        var svgNamespace = 'http://www.w3.org/2000/svg';
        var svg = document.createElementNS(svgNamespace, 'svg');
        svg.setAttribute('viewBox', '0 0 100 100');

        var top = document.createElementNS(svgNamespace, 'circle');
        top.setAttribute('class', 'rv-top');
        top.setAttribute('cx', 50);
        top.setAttribute('cy', 50);
        top.setAttribute('r', 22);
        svg.appendChild(top);

        for (var i = 0; i < count; i++) {
            var angle = (Math.PI * 2 * i) / count - Math.PI / 2;
            var chair = document.createElementNS(svgNamespace, 'circle');
            chair.setAttribute('class', 'rv-chair');
            chair.setAttribute('cx', 50 + 37 * Math.cos(angle));
            chair.setAttribute('cy', 50 + 37 * Math.sin(angle));
            chair.setAttribute('r', 6.5);
            svg.appendChild(chair);
        }

        return svg;
    }

    function showSummary() {
        var when = getWhen();
        var chosen = findChosenSlot();

        if (when) {
            var date = new Date(when);
            var options = { weekday: 'long', day: 'numeric', month: 'long', hour: '2-digit', minute: '2-digit' };
            $('#rvSumWhen').text(date.toLocaleString(language, options));
        } else {
            $('#rvSumWhen').text('-');
        }

        $('#rvSumGuests').text(getGuests());

        if (chosen) {
            $('#rvSumTable').text(getTableName(chosen));
        } else {
            $('#rvSumTable').text(texts.notChosen);
        }

        submitButton.prop('disabled', !(when && chosen));
    }
});
