$(function () {
	$('#selectPos').change(function () {
		$("#contenedorEstilos").css("display", "none");
		var selectedText = $(this).find("option:selected").text();
		var selectedValue = $(this).val();
		var html = '';
		
		var IdPedido = $("#selectPos option:selected").val();
		var IdHorno = $("#selectHorno option:selected").val();
		if (IdPedido !== "0" && IdHorno !== "0") {

			cargarDatosEstilos(IdPedido, IdHorno);

			//$("#regAssort").show();
		} else {
			$("#contenedorReporte").css("display", "none");
		}

	});	

	$('#selectPosProd').change(function () {
		$("#contenedorEstilos").css("display", "none");
		var selectedText = $(this).find("option:selected").text();
		var selectedValue = $(this).val();
		ActualizarSelectEstilosPorPO(selectedValue);		
		$("#contenedorDatos2").css("display", "inline");
		$("#contenedorDatosPersonal").css("display", "inline");
		
	});

	$('#selectPosProdAct').change(function () {
		
		var selectedText = $(this).find("option:selected").text();
		var selectedValue = $(this).val();
		ActualizarSelectEstilosAct2PorPO(selectedValue);
	});

	$('#selectHornoAct').change(function () {

	var selectedText = $(this).find("option:selected").text();
	var selectedValue = $(this).val();
		ActualizarSelectMaquinaOven2(selectedValue);
});
	

	$('#selectHorno').change(function () {
		$("#contenedorEstilos").css("display", "none");
		var selectedText = $(this).find("option:selected").text();
		var selectedValue = $(this).val();
		ActualizarSelectMaquinas(selectedValue);	
		$("#datosMaq").css("display", "inline");
		$("#datosPO").css("display", "inline");
		$("#contenedorDatos").css("display", "inline");
		$("#nuevaPlaneacion").show();
		$("#modificarPlaneacion").hide();
		$("#selectPos").val("").change();

	});
	
	$('#selectPP1').change(function () {
		
		$("nuevaPlaneacion").prop('disabled', this.value !== "");
	
	}).change();

	
	/*$('#tablaMaquinas.selectPP1').on('change', function () {

		var selectedText = $(this).val();

		$(this).parents('td').next().text(selectedText);
		$("#nuevaPlaneacion").prop("disabled", false);
	}).change();
	$('selectPP1').change(function () {
		if ($('selectPP1').text() === "1") {
			$("#nuevaPlaneacion").prop("disabled", false);
		}
	});*/



	/*$("#tablaMaquinas [id*='selectPP1']").change(function () {
		// Find the closest row and find your 'cbo' element and get its value
		$("#nuevaPlaneacion").prop("disabled", false);
	});*/
	
});



function cargarDatosEstilos(IdPedido, IdHorno) {
	var actionData = "{'idPedido':'" + IdPedido + "','idHorno':'" + IdHorno + "'}";
	if (IdPedido === "" || IdHorno === "") {
		var errorR = 0;
		var pedido = $("#selectPos option:selected").val();
		if (pedido === "0") {
			errorR++;
			$("#selectPos option:selected").css('border', '2px solid #e03f3f');
		}
		else {
			$("#selectPos option:selected").css('border', '');
		}

		var horno = $("#selectHorno option:selected").val();
		if (horno === "") {
			errorR++;
			$("#selectHorno option:selected").css('border', '2px solid #e03f3f');
		}
		else {
			$("#selectHorno option:selected").css('border', '');
		}

		if (errorR !== 0) {
			var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
			alert.set({ transition: 'zoom' });
			alert.set('modal', false);
		}
	} else {
		$("#contenedorEstilos").css("display", "inline");
		$.ajax({
			url: "/ProductionPlan/Obtener_Datos_PO_Oven/",
			type: "POST",
			data: actionData,
			contentType: "application/json;charset=UTF-8",
			dataType: "json",
			success: function (jsonData) {
				var html = '';
				var html2 = '';
				var lista_Maquinas = jsonData.Data.listaMaquinas;
				var lista_estilos = jsonData.Data.listaEstilos;
				var listaVacia = 0;
				$.each(lista_Maquinas, function (key, item) {
					listaVacia++;
				});
	
				if (listaVacia !== 0) {
					$("#nuevaPlaneacion").show();
					$("#modificarPlaneacion").hide();
					var contador = 0;
					$.each(lista_Maquinas, function (key, itemL) {
						
						//var hora = new Date(parseInt(itemL.HoraLavado.substr(6)));                  
						html += '<tr id="options">';
						html += '<td align="center" id="po"> <input type="text" name="id" id="id" class="txtDes form-control" style="background-color:transparent; border: 0; box-shadow: none; " value="' + itemL.NoMaquina + '" readonly/></td>';		
						html += '<td align="center" id="po"><select id="selectPP' + contador + '" multiple="multiple" class="form-control selectPP' + contador + '"></select></td>';
						html += '<td align="center" id="po"> <input type="datetime" name="Fecha3" id="FechaEstilo' + contador + '" class=" form-control date-picker"  /></td>';		
						html += '<td align="center" id="po"> <input type="text" name="metedor" id="metedor' + contador + '" class=" form-control" /></td>';	
						html += '<td align="center" id="po"> <input type="text" name="sacador" id="sacador' + contador + '" class=" form-control" /></td>';	
						html += '<td align="center" id="po"> <input type="text" name="cachador" id="cachador' + contador + '" class=" form-control" /></td>';	
						
						/*$.each(lista_estilos, function (key, item) {
							html += '<td align="center" id="po"><select id="selectPL" class="form-control"><option  value="' + item.IdItems + '">' + item.ItemDescripcion.Descripcion + '</option></select></td>';
						});*/
						
						html += '</tr>';
						
						contador++;
					});				

					if (Object.keys(lista_Maquinas).length === 0) {
						html += '<tr class="odd"><td valign="middle" align="center" colspan="10" class="dataTables_empty">No sizes were found for the style.</td></tr>';
					}
					$('.tbodyMaq').html(html);
					ActualizarSelectEstilos(lista_estilos, lista_Maquinas, html);
					
					/*$.each(lista_estilos, function (key, item) {
						html2 += '<option value="' + item.IdItems + '">' + item.ItemDescripcion.Descripcion + '</option>';
					});
					$('#selectPP').append(html2);
					$('#selectPP').parent().show();*/
					//ActualizarSelectEstilos(lista_estilos);
					//$("#tablaPruebaLavado").hide();
	
				} /*else {
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
	
	
				$("#loading").css('display', 'none');*/

			},
			error: function (errormessage) { alert(errormessage.responseText); }
		});
	}
	
}

function ActualizarSelectEstilos(lista_estilos, lista_Maquinas, html) {
	var contador = 0;
	$.each(lista_Maquinas, function (key, item) {
		var nombre = "#selectPP" + contador;
		var fecha = "#FechaEstilo" + contador;
		$(fecha).datepicker();
		//$(nombre).find('option:not(:first)').remove();
		$(nombre).prepend('<option selected></option>').select2({
			placeholder: 'Select a Style',
			allowClear: true,
			width: '300px',
			maximumSelectionLength: 2
		});
		$.each(lista_estilos, function (key, item) {
			$(nombre).append('<option value=' + item.IdItems + '>' + item.ItemDescripcion.Descripcion + '</option>'); 
		});
		contador++;
	});
}

function ActualizarSelectMaquinas(idHorno) {
	$('#selectMachine').find('option:not(:first)').remove();
	$.ajax({
		url: "/ProductionPlan/ListadoMaquinasPorHorno/" + idHorno,
		method: 'POST',
		dataType: "json",
		success: function (jsonData) {
			var html = '';
			var listaMaquinas = jsonData.Data.listMaquinas;

			$.each(listaMaquinas, function (key, item) {
				html += '<option  value="' + item.IdProdOverMachine + '">' + item.NoMaquina + '</option>';
			});
			$('#selectMachine').append(html);
			$('#selectMachine').parent().show();
		},
		error: function (errormessage) {
			alert(errormessage.responseText);
		}
	}).done(function (data) {

	});
}

function ActualizarSelectMaquinaOven(idHorno,idMaquina) {
	$('#selectMachineAct').find('option:not(:first)').remove();
	$.ajax({
		url: "/ProductionPlan/ListadoMaquinasPorHorno/" + idHorno,
		method: 'POST',
		dataType: "json",
		success: function (jsonData) {
			var html = '';
			var listaMaquinas = jsonData.Data.listMaquinas;

			$.each(listaMaquinas, function (key, item) {
				html += '<option  value="' + item.IdProdOverMachine + '">' + item.NoMaquina + '</option>';
			});
			$('#selectMachineAct').append(html);
			$('#selectMachineAct').parent().show();
			$("#selectMachineAct").val(idMaquina).change();
		},
		error: function (errormessage) {
			alert(errormessage.responseText);
		}
	}).done(function (data) {

	});
}

function ActualizarSelectMaquinaOven2(idHorno) {
	$('#selectMachineAct').find('option:not(:first)').remove();
	$.ajax({
		url: "/ProductionPlan/ListadoMaquinasPorHorno/" + idHorno,
		method: 'POST',
		dataType: "json",
		success: function (jsonData) {
			var html = '';
			var listaMaquinas = jsonData.Data.listMaquinas;

			$.each(listaMaquinas, function (key, item) {
				html += '<option  value="' + item.IdProdOverMachine + '">' + item.NoMaquina + '</option>';
			});
			$('#selectMachineAct').append(html);
			$('#selectMachineAct').parent().show();
			
		},
		error: function (errormessage) {
			alert(errormessage.responseText);
		}
	}).done(function (data) {

	});
}

function ActualizarSelectEstilosPorPO(idPedido) {
	$('#selectEstilosProd').find('option:not(:first)').remove();
	$.ajax({
		url: "/ProductionPlan/ListadoEstilosPOHorno/" + idPedido,
		method: 'POST',
		dataType: "json",
		success: function (jsonData) {
			var html = '';
			var listaEstilos = jsonData.Data.listEstilos;

			$.each(listaEstilos, function (key, item) {
				html += '<option  value="' + item.IdItems + '">' + item.ItemDescripcion.Descripcion + '</option>';
			});
			$('#selectEstilosProd').append(html);
			$('#selectEstilosProd').parent().show();
		},
		error: function (errormessage) {
			alert(errormessage.responseText);
		}
	}).done(function (data) {

	});
}


function ActualizarSelectEstilosActPorPO(idPedido, idSummary) {
	$('#selectEstilosProdAct').find('option:not(:first)').remove();
	$.ajax({
		url: "/ProductionPlan/ListadoEstilosPOHorno/" + idPedido,
		method: 'POST',
		dataType: "json",
		success: function (jsonData) {
			var html = '';
			var listaEstilos = jsonData.Data.listEstilos;

			$.each(listaEstilos, function (key, item) {
				html += '<option  value="' + item.IdItems + '">' + item.ItemDescripcion.Descripcion + '</option>';
			});
			$('#selectEstilosProdAct').append(html);
			$('#selectEstilosProdAct').parent().show();
			$('#selectEstilosProdAct').val(idSummary).change();
		},
		error: function (errormessage) {
			alert(errormessage.responseText);
		}
	}).done(function (data) {

	});
}

function ActualizarSelectEstilosAct2PorPO(idPedido) {
	$('#selectEstilosProdAct').find('option:not(:first)').remove();
	$.ajax({
		url: "/ProductionPlan/ListadoEstilosPOHorno/" + idPedido,
		method: 'POST',
		dataType: "json",
		success: function (jsonData) {
			var html = '';
			var listaEstilos = jsonData.Data.listEstilos;

			$.each(listaEstilos, function (key, item) {
				html += '<option  value="' + item.IdItems + '">' + item.ItemDescripcion.Descripcion + '</option>';
			});
			$('#selectEstilosProdAct').append(html);
			$('#selectEstilosProdAct').parent().show();
		
		},
		error: function (errormessage) {
			alert(errormessage.responseText);
		}
	}).done(function (data) {

	});
}

