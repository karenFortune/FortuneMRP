﻿$(document).ready(function () {
    var ID = $("#IdPedido").val();
    buscar_estilos(ID);
    $("#div_tabla_packing").css('display', 'none');

});


$(document).on("click", "#btnDone", function () {
    window.location.reload();
});

function probar() {
    $('#tabless tr').on('click', function (e) {
        $('#tabless tr').removeClass('highlighted');
        $(this).addClass('highlighted');
    });   
}

$(document).on("dblclick", "#tabless tr", function () {
	var row = this.rowIndex;
	if (row !== 0) {
		var numEstilo = $('#tabless tr:eq(' + row + ') td:eq(0)').html();
		//var estilo = $('#tabless tr:eq(' + row + ') td:eq(2)').html();
		obtenerListaTallas(numEstilo);
	}

});

$(document).on("input", ".numeric", function () {
    this.value = this.value.replace(/\D/g, '');
});

//Autocomplete tallas
$(function () {
    var list_datalist = Array();
    $.ajax({
        url: '/Tallas/Lista_Tallas',
        type: 'GET',
        success: function (data) {
            for (var i = 0; i < data.length; i++) {
                var html = "";
                list_datalist.push(data[i].Talla);
            }
        }
    });
    var availableTags = list_datalist;
    $(document).on("focus keyup", "input.talla", function (event) {
        $(this).autocomplete({
            source: availableTags,
            select: function (event, ui) {
                event.preventDefault();
                this.value = ui.item.label;
            },
            focus: function (event, ui) {
                event.preventDefault();
                this.value = ui.item.label;
            }
        });
    });
});

function buscar_estilos(ID) {
    var tempScrollTop = $(window).scrollTop();
    $.ajax({
        url: "/Pedidos/Lista_Estilos_PO/" + ID,
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var html = '';
            var html2 = '';
            var lista_estilo = jsonData.Data.listaItem;
            var noCliente;
            html2 += '<tr><th style="border-top-left-radius:0px !important;"># </th> ';
            $.each(lista_estilo, function (key, item) {
                noCliente = item.NumCliente
            });
            if (noCliente == "2") {
                html2 += '<th>PO FANTASY#</th>';
            }
            html2 += '<th>ITEM</th>' +
                ' <th>ITEM DESCRIPTION</th>' +
                '<th>COLOR CODE</th>' +
                '<th>COLOR DESCRIPTION</th>' +
                '<th>FORM PACK</th>' +
                '<th>INSTR.</th>' +
                '<th>QTY</th>' +
                '<th>PRICE</th>' +
                '<th>TOTAL</th>' +
                '</tr>';
            $(".encabezadoPack").html(html2);
            $.each(lista_estilo, function (key, item) {
                html += '<tr  onclick="probar()">';
                html += '<td>' + item.IdItems + '</td>';
                if (item.NumCliente === "2" || item.NumCliente === 2) {
                    var poF = item.POFantasy === null ? "-" : item.POFantasy;
                    html += '<td>' + poF + '</td>';
                } 

                html += '<td>' + item.EstiloItem + '</td>';
                html += '<td>' + item.ItemDescripcion.Descripcion + '</td>';
                html += '<td>' + item.CatColores.CodigoColor + '</td>';
				html += '<td>' + item.CatColores.DescripcionColor + '</td>';
                html += '<td>' + item.CatTipoFormPack.TipoFormPack + '</td>';
                var estatus = item.HistorialPacking;
                if (estatus !== 0) {
                    html += '<td>X</td>';
                } else {
                    html += '<td>-</td>';
                }
                html += '<td>' + item.Cantidad + '</td>';
                html += '<td>' + item.Price + '</td>';
                html += '<td>' + item.Total + '</td>';
                //html += '<td><a href="#" onclick="obtenerListaTallas(' + item.IdItems + ');" class = "btn btn-default glyphicon glyphicon-search l1s" style = "color:black; padding:0px 5px 0px 5px;" Title = "Sizes"></a></td>';
                html += '</tr>';
            });
            if (Object.keys(lista_estilo).length === 0) {
                html += '<tr class="odd"><td valign="middle" align="center" colspan="10" class="dataTables_empty">No styles were found for the PO.</td></tr>';

            }
            $('.tbody').html(html);
            $("#div_estilos_orden").css("display", "inline"); 
            $(window).scrollTop(tempScrollTop);
        },
        error: function (errormessage) { alert(errormessage.responseText); }
    });
}

