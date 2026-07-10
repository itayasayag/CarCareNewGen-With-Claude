/* ============================================================
   CarCare — shared side menu + dark mode
   Self-contained: builds the hamburger button, slide-in drawer,
   navigation links, and a persistent dark-mode toggle.
   Include on every page with: <script src="../JS/menu.js"></script>
   ============================================================ */
(function () {
    // ---- Apply saved dark-mode preference ASAP ----
    try {
        if (localStorage.getItem('cc-theme') === 'dark') {
            document.documentElement.classList.add('cc-dark');
        }
    } catch (e) {}

    // Navigation items: label, target page, icon image.
    // Icons are the app's own artwork (not emoji) so the menu matches the
    // rest of the UI. They're rendered at emoji size (20px) via .cc-ico.
    var LINKS = [
        { label: 'מסך הבית',     href: 'HomePage.html',        ico: 'images/menu-home.png' },
        { label: 'ספר הרכב שלי', href: 'CarBookPage.html',     ico: 'images/book.png' },
        { label: 'תזכורת חדשה',  href: 'AlertsPage.html',      ico: 'images/BellBlue.png' },
        { label: 'הוסף טיפול',   href: 'AddCarCarePage.html',  ico: 'images/wrench (2).png' },
        { label: 'הוסף רכב',     href: 'addCarPage.html',      ico: 'images/car_icon.png' },
        { label: 'מצא מוסך',     href: 'FindGaragePage.html',  ico: 'images/location.png' },
        { label: 'מקרא נורות',   href: 'LightIndicator.html',  ico: 'images/oil-indicator.png' },
        { label: 'העברת רכב',    href: 'SendCar.html',         ico: 'images/send.png' },
        { label: 'התנתק',        href: 'SignUpPage.html',      ico: 'images/menu-logout.png', logout: true }
    ];

    function build() {
        if (document.querySelector('.cc-menu-btn')) return; // avoid duplicates

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
            html += '<a href="' + l.href + '"' + (l.logout ? ' data-logout="1"' : '') +
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