function registrarPlaneacion() {
	var r = 0; var c = 0; var i = 0; var cadena = new Array(6);
	cadena[0] = ''; cadena[1] = ''; cadena[2] = ''; cadena[3] = ''; cadena[4] = ''; cadena[5] = '';
	var nFilas = $("#tablaMaquinas tbody>tr").length;
	var nColumnas = $("#tablaMaquinas tr:last td").length;
	var resultado = '';
	$('#tablaMaquinas tbody>tr').each(function () {
		r = 0;
		c = 0;

		$(this).find("input").each(function () {
			$(this).closest('td').find("input").each(function () {
				var datos = this.value;
				//if (datos !== "") {
					cadena[c] += datos + "*";
				//}
				
				c++;
			});

			r++;
		});
	});
	$('#tablaMaquinas tbody>tr').each(function () {

		var datoEstilo = $(this).find("select option:selected").val();
		resultado += datoEstilo + "*";
		/*if (datoEstilo !== "") {
			
		} else {
			resultado += "*";
		}*/
		


	});
	cadena.splice(1, 1, resultado);
	var errorR = 0;

	var metedor = $("#Metedor").val();
	if (metedor === "") {
		errorR++;
		$('#Metedor').css('border', '2px solid #e03f3f');
	}
	else {
		$('#Metedor').css('border', '');
	}

	var sacador = $("#Sacador").val();
	if (sacador === "") {
		errorR++;
		$('#Sacador').css('border', '2px solid #e03f3f');
	}
	else {
		$('#Sacador').css('border', '');
	}

	var cachador = $("#Cachador").val();
	if (cachador === "") {
		errorR++;
		$('#Cachador').css('border', '2px solid #e03f3f');
	}
	else {
		$('#Cachador').css('border', '');
	}

	var fecha = $("#Fecha").val();
	if (fecha === "") {
		errorR++;
		$('#Fecha').css('border', '2px solid #e03f3f');
	}
	else {
		$('#Fecha').css('border', '');
	}

	var turno = $("#Turnos option:selected").val();
	if (turno === "0") {
		errorR++;
		/*$("#Turnos").each(function () {
			$(this).siblings(".select2-container").css('border', '2px solid red');
		});*/
		$("#Turnos").css('border', '2px solid #e03f3f');
	}
	else {
		$("#Turnos").css('border', '');
	}

	var prioridad = $("#Prioridades option:selected").val();
	if (prioridad === "0") {
		errorR++;
		/*$("#Turnos").each(function () {
			$(this).siblings(".select2-container").css('border', '2px solid red');
		});*/
		$("#Prioridades").css('border', '2px solid #e03f3f');
	}
	else {
		$("#Prioridades").css('border', '');
	}

	var horno = $("#selectHorno option:selected").val();
	if (horno === "0") {
		errorR++;
		$("#selectHorno").each(function () {
			$(this).siblings(".select2-container").css('border', '2px solid red');
		});
	}
	else {
		$("#selectHorno").each(function () {
			$(this).siblings(".select2-container").css('border', '');
		});
	}

	var pedido = $("#selectPosProd option:selected").val();
	if (pedido === "0") {
		errorR++;
		$("#selectPosProd").each(function () {
			$(this).siblings(".select2-container").css('border', '2px solid red');
		});
	}
	else {
		$("#selectPosProd").each(function () {
			$(this).siblings(".select2-container").css('border', '');
		});
	}

	var noEstilo = $("#selectEstilosProd option:selected").val();
	if (noEstilo === "") {
		errorR++;
		$("#selectEstilosProd").each(function () {
			$(this).siblings(".select2-container").css('border', '2px solid red');
		});
	}
	else {
		$("#selectEstilosProd").each(function () {
			$(this).siblings(".select2-container").css('border', '');
		});
	}

	var maquina = $("#selectMachine option:selected").val();
	if (maquina === "") {
		errorR++;
		$("#selectMachine").each(function () {
			$(this).siblings(".select2-container").css('border', '2px solid red');
		});
	}
	else {
		$("#selectMachine").each(function () {
			$(this).siblings(".select2-container").css('border', '');
		});
	}

	enviarPlaneacion(cadena, errorR);

}


