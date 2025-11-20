/*
 Validación custom y UX con jQuery para el formulario de becas
 - Valida cédula costarricense de 9 dígitos
 - Colorea campos (rojo error, verde válido)
 - Animaciones: fade para mensajes, shake en error
 - Limpia formulario tras envío exitoso (cuando aparece TempData)
*/

(function ($) {
    function shake($el) {
        $el.css('position', 'relative');
        $el.animate({ left: -10 }, 80)
            .animate({ left: 10 }, 80)
            .animate({ left: -6 }, 60)
            .animate({ left: 6 }, 60)
            .animate({ left: 0 }, 50);
    }

    // Regla custom para cédula CR: 9 dígitos
    if ($.validator && $.validator.addMethod) {
        $.validator.addMethod("cedulacr", function (value, element) {

            return this.optional(element) || /^\d{9}$/.test(value);
        }, "La cédula debe tener exactamente 9 dígitos.");

        // Adaptador unobtrusive
        $.validator.unobtrusive.adapters.addBool("cedulacr");
    }

    $(function () {
        var $forms = $("#formBeca, #formDemo");

        $forms.each(function () {
            var $form = $(this);

            // Asignar regla custom a campo cédula si existe
            var $cedula = $form.find("#Cedula");
            if ($cedula.length) {
                $cedula.rules && $cedula.rules("add", { cedulacr: true });
            }

            // Validación en tiempo real
            $form.find("input, select, textarea").on("input change blur", function () {
                var $field = $(this);
                if (!$field.closest("form").length) return;
                if ($field.valid && $field.valid()) {
                    $field.removeClass("is-invalid").addClass("is-valid");
                } else {
                    $field.removeClass("is-valid").addClass("is-invalid");
                }
            });

            // Interceptar submit para animación shake en errores
            $form.on("submit", function (e) {
                var isValid = $form.valid ? $form.valid() : true;
                if (!isValid) {
                    e.preventDefault();
                    var $firstError = $form.find(".input-validation-error, .is-invalid").first();
                    if ($firstError.length) {
                        shake($firstError);
                        $('html, body').animate({ scrollTop: $firstError.offset().top - 100 }, 300);
                        console.log("Esta llegando valido?", isValid);
                    }
                }
            });
        });

        // Mensaje de éxito: aplicar animación a alertas conocidas
        var $msgs = $("#mensajeExito, #mensajeDemo");
        $msgs.each(function () {
            var $msg = $(this);
            $msg.hide().fadeIn(300).delay(1800).fadeOut(500);
        });
    });
})(jQuery);
