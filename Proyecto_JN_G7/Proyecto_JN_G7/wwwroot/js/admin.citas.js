(() => {
    'use strict';

    if (window.__citasBound) return;
    window.__citasBound = true;

    const root = document.getElementById('adminContent');
    if (!root) return;

    const API = window.ApiBaseUrl;

    // Helpers básicos
    const dq = (s) => document.querySelector(s);
    const val = (s) => (dq(s)?.value ?? '').trim();
    const set = (s, v) => { const el = dq(s); if (el) el.value = v ?? ''; };
    const modal = (id) => {
        const el = document.getElementById(id);
        return el ? bootstrap.Modal.getOrCreateInstance(el) : null;
    };
    const reloadCitas = () => {
        const a = document.querySelector('#adminSidebar [data-partial="Citas"]');
        if (a) a.click();
    };

    // Lee un <select> y devuelve el entero del doctor (value o data-id)
    function getSelectedInt(sel) {
        const el = dq(sel);
        if (!el) return 0;

        const v = (el.value ?? '').trim();
        if (/^\d+$/.test(v)) return parseInt(v, 10);

        const opt = el.options[el.selectedIndex];
        const d = (opt?.getAttribute('data-id') ?? '').trim();
        return /^\d+$/.test(d) ? parseInt(d, 10) : 0;
    }

    // ---------- NUEVA CITA ----------
    root.addEventListener('click', (e) => {
        const btn = e.target.closest('[data-action="new-cita"]');
        if (!btn) return;

        set('#citaId', '');
        set('#pacienteEdit', '');
        set('#doctorEdit', '');
        set('#fechaHora', '');
        set('#estado', 'Pendiente');
        set('#motivo', '');

        const title = document.getElementById('modalCitaTitle');
        if (title) title.textContent = 'Nueva cita';

        modal('modalCita')?.show();
    });

    // ---------- EDITAR CITA ----------
    root.addEventListener('click', (e) => {
        const btn = e.target.closest('[data-action="edit-cita"]');
        if (!btn) return;

        set('#citaId', btn.dataset.citaId ?? '');
        set('#pacienteEdit', btn.dataset.pacienteId ?? '');
        set('#doctorEdit', btn.dataset.doctorId ?? '');
        set('#fechaHora', btn.dataset.fecha ?? '');
        set('#estado', btn.dataset.estado ?? 'Pendiente');
        set('#motivo', btn.dataset.motivo ?? '');

        const title = document.getElementById('modalCitaTitle');
        if (title) title.textContent = 'Editar cita';

        modal('modalCita')?.show();
    });

    // ---------- GUARDAR CITA (crear/actualizar) ----------
    root.addEventListener('submit', async (e) => {
        const form = e.target.closest('#formCita');
        if (!form) return;
        e.preventDefault();

        const id = val('#citaId');
        const body = {
            PacienteID: parseInt(val('#pacienteEdit') || '0', 10),
            DoctorID: getSelectedInt('#doctorEdit'),
            FechaHora: val('#fechaHora'),
            Estado: val('#estado') || 'Pendiente',
            MotivoConsulta: val('#motivo') || ''
        };

        if (!body.PacienteID) return alert('Seleccione un paciente.');
        if (!body.DoctorID) return alert('Seleccione un doctor.');
        if (!body.FechaHora) return alert('Seleccione fecha y hora.');

        const url = id ? `${API}api/Cita/${id}` : `${API}api/Cita`;
        const method = id ? 'PUT' : 'POST';

        const resp = await fetch(url, {
            method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });

        if (!resp.ok) {
            const txt = await resp.text();
            alert(`No se pudo guardar la cita.\n${txt || ''}`);
            return;
        }

        modal('modalCita')?.hide();
        reloadCitas();
    });

    // ---------- ELIMINAR CITA ----------
    root.addEventListener('click', async (e) => {
        const btn = e.target.closest('[data-action="delete-cita"]');
        if (!btn) return;
        if (!confirm('¿Eliminar esta cita?')) return;

        const id = btn.dataset.citaId;
        const resp = await fetch(`${API}api/Cita/${id}`, { method: 'DELETE' });

        if (!resp.ok) {
            alert('No se pudo eliminar la cita.');
            return;
        }
        reloadCitas();
    });

    // ---------- ATENDER SOLICITUD ----------
    root.addEventListener('click', async (e) => {
        const btn = e.target.closest('[data-action="attend-solicitud"]');
        if (!btn) return;

        set('#solicitudId', btn.dataset.solicitudId ?? '');
        set('#a_motivo', btn.dataset.motivo ?? '');
        set('#a_fechaHora', '');
        set('#a_estado', 'Pendiente');
        set('#a_pacienteSelect', '');
        document.getElementById('a_pacienteHint')?.classList.add('d-none');

        // Buscar paciente por email
        const email = btn.dataset.email || '';
        if (email) {
            try {
                const r = await fetch(`${API}api/Paciente/Buscar?email=${encodeURIComponent(email)}`);
                if (r.ok) {
                    const p = await r.json();
                    set('#a_pacienteSelect', String(p.pacienteID ?? p.PacienteID ?? ''));
                } else {
                    document.getElementById('a_pacienteHint')?.classList.remove('d-none');
                }
            } catch { /* no-op */ }
        }


        const fp = document.getElementById('formPaciente');
        if (fp) fp.dataset.targetSelect = '#a_pacienteSelect';
        set('#p_nombre', btn.dataset.nombre || '');
        set('#p_email', email);

        modal('modalAtender')?.show();
    });

    // ---------- CREAR CITA DESDE SOLICITUD ----------
    root.addEventListener('submit', async (e) => {
        const form = e.target.closest('#formAtender');
        if (!form) return;
        e.preventDefault();

        const sid = val('#solicitudId');
        const body = {
            PacienteID: parseInt(val('#a_pacienteSelect') || '0', 10),
            DoctorID: getSelectedInt('#a_doctorSelect'),
            FechaHora: val('#a_fechaHora'),
            Estado: val('#a_estado'),
            MotivoConsulta: val('#a_motivo')
        };

        if (!body.PacienteID) return alert('Seleccione un paciente.');
        if (!body.DoctorID) return alert('Seleccione un doctor.');
        if (!body.FechaHora) return alert('Seleccione fecha y hora.');

        const resp = await fetch(`${API}api/Cita/Solicitudes/${sid}/Atender`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });

        if (!resp.ok) {
            const txt = await resp.text();
            alert(`No se pudo atender la solicitud.\n${txt || ''}`);
            return;
        }

        modal('modalAtender')?.hide();
        reloadCitas();
    });

    // ---------- DESCARTAR SOLICITUD ----------
    root.addEventListener('click', async (e) => {
        const btn = e.target.closest('[data-action="delete-solicitud"]');
        if (!btn) return;
        if (!confirm('¿Descartar esta solicitud?')) return;

        const sid = btn.dataset.solicitudId;
        const resp = await fetch(`${API}api/Cita/Solicitudes/${sid}`, { method: 'DELETE' });

        if (!resp.ok) {
            alert('No se pudo descartar la solicitud.');
            return;
        }
        reloadCitas();
    });

    // ---------- CREAR PACIENTE  ----------
    root.addEventListener('click', (e) => {
        const btn = e.target.closest('[data-action="open-new-paciente"]');
        if (!btn) return;

        const fp = document.getElementById('formPaciente');
        if (fp) fp.dataset.targetSelect = btn.dataset.targetSelect || '#pacienteEdit';

        if (!val('#p_nombre')) set('#p_nombre', '');
        if (!val('#p_email')) set('#p_email', '');

        set('#p_cedula', '');
        set('#p_pass', '');

        modal('modalPaciente')?.show();
    });

    root.addEventListener('submit', async (e) => {
        const form = e.target.closest('#formPaciente');
        if (!form) return;
        e.preventDefault();

        const body = {
            cedula: val('#p_cedula'),
            nombreCompleto: val('#p_nombre'),
            correoElectronico: val('#p_email'),
            contrasena: val('#p_pass') || null
        };

        if (!body.cedula || !body.nombreCompleto || !body.correoElectronico) {
            alert('Cédula, nombre y correo son obligatorios.');
            return;
        }

        try {
            const r = await fetch(`${API}api/Paciente`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });

            const txt = await r.text();
            if (!r.ok) {
                alert(`No se pudo crear el paciente.\n${txt || ''}`);
                return;
            }

            const data = JSON.parse(txt); // { pacienteID: n }
            const targetSelect = form.dataset.targetSelect || '#pacienteEdit';

            const sel = dq(targetSelect);
            if (sel) {
                const display = `${body.nombreCompleto} (${body.cedula}) — ${body.correoElectronico}`;
                const value = String(data.pacienteID);
                let opt = [...sel.options].find(o => o.value === value);
                if (!opt) {
                    opt = new Option(display, value);
                    sel.add(opt);
                }
                sel.value = value;
                sel.dispatchEvent(new Event('change'));
            }

            modal('modalPaciente')?.hide();
            document.getElementById('a_pacienteHint')?.classList.add('d-none');
        } catch {
            alert('Error de red al crear paciente.');
        }
    });

    const mo = new MutationObserver(() => {
        if (root.querySelector('[data-partial-name="Citas"]')) {

            mo.disconnect();
        }
    });
    mo.observe(root, { childList: true, subtree: true });
})();