function enviarPlaneacion(cadena, errorR) {

	if (errorR !== 0) {
		var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
		alert.set({ transition: 'zoom' });
		alert.set('modal', false);
	} else {

		var horno = $("#selectHorno option:selected").val();
		var turno = $("#Turnos option:selected").val();
		var prioridad = $("#Prioridades option:selected").val();
		var pedido = $("#selectPos option:selected").val();
		var maquina = $("#selectMachine option:selected").val();
		var estilo = $("#selectEstilosProd option:selected").val();
		var fecha = $("#Fecha").val();
		var metedor = $("#Metedor").val();
		var sacador = $("#Sacador").val();
		var cachador = $("#Cachador").val();


		$.ajax({
			url: "/ProductionPlan/Obtener_Production_Planning",
			datatType: 'json',
			data: JSON.stringify({
				IdProdOverMachine: maquina, IdTurno: turno, IdPedido: pedido, IdSummary: estilo, Fecha: fecha, Metedor: metedor, Sacador: sacador, Cachador:cachador, IdPrioridad: prioridad/* ListEstilos: cadena*/
			}),
			cache: false,
			type: 'POST',
			contentType: 'application/json',
			success: function (data) {
				alertify.set('notifier', 'position', 'top-right');
				alertify.notify('Production planning was registered correctly.', 'success', 5, null);

			}
		});
	}

}

function TablaPlaneacion() {
	var tempScrollTop = $(window).scrollTop();
	
	
	$.ajax({
		url: "/ProductionPlan/Obtener_Planeacion_General/",
		type: "POST",
		contentType: "application/json;charset=UTF-8",
		dataType: "json",
		success: function (jsonData) {
			var html = '';

			var lista_planeacion = jsonData.Data.listPlanGnl;
			var lista_Maquinas = jsonData.Data.listaMaquinas;
			var contadorMaq = 1;
			//html += '<tr> <th> # </th>';
			html += '<tr><th style="text-align:center"> # MACHINE </th>';
			html += '<th style="text-align:center"> DATE </th>';
			html += '<th style="text-align:center"> STYLE </th>';
			html += '<th style="text-align:center"> METEDOR </th>';
			html += '<th style="text-align:center"> CACHADOR </th>';
			html += '<th style="text-align:center"> SACADOR </th>';
			html += '<th style="text-align:center"> STATUS </th>';
			html += '<th style="text-align:center"> ACTIONES </th></tr>';
			var contadorPlan = 1;
			$.each(lista_planeacion, function (key, itemP) {
				var d = new Date(itemP.FechaPlan);
				html += '<tr>';
				// diaMa = "Tuesday";
				//if (diaMa === itemP.DiaFecha) {
				//html += '<td id="ext"><input type="text" id="estilo" class=" txtDes form-control estilo" value="' + itemP.IdProductionPlan + '" readonly/></td>';
				html += '<td id="noMq" align="center"><input type="text" id="noMaquina" class=" txtDes form-control noMaquina" style=" background-color:transparent; border: 0; box-shadow: none; width:100px" value="' + itemP.ProdOverMachine.NoMaquina + '" readonly/></td>';
				html += '<td id="fechaPP" align="center"><input type="text" id="fechaEstilo" class=" txtDes form-control fechaEstilo" style=" background-color:transparent; border: 0; box-shadow: none; width:130px;" value="' + itemP.FechaPlan + '" readonly/></td>';
				html += '<td id="descS" align="center"><input type="text" id="descEstilo" class=" txtDes form-control descEstilo" style=" background-color:transparent; border: 0; box-shadow: none; width:200px;" value="' + itemP.ItemDescripcion.Descripcion + '" readonly/></td>';				
				html += '<td id="metedorP" align="center"><input type="text" id="metedor" class=" txtDes form-control metedor" style=" background-color:transparent; border: 0; box-shadow: none; width:200px; text-transform:uppercase;" value="' + itemP.Metedor + '" readonly/></td>';	
				html += '<td id="sacadorP" align="center"><input type="text" id="sacador" class=" txtDes form-control sacador" style=" background-color:transparent; border: 0; box-shadow: none; width:200px; text-transform:uppercase;" value="' + itemP.Sacador + '" readonly/></td>';
				html += '<td id="cachadorP" align="center"><input type="text" id="cachador" class=" txtDes form-control cachador" style=" background-color:transparent; border: 0; box-shadow: none; width:200px; text-transform:uppercase;" value="' + itemP.Cachador + '" readonly/></td>';	
				html += '<td id="estadoP" align="center"><input type="text" id="estado" class=" txtDes form-control estado" style=" background-color:transparent; border: 0; box-shadow: none; width:200px; text-transform:uppercase;" value="' + itemP.Status + '" readonly/></td>';	
				html += '<td> <a href="#"  onclick="cargarFormEditPlaneacion(' + itemP.IdProductionPlan + ', \'' + itemP.ProdOverMachine.NoMaquina+ '\')" class="btn edit_driver edicion_driver btnEdit" Title="Planning Edit"> <span class="glyphicon glyphicon-edit" aria-hidden="true" style="padding: 0px !important;"></span></a></td>';
				//}
				html += '</tr>';
				contadorPlan++;
			});
			if (Object.keys(lista_planeacion).length === 0) {
				html += '<tr class="odd"><td valign="middle" align="center" colspan="10" class="dataTables_empty">No sizes were found for the style.</td></tr>';
			}
			$('.tbodysGenPlan').html(html);
			
			//obtenerIdEstilo(IdEstilo);
			$("#loading").css('display', 'none');
			$(window).scrollTop(tempScrollTop);
		},
		error: function (errormessage) { alert(errormessage.responseText); }
	});
	$("#exampleModalCenter").modal('show');
}

