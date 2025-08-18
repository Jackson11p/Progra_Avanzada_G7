(() => {
    'use strict';

    const root = document.getElementById('adminContent');
    if (!root) return;

    const API = window.ApiBaseUrl;

    const unwrap = (json) => {
        if (Array.isArray(json)) return json;
        if (json && typeof json === 'object' && Array.isArray(json.contenido)) return json.contenido;
        return [];
    };
    const parseJson = (t) => { try { return JSON.parse(t); } catch { return null; } };
    const get = sel => root.querySelector(sel);
    const val = sel => (get(sel)?.value ?? '').trim();
    const set = (sel, v) => { const el = get(sel); if (el) el.value = v ?? ''; };
    const modal = (id) => {
        const el = document.getElementById(id);
        return el ? bootstrap.Modal.getOrCreateInstance(el) : null;
    };

    async function cargarRoles(selected = '') {
        const sel = get('#uRol');
        if (!sel) return;
        sel.innerHTML = `<option value="">Seleccione un rol…</option>`;
        try {
            const r = await fetch(`${API}api/Usuarios/Roles`, { cache: 'no-store' });
            const text = await r.text();
            if (!r.ok) throw new Error(text || r.status);
            const data = unwrap(parseJson(text) ?? []);
            data.forEach(x => {
                const opt = new Option(x.nombreRol ?? x.NombreRol ?? '', String(x.rolID ?? x.RolID ?? ''));
                sel.add(opt);
            });
            if (selected) sel.value = String(selected);
            if (!sel.value) sel.value = '1'; // por defecto "Usuario"
        } catch (err) {
            console.error('Roles error', err);
            alert('No se pudieron cargar los roles.');
        }
    }

    async function cargarUsuarios() {
        const tbody = get('#u-rows');
        if (!tbody) return;
        tbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted py-4">Cargando...</td></tr>`;
        try {
            const resp = await fetch(`${API}api/Usuarios`, { cache: 'no-store' });
            const raw = await resp.text();
            if (!resp.ok) {
                console.error('Usuarios GET', resp.status, raw);
                tbody.innerHTML = `<tr><td colspan="6" class="text-danger text-center py-4">No se pudo cargar usuarios.</td></tr>`;
                return;
            }
            const data = unwrap(parseJson(raw) ?? []);
            if (!data.length) {
                tbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted py-4">Sin datos.</td></tr>`;
                return;
            }
            const rows = data.map(u => `
        <tr>
          <td>${u.cedula ?? ''}</td>
          <td>${u.nombreCompleto ?? ''}</td>
          <td>${u.correoElectronico ?? ''}</td>
          <td>${u.nombreRol ?? ''}</td>
          <td>${u.activo ? 'Sí' : 'No'}</td>
          <td>
            <div class="btn-group btn-group-sm">
              <button class="btn btn-outline-secondary" data-action="u-edit" data-id="${u.usuarioID}">Editar</button>
              <button class="btn btn-outline-warning" data-action="u-toggle" data-id="${u.usuarioID}">
                ${u.activo ? 'Desactivar' : 'Activar'}
              </button>
            </div>
          </td>
        </tr>
      `).join('');
            tbody.innerHTML = rows;
        } catch (err) {
            console.error('Usuarios exception', err);
            tbody.innerHTML = `<tr><td colspan="6" class="text-danger text-center py-4">Error cargando usuarios.</td></tr>`;
        }
    }

    async function abrirNuevo() {
        set('#uId', '');
        set('#uCedula', '');
        set('#uNombre', '');
        set('#uCorreo', '');
        set('#uPass', '');
        set('#uActivo', 'true');
        get('#uPassBox')?.classList.remove('d-none');
        get('#uModalTitle').textContent = 'Nuevo usuario';
        await cargarRoles('1'); // default Usuario
        modal('uModal')?.show();
    }

    async function abrirEditar(id) {
        try {
            const r = await fetch(`${API}api/Usuarios/${id}`);
            const text = await r.text();
            if (!r.ok) throw new Error(text || r.status);
            const env = parseJson(text);
            const u = (env && env.contenido) ? env.contenido : env; // soporta sobre o plano

            set('#uId', String(u.usuarioID ?? ''));
            set('#uCedula', u.cedula ?? '');
            set('#uNombre', u.nombreCompleto ?? '');
            set('#uCorreo', u.correoElectronico ?? '');
            set('#uPass', '');
            set('#uActivo', String(!!u.activo));
            get('#uPassBox')?.classList.add('d-none'); // no se cambia contraseña aquí
            get('#uModalTitle').textContent = 'Editar usuario';

            await cargarRoles(String(u.rolID ?? ''));
            modal('uModal')?.show();
        } catch (err) {
            console.error('Obtener usuario', err);
            alert('No se pudo abrir el usuario.');
        }
    }

    async function guardar(e) {
        e.preventDefault();

        const id = val('#uId');

        const bodyCreate = {
            cedula: val('#uCedula'),
            nombreCompleto: val('#uNombre'),
            correoElectronico: val('#uCorreo'),
            contrasena: val('#uPass'),
            rolID: parseInt(val('#uRol') || '1', 10)
        };

        const bodyUpdate = {
            cedula: val('#uCedula'),
            nombreCompleto: val('#uNombre'),
            correoElectronico: val('#uCorreo'),
            rolID: parseInt(val('#uRol') || '1', 10),
            
            activo: (get('#uActivo')?.value === 'true')
        };

        // Validaciones mínimas
        if (!bodyCreate.cedula || !bodyCreate.nombreCompleto || !bodyCreate.correoElectronico) {
            toastMsg('Cédula, nombre y correo son obligatorios.', false);
            return;
        }
        if (!id && !bodyCreate.contrasena) {
            toastMsg('La contraseña es obligatoria para crear.', false);
            return;
        }

        try {
            const url = id ? `${API}api/Usuarios/${id}` : `${API}api/Usuarios`;
            const method = id ? 'PUT' : 'POST';
            const payload = id ? bodyUpdate : bodyCreate;

            const r = await fetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            const text = await r.text();
            if (!r.ok) {
                console.error('Guardar usuario', r.status, text);
                toastMsg('No se pudo guardar el usuario.', false);
                return;
            }

            modal('uModal')?.hide();
            toastMsg(id ? 'Usuario actualizado.' : 'Usuario creado.');
            await cargarUsuarios();
        } catch (err) {
            console.error('Guardar usuario ex', err);
            toastMsg('Error de red al guardar usuario.', false);
        }
    }


    async function toggle(id) {
        const ok = await confirmNice({
            title: "Cambiar estado",
            message: "¿Desea activar/desactivar este usuario?",
            okText: "Sí, actualizar",
            okClass: "btn-warning"
        });
        if (!ok) return;

        try {
            const r = await fetch(`${API}api/Usuarios/${id}/estado/toggle`, { method: 'PUT' });
            const text = await r.text();
            if (!r.ok) {
                console.error('Toggle usuario', r.status, text);
                toastMsg('No se pudo actualizar el estado.', false);
                return;
            }
            toastMsg('Estado actualizado.');
            await cargarUsuarios();
        } catch (err) {
            console.error('Toggle ex', err);
            toastMsg('Error de red.', false);
        }
    }


    const obs = new MutationObserver(() => {
        const marker = root.querySelector('[data-partial-name="Usuarios"]');
        if (marker) {
            cargarUsuarios();
            get('#u-btn-reload')?.addEventListener('click', cargarUsuarios);
            get('#u-btn-new')?.addEventListener('click', abrirNuevo);
            root.addEventListener('click', (e) => {
                const bEdit = e.target.closest('[data-action="u-edit"]');
                if (bEdit) abrirEditar(bEdit.dataset.id);
                const bTog = e.target.closest('[data-action="u-toggle"]');
                if (bTog) toggle(bTog.dataset.id);
            });
            root.addEventListener('submit', (e) => {
                if (e.target.id === 'uForm') guardar(e);
            });
            obs.disconnect();
        }
    });
    obs.observe(root, { childList: true, subtree: true });

    // Si ya estaba cargada
    if (root.querySelector('[data-partial-name="Usuarios"]')) {
        cargarUsuarios();
        get('#u-btn-reload')?.addEventListener('click', cargarUsuarios);
        get('#u-btn-new')?.addEventListener('click', abrirNuevo);
        root.addEventListener('click', (e) => {
            const bEdit = e.target.closest('[data-action="u-edit"]');
            if (bEdit) abrirEditar(bEdit.dataset.id);
            const bTog = e.target.closest('[data-action="u-toggle"]');
            if (bTog) toggle(bTog.dataset.id);
        });
        root.addEventListener('submit', (e) => {
            if (e.target.id === 'uForm') guardar(e);
        });
    }
})();

