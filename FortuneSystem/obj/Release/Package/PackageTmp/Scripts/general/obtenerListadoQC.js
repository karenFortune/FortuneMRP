$(document).ready(function () {
    var ID = $("#IdPedido").val();
    buscar_estilos_Reporte(ID);
    $("#div_tabla_pnl").css("visibility", "hidden");
});

function probar() {
    $('#tableEstilos tr').on('click', function (e) {
        $('#tableEstilos tr').removeClass('highlighted');
        $(this).addClass('highlighted');
    });
    //obtener_tallas_item(id);
}
var cliente;
$(document).on("dblclick", "#tableEstilos tr", function () {
    var row = this.rowIndex;
    if (row !== 0) {
        var numEstilo = $('#tableEstilos tr:eq(' + row + ') td:eq(0)').html();
        //var estilo = $('#tabless tr:eq(' + row + ') td:eq(2)').html();
        //obtener_tallas_item(numEstilo);
    }

});
function registrarReporte1() {

    var r = 0; var c = 0; var i = 0; var cadena = new Array(3);
    cadena[0] = ''; cadena[1] = ''; cadena[2] = '';
    var nFilas = $("#tablaPruebaLavado tbody>tr").length;
    var nColumnas = $("#tablaPruebaLavado tr:last td").length;
    var resultado = '';
    $('#tablaPruebaLavado tbody>tr').each(function () {
        r = 0;
        c = 0;

        $(this).find("input").each(function () {
            $(this).closest('td').find("input").each(function () {

                cadena[c] += this.value + "*";
                c++;
            });

            r++;
        });
    });
    $('#tablaPruebaLavado tbody>tr').each(function () {


        resultado += $(this).find("select option:selected").val() + "*";


    });
    cadena.splice(2, 1, resultado);
    var errorR = 0;
    var fecha = $("#QCReport_Fecha").val();
    if (fecha === "") {
        errorR++;
        $('#QCReport_Fecha').css('border', '2px solid #e03f3f');
    }
    else {
        $('#QCReport_Fecha').css('border', '');
    }

    var turno = $("#QCReport_Turnos option:selected").val();
    if (turno === "0") {
        errorR++;
        $("#QCReport_Turnos option:selected").css('border', '2px solid #e03f3f');
    }
    else {
        $("#QCReport_Turnos option:selected").css('border', '');
    }

    var maquinas = $("#QCReport_Maquinas option:selected").val();
    if (maquinas === "0") {
        errorR++;
        $("#QCReport_Maquinas option:selected").css('border', '2px solid #e03f3f');
    }
    else {
        $("#QCReport_Maquinas option:selected").css('border', '');
    }

    var reporte = $("#QCReport_ReporteG").val();
    if (reporte === "") {
        errorR++;
        $('#QCReport_ReporteG').css('border', '2px solid #e03f3f');
    }
    else {
        $('#QCReport_ReporteG').css('border', '');
    }

    var sacador = $("#QCReport_Sacador").val();
    if (sacador === "") {
        errorR++;
        $('#QCReport_Sacador').css('border', '2px solid #e03f3f');
    }
    else {
        $('#QCReport_Sacador').css('border', '');
    }

    var cachador = $("#QCReport_Cachador").val();
    if (cachador === "") {
        errorR++;
        $('#QCReport_Cachador').css('border', '2px solid #e03f3f');
    }
    else {
        $('#QCReport_Cachador').css('border', '');
    }

    var metedor = $("#QCReport_Metedor").val();
    if (metedor === "") {
        errorR++;
        $('#QCReport_Metedor').css('border', '2px solid #e03f3f');
    }
    else {
        $('#QCReport_Metedor').css('border', '');
    }

    var inspector = $("#QCReport_QCInspector").val();
    if (inspector === "") {
        errorR++;
        $('#QCReport_QCInspector').css('border', '2px solid #e03f3f');
    }
    else {
        $('#QCReport_QCInspector').css('border', '');
    }
    enviarReporte1(cadena, errorR);

}