function cargarFormEditPlaneacion(idPlaneacion, idMaquina) {
	$("#IdProductionPlan").val(idPlaneacion);

	var actionData = "{'id':'" + idPlaneacion + "','idMaquina':'" + idMaquina + "'}";
	$.ajax({
		url: "/ProductionPlan/Obtener_Informacion_Planeacion_Edit/",
		type: "POST",
		data: actionData,
		contentType: "application/json;charset=UTF-8",
		dataType: "json",
		success: function (jsonData) {
			var reporte = jsonData.Data.planeacion;
			var fechas = new Date(parseInt(reporte.FechaAct.substr(6)));
			$("#TurnosAct").val(reporte.IdTurno).change();
			$("#FechaAct").val(fechas.format("mm/dd/yyyy"));
			$("#selectHornoAct").val(reporte.IdHorno).change();
			ActualizarSelectMaquinaOven(reporte.IdHorno, reporte.IdProdOverMachine);			
			$("#selectPosProdAct").val(reporte.IdPedido).change();
			ActualizarSelectEstilosActPorPO(reporte.IdPedido, reporte.IdSummary);			
			$('#PrioridadesAct').val(reporte.IdPrioridad).change();
			$("#CachadorAct").val(reporte.CachadorAct);
			$("#MetedorAct").val(reporte.MetedorAct);
			$("#SacadorAct").val(reporte.SacadorAct);
			var idStatus;
			if (reporte.Status === "ABIERTO") {
				idStatus = 1;
			} else {
				idStatus = 2;
			}
			$("#Estatus").val(idStatus).change();
	
			$("#contenedorEdit").css("display", "inline");


		},
		error: function (errormessage) {
			alert(errormessage.responseText);
		}
	}).done(function (data) {

	});

}

