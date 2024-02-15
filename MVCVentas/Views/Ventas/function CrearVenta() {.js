        function CrearVenta() {
            
            var detallesventa = [];
            
            $('#table-resumen-venta tbody tr').each(function () {
                var detalle = {
                    Id_Articulo: $(this).find("#id_Articulo").text(),
                    Cantidad: $(this).find("#cantidad").text(),
                    Precio: $(this).find("#precio").text(),
                    PrecioTotal: $(this).find("#precio-Total").text()
                };
                detallesventa.push(detalle);
            });

            var formData = $("#formVenta").serialize();

            formData += "&detallesventa=" + JSON.stringify(detallesventa);

            console.log("FormData: " + formData);

            $.ajax({
                url: '@Url.Action("CrearVenta", "Ventas")',
                type: 'POST',
                data: formData,
                success: function (response) {
                    if (response.success == true) {
                        console.log("Venta realizada con éxito.");
                        varciarTablaResumen();
                    }
                    else {
                        console.log("Error al realizar la venta.");
                    }
                },
                error: function (response) {
                }
            })
        }