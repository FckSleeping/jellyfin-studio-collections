/**
 * StudioCollections - Plugin Jellyfin
 * Affiche une section par studio avec logos et effet hover backdrop/intro.
 * Injecté automatiquement dans l'interface Jellyfin.
 */
(function () {
    'use strict';

    // ── Configuration par défaut (surchargée par /StudioCollections/config) ──
    let CONFIG = {
        SectionTitle: 'Par Studio',
        EnableHoverEffect: true,
        HoverAnimationDuration: 400,
        InjectOnHomepage: true
    };

    // ── Styles CSS injectés une seule fois ──────────────────────────────────
    const CSS = `
    #sc-studios-section {
        padding: 1.5em 0 2em;
        margin-top: 1em;
    }
    #sc-studios-section .sc-section-header {
        font-size: 1.2em;
        font-weight: 600;
        color: #fff;
        margin: 0 2em 0.75em;
        letter-spacing: 0.03em;
    }
    #sc-studios-section .sc-section-header span {
        font-size: 0.75em;
        color: rgba(255,255,255,0.5);
        margin-left: 0.5em;
        cursor: pointer;
        text-decoration: underline dotted;
    }
    .sc-studios-row {
        display: flex;
        flex-direction: row;
        gap: 1.2em;
        overflow-x: auto;
        padding: 0.5em 2em 1em;
        scrollbar-width: thin;
        scrollbar-color: rgba(255,255,255,0.2) transparent;
    }
    .sc-studios-row::-webkit-scrollbar {
        height: 4px;
    }
    .sc-studios-row::-webkit-scrollbar-thumb {
        background: rgba(255,255,255,0.2);
        border-radius: 2px;
    }

    /* ── Carte studio ──────────────────────────────────────────────────────── */
    .sc-studio-card {
        flex: 0 0 200px;
        height: 120px;
        position: relative;
        border-radius: 8px;
        overflow: hidden;
        cursor: pointer;
        background: #1a1a2e;
        border: 1px solid rgba(255,255,255,0.08);
        transition: transform 0.2s ease, box-shadow 0.2s ease;
    }
    .sc-studio-card:hover {
        transform: scale(1.04) translateY(-2px);
        box-shadow: 0 8px 32px rgba(0,0,0,0.5);
        z-index: 10;
    }

    /* ── Backdrop affiché au hover ─────────────────────────────────────────── */
    .sc-studio-backdrop {
        position: absolute;
        inset: 0;
        background-size: cover;
        background-position: center;
        opacity: 0;
        transition: opacity var(--sc-hover-duration, 400ms) ease;
    }
    .sc-studio-backdrop::after {
        content: '';
        position: absolute;
        inset: 0;
        background: linear-gradient(
            to bottom,
            rgba(0,0,0,0.1) 0%,
            rgba(0,0,0,0.6) 70%,
            rgba(0,0,0,0.85) 100%
        );
    }
    .sc-studio-card:hover .sc-studio-backdrop {
        opacity: 1;
    }

    /* ── Logo du studio ─────────────────────────────────────────────────────── */
    .sc-studio-logo-wrap {
        position: absolute;
        inset: 0;
        display: flex;
        align-items: center;
        justify-content: center;
        padding: 1em;
        z-index: 2;
        transition: opacity var(--sc-hover-duration, 400ms) ease;
    }
    .sc-studio-logo {
        max-width: 85%;
        max-height: 65%;
        object-fit: contain;
        filter: brightness(0) invert(1) drop-shadow(0 2px 4px rgba(0,0,0,0.5));
        transition: filter var(--sc-hover-duration, 400ms) ease;
    }
    .sc-studio-card:hover .sc-studio-logo {
        filter: none drop-shadow(0 2px 8px rgba(0,0,0,0.8));
    }

    /* Fallback texte si pas de logo */
    .sc-studio-name-fallback {
        color: #fff;
        font-size: 0.9em;
        font-weight: 600;
        text-align: center;
        text-shadow: 0 1px 3px rgba(0,0,0,0.7);
        line-height: 1.3;
    }

    /* ── Infos au hover ─────────────────────────────────────────────────────── */
    .sc-studio-hover-info {
        position: absolute;
        bottom: 0;
        left: 0;
        right: 0;
        padding: 0.5em 0.75em;
        z-index: 3;
        opacity: 0;
        transform: translateY(4px);
        transition: opacity var(--sc-hover-duration, 400ms) ease,
                    transform var(--sc-hover-duration, 400ms) ease;
    }
    .sc-studio-card:hover .sc-studio-hover-info {
        opacity: 1;
        transform: translateY(0);
    }
    .sc-hover-count {
        font-size: 0.7em;
        color: rgba(255,255,255,0.75);
    }
    .sc-hover-name {
        font-size: 0.85em;
        font-weight: 700;
        color: #fff;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    /* ── Mini-bande d'items récents au hover ────────────────────────────────── */
    .sc-studio-items-preview {
        position: absolute;
        bottom: 38px;
        left: 0;
        right: 0;
        display: flex;
        gap: 4px;
        padding: 0 6px;
        z-index: 3;
        opacity: 0;
        transform: translateY(6px);
        transition: opacity var(--sc-hover-duration, 400ms) ease 100ms,
                    transform var(--sc-hover-duration, 400ms) ease 100ms;
    }
    .sc-studio-card:hover .sc-studio-items-preview {
        opacity: 1;
        transform: translateY(0);
    }
    .sc-item-thumb {
        flex: 0 0 36px;
        height: 52px;
        border-radius: 3px;
        background-size: cover;
        background-position: center;
        background-color: #333;
        box-shadow: 0 2px 6px rgba(0,0,0,0.5);
    }

    /* ── Loader ─────────────────────────────────────────────────────────────── */
    .sc-loading {
        padding: 1em 2em;
        color: rgba(255,255,255,0.5);
        font-size: 0.9em;
    }

    /* ── Responsive ─────────────────────────────────────────────────────────── */
    @media (max-width: 600px) {
        .sc-studio-card { flex: 0 0 160px; height: 95px; }
        .sc-studios-row { gap: 0.75em; padding: 0.4em 1em 0.8em; }
    }
    `;

    // ── Utilitaires ──────────────────────────────────────────────────────────
    function injectCSS() {
        if (document.getElementById('sc-studios-styles')) return;
        const style = document.createElement('style');
        style.id = 'sc-studios-styles';
        style.textContent = CSS;
        document.head.appendChild(style);
    }

    function getApiKey() {
        try {
            const creds = ApiClient._currentUser || window.ApiClient?._currentUser;
            return ApiClient.accessToken() || '';
        } catch { return ''; }
    }

    function buildImageUrl(path) {
        if (!path) return null;
        const base = ApiClient.serverAddress();
        const token = getApiKey();
        return `${base}${path}${token ? '?api_key=' + token : ''}`;
    }

    async function fetchConfig() {
        try {
            const base = ApiClient.serverAddress();
            const token = getApiKey();
            const res = await fetch(`${base}/StudioCollections/config${token ? '?api_key=' + token : ''}`);
            if (res.ok) Object.assign(CONFIG, await res.json());
        } catch (e) { /* garder les valeurs par défaut */ }
    }

    async function fetchStudios() {
        const base = ApiClient.serverAddress();
        const token = getApiKey();
        const res = await fetch(`${base}/StudioCollections/studios${token ? '?api_key=' + token : ''}`);
        if (!res.ok) throw new Error('HTTP ' + res.status);
        return res.json();
    }

    // ── Construction du DOM ──────────────────────────────────────────────────
    function buildCard(studio) {
        const dur = CONFIG.HoverAnimationDuration;
        const card = document.createElement('div');
        card.className = 'sc-studio-card';
        card.title = studio.Name;
        card.style.setProperty('--sc-hover-duration', dur + 'ms');

        // Backdrop
        const backdrop = document.createElement('div');
        backdrop.className = 'sc-studio-backdrop';
        if (studio.BackdropUrl && CONFIG.EnableHoverEffect) {
            const imgUrl = buildImageUrl(studio.BackdropUrl);
            if (imgUrl) backdrop.style.backgroundImage = `url('${imgUrl}')`;
        }
        card.appendChild(backdrop);

        // Logo / nom
        const logoWrap = document.createElement('div');
        logoWrap.className = 'sc-studio-logo-wrap';

        if (studio.LogoUrl) {
            const logoUrl = buildImageUrl(studio.LogoUrl);
            const img = document.createElement('img');
            img.className = 'sc-studio-logo';
            img.src = logoUrl || '';
            img.alt = studio.Name;
            img.onerror = function () {
                this.style.display = 'none';
                const fb = document.createElement('span');
                fb.className = 'sc-studio-name-fallback';
                fb.textContent = studio.Name;
                logoWrap.appendChild(fb);
            };
            logoWrap.appendChild(img);
        } else {
            const fallback = document.createElement('span');
            fallback.className = 'sc-studio-name-fallback';
            fallback.textContent = studio.Name;
            logoWrap.appendChild(fallback);
        }
        card.appendChild(logoWrap);

        // Mini-préview des items récents (hover)
        if (CONFIG.EnableHoverEffect && studio.RecentItems && studio.RecentItems.length > 0) {
            const preview = document.createElement('div');
            preview.className = 'sc-studio-items-preview';
            const maxThumbs = Math.min(studio.RecentItems.length, 5);
            for (let i = 0; i < maxThumbs; i++) {
                const item = studio.RecentItems[i];
                const thumb = document.createElement('div');
                thumb.className = 'sc-item-thumb';
                const imgSrc = buildImageUrl(item.PrimaryImageUrl || item.BackdropUrl);
                if (imgSrc) thumb.style.backgroundImage = `url('${imgSrc}')`;
                preview.appendChild(thumb);
            }
            card.appendChild(preview);
        }

        // Infos hover (nom + nb items)
        if (CONFIG.EnableHoverEffect) {
            const info = document.createElement('div');
            info.className = 'sc-studio-hover-info';
            info.innerHTML = `
                <div class="sc-hover-name">${studio.Name}</div>
                <div class="sc-hover-count">${studio.ItemCount} titre${studio.ItemCount > 1 ? 's' : ''}</div>
            `;
            card.appendChild(info);
        }

        // Click → naviguer vers la page du studio
        card.addEventListener('click', function () {
            try {
                window.location.hash = studio.BrowseUrl.replace('/web/', '');
            } catch (e) {
                window.location.href = studio.BrowseUrl;
            }
        });

        return card;
    }

    function buildSection(studios) {
        const section = document.createElement('div');
        section.id = 'sc-studios-section';

        // En-tête
        const header = document.createElement('div');
        header.className = 'sc-section-header';
        header.innerHTML = `${CONFIG.SectionTitle} <span>Voir tout →</span>`;
        header.querySelector('span').addEventListener('click', function (e) {
            e.stopPropagation();
            window.location.hash = '/list.html?genrefilter=false';
        });
        section.appendChild(header);

        // Ligne horizontale scrollable
        const row = document.createElement('div');
        row.className = 'sc-studios-row';

        if (!studios || studios.length === 0) {
            const empty = document.createElement('div');
            empty.className = 'sc-loading';
            empty.textContent = 'Aucun studio trouvé.';
            row.appendChild(empty);
        } else {
            studios.forEach(studio => row.appendChild(buildCard(studio)));
        }

        section.appendChild(row);
        return section;
    }

    // ── Injection dans la page d'accueil ─────────────────────────────────────
    async function inject() {
        // Supprimer la section précédente si elle existe (refresh)
        const old = document.getElementById('sc-studios-section');
        if (old) old.remove();

        // Insérer un placeholder pendant le chargement
        const placeholder = document.createElement('div');
        placeholder.id = 'sc-studios-section';
        placeholder.innerHTML = '<div class="sc-loading">Chargement des studios…</div>';

        // Trouver un bon point d'insertion dans l'accueil Jellyfin
        const targets = [
            '.homePage .sections',
            '.homePage section',
            '.sections',
            '#indexPage',
            '.mainAnimatedPage'
        ];
        let anchor = null;
        for (const sel of targets) {
            anchor = document.querySelector(sel);
            if (anchor) break;
        }
        if (!anchor) return;

        // Insérer après le premier enfant (après le héros / slider)
        const firstChild = anchor.children[1] || anchor.firstElementChild;
        if (firstChild) {
            anchor.insertBefore(placeholder, firstChild.nextSibling);
        } else {
            anchor.prepend(placeholder);
        }

        try {
            const studios = await fetchStudios();
            const section = buildSection(studios);
            placeholder.replaceWith(section);
        } catch (err) {
            placeholder.innerHTML = `<div class="sc-loading" style="color:#f66">Erreur StudioCollections: ${err.message}</div>`;
            console.error('[StudioCollections]', err);
        }
    }

    // ── Détection de navigation SPA ──────────────────────────────────────────
    function onRouteChange() {
        if (!CONFIG.InjectOnHomepage) return;
        const hash = window.location.hash || '';
        const isHome = hash === '' || hash === '#/' || hash.includes('index.html') ||
                       hash === '#' || hash.endsWith('/');
        if (isHome) {
            // Petit délai pour laisser Jellyfin rendre le DOM
            setTimeout(inject, 600);
        }
    }

    // ── Initialisation ────────────────────────────────────────────────────────
    async function init() {
        injectCSS();
        await fetchConfig();

        // Écouter les changements de route SPA
        window.addEventListener('hashchange', onRouteChange);

        // Injection initiale
        onRouteChange();

        // Ré-injection si Jellyfin émet des événements de navigation
        document.addEventListener('viewshow', function (e) {
            if (!CONFIG.InjectOnHomepage) return;
            const view = e.target;
            if (view && (view.id === 'indexPage' || view.classList.contains('homePage'))) {
                setTimeout(inject, 400);
            }
        });
    }

    // ── Attendre que ApiClient soit disponible ───────────────────────────────
    function waitForApiClient(cb, tries = 0) {
        if (typeof ApiClient !== 'undefined' && ApiClient.serverAddress) {
            cb();
        } else if (tries < 30) {
            setTimeout(() => waitForApiClient(cb, tries + 1), 300);
        }
    }

    waitForApiClient(init);

})();
