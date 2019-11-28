function obtenerPalletBulk() {
    var r = 0; var c = 0; var i = 0; var cadena = new Array(6);
    cadena[0] = ''; cadena[1] = ''; cadena[2] = ''; cadena[3] = ''; cadena[4] = ''; cadena[5] = '';
    var nFilas = $("#tablaTallasPallet tbody>tr").length;
    var nColumnas = $("#tablaTallasPallet tr:last td").length;
    $('#tablaTallasPallet tbody>tr').each(function () {
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
   error = 0;
    $('#tablaTallasPallet').find('td.cBox').each(function (i, el) {
        var valor = $(el).children().val();
        if ($(el).children().val() === '') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '1px solid #cccccc');
        }
    });
    var clienteNombre = $("#CatClienteFinal_NombreCliente").val();
    var cliente = $.trim(clienteNombre);

    $('#tablaTallasPallet').find('td.tFalB').each(function (i, el) {
        var valor = $(el).children().val();
        var faltBox = parseInt(valor);
        var c = i + 1;
        var nombreC = "#pallet" + c + " .cantCajas";
        if (cliente === "FEA TARGET") {
            faltantesBulkFantasy(faltBox, error, nombreC, el);
        } else if (cliente === "BRAVADO TARGET") {
            faltantesBulkFantasy(faltBox, error, nombreC, el);
        } else if (cliente === "Merch Traffic Target") {
            faltantesBulkFantasy(faltBox, error, nombreC, el);
        } else {
            faltantesBulk(faltBox, error, nombreC, el);
        }
    });

    var tipoTurno = $("#Packing_Turnos option:selected").val();
    if (tipoTurno === "0") {
        error++;
        $('#Packing_Turnos').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_Turnos').css('border', '');
    }
    enviarListaTallaPallet(cadena, error);
}

function faltantesBulk(faltBox, error, nombreC, el) {

    if (faltBox < 0) {
        error++;
        $(el).children().css('border', '2px solid #e03f3f');
        $(nombreC).css('border', '2px solid #e03f3f');


    } else {
        if ($(nombreC).val() === '') {
            error++;
            $(nombreC).css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '1px solid #cccccc');
            $(nombreC).css('border', '1px solid #cccccc');
        }

    }
}

function faltantesBulkFantasy(faltBox, error, nombreC, el) {

	/*if (faltBox >= -5 ) {
		error++;
		$(el).children().css('border', '2px solid #e03f3f');
		$(nombreC).css('border', '2px solid #e03f3f');


	} else {*/
    if ($(nombreC).val() === '') {
        error++;
        $(nombreC).css('border', '2px solid #e03f3f');

    } else {
        $(el).children().css('border', '1px solid #cccccc');
        $(nombreC).css('border', '1px solid #cccccc');
    }

    //}
}

function obtenerPalletPPK() {
    var r = 0; var c = 0; var i = 0; var cadena = new Array(6);
    cadena[0] = ''; cadena[1] = ''; cadena[2] = ''; cadena[3] = ''; cadena[4] = ''; cadena[5] = '';
    var nFilas = $("#tablaTallasPallet tbody>tr").length;
    var nColumnas = $("#tablaTallasPallet tr:last td").length;
    $('#tablaTallasPallet tbody>tr').each(function () {
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
    $('#tablaTallasPallet').find('td.cBox').each(function (i, el) {
        var valor = $(el).children().val();
        if ($(el).children().val() === '' || $(el).children().val() === '0') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '1px solid #cccccc');
        }
    });

    var tipoTurno = $("#Packing_Turnos option:selected").val();
    if (tipoTurno === "0") {
        error++;
        $('#Packing_Turnos').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_Turnos').css('border', '');
    }


    var numCaja = $("#Packing_CantBox").val();
    if (numCaja === "0" || numCaja === "") {
        error++;
        $('#Packing_CantBox').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_CantBox').css('border', '');
    }
    enviarListaTallaPallet(cadena, error);
}

