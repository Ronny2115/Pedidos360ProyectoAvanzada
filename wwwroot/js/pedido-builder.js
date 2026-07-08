(function () {
    "use strict";

    var lineas = [];
    var debounceTimer = null;

    var $buscarProducto = $("#buscarProducto");
    var $resultados = $("#resultadosProducto");
    var $lineasBody = $("#lineasBody");
    var $lineasInputs = $("#lineasInputs");
    var $btnConfirmar = $("#btnConfirmar");

    function formatoMoneda(valor) {
        return "$" + Number(valor).toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    $buscarProducto.on("input", function () {
        var termino = $(this).val().trim();
        clearTimeout(debounceTimer);

        if (termino.length === 0) {
            $resultados.empty();
            return;
        }

        debounceTimer = setTimeout(function () {
            $.get("/api/productos/buscar", { q: termino })
                .done(function (productos) {
                    renderResultados(productos);
                })
                .fail(function () {
                    $resultados.empty();
                });
        }, 300);
    });

    $(document).on("click", function (e) {
        if (!$(e.target).closest("#buscarProducto, #resultadosProducto").length) {
            $resultados.empty();
        }
    });

    function renderResultados(productos) {
        $resultados.empty();

        if (!productos || productos.length === 0) {
            return;
        }

        productos.forEach(function (producto) {
            var $item = $("<button type='button' class='list-group-item list-group-item-action d-flex align-items-center gap-2'></button>")
                .on("click", function () {
                    agregarLinea(producto);
                    $buscarProducto.val("");
                    $resultados.empty();
                });
            $item.append($("<i class='bi bi-cookie text-body-secondary'></i>"));
            $item.append($("<span></span>").text(producto.nombre + " — " + formatoMoneda(producto.precio) + " (stock: " + producto.stock + ")"));
            $resultados.append($item);
        });
    }

    function agregarLinea(producto) {
        var existente = lineas.find(function (l) { return l.productoId === producto.id; });
        if (existente) {
            existente.cantidad += 1;
        } else {
            lineas.push({
                productoId: producto.id,
                nombre: producto.nombre,
                precioUnit: producto.precio,
                stock: producto.stock,
                cantidad: 1,
                descuento: 0
            });
        }
        renderLineas();
    }

    function eliminarLinea(index) {
        lineas.splice(index, 1);
        renderLineas();
    }

    function renderLineas() {
        $lineasBody.empty();
        $lineasInputs.empty();

        if (lineas.length === 0) {
            $lineasBody.append("<tr id='sinLineas'><td colspan='6' class='text-center text-muted py-4'><i class='bi bi-basket3 fs-4 d-block mb-1'></i>Aun no ha agregado productos.</td></tr>");
            $btnConfirmar.prop("disabled", true);
            actualizarTotales({ subtotal: 0, impuestos: 0, total: 0 });
            return;
        }

        lineas.forEach(function (linea, index) {
            var $row = $("<tr></tr>").attr("data-index", index);
            $row.append($("<td></td>").append($("<i class='bi bi-cookie text-body-secondary me-1'></i>"), document.createTextNode(linea.nombre)));
            $row.append($("<td></td>").append(
                $("<input type='number' min='1' class='form-control form-control-sm cantidad-input' />").val(linea.cantidad)
            ));
            $row.append($("<td></td>").append(
                $("<input type='number' min='0' max='100' step='0.01' class='form-control form-control-sm descuento-input' />").val(linea.descuento)
            ));
            $row.append($("<td></td>").text(formatoMoneda(linea.precioUnit)));
            $row.append($("<td class='total-linea'></td>").text("..."));
            $row.append($("<td></td>").append(
                $("<button type='button' class='btn btn-sm btn-outline-danger' title='Eliminar'><i class=\"bi bi-trash\"></i></button>").on("click", function () {
                    eliminarLinea(index);
                })
            ));
            $lineasBody.append($row);

            $lineasInputs.append(
                "<input type='hidden' name='Lineas[" + index + "].ProductoId' value='" + linea.productoId + "' />" +
                "<input type='hidden' name='Lineas[" + index + "].Cantidad' value='" + linea.cantidad + "' />" +
                "<input type='hidden' name='Lineas[" + index + "].Descuento' value='" + linea.descuento + "' />"
            );
        });

        $btnConfirmar.prop("disabled", false);
        recalcular();
    }

    $lineasBody.on("change", ".cantidad-input", function () {
        var index = $(this).closest("tr").data("index");
        var valor = parseInt($(this).val(), 10);
        lineas[index].cantidad = isNaN(valor) || valor < 1 ? 1 : valor;
        renderLineas();
    });

    $lineasBody.on("change", ".descuento-input", function () {
        var index = $(this).closest("tr").data("index");
        var valor = parseFloat($(this).val());
        lineas[index].descuento = isNaN(valor) ? 0 : Math.min(100, Math.max(0, valor));
        renderLineas();
    });

    function recalcular() {
        var payload = {
            lineas: lineas.map(function (l) {
                return { productoId: l.productoId, cantidad: l.cantidad, descuento: l.descuento };
            })
        };

        $.ajax({
            url: "/api/pedidos/calcular",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload)
        }).done(function (resultado) {
            var $filas = $lineasBody.find("tr");
            resultado.lineas.forEach(function (lineaResultado, index) {
                var $fila = $filas.eq(index);
                $fila.find(".total-linea").text(formatoMoneda(lineaResultado.totalLinea));
                $fila.toggleClass("table-danger", !lineaResultado.stockSuficiente);
            });

            var hayStockInsuficiente = resultado.lineas.some(function (l) { return !l.stockSuficiente; });
            $btnConfirmar.prop("disabled", hayStockInsuficiente || lineas.length === 0);

            actualizarTotales(resultado);
        });
    }

    function actualizarTotales(resultado) {
        $("#totalSubtotal").text(formatoMoneda(resultado.subtotal));
        $("#totalImpuestos").text(formatoMoneda(resultado.impuestos));
        $("#totalTotal").text(formatoMoneda(resultado.total));
    }
})();
