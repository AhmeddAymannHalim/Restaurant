function Delete(id) {
    swal({
        title: "هل انت متأكد",
        text: "بمجرد الحذف لن تتمكن من استعادته مجددا",
        icon: "warning",
        buttons: ["الغاء", "حذف"],
        dangerMode: true,
    })
        .then((willDelete) => {
            if (willDelete) {
                window.location.href = "/Table/Delete/" + id;
                swal("تم الحذف ينجاح", {
                    icon: "success",
                });
            } else {
                swal("تم الغاء عملية الحذف");
            }
        });
}