function actualizarPlaneacion() {

	var errorR = 0;

	var metedor = $("#MetedorAct").val();
	if (metedor === "") {
		errorR++;
		$('#MetedorAct').css('border', '2px solid #e03f3f');
	}
	else {
		$('#MetedorAct').css('border', '');
	}

	var sacador = $("#SacadorAct").val();
	if (sacador === "") {
		errorR++;
		$('#SacadorAct').css('border', '2px solid #e03f3f');
	}
	else {
		$('#SacadorAct').css('border', '');
	}

	var cachador = $("#CachadorAct").val();
	if (cachador === "") {
		errorR++;
		$('#CachadorAct').css('border', '2px solid #e03f3f');
	}
	else {
		$('#CachadorAct').css('border', '');
	}

	var fecha = $("#FechaAct").val();
	if (fecha === "") {
		errorR++;
		$('#FechaAct').css('border', '2px solid #e03f3f');
	}
	else {
		$('#FechaAct').css('border', '');
	}

	var turno = $("#TurnosAct option:selected").val();
	if (turno === "0") {
		errorR++;
		/*$("#Turnos").each(function () {
			$(this).siblings(".select2-container").css('border', '2px solid red');
		});*/
		$("#TurnosAct").css('border', '2px solid #e03f3f');
	}
	else {
		$("#TurnosAct").css('border', '');
	}

	var prioridad = $("#PrioridadesAct option:selected").val();
	if (prioridad === "0") {
		errorR++;
		/*$("#Turnos").each(function () {
			$(this).siblings(".select2-container").css('border', '2px solid red');
		});*/
		$("#PrioridadesAct").css('border', '2px solid #e03f3f');
	}
	else {
		$("#PrioridadesAct").css('border', '');
	}

	var horno = $("#selectHornoAct option:selected").val();
	if (horno === "0") {
		errorR++;
		$("#selectHornoAct").each(function () {
			$(this).siblings(".select2-container").css('border', '2px solid red');
		});
	}
	else {
		$("#selectHornoAct").each(function () {
			$(this).siblings(".select2-container").css('border', '');
		});
	}

	var pedido = $("#selectPosProdAct option:selected").val();
	if (pedido === "0") {
		errorR++;
		$("#selectPosProdAct").each(function () {
			$(this).siblings(".select2-container").css('border', '2px solid red');
		});
	}
	else {
		$("#selectPosProdAct").each(function () {
			$(this).siblings(".select2-container").css('border', '');
		});
	}

	var noEstilo = $("#selectEstilosProdAct option:selected").val();
	if (noEstilo === "") {
		errorR++;
		$("#selectEstilosProdAct").each(function () {
			$(this).siblings(".select2-container").css('border', '2px solid red');
		});
	}
	else {
		$("#selectEstilosProdAct").each(function () {
			$(this).siblings(".select2-container").css('border', '');
		});
	}

	var maquina = $("#selectMachineAct option:selected").val();
	if (maquina === "") {
		errorR++;
		$("#selectMachineAct").each(function () {
			$(this).siblings(".select2-container").css('border', '2px solid red');
		});
	}
	else {
		$("#selectMachineAct").each(function () {
			$(this).siblings(".select2-container").css('border', '');
		});
	}

	enviarActualizacionPlaneacion(errorR);

}


function enviarActualizacionPlaneacion(errorR) {

	if (errorR !== 0) {
		var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
		alert.set({ transition: 'zoom' });
		alert.set('modal', false);
	} else {

		var horno = $("#selectHornoAct option:selected").val();
		var turno = $("#TurnosAct option:selected").val();
		var prioridad = $("#PrioridadesAct option:selected").val();
		var pedido = $("#selectPosProdAct option:selected").val();
		var maquina = $("#selectMachineAct option:selected").val();
		var estilo = $("#selectEstilosProdAct option:selected").val();
		var fecha = $("#FechaAct").val();
		var metedor = $("#MetedorAct").val();
		var sacador = $("#SacadorAct").val();
		var cachador = $("#CachadorAct").val();
		var idPlan=  $("#IdProductionPlan").val();


		$.ajax({
			url: "/ProductionPlan/Actualizar_Production_Planning",
			datatType: 'json',
			data: JSON.stringify({
				IdProductionPlan:idPlan, IdProdOverMachine: maquina, IdTurno: turno, IdPedido: pedido, IdSummary: estilo, FechaAct: fecha, MetedorAct: metedor, SacadorAct: sacador, CachadorAct: cachador, IdPrioridad: prioridad/* ListEstilos: cadena*/
			}),
			cache: false,
			type: 'POST',
			contentType: 'application/json',
			success: function (data) {
				alertify.set('notifier', 'position', 'top-right');
				alertify.notify('Production planning was modified correctly.', 'success', 5, null);
				$("#contenedorEdit").css("display", "none");
			}
		});
	}

}
