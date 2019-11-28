
var tabs = [
    {
        id: 0,
        text: "WIP",
        icon: "card",
        content: "WIP tab content"
    },
    {
        id: 1,
        text: "SHIPPED",
        icon: "box",
        content: "SHIPPED tab content"
    },
    {
        id: 2,
        text: "CANCELLED",
        icon: "clear",
        content: "CANCELLED tab content"
    }
];


var priorities = ["Normal", "Modify"];

var sucursales = ["Fortune", "Lucky1"];

   

function TablaRecibos(IdSum, IdPed, IdEst) {
    var IdSummary = parseInt(IdSum);
    var IdPedido = parseInt(IdPed);
    var IdEstilo = parseInt(IdEst);
    var actionData = "{'idSummary':'" + IdSummary + "','idPedido':'" + IdPedido + "','idEstilo':'" + IdEstilo + "'}";
    $.ajax({
        url: "/Pedidos/Lista_Tallas_Estilo_Recibos/",
        type: "POST",
        data: actionData,
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var html = '';
            var estilos = jsonData.Data.estilos;
            var EstiloDescription;
            var lista_estilo_Desc = jsonData.Data.listaTalla;
            $.each(lista_estilo_Desc, function (key, item) {

                EstiloDescription = item.DescripcionEstilo;

            });

            var lista_estilo = jsonData.Data.listaTalla;
            listaEstiloPO = lista_estilo;
            html += '<tr> <th>  </th>';
            var cadena_talla = "";
            $.each(lista_estilo, function (key, item) {

                html += '<th>' + item.Talla + '</th>';
                cadena_talla += "*" + item.IdTalla;

            });
            html += '<th> Total </th>';
            var cantidades_array_talla = cadena_talla.split('*');
            html += '</tr><tr><td>PO Quantity</td>';
            var cantidadesPO = 0;
            var cadena_cantidades = "";
            $.each(lista_estilo, function (key, item) {

                html += '<td class="total" >' + item.Cantidad + '</td>';
                cantidadesPO += item.Cantidad;
                cadena_cantidades += "*" + item.Cantidad;
            });
            var cantidades_array = cadena_cantidades.split('*');
            html += '<td>' + cantidadesPO + '</td>';
            html += '</tr>';
            var numTallas = 0;
            $.each(lista_estilo, function (key, item) {
                numTallas++;
            });

            html += '<tr><td>Receipt Quantity</td>';
            var cantidades = 0;
            var lista_Recibo = jsonData.Data.listaRecibos;
            var listado_Recibos = jsonData.Data.listadoRecibo;       
            var numRecibos = 0;
            var cadena_Tallas_Rec = "";
            $.each(lista_Recibo, function (key, item) {
                numRecibos++;
               
            });
           
            var cont = 1;
            if (lista_Recibo.length === 0) {
                var totalR = 0;
                for (var v = 0; v < numTallas; v++) {

                    html += '<td>' + totalR + '</td>';
                    cantidades += totalR;
                }
            } else {
                var cont2 = 0;
                //if (numRecibos !== numTallas) {

                    $.each(lista_estilo, function (key, item) {
                        var temp = 0;
                        $.each(lista_Recibo, function (key, itemRec) {

                            if (item.IdTalla === itemRec.Inventario.id_size) {
                                temp = itemRec.Inventario.total;
                                cantidades += itemRec.Inventario.total;
                            }
                        });
                        html += '<td>' + temp + '</td>';
                        cadena_Tallas_Rec += "*" + temp;
                    });
                    html += '<td>' + cantidades + '</td>';
                //}
            }
            var cantidades_array_Size_Rec = cadena_Tallas_Rec.split('*');
            html += '</tr>';
            var contador = 1;
            $.each(listado_Recibos, function (key, itemRec) {
                html += '</tr><tr><td class="total">Receipt#' + contador + '-' + itemRec.fecha + '</td>';


                $.each(lista_estilo, function (key, item) {
                    if (item.IdTalla === itemRec.Inventario.id_size) {

                        html += '<td>' + itemRec.Inventario.total + '</td>';

                    } else {
                        html += '<td>' + 0 + '</td>';
                    }
                });
                contador++;

            });

            html += '</tr>';
            html += '</tr><tr ><td class="total">+/-</td>';
            var totales = 0;
            var i = 1;
            var sumaTotal = 0; 
            for (i; i < numTallas+1; i++) {
                var resta = parseInt(cantidades_array_Size_Rec[i]) - parseFloat(cantidades_array[i]);
                if (resta === 0) {
                    html += '<td class="restaPrint" style="color:black;">' + resta + '</td>';
                } else if (resta >= 0) {
                    html += '<td class="restaPrint" style="color:blue;">' + resta + '</td>';
                } else {
                    html += '<td class="restaPrint" style="color:red;">' + resta + '</td>';
                }               
                sumaTotal += resta;
            }
            html += '<td>' + sumaTotal + '</td>';

            html += '</tr>';
            if (Object.keys(lista_estilo).length === 0) {
                html += '<tr class="odd"><td valign="middle" align="center" colspan="10" class="dataTables_empty">No receipt were found for the style.</td></tr>';
            }
            $('.tbodys').html(html);

        },
        error: function (errormessage) { alert(errormessage.responseText); }
    });
}



function TablaTrims(IdSum,IdPed) {
    var IdPedido = parseInt(IdPed);
    var IdSummary = parseInt(IdSum);
    var actionData = "{'idSummary':'" + IdSummary + "','idPedido':'" + IdPedido + "'}";
    $.ajax({
        url: "/Pedidos/Lista_Trims/",
        type: "POST",
        data: actionData,
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var html = '';
            var html2 = '';
            var lista_trims = jsonData.Data.listaTrims;

            html += '<tr>';
            html += '<th> # </th>';
            html += '<th> TRIMS </th>';
            html += '<th> DESCRIPTION </th>';
            html += '<th> SIZE </th>';
            html += '<th> TOTAL </th>';
            html += '<th> REMAINING </th>';
            html += '</tr>';
          
            $.each(lista_trims, function (key, item) {
                html += '<tr>';
                html += '<td>' + item.id_request + '</td>';
                html += '<td>' + item.Descripcion + '</td>';
                html += '<td>' + item.tipo_item + '</td>';
                html += '<td>' + item.talla + '</td>';
                html += '<td>' + item.total + '</td>';
                html += '<td>' + item.restante + '</td>';
                html += '</tr>';
            });            
                     
            if (Object.keys(lista_trims).length === 0) {
                html += '<tr class="odd"><td valign="middle" align="center" colspan="10" class="dataTables_empty">No trims were found for the style.</td></tr>';
            }
            $('.tbodyTrims').html(html);

        },
        error: function (errormessage) { alert(errormessage.responseText); }
    });
}

function TablaPriceTicketsTrims(IdPed) {
    var IdPedido = parseInt(IdPed);
    var actionData = "{'idPedido':'" + IdPedido + "'}";
    $.ajax({
        url: "/Pedidos/Lista_Price_Tickets_Trims/",
        type: "POST",
        data: actionData,
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var html = '';

            var lista_trims = jsonData.Data.listaTrims;

            html += '<tr>';
            html += '<th> # </th>';
            html += '<th> TRIMS </th>';
            html += '<th> DESCRIPTION </th>';
            html += '<th> SIZE </th>';
            html += '<th> TOTAL </th>';
            html += '<th> REMAINING </th>';
            html += '</tr>';

            $.each(lista_trims, function (key, item) {
                html += '<tr>';
                html += '<td>' + item.Id_request_pt + '</td>';
                html += '<td>' + item.Descripcion + '</td>';
                html += '<td>' + item.Tipo_item + '</td>';
                html += '<td>' + item.Talla + '</td>';
                html += '<td>' + item.Total + '</td>';
                html += '<td>' + item.Restante + '</td>';
                html += '</tr>';
            });

            if (Object.keys(lista_trims).length === 0) {
                html += '<tr class="odd"><td valign="middle" align="center" colspan="10" class="dataTables_empty">No price tickets were found for the style.</td></tr>';
            }
            $('.tbodyPTrims').html(html);

        },
        error: function (errormessage) { alert(errormessage.responseText); }
    });
}

function exportGridWip() {
    var exportedHandler1 = function (e) {
        // First handler of the "exported" event
    };
}


var exportedHandler2 = function (e) {
    // Second handler of the "exported" event
};

