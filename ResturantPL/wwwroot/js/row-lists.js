/* Repeating rows of a form ("+ Add" button), used by the employee form for extra mobiles, dependents,
   allowances, fixed deductions, leaves and documents. A list is any [data-row-list] that holds
   [data-rows], a [data-row-template] with __i__ in the field names, a [data-row-add] button and
   [data-row-remove] buttons inside each [data-row]. It also opens any collapsed section with a validation error. */
(function () {
    "use strict";

    var lists = document.querySelectorAll("[data-row-list]");
    for (var l = 0; l < lists.length; l++) {
        (function (list) {
            var prefix = list.getAttribute("data-prefix");
            var rows = list.querySelector("[data-rows]");
            var template = list.querySelector("[data-row-template]");
            var pattern = new RegExp("^" + prefix + "\\[[^\\]]*\\]");

            var reindex = function () {
                var items = rows.querySelectorAll("[data-row]");
                for (var i = 0; i < items.length; i++) {
                    var inputs = items[i].querySelectorAll("[name]");
                    for (var j = 0; j < inputs.length; j++) {
                        inputs[j].name = inputs[j].name.replace(pattern, prefix + "[" + i + "]");
                    }
                }
            };

            list.addEventListener("click", function (event) {
                if (event.target.closest("[data-row-add]")) {
                    var html = template.innerHTML.replace(/__i__/g, rows.querySelectorAll("[data-row]").length);
                    rows.insertAdjacentHTML("beforeend", html);
                    var fields = rows.querySelectorAll("[data-row]:last-child input:not([type=hidden]), [data-row]:last-child select");
                    if (fields.length) {
                        fields[0].focus();
                    }

                    return;
                }

                var remove = event.target.closest("[data-row-remove]");
                if (remove) {
                    remove.closest("[data-row]").remove();
                    reindex();
                }
            });
        })(lists[l]);
    }

    var invalid = document.querySelectorAll(".ad-section .input-validation-error, .ad-section .field-validation-error");
    for (var i = 0; i < invalid.length; i++) {
        var section = invalid[i].closest("details");
        if (section) {
            section.open = true;
        }
    }
})();
