
let port = "8000";

// Determine the API base URL dynamically so the same client code works
// both for local development (server on 127.0.0.1) and for GitHub
// Codespaces, where each forwarded port gets its own *-<port>.<...>.github.dev
// hostname instead of 127.0.0.1.
(function () {
    var hostname = window.location.hostname;
    // Codespaces forwarded URLs look like:
    //   <codespace-name>-<frontendPort>.app.github.dev
    // or the legacy <codespace-name>-<frontendPort>.preview.app.github.dev
    var codespacesMatch = hostname.match(/^(.+)-(\d+)\.(.+\.github\.dev)$/);
    if (codespacesMatch) {
        var codespaceBase = codespacesMatch[1];
        var domainSuffix = codespacesMatch[3];
        server = "https://" + codespaceBase + "-" + port + "." + domainSuffix;
    } else {
        // Local development (file://, localhost, 127.0.0.1) - unchanged behavior
        server = `http://127.0.0.1:${port}`;
    }
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
