using ClosedXML.Excel;
using FortuneSystem.Models.Almacen;
using FortuneSystem.Models.Catalogos;
using FortuneSystem.Models.Item;
using FortuneSystem.Models.Items;
using FortuneSystem.Models.Packing;
using FortuneSystem.Models.Pedidos;
using FortuneSystem.Models.PNL;
using FortuneSystem.Models.POSummary;
using FortuneSystem.Models.PrintShop;
using FortuneSystem.Models.Revisiones;
using FortuneSystem.Models.Shipping;
using FortuneSystem.Models.Staging;
using FortuneSystem.Models.Trims;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FortuneSystem.Controllers
{
    public class PedidosController : Controller
    {
        // GET: Pedido		
        readonly CatStatusData objEstados = new CatStatusData();
        readonly CatGeneroData objGenero = new CatGeneroData();
        readonly CatColoresData objColores = new CatColoresData();
        readonly DescripcionItemData objItems = new DescripcionItemData();
        readonly CatTelaData objTela = new CatTelaData();
        readonly CatTipoCamisetaData objTipoC = new CatTipoCamisetaData();
        readonly ItemTallaData objTallas = new ItemTallaData();
        readonly RevisionesData objRevision = new RevisionesData();
        readonly ItemDescripcionData objEst = new ItemDescripcionData();
        readonly PrintShopData objPrint = new PrintShopData();
        readonly PackingData objPacking = new PackingData();
        readonly PnlData objPnl = new PnlData();
        readonly CatEspecialidadesData objEspecialidad = new CatEspecialidadesData();
        readonly CatTipoOrdenData objTipoOrden = new CatTipoOrdenData();
        readonly ReportController reporte = new ReportController();
        readonly CatTypeFormPackData objFormaPacking = new CatTypeFormPackData();

        public int estado;
        public int IdPO;
        public int pedidos;

        public ActionResult Index()
        {
            PedidosData objPedido = new PedidosData();
            int cargo = Convert.ToInt32(Session["idCargo"]);
            List<OrdenesCompra> listaPedidos;
            if (cargo != 0)
            {
                listaPedidos = objPedido.ListaOrdenCompra().ToList();
            }
            else
            {
                Session["idCargo"] = 0;
                listaPedidos = objPedido.ListaOrdenCompra().ToList();
            }

            return View(listaPedidos);
        }

        public void Reporte(int? id)
        {
            Session["idPed"] = id;
            reporte.Imprimir_Reporte_PO();
        }

        [HttpPost]
        public JsonResult Imprimir_Reporte_PO(int id, string po)
        {
            //pedido        
            Session["idPed"] = id;
            Session["nombrePO"] = po;

            return Json("0", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Estilos_PO(int? id)
        {

            List<POSummary> listaItems = objItems.ListaItemsPorPO(id).ToList();
            int cargo = Convert.ToInt32(Session["idCargo"]);
            int numEmpleado = Convert.ToInt32(Session["id_Empleado"]); 
            var result = Json(new { listaItem = listaItems, cargoUser = cargo, nEmpleado= numEmpleado });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Datos_Estilos_PO(int? id)
        {
            List<POSummary> listaItems = objItems.ListaItemsPorPO(id).ToList();
            int cargo = Convert.ToInt32(Session["idCargo"]);
            int numEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            CatClienteFinalData objClienteFinal = new CatClienteFinalData();
            CatClienteData objCliente = new CatClienteData();
            PedidosData objPedido = new PedidosData();
            OrdenesCompra pedido = objPedido.ConsultarListaPO(id);
            pedido.CatCliente = objCliente.ConsultarListaClientes(pedido.Cliente);
            pedido.CatClienteFinal = objClienteFinal.ConsultarListaClientesFinal(pedido.ClienteFinal);
            var result = Json(new { listaItem = listaItems, cargoUser = cargo, nombreCliente = pedido.CatCliente, nombreClienteFinal = pedido.CatClienteFinal, nomPedido = pedido.PO, nomVPO = pedido.VPO, nEmpleado = numEmpleado });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [ChildActionOnly]
        public ActionResult Listado_Estilos_PO()
        {
            int? id = Convert.ToInt32(Convert.ToInt32(Session["idPedidoNuevo"])); //id_pedido			
            List<POSummary> listaItems = objItems.ListaItemsPorPO(id).ToList();

            return PartialView(listaItems);
        }

        [ChildActionOnly]
        public ActionResult Listado_Tallas_Estilo(int? id)
        {
            List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstilo(id).ToList();

            return PartialView(listaTallas);
        }

        public ActionResult HistorialPedidos(int id)
        {
            PedidosData objPedido = new PedidosData();
            List<OrdenesCompra> listaPedidosRev = objPedido.ListaRevisionesPO(id).ToList();

            return View(listaPedidosRev);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Estilo(int? id)
        {
            List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstilo(id).ToList();
            List<StagingD> listaTallasStaging = objTallas.ListaTallasStagingPorEstilo(id).ToList();
            List<int> listaTallasTBatch = objPrint.ListaTotalTallasBatchEstilo(id).ToList();
            List<int> listaTallasPBatch = new List<int>();
            List<int> listaTallasMPBatch = new List<int>();
            List<int> listaTallasDBatch = new List<int>();
            List<int> listaTallasRBatch = new List<int>();
            if (listaTallasTBatch.Count != 0)
            {
                listaTallasPBatch = objPrint.ListaTotalPrintedTallasBatchEstilo(id).ToList();
                listaTallasMPBatch = objPrint.ListaTotalMPTallasBatchEstilo(id).ToList();
                listaTallasDBatch = objPrint.ListaTotalDefTallasBatchEstilo(id).ToList();
                listaTallasRBatch = objPrint.ListaTotalRepTallasBatchEstilo(id).ToList();
            }

            string estilo = "";
            foreach (var item in listaTallas)
            {
                estilo = item.Estilo;

            }
            var result = Json(new
            {
                listaTalla = listaTallas,
                estilos = estilo,
                listTallaStaging = listaTallasStaging,
                listaTallasTotalBatch = listaTallasTBatch,
                listaTallasTotalPBatch = listaTallasPBatch,
                listaTallasTotalMBatch = listaTallasMPBatch,
                listaTallasTotalDBatch = listaTallasDBatch,
                listaTallasTotalRBatch = listaTallasRBatch
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Listado_Tallas_Estilos(int? id)
        {
            int numEstilo = objEst.ObtenerIdEstiloPorsummary(id);
            string nomEstilo = objEst.ObtenerEstiloPorId(numEstilo);
            List<ItemTalla> listaTallas = objTallas.ListadoTallasDetallesPorEstilos(id).ToList();
            List<StagingD> listaTallasStaging = objTallas.ListaTallasStagingPorEstilo(id).ToList();
            List<int> listaTallasTBatch = objPrint.ListaTotalTallasBatchEstilo(id).ToList();
            List<int> listaTallasPBatch = new List<int>();
            List<int> listaTallasMPBatch = new List<int>();
            List<int> listaTallasDBatch = new List<int>();
            List<int> listaTallasRBatch = new List<int>();
            if (listaTallasTBatch.Count != 0)
            {
                listaTallasPBatch = objPrint.ListaTotalPrintedTallasBatchEstilo(id).ToList();
                listaTallasMPBatch = objPrint.ListaTotalMPTallasBatchEstilo(id).ToList();
                listaTallasDBatch = objPrint.ListaTotalDefTallasBatchEstilo(id).ToList();
                listaTallasRBatch = objPrint.ListaTotalRepTallasBatchEstilo(id).ToList();
            }

            string estilo = objEst.ObtenerNombreEstiloId(nomEstilo);
            /* foreach (var item in listaTallas)
             {
                 estilo = item.Estilo;

             }*/
            var result = Json(new
            {
                listaTalla = listaTallas,
                estilos = estilo,
                listTallaStaging = listaTallasStaging,
                listaTallasTotalBatch = listaTallasTBatch,
                listaTallasTotalPBatch = listaTallasPBatch,
                listaTallasTotalMBatch = listaTallasMPBatch,
                listaTallasTotalDBatch = listaTallasDBatch,
                listaTallasTotalRBatch = listaTallasRBatch
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Estilo_PrintShop(int? id/*, int numQtyPrint*/)
        {
            List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstiloPrint(id).ToList();
            //List<ItemTalla> listaTallas = objTallas.ListadoTallasPorEstilos(id).ToList();
            List<ItemTalla> listaTallasQty = objTallas.ListadoTallasPorEstilos(id).ToList();
            List<StagingD> listaTallasStaging = objTallas.ListaTallasStagingPorEstilo(id).ToList();
            List<StagingDatos> listaDatosStaging = objTallas.ListaTallasStagingDatosPorEstilo(id).ToList();
            List<int> listaTallasTBatch = objPrint.ListaTotalTallasBatchEstilo(id).ToList();
            List<int> listaTallasPBatch = new List<int>();
            List<int> listaTallasMPBatch = new List<int>();
            List<int> listaTallasDBatch = new List<int>();
            List<int> listaTallasRBatch = new List<int>();
            if (listaTallasTBatch.Count != 0)
            {
                listaTallasPBatch = objPrint.ListaTotalPrintedTallasBatchEstilo(id).ToList();
                listaTallasMPBatch = objPrint.ListaTotalMPTallasBatchEstilo(id).ToList();
                listaTallasDBatch = objPrint.ListaTotalDefTallasBatchEstilo(id).ToList();
                listaTallasRBatch = objPrint.ListaTotalRepTallasBatchEstilo(id).ToList();
            }
            List<int> listaTallasTBatchPnl = objPnl.ListaTotalTallasBatchEstiloPNL(id).ToList();
            string estilo = "";
            foreach (var item in listaTallas)
            {
                estilo = item.Estilo;

            }
            var result = Json(new
            {
                listaTalla = listaTallas,
                estilos = estilo,
                listTallaStaging = listaTallasStaging,
                listDatosStaging = listaDatosStaging,
                listaTallasTotalBatch = listaTallasTBatch,
                listaTallasTotalPBatch = listaTallasPBatch,
                listaTallasTotalMBatch = listaTallasMPBatch,
                listaTallasTotalDBatch = listaTallasDBatch,
                listaTallasTotalRBatch = listaTallasRBatch,
                listaTallasTotalBatchPnl = listaTallasTBatchPnl,
                listTallaCant = listaTallasQty
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Listado_Tallas_Estilo_Print(int? id)
        {
            List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstiloPrint(id).ToList();

            var result = Json(new
            {
                listaTalla = listaTallas

            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Info_PrintShop_Grafica(int? id)
        {
            List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstiloPrint(id).ToList();
            List<PrintShopC> listaTallasTBatch = objPrint.ListaTotalTallasBatch(id).ToList();

            foreach (var item in listaTallas)
            {
                int CantidadBatch = 0;
                double CantidadGeneral = 0;
                double Cantidadfinal = 0;
                foreach (var itemBatch in listaTallasTBatch)
                {
                    if (item.IdTalla == itemBatch.IdTalla)
                    {
                        CantidadBatch = itemBatch.TotalBatch;
                        //CantidadPrint = item.Cantidad - CantidadBatch;
                        CantidadGeneral = (CantidadBatch * 100);
                        Cantidadfinal = Math.Round(CantidadGeneral / item.Cantidad, 2);
                    }

                }
                //item.Total = CantidadGeneral;
                item.Porcentaje = Cantidadfinal;

            }
            var result = Json(new
            {
                listaTalla = listaTallas,
                listaTallasTotalBatch = listaTallasTBatch

            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult Lista_Tallas_Estilo_Recibos(int? idSummary/*, int? idPedido, int? idEstilo*/)
        {
            PedidosData objPedido = new PedidosData();
            List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstiloRecibo(idSummary).ToList();
            List<recibo> listaTallasRecibo = objPedido.ListaRecibosTotales(idSummary).ToList();
            List<recibo> listaTRecibo = objPedido.ListaRecibos(idSummary).ToList();
            string estilo = "";
            foreach (var item in listaTallas)
            {
                estilo = item.Estilo;

            }
            var result = Json(new
            {
                listaTalla = listaTallas,
                estilos = estilo,
                listaRecibos = listaTallasRecibo,
                listadoRecibo = listaTRecibo
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        //Trims
        [HttpPost]
        public JsonResult Lista_Trims(int? idSummary, int? idPedido)
        {
            PedidosData objPedido = new PedidosData();
            List<Trim_requests> listadoTrims = objPedido.ObtenerInformacionTrims(idPedido, idSummary).ToList();

            foreach (var item in listadoTrims)
            {
                string DatoTalla = objTallas.ObtenerTallasPorId(item.id_talla);
                if (DatoTalla != "")
                {
                    item.talla = DatoTalla;
                }
                else
                {
                    item.talla = "0";
                }
            }

            var result = Json(new
            {
                listaTrims = listadoTrims

            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Lista_Price_Tickets_Trims(int? idPedido)
        {
            PedidosData objPedido = new PedidosData();
            List<InfoPriceTickets> listadoTrims = objPedido.ObtenerInformacionPriceTicketsTrims(idPedido).ToList();

            foreach (var item in listadoTrims)
            {
                string DatoTalla = objTallas.ObtenerTallasPorId(item.Id_talla);
                if (DatoTalla != "")
                {
                    item.Talla = DatoTalla;
                }
                else
                {
                    item.Talla = "0";
                }
            }

            var result = Json(new
            {
                listaTrims = listadoTrims

            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Lista_Tallas_Estilo_Det(int? id)
        {
            List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstiloRev(id).ToList();
            List<StagingD> listaTallasStaging = objTallas.ListaTallasStagingPorEstilo(id).ToList();
            List<int> listaTallasTBatch = objPrint.ListaTotalTallasBatchEstilo(id).ToList();
            List<int> listaTallasPBatch = new List<int>();
            List<int> listaTallasMPBatch = new List<int>();
            List<int> listaTallasDBatch = new List<int>();
            List<int> listaTallasRBatch = new List<int>();
            if (listaTallasTBatch.Count != 0)
            {
                listaTallasPBatch = objPrint.ListaTotalPrintedTallasBatchEstilo(id).ToList();
                listaTallasMPBatch = objPrint.ListaTotalMPTallasBatchEstilo(id).ToList();
                listaTallasDBatch = objPrint.ListaTotalDefTallasBatchEstilo(id).ToList();
                listaTallasRBatch = objPrint.ListaTotalRepTallasBatchEstilo(id).ToList();
            }

            string estilo = "";
            foreach (var item in listaTallas)
            {
                estilo = item.Estilo;

            }
            var result = Json(new
            {
                listaTalla = listaTallas,
                estilos = estilo,
                listTallaStaging = listaTallasStaging,
                listaTallasTotalBatch = listaTallasTBatch,
                listaTallasTotalPBatch = listaTallasPBatch,
                listaTallasTotalMBatch = listaTallasMPBatch,
                listaTallasTotalDBatch = listaTallasDBatch,
                listaTallasTotalRBatch = listaTallasRBatch
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Estilo_Pnl(int? id)
        {
            List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstilopnl(id).ToList();
            List<ItemTalla> listaTallasQty = objTallas.ListadoTallasPorEstilos(id).ToList();
            List<StagingD> listaTallasStaging = objTallas.ListaTallasStagingPorEstilo(id).ToList();
            List<StagingDatos> listaDatosStaging = objTallas.ListaTallasStagingDatosPorEstilo(id).ToList();
            List<int> listaTallasTPnlBatch = objPnl.ListaTotalTallasPNLBatchEstilo(id).ToList();
            List<int> listaTallasPBatchPnl = new List<int>();
            List<int> listaTallasMPBatchPnl = new List<int>();
            List<int> listaTallasDBatchPnl = new List<int>();
            List<int> listaTallasRBatch = new List<int>();
            if (listaTallasTPnlBatch.Count != 0)
            {
                listaTallasPBatchPnl = objPnl.ListaTotalPrintedTallasBatchEstilo(id).ToList();
                listaTallasMPBatchPnl = objPnl.ListaTotalMPTallasBatchEstilo(id).ToList();
                listaTallasDBatchPnl = objPnl.ListaTotalDefTallasBatchEstilo(id).ToList();
                listaTallasRBatch = objPnl.ListaTotalRepTallasBatchEstilo(id).ToList();
            }
            List<int> listaTallasTBatch = objPrint.ListaTotalTallasBatchEstilo(id).ToList();
            string estilo = "";
            string color = "";
            foreach (var item in listaTallas)
            {
                estilo = item.Estilo;
                color = item.Color;

            }
            var result = Json(new
            {
                listaTalla = listaTallas,
                estilos = estilo,
                colores = color,
                listTallaStaging = listaTallasStaging,
                listDatosStaging = listaDatosStaging,
                listaTallasTotalPnlBatch = listaTallasTPnlBatch,
                listaTallasTotalBatch = listaTallasTBatch,
                listaTallasTotalPBatchPNL = listaTallasPBatchPnl,
                listaTallasTotalMBatchPnl = listaTallasMPBatchPnl,
                listaTallasTotalDBatchPnl = listaTallasDBatchPnl,
                listaTallasTotalRBatch = listaTallasRBatch,
                listTallaCant = listaTallasQty
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ObtenerIdPedidoPorNombre(string estilo)
        {
            PedidosData objPedido = new PedidosData();
            int pedido = objPedido.Obtener_Id_Pedido_Nombre(estilo);


            var result = Json(new
            {
                numPedido = pedido

            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Info_Pnl_Grafica(int? id)
        {
            List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstilopnl(id).ToList();
            List<Pnl> listaTallasTPnlBatch = objPnl.ListaTotalTallasPNLBatch(id).ToList();

            foreach (var item in listaTallas)
            {
                int CantidadBatch = 0;
                double CantidadGeneral = 0;
                double Cantidadfinal = 0;
                foreach (var itemBatch in listaTallasTPnlBatch)
                {
                    if (item.IdTalla == itemBatch.IdTalla)
                    {
                        CantidadBatch = itemBatch.TotalBatch;
                        //CantidadPrint = item.Cantidad - CantidadBatch;
                        CantidadGeneral = (CantidadBatch * 100);
                        Cantidadfinal = Math.Round(CantidadGeneral / item.Cantidad, 2);
                    }

                }
                //item.Total = CantidadGeneral;
                item.Porcentaje = Cantidadfinal;

            }
            var result = Json(new
            {
                listaTalla = listaTallas,
                listaTallasTotalBatch = listaTallasTPnlBatch

            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Lista_Tallas_Estilo_Packing(int? id)
        {
            List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstilo(id).ToList();
            List<StagingD> listaTallasStaging = objTallas.ListaTallasStagingPorEstilo(id).ToList();
            List<int> listaTallasTPrintShopBatch = objPrint.ListaTotalTallasBatchEstilo(id).ToList();
            List<int> listaTallasTPnlBatch = objPnl.ListaTotalTallasPNLBatchEstilo(id).ToList();
            List<int> listaTallasTPackingBatch = objPacking.ListaTotalTallasPackingBatchEstilo(id).ToList();
            List<int> listaTallasPBatchPacking = new List<int>();
            List<int> listaTallasEBatchPacking = new List<int>();
            List<int> listaTallasDBatchPacking = new List<int>();
            if (listaTallasTPackingBatch.Count != 0)
            {
                listaTallasPBatchPacking = objPacking.ListaTotalCajasTallasBatchEstilo(id).ToList();
                listaTallasEBatchPacking = objPacking.ListaTotalETallasBatchEstilo(id).ToList();
                listaTallasDBatchPacking = objPacking.ListaTotalDefTallasBatchEstilo(id).ToList();
            }

            string estilo = "";
            foreach (var item in listaTallas)
            {
                estilo = item.Estilo;

            }
            var result = Json(new
            {
                listaTalla = listaTallas,
                estilos = estilo,
                listTallaStaging = listaTallasStaging,
                listaTallasTotalPnlBatch = listaTallasTPnlBatch,
                listaTallasTotalBatch = listaTallasTPrintShopBatch,
                listaTallasTotalPackingBatch = listaTallasTPackingBatch,
                listaTallasTotalPBatchPacking = listaTallasPBatchPacking,
                listaTallasTotalEBatchPacking = listaTallasEBatchPacking,
                listaTallasTotalDBatchPacking = listaTallasDBatchPacking
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
                          
        [HttpPost]
        public JsonResult Lista_Tallas_Staging_Estilo(int? id)
        {
            List<StagingD> listaTallas = objTallas.ListaTallasStagingPorEstilo(id).ToList();
            /*string estilo = "";
            foreach (var item in listaTallas)
            {
                estilo = item.Estilo;

            }*/
            var result = Json(new { listaTalla = listaTallas/*, estilos = estilo*/ });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_PrintShop_Estilo(int? id)
        {
            List<PrintShopC> listaTallas = objPrint.ListaTallasPrintShop(id).ToList();
            List<PrintShopC> listaTallasEstilo = objPrint.ObtenerTallas(id).ToList();
            List<int> listaTallasTBatch = objPrint.ListaTotalTallasBatchEstilo(id).ToList();
            var result = Json(new { listaTalla = listaTallas, listaEstiloTallas = listaTallasEstilo, listaPrint = listaTallasTBatch });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Pnl_Estilo(int? id)
        {
            List<Pnl> listaTallas = objPnl.ListaTallasPnl(id).ToList();
            List<Pnl> listaTallasEstilo = objPnl.ObtenerTallas(id).ToList();
            List<int> listaTallasTBatch = objPnl.ListaTotalTallasPNLBatchEstilo(id).ToList();
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            var result = Json(new { listaTalla = listaTallas, listaEstiloTallas = listaTallasEstilo, listaPrint = listaTallasTBatch, empleado = noEmpleado });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Packing_Estilo(int? id)
        {
            List<PackingM> listaTallas = objPacking.ListaTallasPacking(id).ToList();
            List<PackingM> listaTallasEstilo = objPacking.ObtenerTallas(id).ToList();
            List<int> listaTallasTBatch = objPacking.ListaTotalTallasPackingBatchEstilo(id).ToList();
            var result = Json(new { listaTalla = listaTallas, listaEstiloTallas = listaTallasEstilo, listaPrint = listaTallasTBatch });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Total_PrintShop_Estilo(int? id)
        {
            List<PrintShopC> listaTallas = objPrint.ListaTallasTotalPrintShop(id).ToList();
            /*string estilo = "";
            foreach (var item in listaTallas)
            {
                estilo = item.Estilo;

            }*/
            var result = Json(new { listaTalla = listaTallas/*, estilos = estilo*/ });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult CrearPO()
        {
            OrdenesCompra pedido = new OrdenesCompra();
            POSummary summary = new POSummary();
            ListasClientes(pedido);
            ListaEstados(pedido);
            ListaGenero(summary);
            ListaTela(summary);
            ListaTipoCamiseta(summary);
            ListaEspecialidades(summary);
            ListaTipoOrden(pedido);
            ListaTipoFormaPacking(summary);
            return View();
        }

        public void Screnn()
        {
            Process snippingToolProcess = new Process
            {
                EnableRaisingEvents = true
            };
            if (!Environment.Is64BitProcess)
            {
                snippingToolProcess.StartInfo.FileName = "C:\\Windows\\sysnative\\SnippingTool.exe";
                snippingToolProcess.Start();
            }
            else
            {
                snippingToolProcess.StartInfo.FileName = "C:\\Windows\\system32\\SnippingTool.exe";
                snippingToolProcess.Start();
            }
            /*Process SnippingTool = new Process();
			String FilePath = @"C:\WINDOWS\system32\SnippingTool.exe";
			SnippingTool.StartInfo.FileName = System.IO.Path.GetDirectoryName(FilePath);
			SnippingTool.StartInfo.Arguments = "SnippingTool.exe";
			SnippingTool.Start();*/

        }
        [HttpPost]
        public JsonResult RegistrarPO([Bind] OrdenesCompra ordenCompra,/* string po, string VPO, DateTime FechaCancel, DateTime FechaOrden, int Cliente, int Clientefinal, int TotalUnidades, int IdTipoOrden,*/ List<string> ListaMillPO)
        {
            PedidosData objPedido = new PedidosData();
            ListaEstados(ordenCompra);
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            ordenCompra.Usuario = noEmpleado;
            Session["cliente"] = ordenCompra.Cliente;
            List<OrdenesCompra> listaPedidos = objPedido.ListaOrdenCompra().ToList();
            OrdenesCompra result = listaPedidos.Find(x => x.PO.TrimEnd() == ordenCompra.PO);
            int duplicado = 0;
            if (result == null)
            {
                objPedido.AgregarPO(ordenCompra);
                Session["idPedido"] = objPedido.Obtener_Utlimo_po();
                this.Obtener_Lista_MillPO(ListaMillPO);
                duplicado = 2;
            }
            else
            {
                duplicado = 1;
                //TempData["duplicadoPO"] = "There is already a purchase order with that PO ("+ ordenCompra.PO + ")." ;
            }
            var resultado = Json(new { doblePO = duplicado });
            return Json(resultado, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Obtener_Lista_MillPO(List<string> ListaMillPO)
        {
            PedidosData objPedido = new PedidosData();
            InfoMillPO datos = new InfoMillPO();
            List<string> descMPO = ListaMillPO[0].Split('*').ToList();
            int i = 0;
            foreach (var item in descMPO)
            {
                i++;
            }

            i -= 1;
            for (int v = 0; v < i; v++)
            {

                datos.MillPO = descMPO[v];
                int IdPedido = Convert.ToInt32(Session["idPedido"]);
                datos.IdPedido = IdPedido;
                objPedido.RegistroMillPO(datos);

            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearPO([Bind] OrdenesCompra ordenCompra)
        {
            if (ModelState.IsValid)
            {
                ObtenerIdClientes(ordenCompra);
                ListaEstados(ordenCompra);
                //objPedido.AgregarPO(pedido);
            }

            return View();
        }

        [HttpGet]
        public ActionResult Detalles(int? id)
        {
            if (id == null)
            {
                return View();
            }
            CatClienteFinalData objClienteFinal = new CatClienteFinalData();
            CatClienteData objCliente = new CatClienteData();
            PedidosData objPedido = new PedidosData();
            OrdenesCompra pedido = objPedido.ConsultarListaPO(id);
            pedido.CatCliente = objCliente.ConsultarListaClientes(pedido.Cliente);
            pedido.CatClienteFinal = objClienteFinal.ConsultarListaClientesFinal(pedido.ClienteFinal);
            pedido.IdPedido = Convert.ToInt32(id);

            if (pedido == null)
            {
                return View();
            }
            return View(pedido);
        }

        [HttpGet]
        public ActionResult DetallesRev(int? id)
        {
            if (id == null)
            {
                return View();
            }
            CatClienteFinalData objClienteFinal = new CatClienteFinalData();
            CatClienteData objCliente = new CatClienteData();
            PedidosData objPedido = new PedidosData();
            OrdenesCompra pedido = objPedido.ConsultarListaPO(id);
            pedido.CatCliente = objCliente.ConsultarListaClientes(pedido.Cliente);
            pedido.CatClienteFinal = objClienteFinal.ConsultarListaClientesFinal(pedido.ClienteFinal);
            pedido.IdPedido = Convert.ToInt32(id);

            if (pedido == null)
            {
                return View();
            }
            return View(pedido);
        }

        [HttpGet]
        public int ObtenerPORevision(int? id)
        {
            PedidosData objPedido = new PedidosData();
            OrdenesCompra pedido = objPedido.ConsultarListaPO(id);
            SeleccionarClientes(pedido);
            SeleccionarClienteFinal(pedido);
            /*int revisiones = objRevision.ObtenerNumeroRevisiones(id);
            int identificador = 0;
            string rev;
            if (revisiones == 0 && pedido.IdStatus != 3)
            {
                identificador = revisiones + 1;
                rev = pedido.PO + "-REV" + identificador;
            }
            else
            {
                identificador = revisiones + 1;
                rev = pedido.PO + "-REV" + identificador;
            }pedido.PO = rev.Replace(" ", "");*/


            // pedido.IdPedido = Convert.ToInt32(id);
            pedido.FechaOrden = DateTime.Today;
            Session["id_pedido"] = id;

            ObtenerEstadoRevisado(pedido);

            objPedido.AgregarPO(pedido);
            Session["idPedidoNuevo"] = objPedido.Obtener_Utlimo_po();
            //Cambia estado pedido original a 5
            objPedido.ActualizarEstadoPO(Convert.ToInt32(Session["id_pedido"]));

            //Registrar en Revisado el Pedido Nuevo 
            //int PedidosId = objPedido.Obtener_Utlimo_po();
            //Session["idPedidoNuevo"] = PedidosId;
            int PedidoNuevo = Convert.ToInt32(Session["idPedidoNuevo"]);
            if (PedidoNuevo != 0)
            {
                Revision revisionPO = new Revision()
                {
                    IdPedido = Convert.ToInt32(Session["id_pedido"]),
                    IdRevisionPO = Convert.ToInt32(Session["idPedidoNuevo"]),
                    FechaRevision = DateTime.Today,
                    IdStatus = pedido.IdStatus

                };
                objRevision.AgregarRevisionesPO(revisionPO);
            }
            //Obtener los estilos por ID Pedido Anterior
            List<POSummary> listaItems = objItems.ListaEstilosPorPO(Convert.ToInt32(Session["id_pedido"])).ToList();
            POSummary estilos = new POSummary();
            foreach (var item in listaItems)
            {
                estilos.EstiloItem = item.EstiloItem;
                estilos.IdColor = item.CatColores.CodigoColor;
                estilos.Cantidad = item.Cantidad;
                estilos.Precio = item.Precio;
                estilos.PedidosId = item.PedidosId;
                estilos.IdGenero = item.CatGenero.GeneroCode;
                estilos.IdTela = item.IdTela;
                estilos.TipoCamiseta = item.CatTipoCamiseta.TipoProducto;
                estilos.IdItems = item.IdItems;
                estilos.IdEspecialidad = item.CatEspecialidades.IdEspecialidad;
                estilos.IdEstado = item.IdEstado;
                Session["id_estilo"] = estilos.IdItems;
                int? idEstilo = Convert.ToInt32(Convert.ToInt32(Session["id_estilo"]));
                estilos.PedidosId = Convert.ToInt32(Session["idPedidoNuevo"]);
                objItems.AgregarItems(estilos);
                Session["estiloIdItem"] = objItems.Obtener_Utlimo_Item();
                //Obtener la lista de tallas del item
                List<ItemTalla> listaTallas = objTallas.ListaTallasPorSummary(idEstilo).ToList();
                ItemTalla tallas = new ItemTalla();

                foreach (var itemT in listaTallas)
                {

                    tallas.Talla = itemT.Talla;
                    tallas.Cantidad = itemT.Cantidad;
                    tallas.Extras = itemT.Extras;
                    tallas.Ejemplos = itemT.Ejemplos;
                    tallas.IdSummary = Convert.ToInt32(Session["estiloIdItem"]);

                    objTallas.RegistroTallas(tallas);
                }

            }
            return Convert.ToInt32(Session["idPedidoNuevo"]);
        }

        [HttpGet]
        public ActionResult Revision(int? id)
        {
            PedidosData objPedido = new PedidosData();
            int idPedido = ObtenerPORevision(id);
            Session["idPedidoRevision"] = idPedido;
            POSummary summary = new POSummary();
            ListaGenero(summary);
            ListaTela(summary);
            ListaTipoCamiseta(summary);
            ListaTipoFormaPacking(summary);

            if (id == null)
            {
                return View();
            }
            OrdenesCompra pedidos = objPedido.ConsultarListaPO(idPedido);
            ListasClientes(pedidos);


            /* if(id != null)
             {
                 RegistrarRevisionPO(pedidos);
             }   */


            if (pedidos == null)
            {
                return View();
            }

            return View(pedidos);

        }
        [HttpPost]
        public ActionResult RegistrarRevisionPO([Bind] OrdenesCompra pedido)
        {
            //List<POSummary> listaItems = objItems.ListaItemsPorPO(pedido.IdPedido).ToList();


            // List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstilo(id).ToList();
            return View(pedido);
        }

        public ActionResult CancelarPO(int id)
        {
            PedidosData objPedido = new PedidosData();
            objPedido.ActualizarEstadoPOCancelado(id);
            List<POSummary> listaItems = objItems.ListaItemsPorPO(id).ToList();
            foreach (var item in listaItems)
            {
                objPedido.ActualizarEstadoStyleCancelado(item.IdItems);

            }

            TempData["cancelarPO"] = "The purchase order was canceled correctly.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CancelarStyle(int id, int IdPedido)
        {
            PedidosData objPedido = new PedidosData();
            objPedido.ActualizarEstadoStyleCancelado(id);
            TempData["cancelarStylePO"] = "The Style was canceled correctly.";
            return Json(new
            {
                redirectUrl = Url.Action("Detalles", "Pedidos", new { id = IdPedido }),
                isRedirect = true
            });
        }

        [HttpPost]
        public ActionResult EliminarStyle(int id, int IdPedido)
        {
            ItemTalla tallaItem = new ItemTalla();
            PedidosData objPedido = new PedidosData();
            PrintShopC datosPrint = new PrintShopC();
            Pnl datosPnl = new Pnl();
            tallaItem.HistorialPacking = objPacking.ObtenerNumeroPacking(id);
            datosPnl.HistorialPNL = objPnl.ObtenerNumeroPNL(id);
            datosPrint.HistorialPrintshop = objPrint.ObtenerNumeroPrintShop(id);
            if (tallaItem.HistorialPacking != 0 || datosPnl.HistorialPNL != 0 || datosPrint.HistorialPrintshop != 0)
            {
                TempData["deleteNoStylePO"] = "The style cannot be deleted. It can only be canceled.";
            }
            else
            {
                objPedido.EliminarTallasEstilos(id);
                objPedido.EliminarArteEstilos(id);
                objPedido.EliminarTipoPackEstilo(id);
                objPedido.EliminarEstilo(id);
                TempData["deleteStylePO"] = "The Style was delete correctly.";
            }
            return Json(new
            {
                redirectUrl = Url.Action("Detalles", "Pedidos", new { id = IdPedido }),
                isRedirect = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Revision([Bind] OrdenesCompra pedido)
        {
            string cliente = Request.Form["Nombre"].ToString();
            pedido.Cliente = Int32.Parse(cliente);

            string clienteFinal = Request.Form["NombreCliente"].ToString();
            pedido.ClienteFinal = Int32.Parse(clienteFinal);
            /*if (id != pedido.IdPedido)
            {
                return View();
            }*/
            /* if (ModelState.IsValid)
             {
                 objPedido.ActualizarPedidos(pedido);
                 TempData["pedidoRevision"] = "Se registro correctamente la revisión de la orden de compra .";
                 return RedirectToAction("Index");
             }
             else
             {
                 TempData["pedidoRevisionError"] = "No se pudo registrar la revisión de la orden de compra, intentelo más tarde.";
             }*/
            return View(pedido);
        }



        [HttpGet]
        public ActionResult EditarEstilo(int? id)
        {
            if (id == null)
            {
                return View();
            }


            POSummary items = objItems.ConsultarListaEstilos(id);
            ListaGenero(items);
            ListaTela(items);
            ListaTipoCamiseta(items);
            ListaEspecialidades(items);
            ListaTipoFormaPacking(items);
            items.CatColores = objColores.ConsultarListaColores(items.ColorId);
            items.ItemDescripcion = objEst.ConsultarListaItemDesc(items.IdItems);
            items.CatEspecialidades = objEspecialidad.ConsultarListaEspecialidad(items.IdEspecialidad);
            items.CatTipoFormPack = objFormaPacking.ConsultarListatipoFormPack(items.IdTipoFormPack);
            items.PedidosId = items.PedidosId;
            SeleccionarGenero(items);
            SeleccionarTela(items);
            SeleccionarTipoCamiseta(items);
            SeleccionarTipoEspecialidad(items);
            SeleccionarTipoFormaPack(items);

            if (items == null)
            {

                return View();
            }

            return PartialView(items);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarEstilo(int id, [Bind] POSummary items)
        {
            items.IdItems = id;
            /* if (id != items.IdItems)
             {
                 return View();
             }*/
            string genero = Request.Form["Genero"].ToString();
            items.Id_Genero = objGenero.ObtenerIdGenero(genero);
            string tipoCamiseta = Request.Form["DescripcionTipo"].ToString();
            items.IdCamiseta = objTipoC.ObtenerIdTipoCamiseta(tipoCamiseta);
            string tela = Request.Form["Tela"].ToString();
            items.IdTela = Int32.Parse(tela);
            string estilo = items.ItemDescripcion.ItemEstilo;
            items.IdEstilo = objEst.ObtenerIdEstilo(estilo);
            string color = items.CatColores.CodigoColor;
            items.ColorId = objColores.ObtenerIdColor(color);
            if (items.IdItems != 0)
            {
                objItems.ActualizarEstilos(items);
                TempData["itemEditar"] = "The style was modified correctly.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["itemEditarError"] = "The style could not be modified, try it later.";
            }
            return View(items);
        }

        [HttpGet]
        public ActionResult EditarEstiloNuevo(int? id)
        {
            if (id == null)
            {
                return View();
            }
            CatClienteData objCliente = new CatClienteData();
            POSummary items = objItems.ConsultarListaEstilos(id);
            ListaGenero(items);
            ListaTela(items);
            ListaTipoCamiseta(items);
            ListaEspecialidades(items);
            ListaTipoFormaPacking(items);
            items.CatColores = objColores.ConsultarListaColores(items.ColorId);
            items.ItemDescripcion = objEst.ConsultarListaItemDesc(items.IdItems);
            items.CatEspecialidades = objEspecialidad.ConsultarListaEspecialidad(items.IdEspecialidad);
            items.CatTipoFormPack = objFormaPacking.ConsultarListatipoFormPack(items.IdTipoFormPack);
            items.PedidosId = items.PedidosId;
            items.NumCliente = objCliente.ObtenerNumeroCliente(items.PedidosId);
            SeleccionarGenero(items);
            SeleccionarTela(items);
            SeleccionarTipoCamiseta(items);
            SeleccionarTipoEspecialidad(items);
            SeleccionarTipoFormaPack(items);
            string[] separadas;

            separadas = items.TipoImpresion.Split(',');

            int i = 0;
            foreach (var item in separadas)
            {
                i++;
            }
            int x = i - 1;
            for (int v = 0; v < x; v++)
            {
                string dato = separadas[v];
                if (dato == "Sleeve")
                {
                    items.TipoImpSleeve = true;
                }

                if (dato == "Sleeve2")
                {
                    items.TipoImpSleeve2 = true;
                }

                if (dato == "Back")
                {
                    items.TipoImpBack = true;
                }

            }



            if (items == null)
            {

                return View();
            }

            return PartialView(items);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarEstiloNuevo(int id, [Bind] POSummary items)
        {
            items.IdItems = id;
            /* if (id != items.IdItems)
             {
                 return View();
             }*/
            string genero = Request.Form["Genero"].ToString();
            items.Id_Genero = objGenero.ObtenerIdGenero(genero);
            string tipoCamiseta = Request.Form["DescripcionTipo"].ToString();
            items.IdCamiseta = objTipoC.ObtenerIdTipoCamiseta(tipoCamiseta);
            string tela = Request.Form["Tela"].ToString();
            items.IdTela = Int32.Parse(tela);
            string estilo = items.ItemDescripcion.ItemEstilo;
            items.IdEstilo = objEst.ObtenerIdEstilo(estilo);
            string color = items.CatColores.CodigoColor;
            items.ColorId = objColores.ObtenerIdColor(color);
            if (items.IdItems != 0)
            {
                objItems.ActualizarEstilos(items);
                TempData["itemEditar"] = "The style was modified correctly.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["itemEditarError"] = "The style could not be modified, try it later.";
            }
            return View(items);
        }

        [HttpGet]
        public ActionResult EditarPO(int? id)
        {
            if (id == null)
            {
                return View();
            }
            CatClienteFinalData objClienteFinal = new CatClienteFinalData();
            CatClienteData objCliente = new CatClienteData();
            PedidosData objPedido = new PedidosData();
            OrdenesCompra pedido = objPedido.ConsultarListaPO(id);
            pedido.FechaCancelada = String.Format("{0:MM/dd/yyyy}", pedido.FechaCancel);
            pedido.FechaOrdenFinal = String.Format("{0:MM/dd/yyyy}", pedido.FechaOrden);
            pedido.NombrePO = pedido.PO.TrimEnd(' ');
            pedido.PO = pedido.NombrePO;
            ListasClientes(pedido);
            ListaTipoOrden(pedido);
            pedido.IdEstilo = pedido.IdPedido;
            pedido.CatCliente = objCliente.ConsultarListaClientes(pedido.Cliente);
            pedido.CatClienteFinal = objClienteFinal.ConsultarListaClientesFinal(pedido.ClienteFinal);
            pedido.CatTipoOrden = objTipoOrden.ConsultarListaTipoOrden(pedido.IdTipoOrden);

            if (pedido == null)
            {

                return View();
            }

            return View(pedido);

        }

        public JsonResult Lista_MillPO(int? id)
        {
            PedidosData objPedido = new PedidosData();
            OrdenesCompra pedido = new OrdenesCompra();
            List<InfoMillPO> listaMPO = objPedido.ListaMillPOPedido(id).ToList();
            pedido.ListaMillPO = listaMPO;
            var result = Json(new { listMpo = listaMPO });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Lista_Packings(int? id)
        {
            OrdenesCompra pedido = new OrdenesCompra();
            List<CatTypePackItem> listaPack = objItems.ListaPackPorEstilo(id).ToList();
            pedido.ListaTypePack = listaPack;
            var result = Json(new { listPack = listaPack });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ActualizarInfPO([Bind] OrdenesCompra pedido, int id,/* string po, string VPO, DateTime FechaCancel, DateTime FechaOrden, int Cliente, int Clientefinal, int TotalUnidades, int IdTipoOrden,*/ List<string> ListaMPO)
        {
            pedido.IdPedido = id;

            if (pedido.IdPedido != 0)
            {
                PedidosData objPedido = new PedidosData();
                objPedido.ActualizarPedidos(pedido);
                this.Obtener_Lista_MIllPO(ListaMPO, pedido.IdPedido);
                TempData["itemEditar"] = "The purchase order was modified correctly.";
                //return RedirectToAction("Index");
                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Pedidos"),
                    isRedirect = true
                });
            }
            else
            {
                TempData["itemEditarError"] = "The purchase order could not be modified, try it later.";
            }



            return Json("0", JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public void EliminarMillPO(string id)
        {
            PedidosData objPedido = new PedidosData();
            int idMillpo = Convert.ToInt32(id);
            objPedido.EliminarMillPO(idMillpo);
        }

        [HttpPost]
        public void EliminarPacking(string id)
        {
            PedidosData objPedido = new PedidosData();
            int idPacking = Convert.ToInt32(id);
            objPedido.EliminarPackEstilo(idPacking);
        }

        [HttpPost]
        public JsonResult Obtener_Lista_MIllPO(List<string> ListaMillpo, int idPedido)
        {
            PedidosData objPedido = new PedidosData();
            InfoMillPO datoMPO = new InfoMillPO();
            List<string> datosIds = ListaMillpo[0].Split('*').ToList();
            List<string> descMPO = ListaMillpo[1].Split('*').ToList();
            int i = 0;
            foreach (var item in datosIds)
            {
                i++;
            }
            int x = i - 1;
            for (int v = 0; v < x; v++)
            {
                string id = datosIds[v];
                if (id == "0")
                {
                    datoMPO.MillPO = descMPO[v];
                    datoMPO.IdPedido = idPedido;
                    objPedido.RegistroMillPO(datoMPO);

                }
                else
                {
                    datoMPO.IdMillPO = Convert.ToInt32(id);
                    datoMPO.MillPO = descMPO[v];
                    objPedido.ActualizarInfoMPO(datoMPO);

                }
            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public ActionResult Eliminar(int? id)
        {
            objItems.EliminarTallasEstilo(id);
            objItems.EliminarEstilos(id);
            TempData["eliminarEstilo"] = "The style was removed correctly.";
            return View();
        }



        public void SeleccionarClientes(OrdenesCompra pedido)
        {
            CatClienteData objCliente = new CatClienteData();
            List<CatCliente> listaClientes = objCliente.ListaClientes().ToList();
            pedido.CatCliente = objCliente.ConsultarListaClientes(pedido.Cliente);
            pedido.CatCliente.Customer = pedido.Cliente;
            ViewBag.listCliente = new SelectList(listaClientes, "Customer", "Nombre", pedido.Cliente);


        }

        public void SeleccionarClienteFinal(OrdenesCompra pedido)
        {
            CatClienteFinalData objClienteFinal = new CatClienteFinalData();
            List<CatClienteFinal> listaClientesFinal = objClienteFinal.ListaClientesFinal().ToList();
            pedido.CatClienteFinal = objClienteFinal.ConsultarListaClientesFinal(pedido.ClienteFinal);
            pedido.CatClienteFinal.CustomerFinal = pedido.ClienteFinal;
            ViewBag.listClienteFinal = new SelectList(listaClientesFinal, "CustomerFinal", "NombreCliente", pedido.ClienteFinal);

        }

        public void SeleccionarGenero(POSummary items)
        {
            List<CatGenero> listaGenero = objGenero.ListaGeneros().ToList();
            items.CatGenero = objGenero.ConsultarListaGenero(items.Id_Genero);
            items.CatGenero.IdGender = items.Id_Genero;
            ViewBag.listGenero = new SelectList(listaGenero, "GeneroCode", "Genero", items.CatGenero.GeneroCode);

        }

        public void SeleccionarTela(POSummary items)
        {
            List<CatTela> listaTela = objTela.ListaTela().ToList();
            items.CatTela = objTela.ConsultarListaTelas(items.IdTela);
            items.CatTela.Id_Tela = items.IdTela;
            ViewBag.listTela = new SelectList(listaTela, "Id_Tela", "Tela", items.IdTela);
        }

        public void SeleccionarTipoCamiseta(POSummary items)
        {
            List<CatTipoCamiseta> listaTipoCamiseta = objTipoC.ListaTipoCamiseta().ToList();
            items.CatTipoCamiseta = objTipoC.ConsultarListaCamisetas(items.IdCamiseta);
            items.CatTipoCamiseta.IdTipo = items.IdCamiseta;
            ViewBag.listTipoCamiseta = new SelectList(listaTipoCamiseta, "TipoProducto", "DescripcionTipo", items.CatTipoCamiseta.TipoProducto);
        }

        public void SeleccionarTipoEspecialidad(POSummary items)
        {
            List<CatEspecialidades> listaEspecialidades = objEspecialidad.ListaEspecialidades().ToList();
            items.CatEspecialidades = objEspecialidad.ConsultarListaEspecialidad(items.IdEspecialidad);
            items.CatEspecialidades.IdEspecialidad = items.IdEspecialidad;
            ViewBag.listEspecialidad = new SelectList(listaEspecialidades, "IdEspecialidad", "Especialidad", items.IdEspecialidad);
        }

        public void SeleccionarTipoFormaPack(POSummary items)
        {
            List<CatTypeFormPack> listaTipoFormPack = objFormaPacking.ListaTipoFormaPack().ToList();
            items.CatTipoFormPack = objFormaPacking.ConsultarListatipoFormPack(items.IdTipoFormPack);
            items.IdTipoFormPack = (items.IdTipoFormPack == 0 ? 1 : items.IdTipoFormPack);
            items.CatTipoFormPack.IdTipoFormPack = items.IdTipoFormPack;
            ViewBag.listTipoFormPack = new SelectList(listaTipoFormPack, "IdTipoFormPack", "TipoFormPack", items.IdTipoFormPack);
        }

        public void ListasClientes(OrdenesCompra pedido)
        {
            CatClienteFinalData objClienteFinal = new CatClienteFinalData();
            CatClienteData objCliente = new CatClienteData();
            List<CatCliente> listaClientes = objCliente.ListaClientes().ToList();

            ViewBag.listCliente = new SelectList(listaClientes, "Customer", "Nombre", pedido.Cliente);

            List<CatClienteFinal> listaClientesFinal = objClienteFinal.ListaClientesFinal().ToList();
            ViewBag.listClienteFinal = new SelectList(listaClientesFinal, "CustomerFinal", "NombreCliente", pedido.ClienteFinal);
        }

        public void ListaTipoOrden(OrdenesCompra pedido)
        {
            List<CatTipoOrden> listaTipoOrden = objTipoOrden.ListaTipoOrden().ToList();

            ViewBag.listTipoOrden = new SelectList(listaTipoOrden, "IdTipoOrden", "TipoOrden", pedido.IdTipoOrden);

        }

        public void ObtenerIdClientes(OrdenesCompra pedido)
        {
            CatClienteFinalData objClienteFinal = new CatClienteFinalData();
            CatClienteData objCliente = new CatClienteData();
            string cliente = Request.Form["listCliente"].ToString();
            pedido.Cliente = Int32.Parse(cliente);
            pedido.CatCliente = objCliente.ConsultarListaClientes(pedido.Cliente);


            string clienteFinal = Request.Form["listClienteFinal"].ToString();
            pedido.ClienteFinal = Int32.Parse(clienteFinal);
            pedido.CatClienteFinal = objClienteFinal.ConsultarListaClientesFinal(pedido.ClienteFinal);



        }

        public void ListaGenero(POSummary summary)
        {
            List<CatGenero> listaGenero = objGenero.ListaGeneros().ToList();

            ViewBag.listGenero = new SelectList(listaGenero, "GeneroCode", "Genero", summary.IdGenero);

        }

        public void ListaTela(POSummary summary)
        {
            List<CatTela> listaTela = objTela.ListaTela().ToList();

            ViewBag.listTela = new SelectList(listaTela, "Id_Tela", "Tela", summary.IdTela);

        }

        public void ListaTipoCamiseta(POSummary summary)
        {
            List<CatTipoCamiseta> listaTipoCamiseta = objTipoC.ListaTipoCamiseta().ToList();

            ViewBag.listTipoCamiseta = new SelectList(listaTipoCamiseta, "TipoProducto", "DescripcionTipo", summary.TipoCamiseta);

        }

        public void ListaEspecialidades(POSummary summary)
        {
            List<CatEspecialidades> listaEspecialidades = objEspecialidad.ListaEspecialidades().ToList();

            ViewBag.listEspecialidad = new SelectList(listaEspecialidades, "IdEspecialidad", "Especialidad", summary.IdEspecialidad);

        }

        public void ListaTipoFormaPacking(POSummary summary)
        {
            List<CatTypeFormPack> listaFomPack = objFormaPacking.ListaTipoFormaPack().ToList();
            ViewBag.listTipoFormPack = new SelectList(listaFomPack, "IdTipoFormPack", "TipoFormPack", summary.IdTipoFormPack);

        }

        public void ListaEstados(OrdenesCompra pedido)
        {
            List<CatStatus> listaEstados = objEstados.ListarEstados().ToList();

            ViewBag.listEstados = new SelectList(listaEstados, "IdStatus", "Estado", pedido.IdStatus);
            foreach (var item in listaEstados)
            {
                if (item.IdStatus == 1)
                {
                    pedido.IdStatus = item.IdStatus;
                }

            }

        }
        public void ObtenerEstadoRevisado(OrdenesCompra pedido)
        {
            List<CatStatus> listaEstados = objEstados.ListarEstados().ToList();

            ViewBag.listEstados = new SelectList(listaEstados, "IdStatus", "Estado", pedido.IdStatus);
            foreach (var item in listaEstados)
            {
                if (item.IdStatus == 1)
                {
                    pedido.IdStatus = item.IdStatus;
                }

            }
        }

        public ActionResult ReportWIP()
        {
            reporte.ObtenerReporteWIP();
            return Json("0", JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListadoPoPorEstilo(int? id)
        {
            PedidosData objPedido = new PedidosData();
            int id_estilo = objEst.ObtenerIdEstiloPorsummary(id);
            List<Estilo_PO> lista_estilos = objPedido.Obtener_pedidos_po_estilo(id_estilo);
            var result = Json(new { listaPO = lista_estilos });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}