async function confirmNice({
    title = "Confirmar",
    message = "¿Seguro?",
    okText = "Sí, continuar",
    okClass = "btn-danger"
} = {}) {
    return new Promise(resolve => {
        const el = document.getElementById('confirmModal');
        const m = bootstrap.Modal.getOrCreateInstance(el);

        document.getElementById('cfTitle').textContent = title;
        document.getElementById('cfMessage').textContent = message;
        document.getElementById('cfOkText').textContent = okText;

        const okBtn = document.getElementById('cfOk');
        const cancelBtn = document.getElementById('cfCancel');
        okBtn.className = `btn ${okClass}`;

        const handleOk = () => { cleanup(); m.hide(); resolve(true); };
        const handleCancel = () => { cleanup(); resolve(false); };
        const handleHidden = () => { cleanup(); resolve(false); };

        function cleanup() {
            okBtn.removeEventListener('click', handleOk);
            cancelBtn.removeEventListener('click', handleCancel);
            el.removeEventListener('hidden.bs.modal', handleHidden);
        }

        okBtn.addEventListener('click', handleOk, { once: true });
        cancelBtn.addEventListener('click', handleCancel, { once: true });
        // Si se cierra con X o clic fuera, se considera "cancelar"
        el.addEventListener('hidden.bs.modal', handleHidden, { once: true });

        m.show();
    });
}


// Toast de éxito/error
function toastMsg(message, success = true) {
    const t = document.getElementById('liveToast');
    t.classList.toggle('text-bg-success', success);
    t.classList.toggle('text-bg-danger', !success);
    document.getElementById('toastBody').textContent = message;
    bootstrap.Toast.getOrCreateInstance(t, { delay: 2500 }).show();
}
