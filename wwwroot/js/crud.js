// Simple client-side filter for the CRUD list table.
// This is an in-memory filter for phase 1 — server-side search/sort/paging comes in phase 2.
document.addEventListener("DOMContentLoaded", function () {
    var searchBox = document.getElementById("crud-search-box");
    var table = document.getElementById("crud-table");
    if (!searchBox || !table) return;

    searchBox.addEventListener("input", function () {
        var term = searchBox.value.trim().toLowerCase();
        var rows = table.querySelectorAll("tbody tr");

        rows.forEach(function (row) {
            var text = row.textContent.toLowerCase();
            row.style.display = text.indexOf(term) === -1 ? "none" : "";
        });
    });
});
