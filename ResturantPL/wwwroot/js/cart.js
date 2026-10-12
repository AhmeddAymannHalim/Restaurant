(function () {
    var KEY = "basket";

    function read() {
        try {
            var value = JSON.parse(localStorage.getItem(KEY)) || {};
            return Array.isArray(value) ? {} : value;
        } catch (e) {
            return {};
        }
    }

    function count() {
        var basket = read();
        return Object.keys(basket).reduce(function (sum, id) {
            return sum + (parseInt(basket[id], 10) || 0);
        }, 0);
    }

    function paint(animate) {
        var total = count();
        document.querySelectorAll("[data-cart-count]").forEach(function (badge) {
            badge.textContent = total > 99 ? "99+" : total;
            badge.hidden = total === 0;
            if (animate) {
                badge.classList.remove("is-pop");
                void badge.offsetWidth;
                badge.classList.add("is-pop");
            }
        });
    }

    function write(basket, animate) {
        localStorage.setItem(KEY, JSON.stringify(basket));
        paint(animate);
        document.dispatchEvent(new CustomEvent("cart:changed"));
    }

    function add(id, quantity) {
        var basket = read();
        basket[id] = (parseInt(basket[id], 10) || 0) + (quantity || 1);
        write(basket, true);
    }

    function set(id, quantity) {
        var basket = read();
        if (quantity < 1) {
            delete basket[id];
        } else {
            basket[id] = quantity;
        }
        write(basket, false);
    }

    function clear() {
        write({}, false);
    }

    function notify(message) {
        if (window.toastr && message) {
            toastr.success(message);
        }
    }

    window.siteCart = { read: read, count: count, add: add, set: set, clear: clear, notify: notify };

    document.addEventListener("click", function (event) {
        var button = event.target.closest("[data-cart-add]");
        if (!button) {
            return;
        }

        event.preventDefault();
        add(button.getAttribute("data-cart-add"), 1);

        var cart = document.querySelector("[data-cart]");
        notify(cart ? cart.getAttribute("data-added") : "");
    });

    window.addEventListener("storage", function (event) {
        if (event.key === KEY) {
            paint(false);
        }
    });

    var drawer = document.getElementById("cartDrawer");
    var scrim = document.querySelector(".cart-scrim");
    var menu = null;
    var pricing = { taxPercent: 0, service: 0, zones: [] };
    var loading = false;
    var lastFocus = null;

    function sizeName(size) {
        var names = {};
        try {
            names = JSON.parse(drawer.dataset.sizes || "{}");
        } catch (e) {
            names = {};
        }

        return names[size] || size;
    }

    function itemName(item) {
        var arabic = drawer.dataset.arabic === "1";
        var name = arabic ? (item.arName || item.enName) : (item.enName || item.arName);
        return item.size ? name + " (" + sizeName(item.size) + ")" : name;
    }

    function line(item, quantity) {
        var li = document.createElement("li");
        li.className = "cart-line";
        li.dataset.id = item.id;

        var media = document.createElement("div");
        media.className = "cart-media";
        if (item.imageUrl) {
            var img = document.createElement("img");
            img.alt = "";
            img.src = item.imageUrl;
            media.appendChild(img);
        } else {
            media.innerHTML = '<i class="bi bi-egg-fried" aria-hidden="true"></i>';
        }

        var info = document.createElement("div");
        info.className = "cart-info";
        var name = document.createElement("strong");
        name.textContent = itemName(item);
        var price = document.createElement("span");
        price.textContent = item.price.toFixed(2) + " " + drawer.dataset.currency;
        info.appendChild(name);
        info.appendChild(price);

        var stepper = document.createElement("div");
        stepper.className = "cart-step";
        stepper.innerHTML = '<button type="button" data-step="-1" aria-label="-">-</button><span>' + quantity + '</span><button type="button" data-step="1" aria-label="+">+</button>';

        var remove = document.createElement("button");
        remove.type = "button";
        remove.className = "cart-remove";
        remove.dataset.remove = item.id;
        remove.title = drawer.dataset.remove;
        remove.setAttribute("aria-label", drawer.dataset.remove);
        remove.innerHTML = '<i class="bi bi-trash3" aria-hidden="true"></i>';

        li.appendChild(media);
        li.appendChild(info);
        li.appendChild(stepper);
        li.appendChild(remove);
        return li;
    }

    function render() {
        if (!drawer || !menu) {
            return;
        }

        var basket = read();
        var ids = Object.keys(basket).filter(function (id) { return menu[id]; });
        var list = document.getElementById("cartDrawerLines");
        list.innerHTML = "";

        var total = 0;
        ids.forEach(function (id) {
            var quantity = parseInt(basket[id], 10) || 1;
            total += menu[id].price * quantity;
            list.appendChild(line(menu[id], quantity));
        });

        var tax = Math.round(total * pricing.taxPercent) / 100;
        var service = ids.length > 0 ? pricing.service : 0;

        document.getElementById("cartSub").textContent = total.toFixed(2);
        document.getElementById("cartTax").textContent = tax.toFixed(2);
        document.getElementById("cartTaxLabel").textContent = drawer.dataset.tax + (pricing.taxPercent > 0 ? " (" + pricing.taxPercent + "%)" : "");
        document.getElementById("cartTaxRow").hidden = tax <= 0;
        document.getElementById("cartService").textContent = service.toFixed(2);
        document.getElementById("cartServiceRow").hidden = service <= 0;
        document.getElementById("cartDrawerTotal").textContent = (total + tax + service).toFixed(2);
        renderDelivery();
        document.getElementById("cartDrawerEmpty").hidden = ids.length > 0;
        document.getElementById("cartDrawerFoot").hidden = ids.length === 0;
    }

    function renderDelivery() {
        var note = document.getElementById("cartDeliveryNote");
        var fees = pricing.zones.map(function (zone) { return zone.fee; });
        if (fees.length === 0) {
            note.hidden = true;
            return;
        }

        var min = Math.min.apply(null, fees);
        var max = Math.max.apply(null, fees);
        var text = min === max ? drawer.dataset.deliveryOne : drawer.dataset.deliveryRange;
        note.textContent = text.replace("{0}", min.toFixed(2)).replace("{1}", max.toFixed(2));
        note.hidden = false;
    }

    function load() {
        if (menu || loading) {
            return;
        }

        loading = true;
        Promise.all([
            fetch("/Home/MenuItems", { credentials: "same-origin" }).then(function (response) { return response.json(); }),
            fetch("/Home/Pricing", { credentials: "same-origin" }).then(function (response) { return response.json(); })
        ])
            .then(function (results) {
                menu = {};
                results[0].forEach(function (item) { menu[item.id] = item; });
                pricing = results[1];
                render();
            })
            .catch(function () {
                if (window.toastr) {
                    toastr.error(drawer.dataset.items);
                }
            })
            .then(function () { loading = false; });
    }

    function openDrawer() {
        lastFocus = document.activeElement;
        drawer.hidden = false;
        scrim.hidden = false;
        document.body.classList.add("cart-open");
        requestAnimationFrame(function () {
            drawer.classList.add("is-open");
            scrim.classList.add("is-open");
        });
        load();
        render();
        drawer.querySelector(".cart-close").focus();
    }

    function closeDrawer() {
        if (drawer.hidden) {
            return;
        }

        drawer.classList.remove("is-open");
        scrim.classList.remove("is-open");
        document.body.classList.remove("cart-open");
        setTimeout(function () {
            drawer.hidden = true;
            scrim.hidden = true;
        }, 260);
        if (lastFocus) {
            lastFocus.focus();
        }
    }

    if (drawer) {
        document.addEventListener("click", function (event) {
            if (event.target.closest("[data-cart-open]")) {
                openDrawer();
                return;
            }

            if (event.target.closest("[data-cart-close]")) {
                closeDrawer();
                return;
            }

            var step = event.target.closest("[data-step]");
            var remove = event.target.closest("[data-remove]");
            var row = event.target.closest(".cart-line");
            if (row && step) {
                var id = row.dataset.id;
                set(id, (parseInt(read()[id], 10) || 0) + parseInt(step.dataset.step, 10));
            } else if (row && remove) {
                set(row.dataset.id, 0);
            }
        });

        document.addEventListener("keydown", function (event) {
            if (event.key === "Escape") {
                closeDrawer();
            }
        });

        document.addEventListener("cart:changed", render);
    }

    paint(false);
})();
