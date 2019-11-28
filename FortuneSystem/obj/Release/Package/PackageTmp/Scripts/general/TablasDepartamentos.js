
function TablaPacking() {
    var EstiloId = $("#IdSummaryOrden").val();
    var cliente = $("#CatClienteFinal_NombreCliente").val();
    var nombreCliente = $.trim(cliente);
    if (nombreCliente === "HOT TOPIC") {
        TablaPackingHT(EstiloId);
    } else if (nombreCliente === "URBAN OUTFITTERS") {
        TablaPackingHT(EstiloId);
    } else {
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
                var html = '';
                var estilos = jsonData.Data.estilos;
                var tipoEmp = "";
                $.each(listaEmpaque, function (key, item) {
                    tipoEmp = item.NombreTipoPak;
                });
                $("#titulo_Tipo_Empaque").css('display', 'inline');
                $("#titulo_Tipo_Empaque").html("<h1> Packing- " + tipoEmp + "</h1>");
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
                        numTipoPack = 4;
                        var contadorP = 0;
                        var valor = 0;
                        $.each(listaEPPK, function (key, item) {
                            contadorP = contadorP + 1;
                            html += '</tr><tr><td width="30%">PPK - #Ratio-' + item.NombrePacking + '</td>';

                            $.each(item.ListaEmpaque, function (key, i) {
                                html += '<td class="ratioPPKS' + item.NombrePacking + '">' + i.Ratio + '</td>';
                                cantidadesEmp += i.Ratio;
                            });
                            html += '</tr><tr id="empaque" class="empaque"><td width="30%">QTY</td>';
                            $.each(item.ListaEmpaque, function (key, i) {
                                html += '<td class="qtyPPKs' + item.NombrePacking + '">' + i.Cantidad + '</td>';
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
                                        html += '<td class="box' + i.NombrePacking + '">' + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Total Box # " + i.TotalCajas + '</td>';
                                        numBoxPPKS = i;
                                        valor = valor + 1;
                                    } else {
                                        html += '<td class="box' + i.NombrePacking + '">' + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Total Box # " + i.TotalCajas + '</td>';
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

                        });

                    } else if (tipoEmp === "BULKS") {
                        $("#opcSelectPackBULKS").css("display", "inline");
                        numTipoPack = 5;
                        var contadorBulk = 0;
                        $.each(listaEBulk, function (key, item) {
                            contadorBulk = contadorBulk + 1;
                            html += '</tr><tr><td width="30%">BULK-' + item.PackingNameBulk + ' Pieces</td>';

                            $.each(item.ListaEmpaque, function (key, i) {

                                html += '<td class="qtyBulkPcs' + item.PackingNameBulk + '">' + i.Pieces + '</td>';

                            });
                            html += '</tr><tr id="empaque" class="empaque"><td width="30%">Packed</td>';
                            $.each(item.ListaEmpaque, function (key, i) {

                                html += '<td class="qtyBulkTPcs' + item.PackingNameBulk + '">' + i.TotalBulk + '</td>';

                            });
                        });
                    }

                }
                $.each(listaEmpaque, function (key, item) {
                    tipoEmp = item.NombreTipoPak;
                });
                if (tipoEmp === "BULK" || tipoEmp === "PPK") {
                    html += '</tr><tr><td width="30%">Packed</td>';
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
            },
            error: function (errormessage) {
                alert(errormessage.responseText);
            }
        }).done(function (data) {

        });
    }
    $("#ModalPacking").modal('show');
}

