using FortuneSystem.Models.Almacen;
using FortuneSystem.Models.Catalogos;
using FortuneSystem.Models.Item;
using FortuneSystem.Models.Items;
using FortuneSystem.Models.Packing;
using FortuneSystem.Models.Pedidos;
using FortuneSystem.Models.POSummary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FortuneSystem.Controllers
{
    public class PackingController : Controller
    {
        readonly PedidosData objPedido = new PedidosData();
        readonly CatClienteData objCliente = new CatClienteData();
        readonly CatClienteFinalData objClienteFinal = new CatClienteFinalData();
        readonly CatTallaItemData objTalla = new CatTallaItemData();
        readonly PackingData objPacking = new PackingData();
        readonly ItemTallaData objTallas = new ItemTallaData();
        readonly DescripcionItemData objSummary = new DescripcionItemData();
        readonly FuncionesInventarioGeneral objInv = new FuncionesInventarioGeneral();
        // GET: Packing
        public ActionResult Index()
        {
            List<OrdenesCompra> listaPedidos = objPedido.ListaOrdenCompra().ToList();
            return View(listaPedidos);
        }

        public ActionResult IndexPack()
        {
            List<OrdenesCompra> listaPedidos = objPedido.ListaOrdenCompra().ToList();
            return View(listaPedidos);
        }

        [HttpGet]
        public ActionResult Detalles(int? id, string estatus)
        {
            if (id == null)
            {
                return View();
            }
            int cargo = Convert.ToInt32(Session["idCargo"]);
            if (cargo == 0)
            {
                Session["idCargo"] = 0;
            }
            PackingM packing = new PackingM();
            PackingTypeSize packSize = new PackingTypeSize();
            OrdenesCompra pedido = objPedido.ConsultarListaPO(id);
            pedido.Packing = packing;
            pedido.Packing.PackingTypeSize = packSize;
            pedido.ListItems = objPedido.ListaItemEstilosPorIdPedido(id).ToList();
            Session["id_Pedido"] = id;
            pedido.CatCliente = objCliente.ConsultarListaClientes(pedido.Cliente);
            pedido.CatClienteFinal = objClienteFinal.ConsultarListaClientesFinal(pedido.ClienteFinal);
            if (pedido.NombreClienteFinal == null)
            {
                pedido.NombreClienteFinal = "";
            }
            pedido.NombreClienteFinal = pedido.CatClienteFinal.NombreCliente.TrimEnd();

            ListasEstilos(pedido, id);
            ListaPackingRegistrados(pedido, id);
            ListaPackingRegistradosPPKs(pedido, id);
            ListaPackingRegistradosVariosPPKs(pedido, id);
            ListaPackingRegistradosVariosBulks(pedido, id);

            pedido.IdPedido = Convert.ToInt32(id);
            int cont = 0;
            Session["id_Block"] = objPacking.ObtenerNumBlock(pedido.ListItems, id);
            int numIdBlock = Convert.ToInt32(Session["id_Block"]) + 1;
            string namePack = "PACK-" + numIdBlock;
            string nameAssort = "ASMT-" + numIdBlock;
            pedido.Packing.PackingTypeSize.PackingName = namePack;
            pedido.Packing.PackingTypeSize.AssortName = nameAssort;
            //Session[""] = objPacking.ObtenerNumBlock();

            foreach (var item in pedido.ListItems)
            {

                List<PackingTypeSize> estilo = objPacking.BuscarPackingTypeSizePorEstilo(item.IdSummary);

                /*if(estilo.Count != 0)
                {
                    cont++;
                }*/
            }

            pedido.HistorialPacking = cont;
            pedido.EstatusPackAssort = estatus;


            if (pedido == null)
            {
                return View();
            }
            return View(pedido);
        }

        [HttpGet]
        public ActionResult DetallesTrims(int? id, string estatus)
        {
            if (id == null)
            {
                return View();
            }
            int cargo = Convert.ToInt32(Session["idCargo"]);
            if (cargo == 0)
            {
                Session["idCargo"] = 0;
            }
            PackingM packing = new PackingM();
            PackingTypeSize packSize = new PackingTypeSize();
            OrdenesCompra pedido = objPedido.ConsultarListaPO(id);
            pedido.Packing = packing;
            pedido.Packing.PackingTypeSize = packSize;
            pedido.ListItems = objPedido.ListaItemEstilosPorIdPedido(id).ToList();
            Session["id_Pedido"] = id;
            pedido.CatCliente = objCliente.ConsultarListaClientes(pedido.Cliente);
            pedido.CatClienteFinal = objClienteFinal.ConsultarListaClientesFinal(pedido.ClienteFinal);
            if (pedido.NombreClienteFinal == null)
            {
                pedido.NombreClienteFinal = "";
            }
            pedido.NombreClienteFinal = pedido.CatClienteFinal.NombreCliente.TrimEnd();

            ListasEstilos(pedido, id);
            ListaPackingRegistrados(pedido, id);
            ListaPackingRegistradosPPKs(pedido, id);
            ListaPackingRegistradosVariosPPKs(pedido, id);
            ListaPackingRegistradosVariosBulks(pedido, id);

            pedido.IdPedido = Convert.ToInt32(id);
            int cont = 0;
            Session["id_Block"] = objPacking.ObtenerNumBlock(pedido.ListItems, id);

            Session["id_Block_PPK"] = objPacking.ObtenerNumBlockPPK(pedido.ListItems, id);
            int numBlockPPK = Convert.ToInt32(Session["id_Block_PPK"]) + 1;
            int numIdBlock = Convert.ToInt32(Session["id_Block"]) + 1;
            string namePack = "PACK-" + numIdBlock;
            string namePackPPK = "PACK-"/* + numBlockPPK*/;
            string nameAssort = "ASMT-" + numIdBlock;
            pedido.Packing.PackingTypeSize.PackingName = namePack;
            pedido.Packing.PackingTypeSize.NombrePacking = namePackPPK;
            pedido.Packing.PackingTypeSize.PackingNameBulk = namePackPPK;
            pedido.Packing.PackingTypeSize.AssortName = nameAssort;
            //Session[""] = objPacking.ObtenerNumBlock();

            foreach (var item in pedido.ListItems)
            {

                List<PackingTypeSize> estilo = objPacking.BuscarPackingTypeSizePorEstilo(item.IdSummary);

                /*if(estilo.Count != 0)
                {
                    cont++;
                }*/
            }

            pedido.HistorialPacking = cont;
            pedido.EstatusPackAssort = estatus;

            if (pedido == null)
            {
                return View();
            }
            return View(pedido);
        }

        public void ListasEstilos(OrdenesCompra pedido, int? id)
        {
            List<ItemDescripcion> listaEstilos = objPedido.ListaEstilosPorIdPedido(id).ToList();
            /*List<ItemDescripcion> listaEstilos = new List<ItemDescripcion>();
            foreach (var item in listEstilos)
            {
                List<PackingTypeSize> estilo = objPacking.BuscarPackingTypeSizePorEstilo(item.IdSummary);
                if (estilo.Count == 0)
                {
                    listaEstilos.Add(item);
                }
            }*/
            ViewBag.listEstilo = new SelectList(listaEstilos, "IdSummary", "Descripcion", pedido.IdEstilo);
        }

        public void ListaPackingRegistrados(OrdenesCompra pedido, int? id)
        {
            List<ItemDescripcion> listaEstilos = objPedido.ListaEstilosPorIdPedido(id).ToList();
            List<PackingTypeSize> listPacking = objPacking.ListaPackingName(listaEstilos);
            ViewBag.listPack = new SelectList(listPacking, "PackingRegistrado", "PackingRegistrado", pedido.Packing.PackingTypeSize.PackingRegistrado);

        }

        public void ListaPackingRegistradosPPKs(OrdenesCompra pedido, int? id)
        {
            PackingM pack = new PackingM();
            PackingTypeSize packingSize = new PackingTypeSize
            {
                ListaPackingName = objPacking.ListaPackingNamePPKS(id)
            };
            pedido.Packing = pack;
            pedido.Packing.PackingTypeSize = packingSize;
            ViewBag.listPacks = new SelectList(pedido.Packing.PackingTypeSize.ListaPackingName, "PackingRegistradoPPK", "PackingRegistradoPPK");

        }

        public void ListaPackingRegistradosVariosPPKs(OrdenesCompra pedido, int? id)
        {
            PackingM pack = new PackingM();
            PackingTypeSize packingSize = new PackingTypeSize
            {
                ListaPackingName = objPacking.ListaPackingNamePPKS(id)
            };
            pedido.Packing = pack;
            pedido.Packing.PackingTypeSize = packingSize;
            ViewBag.listPacksName = new SelectList(pedido.Packing.PackingTypeSize.ListaPackingName, "PackingRegistradoPPK", "PackingRegistradoPPK");

        }

        public void ListaPackingRegistradosVariosBulks(OrdenesCompra pedido, int? id)
        {
            PackingM pack = new PackingM();
            PackingTypeSize packingSize = new PackingTypeSize
            {
                ListaPackingName = objPacking.ListaPackingNameBULKS(id)
            };
            pedido.Packing = pack;
            pedido.Packing.PackingTypeSize = packingSize;
            ViewBag.listPacksName = new SelectList(pedido.Packing.PackingTypeSize.ListaPackingName, "PackingRegistradoVariosBULKS", "PackingRegistradoVariosBULKS");

        }

        public JsonResult ListadoPackingRegistrados(int? id)
        {
            List<ItemDescripcion> listaEstilos = objPedido.ListaEstilosPorIdPedido(id).ToList();
            List<PackingTypeSize> listPacking = objPacking.ListaPackingName(listaEstilos);
            /*List<ItemDescripcion> listaEstilos = new List<ItemDescripcion>();
            foreach (var item in listEstilos)
            {
                List<PackingTypeSize> estilo = objPacking.BuscarPackingTypeSizePorEstilo(item.IdSummary);
                if (estilo.Count == 0)
                {
                    listaEstilos.Add(item);
                }

            }*/
            //ViewBag.listEstilo = new SelectList(listaEstilos, "IdSummary", "ItemEstilo", pedido.IdEstilo);
            var result = Json(new { listEstilo = listPacking });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListadoPackingRegistradosPPK(int? id)
        {
            List<PackingTypeSize> listPacking = objPacking.ListaPackingNamePPKS(id);
            var result = Json(new { listEstilo = listPacking });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListadoPackingRegistradosVariosPPK(int? id)
        {
            List<PackingTypeSize> listPacking = objPacking.ListaPackingNamePPKS(id);
            var result = Json(new { listEstilo = listPacking });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListadoPackingRegistradosVariosBULKS(int? id)
        {
            List<PackingTypeSize> listPacking = objPacking.ListaPackingNameBULKS(id);
            var result = Json(new { listEstilo = listPacking });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListadoEstilos(int? id)
        {
            List<ItemDescripcion> listaEstilos = objPedido.ListaEstilosPorIdPedido(id).ToList();
            Session["id_Block"] = objPacking.ObtenerNumBlock(listaEstilos, id);
            int NumBlock = Convert.ToInt32(Session["id_Block"]) + 1;
            string datosBlock = "PACK-" + NumBlock;
            string nameAssort = "ASMT-" + NumBlock;
            /*List<ItemDescripcion> listaEstilos = new List<ItemDescripcion>();
            foreach (var item in listEstilos)
            {
                List<PackingTypeSize> estilo = objPacking.BuscarPackingTypeSizePorEstilo(item.IdSummary);
                if (estilo.Count == 0)
                {
                    listaEstilos.Add(item);
                }

            }*/
            //ViewBag.listEstilo = new SelectList(listaEstilos, "IdSummary", "ItemEstilo", pedido.IdEstilo);
            var result = Json(new { listEstilo = listaEstilos, Block = datosBlock, AssortBlock = nameAssort });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Obtener_Lista_Tallas_Packing(List<string> ListTalla, int EstiloID)
        {
            PackingM tallaItem = new PackingM();
            PackingSize packingSize = new PackingSize();
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            tallaItem.Usuario = noEmpleado;
            packingSize.IdSummary = EstiloID;
            //int numBatch = objPacking.ObtenerIdBatch(EstiloID);
            // tallaItem.IdBatch = numBatch + 1;
            List<string> tallas = ListTalla[0].Split('*').ToList();
            List<string> calidad = ListTalla[1].Split('*').ToList();
            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }
            i -= 1;
            string talla;
            for (int v = 0; v < i; v++)
            {
                talla = tallas[v];
                packingSize.IdTalla = objTalla.ObtenerIdTalla(talla);
                string calidadT = calidad[v];
                if (calidadT == "")
                {
                    calidadT = "0";
                }
                packingSize.Calidad = Int32.Parse(calidadT);
                tallaItem.PackingSize = packingSize;

                objPacking.AgregarTallasP(tallaItem);
            }
            return Json("0", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Obtener_Lista_Tallas_Packing_Bulk(List<string> ListTalla, int EstiloID, string TipoPackID)
        {
            PackingM tallaItem = new PackingM();
            PackingTypeSize packingTSize = new PackingTypeSize();
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            tallaItem.Usuario = noEmpleado;
            packingTSize.IdSummary = EstiloID;
            packingTSize.IdTipoEmpaque = Int32.Parse(TipoPackID);
            packingTSize.PackingName = "";
            packingTSize.AssortName = "";
            packingTSize.NumUsuario = noEmpleado;
            // int numBatch = objPacking.ObtenerIdBatch(EstiloID);
            // tallaItem.IdBatch = numBatch + 1;
            List<string> tallas = ListTalla[0].Split('*').ToList();
            List<string> piezas = ListTalla[1].Split('*').ToList();
            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }
            i -= 1;
            string talla;
            for (int v = 0; v < i; v++)
            {
                talla = tallas[v];
                packingTSize.IdTalla = objTalla.ObtenerIdTalla(talla);
                string piezasT = piezas[v];
                if (piezasT == "")
                {
                    piezasT = "0";
                }
                packingTSize.Pieces = Int32.Parse(piezasT);
                tallaItem.PackingTypeSize = packingTSize;

                objPacking.AgregarTallasTypePack(tallaItem);
            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Actualizar_Lista_Tallas_Packing_Bulk(List<string> ListTalla)
        {
            PackingM tallaItem = new PackingM();
            PackingTypeSize packingTSize = new PackingTypeSize();
            List<string> numPack = ListTalla[0].Split('*').ToList();
            List<string> tallas = ListTalla[1].Split('*').ToList();
            List<string> piezas = ListTalla[2].Split('*').ToList();
            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }
            i -= 1;
            for (int v = 0; v < i; v++)
            {
                string NumPackT = numPack[v];
                if (NumPackT == "")
                {
                    NumPackT = "0";
                }
                packingTSize.IdPackingTypeSize = Int32.Parse(NumPackT);
                string piezasT = piezas[v];
                if (piezasT == "")
                {
                    piezasT = "0";
                }
                packingTSize.Pieces = Int32.Parse(piezasT);
                tallaItem.PackingTypeSize = packingTSize;

                objPacking.ActualizarCantidadesBulk(tallaItem);
            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Actualizar_Lista_Tallas_Packing_PPK(List<string> ListTalla)
        {
            PackingM tallaItem = new PackingM();
            PackingTypeSize packingTSize = new PackingTypeSize();
            List<string> numPack = ListTalla[0].Split('*').ToList();
            List<string> tallas = ListTalla[1].Split('*').ToList();
            List<string> ratio = ListTalla[2].Split('*').ToList();
            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }

            i -= 1;
            for (int v = 0; v < i; v++)
            {
                string NumPackT = numPack[v];
                if (NumPackT == "")
                {
                    NumPackT = "0";
                }
                packingTSize.IdPackingTypeSize = Int32.Parse(NumPackT);
                string ratioT = ratio[v];
                if (ratioT == "")
                {
                    ratioT = "0";
                }
                packingTSize.Ratio = Int32.Parse(ratioT);
                tallaItem.PackingTypeSize = packingTSize;


                objPacking.ActualizarCantidadesPPK(tallaItem);


            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Obtener_Lista_Tallas_Packing_PPK(List<string> ListTalla, int EstiloID, string TipoPackID)
        {
            PackingM tallaItem = new PackingM();
            PackingTypeSize packingTSize = new PackingTypeSize();
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            tallaItem.Usuario = noEmpleado;
            packingTSize.IdSummary = EstiloID;
            packingTSize.IdTipoEmpaque = Int32.Parse(TipoPackID);
            packingTSize.PackingName = "";
            packingTSize.AssortName = "";
            packingTSize.NumUsuario = noEmpleado;
            // int numBatch = objPacking.ObtenerIdBatch(EstiloID);
            // tallaItem.IdBatch = numBatch + 1;
            List<string> tallas = ListTalla[0].Split('*').ToList();
            List<string> ratio = ListTalla[1].Split('*').ToList();
            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }

            i -= 1;
            string talla;
            for (int v = 0; v < i; v++)
            {
                talla = tallas[v];
                packingTSize.IdTalla = objTalla.ObtenerIdTalla(talla);
                string ratioT = ratio[v];
                if (ratioT == "")
                {
                    ratioT = "0";
                }
                packingTSize.Ratio = Int32.Parse(ratioT);
                tallaItem.PackingTypeSize = packingTSize;


                objPacking.AgregarTallasTypePack(tallaItem);


            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Actualizar_Lista_Tallas_Packing_Varios_PPK(List<string> ListTalla, int NumeroPcs, string NombrePacking)
        {
            PackingM tallaItem = new PackingM();
            PackingTypeSize packingTSize = new PackingTypeSize();
            List<string> numPack = ListTalla[0].Split('*').ToList();
            List<string> tallas = ListTalla[1].Split('*').ToList();
            List<string> cantidad = ListTalla[2].Split('*').ToList();
            List<string> ratio = ListTalla[3].Split('*').ToList();
            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }

            i -= 1;
            for (int v = 0; v < i; v++)
            {
                string NumPackT = numPack[v];
                if (NumPackT == "")
                {
                    NumPackT = "0";
                }
                packingTSize.IdPackingTypeSize = Int32.Parse(NumPackT);
                string ratioT = ratio[v];
                if (ratioT == "")
                {
                    ratioT = "0";
                }
                packingTSize.Ratio = Int32.Parse(ratioT);
                string cantidadT = cantidad[v];
                if (cantidadT == "")
                {
                    cantidadT = "0";
                }
                packingTSize.Cantidad = Int32.Parse(cantidadT);
                packingTSize.NumberPPKs = NumeroPcs;
                packingTSize.NombrePackingPPKs = NombrePacking;

                packingTSize.TotalCartones = packingTSize.Cantidad / packingTSize.Ratio;
                tallaItem.PackingTypeSize = packingTSize;


                objPacking.ActualizarCantidadesVariosPPK(tallaItem);


            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Actualizar_Lista_Tallas_Packing_Varios_BULKS(List<string> ListTalla, string NombrePacking)
        {
            PackingM tallaItem = new PackingM();
            PackingTypeSize packingTSize = new PackingTypeSize();
            List<string> numPack = ListTalla[0].Split('*').ToList();
            List<string> tallas = ListTalla[1].Split('*').ToList();
            List<string> cantidad = ListTalla[2].Split('*').ToList();
            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }

            i -= 1;
            for (int v = 0; v < i; v++)
            {
                string NumPackT = numPack[v];
                if (NumPackT == "")
                {
                    NumPackT = "0";
                }
                packingTSize.IdPackingTypeSize = Int32.Parse(NumPackT);
                string cantidadT = cantidad[v];
                if (cantidadT == "")
                {
                    cantidadT = "0";
                }
                packingTSize.Pieces = Int32.Parse(cantidadT);
                packingTSize.NombrePackingBulks = NombrePacking;

                tallaItem.PackingTypeSize = packingTSize;

                objPacking.ActualizarCantidadesVariosBULKS(tallaItem);


            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Obtener_Lista_Tallas_Varios_Packing_PPK(List<string> ListTalla, int EstiloID, string TipoPackID, int NumeroPcs, string NombrePacking, int numPedido)
        {
            PackingM tallaItem = new PackingM();
            PackingTypeSize packingTSize = new PackingTypeSize();
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            tallaItem.Usuario = noEmpleado;
            packingTSize.IdSummary = EstiloID;
            int numeroblock = objPacking.ObtenerIdBlockPPKs(packingTSize.IdSummary/*, NombrePacking*/);
            packingTSize.IdBlockPack = numeroblock + 1;
            packingTSize.IdTipoEmpaque = Int32.Parse(TipoPackID);
            packingTSize.PackingName = NombrePacking;
            packingTSize.AssortName = "";
            packingTSize.NumberPKK = NumeroPcs;
            packingTSize.NumUsuario = noEmpleado;
            // int numBatch = objPacking.ObtenerIdBatch(EstiloID);
            // tallaItem.IdBatch = numBatch + 1;
            List<string> tallas = ListTalla[0].Split('*').ToList();
            List<string> cantidades = ListTalla[1].Split('*').ToList();
            List<string> ratio = ListTalla[2].Split('*').ToList();
            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }

            i -= 1;
            string talla;
            for (int v = 0; v < i; v++)
            {
                talla = tallas[v];
                packingTSize.IdTalla = objTalla.ObtenerIdTalla(talla);
                string ratioT = ratio[v];
                if (ratioT == "")
                {
                    ratioT = "0";
                }
                packingTSize.Ratio = Int32.Parse(ratioT);

                string cantidadesT = cantidades[v];
                if (cantidadesT == "")
                {
                    cantidadesT = "0";
                }
                packingTSize.Cantidad = Int32.Parse(cantidadesT);
                tallaItem.PackingTypeSize = packingTSize;
                if (packingTSize.TotalCartones == 0)
                {
                    packingTSize.TotalCartones = packingTSize.Cantidad / packingTSize.Ratio;
                }

                objPacking.AgregarTallasTypePack(tallaItem);


            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }



        [HttpPost]
        public JsonResult Obtener_Lista_Tallas_Varios_Packing_Bulk(List<string> ListTalla, int EstiloID, string TipoPackID, string NombrePacking, int numPedido)
        {
            PackingM tallaItem = new PackingM();
            PackingTypeSize packingTSize = new PackingTypeSize();
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            tallaItem.Usuario = noEmpleado;
            packingTSize.IdSummary = EstiloID;
            int numeroblock = objPacking.ObtenerIdBlockPPKs(packingTSize.IdSummary/*, NombrePacking*/);
            packingTSize.IdBlockPack = numeroblock + 1;
            packingTSize.IdTipoEmpaque = Int32.Parse(TipoPackID);
            packingTSize.PackingName = NombrePacking;
            packingTSize.AssortName = "";
            packingTSize.NumUsuario = noEmpleado;
            // int numBatch = objPacking.ObtenerIdBatch(EstiloID);
            // tallaItem.IdBatch = numBatch + 1;
            List<string> tallas = ListTalla[0].Split('*').ToList();
            List<string> cantidades = ListTalla[1].Split('*').ToList();
            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }

            i -= 1;
            string talla;
            for (int v = 0; v < i; v++)
            {
                talla = tallas[v];
                packingTSize.IdTalla = objTalla.ObtenerIdTalla(talla);
                string cantidadesPiezasT = cantidades[v];
                if (cantidadesPiezasT == "")
                {
                    cantidadesPiezasT = "0";
                }
                packingTSize.Pieces = Int32.Parse(cantidadesPiezasT);
                tallaItem.PackingTypeSize = packingTSize;


                objPacking.AgregarTallasTypePack(tallaItem);


            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Obtener_Lista_Tallas_Packing_Assort(List<string> ListTalla, int EstiloID, string PackingName, string AssortName, int NumQty, int NumCart, int TotalUnidades)
        {
            PackingM tallaItem = new PackingM();
            PackingTypeSize packingTSize = new PackingTypeSize();
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            tallaItem.Usuario = noEmpleado;
            packingTSize.IdSummary = EstiloID;
            packingTSize.IdTipoEmpaque = 3;
            packingTSize.PackingName = PackingName;
            packingTSize.AssortName = AssortName;
            packingTSize.Pieces = NumQty;
            packingTSize.TotalCartones = NumCart;
            packingTSize.TotalUnits = TotalUnidades;
            packingTSize.NumUsuario = noEmpleado;
            packingTSize.IdBlockPack = Convert.ToInt32(Session["id_Block"]) + 1;
            // int numBatch = objPacking.ObtenerIdBatch(EstiloID);
            // tallaItem.IdBatch = numBatch + 1;
            List<string> tallas = ListTalla[0].Split('*').ToList();
            List<string> ratio = ListTalla[1].Split('*').ToList();
            List<string> piezas = ListTalla[2].Split('*').ToList();
            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }

            i -= 1;
            string talla;
            for (int v = 0; v < i; v++)
            {
                talla = tallas[v];
                packingTSize.IdTalla = objTalla.ObtenerIdTalla(talla);
                string ratioT = ratio[v];
                if (ratioT == "")
                {
                    ratioT = "0";
                }
                packingTSize.Ratio = Int32.Parse(ratioT);

                string piezasT = piezas[v];
                if (piezasT == "")
                {
                    piezasT = "0";
                }
                packingTSize.TotalPieces = Int32.Parse(piezasT);
                tallaItem.PackingTypeSize = packingTSize;


                objPacking.AgregarTallasTypePack(tallaItem);


            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Obtener_Lista_Tallas_Packing_Pallet(List<string> ListTalla, int EstiloID, int TipoTurnoID, int NumCaja, string TipoEmpaque)
        {
            PackingM tallaItem = new PackingM();
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            tallaItem.Usuario = noEmpleado;
            tallaItem.IdSummary = EstiloID;
            tallaItem.IdTurno = TipoTurnoID;//Int32.Parse(TipoTurnoID);           
            int numBatch = objPacking.ObtenerIdBatch(EstiloID);
            tallaItem.IdBatch = numBatch + 1;
            tallaItem.NombreEmpaque = "";

            List<string> idPack = ListTalla[0].Split('*').ToList();
            List<string> tallas = ListTalla[1].Split('*').ToList();

            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }
            i -= 1;
            string talla;
            string idPacking;
            if (TipoEmpaque == "BULK")
            {

                List<string> cajas = ListTalla[2].Split('*').ToList();
                List<string> partial = ListTalla[4].Split('*').ToList();
                List<string> totales = ListTalla[5].Split('*').ToList();
                for (int v = 0; v < i; v++)
                {
                    talla = tallas[v];
                    tallaItem.IdTalla = objTalla.ObtenerIdTalla(talla);
                    idPacking = idPack[v];
                    tallaItem.IdPackingTypeSize = Int32.Parse(idPacking);
                    string cantBox = cajas[v];
                    if (cantBox == "")
                    {
                        cantBox = "0";
                    }
                    tallaItem.CantBox = Int32.Parse(cantBox);
                    string totalP = totales[v];
                    if (totalP == "")
                    {
                        totalP = "0";
                    }
                    tallaItem.TotalPiezas = Int32.Parse(totalP);
                    string cantPartial = partial[v];
                    if (cantPartial == "")
                    {
                        cantPartial = "0";
                    }
                    tallaItem.Partial = Int32.Parse(cantPartial);
                    tallaItem.FechaPack = DateTime.Today;
                    objPacking.AgregarTallasPacking(tallaItem);
                }
            }
            else if (TipoEmpaque == "PPK")
            {
                List<string> totales = ListTalla[3].Split('*').ToList();
                for (int v = 0; v < i; v++)
                {
                    talla = tallas[v];
                    tallaItem.IdTalla = objTalla.ObtenerIdTalla(talla);
                    tallaItem.CantBox = NumCaja;
                    idPacking = idPack[v];
                    tallaItem.IdPackingTypeSize = Int32.Parse(idPacking);
                    string totalP = totales[v];
                    if (totalP == "")
                    {
                        totalP = "0";
                    }
                    tallaItem.TotalPiezas = Int32.Parse(totalP);

                    tallaItem.FechaPack = DateTime.Today;
                    objPacking.AgregarTallasPacking(tallaItem);
                }
            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Obtener_Lista_Tallas_Packing_PPKS_Pallet(List<string> ListTalla, int EstiloID, int TipoTurnoID, int NumCaja, string TipoEmpaque, string NamePack)
        {
            PackingM tallaItem = new PackingM();
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            tallaItem.Usuario = noEmpleado;
            tallaItem.IdSummary = EstiloID;
            tallaItem.IdTurno = TipoTurnoID;//Int32.Parse(TipoTurnoID);           
            int numBatch = objPacking.ObtenerIdBatch(tallaItem.IdSummary);
            tallaItem.IdBatch = numBatch + 1;
            tallaItem.NombreEmpaque = NamePack.TrimEnd();
            List<string> idPack = ListTalla[0].Split('*').ToList();
            List<string> tallas = ListTalla[1].Split('*').ToList();

            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }
            i -= 1;
            string talla;
            string idPacking;

            List<string> totales = ListTalla[3].Split('*').ToList();
            for (int v = 0; v < i; v++)
            {
                talla = tallas[v];
                tallaItem.IdTalla = objTalla.ObtenerIdTalla(talla);
                tallaItem.CantBox = NumCaja;
                idPacking = idPack[v];
                tallaItem.IdPackingTypeSize = Int32.Parse(idPacking);
                string totalP = totales[v];
                if (totalP == "")
                {
                    totalP = "0";
                }
                tallaItem.TotalPiezas = Int32.Parse(totalP);
                /*int numBatch = objPacking.ObtenerNumBatchPorId(tallaItem.IdPackingTypeSize);
				tallaItem.IdBatch = numBatch + 1;
				*/
                tallaItem.FechaPack = DateTime.Today;
                objPacking.AgregarTallasPacking(tallaItem);
            }

            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Obtener_Lista_Tallas_Packing_Bulks_Pallet(List<string> ListTalla, int EstiloID, int TipoTurnoID, string NamePack)
        {
            PackingM tallaItem = new PackingM();
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            tallaItem.Usuario = noEmpleado;
            tallaItem.IdSummary = EstiloID;
            tallaItem.IdTurno = TipoTurnoID;//Int32.Parse(TipoTurnoID);           
            int numBatch = objPacking.ObtenerIdBatch(tallaItem.IdSummary);
            tallaItem.IdBatch = numBatch + 1;
            tallaItem.NombreEmpaque = NamePack.TrimEnd();
            List<string> idPack = ListTalla[0].Split('*').ToList();
            List<string> tallas = ListTalla[1].Split('*').ToList();
            List<string> cantidad = ListTalla[2].Split('*').ToList();
            List<string> piezas = ListTalla[3].Split('*').ToList();

            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }
            i -= 1;
            string talla;
            string idPacking;


            for (int v = 0; v < i; v++)
            {
                talla = tallas[v];
                tallaItem.IdTalla = objTalla.ObtenerIdTalla(talla);
                idPacking = idPack[v];
                tallaItem.IdPackingTypeSize = Int32.Parse(idPacking);

                string cantidadT = cantidad[v];
                if (cantidadT == "")
                {
                    cantidadT = "0";
                }
                tallaItem.CantBox = Int32.Parse(cantidadT);

                string cantPiezas = piezas[v];
                if (cantPiezas == "")
                {
                    cantPiezas = "0";
                }
                tallaItem.TotalPiezas = Int32.Parse(cantPiezas);
                /*int numBatch = objPacking.ObtenerNumBatchPorId(tallaItem.IdPackingTypeSize);
				tallaItem.IdBatch = numBatch + 1;
				*/
                tallaItem.FechaPack = DateTime.Today;
                objPacking.AgregarTallasPacking(tallaItem);
            }

            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Obtener_Lista_Tallas_Packing_Assort_Pallet(string nomEstilo, int NumCartones, int numTotalP, string packName, int numBlock, int idPedido)
        {
            PackingM tallaItem = new PackingM();
            PackingAssortment packAssort = new PackingAssortment();
            ItemDescripcionData objItem = new ItemDescripcionData();
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            packAssort.Usuario = noEmpleado;
            packAssort.IdTurno = Convert.ToInt32(Session["noTurno"]);
            packAssort.PackingName = packName;
            packAssort.CantCartons = NumCartones;
            packAssort.TotalPiezas = numTotalP;
            packAssort.IdBlock = numBlock;
            packAssort.IdPedido = idPedido;
            int numeroEstilo = objItem.ObtenerIdEstilo(nomEstilo);
            packAssort.IdSummary = objItem.ObtenerIdSummaryPorIdEstilo(numeroEstilo, idPedido);
            //Int32.Parse(TipoTurnoID);           
            int numBatch = objPacking.ObtenerIdBatchAssort(idPedido, numBlock);
            packAssort.IdBatch = numBatch + 1;

            packAssort.FechaPackAssort = DateTime.Today;
            tallaItem.PackingAssort = packAssort;

            objPacking.AgregarPackingAssort(tallaItem);


            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Obtener_Lista_Tallas_Packing_HT_Pallet(List<string> ListTalla, int EstiloID, int TipoTurnoID, int NumeroPO, string TipoEmpaque, int NumeroCaja, int NumeroPPK)
        {
            PackingM tallaItem = new PackingM();
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            tallaItem.Usuario = noEmpleado;
            tallaItem.IdSummary = EstiloID;
            tallaItem.IdTurno = TipoTurnoID;//Int32.Parse(TipoTurnoID);           
            int numBatch = objPacking.ObtenerIdBatch(EstiloID);
            tallaItem.IdBatch = numBatch + 1;
            tallaItem.NombreEmpaque = "";
            // List<string> idPack = ListTalla[0].Split('*').ToList();
            List<string> tallas = ListTalla[0].Split('*').ToList();

            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }
            i -= 1;
            string talla;


            if (TipoEmpaque == "BULK")
            {

                List<string> cajas = ListTalla[1].Split('*').ToList();
                List<string> piezas = ListTalla[2].Split('*').ToList();
                // piezas = ListTalla[3].Split('*').ToList();
                //totales = ListTalla[4].Split('*').ToList();
                for (int v = 0; v < i; v++)
                {
                    talla = tallas[v];
                    tallaItem.IdTalla = objTalla.ObtenerIdTalla(talla);
                    tallaItem.IdPackingTypeSize = objPacking.ObtenerIdPackingtypeSize(1, tallaItem.IdSummary, tallaItem.IdTalla, NumeroPO);
                    //int caja= objPacking.ObtenerNoCajasBulkHT(tallaItem.IdSummary, tallaItem.IdTalla, NumeroPO, 1);

                    string cantBox = cajas[v];
                    if (cantBox == "")
                    {
                        cantBox = "0";
                    }
                    tallaItem.CantBox = Int32.Parse(cantBox);

                    string cantPiezas = piezas[v];
                    if (cantPiezas == "")
                    {
                        cantPiezas = "0";
                    }
                    int piezasTot = Int32.Parse(cantPiezas);
                    tallaItem.TotalPiezas = piezasTot;
                    // tallaItem.CantBox * piezasTot;
                    /*if (caja > 0)
                    {
                        tallaItem.CantBox = 0;
                    }*/
                    tallaItem.FechaPack = DateTime.Today;
                    objPacking.AgregarTallasPacking(tallaItem);
                }
            }
            else if (TipoEmpaque == "PPK")
            {

                //   piezas = ListTalla[2].Split('*').ToList();
                List<string> totales = ListTalla[2].Split('*').ToList();
                for (int v = 0; v < i - 1; v++)
                {
                    talla = tallas[v];
                    tallaItem.IdTalla = objTalla.ObtenerIdTalla(talla);
                    tallaItem.CantBox = NumeroCaja;
                    tallaItem.CantidadPPKS = NumeroPPK;
                    tallaItem.IdPackingTypeSize = objPacking.ObtenerIdPackingtypeSize(2, tallaItem.IdSummary, tallaItem.IdTalla, NumeroPO);
                    //idPacking = idPack[v];
                    // tallaItem.IdPackingTypeSize = Int32.Parse(idPacking);
                    string totalP = totales[v];
                    if (totalP == "")
                    {
                        totalP = "0";
                    }
                    tallaItem.TotalPiezas = Int32.Parse(totalP);
                    tallaItem.FechaPack = DateTime.Today;
                    objPacking.AgregarTallasPacking(tallaItem);
                }
            }
            return Json("0", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Obtener_Lista_Tallas_Packing_Bulk_HT(List<string> ListTalla, int EstiloID, string FormaPackID, int NumberPOID)
        {
            PackingM tallaItem = new PackingM();
            PackingTypeSize packingTSize = new PackingTypeSize();
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            tallaItem.Usuario = noEmpleado;
            packingTSize.IdSummary = EstiloID;
            packingTSize.IdTipoEmpaque = 1;
            packingTSize.IdFormaEmpaque = Int32.Parse(FormaPackID);
            packingTSize.NumberPO = NumberPOID;
            packingTSize.PackingName = "";
            packingTSize.AssortName = "";
            packingTSize.NumUsuario = noEmpleado;
            // int numBatch = objPacking.ObtenerIdBatch(EstiloID);
            // tallaItem.IdBatch = numBatch + 1;
            List<string> tallas = ListTalla[0].Split('*').ToList();
            List<string> cantidad = ListTalla[1].Split('*').ToList();
            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }

            i -= 1;
            string talla;
            for (int v = 0; v < i; v++)
            {
                talla = tallas[v];
                packingTSize.IdTalla = objTalla.ObtenerIdTalla(talla);
                string cantidadT = cantidad[v];
                if (cantidadT == "")
                {
                    cantidadT = "0";
                }
                packingTSize.Cantidad = Int32.Parse(cantidadT);
                List<string> cartones = ListTalla[2].Split('*').ToList();
                string cartonesT = cartones[v];
                if (cartonesT == "")
                {
                    cartonesT = "0";
                }
                packingTSize.Cartones = Int32.Parse(cartonesT);
                List<string> partiales = ListTalla[3].Split('*').ToList();
                string parcialesT = partiales[v];
                if (parcialesT == "")
                {
                    parcialesT = "0";
                }
                packingTSize.PartialNumber = Int32.Parse(parcialesT);
                List<string> totalCartones = ListTalla[4].Split('*').ToList();
                string cartonesTotal = totalCartones[v];
                if (cartonesTotal == "")
                {
                    cartonesTotal = "0";
                }
                packingTSize.TotalCartones = Int32.Parse(cartonesTotal);
                tallaItem.PackingTypeSize = packingTSize;

                objPacking.AgregarTallasTypePack(tallaItem);

            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Actualizar_Lista_Tallas_Packing_Bulk_HT(List<string> ListTalla, int EstiloID, int NumberPOID)
        {
            PackingM tallaItem = new PackingM();
            PackingTypeSize packingTSize = new PackingTypeSize();
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            tallaItem.Usuario = noEmpleado;
            packingTSize.IdSummary = EstiloID;
            packingTSize.NumberPO = NumberPOID;
            // int numBatch = objPacking.ObtenerIdBatch(EstiloID);
            // tallaItem.IdBatch = numBatch + 1;
            List<string> numPack = ListTalla[0].Split('*').ToList();
            List<string> tallas = ListTalla[1].Split('*').ToList();
            List<string> cantidad = ListTalla[2].Split('*').ToList();
            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }

            i -= 1;
            string talla;
            for (int v = 0; v < i; v++)
            {
                talla = tallas[v];
                packingTSize.IdTalla = objTalla.ObtenerIdTalla(talla);
                string NumPackT = numPack[v];
                if (NumPackT == "")
                {
                    NumPackT = "0";
                }
                packingTSize.IdPackingTypeSize = Int32.Parse(NumPackT);
                string cantidadT = cantidad[v];
                if (cantidadT == "")
                {
                    cantidadT = "0";
                }
                packingTSize.Cantidad = Int32.Parse(cantidadT);
                List<string> cartones = ListTalla[3].Split('*').ToList();
                string cartonesT = cartones[v];
                if (cartonesT == "")
                {
                    cartonesT = "0";
                }
                packingTSize.Cartones = Int32.Parse(cartonesT);
                List<string> partiales = ListTalla[4].Split('*').ToList();
                string parcialesT = partiales[v];
                if (parcialesT == "")
                {
                    parcialesT = "0";
                }
                packingTSize.PartialNumber = Int32.Parse(parcialesT);
                List<string> totalCartones = ListTalla[5].Split('*').ToList();
                string cartonesTotal = totalCartones[v];
                if (cartonesTotal == "")
                {
                    cartonesTotal = "0";
                }
                packingTSize.TotalCartones = Int32.Parse(cartonesTotal);
                tallaItem.PackingTypeSize = packingTSize;

                objPacking.ActualizarCantidadesPackBulkHT(tallaItem);

            }
            return Json("0", JsonRequestBehavior.AllowGet);


        }

        [HttpPost]
        public JsonResult Obtener_Lista_Tallas_Packing_PPK_HT(List<string> ListTalla, int EstiloID, int NumberPOID, int NumberTotU)
        {
            PackingM tallaItem = new PackingM();
            PackingTypeSize packingTSize = new PackingTypeSize();
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            tallaItem.Usuario = noEmpleado;
            packingTSize.IdSummary = EstiloID;
            packingTSize.IdTipoEmpaque = 2;
            packingTSize.NumberPO = NumberPOID;
            packingTSize.TotalUnits = NumberTotU;
            packingTSize.PackingName = "";
            packingTSize.AssortName = "";
            packingTSize.NumUsuario = noEmpleado;
            // int numBatch = objPacking.ObtenerIdBatch(EstiloID);
            // tallaItem.IdBatch = numBatch + 1;
            List<string> tallas = ListTalla[0].Split('*').ToList();
            List<string> ratio = ListTalla[1].Split('*').ToList();
            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }

            i -= 1;
            string talla;
            for (int v = 0; v < i; v++)
            {
                talla = tallas[v];
                packingTSize.IdTalla = objTalla.ObtenerIdTalla(talla);
                string ratioT = ratio[v];
                if (ratioT == "")
                {
                    ratioT = "0";
                }
                packingTSize.Ratio = Int32.Parse(ratioT);
                tallaItem.PackingTypeSize = packingTSize;


                objPacking.AgregarTallasTypePack(tallaItem);


            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Actualizar_Lista_Tallas_Packing_PPK_HT(List<string> ListTalla, int EstiloID, int NumberPOID, int NumberTotU)
        {
            PackingM tallaItem = new PackingM();
            PackingTypeSize packingTSize = new PackingTypeSize();
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            tallaItem.Usuario = noEmpleado;
            packingTSize.IdSummary = EstiloID;
            packingTSize.IdTipoEmpaque = 2;
            packingTSize.NumberPO = NumberPOID;
            packingTSize.TotalUnitsPPKActHT = NumberTotU;
            packingTSize.PackingName = "";
            packingTSize.AssortName = "";
            // int numBatch = objPacking.ObtenerIdBatch(EstiloID);
            // tallaItem.IdBatch = numBatch + 1;
            List<string> numPack = ListTalla[0].Split('*').ToList();
            List<string> tallas = ListTalla[1].Split('*').ToList();
            List<string> ratio = ListTalla[2].Split('*').ToList();
            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }

            i -= 1;
            string talla;
            for (int v = 0; v < i; v++)
            {
                talla = tallas[v];
                packingTSize.IdTalla = objTalla.ObtenerIdTalla(talla);
                string NumPackT = numPack[v];
                if (NumPackT == "")
                {
                    NumPackT = "0";
                }
                packingTSize.IdPackingTypeSize = Int32.Parse(NumPackT);
                string ratioT = ratio[v];
                if (ratioT == "")
                {
                    ratioT = "0";
                }
                packingTSize.Ratio = Int32.Parse(ratioT);
                tallaItem.PackingTypeSize = packingTSize;


                objPacking.ActualizarCantidadesPackPPKHT(tallaItem);


            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }
        //Actualizar cantidades de primera calidad para empaque
        [HttpPost]
        public JsonResult Actualizar_Cantidades_Primera_Calidad_Empaque(List<string> ListTalla)
        {
            PackingM tallaItem = new PackingM();
            PackingSize packingSize = new PackingSize();
            // int numBatch = objPacking.ObtenerIdBatch(EstiloID);
            // tallaItem.IdBatch = numBatch + 1;
            List<string> numPack = ListTalla[0].Split('*').ToList();
            List<string> tallas = ListTalla[1].Split('*').ToList();
            List<string> cantidades = ListTalla[2].Split('*').ToList();
            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }

            i -= 1;
            for (int v = 0; v < i; v++)
            {
                string NumPackT = numPack[v];
                if (NumPackT == "")
                {
                    NumPackT = "0";
                }
                packingSize.IdPackingSize = Int32.Parse(NumPackT);
                string cantidadT = cantidades[v];
                if (cantidadT == "")
                {
                    cantidadT = "0";
                }
                packingSize.Calidad = Int32.Parse(cantidadT);
                tallaItem.PackingSize = packingSize;


                objPacking.ActualizarCantidadesPCEmpaque(tallaItem);


            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Actualizar_Lista_Tallas_Batch(List<string> ListTalla, int TipoTurnoID, int EstiloID, int IdBatch, int NumCaja, string TipoEmpaque)
        {
            PackingM tallaItem = new PackingM();
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            tallaItem.UsuarioModif = noEmpleado;
            tallaItem.Usuario = noEmpleado;
            tallaItem.IdSummary = EstiloID;
            tallaItem.IdTurno = TipoTurnoID;//Int32.Parse(TipoTurnoID);           
            tallaItem.IdBatch = IdBatch;
            List<string> idPack = ListTalla[0].Split('*').ToList();
            List<string> tallas = ListTalla[1].Split('*').ToList();

            int i = 0;
            foreach (var item in tallas)
            {
                i++;
            }
            i -= 1;
            string talla;
            string idPacking;
            if (TipoEmpaque == "BULK")
            {
                List<string> cajas = ListTalla[2].Split('*').ToList();
                List<string> partial = ListTalla[4].Split('*').ToList();
                List<string> totales = ListTalla[5].Split('*').ToList();
                for (int v = 0; v < i; v++)
                {
                    talla = tallas[v];
                    tallaItem.IdTalla = objTalla.ObtenerIdTalla(talla);
                    idPacking = idPack[v];
                    tallaItem.IdPacking = Int32.Parse(idPacking);
                    tallaItem.IdPackingTypeSize = objPacking.ObtenerIdPackingSize(tallaItem.IdPacking);
                    tallaItem.Usuario = objPacking.ObtenerIdUsuarioPorBatchEstilo(tallaItem.IdBatch, tallaItem.IdSummary, tallaItem.IdTalla);
                    string cantBox = cajas[v];
                    if (cantBox == "")
                    {
                        cantBox = "0";
                    }
                    tallaItem.CantBox = Int32.Parse(cantBox);
                    string totalP = totales[v];
                    if (totalP == "")
                    {
                        totalP = "0";
                    }
                    tallaItem.TotalPiezas = Int32.Parse(totalP);
                    string cantPartial = partial[v];
                    if (cantPartial == "")
                    {
                        cantPartial = "0";
                    }
                    tallaItem.Partial = Int32.Parse(cantPartial);

                    objPacking.ActualizarTallasPacking(tallaItem);


                }
            }
            else if (TipoEmpaque == "PPK")
            {
                List<string> totales = ListTalla[3].Split('*').ToList();
                for (int v = 0; v < i; v++)
                {
                    talla = tallas[v];
                    tallaItem.IdTalla = objTalla.ObtenerIdTalla(talla);
                    tallaItem.CantBox = NumCaja;
                    idPacking = idPack[v];
                    tallaItem.IdPacking = Int32.Parse(idPacking);
                    tallaItem.IdPackingTypeSize = objPacking.ObtenerIdPackingSize(tallaItem.IdPacking);
                    tallaItem.Usuario = objPacking.ObtenerIdUsuarioPorBatchEstilo(tallaItem.IdBatch, tallaItem.IdSummary, tallaItem.IdTalla);
                    string totalP = totales[v];
                    if (totalP == "")
                    {
                        totalP = "0";
                    }
                    tallaItem.TotalPiezas = Int32.Parse(totalP);


                    objPacking.ActualizarTallasPacking(tallaItem);


                }
            }
            return Json("0", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Lista_Batch_Estilo(int? id, int tipoEmpaque)
        {
            List<PackingM> listaBatch = objPacking.ListaBatch(id, tipoEmpaque).ToList();
            int cargo = Convert.ToInt32(Session["idCargo"]);
            string sucursalName = ((string)Session["sucursal"]);
            var result = Json(new { listaPO = listaBatch, cargoUser = cargo, sucursal = sucursalName });
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Lista_Batch_HT_Estilo(int? id)
        {
            List<PackingM> listaBatch = objPacking.ListaBatchHT(id).ToList();
            int cargo = Convert.ToInt32(Session["idCargo"]);
            var result = Json(new { listaPO = listaBatch , cargoUser = cargo, });
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Actualizar_Tipo_Packing(int EstiloID, int tipoEmpaque)
        {
            List<PackingTypeSize> listaInfoPacking = objPacking.ListaInfoPacking(EstiloID, tipoEmpaque).ToList();


            PackingTypeSize empaque = new PackingTypeSize
            {
                ListaEmpaque = listaInfoPacking
            };
            if (tipoEmpaque == 1)
            {
                foreach (var item in empaque.ListaEmpaque)
                {
                    empaque.IdPackingTypeSize = item.IdPackingTypeSize;
                    empaque.Pieces = item.Pieces;
                    objPacking.CambiarTipoPackBulk(empaque.IdPackingTypeSize, empaque.Pieces);

                }
            }
            else if (tipoEmpaque == 2)
            {
                foreach (var item in empaque.ListaEmpaque)
                {
                    empaque.IdPackingTypeSize = item.IdPackingTypeSize;
                    empaque.Ratio = item.Ratio;
                    objPacking.CambiarTipoPackPPK(empaque.IdPackingTypeSize, empaque.Ratio);

                }
            }

            return Json("0", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Batch_Assort(int numBlock, int idPedido)
        {

            List<PackingAssortment> listaBatch = objPacking.ListaBatchAssort(numBlock, idPedido).ToList();
            int cargo = Convert.ToInt32(Session["idCargo"]);
            var result = Json(new { listaTalla = listaBatch, cargoUser = cargo });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EliminarBatchAssort(int idBatch)
        {
            objPacking.EliminarInfoBatch(idBatch);
            TempData["eliminarBatch"] = "The Pallet was delete correctly.";
            return Json("0", JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult EliminarBatchPackings(int idBatch, int idSummary)
        {
            objPacking.EliminarInfoBatchPackings(idBatch, idSummary);
            TempData["eliminarBatch"] = "The Pallet was delete correctly.";
            return Json("0", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EliminarBatchHT(int idBatch, int idSummary)
        {
            objPacking.EliminarInfoBatchPackings(idBatch, idSummary);
            TempData["eliminarBatch"] = "The Pallet was delete correctly.";
            return Json("0", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Packing_IdEstilo_IdBatch(int? idEstilo, int idBatch)
        {
            List<PackingM> listaCantidadesTallasBatch = objPacking.ListaCantidadesTallaPorIdBatchEstilo(idEstilo, idBatch).ToList();
            List<int> listaTallasTBatch = objPacking.ListaTotalTallasPackingBatchEstilo(idEstilo).ToList();
            List<PackingTypeSize> listaTallasEmpaque = objPacking.ObtenerCajasPackingPorEstilo(idEstilo, idBatch);
            var result = Json(new { listaTalla = listaCantidadesTallasBatch, listaPrint = listaTallasTBatch, listaEmpaqueTallas = listaTallasEmpaque, });
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Lista_Tallas_Packing_PPK_IdEstilo_IdBatch(int? idEstilo, int idBatch)
        {
            List<PackingM> listaCantidadesTallasBatch = objPacking.ListaCantidadesTallaPorIdBatchEstilo(idEstilo, idBatch).ToList();
            List<int> listaTallasTBatch = objPacking.ListaTotalTallasPackingBatchEstilo(idEstilo).ToList();
            List<PackingTypeSize> listaTallasEmpaque = objPacking.ObtenerCajasPackingPPKPorEstilo(idEstilo, idBatch);
            var result = Json(new { listaTalla = listaCantidadesTallasBatch, listaPrint = listaTallasTBatch, listaEmpaqueTallas = listaTallasEmpaque, });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Lista_Tallas_Batch(int? id)
        {
            List<PackingM> listaTallas = objPacking.ListaTallasBatchId(id).ToList();

            var result = Json(new { listaPO = listaTallas });
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Lista_Tallas_Por_Estilo_Packing(int? id)
        {
            OrdenesCompra pedido = new OrdenesCompra();
            List<PackingM> listaTallasEstilo = objPacking.ObtenerTallas(id).ToList();
            List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstilo(id).ToList();
            List<ItemTalla> listaTallasPack = objTallas.ListaTallasPacking(id).ToList();
            List<int> listaCajasPacking = objPacking.ListaTotalTallasPackingBatchEstilo(id).ToList();
            List<int> listaPartialPacking = objPacking.ListaTallasPartialPackingBulkEstilo(id).ToList();
            List<PackingSize> listaTallasPacking = objPacking.ObtenerListaPackingSizePorEstilo(id).ToList();
            List<PackingTypeSize> listaTallasEmpaque = objPacking.ObtenerListaPackingTypeSizePorEstilo(id).ToList();
            List<PackingTypeSize> listaTotalPiezasTallas = objPacking.ListaTotalPiezasTallasAssortPorEstilo(id).ToList();
            List<ItemTalla> listCantidadesTallas = objTallas.ListaCantidadesTallasPorEstilo(id).ToList();
            List<PackingTypeSize> listaTallasEmpaquePPK = objPacking.ObtenerListaPackingVariosPPKS(id).ToList();
            List<PackingTypeSize> listaTallasEmpaqueBulk = objPacking.ObtenerListaPackingVariosBulks(id).ToList();
            List<int> listaTallasCBatch = new List<int>();

            if (listaCajasPacking.Count != 0)
            {
                listaTallasCBatch = objPacking.ListaTotalCajasTallasBatchEstilo(id).ToList();
            }
            List<int> listaBatchPPKS = new List<int>();
            if (listaCajasPacking.Count != 0)
            {
                listaBatchPPKS = objPacking.ListaTotalCajasTallasBatchEstilo(id).ToList();
            }
            string estilo = "";
            foreach (var item in listaTallas)
            {
                estilo = item.Estilo;

            }

            int idPedido = objInv.obtener_id_pedido_summary(Convert.ToInt32(id));
            pedido = objPedido.ObtenerPedido(idPedido);
            ListaPackingRegistradosPPKs(pedido, id);
            //int idPedido = Convert.ToInt32(Session["id_Pedido"]);
            int totalPiezasEstilos = objSummary.ObtenerPiezasTotalesEstilos(idPedido);
            int totalPiezasPack = objSummary.ObtenerPiezasTotalesPorPackAssort(id);
            int cargo = Convert.ToInt32(Session["idCargo"]);
            var result = Json(new
            {
                lista = listaTallas,
                listaTalla = listaTallasEstilo,
                listaPackingS = listaTallasPacking,
                listaEmpaqueTallas = listaTallasEmpaque,
                listaTotalCajasPack = listaCajasPacking,
                listaCajasT = listaTallasCBatch,
                estilos = estilo,
                cargoUser = cargo,
                numTPSyle = totalPiezasEstilos,
                numTPack = totalPiezasPack,
                listaTotalPiezas = listaTotalPiezasTallas,
                listCantTalla = listCantidadesTallas,
                listaPartial = listaPartialPacking,
                listaPack = listaTallasPack,
                listaEmpPPKS = listaTallasEmpaquePPK,
                listaEmpBULKS = listaTallasEmpaqueBulk
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Info_Packing_Grafica(int? id)
        {
            //List<ItemTalla> listaTallas = objTallas.ObtenerListaPackingSizePorEstilo(id).ToList();
            List<PackingSize> listaTallas = objPacking.ObtenerListaPackingSizePorEstilo(id).ToList();
            List<PackingM> listaTallasTPackingBatch = objPacking.ListaTotalTallasPackingBatch(id).ToList();

            foreach (var item in listaTallas)
            {
                int CantidadBatch = 0;
                double CantidadGeneral = 0;
                double Cantidadfinal = 0;
                foreach (var itemBatch in listaTallasTPackingBatch)
                {
                    if (item.IdTalla == itemBatch.IdTalla)
                    {
                        CantidadBatch = itemBatch.TotalBatch;
                        //CantidadPrint = item.Cantidad - CantidadBatch;
                        CantidadGeneral = (CantidadBatch * 100);
                        Cantidadfinal = Math.Round(CantidadGeneral / item.Calidad, 2);
                    }

                }
                //item.Total = CantidadGeneral;
                item.Porcentaje = Cantidadfinal;

            }
            var result = Json(new
            {
                listaTalla = listaTallas,
                listaTallasTotalBatch = listaTallasTPackingBatch

            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Info_PackingHT_Grafica(int? id)
        {
            List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstilo(id).ToList();
            //List<ItemTalla> listaTallas = objTallas.ObtenerListaPackingSizePorEstilo(id).ToList();
            List<int> listaBatch = objPacking.ObtenerListaBatchIdSummary(id).ToList();
            string valores = "";
            for (int v = 0; v < listaBatch.Count; v++)
            {
                if (v > 0)
                {
                    valores += "," + listaBatch[v];
                }
                else
                {
                    valores += listaBatch[v];
                }
            }

            string query = valores;

            List<PackingM> listaTallasTPackingBatch = objPacking.ListaTotalTallasPackingBatchHT(id, query).ToList();

            foreach (var item in listaTallas)
            {
                int CantidadBatch = 0;
                double CantidadGeneral = 0;
                double Cantidadfinal = 0;
                foreach (var itemBatch in listaTallasTPackingBatch)
                {
                    if (item.IdTalla == itemBatch.IdTalla)
                    {
                        CantidadBatch = itemBatch.SumaTotalBatch;
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
                listaTallasTotalBatch = listaTallasTPackingBatch

            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Por_Estilo_BULK(int? id)
        {
            List<PackingM> listaTallasEstilo = objPacking.ObtenerTallas(id).ToList();
            List<PackingSize> listaTallasPacking = objPacking.ObtenerListaPackingSizePorEstilo(id).ToList();
            List<PackingTypeSize> listaTallasEmpaque = objPacking.ObtenerListaPackingTypeSizePorEstilo(id).ToList();

            var result = Json(new
            {

                listaTalla = listaTallasEstilo,
                listaPackingS = listaTallasPacking,
                listaEmpaqueTallas = listaTallasEmpaque

            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Cantidades_Primera_Calidad_Packing(int? id)
        {
            List<PackingM> listaTallasEstilo = objPacking.ObtenerTallas(id).ToList();
            List<PackingSize> listaTallasPacking = objPacking.ObtenerListaPackingSizePorEstilo(id).ToList();
            List<PackingTypeSize> listaTallasEmpaque = objPacking.ObtenerListaPackingTypeSizePorEstilo(id).ToList();
            List<ItemTalla> listaTallasPack = objTallas.ListaTallasPacking(id).ToList();
            List<PackingTypeSize> listaTotalPiezasTallas = objPacking.ListaTotalPiezasTallasAssortPorEstilo(id).ToList();

            var result = Json(new
            {

                lista = listaTallasEstilo,
                listaPackingS = listaTallasPacking,
                listaEmpaqueTallas = listaTallasEmpaque,
                listaPack = listaTallasPack,
                listaTotalPiezas = listaTotalPiezasTallas

            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Por_Estilo_VariosPPK(int? id, string packingName)
        {

            List<PackingTypeSize> listaTallasEmpaque = objPacking.ObtenerListaPackingTypePPKsSizePorEstilo(id, packingName).ToList();

            var result = Json(new
            {

                listaEmpaqueTallas = listaTallasEmpaque

            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Por_Estilo_VariosBulks(int? id, string packingName)
        {

            List<PackingTypeSize> listaTallasEmpaque = objPacking.ObtenerListaPackingTypeBulksSizePorEstilo(id, packingName).ToList();

            var result = Json(new
            {

                listaEmpaqueTallas = listaTallasEmpaque

            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Por_Estilo_PPK(int? id)
        {
            List<PackingM> listaTallasEstilo = objPacking.ObtenerTallas(id).ToList();
            List<PackingSize> listaTallasPacking = objPacking.ObtenerListaPackingSizePorEstilo(id).ToList();
            List<PackingTypeSize> listaTallasEmpaque = objPacking.ObtenerListaPackingTypeSizePorEstilo(id).ToList();

            var result = Json(new
            {

                listaTalla = listaTallasEstilo,
                listaPackingS = listaTallasPacking,
                listaEmpaqueTallas = listaTallasEmpaque

            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Lista_Tallas_Empaque_Por_Estilo(int? id)
        {
            List<PackingSize> listaTallasEstilo = objPacking.ListaTallasCalidadPack(id).ToList();
            List<PackingTypeSize> listaTallasPacking = objPacking.ObtenerListaPackingTypeSizePorEstilo(id).ToList();
            var result = Json(new { listaTalla = listaTallasEstilo, listaPackingS = listaTallasPacking });
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Lista_Tallas_Empaque_Edicion_Por_Estilo(int? id, int TipoEmp)
        {
            List<PackingSize> listaTallasEstilo = objPacking.ListaTallasCalidadPack(id).ToList();
            List<PackingTypeSize> listaTallasPacking = objPacking.ObtenerListaPackingTypeSizePorEstiloTipoEmp(id, TipoEmp).ToList();
            var result = Json(new { listaTalla = listaTallasEstilo, listaPackingS = listaTallasPacking });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Empaque_Varios_PPK_Por_Estilo(int? id, string packName)
        {

            List<PackingTypeSize> listaTallasPacking = objPacking.ObtenerListaPackingTypePPKsSizePorEstilo(id, packName).ToList();
            int NumCartones = 0;
            foreach (var item in listaTallasPacking)
            {
                if (NumCartones == 0)
                {
                    NumCartones = item.TotalCartones;
                }
            }
            var result = Json(new { listaPackingS = listaTallasPacking, totalCartones = NumCartones });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Empaque_Varios_Bulks_Por_Estilo(int? id, string packName)
        {

            List<PackingTypeSize> listaTallasPacking = objPacking.ObtenerListaPackingTypeBulksSizePorEstilo(id, packName).ToList();
            int NumCartones = 0;
            foreach (var item in listaTallasPacking)
            {
                if (NumCartones == 0)
                {
                    NumCartones = item.TotalCartones;
                }
            }
            var result = Json(new { listaPackingS = listaTallasPacking, totalCartones = NumCartones });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Empaque_HT_Por_Estilo(int? estiloId, int nPO, int tEmpaque)
        {
            List<ItemTalla> listaTallasPO = objTallas.ListaTallasPorEstilo(estiloId).ToList();
            List<PackingSize> listaTallasEstilo = objPacking.ListaTallasCalidadPack(estiloId).ToList();
            List<PackingTypeSize> listaTallasPacking = objPacking.ObtenerListaPackingTypeHTPorEstilo(estiloId, nPO, tEmpaque).ToList();
            List<PackingTypeSize> listaTallasPackingBox = objPacking.ObtenerListaPackingTypeBulkHTBox(estiloId, nPO, tEmpaque).ToList();


            var result = Json(new { lista = listaTallasPO, listaTalla = listaTallasEstilo, listaPackingS = listaTallasPacking, listaPackingBox = listaTallasPackingBox });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Empaque_HT_Registrar_Por_Estilo(int? idEst)
        {
            List<ItemTalla> listaTallasPO = objTallas.ListaTallasPorEstilo(idEst).ToList();

            var result = Json(new { lista = listaTallasPO });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Empaque_HT_PPK_Por_Estilo(int? estiloId, int nPO, int tEmpaque)
        {
            List<ItemTalla> listaTallasPO = objTallas.ListaTallasPorEstilo(estiloId).ToList();
            List<PackingSize> listaTallasEstilo = objPacking.ListaTallasCalidadPack(estiloId).ToList();
            List<PackingTypeSize> listaTallasPacking = objPacking.ObtenerListaPackingTypeHTPorEstilo(estiloId, nPO, tEmpaque).ToList();
            List<PackingTypeSize> listaTallasPackingBox = objPacking.ObtenerListaPackingTypeBulkHTBox(estiloId, nPO, tEmpaque).ToList();


            var result = Json(new { lista = listaTallasPO, listaTalla = listaTallasEstilo, listaPackingS = listaTallasPacking, listaPackingBox = listaTallasPackingBox });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Estilos_Empaque_Assort(string packingName)
        {
            int idPedido = Convert.ToInt32(Session["id_Pedido"]);
            List<PackingTypeSize> listaEstilos = objPacking.ListadoPackingPorIdEstilo(idPedido, packingName);
            int numBlock = objPacking.ObtenerIdBlock(idPedido, packingName);
            int numTotalCart = objPacking.ObtenerTotalCartonesAssort(idPedido, packingName);
            int numTotalPiezas = objPacking.ObtenerTotalPiezasAssort(idPedido, packingName);
            int tCartonesFalt = objPacking.ObtenerTotalCartonesFaltantesAssort(idPedido, numBlock, numTotalCart);
            int tPiezasFalt = objPacking.ObtenerTotalPiezasFaltantesAssort(idPedido, numBlock, numTotalPiezas);
            var result = Json(new { listaPackingBox = listaEstilos, numTotalCartonesFalt = tCartonesFalt, numTotalPiezasFat = tPiezasFalt });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Listado_Cantidades_Estilos_Empaque_Assort(string packingName)
        {
            int idPedido = Convert.ToInt32(Session["id_Pedido"]);
            List<PackingTypeSize> listaEstilos = objPacking.ListadoPackingPorIdEstilo(idPedido, packingName);
            var result = Json(new { listaPackingBox = listaEstilos });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Listado_Packing_Assort()
        {
            int idPedido = Convert.ToInt32(Session["id_Pedido"]);
            int numRegistros = objPacking.ListadoPackingTypeAssort(idPedido);
            int cargo = Convert.ToInt32(Session["idCargo"]);
            int totalPiezasEstilos = objSummary.ObtenerPiezasTotalesEstilos(idPedido);
            int totalPiezasPack = objSummary.ObtenerPiezasTotalesPorPack(idPedido);
            var result = Json(new { totalRegistros = numRegistros, cargoUser = cargo, numTPSyle = totalPiezasEstilos, numTPack = totalPiezasPack });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Empaque_Assort(int? estiloId)
        {

            List<ItemTalla> listaTallasPO = objTallas.ListaTallasPorEstilo(estiloId).ToList();
            List<ItemTalla> listCantidadesTallas = objTallas.ListaCantidadesTallasPorEstilo(estiloId).ToList();
            List<PackingSize> listaTallasPacking = objPacking.ObtenerListaPackingSizePorEstilo(estiloId).ToList();
            /*List<PackingSize> listaTallasEstilo = objPacking.ListaTallasCalidadPack(estiloId).ToList();
            List<PackingTypeSize> listaTallasPacking = objPacking.ObtenerListaPackingTypeHTPorEstilo(estiloId, nPO, tEmpaque).ToList();
            */
            //List<ItemDescripcion> listaTallasPackingBox = objPedido.ListaItemEstilosPorIdPedido(idEstilo).ToList();
            //List<PackingTypeSize> listaTallasPacking = objPacking.BuscarPackingTypeSizePorEstilo(estiloId);

            var result = Json(new { lista = listaTallasPO, listCantTalla = listCantidadesTallas, listaPackingS = listaTallasPacking/*, listaTalla = listaTallasEstilo, listaPackingS = listaTallasPacking, listaPackingBox = listaTallasPackingBox */});
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_HT_Por_Estilo(int? id)
        {
            // List<PackingM> listaTallasEstilo = objPacking.ObtenerTallas(id).ToList();
            List<ItemTalla> listaTallas = objTallas.ListaTallasPorEstilo(id).ToList(); 	//lISTA EXTRAS Y EXAMPLES		
            List<ItemTalla> listaTall = objTallas.ListaTallasHTPorEstilo(id).ToList();
            List<int> listaPiezasPackingBulk = objPacking.ListaTallasPackingBulkEstilo(id).ToList();
            List<int> listaPiezasPackingPPK = objPacking.ListaTallasPackingPPKEstilo(id).ToList();
            //List<PackingSize> listaTallasPacking = objPacking.ObtenerListaPackingSizePorEstilo(id).ToList();
            //List<PackingTypeSize> listaTallasEmpaquePPK = objPacking.ObtenerListaPackingPPKPorEstilo(id).ToList();
            List<PackingTypeSize> listaTallasEmpaquePPK = objPacking.ObtenerListaPackingPPK(id).ToList();
            List<PackingTypeSize> listaTallasEmpaqueBulk = objPacking.ObtenerListaPackingBulkPONumber(id).ToList();

            List<int> listaTallasCBatch = new List<int>();
            if (listaPiezasPackingBulk.Count != 0)
            {
                listaTallasCBatch = objPacking.ListaTotalCajasTallasBatchBulkHTEstilo(id).ToList();
            }
            string estilo = "";
            foreach (var item in listaTallas)
            {
                estilo = item.Estilo;

            }
            int cargo = Convert.ToInt32(Session["idCargo"]);
            var result = Json(new
            {
                lista = listaTallas,
                listaT = listaTall,
                //listaTalla = listaTallasEstilo,
                //listaPackingS = listaTallasPacking,
                estilos = estilo,
                listaPTBulk = listaPiezasPackingBulk,
                listaPTPPK = listaPiezasPackingPPK,
                listaEmpPPK = listaTallasEmpaquePPK,
                listaEmpBulk = listaTallasEmpaqueBulk,
                listaTotalCajasPack = listaTallasCBatch,
                cargoUser = cargo
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Tallas_Assort_Por_Estilo(int? idPedido, int numBlock, string namePack)
        {
            List<PackingM> listaTallasPO = objPacking.ObtenerTallasAssort(idPedido, namePack).ToList();
            List<ItemTalla> listaTallas = objTallas.ListaTallasAssortPorEstilo(idPedido, namePack).ToList();
            List<PackingTypeSize> listaRatios = objPacking.ListaRatiosPackAssort(idPedido, namePack, numBlock).ToList();
            int numTotalCartones = objPacking.ObtenerCantCartonesAssort(idPedido, numBlock);
            var result = Json(new
            {
                lista = listaTallas,
                listaPO = listaTallasPO,
                listRatio = listaRatios,
                numCartones = numTotalCartones
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Lista_Total_Tallas_Batch(int id)
        {
            List<int> listaTallas = objPacking.ListaTotalTallasPackingBatchEstilo(id).ToList();

            var result = Json(new { listaTallaBatch = listaTallas });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ActualizarPackingAssort(string qty, string cart, string totalUnits, string packName)
        {
            int numCant = Int32.Parse(qty);
            int numCart = Int32.Parse(cart);
            int numTU = Int32.Parse(totalUnits);
            objPacking.ActualizarCantidadesPackAssort(numCant, numCart, numTU, packName);

            return Json("0", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EliminarInstruccionPacking(int id)
        {
            objPacking.EliminarPacking(id);
           // objPacking.EliminarPrimerCalidadPacking(id);

            return Json("0", JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult EliminarInstruccionPackingAssort(int id, string packName)
        {
            //objPacking.EliminarPacking(id);
            //objPacking.EliminarPrimerCalidadPacking(id);
            List<ItemDescripcion> listaEstilos = objPedido.ListaEstilosPorIdPedido(id).ToList();
            List<PackingTypeSize> listPacking = objPacking.ListaPackingNameAssort(listaEstilos);
            foreach (var item in listPacking)
            {
                objPacking.EliminarPackingAssort(item.IdPackingTypeSize);
            }


            return Json("0", JsonRequestBehavior.AllowGet);
        }

    }
}