function obtenerPalletPPKS() {
	var r = 0; var c = 0; var i = 0; var cadena = new Array(4);
	cadena[0] = ''; cadena[1] = ''; cadena[2] = ''; cadena[3] = ''; 
	var nFilas = $("#tablaTallasPallet tbody>tr").length;
	var nColumnas = $("#tablaTallasPallet tr:last td").length;
	$('#tablaTallasPallet tbody>tr').each(function () {
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
	/*$('#tablaTallasPallet').find('td.cBox').each(function (i, el) {
		var valor = $(el).children().val();
		if ($(el).children().val() === '' || $(el).children().val() === '0') {
			error++;
			$(el).children().css('border', '2px solid #e03f3f');

		} else {
			$(el).children().css('border', '1px solid #cccccc');
		}
	});*/

	var tipoTurno = $("#Packing_TurnosPPK option:selected").val();
	if (tipoTurno === "0") {
		error++;
		$('#Packing_TurnosPPK').css('border', '2px solid #e03f3f');
	}
	else {
		$('#Packing_TurnosPPK').css('border', '');
	}


	var numCaja = $("#Packing_CantBoxPPK").val();
	if (numCaja === "0" || numCaja === "") {
		error++;
		$('#Packing_CantBoxPPK').css('border', '2px solid #e03f3f');
	}
	else {
		$('#Packing_CantBoxPPK').css('border', '');
	}

	var namePack = $("#selectPackingNameVariosPPKS option:selected").val();
	if (namePack === "0") {
		error++;
		$('#selectPackingNameVariosPPKS').css('border', '2px solid #e03f3f');
	}
	else {
		$('#selectPackingNameVariosPPKS').css('border', '');
	}


	enviarListaTallaPalletPPKS(cadena, error);
}

function obtenerPalletBulks() {
    var r = 0; var c = 0; var i = 0; var x = 1;  var cadena = new Array(4);
    cadena[0] = ''; cadena[1] = ''; cadena[2] = ''; cadena[3] = '';
    var nFilas = $("#tablaTallasPallet tbody>tr").length;
    var nColumnas = $("#tablaTallasPallet tr:last td").length;
    $('#tablaTallasPallet tbody>tr').each(function () {
        r = 0;
        c = 0;
        $(this).find("input").each(function () {
            $(this).closest('td').find("input").each(function () {
                var namePacking = $("#selectPackingNameVariosBULKS option:selected").val();
                var idNombre = "l-totalPiezas"+x;
                var inputPcs = "#l-totalPiezas" + x;
                var nombre = this.id;
                var dato;
                if (nombre === idNombre) {
                    dato = $(inputPcs).is('[readonly]');
                    x++;
                } else {
                    dato = false;
                }  

                if (dato === true) {
                    cadena[c] += 0 + "*";
                } else {
                    cadena[c] += this.value + "*";                  
                }   
               
                c++;
               
            });
           
            r++;
        });
    });
    var error = 0;
	/*$('#tablaTallasPallet').find('td.cBox').each(function (i, el) {
		var valor = $(el).children().val();
		if ($(el).children().val() === '' || $(el).children().val() === '0') {
			error++;
			$(el).children().css('border', '2px solid #e03f3f');

		} else {
			$(el).children().css('border', '1px solid #cccccc');
		}
	});*/

    var tipoTurno = $("#Packing_TurnosBulks option:selected").val();
    if (tipoTurno === "0") {
        error++;
        $('#Packing_TurnosBulks').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_TurnosBulks').css('border', '');
    }

    var namePack = $("#selectPackingNameVariosBULKS option:selected").val();
    if (namePack === "0") {
        error++;
        $('#selectPackingNameVariosBULKS').css('border', '2px solid #e03f3f');
    }
    else {
        $('#selectPackingNameVariosBULKS').css('border', '');
    }


    enviarListaTallaPalletBulks(cadena, error);
}