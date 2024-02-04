// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Inicio - Mostrar Modal de mensaje de error:
$(document).ready(function () {
    if (mensajeError != null && mensajeError !== "") {
        // Decodifica el mensaje para mostrar símbolos
        var mensajeDecodificado = decodeURIComponent(mensajeError);
        $('#errorModalMessage').html(mensajeDecodificado);

        // Muestra el modal
        $('#errorModal').modal('show');
    }
});
// Fin - Mostrar Modal de mensaje de error: