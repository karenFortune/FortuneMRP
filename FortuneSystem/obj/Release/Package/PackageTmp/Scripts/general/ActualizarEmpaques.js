function ActualizarEmpaqueBulkHT(nPO, tEmpaque) {
    if (nPO === "" || nPO === null) {
        nPO = 0;
    }

    $("#tablaTallasActPPKHT").hide();
    $("#saveEmpaqueBulkHT").prop("disabled", false);
    $('label[for="Packing_PackingTypeSize_TotalUnitsPPKActHT"]').hide();
    $("#Packing_PackingTypeSize_TotalUnitsPPKActHT").hide();
    var totalUnidades = 0;
    var actionData = "{'estiloId':'" + estiloId + "','nPO':'" + nPO + "','tEmpaque':'" + tEmpaque + "'}";
    $("#tablaTallasActBulkHT").show();
    $.ajax({
        url: "/Packing/Lista_Tallas_Empaque_HT_Por_Estilo/",
        type: 'POST',
        dataType: "json",
        data: actionData,
        contentType: "application/json;charset=UTF-8",
        success: function (jsonData) {
            var listaT = jsonData.Data.listaTalla;
            var listaPO = jsonData.Data.lista;
            var listaPacking = jsonData.Data.listaPackingS;
            var listaPackingBox = jsonData.Data.listaPackingBox;
            var html = '';
            var htmlB = '';


			/*   html += '<table class="table" id="tablaTallasBulkHT"><thead>';
			   html += '<tr><th>Size</th>' +
				   ' <th>QTY#</th>' +
				   ' <th>CARTONS 50PCS#</th>' +
				   ' <th>PARTIAL#</th>' +
				   ' <th>TOTALCARTONS#</th>' +
				   '</tr>' +
			   '</thead><tbody class="packBulkReg">';*/
            var cont = 0;

            $.each(listaPacking, function (key, item) {
                cont = cont + 1;
                html += '<tr id="pallet' + cont + '" class="pallet">';
                html += '<td width="20%"><input type="text" id="f-talla" class="form-control talla" value="' + item.IdPackingTypeSize + '" readonly/></td>';
                html += '<td width="20%"><input type="text" id="f-talla" class="form-control talla" value="' + item.Talla + '" readonly/></td>';
                html += '<td width="20%"><input type="text" name="l-cantidad" id="l-cantidad" class="form-control numeric qty " onkeyup="obtTotalCartones(' + cont + ')" value="' + item.Cantidad + '"  /></td>';
                html += '<td width="20%"><input type="text" name="l-cartones" id="l-cartones" class="form-control numeric cart " value="' + item.Cartones + '"  readonly/></td>';
                html += '<td width="20%"><input type="text" name="l-partial" id="l-partial" class="form-control numeric part " value="' + item.PartialNumber + '"  readonly/></td>';
                html += '<td width="20%"><input type="text" name="l-totCartones" id="l-totCartones" class="form-control numeric tcart " value="' + item.TotalCartones + '"  readonly/></td>';
                // html += '<td width="250"><button type="button" id="btnDelete" class="deleteTalla btn btn btn-danger btn-xs" value="4">Delete</button></td>';
                html += '</tr>';
                //$("#Packing_PackingTypeSize_IdPackingTypeSize").val(item.IdPackingTypeSize);
            });
            //html += '</tbody> </table>'; 
            $('.packBulkActReg').html(html);
            htmlB += '<button type="button" id="saveEmpaqueBulkHT" class="btn btn-success btn-md pull-right btn-sm"><span class="glyphicon glyphicon-floppy-disk" aria-hidden="true"></span> Save</button>';
            $('#listaTallaActPHT').html(htmlB);

        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {
    });
}

$(document).on("click", "#saveEmpaqueBulkHT", function () {
    var r = 0; var c = 0; var i = 0; var cadena = new Array(6);
    cadena[0] = ''; cadena[1] = ''; cadena[2] = ''; cadena[3] = ''; cadena[4] = ''; cadena[5] = '';
    var nFilas = $("#tablaTallasActBulkHT tbody>tr").length;
    var nColumnas = $("#tablaTallasActBulkHT tr:last td").length;
    $('#tablaTallasActBulkHT tbody>tr').each(function () {
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
    var error = 0;
    $('#tablaTallasActBulkHT').find('td').each(function (i, el) {
        var valor = $(el).children().val();
        if ($(el).children().val() === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '1px solid #cccccc');
        }
    });


    var numberPO = $("#Packing_PackingTypeSize_NumberPO").val();
    if (numberPO === "") {
        error++;
        $('#Packing_PackingTypeSize_NumberPO').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_PackingTypeSize_NumberPO').css('border', '');
    }

    enviarListaTallaActBulkHT(cadena, error);
});
//}


function enviarListaTallaActBulkHT(cadena, error) {
    var idNumberPo = $("#Packing_PackingTypeSize_NumberPO").val();
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {
        $.ajax({
            url: "/Packing/Actualizar_Lista_Tallas_Packing_Bulk_HT",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena, EstiloID: estiloId, NumberPOID: idNumberPo }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The packaging is modified correctly.', 'success', 5, null);
                $("#saveEmpaqueBulkHT").prop("disabled", true);
                // obtenerListaTallas(estiloId);
            },
            error: function (xhr, ajaxOptions, thrownError) {

            }
        });
    }
}

function ActualizarEmpaquePPKHT(nPO, tEmpaque) {
    $("#tablaTallasActBulkHT").hide();
    if (nPO === "" || nPO === null) {
        nPO = 0;
    }
    var totalUnidades = 0;
    var actionData = "{'estiloId':'" + estiloId + "','nPO':'" + nPO + "','tEmpaque':'" + tEmpaque + "'}";
    $("#tablaTallasActPPKHT").show();
    $('label[for="Packing_PackingTypeSize_TotalUnitsPPKActHT"]').show();
    $("#Packing_PackingTypeSize_TotalUnitsPPKActHT").show();
    $("#saveEmpaquePPKHT").prop("disabled", false);
    $.ajax({
        url: "/Packing/Lista_Tallas_Empaque_HT_Por_Estilo/",
        type: 'POST',
        dataType: "json",
        data: actionData,
        contentType: "application/json;charset=UTF-8",
        success: function (jsonData) {
            var listaT = jsonData.Data.listaTalla;
            var listaPO = jsonData.Data.lista;
            var listaPacking = jsonData.Data.listaPackingS;
            var html = '';
            var htmlB = '';


            /*    html += '<table class="table" id="tablaTallasPPKHT"><thead>';tablaTallasPPKHT
                html += '<tr><th>Size</th>' +
                    ' <th>Ratio</th>' +
                    '</tr>' +
                    '</thead><tbody class="tbodyHTPack">';*/
            $.each(listaPacking, function (key, item) {
                html += '<tr>';
                html += '<td width="20%"><input type="text" id="f-talla" class="form-control talla" value="' + item.IdPackingTypeSize + '" readonly/></td>';
                html += '<td width="250"><input type="text" id="f-talla" class="form-control talla" value="' + item.Talla + '" readonly/></td>';
                html += '<td width="250"><input type="text" name="l-cantidad" id="l-cantidad" class="form-control numeric qtyRat " value="' + item.Ratio + '"  /></td>';
                //  html += '<td width="250"><button type="button" id="btnDelete" class="deleteTalla btn btn btn-danger btn-xs" value="4">Delete</button></td>';
                html += '</tr>';
                $("#Packing_PackingTypeSize_TotalUnitsPPKActHT").val(item.TotalUnits);
            });
            //html += '</tbody> </table>';
            $('.packPPKActReg').html(html);
            htmlB += '<button type="button" id="saveEmpaquePPKHT" class="btn btn-success btn-md pull-right btn-sm"><span class="glyphicon glyphicon-floppy-disk" aria-hidden="true"></span>Save</button>';
            $('#listaTallaActPHT').html(htmlB);


        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {
    });
}

$(document).on("click", "#saveEmpaquePPKHT", function () {
    var r = 0; var c = 0; var i = 0; var cadena = new Array(3);
    cadena[0] = ''; cadena[1] = ''; cadena[2] = '';
    var nFilas = $("#tablaTallasActPPKHT tbody>tr").length;
    var nColumnas = $("#tablaTallasActPPKHT tr:last td").length;
    $('#tablaTallasActPPKHT tbody>tr').each(function () {
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
    var error = 0;
    $('#tablaTallasActPPKHT').find('td').each(function (i, el) {
        var valor = $(el).children().val();
        if ($(el).children().val() === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '1px solid #cccccc');
        }
    });

    var numberPO = $("#Packing_PackingTypeSize_NumberPO").val();
    if (numberPO === "") {
        error++;
        $('#Packing_PackingTypeSize_NumberPO').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_PackingTypeSize_NumberPO').css('border', '');
    }

    var tUnits = $("#Packing_PackingTypeSize_TotalUnitsPPKActHT").val();
    if (tUnits === "") {
        error++;
        $('#Packing_PackingTypeSize_TotalUnitsPPKActHT').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_PackingTypeSize_TotalUnitsPPKActHT').css('border', '');
    }



    enviarListaTallaPPKActHT(cadena, error);
});


function enviarListaTallaPPKActHT(cadena, error) {
    var idNumberPo = $("#Packing_PackingTypeSize_NumberPO").val();
    var totalUnits = $("#Packing_PackingTypeSize_TotalUnitsPPKActHT").val();
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {
        $.ajax({
            url: "/Packing/Actualizar_Lista_Tallas_Packing_PPK_HT",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena, EstiloID: estiloId, NumberPOID: idNumberPo, NumberTotU: totalUnits }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The type of packaging is registered correctly.', 'success', 5, null);
                $("#saveEmpaquePPKHT").prop("disabled", true);
                //obtenerListaTallas(estiloId);
            },
            error: function (xhr, ajaxOptions, thrownError) {

            }
        });
    }
}

function TallasEmpaqueAddBulkHT(idEst) {
    var actionData = "{'idEst':'" + idEst + "'}";
    $("#tablaTallasAddPPKHT").hide();
    $.ajax({
        url: "/Packing/Lista_Tallas_Empaque_HT_Registrar_Por_Estilo/",
        type: 'POST',
        dataType: "json",
        data: actionData,
        contentType: "application/json;charset=UTF-8",
        success: function (jsonData) {
            var listaT = jsonData.Data.listaTalla;
            var listaPO = jsonData.Data.lista;
            var listaPacking = jsonData.Data.listaPackingS;
            var html = '';
            var htmlB = '';


            $('#btnAddNuevoPPK').hide();
            $("#btnAddNuevo").prop("disabled", true);
            $("#btnAddDone").prop("disabled", true);
            $('label[id="numTotalUnitLabel"]').hide();
            $("#numTotalUnit").hide();
            $('#listaTallaBatchHT').css("display", "none");
            $('#listaBatchHT').css("display", "none");
            $("#div_titulo_Bulk_Add").html("<h3>REGISTRATION OF TYPE OF PACKAGING - BULK</h3>");
            $("#div_titulo_Bulk_Add").css("display", "inline");
            $("#opcionesNumPO").css("display", "inline");
            $("#div_titulo_Register").css("display", "none");
            $("#opcionesRegistro").css("display", "inline");

			/*   html += '<table class="table" id="tablaTallasBulkHT"><thead>';
			   html += '<tr><th>Size</th>' +
				   ' <th>QTY#</th>' +
				   ' <th>CARTONS 50PCS#</th>' +
				   ' <th>PARTIAL#</th>' +
				   ' <th>TOTALCARTONS#</th>' +
				   '</tr>' +
			   '</thead><tbody class="packBulkReg">';*/
            var cont = 0;
            $.each(listaPO, function (key, item) {
                cont = cont + 1;
                html += '<tr id="pallet' + cont + '" class="pallet">';
                html += '<td width="20%"><input type="text" id="f-talla" class="form-control talla" value="' + item.Talla + '" readonly/></td>';
                html += '<td width="20%"><input type="text" name="l-cantidad" id="l-cantidad" class="form-control numeric qty " onfocus="focusingBulk(' + cont + ')" onkeyup="obtTotalCartones(' + cont + ')" value="' + 0 + '"  /></td>';
                html += '<td width="20%"><input type="text" name="l-cartones" id="l-cartones" class="form-control numeric cart " value="' + 0 + '"  readonly/></td>';
                html += '<td width="20%"><input type="text" name="l-partial" id="l-partial" class="form-control numeric part " value="' + 0 + '"  readonly/></td>';
                html += '<td width="20%"><input type="text" name="l-totCartones" id="l-totCartones" class="form-control numeric tcart " value="' + 0 + '"  readonly/></td>';
                // html += '<td width="250"><button type="button" id="btnDelete" class="deleteTalla btn btn btn-danger btn-xs" value="4">Delete</button></td>';
                html += '</tr>';
            });
            //html += '</tbody> </table>'; 
            $('.packBulkAddReg').html(html);
            htmlB += '<button type="button" id="nuevoAddEmpaqueBulkHT" class="btn btn-success btn-md pull-right btn-sm"><span class="glyphicon glyphicon-floppy-disk" aria-hidden="true"></span> Save</button>';
            $('#listaTallaActAddPHT').html(htmlB);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {
    });
}


function TallasEmpaqueAddPPKHT(idEst) {
    $("#tablaTallasAddBulkHT").hide();
    var actionData = "{'idEst':'" + idEst + "'}";
    $.ajax({
        url: "/Packing/Lista_Tallas_Empaque_HT_Registrar_Por_Estilo/",
        type: 'POST',
        dataType: "json",
        data: actionData,
        contentType: "application/json;charset=UTF-8",
        success: function (jsonData) {
            var listaT = jsonData.Data.listaTalla;
            var listaPO = jsonData.Data.lista;
            var listaPacking = jsonData.Data.listaPackingS;
            var html = '';
            var htmlB = '';
            $("#btnAddP").show();
            $("#btnAddNuevoPPK").show();
            $("#btnAddNuevo").hide();
            $("#btnAddNuevo").prop("disabled", true);
            $("#btnAddNext").hide();
            $("#btnAddDone").prop("disabled", true);
            $("#btnAddNuevoPPK").prop("disabled", true);
            $("#opcionesRegistroPPK").css("display", "inline");
            $("#opcionestotalPPK").css("display", "inline");
            $("#opcionesNumPO").css("display", "none");
            $("#div_titulo_Bulk_Add").html("<h3>REGISTRATION OF TYPE OF PACKAGING - PPK</h3>");
            $("#div_titulo_Bulk_Add").css("display", "inline");

            /*    html += '<table class="table" id="tablaTallasPPKHT"><thead>';tablaTallasPPKHT
                html += '<tr><th>Size</th>' +
                    ' <th>Ratio</th>' +
                    '</tr>' +
                    '</thead><tbody class="tbodyHTPack">';*/
            var cont = 0;
            $.each(listaPO, function (key, item) {
                cont = cont + 1;
                html += '<tr id="pallet' + cont + '" class="pallet">';
                html += '<td width="250"><input type="text" id="f-talla" class="form-control talla" value="' + item.Talla + '" readonly/></td>';
                html += '<td width="250"><input type="text" name="l-cantidad" id="l-cantidad" class="form-control numeric qtyRat " onfocus="focusingPPK(' + cont + ')" value="' + 0 + '"  /></td>';
                //  html += '<td width="250"><button type="button" id="btnDelete" class="deleteTalla btn btn btn-danger btn-xs" value="4">Delete</button></td>';
                html += '</tr>';

            });
            //html += '</tbody> </table>';
            $('.packPPKAddReg').html(html);
            htmlB += '<button type="button" id="nuevoAddEmpaquePPKHT" class="btn btn-success btn-md pull-right btn-sm"><span class="glyphicon glyphicon-floppy-disk" aria-hidden="true"></span>Save</button>';
            $('#listaTallaActAddPHT').html(htmlB);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {
    });
}

//Actualizar cantidades de primera calidad 
$(document).on("click", "#savePCEmpaque", function () {
    var r = 0; var c = 0; var i = 0; var cadena = new Array(3);
    cadena[0] = ''; cadena[1] = ''; cadena[2] = '';
    var nFilas = $("#tableQtySizePack tbody>tr").length;
    var nColumnas = $("#tableQtySizePack tr:last td").length;
    $('#tableQtySizePack tbody>tr').each(function () {
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
    var error = 0;
    $('#tableQtySizePack').find('td').each(function (i, el) {
        var valor = $(el).children().val();
        if ($(el).children().val() === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '1px solid #cccccc');
        }
    });

    enviarListaActualizarPrimerCalidad(cadena, error);
});


function enviarListaActualizarPrimerCalidad(cadena, error) {
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {
        $.ajax({
            url: "/Packing/Actualizar_Cantidades_Primera_Calidad_Empaque",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The first quality of the packaging was modified correctly.', 'success', 5, null);
                $("#savePCEmpaque").prop("disabled", true);
                //obtenerListaTallas(estiloId);
            },
            error: function (xhr, ajaxOptions, thrownError) {

            }
        });
    }
}

//Actualizar informacion de empaque tipo bulk


$(document).on("click", "#guardarBulk", function () {
    var r = 0; var c = 0; var i = 0; var cadena = new Array(3);
    cadena[0] = ''; cadena[1] = ''; cadena[2] = '';
    var nFilas = $("#tablaTallasBulkPcsEditar tbody>tr").length;
    var nColumnas = $("#tablaTallasBulkPcsEditar tr:last td").length;
    $('#tablaTallasBulkPcsEditar tbody>tr').each(function () {
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
    var error = 0;
    $('#tablaTallasBulkPcsEditar').find('td').each(function (i, el) {
        var valor = $(el).children().val();
        if ($(el).children().val() === '' || $(el).children().val() === '0') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '1px solid #cccccc');
        }
    });
    enviarListaTallaActualizadaPackBulk(cadena, error);

});
//});

function enviarListaTallaActualizadaPackBulk(cadena, error) {
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {
        $("#modificarPack").show();
        $("#editarPack").show();
        $("#modificarPack").prop("disabled", false);
        $("#editarPack").prop("disabled", false);
        $.ajax({
            url: "/Packing/Actualizar_Lista_Tallas_Packing_Bulk",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The packing instruction was modified correctly.', 'success', 5, null);
                obtenerListaTallas(estiloId);
            }
        });
    }
}

//Actualizar informacion de empaque tipo ppk
$(document).on("click", "#guardarPPK", function () {
    var r = 0; var c = 0; var i = 0; var cadena = new Array(3);
    cadena[0] = ''; cadena[1] = ''; cadena[2] = '';
    var nFilas = $("#tablaTallasPPKRatioEditar tbody>tr").length;
    var nColumnas = $("#tablaTallasPPKRatioEditar tr:last td").length;
    $('#tablaTallasPPKRatioEditar tbody>tr').each(function () {
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
    var error = 0;
    $('#tablaTallasPPKRatioEditar').find('td').each(function (i, el) {
        var valor = $(el).children().val();
        if ($(el).children().val() === '' || $(el).children().val() === '0') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');
        } else {
            $(el).children().css('border', '1px solid #cccccc');
        }
    });
    enviarListaTallaActualizarPackPPK(cadena, error);
});

function enviarListaTallaActualizarPackPPK(cadena, error) {
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {
        $("#modificarPack").show();
        $("#editarPack").show();
        $("#modificarPack").prop("disabled", false);
        $("#editarPack").prop("disabled", false);
        $.ajax({
            url: "/Packing/Actualizar_Lista_Tallas_Packing_PPK",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The packing instruction was modified correctly.', 'success', 5, null);
                obtenerListaTallas(estiloId);
            }
        });
    }
}

//Registrar el nuevo empaque de varios PPKs
$(document).on("click", "#nuevoEmpaqueVariosPPK", function () {
    var r = 0; var c = 0; var i = 0; var cadena = new Array(3);
    cadena[0] = ''; cadena[1] = ''; cadena[2] = '';
    var nFilas = $("#tablaTallasVariosPPKRatio tbody>tr").length;
    var nColumnas = $("#tablaTallasVariosPPKRatio tr:last td").length;
    $('#tablaTallasVariosPPKRatio tbody>tr').each(function () {
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
    var error = 0;
    $('#tablaTallasVariosPPKRatio').find('td').each(function (i, el) {
        var valor = $(el).children().val();
        if ($(el).children().val() === '' || $(el).children().val() === '0') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');
        } else {
            $(el).children().css('border', '1px solid #cccccc');
        }
    });
    var tipoEmpaque = $("#Packing_PackingTypeSize_TipoEmpaque option:selected").val();
    if (tipoEmpaque === "0") {
        error++;
        $('#Packing_PackingTypeSize_TipoEmpaque').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_PackingTypeSize_TipoEmpaque').css('border', '');
    }
    var numPiezas = $("#Packing_PackingTypeSize_NumberPKK").val();
    if (numPiezas === "0" || numPiezas === '') {
        error++;
        $('#Packing_PackingTypeSize_NumberPKK').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_PackingTypeSize_NumberPKK').css('border', '');
    }
    var nombrePack = $("#Packing_PackingTypeSize_NombrePacking").val();
    if (nombrePack === "") {
        error++;
        $('#Packing_PackingTypeSize_NombrePacking').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_PackingTypeSize_NombrePacking').css('border', '');
    }
    enviarListaTallaVariosPPK(cadena, error);
});

function enviarListaTallaVariosPPK(cadena, error) {
    var numeroPedido = $("#IdPedido").val();
    var idTipoP = $("#Packing_PackingTypeSize_TipoEmpaque option:selected").val();
    var numPcs = $("#Packing_PackingTypeSize_NumberPKK").val();
    var nombrePack = $("#Packing_PackingTypeSize_NombrePacking").val();
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {
        $("#modificarPack").show();
        $("#editarPack").show();
        $("#modificarPack").prop("disabled", false);
        $("#editarPack").prop("disabled", false);
        $("#botonAddNuevoPPK").prop("disabled", false);
        $.ajax({
            url: "/Packing/Obtener_Lista_Tallas_Varios_Packing_PPK",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena, EstiloID: estiloId, TipoPackID: idTipoP, NumeroPcs: numPcs, NombrePacking: nombrePack, numPedido: numeroPedido }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The type of packaging is registered correctly.', 'success', 5, null);
                $("#nuevoEmpaquePPK").prop("disabled", true);
                $("#nuevoEmpaqueVariosPPK").prop("disabled", true);
                //obtenerListaTallas(estiloId);
            }
        });
    }
}

//Registrar el nuevo empaque de varios Bulks
$(document).on("click", "#nuevoEmpaqueVariosBulks", function () {
    var r = 0; var c = 0; var i = 0; var cadena = new Array(3);
    cadena[0] = ''; cadena[1] = ''; cadena[2] = '';
    var nFilas = $("#tablaTallasVariosBulks tbody>tr").length;
    var nColumnas = $("#tablaTallasVariosBulks tr:last td").length;
    $('#tablaTallasVariosBulks tbody>tr').each(function () {
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
    var error = 0;
    $('#tablaTallasVariosBulks').find('td').each(function (i, el) {
        var valor = $(el).children().val();
        if ($(el).children().val() === '' || $(el).children().val() === '0') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');
        } else {
            $(el).children().css('border', '1px solid #cccccc');
        }
    });
    var tipoEmpaque = $("#Packing_PackingTypeSize_TipoEmpaque option:selected").val();
    if (tipoEmpaque === "0") {
        error++;
        $('#Packing_PackingTypeSize_TipoEmpaque').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_PackingTypeSize_TipoEmpaque').css('border', '');
    }
    var nombrePack = $("#Packing_PackingTypeSize_PackingNameBulk").val();
    if (nombrePack === "") {
        error++;
        $('#Packing_PackingTypeSize_PackingNameBulk').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_PackingTypeSize_PackingNameBulk').css('border', '');
    }
    enviarListaTallaVariosBulks(cadena, error);
});

function enviarListaTallaVariosBulks(cadena, error) {
    var numeroPedido = $("#IdPedido").val();
    var idTipoP = $("#Packing_PackingTypeSize_TipoEmpaque option:selected").val();
    var nombrePack = $("#Packing_PackingTypeSize_PackingNameBulk").val();
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {
        $("#modificarPack").show();
        $("#editarPack").show();
        $("#modificarPack").prop("disabled", false);
        $("#editarPack").prop("disabled", false);
        $("#botonAddNuevoBulk").prop("disabled", false);
        $.ajax({
            url: "/Packing/Obtener_Lista_Tallas_Varios_Packing_Bulk",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena, EstiloID: estiloId, TipoPackID: idTipoP, NombrePacking: nombrePack, numPedido: numeroPedido }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The type of packaging is registered correctly.', 'success', 5, null);
                $("#nuevoEmpaquePPK").prop("disabled", true);
                $("#nuevoEmpaqueVariosBulks").prop("disabled", true);
                //obtenerListaTallas(estiloId);
            }
        });
    }
}


//añADIR UN nuevo empaque de varios PPKs
$(document).on("click", "#nuevoEmpaqueVariosAddPPK", function () {
    debugger
    var r = 0; var c = 0; var i = 0; var cadena = new Array(3);
    cadena[0] = ''; cadena[1] = ''; cadena[2] = '';
    var nFilas = $("#tablaTallasVariosAddPPKRatio tbody>tr").length;
    var nColumnas = $("#tablaTallasVariosAddPPKRatio tr:last td").length;
    $('#tablaTallasVariosAddPPKRatio tbody>tr').each(function () {
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
    var error = 0;
    $('#tablaTallasVariosAddPPKRatio').find('td').each(function (i, el) {
        var valor = $(el).children().val();
        if ($(el).children().val() === '' || $(el).children().val() === '0') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');
        } else {
            $(el).children().css('border', '1px solid #cccccc');
        }
    });
    var tipoEmpaque = $("#Packing_PackingTypeSize_TipoEmpaquePPK option:selected").val();
    if (tipoEmpaque === "0") {
        error++;
        $('#Packing_PackingTypeSize_TipoEmpaquePPK').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_PackingTypeSize_TipoEmpaquePPK').css('border', '');
    }
    var numPiezas = $("#Packing_PackingTypeSize_NumberAddPPKs").val();
    if (numPiezas === "0" || numPiezas === '') {
        error++;
        $('#Packing_PackingTypeSize_NumberAddPPKs').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_PackingTypeSize_NumberAddPPKs').css('border', '');
    }
    var nombrePack = $("#Packing_PackingTypeSize_NombrePackingAddPPKs").val();
    if (nombrePack === "") {
        error++;
        $('#Packing_PackingTypeSize_NombrePackingAddPPKs').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_PackingTypeSize_NombrePackingAddPPKs').css('border', '');
    }
    enviarListaTallaVariosAddPPKs(cadena, error);
});

function enviarListaTallaVariosAddPPKs(cadena, error) {
    var numeroPedido = $("#IdPedido").val();
    var idTipoP = $("#Packing_PackingTypeSize_TipoEmpaquePPK option:selected").val();
    var numPcs = $("#Packing_PackingTypeSize_NumberAddPPKs").val();
    var nombrePack = $("#Packing_PackingTypeSize_NombrePackingAddPPKs").val();
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {
        $("#modificarPack").show();
        $("#editarPack").show();
        $("#modificarPack").prop("disabled", false);
        $("#editarPack").prop("disabled", false);
        $("#botonAddNuevoPPK").prop("disabled", false);
        $.ajax({
            url: "/Packing/Obtener_Lista_Tallas_Varios_Packing_PPK",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena, EstiloID: estiloId, TipoPackID: idTipoP, NumeroPcs: numPcs, NombrePacking: nombrePack, numPedido: numeroPedido }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The type of packaging is registered correctly.', 'success', 5, null);
                $("#botonAddNuevoPPKs").prop("disabled", true);
                $("#nuevoEmpaqueVariosAddPPK").prop("disabled", true);
                //obtenerListaTallas(estiloId);
            }
        });
    }
}

//Actualizar informacion de empaque tipo varios ppk
$(document).on("click", "#guardarVariosPPK", function () {
    var r = 0; var c = 0; var i = 0; var cadena = new Array(4);
    cadena[0] = ''; cadena[1] = ''; cadena[2] = ''; cadena[3] = '';
    var nFilas = $("#tablaTallasVariosPPKEditarRatio tbody>tr").length;
    var nColumnas = $("#tablaTallasVariosPPKEditarRatio tr:last td").length;
    $('#tablaTallasVariosPPKEditarRatio tbody>tr').each(function () {
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
    var error = 0;
    $('#tablaTallasVariosPPKEditarRatio').find('td').each(function (i, el) {
        var valor = $(el).children().val();
        if ($(el).children().val() === '' || $(el).children().val() === '0') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');
        } else {
            $(el).children().css('border', '1px solid #cccccc');
        }
    });

    var numPiezas = $("#Packing_PackingTypeSize_NumberPPKs").val();
    if (numPiezas === "0" || numPiezas === '' || numPiezas === 0) {
        error++;
        $('#Packing_PackingTypeSize_NumberPPKs').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_PackingTypeSize_NumberPPKs').css('border', '');
    }
    var nombrePack = $("#Packing_PackingTypeSize_NombrePackingPPKs").val();
    if (nombrePack === "") {
        error++;
        $('#Packing_PackingTypeSize_NombrePackingPPKs').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_PackingTypeSize_NombrePackingPPKs').css('border', '');
    }
    enviarListaTallaActualizarPackVariosPPK(cadena, error);
});

function enviarListaTallaActualizarPackVariosPPK(cadena, error) {
    var numPcs = $("#Packing_PackingTypeSize_NumberPPKs").val();
    var nombrePack = $("#Packing_PackingTypeSize_NombrePackingPPKs").val();
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {
        $("#modificarPack").show();
        $("#editarPack").show();
        $("#modificarPack").prop("disabled", false);
        $("#editarPack").prop("disabled", false);
        $.ajax({
            url: "/Packing/Actualizar_Lista_Tallas_Packing_Varios_PPK",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena, NumeroPcs: numPcs, NombrePacking: nombrePack, }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The packing instruction was modified correctly.', 'success', 5, null);
                obtenerListaTallas(estiloId);
            }
        });
    }
}


//añADIR UN nuevo empaque de varios BULKS
$(document).on("click", "#nuevoEmpaqueVariosAddBulks", function () {
    var r = 0; var c = 0; var i = 0; var cadena = new Array(3);
    cadena[0] = ''; cadena[1] = ''; cadena[2] = '';
    var nFilas = $("#tablaTallasVariosAddBulkPcs tbody>tr").length;
    var nColumnas = $("#tablaTallasVariosAddBulkPcs tr:last td").length;
    $('#tablaTallasVariosAddBulkPcs tbody>tr').each(function () {
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
    var error = 0;
    $('#tablaTallasVariosAddBulkPcs').find('td').each(function (i, el) {
        var valor = $(el).children().val();
        if ($(el).children().val() === '' || $(el).children().val() === '0') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');
        } else {
            $(el).children().css('border', '1px solid #cccccc');
        }
    });
    var tipoEmpaque = $("#Packing_PackingTypeSize_TipoEmpaqueBulk option:selected").val();
    if (tipoEmpaque === "0") {
        error++;
        $('#Packing_PackingTypeSize_TipoEmpaqueBulk').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_PackingTypeSize_TipoEmpaqueBulk').css('border', '');
    }
    var nombrePack = $("#Packing_PackingTypeSize_NombrePackingAddBulks").val();
    if (nombrePack === "") {
        error++;
        $('#Packing_PackingTypeSize_NombrePackingAddBulks').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_PackingTypeSize_NombrePackingAddBulks').css('border', '');
    }
    enviarListaTallaVariosAddBulks(cadena, error);
});

function enviarListaTallaVariosAddBulks(cadena, error) {
    var numeroPedido = $("#IdPedido").val();
    var idTipoP = $("#Packing_PackingTypeSize_TipoEmpaqueBulk option:selected").val();
    var nombrePack = $("#Packing_PackingTypeSize_NombrePackingAddBulks").val();
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {
        $("#modificarPack").show();
        $("#editarPack").show();
        $("#modificarPack").prop("disabled", false);
        $("#editarPack").prop("disabled", false);
        $.ajax({
            url: "/Packing/Obtener_Lista_Tallas_Varios_Packing_Bulk",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena, EstiloID: estiloId, TipoPackID: idTipoP, NombrePacking: nombrePack, numPedido: numeroPedido }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The type of packaging is registered correctly.', 'success', 5, null);
                $("#botonAddNuevoPPKs").prop("disabled", true);
                $("#nuevoEmpaqueVariosAddBulks").prop("disabled", true);
                //obtenerListaTallas(estiloId);
            }
        });
    }
}

