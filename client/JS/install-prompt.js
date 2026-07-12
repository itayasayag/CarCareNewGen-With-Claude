/* ============================================================
   CarCare — "Install app" affordances
   ------------------------------------------------------------
   Two elements, both optional per page:
     #installAppBtn  — real one-tap install button (Chrome/Edge only)
     #installInfoBtn — subtle ⓘ button shown ONLY on iOS, where no
                       install API exists; tapping it explains the
                       manual Safari "Add to Home Screen" steps.

   Why the split: on iOS every browser is forced onto Apple's WebKit,
   which has never implemented beforeinstallprompt — so a real install
   button is impossible there. The ⓘ hint is the best achievable UX.

   If the app is already installed (standalone), neither shows.
   ============================================================ */
(function () {
    var installBtn = document.getElementById('installAppBtn');
    var infoBtn = document.getElementById('installInfoBtn');

    // Already installed / running standalone? Show nothing.
    var isStandalone = false;
    try {
        isStandalone = window.matchMedia('(display-mode: standalone)').matches
            || window.navigator.standalone === true;
    } catch (e) {}
    if (isStandalone) return;

    // Detect iOS (iPhone/iPad, incl. iPadOS reporting as Mac with touch).
    var ua = navigator.userAgent || '';
    var isIOS = /iPad|iPhone|iPod/.test(ua)
        || (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);

    // ---- iOS: show the subtle ⓘ, wire it to the manual instructions ----
    if (isIOS && infoBtn) {
        infoBtn.classList.remove('is-hidden');
        infoBtn.addEventListener('click', function () {
            if (window.Swal) {
                Swal.fire({
                    title: 'התקנת האפליקציה',
                    html: '<div style="text-align:right;direction:rtl;line-height:1.9;font-size:15px;">' +
                          'כדי להוסיף את CarCare למסך הבית ולפתוח אותה כמו אפליקציה:' +
                          '<br>1. פִּתחו את האתר ב־<b>Safari</b>' +
                          '<br>2. הקישו על כפתור <b>שיתוף</b> (הריבוע עם החץ ↑)' +
                          '<br>3. גללו ובחרו <b>הוסף למסך הבית</b>' +
                          '<br>4. הקישו <b>הוסף</b>' +
                          '</div>',
                    confirmButtonText: 'הבנתי',
                    confirmButtonColor: '#1E5A82'
                });
            }
        });
        return; // iOS gets the ⓘ only, never the real button
    }

    // ---- Chrome/Edge (Android + desktop): the real install button ----
    if (!installBtn) return;

    var deferredInstallPrompt = null;

    window.addEventListener('beforeinstallprompt', function (e) {
        e.preventDefault();
        deferredInstallPrompt = e;
        installBtn.classList.remove('is-hidden');
    });

    window.addEventListener('appinstalled', function () {
        deferredInstallPrompt = null;
        installBtn.classList.add('is-hidden');
    });

    installBtn.addEventListener('click', function () {
        if (!deferredInstallPrompt) return;
        deferredInstallPrompt.prompt();
        deferredInstallPrompt.userChoice.then(function () {
            deferredInstallPrompt = null;
            installBtn.classList.add('is-hidden');
        });
    });
})();
