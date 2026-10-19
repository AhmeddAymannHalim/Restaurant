// Reservation buttons used on the reservations page and on the dashboard.
// The texts come from the hidden #reservationTexts element (it is written inside the dashboard and reservation pages).

$(function () {
    fillReservationStatuses();
    countFloorTables();

    $(document).on('click', '.res-confirm', function () {
        setReservationStatus($(this).data('id'), 'Confirmed', '');
    });

    $(document).on('click', '.res-reject', function () {
        askRejectReason($(this).data('id'));
    });

    $(document).on('click', '.res-arrive', function () {
        guestsArrived($(this));
    });
});

// Writes the translated status name into every <span data-status="Pending"> on the page.
function fillReservationStatuses() {
    var texts = $('#reservationTexts').data();

    $('[data-status]').each(function () {
        var status = String($(this).data('status')).toLowerCase();
        $(this).text(texts[status]);
    });
}

// Writes how many tables are free, reserved and in use next to the legend.
function countFloorTables() {
    $('[data-count]').each(function () {
        var state = $(this).data('count');
        $(this).text($('.floor-table.is-' + state).length);
    });
}

function setReservationStatus(id, status, reason) {
    var texts = $('#reservationTexts').data();

    $.post('/TableReservation/SetStatus', { id: id, status: status, reason: reason })
        .done(function (result) {
            if (result.success) {
                location.reload();
            } else {
                Swal.fire(result.errorMessage || texts.error, '', 'error');
            }
        })
        .fail(function () {
            Swal.fire(texts.error, '', 'error');
        });
}

function askRejectReason(id) {
    var texts = $('#reservationTexts').data();

    Swal.fire({
        text: texts.rejectTitle,
        input: 'text',
        showCancelButton: true,
        confirmButtonText: texts.reject,
        cancelButtonText: texts.back,
        confirmButtonColor: '#d6453a'
    }).then(function (answer) {
        if (answer.isConfirmed) {
            setReservationStatus(id, 'Rejected', answer.value);
        }
    });
}

// The guests are here: mark the booking as seated and open a new order for their table.
function guestsArrived(button) {
    var texts = $('#reservationTexts').data();
    button.prop('disabled', true);

    $.post('/TableReservation/Arrive', { id: button.data('id') })
        .done(function (result) {
            if (result.success) {
                window.location.href = result.url;
            } else {
                button.prop('disabled', false);
                toastr.error(result.errorMessage);
            }
        })
        .fail(function () {
            button.prop('disabled', false);
            toastr.error(texts.error);
        });
}
