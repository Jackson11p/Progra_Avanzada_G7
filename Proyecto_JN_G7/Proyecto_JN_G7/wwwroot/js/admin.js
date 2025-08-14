(() => {
    'use strict';

    const sidebar = document.getElementById('adminSidebar');
    const content = document.getElementById('adminContent');
    if (!sidebar || !content) return;

    function setActive(el) {
        [...sidebar.querySelectorAll('.list-group-item')].forEach(a => a.classList.remove('active'));
        el.classList.add('active');
    }

    async function loadPartial(name) {
        // UI: loading
        content.innerHTML = `
      <div class="card-body">
        <div class="d-flex align-items-center">
          <div class="spinner-border me-2" role="status" aria-hidden="true"></div>
          <strong>Cargando ${name}…</strong>
        </div>
      </div>`;

        try {
            const resp = await fetch(`/Admin/${name}`, { method: 'GET', headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
            const html = await resp.text();
            content.innerHTML = html;
        } catch (err) {
            console.error(err);
            content.innerHTML = `
        <div class="card-body">
          <div class="alert alert-danger mb-0">
            No se pudo cargar el módulo <strong>${name}</strong>. Intenta más tarde.
          </div>
        </div>`;
        }
    }

    sidebar.addEventListener('click', (e) => {
        const a = e.target.closest('[data-partial]');
        if (!a) return;
        e.preventDefault();
        const name = a.getAttribute('data-partial');
        setActive(a);
        loadPartial(name);
    });
})();
