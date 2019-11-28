
$(document).ready(function () {
    var ID = $("#IdPedido").val();
    buscar_estilos(ID);
    var StatusPack = $("#EstatusPackAssort").val();
    if (StatusPack === "X") {
        $("#btnAssort").removeClass("btn btn-primary");
        $("#btnAssort").addClass("btn btn-success");
    } else {
        $("#btnAssort").removeClass("btn btn-success");
        $("#btnAssort").addClass("btn btn-primary");
    }
    $("#div_tabla_packing").css("visibility", "hidden");
  
});

function select() {
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
    $("#packBPPK").show();   
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
                '<th>FORM PACK</th>'+
                '<th>INSTR.</th>'+
                '<th>QTY</th>' +
                '<th>PRICE</th>' +
                '<th>TOTAL</th>' +
                '</tr>';
            $(".encabezadoPack").html(html2);
            $.each(lista_estilo, function (key, item) {
                html += '<tr  onclick="select();">';
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
              //  html += '<td><a href="#" onclick="obtenerListaTallas(' + item.IdItems + ');" class = "btn btn-default glyphicon glyphicon-search l1s" style = "color:black; padding:0px 5px 0px 5px;" Title = "Packing"></a></td>';
                html += '</tr>';
            });
            if (Object.keys(lista_estilo).length === 0) {
                html += '<tr class="odd"><td valign="middle" align="center" colspan="10" class="dataTables_empty">No styles were found for the PO.</td></tr>';

            }
			$('.tbody').html(html);
            $("#div_estilos_orden").css("visibility", "visible");
            $(window).scrollTop(tempScrollTop);
        },
        error: function (errormessage) { alert(errormessage.responseText); }
    });
}
var estiloId;
var tipoEmp = "";
var numTipoPack;
var listaPsc;
var listCantTalla;
var numBoxPPK = 0;
var numBoxPPKS = 0;
function obtenerListaTallas(EstiloId) {
   $("#loading").css('display', 'inline');   
    $("#panelPacking").css('display', 'inline');
	$("#consultaTalla").css('width', '100%');
    $("#InfoSummary_IdItems").val(EstiloId);
    $("#IdSummaryOrden").val(EstiloId);	
	$("#tablaTallasBulkPcs").hide();
	$("#tablaTallasPPKRatio").hide();
    estiloId = EstiloId;
        $.ajax({
            url: "/Packing/Lista_Tallas_Por_Estilo_Packing/" + EstiloId,
        method: 'POST',
        dataType: "json",
            success: function (jsonData) {  
               
                var listaT = jsonData.Data.lista;
                var listaPacking = jsonData.Data.listaPackingS;
                var listaEmpaque = jsonData.Data.listaEmpaqueTallas;
                var listaTCajas = jsonData.Data.listaTotalCajasPack;
                var cargo = jsonData.Data.cargoUser;
                var tPiezasEstilos = jsonData.Data.numTPSyle;
                var tPiezasPack = jsonData.Data.numTPack;
				var listTotalPiezas = jsonData.Data.listaTotalPiezas;
				var listadoPack = jsonData.Data.listaPack;
                var listaEPPK = jsonData.Data.listaEmpPPKS;
                var listaEBulk = jsonData.Data.listaEmpBULKS;
                listaPsc = jsonData.Data.listaTotalPiezas;
                listCantTalla = jsonData.Data.listCantTalla;
                //var listaTCajas = jsonData.Data.listaCajasT;
                var html = '';
                var estilos = jsonData.Data.estilos;
               // if (tPiezasPack <= tPiezasEstilos) { 
                if (listaPacking.length === 0) {
					if (cargo === 1 || cargo === 10) {
						$("#tableQtySize").show();
						$("#panelNoEstilosBPPK").css('display', 'none');
                        $("#btnAdd").show();
                        $("#nuevaTalla").show();
                        $("#nuevoPallet").hide();
                        $("#nuevoPalletBulks").hide();	
                        $("#modificarBatch").hide();
                        $("#registrarNuevo").hide();
						$("#tableTallasBulk").hide();				
                        $("#titulo_Tipo_Empaque").css('display', 'none');
                        $("#opcSelectPackBULKS").css('display', 'none');
                        var lista_estilo_Desc = jsonData.Data.lista;
                        $.each(lista_estilo_Desc, function (key, item) {

                            EstiloDescription = item.DescripcionEstilo;

                        });
                        $("#div_Desc_Estilo").html("<h2>Item: " + estilos + "-" + $.trim(EstiloDescription) + "</h2>");                                           
                        $("#div_estilo").html("<h3>REGISTER 1rst QUALITY OF SIZES</h3>");
						$("#modificarPack").hide();
						$("#editarPack").hide();
						//listaPsc
						$.each(listadoPack, function (key, item) {
							var cont = 0;
							if (listaPsc.length !== 0) {
								$.each(listaPsc, function (key, itemT) {
									
									if (item.IdTalla === itemT.IdTalla) {
										var resultado = item.Cantidad - itemT.SumaTotal;
										html += '<tr id="pallet' + cont + '" class="pallet">';
										html += '<td width="250"><input type="text" id="f-talla" class="form-control talla" value="' + item.Talla + '"/></td>';
										html += '<td width="250"><input type="text" name="l-cantidad" id="l-cantidad' + cont + '" class="form-control numeric qualityT" value="' + parseInt(resultado) + '" /></td>';
										html += '<td width="250"><button type="button" id="btnDelete" class="deleteTalla btn btn btn-danger btn-xs" value="4">Delete</button></td>';
										html += '</tr>';
										cont = cont + 1;
									}									
								});
							} else {
								html += '<tr id="pallet' + cont + '" class="pallet">';
								html += '<td width="250"><input type="text" id="f-talla" class="form-control talla" value="' + item.Talla + '"/></td>';
								html += '<td width="250"><input type="text" name="l-cantidad" id="l-cantidad' + cont + '" class="form-control numeric qualityT" value="' + item.Cantidad + '" /></td>';
								html += '<td width="250"><button type="button" id="btnDelete" class="deleteTalla btn btn btn-danger btn-xs" value="4">Delete</button></td>';
								html += '</tr>';
								cont = cont + 1;
							}							
						});
                       // html += '</tbody > ';
                        ocultarOpciones();
						$('.tbodyQtyTall').html(html);					
                    } else {
                        $("#btnAdd").hide();
                        $("#nuevaTalla").hide();
                        $("#nuevoPallet").hide();
                        $("#nuevoPalletBulks").hide();	
						$("#tableQtySize").hide();
                        $("#modificarBatch").hide();					
						$("#modificarPack").hide();
						$("#imgPanelBPPK").css('cursor', 'none');
						$("#listaTallaPacking").hide();
						$("#opcionesPack").hide();	
						$("#div_estilo_pack").hide();						
                        $("#div_estilo").hide();
                        $("#div_titulo").hide();
                        $("#tablePacking").hide();
						$("#listaTallaBatch").hide();
						$("#div_Desc_Estilo").hide();
						$("#containerPie").css('display', 'none');
						$("#titulo_Tipo_Empaque").hide();
						$("#consultaTalla").css('height', '700px');
						$("#panelNoEstilosBPPK").css('display', 'inline');
                        $("#opcionesPackVArios").css('display', 'none');
                        $("#opcSelectPackBULKS").css('display', 'none');
						$("#tablaPackingcont").hide();
						$("#nuevoPalletPPK").hide();
						
						
                    }
                } else {
                    if (cargo !== 1 || cargo !== 9) {
                        $("#div_estilo").show();
                        $("#panelNoEstilosBPPK").css('display', 'none');
                        $("#consultaTalla").css('height', '1500px');

					} else {						
						$("#consultaTalla").css('height', '1600px');
						$("#panelNoEstilosBPPK").css('display', 'inline');												
						$("#imgPanelBPPK").css('cursor', 'none');
					}
					if (cargo === 15) {
						$("#modificarPack").hide();
						$("#panelNoEstilosBPPK").css('display', 'inline');
						$("#imgPanelBPPK").css('cursor', 'none');
					}
                    $("#btnAdd").hide();
                    $("#nuevaTalla").hide();
                    $("#nuevoPallet").hide();
                    $("#nuevoPalletPPK").hide();
                    $("#nuevoPalletBulks").hide();
					$("#tablaTallas").hide();
					$("#tableQtySize").hide();
                    $("#tablePacking").show();
					$("#modificarBatch").hide();  
					$("#tablaPackingcont").show();
                    $.each(listaEmpaque, function (key, item) {
                        tipoEmp = item.NombreTipoPak;
                    });                    
                    var lista_estilo_Descrip = jsonData.Data.lista;
                    var EstiloDescrip;
                    $.each(lista_estilo_Descrip, function (key, item) {

                        EstiloDescrip = item.DescripcionEstilo;

					});
					$("#div_Desc_Estilo").show();
                    $("#div_Desc_Estilo").html("<h2>Item: " + estilos + "-" + $.trim(EstiloDescrip) + "</h2>"); 
                    $("#titulo_Tipo_Empaque").css('display', 'inline');                      
                    $("#titulo_Tipo_Empaque").html("<h1> Packing- " + tipoEmp +"</h1>");
                    $("#div_estilo").html("<h3>QUALITY OF SIZES</h3>");
                    html += '<tr> <th width="30%"> Size </th>';
                    var numTallas = 0;
                    $.each(listaPacking, function (key, item) {
                        numTallas++;
                    });
                    $.each(listaPacking, function (key, item) {
                        html += '<th>' + item.Talla + '</th>';
                    });
                    html += '<th width="30%"> Total </th>';                    
					html += '</tr><tr><td width="30%">Total Orden</td>';
					var cantidadesTotal = 0;
					var cadena_cantidadesTotal = "";
					$.each(listaT, function (key, item) {
						html += '<td class="">' + item.Cantidad + '</td>';
						cantidadesTotal += item.Cantidad;
						cadena_cantidadesTotal += "*" + item.Cantidad;
					});
					var cantidades_arrayTotal = cadena_cantidadesTotal.split('*');
					html += '<td>' + cantidadesTotal + '</td>';
					html += '</tr><tr><td width="30%">1rst Quantity</td>';
					var cantidades = 0;
					var cadena_cantidades = "";
					$.each(listaPacking, function (key, item) {
						html += '<td class="calidad">' + item.Calidad + '</td>';
						cantidades += item.Calidad;
						cadena_cantidades += "*" + item.Calidad;
					});
					var cantidades_array = cadena_cantidades.split('*');
					html += '<td>' + cantidades + '</td>';
                    var cantidadesEmp = 0;
                    if (listaEmpaque.length === 0) {
                        listaEmpaque;
                        html += '</tr><tr><td width="30%">Type Packing</td>';

                        var total = 0;
                        for (var v = 0; v < numTallas; v++) {

                            html += '<td>' + total + '</td>';
                            cantidadesEmp += total;
                        }
                        html += '<td>' + cantidadesEmp + '</td>';

                    } else {
                        $.each(listaEmpaque, function (key, item) {
                            tipoEmp = item.NombreTipoPak;
                        });
                        if (tipoEmp === "BULK") {
                            numTipoPack = 1;
                            html += '</tr><tr><td width="30%">' + tipoEmp + '- #Pieces' + '</td>';

                            $.each(listaEmpaque, function (key, item) {
                                html += '<td>' + item.Pieces + '</td>';
                                cantidadesEmp += item.Pieces;

							});
							html += '<td>' + cantidadesEmp + '</td>';
                        } else if (tipoEmp === "PPK") {
                            numTipoPack = 2;
                            html += '</tr><tr><td width="30%">' + tipoEmp + '- #Ratio' + '</td>';
                            $.each(listaEmpaque, function (key, item) {
                                html += '<td class="numRatio">' + item.Ratio + '</td>';
                                cantidadesEmp += item.Ratio;

							});
							html += '<td>' + cantidadesEmp + '</td>';
							html += '</tr>';
						} else if (tipoEmp === "PPKS") {
							ActualizarSelectPackingNameVariosPPK2(EstiloId);
							numTipoPack = 4;
							var contadorP = 0;
							var valor = 0;
							$.each(listaEPPK, function (key, item) {
								contadorP = contadorP + 1;
								html += '</tr><tr><td width="30%">PPK - #Ratio-' + item.NombrePacking + '</td>';

								$.each(item.ListaEmpaque, function (key, i) {
									html += '<td class="ratioPPKS' + item.NombrePacking +'">' + i.Ratio + '</td>';
									cantidadesEmp += i.Ratio;
								});
								//html += '<td>' + cantidadesEmpPPK + '</td>';
								html += '</tr><tr id="empaque" class="empaque"><td width="30%">QTY</td>';
								$.each(item.ListaEmpaque, function (key, i) {

									html += '<td class="qtyPPKs' + item.NombrePacking +'">' + i.Cantidad + '</td>';

								});
								html += '</tr><tr id="empaque" class="empaque"><td width="30%">Packed</td>';
								$.each(item.ListaEmpaque, function (key, i) {

									html += '<td class="qtyPPK">' + i.TotalRatio + '</td>';

								});
								html += '</tr><tr id="empaque" class="empaque">';
								var listaEmpBatch = 0;
								$.each(item.ListaEmpaque, function (key, item) {
									listaEmpBatch++;
								});
								
								$.each(item.ListaEmpaque, function (key, i) {
								
									if (key === 1) {
										if (listaEmpBatch === 0) {
											i = 0;
											html += '<td class="box' + i.NombrePacking+'">' + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Total Box # " + i.TotalCajas + '</td>';
											numBoxPPKS = i;
											valor = valor + 1;
										} else {
											html += '<td class="box' + i.NombrePacking +'">' + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Total Box # " + i.TotalCajas + '</td>';
											numBoxPPKS = i;
											valor = valor + 1;
										}
									} else {
										if (listaEmpBatch === 0) {
											i = 0;
											numBoxPPK = i;
										} else {
											numBoxPPK = i;
										}
									}


								});
								
								/*html += '</tr><tr id="empaque" class="empaque">';
								var lista_Batch_Box = jsonData.Data.listaCajasT;
								var listaPBatch = 0;
								$.each(lista_Batch_Box, function (key, item) {
									listaPBatch++;
								});
								if (listaPBatch === 0) {
									lista_Batch_Box = listaPacking;
								} else {
									lista_Batch_Box;
								}
								$.each(lista_Batch_Box, function (key, item) {

									if (tipoEmp === "PPKS") {
										if (key === 1) {
											if (listaPBatch === 0) {
												item = 0;
												html += '<td>' + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Total Box # " + item + '</td>';
												numBoxPPK = item;
											} else {
												html += '<td>' + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Total Box # " + item + '</td>';
												numBoxPPK = item;
											}
										} else {
											if (listaPBatch === 0) {
												item = 0;
												numBoxPPK = item;
											} else {
												numBoxPPK = item;
											}
										}
									} else {
										if (listaPBatch === 0) {
											item = 0;
											html += '<td>' + item + '</td>';
										} else {
											html += '<td>' + item + '</td>';
										}
									}
									// cantidadesPrinted += item;
								});*/
								//html += '</tr>';
							});
							
                        } else if (tipoEmp === "BULKS") {
                            $("#opcSelectPackBULKS").css("display", "inline");
                            ActualizarSelectPackingNameVariosBulk(EstiloId);
                            numTipoPack = 5;
                            var contadorBulk = 0;
                            $.each(listaEBulk, function (key, item) {
                                contadorBulk = contadorBulk + 1;
                                html += '</tr><tr><td width="30%">BULK-' + item.PackingNameBulk + ' Pieces</td>';

                                $.each(item.ListaEmpaque, function (key, i) {

                                    html += '<td class="qtyBulkPcs' + item.PackingNameBulk +'">' + i.Pieces + '</td>';

                                });
                                html += '</tr><tr id="empaque" class="empaque"><td width="30%">Packed</td>';
                                $.each(item.ListaEmpaque, function (key, i) {

                                    html += '<td class="qtyBulkTPcs' + item.PackingNameBulk +'">' + i.TotalBulk + '</td>';

                                });
                                //html += '</tr>';
                            });
                        }
                       
                    }
					$.each(listaEmpaque, function (key, item) {
						tipoEmp = item.NombreTipoPak;
					});
					if (tipoEmp === "BULK" || tipoEmp === "PPK") {
						html += '</tr><tr><td width="30%">Packed</td>';
						/*  $.each(listaTCajas, function (key, item) {
							  html += '<td>' + item.TotalPiezas + '</td>';
						  });*/
						var cantidadesTBox = 0;
						var lista_Batch = jsonData.Data.listaTallasTotalBatch;
						var listaTBatch = 0;
						$.each(listaTCajas, function (key, item) {
							listaTBatch++;
						});
						if (listaTBatch === 0) {
							listaTCajas = listaPacking;
						} else {
							listaTCajas;
						}
						$.each(listaTCajas, function (key, item) {
							if (listaTBatch === 0) {
								item = 0;
								html += '<td class="cantPack">' + item + '</td>';
							} else {
								html += '<td class="cantPack">' + item + '</td>';
							}

							cantidadesTBox += item;
						});
						html += '<td>' + cantidadesTBox + '</td>';
						html += '</tr><tr>';
						if (tipoEmp === "BULK") {
							html += '<td class="cajasQty">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Total Box # </td>';
						}

						var lista_Batch_Box = jsonData.Data.listaCajasT;
						var listaPBatch = 0;
						$.each(lista_Batch_Box, function (key, item) {
							listaPBatch++;
						});
						if (listaPBatch === 0) {
							lista_Batch_Box = listaPacking;
						} else {
							lista_Batch_Box;
                        }
                        var cantidadTotalBoxPacked = 0;
						$.each(lista_Batch_Box, function (key, item) {

							if (tipoEmp === "PPK") {
								if (key === 1) {
									if (listaPBatch === 0) {
										item = 0;
										html += '<td>' + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Total Box # " + item + '</td>';
                                        numBoxPPK = item;                                        
									} else {
										html += '<td>' + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Total Box # " + item + '</td>';
                                        numBoxPPK = item;                                        
									}
								} else {
									if (listaPBatch === 0) {
										item = 0;
                                        numBoxPPK = item;
                                       
									} else {
                                        numBoxPPK = item;
                                        
									}
								}
							} else {
								if (listaPBatch === 0) {
									item = 0;
                                    html += '<td>' + item + '</td>';
                                    cantidadTotalBoxPacked += item;
								} else {
                                    html += '<td>' + item + '</td>';
                                    cantidadTotalBoxPacked += item;
								}
                            }
                            
							// cantidadesPrinted += item;
                        });
                        html += '<td>' + cantidadTotalBoxPacked + '</td>';
						var lista_Partial = jsonData.Data.listaPartial;
                        var listaPartBatch = 0;
                        var cantidadTotalPartialPacked = 0;
						$.each(lista_Partial, function (key, item) {
							listaPartBatch++;
						});
						if (listaPartBatch === 0) {
							lista_Partial = listaPacking;
						} else {
							lista_Partial;
						}
						if (tipoEmp === "BULK") {
							html += '</tr><tr><td width="30%">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Total Partial #</td>';

							$.each(lista_Partial, function (key, item) {

								if (listaPartBatch === 0) {
									item = 0;
                                    html += '<td>' + item + '</td>';
                                    cantidadTotalPartialPacked += item;
								} else {
                                    html += '<td>' + item + '</td>';
                                    cantidadTotalPartialPacked += item;
								}

							});

                        }
                        html += '<td>' + cantidadTotalPartialPacked + '</td>';
						var sumaTotal = 0;
						html += '</tr><tr><td width="30%" >+/-</td>';
						var totales = 0;
						var i = 1;
						$.each(listaTCajas, function (key, item) {
							if (listaTBatch === 0) {
								item = 0;
							}
							var resta = parseInt(item) - parseInt(cantidades_array[i]);

							if (resta === 0) {
								html += '<td class="faltante" style="color:black;">' + resta + '</td>';
							} else if (resta >= 0) {
								html += '<td class="faltante" style="color:blue;">' + resta + '</td>';
							} else {
								html += '<td class="faltante" style="color:red;">' + resta + '</td>';
							}

							$('.faltante').css('color', '2px solid #e03f3f');
							i++;
							sumaTotal += resta;
						});
						html += '<td>' + sumaTotal + '</td>';
						html += '</tr>';
                    } else if (tipoEmp === "PPKS") {
						var sumaTotalPPK = 0;
						html += '</tr><tr><td width="30%" >+/-</td>';
						var totalesPPK = 0;
						var x = 1;
						var listaTBatchPPK = 0;
						$.each(listaTCajas, function (key, item) {
							listaTBatchPPK++;
						});
						if (listaTBatchPPK === 0) {
							listaTCajas = listaPacking;
						} else {
							listaTCajas;
						}
						$.each(listaTCajas, function (key, item) {
							if (listaTBatchPPK === 0) {
								item = 0;
							}
							var resta = parseInt(item) - parseInt(cantidades_array[x]);

							if (resta === 0) {
								html += '<td class="faltante" style="color:black;">' + resta + '</td>';
							} else if (resta >= 0) {
								html += '<td class="faltante" style="color:blue;">' + resta + '</td>';
							} else {
								html += '<td class="faltante" style="color:red;">' + resta + '</td>';
							}

							$('.faltante').css('color', '2px solid #e03f3f');
							x++;
							sumaTotalPPK += resta;
						});
						html += '<td>' + sumaTotalPPK + '</td>';
						html += '</tr>';
                    } else {
                        var sumaTotalBulk = 0;
                        html += '</tr><tr><td width="30%" >+/-</td>';
                        var totalesBulks = 0;
                        var d = 1;
                        var listaTBatchBulks = 0;
                        $.each(listaTCajas, function (key, item) {
                            listaTBatchBulks++;
                        });
                        if (listaTBatchBulks === 0) {
                            listaTCajas = listaPacking;
                        } else {
                            listaTCajas;
                        }
                        $.each(listaTCajas, function (key, item) {
                            if (listaTBatchBulks === 0) {
                                item = 0;
                            }
                            var resta = parseInt(item) - parseInt(cantidades_array[d]);

                            if (resta === 0) {
                                html += '<td class="faltante" style="color:black;">' + resta + '</td>';
                            } else if (resta >= 0) {
                                html += '<td class="faltante" style="color:blue;">' + resta + '</td>';
                            } else {
                                html += '<td class="faltante" style="color:red;">' + resta + '</td>';
                            }

                            $('.faltante').css('color', '2px solid #e03f3f');
                            d++;
                            sumaTotalBulk += resta;
                        });
                        html += '<td>' + sumaTotalBulk + '</td>';
                        html += '</tr>';
                    }

                    $('.tbodyP').html(html);
                    if (cargo === 1 || cargo === 9) {
						$("#tablaTallasPPK").hide();
						$("#tablaTallasBulk").hide();
						$("#nuevoEmpaque").hide();
						$("#nuevoEmpaquePPK").hide();
						if (tipoEmp === "BULK") {
							registrarPallet(EstiloId);
						} else if (tipoEmp === "PPK") {
							registrarPalletPPK(EstiloId);
						} else if (tipoEmp === "PPKS") {
							MostrarOpcionesPalletPPKS();
                        } else if (tipoEmp === "BULKS") {
                            MostrarOpcionesPalletBulks();
                        }
                    }

                } 
            //}
            $("#consultaTalla").css("visibility", "visible"); 
				$("#arte").css("display", "inline-block");
				var datoItem = $("#InfoSummary_IdItems").val();
				obtenerImagenPNL(estilos, datoItem);
           obtenerImagenArte(estilos);
           obtener_bacth_estilo_pack(EstiloId);
            $("#packAssort").hide(); 
            $("#packBPPK").show();
                //$("#loading").css('display', 'none');
				
            setTimeout(function () { $("#loading").css('display', 'none'); }, 3000);


        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {

    });
}



$(document).on('click', '#registrarNuevo', function () {
    if (tipoEmp === "BULK") {
        registrarPallet(estiloId);
    } else if (tipoEmp === "PPK") {
        registrarPalletPPK(estiloId);
    }
});

function ActualizarSelectPackingNameVariosPPK2(idEstilo) {
	$('#selectPackingNameVariosPPKS').find('option:not(:first)').remove();
	$.ajax({
		url: "/Packing/ListadoPackingRegistradosPPK/" + idEstilo,
		method: 'POST',
		dataType: "json",
		success: function (jsonData) {
			var html = '';
			var listaEstilos = jsonData.Data.listEstilo;

			$.each(listaEstilos, function (key, item) {
				html += '<option  value="' + item.PackingRegistradoPPK + '">' + item.PackingRegistradoPPK + '</option>';
			});
			$('#selectPackingNameVariosPPKS').append(html);
			$('#selectPackingNameVariosPPKS').parent().show();
		},
		error: function (errormessage) {
			alert(errormessage.responseText);
		}
	}).done(function (data) {

	});
}

function ActualizarSelectPackingNameVariosBulk(idEstilo) {
    $('#selectPackingNameVariosBULKS').find('option:not(:first)').remove();
    $.ajax({
        url: "/Packing/ListadoPackingRegistradosVariosBULKS/" + idEstilo,
        method: 'POST',
        dataType: "json",
        success: function (jsonData) {
            var html = '';
            var listaEstilos = jsonData.Data.listEstilo;

            $.each(listaEstilos, function (key, item) {
                html += '<option  value="' + item.PackingRegistradoVariosBULKS + '">' + item.PackingRegistradoVariosBULKS + '</option>';
            });
            $('#selectPackingNameVariosBULKS').append(html);
            $('#selectPackingNameVariosBULKS').parent().show();
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {

    });
}

function ocultarOpciones() {
    $("#div_titulo").css("display", "none");
    $("#div_estilo_pack").css("display", "none");
    $("#opciones").css("display", "none");
    $("#opcionesPack").css("display", "none");
    $("#tablePacking").hide();
    $("#listaTallaBatch").hide();    
    $("#tablaTallasBulk").hide();
    $("#tablaTallasPPK").hide();
    $("#nuevoEmpaquePPK").hide();
    $("#nuevoEmpaque").hide();
    $("#btnAddP").hide();
    $("#tablaTallasPallet").hide();
    $("#div_estilo_pack").css("display", "none");
}
 
$(document).on("keyup", "input.cantBox", function () {
    obtTotalPiezas(numBoxPPK);
});

$(document).on("keyup", "input.cantBoxPPKS", function () {
	var namePack = $("#selectPackingNameVariosPPKS option:selected").val();
	var nombreCant = ".box" + namePack;
	var campoCaja = $(nombreCant).parent("tr").find("td").text();
	var numeroBox = getNumbersInString(campoCaja);
	obtTotalPiezasPPKS(numeroBox);
});
var tipoPacking = ""; 
function registrarPallet(EstiloId) {
	$("#listaTallaPacking").show();
    $("#nuevoPalletPPK").hide();
    $("#nuevoPalletBulks").hide();
    $.ajax({
        url: "/Packing/Lista_Tallas_Por_Estilo_BULK/" + EstiloId,
        method: 'POST',
        dataType: "json",
        success: function (jsonData) {
            var listaT = jsonData.Data.listaTalla;
            var listaPacking = jsonData.Data.listaPackingS;
            var listaEmpaque = jsonData.Data.listaEmpaqueTallas;
            var html = '';
            $("#btnAddP").hide();
            $("#modificarBatch").hide();       
            $("#opcionesPack").css("display", "inline");
            $("#div_estilo_pack").html("<h3>REGISTRATION OF PALLET</h3>");
            $("#div_estilo_pack").css("display", "inline");
			$("#tablaTallasBulkPcs").hide(); 
			$("#tablaTallasPPKRatio").hide(); 
            var tipoEmp = "";    
            $.each(listaEmpaque, function (key, item) {
                tipoEmp = item.NombreTipoPak;
            });
           
            $('#Packing_PackingTypeSize_NombreTipoPak').show(); 
            $('#Packing_PackingTypeSize_NombreTipoPak').val(tipoEmp);
                $('label[for="Packing_CantBox"]').hide();
            $("#Packing_CantBox").hide();
            $('label[for="Packing_TotalCartonsPPK"]').hide();
            $("#Packing_TotalCartonsPPK").hide();
            $('label[for="Packing_TotalCartonesFaltPPK"]').hide();
            $("#Packing_TotalCartonesFaltPPK").hide();
            $("#Packing_Turnos").val(0);
            var numClienteFant = 0;
            html += '<table class="table" id="tablaTallasPallet"><thead>';
            $.each(listaEmpaque, function (key, item) {
                var clienteNombreF = $("#CatClienteFinal_NombreCliente").val();
                var cliente2 = $.trim(clienteNombreF);
                if (cliente2 === "FEA TARGET") {
                    numClienteFant = 1;
                } else if (cliente2 === "BRAVADO TARGET") {
                    numClienteFant = 2;
                } else if (cliente2 === "Merch Traffic Target") {
                    numClienteFant = 3;
                }
                
            });  
            if (numClienteFant != 0) {
                html += '<tr><th style="visibility:hidden;"> </th> ' +
                    '<th> Size</th> ' +
                    '<th>Box#</th>' +
                    '<th>CantxBox#</th>' +
                    '<th>PartialBox#</th>' +
                    '<th>TotalPieces#</th>' +
                    '<th>TotalBoxPO#</th>' +
                    '<th>CapturedBox#</th>' +
                    '<th>BoxFalt&Extras#</th>' +
                    '</tr>' +
                    '</thead><tbody>';

            } else {
                html += '<tr><th style="visibility:hidden;"> </th> ' +
                    '<th> Size</th> ' +
                    '<th>Box#</th>' +
                    '<th>CantxBox#</th>' +
                    '<th>PartialBox#</th>' +
                    '<th>TotalPieces#</th>' +
                    '<th>TotalBoxPO#</th>' +
                    '<th>BoxFalt&Extras#</th>' +
                    '</tr>' +
                    '</thead><tbody>';
            }          
               
            var cantidadesEmp = 0;
            var cantidadesT = 0;
            var cantidadPiezas;
            var cantidadRatio;
            var cont = 0;
           
            $.each(listaEmpaque, function (key, item) {              
                cont = cont + 1;
                var numCliente = 0;
                var valorCalidad = $(".calidad").parent("tr").find("td").eq(cont).text();
                var pCalidad = parseInt(valorCalidad);
                var numTotalBox;
                var numTotalC = pCalidad / item.Pieces;
                var totalBox = Math.floor(numTotalC);
                var totalBoxOrden = Math.floor(numTotalC);
                var cajasT;
                var valorCajas = $(".cajasQty").parent("tr").find("td").eq(cont).text();
                var nCajas = parseInt(valorCajas);
                var totalCajas;
                var clienteNombre = $("#CatClienteFinal_NombreCliente").val();
                var cliente = $.trim(clienteNombre);
                var cajas;
             
                if (cliente === "FEA TARGET") {
                    totalCajas = parseInt(totalBox) + 100;
                    numTotalBox = totalCajas;
                    cajasT = totalBox;
                    numCliente = 1;
                } else if (cliente === "BRAVADO TARGET") {
                    totalCajas = parseInt(totalBox) + 100;
                    numTotalBox = totalCajas;
                    cajasT = totalBox;
                    cajas = numTotalBox - cajasT;
                    numCliente = 2;
                } else if (cliente === "Merch Traffic Target") {
                    totalCajas = parseInt(totalBox) + 100;
                    numTotalBox = totalCajas;
                    cajasT = totalBox;

                    numCliente = 3;
                } else {
                    totalCajas = parseInt(totalBox);
                    numTotalBox = pCalidad / item.Pieces;
                    cajasT = totalBox;
                }
                var resta = 0;
                if (numTotalBox <= 1) {
                    totalCajas = 1;
                    resta = parseInt(totalCajas) - nCajas;
                }
                var partial = parseInt(totalCajas);
                if (pCalidad > item.Pieces) {
                    if (numTotalBox <= 1) {
                        cajasT = cajasT;
                    } if (numTotalBox % 1 === 0) {
                        cajasT = cajasT;
                    } else {
                        cajasT = cajasT + 1;
                    }
                    if (numCliente !== 0) {
                        if (parseInt(valorCajas) !== 0) {
                            //resta = parseInt(totalCajas) - nCajas; 
                            //ncajas
                            if (totalCajas > totalBox) {
                                resta = parseInt(numTotalBox) - nCajas;
                            } else {
                                resta = parseInt(totalBox) - nCajas;
                            }

                            if (parseInt(resta) > 0) {
                                resta = parseInt(resta);
                            } else {
                                resta = parseInt(resta) * -1;
                            }
                        } else {
                            resta = parseInt(cajasT) - nCajas;
                        }

                    } else {
                        resta = parseInt(cajasT) - nCajas;
                    }

                }
                /*var numCajas = parseInt(valorCajas);
                if (numCajas === totalBox) {
                    totalBox = 0;
                }*/
                
                html += '<tr id="pallet' + cont + '" class="pallet">';
                html += '<td width="1" style="visibility:hidden;"><input type="text" id="f-id" class="form-control" value="' + item.IdPackingTypeSize + '" /></td>';
                html += '<td width="250"><input type="text" id="f-talla" class="form-control talla" value="' + item.Talla + '" readonly/></td>'; //onChange="calcular_TotalPiezas()"

                if (resta !== 0 /*|| totalBox !== 0*/) {
                    html += '<td width="250" class="cBox"><input type="text" name="l-cantidadBox" id="l-cantidadBox" class="form-control numeric cantCajas"  onkeyup="obtTotalMat(' + cont + ')" value="' + 0 + '"></td>';
                } else {
                    html += '<td width="250" class="cBox"><input type="text" name="l-cantidadBox" id="l-cantidadBox" class="form-control numeric cantCajas"  onkeyup="obtTotalMat(' + cont + ')" value="' + 0 + '" readonly></td>';
                            
                }
              
                html += '<td width="250"><input type="text" name="l-piezas" id="l-piezas" class="form-control numeric cant" value="' + item.Pieces + '"  readonly/></td>';
                cantidadesEmp += item.Pieces;  
                if (resta !== 0 /*|| totalBox !== 0*/) {
                    html += '<td width="250"><input type="text" name="l-totBoxPartial" id="l-totBoxPartial" class="form-control numeric totBoxPartial" onblur="ActualizarPiezasPackingBulk(' + cont + ')" value="' + 0 + '" /></td>';
                } else {
                    html += '<td width="250"><input type="text" name="l-totBoxPartial" id="l-totBoxPartial" class="form-control numeric totBoxPartial" onblur="ActualizarPiezasPackingBulk(' + cont + ')" value="' + 0 + '" readonly/></td>';

                }
                
                html += '<td width="250"><input type="text" name="l-totalPiezas" id="l-totalPiezas" class="form-control numeric totalPiezas" value="' + 0 + '" readonly/></td>';
                if (parseInt(resta) === 0) { // valorCajas
                    html += '<td width="250"><input type="text" name="l-totBox" id="l-totBox" class="form-control numeric totBox" value="' + totalBoxOrden + '" readonly/></td>';
                } else {
                    html += '<td width="250"><input type="text" name="l-totBox" id="l-totBox" class="form-control numeric totBox" value="' + totalBoxOrden + '" readonly/></td>';
                }

                if (numCliente != 0) {
                    html += '<td width="250"><input type="text" name="l-totBoxCap" id="l-totBoxCap" class="form-control numeric totBoxCap" value="' + nCajas + '" readonly/></td>';

                }
                if (parseInt(resta)*-1 === 0) { // valorCajas
                    html += '<td width="250" class="tFalB"><input type="text" name="l-totFaltantes" id="l-totFaltantes" class="form-control numeric totFaltantes" value="' + totalBox + '" readonly/></td>';
                } else {
                    html += '<td width="250" class="tFalB"><input type="text" name="l-totFaltantes" id="l-totFaltantes" class="form-control numeric totFaltantes" value="' + resta + '" readonly/></td>';
                }
               
                html += '</tr>';  
                $(".totBoxPartial").val(partial);
            });
           
            html += '</tbody> </table>';
            $("#div_titulo").html("<h3></h3>");
            $("#div_titulo").css("display", "inline");
            $("#opciones").css("display", "none");
            $("#btnAdd").hide();
            $("#nuevoPallet").show(); 
            $("#modificarBatch").hide();
            $("#registrarNuevo").hide();      
            $('#listaTallaPacking').html(html);    
            
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {

    });
}

function registrarPalletVariosBulks(EstiloId, namePack) {
    $("#modificarPack").hide();
    $("#crearPackPPK").hide();
    $("#crearPackBulks").show();
    $("#changePack").hide();
    $("#editarBPPack").show();
    $("#eliminarPackBP").show();
    var actionData = "{'id':'" + EstiloId + "','packingName':'" + namePack + "'}";
    $.ajax({
        url: "/Packing/Lista_Tallas_Por_Estilo_VariosBulks/",
        method: 'POST',
        dataType: "json",
        data: actionData,
        contentType: "application/json;charset=UTF-8",
        success: function (jsonData) {
            var listaEmpaque = jsonData.Data.listaEmpaqueTallas;
         
            var html = '';
            $("#btnAddP").hide();
            $("#modificarBatch").hide();
            $("#changePack").hide();
            $("#opcionesPack").css("display", "none");
            $("#div_estilo_pack").html("<h3>REGISTRATION OF PALLET</h3>");
            $("#div_estilo_pack").css("display", "inline");
            var tipoEmp = "";
			/*var valorCalidad = $(".calidad").parent("tr").find("td").eq(1).text();
			var valorRatio = $(".numRatio").parent("tr").find("td").eq(1).text();
			var pCalidad = parseInt(valorCalidad);
			var numRatio = parseInt(valorRatio);
			var numTotalCart = pCalidad / numRatio;
			var numBox = parseInt(numBoxPPK);
			// var numBox = parseInt($("#Packing_CantBox").val());
			$("#Packing_TotalCartonsPPK").val(numTotalCart);

			if (numBox === 0) {
				$("#Packing_TotalCartonesFaltPPK").val(numTotalCart);
			} else {
				var restar = parseInt(numTotalCart) - parseInt(numBox);
				$("#Packing_TotalCartonesFaltPPK").val(restar);
			}*/



            $.each(listaEmpaque, function (key, item) {
                tipoEmp = item.NombreTipoPak;
            });

            html += '<table class="table" id="tablaTallasPallet"><thead>';
            html += '<tr><th style="visibility:hidden;"> </th> ' +
                '<th>Size</th>' +
                ' <th>Pieces#</th>' +
                ' <th>TotalPieces#</th>' +
                ' <th>FaltantePieces#</th>' +
                '</tr>' +
                '</thead><tbody>';
            var cantidadesEmp = 0;
            var cantidadesT = 0;
            var cantidadPiezas;
            var cantidadRatio;
            var cont = 0;

            $.each(listaEmpaque, function (key, item) {
                cont = cont + 1;
                var nombreBulk = ".qtyBulkPcs" + namePack;
                var nombreBulk2 = ".qtyBulkTPcs" + namePack; 
                var valorPiezas = $(nombreBulk).parent("tr").find("td").eq(cont).text();
                var valorTPiezas = $(nombreBulk2).parent("tr").find("td").eq(cont).text();


                var resta;
                if (parseInt(valorTPiezas) <= parseInt(valorPiezas)) {
                    resta = parseInt(valorPiezas) - parseInt(valorTPiezas);
                } 
                
               

                html += '<tr id="pallet' + cont + '" class="pallet">';
                html += '<td width="1" style="visibility:hidden;"><input type="text" id="f-id" class="form-control" value="' + item.IdPackingTypeSize + '" /></td>';
                html += '<td width="250"><input type="text" id="f-talla" class="form-control talla" value="' + item.Talla + '" readonly/></td>'; //onChange="calcular_TotalPiezas()"
                html += '<td width="250"><input type="text" name="l-piezas" id="l-piezas" class="form-control numeric cant" onkeyup="obtTotalMat(' + cont + ')" value="' + item.Pieces + '"  readonly/></td>';
                cantidadesEmp += item.Pieces;
                if (resta !== 0) {
                    html += '<td width="250"><input type="text" name="l-totalPiezas" id="l-totalPiezas' + cont + '" class="form-control numeric totalPiezas" onkeyup="ValidarPiezasTotales(' + cont + ')" value="' + parseInt(item.Cantidad) + '" /></td>';
                } else {
                    html += '<td width="250"><input type="text" name="l-totalPiezas" id="l-totalPiezas' + cont + '" class="form-control numeric totalPiezas" onkeyup="ValidarPiezasTotales(' + cont + ')" value="' + parseInt(valorTPiezas) + '" readonly/></td>';
                }

               

                html += '<td width="250"><input type="text" name="l-totalPiezasFaltantes" id="l-totalPiezasFaltantes' + cont +'" class="form-control numeric totalPiezasFaltantes" value="' + parseInt(resta) + '" readonly/></td>';
                html += '</tr>';

            });

            html += '</tbody> </table>';
            $("#div_titulo").html("<h3></h3>");
            $("#div_titulo").css("display", "inline");
            $("#opciones").css("display", "none");
            $("#btnAdd").hide();
            $("#nuevoPalletBulks").show();
            $("#registrarNuevo").hide();
            $('#listaTallaPacking').html(html);

        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {

    });
}

function registrarPalletPPK(EstiloId) {
	$("#listaTallaPacking").show();
    $('label[for="Packing_TotalCartonsPPK"]').show();
    $("#Packing_TotalCartonsPPK").show();
    $('label[for="Packing_TotalCartonesFaltPPK"]').show();
	$("#Packing_TotalCartonesFaltPPK").show();
	$("#nuevoPalletPPK").hide();
	
    $.ajax({
        url: "/Packing/Lista_Tallas_Por_Estilo_PPK/" + EstiloId,
        method: 'POST',
        dataType: "json",
        success: function (jsonData) {
            var listaT = jsonData.Data.listaTalla;
            var listaPacking = jsonData.Data.listaPackingS;
            var listaEmpaque = jsonData.Data.listaEmpaqueTallas;
            var html = '';
            $("#btnAddP").hide();
            $("#modificarBatch").hide();
            $("#registrarNuevo").hide();
            $("#nuevoPalletBulks").hide();	
            $("#opcionesPack").css("display", "inline");
            $("#div_estilo_pack").html("<h3>REGISTRATION OF PALLET</h3>");
			$("#div_estilo_pack").css("display", "inline");
			$("#listaTallaPacking").css('height', '350px');
            var tipoEmp = "";
            var valorCalidad = $(".calidad").parent("tr").find("td").eq(1).text();
            var valorRatio = $(".numRatio").parent("tr").find("td").eq(1).text();
            var pCalidad = parseInt(valorCalidad);
            var numRatio = parseInt(valorRatio);
            var numTotalCart = pCalidad / numRatio;
            var numBox = parseInt(numBoxPPK);
           // var numBox = parseInt($("#Packing_CantBox").val());
            $("#Packing_TotalCartonsPPK").val(numTotalCart);
            
            if (numBox === 0) {
                $("#Packing_TotalCartonesFaltPPK").val(numTotalCart);
            } else {
                var restar = parseInt(numTotalCart) - parseInt(numBox);
                $("#Packing_TotalCartonesFaltPPK").val(restar);
            } 

            $.each(listaEmpaque, function (key, item) {
                tipoEmp = item.NombreTipoPak;
            });
            
            $('#Packing_PackingTypeSize_NombreTipoPak').show();
            $('#Packing_PackingTypeSize_NombreTipoPak').val(tipoEmp);
            $('label[for="Packing_CantBox"]').show();
            $("#Packing_CantBox").show();
            $("#Packing_CantBox").val(0);
            $("#Packing_Turnos").val(0);
                html += '<table class="table" id="tablaTallasPallet"><thead>';
            html += '<tr><th style="visibility:hidden;"> </th> ' +
                    '<th>Size</th>' +
                    ' <th>CantxBox#</th>' +
                    ' <th>TotalPieces#</th>' +
                    '</tr>' +
                '</thead><tbody>';
            var cantidadesEmp = 0;
            var cantidadesT = 0;
            var cantidadPiezas;
            var cantidadRatio;
            var cont = 0;
        
            $.each(listaEmpaque, function (key, item) {
                cont = cont + 1;           
                
                html += '<tr id="pallet' + cont + '" class="pallet">';
                html += '<td width="1" style="visibility:hidden;"><input type="text" id="f-id" class="form-control" value="' + item.IdPackingTypeSize + '" /></td>';
                html += '<td width="250"><input type="text" id="f-talla" class="form-control talla" value="' + item.Talla + '" readonly/></td>'; //onChange="calcular_TotalPiezas()"
                html += '<td width="250"><input type="text" name="l-ratio" id="l-ratio" class="form-control numeric cant" onkeyup="obtTotalMat(' + cont + ')" value="' + item.Ratio + '"  readonly/></td>';
                cantidadesEmp += item.Ratio;
                html += '<td width="250"><input type="text" name="l-totalPiezas" id="l-totalPiezas" class="form-control numeric totalPiezas" value="' + 0 + '" readonly/></td>';
                html += '</tr>';
             
            });

            html += '</tbody> </table>';
            $("#div_titulo").html("<h3></h3>");
            $("#div_titulo").css("display", "inline");
            $("#opciones").css("display", "none");
            $("#btnAdd").hide();
            $("#nuevoPallet").show();
            $("#registrarNuevo").hide();
            $('#listaTallaPacking').html(html);

        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {

    });
}

function MostrarOpcionesPalletPPKS() {
	$("#listaTallaPacking").show();
	$('label[for="Packing_TotalCartonsPPK"]').show();
	$("#Packing_TotalCartonsPPK").show();
	$('label[for="Packing_TotalCartonesFaltPPK"]').show();
	$("#Packing_TotalCartonesFaltPPK").show();
	$("#listaTallaPacking").hide();
	$("#nuevoPallet").css("display", "none");
    $("#nuevoPalletPPK").css("display", "none");
    $("#nuevoPalletBulks").css("display", "none");	
	$("#opcionesPackVArios").css("display", "inline");
	$("#div_estilo_pack").html("<h3>REGISTRATION OF PALLET</h3>");
	$("#div_estilo_pack").css("display", "inline");
}

function MostrarOpcionesPalletBulks() {
    $("#opcSelectPackBULKS").css("display", "inline");
    $("#div_estilo_pack").html("<h3>REGISTRATION OF PALLET</h3>");
    $("#div_estilo_pack").css("display", "inline");
}

$(function () {
	$('#selectPackingNameVariosPPKS').change(function () {
		$("#Packing_PackingAssort_CantCartons").css('border', '1px solid #cccccc');
		$('#Packing_PackingAssort_Turnos').css('border', '1px solid #cccccc');
		$("#tablaTallasAssortReg").hide();
		$("#Packing_TotalCartonsPPK").val('0');
		$("#modificarPack").hide();
		$("#editarBPPack").show();
		var selectedText = $(this).find("option:selected").text();
		var selectedValue = $(this).val();
		var html = '';
		var namePack = $("#selectPackingNameVariosPPKS option:selected").val();
		if (namePack !== "") {
			registrarPalletVariosPPK2(namePack);

			//$("#regAssort").show();
		} else {
			// $("#regAssort").hide();
		}

	});
});

$(function () {
    $('#selectPackingNameVariosBULKS').change(function () {
        $("#tablaTallasAssortReg").hide();
        var selectedText = $(this).find("option:selected").text();
        var selectedValue = $(this).val();
        var html = '';
        var namePack = $("#selectPackingNameVariosBULKS option:selected").val();
        if (namePack !== "") {
            registrarPalletVariosBulks(estiloId, namePack);
            //$("#regAssort").show();
        } else {
            $("#nuevoPalletBulks").hide();
            $("#tablaTallasPallet").hide();
            // $("#regAssort").hide();
        }

    });
});

function registrarPalletVariosPPK2(namePack) {
	$("#listaTallaPacking").show();
	$("#nuevoPalletPPK").show();
	var actionData = "{'id':'" + estiloId + "','packName':'" + namePack + "'}"; 
	$.ajax({
		url: "/Packing/Lista_Tallas_Empaque_Varios_PPK_Por_Estilo/",
		method: 'POST',
		dataType: "json",
		data: actionData,
		contentType: "application/json;charset=UTF-8",
		success: function (jsonData) {
			var listaT = jsonData.Data.listaTalla;
			var listaPacking = jsonData.Data.listaPackingS;
			var listaEmpaque = jsonData.Data.listaEmpaqueTallas;
			var CantidadTotalCartones = jsonData.Data.totalCartones;
			var html = '';
			$("#btnAddP").hide();
			$("#modificarBatch").hide();
			$("#registrarNuevo").hide();		
			
			var tipoEmp = "";
			//var valorCalidad = $(".calidad").parent("tr").find("td").eq(1).text();
			//var valorRatio = $(".numRatio").parent("tr").find("td").eq(1).text();
			//var pCalidad = parseInt(valorCalidad);
			//var numRatio = parseInt(valorRatio);
			//var numTotalCart = pCalidad / numRatio;		
			var nombreCant = ".box" + namePack;
			var campoCaja = $(nombreCant).parent("tr").find("td").text();
			var numeroBox = getNumbersInString(campoCaja);
			var numBox = parseInt(numeroBox);
			var numCartons = parseInt(CantidadTotalCartones);
			// var numBox = parseInt($("#Packing_CantBox").val());
			$("#Packing_TotalCartonsPPKS").val(numCartons);			
			var numTotalCart = $("#Packing_TotalCartonsPPKS").val();
			if (numBox === 0) {
				$("#Packing_TotalCartonesFaltPPKS").val(numCartons);
			} else {
				var restar = parseInt(numTotalCart) - parseInt(numBox);
				$("#Packing_TotalCartonesFaltPPKS").val(restar);
			}

			$.each(listaEmpaque, function (key, item) {
				tipoEmp = item.NombreTipoPak;
			});

			$('#Packing_PackingTypeSize_NombreTipoPak').show();
			$('#Packing_PackingTypeSize_NombreTipoPak').val(tipoEmp);
			$('label[for="Packing_CantBox"]').show();
			$("#Packing_CantBox").show();
			$("#Packing_CantBox").val(0);
			$("#Packing_Turnos").val(0);
			html += '<table class="table" id="tablaTallasPallet"><thead>';
			html += '<tr><th style="visibility:hidden;"> </th> ' +
				'<th>Size</th>' +
				' <th>Ratio#</th>' +
				' <th>TotalPieces#</th>' +
				'</tr>' +
				'</thead><tbody>';
			var cantidadesEmp = 0;
			var cantidadesT = 0;
			var cantidadPiezas;
			var cantidadRatio;
			var cont = 0;

			$.each(listaPacking, function (key, item) {
				cont = cont + 1;

				html += '<tr id="pallet' + cont + '" class="pallet">';
				html += '<td width="1" style="visibility:hidden;"><input type="text" id="f-id" class="form-control" value="' + item.IdPackingTypeSize + '" /></td>';
				html += '<td width="250"><input type="text" id="f-talla" class="form-control talla" value="' + item.Talla + '" readonly/></td>'; //onChange="calcular_TotalPiezas()"
				html += '<td width="250"><input type="text" name="l-ratio" id="l-ratio" class="form-control numeric cant" onkeyup="obtTotalMat(' + cont + ')" value="' + item.Ratio + '"  readonly/></td>';
				cantidadesEmp += item.Ratio;
				html += '<td width="250"><input type="text" name="l-totalPiezas" id="l-totalPiezas" class="form-control numeric totalPiezas" value="' + 0 + '" readonly/></td>';
				html += '</tr>';

			});

			html += '</tbody> </table>';
			$("#div_titulo").html("<h3></h3>");
			$("#div_titulo").css("display", "inline");
			$("#opciones").css("display", "none");
			$("#btnAdd").hide();		
			$("#registrarNuevo").hide();
			$('#listaTallaPacking').html(html);

		},
		error: function (errormessage) {
			alert(errormessage.responseText);
		}
	}).done(function (data) {

	});
}

function getNumbersInString(string) {
	var tmp = string.split("");
	var map = tmp.map(function (current) {
		if (!isNaN(parseInt(current))) {
			return current;
		}
	});

	var numbers = map.filter(function (value) {
		return value !== undefined;
	});

	return numbers.join("");
}

$(function () {
    $("#btnAddP").hide();
    $("#nuevoEmpaque").hide();
    $("#nuevoEmpaquePPK").hide();
    $("#registrarNuevo").hide();
    $('#Packing_PackingTypeSize_TipoEmpaque').change(function () {
        var selectedText = $(this).find("option:selected").text();
        var selectedValue = $(this).val();
          var html = '';
        if (selectedValue === "1") {
            TallasEmpaqueBulk();
            $("#packingBulk").css("visibility", "visible");
            $("#btnAddP").show();
            $("#nuevoEmpaque").show();
            $("#nuevoEmpaquePPK").hide();
        } else if (selectedValue === "2") {
            TallasEmpaquePPK();
            $("#packingBulk").css("visibility", "visible");
            $("#btnAddP").show();
            $("#nuevoEmpaquePPK").show();
            $("#nuevoEmpaque").hide();
        }
    });
});

function TallasEmpaqueBulk() {
	$("#tablaTallasBulkPcs").show();
	$("#tablaTallasPPKRatio").hide();
    $.ajax({
        url: "/Packing/Lista_Tallas_Empaque_Por_Estilo/" + estiloId,
        method: 'POST',
        dataType: "json",
        success: function (jsonData) {
            var listaT = jsonData.Data.listaTalla;
            var listaPacking = jsonData.Data.listaPackingS;
			var html = '';
			var htmlB = '';
            if (listaPacking.length === 0) {
                $("#div_titulo").html("<h3>REGISTRATION OF TYPE OF PACKAGING</h3>");
                $("#div_titulo").css("display", "inline"); 
               // $("#btnAddP").hide(); 
                $("#modificarBatch").hide();                
               /* html += '<table class="table" id="tablaTallasBulk"><thead>';
                html += '<tr><th>Size</th>' +
                    ' <th>Pieces#</th>' +
                    '</tr>' +
                    '</thead><tbody>';*/
                $.each(listaT, function (key, item) {
                    html += '<tr>';
                    html += '<td width="250"><input type="text" id="f-talla" class="form-control talla" value="' + item.Talla + '"/></td>';
                    html += '<td width="250"><input type="text" name="l-cantidad" id="l-cantidad" class="form-control numeric " value="' + 0 + '"  /></td>';
                    html += '<td width="250"><button type="button" id="btnDelete" class="deleteTalla btn btn btn-danger btn-xs" value="4">Delete</button></td>';
                    html += '</tr>';
                });
               // html += '</tbody> </table>';
				$('.tbodyTallaBulkPcs').html(html);
                htmlB += ' <button type="button" id="nuevoEmpaque" class="btn btn-success btn-md pull-right btn-sm"><span class="glyphicon glyphicon-floppy-disk" aria-hidden="true"></span> Save</button>';
                $('#listaTallaP').html(htmlB);
            } else {
                $("#div_titulo").html("<h3>PACKING DETAILS</h3>");
                $('#Packing_PackingTypeSize_TipoEmpaque').hide();
                $.each(listaPacking, function (key, item) {
                    $('#Packing_PackingTypeSize_NombreTipoPak').val(item.NombreTipoPak);
                });
               
                $("#div_titulo").css("display", "inline");
                html += '<tr> <th width="30%"> Size </th>';
                $.each(listaPacking, function (key, item) {
                    html += '<th width="30%">' + item.Talla + '</th>';
                });
                html += '</tr><tr><td width="30%">#Pieces</td>';
                var cantidades = 0;
                var cadena_cantidades = "";
                $.each(listaPacking, function (key, item) {
                    html += '<td width="30%">' + item.Pieces + '</td>';
                    cantidades += item.Pieces;
                    cadena_cantidades += "*" + item.Pieces;
                });
                html += '</tr>';
                $('.tbodyBulk').html(html);
            }
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {
    });
}

function TallasEmpaquePPK() {
	$("#tablaTallasBulkPcs").hide();
	$("#tablaTallasPPKRatio").show();
    $.ajax({
        url: "/Packing/Lista_Tallas_Empaque_Por_Estilo/" + estiloId,
        method: 'POST',
        dataType: "json",
        success: function (jsonData) {
            var listaT = jsonData.Data.listaTalla;
            var listaPacking = jsonData.Data.listaPackingS;
			var html = '';
			var htmlB = '';
            if (listaPacking.length === 0) {
                $("#div_titulo").html("<h3>REGISTRATION OF TYPE OF PACKAGING</h3>");
                $("#div_titulo").css("display", "inline");
                
              /*  html += '<table class="table" id="tablaTallasPPK"><thead>';
                html += '<tr><th>Size</th>' +
                    ' <th>Ratio</th>' +
                    '</tr>' +
                    '</thead><tbody>';*/
                $.each(listaT, function (key, item) {
                    html += '<tr>';
                    html += '<td width="250"><input type="text" id="f-talla" class="form-control talla" value="' + item.Talla + '"/></td>';
                    html += '<td width="250"><input type="text" name="l-cantidad" id="l-cantidad" class="form-control numeric " value="' + 0 + '"  /></td>';
                    html += '<td width="250"><button type="button" id="btnDelete" class="deleteTalla btn btn btn-danger btn-xs" value="4">Delete</button></td>';
                    html += '</tr>';
                });
               // html += '</tbody> </table>';
				$('.tbodyTallaPPKRatio').html(html);
                htmlB += '<button type="button" id="nuevoEmpaquePPK" class="btn btn-success btn-md pull-right btn-sm"><span class="glyphicon glyphicon-floppy-disk" aria-hidden="true"></span> Save</button>';               
                $('#listaTallaP').html(htmlB);
            } else {
                $("#div_titulo").html("<h3>PACKING DETAILS</h3>");
                $('#Packing_PackingTypeSize_TipoEmpaque').hide();
                $.each(listaPacking, function (key, item) {
                    $('#Packing_PackingTypeSize_NombreTipoPak').val(item.NombreTipoPak);
                });

                $("#div_titulo").css("display", "inline");
                html += '<tr> <th width="30%"> Size </th>';
                $.each(listaPacking, function (key, item) {
                    html += '<th width="30%">' + item.Talla + '</th>';
                });
                html += '</tr><tr><td width="30%">#Ratio</td>';
                var cantidades = 0;
                var cadena_cantidades = "";
                $.each(listaPacking, function (key, item) {
                    html += '<td width="30%">' + item.Ratio + '</td>';
                    cantidades += item.Ratio;
                    cadena_cantidades += "*" + item.Ratio;
                });
                html += '</tr>';
                $('.tbodyBulk').html(html);
            }
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {
    });
}
var listaPO;
function obtener_tallas_PO(IdEstilo) {
    $.ajax({
        url: "/Pedidos/Lista_Tallas_Estilo/" + IdEstilo,
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var html = '';

            listaPO = jsonData.Data.listaTalla;
        },
        error: function (errormessage) { alert(errormessage.responseText); }

    });

}

function obtenerIdEstilo(IdEstilo) {
    $.ajax({
        url: "/Arte/ObtenerIdEstilo/" + IdEstilo,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        success: function () {
        },
        error: function (errormessage) { alert(errormessage.responseText); }

    });

}

function obtener_bacth_estilo_pack(IdEstilo) {
    var tempScrollTop = $(window).scrollTop();
    if (numTipoPack === undefined) {
        numTipoPack = 0;
    }
    //  $("#loading").css('display', 'inline');
    $.ajax({
        url: "/Packing/Lista_Batch_Estilo/",
        type: "POST",
        data: JSON.stringify({ id: IdEstilo, tipoEmpaque: numTipoPack }),
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
              
            var lista_batch = jsonData.Data.listaPO;
			var cargoUser = jsonData.Data.cargoUser;
			var sucursal = jsonData.Data.sucursal;
            var numBatch = 0;
            $.each(lista_batch, function (key, item) {
                numBatch++;
            });
            if (numBatch === 0) {
                // $("#div_tabla_talla").hide();
				//$("#panelNoEstilosBPPK").css('display', 'inline');
				var html2 = '';   
				html2 += '<table class="table table-sm table-striped table-hover" id="tablaTallasBulk"><thead></thead><tbody>';
				if (Object.keys(lista_batch).length === 0) {
					html2 += '<tr class="odd"><td valign="middle" align="center" colspan="10" class="dataTables_empty">No batches were found for the style.</td></tr>';
				}
				html2 += '</tbody> </table>';
				$('#listaTallaBatch').html(html2);
            } else {
                var html = '';   
				var estilos = jsonData.Data.estilos;
				$("#panelNoEstilosBPPK").css('display', 'none');
                if (estilos !== '') {
                    $("#div_titulo").html("<h3>BATCH REVIEW </h3>");
                    $("#div_titulo").css("display", "inline");
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
                     html += '<th>' + item.Talla + '</th>';
                });
            } else {
                $.each(size, function (key, item) {
                    html += '<th>' + item.Talla + '</th>';
                });
                }
                
           // 
            //html += '<th> Total </th>';
                if (tipoEmpaque === 2) {
                    html += '<th> Box# </th>';
                } 
				html += '<th> Type Packing </th>';
                if (tipoEmpaque === 4 || tipoEmpaque === 5) {
                    html += '<th> Name Pack </th>';
                } 

            html += '<th> Date </th>';
            html += '<th> User </th>';
            html += '<th> Shift </th>';
            html += '<th> User Modif </th>';
            html += '<th> Actions </th>';
            html += '</tr>';

            html += '</thead><tbody>';
            $.each(lista_batch, function (key, item) {
                html += '<tr><td>Pallet-' + item.IdBatch + '</td>';

                var cantidad = 0;
                $.each(item.Batch, function (key, i) {
                    html += '<td class="total" >' + i.TotalPiezas + " PCS"+'</td>';
                    //html += '<td class="total" >' + i.TotalPiezas + '</td>';               
                });
                if (item.TipoEmpaque === 2) {
                    
                    $.each(item.Batch, function (key, i) {
                       /* if (key === 1) {
                            html += '<td>' + i.CantBox + '</td>';
                        } else */
                        if (key === 0) {
                            html += '<td>' + i.CantBox + '</td>';
                        }
                        
                    });
                }
                if (numTipoPack === 0) {
                    numTipoPack = item.TipoEmpaque;
                }
                if (numTipoPack === 1) {                   
                    html += '<td>BULK</td>';
                } else if (numTipoPack === 2) {
                  html += '<td>PPK</td>';
				} else if (numTipoPack === 4) {
					html += '<td>PPKS</td>';
                } else if (numTipoPack === 5) {
                    html += '<td>BULKS</td>';
                } else {
                    html += '<td>ASSORTMENT</td>';
				}
                if (item.TipoEmpaque === 4 || item.TipoEmpaque === 5) {
                    html += '<td>' + item.NombreEmpaque + '</td>';
                }

                if (item.FechaPacking !== "-") {
                    html += '<td>' + item.FechaPacking + '</td>';
                }
                else {
                    html += '<td>' + item.FechaPacking + '</td>';
                }

				html += '<td>' + item.NombreUsr + '</td>';

				//if (sucursal === "FORTUNE") {
					if (item.TipoTurno === 1) {
						html += '<td>First Turn</td>';
					} else {
						html += '<td>Second Turn</td>';
					}
				//} else if (sucursal === "LUCKY1") {
				//	if (item.TipoTurno === 1) {
				//		html += '<td>First Turn - Lucky1</td>';
				//	} else {
				//		html += '<td>Second Turn - Lucky1</td>';
//}
					//	html += '<td>Lucky1</td>';
				//}

                html += '<td>' + item.NombreUsrModif + '</td>';
               
                if (/*cargoUser === 9 ||*/ cargoUser === 1) {
                    html += '<td><a href="#" onclick="obtenerTallas_Batch(' + item.IdBatch + ',' + item.TipoTurno + ',' + item.IdPacking + ',' + numTipoPack  /*+ ',\'' + item.Status + '\'*/ + ');" class = "btn btn-default glyphicon glyphicon-search l1s" style = "color:black; padding:0px 5px 0px 5px;" Title = "Details Bacth"></a>';
                    html += '<a href="#" onclick="event.preventDefault();ConfirmDeleteBatchPack(' + item.IdBatch + ',' + IdEstilo + ',' + numTipoPack + ')" class="btn btn-default glyphicon glyphicon-trash l1s" style="color:black; padding:0px 5px 0px 5px;" Title="Delete Bacth"></a></td > ';
                } else if (cargoUser === 9) {
                    html += '<td><a href="#" onclick="event.preventDefault();ConfirmDeleteBatchPack(' + item.IdBatch + ',' + IdEstilo + ',' + numTipoPack + ')" class = "btn btn-default glyphicon glyphicon-trash l1s" style = "color:black; padding:0px 5px 0px 5px;" Title = "Delete Bacth"></a></td>';
                }
                else {
                    html += '</td>';
                }
               
                html += '</tr>';
            });
            if (Object.keys(lista_batch).length === 0) {
                html += '<tr class="odd"><td valign="middle" align="center" colspan="10" class="dataTables_empty">No batches were found for the style.</td></tr>';
            }
            html += '</tbody> </table>';
                $('#listaTallaBatch').html(html);
                $('#listaTallaBatch').show();
                $("#div_titulo").css("display", "inline");
				$("#modificarPack").prop("disabled", true);
				$("#editarPack").prop("disabled", true);
				
            // $("#loading").css('display', 'none');
				$(window).scrollTop(tempScrollTop);
				var IdEstiloInf = $("#InfoSummary_IdItems").val();
				obtenerListaPacking(IdEstiloInf);			
				$("#containerPie").css("display", "inline");
            }            
        },
        error: function (errormessage) { alert(errormessage.responseText); }
    });
}

function ConfirmDeleteBatchPack(idBatch, idSummary) {
    alertify.confirm("Are you sure you want to delete pallet ?", function (result) {
        $.ajax({
            url: '/Packing/EliminarBatchPackings/',
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

function obtenerImagenPNL(nombreEstilo, numEstilo) {
	$('#imagenPNL').attr('src', '/Arte/ConvertirImagenPNLEstilo?nombreEstilo=' + nombreEstilo + '&IdItem=' + numEstilo);
	//+ '&color=' + color
}

function obtenerImagenArte(nombreEstilo) {
	$('#imagenArte').attr('src', '/Arte/BuscarImagenArte?nombreEstilo=' + nombreEstilo);
}

function ConfirmRev(a) {
    alertify.confirm("Are you sure you want to modify the pallet?", function (result) {
        actualizarPallet();
        $("#modificarBatch").prop("disabled", true);
    }).set({
        title: "Confirmation"
    });
}

function ConfirmCambioPack(a) {
    alertify.confirm("Are you sure to change the type of packing?", function (result) {
        CambiarTipoPacking();
    }).set({
        title: "Confirmation"
    });
}

function ConfirmEditarPack(a) {
	alertify.confirm("Are you sure to edit the type of packing?", function (result) {
		$('#packingModal').modal('show');
	}).set({
		title: "Confirmation"
	});
}

function CambiarTipoPacking() {
    var tipoEmpaque = $('#Packing_PackingTypeSize_NombreTipoPak').val();
    var idEmpaque = 0;
    if (tipoEmpaque === "BULK") {
        idEmpaque = 1;
    } else if (tipoEmpaque === "PPK") {
        idEmpaque = 2;
    } else {
        idEmpaque = 3;
    }
    $.ajax({
        url: "/Packing/Actualizar_Tipo_Packing",
        datatType: 'json',
        data: JSON.stringify({ EstiloID: estiloId, TipoEmpaque: idEmpaque }),
        cache: false,
        type: 'POST',
        contentType: 'application/json',
        success: function (data) {
            alertify.set('notifier', 'position', 'top-right');
            alertify.notify('The type of packing has been changed correctly.', 'success', 5, null);
            obtenerListaTallas(estiloId);
        }
    });
}


//Registrar tallas
    $(document).on("click", "#nuevaTalla", function () {
        var error = 0;
        var r = 0; var c = 0; var i = 0; var x = 1; var cadena = new Array(2);
        cadena[0] = ''; cadena[1] = '';
		var nFilas = $("#tableQtySize tbody>tr").length;
		var nColumnas = $("#tableQtySize tr:last td").length;
        var total = 0;      
        var cadena_cantidades = "";
        var cantidades_array = "";
		$('#tableQtySize tbody>tr').each(function () {           
            $(this).find(".qualityT").each(function () {
                $(this).closest('td').find(".qualityT").each(function () {
                    cadena_cantidades += this.value + "*";
                    cantidades_array = cadena_cantidades.split('*');               
                });      
            });
        });
        var cadena_cantidades_Talla = "";
        var cantidades_array_Talla = "";
        $.each(listCantTalla, function (key, item) {
            //total = item.TotalPieces + parseInt(this.value);
            cadena_cantidades_Talla += item.Cantidad + "*";
            cantidades_array_Talla = cadena_cantidades_Talla.split('*');  
        });
        var errorT = 0;
        $.each(listaPsc, function (key, item) {
            //total = item.TotalPieces + parseInt(this.value);
            var sum = parseInt(cantidades_array[i]) + parseInt(item.TotalPieces);
            var result = parseInt(cantidades_array_Talla[i]);
            var nombre = "#l-cantidad" + i;
            if (parseInt(sum) > parseInt(result)) {
                errorT++;
                $(nombre).css('border', '2px solid #e03f3f');                
            } else {
                $(nombre).css('border', '1px solid #cccccc');
            }
            i++;
        });
       
		$('#tableQtySize tbody>tr').each(function () {
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
        
		$('#tableQtySize').find('td').each(function (i, el) {

            var valor = $(el).children().val();

            if ($(el).children().val() === '' || $(el).children().val() === '0' ) {
                error++;
                $(el).children().css('border', '2px solid #e03f3f');

            }/* else {
                $(el).children().css('border', '1px solid #cccccc');

            }*/
        });
            enviarListaTalla(cadena, error, errorT);
        
    });


function enviarListaTalla(cadena, error, errorT) {
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else if (errorT !== 0) {
        var alertE = alertify.alert("Message", 'Review the quantities of the packaging exceed the order.').set('label', 'Aceptar');
        alertE.set({ transition: 'zoom' });
        alertE.set('modal', false);
    } else {
        $.ajax({
            url: "/Packing/Obtener_Lista_Tallas_Packing",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena, EstiloID: estiloId }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The sizes were correctly registered.', 'success', 5, null);
                obtenerListaTallas(estiloId);
            }
        });
    }
}

//Registrar tallas Bulk

//$(document).ready(function () {
$(document).on("click", "#nuevoEmpaque", function ()  {
        var r = 0; var c = 0; var i = 0; var cadena = new Array(2);
        cadena[0] = ''; cadena[1] = '';
		var nFilas = $("#tablaTallasBulkPcs tbody>tr").length;
		var nColumnas = $("#tablaTallasBulkPcs tr:last td").length;
		$('#tablaTallasBulkPcs tbody>tr').each(function () {
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
		$('#tablaTallasBulkPcs').find('td').each(function (i, el) {
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
        enviarListaTallaBulk(cadena, error);
    });
//});

function enviarListaTallaBulk(cadena, error) {
    var idTipoP = $("#Packing_PackingTypeSize_TipoEmpaque option:selected").val();
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
            url: "/Packing/Obtener_Lista_Tallas_Packing_Bulk",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena, EstiloID: estiloId, TipoPackID: idTipoP }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The type of packaging is registered correctly.', 'success', 5, null);
                obtenerListaTallas(estiloId);
            }
        });
    }
}

//Registrar tallas PPK
   
    $(document).on("click", "#nuevoEmpaquePPK", function () {
        var r = 0; var c = 0; var i = 0; var cadena = new Array(2);
        cadena[0] = ''; cadena[1] = '';
		var nFilas = $("#tablaTallasPPKRatio tbody>tr").length;
		var nColumnas = $("#tablaTallasPPKRatio tr:last td").length;
		$('#tablaTallasPPKRatio tbody>tr').each(function () {
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
        $('#tablaTallasPPKRatio').find('td').each(function (i, el) {
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
        enviarListaTallaPPK(cadena, error);
    });

function enviarListaTallaPPK(cadena, error) {
    var idTipoP = $("#Packing_PackingTypeSize_TipoEmpaque option:selected").val();
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
            url: "/Packing/Obtener_Lista_Tallas_Packing_PPK",
            datatType: 'json',
            data: JSON.stringify({ ListTalla: cadena, EstiloID: estiloId, TipoPackID: idTipoP}),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The type of packaging is registered correctly.', 'success', 5, null);
                obtenerListaTallas(estiloId);
            }
        });
    }
}

//Registrar pallet
$(document).ready(function () {
    $('#nuevoPallet').on('click', function () {
        var tipoEmpaque = $('#Packing_PackingTypeSize_NombreTipoPak').val();
        if (tipoEmpaque === "BULK") {
            $("#Packing_CantBox").val(0);
            obtenerPalletBulk();         
        } else if (tipoEmpaque === "PPK") {
            obtenerPalletPPK();
         }   
        
	});

	$('#nuevoPalletPPK').on('click', function () {
		obtenerPalletPPKS();

    });
    $('#nuevoPalletBulks').on('click', function () {
        obtenerPalletBulks();

    });
});

function enviarListaTallaPallet(cadena, error) {
    var idTipoTurno = $("#Packing_Turnos option:selected").val();
    var numCaja = $("#Packing_CantBox").val();
    var tipoEmpaque = $('#Packing_PackingTypeSize_NombreTipoPak').val();
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {        
        $.ajax({
            url: "/Packing/Obtener_Lista_Tallas_Packing_Pallet",
            datatType: 'json',
            data: JSON.stringify({
                ListTalla: cadena, EstiloID: estiloId, TipoTurnoID: idTipoTurno, NumCaja: numCaja, TipoEmpaque: tipoEmpaque}),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The packaging was registered correctly.', 'success', 5, null);
                obtenerListaTallas(estiloId);
               
            }
        });
    }
}

function enviarListaTallaPalletPPKS(cadena, error) {
	var idTipoTurno = $("#Packing_TurnosPPK option:selected").val();
	var numCaja = $("#Packing_CantBoxPPK").val();
	var tipoEmpaque = $('#Packing_PackingTypeSize_NombreTipoPak').val();
	var namePack = $("#selectPackingNameVariosPPKS option:selected").val();
	if (error !== 0) {
		var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
		alert.set({ transition: 'zoom' });
		alert.set('modal', false);
	} else {
		$.ajax({
			url: "/Packing/Obtener_Lista_Tallas_Packing_PPKS_Pallet",
			datatType: 'json',
			data: JSON.stringify({
				ListTalla: cadena, EstiloID: estiloId, TipoTurnoID: idTipoTurno, NumCaja: numCaja, TipoEmpaque: tipoEmpaque, NamePack: namePack}),
			cache: false,
			type: 'POST',
			contentType: 'application/json',
			success: function (data) {
				alertify.set('notifier', 'position', 'top-right');
				alertify.notify('The packaging was registered correctly.', 'success', 5, null);
				obtenerListaTallas(estiloId);

			}
		});
	}
}
var batchID;
function actualizarPallet() {  
        var tipoEmpaque = $('#Packing_PackingTypeSize_NombreTipoPak').val();
        if (tipoEmpaque === "BULK") {
            $("#Packing_CantBox").val(0);
            obtenerPalletBulkAct(batchID);
        } else if (tipoEmpaque === "PPK") {
            obtenerPalletPPKAct(batchID);
        }

  
}

function enviarListaTallaPalletAct(cadena, error, batchID) {
    var idTipoTurno = $("#Packing_Turnos option:selected").val();
    var numCaja = $("#Packing_CantBox").val();
    var tipoEmpaque = $('#Packing_PackingTypeSize_NombreTipoPak').val();
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {       
        $.ajax({
            url: "/Packing/Actualizar_Lista_Tallas_Batch",
            datatType: 'json',
            data: JSON.stringify({
                ListTalla: cadena, EstiloID: estiloId, TipoTurnoID: idTipoTurno, NumCaja: numCaja, TipoEmpaque: tipoEmpaque, IdBatch: batchID
            }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The packaging was modified correctly.', 'success', 5, null);
                obtenerListaTallas(estiloId);
                //$("#tablaTallasBulk").table("refresh");        
                //$('#tablaTallasBulk').table().data("table").refresh();
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

function enviarListaTallaPalletBulks(cadena, error) {
    var idTipoTurno = $("#Packing_TurnosBulks option:selected").val();
    var namePack = $("#selectPackingNameVariosBULKS option:selected").val();
    if (error !== 0) {
        var alert = alertify.alert("Message", 'All fields are required.').set('label', 'Aceptar');
        alert.set({ transition: 'zoom' });
        alert.set('modal', false);
    } else {
        $.ajax({
            url: "/Packing/Obtener_Lista_Tallas_Packing_Bulks_Pallet",
            datatType: 'json',
            data: JSON.stringify({
                ListTalla: cadena, EstiloID: estiloId, TipoTurnoID: idTipoTurno, NamePack: namePack
            }),
            cache: false,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                alertify.set('notifier', 'position', 'top-right');
                alertify.notify('The packaging was registered correctly.', 'success', 5, null);
                obtenerListaTallas(estiloId);
                $("#selectPackingNameVariosBULKS  option:selected").prop("selected", false);
                $("#selectPackingNameVariosBULKS  option:first").prop("selected", "selected");

            }
        });
    }
}

function obtenerTallas_Batch(idBatch, idTurno, idPacking, idTipoEmpaque) {
    // var tempScrollTop = $(window).scrollTop(); 
    $("#Packing_Turnos").val(idTurno);
    $('#Packing_Turnos').css('border', '');
    $("#div_titulo").css("display", "inline");
    $("#modificarBatch").prop("disabled", false);
    if (idTipoEmpaque === 1) {
         actualizarEmpaqueBulk(idBatch);
    } else if (idTipoEmpaque === 2) {
         actualizarEmpaquePPK(idBatch);
     }
                
}

function actualizarEmpaqueBulk(idBatch) {    
    batchID = idBatch;
    var actionData = "{'idEstilo':'" + estiloId + "','idBatch':'" + idBatch + "'}";
    $.ajax({
        url: "/Packing/Lista_Tallas_Packing_IdEstilo_IdBatch/",
        type: "POST",
        data: actionData,
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var listaT = jsonData.Data.listaTalla;
            var lista_batch = jsonData.Data.listaPrint;
            var listaEmpaque = jsonData.Data.listaEmpaqueTallas;
            var html = '';
            $("#btnAddP").hide();
            $("#opcionesPack").css("display", "inline");
            $("#div_estilo_pack").html("<h3>DETAILS PALLET</h3>");
            $("#div_estilo_pack").css("display", "inline");         
            $("#modificarBatch").show();
            $("#registrarNuevo").show();
            var tipoEmp = "";
            $.each(listaEmpaque, function (key, item) {
                tipoEmp = item.NombreTipoPak;
            });
            $('#Packing_PackingTypeSize_NombreTipoPak').show();
            $('#Packing_PackingTypeSize_NombreTipoPak').val(tipoEmp);
            $('label[for="Packing_CantBox"]').hide();
            $("#Packing_CantBox").hide();
            html += '<table class="table" id="tablaTallasPallet"><thead>';
            html += '<tr><th style="visibility:hidden;"> </th> ' +
                '<th> Size</th> ' +
                '<th>Box#</th>' +
                '<th>CantxBox#</th>' +
                '<th>PartialBox#</th>' +
                '<th>TotalPieces#</th>' +
                '<th>TotalBox</th>' +
                '<th>BoxFaltante#</th>' +
                '</tr>' +
                '</thead><tbody>';     

            var cantidadesEmp = 0;
            var cantidadesT = 0;
            var cantidadPiezas;
            var cantidadRatio;
            var cont = 0;

            $.each(listaEmpaque, function (key, item) {
                cont = cont + 1;
                var valorCalidad = $(".calidad").parent("tr").find("td").eq(cont).text();
                var pCalidad = parseInt(valorCalidad);
                var numTotalBox = pCalidad / item.Pieces;
                var totalBox = Math.floor(numTotalBox);
                var valorCajas = $(".cajasQty").parent("tr").find("td").eq(cont).text();
                var nCajas = parseInt(valorCajas);
                if (isNaN(nCajas)) {
                    nCajas = 0;
                }
                var totalCajas = parseInt(totalBox);
                var resta = 0;
                if (numTotalBox < 1) {
                    totalCajas = 1;
                    resta = parseInt(totalCajas) - nCajas;
                }
                var partial = parseInt(totalCajas);
                if (pCalidad > item.Pieces) {
                    if (numTotalBox < 1) {
                        totalBox = totalBox;
                    } if (numTotalBox % 1 === 0) {
                        totalBox = totalBox;
                    } else {
                        totalBox = totalBox + 1;
                    }
                    resta = parseInt(totalBox) - nCajas;
                }
                var x = item.PackingM;
                
                html += '<tr id="pallet' + cont + '" class="pallet">';
                html += '<td width="1" style="visibility:hidden;"><input type="text" id="f-id" class="form-control " value="' + x.IdPacking + '"/></td>';
                html += '<td width="250"><input type="text" id="f-talla" class="form-control talla" value="' + item.Talla + '" readonly/></td>';
                if (x.CantBox === 0) {
                    html += '<td width="250" class="cBox"><input type="text" name="l-cantidadBox" id="l-cantidadBox" class="form-control numeric cantCajas" onkeyup="obtTotalMat(' + cont + ')" value="' + x.CantBox + '" readonly></td>';
                } else {
                    html += '<td width="250" class="cBox"><input type="text" name="l-cantidadBox" id="l-cantidadBox" class="form-control numeric cantCajas" onkeyup="obtTotalMat(' + cont + ')" value="' + x.CantBox + '"></td>';
                }
                
                html += '<td width="250"><input type="text" name="l-piezas" id="l-piezas" class="form-control numeric cant"  value="' + item.Pieces + '"  readonly/></td>';
              //  var valorCalidad = $(".calidad").parent("tr").find("td").eq(cont).text();
              //  var pCalidad = parseInt(valorCalidad);
             //   var numTotalBox = pCalidad / item.Pieces;
             //   var totalBox = Math.floor(numTotalBox);


                if (x.Partial === 0) {
                    html += '<td width="250"><input type="text" name="l-totBoxPartial" id="l-totBoxPartial" class="form-control numeric totBoxPartial" onblur="ActualizarPiezasPackingBulk(' + cont + ')" value="' + x.Partial + '" readonly/></td>';
                } else {
                    html += '<td width="250"><input type="text" name="l-totBoxPartial" id="l-totBoxPartial" class="form-control numeric totBoxPartial" onblur="ActualizarPiezasPackingBulk(' + cont + ')" value="' + x.Partial + '" /></td>';
                }
                
                html += '<td width="250"><input type="text" name="l-totalPiezas" id="l-totalPiezas" class="form-control numeric totalPiezas" value="' + x.TotalPiezas + '" readonly/></td>';
                html += '<td width="250"><input type="text" name="l-totBox" id="l-totBox" class="form-control numeric totBox" value="' + totalBox + '" readonly/></td>';
               // var valorCajas = $(".cajasQty").parent("tr").find("td").eq(cont).text();
               // var nCajas = parseInt(valorCajas);
               // var resta = parseInt(totalBox) - nCajas;
                if (parseInt(valorCajas) === 0) {
                    html += '<td width="250" class="tFalB"><input type="text" name="l-totFaltantes" id="l-totFaltantes" class="form-control numeric totFaltantes" value="' + totalBox + '" readonly/></td>';
                } else {
                    html += '<td width="250" class="tFalB"><input type="text" name="l-totFaltantes" id="l-totFaltantes" class="form-control numeric totFaltantes" value="' + resta + '" readonly/></td>';
                }
                html += '</tr>';
            });

            html += '</tbody> </table>';
            $("#opciones").css("display", "none");
            $("#btnAdd").hide();
            $("#nuevoPallet").hide();
            $('#listaTallaPacking').html(html);

        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {

    });
}

function actualizarEmpaquePPK(idBatch) {  
    batchID = idBatch;
    var actionData = "{'idEstilo':'" + estiloId + "','idBatch':'" + idBatch + "'}";    
    $.ajax({
        url: "/Packing/Lista_Tallas_Packing_PPK_IdEstilo_IdBatch/",
        type: "POST",
        data: actionData,
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var listaT = jsonData.Data.listaTalla;
            var lista_batch = jsonData.Data.listaPrint;
            var listaEmpaque = jsonData.Data.listaEmpaqueTallas;
            var html = '';
            $("#btnAddP").hide();
            $("#opcionesPack").css("display", "inline");
            $("#div_estilo_pack").html("<h3>DETAILS PALLET</h3>");
            $("#div_estilo_pack").css("display", "inline");         
            $("#modificarBatch").show();
            $("#registrarNuevo").show();

            var tipoEmp = "";
            $.each(listaEmpaque, function (key, item) {
                tipoEmp = item.NombreTipoPak;
            });
            $('#Packing_PackingTypeSize_NombreTipoPak').show();
            $('#Packing_PackingTypeSize_NombreTipoPak').val(tipoEmp);
            $('label[for="Packing_CantBox"]').show();
            $("#Packing_CantBox").show();
            html += '<table class="table" id="tablaTallasPallet"><thead>';
            html += '<tr><th style="visibility:hidden;"> </th> ' +
                '<th>Size</th>' +
                ' <th>CantxBox#</th>' +
                ' <th>TotalPieces#</th>' +
                '</tr>' +
                '</thead><tbody>';
            var cantidadesEmp = 0;
            var cantidadesT = 0;
            var cantidadPiezas;
            var cantidadRatio;
            var cont = 0;
            $.each(listaEmpaque, function (key, item) {
                cont = cont + 1;
                var x = item.PackingM;
                $("#Packing_CantBox").val(x.CantBox);
                html += '<tr id="pallet' + cont + '" class="pallet">';
                html += '<td width="1" style="visibility:hidden;"><input type="text" id="f-id" class="form-control" value="' + x.IdPacking+ '" /></td>';
                html += '<td width="250"><input type="text" id="f-talla" class="form-control talla" value="' + item.Talla + '" readonly/></td>'; //onChange="calcular_TotalPiezas()"
                html += '<td width="250"><input type="text" name="l-ratio" id="l-ratio" class="form-control numeric cant" onkeyup="obtTotalMat(' + cont + ')" value="' + item.Ratio + '"  readonly/></td>';
                cantidadesEmp += item.Ratio;
                html += '<td width="250"><input type="text" name="l-totalPiezas" id="l-totalPiezas" class="form-control numeric totalPiezas" value="' + x.TotalPiezas + '" readonly/></td>';
                html += '</tr>';
            });

            html += '</tbody> </table>';            
            $("#opciones").css("display", "none");
            $("#btnAdd").hide();
            $("#nuevoPallet").hide();
            $('#listaTallaPacking').html(html);

        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {

    });
}