function enviarReporte1(cadena, errorR) {

    if (errorR !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {

        var fecha = $("#QCReport_Fecha").val();
        var reporte = $("#QCReport_ReporteG").val();
        var cachador = $("#QCReport_Cachador").val();
        var metedor = $("#QCReport_Metedor").val();
        var inspector = $("#QCReport_QCInspector").val();
        var maquina = $("#QCReport_Maquinas option:selected").val();
        var aql = $("#QCReport_AQL option:selected").val();
        var sacador = $("#QCReport_Sacador").val();
        var idSummary = $("#InfoSummary_IdItems").val();
        var turno = $("#QCReport_Turnos option:selected").val();


        $.ajax({
            url: "/QCReport/Obtener_Reporte_General",
            datatType: 'json',
            data: JSON.stringify({
                ReporteG: reporte, Sacador: sacador, Cachador: cachador, Metedor: metedor, QCInspector: inspector, DatoAQL: aql,
                Turno: turno, Fecha: fecha, IdMaquina: maquina, IdSummary: idSummary, ListTalla: cadena
            }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The Report were correctly registered.', 'success', 5, null);

            }
        });
    }

}

function actualizarReporte() {

    var r = 0; var c = 0; var i = 0; var cadena = new Array(4);
    cadena[0] = ''; cadena[1] = ''; cadena[2] = ''; cadena[3] = '';
    var nFilas = $("#tablaPruebaLavadoModif tbody>tr").length;
    var nColumnas = $("#tablaPruebaLavadoModif tr:last td").length;
    var resultado = '';
    $('#tablaPruebaLavadoModif tbody>tr').each(function () {
        r = 0;
        c = 0;

        $(this).find("input").each(function () {
            $(this).closest('td').find("input").each(function () {

                cadena[c] += this.value + "*";
                c++;
            });

            r++;
        });
    });
    $('#tablaPruebaLavadoModif tbody>tr').each(function () {


        resultado += $(this).find("select option:selected").val() + "*";


    });
    cadena.splice(3, 1, resultado);
    var errorR = 0;

    var fecha = $("#QCReport_Fecha").val();
    if (fecha === "") {
        errorR++;
        $('#QCReport_Fecha').css('border', '2px solid #e03f3f');
    }
    else {
        $('#QCReport_Fecha').css('border', '');
    }

    var turno = $("#QCReport_Turnos option:selected").val();
    if (turno === "0") {
        errorR++;
        $("#QCReport_Turnos option:selected").css('border', '2px solid #e03f3f');
    }
    else {
        $("#QCReport_Turnos option:selected").css('border', '');
    }

    var maquinas = $("#QCReport_Maquinas option:selected").val();
    if (maquinas === "0") {
        errorR++;
        $("#QCReport_Maquinas option:selected").css('border', '2px solid #e03f3f');
    }
    else {
        $("#QCReport_Maquinas option:selected").css('border', '');
    }

    var reporte = $("#QCReport_ReporteG").val();
    if (reporte === "") {
        errorR++;
        $('#QCReport_ReporteG').css('border', '2px solid #e03f3f');
    }
    else {
        $('#QCReport_ReporteG').css('border', '');
    }

    var sacador = $("#QCReport_Sacador").val();
    if (sacador === "") {
        errorR++;
        $('#QCReport_Sacador').css('border', '2px solid #e03f3f');
    }
    else {
        $('#QCReport_Sacador').css('border', '');
    }

    var cachador = $("#QCReport_Cachador").val();
    if (cachador === "") {
        errorR++;
        $('#QCReport_Cachador').css('border', '2px solid #e03f3f');
    }
    else {
        $('#QCReport_Cachador').css('border', '');
    }

    var metedor = $("#QCReport_Metedor").val();
    if (metedor === "") {
        errorR++;
        $('#QCReport_Metedor').css('border', '2px solid #e03f3f');
    }
    else {
        $('#QCReport_Metedor').css('border', '');
    }

    var inspector = $("#QCReport_QCInspector").val();
    if (inspector === "") {
        errorR++;
        $('#QCReport_QCInspector').css('border', '2px solid #e03f3f');
    }
    else {
        $('#QCReport_QCInspector').css('border', '');
    }

    enviarReporteActualizdo(cadena, errorR);

}


function enviarReporteActualizdo(cadena, errorR) {

    if (errorR !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {

        var fecha = $("#QCReport_Fecha").val();
        var reporte = $("#QCReport_ReporteG").val();
        var cachador = $("#QCReport_Cachador").val();
        var metedor = $("#QCReport_Metedor").val();
        var inspector = $("#QCReport_QCInspector").val();
        var maquina = $("#QCReport_Maquinas option:selected").val();
        var aql = $("#QCReport_AQL option:selected").val();
        var sacador = $("#QCReport_Sacador").val();
        var idSummary = $("#InfoSummary_IdItems").val();
        var turno = $("#QCReport_Turnos option:selected").val();
        var idReporte = $("#QCReport_IdQCReport").val();

        $.ajax({
            url: "/QCReport/Actualizar_Reporte_General",
            datatType: 'json',
            data: JSON.stringify({
                ReporteG: reporte, Sacador: sacador, Cachador: cachador, Metedor: metedor, QCInspector: inspector, DatoAQL: aql,
                Turno: turno, Fecha: fecha, IdMaquina: maquina, IdSummary: idSummary, IdQCReport: idReporte, ListTalla: cadena
            }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The Report were correctly registered.', 'success', 5, null);

            }
        });
    }

}

$(document).on("input", ".numeric", function () {
    this.value = this.value.replace(/\D/g, '');
});

$(document).on("input", ".number", function () {
    this.value = this.value.replace(/\D/g, '');
});

function buscar_estilos_Reporte(ID) {
    var tempScrollTop = $(window).scrollTop();
    $.ajax({
        url: "/Pedidos/Lista_Estilos_PO/" + ID,
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var html = '';
            var lista_estilo = jsonData.Data.listaItem;

            $.each(lista_estilo, function (key, item) {
                html += '<tr  onclick="probar()">';
                html += '<td>' + item.IdItems + '</td>';
                cliente = item.NumCliente;
                if (item.NumCliente === "2" || item.NumCliente === 2) {
                    var poF = item.POFantasy === null ? "-" : item.POFantasy;
                    html += '<td>' + poF + '</td>';
                }
                html += '<td>' + item.EstiloItem + '</td>';
                html += '<td>' + item.ItemDescripcion.Descripcion + '</td>';
                html += '<td>' + item.CatColores.CodigoColor + '</td>';
                html += '<td>' + item.CatColores.DescripcionColor + '</td>';
                html += '<td>' + item.Cantidad + '</td>';
                html += '<td>' + item.Price + '</td>';
                html += '<td>' + item.Total + '</td>';
                html += '<td><a href="#" onclick="obtener_Tallas_Reporte(' + item.IdItems + ',\'' + item.EstiloItem + '\');" class = "btn edit_driver edicion_driver"  Title = "Report"> <span class="glyphicon glyphicon-check" aria-hidden="true" style="padding: 0px !important;"></span></a></td>';

                // html += '<td><a href="#" onclick="obtener_tallas_item(' + item.IdItems + ');" class = "btn btn-default glyphicon glyphicon-search l1s" style = "color:black; padding:0px 5px 0px 5px;" Title = "Sizes"></a></td>';
                html += '</tr>';
            });
            if (Object.keys(lista_estilo).length === 0) {
                html += '<tr class="odd"><td valign="middle" align="center" colspan="10" class="dataTables_empty">No styles were found for the PO.</td></tr>';

            }
            $('.tbodyQC').html(html);
            $("#div_estilos_orden").css("visibility", "visible");
            $(window).scrollTop(tempScrollTop);
        },
        error: function (errormessage) { alert(errormessage.responseText); }
    });
}
var listaPO;
var DescEstilo = "";

function obtener_Tallas_Reporte(IdEstilo, nombreEstilo) {
    var tempScrollTop = $(window).scrollTop();
    DescEstilo = nombreEstilo;
    $("#panelReport").css('display', 'inline');
    $("#loading").css('display', 'inline');
    $("#modificarReportT1").hide();
    $("#InfoSummary_IdItems").val(IdEstilo);
    obtenerTablaMisprints(IdEstilo);
    $("#datosReporte").css("visibility", "visible");
    $("#loading").css('display', 'none');


}
var detallesQC = 0;
$(function () {
    $('#QCReport_Turnos').change(function () {
        $("#contenedorReporte").css("display", "none");
        var selectedText = $(this).find("option:selected").text();
        var selectedValue = $(this).val();
        var html = '';
        var estiloId = $("#InfoSummary_IdItems").val();
        var turno = $("#QCReport_Turnos option:selected").val();
        if (turno !== "0") {

            cargarReporte(estiloId, turno);

            //$("#regAssort").show();
        } else {
            $("#contenedorReporte").css("display", "none");
        }

    });

    $('#QCReport_TurnosQC').change(function () {
        $("#contenedorReporte").css("display", "none");
        var selectedText = $(this).find("option:selected").text();
        var selectedValue = $(this).val();
        var html = '';
        var estiloId = $("#InfoSummary_IdItems").val();
        var turno = $("#QCReport_TurnosQC option:selected").val();
        if (turno !== "0") {

            cargarDetalleReporte(estiloId, turno);

            //$("#regAssort").show();
        } else {
            $("#contenedorReporte").css("display", "none");
        }

    });
});

function formatJSONDate(jsonDate) {
    var newDate = dateFormat(jsonDate, "mm/dd/yy");
    return newDate;
}

function cargarReporte(estiloId, turno) {
    var actionData = "{'id':'" + estiloId + "','turno':'" + turno + "'}";
    $.ajax({
        url: "/QCReport/Obtener_Datos_Reporte/",
        type: "POST",
        data: actionData,
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var reporte = jsonData.Data.datos;
            if (reporte.ReporteG !== null) {
                var fechas = new Date(parseInt(reporte.Fecha.substr(6)));
                $("#QCReport_IdQCReport").val(reporte.IdQCReport);
                $("#QCReport_Fecha").val(fechas.format("mm/dd/yyyy"));
                $("#QCReport_ReporteG").val(reporte.ReporteG);
                $("#QCReport_Cachador").val(reporte.Cachador);
                $("#QCReport_Metedor").val(reporte.Metedor);
                $("#QCReport_QCInspector").val(reporte.QCInspector);
                $("#QCReport_Maquinas").val(reporte.IdMaquina).change();
                var aql;
                if (reporte.AQL === false) {
                    aql = "false";
                } else {
                    aql = "true";
                }
                $("#QCReport_AQL").val(aql).change();
                $("#QCReport_Sacador").val(reporte.Sacador);
                $("#contenedorReporte").css("display", "inline");
                $("#nuevoReporteT1").hide();
                $("#modificarReportT1").show();
                obtener_tabla_Lavado(estiloId, reporte.IdQCReport);
            } else {
                var fechas2 = new Date();
                $("#QCReport_IdQCReport").val(0);
                $("#QCReport_Fecha").val(fechas2.format("mm/dd/yyyy"));
                $("#QCReport_ReporteG").val(reporte.ReporteG);
                $("#QCReport_Cachador").val(reporte.Cachador);
                $("#QCReport_Metedor").val(reporte.Metedor);
                $("#QCReport_QCInspector").val(reporte.QCInspector);
                $("#QCReport_Maquinas").val("0").change();
                $("#QCReport_AQL").val("true").change();
                $("#QCReport_Sacador").val(reporte.Sacador);
                var id = $("#QCReport_IdQCReport").val();
                obtener_tabla_Lavado(estiloId, id);
                $("#nuevoReporteT1").show();
                $("#modificarReportT1").hide();
                $("#contenedorReporte").css("display", "inline");
            }
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {

    });

}

function cargarDetalleReporte(estiloId, turno) {
    var actionData = "{'id':'" + estiloId + "','turno':'" + turno + "'}";
    $.ajax({
        url: "/QCReport/Obtener_Datos_Reporte/",
        type: "POST",
        data: actionData,
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var reporte = jsonData.Data.datos;
            if (reporte.ReporteG !== null) {
                var fechas = new Date(parseInt(reporte.Fecha.substr(6)));
                $("#QCReport_IdQCReport").val(reporte.IdQCReport);
                $("#QCReport_Fecha").val(fechas.format("mm/dd/yyyy"));
                $("#QCReport_ReporteG").val(reporte.ReporteG);
                $("#QCReport_Cachador").val(reporte.Cachador);
                $("#QCReport_Metedor").val(reporte.Metedor);
                $("#QCReport_QCInspector").val(reporte.QCInspector);
                $("#QCReport_Maquinas").val(reporte.IdMaquina).change();
                var aql;
                if (reporte.AQL === false) {
                    aql = "false";
                } else {
                    aql = "true";
                }
                $("#QCReport_AQL").val(aql).change();
                $("#QCReport_Sacador").val(reporte.Sacador);
                $("#contenedorReporte").css("display", "inline");
                obtener_Detalle_tabla_Lavado(estiloId, reporte.IdQCReport);
            } else {
                var fechas2 = new Date();
                $("#QCReport_IdQCReport").val(0);
                $("#QCReport_Fecha").val(fechas2.format("mm/dd/yyyy"));
                $("#QCReport_ReporteG").val(reporte.ReporteG);
                $("#QCReport_Cachador").val(reporte.Cachador);
                $("#QCReport_Metedor").val(reporte.Metedor);
                $("#QCReport_QCInspector").val(reporte.QCInspector);
                $("#QCReport_Maquinas").val("0").change();
                $("#QCReport_AQL").val("true").change();
                $("#QCReport_Sacador").val(reporte.Sacador);
                var id = $("#QCReport_IdQCReport").val();
                obtener_Detalle_tabla_Lavado(estiloId, id);
                $("#contenedorReporte").css("display", "inline");
            }


        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {

    });

}

function obtener_tabla_Lavado(IdEstilo, idReporte) {
    var actionData = "{'id':'" + IdEstilo + "','idReporte':'" + idReporte + "'}";
    $.ajax({
        url: "/QCReport/Lista_Tallas_Estilo/",
        type: "POST",
        data: actionData,
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var html = '';
            var lista_estilo_Desc = jsonData.Data.listaTalla;
            var lista_prueba_lavado = jsonData.Data.listaPrueba;
            var listaVacia = 0;
            $.each(lista_prueba_lavado, function (key, item) {
                listaVacia++;
            });

            if (listaVacia !== 0) {
                $("#tablaPruebaLavadoModif").show();
                $.each(lista_prueba_lavado, function (key, itemL) {
                    var d = new Date(itemL.Fecha);
                    //var hora = new Date(parseInt(itemL.HoraLavado.substr(6)));                   
                    html += '<tr>';
                    html += '<td align="center" id="po"> <input type="text" name="id" id="id" class="txtDes form-control" style="width:30px;" value="' + itemL.IdQCPruebasLavados + '" /></td>';
                    html += '<td align="center"> <input type="time" name="fechasp" id="fechasp" class="form-control" style="width:120px;" value="' + d.format("HH:MM") + '" /></td>';
                    html += '<td align="center" id="po"><input type="text" id="po" class="txtDes form-control cantPO"  value="' + itemL.Talla + '" style=" background-color:transparent; border: 0; box-shadow: none;" readonly/></td>';
                    var result;
                    if (itemL.Results === 0) {
                        result = "0";
                        html += '<td align="center" id="po"><select id="selectPL" class="form-control">' +
                            '<option value="' + result + '" selected="selected">N/A</option>' +
                            '<option value="1">Approved</option>' +
                            '<option value="2">Not approved</option>' +
                            '</select></td>';
                    } else if (itemL.Results === 1) {
                        result = "1";
                        html += '<td align="center" id="po"><select id="selectPL" class="form-control">' +
                            '<option value="0">N/A</option>' +
                            '<option value="' + result + '" selected="selected">Approved</option>' +
                            '<option value="2">Not approved</option>' +
                            '</select></td>';
                    } else {
                        result = "2";
                        html += '<td align="center" id="po"><select id="selectPL" class="form-control">' +
                            '<option value="0">N/A</option>' +
                            '<option value="1">Approved</option>' +
                            '<option value="' + result + '" selected="selected">Not approved</option>' +
                            '</select></td>';
                    }

                    html += '</tr>';
                });

                if (Object.keys(lista_estilo_Desc).length === 0) {
                    html += '<tr class="odd"><td valign="middle" align="center" colspan="10" class="dataTables_empty">No sizes were found for the style.</td></tr>';
                }
                $('.tbodyPruebaLavadoModif').html(html);

                $("#tablaPruebaLavado").hide();

            } else {
                $.each(lista_estilo_Desc, function (key, item) {
                    html += '<tr>';
                    html += '<td align="center"> <input type="time" name="fechasp" id="fechasp" class="form-control" style="width:120px;" value="00:00" /></td>';
                    html += '<td align="center" id="po"><input type="text" id="po" class="txtDes form-control cantPO"  value="' + item.Talla + '" style=" background-color:transparent; border: 0; box-shadow: none;" readonly/></td>';
                    html += '<td align="center" id="po"><select id="selectPL" class="form-control">' +
                        '<option value="0" selected="selected">N/A</option>' +
                        '<option value="1">Approved</option>' +
                        '<option value="2">Not approved</option>' +
                        '</select></td>';
                    html += '</tr>';
                });

                if (Object.keys(lista_estilo_Desc).length === 0) {
                    html += '<tr class="odd"><td valign="middle" align="center" colspan="10" class="dataTables_empty">No sizes were found for the style.</td></tr>';
                }
                $('.tbodyPruebaLavado').html(html);
                $("#tablaPruebaLavadoModif").hide();
                $("#tablaPruebaLavado").show();
            }

            $("#datosReporte").css("visibility", "visible");
            $("#div_estilo").css("visibility", "visible");
            var dt = $("#InfoSummary_IdItems").val();


            $("#loading").css('display', 'none');

        },
        error: function (errormessage) { alert(errormessage.responseText); }
    });
}


function obtener_Detalle_tabla_Lavado(IdEstilo, idReporte) {
    var actionData = "{'id':'" + IdEstilo + "','idReporte':'" + idReporte + "'}";
    $.ajax({
        url: "/QCReport/Lista_Tallas_Estilo/",
        type: "POST",
        data: actionData,
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var html = '';
            var lista_estilo_Desc = jsonData.Data.listaTalla;
            var lista_prueba_lavado = jsonData.Data.listaPrueba;
            var listaVacia = 0;
            $.each(lista_prueba_lavado, function (key, item) {
                listaVacia++;
            });

            if (listaVacia !== 0) {
                $("#tablaPruebaLavadoModif").show();
                $.each(lista_prueba_lavado, function (key, itemL) {
                    var d = new Date(itemL.Fecha);
                    //var hora = new Date(parseInt(itemL.HoraLavado.substr(6)));                   
                    html += '<tr>';
                    html += '<td align="center" id="po"> <input type="text" name="id" id="id" class="txtDes form-control" style="width:50px; background-color:transparent; border: 0; box-shadow: none;" value="' + itemL.IdQCPruebasLavados + '" readonly/></td>';
                    html += '<td align="center"> <input type="time" name="fechasp" id="fechasp" class="form-control" style="width:120px; background-color:transparent; border: 0; box-shadow: none;" value="' + d.format("HH:MM") + '" readonly/></td>';
                    html += '<td align="center" id="po"><input type="text" id="po" class="txtDes form-control cantPO"  value="' + itemL.Talla + '" style=" background-color:transparent; border: 0; box-shadow: none;" readonly/></td>';
                    var result;
                    if (itemL.Results === 0) {
                        result = "0";
                        html += '<td align="center" id="po"><select id="selectPL" class="form-control" style=" background-color:transparent; border: 0; box-shadow: none;" disabled="disabled">' +
                            '<option value="' + result + '" selected="selected">N/A</option>' +
                            '<option value="1">Approved</option>' +
                            '<option value="2">Not approved</option>' +
                            '</select></td>';
                    } else if (itemL.Results === 1) {
                        result = "1";
                        html += '<td align="center" id="po"><select id="selectPL" class="form-control" style=" background-color:transparent; border: 0; box-shadow: none;" disabled="disabled">' +
                            '<option value="0">N/A</option>' +
                            '<option value="' + result + '" selected="selected">Approved</option>' +
                            '<option value="2">Not approved</option>' +
                            '</select></td>';
                    } else {
                        result = "2";
                        html += '<td align="center" id="po"><select id="selectPL" class="form-control" style=" background-color:transparent; border: 0; box-shadow: none;" disabled="disabled">' +
                            '<option value="0">N/A</option>' +
                            '<option value="1">Approved</option>' +
                            '<option value="' + result + '" selected="selected">Not approved</option>' +
                            '</select></td>';
                    }

                    html += '</tr>';
                });

                if (Object.keys(lista_estilo_Desc).length === 0) {
                    html += '<tr class="odd"><td valign="middle" align="center" colspan="10" class="dataTables_empty">No sizes were found for the style.</td></tr>';
                }
                $('.tbodyPruebaLavadoModif').html(html);

                $("#tablaPruebaLavado").hide();

            } else {
                $.each(lista_estilo_Desc, function (key, item) {
                    html += '<tr>';
                    html += '<td align="center"> <input type="time" name="fechasp" id="fechasp" class="form-control" style="width:120px;" value="00:00" readonly/></td>';
                    html += '<td align="center" id="po"><input type="text" id="po" class="txtDes form-control cantPO"  value="' + item.Talla + '" style=" background-color:transparent; border: 0; box-shadow: none;" readonly/></td>';
                    html += '<td align="center" id="po"><select id="selectPL" class="form-control" style=" background-color:transparent; border: 0; box-shadow: none;" disabled="disabled">' +
                        '<option value="0" selected="selected">N/A</option>' +
                        '<option value="1">Approved</option>' +
                        '<option value="2">Not approved</option>' +
                        '</select></td>';
                    html += '</tr>';
                });

                if (Object.keys(lista_estilo_Desc).length === 0) {
                    html += '<tr class="odd"><td valign="middle" align="center" colspan="10" class="dataTables_empty">No sizes were found for the style.</td></tr>';
                }
                $('.tbodyPruebaLavado').html(html);
                $("#tablaPruebaLavadoModif").hide();
                $("#tablaPruebaLavado").show();
            }

            $("#datosReporte").css("visibility", "visible");
            $("#div_estilo").css("visibility", "visible");
            var dt = $("#InfoSummary_IdItems").val();


            $("#loading").css('display', 'none');

        },
        error: function (errormessage) { alert(errormessage.responseText); }
    });
}

function obtenerTablaMisprints(idEstilo) {
    $("#loading").css('display', 'none');
    $.ajax({
        url: "/QCReport/Lista_Tallas_MisPrints/" + idEstilo,
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var html = '';

            var lista_extras = jsonData.Data.listExtras;
            var lista_Datos_MP = jsonData.Data.datosMP;

            var listaVacia = 0;
            $.each(lista_Datos_MP, function (key, item) {
                listaVacia++;
            });
            if (listaVacia === 0) {
                $("#nuevoRegistroMP").show();
                $("#modificarMP").hide();
                html += '<tr> <td> SIZE </td>';
                var lista_estilo_Tallas = jsonData.Data.listaTalla;
                $.each(lista_estilo_Tallas, function (key, item) {
                    html += '<td><input type="text" id="talla" class=" txtDes form-control talla" value="' + item.Talla + '"/></td>';
                });
                var cadena_cantidades_extras = "";
                var contadorExtras = 0;
                html += '</tr><tr><td>MisPrint CPO</td>';
                $.each(lista_extras, function (key, item) {
                    html += '<td id="ext"><input type="text" id="ext' + contadorExtras + '" class=" txtDes form-control extras" value="' + item.Extras + '" readonly/></td>';
                    cadena_cantidades_extras += "*" + item.Extras;
                    contadorExtras++;
                });
                var cantidades_arrayExtras = cadena_cantidades_extras.split('*');

                var contadorMP1 = 0;
                html += '</tr><tr><td>MisPrint 1st</td>';
                $.each(lista_estilo_Tallas, function (key, item) {
                    html += '<td class="printedmp1"><input type="text" id="mp1' + contadorMP1 + '" onfocus="focusingMP1(' + contadorMP1 + ')" onChange="calcular_MisPrint_QC(' + contadorMP1 + ')" class=" form-control number cmp1" value="' + 0 + '"/></td>';
                    contadorMP1++;
                });
                var contadorMP2 = 0;
                html += '</tr><tr><td>MisPrint 2nd</td>';
                $.each(lista_estilo_Tallas, function (key, item) {
                    html += '<td class="printedmp2"><input type="text" id="mp2' + contadorMP2 + '" onfocus="focusingMP2(' + contadorMP2 + ')" onChange="calcular_MisPrint_QC(' + contadorMP2 + ')" class=" form-control number cmp2" value="' + 0 + '"/></td>';
                    contadorMP2++;
                });
                html += '</tr><tr ><td class="total">+/-</td>';
                var i = 1;
                var contadorTMP = 0;
                $.each(lista_estilo_Tallas, function (key, item) {
                    var resta = parseFloat(cantidades_arrayExtras[i]);
                    if (resta === 0) {
                        html += '<td ><input type="text" id="totalMP' + contadorTMP + '" class=" txtDes form-control ctotalMP" style="color:black; background-color:transparent; border: 0; box-shadow: none;" value="' + resta + '" readonly/></td>';
                    } else if (resta >= 0) {
                        html += '<td ><input type="text"  id="totalMP' + contadorTMP + '" class=" txtDes form-control ctotalMP" style="color:blue; background-color:transparent; border: 0; box-shadow: none;" value="' + resta + '" readonly/></td>';
                    } else {
                        html += '<td ><input type="text"  id="totalMP' + contadorTMP + '" class=" txtDes form-control ctotalMP" style="color:red; background-color:transparent; border: 0; box-shadow: none;" value="' + resta + '" readonly/></td>';
                    }
                    i++;
                    contadorTMP++;
                });
                var contadorR1 = 0;
                html += '</tr><tr><td>Repairs 1st</td>';
                $.each(lista_estilo_Tallas, function (key, item) {
                    html += '<td class="printedrp1"><input type="text" id="rep1' + contadorR1 + '" onfocus="focusingR1(' + contadorR1 + ')" class=" form-control number rep1" value="' + 0 + '"/></td>';
                    contadorR1++;
                });
                var contadorR2 = 0;
                html += '</tr><tr><td>Repairs 2nd</td>';
                $.each(lista_estilo_Tallas, function (key, item) {
                    html += '<td class="printedrp2"><input type="text" id="rep2' + contadorR2 + '" onfocus="focusingR2(' + contadorR2 + ')" class=" form-control number rep2" value="' + 0 + '"/></td>';
                    contadorR2++;
                });
                var contadorSP1 = 0;
                html += '</tr><tr><td>Sprayed 1st</td>';
                $.each(lista_estilo_Tallas, function (key, item) {
                    html += '<td class="printedsp1"><input type="text" id="spra1' + contadorSP1 + '" onfocus="focusingSP1(' + contadorSP1 + ')" class=" form-control number spra1" value="' + 0 + '"/></td>';
                    contadorSP1++;
                });
                var contadorSP2 = 0;
                html += '</tr><tr><td>Sprayed 2nd</td>';
                $.each(lista_estilo_Tallas, function (key, item) {
                    html += '<td class="printedsp2"><input type="text" id="spra2' + contadorSP2 + '" onfocus="focusingSP2(' + contadorSP2 + ')" class=" form-control number spra2" value="' + 0 + '"/></td>';
                    contadorSP2++;
                });
                var contadorD1 = 0;
                html += '</tr><tr><td>Defects 1st</td>';
                $.each(lista_estilo_Tallas, function (key, item) {
                    html += '<td class="printeddef1"><input type="text" id="def1' + contadorD1 + '" onfocus="focusingD1(' + contadorD1 + ')" class="txtes form-control number def1" value="' + 0 + '"/></td>';
                    contadorD1++;
                });
                var contadorD2 = 0;
                html += '</tr><tr><td>Defects 2nd</td>';
                $.each(lista_estilo_Tallas, function (key, item) {
                    html += '<td class="printeddef2"><input type="text" id="def2' + contadorD2 + '" onfocus="focusingD2(' + contadorD2 + ')" class="txt form-control number def2" value="' + 0 + '"/></td>';
                    contadorD2++;
                });
                html += '</tr>';

                $('.tbodyMisp').html(html);
            } else {
                $("#nuevoRegistroMP").hide();
                $("#modificarMP").show();
                var fechasReg;
                $.each(lista_Datos_MP, function (key, item) {
                    fechasReg = new Date(parseInt(item.FechaRegistro.substr(6)));
                });
                $("#QCReport_Fecha2").val(fechasReg.format("mm/dd/yyyy"));
                html += '<tr> <td> SIZE </td>';
                var lista_Tallas = jsonData.Data.listaTalla;
                $.each(lista_Tallas, function (key, item) {
                    html += '<td><input type="text" id="talla" class=" txtDes form-control talla" value="' + item.Talla + '"/></td>';
                });
                var cadena_cantidades_extras2 = "";
                var contadorExtras2 = 0;
                html += '</tr><tr><td>MisPrint CPO</td>';
                $.each(lista_Tallas, function (key, item) {
                    html += '<td id="ext"><input type="text" id="ext' + contadorExtras2 + '" class=" txtDes form-control extras" value="' + item.Extras + '" readonly/></td>';
                    cadena_cantidades_extras2 += "*" + item.Extras;
                    contadorExtras2++;
                });
                var cantidades_arrayExtras2 = cadena_cantidades_extras2.split('*');
                var contMP1 = 0;
                var cadena_cantidades_misp1 = "";
                html += '</tr><tr><td>MisPrint 1st</td>';
                $.each(lista_Datos_MP, function (key, item) {
                    html += '<td class="printedmp1"><input type="text" id="mp1' + contMP1 + '" onfocus="focusingMP1(' + contMP1 + ')"  onChange="calcular_MisPrint_QC(' + contMP1 + ')" class=" form-control number cmp1" value="' + item.MisPrint1st + '"/></td>';
                    contMP1++;
                    cadena_cantidades_misp1 += "*" + item.MisPrint1st;
                });
                var cantidades_arrayMisp1 = cadena_cantidades_misp1.split('*');
                var contMP2 = 0;
                var cadena_cantidades_misp2 = "";
                html += '</tr><tr><td>MisPrint 2nd</td>';
                $.each(lista_Datos_MP, function (key, item) {
                    html += '<td class="printedmp2"><input type="text" id="mp2' + contMP2 + '" onfocus="focusingMP2(' + contMP2 + ')" onChange="calcular_MisPrint_QC(' + contMP2 + ')" class=" form-control number cmp2" value="' + item.MisPrint2nd + '"/></td>';
                    contMP2++;
                    cadena_cantidades_misp2 += "*" + item.MisPrint2nd;
                });
                var cantidades_arrayMisp2 = cadena_cantidades_misp2.split('*');
                var x = 1;
                var contTMP = 0;
                html += '</tr><tr ><td class="total">+/-</td>';
                $.each(lista_Datos_MP, function (key, item) {
                    var suma2 = parseFloat(cantidades_arrayMisp1[x]) + parseFloat(cantidades_arrayMisp2[x]);
                    var resta2 = parseFloat(cantidades_arrayExtras2[x]) - suma2;
                    if (resta2 === 0) {
                        html += '<td ><input type="text" id="totalMP' + contTMP + '" class=" txtDes form-control ctotalMP" style="color:black;" value="' + resta2 + '" readonly/></td>';
                    } else if (resta2 >= 0) {
                        html += '<td ><input type="text" id="totalMP' + contTMP + '" class=" txtDes form-control ctotalMP" style="color:blue;" value="' + resta2 + '" readonly/></td>';
                    } else {
                        html += '<td ><input type="text" id="totalMP' + contTMP + '" class=" txtDes form-control ctotalMP" style="color:red;" value="' + resta2 + '" readonly/></td>';
                    }
                    x++;
                    contTMP++;
                });
                var contR1 = 0;
                html += '</tr><tr><td>Repairs 1st</td>';
                $.each(lista_Datos_MP, function (key, item) {
                    html += '<td class="printedrp1"><input type="text" id="rep1' + contR1 + '" onfocus="focusingR1(' + contR1 + ')" class=" form-control number rep1" value="' + item.Repairs1st + '"/></td>';
                    contR1++;
                });
                var contR2 = 0;
                html += '</tr><tr><td>Repairs 2nd</td>';
                $.each(lista_Datos_MP, function (key, item) {
                    html += '<td class="printedrp2"><input type="text" id="rep2' + contR2 + '" onfocus="focusingR2(' + contR2 + ')" class=" form-control number rep2" value="' + item.Repairs2nd + '"/></td>';
                    contR2++;
                });
                var contSP1 = 0;
                html += '</tr><tr><td>Sprayed 1st</td>';
                $.each(lista_Datos_MP, function (key, item) {
                    html += '<td class="printedsp1"><input type="text" id="spra1' + contSP1 + '" onfocus="focusingSP1(' + contSP1 + ')" class=" form-control number spra1" value="' + item.Sprayed1st + '"/></td>';
                    contSP1++;
                });
                var contSP2 = 0;
                html += '</tr><tr><td>Sprayed 2nd</td>';
                $.each(lista_Datos_MP, function (key, item) {
                    html += '<td class="printedsp2"><input type="text" id="spra2' + contSP2 + '" onfocus="focusingSP2(' + contSP2 + ')" class=" form-control number spra2" value="' + item.Sprayed2nd + '"/></td>';
                    contSP2++;
                });
                var contD1 = 0;
                html += '</tr><tr><td>Defects 1st</td>';
                $.each(lista_Datos_MP, function (key, item) {
                    html += '<td class="printeddef1"><input type="text" id="def1' + contD1 + '" onfocus="focusingD1(' + contD1 + ')"  class="txtes form-control number def1" value="' + item.Defects1st + '"/></td>';
                    contD1++;
                });
                var contD2 = 0;
                html += '</tr><tr><td>Defects 2nd</td>';
                $.each(lista_Datos_MP, function (key, item) {
                    html += '<td class="printeddef2"><input type="text" id="def2' + contD2 + '" onfocus="focusingD2(' + contD2 + ')" class="txt form-control number def2" value="' + item.Defects2nd + '"/></td>';
                    contD2++;
                });
                html += '</tr>';

                $('.tbodyMisp').html(html);
            }
        },
        error: function (errormessage) { alert(errormessage.responseText); }
    });

}

