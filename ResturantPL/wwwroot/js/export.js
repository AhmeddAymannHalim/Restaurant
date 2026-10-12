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
