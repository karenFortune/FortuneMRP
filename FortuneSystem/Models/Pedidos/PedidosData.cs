
using FortuneSystem.Models.Almacen;
using FortuneSystem.Models.Arte;
using FortuneSystem.Models.Catalogos;
using FortuneSystem.Models.Items;
using FortuneSystem.Models.POSummary;
using FortuneSystem.Models.PrintShop;
using FortuneSystem.Models.Revisiones;
using FortuneSystem.Models.Shipping;
using FortuneSystem.Models.Trims;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace FortuneSystem.Models.Pedidos
{
    public class PedidosData
    {

        readonly RevisionesData objRevision = new RevisionesData();
        readonly CatTipoOrdenData objTipoOrden = new CatTipoOrdenData();
        readonly CatComentariosData objComent = new CatComentariosData();
        readonly CatTypeBrandData objTipoBrand = new CatTypeBrandData();

        // DescripcionItemData objSummary = new DescripcionItemData();
        readonly ArteData objArte = new ArteData();

        //Muestra la lista de PO
        public IEnumerable<OrdenesCompra> ListaOrdenCompra()
        {
            Conexion conn = new Conexion();
            List<OrdenesCompra> listPedidos = new List<OrdenesCompra>();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                // comando.CommandText = "Listar_Pedidos";
                comando.CommandText = "SELECT P.ID_PEDIDO, P.PO, P.VPO, P.CUSTOMER, P.CUSTOMER_FINAL, C.NAME_FINAL, " +
                    "P.DATE_CANCEL, P.DATE_ORDER, P.TOTAL_UNITS, P.ID_STATUS, S.ESTADO FROM PEDIDO P, CAT_STATUS S, " +
                    "CAT_CUSTOMER_PO C WHERE P.ID_STATUS = S.ID_STATUS AND P.CUSTOMER_FINAL = C.CUSTOMER_FINAL AND " +
                    "P.ID_STATUS = 1 ORDER BY P.DATE_ORDER DESC ";
                comando.CommandType = CommandType.Text;
                leer = comando.ExecuteReader();

                while (leer.Read())
                {

                    OrdenesCompra pedidos = new OrdenesCompra()
                    {
                        IdPedido = Convert.ToInt32(leer["ID_PEDIDO"]),
                        PO = leer["PO"].ToString().TrimEnd(),
                        VPO = leer["VPO"].ToString(),
                        Cliente = Convert.ToInt32(leer["CUSTOMER"]),
                        ClienteFinal = Convert.ToInt32(leer["CUSTOMER_FINAL"]),
                        FechaCancel = Convert.ToDateTime(leer["DATE_CANCEL"]),
                        FechaOrden = Convert.ToDateTime(leer["DATE_ORDER"]),
                        TotalUnidades = Convert.ToInt32(leer["TOTAL_UNITS"]),
                        IdStatus = Convert.ToInt32(leer["ID_STATUS"]),

                    };
                    CatStatus status = new CatStatus()
                    {
                        Estado = leer["ESTADO"].ToString()
                    };
                    CatClienteFinal clienteFinal = new CatClienteFinal()
                    {
                        NombreCliente = leer["NAME_FINAL"].ToString()
                    };

                    pedidos.Historial = objRevision.ObtenerPedidoRevisiones(pedidos.IdPedido);
                    pedidos.CatStatus = status;
                    pedidos.CatClienteFinal = clienteFinal;

                    pedidos.EstatusPack = ObtenerEstilos(pedidos.IdPedido);
                    pedidos.EstatusArtePnl = ObtenerArtePNLEstilos(pedidos.IdPedido);
                    pedidos.EstatusPackAssort = ObtenerEstilosAssort(pedidos.IdPedido);       


                    listPedidos.Add(pedidos);
                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

            return listPedidos;
        }

        public IEnumerable<OrdenesCompra> ListaOrdenCompraPorPO()
        {
            Conexion conn = new Conexion();
            List<OrdenesCompra> listPedidos = new List<OrdenesCompra>();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                // comando.CommandText = "Listar_Pedidos";
                comando.CommandText = "SELECT P.ID_PEDIDO, P.PO FROM PEDIDO P WHERE P.ID_STATUS = 1 ORDER BY P.DATE_ORDER DESC ";
                comando.CommandType = CommandType.Text;
                leer = comando.ExecuteReader();

                while (leer.Read())
                {

                    OrdenesCompra pedidos = new OrdenesCompra()
                    {
                        IdPedido = Convert.ToInt32(leer["ID_PEDIDO"]),
                        PO = leer["PO"].ToString().TrimEnd()

                    };


                    listPedidos.Add(pedidos);
                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

            return listPedidos;
        }

        //Saber si existe instrucción de empaque del tipo Assort
        public string ObtenerEstilosAssort(int IdPedido)
        {

            Conexion conn = new Conexion();
            string dato = "";
            int valor = 0;
            int numEstilos = 0;
            int contador = 0;
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();

                List<ItemDescripcion> listaEstilos = new List<ItemDescripcion>();
                listaEstilos = ListaEstilos(IdPedido).ToList();
                //string valores = "";
                numEstilos = listaEstilos.Count;
                for (int v = 0; v < listaEstilos.Count; v++)
                {

                    int val = listaEstilos[v].IdSummary;
                    string query = Convert.ToString(val);
                    if (query == "")
                    {
                        query = "0";
                    }

                    comando.CommandText = "select DISTINCT(R.ID_SUMMARY) AS numPack from PACKING_TYPE_SIZE R " +
                        "WHERE R.ID_SUMMARY in(" + query + ") AND TYPE_PACKING=3";
                    comando.CommandType = CommandType.Text;
                    leer = comando.ExecuteReader();

                    while (leer.Read())
                    {

                        int numero = Convert.ToInt32(leer["numPack"]);

                        if (numero != 0)
                        {
                            contador++;

                        }
                        valor = contador;


                    }
                    leer.Close();
                }

                if (valor != 0)
                {
                    dato = "X";
                }
                else
                {
                    dato = "-";
                }
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }
            return dato;
        }

        public string ObtenerEstilos(int IdPedido)
        {

            Conexion conn = new Conexion();
            string dato = "";
            int valor = 0;
            int numEstilos = 0;
            int contador = 0;
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();

                List<ItemDescripcion> listaEstilos = new List<ItemDescripcion>();
                listaEstilos = ListaEstilos(IdPedido).ToList();
                //string valores = "";
                numEstilos = listaEstilos.Count;
                for (int v = 0; v < listaEstilos.Count; v++)
                {

                    int val = listaEstilos[v].IdSummary;
                    string query = Convert.ToString(val);
                    if (query == "")
                    {
                        query = "0";
                    }

                    comando.CommandText = "select DISTINCT(R.ID_SUMMARY) AS numPack from PACKING_TYPE_SIZE R " +
                        "WHERE R.ID_SUMMARY in(" + query + ") ";
                    comando.CommandType = CommandType.Text;
                    leer = comando.ExecuteReader();

                    while (leer.Read())
                    {

                        int numero = Convert.ToInt32(leer["numPack"]);

                        if (numero != 0)
                        {
                            contador++;

                        }
                        valor = contador;


                    }
                    leer.Close();
                }

                if (valor == numEstilos)
                {
                    dato = "X";
                }
                else
                {
                    dato = "-";
                }
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }
            return dato;
        }

        public string ObtenerArtePNLEstilos(int IdPedido)
        {

            Conexion conn = new Conexion();
            string dato = "";
            int valor = 0;
            int numEstilos = 0;
            int contador = 0;
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();

                List<ItemDescripcion> listaEstilos = new List<ItemDescripcion>();
                listaEstilos = ListaEstilos(IdPedido).ToList();
                //string valores = "";
                numEstilos = listaEstilos.Count;
                for (int v = 0; v < listaEstilos.Count; v++)
                {

                    int val = listaEstilos[v].IdSummary;
                    string query = Convert.ToString(val);
                    if (query == "")
                    {
                        query = "0";
                    }

                    comando.CommandText = "select COUNT(R.IdSummary) AS numArte from IMAGEN_ARTE_PNL R " +
                        "WHERE R.IdSummary in(" + query + ") AND R.extensionPNL !='' ";
                    comando.CommandType = CommandType.Text;
                    leer = comando.ExecuteReader();

                    while (leer.Read())
                    {

                        int numero = Convert.ToInt32(leer["numArte"]);

                        if (numero != 0)
                        {
                            contador++;

                        }
                        valor = contador;


                    }
                    leer.Close();
                }

                if (valor == numEstilos)
                {
                    dato = "X";
                }
                else
                {
                    dato = "-";
                }
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }
            return dato;
        }

        //Muestra la lista de PO para WIP
        public IEnumerable<OrdenesCompra> ListaOrdenCompraWIP(int estadoTab)
        {
            Conexion conn = new Conexion();
            List<OrdenesCompra> listPedidos = new List<OrdenesCompra>();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                // comando.CommandText = "Listar_Pedidos";
                if (estadoTab == 1)
                {
                    //bd = "PEDIDO P";
                    comando.CommandText = "SELECT P.ID_PEDIDO, CU.NAME, CF.NAME_FINAL,P.DATE_CANCEL, P.DATE_ORDER, P.PO, P.VPO,PO.ID_PO_SUMMARY, PO.QTY,I.ITEM_STYLE, I.DESCRIPTION, CC.DESCRIPCION,G.GENERO, P.ID_TYPE_ORDER,I.ITEM_ID, PO.FECHA_UCC,PO.ID_ESTADO,PO.ID_SUCURSAL  FROM PEDIDO P " +
                    "INNER JOIN CAT_CUSTOMER CU ON CU.CUSTOMER=P.CUSTOMER " +
                    "INNER JOIN CAT_CUSTOMER_PO CF ON CF.CUSTOMER_FINAL=P.CUSTOMER_FINAL " +
                    "INNER JOIN PO_SUMMARY PO ON PO.ID_PEDIDOS=P.ID_PEDIDO " +
                    "INNER JOIN ITEM_DESCRIPTION I ON I.ITEM_ID=PO.ITEM_ID " +
                    "INNER JOIN CAT_COLORES CC ON CC.ID_COLOR=PO.ID_COLOR " +
                    "INNER JOIN CAT_GENDER G ON G.ID_GENDER=PO.ID_GENDER " +
                    "WHERE PO.ID_ESTADO=1 "; //AND YEAR(P.DATE_CANCEL) = 2019
                }
                else if (estadoTab == 2)
                {
                    // bd = "shipping_ids S";
                    List<Shipped> listaShipped = new List<Shipped>();
                    listaShipped = ListadoShipped().ToList();
                    string valores = "";
                    for (int v = 0; v < listaShipped.Count; v++)
                    {
                        if (v > 0)
                        {
                            valores += "," + listaShipped[v].Id_summary;
                        }
                        else
                        {
                            valores += listaShipped[v].Id_summary;
                        }

                    }
                    string query = valores;

                    comando.CommandText = "SELECT P.ID_PEDIDO, CU.NAME, CF.NAME_FINAL,P.DATE_CANCEL, P.DATE_ORDER, P.PO, P.VPO,PO.ID_PO_SUMMARY, PO.QTY,I.ITEM_STYLE, I.DESCRIPTION, CC.DESCRIPCION,G.GENERO, P.ID_TYPE_ORDER,I.ITEM_ID, PO.FECHA_UCC,PO.ID_ESTADO,PO.ID_SUCURSAL  FROM PEDIDO P " +
                    "INNER JOIN CAT_CUSTOMER CU ON CU.CUSTOMER=P.CUSTOMER " +
                    "INNER JOIN CAT_CUSTOMER_PO CF ON CF.CUSTOMER_FINAL=P.CUSTOMER_FINAL " +
                    "INNER JOIN PO_SUMMARY PO ON PO.ID_PEDIDOS=P.ID_PEDIDO " +
                    "INNER JOIN ITEM_DESCRIPTION I ON I.ITEM_ID=PO.ITEM_ID " +
                    "INNER JOIN CAT_COLORES CC ON CC.ID_COLOR=PO.ID_COLOR " +
                    "INNER JOIN CAT_GENDER G ON G.ID_GENDER=PO.ID_GENDER " +
                    // "INNER JOIN shipping_ids S ON S.id_po_summary=PO.ID_PO_SUMMARY " +
                    "WHERE PO.ID_ESTADO=7 AND PO.ID_PO_SUMMARY in(" + query + ") ";
                }
                else if (estadoTab == 3)
                {
                    comando.CommandText = "SELECT P.ID_PEDIDO, CU.NAME, CF.NAME_FINAL,P.DATE_CANCEL, P.DATE_ORDER, P.PO, P.VPO,PO.ID_PO_SUMMARY, PO.QTY,I.ITEM_STYLE, I.DESCRIPTION, CC.DESCRIPCION,G.GENERO, P.ID_TYPE_ORDER,I.ITEM_ID, PO.FECHA_UCC, PO.ID_SUCURSAL  FROM PEDIDO P " +
                    "INNER JOIN CAT_CUSTOMER CU ON CU.CUSTOMER=P.CUSTOMER " +
                    "INNER JOIN CAT_CUSTOMER_PO CF ON CF.CUSTOMER_FINAL=P.CUSTOMER_FINAL " +
                    "INNER JOIN PO_SUMMARY PO ON PO.ID_PEDIDOS=P.ID_PEDIDO " +
                    "INNER JOIN ITEM_DESCRIPTION I ON I.ITEM_ID=PO.ITEM_ID " +
                    "INNER JOIN CAT_COLORES CC ON CC.ID_COLOR=PO.ID_COLOR " +
                    "INNER JOIN CAT_GENDER G ON G.ID_GENDER=PO.ID_GENDER " +
                    "WHERE PO.ID_ESTADO=6 ";
                }
                comando.CommandType = CommandType.Text;
                leer = comando.ExecuteReader();

                while (leer.Read())
                {
                    InfoSummary infSummary = new InfoSummary();

                    OrdenesCompra pedidos = new OrdenesCompra()
                    {
                        IdPedido = Convert.ToInt32(leer["ID_PEDIDO"]),
                        PO = leer["PO"].ToString(),
                        VPO = leer["VPO"].ToString(),
                        FechaCancel = Convert.ToDateTime(leer["DATE_CANCEL"]),
                        FechaOrden = Convert.ToDateTime(leer["DATE_ORDER"])

                    };
                    CatCliente cliente = new CatCliente()
                    {
                        Nombre = leer["NAME"].ToString()
                    };
                    CatClienteFinal clienteFinal = new CatClienteFinal()
                    {
                        NombreCliente = leer["NAME_FINAL"].ToString()
                    };
                    POSummary.POSummary itemsPO = new POSummary.POSummary()
                    {
                        IdItems = Convert.ToInt32(leer["ID_PO_SUMMARY"]),
                        Cantidad = Convert.ToInt32(leer["QTY"]),
                        IdSucursal = Convert.ToInt32(leer["ID_SUCURSAL"])
                    };

                    ItemDescripcion estilosI = new ItemDescripcion()
                    {
                        ItemEstilo = leer["ITEM_STYLE"].ToString(),
                        Descripcion = leer["DESCRIPTION"].ToString(),
                        ItemId = Convert.ToInt32(leer["ITEM_ID"])
                    };

                    CatColores colores = new CatColores()
                    {
                        DescripcionColor = leer["DESCRIPCION"].ToString()
                    };

                    CatGenero genero = new CatGenero()
                    {
                        Genero = leer["GENERO"].ToString()
                    };


                    if (!Convert.IsDBNull(leer["ID_TYPE_ORDER"]))
                    {
                        pedidos.IdTipoOrden = Convert.ToInt32(leer["ID_TYPE_ORDER"]);
                    }

                    if (!Convert.IsDBNull(leer["FECHA_UCC"]))
                    {
                        itemsPO.FechaUCC = Convert.ToDateTime(leer["FECHA_UCC"]);
                        infSummary.FechaUCC = String.Format("{0:dd/MMM/yyyy}", itemsPO.FechaUCC);
                    }
                    else
                    {

                        infSummary.FechaUCC = "";
                    }

                    CatTipoOrden tipoOrden = new CatTipoOrden
                    {
                        TipoOrden = objTipoOrden.Obtener_Tipo_Orden_Por_id(pedidos.IdTipoOrden)
                    };
                    if (tipoOrden.TipoOrden == "")
                    {
                        tipoOrden.TipoOrden = "N/A";
                    }
                    //Datos Comentarios
                    CatComentarios datosComentario = objComent.Obtener_Utlimo_Comentario_Por_IdSummnary(itemsPO.IdItems);
                    pedidos.CatComentarios = datosComentario;
                    if (pedidos.CatComentarios.IdComentario > 0)
                    {

                        pedidos.CatComentarios.FechaComents = String.Format("{0:dd/MM/yyyy}", pedidos.CatComentarios.FechaComentario);
                        pedidos.FechaActComent = String.Format("{0:dd/MM/yyyy}", pedidos.CatComentarios.FechaComentario);
                    }
                    else
                    {
                        pedidos.CatComentarios.FechaComents = "";
                    }

                    string codigoBrand = estilosI.ItemEstilo.Substring(0, 4);
                    CatTypeBrand tipoBrand = new CatTypeBrand
                    {
                        TipoBrandName = objTipoBrand.Obtener_Tipo_Brand_Por_Estilo(codigoBrand)
                    };
                    if (tipoBrand.TipoBrandName == "")
                    {
                        tipoBrand.TipoBrandName = "GENERIC";
                    }
                    //Status de Imagen Arte
                    IMAGEN_ARTE imgArte = new IMAGEN_ARTE();
                    imgArte = objArte.BuscarEstiloArteImagen(estilosI.ItemId);
                    if (imgArte.StatusArte == 1)
                    {
                        imgArte.StatusArteInf = "APPROVED";

                    }
                    else if (imgArte.StatusArte == 2)
                    {
                        imgArte.StatusArteInf = "REVIEWED";
                    }
                    else if (imgArte.StatusArte == 4)
                    {
                        imgArte.StatusArteInf = "IN HOUSE";
                    }
                    else if (imgArte.StatusArte == 3)
                    {
                        imgArte.StatusArteInf = "PENDING";
                    }
                    else
                    {
                        imgArte.StatusArteInf = "PENDING";
                    }

                    //Status de Imagen PNL
                    IMAGEN_ARTE_PNL imgPNL = new IMAGEN_ARTE_PNL();
                    imgPNL = objArte.BuscarEstiloArtePnlImagen(itemsPO.IdItems/*, estilosI.Descripcion*/);
                    if (imgPNL.StatusPNL == 1)
                    {
                        imgPNL.StatusArtePnlInf = "APPROVED";

                    }
                    else if (imgPNL.StatusPNL == 2)
                    {
                        imgPNL.StatusArtePnlInf = "IN HOUSE";
                    }
                    else if (imgPNL.StatusPNL == 3)
                    {
                        imgPNL.StatusArtePnlInf = "REVIEWED";
                    }
                    else if (imgPNL.StatusPNL == 4)
                    {
                        imgPNL.StatusArtePnlInf = "PENDING";
                    }
                    else
                    {
                        imgPNL.StatusArtePnlInf = "PENDING";
                    }

                    string gender = genero.Genero.TrimEnd(' ');
                    genero.Genero = gender;

                    //LISTA DE COMENTARIOS
                    pedidos.ListaComentarios = objComent.ListaComentarios(itemsPO.IdItems).ToList();
                    //CADENA DE MILLPO
                    //pedidos.MillPO = Obtener_Mill_PO_Recibos(pedidos.IdPedido, estilosI.ItemId);
                    pedidos.MillPO = Obtener_Mill_PO(pedidos.IdPedido);

                    if (pedidos.MillPO == null)
                    {
                        pedidos.MillPO = "";
                    }


                    //LISTA DE BLANKS

                    infSummary.ListSummary = ListaSummaryBlanksId(itemsPO.IdItems).ToList();

                    pedidos.ListadoRecibos = ListaRecibos(itemsPO.IdItems).ToList();

                    foreach (var itemSummary in infSummary.ListSummary)
                    {
                        int Talla = itemSummary.IdTalla;
                        infSummary.TotalEstilo = itemSummary.TotalEstilo;
                        infSummary.Talla = itemSummary.Talla;
                        foreach (var itemRecibo in pedidos.ListadoRecibos)
                        {
                            int TallaRecibo = itemRecibo.Inventario.id_size;
                            if (Talla == TallaRecibo)
                            {
                                pedidos.TotalRecibo += itemRecibo.total;
                            }

                        }

                    }

                    //LISTA DE SHIPPED
                    List<Shipped> listaShipped = new List<Shipped>();
                    listaShipped = ListadoShipped().ToList();
                    Shipped datosShipped = new Shipped();
                    foreach (var item in listaShipped)
                    {

                        if (item.Id_summary == itemsPO.IdItems)
                        {
                            // itemsPO.Cantidad = itemsPO.Cantidad - item.Cantidad;
                            itemsPO.Cantidad -= item.Cantidad;
                            datosShipped.Cantidad = item.Cantidad;
                        }

                    }

                    // PACK INSTRUCTION
                    InfoPackInstruction packInst = new InfoPackInstruction();
                    packInst = ObtenerFechaPackingInst(pedidos.IdPedido);
                    if (packInst.IdInstructionPack != 0)
                    {
                        if (packInst.EstadoPack == 1)
                        {
                            packInst.Fecha_Pack = String.Format("{0:dd/MMM/yyyy}", packInst.Fecha_Rec_Pack);
                        }
                    }


                    pedidos.InfoPackInstruction = packInst;
                    pedidos.Shipped = datosShipped;
                    infSummary.ItemDesc = estilosI;
                    infSummary.CatColores = colores;
                    infSummary.CatGenero = genero;
                    infSummary.IdItems = itemsPO.IdItems;
                    infSummary.IdSucursal = itemsPO.IdSucursal;
                    infSummary.CantidadEstilo = itemsPO.Cantidad;
                    pedidos.CatCliente = cliente;
                    pedidos.CatClienteFinal = clienteFinal;
                    pedidos.CatTipoOrden = tipoOrden;
                    pedidos.CatTipoBrand = tipoBrand;
                    pedidos.IdSummaryOrden = itemsPO.IdItems;
                    pedidos.ImagenArte = imgArte;
                    pedidos.ImagenArtePnl = imgPNL;
                    pedidos.InfoSummary = infSummary;
                    pedidos.IdEstilo = estilosI.ItemId;

                    //TOTAL DE CAMISETAS IMPRESAS EN PRINTSHOP
                    pedidos.TotalPrinted = TotalPrintedPorIdSummary(pedidos.IdSummaryOrden);
                    pedidos.RestaPrintshop = itemsPO.Cantidad - pedidos.TotalPrinted;
                    pedidos.DestinoSalida = ObtenerDestinoSalidaSummary(infSummary.IdItems);

                    //Trims 
                    // pedidos.Trims = ObtenerTotalTrims(infSummary.IdItems);
                    pedidos.Trims = ObtenerTotalTrims(pedidos.IdPedido);
                    if (pedidos.Trims.id_request != 0)
                    {
                        if (pedidos.Trims.estado == "1")
                        {
                            pedidos.Trims.recibo = ObtenerUltimoReciboTrims(pedidos.IdPedido);
                            if (pedidos.Trims.recibo != 0)
                            {
                                DateTime fechaRec = ObtenerFechaUltimoReciboTrims(pedidos.Trims.recibo);
                                pedidos.Trims.fecha_recibo = String.Format("{0:dd/MMM/yyyy}", fechaRec);
                            }
                            else
                            {
                                DateTime fechaTrims = ObtenerFechaUltimoTrims(pedidos.IdPedido);
                                pedidos.Trims.fecha_recibo = String.Format("{0:dd/MMM/yyyy}", fechaTrims);
                            }
                        }
                    }
                    //Price tickets
                    pedidos.InfoPriceTickets = ObtenerTotalPriceTicketsTrims(pedidos.IdPedido);
                    if (pedidos.InfoPriceTickets.Id_request_pt != 0)
                    {
                        if (pedidos.InfoPriceTickets.Estado == "1")
                        {
                            pedidos.InfoPriceTickets.Recibo = ObtenerUltimoReciboPriceTicketsTrims(infSummary.IdItems);
                            if(pedidos.InfoPriceTickets.Recibo != 0)
                            {
                                DateTime fechaRec = ObtenerFechaUltimoReciboTrims(pedidos.InfoPriceTickets.Recibo);
                                pedidos.InfoPriceTickets.Fecha_recibo = String.Format("{0:dd/MMM/yyyy}", fechaRec);
                            }
                            else
                            {
                                DateTime fechaPrice = ObtenerFechaUltimoTrims(pedidos.IdPedido);
                                pedidos.InfoPriceTickets.Fecha_recibo = String.Format("{0:dd/MMM/yyyy}", fechaPrice);
                            }

                            
                        }
                    }


                    int TotalRest = infSummary.CantidadEstilo - pedidos.TotalRecibo;
                    if (pedidos.TotalRecibo != 0)
                    {
                        if (TotalRest <= 2)
                        {
                            pedidos.TipoPartial = "COMPLETE";
                        }
                        else
                        {
                            pedidos.TipoPartial = "PARTIAL";
                        }
                        pedidos.TotalRestante = pedidos.TotalRecibo - infSummary.CantidadEstilo;
                    }

                    //pedidos.FechaCancelada = pedidos.FechaCancel.ToString("dd/MMMM/yyyy");

                    pedidos.FechaCancelada = String.Format("{0:dd/MMM/yyyy}", pedidos.FechaCancel);
                    pedidos.FechaRecOrden = String.Format("{0:dd/MM/yyyy}", pedidos.FechaOrden);

                    DateTime fecha = pedidos.FechaCancel;
                    DateTime dt = fecha;
                    for (int k = 0; k < 2; k++)
                    {
                        dt = dt.AddDays(-1);
                    }
                    pedidos.FechaFinalOrden = dt;
                    pedidos.FechaOrdenFinal = String.Format("{0:dd/MMM/yyyy}", pedidos.FechaFinalOrden);

                    //pedidos.FechaCancel = fechaCancelada.;

                    // DateTime.Now.ToString("MM/dd/yyyy")
                    listPedidos.Add(pedidos);
                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }
            return listPedidos;
        }

        //Muestra la lista de ordenes de compra Shipped 
        public IEnumerable<Shipped> ListadoShipped()
        {

            Conexion conn = new Conexion();
            List<Shipped> listShipped = new List<Shipped>();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                // comando.CommandText = "Listar_Pedidos";
                //comando.CommandText = "SELECT  S.id_po_summary, S.cantidad FROM shipping_ids S ";
                comando.CommandText = "SELECT  S.id_summary, S.total FROM totales_envios S ";
                comando.CommandType = CommandType.Text;
                leer = comando.ExecuteReader();

                while (leer.Read())
                {
                    Shipped datos = new Shipped();
                    int id = Convert.ToInt32(leer["id_summary"]);
                    Shipped result = listShipped.Find(x => x.Id_summary == id);
                    if (result == null)
                    {
                        datos.Id_summary = Convert.ToInt32(leer["id_summary"]);
                        datos.Cantidad = Convert.ToInt32(leer["total"]);
                        listShipped.Add(datos);

                    }
                    else
                    {
                        if (result.Id_summary == id)
                        {
                            result.Cantidad += Convert.ToInt32(leer["total"]);
                        }
                    }
                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }
            return listShipped;
        }

        //Lista pedidos
        public IEnumerable<ItemDescripcion> ListaEstilos(int? id)
        {
            int pedido = Convert.ToInt32(id);
            Conexion conn = new Conexion();
            List<ItemDescripcion> listEstilos = new List<ItemDescripcion>();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                // comando.CommandText = "Listar_Pedidos";
                comando.CommandText = "select  PS.ID_PO_SUMMARY from po_summary PS " +
                    "where  PS.id_pedidos= '" + pedido + "'";
                comando.CommandType = CommandType.Text;
                leer = comando.ExecuteReader();

                while (leer.Read())
                {

                    ItemDescripcion estilos = new ItemDescripcion()
                    {

                        IdSummary = Convert.ToInt32(leer["ID_PO_SUMMARY"])
                    };

                    listEstilos.Add(estilos);
                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }
            return listEstilos;
        }


        //Muestra la lista de estilos por IdPedido
        public IEnumerable<ItemDescripcion> ListaEstilosPorIdPedido(int? id)
        {
            int pedido = Convert.ToInt32(id);
            Conexion conn = new Conexion();
            List<ItemDescripcion> listEstilos = new List<ItemDescripcion>();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                // comando.CommandText = "Listar_Pedidos";
                comando.CommandText = "select  ITD.ITEM_ID, ITD.ITEM_STYLE, ITD.DESCRIPTION, PS.ID_PO_SUMMARY from po_summary PS " +
                    "INNER JOIN item_description ITD ON PS.ITEM_ID = ITD.ITEM_ID " +
                    "where  PS.id_pedidos= '" + pedido + "'";
                comando.CommandType = CommandType.Text;
                leer = comando.ExecuteReader();

                while (leer.Read())
                {

                    ItemDescripcion estilos = new ItemDescripcion()
                    {

                        ItemEstilo = leer["ITEM_STYLE"].ToString(),
                        ItemId = Convert.ToInt32(leer["ITEM_ID"]),
                        IdSummary = Convert.ToInt32(leer["ID_PO_SUMMARY"]),
                        Descripcion = leer["DESCRIPTION"].ToString()
                    };



                    listEstilos.Add(estilos);
                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }
            return listEstilos;
        }

        //Muestra la lista de estilos por IdPedido
        public IEnumerable<ItemDescripcion> ListaItemEstilosPorIdPedido(int? id)
        {
            int pedido = Convert.ToInt32(id);
            Conexion conn = new Conexion();
            List<ItemDescripcion> listEstilos = new List<ItemDescripcion>();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select PS.ID_PO_SUMMARY, PS.ITEM_ID, RTRIM(ITD.ITEM_STYLE) as ITEM_STYLE, RTRIM(ITD.DESCRIPTION) as DESCRIPTION, PS.ID_COLOR, CONCAT(RTRIM(C.CODIGO_COLOR),'  ',C.DESCRIPCION ) AS DESCRIPCION, RTRIM(C.CODIGO_COLOR) AS CODIGO from po_summary PS " +
                    "INNER JOIN item_description ITD ON PS.ITEM_ID = ITD.ITEM_ID " +
                    "INNER JOIN cat_colores C ON PS.ID_COLOR = C.ID_COLOR " +
                    "where PS.id_pedidos= '" + pedido + "'";
                comando.CommandType = CommandType.Text;
                leer = comando.ExecuteReader();

                while (leer.Read())
                {

                    ItemDescripcion estilos = new ItemDescripcion()
                    {

                        ItemId = Convert.ToInt32(leer["ITEM_ID"]),
                        ItemEstilo = leer["ITEM_STYLE"].ToString(),
                        Descripcion = leer["DESCRIPTION"].ToString(),
                        IdSummary = Convert.ToInt32(leer["ID_PO_SUMMARY"])
                    };

                    CatColores CatColores = new CatColores()
                    {
                        DescripcionColor = leer["DESCRIPCION"].ToString(),
                        IdColor = Convert.ToInt32(leer["ID_COLOR"]),
                        CodigoColor = leer["CODIGO"].ToString()
                    };


                    estilos.CatColores = CatColores;



                    listEstilos.Add(estilos);
                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }
            return listEstilos;
        }

        //Muestra la lista de PO
        public IEnumerable<OrdenesCompra> ListaRevisionesPO(int idEstilo)
        {

            int resultado = 2;
            int temp = idEstilo;

            List<OrdenesCompra> listPedidos = new List<OrdenesCompra>();

            Conexion conex = new Conexion();
            try
            {
                while (resultado != 0)
                {
                    SqlCommand com = new SqlCommand();
                    SqlDataReader leerF = null;
                    com.Connection = conex.AbrirConexion();
                    com.CommandText = "Listar_Pedidos_Revisiones";
                    com.CommandType = CommandType.StoredProcedure;
                    com.Parameters.AddWithValue("@Id", temp);
                    leerF = com.ExecuteReader();
                    resultado = objRevision.ObtenerNoPedidoRevisiones(temp);
                    while (leerF.Read())
                    {

                        OrdenesCompra pedidos = new OrdenesCompra()
                        {
                            IdPedido = Convert.ToInt32(leerF["ID_PEDIDO"]),
                            PO = leerF["PO"].ToString(),
                            VPO = leerF["VPO"].ToString(),
                            Cliente = Convert.ToInt32(leerF["CUSTOMER"]),
                            ClienteFinal = Convert.ToInt32(leerF["CUSTOMER_FINAL"]),
                            FechaCancel = Convert.ToDateTime(leerF["DATE_CANCEL"]),
                            FechaOrden = Convert.ToDateTime(leerF["DATE_ORDER"]),
                            TotalUnidades = Convert.ToInt32(leerF["TOTAL_UNITS"]),
                            IdStatus = Convert.ToInt32(leerF["ID_STATUS"]),

                        };

                        Revision revision = new Revision()
                        {
                            Id = Convert.ToInt32(leerF["ID"]),
                            IdPedido = Convert.ToInt32(leerF["ID_PEDIDO"]),
                            IdRevisionPO = Convert.ToInt32(leerF["ID_REVISION_PO"]),
                            FechaRevision = Convert.ToDateTime(leerF["FECHA_REVISION"])
                        };

                        CatClienteFinal clienteFinal = new CatClienteFinal()
                        {
                            NombreCliente = leerF["NAME_FINAL"].ToString()
                        };

                        CatCliente cliente = new CatCliente()
                        {
                            Nombre = leerF["NAME"].ToString()
                        };

                        temp = revision.IdPedido;

                        pedidos.Revision = revision;
                        pedidos.CatCliente = cliente;
                        pedidos.CatClienteFinal = clienteFinal;

                        listPedidos.Add(pedidos);
                    }
                    leerF.Close();
                }
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return listPedidos;
        }

        //Muestra la lista de PO por fechas
        public IEnumerable<OrdenesCompra> ListaOrdenCompraPorFechas(DateTime? fechaCanc, DateTime? fechaOrden)
        {
            Conexion conex = new Conexion();
            List<OrdenesCompra> listPedidos = new List<OrdenesCompra>();
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;
                com.Connection = conex.AbrirConexion();
                com.CommandText = "Listar_Pedidos_Por_Fechas";
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.AddWithValue("@FechaCanc", fechaCanc);
                com.Parameters.AddWithValue("@FechaOrden", fechaOrden);
                leerF = com.ExecuteReader();

                while (leerF.Read())
                {

                    OrdenesCompra pedidos = new OrdenesCompra()
                    {
                        IdPedido = Convert.ToInt32(leerF["ID_PEDIDO"]),
                        PO = leerF["PO"].ToString(),
                        VPO = leerF["VPO"].ToString(),
                        Cliente = Convert.ToInt32(leerF["CUSTOMER"]),
                        ClienteFinal = Convert.ToInt32(leerF["CUSTOMER_FINAL"]),
                        FechaCancel = Convert.ToDateTime(leerF["DATE_CANCEL"]),
                        FechaOrden = Convert.ToDateTime(leerF["DATE_ORDER"]),
                        TotalUnidades = Convert.ToInt32(leerF["TOTAL_UNITS"]),
                        IdStatus = Convert.ToInt32(leerF["ID_STATUS"])

                    };
                    CatStatus status = new CatStatus()
                    {
                        Estado = leerF["ESTADO"].ToString()
                    };
                    CatClienteFinal clienteFinal = new CatClienteFinal()
                    {
                        NombreCliente = leerF["NAME_FINAL"].ToString()
                    };

                    pedidos.CatStatus = status;
                    pedidos.CatClienteFinal = clienteFinal;


                    listPedidos.Add(pedidos);
                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return listPedidos;
        }

        //Permite consultar los detalles de un PO
        public OrdenesCompra ConsultarListaPO(int? id)
        {
            Conexion conexion = new Conexion();
            OrdenesCompra pedidos = new OrdenesCompra();
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;

                com.Connection = conexion.AbrirConexion();
                com.CommandText = "Lista_Pedido_Por_Id";
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.AddWithValue("@Id", id);

                leerF = com.ExecuteReader();
                while (leerF.Read())
                {

                    pedidos.IdPedido = Convert.ToInt32(leerF["ID_PEDIDO"]);
                    pedidos.PO = leerF["PO"].ToString();
                    pedidos.VPO = leerF["VPO"].ToString();
                    pedidos.Cliente = Convert.ToInt32(leerF["CUSTOMER"]);
                    pedidos.ClienteFinal = Convert.ToInt32(leerF["CUSTOMER_FINAL"]);
                    pedidos.FechaCancel = Convert.ToDateTime(leerF["DATE_CANCEL"]);
                    pedidos.FechaOrden = Convert.ToDateTime(leerF["DATE_ORDER"]);
                    pedidos.TotalUnidades = Convert.ToInt32(leerF["TOTAL_UNITS"]);
                    pedidos.IdStatus = Convert.ToInt32(leerF["ID_STATUS"]);
                    pedidos.Usuario = Convert.ToInt32(leerF["ID_USUARIO"]);
                    if (!Convert.IsDBNull(leerF["ID_TYPE_ORDER"]))
                    {
                        pedidos.IdTipoOrden = Convert.ToInt32(leerF["ID_TYPE_ORDER"]);
                    }

                    DateTime fecha = pedidos.FechaCancel;
                    DateTime dt = fecha;
                    for (int k = 0; k < 2; k++)
                    {
                        dt = dt.AddDays(-1);
                    }
                    if (dt.DayOfWeek == DayOfWeek.Saturday) { dt = dt.AddDays(-2); }
                    if (dt.DayOfWeek == DayOfWeek.Sunday) { dt = dt.AddDays(-2); }
                    pedidos.FechaFinalOrden = dt;
                }
                leerF.Close();
            }
            finally
            {
                conexion.CerrarConexion();
                conexion.Dispose();
            }

            return pedidos;

        }


        //Permite crear un nuevo PO
        public void AgregarPO(OrdenesCompra ordenCompra)
        {
            Conexion conn = new Conexion();
            try
            {
                SqlCommand comando = new SqlCommand
                {
                    Connection = conn.AbrirConexion(),
                    CommandText = "AgregarPedido",
                    CommandType = CommandType.StoredProcedure
                };

                comando.Parameters.AddWithValue("@idPO", ordenCompra.PO.ToUpper());
                comando.Parameters.AddWithValue("@idPOF", ordenCompra.VPO.ToUpper());
                comando.Parameters.AddWithValue("@Customer", ordenCompra.Cliente);
                comando.Parameters.AddWithValue("@CustomerF", ordenCompra.ClienteFinal);
                comando.Parameters.AddWithValue("@datecancel", ordenCompra.FechaCancel);
                comando.Parameters.AddWithValue("@datePO", ordenCompra.FechaOrden);
                comando.Parameters.AddWithValue("@totUnid", ordenCompra.TotalUnidades);
                comando.Parameters.AddWithValue("@idStatus", ordenCompra.IdStatus);
                comando.Parameters.AddWithValue("@idUser", ordenCompra.Usuario);
                comando.Parameters.AddWithValue("@idTipoOrden", ordenCompra.IdTipoOrden);

                comando.ExecuteNonQuery();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

        }

        //Permite actualiza la informacion de un PO
        public void ActualizarPedidos(OrdenesCompra ordenCompra)
        {
            Conexion conn = new Conexion();
            try
            {
                SqlCommand comando = new SqlCommand
                {
                    Connection = conn.AbrirConexion(),
                    CommandText = "Actualizar_Pedido",
                    CommandType = CommandType.StoredProcedure
                };

                comando.Parameters.AddWithValue("@Id", ordenCompra.IdPedido);
                comando.Parameters.AddWithValue("@PO", ordenCompra.PO.ToUpper());
                comando.Parameters.AddWithValue("@VPO", ordenCompra.VPO.ToUpper());
                comando.Parameters.AddWithValue("@IdCliente", ordenCompra.Cliente);
                comando.Parameters.AddWithValue("@IdClienteF", ordenCompra.ClienteFinal);
                comando.Parameters.AddWithValue("@DateCancel", ordenCompra.FechaCancel);
                comando.Parameters.AddWithValue("@DateOrden", ordenCompra.FechaOrden);
                comando.Parameters.AddWithValue("@TotalU", ordenCompra.TotalUnidades);
                comando.Parameters.AddWithValue("@IdTipoOrden", ordenCompra.IdTipoOrden);

                comando.ExecuteNonQuery();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }
        }

        public int Obtener_Utlimo_po()
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "SELECT ID_PEDIDO FROM PEDIDO WHERE ID_PEDIDO = (SELECT MAX(ID_PEDIDO) FROM PEDIDO) ";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    return Convert.ToInt32(reader["ID_PEDIDO"]);
                }

            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return 0;
        }
        //Obtener el id de orden por IdSummary
        public int Obtener_Id_Pedido(int? id)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "SELECT ID_PEDIDOS FROM PO_SUMMARY WHERE ID_PO_SUMMARY = '" + id + "' ";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    return Convert.ToInt32(reader["ID_PEDIDOS"]);
                }

            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return 0;
        }

        //Obtener el id de orden por Nombre Pedido
        public int Obtener_Id_Pedido_Nombre(string pedido)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "select ID_PEDIDO from pedido where po like '%" + pedido + "%' ";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    return Convert.ToInt32(reader["ID_PEDIDO"]);
                }

            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return 0;
        }


        public void ActualizarEstadoPO(int id)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "UPDATE PEDIDO SET ID_STATUS =5 WHERE ID_PEDIDO='" + id + "'";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                conex.CerrarConexion();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

        }

        public void ActualizarEstadoPOCancelado(int id)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "UPDATE PEDIDO SET ID_STATUS =6 WHERE ID_PEDIDO='" + id + "'";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                conex.CerrarConexion();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

        }

        public void ActualizarEstadoStyleCancelado(int id)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "UPDATE PO_SUMMARY SET ID_ESTADO =6 WHERE ID_PO_SUMMARY='" + id + "'";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                conex.CerrarConexion();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

        }

        public void EliminarEstilo(int id)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "DELETE FROM PO_SUMMARY WHERE ID_PO_SUMMARY='" + id + "' ";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                conex.CerrarConexion();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

        }

        public void EliminarTallasEstilos(int id)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "DELETE FROM ITEM_SIZE WHERE ID_SUMMARY='" + id + "' ";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                conex.CerrarConexion();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

        }

        public void EliminarArteEstilos(int id)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "DELETE FROM ARTE WHERE IdSummary='" + id + "' ";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                conex.CerrarConexion();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

        }

        public void EliminarTipoPackEstilo(int id)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "DELETE FROM CAT_TYPE_PACK_STYLE WHERE ID_SUMMARY='" + id + "' ";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                conex.CerrarConexion();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

        }

        //Muestra la lista de recibos por Pedido y Estilo
        public IEnumerable<recibo> ListaRecibos(int? id)
        {
            Conexion conex = new Conexion();
            List<recibo> listPedidos = new List<recibo>();
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;
                com.Connection = conex.AbrirConexion();
                com.CommandText = "SELECT R.id_recibo,R.fecha,R.mill_po,INV.id_size, R.total, INV.TOTAL, S.TALLA FROM recibos R " +
                    "INNER JOIN recibos_items RI ON RI.id_recibo=R.id_recibo " +
                    "INNER JOIN inventario INV ON INV.id_inventario=RI.id_inventario " +
                    "INNER JOIN CAT_ITEM_SIZE S ON S.ID=INV.id_size " +
                    "WHERE INV.id_summary='" + id + "' ORDER by cast(S.ORDEN AS int) ASC";
                com.CommandType = CommandType.Text;
                leerF = com.ExecuteReader();

                while (leerF.Read())
                {

                    OrdenesCompra pedidos = new OrdenesCompra();
                    recibo DatoRecibo = new recibo()
                    {
                        id_recibo = Convert.ToInt32(leerF["id_recibo"]),
                        fecha = Convert.ToDateTime(leerF["fecha"]).ToString("dd-MM-yyyy"),
                        mill_po = leerF["mill_po"].ToString(),
                        total = Convert.ToInt32(leerF["total"]),
                        TallaRecibo = leerF["TALLA"].ToString()

                    };
                    Inventario inventario = new Inventario()
                    {
                        id_size = Convert.ToInt32(leerF["id_size"]),
                        total = Convert.ToInt32(leerF["TOTAL"])
                    };

                    pedidos.TallaRecibo = leerF["TALLA"].ToString();

                    DatoRecibo.Inventario = inventario;

                    listPedidos.Add(DatoRecibo);
                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return listPedidos;
        }

        //Muestra la lista de recibos por Pedido y Estilo
        public IEnumerable<recibo> ListaRecibosTotales(int? id)
        {
            Conexion conex = new Conexion();
            List<recibo> listPedidos = new List<recibo>();
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;
                com.Connection = conex.AbrirConexion();
                com.CommandText = "SELECT R.id_recibo,R.fecha,R.mill_po,INV.id_size, R.total, INV.TOTAL, S.TALLA FROM recibos R " +
                    "INNER JOIN recibos_items RI ON RI.id_recibo=R.id_recibo " +
                    "INNER JOIN inventario INV ON INV.id_inventario=RI.id_inventario " +
                    "INNER JOIN CAT_ITEM_SIZE S ON S.ID=INV.id_size " +
                    "WHERE INV.id_summary='" + id + "' ORDER by cast(S.ORDEN AS int) ASC";
                com.CommandType = CommandType.Text;
                leerF = com.ExecuteReader();

                while (leerF.Read())
                {
                    

                    OrdenesCompra pedidos = new OrdenesCompra();
                    recibo DatoRecibo = new recibo()
                    {
                        id_recibo = Convert.ToInt32(leerF["id_recibo"]),
                        fecha = Convert.ToDateTime(leerF["fecha"]).ToString("dd-MM-yyyy"),
                        mill_po = leerF["mill_po"].ToString(),
                        total = Convert.ToInt32(leerF["total"]),
                        TallaRecibo = leerF["TALLA"].ToString()

                    };

                    Inventario inventario = new Inventario()
                    {
                        id_size = Convert.ToInt32(leerF["id_size"]),
                        total = Convert.ToInt32(leerF["TOTAL"])
                    };
                    pedidos.TallaRecibo = leerF["TALLA"].ToString();
                    DatoRecibo.Inventario = inventario;
                    recibo result = listPedidos.Find(x => x.TallaRecibo == DatoRecibo.TallaRecibo);

                    if(result == null)
                    {
                        listPedidos.Add(DatoRecibo);
                    }
                    else
                    {
                        if (result.TallaRecibo == DatoRecibo.TallaRecibo)
                        {
                            result.Inventario.total += Convert.ToInt32(leerF["TOTAL"]);
                        }
                    }

                   
                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return listPedidos;
        }

        //Muestra la lista de recibos Blanks por Pedido y Estilo


        //Muestra la lista estilos y tallas por ID pedido y ID Estilo ListaSummaryBlanksId(int? id, int? idEstilo)
        public IEnumerable<InfoSummary> ListaSummaryBlanksId(int? id)
        {
            Conexion conn = new Conexion();
            List<InfoSummary> listSummary = new List<InfoSummary>();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT PO.ID_PO_SUMMARY,PO.QTY, S.TALLA_ITEM, ITS.TALLA, S.CANTIDAD,S.EJEMPLOS FROM PO_SUMMARY PO " +
                    "INNER JOIN ITEM_SIZE S ON S.ID_SUMMARY=PO.ID_PO_SUMMARY " +
                    "INNER JOIN CAT_ITEM_SIZE ITS ON ITS.ID=S.TALLA_ITEM " +
                    "WHERE PO.ID_PO_SUMMARY='" + id + "'  ORDER by cast(ITS.ORDEN AS int) ASC ";
                comando.CommandType = CommandType.Text;
                leer = comando.ExecuteReader();

                while (leer.Read())
                {
                    InfoSummary ItemSummary = new InfoSummary
                    {
                        Talla = leer["TALLA"].ToString(),
                        TotalEstilo = Convert.ToInt32(leer["QTY"]),
                        IdTalla = Convert.ToInt32(leer["TALLA_ITEM"]),
                        IdItems = Convert.ToInt32(leer["ID_PO_SUMMARY"]),
                        CantidadTalla = Convert.ToInt32(leer["CANTIDAD"]) + Convert.ToInt32(leer["EJEMPLOS"])
                    };
                    listSummary.Add(ItemSummary);

                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

            return listSummary;
        }

        //Muestra la lista de millpo por Pedido
        public IEnumerable<InfoMillPO> ListaMillPOPedido(int? id)
        {
            Conexion conn = new Conexion();
            List<InfoMillPO> listMPO = new List<InfoMillPO>();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT ID_MILLPO, MILLPO FROM MILLPO_LIST WHERE ID_PEDIDO='" + id + "' ";
                comando.CommandType = CommandType.Text;
                leer = comando.ExecuteReader();

                while (leer.Read())
                {
                    InfoMillPO millPO = new InfoMillPO()
                    {
                        IdMillPO = Convert.ToInt32(leer["ID_MILLPO"]),
                        MillPO = leer["MILLPO"].ToString()

                    };

                    listMPO.Add(millPO);

                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

            return listMPO;
        }

        //Obtener mill po de recibos
        public string Obtener_Mill_PO_Recibos(int? idPedido, int? idEstilo)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            string millPO = "";
            int cont = 0;
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "SELECT distinct(R.mill_po) FROM recibos R " +
                    "INNER JOIN recibos_items RI ON RI.id_recibo=R.id_recibo " +
                    "INNER JOIN inventario INV ON INV.id_inventario=RI.id_inventario " +
                    "WHERE INV.id_pedido = '" + idPedido + "' AND INV.id_estilo= '" + idEstilo + "' ";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string mp = reader["mill_po"].ToString();
                    if (cont == 0)
                    {
                        millPO = mp;
                        cont++;
                    }
                    else
                    {
                        millPO += "," + mp;
                    }

                }

            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return millPO;
        }

        //Obtener mill po de Millpo_List
        public string Obtener_Mill_PO(int? idPedido)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            string millPO = "";
            int cont = 0;
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "SELECT ID_MILLPO, MILLPO FROM MILLPO_LIST WHERE ID_PEDIDO='" + idPedido + "' ";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string mp = reader["MILLPO"].ToString();
                    if (cont == 0)
                    {
                        millPO = mp;
                        cont++;
                    }
                    else
                    {
                        millPO += "," + mp;
                    }

                }

            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return millPO;
        }


        //Obtener Actualizar información de MILLPO
        public void ActualizarInfoMPO(InfoMillPO datoMPO)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "UPDATE MILLPO_LIST SET MILLPO ='" + datoMPO.MillPO.ToUpper() + "' WHERE ID_MILLPO='" + datoMPO.IdMillPO + "'";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                conex.CerrarConexion();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

        }


        //Muestra la lista de suma de Printed tallas por IdSummary
        public int TotalPrintedPorIdSummary(int? idSummary)
        {
            Conexion conex = new Conexion();
            int suma = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;
                com.Connection = conex.AbrirConexion();
                com.CommandText = "SELECT P.ID_PRINTSHOP, P.PRINTED FROM PRINTSHOP P WHERE ID_SUMMARY='" + idSummary + "' ";
                leerF = com.ExecuteReader();
                while (leerF.Read())
                {
                    suma += Convert.ToInt32(leerF["PRINTED"]);

                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

            return suma;
        }

        //Obtiene la información de un destino por IdSummary
        public int ObtenerDestinoSalidaSummary(int? IdSummary)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            int destino = 0;
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "SELECT SI.id_salida, S.id_destino  FROM salidas_items SI " +
                    "INNER JOIN salidas S ON S.id_salida=SI.id_salida " +
                    "WHERE SI.id_summary = '" + IdSummary + "' ";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    destino = Convert.ToInt32(reader["id_destino"]);


                }

            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return destino;

        }

        //Obtener Total de trims por IdSummary
        public Trim_requests ObtenerTotalTrims(int? idSummary)
        {
            Conexion conexion = new Conexion();
            Trim_requests trims = new Trim_requests();
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;

                com.Connection = conexion.AbrirConexion();
                com.CommandText = "SELECT TR.id_request, TR.restante FROM trim_requests TR " +
                    "INNER JOIN items_catalogue IC ON IC.item_id=TR.id_item " +
                    "WHERE TR.id_pedido= '" + idSummary + "' and IC.fabric_type !='PRICE TICKETS' AND IC.fabric_type !='PRICE TICKET' ";
                com.CommandType = CommandType.Text;

                leerF = com.ExecuteReader();
                while (leerF.Read())
                {
                    trims.estado = "1";
                    trims.id_request = Convert.ToInt32(leerF["id_request"]);
                    trims.restante += Convert.ToInt32(leerF["restante"]);

                }
                leerF.Close();
            }
            finally
            {
                conexion.CerrarConexion();
                conexion.Dispose();
            }

            return trims;

        }

        //Obtener informacion de pedido

        //Obtener Total de trims por IdSummary
        public OrdenesCompra ObtenerPedido(int? idPedido)
        {
            Conexion conexion = new Conexion();
            OrdenesCompra pedidos = new OrdenesCompra();
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;

                com.Connection = conexion.AbrirConexion();
                com.CommandText = "SELECT * FROM PEDIDO WHERE ID_PEDIDO= '" + idPedido + "' ";
                com.CommandType = CommandType.Text;

                leerF = com.ExecuteReader();
                while (leerF.Read())
                {
                    pedidos.IdPedido = Convert.ToInt32(leerF["ID_PEDIDO"]);
                    pedidos.PO = leerF["PO"].ToString();
                    pedidos.VPO = leerF["VPO"].ToString();
                    pedidos.Cliente = Convert.ToInt32(leerF["CUSTOMER"]);
                    pedidos.ClienteFinal = Convert.ToInt32(leerF["CUSTOMER_FINAL"]);
                    pedidos.FechaCancel = Convert.ToDateTime(leerF["DATE_CANCEL"]);
                    pedidos.FechaOrden = Convert.ToDateTime(leerF["DATE_ORDER"]);
                    pedidos.TotalUnidades = Convert.ToInt32(leerF["TOTAL_UNITS"]);
                    pedidos.IdStatus = Convert.ToInt32(leerF["ID_STATUS"]);
                    pedidos.Usuario = Convert.ToInt32(leerF["ID_USUARIO"]);
                    if (!Convert.IsDBNull(leerF["ID_TYPE_ORDER"]))
                    {
                        pedidos.IdTipoOrden = Convert.ToInt32(leerF["ID_TYPE_ORDER"]);
                    }

                    DateTime fecha = pedidos.FechaCancel;
                    DateTime dt = fecha;
                    for (int k = 0; k < 2; k++)
                    {
                        dt = dt.AddDays(-1);
                    }
                    if (dt.DayOfWeek == DayOfWeek.Saturday) { dt = dt.AddDays(-1); }
                    if (dt.DayOfWeek == DayOfWeek.Sunday) { dt = dt.AddDays(-2); }
                    pedidos.FechaFinalOrden = dt;
                }
                leerF.Close();
            }
            finally
            {
                conexion.CerrarConexion();
                conexion.Dispose();
            }

            return pedidos;

        }

        //Obtener Total de price tickets trims por IdSummary
        public InfoPriceTickets ObtenerTotalPriceTicketsTrims(int? idPedido)
        {
            Conexion conexion = new Conexion();
            InfoPriceTickets trims = new InfoPriceTickets();
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;

                com.Connection = conexion.AbrirConexion();
                com.CommandText = "SELECT TR.id_request, IC.descripcion, IC.fabric_type, TR.id_size, TR.total, TR.restante FROM trim_requests TR " +
                    "INNER JOIN items_catalogue IC ON IC.item_id=TR.id_item " +
                    "WHERE TR.id_pedido= '" + idPedido + "' AND (IC.fabric_type='PRICE TICKETS' OR IC.fabric_type='PRICE TICKET') ";
                com.CommandType = CommandType.Text;

                leerF = com.ExecuteReader();
                while (leerF.Read())
                {
                    trims.Estado = "1";
                    trims.Id_request_pt = Convert.ToInt32(leerF["id_request"]);
                    trims.Restante += Convert.ToInt32(leerF["restante"]);
                }
                leerF.Close();
            }
            finally
            {
                conexion.CerrarConexion();
                conexion.Dispose();
            }

            return trims;

        }


        //Obtener Información de trims por Pedido
        public IEnumerable<Trim_requests> ObtenerInformacionTrims(int? idPedido, int? idSummary)
        {
            Conexion conexion = new Conexion();
            List<Trim_requests> Listatrims = new List<Trim_requests>();
           
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;
                com.Connection = conexion.AbrirConexion();
                com.CommandText = "SELECT TR.id_request, IC.descripcion, IC.fabric_type, TR.id_size, TR.total, TR.restante FROM trim_requests TR " +
                    "INNER JOIN items_catalogue IC ON IC.item_id=TR.id_item " +
                    "WHERE TR.id_pedido= '" + idPedido + "' AND IC.fabric_type !='PRICE TICKETS' AND IC.fabric_type !='PRICE TICKET' AND IC.fabric_type !='HANGTAGS'";
                com.CommandType = CommandType.Text;
                leerF = com.ExecuteReader();
                while (leerF.Read())
                {

                    Trim_requests trims = new Trim_requests();
                    trims.estado = "1";
                    trims.id_request = Convert.ToInt32(leerF["id_request"]);
                    trims.Descripcion = leerF["descripcion"].ToString();
                    trims.id_talla = Convert.ToInt32(leerF["id_size"]);
                    trims.total = Convert.ToInt32(leerF["total"]);
                    trims.restante = Convert.ToInt32(leerF["restante"]);
                    trims.tipo_item = leerF["fabric_type"].ToString();
                    

                    Listatrims.Add(trims);
                }
                List<Trim_requests>Lista_Hangtags = ObtenerInformacionTrimsHangtags(idSummary).ToList();
                Listatrims.AddRange(Lista_Hangtags);
                leerF.Close();
            }
            finally
            {
                conexion.CerrarConexion();
                conexion.Dispose();
            }

            return Listatrims;

        }

        //Obtener Información de HANGTAGS  por IdSummary
        public IEnumerable<Trim_requests> ObtenerInformacionTrimsHangtags(int? idSummary)
        {
            Conexion conexion = new Conexion();
            List<Trim_requests> ListatrimsHangtags = new List<Trim_requests>();
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;
                com.Connection = conexion.AbrirConexion();
                com.CommandText = "SELECT TR.id_request, IC.descripcion, IC.fabric_type, TR.id_size, TR.total, TR.restante FROM trim_requests TR " +
                    "INNER JOIN items_catalogue IC ON IC.item_id=TR.id_item " +
                    "INNER JOIN trim_estilos TE ON TE.id_request=TR.id_request " +
                    "WHERE TE.id_summary= '" + idSummary + "' AND  IC.fabric_type ='HANGTAGS'";
                com.CommandType = CommandType.Text;
                leerF = com.ExecuteReader();
                while (leerF.Read())
                {
                    Trim_requests trims = new Trim_requests()
                    {
                        estado = "1",
                        id_request = Convert.ToInt32(leerF["id_request"]),
                        Descripcion = leerF["descripcion"].ToString(),
                        id_talla = Convert.ToInt32(leerF["id_size"]),
                        total = Convert.ToInt32(leerF["total"]),
                        restante = Convert.ToInt32(leerF["restante"]),
                        tipo_item = leerF["fabric_type"].ToString()
                    };
                    ListatrimsHangtags.Add(trims);
                }
                leerF.Close();
            }
            finally
            {
                conexion.CerrarConexion();
                conexion.Dispose();
            }

            return ListatrimsHangtags;

        }

        //Obtener Información de price tickets trims por IdSummary
        public IEnumerable<InfoPriceTickets> ObtenerInformacionPriceTicketsTrims(int? idPedido)
        {
            Conexion conexion = new Conexion();
            List<InfoPriceTickets> Listatrims = new List<InfoPriceTickets>();
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;
                com.Connection = conexion.AbrirConexion();
                com.CommandText = "SELECT TR.id_request, IC.descripcion, IC.fabric_type, TR.id_size, TR.total, TR.restante FROM trim_requests TR " +
                    "INNER JOIN items_catalogue IC ON IC.item_id=TR.id_item " +
                    "WHERE TR.id_pedido= '" + idPedido + "' AND (IC.fabric_type='PRICE TICKETS' OR IC.fabric_type='PRICE TICKET') ";
                com.CommandType = CommandType.Text;
                leerF = com.ExecuteReader();
                while (leerF.Read())
                {
                    InfoPriceTickets trims = new InfoPriceTickets()
                    {
                        Estado = "1",
                        Id_request_pt = Convert.ToInt32(leerF["id_request"]),
                        Descripcion = leerF["descripcion"].ToString(),
                        Id_talla = Convert.ToInt32(leerF["id_size"]),
                        Total = Convert.ToInt32(leerF["total"]),
                        Restante = Convert.ToInt32(leerF["restante"]),
                        Tipo_item = leerF["fabric_type"].ToString()
                    };
                    Listatrims.Add(trims);
                }
                leerF.Close();
            }
            finally
            {
                conexion.CerrarConexion();
                conexion.Dispose();
            }

            return Listatrims;

        }

        //Obtener el último recibo de trims por IdSummary
        public int ObtenerUltimoReciboTrims(int? IdSummary)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            int reciboTrims = 0;
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "SELECT TOP 1 TR.id_recibo FROM trim_requests TR " +
                    "INNER JOIN items_catalogue IC ON IC.item_id=TR.id_item " +
                    "WHERE TR.id_pedido='" + IdSummary + "'  and IC.fabric_type !='PRICE TICKETS' AND IC.fabric_type !='PRICE TICKET' ";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    reciboTrims = Convert.ToInt32(reader["id_recibo"]);
                }
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return reciboTrims;
        }

        //Obtener el último recibo de price tickets trims por IdSummary
        public int ObtenerUltimoReciboPriceTicketsTrims(int? IdSummary)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            int reciboTrims = 0;
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "SELECT TOP 1 TR.id_recibo FROM trim_requests TR " +
                    "INNER JOIN items_catalogue IC ON IC.item_id=TR.id_item " +
                    "WHERE TR.id_pedido='" + IdSummary + "'  and IC.fabric_type ='PRICE TICKETS' AND IC.fabric_type ='PRICE TICKET' ";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    reciboTrims = Convert.ToInt32(reader["id_recibo"]);
                }
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return reciboTrims;
        }

        //Obtener la fecha del ÚLTIMO RECIBO POR ID_RECIBO
        public DateTime ObtenerFechaUltimoReciboTrims(int? IdRecibo)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            DateTime fechaRecibo = new DateTime();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "SELECT fecha FROM recibos WHERE id_recibo= '" + IdRecibo + "' ";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    fechaRecibo = Convert.ToDateTime(reader["fecha"]);
                }
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return fechaRecibo;
        }

        //Obtener la fecha del ultimo trims
        public DateTime ObtenerFechaUltimoTrims(int? IdPedido)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            DateTime fechaTrims = new DateTime();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "SELECT TOP 1 TR.id_recibo, tr.fecha FROM trim_requests TR  WHERE TR.id_pedido= '" + IdPedido + "' ";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    fechaTrims = Convert.ToDateTime(reader["fecha"]);
                }
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return fechaTrims;
        }

        //Obtener la fecha de la instruccion de packing
        public InfoPackInstruction ObtenerFechaPackingInst(int? IdPedido)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            InfoPackInstruction instruccion = new InfoPackInstruction();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "SELECT id_instruccion, estado, fecha FROM instrucciones_empaque WHERE id_pedido= '" + IdPedido + "' ";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    instruccion.IdInstructionPack = Convert.ToInt32(reader["id_instruccion"]);
                    instruccion.Fecha_Rec_Pack = Convert.ToDateTime(reader["fecha"]);
                    instruccion.EstadoPack = Convert.ToInt32(reader["estado"]);

                }
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return instruccion;
        }

        public void RegistroMillPO(InfoMillPO datosMPO)
        {
            Conexion conn = new Conexion();
            try
            {
                SqlCommand comando = new SqlCommand
                {
                    Connection = conn.AbrirConexion(),
                    CommandText = "INSERT INTO  MILLPO_LIST (MILLPO, ID_PEDIDO) " +
                    " VALUES('" + datosMPO.MillPO.ToUpper() + "','" + datosMPO.IdPedido + "')"
                };
                comando.ExecuteNonQuery();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }
        }

        //Permite eliminar la informacion de un millpo por id
        public void EliminarMillPO(int id)
        {

            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "DELETE FROM MILLPO_LIST WHERE ID_MILLPO='" + id + "' ";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                conex.CerrarConexion();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

        }


        //Permite eliminar la informacion de un tipo packing del estilo
        public void EliminarPackEstilo(int id)
        {

            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "DELETE FROM CAT_TYPE_PACK_STYLE WHERE ID_PACK_STYLE='" + id + "' ";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                conex.CerrarConexion();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

        }


        //Muestra la lista periodos de la tabla Pedidos
        public IEnumerable<Periodo> ListadoPeriodos()
        {
            Conexion conn = new Conexion();
            List<Periodo> listPeriodo = new List<Periodo>();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select DISTINCT YEAR(DATE_CANCEL) AS PERIODO from pedido ";
                comando.CommandType = CommandType.Text;
                leer = comando.ExecuteReader();

                while (leer.Read())
                {
                    Periodo year = new Periodo()
                    {
                        NumPeriodo = Convert.ToInt32(leer["PERIODO"])
                    };

                    listPeriodo.Add(year);

                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

            return listPeriodo;
        }

        public List<Estilo_PO> Obtener_pedidos_po_estilo(int? estilo)
        {
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<Estilo_PO> lista = new List<Estilo_PO>();
            Conexion con = new Conexion();
            try
            {//Regex.Replace(, @"\s+", " ");
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT ID_PO_SUMMARY,ID_COLOR,QTY,ID_PEDIDOS FROM PO_SUMMARY WHERE ITEM_ID='" + estilo + "' ";
                leer = com.ExecuteReader();
                while (leer.Read())
                {
                    Estilo_PO epo = new Estilo_PO
                    {
                        id_pedido = Convert.ToInt32(leer["ID_PEDIDOS"])
                    };
                    epo.pedido = Regex.Replace(consultas.obtener_po_id((epo.id_pedido).ToString()), @"\s+", " ");
                    epo.id_summary = Convert.ToInt32(leer["ID_PO_SUMMARY"]);
                    epo.id_color = Convert.ToInt32(leer["ID_COLOR"]);
                    epo.total = Convert.ToInt32(leer["QTY"]);
                    epo.id_estilo = Convert.ToInt32(estilo);
                    epo.color = Regex.Replace(consultas.obtener_color_id((epo.id_color).ToString()), @"\s+", " ");
                    epo.estilo = Regex.Replace(consultas.obtener_estilo(epo.id_estilo), @"\s+", " ");
                    epo.descripcion = Regex.Replace(consultas.buscar_descripcion_estilo(epo.id_estilo), @"\s+", " ");
                    int enviadas = Buscar_totales_enviadas_summary(epo.id_summary);
                    if (enviadas >= epo.total)
                    {
                        epo.estado = "COMPLETE";
                    }
                    else
                    {
                        epo.estado = "INCOMPLETE";
                    }
                    epo.total -= enviadas;
                    lista.Add(epo);
                }
                leer.Close();
            }
            finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }

        public int Buscar_totales_enviadas_summary(int summary)
        {
            int id = 0;
            Conexion con_u_r = new Conexion();
            try
            {
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT total FROM totales_envios where id_summary='" + summary + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read())
                {
                    id += Convert.ToInt32(leer_u_r["total"]);
                }
                leer_u_r.Close();
            }
            finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return id;
        }

    }
}