function registrarTablaMisprint() {
    var nColumnas = $("#tablaMisprints tr:last td").length;

    var r = 0; var c = 0; var i = 0; var cadena = new Array(nColumnas - 1);
    for (var x = 0; x < nColumnas - 1; x++) {
        cadena[x] = '';
    }
    var nFilas = $("#tablaMisprints tbody>tr").length;
    r = 0;
    $('#tablaMisprints tbody>tr').each(function () {
        if (r >= 0) {
            c = 0;
            $(this).find("input").each(function () {
                $(this).closest('td').find("input").each(function () {
                    cadena[c] += this.value + "*";
                    c++;
                });


            });
        }
        r++;
    });
    var error = 0;
    $('#tablaMisprints').find('td.printedmp1').each(function (i, el) {
        var valor = $(el).children().val();
        if (valor === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '');
        }
    });

    $('#tablaMisprints').find('td.printedmp2').each(function (i, el) {
        var valor = $(el).children().val();
        if (valor === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '');
        }
    });

    $('#tablaMisprints').find('td.printedrp1').each(function (i, el) {
        var valor = $(el).children().val();
        if (valor === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '');
        }
    });

    $('#tablaMisprints').find('td.printedrp2').each(function (i, el) {
        var valor = $(el).children().val();
        if (valor === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '');
        }
    });

    $('#tablaMisprints').find('td.printedsp1').each(function (i, el) {
        var valor = $(el).children().val();
        if (valor === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '');
        }
    });

    $('#tablaMisprints').find('td.printedsp2').each(function (i, el) {
        var valor = $(el).children().val();
        if (valor === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '');
        }
    });

    $('#tablaMisprints').find('td.printeddef1').each(function (i, el) {
        var valor = $(el).children().val();
        if (valor === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '');
        }
    });

    $('#tablaMisprints').find('td.printeddef2').each(function (i, el) {
        var valor = $(el).children().val();
        if (valor === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '');
        }
    });

    var fecha = $("#QCReport_Fecha2").val();
    if (fecha === "") {
        error++;
        $('#QCReport_Fecha2').css('border', '2px solid #e03f3f');
    }
    else {
        $('#QCReport_Fecha2').css('border', '');
    }

    enviarListaTablaMisPrint(cadena, error);
}