function GridWIP(ordenes, output, comments) {
    var editCells = [];
	var popup = $("#popup").dxPopup({
		title: "BLANKS",
        width: 500,
        height: 300,
        animation: { show: { type: 'fade', duration: 10 }, hide: { type: 'fade', duration: 10 } },
	}).dxPopup("instance");

	var popupSuc = $("#popupSuc").dxPopup({
		title: "FACTORY",
		width: 300,
        height: 150,
        animation: { show: { type: 'fade', duration: 10 }, hide: { type: 'fade', duration: 10 } },
	}).dxPopup("instance");

	/*var popupTrims = $("#popupTrims").dxPopover({
		width: 600,
		height: 230
	}).dxPopover("instance");*/
	var popupTrims = $("#popupTrims").dxPopup({
		title: "TRIMS",
        width: 600,
        height: 300,
        closeOnBackButton: true,
        closeOnOutsideClick: true,  
        animation: { show: { type: 'fade', duration: 10 }, hide: { type: 'fade', duration: 10 } },
	}).dxPopup("instance");

	var popupPriceTrims = $("#popupPriceTrims").dxPopup({
		title: "PRICE TICKET",
        width: 600,
        height: 300,
        animation: { show: { type: 'fade', duration: 10 }, hide: { type: 'fade', duration: 10 } },
	}).dxPopup("instance");

    var gridOrd = $("#gridContainer").dxDataGrid({
        onInitialized: function (e) {
			gridOrd = e.component;
        },
        dataSource: {
			store: ordenes
        },
        keyExpr: "IdSummaryOrden",
        selection: {
            mode: "single"
        },
        export: {
            enabled: true,
            fileName: "WIP",
            //allowExportSelectedData: true,
            excelFilterEnabled: true,
            customizeExcelCell: options => {
                if (options.gridCell.rowType === 'header') {
                    options.backgroundColor = '#000000';
                    options.font.color = '#ffffff';
                    options.font.bold = true;
                }
                if (options.gridCell.rowType === 'data') {
                    if (options.gridCell.column.dataField === 'ImagenArte.StatusArteInf') {
						if (options.gridCell.data.ImagenArte.StatusArteInf === 'IN HOUSE') {
							options.font.bold = true;
							options.backgroundColor = '#44c174';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArte.StatusArteInf === 'REVIEWED') {
							options.font.bold = true;
							options.backgroundColor = '#66c2ff';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArte.StatusArteInf === 'PENDING') {
							options.font.bold = true;
							options.backgroundColor = '#ec5f5f';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArte.StatusArteInf === 'APPROVED') {
							options.font.bold = true;
							options.backgroundColor = '#ec8a47';
							options.font.color = '#000000';
						} else {
							options.font.bold = true;
							options.backgroundColor = '#ec5f5f';
							options.font.color = '#000000';
						}
					}
					if (options.gridCell.column.dataField === 'ImagenArtePnl.StatusArtePnlInf') {
						if (options.gridCell.data.ImagenArtePnl.StatusArtePnlInf === 'IN HOUSE') {
							options.font.bold = true;
							options.backgroundColor = '#44c174';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArtePnl.StatusArtePnlInf === 'REVIEWED') {
							options.font.bold = true;
							options.backgroundColor = '#66c2ff';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArtePnl.StatusArtePnlInf === 'PENDING') {
							options.font.bold = true;
							options.backgroundColor = '#ec5f5f';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArtePnl.StatusArtePnlInf === 'APPROVED') {
							options.font.bold = true;
							options.backgroundColor = '#ec8a47';
							options.font.color = '#000000';
						} else {
							options.font.bold = true;
							options.backgroundColor = '#ec5f5f';
							options.font.color = '#000000';
						}
					}
                    if (options.gridCell.column.dataField === 'PO') {
                        if (options.gridCell.data.RestaPrintshop <= 10) {
                            options.font.bold = true;
                            options.backgroundColor = '#5F9DCD';
                            options.font.color = '#000000';

                        } else {
                            options.font.bold = true;
                            options.font.color = '#101010';
                        }

                    }
                    if (options.gridCell.column.dataField === 'TotalRestante') {

                        if (options.gridCell.data.TotalRestante === 0) {
                            options.font.bold = true;
                            options.font.color = '#101010';
                        } else if (options.gridCell.data.TotalRestante !== options.gridCell.data.InfoSummary.TotalEstilo) {
                            options.font.bold = true;
                            options.font.color = '#F20101';
                        }
                    }

                    if (options.gridCell.column.dataField === 'InfoSummary.ItemDesc.Descripcion') {

                        if (options.gridCell.data.DestinoSalida === 2) {
                            options.font.bold = true;
                            options.font.color = '#000000';
                            options.backgroundColor = '#beaef1';
                        } else {
                            options.font.bold = true;
                            options.font.color = '#101010';
                        }
                    }

                    if (options.gridCell.column.dataField === 'Trims_fecha_recibo') {

                        if (options.gridCell.data.Trims.restante <= 0 && options.gridCell.data.Trims.estado === "1") {
                            options.font.bold = true;
                            options.font.color = '#101010';
                            options.backgroundColor = '#f5d543';
                        } else {
                            options.font.bold = true;
                            options.font.color = '#101010';
                        }
                    }

                    if (options.gridCell.column.dataField === 'CatTipoBrand.TipoBrandName') {
                        options.font.bold = true;
                    }
                    if (options.gridCell.column.dataField === 'CatTipoOrden.TipoOrden') {
                        options.font.bold = true;
                    }
                    if (options.gridCell.column.dataField === 'POSummary.ItemDescripcion.Descripcion') {
                        options.font.bold = true;
                    }
                    if (options.gridCell.column) {
                        if (options.gridCell.data.FechaRecOrden === output) {
                            options.font.color = '#101010';
                            options.backgroundColor = '#f5d543';
                            //options.rowElement.css('background', '#FEF2E0');
                            //options.rowElement.removeClass("dx-row-alt").addClass("");
                        }

					}
					if (options.gridCell.column.dataField === 'FechaOrdenFinal') {

						if (options.gridCell.data.FechaOrdenFinal < output) {							
							options.font.bold = true;
							options.font.color = '#101010';
						} else {
							options.font.bold = true;
							options.font.color = '#101010';
						}
					}
                    /*if (e.data.FechaRecOrden == output) {
                        e.rowElement.css('background', '#FEF2E0');
                        e.rowElement.removeClass("dx-row-alt").addClass("");
                    }*/
                }
            }
        },
        onExporting: function (e) {
            e.component.beginUpdate();
            e.component.columnOption("TipoPartial", "visible", true);
        },
        onExported: function (e) {
            e.component.columnOption("TipoPartial", "visible", false);
            e.component.endUpdate();
        },
        groupPanel: {
            visible: true
        },
        editing: {
            mode: "batch",
            allowUpdating: true
        },
        allowColumnReordering: true,
        allowColumnResizing: true,
        columnAutoWidth: true,
        showBorders: true,
        showColumnLines: true,
        showRowLines: true,
		rowAlternationEnabled: true,
		columnRenderingMode: "virtual",
		rowRenderingMode: "virtual", 
        /*scrolling: {
            columnRenderingMode: "virtual"
        },*/
        remoteOperations: false,
		scrolling: {
			//columnRenderingMode: "virtual"
			//mode: "virtual"
			mode: "virtual",
			columnRenderingMode: "virtual"
		},
		paging: {
			enabled: false
		},
        searchPanel: {
            visible: true,
            width: 200,
            placeholder: "Search..."
        },
        filterRow: {
            visible: true
        },
        headerFilter: {
            visible: true
        },
        loadPanel: {
            enabled: true,
            shadingColor: "rgba(255,255,255,0.4)",
            position: { of: "#gridContainer" },
            visible: false,
            showIndicator: true,
            showPane: true,
            shading: true
        },
        onRowPrepared: function (e) {
            if (e.rowType === 'data' && e.data.CatComentarios.FechaComents === output) {
                e.rowElement.css('background', '#FEF2E0');
                e.rowElement.removeClass("dx-row-alt").addClass("");
            }
        },
        onCellHoverChanged: function (e) {
            if (e.column.dataField === "TotalRestante") {
                if (e.rowType === 'data') {
                    if (e.key.TotalRestante !== 0) {
                        popup.option("contentTemplate",
                            function (contentElement) {
                                IdSum = e.key.IdSummaryOrden;
                                IdPed = e.key.IdPedido;
                                IdEst = e.key.IdEstilo;
                                var tabla = TablaRecibos(IdSum, IdPed, IdEst);
                                var gridPop = $("<div/>")
                                    .append(gridPop)
                                    .append('<div>' + e.key.InfoSummary.ItemDesc.ItemEstilo + '</div>')
                                    .append('<div>').addClass('table-wrapper-scroll-y my-custom-scrollbar')
                                    .append('<table id="dtHorizontalVerticalExample" cellspacing="0" width = "100%">').addClass('table table-sm table-striped table-hover')
                                    .append('<thead class="encabezado"></thead>')
                                    .append('<tbody class="tbodys"></tbody>')
                                    .append('</table>')
                                    .appendTo(contentElement);

                            });
                        popup.option("target", e.cellElement);
                        popup.show();
                    } else {
                        popup.hide();
                    }
                }
                popupTrims.hide();
                popupSuc.hide();
                popupPriceTrims.hide();
            }
            if (e.column.dataField === "Trims.fecha_recibo") {
                if (e.rowType === 'data') {
                    if (e.key.Trims.fecha_recibo !== null) {
                        popupTrims.option("contentTemplate",
                            function (contentElement) {
                                IdSum = e.key.IdSummaryOrden;
                                IdPed = e.key.IdPedido;
                                IdEst = e.key.IdEstilo;
                                var tablaTrims = TablaTrims(IdSum,IdPed);
                                var gridPopTrims = $("<div/>")
                                    .append(gridPopTrims)
                                    .append('<div>' + e.key.InfoSummary.ItemDesc.ItemEstilo + '</div>')
                                    .append('<div').addClass('table-wrapper-scroll-y my-custom-scrollbar')
                                    .append('<table id="tableGeneralRecibo">').addClass('table table-sm table-striped table-hover')
                                    .append('<thead class="encabezado"></thead>')
                                    .append('<tbody class="tbodyTrims"></tbody>')                        
                                    .append('</table>')
                                    .appendTo(contentElement);

                            });
                        popupTrims.option("target", e.cellElement);
                        popupTrims.show();
                    } else {
                        popupTrims.hide();
                    }
                }
                popupSuc.hide();
                popup.hide();
                popupPriceTrims.hide();
            }
            if (e.column.dataField === "InfoPriceTickets.Fecha_recibo") {
                if (e.rowType === 'data') {
                    if (e.key.Trims.fecha_recibo !== null) {
                        popupPriceTrims.option("contentTemplate",
                            function (contentElement) {
                                IdSum = e.key.IdSummaryOrden;
                                IdPed = e.key.IdPedido;
                                IdEst = e.key.IdEstilo;
                                var tablaPriceTrims = TablaPriceTicketsTrims(IdPed);
                                var gridPopPriceTrims = $("<div/>")
                                    .append(gridPopPriceTrims)
                                    .append('<div>' + e.key.InfoSummary.ItemDesc.ItemEstilo + '</div>')
                                    .append('<div').addClass('table-wrapper-scroll-y my-custom-scrollbar')
                                    .append('<table>').addClass('table table-sm table-striped table-hover')
                                    .append('<thead class="encabezado"></thead>')
                                    .append('<tbody class="tbodyPTrims"></tbody>')
                                    .append('</table>')
                                    .appendTo(contentElement);
                            });
                        popupPriceTrims.option("target", e.cellElement);
                        popupPriceTrims.show();
                    } else {
                        popupPriceTrims.hide();
                    }
                }
                popupTrims.hide();
                popupSuc.hide();
                popup.hide();
			}
			if (e.column.dataField === "InfoSummary.ItemDesc.Descripcion") {
                if (e.rowType === 'data') {                   
                    var numSucursal;
					if (e.key.InfoSummary.ItemDesc.Descripcion !== null) {
						popupSuc.option("contentTemplate",
							function (contentElement) {
								IdSum = e.key.IdSummaryOrden;
								IdPed = e.key.IdPedido;
								IdEst = e.key.IdEstilo;
                                var valor;
                                if (numSucursal == undefined) {
                                    if (e.key.InfoSummary.IdSucursal === 2) {
                                        valor = 2;
                                    } else {
                                        valor = 1;
                                    }
                                } else {
                                    valor = numSucursal;
                                }
								
                                ConsultarSucursalEstilo(IdSum);
								var datos = $('<div>')
									.dxRadioGroup({
										dataSource: [{
											id: 1,
											text: 'FORTUNE'
										}, {
											id: 2,
											text: 'LUCKY1'
										}],
										valueExpr: "id",
										displayExpr: "text",
										onValueChanged: function (dato) {
											//var d = $.Deferred();											
											var previousValue = dato.previousValue;
											var newSucursal = dato.value;
											//var text = radioGroup1.Properties.Items[radioGroup1.SelectedIndex].Description;
                                            ActualizarSucursalEstilo(/*d,*/ newSucursal, IdSum);
                                            
											if (newSucursal === 2) {													
												e.cellElement.css("background-color", "#beaef1");
                                                e.cellElement.css("color", "white");
                                                numSucursal = 2;
                                                e.key.InfoSummary.IdSucursal = 2;
                                                valor = numSucursal;
                                              
											} else {
												e.cellElement.css("background-color", "");
                                                e.cellElement.css("color", "black");	
                                                numSucursal = 1;
                                                e.key.InfoSummary.IdSucursal = 1;
                                                valor = numSucursal;
											}
										//	return d.promise();
										},
                                        value: valor,
										layout: "horizontal"
									});
								var gridPopSucursal = $("<div/>")
									.append(gridPopSucursal)
									.append('<div>' + e.key.InfoSummary.ItemDesc.Descripcion + '</div>')
									.append('<br/>')
									.append(datos)						
									.appendTo(contentElement);
							});
						popupSuc.option("target", e.cellElement);
						popupSuc.show();
					} else {
						popupSuc.hide();
					}
                }
                popupTrims.hide();
                popup.hide();
                popupPriceTrims.hide();
            }        

        },
        columns: [
            /* {
                 caption: "STATUS",
                 dataField: "status",
                 allowEditing: false,
                 cssClass: "myClass",
                 headerCellTemplate: $('<b style="color: gray">STATUS</b>')
                 cellTemplate: function (cellElement, cellInfo) {
                     $('<div>')
                         .appendTo(cellElement)
                         .dxRadioGroup({
                             items: priorities,
                             value: priorities[0],
                             layout: "horizontal"
                         })
                 }
             },*/
            {
                caption: "#",
                dataField: "IdSummaryOrden",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">#</b>')
            },
            {
                caption: "CUSTOMER",
                dataField: "CatCliente.Nombre",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">CUSTOMER</b>')
            }, {
                caption: "RETAILER",
                dataField: "CatClienteFinal.NombreCliente",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">RETAILER</b>')
            }, {
                caption: "PO RECVD DATE",
                dataField: "FechaRecOrden",
                alignment: "center",
                dataType: "date",
                format: "dd/MM/yyyy",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">PO RECVD DATE</b>')

            }, {
                caption: "PO NO",
                dataField: "PO",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">PO NO</b>'),
                cellTemplate: function (element, info) {
                    if (info.data.RestaPrintshop <= 10) {
                        element.append('<div>' + info.text + '</div>')
                            .css("background-color", "#5F9DCD");
                        element.append('<div></div>')
                            .css("color", "white");

                    } else {
                        element.append('<div>' + info.text + '</div>')
                            .css("color", "black");
                    }
                }
            }, {
                caption: "BRAND NAME",
                dataField: "CatTipoBrand.TipoBrandName",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">BRAND NAME</b>')
            }, {
                caption: "AMT PO",
                dataField: "VPO",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">AMT PO</b>')
            }, {
                caption: "REG/BULK",
                dataField: "CatTipoOrden.TipoOrden",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">REG/BULK</b>')
            }, {
                caption: "BALANCE QTY",
                dataField: "InfoSummary.CantidadEstilo",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">BALANCE QTY</b>')
            }, {
                caption: "EXPECTED SHIP DATE",
                dataField: "FechaOrdenFinal",
                alignment: "center",
                dataType: "date",
                format: "dd/MM/yyyy",
                allowEditing: false,
                cssClass: "myClass",
				headerCellTemplate: $('<b style="color: gray">EXPECTED SHIP DATE</b>'),
				cellTemplate: function (element, info) {
					if (info.data.FechaOrdenFinal < output) {
						info.text = "TBD";
						element.append('<div>' + info.text + '</div>')
							.css("color", "black");

					} else {
						element.append('<div>' + info.text + '</div>')
							.css("color", "black");
					}
				}
            }, {
                caption: "ORIGINAL CUST DUE DATE",
                dataField: "FechaCancelada",
                alignment: "center",
                dataType: "date",
                format: "dd/MM/yyyy",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">ORIGINAL CUST DUE DATE</b>')

            }, {
                caption: "DESIGN NAME",
                dataField: "InfoSummary.ItemDesc.Descripcion",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">DESIGN NAME</b>'),
				cellTemplate: function (element, info) {					
					//info.data.DestinoSalida
				if (info.data.InfoSummary.IdSucursal === 2) {
                        element.append('<div>' + info.text + '</div>')
                            .css("background-color", "#beaef1");
                        element.append('<div></div>')
                            .css("color", "white");	
                    } else {
                        element.append('<div>' + info.text + '</div>')
							.css("color", "black");			
					}								
                }

            }, {
                caption: "STYLE",
                dataField: "InfoSummary.ItemDesc.ItemEstilo",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">STYLE</b>')
            }, {
                caption: "MILL PO",
                dataField: "MillPO",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">MILL PO</b>')
            }, {
                caption: "COLOR",
                dataField: "InfoSummary.CatColores.DescripcionColor",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">COLOR</b>')
            }, {
                caption: "GENDER",
                dataField: "InfoSummary.CatGenero.Genero",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">GENDER</b>')
            }, {
                caption: "BLANKS RECEIVED",
                dataField: "TotalRestante",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">BLANKS RECEIVED</b>'),
                cellTemplate: function (container, options) {
                    /*$('<a/>').addClass('dx-link')
                        .text('details')
                        .on('dxclick', function () {
                            $("#popup").dxPopup("instance").show();
                            $("#txt").dxTextArea("instance").option("value", options.data.PO);
                        }).appendTo(container);*/
                    if (options.data.TotalRestante === 0) {
                        options.text = "";
                        container.append("<div>" + options.text + "</div>")
                            .css("color", "black");
                    } else if (options.data.TotalRestante !== options.data.InfoSummary.TotalEstilo) {
                        container.append("<div>" + options.text + "</div>")
                            .css("color", "red");
                    }
                }
            }, {
                caption: "PARTIAL/COMPLETE BLANKS",
                dataField: "TipoPartial",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                visible: false,
                headerCellTemplate: $('<b style="color: gray">PARTIAL/COMPLETE BLANKS</b>'),
                cellTemplate: function (element, info) {
                    if (info.text === 'PARTIAL') {
                        element.append('<div>' + info.text + '</div>')
                            .css("background-color", "#F9881D");
                        element.append('<div></div>')
                            .css("color", "white");

                    } else if (info.text === 'COMPLETE') {
                        element.append("<div>" + info.text + "</div>")
                            .css("background-color", "#40bf80");
                        element.append("<div></div>")
                            .css("color", "white");
                    }
                }
            }, {
                caption: "ART RECEIVED",
                dataField: "ImagenArte.StatusArteInf",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">ART RECEIVED</b>'),
                cellTemplate: function (element, info) {
                    if (info.text === 'IN HOUSE') {
                        element.append("<div>" + info.text + "</div>")
                            .css("background-color", "#44c174");
                        element.append("<div></div>")
                            .css("color", "white");
                       /* if (info.data.ImagenArte.extensionArte === "") {
                            info.data.ImagenArte.extensionArte = "/Content/img/noImagen.png";
                        }*/
                        element.append($('<img/>').attr('src', '/Arte/ConvertirImagenArte?extensionArte=' + info.data.ImagenArte.extensionArte).on('click', function (event) {
                            $('.enlargeImageModalSource').attr('src', $(this).attr('src'));
                            $('#enlargeImageModal').modal('show');
                        }));

                    } else if (info.text === 'REVIEWED') {
                        element.append("<div>" + info.text + "</div>")
                            .css("background-color", "#66c2ff");
                        element.append("<div></div>")
                            .css("color", "white");
                    } else if (info.text === 'PENDING') {
                        element.append("<div>" + info.text + "</div>")
                            .css("background-color", "#ec5f5f");
                        element.append("<div></div>")
                            .css("color", "white");
                    } else if (info.text === 'APPROVED') {   
                        var infoArte = info.text + "-" + info.data.ImagenArte.FechaArte;
                        element.append("<div>" + infoArte + "</div>")
                            .css("background-color", "#de5a00");
                        element.append("<div></div>")
							.css("color", "white");
						/*if (info.data.ImagenArte.extensionArte === "") {
							info.data.ImagenArte.extensionArte = "/Content/img/noImagen.png";
						}*/
                        element.append($('<img />').attr('src', '/Arte/ConvertirImagenArte?extensionArte=' + info.data.ImagenArte.extensionArte).on('click', function (event) {
							$('.enlargeImageModalSource').attr('src', $(this).attr('src'));
							$('#enlargeImageModal').modal('show');
						}));
                    } 
                }
			}, {
				caption: "PNL RECEIVED",
				dataField: "ImagenArtePnl.StatusArtePnlInf",
				alignment: "center",
				allowEditing: false,
				cssClass: "myClass",
				headerCellTemplate: $('<b style="color: gray">PNL RECEIVED</b>'),
				cellTemplate: function (element, info) {
					if (info.text === 'IN HOUSE') {
						element.append("<div>" + info.text + "</div>")
							.css("background-color", "#44c174");
						element.append("<div></div>")
							.css("color", "white");
						/*if (info.data.ImagenArtePnl.ExtensionPNL === "") {
							info.data.ImagenArtePnl.ExtensionPNL = "/Content/img/noImagen.png";
						}*/
                        element.append($('<img/>').attr('src', '/Arte/ConvertirImagenArtePNL?extensionPnl=' + info.data.ImagenArtePnl.extensionPNL).on('click', function (event) {
							$('.enlargeImageModalSource').attr('src', $(this).attr('src'));
							$('#enlargeImageModal').modal('show');
						}));

					} else if (info.text === 'REVIEWED') {
						element.append("<div>" + info.text + "</div>")
                            .css("background-color", "#66c2ff");
						element.append("<div></div>")
							.css("color", "white");
					} else if (info.text === 'PENDING') {
						element.append("<div>" + info.text + "</div>")
							.css("background-color", "#ec5f5f");
						element.append("<div></div>")
							.css("color", "white");
					} else if (info.text === 'APPROVED') {
						var infoArte = info.text + "-" + info.data.ImagenArtePnl.FechaArtePnl;
						element.append("<div>" + infoArte + "</div>")
							.css("background-color", "#ec8a47");
						element.append("<div></div>")
							.css("color", "white");
						/*if (info.data.ImagenArtePnl.ExtensionPNL === "") {
							info.data.ImagenArtePnl.ExtensionPNL = "/Content/img/noImagen.png";
						}*/
                        element.append($('<img/>').attr('src', '/Arte/ConvertirImagenArtePNL?extensionPnl=' + info.data.ImagenArtePnl.extensionPNL).on('click', function (event) {
							$('.enlargeImageModalSource').attr('src', $(this).attr('src'));
							$('#enlargeImageModal').modal('show');
						}));
					}
				}
			}, {
                caption: "TRIM RECEIVED",
                dataField: "Trims.fecha_recibo",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                dataType: "date",
                format: "dd/MM/yyyy",
                headerCellTemplate: $('<b style="color: gray">TRIM RECEIVED</b>'),
                cellTemplate: function (container, options) {
                    if (options.data.Trims.restante <= 0 && options.data.Trims.estado === "1") {
                        container.append("<div>" + options.text + "</div>")
                            .css("background-color", "#44c174");
                        container.append("<div></div>")
                            .css("color", "white");
                    }
                    else if (options.data.Trims.restante >= 1 && options.data.Trims.estado === "1") {
                        container.append("<div >" + options.text + "</div>")
                            .css("background-color", "#f5d543");
                        container.append("<div></div>")
                            .css("color", "white");
                    } else {
                        container.append("<div>" + options.text + "</div>")
                            .css("color", "black");
                    }
                }
            }, {
                caption: "PACK INST. RCVD",
                dataField: "InfoPackInstruction.Fecha_Pack",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                dataType: "date",
                format: "dd/MM/yyyy",
                headerCellTemplate: $('<b style="color: gray">PACK INST. RCVD</b>'),
                cellTemplate: function (container, options) {
                    if (options.data.InfoPackInstruction.Fecha_Pack !== "" && options.data.InfoPackInstruction.EstadoPack === 1) {
                        container.append("<div>" + options.text + "</div>")
                            .css("background-color", "#44c174");
                        container.append("<div></div>")
                            .css("color", "white");
                    }
                     else {
                        container.append("<div>" + options.text + "</div>")
                            .css("color", "black");
                    }
                }
            }, {
                caption: "PRICE TICKET RECEIVED",
                dataField: "InfoPriceTickets.Fecha_recibo", 
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                dataType: "date",
                format: "dd/MM/yyyy",
                headerCellTemplate: $('<b style="color: gray">PRICE TICKET RECEIVED</b>'),
                cellTemplate: function (container, options) {
                    if (options.data.InfoPriceTickets.Restante <= 0 && options.data.InfoPriceTickets.Estado === "1") {
                        container.append("<div>" + options.text + "</div>")
                            .css("background-color", "#44c174");
                        container.append("<div></div>")
                            .css("color", "white");
                    }
                    else if (options.data.InfoPriceTickets.Restante >= 1 && options.data.InfoPriceTickets.Estado === "1") {
                        container.append("<div >" + options.text + "</div>")
                            .css("background-color", "#f5d543");
                        container.append("<div></div>")
                            .css("color", "white");
                    } else {
                        container.append("<div>" + options.text + "</div>")
                            .css("color", "black");
                    }
                }
            }, {
                caption: "UCC RECEIVED",
                dataField: "InfoSummary.FechaUCC",
                alignment: "center",
                cssClass: "myClass",
                dataType: "date",
                format: "dd/MM/yyyy",
                headerCellTemplate: $('<b style="color: gray">UCC RECEIVED</b>'),
                cellTemplate: function (container, options) {
                    if (options.text !== "") {
                        container.append("<div>" + options.text + "</div>")
                            .css("background-color", "#44c174");
                        container.append("<div></div>")
                            .css("color", "white");
                    } else {
                        options.text = " ";
                        container.append("<div>" + options.text + "</div>")
                            .css("color", "black");
                        //.css("background-color", "#f5d543"); amarillo
                    }
                }
            }, {
                caption: "COMMENTS UPDATE",
                dataField: "CatComentarios.FechaComents",
                alignment: "center",
                dataType: "date",
                format: "dd/MM/yyyy",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">COMMENTS UPDATE</b>')
            }, {
                caption: "COMMENTS",
                dataField: "CatComentarios.Comentario",
                alignment: "center",
                id: "COMENTARIO",
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">COMMENTS</b>')
            }, {
                caption: "ID COMMENT",
                dataField: "CatComentarios.IdComentario",
                visible: false,
                allowEditing: false,
                id: "ID"
            }
        ],
        onCellPrepared: function (e) {
            /*if (e.rowType === "data" && $.inArray(e.rowIndex + ":" + e.columnIndex, editCells) >= 0) {
                e.cellElement.css("background-color", "lightblue");
            }*/
        },
        onEditorPreparing: function (e) {
            var grid = e.component;
            if (e.parentType === "dataRow") {
                var oldOnValueChanged = e.editorOptions.onValueChanged;
                e.editorOptions.onValueChanged = function (e) {
                    oldOnValueChanged.apply(this, arguments);
                    for (var i = 0; i < editCells.length; i++) {
                        var rowIndex = Number(editCells[i].split(":")[0]);
                        var columnIndex = Number(editCells[i].split(":")[1]);
                        grid.cellValue(rowIndex, columnIndex, e.value);
                    }
                }
            }
        },
        onCellClick: function (e) {
            if (e.jQueryEvent.ctrlKey) {
                editCells.push(e.rowIndex + ":" + e.columnIndex);
            }
            else if (editCells.length) {
                editCells = [];
                e.component.repaint();
            }
        },
        onContentReady: function (e) {
            e.element.find(".dx-datagrid-save-button").click(function (e) {
                if (editCells.length)
                    editCells = [];
            });
            e.element.find(".dx-datagrid-cancel-button ").click(function (e) {
                if (editCells.length)
                    editCells = [];
            });
        },
        masterDetail: {
            enabled: true,
            template: function (container, options) {
                var currentEmployeeData = options.data;
                $("<div>")
                    .addClass("master-detail-caption")
                    .text("COMMENTS LIST " + "PO- " + currentEmployeeData.PO)
                    .appendTo(container);

                $("<div>")
                    .dxDataGrid({
                        showBorders: true,
                        columns: [{
                            caption: "COMMENT",
                            dataField: "Comentario",
                            width: 350
                        },
                        {
                            caption: "DATE COMMENT",
                            dataField: "FechaComents",
                            dataType: "date",
                            format: "dd/MM/yyyy",
                            width: 150
                        },
                        {
                            caption: "USER",
                            dataField: "NombreUsuario",
                            width: 350
                        }
                        ],
                        dataSource: new DevExpress.data.DataSource({
                            key: "IdSummaryOrden",
                            store: comments,
                            filter: ["IdSummary", "=", options.key.IdSummaryOrden]
                        })
                    }).appendTo(container);
            }
        }
    }).dxDataGrid("instance");
	return gridOrd;
}

