(() => {
    'use strict';

    if (window.__pacientesBound) return;
    window.__pacientesBound = true;

    const root = document.getElementById('adminContent');
    if (!root) return;

    const API = window.ApiBaseUrl;

    const parseJson = (t) => { try { return JSON.parse(t); } catch { return null; } };
    const unwrap = (json) => {
        if (Array.isArray(json)) return json;
        if (json && typeof json === 'object') {
            if (Array.isArray(json.contenido)) return json.contenido;
            if (json.contenido) return json.contenido;
        }
        return json ?? [];
    };

    const get = (sel) => root.querySelector(sel);
    const val = (sel) => (get(sel)?.value ?? '').trim();
    const set = (sel, v) => { const el = get(sel); if (el) el.value = v ?? ''; };
    const modal = (id) => {
        const el = document.getElementById(id);
        return el ? bootstrap.Modal.getOrCreateInstance(el) : null;
    };

    // ENDPOINTS
    const URL_LIST = `${API}api/Paciente`;                  // GET lista admin
    const URL_GET = (id) => `${API}api/Paciente/${id}`;    // GET detalle
    const URL_CREATE = `${API}api/Paciente`;                  // POST crear
    const URL_UPDATE = (id) => `${API}api/Paciente/${id}`;    // PUT actualizar
    const URL_TOGGLE = (id) => `${API}api/Paciente/${id}/estado/toggle`; // PUT

    async function cargarPacientes() {
        const tbody = get('#p-rows');
        if (!tbody) return;
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted py-4">Cargando...</td></tr>`;

        try {
            const r = await fetch(URL_LIST, { cache: 'no-store' });
            const text = await r.text();

            if (!r.ok) {
                console.error('Pacientes LIST', r.status, text);
                tbody.innerHTML = `<tr><td colspan="8" class="text-danger text-center py-4">No se pudo cargar pacientes.</td></tr>`;
                return;
            }

            const data = unwrap(parseJson(text) ?? []) || [];
            if (!Array.isArray(data) || data.length === 0) {
                tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted py-4">Sin datos.</td></tr>`;
                return;
            }

            const rows = data.map(p => {
                let fecha = '';
                if (p.fechaNacimiento) {
                    const s = String(p.fechaNacimiento);
                    fecha = s.length >= 10 ? s.slice(0, 10) : s;
                }
                const genero = p.genero ?? '';
                const active =
                    p.activo === true || p.activo === 1 ||
                    (typeof p.activo === 'string' && p.activo.toLowerCase() === 'true');

                return `
          <tr>
            <td>${p.cedula ?? ''}</td>
            <td>${p.nombreCompleto ?? ''}</td>
            <td>${p.correoElectronico ?? ''}</td>
            <td>${p.telefono ?? ''}</td>
            <td>${fecha}</td>
            <td>${genero}</td>
            <td>${active ? 'Sí' : 'No'}</td>
            <td>
              <div class="btn-group btn-group-sm">
                <button class="btn btn-outline-secondary" data-action="p-edit" data-id="${p.pacienteID}">Editar</button>
                <button class="btn btn-outline-warning" data-action="p-toggle" data-id="${p.pacienteID}">
                  ${active ? 'Desactivar' : 'Activar'}
                </button>
              </div>
            </td>
          </tr>
        `;
            }).join('');

            tbody.innerHTML = rows;
        } catch (err) {
            console.error('Pacientes exception', err);
            tbody.innerHTML = `<tr><td colspan="8" class="text-danger text-center py-4">Error cargando pacientes.</td></tr>`;
        }
    }

    function abrirNuevo() {
        set('#pId', '');
        set('#pActivo', 'true');
        set('#pCedula', '');
        set('#pNombre', '');
        set('#pCorreo', '');
        set('#pTelefono', '');
        set('#pFechaNac', '');
        set('#pGenero', '');
        set('#pDireccion', '');
        get('#pModalTitle').textContent = 'Nuevo paciente';
        modal('pModal')?.show();
    }

    async function abrirEditar(id) {
        try {
            const r = await fetch(URL_GET(id));
            const text = await r.text();
            if (!r.ok) throw new Error(text || r.status);
            const env = parseJson(text);
            const p = (env && env.contenido) ? env.contenido : env;

            set('#pId', String(p.pacienteID ?? ''));
            set('#pActivo', String(!!p.activo));
            set('#pCedula', p.cedula ?? '');
            set('#pNombre', p.nombreCompleto ?? '');
            set('#pCorreo', p.correoElectronico ?? '');
            set('#pTelefono', p.telefono ?? '');
            set('#pFechaNac', p.fechaNacimiento ? String(p.fechaNacimiento).substring(0, 10) : '');
            set('#pGenero', p.genero ?? '');
            set('#pDireccion', p.direccion ?? '');

            get('#pModalTitle').textContent = 'Editar paciente';
            modal('pModal')?.show();
        } catch (err) {
            console.error('Obtener paciente', err);
            alert('No se pudo abrir el paciente.');
        }
    }

    let saving = false;
    async function guardar(e) {
        e.preventDefault();
        if (saving) return;
        saving = true;

        const btn = get('#pForm .btn[type="submit"]');
        btn?.setAttribute('disabled', 'disabled');

        const id = val('#pId'); // hidden
        const body = {
            cedula: val('#pCedula'),
            nombreCompleto: val('#pNombre'),
            correoElectronico: val('#pCorreo'),
            telefono: val('#pTelefono') || null,
            fechaNacimiento: val('#pFechaNac') || null, // "YYYY-MM-DD"
            genero: val('#pGenero') || null,
            direccion: val('#pDireccion') || null
        };

        if (!body.cedula || !body.nombreCompleto || !body.correoElectronico) {
            toastMsg('Cédula, nombre y correo son obligatorios.', false);
            btn?.removeAttribute('disabled');
            saving = false;
            return;
        }

        try {
            const url = id ? URL_UPDATE(id) : URL_CREATE;
            const method = id ? 'PUT' : 'POST';

            const resp = await fetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });

            if (resp.ok) {
                modal('pModal')?.hide();
                toastMsg(id ? 'Paciente actualizado.' : 'Paciente creado.', true);
                setTimeout(cargarPacientes, 0);
            } else {
                const raw = await resp.text();
                let msg = 'No se pudo guardar el paciente.';
                if ((resp.headers.get('Content-Type') || '').includes('application/json')) {
                    const env = parseJson(raw);
                    msg = env?.mensaje || env?.Mensaje || msg;
                } else if (raw) {
                    msg = raw;
                }
                toastMsg(msg, false);
            }
        } catch (err) {
            console.error('Guardar paciente ex', err);
            toastMsg('Error de red al guardar paciente.', false);
        } finally {
            btn?.removeAttribute('disabled');
            saving = false;
        }
    }

    async function toggle(id) {
        const ok = await confirmNice({
            title: "Cambiar estado",
            message: "¿Desea activar/desactivar este paciente?",
            okText: "Sí, actualizar",
            okClass: "btn-warning"
        });
        if (!ok) return;

        try {
            const r = await fetch(URL_TOGGLE(id), { method: 'PUT' });
            const text = await r.text();
            if (!r.ok) {
                console.error('Toggle paciente', r.status, text);
                toastMsg('No se pudo actualizar el estado.', false);
                return;
            }
            toastMsg('Estado actualizado.', true);
            setTimeout(cargarPacientes, 0);
        } catch (err) {
            console.error('Toggle ex', err);
            toastMsg('Error de red.', false);
        }
    }

    let eventsBound = false;
    const bindIfNeeded = () => {
        if (eventsBound) return;
        const marker = root.querySelector('[data-partial-name="Pacientes"]');
        if (!marker) return;

        cargarPacientes();

        get('#p-btn-reload')?.addEventListener('click', cargarPacientes);
        get('#p-btn-new')?.addEventListener('click', abrirNuevo);

        root.addEventListener('click', (e) => {
            const bEdit = e.target.closest('[data-action="p-edit"]');
            if (bEdit) abrirEditar(bEdit.dataset.id);

            const bTog = e.target.closest('[data-action="p-toggle"]');
            if (bTog) toggle(bTog.dataset.id);
        });

        root.addEventListener('submit', (e) => {
            if (e.target.id === 'pForm') guardar(e);
        });

        eventsBound = true;
    };

    const obs = new MutationObserver(bindIfNeeded);
    obs.observe(root, { childList: true, subtree: true });

    bindIfNeeded();

})();