function enviarListaTablaMisPrint(cadena, error) {
    var fecha = $("#QCReport_Fecha2").val();
    var estiloId = $("#InfoSummary_IdItems").val();
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {
        $.ajax({
            url: "/QCReport/Obtener_Tabla_MisPrints",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena, IdSummary: estiloId, FechaRegistro: fecha }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The MisPrint was correctly registered.', 'success', 5, null);
                $('.number').val('0');
                obtener_tallas_item(estiloId);
            }
        });
    }
}

function actualizarTablaMisprint() {
    var nColumnas = $("#tablaMisprints tr:last td").length;

    var r = 0; var c = 0; var i = 0; var cadena = new Array(nColumnas - 1);
    for (var x = 0; x < nColumnas - 1; x++) {
        cadena[x] = '';
    }
    var nFilas = $("#tablaMisprints tbody>tr").length;
    r = 0;
    $('#tablaMisprints tbody>tr').each(function () {
        if (r >= 0) {
            c = 0;
            $(this).find("input").each(function () {
                $(this).closest('td').find("input").each(function () {
                    cadena[c] += this.value + "*";
                    c++;
                });


            });
        }
        r++;
    });
    var error = 0;
    $('#tablaMisprints').find('td.printedmp1').each(function (i, el) {
        var valor = $(el).children().val();
        if (valor === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '');
        }
    });

    $('#tablaMisprints').find('td.printedmp2').each(function (i, el) {
        var valor = $(el).children().val();
        if (valor === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '');
        }
    });

    $('#tablaMisprints').find('td.printedrp1').each(function (i, el) {
        var valor = $(el).children().val();
        if (valor === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '');
        }
    });

    $('#tablaMisprints').find('td.printedrp2').each(function (i, el) {
        var valor = $(el).children().val();
        if (valor === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '');
        }
    });

    $('#tablaMisprints').find('td.printedsp1').each(function (i, el) {
        var valor = $(el).children().val();
        if (valor === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '');
        }
    });

    $('#tablaMisprints').find('td.printedsp2').each(function (i, el) {
        var valor = $(el).children().val();
        if (valor === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '');
        }
    });

    $('#tablaMisprints').find('td.printeddef1').each(function (i, el) {
        var valor = $(el).children().val();
        if (valor === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '');
        }
    });

    $('#tablaMisprints').find('td.printeddef2').each(function (i, el) {
        var valor = $(el).children().val();
        if (valor === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '');
        }
    });

    var fecha = $("#QCReport_Fecha2").val();
    if (fecha === "") {
        error++;
        $('#QCReport_Fecha2').css('border', '2px solid #e03f3f');
    }
    else {
        $('#QCReport_Fecha2').css('border', '');
    }


    enviarListaTablaMisPrintActualizar(cadena, error);
}

