(function () {
    "use strict";

    document.documentElement.classList.add("js");

    var reveals = document.querySelectorAll(".ws-reveal");
    if ("IntersectionObserver" in window) {
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (e) {
                if (e.isIntersecting) {
                    e.target.classList.add("is-in");
                    io.unobserve(e.target);
                }
            });
        }, { rootMargin: "0px 0px -8% 0px" });
        reveals.forEach(function (el) { io.observe(el); });
    } else {
        reveals.forEach(function (el) { el.classList.add("is-in"); });
    }

    var root = document.querySelector("[data-ws-menu]");
    if (!root) { return; }

    var list = root.querySelector("[data-ws-dishes]");
    var tabs = Array.prototype.slice.call(root.querySelectorAll(".ws-tab"));
    var pill = root.querySelector(".ws-tab-pill");
    var arabic = root.dataset.ar === "1";
    var cache = {};
    var current = null;
    var ticket = 0;
    var sizeNames = {};
    try {
        sizeNames = JSON.parse(root.dataset.sizes || "{}");
    } catch (e) {
        sizeNames = {};
    }

    function esc(text) {
        var d = document.createElement("div");
        d.textContent = text == null ? "" : text;
        return d.innerHTML;
    }

    function movePill(tab) {
        if (!pill || !tab) { return; }
        pill.style.setProperty("--x", tab.offsetLeft + "px");
        pill.style.setProperty("--w", tab.offsetWidth + "px");
    }

    function card(item, index) {
        var name = arabic ? (item.arName || item.enName) : (item.enName || item.arName);
        var price = Number(item.price).toLocaleString(undefined, { minimumFractionDigits: 0, maximumFractionDigits: 2 });
        var image = item.imageUrl
            ? '<img src="' + esc(item.imageUrl) + '" alt="' + esc(name) + '" loading="lazy" />'
            : "";
        var size = item.size ? '<span class="ws-dish-size">' + esc(sizeNames[item.size] || item.size) + "</span>" : "";
        var text = arabic ? (item.arDescription || item.enDescription) : (item.enDescription || item.arDescription);
        var desc = text ? '<p class="ws-dish-desc">' + esc(text) + "</p>" : "";
        var ingredients = arabic ? (item.arIngredients || item.enIngredients) : (item.enIngredients || item.arIngredients);
        var made = ingredients ? '<p class="ws-dish-desc ws-dish-made">' + esc(root.dataset.madeOf) + " " + esc(ingredients) + "</p>" : "";

        return '<article class="ws-dish" style="--i:' + index + '">' +
            '<div class="ws-dish-img">' + image + "</div>" +
            '<div class="ws-dish-body">' +
            '<div class="ws-dish-head"><h3 class="ws-dish-name">' + esc(name) + '</h3>' +
            '<span class="ws-dish-price">' + price + " " + esc(root.dataset.currency) + "</span></div>" +
            size + desc + made +
            '<button type="button" class="ws-dish-add" data-cart-add="' + esc(String(item.id)) + '">' +
            '<i class="bi bi-bag-plus" aria-hidden="true"></i> ' + esc(root.dataset.add) + "</button>" +
            "</div></article>";
    }

    function show(items) {
        list.classList.remove("is-leaving");
        if (!items.length) {
            list.innerHTML = '<p class="ws-note">' + esc(root.dataset.empty) + "</p>";
            return;
        }
        list.innerHTML = items.map(card).join("");
    }

    function skeleton() {
        list.innerHTML = '<div class="ws-skel"></div><div class="ws-skel"></div><div class="ws-skel"></div>';
    }

    function load(sectionId) {
        var key = sectionId || "all";
        var mine = ++ticket;
        current = key;

        var render = function (items) {
            if (mine !== ticket) { return; }
            show(items);
        };

        list.classList.add("is-leaving");
        var wait = new Promise(function (resolve) { setTimeout(resolve, 170); });

        if (cache[key]) {
            wait.then(function () { render(cache[key]); });
            return;
        }

        var url = root.dataset.url + (sectionId ? "?sectionId=" + encodeURIComponent(sectionId) : "");
        var request = fetch(url, { headers: { "Accept": "application/json" }, credentials: "same-origin" })
            .then(function (r) { if (!r.ok) { throw new Error(r.status); } return r.json(); });

        wait.then(function () { if (mine === ticket && !cache[key]) { skeleton(); list.classList.remove("is-leaving"); } });

        request.then(function (items) {
            cache[key] = items;
            return wait.then(function () { render(items); });
        }).catch(function () {
            if (mine !== ticket) { return; }
            list.classList.remove("is-leaving");
            list.innerHTML = '<p class="ws-note">' + esc(root.dataset.failed) + "</p>";
        });
    }

    function select(tab) {
        tabs.forEach(function (t) {
            var on = t === tab;
            t.classList.toggle("is-on", on);
            t.setAttribute("aria-selected", on ? "true" : "false");
        });
        movePill(tab);
        if (tab.scrollIntoView) { tab.scrollIntoView({ inline: "center", block: "nearest", behavior: "smooth" }); }
        load(tab.dataset.section);
    }

    tabs.forEach(function (tab) {
        tab.addEventListener("click", function () {
            if (tab.classList.contains("is-on") && current !== null) { return; }
            select(tab);
        });
    });

    window.addEventListener("resize", function () {
        movePill(root.querySelector(".ws-tab.is-on"));
    });

    var first = root.querySelector(".ws-tab.is-on");
    if (first) {
        (document.fonts && document.fonts.ready ? document.fonts.ready : Promise.resolve()).then(function () { movePill(first); });
        movePill(first);
    }
    load(first ? first.dataset.section : "");
})();

(function () {
    if (window.GLightbox) {
        GLightbox({ selector: ".glightbox" });
    }
})();
