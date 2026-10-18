// Shared dialogs. Every delete button on the site calls Delete('/Controller/Delete/5').
// The texts come from the localization files (see Views/Shared/_DeleteText.cshtml).

function Delete(url, redirectUrl) {
    var text = window.deleteText;

    Swal.fire({
        title: text.title,
        text: text.warning,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: text.confirm,
        cancelButtonText: text.cancel,
        confirmButtonColor: '#d6453a'
    }).then(function (answer) {
        if (!answer.isConfirmed) {
            return;
        }

        $.post(url)
            .done(function (result) {
                if (result.success) {
                    deleteDone(redirectUrl);
                } else {
                    Swal.fire(text.failed, result.errorMessage || '', 'error');
                }
            })
            .fail(function () {
                Swal.fire(text.error, '', 'error');
            });
    });
}

function deleteDone(redirectUrl) {
    Swal.fire({
        icon: 'success',
        title: window.deleteText.done,
        timer: 900,
        showConfirmButton: false
    }).then(function () {
        if (redirectUrl) {
            window.location.href = redirectUrl;
        } else if (window.appAjax) {
            window.appAjax.reload();
        } else {
            location.reload();
        }
    });
}
