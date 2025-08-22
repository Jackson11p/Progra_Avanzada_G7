// wwwroot/js/admin.historial.js
(() => {
    'use strict';

    if (window.__historialBound) return;
    window.__historialBound = true;

    const root = document.getElementById('adminContent');
    if (!root) return;

    const API = (window.ApiBaseUrl || '').trim();
    const BASE = API ? (API.endsWith('/') ? API : API + '/') : '/';

    // ---------- Selectores de la vista ----------
    const SEL_PAC = '#hmPaciente';   
    const TBODY = '#hm-rows';      
    const BTN_RELOAD = '#hm-btn-reload';

    // ---------- Helpers ----------
    const q = (s, ctx = root) => (ctx || document).querySelector(s);
    const set = (s, v) => { const el = q(s); if (el) el.value = v ?? ''; };
    const parseJson = (t) => { try { return JSON.parse(t); } catch { return null; } };

    function toastMsg(message, success = true) {
        const t = document.getElementById('liveToast');
        if (!t) { alert(message || (success ? 'Listo' : 'Ocurrió un error')); return; }
        t.classList.toggle('text-bg-success', success);
        t.classList.toggle('text-bg-danger', !success);
        document.getElementById('toastBody').textContent = message || (success ? 'Listo' : 'Ocurrió un error');
        bootstrap.Toast.getOrCreateInstance(t, { delay: 2500 }).show();
    }

    if (!window.confirmNice) {
        window.confirmNice = function ({ title = "Confirmar", message = "¿Seguro?", okText = "Sí, continuar", okClass = "btn-danger" } = {}) {
            return new Promise(resolve => {
                const el = document.getElementById('confirmModal');
                const m = bootstrap.Modal.getOrCreateInstance(el);
                root.querySelector('#cfTitle').textContent = title;
                root.querySelector('#cfMessage').textContent = message;
                root.querySelector('#cfOkText').textContent = okText;
                const okBtn = root.querySelector('#cfOk');
                okBtn.className = `btn ${okClass}`;
                const onOk = () => { cleanup(); m.hide(); resolve(true); };
                const onCancel = () => { cleanup(); resolve(false); };
                function cleanup() {
                    okBtn.removeEventListener('click', onOk);
                    el.removeEventListener('hidden.bs.modal', onCancel);
                }
                okBtn.addEventListener('click', onOk);
                el.addEventListener('hidden.bs.modal', onCancel, { once: true });
                m.show();
            });
        };
    }

    const modal = (id) => {
        const el = document.getElementById(id);
        return el ? bootstrap.Modal.getOrCreateInstance(el) : null;
    };

    // ---------- Endpoints ----------
    const URL_PACIENTES = `${BASE}api/Paciente/ListaSimple`;
    const URL_HISTORIAL = (pacienteId) => `${BASE}api/Cita/Historial/${pacienteId}`;
    const URL_ADJ_LIST = (citaId) => `${BASE}api/Cita/Adjuntos/PorCita/${citaId}`;
    const URL_ADJ_UPLOAD = `${BASE}api/Cita/Adjuntos`;
    const URL_ADJ_DELETE = (adjId) => `${BASE}api/Cita/Adjuntos/${adjId}`;
    const URL_ADJ_DOWNLOAD = (adjId) => `${BASE}api/Cita/Adjuntos/${adjId}/Descargar`;

    // ---------- Cargar pacientes (select) ----------
    async function cargarPacientesSelect(preserve = true) {
        const sel = q(SEL_PAC);
        if (!sel) return;

        const current = preserve ? (sel.value || '') : '';
        sel.innerHTML = `<option value="">Cargando pacientes…</option>`;

        try {
            const r = await fetch(URL_PACIENTES, { cache: 'no-store' });
            const raw = await r.text();
            if (!r.ok) {
                console.error('Pacientes LIST', r.status, raw);
                sel.innerHTML = `<option value="">No se pudo cargar pacientes</option>`;
                return;
            }

            const data = parseJson(raw) ?? [];
            if (!Array.isArray(data) || data.length === 0) {
                sel.innerHTML = `<option value="">Sin pacientes</option>`;
                return;
            }

            const opts = [`<option value="">Seleccione un paciente…</option>`];
            for (const p of data) {
                const id = p.pacienteID ?? p.PacienteID;
                const ced = p.cedula ?? p.Cedula ?? '';
                const nom = p.nombreCompleto ?? p.NombreCompleto ?? '';
                const mail = p.correoElectronico ?? p.CorreoElectronico ?? '';
                if (!id) continue;
                opts.push(`<option value="${id}">${nom} (${ced}) — ${mail}</option>`);
            }

            sel.innerHTML = opts.join('');
            if (preserve && current && [...sel.options].some(o => o.value === current)) {
                sel.value = current;
            } else {
                sel.value = '';
            }
        } catch (err) {
            console.error('Pacientes LIST ex', err);
            sel.innerHTML = `<option value="">Error cargando pacientes</option>`;
        }
    }

    // ---------- Cargar historial ----------
    async function cargarHistorial() {
        const sel = q(SEL_PAC);
        const tbody = q(TBODY);
        if (!sel || !tbody) return;

        const pid = parseInt((sel.value || '0'), 10);
        if (!pid) {
            tbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted py-4">Seleccione un paciente para ver su historial.</td></tr>`;
            return;
        }

        tbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted py-4">Cargando…</td></tr>`;

        try {
            const r = await fetch(URL_HISTORIAL(pid), { cache: 'no-store' });
            const raw = await r.text();
            if (!r.ok) {
                console.error('Historial GET', r.status, raw);
                tbody.innerHTML = `<tr><td colspan="6" class="text-center text-danger py-4">No se pudo cargar el historial.</td></tr>`;
                return;
            }

            const data = parseJson(raw) ?? [];
            if (!Array.isArray(data) || data.length === 0) {
                tbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted py-4">Sin registros.</td></tr>`;
                return;
            }

            const rows = data.map(x => {
                const citaId = x.citaID ?? x.CitaID;
                const fechaRaw = x.fechaHora ?? x.FechaHora;
                const fecha = fechaRaw ? String(fechaRaw).replace('T', ' ').substring(0, 16) : '';
                const estado = x.estado ?? x.Estado ?? '';
                const motivo = x.motivoConsulta ?? x.MotivoConsulta ?? x.motivo ?? x.Motivo ?? '—';
                const doctor = x.doctorNombre ?? x.DoctorNombre ?? `DoctorID: ${x.doctorID ?? x.DoctorID ?? ''}`;
                const cantAdj = x.cantAdjuntos ?? x.CantAdjuntos ?? 0;

                return `
          <tr data-cita-id="${citaId}">
            <td>${fecha}</td>
            <td>${doctor}</td>
            <td>${estado}</td>
            <td class="text-truncate" style="max-width:280px;">${motivo}</td>
            <td><span class="badge text-bg-secondary">${cantAdj}</span></td>
            <td>
              <div class="btn-group btn-group-sm">
                <button class="btn btn-outline-primary" data-action="h-files" data-id="${citaId}">
                  <i class="bi bi-paperclip"></i> Adjuntos
                </button>
              </div>
            </td>
          </tr>`;
            }).join('');

            tbody.innerHTML = rows;

        } catch (err) {
            console.error('Historial exception', err);
            tbody.innerHTML = `<tr><td colspan="6" class="text-center text-danger py-4">Error cargando historial.</td></tr>`;
        }
    }

    // ---------- Adjuntos: abrir modal & listar ----------
    async function cargarAdjuntosEnModal(citaId) {
        const list = q('#hListBody', document);
        set('#hUpCitaId', String(citaId));
        if (list) list.innerHTML = `<div class="text-muted">Cargando adjuntos…</div>`;

        try {
            const r = await fetch(URL_ADJ_LIST(citaId), { cache: 'no-store' });
            const raw = await r.text();
            if (!r.ok) {
                console.error('Adj LIST', r.status, raw);
                if (list) list.innerHTML = `<div class="text-danger">No se pudieron cargar los adjuntos.</div>`;
                return;
            }

            const items = parseJson(raw) ?? [];
            if (!Array.isArray(items) || items.length === 0) {
                if (list) list.innerHTML = `<div class="text-muted">Sin adjuntos.</div>`;
                return;
            }

            if (list) {
                list.innerHTML = `
          <ul class="list-group">
            ${items.map(a => `
              <li class="list-group-item d-flex align-items-center justify-content-between">
                <div class="me-3">
                  <i class="bi bi-file-earmark me-1"></i> ${a.nombreArchivo ?? a.NombreArchivo}
                  ${a.sizeBytes ? `<div class="small text-muted">${a.sizeBytes} bytes</div>` : ''}
                </div>
                <div class="btn-group btn-group-sm">
                  <a class="btn btn-outline-primary" href="${URL_ADJ_DOWNLOAD(a.adjuntoID ?? a.AdjuntoID)}" title="Descargar">
                    <i class="bi bi-download"></i>
                  </a>
                  <button class="btn btn-outline-danger" data-action="h-del-adj"
                          data-id="${a.adjuntoID ?? a.AdjuntoID}" title="Eliminar">
                    <i class="bi bi-trash"></i>
                  </button>
                </div>
              </li>
            `).join('')}
          </ul>`;
            }
        } catch (err) {
            console.error('Adj exception', err);
            if (list) list.innerHTML = `<div class="text-danger">Error cargando adjuntos.</div>`;
        }
    }

    // Abrir modal de adjuntos
    root.addEventListener('click', (e) => {
        const btn = e.target.closest('[data-action="h-files"]');
        if (!btn) return;
        const id = parseInt(btn.dataset.id || '0', 10);
        if (!id) return;

        cargarAdjuntosEnModal(id);
        modal('hListModal')?.show();
    });

    // Subir adjunto
    document.getElementById('hUpForm')?.addEventListener('submit', async (e) => {
        e.preventDefault();
        const citaId = parseInt((q('#hUpCitaId', document)?.value || '0'), 10);
        const file = q('#hUpFile', document)?.files?.[0];

        if (!citaId) return toastMsg('Cita inválida.', false);
        if (!file) return toastMsg('Seleccione un archivo.', false);

        const info = q('#hUpMsg', document);
        info?.classList.remove('d-none');

        const fd = new FormData();
        fd.append('CitaID', String(citaId));
        fd.append('file', file, file.name);

        try {
            const r = await fetch(URL_ADJ_UPLOAD, { method: 'POST', body: fd });
            const raw = await r.text();
            if (!r.ok) {
                console.error('Adj UP', r.status, raw);
                toastMsg('No se pudo subir el archivo.', false);
                return;
            }
            toastMsg('Archivo subido.');
            await cargarAdjuntosEnModal(citaId);

            // Actualiza badge en la tabla
            const badge = q(`tr[data-cita-id="${citaId}"] td:nth-child(5) .badge`, root);
            if (badge) badge.textContent = String((parseInt(badge.textContent || '0', 10) || 0) + 1);
            set('#hUpFile', '');
        } catch (err) {
            console.error('Adj UP ex', err);
            toastMsg('Error de red al subir archivo.', false);
        } finally {
            info?.classList.add('d-none');
        }
    });

    // Eliminar adjunto
    document.getElementById('hListBody')?.addEventListener('click', async (e) => {
        const btn = e.target.closest('[data-action="h-del-adj"]');
        if (!btn) return;

        const adjId = parseInt(btn.dataset.id || '0', 10);
        const citaId = parseInt(q('#hUpCitaId', document)?.value || '0', 10);
        if (!adjId || !citaId) return;

        const ok = await confirmNice({
            title: 'Eliminar adjunto',
            message: 'Esta acción no se puede deshacer. ¿Deseas continuar?',
            okText: 'Sí, eliminar',
            okClass: 'btn-danger'
        });
        if (!ok) return;

        try {
            const r = await fetch(URL_ADJ_DELETE(adjId), { method: 'DELETE' });
            const raw = await r.text();
            if (!r.ok) {
                console.error('Adj DEL', r.status, raw);
                return toastMsg('No se pudo eliminar el adjunto.', false);
            }
            toastMsg('Adjunto eliminado.');
            await cargarAdjuntosEnModal(citaId);

            // Actualiza badge en la tabla
            const badge = q(`tr[data-cita-id="${citaId}"] td:nth-child(5) .badge`, root);
            if (badge) {
                const n = Math.max(0, (parseInt(badge.textContent || '1', 10) || 1) - 1);
                badge.textContent = String(n);
            }
        } catch (err) {
            console.error('Adj DEL ex', err);
            toastMsg('Error eliminando adjunto.', false);
        }
    });

    // Recargar
    root.addEventListener('click', (e) => {
        if (e.target.closest(BTN_RELOAD)) {
            cargarPacientesSelect(true).then(cargarHistorial);
        }
    });

    // Cambio de paciente
    root.addEventListener('change', (e) => {
        if (e.target.id === 'hmPaciente') cargarHistorial();
    });

    const obs = new MutationObserver(() => {
        const marker = root.querySelector('[data-partial-name="HistorialMedico"]');
        if (marker) {
            cargarPacientesSelect(false).then(() => {
                const sel = q(SEL_PAC);
                if (sel && sel.value) cargarHistorial();
            });
            obs.disconnect();
        }
    });
    obs.observe(root, { childList: true, subtree: true });


    if (root.querySelector('[data-partial-name="HistorialMedico"]')) {
        cargarPacientesSelect(false).then(() => {
            const sel = q(SEL_PAC);
            if (sel && sel.value) cargarHistorial();
        });
    }
})();