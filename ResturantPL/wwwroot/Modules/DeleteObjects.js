function Delete(url, redirectUrl) {
    swal({
        title: "هل انت متأكد",
        text: "بمجرد الحذف لن تتمكن من استعادته مجددا",
        icon: "warning",
        buttons: ["الغاء", "حذف"],
        dangerMode: true,
    })
        .then((willDelete) => {
            if (willDelete) {
                $.ajax({
                    url: url,
                    type: "POST",
                    success: function (result) {
                        if (result.success) {
                            swal("تم الحذف بنجاح", {
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
                            swal("فشلت العملية", result.errorMessage || "", "error");
                        }
                    },
                    error: function () {
                        swal("حدث خطأ أثناء عملية الحذف", "", "error");
                    }
                });
            } else {
                swal("تم الغاء عملية الحذف");
            }
        });
}
