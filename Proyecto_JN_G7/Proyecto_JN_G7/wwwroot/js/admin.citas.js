(() => {
    'use strict';

    // Espera a que el DOM base esté listo (admin/Index cargado)
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    function init() {
        const content = document.getElementById('adminContent');
        if (!content) return;

        const el = sel => document.querySelector(sel);

        // Utilidad: obtener/crear modal por id cuando ya existe en el DOM
        function getModal(id) {
            const node = document.getElementById(id);
            if (!node) return null;
            return bootstrap.Modal.getOrCreateInstance(node);
        }

        // Recargar la parcial de Citas
        function reloadCitas() {
            const a = document.querySelector('#adminSidebar [data-partial="Citas"]');
            if (a) a.click();
        }

        // ---------- NUEVA CITA ----------
        content.addEventListener('click', e => {
            const btn = e.target.closest('[data-action="new-cita"]');
            if (!btn) return;

            // Rellena el formulario vacío
            setValue('#citaId', '');
            setValue('#pacienteId', '');
            setValue('#doctorId', '');
            setValue('#fechaHora', '');
            setValue('#estado', 'Pendiente');
            setValue('#motivo', '');

            const title = document.getElementById('modalCitaTitle');
            if (title) title.textContent = 'Nueva cita';

            const modal = getModal('modalCita');
            if (modal) modal.show();
        });

        // ---------- EDITAR CITA ----------
        content.addEventListener('click', e => {
            const btn = e.target.closest('[data-action="edit-cita"]');
            if (!btn) return;

            setValue('#citaId', btn.dataset.citaId || '');
            setValue('#pacienteId', btn.dataset.pacienteId || '');
            setValue('#doctorId', btn.dataset.doctorId || '');
            setValue('#fechaHora', btn.dataset.fecha || '');
            setValue('#estado', btn.dataset.estado || 'Pendiente');
            setValue('#motivo', btn.dataset.motivo || '');

            const title = document.getElementById('modalCitaTitle');
            if (title) title.textContent = 'Editar cita';

            const modal = getModal('modalCita');
            if (modal) modal.show();
        });

        // ---------- GUARDAR CITA (crear/actualizar) ----------
        content.addEventListener('submit', async e => {
            const form = e.target.closest('#formCita');
            if (!form) return;
            e.preventDefault();

            const id = getValue('#citaId').trim();
            const body = {
                PacienteID: parseInt(getValue('#pacienteId'), 10),
                DoctorID: parseInt(getValue('#doctorId'), 10),
                FechaHora: getValue('#fechaHora'),
                Estado: getValue('#estado'),
                MotivoConsulta: getValue('#motivo')
            };

            const url = id
                ? `${window.ApiBaseUrl}api/Cita/${id}`
                : `${window.ApiBaseUrl}api/Cita`;

            const resp = await fetch(url, {
                method: id ? 'PUT' : 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });

            if (!resp.ok) {
                alert('No se pudo guardar la cita.');
                return;
            }

            const modal = getModal('modalCita');
            if (modal) modal.hide();
            reloadCitas();
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
        content.addEventListener('click', e => {
            const btn = e.target.closest('[data-action="attend-solicitud"]');
            if (!btn) return;

            setValue('#solicitudId', btn.dataset.solicitudId || '');
            setValue('#a_pacienteId', '');
            setValue('#a_doctorId', '');
            setValue('#a_fechaHora', '');
            setValue('#a_estado', 'Pendiente');
            setValue('#a_motivo', btn.dataset.motivo || '');

            const modal = getModal('modalAtender');
            if (modal) modal.show();
        });

        // ---------- CREAR CITA DESDE SOLICITUD ----------
        content.addEventListener('submit', async e => {
            const form = e.target.closest('#formAtender');
            if (!form) return;
            e.preventDefault();

            const sid = getValue('#solicitudId');
            const body = {
                PacienteID: parseInt(getValue('#a_pacienteId'), 10),
                DoctorID: parseInt(getValue('#a_doctorId'), 10),
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

            const modal = getModal('modalAtender');
            if (modal) modal.hide();
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

        // Helpers de inputs
        function setValue(selector, val) {
            const n = document.querySelector(selector);
            if (n) n.value = val;
        }
        function getValue(selector) {
            const n = document.querySelector(selector);
            return n ? n.value : '';
        }
    }
})();