//Actualizar informacion de empaque tipo varios bulks
$(document).on("click", "#guardarBulks", function () {
    var r = 0; var c = 0; var i = 0; var cadena = new Array(3);
    cadena[0] = ''; cadena[1] = ''; cadena[2] = '';
    var nFilas = $("#tablaTallasVariosBulksEditarPcs tbody>tr").length;
    var nColumnas = $("#tablaTallasVariosBulksEditarPcs tr:last td").length;
    $('#tablaTallasVariosBulksEditarPcs tbody>tr').each(function () {
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
    var error = 0;
    $('#tablaTallasVariosBulksEditarPcs').find('td').each(function (i, el) {
        var valor = $(el).children().val();
        if ($(el).children().val() === '' || $(el).children().val() === '0') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');
        } else {
            $(el).children().css('border', '1px solid #cccccc');
        }
    });

    var nombrePack = $("#Packing_PackingTypeSize_NombrePackingBulks").val();
    if (nombrePack === "") {
        error++;
        $('#Packing_PackingTypeSize_NombrePackingBulks').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_PackingTypeSize_NombrePackingBulks').css('border', '');
    }
    enviarListaTallaActualizarPackVariosBulks(cadena, error);
});

function enviarListaTallaActualizarPackVariosBulks(cadena, error) {
    var nombrePack = $("#Packing_PackingTypeSize_NombrePackingBulks").val();
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {
        $("#modificarPack").show();
        $("#editarPack").show();
        $("#modificarPack").prop("disabled", false);
        $("#editarPack").prop("disabled", false);
        $.ajax({
            url: "/Packing/Actualizar_Lista_Tallas_Packing_Varios_BULKS",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena, NombrePacking: nombrePack }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The packing instruction was modified correctly.', 'success', 5, null);
                obtenerListaTallas(estiloId);
            }
        });
    }
}