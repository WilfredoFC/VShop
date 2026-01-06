document.addEventListener("DOMContentLoaded", function () {
    const formatter = new Intl.NumberFormat("en-US");

    document.querySelectorAll(".moneda-entero").forEach(function (input) {
        
        if (input.value) {
            const cleaned = input.value.replace(/,/g, '');
            const numeric = parseInt(cleaned);
            input.value = isNaN(numeric) ? '' : formatter.format(numeric);
        }

        // Formatear en tiempo real
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

    // Validar y limpiar antes de enviar
    const form = document.querySelector("form");
    if (form) {
        form.addEventListener("submit", function (e) {
            let isValid = true;

            document.querySelectorAll(".moneda-entero").forEach(function (input) {
                const clean = input.value.replace(/,/g, '');
                const value = parseFloat(clean);

                // Validación personalizada: debe ser > 0
                if (isNaN(value) || value <= 0) {
                    isValid = false;

                    // Mostrar mensaje de error si no existe ya
                    let errorSpan = input.nextElementSibling;
                    if (!errorSpan || !errorSpan.classList.contains("field-validation-error")) {
                        errorSpan = document.createElement("span");
                        errorSpan.className = "text-danger field-validation-error";
                        errorSpan.innerText = "El límite debe ser mayor a 0.";
                        input.parentNode.appendChild(errorSpan);
                    } else {
                        errorSpan.innerText = "El límite debe ser mayor a 0.";
                    }
                } else {
                    // Quitar mensaje de error si ya es válido
                    const errorSpan = input.nextElementSibling;
                    if (errorSpan && errorSpan.classList.contains("field-validation-error")) {
                        errorSpan.remove();
                    }
                }

                // Limpiar valor antes de enviar
                input.value = clean;
            });

            if (!isValid) {
                e.preventDefault(); // detener envío
            }
        });
    }
});
