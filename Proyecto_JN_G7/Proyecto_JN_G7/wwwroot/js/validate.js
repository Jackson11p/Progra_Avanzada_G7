// wwwroot/js/validate.js

(() => {
    'use strict';

    const form = document.getElementById('contactForm');
    const loading = document.getElementById('loading');
    const errorMsg = document.getElementById('errorMsg');
    const successMsg = document.getElementById('successMsg');
    const fields = ['Nombre', 'Email', 'Asunto', 'Mensaje']
        .map(name => document.getElementById(name));

    function validateField(field) {
        if (field.checkValidity()) {
            field.classList.remove('is-invalid');
            field.classList.add('is-valid');
        } else {
            field.classList.remove('is-valid');
            field.classList.add('is-invalid');
        }
    }

    fields.forEach(field => {
        field.addEventListener('input', () => validateField(field));
    });

    form.addEventListener('submit', async e => {
        e.preventDefault();
        e.stopPropagation();

        fields.forEach(f => validateField(f));
        if (!form.checkValidity()) return;

        const data = {
            Nombre: form.Nombre.value.trim(),
            Email: form.Email.value.trim(),
            Asunto: form.Asunto.value.trim(),
            Mensaje: form.Mensaje.value.trim()
        };

        loading.classList.remove('d-none');
        errorMsg.classList.add('d-none');
        successMsg.classList.add('d-none');

        try {
            const resp = await fetch(
                
                `${window.ApiBaseUrl}api/Contacto/Enviar`,
                {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                }
            );

            if (!resp.ok) throw new Error(`Error ${resp.status}`);

            successMsg.classList.remove('d-none');
            form.reset();
            fields.forEach(f => f.classList.remove('is-valid'));
        } catch (err) {
            errorMsg.textContent = 'Ocurrió un problema. Intenta más tarde.';
            errorMsg.classList.remove('d-none');
        } finally {
            loading.classList.add('d-none');
        }
    });
})();