function GridShipping(ordersShipped, output, commentsShipped) {
	var popup = $("#popup").dxPopup({
		title: "BLANKS",
		width: 500,
		height: 300
	}).dxPopup("instance");

	var popupSuc = $("#popupSuc").dxPopup({
		title: "FACTORY",
		width: 300,
		height: 150
	}).dxPopup("instance");

	var popupTrims = $("#popupTrims").dxPopup({
		title: "TRIMS",
		width: 600,
        height: 300,
        animation: { show: { type: 'fade', duration: 0 }, hide: { type: 'fade', duration: 0 } }
	}).dxPopup("instance");

	var popupPriceTrims = $("#popupPriceTrims").dxPopup({
		title: "PRICE TICKET",
		width: 600,
		height: 300
	}).dxPopup("instance");

    var gridShip = $("#gridContainer").dxDataGrid({
        onInitialized: function (e) {
            gridShip = e.component;
        },
        dataSource: {
            store: ordersShipped
        },
        keyExpr: "IdSummaryOrden",
        selection: {
            mode: "single"
        },
        export: {
            enabled: true,
            fileName: "SHIPPED",
            //allowExportSelectedData: true,
            excelFilterEnabled: true,
            customizeExcelCell: options => {
                if (options.gridCell.rowType === 'header') {
                    options.backgroundColor = '#000000';
                    options.font.color = '#ffffff';
                    options.font.bold = true;
                }
                if (options.gridCell.rowType === 'data') {
					if (options.gridCell.column.dataField === 'ImagenArte.StatusArteInf') {
						if (options.gridCell.data.ImagenArte.StatusArteInf === 'IN HOUSE') {
							options.font.bold = true;
							options.backgroundColor = '#44c174';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArte.StatusArteInf === 'REVIEWED') {
							options.font.bold = true;
							options.backgroundColor = '#66c2ff';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArte.StatusArteInf === 'PENDING') {
							options.font.bold = true;
							options.backgroundColor = '#ec5f5f';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArte.StatusArteInf === 'APPROVED') {
							options.font.bold = true;
							options.backgroundColor = '#ec8a47';
							options.font.color = '#000000';
						} else {
							options.font.bold = true;
							options.backgroundColor = '#ec5f5f';
							options.font.color = '#000000';
						}
					}
					if (options.gridCell.column.dataField === 'ImagenArtePnl.StatusArtePnlInf') {
						if (options.gridCell.data.ImagenArtePnl.StatusArtePnlInf === 'IN HOUSE') {
							options.font.bold = true;
							options.backgroundColor = '#44c174';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArtePnl.StatusArtePnlInf === 'REVIEWED') {
							options.font.bold = true;
							options.backgroundColor = '#66c2ff';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArtePnl.StatusArtePnlInf === 'PENDING') {
							options.font.bold = true;
							options.backgroundColor = '#ec5f5f';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArtePnl.StatusArtePnlInf === 'APPROVED') {
							options.font.bold = true;
							options.backgroundColor = '#ec8a47';
							options.font.color = '#000000';
						} else {
							options.font.bold = true;
							options.backgroundColor = '#ec5f5f';
							options.font.color = '#000000';
						}
					}
                    if (options.gridCell.column.dataField === 'PO') {
                        if (options.gridCell.data.RestaPrintshop <= 10) {
                            options.font.bold = true;
                            options.backgroundColor = '#5F9DCD';
                            options.font.color = '#000000';

                        } else {
                            options.font.bold = true;
                            options.font.color = '#101010';
                        }

                    }
                    if (options.gridCell.column.dataField === 'TotalRestante') {

                        if (options.gridCell.data.TotalRestante === 0) {
                            options.font.bold = true;
                            options.font.color = '#101010';
                        } else if (options.gridCell.data.TotalRestante !== options.gridCell.data.InfoSummary.TotalEstilo) {
                            options.font.bold = true;
                            options.font.color = '#F20101';
                        }
                    }

                    if (options.gridCell.column.dataField === 'InfoSummary.ItemDesc.Descripcion') {

                        if (options.gridCell.data.DestinoSalida === 2) {
                            options.font.bold = true;
                            options.font.color = '#000000';
                            options.backgroundColor = '#beaef1';
                        } else {
                            options.font.bold = true;
                            options.font.color = '#101010';
                        }
                    }

                    if (options.gridCell.column.dataField === 'Trims_fecha_recibo') {

                        if (options.gridCell.data.Trims.restante <= 0 && options.gridCell.data.Trims.estado === "1") {
                            options.font.bold = true;
                            options.font.color = '#101010';
                            options.backgroundColor = '#f5d543';
                        } else {
                            options.font.bold = true;
                            options.font.color = '#101010';
                        }
                    }

                    if (options.gridCell.column.dataField === 'CatTipoBrand.TipoBrandName') {
                        options.font.bold = true;
                    }
                    if (options.gridCell.column.dataField === 'CatTipoOrden.TipoOrden') {
                        options.font.bold = true;
                    }
                    if (options.gridCell.column.dataField === 'POSummary.ItemDescripcion.Descripcion') {
                        options.font.bold = true;
                    }
                    if (options.gridCell.column) {
                        if (options.gridCell.data.FechaRecOrden === output) {
                            options.font.color = '#101010';
                            options.backgroundColor = '#f5d543';
                            //options.rowElement.css('background', '#FEF2E0');
                            //options.rowElement.removeClass("dx-row-alt").addClass("");
                        }

                    }

                    /*if (e.data.FechaRecOrden == output) {
                        e.rowElement.css('background', '#FEF2E0');
                        e.rowElement.removeClass("dx-row-alt").addClass("");
                    }*/
                }
            }
        },
        onExporting: function (e) {
            e.component.beginUpdate();
            e.component.columnOption("TipoPartial", "visible", true);
        },
        onExported: function (e) {
            e.component.columnOption("TipoPartial", "visible", false);
            e.component.endUpdate();
        },
        groupPanel: {
            visible: true
        },
        editing: {
            mode: "batch",
            allowUpdating: true
        },
        allowColumnReordering: true,
        allowColumnResizing: true,
        columnAutoWidth: true,
        showBorders: true,
        showColumnLines: true,
        showRowLines: true,
        rowAlternationEnabled: true,
        scrolling: {
            columnRenderingMode: "virtual"
        },
        remoteOperations: false,
        paging: {
            pageSize: 10
        },
        searchPanel: {
            visible: true,
            width: 240,
            placeholder: "Search..."
        },
        filterRow: {
            visible: true
        },
        headerFilter: {
            visible: true
        },
        loadPanel: {
            enabled: true,
            shadingColor: "rgba(255,255,255,0.4)",
            position: { of: "#gridContainer" },
            visible: false,
            showIndicator: true,
            showPane: true,
            shading: true
        },
        onRowPrepared: function (e) {
            if (e.rowType === 'data' && e.data.FechaRecOrden === output) {
                e.rowElement.css('background', '#FEF2E0');
                e.rowElement.removeClass("dx-row-alt").addClass("");
            }
        },
        onCellHoverChanged: function (e) {
            if (e.column.dataField === "TotalRestante") {
                if (e.rowType === 'data') {
                    if (e.key.TotalRestante !== 0) {
                        popup.option("contentTemplate",
                            function (contentElement) {
                                IdSum = e.key.IdSummaryOrden;
                                IdPed = e.key.IdPedido;
                                IdEst = e.key.IdEstilo;
                                var tabla = TablaRecibos(IdSum, IdPed, IdEst);
                                var gridPop = $("<div/>")
                                    .append(gridPop)
                                    .append('<div>' + e.key.InfoSummary.ItemDesc.ItemEstilo + '</div>')
                                    .append('<div>').addClass('table-wrapper-scroll-y my-custom-scrollbar')
                                    .append('<table id="dtHorizontalVerticalExample" cellspacing="0" width = "100%">').addClass('table table-sm table-striped table-hover')
                                    .append('<thead class="encabezado"></thead>')
                                    .append('<tbody class="tbodys"></tbody>')
                                    .append('</table>')
                                    .appendTo(contentElement);

                            });
                        popup.option("target", e.cellElement);
                        popup.show();
                    } else {
                        popup.hide();
                    }
                }
                popupSuc.hide();
                popupTrims.hide();
                popupPriceTrims.hide();
            }
            if (e.column.dataField === "Trims.fecha_recibo") {
                if (e.rowType === 'data') {
                    if (e.key.Trims.fecha_recibo !== null) {
                        popupTrims.option("contentTemplate",
                            function (contentElement) {
                                IdSum = e.key.IdSummaryOrden;
                                IdPed = e.key.IdPedido;
                                IdEst = e.key.IdEstilo;
                                var tablaTrims = TablaTrims(IdSum,IdPed);
                                var gridPopTrims = $("<div/>")
                                    .append(gridPopTrims)
                                    .append('<div>' + e.key.InfoSummary.ItemDesc.ItemEstilo + '</div>')
                                    .append('<div').addClass('table-wrapper-scroll-y my-custom-scrollbar')
                                    .append('<table id="tableGeneralRecibo">').addClass('table table-sm table-striped table-hover')
                                    .append('<thead class="encabezado"></thead>')
                                    .append('<tbody class="tbodyTrims"></tbody>')
                                    .append('</table>')
                                    .appendTo(contentElement);

                            });
                        popupTrims.option("target", e.cellElement);
                        popupTrims.show();
                    } else {
                        popupTrims.hide();


                    }
                }
                popup.hide();
                popupPriceTrims.hide();
                popupSuc.hide();
            }
            if (e.column.dataField === "InfoPriceTickets.Fecha_recibo") {
                if (e.rowType === 'data') {
                    if (e.key.Trims.fecha_recibo !== null) {
                        popupPriceTrims.option("contentTemplate",
                            function (contentElement) {
                                IdSum = e.key.IdSummaryOrden;
                                IdPed = e.key.IdPedido;
                                IdEst = e.key.IdEstilo;
                                var tablaPriceTrims = TablaPriceTicketsTrims(IdPed);
                                var gridPopPriceTrims = $("<div/>")
                                    .append(gridPopPriceTrims)
                                    .append('<div>' + e.key.InfoSummary.ItemDesc.ItemEstilo + '</div>')
                                    .append('<div').addClass('table-wrapper-scroll-y my-custom-scrollbar')
                                    .append('<table>').addClass('table table-sm table-striped table-hover')
                                    .append('<thead class="encabezado"></thead>')
                                    .append('<tbody class="tbodyPTrims"></tbody>')
                                    .append('</table>')
                                    .appendTo(contentElement);

                            });
                        popupPriceTrims.option("target", e.cellElement);
                        popupPriceTrims.show();
                    } else {
                        popupPriceTrims.hide();
                    }
                }
                popupTrims.hide();
                popupSuc.hide();
                popup.hide();
			}
			if (e.column.dataField === "InfoSummary.ItemDesc.Descripcion") {
				if (e.rowType === 'data') {
					if (e.key.InfoSummary.ItemDesc.Descripcion !== null) {
						popupSuc.option("contentTemplate",
							function (contentElement) {
								IdSum = e.key.IdSummaryOrden;
								IdPed = e.key.IdPedido;
								IdEst = e.key.IdEstilo;
								var valor;
								if (e.key.InfoSummary.IdSucursal === 2) {
									valor = 2;
								} else {
									valor = 1;
								}

								var datos = $('<div>')
									.dxRadioGroup({
										dataSource: [{
											id: 1,
											text: 'FORTUNE'
										}, {
											id: 2,
											text: 'LUCKY1'
										}],
										valueExpr: "id",
										displayExpr: "text",
										onValueChanged: function (dato) {
											//var d = $.Deferred();											
											var previousValue = dato.previousValue;
											var newSucursal = dato.value;
											//var text = radioGroup1.Properties.Items[radioGroup1.SelectedIndex].Description;
											ActualizarSucursalEstilo(/*d,*/ newSucursal, IdSum);
											if (newSucursal === 2) {
												e.cellElement.css("background-color", "#beaef1");
												e.cellElement.css("color", "white");
											} else {
												e.cellElement.css("background-color", "");
												e.cellElement.css("color", "black");
											}
											//	return d.promise();
										},
										value: valor,
										layout: "horizontal"
									});
								var gridPopSucursal = $("<div/>")
									.append(gridPopSucursal)
									.append('<div>' + e.key.InfoSummary.ItemDesc.Descripcion + '</div>')
									.append('<br/>')
									.append(datos)
									.appendTo(contentElement);
							});
						popupSuc.option("target", e.cellElement);
						popupSuc.show();
					} else {
						popupSuc.hide();
					}
                }
                popupTrims.hide();
                popup.hide();
                popupPriceTrims.hide();
			}

        },
        columns: [
            {
                caption: "#",
                dataField: "IdSummaryOrden",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">CUSTOMER</b>')
            },
            {
                caption: "CUSTOMER",
                dataField: "CatCliente.Nombre",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">CUSTOMER</b>')
            }, {
                caption: "RETAILER",
                dataField: "CatClienteFinal.NombreCliente",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">RETAILER</b>')
            }, {
                caption: "PO RECVD DATE",
                dataField: "FechaRecOrden",
                alignment: "center",
                dataType: "date",
                format: "dd/MM/yyyy",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">PO RECVD DATE</b>')

            }, {
                caption: "PO NO",
                dataField: "PO",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">PO NO</b>'),
                cellTemplate: function (element, info) {
                    if (info.data.RestaPrintshop <= 10) {
                        element.append('<div>' + info.text + '</div>')
                            .css("background-color", "#5F9DCD");
                        element.append('<div></div>')
                            .css("color", "white");

                    } else {
                        element.append('<div>' + info.text + '</div>')
                            .css("color", "black");
                    }
                }
            }, {
                caption: "BRAND NAME",
                dataField: "CatTipoBrand.TipoBrandName",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">BRAND NAME</b>')
            }, {
                caption: "AMT PO",
                dataField: "VPO",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">AMT PO</b>')
            }, {
                caption: "REG/BULK",
                dataField: "CatTipoOrden.TipoOrden",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">REG/BULK</b>')
            }, {
                caption: "BALANCE QTY",
                dataField: "Shipped.Cantidad",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">BALANCE QTY</b>')
            }, {
                caption: "EXPECTED SHIP DATE",
                dataField: "FechaOrdenFinal",
                alignment: "center",
                dataType: "date",
                format: "dd/MM/yyyy",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">EXPECTED SHIP DATE</b>')
            }, {
                caption: "ORIGINAL CUST DUE DATE",
                dataField: "FechaCancelada",
                alignment: "center",
                dataType: "date",
                format: "dd/MM/yyyy",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">ORIGINAL CUST DUE DATE</b>')

            }, {
                caption: "DESIGN NAME",
                dataField: "InfoSummary.ItemDesc.Descripcion",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">DESIGN NAME</b>'),
                cellTemplate: function (element, info) {
					if (info.data.InfoSummary.IdSucursal === 2) {
						element.append('<div>' + info.text + '</div>')
							.css("background-color", "#beaef1");
						element.append('<div></div>')
							.css("color", "white");
					} else {
						element.append('<div>' + info.text + '</div>')
							.css("color", "black");
					}	
                }

            }, {
                caption: "STYLE",
                dataField: "InfoSummary.ItemDesc.ItemEstilo",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">STYLE</b>')
            }, {
                caption: "MILL PO",
                dataField: "MillPO",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">MILL PO</b>')
            }, {
                caption: "COLOR",
                dataField: "InfoSummary.CatColores.DescripcionColor",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">COLOR</b>')
            }, {
                caption: "GENDER",
                dataField: "InfoSummary.CatGenero.Genero",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">GENDER</b>')
            }, {
                caption: "BLANKS RECEIVED",
                dataField: "TotalRestante",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">BLANKS RECEIVED</b>'),
                cellTemplate: function (container, options) {
                      if (options.data.TotalRestante === 0) {
                        options.text = "";
                        container.append("<div>" + options.text + "</div>")
                            .css("color", "black");
                    } else if (options.data.TotalRestante !== options.data.InfoSummary.TotalEstilo) {
                        container.append("<div>" + options.text + "</div>")
                            .css("color", "red");
                    }
                }
            }, {
                caption: "PARTIAL/COMPLETE BLANKS",
                dataField: "TipoPartial",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                visible: false,
                headerCellTemplate: $('<b style="color: gray">PARTIAL/COMPLETE BLANKS</b>'),
                cellTemplate: function (element, info) {
                    if (info.text === 'PARTIAL') {
                        element.append('<div>' + info.text + '</div>')
                            .css("background-color", "#F9881D");
                        element.append('<div></div>')
                            .css("color", "white");

                    } else if (info.text === 'COMPLETE') {
                        element.append("<div>" + info.text + "</div>")
                            .css("background-color", "#40bf80");
                        element.append("<div></div>")
                            .css("color", "white");
                    }
                }
            }, {
                caption: "ART RECEIVED",
                dataField: "ImagenArte.StatusArteInf",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">ART RECEIVED</b>'),
                cellTemplate: function (element, info) {
                    if (info.text === 'IN HOUSE') {
                        element.append("<div>" + info.text + "</div>")
                            .css("background-color", "#44c174");
                        element.append("<div></div>")
                            .css("color", "white");
                        if (info.data.ImagenArte.extensionArte === "") {
                            info.data.ImagenArte.extensionArte = "/Content/img/noImagen.png";
                        }
                        element.append($('<img />').attr('src', '/Arte/ConvertirImagenArte?extensionArte=' + info.data.ImagenArte.extensionArte).on('click', function (event) {
                            $('.enlargeImageModalSource').attr('src', $(this).attr('src'));
                            $('#enlargeImageModal').modal('show');
                        }));

                    } else if (info.text === 'REVIEWED') {                     
                        element.append("<div>" + info.text + "</div>")
                            .css("background-color", "#66c2ff");
                        element.append("<div></div>")
                            .css("color", "white");
                    } else if (info.text === 'PENDING') {
                        element.append("<div>" + info.text + "</div>")
                            .css("background-color", "#ec5f5f");
                        element.append("<div></div>")
                            .css("color", "white");
                    } else if (info.text === 'APPROVED') {
                        var infoArte = info.text + "-" + info.data.ImagenArte.FechaArte;
                        element.append("<div>" + infoArte + "</div>")
                            .css("background-color", "#de5a00");
                        element.append("<div></div>")
                            .css("color", "white");

                        element.append($('<img />').attr('src', '/Arte/ConvertirImagenArte?extensionArte=' + info.data.ImagenArte.extensionArte).on('click', function (event) {
                            $('.enlargeImageModalSource').attr('src', $(this).attr('src'));
                            $('#enlargeImageModal').modal('show');
                        }));

                    }
                }
			}, {
				caption: "PNL RECEIVED",
				dataField: "ImagenArtePnl.StatusArtePnlInf",
				alignment: "center",
				allowEditing: false,
				cssClass: "myClass",
				headerCellTemplate: $('<b style="color: gray">PNL RECEIVED</b>'),
				cellTemplate: function (element, info) {
					if (info.text === 'IN HOUSE') {
						element.append("<div>" + info.text + "</div>")
							.css("background-color", "#44c174");
						element.append("<div></div>")
							.css("color", "white");
						if (info.data.ImagenArtePnl.ExtensionPNL === "") {
							info.data.ImagenArtePnl.ExtensionPNL = "/Content/img/noImagen.png";
						}
                        element.append($('<img/>').attr('src', '/Arte/ConvertirImagenArtePNL?extensionPnl=' + info.data.ImagenArtePnl.extensionPNL).on('click', function (event) {
                            $('.enlargeImageModalSource').attr('src', $(this).attr('src'));
                            $('#enlargeImageModal').modal('show');
                        }));

					} else if (info.text === 'REVIEWED') {
						element.append("<div>" + info.text + "</div>")
                            .css("background-color", "#66c2ff");
						element.append("<div></div>")
							.css("color", "white");
					} else if (info.text === 'PENDING') {
						element.append("<div>" + info.text + "</div>")
							.css("background-color", "#ec5f5f");
						element.append("<div></div>")
							.css("color", "white");
					} else if (info.text === 'APPROVED') {
						var infoArte = info.text + "-" + info.data.ImagenArtePnl.FechaArtePnl;
						element.append("<div>" + infoArte + "</div>")
							.css("background-color", "#ec8a47");
						element.append("<div></div>")
                            .css("color", "white");
                        element.append($('<img/>').attr('src', '/Arte/ConvertirImagenArtePNL?extensionPnl=' + info.data.ImagenArtePnl.extensionPNL).on('click', function (event) {
                            $('.enlargeImageModalSource').attr('src', $(this).attr('src'));
                            $('#enlargeImageModal').modal('show');
                        }));
					}
				}
			}, {
                caption: "TRIIM RECEIVED",
                dataField: "Trims.fecha_recibo",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                dataType: "date",
                format: "dd/MM/yyyy",
                headerCellTemplate: $('<b style="color: gray">TRIIM RECEIVED</b>'),
                cellTemplate: function (container, options) {
                    if (options.data.Trims.restante <= 0 && options.data.Trims.estado === "1") {
                        container.append("<div>" + options.text + "</div>")
                            .css("background-color", "#44c174");
                        container.append("<div></div>")
                            .css("color", "white");
                    }
                    else if (options.data.Trims.restante >= 1 && options.data.Trims.estado === "1") {
                        container.append("<div >" + options.text + "</div>")
                            .css("background-color", "#f5d543");
                        container.append("<div></div>")
                            .css("color", "white");
                    } else {
                        container.append("<div>" + options.text + "</div>")
                            .css("color", "black");
                    }
                }
            }, {
                caption: "PACK INST. RCVD",
                dataField: "InfoPackInstruction.Fecha_Pack",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                dataType: "date",
                format: "dd/MM/yyyy",
                headerCellTemplate: $('<b style="color: gray">PACK INST. RCVD</b>'),
                cellTemplate: function (container, options) {
                    if (options.data.InfoPackInstruction.Fecha_Pack !== "" && options.data.InfoPackInstruction.EstadoPack === 1) {
                        container.append("<div>" + options.text + "</div>")
                            .css("background-color", "#44c174");
                        container.append("<div></div>")
                            .css("color", "white");
                    }
                    else {
                        container.append("<div>" + options.text + "</div>")
                            .css("color", "black");
                    }
                }
            }, {
                caption: "PRICE TICKET RECEIVED",
                dataField: "InfoPriceTickets.Fecha_recibo",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                dataType: "date",
                format: "dd/MM/yyyy",
                headerCellTemplate: $('<b style="color: gray">PRICE TICKET RECEIVED</b>'),
                cellTemplate: function (container, options) {
                    if (options.data.InfoPriceTickets.Restante <= 0 && options.data.InfoPriceTickets.Estado === "1") {
                        container.append("<div>" + options.text + "</div>")
                            .css("background-color", "#44c174");
                        container.append("<div></div>")
                            .css("color", "white");
                    }
                    else if (options.data.InfoPriceTickets.Restante >= 1 && options.data.InfoPriceTickets.Estado === "1") {
                        container.append("<div >" + options.text + "</div>")
                            .css("background-color", "#f5d543");
                        container.append("<div></div>")
                            .css("color", "white");
                    } else {
                        // options.text = " ";
                        container.append("<div>" + options.text + "</div>")
                            .css("color", "black");
                        //.css("background-color", "#f5d543"); amarillo
                    }
                }
            }, {
                caption: "UCC RECEIVED",
                dataField: "InfoSummary.FechaUCC",
                alignment: "center",
                cssClass: "myClass",
                dataType: "date",
                format: "dd/MM/yyyy",
                headerCellTemplate: $('<b style="color: gray">UCC RECEIVED</b>'),
                cellTemplate: function (container, options) {
                    if (options.text !== "") {
                        container.append("<div>" + options.text + "</div>")
                            .css("background-color", "#44c174");
                        container.append("<div></div>")
                            .css("color", "white");
                    } else {
                        options.text = " ";
                        container.append("<div>" + options.text + "</div>")
                            .css("color", "black");
                    }
                }
            }, {
                caption: "COMMENTS UPDATE",
                dataField: "CatComentarios.FechaComents",
                alignment: "center",
                dataType: "date",
                format: "dd/MM/yyyy",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">COMMENTS UPDATE</b>')
            }, {
                caption: "COMMENTS",
                dataField: "CatComentarios.Comentario",
                alignment: "center",
                id: "COMENTARIO",
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">COMMENTS</b>')
            }, {
                caption: "ID COMMENT",
                dataField: "CatComentarios.IdComentario",
                visible: false,
                allowEditing: false,
                id: "ID"
            }
        ],
        onCellPrepared: function (e) {
            /*if (e.rowType === "data" && $.inArray(e.rowIndex + ":" + e.columnIndex, editCells) >= 0) {
                e.cellElement.css("background-color", "lightblue");
            }*/
        },
        onEditorPreparing: function (e) {
            var gridShip = e.component;
            if (e.parentType === "dataRow") {
                var oldOnValueChanged = e.editorOptions.onValueChanged;
                e.editorOptions.onValueChanged = function (e) {
                    oldOnValueChanged.apply(this, arguments);
                    for (var i = 0; i < editCells.length; i++) {
                        var rowIndex = Number(editCells[i].split(":")[0]);
                        var columnIndex = Number(editCells[i].split(":")[1]);
                        gridShip.cellValue(rowIndex, columnIndex, e.value);
                    }
                }
            }
        },
        onCellClick: function (e) {
            if (e.jQueryEvent.ctrlKey) {
                editCells.push(e.rowIndex + ":" + e.columnIndex);
            }
            else if (editCells.length) {
                editCells = [];
                e.component.repaint();
            }
        },
        onContentReady: function (e) {
            e.element.find(".dx-datagrid-save-button").click(function (e) {
                if (editCells.length)
                    editCells = [];
            });
            e.element.find(".dx-datagrid-cancel-button ").click(function (e) {
                if (editCells.length)
                    editCells = [];
            });
        },
        masterDetail: {
            enabled: true,
            template: function (container, options) {
                var currentEmployeeData = options.data;
                $("<div>")
                    .addClass("master-detail-caption")
                    .text("COMMENTS LIST " + "PO- " + currentEmployeeData.PO)
                    .appendTo(container);

                $("<div>")
                    .dxDataGrid({
                        showBorders: true,
                        columns: [{
                            caption: "COMMENT",
                            dataField: "Comentario",
                            width: 350
                        },
                        {
                            caption: "DATE COMMENT",
                            dataField: "FechaComents",
                            dataType: "date",
                            format: "dd/MM/yyyy",
                            width: 150
                        },
                        {
                            caption: "USER",
                            dataField: "NombreUsuario",
                            width: 350
                        }
                        ],
                        dataSource: new DevExpress.data.DataSource({
                            key: "IdSummaryOrden",
                            store: commentsShipped,
                            filter: ["IdSummary", "=", options.key.IdSummaryOrden]
                        })
                    }).appendTo(container);
            }
        }
    }).dxDataGrid("instance");

    return gridShip;
}

