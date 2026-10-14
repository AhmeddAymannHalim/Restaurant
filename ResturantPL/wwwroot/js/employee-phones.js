/* Employee form: "+ Add" button for extra mobile numbers (saved as EmployeePhones rows),
   and opens any collapsed section that holds a validation error. */
(function () {
    "use strict";

    var list = document.querySelector("[data-phone-list]");
    if (list) {
        var rows = list.querySelector("[data-phone-rows]");
        var template = list.querySelector("[data-phone-template]");

        var reindex = function () {
            var items = rows.querySelectorAll("[data-phone-row]");
            for (var i = 0; i < items.length; i++) {
                var inputs = items[i].querySelectorAll("input[name]");
                for (var j = 0; j < inputs.length; j++) {
                    inputs[j].name = inputs[j].name.replace(/Phones\[[^\]]*\]/, "Phones[" + i + "]");
                }
            }
        };

        list.addEventListener("click", function (event) {
            var add = event.target.closest("[data-phone-add]");
            if (add) {
                var html = template.innerHTML.replace(/__i__/g, rows.querySelectorAll("[data-phone-row]").length);
                rows.insertAdjacentHTML("beforeend", html);
                var inputs = rows.querySelectorAll("[data-phone-row] input[type=text]");
                inputs[inputs.length - 1].focus();
                return;
            }

            var remove = event.target.closest("[data-phone-remove]");
            if (remove) {
                remove.closest("[data-phone-row]").remove();
                reindex();
            }
        });
    }

    var invalid = document.querySelectorAll(".ad-section .input-validation-error, .ad-section .field-validation-error");
    for (var i = 0; i < invalid.length; i++) {
        var section = invalid[i].closest("details");
        if (section) {
            section.open = true;
        }
    }
})();
