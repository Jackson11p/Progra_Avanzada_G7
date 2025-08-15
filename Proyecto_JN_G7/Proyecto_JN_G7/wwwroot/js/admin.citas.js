(() => {
    'use strict';

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    function init() {
        const content = document.getElementById('adminContent');
        if (!content) return;

        // Helpers únicos
        const getValue = sel => (document.querySelector(sel)?.value ?? '').trim();
        const setValue = (sel, v) => { const el = document.querySelector(sel); if (el) el.value = v ?? ''; };
        const getModal = id => {
            const el = document.getElementById(id);
            return el ? bootstrap.Modal.getOrCreateInstance(el) : null;
        };
        const reloadCitas = () => {
            const a = document.querySelector('#adminSidebar [data-partial="Citas"]');
            if (a) a.click();
        };

        // ---------- NUEVA CITA ----------
        content.addEventListener('click', e => {
            const btn = e.target.closest('[data-action="new-cita"]');
            if (!btn) return;

            setValue('#citaId', '');
            setValue('#pacienteEdit', '');
            setValue('#doctorEdit', '');   
            setValue('#fechaHora', '');
            setValue('#estado', 'Pendiente');
            setValue('#motivo', '');

            const title = document.getElementById('modalCitaTitle');
            if (title) title.textContent = 'Nueva cita';
            getModal('modalCita')?.show();
        });

        // ---------- EDITAR CITA ----------
        content.addEventListener('click', e => {
            const btn = e.target.closest('[data-action="edit-cita"]');
            if (!btn) return;

            setValue('#citaId', btn.dataset.citaId || '');
            setValue('#pacienteEdit', btn.dataset.pacienteId || '');
            setValue('#doctorEdit', btn.dataset.doctorId || '');
            setValue('#fechaHora', btn.dataset.fecha || '');
            setValue('#estado', btn.dataset.estado || 'Pendiente');
            setValue('#motivo', btn.dataset.motivo || '');

            const title = document.getElementById('modalCitaTitle');
            if (title) title.textContent = 'Editar cita';
            getModal('modalCita')?.show();
        });

        // ---------- GUARDAR CITA (crear/actualizar) ----------
        content.addEventListener('submit', async e => {
            const form = e.target.closest('#formCita');
            if (!form) return;
            e.preventDefault();

            const pid = parseInt(getValue('#pacienteEdit'), 10);
            const did = parseInt(getValue('#doctorEdit'), 10);
            const fecha = getValue('#fechaHora');

            if (!Number.isInteger(pid) || pid <= 0) { alert('Seleccione un paciente.'); return; }
            if (!Number.isInteger(did) || did <= 0) { alert('Seleccione un doctor.'); return; }
            if (!fecha) { alert('Seleccione fecha y hora.'); return; }

            const id = (getValue('#citaId') || '').trim();
            const url = id
                ? `${window.ApiBaseUrl}api/Cita/${id}`
                : `${window.ApiBaseUrl}api/Cita`;

            const body = {
                PacienteID: pid,
                DoctorID: did,
                FechaHora: fecha,
                Estado: getValue('#estado') || 'Pendiente',
                MotivoConsulta: getValue('#motivo') || null
            };

            try {
                const resp = await fetch(url, {
                    method: id ? 'PUT' : 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(body)
                });

                if (!resp.ok) {
                    const txt = await resp.text();
                    console.error('Error al guardar cita:', resp.status, txt);
                    alert(`No se pudo guardar la cita.\n${resp.status}\n${txt || ''}`);
                    return;
                }

                getModal('modalCita')?.hide();
                reloadCitas();
            } catch (err) {
                console.error(err);
                alert('Error de red al guardar la cita.');
            }
        });

        // ---------- ELIMINAR CITA ----------
        content.addEventListener('click', async e => {
            const btn = e.target.closest('[data-action="delete-cita"]');
            if (!btn) return;
            if (!confirm('¿Eliminar esta cita?')) return;

            const id = btn.dataset.citaId;
            const resp = await fetch(`${window.ApiBaseUrl}api/Cita/${id}`, { method: 'DELETE' });
            if (!resp.ok) {
                alert('No se pudo eliminar la cita.');
                return;
            }
            reloadCitas();
        });

        // ---------- ATENDER SOLICITUD ----------
        content.addEventListener('click', async e => {
            const btn = e.target.closest('[data-action="attend-solicitud"]');
            if (!btn) return;

            setValue('#solicitudId', btn.dataset.solicitudId || '');
            setValue('#a_motivo', btn.dataset.motivo || '');
            setValue('#a_fechaHora', '');
            setValue('#a_estado', 'Pendiente');

            setValue('#a_pacienteSelect', '');
            document.getElementById('a_pacienteHint')?.classList.add('d-none');

            const docAtVal = getValue('#a_doctorSelect');
            if (!docAtVal) {
                document.querySelector('#a_doctorSelect')?.classList.add('is-invalid');
                return;
            }
            document.querySelector('#a_doctorSelect')?.classList.remove('is-invalid');

            const email = btn.dataset.email || '';
            if (email) {
                try {
                    const resp = await fetch(`${window.ApiBaseUrl}api/Paciente/Buscar?email=${encodeURIComponent(email)}`);
                    if (resp.ok) {
                        const p = await resp.json();
                        setValue('#a_pacienteSelect', String(p.pacienteID));
                    } else {
                        document.getElementById('a_pacienteHint')?.classList.remove('d-none');
                    }
                } catch { /* no-op */ }
            }

            // Para crear paciente y que apunte a este select:
            const fp = document.getElementById('formPaciente');
            if (fp) fp.dataset.targetSelect = '#a_pacienteSelect';
            // Prellenar modal de paciente:
            setValue('#p_nombre', btn.dataset.nombre || '');
            setValue('#p_email', email);

            getModal('modalAtender')?.show();
        });

        // ---------- CREAR CITA DESDE SOLICITUD ----------
        content.addEventListener('submit', async e => {
            const form = e.target.closest('#formAtender');
            if (!form) return;
            e.preventDefault();

            const sid = getValue('#solicitudId');
            const body = {
                PacienteID: parseInt(getValue('#a_pacienteSelect'), 10),
                DoctorID: parseInt(getValue('#a_doctorSelect'), 10),
                FechaHora: getValue('#a_fechaHora'),
                Estado: getValue('#a_estado'),
                MotivoConsulta: getValue('#a_motivo')
            };

            const resp = await fetch(`${window.ApiBaseUrl}api/Cita/Solicitudes/${sid}/Atender`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });

            if (!resp.ok) {
                alert('No se pudo atender la solicitud.');
                return;
            }
            getModal('modalAtender')?.hide();
            reloadCitas();
        });

        // ---------- DESCARTAR SOLICITUD ----------
        content.addEventListener('click', async e => {
            const btn = e.target.closest('[data-action="delete-solicitud"]');
            if (!btn) return;
            if (!confirm('¿Descartar esta solicitud?')) return;

            const sid = btn.dataset.solicitudId;
            const resp = await fetch(`${window.ApiBaseUrl}api/Cita/Solicitudes/${sid}`, { method: 'DELETE' });
            if (!resp.ok) {
                alert('No se pudo descartar la solicitud.');
                return;
            }
            reloadCitas();
        });

        // ---------- ABRIR MODAL "CREAR PACIENTE" ----------
        content.addEventListener('click', e => {
            const btn = e.target.closest('[data-action="open-new-paciente"]');
            if (!btn) return;

            const fp = document.getElementById('formPaciente');
            if (fp) fp.dataset.targetSelect = btn.dataset.targetSelect || '#pacienteEdit';

            // Si no estaban prellenados por "Atender", se limpian:
            if (!getValue('#p_nombre')) setValue('#p_nombre', '');
            if (!getValue('#p_email')) setValue('#p_email', '');
            setValue('#p_cedula', '');
            setValue('#p_pass', '');

            getModal('modalPaciente')?.show();
        });

        // ---------- SUBMIT "CREAR PACIENTE" ----------
        content.addEventListener('submit', async e => {
            const form = e.target.closest('#formPaciente');
            if (!form) return;
            e.preventDefault();

            const body = {
                cedula: getValue('#p_cedula'),
                nombreCompleto: getValue('#p_nombre'),
                correoElectronico: getValue('#p_email'),
                contrasena: getValue('#p_pass') || null
            };

            if (!body.cedula || !body.nombreCompleto || !body.correoElectronico) {
                alert('Cédula, nombre y correo son obligatorios.');
                return;
            }

            try {
                const resp = await fetch(`${window.ApiBaseUrl}api/Paciente`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(body)
                });
                if (!resp.ok) {
                    const txt = await resp.text();
                    alert(`No se pudo crear el paciente.\n${txt || ''}`);
                    return;
                }

                const data = await resp.json(); 
                const targetSelect = form.dataset.targetSelect || '#pacienteEdit';

                const text = `${body.nombreCompleto} (${body.cedula}) — ${body.correoElectronico}`;
                const value = String(data.pacienteID);

                const sel = document.querySelector(targetSelect);
                if (sel) {
                    let opt = [...sel.options].find(o => o.value === value);
                    if (!opt) {
                        opt = new Option(text, value);
                        sel.add(opt);
                    }
                    sel.value = value;
                    sel.dispatchEvent(new Event('change'));
                }

                getModal('modalPaciente')?.hide();
                document.getElementById('a_pacienteHint')?.classList.add('d-none');
            } catch {
                alert('Error de red al crear paciente.');
            }
        });
    }
})();
