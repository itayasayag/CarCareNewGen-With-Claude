/* ============================================================
   CarCare — shared side menu + dark mode
   Self-contained: builds the hamburger button, slide-in drawer,
   navigation links, and a persistent dark-mode toggle.
   Include on every page with: <script src="../JS/menu.js"></script>
   ============================================================ */
(function () {
    // ---- Site-wide login guard ────────────────────────────────────────────
    // menu.js loads on every page, so this is the single place that can
    // reliably protect ALL of them. Several pages (CarBookPage, MyCarsPage,
    // FindGaragePage, LightIndicator, SendCar) had no guard of their own —
    // a direct link let a logged-out visitor reach them. Public pages are
    // explicitly excluded below.
    var PUBLIC_PAGES = ['SignUpPage.html', 'HomePage - Guest.html'];
    var currentPage = decodeURIComponent(location.pathname.split('/').pop() || '');
    var isPublicPage = PUBLIC_PAGES.indexOf(currentPage) !== -1;

    // One local icon language for static pages and dynamically injected forms.
    var REFRESH_ICONS = {
        'back.png': 'back',
        'bellblue.png': 'bell',
        'book.png': 'book-open',
        'brakes.png': 'disc-3',
        'calendar.png': 'calendar-days',
        'car (3).png': 'car-front',
        'car_icon.png': 'car-front',
        'car-battery (1).png': 'battery-charging',
        'car-insurance.png': 'shield-check',
        'car-oilblue.png': 'droplets',
        'carinfo.png': 'info',
        'coins.png': 'coins',
        'deal.png': 'handshake',
        'edit.png': 'square-pen',
        'file (1).png': 'upload',
        'invoice1.png': 'receipt-text',
        'kilometer.png': 'gauge',
        'location.png': 'map-pin',
        'maintenance (3).png': 'garage',
        'menu-bell.png': 'bell',
        'menu-book.png': 'book-open',
        'menu-car.png': 'car-front',
        'menu-home.png': 'house',
        'menu-install.png': 'upload',
        'menu-lights.png': 'circle-alert',
        'menu-location.png': 'map-pin',
        'menu-logout.png': 'log-out',
        'menu-transfer.png': 'send',
        'menu-wrench.png': 'wrench',
        'money (2).png': 'coins',
        'notes (1).png': 'notebook-pen',
        'odometer-for-kilometers-and-speed-control (2).png': 'gauge',
        'oil-indicator.png': 'circle-alert',
        'picture-as-pdf.png': 'file-text',
        'send.png': 'send',
        'test.png': 'book-open',
        'testblue.png': 'clipboard-check',
        'tire-pressureblue.png': 'gauge',
        'tiresblue.png': 'circle-dot-dashed',
        'uploadcarpapers.png': 'upload',
        'uploadimage.png': 'upload',
        'uploadrecipt.png': 'receipt-text',
        'user.png': 'user-round',
        'wrench (2).png': 'wrench'
    };

    function refreshIcons(root) {
        var images = [];
        if (root && root.matches && root.matches('img[src]')) images.push(root);
        if (root && root.querySelectorAll) {
            images = images.concat(Array.prototype.slice.call(root.querySelectorAll('img[src]')));
        }
        images.forEach(function (img) {
            var cleanSrc = decodeURIComponent((img.getAttribute('src') || '').split('?')[0]).replace(/\\/g, '/');
            var fileName = cleanSrc.substring(cleanSrc.lastIndexOf('/') + 1).toLowerCase();
            var replacement = REFRESH_ICONS[fileName];
            if (replacement) img.setAttribute('src', 'images/ui/' + replacement + '.svg');
        });
    }

    function watchForInjectedIcons() {
        if (!document.body || document.documentElement.dataset.ccIconRefresh === '1') return;
        document.documentElement.dataset.ccIconRefresh = '1';
        refreshIcons(document);
        new MutationObserver(function (mutations) {
            mutations.forEach(function (mutation) {
                mutation.addedNodes.forEach(function (node) {
                    if (node.nodeType === 1) refreshIcons(node);
                });
            });
        }).observe(document.body, { childList: true, subtree: true });
    }

    if (!isPublicPage) {
        var loggedIn = null;
        try { loggedIn = localStorage.getItem('LogInUser'); } catch (e) {}
        if (!loggedIn) {
            // Redirect immediately — don't let protected markup render/flash.
            location.replace('SignUpPage.html');
            return; // stop the rest of menu.js on this page
        }
    }

    // ---- Apply saved dark-mode preference ASAP ----
    try {
        if (localStorage.getItem('cc-theme') === 'dark') {
            document.documentElement.classList.add('cc-dark');
        }
    } catch (e) {}

    // Navigation items: label, target page, icon image.
    // General navigation uses the local blue duotone family. Semantic artwork
    // such as red reminders, missing-receipt warnings and dashboard lights is
    // deliberately excluded so its real warning colour is never overwritten.
    var LINKS = [
        { label: 'מסך הבית',     href: 'HomePage.html',        ico: 'images/ui/house.svg' },
        { label: 'ספר הרכב שלי', href: 'CarBookPage.html',     ico: 'images/ui/book-open.svg' },
        { label: 'תזכורת חדשה',  href: 'AlertsPage.html',      ico: 'images/ui/bell.svg' },
        { label: 'הוסף טיפול',   href: 'AddCarCarePage.html',  ico: 'images/ui/wrench.svg' },
        { label: 'הוסף רכב',     href: 'addCarPage.html',      ico: 'images/ui/car-front.svg' },
        { label: 'מצא מוסך',     href: 'FindGaragePage.html',  ico: 'images/ui/map-pin.svg' },
        { label: 'מקרא נורות',   href: 'LightIndicator.html',  ico: 'images/ui/circle-alert.svg' },
        { label: 'עולם המכירה',  href: 'CarSell.html',         ico: 'images/ui/handshake.svg' },
        { label: 'התנתק',        href: 'SignUpPage.html',      ico: 'images/ui/log-out.svg', logout: true }
    ];

    function build() {
        if (document.querySelector('.cc-menu-btn')) return; // avoid duplicates
        watchForInjectedIcons();
        // No hamburger/drawer on the sign-up/login page itself — there's
        // nothing a logged-out visitor should be navigating to yet, and it
        // previously let people tap straight into protected pages.
        if (currentPage === 'SignUpPage.html') return;

        // Replace the +(plus) image glyph with a transparent pixel so the CSS
        // gradient circle + drawn "+" shows cleanly (the PNG art was off-ratio).
        document.querySelectorAll('img.plus').forEach(function (img) {
            img.src = 'data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==';
        });

        // Hamburger button
        var btn = document.createElement('button');
        btn.className = 'cc-menu-btn';
        btn.setAttribute('aria-label', 'תפריט');
        btn.innerHTML = '<span></span><span></span><span></span>';

        // Backdrop
        var backdrop = document.createElement('div');
        backdrop.className = 'cc-backdrop';

        // Drawer
        var drawer = document.createElement('nav');
        drawer.className = 'cc-drawer';
        drawer.setAttribute('aria-label', 'ניווט');

        var html = '<div class="cc-drawer-title">CarCare</div>';
        LINKS.forEach(function (l) {
            var isCurrent = currentPage.toLowerCase() === l.href.toLowerCase();
            html += '<a href="' + l.href + '"' + (l.logout ? ' data-logout="1"' : '') +
                    (isCurrent ? ' class="is-current" aria-current="page"' : '') +
                    '><img class="cc-ico" src="' + encodeURI(l.ico) + '" alt="" aria-hidden="true">' +
                    l.label + '</a>';
        });
        html += '<div class="cc-drawer-spacer"></div>';
        html += '<div class="cc-dm-row"><span>מצב כהה 🌙</span>' +
                '<div class="cc-dm-switch" role="switch" tabindex="0" aria-label="מצב כהה"></div></div>';
        drawer.innerHTML = html;

        document.body.appendChild(btn);
        document.body.appendChild(backdrop);
        document.body.appendChild(drawer);

        function open()  { document.documentElement.classList.add('cc-menu-open'); }
        function close() { document.documentElement.classList.remove('cc-menu-open'); }
        function toggle(){ document.documentElement.classList.toggle('cc-menu-open'); }

        btn.addEventListener('click', toggle);
        backdrop.addEventListener('click', close);
        document.addEventListener('keydown', function (e) { if (e.key === 'Escape') close(); });

        // Logout link clears storage
        drawer.querySelectorAll('a[data-logout]').forEach(function (a) {
            a.addEventListener('click', function () {
                try { localStorage.clear(); } catch (e) {}
            });
        });

        // Dark-mode switch
        var sw = drawer.querySelector('.cc-dm-switch');
        function applyTheme(dark) {
            document.documentElement.classList.toggle('cc-dark', dark);
            try { localStorage.setItem('cc-theme', dark ? 'dark' : 'light'); } catch (e) {}
        }
        sw.addEventListener('click', function () {
            applyTheme(!document.documentElement.classList.contains('cc-dark'));
        });
        sw.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                applyTheme(!document.documentElement.classList.contains('cc-dark'));
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', build);
    } else {
        build();
    }
})();