function TablaPackingHT(EstiloId) {   
    var datosPO = "";
    $.ajax({
        url: "/Packing/Lista_Tallas_HT_Por_Estilo/" + EstiloId,
        method: 'POST',
        dataType: "json",
        success: function (jsonData) {
            var listaPO = jsonData.Data.lista;
            var listaQty = jsonData.Data.listaT;
            var listaPBulk = jsonData.Data.listaPTBulk;
            var listaEPPK = jsonData.Data.listaEmpPPK;
            var listaEmpBulk = jsonData.Data.listaEmpBulk;
            var listaPPPK = jsonData.Data.listaPTPPK;
            var listaTCajas = jsonData.Data.listaTotalCajasPack;
            var html = '';
            var estilos = jsonData.Data.estilos;
            var cargo = jsonData.Data.cargoUser;
            var lista_estilo_Descrip = jsonData.Data.lista;
            var EstiloDescrip;
            $.each(lista_estilo_Descrip, function (key, item) {

                EstiloDescrip = item.DescripcionEstilo;

            });           
           
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
                html += '</tr><tr id="empaque" class="empaque"><td width="30%">Packed</td>';
                $.each(item.ListaEmpaque, function (key, i) {

                    html += '<td class="qtyPPK">' + i.TotalBulk + '</td>';
                    cantidadesPackedB += i.TotalBulk;

                });
                html += '<td>' + cantidadesPackedB + '</td>';
            });

            $.each(listaEPPK, function (key, item) {
                cont = cont + 1;
                html += '</tr><tr><td width="30%">PPK - #Ratio- PO#' + item.NumberPO + '</td>';

                $.each(item.ListaEmpaque, function (key, i) {
                    html += '<td>' + i.Ratio + '</td>';
                    cantidadesEmpPPK += i.Ratio;
                });
             
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
                html += '<td class="faltante" ></td>';
                i++;
            });     
            html += '</tr>';

            $('.tbodyP').html(html);
            var nColumnas = $("#tablePacking tr:last td").length;
            var totalRows = $("#tablePacking tr").length;
            for (var v = 1; v < lPOTotal + 1; v++) {
                datosPO += "*" + $('#tablePacking tr:eq(2) td:eq(' + v + ')').html();
            }

            var temp = "";
            var arrayCantidades = new Array();

            var mArray = new Array();
            var mArrayResult = new Array();

            for (var z = 0; z < totalRows; z++) {
                var valor = $('#tablePacking tr:eq(' + z + ') td:eq(0)').html();
                var contener = "";
                if (valor !== undefined) {
                    contener = valor.includes('Packed');
                }
                if (contener === true) {
                    for (var j = 1; j < lPOTotal + 1; j++) {
                        temp += "*" + $('#tablePacking tr:eq(' + z + ') td:eq(' + j + ')').html();
                    }
                    arrayCantidades.push(temp);
                    temp = "";
                }
            }
            mArray[arrayCantidades.lenght];
            for (var x = 0; x < arrayCantidades.length; x++) {
                var cantidadesV = arrayCantidades[x].split("*");

                mArray[x] = cantidadesV.map(function (x) {
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
  
            if (numTallas !== 0) {
                var suma = 0;
                var valorFalt = 0;
                var val = parseInt(resultPO.length);
                for (var l = 1; l < mArray[0].length; l++) {
                    suma = 0;
                    for (var r = 0; r < arrayCantidades.length; r++) {
                        suma += mArray[r][l];
                    }
                    mArrayResult[l] = suma;
                }

                mArrayResult[0] = 0;
                for (var m = 1; m < val; m++) {
                    var totalC = mArrayResult[m] - resultPO[m];
                    resultPO[m] = totalC;
                    $('#tablePacking tr:eq(' + (totalRows - 1) + ') td:eq(' + m + ')').html(resultPO[m]);
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
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    }).done(function (data) {

    });
}



function TablaPNLPack() {
    var IdEstilo = $("#IdSummaryOrden").val();
    var tempScrollTop = $(window).scrollTop();

    $.ajax({
        url: "/Pedidos/Lista_Tallas_Estilo_Pnl/" + parseInt(IdEstilo),
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var html = '';
            var lista_estilo = jsonData.Data.listaTalla;
            var cantidadesPOTotal = 0;
            var cadena_cantidadesTotal = "";
            $.each(lista_estilo, function (key, item) {

                cantidadesPOTotal += item.Cantidad;
                cadena_cantidadesTotal += "*" + item.Cantidad;
            });
            var cantidades_arrayTotal = cadena_cantidadesTotal.split('*');
            html += '<tr> <th>  </th>';
            $.each(lista_estilo, function (key, item) {

                html += '<th>' + item.Talla + '</th>';

            });
            html += '<th> Total </th>';
            html += '</tr><tr><td>PNL Quantity</td>';
            var cantidadesPnl = 0;
            var lista_Batch_PNL = jsonData.Data.listaTallasTotalPnlBatch;
            var listaTBatchPnl = 0;
            $.each(lista_Batch_PNL, function (key, item) {
                listaTBatchPnl++;
            });
            if (listaTBatchPnl === 0) {
                lista_Batch_PNL = lista_estilo;
            } else {
                lista_Batch_PNL;
            }
            $.each(lista_Batch_PNL, function (key, item) {
                if (listaTBatchPnl === 0) {
                    item = 0;
                    html += '<td>' + item + '</td>';
                } else {
                    html += '<td>' + item + '</td>';
                }

                cantidadesPnl += item;
            });
            html += '<td>' + cantidadesPnl + '</td>';
            html += '</tr><tr><td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;+ Printed</td>';
            var cantidadesPrinted = 0;
            var lista_Batch_Printed = jsonData.Data.listaTallasTotalPBatchPNL;
            var listaPBatch = 0;
            $.each(lista_Batch_Printed, function (key, item) {
                listaPBatch++;
            });
            if (listaPBatch === 0) {
                lista_Batch_Printed = lista_estilo;
            } else {
                lista_Batch_Printed;
            }
            $.each(lista_Batch_Printed, function (key, item) {
                if (listaPBatch === 0) {
                    item = 0;
                    html += '<td>' + item + '</td>';
                    cantidadesPrinted += item;
                } else {
                    html += '<td>' + item + '</td>';
                    cantidadesPrinted += item;
                }

                // cantidadesPrinted += item;
            });
            html += '<td>' + cantidadesPrinted + '</td>';
            html += '</tr><tr><td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;+ MisPrint</td>';
            var cantidadesMisPrintB = 0;
            var lista_Batch_MP = jsonData.Data.listaTallasTotalMBatchPnl;
            var listaMPBatch = 0;
            $.each(lista_Batch_MP, function (key, item) {
                listaMPBatch++;
            });
            if (listaMPBatch === 0) {
                lista_Batch_MP = lista_estilo;
            } else {
                lista_Batch_MP;
            }
            $.each(lista_Batch_MP, function (key, item) {
                if (listaMPBatch === 0) {
                    item = 0;
                    html += '<td>' + item + '</td>';
                    cantidadesMisPrintB += item;
                } else {
                    html += '<td>' + item + '</td>';
                    cantidadesMisPrintB += item;
                }

                // cantidadesMisPrintB += item;
            });
            html += '<td>' + cantidadesMisPrintB + '</td>';
            html += '</tr><tr><td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;+ Defect</td>';
            var cantidadesDefectB = 0;
            var lista_Batch_Defect = jsonData.Data.listaTallasTotalDBatchPnl;
            var listaDefBatch = 0;
            $.each(lista_Batch_Defect, function (key, item) {
                listaDefBatch++;
            });
            if (listaDefBatch === 0) {
                lista_Batch_Defect = lista_estilo;
            } else {
                lista_Batch_Defect;
            }
            $.each(lista_Batch_Defect, function (key, item) {
                if (listaDefBatch === 0) {
                    item = 0;
                    html += '<td>' + item + '</td>';
                    cantidadesDefectB += item;
                } else {
                    html += '<td>' + item + '</td>';
                    cantidadesDefectB += item;
                }

                // cantidadesDefectB += item;
            });
            html += '<td>' + cantidadesDefectB + '</td>';
            html += '</tr>';
            html += '</tr><tr><td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;+ Repair</td>';
            var cantidadesRepairB = 0;
            var lista_Batch_Repair = jsonData.Data.listaTallasTotalRBatch;
            var listaRepBatch = 0;
            $.each(lista_Batch_Repair, function (key, item) {
                listaRepBatch++;
            });
            if (listaRepBatch === 0) {
                lista_Batch_Repair = lista_estilo;
            } else {
                lista_Batch_Repair;
            }
            $.each(lista_Batch_Repair, function (key, item) {
                if (listaRepBatch === 0) {
                    item = 0;
                    html += '<td>' + item + '</td>';
                    cantidadesRepairB += item;
                } else {
                    html += '<td>' + item + '</td>';
                    cantidadesRepairB += item;
                }

                // cantidadesRepairB += item;
            });
            html += '<td>' + cantidadesRepairB + '</td>';
            html += '</tr>';
            html += '<tr><td>+/-</td>';
            var totales = 0;
            var i = 1;
            var sumaTotal = 0;
            $.each(lista_Batch_PNL, function (key, item) {
                if (listaTBatchPnl === 0) {
                    item = 0;
                }
                var resta = parseFloat(item) - parseFloat(cantidades_arrayTotal[i]);
                if (resta === 0) {
                    html += '<td class="restaPnl" style="color:black;">' + resta + '</td>';
                } else if (resta >= 0) {
                    html += '<td class="restaPnl" style="color:blue;">' + resta + '</td>';
                } else {
                    html += '<td class="restaPnl" style="color:red;">' + resta + '</td>';
                }
                i++;
                sumaTotal += resta;
            });
            html += '<td>' + sumaTotal + '</td>';
            html += '</tr>';

            if (Object.keys(lista_estilo).length === 0) {
                html += '<tr class="odd"><td valign="middle" align="center" colspan="10" class="dataTables_empty">No sizes were found for the style.</td></tr>';
            }
            $('.tbodysPNL').html(html);

            //obtenerIdEstilo(IdEstilo);
            $("#loading").css('display', 'none');
            $(window).scrollTop(tempScrollTop);
        },
        error: function (errormessage) { alert(errormessage.responseText); }
    });
    $("#ModalTablaPNL").modal('show');
}

function TablaPRINTSHOPPack() {
    var tempScrollTop = $(window).scrollTop();
    var IdEstilo = $("#IdSummaryOrden").val();
    $.ajax({
        url: "/Pedidos/Lista_Tallas_Estilo_PrintShop/" + parseInt(IdEstilo),
        //  data: "{'id':'" + IdEstilo + "','numUnits':'" + numUnits + "'}",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (jsonData) {
            var html = '';
            var estilos = jsonData.Data.estilos;
            var EstiloDescription;
            var lista_estilo_Desc = jsonData.Data.listaTalla;
            var lista_Qty_Tallas = jsonData.Data.listTallaCant;
            $.each(lista_estilo_Desc, function (key, item) {

                EstiloDescription = item.DescripcionEstilo;

            });


            var lista_estilo = jsonData.Data.listaTalla;
            listaEstiloPO = lista_estilo;
            html += '<tr> <th>  </th>';
            $.each(lista_estilo, function (key, item) {

                html += '<th>' + item.Talla + '</th>';

            });
            html += '<th> Total </th>';
            html += '</tr><tr><td>Total Orden</td>';
            var cantidadesPOTotal = 0;
            var cadena_cantidadesTotal = "";
            $.each(lista_estilo, function (key, item) {

                html += '<td class="total" >' + item.Cantidad + '</td>';
                cantidadesPOTotal += item.Cantidad;
                cadena_cantidadesTotal += "*" + item.Cantidad;
            });
            var cantidades_arrayTotal = cadena_cantidadesTotal.split('*');
            html += '<td>' + cantidadesPOTotal + '</td>';
            html += '</tr><tr><td>1rst Quantity</td>';
            var cantidadesPO = 0;
            var cadena_cantidades = "";
            $.each(lista_Qty_Tallas, function (key, item) {

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
            html += '</tr><tr><td>PrintShop Quantity</td>';
            var cantidadesPrint = 0;
            var lista_Batch = jsonData.Data.listaTallasTotalBatch;
            var listaTBatch = 0;
            $.each(lista_Batch, function (key, item) {
                listaTBatch++;
            });
            if (listaTBatch === 0) {
                lista_Batch = lista_estilo;
            } else {
                lista_Batch;
            }
            $.each(lista_Batch, function (key, item) {
                if (listaTBatch === 0) {
                    item = 0;
                    html += '<td>' + item + '</td>';
                } else {
                    html += '<td>' + item + '</td>';
                }

                cantidadesPrint += item;
            });
            html += '<td>' + cantidadesPrint + '</td>';
            html += '</tr><tr><td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;+ Printed</td>';
            var cantidadesPrinted = 0;
            var lista_Batch_Printed = jsonData.Data.listaTallasTotalPBatch;
            var listaPBatch = 0;
            $.each(lista_Batch_Printed, function (key, item) {
                listaPBatch++;
            });
            if (listaPBatch === 0) {
                lista_Batch_Printed = lista_estilo;
            } else {
                lista_Batch_Printed;
            }
            $.each(lista_Batch_Printed, function (key, item) {
                if (listaPBatch === 0) {
                    item = 0;
                    html += '<td>' + item + '</td>';
                    cantidadesPrinted += item;
                } else {
                    html += '<td>' + item + '</td>';
                    cantidadesPrinted += item;
                }

                // cantidadesPrinted += item;
            });
            html += '<td>' + cantidadesPrinted + '</td>';
            html += '</tr><tr><td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;+ MisPrint</td>';
            var cantidadesMisPrintB = 0;
            var lista_Batch_MP = jsonData.Data.listaTallasTotalMBatch;
            var listaMPBatch = 0;
            $.each(lista_Batch_MP, function (key, item) {
                listaMPBatch++;
            });
            if (listaMPBatch === 0) {
                lista_Batch_MP = lista_estilo;
            } else {
                lista_Batch_MP;
            }
            $.each(lista_Batch_MP, function (key, item) {
                if (listaMPBatch === 0) {
                    item = 0;
                    html += '<td>' + item + '</td>';
                    cantidadesMisPrintB += item;
                } else {
                    html += '<td>' + item + '</td>';
                    cantidadesMisPrintB += item;
                }

                // cantidadesMisPrintB += item;
            });
            html += '<td>' + cantidadesMisPrintB + '</td>';
            html += '</tr><tr><td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;+ Defect</td>';
            var cantidadesDefectB = 0;
            var lista_Batch_Defect = jsonData.Data.listaTallasTotalDBatch;
            var listaDefBatch = 0;
            $.each(lista_Batch_Defect, function (key, item) {
                listaDefBatch++;
            });
            if (listaDefBatch === 0) {
                lista_Batch_Defect = lista_estilo;
            } else {
                lista_Batch_Defect;
            }
            $.each(lista_Batch_Defect, function (key, item) {
                if (listaDefBatch === 0) {
                    item = 0;
                    html += '<td>' + item + '</td>';
                    cantidadesDefectB += item;
                } else {
                    html += '<td>' + item + '</td>';
                    cantidadesDefectB += item;
                }

                // cantidadesDefectB += item;
            });
            html += '<td>' + cantidadesDefectB + '</td>';
            html += '</tr>';
            html += '</tr><tr><td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;+ Repair</td>';
            var cantidadesRepairB = 0;
            var lista_Batch_Repair = jsonData.Data.listaTallasTotalRBatch;
            var listaRepBatch = 0;
            $.each(lista_Batch_Repair, function (key, item) {
                listaRepBatch++;
            });
            if (listaRepBatch === 0) {
                lista_Batch_Repair = lista_estilo;
            } else {
                lista_Batch_Repair;
            }
            $.each(lista_Batch_Repair, function (key, item) {
                if (listaRepBatch === 0) {
                    item = 0;
                    html += '<td>' + item + '</td>';
                    cantidadesRepairB += item;
                } else {
                    html += '<td>' + item + '</td>';
                    cantidadesRepairB += item;
                }

                // cantidadesRepairB += item;
            });
            html += '<td>' + cantidadesRepairB + '</td>';
            html += '</tr>';
            html += '<tr><td>+/-</td>';
            var totales = 0;
            var i = 1;
            var sumaTotal = 0;
            $.each(lista_Batch, function (key, item) {
                if (listaTBatch === 0) {
                    item = 0;
                }
                var resta = parseFloat(item) - parseFloat(cantidades_arrayTotal[i]);

                if (resta === 0) {
                    html += '<td class="restaPrint" style="color:black;">' + resta + '</td>';
                } else if (resta >= 0) {
                    html += '<td class="restaPrint" style="color:blue;">' + resta + '</td>';
                } else {
                    html += '<td class="restaPrint" style="color:red;">' + resta + '</td>';
                }
                i++;
                sumaTotal += resta;
            });
            html += '<td>' + sumaTotal + '</td>';

            html += '</tr>';

            if (Object.keys(lista_estilo).length === 0) {
                html += '<tr class="odd"><td valign="middle" align="center" colspan="10" class="dataTables_empty">No sizes were found for the style.</td></tr>';
            }
            $('.tbodysPrintShop').html(html);

            $(window).scrollTop(tempScrollTop);
        },
        error: function (errormessage) { alert(errormessage.responseText); }
    });
    $("#ModalTablaPrintS").modal('show');
}