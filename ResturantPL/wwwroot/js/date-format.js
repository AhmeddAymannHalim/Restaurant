/* Shows every <input type="date"> as a readable date ("14 أكتوبر 2026" / "14 October 2026") instead of the
   browser's dd/mm/yyyy mask. The input itself stays a native date input (same ISO value, same name, same
   picker), so form posting and validation are untouched. Rows added later by row-lists.js are picked up too. */
(function () {
    "use strict";

    var arabic = (document.documentElement.lang || "").indexOf("ar") === 0;
    var formatter = new Intl.DateTimeFormat(arabic ? "ar-EG-u-nu-latn" : "en-GB", { day: "numeric", month: "long", year: "numeric" });
    var placeholder = arabic ? "اختر التاريخ" : "Select date";

    var render = function (input, label) {
        var m = /^(\d{4})-(\d{2})-(\d{2})/.exec(input.value || "");
        if (m) {
            label.textContent = formatter.format(new Date(+m[1], +m[2] - 1, +m[3]));
            label.classList.remove("is-empty");
        } else {
            label.textContent = placeholder;
            label.classList.add("is-empty");
        }
    };

    var upgrade = function (input) {
        if (input.dataset.dateUpgraded) {
            return;
        }
        input.dataset.dateUpgraded = "1";

        var wrap = document.createElement("span");
        wrap.className = "ad-date" + (input.classList.contains("w-auto") ? " w-auto" : "");
        var label = document.createElement("span");
        label.className = "ad-date-text form-control";
        label.setAttribute("aria-hidden", "true");
        input.parentNode.insertBefore(wrap, input);
        wrap.appendChild(label);
        wrap.appendChild(input);
        input.classList.add("ad-date-input");

        var refresh = function () { render(input, label); };
        input.addEventListener("input", refresh);
        input.addEventListener("change", refresh);
        input.addEventListener("click", function () {
            if (input.showPicker) {
                try { input.showPicker(); } catch (e) { /* picker already open */ }
            }
        });
        refresh();
    };

    var scan = function (root) {
        var inputs = root.querySelectorAll('input[type="date"]:not([data-date-upgraded])');
        for (var i = 0; i < inputs.length; i++) {
            upgrade(inputs[i]);
        }
    };

    scan(document);
    new MutationObserver(function () { scan(document); }).observe(document.body, { childList: true, subtree: true });
})();
