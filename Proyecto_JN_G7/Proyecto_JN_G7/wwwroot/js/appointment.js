// wwwroot/js/appointment.js
(() => {
    'use strict';

    const form = document.getElementById('appointmentForm')
        || document.querySelector('#appointment form')
        || document.querySelector('.appointment form');
    if (!form) return;

    const loading = form.querySelector('.loading');
    const errorMsg = form.querySelector('.error-message');
    const successMsg = form.querySelector('.sent-message'); 

    const byId = id => document.getElementById(id);
    const name = byId('name');
    const email = byId('email');
    const phone = byId('phone');
    const date = byId('date');
    const department = byId('department');
    const doctor = byId('doctor'); 
    const message = form.querySelector('[name="message"]');

    const fields = [name, email, phone, date, department];

   
    function validateField(el) {
        if (!el) return;
        if (el.checkValidity()) {
            el.classList.remove('is-invalid');
            el.classList.add('is-valid');
        } else {
            el.classList.remove('is-valid');
            el.classList.add('is-invalid');
        }
    }
    fields.forEach(f => f.addEventListener('input', () => validateField(f)));
    if (doctor) doctor.addEventListener('input', () => validateField(doctor));


    (function setMinDate() {
        const now = new Date();
        now.setMinutes(now.getMinutes() + 15);
        const pad = n => String(n).padStart(2, '0');
        const local = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}T${pad(now.getHours())}:${pad(now.getMinutes())}`;
        date.min = local;
    })();

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        e.stopPropagation();

        fields.forEach(validateField);
        if (!form.checkValidity()) return;

        const localVal = date.value; // "YYYY-MM-DDTHH:mm"
        const fechaIso = localVal ? new Date(localVal).toISOString() : null;

        const payload = {
            Nombre: name.value.trim(),
            Email: email.value.trim(),
            Telefono: phone.value.trim(),
            FechaHoraPreferida: fechaIso,
            Especialidad: department.value,
            DoctorNombre: doctor && doctor.value ? doctor.value : null,
            Mensaje: message && message.value ? message.value.trim() : null
        };

        // UI reset
        loading.classList.remove('d-none'); loading.style.display = '';
        errorMsg.classList.add('d-none'); errorMsg.style.display = 'none';
        if (successMsg) { successMsg.classList.add('d-none'); successMsg.style.display = 'none'; }

        try {
            const apiBase = window.ApiBaseUrl || '';
            const url = new URL('api/Cita/RegistroPublico', apiBase).toString();

            const resp = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            const contentType = resp.headers.get('content-type') || '';
            const body = contentType.includes('application/json') ? await resp.json() : await resp.text();

            if (!resp.ok) {
                const msg = (body && body.mensaje) || (typeof body === 'string' ? body : `HTTP ${resp.status}`);
                throw new Error(msg);
            }

            // --- ÉXITO: llenar y mostrar modal ---
            const modalEl = document.getElementById('appointmentSuccessModal');
            if (modalEl) {
                
                const displayDate = localVal
                    ? new Date(localVal).toLocaleString([], { dateStyle: 'medium', timeStyle: 'short' })
                    : '';

                byId('apmtModalName').textContent = name.value.trim();
                byId('apmtModalDate').textContent = displayDate;
                byId('apmtModalDept').textContent = department.value;
                byId('apmtModalDoctor').textContent = (doctor && doctor.value) ? doctor.value : 'Por asignar';

                const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
                modal.show();

                
                modalEl.addEventListener('hidden.bs.modal', () => {
                    form.reset();
                    [...form.querySelectorAll('.is-valid')].forEach(el => el.classList.remove('is-valid'));
                }, { once: true });
            } else {
                
                if (successMsg) {
                    const okMsg = (body && body.mensaje) || '¡Solicitud enviada! Te contactaremos pronto.';
                    successMsg.textContent = okMsg;
                    successMsg.classList.remove('d-none'); successMsg.style.display = '';
                    form.reset();
                    [...form.querySelectorAll('.is-valid')].forEach(el => el.classList.remove('is-valid'));
                }
            }

        } catch (err) {
            console.error(err);
            errorMsg.textContent = err?.message || 'No pudimos enviar la solicitud. Intenta más tarde.';
            errorMsg.classList.remove('d-none'); errorMsg.style.display = '';
        } finally {
            loading.classList.add('d-none'); loading.style.display = 'none';
        }
    });
})();

(function () {
    const dept = document.getElementById('department');
    const docSel = document.getElementById('doctor');
    if (!dept || !docSel) return;

    const allOptions = [...docSel.querySelectorAll('option')].slice(1);

    function renderDoctors(filter) {
        // reset opciones (deja la de "sin preferencia")
        docSel.length = 1;

        const toShow = !filter
            ? allOptions
            : allOptions.filter(o => (o.getAttribute('data-esp') || '').toLowerCase() === filter.toLowerCase());

        toShow.forEach(o => docSel.add(o.cloneNode(true)));
        docSel.value = ""; // reset selección
    }

    dept.addEventListener('change', () => {
        const val = dept.value.trim();
        renderDoctors(val === "" ? null : val);
    });

    // render inicial por si ya viene algo seleccionado
    renderDoctors(dept.value.trim() || null);
})();