var estiloId;
var tipoEmp = "";
var datosPO = "";
var tUnidades = 0;
function obtenerListaTallas(EstiloId) {
    $("#loading").css('display', 'inline');
    $("#panelHotTopic").css('display', 'inline');
    $("#InfoSummary_IdItems").val(EstiloId);	
    $("#IdSummaryOrden").val(EstiloId);
    estiloId = EstiloId;
    $.ajax({
        url: "/Packing/Lista_Tallas_HT_Por_Estilo/" + EstiloId,
        method: 'POST',
        dataType: "json",
        success: function (jsonData) {
            //var listaT = jsonData.Data.listaTalla;
            //var listaPacking = jsonData.Data.listaPackingS;
			var listaPO = jsonData.Data.lista;
			var listaQty = jsonData.Data.listaT;
            var listaPBulk = jsonData.Data.listaPTBulk;
			var listaEPPK = jsonData.Data.listaEmpPPK;
			var listaEmpBulk = jsonData.Data.listaEmpBulk;
            var listaPPPK = jsonData.Data.listaPTPPK;
            var listaTCajas = jsonData.Data.listaTotalCajasPack;
            //var listaTCajas = jsonData.Data.listaCajasT;
            var html = '';
			var estilos = jsonData.Data.estilos;
			var cargo = jsonData.Data.cargoUser;
			var lista_estilo_Descrip = jsonData.Data.lista;
			var EstiloDescrip;
			$.each(lista_estilo_Descrip, function (key, item) {

				EstiloDescrip = item.DescripcionEstilo;

			});
            $("#btnAdd").hide();
			$("#nuevaTalla").hide();
			$("#div_Desc_Estilo").show();
			$("#div_Desc_Estilo").html("<h2>Item: " + estilos + "-" + $.trim(EstiloDescrip) + "</h2>"); 
               $("#div_estilo_ht").html("<h3>QUALITY OF SIZES</h3>");
                    html += '<tr> <th width="30%"> Size </th>';
                $.each(listaPO, function (key, item) {
                    html += '<th>' + item.Talla + '</th>';
                });
			html += '<th width="30%"> Total </th>'; 
			html += '</tr><tr><td width="30%">Total Orden</td>';
			var cantidades_total = 0;
			var cadena_cantidades_total = "";
			var lPOtotalorden = listaPO.length;

			$.each(listaPO, function (key, item) {
				html += '<td class="">' + item.Cantidad + '</td>';
				cantidades_total += item.Cantidad;
				cadena_cantidades_total += "*" + item.Cantidad;
			});
			var cantidades_array_total = cadena_cantidades_total.split('*');
			html += '<td>' + cantidades_total + '</td>';
                html += '</tr><tr><td width="30%">1rst QTY</td>';
                var cantidades = 0;
            var cadena_cantidades = "";
			var lPOTotal = listaQty.length;         
              
			$.each(listaQty, function (key, item) {
                    html += '<td class="calidad">' + item.Cantidad + '</td>';
                    cantidades += item.Cantidad;
                    cadena_cantidades += "*" + item.Cantidad;
                });
                var cantidades_array = cadena_cantidades.split('*');
                html += '<td>' + cantidades + '</td>';

           /* var cantidadesEmpBulk = 0;
            html += '</tr><tr><td width="30%">Bulk - #Pieces</td>';
            var listaTBatchBulk = 0;
            $.each(listaTCajas, function (key, item) {
                listaTBatchBulk++;
            });
          
            $.each(listaPBulk, function (key, item) {
                if (listaTBatchBulk === 0) {
                    item = 0;
                    html += '<td>' + item + '</td>';
                } else {
                    html += '<td>' + item + '</td>';
                }                
                cantidadesEmpBulk += item;              
            });
            
            html += '<td>' + cantidadesEmpBulk + '</td>';
            html += '</tr><tr><td width="30%">Packed</td>';
            var cantidades_PBulk = "";          
            var listaTBatch = 0;
            $.each(listaTCajas, function (key, item) {
                listaTBatch++;
            });
            if (listaTBatch === 0) {
                listaTCajas = listaPBulk;
            } else {
                listaTCajas;
            }
            $.each(listaTCajas, function (key, item) {
                if (listaTBatch === 0) {
                    item = 0;
                    html += '<td class="qtyBulk">' + item + '</td>';
                } else {
                    html += '<td class="qtyBulk">' + item + '</td>';
                }                  
                cantidades_PBulk += "*" + item;
            });
            var cantidades_array_pbulk = cantidades_PBulk.split('*');*/
           
            var total = 0;            
			var cantidadesEmpPPK = 0;
            var cantidadesEmpBulkT = 0;
            var cantidadesPackedB = 0;
            var cantidadesPackedP = 0;
			var listaTallasPPK = listaEPPK.lenght;
			var listaTallasBulk = listaEmpBulk.lenght;
			var totalTallas = 0;
			var cont = 0;
			$.each(listaEmpBulk, function (key, item) {
				cont = cont + 1;
				html += '</tr><tr><td width="30%">BULK #PCS- PO#' + item.NumberPO + '</td>';

				$.each(item.ListaEmpaque, function (key, i) {
					html += '<td>' + i.Cantidad + '</td>';
					cantidadesEmpBulkT += i.Cantidad;
				});
				//html += '<td>' + cantidadesEmpPPK + '</td>';

				html += '</tr><tr id="empaque" class="empaque"><td width="30%">Packed</td>';
				$.each(item.ListaEmpaque, function (key, i) {

					html += '<td class="qtyPPK">' + i.TotalBulk + '</td>';
                    cantidadesPackedB += i.TotalBulk;
				});
                html += '<td>' + cantidadesPackedB + '</td>';
			});
            
		
            $.each(listaEPPK, function (key, item) {
                cont = cont + 1;
                    html += '</tr><tr><td width="30%">PPK - #Ratio- PO#' + item.NumberPO+'</td>';
                  
                $.each(item.ListaEmpaque, function (key, i) {
                    html += '<td>' + i.Ratio + '</td>';
                    cantidadesEmpPPK += i.Ratio;               
                });
                //html += '<td>' + cantidadesEmpPPK + '</td>';
				
                html += '</tr><tr id="empaque" class="empaque"><td width="30%">Packed</td>';
                $.each(item.ListaEmpaque, function (key, i) {

					html += '<td class="qtyPPK">' + i.TotalRatio + '</td>';
                    cantidadesPackedP += i.TotalBulk;
                });
                html += '<td>' + cantidadesPackedP + '</td>';
				html += '</tr>';
			});

		

            html += '<tr><td width="30%">+/-</td>';
            var totales = 0;
            var i = 1;           
			var sum = 0;

			$.each(listaTCajas, function (key, item) {	
				/*var valores = 0;
				var totalDeuda = 0;
				$(".empaque").each(function () {
					//totalDeuda += parseInt($(this).html()) || 0;
					totalDeuda += $(".qtyPPK").parent("tr").find("td").eq(i).text();
					$(".qtyPPK").parents("tr").find("td").eq(i).each(function () {
						valores += parseFloat($(this).text()) + "\n";
						
					});
				});
                var valorCalidad = $(".calidad").parent("tr").find("td").eq(i).text();
				var valorQtyBulk = $(".qtyBulk").parent("tr").find("td").eq(i).text();
				var qtyPPKResult = ".qtyPPK" + cont;
				
				var valorQtyPPK = $(".qtyPPK").parent("tr").find("td").eq(i).text();                
                var qtyPO = parseInt(valorCalidad);             
                var qtyValorB = parseInt(valorQtyBulk);
                var qtyValorP = parseInt(valorQtyPPK);
                if (isNaN(qtyPO)) {
                    qtyPO = 0;
                }
                if (isNaN(qtyValorB)) {
                    qtyValorB = 0;
                }
                if (isNaN(qtyValorP)) {
                    qtyValorP = 0;
                }
                sum = qtyValorB + qtyValorP;
                if (isNaN(sum)) {
                    sum = 0;
                }
                var resta = sum - qtyPO;
                if (isNaN(resta)) {
                    resta = 0;
                }
               // var resta = (parseFloat(cantidades_array[i]) - parseFloat(item))
                if (resta === 0) {
                    html += '<td class="faltante" style="color:black;">' + resta + '</td>';
                } else if (resta >= 0) {
                    html += '<td class="faltante" style="color:blue;">' + resta + '</td>';
                } else {
                    html += '<td class="faltante" style="color:red;">' + resta + '</td>';
                }*/
               html += '<td class="faltante" ></td>';
               i++;
            });
			//html += '<td class="faltante" ></td>';
            html += '</tr>';          

		
			$('.tbodyPHT').html(html);
            var nColumnas = $("#tablePackingHT tr:last td").length;
            var totalRows = $("#tablePackingHT tr").length;
			for (var v = 1; v < lPOTotal+1; v++) {
                 datosPO += "*" + $('#tablePackingHT tr:eq(2) td:eq(' + v + ')').html();
              }

            var temp = "";
            var arrayCantidades = new Array();
          
			var mArray = new Array();
			var mArrayResult = new Array();
            
            for (var z = 0; z < totalRows; z++) {
                var valor = $('#tablePackingHT tr:eq(' + z + ') td:eq(0)').html();
                var contener = "";
                if ( valor !== undefined) {
					contener = valor.includes('Packed');
                }               
                if (contener === true) {
					for (var j = 1; j < lPOTotal+1; j++) {
                        temp += "*" + $('#tablePackingHT tr:eq(' + z + ') td:eq(' + j + ')').html();
                    }
                    arrayCantidades.push(temp);
                    temp = "";
                }                
			}
            mArray[arrayCantidades.lenght];
            for (var x = 0; x < arrayCantidades.length; x++) {               
				var cantidadesV = arrayCantidades[x].split("*");
				
				mArray[x] = cantidadesV.map(function (x) {
					//mArrayResult[x] += mArray[x];
                        return parseInt(x, 10);
				});	

		
			}

			var numTallas = 0;
			$.each(mArray, function (key, item) {
				numTallas++;
			});

            var iDatosPO = datosPO.split("*");
            var resultPO = iDatosPO.map(function (x) {
                return parseInt(x, 10);
            });
            //var l = 0;
			if (numTallas !== 0) {

			
			var suma = 0;
            var valorFalt = 0;
            var val = parseInt(resultPO.length);//val
			for (var l = 1; l < mArray[0].length; l++) {
				suma = 0;
				for (var r = 0; r < arrayCantidades.length; r++) {
					suma += mArray[r][l];
                }
				mArrayResult[l]=suma;
            }
          
			mArrayResult[0] = 0;
			for (var m = 1; m < val; m++) {
				var totalC = mArrayResult[m] - resultPO[m];
				resultPO[m] = totalC;				
				$('#tablePackingHT tr:eq(' + (totalRows - 1) + ') td:eq(' + m + ')').html(resultPO[m]);
				valorFalt = $(".faltante").parent("tr").find("td").eq(m).text();
				var qtyFalt = parseInt(valorFalt);

				if (qtyFalt === 0) {
					$(".faltante").css('color', '1px solid black');
				} else if (qtyFalt >= 0) {
					$(".faltante").css('color', '1px solid blue');
				} else {
					$(".faltante").css('color', '1px solid #e03f3f');
				}
			}
			}
            
            
            if (listaPBulk.length === 0 && listaEPPK.length === 0) {
				if (cargo === 1 ) {
					if (listaPBulk.length !== 0) {
						$("#btnNext").prop("disabled", false);
					} else {
						$("#btnNext").prop("disabled", true);						
					}
					$("#consultaTallaHT").css('height', '2200px');
					TallasEmpaqueBulkHT(EstiloId);					
				} else {
					$("#grupoBotones").hide();
					$('label[id="numTotalUnitLabel"]').hide();
					$("#numTotalUnit").hide();
					$("#panelNoEstilosHT").css('display', 'inline');					
					$("#consultaTallaHT").css('height', '1080px');
					$("#containerHTPie").css('display', 'none');
					$("#tablaTallasBulkHT").hide();
					$("#tablaTallasPPKHT").hide();
					$("#tablaTallasPalletHT").hide();

					
				}
			} else {
				if (cargo === 1 || cargo === 9) {
					$("#consultaTallaHT").css('height', '2200px');
					$("#grupoBotones").hide();
					$("#div_titulo_Register").css("display", "inline");
					$("#div_titulo_Register").html("<h3>REGISTRATION OF PALLET</h3>");
					$('label[for="Packing_CantBox"]').hide();
					$("#numeroCajas").hide();
					$('label[for="Packing_CantidadPPKS"]').hide();
					$("#Packing_CantidadPPKS").hide();
					$("#opcionesRegistro").css("display", "inline");
					$('label[id="numTotalUnitLabel"]').hide();
					$("#numTotalUnit").hide();
					$("#div_titulo_Bulk").css("display", "none");
					$("#opciones").css("display", "none");		
					$("#panelNoEstilosHT").css('display', 'none');
					$("#tablaTallasBulkHT").hide();
					$("#tablaTallasPPKHT").hide();
					$("#tablaTallasPalletHT").hide();
					obtener_bacth_estilo(estiloId);
				} else {
					$("#grupoBotones").hide();
					$('label[id="numTotalUnitLabel"]').hide();
					$("#numTotalUnit").hide();
					$("#panelNoEstilosHT").css('display', 'inline');
					obtener_bacth_estilo(estiloId);
				} 
            }            
      
            $("#consultaTallaHT").css("visibility", "visible");
            $("#arte").css("display", "inline-block");
			var datoItem = $("#InfoSummary_IdItems").val();
			obtenerImagenPNL(estilos, datoItem);
            obtenerImagenArte(estilos);
           // $("#loading").css('display', 'none');
            setTimeout(function () { $("#loading").css('display', 'none'); }, 3000);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {

    });
}

function TallasEmpaqueBulkHT(idEst) {
	var actionData = "{'idEst':'" + idEst + "'}";
	$("#tablaTallasPPKHT").hide();
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
            $('#btnNuevoPPK').hide();
                $("#btnNuevo").prop("disabled", true);               
            $("#btnDone").prop("disabled", true);     
            $('label[id="numTotalUnitLabel"]').hide();
            $("#numTotalUnit").hide(); 
            $('#listaTallaBatchHT').hide(); 
            $('#listaBatchHT').css("display", "none");  
                $("#div_titulo_Bulk").html("<h3>REGISTRATION OF TYPE OF PACKAGING - BULK</h3>");
            $("#div_titulo_Bulk").css("display", "inline"); 
            $("#opciones").css("display", "inline");
                $("#div_titulo_Register").css("display", "none");
                $("#opcionesRegistro").css("display", "none");                          
            
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
                    html += '<td width="20%"><input type="text" name="l-cantidad" id="l-cantidad" class="form-control numeric qty " onkeyup="obtTotalCartones(' + cont + ')" value="' + 0 + '"  /></td>';
                    html += '<td width="20%"><input type="text" name="l-cartones" id="l-cartones" class="form-control numeric cart " value="' + 0 + '"  readonly/></td>';
                    html += '<td width="20%"><input type="text" name="l-partial" id="l-partial" class="form-control numeric part " value="' + 0 + '"  readonly/></td>';
                html += '<td width="20%"><input type="text" name="l-totCartones" id="l-totCartones" class="form-control numeric tcart " value="' + 0 + '"  readonly/></td>';
                   // html += '<td width="250"><button type="button" id="btnDelete" class="deleteTalla btn btn btn-danger btn-xs" value="4">Delete</button></td>';
                    html += '</tr>';
                });
			//html += '</tbody> </table>'; 
			$('.packBulkReg').html(html);
                htmlB += '<button type="button" id="nuevoEmpaqueBulkHT" class="btn btn-success btn-md pull-right btn-sm"><span class="glyphicon glyphicon-floppy-disk" aria-hidden="true"></span> Save</button>';               
                $('#listaTallaPHT').html(htmlB);
		
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {
    });
}
var cadenaCantidad = ""; 
var numTotalCartones;
function RegistrarEmpaqueBulkHT(nPO, tEmpaque) {
    if (nPO === "" || nPO === null) {
        nPO = 0;
	}
	$("#tablaTallasBulkHT").hide();
	$("#tablaTallasPPKHT").hide();
	$("#tablaTallasPalletHT").show();
	
    var actionData = "{'estiloId':'" + estiloId + "','nPO':'" + nPO + "','tEmpaque':'" + tEmpaque + "'}";
    $.ajax({
        url: "/Packing/Lista_Tallas_Empaque_HT_Por_Estilo/",
        type: 'POST',
        data: actionData,
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var listaT = jsonData.Data.listaTalla;
            var listaPO = jsonData.Data.lista;
            var listaPacking = jsonData.Data.listaPackingS;
            var listaPackingBox = jsonData.Data.listaPackingBox;
			var html = '';  
			var htmlB = '';

            /*html += '<table class="table" id="tablaTallasPalletHT"><thead>';
            html += '<tr><th>Size</th>' +
				' <th>Box#</th>' +
				' <th>PIECES#</th>' +
                ' <th>QTY</th>' +
				' <th>TOTALCARTONS#</th>' +
				' <th>CARTONSFALTANTES#</th>' +
                '</tr>' +
                '</thead><tbody class="tbodyHTPack">';*/
            if (listaPackingBox.length === 0) {
                listaPackingBox = listaPacking;
            } else {
                listaPackingBox;
            }
       
            if (listaPackingBox.length !== 0) {
            var totalTallas = listaPackingBox.length;
                var tallaTemp = listaPackingBox[0].Talla;
                var cajaTemp = 0;
                if (listaPackingBox[0].PackingM !== null) {
                   // cajaTemp = parseFloat(listaPackingBox[0].PackingM.CantBox);
                }
				var cantTemp = parseFloat(listaPackingBox[0].Cantidad);
				var cantTempPart = parseFloat(listaPackingBox[0].PartialNumber);
                var cartonsTemp = parseFloat(listaPackingBox[0].TotalCartones);
                var i = 0;
                var cont = 0;
                var indicador = 1;
				var valorTempCaja = 0;
				var totalPiezas = 0;
				numTotalCartones = cartonsTemp;
            $.each(listaPackingBox, function (key, item) {
                    i++;
                if (tallaTemp === item.Talla) {
                    if (item.PackingM !== null)
                    {
                       cajaTemp += parseFloat(item.PackingM.CantBox);
                    } 
                   
                } else {
					cadenaCantidad += cajaTemp + "*";
                    cont = cont + 1;
                    html += '<tr id="pallet' + cont + '" class="pallet">';
					html += '<td width="20%"><input type="text" id="f-talla" class="form-control talla" value="' + tallaTemp + '" readonly/></td>';
					if (cajaTemp < cartonsTemp) {
						var resulCajas = cartonsTemp - cajaTemp;
                        html += '<td width="20%" class="bCajas"><input type="text" name="l-cajas" id="l-cajas' + cont + '" class="form-control numeric caja " onfocus="focusingPalletBulk(' + cont + ')"   value="' + cartonsTemp + '"  /></td>';//onkeyup="obtTotalCartonesBulkHT(' + cont + ')"
					} else {						
						html += '<td width="20%" class="bCajas"><input type="text" name="l-cajas" id="l-cajas'+cont+'" class="form-control numeric caja "  value="' + cajaTemp+ '"  readonly/></td>';
					}
					if (cajaTemp === cartonsTemp) {
						totalPiezas = cantTemp;
						html += '<td width="20%"><input type="text" name="l-cantidadPcs" id="l-cantidadPcs'+cont+'" class="form-control numeric qtyPcs "  value="' + totalPiezas + '"  readonly/></td>';
					} else {
                        html += '<td width="20%"><input type="text" name="l-cantidadPcs" id="l-cantidadPcs' + cont + '" class="form-control numeric qtyPcs " onfocus="focusingPalletBulkPcs(' + cont + ')" value="' + cantTemp + '"  /></td>';
					}
					
					html += '<td width="20%"><input type="text" name="l-cantidad" id="l-cantidad" class="form-control numeric qtyBox " value="' + cantTemp + '"  readonly/></td>';
					html += '<td width="20%"><input type="text" name="l-cantidadPart" id="l-cantidadPart" class="form-control numeric cantPart " value="' + cantTempPart + '"  readonly/></td>';
					html += '<td width="20%"><input type="text" name="l-cartons" id="l-cartons" class="form-control numeric cartones " value="' + cartonsTemp + '"  readonly/></td>';
					if (cajaTemp === 0) {
						html += '<td width="20%"><input type="text" name="l-cartons-falt" id="l-cartons-falt" class="form-control numeric cartonesF " value="' + cartonsTemp + '"  readonly/></td>';
					} else {
						var resulTotalCartones = cartonsTemp - cajaTemp;
						html += '<td width="20%"><input type="text" name="l-cartons-falt" id="l-cartons-falt" class="form-control numeric cartonesF " value="' + parseInt(resulTotalCartones) + '"  readonly/></td>';
					}
					html += '</tr>';
                    tallaTemp = item.Talla;
                    if (item.PackingM !== null) {
                        cajaTemp = parseFloat(item.PackingM.CantBox);
                    } 
                    cantTemp = parseFloat(item.Cantidad);
					cartonsTemp = parseFloat(item.TotalCartones);
					cantTempPart = parseInt(item.PartialNumber);
                }
				if (i === totalTallas) {
					cont = cont + 1;
                    cadenaCantidad += cajaTemp + "*";
					html += '<tr id="pallet' + cont + '" class="pallet">';
                    html += '<td width="20%"><input type="text" id="f-talla" class="form-control talla" value="' + tallaTemp + '" readonly/></td>';
					if (cajaTemp < cartonsTemp) {
						var resulCaja = cartonsTemp - cajaTemp;
                        html += '<td width="20%" class="bCajas"><input type="text" name="l-cajas" id="l-cajas'+cont+'" class="form-control numeric caja " onfocus="focusingPalletBulk(' + cont + ')"  value="' + 0 + '"  /></td>';//onkeyup="obtTotalCartonesBulkHT(' + cont + ')"
                    } else {
						html += '<td width="20%" class="bCajas"><input type="text" name="l-cajas" id="l-cajas'+cont+'" class="form-control numeric caja "  value="' + cajaTemp + '"  readonly/></td>';
					}
					
					if (cajaTemp === cartonsTemp) {
						totalPiezas = cantTemp;
						html += '<td width="20%"><input type="text" name="l-cantidadPcs" id="l-cantidadPcs'+cont+'" class="form-control numeric qtyPcs "   value="' + totalPiezas + '"  readonly/></td>';
					} else {
						html += '<td width="20%"><input type="text" name="l-cantidadPcs" id="l-cantidadPcs'+cont+'" class="form-control numeric qtyPcs "   onfocus="focusingPalletBulkPcs(' + cont + ')" value="' + 0 + '"  /></td>';
					}
					html += '<td width="20%"><input type="text" name="l-cantidad" id="l-cantidad" class="form-control numeric qtyBox " value="' + cantTemp + '"  readonly/></td>';
					html += '<td width="20%"><input type="text" name="l-cantidadPart" id="l-cantidadPart" class="form-control numeric cantPart " value="' + cantTempPart + '"  readonly/></td>';
					html += '<td width="20%"><input type="text" name="l-cartons" id="l-cartons" class="form-control numeric cartones " value="' + cartonsTemp + '"  readonly/></td>';
					if (cajaTemp === 0) {
						html += '<td width="20%"><input type="text" name="l-cartons-falt" id="l-cartons-falt" class="form-control numeric cartonesF " value="' + cartonsTemp + '"  readonly/></td>';
					} else {
						var resultTotalCartones = cartonsTemp - cajaTemp;
						html += '<td width="20%"><input type="text" name="l-cartons-falt" id="l-cartons-falt" class="form-control numeric cartonesF " value="' + parseInt(resultTotalCartones) + '"  readonly/></td>';
					}
					html += '</tr>';
                }
                });
            }

			$('.tbodyHTPack').html(html);
            htmlB += '<button type="button" id="guardarBulkHT" class="btn btn-success btn-md pull-right btn-sm"><span class="glyphicon glyphicon-floppy-disk" aria-hidden="true"></span> Pallet</button>';
            $('#listaTallaPHT').html(htmlB);
            
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {
    });
}

$(document).on("keyup", "input.cantCajas", function () {   
    obtTotalPiezasPPK();
});
var cadenaCantidadPPK = "";
function RegistrarEmpaquePPKHT(nPO, tEmpaque) {
	$("#tablaTallasBulkHT").hide();
	$("#tablaTallasPalletHT").hide();
	
    if (nPO === "" || nPO === null) {
        nPO = 0;
    } 
    var totalUnidades = 0;
    var actionData = "{'estiloId':'" + estiloId + "','nPO':'" + nPO + "','tEmpaque':'" + tEmpaque + "'}";
    $.ajax({
        url: "/Packing/Lista_Tallas_Empaque_HT_Por_Estilo/",
        type: 'POST',
        data: actionData,
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var listaT = jsonData.Data.listaTalla;
            var listaPO = jsonData.Data.lista;
            var listaPacking = jsonData.Data.listaPackingS;
            var listaPackingBox = jsonData.Data.listaPackingBox;
            var html = '';

            html += '<table class="table" id="tablaTallasPalletHT"><thead>';
            html += '<tr><th>Size</th>' +
                ' <th>Ratio#</th>' +
                ' <th>Pieces#</th>' +
                ' <th>QtyPieces#</th>' +
                ' <th>TotalPieces#</th>' +
                '</tr>' +
                '</thead><tbody class="tbodyHTPack">';
            if (listaPackingBox.length === 0) {
                listaPackingBox = listaPacking;
            } else {
                listaPackingBox;
            }

            if (listaPackingBox.length !== 0) {
                var totalTallas = listaPackingBox.length;
                var tallaTemp = listaPackingBox[0].Talla;
                var cajaTemp = 0;
                var totalPiezas = 0;
                var piezasTemp = 0;                
                if (listaPackingBox[0] !== null) {
                    cajaTemp = parseFloat(listaPackingBox[0].Ratio);
                    tUnidades = parseFloat(listaPackingBox[0].TotalUnits);
                  
                 }
                if (listaPackingBox[0].PackingM !== null) {
                    piezasTemp = parseFloat(listaPackingBox[0].PackingM.TotalPiezas);
                }
               // var cantTemp = parseFloat(listaPackingBox[0].Cantidad);
                var i = 0;
                var cont = 0;
                $.each(listaPackingBox, function (key, item) {
                    i++;
                    if (tallaTemp === item.Talla) {                   
                          cajaTemp = parseFloat(item.Ratio);
                        if (item.PackingM !== null) {
                            if (piezasTemp === 0) {
                                piezasTemp += parseFloat(item.PackingM.TotalPiezas);
                            }
                           
                        }

                    } else {
                        cadenaCantidad += cajaTemp + "*";
                        cont = cont + 1;
                        html += '<tr id="pallet' + cont + '" class="pallet">';
                        html += '<td width="20%"><input type="text" id="f-talla" class="form-control talla" value="' + tallaTemp + '" readonly/></td>';           
                        html += '<td width="20%"><input type="text"  name="l-ratio" id="l-ratio" class="form-control numeric ratio " onkeyup="obtTotalPiezasPPK(' + cont + ')"  value="' + cajaTemp + '"  readonly/></td>';                     
                        html += '<td width="20%"><input type="text" name="l-piezas" id="l-piezas" class="form-control numeric piezas " value="' + 0 + '"  readonly/></td>';
                        html += '<td width="20%"><input type="text" name="l-tpiezas" id="l-tpiezas" class="form-control numeric tpiezas " value="' + piezasTemp + '"  readonly/></td>';
                        html += '<td width="20%"><input type="text" name="l-cantTP" id="l-cantTP" class="form-control numeric cantTP " value="' + 0 + '"  readonly/></td>';
                        html += '</tr>';
                        if (item.PackingM !== null) {
                            piezasTemp = parseFloat(item.PackingM.TotalPiezas);
                           
                        }
                        tallaTemp = item.Talla;
                        cajaTemp = parseFloat(item.Ratio);
                    }
                    if (i === totalTallas) {
                        cadenaCantidad += cajaTemp + "*";
                        cont = cont + 1;
                        html += '<tr id="pallet' + cont + '" class="pallet">';
                        html += '<td width="20%"><input type="text" id="f-talla" class="form-control talla" value="' + tallaTemp + '" readonly/></td>';
                        html += '<td width="20%"><input type="text"  name="l-ratio" id="l-ratio" class="form-control numeric ratio " onkeyup="obtTotalPiezasPPK(' + cont + ')"  value="' + cajaTemp + '"  readonly/></td>';
                        html += '<td width="20%"><input type="text" name="l-piezas" id="l-piezas" class="form-control numeric piezas " value="' + 0 + '"  readonly/></td>';
                        html += '<td width="20%"><input type="text" name="l-tpiezas" id="l-tpiezas" class="form-control numeric tpiezas " value="' + piezasTemp + '"  readonly/></td>';
                        html += '<td width="20%"><input type="text" name="l-cantTP" id="l-cantTP" class="form-control numeric cantTP " value="' + 0 + '"  readonly/></td>';
                        html += '</tr>';
                    }
                });
            }
            html += '<tr><td width="20%">+/-</td>';
            html += '<td width="20%"></td>';
            html += '<td width="20%"></td>';
            html += '<td width="20%"></td>';
            html += '<td width="20%"><input type="text" name="l-cantTU" id="l-cantTU" class="form-control numeric cantTU " value="' + totalUnidades + '"  readonly/></td>';
            html += '</tr>';          

            html += '</tbody> </table>';
            html += '<button type="button" id="guardarBulkHT" class="btn btn-success btn-md pull-right btn-sm"><span class="glyphicon glyphicon-floppy-disk" aria-hidden="true"></span> Pallet</button>';

            if (isNaN(totalUnidades)) {
                $("#guardarBulkHT").prop("disabled", true);
            } else {
                $("#guardarBulkHT").prop("disabled", false);
            }
            $('#listaTallaPHT').html(html);
            $("#numTotalUnit").val(tUnidades);
           
           
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {
    });
}

function ConfirmEmpaqueBulk(a) {
    var confirm = alertify.confirm('Confirmation', 'Do you want to register a new type of packaging Bulk ?', null, null).set('labels', { ok: 'Accept', cancel: 'Cancel' });
    confirm.set('closable', false); 
    confirm.set('onok', function () {
        //crearTallasBulk();
        limpiarFormBulk();
        $("#btnNuevo").prop("disabled", true);
        $("#btnNext").prop("disabled", true);
        $("#btnDone").prop("disabled", true);
        $("#nuevoEmpaqueBulkHT").prop("disabled", false);
       // alertify.success('The packaging was registered correctly.');
    });
    confirm.set('oncancel', function () { 

    });
}

function ConfirmEmpaquePPK(a) {
    var confirm = alertify.confirm('Confirmation', 'Do you want to register a new type of packaging PPK ?', null, null).set('labels', { ok: 'Accept', cancel: 'Cancel' });
    confirm.set('closable', false);
    confirm.set('onok', function () {
        //crearTallasBulk();
        limpiarFormPPK();
        $("#btnNuevoPPK").prop("disabled", true);
        $("#btnNext").prop("disabled", true);
        $("#btnDone").prop("disabled", true);
        $("#nuevoEmpaquePPKHT").prop("disabled", false);
        // alertify.success('The packaging was registered correctly.');
    });
    confirm.set('oncancel', function () {

    });
}

function limpiarFormBulk() {
    $('#tablaTallasBulkHT tbody>tr').each(function () {      
        $(this).find("input.qty").each(function () {
            $(this).closest('td').find("input.qty").each(function () {
              var valor= $(this).val(0);
            });        
        }); 
    });
    $('#tablaTallasBulkHT tbody>tr').each(function () {
        $(this).find("input.cart").each(function () {
            $(this).closest('td').find("input.cart").each(function () {
                var valor = $(this).val(0);
            });
        });
    });
    $('#tablaTallasBulkHT tbody>tr').each(function () {
        $(this).find("input.part").each(function () {
            $(this).closest('td').find("input.part").each(function () {
                var valor = $(this).val(0);
            });
        });
    });
    $('#tablaTallasBulkHT tbody>tr').each(function () {
        $(this).find("input.tcart").each(function () {
            $(this).closest('td').find("input.tcart").each(function () {
                var valor = $(this).val(0);
            });
        });
    });
    
    $('#Packing_PackingTypeSize_FormaEmpaque').val(0);
    $('#Packing_PackingTypeSize_NumberPO').val('');
}

function limpiarFormPPK() {
    $('#tablaTallasPPKHT tbody>tr').each(function () {
        $(this).find("input.qty").each(function () {
            $(this).closest('td').find("input.qty").each(function () {
                var valor = $(this).val(0);
            });
        });
    });
    //$('#Packing_PackingTypeSize_FormaEmpaque').val(0)
    $('#Packing_PackingTypeSize_NumberPO').val('');
    $("#Packing_PackingTypeSize_TotalUnits").val(0);
}
//Registrar tallas Bulk HT
//function crearTallasBulk() {
    $(document).on("click", "#nuevoEmpaqueBulkHT", function () {
        var r = 0; var c = 0; var i = 0; var cadena = new Array(5);
        cadena[0] = ''; cadena[1] = ''; cadena[2] = ''; cadena[3] = ''; cadena[4] = '';
        var nFilas = $("#tablaTallasBulkHT tbody>tr").length;
        var nColumnas = $("#tablaTallasBulkHT tr:last td").length;
        $('#tablaTallasBulkHT tbody>tr').each(function () {
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
        $('#tablaTallasBulkHT').find('td').each(function (i, el) {
            var valor = $(el).children().val();
            if ($(el).children().val() === '') {
                error++;
                $(el).children().css('border', '2px solid #e03f3f');

            } else {
                $(el).children().css('border', '1px solid #cccccc');
            }
        });
        var formaEmpaque = $("#Packing_PackingTypeSize_FormaEmpaque option:selected").val();
        if (formaEmpaque === "0") {
            error++;
            $('#Packing_PackingTypeSize_FormaEmpaque').css('border', '2px solid #e03f3f');
        }
        else {
            $('#Packing_PackingTypeSize_FormaEmpaque').css('border', '');
        }

        var numberPO = $("#Packing_PackingTypeSize_NumberPO").val();
        if (numberPO === "" ) {
            error++;
            $('#Packing_PackingTypeSize_NumberPO').css('border', '2px solid #e03f3f');
        }
        else {
            $('#Packing_PackingTypeSize_NumberPO').css('border', '');
        }

        enviarListaTallaBulkHT(cadena, error);
    });
//}


function enviarListaTallaBulkHT(cadena, error) {
    var idFormaP = $("#Packing_PackingTypeSize_FormaEmpaque option:selected").val();
    var idNumberPo = $("#Packing_PackingTypeSize_NumberPO").val();
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {
        $.ajax({
            url: "/Packing/Obtener_Lista_Tallas_Packing_Bulk_HT",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena, EstiloID: estiloId, FormaPackID: idFormaP, NumberPOID: idNumberPo }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The type of packaging is registered correctly.', 'success', 5, null);
                $("#btnNuevo").prop("disabled", false);
                $("#btnNext").prop("disabled", false);
                $("#btnDone").prop("disabled", false);
                $("#nuevoEmpaqueBulkHT").prop("disabled", true);
                // obtenerListaTallas(estiloId);
            },
            error: function (xhr, ajaxOptions, thrownError) {
               
            }
        });
    }
}


$(document).on("click", "#btnNext", function () {
    $('#Packing_PackingTypeSize_NumberPO').val('');
    $("#Packing_PackingTypeSize_TotalUnits").val(0);
    TallasEmpaquePPKHT(estiloId);
});

$(document).on("click", "#btnNewP", function () {
    $('#Packing_PackingTypeSize_NumberPO').val('');
    $("#Packing_PackingTypeSize_TotalUnits").val(0);
    TallasEmpaqueBulkHT(estiloId);
});

function TallasEmpaquePPKHT(idEst) {
	$("#tablaTallasBulkHT").hide();
	$("#tablaTallasPalletHT").hide();
	$("#tablaTallasPPKHT").show();
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
                $("#btnNuevoPPK").show();
                $("#btnNuevo").hide();
                $("#btnNuevo").prop("disabled", true);
                $("#btnNext").hide();
                $("#btnDone").prop("disabled", true);
                $("#btnNuevoPPK").prop("disabled", true);
                $('#Packing_PackingTypeSize_FormaEmpaque').hide();
            $('label[for="Packing_PackingTypeSize_FormaEmpaque"]').hide();
            $("#opciones").css("display", "inline");
            $('#listaTallaBatchHT').hide(); 
            $('#listaBatchHT').css("display", "none"); 
            $('#opcionTotal').css("display", "inline");    
            $('label[id="numTotalUnitLabel"]').hide();
            $("#numTotalUnit").hide(); 
                $("#div_titulo_Bulk").html("<h3>REGISTRATION OF TYPE OF PACKAGING - PPK</h3>");
                $("#div_titulo_Bulk").css("display", "inline"); 

            /*    html += '<table class="table" id="tablaTallasPPKHT"><thead>';tablaTallasPPKHT
                html += '<tr><th>Size</th>' +
                    ' <th>Ratio</th>' +
                    '</tr>' +
                    '</thead><tbody class="tbodyHTPack">';*/
                $.each(listaPO, function (key, item) {
                    html += '<tr>';
                    html += '<td width="250"><input type="text" id="f-talla" class="form-control talla" value="' + item.Talla + '" readonly/></td>';
                    html += '<td width="250"><input type="text" name="l-cantidad" id="l-cantidad" class="form-control numeric qty " value="' + 0 + '"  /></td>';
                  //  html += '<td width="250"><button type="button" id="btnDelete" class="deleteTalla btn btn btn-danger btn-xs" value="4">Delete</button></td>';
                    html += '</tr>';
                });
			//html += '</tbody> </table>';
			$('.packPPKReg').html(html);
                htmlB += '<button type="button" id="nuevoEmpaquePPKHT" class="btn btn-success btn-md pull-right btn-sm"><span class="glyphicon glyphicon-floppy-disk" aria-hidden="true"></span>Save</button>';
                $('#listaTallaPHT').html(htmlB);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {
    });
}
//Registrar tallas PPK

$(document).on("click", "#nuevoEmpaquePPKHT", function () {
    var r = 0; var c = 0; var i = 0; var cadena = new Array(2);
    cadena[0] = ''; cadena[1] = '';
    var nFilas = $("#tablaTallasPPKHT tbody>tr").length;
    var nColumnas = $("#tablaTallasPPKHT tr:last td").length;
    $('#tablaTallasPPKHT tbody>tr').each(function () {
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
    $('#tablaTallasPPKHT').find('td').each(function (i, el) {
        var valor = $(el).children().val();
        if ($(el).children().val() === '' ) {
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

    var tUnits = $("#Packing_PackingTypeSize_TotalUnits").val();
    if (tUnits === "") {
        error++;
        $('#Packing_PackingTypeSize_TotalUnits').css('border', '2px solid #e03f3f');
    }
    else {
        $('#Packing_PackingTypeSize_TotalUnits').css('border', '');
    }

    enviarListaTallaPPKHT(cadena, error);
});


function enviarListaTallaPPKHT(cadena, error) {
    var idNumberPo = $("#Packing_PackingTypeSize_NumberPO").val();
    var totalUnits = $("#Packing_PackingTypeSize_TotalUnits").val();
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {
        $.ajax({
            url: "/Packing/Obtener_Lista_Tallas_Packing_PPK_HT",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena, EstiloID: estiloId, NumberPOID: idNumberPo, NumberTotU: totalUnits }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The type of packaging is registered correctly.', 'success', 5, null);
                $("#btnNext").prop("disabled", false);
                $("#btnDone").prop("disabled", false);
                $("#btnNuevoPPK").prop("disabled", false);                
                $("#nuevoEmpaquePPKHT").prop("disabled", true);
                //obtenerListaTallas(estiloId);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                
            }
        });
    }
}

$(function () {
    $("#btnAddP").hide();
    $("#nuevoEmpaque").hide();
    $("#nuevoEmpaquePPK").hide();
    $("#registrarNuevo").hide();
    $('#TipoEmp').change(function () {
        var selectedText = $(this).find("option:selected").text();
        var selectedValue = $(this).val();
        var html = '';
        var npo = $("#numeroPOHT").val();
        var tEmpaque = $("#TipoEmp option:selected").val();
        
        if (selectedValue === "1") {
            $('label[for="Packing_CantBox"]').hide();
            $("#numeroCajas").hide();
            $('label[for="Packing_CantidadPPKS"]').hide();
            $("#Packing_CantidadPPKS").hide();
            $('label[id="numTotalUnitLabel"]').hide();
            $("#numTotalUnit").hide(); 
            if (npo === "") {
                npo = 0;
            }
            RegistrarEmpaqueBulkHT(npo, tEmpaque);
            
            
        } else if (selectedValue === "2") {
            $('label[for="Packing_CantBox"]').show();
            $("#numeroCajas").show();
            $("#numeroCajas").val(0);
            $('label[for="Packing_CantidadPPKS"]').show();
            $("#Packing_CantidadPPKS").show();
            $("#Packing_CantidadPPKS").val(0); 
            $('label[id="numTotalUnitLabel"]').show();
            $("#numTotalUnit").show();
            $("#numTotalUnit").val(0);
            if (npo === "") {
                npo = 0;
            }
            RegistrarEmpaquePPKHT(npo, tEmpaque);
            
        }
    });
});

$(document).on('change', '#numeroPOHT', function () {
    var npo = $("#numeroPOHT").val();
    if (npo === "") {
        npo = 0;
	}

    var tEmpaque = $("#TipoEmp option:selected").val();
	if (tEmpaque === "1") {		
        RegistrarEmpaqueBulkHT(npo, tEmpaque);
    } else if (tEmpaque === "2") {         
        RegistrarEmpaquePPKHT(npo, tEmpaque);
    }
    
});

$(document).on("click", "#guardarBulkHT", function () {
    var tEmpaque = $("#TipoEmp option:selected").val();
    if (tEmpaque === "1") {
        obtenerPalletBulkHT();
    } else if (tEmpaque === "2") {
        obtenerPalletBulkHT();
    }
       
});
function limpiarTableBulkHT() {
    $('#tablaTallasPalletHT tbody>tr').each(function () {
        $(this).find("input.qty").each(function () {
            $(this).closest('td').find("input.qty").each(function () {
                var valor = $(this).val(0);
            });
        });
    });
    $('#TipoEmp').val(0);
    $('#Packing_Turnos').val(0);    
    $('#numeroPOHT').val('');
    $("#tablaTallasPalletHT").hide();
    $("#guardarBulkHT").hide();
    
}
function obtenerPalletBulkHT() {
    cadenaCantidad;
    var cantidades = cadenaCantidad.split("*");
    var r = 0; var c = 0; var i = 0; var x = 0; var d = 1; var cadena = new Array(5);
    cadena[0] = ''; cadena[1] = ''; cadena[2] = ''; cadena[3] = ''; cadena[4] = '';
    var nFilas = $("#tablaTallasPalletHT tbody>tr").length;
    var nColumnas = $("#tablaTallasPalletHT tr:last td").length;
    $('#tablaTallasPalletHT tbody>tr').each(function () {
        r = 0;
        c = 0;
        $(this).find("input").each(function () {
            $(this).closest('td').find("input").each(function () {

                var idNomCant = "l-cantidadPcs"+x;
                var inputCant = "#l-cantidadPcs"+x;
                var idNomBox = "l-cajas"+x;
                var inputBox = "#l-cajas"+x;
                var nombre = this.id;                
                var dato;              
                if (nombre === idNomCant) {
                    dato = $(inputCant).is('[readonly]');
                } else if (nombre === idNomBox) {
                    dato = $(inputBox).is('[readonly]');
                }else {
                    dato = false;
                }          
  
        
                if (c === 1) {
                    if (cantidades[x] === "0" || cantidades[x] < numTotalCartones) {                     
                       
                        if (dato === true) {
                            cadena[c] += 0 + "*";
                        } else {
                            cadena[c] += this.value + "*";
                        }                              
                    } else {
                        cadena[c] += 0 + "*";
                    }
                    x++;
                
                } else {
                    if (dato === true) {
                        cadena[c] += 0 + "*";
                    } else {
                        cadena[c] += this.value + "*";
                    }
                   
                }                  
                c++;
               
            });
            r++;
            d++; 
        });
    });
    error = 0;
    $('#tablaTallasPalletHT').find('td.cBox').each(function (i, el) {
        var valor = $(el).children().val();
        if ($(el).children().val() === '' || $(el).children().val() === '0') {
            error++;
            $(el).children().css('border', '2px solid #e03f3f');

        } else {
            $(el).children().css('border', '1px solid #cccccc');
        }
    }); 
    $('#tablaTallasPalletHT').find('td.bCajas').each(function (i, el) {
        var valor = $(el).children().val();
        if ($(el).children().val() === '') {
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

    var tEmpaque = $("#TipoEmp option:selected").val();
    if (tEmpaque === "0") {
        error++;
        $('#TipoEmp').css('border', '2px solid #e03f3f');
    }
    else {
        $('#TipoEmp').css('border', '');
    }

    var npo = $("#numeroPOHT").val();
    if (npo === "") {
        error++;
        $('#numeroPOHT').css('border', '2px solid #e03f3f');
    }
    else {
        $('#numeroPOHT').css('border', '');
    }    

    enviarListaTallaPalletHT(cadena, error);
}

function enviarListaTallaPalletHT(cadena, error) {
    var idTipoTurno = $("#Packing_Turnos option:selected").val();
    var numPO = $("#numeroPOHT").val();
    var tipoEmpaque = $('#TipoEmp option:selected').text();
    var numPPK;
    var numCaja;
    if (tipoEmpaque === "PPK") {
        numPPK = $("#Packing_CantidadPPKS").val();
        numCaja = $("#numeroCajas").val();
    } else if (tipoEmpaque === "BULK") {
        numPPK = 0;
        numCaja = 0;
    }
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
	} else {
		$("#guardarBulkHT").prop("disabled", true);
        $.ajax({
            url: "/Packing/Obtener_Lista_Tallas_Packing_HT_Pallet",
            datatType: 'json',
            data: JSON.stringify({
                ListTalla: cadena, EstiloID: estiloId, TipoTurnoID: idTipoTurno, NumeroPO: numPO, TipoEmpaque: tipoEmpaque,
                NumeroCaja: numCaja, NumeroPPK: numPPK
            }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The packaging was registered correctly.', 'success', 5, null);
                obtenerListaTallas(estiloId);
                limpiarTableBulkHT();
                

            },
            error: function (xhr, ajaxOptions, thrownError) {
                /*showError(xhr.status, xhr.responseText);
                if (data.error === 1) {
                    alertify.notify('Check.', 'error', 5, null);
                }*/
            }
        });
    }
}

function obtener_bacth_estilo(IdEstilo) {
    var tempScrollTop = $(window).scrollTop();
    //  $("#loading").css('display', 'inline');
    $.ajax({
        url: "/Packing/Lista_Batch_HT_Estilo/",
        type: "POST",
        data: JSON.stringify({ id: IdEstilo }),
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {

            var lista_batch = jsonData.Data.listaPO;
            var cargoUser = jsonData.Data.cargoUser;
            var numBatch = 0;
            var totalPiezasBatchBox = 0;
            var totalPiezasBatchPcs = 0;
            var totalGeneralBatch = 0;
            $.each(lista_batch, function (key, item) {
                numBatch++;
            });
            if (numBatch === 0) {
                // $("#div_tabla_talla").hide();
				

			} else {
                $("#panelNoEstilosHT").css('display', 'none');
                $("#paneltablasHT").css('display', 'inline');
                
                var html = '';
                var html2 = '';
                var estilos = jsonData.Data.estilos;
                if (estilos !== '') {
                    $("#div_titulo_Bulk").html("<h3>BATCH REVIEW </h3>");
                    $("#div_titulo_Bulk").css("display", "inline"); 
                } else {
                    //$("#div_estilo_batch").hide();
                }
                html += '<table class="table table-sm table-striped table-hover" id="tablaTallasBulk"><thead>';
                html += '<tr> <th>   </th>';
                var tipoEmpaque;
                $.each(lista_batch, function (key, item) {
                    size = item.Batch;
                    tipoEmpaque = item.TipoEmpaque;
                });
                if (numBatch === 0) {
                    $.each(size, function (key, item) {
                        // html += '<th>' + item.Talla + '</th>';
                    });
                } else {
                    $.each(size, function (key, item) {
                        html += '<th>' + item.Talla + '</th>';
                    });
                }

                // 
                //html += '<th> Total </th>';
       
                html += '<th> Box# </th>';               
                html += '<th> Type Packing </th>';
                html += '<th> PO# </th>';
                html += '<th> Date </th>';
                html += '<th> User </th>';
                html += '<th> Turn </th>';
                html += '<th> User Modif </th>';
                html += '<th> Actions </th>';
                html += '</tr>';

                html += '</thead><tbody>';
                $.each(lista_batch, function (key, item) {
                    html += '<tr><td>Pallet-' + item.IdBatch + '</td>';

                    var cantidad = 0;
                    if (item.TipoEmpaque === 1) {
                        $.each(item.Batch, function (key, i) {
                            html += '<td class="total" >' + i.CantBox + " BOX" + '</td>';  
                            totalPiezasBatchBox += i.CantBox;
                        });
                    } else {
                        $.each(item.Batch, function (key, i) {
                            html += '<td class="total" >' + i.TotalPiezas + " PCS" +'</td>';
                            //html += '<td class="total" >' + i.TotalPiezas + '</td>'; 
                            totalPiezasBatchPcs += i.TotalPiezas;
                        });
                    }
                    if (item.TipoEmpaque === 2) {
                        $.each(item.Batch, function (key, i) {
                            if (key === 1) {
                                html += '<td>' + i.CantBox + '</td>';
                                totalPiezasBatchBox += i.CantBox;
                            }

                        });
                    } else {
                        $.each(item.Batch, function (key, i) {
							if (key === 1) {
								html += '<td>-</td>';
							}					

                        });
                    }

                    if (item.TipoEmpaque === 1) {
                        html += '<td>BULK</td>';
                    } else if (item.TipoEmpaque === 2) {
                        html += '<td>PPK</td>';
                    } else {
                        html += '<td>ASSORTMENT</td>';
                    }
                    html += '<td>' + item.NumberPO + '</td>';
                    if (item.FechaPacking !== "-") {
                        html += '<td>' + item.FechaPacking + '</td>';
                    }
                    else {
                        html += '<td>' + item.FechaPacking + '</td>';
                    }
                    html += '<td>' + item.NombreUsr + '</td>';
                    if (item.TipoTurno === 1) {
                        html += '<td>1rst Turn</td>';
                    } else {
                        html += '<td>2nd Turn</td>';
                    }
                    html += '<td>' + item.NombreUsrModif + '</td>';

                   // html += '<td><a href="#" onclick="obtenerTallas_Batch(' + item.IdBatch + ',' + item.TipoTurno + ',' + item.IdPacking + ',' + item.TipoEmpaque /*+ ',\'' + item.Status + '\'*/ + ');" class = "btn btn-default glyphicon glyphicon-search l1s" style = "color:black; padding:0px 5px 0px 5px;" Title = "Details Bacth"></a></td>';
                    if (cargoUser === 9 || cargoUser === 1) {
                        html += '<td><a href="#" onclick="event.preventDefault();ConfirmDeleteBatchHT(' + item.IdBatch + ',' + IdEstilo + ')" class = "btn btn-default glyphicon glyphicon-trash l1s" style = "color:black; padding:0px 5px 0px 5px;" Title = "Delete Bacth"></a></td>';
                    } else {
                   //     html += '<td></td>';
                        $('#tablaTallasBulkHT').hide();
                        $('#tablaTallasPPKHT').hide();
                        $('#tablaTallasPalletHT').hide();
                       
                   }
                        html += '</tr>';

                });
                if (cargoUser !== 9 ) {
                    totalGeneralBatch = parseInt(totalPiezasBatchBox) + parseInt(totalPiezasBatchPcs);
                    
                    html2 += '<h3>CAPTURED TOTAL PCS:' + totalGeneralBatch+' </h3>';
                    $('#TotalBatchHT').html(html2);
                    $('#TotalBatchHT').show(); 
                }

                if (Object.keys(lista_batch).length === 0) {
                    html += '<tr class="odd"><td valign="middle" align="center" colspan="10" class="dataTables_empty">No batches were found for the style.</td></tr>';

                }
                html += '</tbody> </table>';
                $('#listaTallaBatchHT').html(html);
                $('#listaTallaBatchHT').show();        
                // $("#loading").css('display', 'none');
				$(window).scrollTop(tempScrollTop);
				var IdEstiloInf = $("#InfoSummary_IdItems").val();
				obtenerListaPackingHT(IdEstiloInf);
            }


        },
        error: function (errormessage) { alert(errormessage.responseText); }
    });
    //calcular_Restantes();
}

function ConfirmDeleteBatchHT(idBatch, idSummary) {
    alertify.confirm("Are you sure you want to delete pallet ?", function (result) {
        $.ajax({
            url: '/Packing/EliminarBatchHT/',
            data: "{'idBatch':'" + idBatch + "','idSummary':'" + idSummary + "'}",
            dataType: 'json',
            contentType: 'application/json',
            type: 'post',
            success: function () {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The Pallet was delete correctly.', 'success', 5, null);
                obtenerListaTallas(idSummary);
            }
        });
    });
}