function enviarListaTablaMisPrintActualizar(cadena, error) {
    var fecha = $("#QCReport_Fecha2").val();
    var estiloId = $("#InfoSummary_IdItems").val();
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {
        $.ajax({
            url: "/QCReport/Actualizar_Tabla_MisPrints",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena, IdSummary: estiloId, FechaRegistro: fecha }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The MisPrints was correctly modified.', 'success', 5, null);
                $('.number').val('0');
                obtener_tallas_item(estiloId);
            }
        });
    }
}

function obtenerTablaReporteMensual(idEstilo, fecha) {
    $("#loading").css('display', 'none');
    $("#contenedorTRM").css('display', 'inline');

    var actionData = "{'id':'" + idEstilo + "','estilo':'" + DescEstilo + "','fecha':'" + fecha + "'}";
    $.ajax({
        url: "/QCReport/Lista_Tallas_Reporte_Mensual/",
        type: "POST",
        data: actionData,
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var html = '';

            var lista_extras = jsonData.Data.listExtras;
            var lista_Datos_MP = jsonData.Data.datosMP;
            var lista_PrintShop = jsonData.Data.listaPrint;
            var lista_MisPrints1 = jsonData.Data.listaMisPrint1;
            var lista_MisPrints2 = jsonData.Data.listaMisPrint2;
            var lista_Repairs1 = jsonData.Data.listaRepairs1;
            var lista_Repairs2 = jsonData.Data.listaRepairs2;
            var lista_Sprayed1 = jsonData.Data.listaSprayed1;
            var lista_Sprayed2 = jsonData.Data.listaSprayed2;
            var lista_Defects1 = jsonData.Data.listaDefects1;
            var lista_Defects2 = jsonData.Data.listaDefects2;

            var listaVacia = 0;
            $.each(lista_Datos_MP, function (key, item) {
                listaVacia++;
            });
            var listaVaciaPrintShop = 0;
            $.each(lista_PrintShop, function (key, item) {
                listaVaciaPrintShop++;
            });
            if (listaVacia !== 0) {
                $("#nuevoRegistroMP").hide();
                $("#modificarMP").show();
                var fechasReg;
                $.each(lista_Datos_MP, function (key, item) {
                    fechasReg = new Date(parseInt(item.FechaRegistro.substr(6)));
                });
                //$("#QCReport_FechaRM").val(fechasReg.format("mm yyyy"));
                html += '<tr> <td> SIZE </td>';
                var lista_Tallas = jsonData.Data.listaTalla;
                $.each(lista_Tallas, function (key, item) {
                    html += '<td><input type="text" id="talla" class=" txtDes form-control talla" value="' + item.Talla + '"/></td>';
                });
                html += '<th> TOTAL </th>';
                html += '</tr><tr><td>PrintShop</td>';
                var cantidadesPOTotal = 0;
                var cadena_cantidades_ps = "";
                var contadorPrintS = 0;
                $.each(lista_PrintShop, function (key, itemP) {
                    html += '<td id="printS"><input type="text" id="printSQty' + contadorPrintS + '" class=" txtDes form-control printSQty" style="background-color:transparent; border: 0; box-shadow: none;" value="' + itemP + '" readonly/></td>';
                    cadena_cantidades_ps += "*" + itemP;
                    cantidadesPOTotal += itemP;
                    contadorPrintS++;
                });
                html += '<td class="txtDes">' + cantidadesPOTotal + '</td>';
                var cantidades_arrayPS = cadena_cantidades_ps.split('*');
                var contMP1 = 0;
                var TotalcontMP1 = 0;
                var cadena_cantidades_misp1 = "";
                html += '</tr><tr><td>MisPrint 1st</td>';
                $.each(lista_MisPrints1, function (key, itemMP1) {
                    html += '<td class="printedmp1"><input type="text" id="mp1' + contMP1 + '" onfocus="focusingMP1(' + contMP1 + ')"  onChange="calcular_MisPrint_QC(' + contMP1 + ')" class="txtDes form-control number cmp1" style=" background-color:transparent; border: 0; box-shadow: none;" value="' + itemMP1 + '" readonly/></td>';
                    contMP1++;
                    cadena_cantidades_misp1 += "*" + itemMP1;
                    TotalcontMP1 += itemMP1;
                });
                html += '<td class="txtDes">' + TotalcontMP1 + '</td>';
                var cantidades_arrayMisp1 = cadena_cantidades_misp1.split('*');
                var contMP2 = 0;
                var TotalcontMP2 = 0;
                var cadena_cantidades_misp2 = "";
                html += '</tr><tr><td>MisPrint 2nd</td>';
                $.each(lista_MisPrints2, function (key, itemMP2) {
                    html += '<td class="printedmp2"><input type="text" id="mp2' + contMP2 + '" onfocus="focusingMP2(' + contMP2 + ')" onChange="calcular_MisPrint_QC(' + contMP2 + ')" class="txtDes form-control number cmp2" style=" background-color:transparent; border: 0; box-shadow: none;" value="' + itemMP2 + '" readonly/></td>';
                    contMP2++;
                    cadena_cantidades_misp2 += "*" + itemMP2;
                    TotalcontMP2 += itemMP2;
                });
                html += '<td class="txtDes">' + TotalcontMP2 + '</td>';
                var TotalcontR1 = 0;
                var contR1 = 0;
                html += '</tr><tr><td>Repairs 1st</td>';
                $.each(lista_Repairs1, function (key, itemR1) {
                    html += '<td class="printedrp1"><input type="text" id="rep1' + contR1 + '" onfocus="focusingR1(' + contR1 + ')" class="txtDes form-control number rep1" style=" background-color:transparent; border: 0; box-shadow: none;" value="' + itemR1 + '" readonly/></td>';
                    contR1++;
                    TotalcontR1 += itemR1;
                });
                html += '<td class="txtDes">' + TotalcontR1 + '</td>';
                var contR2 = 0;
                var TotalcontR2 = 0;
                html += '</tr><tr><td>Repairs 2nd</td>';
                $.each(lista_Repairs2, function (key, itemR2) {
                    html += '<td class="printedrp2"><input type="text" id="rep2' + contR2 + '" onfocus="focusingR2(' + contR2 + ')" class="txtDes form-control number rep2" style=" background-color:transparent; border: 0; box-shadow: none;" value="' + itemR2 + '" readonly/></td>';
                    contR2++;
                    TotalcontR2 += itemR2;
                });
                html += '<td>' + TotalcontR2 + '</td>';
                var contSP1 = 0;
                var TotalcontSP1 = 0;
                html += '</tr><tr><td>Sprayed 1st</td>';
                $.each(lista_Sprayed1, function (key, itemS1) {
                    html += '<td class="printedsp1"><input type="text" id="spra1' + contSP1 + '" onfocus="focusingSP1(' + contSP1 + ')" class="txtDes form-control number spra1" style=" background-color:transparent; border: 0; box-shadow: none;" value="' + itemS1 + '" readonly/></td>';
                    contSP1++;
                    TotalcontSP1 += itemS1;
                });
                html += '<td>' + TotalcontSP1 + '</td>';
                var contSP2 = 0;
                var TotalcontSP2 = 0;
                html += '</tr><tr><td>Sprayed 2nd</td>';
                $.each(lista_Sprayed2, function (key, itemS2) {
                    html += '<td class="printedsp2"><input type="text" id="spra2' + contSP2 + '" onfocus="focusingSP2(' + contSP2 + ')" class="txtDes form-control number spra2" style=" background-color:transparent; border: 0; box-shadow: none;" value="' + itemS2 + '" readonly/></td>';
                    contSP2++;
                    TotalcontSP2 += itemS2;
                });
                html += '<td>' + TotalcontSP2 + '</td>';
                var contD1 = 0;
                var TotalcontD1 = 0;
                html += '</tr><tr><td>Defects 1st</td>';
                $.each(lista_Defects1, function (key, itemD1) {
                    html += '<td class="printeddef1"><input type="text" id="def1' + contD1 + '" onfocus="focusingD1(' + contD1 + ')"  class="txtDes form-control number def1" style=" background-color:transparent; border: 0; box-shadow: none;" value="' + itemD1 + '" readonly/></td>';
                    contD1++;
                    TotalcontD1 += itemD1;
                });
                html += '<td>' + TotalcontD1 + '</td>';
                var contD2 = 0;
                var TotalcontD2 = 0;
                html += '</tr><tr><td>Defects 2nd</td>';
                $.each(lista_Defects2, function (key, itemD2) {
                    html += '<td class="printeddef2"><input type="text" id="def2' + contD2 + '" onfocus="focusingD2(' + contD2 + ')" class="txtDes form-control number def2" style=" background-color:transparent; border: 0; box-shadow: none;" value="' + itemD2 + '" readonly/></td>';
                    contD2++;
                    TotalcontD2 += itemD2;
                });
                html += '<td>' + TotalcontD2 + '</td>';
                html += '</tr>';

                $('.tbodyTRM').html(html);
            } else {
                $("#nuevoRegistroMP").hide();
                $("#modificarMP").show();
                var fechasReg2;
                $.each(lista_Datos_MP, function (key, item) {
                    fechasReg2 = new Date(parseInt(item.FechaRegistro.substr(6)));
                });
                //$("#QCReport_FechaRM").val(fechasReg.format("mm yyyy"));
                html += '<tr> <td> SIZE </td>';
                var lista_Tallas2 = jsonData.Data.listaTalla;
                $.each(lista_Tallas2, function (key, item) {
                    html += '<td><input type="text" id="talla" class=" txtDes form-control talla" value="' + item.Talla + '"/></td>';
                });
                html += '<th> TOTAL </th>';
                html += '</tr><tr><td>PrintShop</td>';
                var cantidadesPOTotal2 = 0;
                var cadena_cantidades_ps2 = "";
                var contadorPrintS2 = 0;
                if (listaVaciaPrintShop === 0) {
                    $.each(lista_Tallas2, function (key, itemP) {
                        html += '<td id="printS"><input type="text" id="printSQty' + contadorPrintS2 + '" class=" txtDes form-control printSQty" style="background-color:transparent; border: 0; box-shadow: none;" value="' + 0 + '" readonly/></td>';
                        //cadena_cantidades_ps2 += "*" + itemP;
                        cantidadesPOTotal2 += 0;
                        contadorPrintS2++;
                    });
                    html += '<td class="txtDes">' + cantidadesPOTotal2 + '</td>';
                } else {
                    $.each(lista_PrintShop, function (key, itemP) {
                        html += '<td id="printS"><input type="text" id="printSQty' + contadorPrintS2 + '" class=" txtDes form-control printSQty" style="background-color:transparent; border: 0; box-shadow: none;" value="' + itemP + '" readonly/></td>';
                        //cadena_cantidades_ps2 += "*" + itemP;
                        cantidadesPOTotal2 += itemP;
                        contadorPrintS2++;
                    });
                    html += '<td class="txtDes">' + cantidadesPOTotal2 + '</td>';
                }

                //var cantidades_arrayPS2 = cadena_cantidades_ps2.split('*');
                var contMP12 = 0;
                var TotalcontMP12 = 0;
                var cadena_cantidades_misp12 = "";
                html += '</tr><tr><td>MisPrint 1st</td>';
                $.each(lista_Tallas2, function (key, itemMP1) {
                    html += '<td class="printedmp1"><input type="text" id="mp1' + contMP12 + '" onfocus="focusingMP1(' + contMP12 + ')"  onChange="calcular_MisPrint_QC(' + contMP12 + ')" class="txtDes form-control number cmp1" style=" background-color:transparent; border: 0; box-shadow: none;" value="' + 0 + '" readonly/></td>';
                    contMP12++;
                    //cadena_cantidades_misp12 += "*" + 0;
                    TotalcontMP12 += 0;
                });
                html += '<td class="txtDes">' + TotalcontMP12 + '</td>';
                //	var cantidades_arrayMisp12 = cadena_cantidades_misp12.split('*');
                var contMP22 = 0;
                var TotalcontMP22 = 0;
                var cadena_cantidades_misp22 = "";
                html += '</tr><tr><td>MisPrint 2nd</td>';
                $.each(lista_Tallas2, function (key, itemMP2) {
                    html += '<td class="printedmp2"><input type="text" id="mp2' + contMP22 + '" onfocus="focusingMP2(' + contMP22 + ')" onChange="calcular_MisPrint_QC(' + contMP22 + ')" class="txtDes form-control number cmp2" style=" background-color:transparent; border: 0; box-shadow: none;" value="' + 0 + '" readonly/></td>';
                    contMP22++;
                    //cadena_cantidades_misp22 += "*" + 0;
                    TotalcontMP22 += 0;
                });
                html += '<td class="txtDes">' + TotalcontMP22 + '</td>';
                var TotalcontR12 = 0;
                var contR12 = 0;
                html += '</tr><tr><td>Repairs 1st</td>';
                $.each(lista_Tallas2, function (key, itemR1) {
                    html += '<td class="printedrp1"><input type="text" id="rep1' + contR12 + '" onfocus="focusingR1(' + contR12 + ')" class="txtDes form-control number rep1" style=" background-color:transparent; border: 0; box-shadow: none;" value="' + 0 + '" readonly/></td>';
                    contR12++;
                    TotalcontR12 += 0;
                });
                html += '<td class="txtDes">' + TotalcontR12 + '</td>';
                var contR22 = 0;
                var TotalcontR22 = 0;
                html += '</tr><tr><td>Repairs 2nd</td>';
                $.each(lista_Tallas2, function (key, itemR2) {
                    html += '<td class="printedrp2"><input type="text" id="rep2' + contR22 + '" onfocus="focusingR2(' + contR22 + ')" class="txtDes form-control number rep2" style=" background-color:transparent; border: 0; box-shadow: none;" value="' + 0 + '" readonly/></td>';
                    contR22++;
                    TotalcontR22 += 0;
                });
                html += '<td>' + TotalcontR22 + '</td>';
                var contSP12 = 0;
                var TotalcontSP12 = 0;
                html += '</tr><tr><td>Sprayed 1st</td>';
                $.each(lista_Tallas2, function (key, itemS1) {
                    html += '<td class="printedsp1"><input type="text" id="spra1' + contSP12 + '" onfocus="focusingSP1(' + contSP12 + ')" class="txtDes form-control number spra1" style=" background-color:transparent; border: 0; box-shadow: none;" value="' + 0 + '" readonly/></td>';
                    contSP12++;
                    TotalcontSP12 += 0;
                });
                html += '<td>' + TotalcontSP12 + '</td>';
                var contSP22 = 0;
                var TotalcontSP22 = 0;
                html += '</tr><tr><td>Sprayed 2nd</td>';
                $.each(lista_Tallas2, function (key, itemS2) {
                    html += '<td class="printedsp2"><input type="text" id="spra2' + contSP22 + '" onfocus="focusingSP2(' + contSP22 + ')" class="txtDes form-control number spra2" style=" background-color:transparent; border: 0; box-shadow: none;" value="' + 0 + '" readonly/></td>';
                    contSP22++;
                    TotalcontSP22 += 0;
                });
                html += '<td>' + TotalcontSP22 + '</td>';
                var contD12 = 0;
                var TotalcontD12 = 0;
                html += '</tr><tr><td>Defects 1st</td>';
                $.each(lista_Tallas2, function (key, itemD1) {
                    html += '<td class="printeddef1"><input type="text" id="def1' + contD12 + '" onfocus="focusingD1(' + contD12 + ')"  class="txtDes form-control number def1" style=" background-color:transparent; border: 0; box-shadow: none;" value="' + 0 + '" readonly/></td>';
                    contD12++;
                    TotalcontD12 += 0;
                });
                html += '<td>' + TotalcontD12 + '</td>';
                var contD22 = 0;
                var TotalcontD22 = 0;
                html += '</tr><tr><td>Defects 2nd</td>';
                $.each(lista_Tallas2, function (key, itemD2) {
                    html += '<td class="printeddef2"><input type="text" id="def2' + contD22 + '" onfocus="focusingD2(' + contD22 + ')" class="txtDes form-control number def2" style=" background-color:transparent; border: 0; box-shadow: none;" value="' + 0 + '" readonly/></td>';
                    contD22++;
                    TotalcontD22 += 0;
                });
                html += '<td>' + TotalcontD22 + '</td>';
                html += '</tr>';

                $('.tbodyTRM').html(html);
            }


        },
        error: function (errormessage) { alert(errormessage.responseText); }
    });

}