function GridCancelled(ordersCancelled, output, commentsCancel) {
	var popup = $("#popup").dxPopup({
		title: "BLANKS",
		width: 500,
		height: 300
	}).dxPopup("instance");

	var popupSuc = $("#popupSuc").dxPopup({
		title: "FACTORY",
		width: 300,
		height: 150
	}).dxPopup("instance");

	var popupTrims = $("#popupTrims").dxPopup({
		title: "TRIMS",
		width: 600,
		height: 300
	}).dxPopup("instance");

	var popupPriceTrims = $("#popupPriceTrims").dxPopup({
		title: "PRICE TICKET",
		width: 600,
		height: 300
	}).dxPopup("instance");

    var gridCancel = $("#gridContainer").dxDataGrid({
        onInitialized: function (e) {
            gridCancel = e.component;
        },
        dataSource: {
            store: ordersCancelled
        },
        keyExpr: "IdSummaryOrden",
        selection: {
            mode: "single"
        },
        export: {
            enabled: true,
            fileName: "CANCELLED",
            //allowExportSelectedData: true,
            excelFilterEnabled: true,
            customizeExcelCell: options => {
                if (options.gridCell.rowType === 'header') {
                    options.backgroundColor = '#000000';
                    options.font.color = '#ffffff';
                    options.font.bold = true;
                }
                if (options.gridCell.rowType === 'data') {
					if (options.gridCell.column.dataField === 'ImagenArte.StatusArteInf') {
						if (options.gridCell.data.ImagenArte.StatusArteInf === 'IN HOUSE') {
							options.font.bold = true;
							options.backgroundColor = '#44c174';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArte.StatusArteInf === 'REVIEWED') {
							options.font.bold = true;
							options.backgroundColor = '#66c2ff';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArte.StatusArteInf === 'PENDING') {
							options.font.bold = true;
							options.backgroundColor = '#ec5f5f';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArte.StatusArteInf === 'APPROVED') {
							options.font.bold = true;
							options.backgroundColor = '#ec8a47';
							options.font.color = '#000000';
						} else {
							options.font.bold = true;
							options.backgroundColor = '#ec5f5f';
							options.font.color = '#000000';
						}
					}
					if (options.gridCell.column.dataField === 'ImagenArtePnl.StatusArtePnlInf') {
						if (options.gridCell.data.ImagenArtePnl.StatusArtePnlInf === 'IN HOUSE') {
							options.font.bold = true;
							options.backgroundColor = '#44c174';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArtePnl.StatusArtePnlInf === 'REVIEWED') {
							options.font.bold = true;
							options.backgroundColor = '#66c2ff';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArtePnl.StatusArtePnlInf === 'PENDING') {
							options.font.bold = true;
							options.backgroundColor = '#ec5f5f';
							options.font.color = '#000000';
						} else if (options.gridCell.data.ImagenArtePnl.StatusArtePnlInf === 'APPROVED') {
							options.font.bold = true;
							options.backgroundColor = '#ec8a47';
							options.font.color = '#000000';
						} else {
							options.font.bold = true;
							options.backgroundColor = '#ec5f5f';
							options.font.color = '#000000';
						}
					}
                    if (options.gridCell.column.dataField === 'PO') {
                        if (options.gridCell.data.RestaPrintshop <= 10) {
                            options.font.bold = true;
                            options.backgroundColor = '#5F9DCD';
                            options.font.color = '#000000';

                        } else {
                            options.font.bold = true;
                            options.font.color = '#101010';
                        }

                    }
                    if (options.gridCell.column.dataField === 'TotalRestante') {

                        if (options.gridCell.data.TotalRestante === 0) {
                            options.font.bold = true;
                            options.font.color = '#101010';
                        } else if (options.gridCell.data.TotalRestante !== options.gridCell.data.InfoSummary.TotalEstilo) {
                            options.font.bold = true;
                            options.font.color = '#F20101';
                        }
                    }

                    if (options.gridCell.column.dataField === 'InfoSummary.ItemDesc.Descripcion') {

                        if (options.gridCell.data.DestinoSalida === 2) {
                            options.font.bold = true;
                            options.font.color = '#000000';
                            options.backgroundColor = '#beaef1';
                        } else {
                            options.font.bold = true;
                            options.font.color = '#101010';
                        }
                    }

                    if (options.gridCell.column.dataField === 'Trims_fecha_recibo') {

                        if (options.gridCell.data.Trims.restante <= 0 && options.gridCell.data.Trims.estado === "1") {
                            options.font.bold = true;
                            options.font.color = '#101010';
                            options.backgroundColor = '#f5d543';
                        } else {
                            options.font.bold = true;
                            options.font.color = '#101010';
                        }
                    }

                    if (options.gridCell.column.dataField === 'CatTipoBrand.TipoBrandName') {
                        options.font.bold = true;
                    }
                    if (options.gridCell.column.dataField === 'CatTipoOrden.TipoOrden') {
                        options.font.bold = true;
                    }
                    if (options.gridCell.column.dataField === 'POSummary.ItemDescripcion.Descripcion') {
                        options.font.bold = true;
                    }
                    if (options.gridCell.column) {
                        if (options.gridCell.data.FechaRecOrden === output) {
                            options.font.color = '#101010';
                            options.backgroundColor = '#f5d543';
                            //options.rowElement.css('background', '#FEF2E0');
                            //options.rowElement.removeClass("dx-row-alt").addClass("");
                        }

                    }

                    /*if (e.data.FechaRecOrden == output) {
                        e.rowElement.css('background', '#FEF2E0');
                        e.rowElement.removeClass("dx-row-alt").addClass("");
                    }*/
                }
            }
        },
        onExporting: function (e) {
            e.component.beginUpdate();
            e.component.columnOption("TipoPartial", "visible", true);
        },
        onExported: function (e) {
            e.component.columnOption("TipoPartial", "visible", false);
            e.component.endUpdate();
        },
        groupPanel: {
            visible: true
        },
        editing: {
            mode: "batch",
            allowUpdating: true
        },
        allowColumnReordering: true,
        allowColumnResizing: true,
        columnAutoWidth: true,
        showBorders: true,
        showColumnLines: true,
        showRowLines: true,
        rowAlternationEnabled: true,
        scrolling: {
            columnRenderingMode: "virtual"
        },
        remoteOperations: false,
        paging: {
            pageSize: 10
        },
        searchPanel: {
            visible: true,
            width: 240,
            placeholder: "Search..."
        },
        filterRow: {
            visible: true
        },
        headerFilter: {
            visible: true
        },
        loadPanel: {
            enabled: true,
            shadingColor: "rgba(255,255,255,0.4)",
            position: { of: "#gridContainer" },
            visible: false,
            showIndicator: true,
            showPane: true,
            shading: true
        },
        onRowPrepared: function (e) {
            if (e.rowType === 'data' && e.data.FechaRecOrden === output) {
                e.rowElement.css('background', '#FEF2E0');
                e.rowElement.removeClass("dx-row-alt").addClass("");
            }
        },
        onCellHoverChanged: function (e) {
            if (e.column.dataField === "TotalRestante") {
                if (e.rowType === 'data') {
                    if (e.key.TotalRestante !== 0) {
                        popup.option("contentTemplate",
                            function (contentElement) {
                                IdSum = e.key.IdSummaryOrden;
                                IdPed = e.key.IdPedido;
                                IdEst = e.key.IdEstilo;
                                var tabla = TablaRecibos(IdSum, IdPed, IdEst);
                                var gridPop = $("<div/>")
                                    .append(gridPop)
                                    .append('<div>' + e.key.InfoSummary.ItemDesc.ItemEstilo + '</div>')
                                    .append('<div>').addClass('table-wrapper-scroll-y my-custom-scrollbar')
                                    .append('<table id="dtHorizontalVerticalExample" cellspacing="0" width = "100%">').addClass('table table-sm table-striped table-hover')
                                    .append('<thead class="encabezado"></thead>')
                                    .append('<tbody class="tbodys"></tbody>')
                                    .append('</table>')
                                    .appendTo(contentElement);

                            });
                        popup.option("target", e.cellElement);
                        popup.show();
                    } else {
                        popup.hide();
                    }
                }
                popupPriceTrims.hide();
                popupSuc.hide();
                popupTrims.hide();
            }
            if (e.column.dataField === "Trims.fecha_recibo") {
                if (e.rowType === 'data') {
                    if (e.key.Trims.fecha_recibo !== null) {
                        popupTrims.option("contentTemplate",
                            function (contentElement) {
                                IdSum = e.key.IdSummaryOrden;
                                IdPed = e.key.IdPedido;
                                IdEst = e.key.IdEstilo;
                                var tablaTrims = TablaTrims(IdSum,IdPed);
                                var gridPopTrims = $("<div/>")
                                    .append(gridPopTrims)
                                    .append('<div>' + e.key.InfoSummary.ItemDesc.ItemEstilo + '</div>')
                                    .append('<div').addClass('table-wrapper-scroll-y my-custom-scrollbar')
                                    .append('<table id="tableGeneralRecibo">').addClass('table table-sm table-striped table-hover')
                                    .append('<thead class="encabezado"></thead>')
                                    .append('<tbody class="tbodyTrims"></tbody>')
                                    .append('</table>')
                                    .appendTo(contentElement);

                            });
                        popupTrims.option("target", e.cellElement);
                        popupTrims.show();
                    } else {
                        popupTrims.hide();
                       
                    }
                }
                popup.hide();
                popupPriceTrims.hide();
                popupSuc.hide();
            }
            if (e.column.dataField === "InfoPriceTickets.Fecha_recibo") {
                if (e.rowType === 'data') {
                    if (e.key.Trims.fecha_recibo !== null) {
                        popupPriceTrims.option("contentTemplate",
                            function (contentElement) {
                                IdSum = e.key.IdSummaryOrden;
                                IdPed = e.key.IdPedido;
                                IdEst = e.key.IdEstilo;
                                var tablaPriceTrims = TablaPriceTicketsTrims(IdPed);
                                var gridPopPriceTrims = $("<div/>")
                                    .append(gridPopPriceTrims)
                                    .append('<div>' + e.key.InfoSummary.ItemDesc.ItemEstilo + '</div>')
                                    .append('<div').addClass('table-wrapper-scroll-y my-custom-scrollbar')
                                    .append('<table>').addClass('table table-sm table-striped table-hover')
                                    .append('<thead class="encabezado"></thead>')
                                    .append('<tbody class="tbodyPTrims"></tbody>')
                                    .append('</table>')
                                    .appendTo(contentElement);

                            });
                        popupPriceTrims.option("target", e.cellElement);
                        popupPriceTrims.show();
                    } else {
                        popupPriceTrims.hide();
                    }
                }                
                popupSuc.hide();
                popup.hide();
                popupTrims.hide();
			}
			if (e.column.dataField === "InfoSummary.ItemDesc.Descripcion") {
				if (e.rowType === 'data') {
					if (e.key.InfoSummary.ItemDesc.Descripcion !== null) {
						popupSuc.option("contentTemplate",
							function (contentElement) {
								IdSum = e.key.IdSummaryOrden;
								IdPed = e.key.IdPedido;
								IdEst = e.key.IdEstilo;
								var valor;
								if (e.key.InfoSummary.IdSucursal === 2) {
									valor = 2;
								} else {
									valor = 1;
								}

								var datos = $('<div>')
									.dxRadioGroup({
										dataSource: [{
											id: 1,
											text: 'FORTUNE'
										}, {
											id: 2,
											text: 'LUCKY1'
										}],
										valueExpr: "id",
										displayExpr: "text",
										onValueChanged: function (dato) {
											//var d = $.Deferred();											
											var previousValue = dato.previousValue;
											var newSucursal = dato.value;
											//var text = radioGroup1.Properties.Items[radioGroup1.SelectedIndex].Description;
											ActualizarSucursalEstilo(/*d,*/ newSucursal, IdSum);
											if (newSucursal === 2) {
												e.cellElement.css("background-color", "#beaef1");
												e.cellElement.css("color", "white");
											} else {
												e.cellElement.css("background-color", "");
												e.cellElement.css("color", "black");
											}
											//	return d.promise();
										},
										value: valor,
										layout: "horizontal"
									});
								var gridPopSucursal = $("<div/>")
									.append(gridPopSucursal)
									.append('<div>' + e.key.InfoSummary.ItemDesc.Descripcion + '</div>')
									.append('<br/>')
									.append(datos)
									.appendTo(contentElement);
							});
						popupSuc.option("target", e.cellElement);
						popupSuc.show();
					} else {
						popupSuc.hide();
					}
                }
                popup.hide();
                popupTrims.hide();
                popupPriceTrims.hide();
			}

        },
        columns: [
            {
                caption: "CUSTOMER",
                dataField: "CatCliente.Nombre",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">CUSTOMER</b>')
            }, {
                caption: "RETAILER",
                dataField: "CatClienteFinal.NombreCliente",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">RETAILER</b>')
            }, {
                caption: "PO RECVD DATE",
                dataField: "FechaRecOrden",
                alignment: "center",
                dataType: "date",
                format: "dd/MM/yyyy",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">PO RECVD DATE</b>')
            }, {
                caption: "PO NO",
                dataField: "PO",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">PO NO</b>'),
                cellTemplate: function (element, info) {
                    if (info.data.RestaPrintshop <= 10) {
                        element.append('<div>' + info.text + '</div>')
                            .css("background-color", "#5F9DCD");
                        element.append('<div></div>')
                            .css("color", "white");

                    } else {
                        element.append('<div>' + info.text + '</div>')
                            .css("color", "black");
                    }
                }
            }, {
                caption: "BRAND NAME",
                dataField: "CatTipoBrand.TipoBrandName",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">BRAND NAME</b>')
            }, {
                caption: "AMT PO",
                dataField: "VPO",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">AMT PO</b>')
            }, {
                caption: "REG/BULK",
                dataField: "CatTipoOrden.TipoOrden",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">REG/BULK</b>')
            }, {
                caption: "BALANCE QTY",
                dataField: "InfoSummary.CantidadEstilo",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">BALANCE QTY</b>')
            }, {
                caption: "EXPECTED SHIP DATE",
                dataField: "FechaOrdenFinal",
                alignment: "center",
                dataType: "date",
                format: "dd/MM/yyyy",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">EXPECTED SHIP DATE</b>')
            }, {
                caption: "ORIGINAL CUST DUE DATE",
                dataField: "FechaCancelada",
                alignment: "center",
                dataType: "date",
                format: "dd/MM/yyyy",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">ORIGINAL CUST DUE DATE</b>')

            }, {
                caption: "DESIGN NAME",
                dataField: "InfoSummary.ItemDesc.Descripcion",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">DESIGN NAME</b>'),
                cellTemplate: function (element, info) {
					if (info.data.InfoSummary.IdSucursal === 2) {
						element.append('<div>' + info.text + '</div>')
							.css("background-color", "#beaef1");
						element.append('<div></div>')
							.css("color", "white");
					} else {
						element.append('<div>' + info.text + '</div>')
							.css("color", "black");
					}	
                }

            }, {
                caption: "STYLE",
                dataField: "InfoSummary.ItemDesc.ItemEstilo",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">STYLE</b>')
            }, {
                caption: "MILL PO",
                dataField: "MillPO",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">MILL PO</b>')
            }, {
                caption: "COLOR",
                dataField: "InfoSummary.CatColores.DescripcionColor",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">COLOR</b>')
            }, {
                caption: "GENDER",
                dataField: "InfoSummary.CatGenero.Genero",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">GENDER</b>')
            }, {
                caption: "BLANKS RECEIVED",
                dataField: "TotalRestante",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">BLANKS RECEIVED</b>'),
                cellTemplate: function (container, options) {
                    if (options.data.TotalRestante === 0) {
                        options.text = "";
                        container.append("<div>" + options.text + "</div>")
                            .css("color", "black");
                    } else if (options.data.TotalRestante !== options.data.InfoSummary.TotalEstilo) {
                        container.append("<div>" + options.text + "</div>")
                            .css("color", "red");
                    }
                }
            }, {
                caption: "PARTIAL/COMPLETE BLANKS",
                dataField: "TipoPartial",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                visible: false,
                headerCellTemplate: $('<b style="color: gray">PARTIAL/COMPLETE BLANKS</b>'),
                cellTemplate: function (element, info) {
                    if (info.text === 'PARTIAL') {
                        element.append('<div>' + info.text + '</div>')
                            .css("background-color", "#F9881D");
                        element.append('<div></div>')
                            .css("color", "white");

                    } else if (info.text === 'COMPLETE') {
                        element.append("<div>" + info.text + "</div>")
                            .css("background-color", "#40bf80");
                        element.append("<div></div>")
                            .css("color", "white");
                    }
                }
            }, {
                caption: "ART RECEIVED",
                dataField: "ImagenArte.StatusArteInf",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">ART RECEIVED</b>'),
                cellTemplate: function (element, info) {
                    if (info.text === 'IN HOUSE') {
                        element.append("<div>" + info.text + "</div>")
                            .css("background-color", "#44c174");
                        element.append("<div></div>")
                            .css("color", "white");
                        if (info.data.ImagenArte.extensionArte === "") {
                            info.data.ImagenArte.extensionArte = "/Content/img/noImagen.png";
                        }
                        element.append($('<img/>').attr('src', '/Arte/ConvertirImagenArte?extensionArte=' + info.data.ImagenArte.extensionArte).on('click', function (event) {
                            $('.enlargeImageModalSource').attr('src', $(this).attr('src'));
                            $('#enlargeImageModal').modal('show');
                        }));

                    } else if (info.text === 'REVIEWED') {
                       
                        element.append("<div>" + info.text + "</div>")
                            .css("background-color", "#68D385");
                        element.append("<div></div>")
                            .css("color", "white");
                    } else if (info.text === 'PENDING') {
                        element.append("<div>" + info.text + "</div>")
                            .css("background-color", "#ec5f5f");
                        element.append("<div></div>")
                            .css("color", "white");
                    } else if (info.text === 'APPROVED') {
                        var infoArte = info.text + "-" + info.data.ImagenArte.FechaArte;
                        element.append("<div>" + infoArte + "</div>")
                            .css("background-color", "#de5a00");
                        element.append("<div></div>")
                            .css("color", "white");
                        element.append($('<img />').attr('src', '/Arte/ConvertirImagenArte?extensionArte=' + info.data.ImagenArte.extensionArte).on('click', function (event) {
                            $('.enlargeImageModalSource').attr('src', $(this).attr('src'));
                            $('#enlargeImageModal').modal('show');
                        }));
                    }
                }
			}, {
				caption: "PNL RECEIVED",
				dataField: "ImagenArtePnl.StatusArtePnlInf",
				alignment: "center",
				allowEditing: false,
				cssClass: "myClass",
				headerCellTemplate: $('<b style="color: gray">PNL RECEIVED</b>'),
				cellTemplate: function (element, info) {
					if (info.text === 'IN HOUSE') {
						element.append("<div>" + info.text + "</div>")
							.css("background-color", "#44c174");
						element.append("<div></div>")
							.css("color", "white");
						if (info.data.ImagenArtePnl.ExtensionPNL === "") {
							info.data.ImagenArtePnl.ExtensionPNL = "/Content/img/noImagen.png";
						}
                        element.append($('<img/>').attr('src', '/Arte/ConvertirImagenArtePNL?extensionPnl=' + info.data.ImagenArtePnl.extensionPNL).on('click', function (event) {
                            $('.enlargeImageModalSource').attr('src', $(this).attr('src'));
                            $('#enlargeImageModal').modal('show');
                        }));

					} else if (info.text === 'REVIEWED') {
						element.append("<div>" + info.text + "</div>")
							.css("background-color", "#68D385");
						element.append("<div></div>")
							.css("color", "white");
					} else if (info.text === 'PENDING') {
						element.append("<div>" + info.text + "</div>")
							.css("background-color", "#ec5f5f");
						element.append("<div></div>")
							.css("color", "white");
					} else if (info.text === 'APPROVED') {
						var infoArte = info.text + "-" + info.data.ImagenArtePnl.FechaArtePnl;
						element.append("<div>" + infoArte + "</div>")
							.css("background-color", "#ec8a47");
						element.append("<div></div>")
                            .css("color", "white");

                        element.append($('<img/>').attr('src', '/Arte/ConvertirImagenArtePNL?extensionPnl=' + info.data.ImagenArtePnl.extensionPNL).on('click', function (event) {
                            $('.enlargeImageModalSource').attr('src', $(this).attr('src'));
                            $('#enlargeImageModal').modal('show');
                        }));
					}
				}
			}, {
                caption: "TRIIM RECEIVED",
                dataField: "Trims.fecha_recibo",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                dataType: "date",
                format: "dd/MM/yyyy",
                headerCellTemplate: $('<b style="color: gray">TRIIM RECEIVED</b>'),
                cellTemplate: function (container, options) {
                    if (options.data.Trims.restante <= 0 && options.data.Trims.estado === "1") {
                        container.append("<div>" + options.text + "</div>")
                            .css("background-color", "#44c174");
                        container.append("<div></div>")
                            .css("color", "white");
                    }
                    else if (options.data.Trims.restante >= 1 && options.data.Trims.estado === "1") {
                        container.append("<div >" + options.text + "</div>")
                            .css("background-color", "#f5d543");
                        container.append("<div></div>")
                            .css("color", "white");
                    } else {
                        container.append("<div>" + options.text + "</div>")
                            .css("color", "black");
                    }
                }
            }, {
                caption: "PACK INST. RCVD",
                dataField: "InfoPackInstruction.Fecha_Pack",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                dataType: "date",
                format: "dd/MM/yyyy",
                headerCellTemplate: $('<b style="color: gray">PACK INST. RCVD</b>'),
                cellTemplate: function (container, options) {
                    if (options.data.InfoPackInstruction.Fecha_Pack !== "" && options.data.InfoPackInstruction.EstadoPack === 1) {
                        container.append("<div>" + options.text + "</div>")
                            .css("background-color", "#44c174");
                        container.append("<div></div>")
                            .css("color", "white");
                    }
                    else {
                        container.append("<div>" + options.text + "</div>")
                            .css("color", "black");
                    }
                }
            }, {
                caption: "PRICE TICKET RECEIVED",
                dataField: "InfoPriceTickets.Fecha_recibo",
                alignment: "center",
                allowEditing: false,
                cssClass: "myClass",
                dataType: "date",
                format: "dd/MM/yyyy",
                headerCellTemplate: $('<b style="color: gray">PRICE TICKET RECEIVED</b>'),
                cellTemplate: function (container, options) {
                    if (options.data.InfoPriceTickets.Restante <= 0 && options.data.InfoPriceTickets.Estado === "1") {
                        container.append("<div>" + options.text + "</div>")
                            .css("background-color", "#44c174");
                        container.append("<div></div>")
                            .css("color", "white");
                    }
                    else if (options.data.InfoPriceTickets.Restante >= 1 && options.data.InfoPriceTickets.Estado === "1") {
                        container.append("<div >" + options.text + "</div>")
                            .css("background-color", "#f5d543");
                        container.append("<div></div>")
                            .css("color", "white");
                    } else {
                      container.append("<div>" + options.text + "</div>")
                            .css("color", "black");
                    }
                }
            }, {
                caption: "UCC RECEIVED",
                dataField: "InfoSummary.FechaUCC",
                alignment: "center",
                cssClass: "myClass",
                dataType: "date",
                format: "dd/MM/yyyy",
                headerCellTemplate: $('<b style="color: gray">UCC RECEIVED</b>'),
                cellTemplate: function (container, options) {
                    if (options.text !== "") {
                        container.append("<div>" + options.text + "</div>")
                            .css("background-color", "#44c174");
                        container.append("<div></div>")
                            .css("color", "white");
                    } else {
                        options.text = " ";
                        container.append("<div>" + options.text + "</div>")
                            .css("color", "black");
                    }
                }
            }, {
                caption: "COMMENTS UPDATE",
                dataField: "CatComentarios.FechaComents",
                alignment: "center",
                dataType: "date",
                format: "dd/MM/yyyy",
                allowEditing: false,
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">COMMENTS UPDATE</b>')
            }, {
                caption: "COMMENTS",
                dataField: "CatComentarios.Comentario",
                alignment: "center",
                id: "COMENTARIO",
                cssClass: "myClass",
                headerCellTemplate: $('<b style="color: gray">COMMENTS</b>')
            }, {
                caption: "ID COMMENT",
                dataField: "CatComentarios.IdComentario",
                visible: false,
                allowEditing: false,
                id: "ID"
            }
        ],
        onCellPrepared: function (e) {
            /*if (e.rowType === "data" && $.inArray(e.rowIndex + ":" + e.columnIndex, editCells) >= 0) {
                e.cellElement.css("background-color", "lightblue");
            }*/
        },
        onEditorPreparing: function (e) {
            var gridCancel = e.component;
            if (e.parentType === "dataRow") {
                var oldOnValueChanged = e.editorOptions.onValueChanged;
                e.editorOptions.onValueChanged = function (e) {
                    oldOnValueChanged.apply(this, arguments);
                    for (var i = 0; i < editCells.length; i++) {
                        var rowIndex = Number(editCells[i].split(":")[0]);
                        var columnIndex = Number(editCells[i].split(":")[1]);
                        gridCancel.cellValue(rowIndex, columnIndex, e.value);
                    }
                }
            }
        },
        onCellClick: function (e) {
            if (e.jQueryEvent.ctrlKey) {
                editCells.push(e.rowIndex + ":" + e.columnIndex);
            }
            else if (editCells.length) {
                editCells = [];
                e.component.repaint();
            }
        },
        onContentReady: function (e) {
            e.element.find(".dx-datagrid-save-button").click(function (e) {
                if (editCells.length)
                    editCells = [];
            });
            e.element.find(".dx-datagrid-cancel-button ").click(function (e) {
                if (editCells.length)
                    editCells = [];
            });
        },
        masterDetail: {
            enabled: true,
            template: function (container, options) {
                var currentEmployeeData = options.data;
                $("<div>")
                    .addClass("master-detail-caption")
                    .text("COMMENTS LIST " + "PO- " + currentEmployeeData.PO)
                    .appendTo(container);

                $("<div>")
                    .dxDataGrid({
                        showBorders: true,
                        columns: [{
                            caption: "COMMENT",
                            dataField: "Comentario",
                            width: 350
                        },
                        {
                            caption: "DATE COMMENT",
                            dataField: "FechaComents",
                            dataType: "date",
                            format: "dd/MM/yyyy",
                            width: 150
                        },
                        {
                            caption: "USER",
                            dataField: "NombreUsuario",
                            width: 350
                        }
                        ],
                        dataSource: new DevExpress.data.DataSource({
                            key: "IdSummaryOrden",
                            store: commentsCancel,
                            filter: ["IdSummary", "=", options.key.IdSummaryOrden]
                        })
                    }).appendTo(container);
            }
        }
    }).dxDataGrid("instance");

    return gridCancel;
  }

function ActualizarSucursalEstilo(newSucursal, idSummary) {
	var sucursal = newSucursal;
	$.ajax({
		type: 'POST',
		url: "/WIP/ActualizarSucursalIdSummary",
		data: { IdSucursal: newSucursal, IdSummary: parseInt(idSummary) },
		success: function (data) {
			//d.resolve(data);
			
            /*if (!timeOut) {
                timeOut = setTimeout(timerCallback, 100);
            }*/
			//var dataGrid = $('#gridContainer').dxDataGrid('instance');
			//dataGrid.refresh();
		},
		error: function (e) {
			alert("error: " + e.responseText);
		}
	});
}

function ConsultarSucursalEstilo(idSummary) {
    
    $.ajax({
        type: 'GET',
        url: "/WIP/ConsultarSucursalIdSummary",
        data: {IdSummary: parseInt(idSummary) },
        success: function (data) {
            debugger
            var dato;
            //d.resolve(data);

            /*if (!timeOut) {
                timeOut = setTimeout(timerCallback, 100);
            }*/
            //var dataGrid = $('#gridContainer').dxDataGrid('instance');
            //dataGrid.refresh();
        },
        error: function (e) {
            alert("error: " + e.responseText);
        }
    });
}