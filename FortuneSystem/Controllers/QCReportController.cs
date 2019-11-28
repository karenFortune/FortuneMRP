using FortuneSystem.Models.Catalogos;
using FortuneSystem.Models.Item;
using FortuneSystem.Models.Items;
using FortuneSystem.Models.Pedidos;
using FortuneSystem.Models.POSummary;
using FortuneSystem.Models.PrintShop;
using FortuneSystem.Models.QCReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FortuneSystem.Controllers
{
    public class QCReportController : Controller
    {
        // GET: QCReportGeneral
        public ActionResult Index()
        {
            PedidosData objPedido = new PedidosData();
            List<OrdenesCompra> listaPedidos = objPedido.ListaOrdenCompra().ToList();
            return View(listaPedidos);
        }

        [HttpGet]
        public ActionResult Detalles(int? id)
        {
            PedidosData objPedido = new PedidosData();
            CatClienteData objCliente = new CatClienteData();
            CatClienteFinalData objClienteFinal = new CatClienteFinalData();


            if (id == null)
            {
                return View();
            }
            int cargo = Convert.ToInt32(Session["idCargo"]);
            if (cargo == 0)
            {
                Session["idCargo"] = 0;
            }
            OrdenesCompra pedido = objPedido.ConsultarListaPO(id);
            pedido.CatCliente = objCliente.ConsultarListaClientes(pedido.Cliente);
            pedido.CatClienteFinal = objClienteFinal.ConsultarListaClientesFinal(pedido.ClienteFinal);
            pedido.IdPedido = Convert.ToInt32(id);


            //report.ListaTallas = objTallas.ListaTallasPorEstiloQC(id).ToList();
            //pedido.QCReport = report;

            if (pedido == null)
            {
                return View();
            }
            return View(pedido);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Estilo(int? id, int? idReporte)
        {
            ItemTallaData objTallas = new ItemTallaData();
            QCPruebaLavadoData objLavado = new QCPruebaLavadoData();
            List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstiloQC(id).ToList();
            List<QCPruebaLavado> listaPruebaL = objLavado.ListaPruebasLavado(idReporte).ToList();

            var result = Json(new
            {
                listaTalla = listaTallas,
                listaPrueba = listaPruebaL
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_MisPrints(int? id)
        {
            QCMisPrintsData objMisPrint = new QCMisPrintsData();
            ItemTallaData objTallas = new ItemTallaData();
            List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstiloQC(id).ToList();
            List<ItemTalla> listaExtas = objTallas.ListaExtrasTallasPorEstilo(id).ToList();
            List<QCMisPrints> datosRegistradosMP = objMisPrint.ListaMPRegistrados(id).ToList();
            var result = Json(new { listaTalla = listaTallas, listExtras = listaExtas, datosMP = datosRegistradosMP });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Reporte_Mensual(int? id, string estilo, string fecha)
        {
            QCMisPrintsData objMisPrint = new QCMisPrintsData();
            ItemTallaData objTallas = new ItemTallaData();
            PrintShopData objPrint = new PrintShopData();
            ItemDescripcionData objItemDesc = new ItemDescripcionData();
            DescripcionItemData objSummary = new DescripcionItemData();
            List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstiloQC(id).ToList();
            int noEstilo = objItemDesc.ObtenerIdEstilo(estilo);
            string mes = "";
            string year = "";
            string[] datos = fecha.Split(' ');
            int contador = 0;
            foreach (var Fechadatos in datos)
            {
                if (contador == 0)
                {
                    mes = Fechadatos;
                    contador++;
                }
                else
                {
                    year = Fechadatos;
                }

            }
            int mesF = ObtenerNumeroMes(mes);
            List<int> listaPOSummary = objSummary.ListaPOSummaryPorIdEstilo(noEstilo).ToList();
            List<int> listaPrintshopQty = objPrint.ListaTotalPrintedTallasEstilosRM(listaPOSummary, mesF, year).ToList();
            List<int> listaMisPrint1Qty = objMisPrint.ListaTotalMisPrintsQC(listaPOSummary, mesF, year).ToList();
            List<int> listaMisPrint2Qty = objMisPrint.ListaTotalMisPrints2QC(listaPOSummary, mesF, year).ToList();
            List<int> listaRepairs1Qty = objMisPrint.ListaTotalRepairs1stQC(listaPOSummary, mesF, year).ToList();
            List<int> listaRepairs2Qty = objMisPrint.ListaTotalRepairs2ndQC(listaPOSummary, mesF, year).ToList();
            List<int> listaSprayed1Qty = objMisPrint.ListaTotalSprayed1stQC(listaPOSummary, mesF, year).ToList();
            List<int> listaSprayed2Qty = objMisPrint.ListaTotalSprayed2ndQC(listaPOSummary, mesF, year).ToList();
            List<int> listaDefects1Qty = objMisPrint.ListaTotalDefects1stQC(listaPOSummary, mesF, year).ToList();
            List<int> listaDefects2Qty = objMisPrint.ListaTotalDefects2ndQC(listaPOSummary, mesF, year).ToList();


            List<ItemTalla> listaExtas = objTallas.ListaExtrasTallasPorEstilo(id).ToList();
            List<QCMisPrints> datosRegistradosMP = objMisPrint.ListaMPRegistrados(id).ToList();

            var result = Json(new
            {
                listaTalla = listaTallas,
                listExtras = listaExtas,
                datosMP = datosRegistradosMP,
                listaPrint = listaPrintshopQty,
                listaMisPrint1 = listaMisPrint1Qty,
                listaMisPrint2 = listaMisPrint2Qty,
                listaRepairs1 = listaRepairs1Qty,
                listaRepairs2 = listaRepairs2Qty,
                listaSprayed1 = listaSprayed1Qty,
                listaSprayed2 = listaSprayed2Qty,
                listaDefects1 = listaDefects1Qty,
                listaDefects2 = listaDefects2Qty
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public int ObtenerNumeroMes(string mes)
        {
            switch (mes)
            {
                case "Enero":
                    return 1;
                case "Febrero":
                    return 1;
                case "Marzo":
                    return 3;
                case "Abril":
                    return 4;
                case "Mayo":
                    return 5;
                case "Junio":
                    return 6;
                case "Julio":
                    return 7;
                case "Agosto":
                    return 8;
                case "Septiembre":
                    return 9;
                case "Octubre":
                    return 10;
                case "Noviembre":
                    return 11;
                case "Diciembre":
                    return 12;

            }
            return 0;
        }

        [HttpPost]
        public JsonResult Obtener_Datos_Reporte(int? id, int? turno)
        {
            ItemTallaData objTallas = new ItemTallaData();
            QCReportData objReporte = new QCReportData();
            QCReportGeneral report = new QCReportGeneral();
            List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstiloQC(id).ToList();
            report = objReporte.ObtenerInformacionReportT1(id, turno);

            var result = Json(new
            {
                listaTalla = listaTallas,
                datos = report
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Obtener_Reporte_General([Bind] QCReportGeneral reporte, List<string> ListTalla)
        {
            QCPruebaLavado pruebaL = new QCPruebaLavado();
            QCReportData objReporte = new QCReportData();
            reporte.AQL = (reporte.DatoAQL == "true") ? true : false;
            reporte.IdUsuario = Convert.ToInt32(Session["id_Empleado"]);
            objReporte.AgregarReporte(reporte);
            pruebaL.IdQCReport = objReporte.Obtener_Utlimo_IdReporte();
            Obtener_Tabla_Prueba_Lavado(pruebaL, ListTalla);

            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Actualizar_Reporte_General([Bind] QCReportGeneral reporte, List<string> ListTalla)
        {
            QCPruebaLavado pruebaL = new QCPruebaLavado();
            QCReportData objReporte = new QCReportData();
            reporte.AQL = (reporte.DatoAQL == "true") ? true : false;
            reporte.IdUsuario = Convert.ToInt32(Session["id_Empleado"]);
            objReporte.ActualizarReporte(reporte);

            Actualizar_Tabla_Prueba_Lavado(pruebaL, ListTalla);

            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Obtener_Tabla_Prueba_Lavado(QCPruebaLavado PruebaL, List<string> ListTalla)
        {
            CatTallaItemData objTalla = new CatTallaItemData();
            QCPruebaLavadoData objPrueba = new QCPruebaLavadoData();
            List<string> horas = ListTalla[0].Split('*').ToList();
            List<string> tallas = ListTalla[1].Split('*').ToList();
            List<string> resultados = ListTalla[2].Split('*').ToList();
            int i = 1;
            foreach (var item in horas)
            {
                i++;
            }

            i -= 2;
            for (int v = 0; v < i; v++)
            {
                string hora = horas[v];
                string talla = tallas[v];
                string result = resultados[v];

                PruebaL.Results = Int32.Parse(result);
                PruebaL.IdTalla = objTalla.ObtenerIdTalla(talla);
                PruebaL.HoraLavado = Convert.ToDateTime(hora);
                objPrueba.AgregarPruebaLavado(PruebaL);
            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public JsonResult Actualizar_Tabla_Prueba_Lavado(QCPruebaLavado PruebaL, List<string> ListTalla)
        {
            CatTallaItemData objTalla = new CatTallaItemData();
            QCPruebaLavadoData objPrueba = new QCPruebaLavadoData();
            List<string> ids = ListTalla[0].Split('*').ToList();
            List<string> horas = ListTalla[1].Split('*').ToList();
            List<string> tallas = ListTalla[2].Split('*').ToList();
            List<string> resultados = ListTalla[3].Split('*').ToList();
            int i = 1;
            foreach (var item in horas)
            {
                i++;
            }

            i -= 2;
            for (int v = 0; v < i; v++)
            {
                string idPrueba = ids[v];
                string hora = horas[v];
                string talla = tallas[v];
                string result = resultados[v];

                PruebaL.Results = Int32.Parse(result); //(result == "true") ? true : false;
                PruebaL.IdQCPruebasLavados = Int32.Parse(idPrueba);
                PruebaL.IdTalla = objTalla.ObtenerIdTalla(talla);
                PruebaL.HoraLavado = Convert.ToDateTime(hora);


                objPrueba.ActualizarPruebaLavado(PruebaL);


            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Obtener_Tabla_MisPrints(QCMisPrints datoMP, List<string> ListTalla)
        {
            CatTallaItemData objTalla = new CatTallaItemData();
            QCMisPrintsData objMP = new QCMisPrintsData();

            int i = 1;
            foreach (var item in ListTalla)
            {
                i++;
            }

            i -= 1;
            for (int v = 0; v < i; v++)
            {
                List<string> tallas = ListTalla[v].Split('*').ToList();
                string mp1;
                string mp2;
                string rp1;
                string rp2;
                string sp1;
                string sp2;
                string d1;
                string d2;
                string talla;
                int c = 0;

                foreach (var item in tallas)
                {
                    if (c == 0)
                    {
                        talla = tallas[0];
                        mp1 = tallas[2];
                        mp2 = tallas[3];
                        rp1 = tallas[5];
                        rp2 = tallas[6];
                        sp1 = tallas[7];
                        sp2 = tallas[8];
                        d1 = tallas[9];
                        d2 = tallas[10];
                        datoMP.IdTalla = objTalla.ObtenerIdTalla(talla);
                        datoMP.MisPrint1st = Int32.Parse(mp1);
                        datoMP.MisPrint2nd = Int32.Parse(mp2);
                        datoMP.Repairs1st = Int32.Parse(rp1);
                        datoMP.Repairs2nd = Int32.Parse(rp2);
                        datoMP.Sprayed1st = Int32.Parse(sp1);
                        datoMP.Sprayed2nd = Int32.Parse(sp2);
                        datoMP.Defects1st = Int32.Parse(d1);
                        datoMP.Defects2nd = Int32.Parse(d2);

                        objMP.AgregarMisPrints(datoMP);
                    }
                    c++;
                }
            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Actualizar_Tabla_MisPrints(QCMisPrints datoMP, List<string> ListTalla)
        {
            CatTallaItemData objTalla = new CatTallaItemData();
            QCMisPrintsData objMP = new QCMisPrintsData();

            int i = 1;
            foreach (var item in ListTalla)
            {
                i++;
            }

            i -= 1;
            for (int v = 0; v < i; v++)
            {
                List<string> tallas = ListTalla[v].Split('*').ToList();
                string mp1;
                string mp2;
                string rp1;
                string rp2;
                string sp1;
                string sp2;
                string d1;
                string d2;
                string talla;
                int c = 0;

                foreach (var item in tallas)
                {
                    if (c == 0)
                    {
                        talla = tallas[0];
                        mp1 = tallas[2];
                        mp2 = tallas[3];
                        rp1 = tallas[5];
                        rp2 = tallas[6];
                        sp1 = tallas[7];
                        sp2 = tallas[8];
                        d1 = tallas[9];
                        d2 = tallas[10];
                        datoMP.IdTalla = objTalla.ObtenerIdTalla(talla);
                        datoMP.MisPrint1st = Int32.Parse(mp1);
                        datoMP.MisPrint2nd = Int32.Parse(mp2);
                        datoMP.Repairs1st = Int32.Parse(rp1);
                        datoMP.Repairs2nd = Int32.Parse(rp2);
                        datoMP.Sprayed1st = Int32.Parse(sp1);
                        datoMP.Sprayed2nd = Int32.Parse(sp2);
                        datoMP.Defects1st = Int32.Parse(d1);
                        datoMP.Defects2nd = Int32.Parse(d2);

                        objMP.ActualizarMisPrints(datoMP);
                    }
                    c++;
                }
            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }
    }
}