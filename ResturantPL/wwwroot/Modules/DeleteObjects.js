function Delete(url, redirectUrl) {
    var t = window.deleteText || {
        title: "هل انت متأكد", warning: "بمجرد الحذف لن تتمكن من استعادته مجددا", cancel: "الغاء", confirm: "حذف",
        done: "تم الحذف بنجاح", failed: "فشلت العملية", error: "حدث خطأ أثناء عملية الحذف", aborted: "تم الغاء عملية الحذف"
    };
    swal({
        title: t.title,
        text: t.warning,
        icon: "warning",
        buttons: [t.cancel, t.confirm],
        dangerMode: true,
    })
        .then((willDelete) => {
            if (willDelete) {
                $.ajax({
                    url: url,
                    type: "POST",
                    success: function (result) {
                        if (result.success) {
                            swal(t.done, {
                                icon: "success",
                            });

                            if (redirectUrl) {
                                window.location.href = redirectUrl;
                            } else if (window.appAjax) {
                                window.appAjax.reload();
                            } else {
                                location.reload();
                            }
                        } else {
                            swal(t.failed, result.errorMessage || "", "error");
                        }
                    },
                    error: function () {
                        swal(t.error, "", "error");
                    }
                });
            } else {
                swal(t.aborted);
            }
        });
}
