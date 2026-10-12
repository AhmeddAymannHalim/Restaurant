(function () {
    var body = document.body;
    var KEY = "dashSideShut";
    var phone = window.matchMedia("(max-width: 860px)");

    function syncAria() {
        var open = phone.matches ? body.classList.contains("side-open") : !body.classList.contains("side-shut");
        document.querySelectorAll("[data-side-toggle]").forEach(function (button) { button.setAttribute("aria-expanded", open); });
    }

    document.querySelectorAll("[data-side-toggle]").forEach(function (button) {
        button.addEventListener("click", function () {
            if (phone.matches) {
                body.classList.toggle("side-open");
            } else {
                body.classList.toggle("side-shut");
                try { localStorage.setItem(KEY, body.classList.contains("side-shut") ? "1" : "0"); } catch (e) { }
            }
            syncAria();
        });
    });

    document.querySelectorAll("[data-side-close]").forEach(function (el) {
        el.addEventListener("click", function () { body.classList.remove("side-open"); syncAria(); });
    });

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") { body.classList.remove("side-open"); syncAria(); }
    });
    phone.addEventListener("change", function () { body.classList.remove("side-open"); syncAria(); });
    syncAria();

    var current = document.querySelector(".side-scroll .is-active");
    if (current) { current.scrollIntoView({ block: "center" }); }

    function setOpen(group, open) {
        group.classList.toggle("is-open", open);
        var button = group.querySelector(".side-parent");
        if (button) { button.setAttribute("aria-expanded", open); }
    }

    document.querySelectorAll(".side-parent").forEach(function (button) {
        button.addEventListener("click", function () {
            var group = button.closest(".side-group");
            if (body.classList.contains("side-shut") && !phone.matches) {
                body.classList.remove("side-shut");
                try { localStorage.setItem(KEY, "0"); } catch (e) { }
                syncAria();
                setOpen(group, true);
                return;
            }
            setOpen(group, !group.classList.contains("is-open"));
        });
    });

    var filter = document.getElementById("sideFilter");
    if (filter) {
        filter.addEventListener("input", function () {
            var term = filter.value.trim().toLowerCase();
            document.querySelectorAll(".side-group").forEach(function (group) {
                if (group.dataset.wasOpen === undefined) { group.dataset.wasOpen = group.classList.contains("is-open"); }

                var shown = 0;
                group.querySelectorAll(".dash-side-link").forEach(function (link) {
                    var match = !term || link.textContent.toLowerCase().indexOf(term) > -1;
                    link.hidden = !match;
                    if (match) { shown++; }
                });

                group.hidden = shown === 0;
                setOpen(group, term ? shown > 0 : group.dataset.wasOpen === "true");
                if (!term) { delete group.dataset.wasOpen; }
            });
        });
    }

    document.addEventListener("pointermove", function (e) {
        var el = e.target.closest && e.target.closest(".tile, .now-card, .ad-card");
        if (!el) { return; }
        var box = el.getBoundingClientRect();
        el.style.setProperty("--mx", (e.clientX - box.left) + "px");
        el.style.setProperty("--my", (e.clientY - box.top) + "px");
    });
})();
