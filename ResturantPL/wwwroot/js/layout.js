(function () {
    document.addEventListener("pointermove", function (e) {
        var el = e.target.closest && e.target.closest(".tile, .now-card, .ad-card");
        if (!el) { return; }
        var box = el.getBoundingClientRect();
        el.style.setProperty("--mx", (e.clientX - box.left) + "px");
        el.style.setProperty("--my", (e.clientY - box.top) + "px");
    });
})();

(function () {
    function text(node) {
        return (node.textContent || "").replace(/\s+/g, " ").trim();
    }

    function visible(node) {
        return !node.hidden && getComputedStyle(node).display !== "none";
    }

    function build(toolbar) {
        var table = document.getElementById(toolbar.dataset.tableId);
        if (!table) { return null; }

        var headCells = Array.from(table.querySelectorAll("thead tr:first-child th"));
        var kept = headCells.map(function (cell, index) { return { cell: cell, index: index }; })
            .filter(function (item) { return item.cell.dataset.exportIgnore !== "true" && visible(item.cell); });

        var rows = Array.from(table.querySelectorAll("tbody tr")).map(function (row) {
            var cells = Array.from(row.children);
            if (cells.length !== headCells.length || cells.some(function (cell) { return cell.colSpan > 1; })) { return null; }
            return kept.map(function (item) { return text(cells[item.index]); });
        }).filter(Boolean);

        return {
            Title: toolbar.dataset.title,
            FileName: toolbar.dataset.title,
            IsRightToLeft: toolbar.dataset.rtl === "1",
            Headings: kept.map(function (item) { return text(item.cell); }),
            Rows: rows
        };
    }

    function download(blob, name) {
        var link = document.createElement("a");
        link.href = URL.createObjectURL(blob);
        link.download = name;
        document.body.appendChild(link);
        link.click();
        link.remove();
        setTimeout(function () { URL.revokeObjectURL(link.href); }, 4000);
    }

    document.addEventListener("click", function (e) {
        var button = e.target.closest && e.target.closest("[data-table-export] [data-export-format]");
        if (!button) { return; }

        var toolbar = button.closest("[data-table-export]");
        var payload = build(toolbar);
        if (!payload || payload.Rows.length === 0) {
            toastr.warning(toolbar.dataset.empty);
            return;
        }

        var format = button.dataset.exportFormat;
        fetch(format === "excel" ? toolbar.dataset.excelUrl : toolbar.dataset.pdfUrl, {
            method: "POST",
            credentials: "same-origin",
            headers: { "Content-Type": "application/json", "RequestVerificationToken": window.csrfToken },
            body: JSON.stringify(payload)
        }).then(function (response) {
            if (!response.ok) { throw new Error(response.status); }
            var disposition = response.headers.get("Content-Disposition") || "";
            var match = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(disposition);
            var name = match ? decodeURIComponent(match[1]) : payload.Title + (format === "excel" ? ".xlsx" : ".pdf");
            return response.blob().then(function (blob) { download(blob, name); });
        }).catch(function () {
            toastr.error(toolbar.dataset.failed);
        });
    });
})();

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
        var pane = invalid[i].closest(".tab-pane");
        var tab = pane && document.getElementById("tab-" + pane.id.replace("pane-", ""));
        if (tab && window.bootstrap) {
            window.bootstrap.Tab.getOrCreateInstance(tab).show();
            break;
        }
    }
})();
