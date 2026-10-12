(function () {
    var REGION = ".bo-page";
    var HEADERS = { "X-Ajax-Form": "1" };

    function region() {
        return document.querySelector(REGION);
    }

    function runToasts(doc) {
        doc.querySelectorAll("script:not([src])").forEach(function (script) {
            if (/toastr\s*[.\[]/.test(script.textContent)) {
                try { (new Function(script.textContent))(); } catch (e) { }
            }
        });
    }

    function parseValidation(root) {
        if (window.jQuery && jQuery.validator && jQuery.validator.unobtrusive) {
            jQuery(root).find("form").each(function () {
                jQuery(this).removeData("validator").removeData("unobtrusiveValidation");
                jQuery.validator.unobtrusive.parse(this);
            });
        }
    }

    function setBusy(form, busy) {
        form.querySelectorAll("[type=submit]").forEach(function (button) { button.disabled = busy; });
    }

    function load(url, options, push) {
        var current = region();

        return fetch(url, Object.assign({ credentials: "same-origin", headers: HEADERS }, options)).then(function (response) {
            var type = response.headers.get("content-type") || "";

            if (type.indexOf("application/json") > -1) {
                return response.json().then(function (data) {
                    if (data.redirect) {
                        window.appLoader.start();
                        location.href = data.redirect;
                    }
                });
            }

            return response.text().then(function (html) {
                var doc = new DOMParser().parseFromString(html, "text/html");
                var fresh = doc.querySelector(REGION);

                if (!response.ok || !fresh || !current) {
                    document.open();
                    document.write(html);
                    document.close();
                    return;
                }

                current.innerHTML = fresh.innerHTML;
                document.title = doc.title;
                parseValidation(current);
                runToasts(doc);

                if (push) {
                    history.pushState({ ajax: true }, "", response.url);
                }
            });
        });
    }

    function reload() {
        return load(location.href, { method: "GET" }, false);
    }

    document.addEventListener("submit", function (e) {
        var form = e.target;
        if (e.defaultPrevented || !form.closest(REGION) || form.hasAttribute("data-no-ajax")) { return; }

        e.preventDefault();
        var data = new FormData(form, e.submitter);
        var method = (form.getAttribute("method") || "get").toUpperCase();
        var action = form.getAttribute("action") || location.href;

        setBusy(form, true);
        var request = method === "GET"
            ? load(action + (action.indexOf("?") > -1 ? "&" : "?") + new URLSearchParams(data).toString(), { method: "GET" }, true)
            : load(action, { method: "POST", body: data }, false);

        request.catch(function () { form.submit(); }).then(function () { setBusy(form, false); });
    });

    document.addEventListener("click", function (e) {
        var link = e.target.closest && e.target.closest(REGION + " .pagination a[href]");
        if (!link || e.defaultPrevented || e.button !== 0 || e.ctrlKey || e.shiftKey || e.metaKey || link.closest(".disabled")) { return; }

        e.preventDefault();
        load(link.href, { method: "GET" }, true).catch(function () { location.href = link.href; });
    });

    history.replaceState({ ajax: true }, "");
    window.addEventListener("popstate", function (e) {
        if (e.state && e.state.ajax) { location.reload(); }
    });

    window.appAjax = { reload: reload };
})();
