
let port = "8000";

// ── API base URL resolution ───────────────────────────────────────────────────
// Priority order:
//   1. PRODUCTION_API (set in config.js) — use when deployed to Railway/cloud
//   2. GitHub Codespaces auto-detection  — works automatically in Codespaces
//   3. Local fallback                    — http://127.0.0.1:8000 (start.bat)
//
// To switch to production: open client/JS/config.js and set PRODUCTION_API.
// ─────────────────────────────────────────────────────────────────────────────
(function () {
    // 1. Explicit production URL (config.js)
    if (typeof PRODUCTION_API !== "undefined" && PRODUCTION_API.trim() !== "") {
        server = PRODUCTION_API.trim().replace(/\/$/, "");
        return;
    }

    var hostname = window.location.hostname;

    // 2. GitHub Codespaces forwarded port
    var codespacesMatch = hostname.match(/^(.+)-(\d+)\.(.+\.github\.dev)$/);
    if (codespacesMatch) {
        var codespaceBase = codespacesMatch[1];
        var domainSuffix  = codespacesMatch[3];
        server = "https://" + codespaceBase + "-" + port + "." + domainSuffix;
        return;
    }

    // 3. Local development
    server = `http://127.0.0.1:${port}`;
})();

function ajaxCall(method, api, data, successCB, errorCB) {
    $.ajax({
        type: method,
        url: api,
        data: data,
        cache: false,
        contentType: "application/json",
        dataType: "json",
        success: successCB,
        error: errorCB
    });
}
