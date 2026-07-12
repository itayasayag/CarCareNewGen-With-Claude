/* ============================================================
   CarCare — "Install app" button (Chrome/Edge; beforeinstallprompt)
   ------------------------------------------------------------
   Include on any page that has a <button id="installAppBtn">.
   The button starts hidden (class="is-hidden") and is revealed only
   when the browser fires beforeinstallprompt on THAT page load.

   Note: the captured event cannot survive a page navigation (each
   page load is a fresh JS context — the browser discards it), so
   every page that wants the button listens for the event itself.
   That's fine here: HomePage.html is only reachable when logged in
   (menu.js's site-wide guard), so this never gets exposed to a
   logged-out visitor the way the earlier drawer-based version did.

   No equivalent event exists on Safari/iOS — there the button simply
   never appears, and Share > Add to Home Screen remains the way in.
   ============================================================ */
(function () {
    var btn = document.getElementById('installAppBtn');
    if (!btn) return;

    var deferredInstallPrompt = null;

    // Already installed/running standalone? Never show the button.
    try {
        if (window.matchMedia('(display-mode: standalone)').matches || window.navigator.standalone) {
            return;
        }
    } catch (e) {}

    window.addEventListener('beforeinstallprompt', function (e) {
        e.preventDefault();
        deferredInstallPrompt = e;
        btn.classList.remove('is-hidden');
    });

    window.addEventListener('appinstalled', function () {
        deferredInstallPrompt = null;
        btn.classList.add('is-hidden');
    });

    btn.addEventListener('click', function () {
        if (!deferredInstallPrompt) return;
        deferredInstallPrompt.prompt();
        deferredInstallPrompt.userChoice.then(function () {
            deferredInstallPrompt = null;
            btn.classList.add('is-hidden');
        });
    });
})();
