(function () {
    var body = document.body;
    var side = document.getElementById("dashSide");
    if (!side) { return; }

    var scroller = side.querySelector(".side-scroll");
    var groups = side.querySelectorAll(".side-group");
    var phone = window.matchMedia("(max-width: 860px)");
    var SHUT_KEY = "dashSideShut";
    var OPEN_KEY = "dashSideOpen";
    var SCROLL_KEY = "dashSideScroll";

    function groupId(group) {
        return group.querySelector(".side-children").id;
    }

    function setOpen(group, open) {
        group.classList.toggle("is-open", open);
        var button = group.querySelector(".side-parent");
        if (button) { button.setAttribute("aria-expanded", open); }
    }

    function saveOpenGroups() {
        var ids = [];
        groups.forEach(function (group) {
            if (group.classList.contains("is-open")) { ids.push(groupId(group)); }
        });
        try { sessionStorage.setItem(OPEN_KEY, JSON.stringify(ids)); } catch (e) { }
    }

    // ---- restore: open groups, then the current link, then the scroll (no animation while restoring)
    body.classList.add("side-init");

    var openIds = [];
    try { openIds = JSON.parse(sessionStorage.getItem(OPEN_KEY) || "[]"); } catch (e) { }
    groups.forEach(function (group) {
        setOpen(group, openIds.indexOf(groupId(group)) > -1);
    });

    // the link of the current page: same controller, and the link query (Type=SAL, report=Profit) must match
    var here = new URL(location.href);
    var herePath = here.pathname.toLowerCase().replace(/\/+$/, "") || "/dashboard";
    var hereController = herePath.split("/")[1];
    var exact = null;
    var sameController = [];

    side.querySelectorAll(".dash-side-link[href]").forEach(function (link) {
        var url = new URL(link.href);
        var path = url.pathname.toLowerCase().replace(/\/+$/, "") || "/dashboard";
        if (path.split("/")[1] !== hereController) { return; }

        var queryMatches = true;
        url.searchParams.forEach(function (value, key) {
            if (here.searchParams.get(key) !== value) { queryMatches = false; }
        });
        if (!queryMatches) { return; }

        if (path === herePath || path === herePath + "/index") { exact = exact || link; }
        sameController.push(link);
    });

    // a sub page (Customer/Upsert) belongs to the only link of its controller
    var current = exact || (sameController.length === 1 ? sameController[0] : null);
    if (current) {
        current.classList.add("is-active");
        current.setAttribute("aria-current", "page");
        var parent = current.closest(".side-group");
        if (parent) { setOpen(parent, true); saveOpenGroups(); }
    }

    var savedScroll = 0;
    try { savedScroll = parseInt(sessionStorage.getItem(SCROLL_KEY) || "0", 10) || 0; } catch (e) { }
    scroller.scrollTop = savedScroll;

    // the current link is out of sight (opened from a dashboard card, for example): bring it into view
    if (current) {
        var link = current.getBoundingClientRect();
        var area = scroller.getBoundingClientRect();
        if (link.top < area.top || link.bottom > area.bottom) { current.scrollIntoView({ block: "center" }); }
    }

    requestAnimationFrame(function () { body.classList.remove("side-init"); });

    scroller.addEventListener("scroll", function () {
        try { sessionStorage.setItem(SCROLL_KEY, scroller.scrollTop); } catch (e) { }
    });

    // ---- open and close a group by hand
    side.querySelectorAll(".side-parent").forEach(function (button) {
        button.addEventListener("click", function () {
            var group = button.closest(".side-group");
            if (body.classList.contains("side-shut") && !phone.matches) {
                body.classList.remove("side-shut");
                try { localStorage.setItem(SHUT_KEY, "0"); } catch (e) { }
                setOpen(group, true);
            } else {
                setOpen(group, !group.classList.contains("is-open"));
            }
            saveOpenGroups();
        });
    });

    // ---- the whole sidebar: shrink on a computer, slide in on a phone
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
                try { localStorage.setItem(SHUT_KEY, body.classList.contains("side-shut") ? "1" : "0"); } catch (e) { }
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

    // ---- filter the menu while typing
    var filter = document.getElementById("sideFilter");
    if (filter) {
        filter.addEventListener("input", function () {
            var term = filter.value.trim().toLowerCase();
            groups.forEach(function (group) {
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
})();
