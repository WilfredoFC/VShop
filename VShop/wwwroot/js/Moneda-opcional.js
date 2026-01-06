document.addEventListener("DOMContentLoaded", function () {
    const formatter = new Intl.NumberFormat("en-US");

    document.querySelectorAll(".formato-opcional").forEach(function (input) {
        // Al cargar: formatear si hay valor
        if (input.value) {
            const cleaned = input.value.replace(/,/g, '');
            const numeric = parseInt(cleaned);
            input.value = isNaN(numeric) ? '' : formatter.format(numeric);
        }

        // Formatear mientras se escribe
        input.addEventListener("input", function () {
            const raw = input.value.replace(/,/g, '').replace(/[^\d]/g, '');
            if (!raw) {
                input.value = '';
                return;
            }

            const numeric = parseInt(raw);
            input.value = formatter.format(numeric);
        });
    });

    // Antes de enviar el formulario: limpiar valores (sin validar)
    const form = document.querySelector("form");
    if (form) {
        form.addEventListener("submit", function () {
            document.querySelectorAll(".formato-opcional").forEach(function (input) {
                const cleaned = input.value.replace(/,/g, '');
                input.value = cleaned;
            });
        });
    }
});
