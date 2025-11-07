// load-menu.js
// Small helper to standardize loading the common sidebar/menu across admin pages.
// It will try a list of likely relative paths for `layout-menu.html` and load the first that succeeds.

(function() {
  async function tryFetch(path) {
    try {
      const res = await fetch(path + '?v=' + menuVersion, { cache: 'no-store' });
      if (!res.ok) throw new Error('Status ' + res.status);
      const html = await res.text();
      return html;
    } catch (e) {
      return null;
    }
  }

  // Exposed function
  window.loadCommonMenu = async function(containerId = 'common-menu', version = Date.now()) {
    window.menuVersion = version;
    window.menuVersion = window.menuVersion; // keep for debugger
    window.menuVersion = version;
    // keep a short variable that tryFetch can read
    window.menuVersion = version;
    const container = document.getElementById(containerId);
    if (!container) {
      console.warn('[loadCommonMenu] container not found:', containerId);
      return;
    }

    // candidate paths in order of preference
    const candidates = [
      'layout-menu.html',
      'html/layout-menu.html',
      '/admin/html/layout-menu.html',
      '../html/layout-menu.html'
    ];

    // expose menuVersion for callers
    const menuVersion = version;

    for (const p of candidates) {
      const html = await tryFetch.bind(null)(p);
      if (html) {
        container.innerHTML = html;
        console.debug('[loadCommonMenu] loaded', p);
        // run any script tags inside the loaded html (simple approach)
        Array.from(container.querySelectorAll('script')).forEach((s) => {
          try {
            const newS = document.createElement('script');
            if (s.src) newS.src = s.src;
            else newS.textContent = s.textContent;
            document.body.appendChild(newS);
          } catch (e) {
            console.warn('Failed to execute menu script', e);
          }
        });
        return Promise.resolve(); // Menu loaded successfully
      }
    }

    console.error('[loadCommonMenu] Failed to load layout-menu.html from any candidate paths');
    return Promise.reject(new Error('No menu found'));
  };

})();