function focusingMP1(valor) {
    if ($("#mp1" + valor).val() === 0 || $("#mp1" + valor).val() === "0") {
        $("#mp1" + valor).val('');
    }
}


function focusingMP2(valor) {
    if ($("#mp2" + valor).val() === 0 || $("#mp2" + valor).val() === "0") {
        $("#mp2" + valor).val('');
    }
}

function focusingR1(valor) {
    if ($("#rep1" + valor).val() === 0 || $("#rep1" + valor).val() === "0") {
        $("#rep1" + valor).val('');
    }
}

function focusingR2(valor) {
    if ($("#rep2" + valor).val() === 0 || $("#rep2" + valor).val() === "0") {
        $("#rep2" + valor).val('');
    }
}

function focusingSP1(valor) {
    if ($("#spra1" + valor).val() === 0 || $("#spra1" + valor).val() === "0") {
        $("#spra1" + valor).val('');
    }
}

function focusingSP2(valor) {
    if ($("#spra2" + valor).val() === 0 || $("#spra2" + valor).val() === "0") {
        $("#spra2" + valor).val('');
    }
}

function focusingD1(valor) {
    if ($("#def1" + valor).val() === 0 || $("#def1" + valor).val() === "0") {
        $("#def1" + valor).val('');
    }
}

function focusingD2(valor) {
    if ($("#def2" + valor).val() === 0 || $("#def2" + valor).val() === "0") {
        $("#def2" + valor).val('');
    }
}
