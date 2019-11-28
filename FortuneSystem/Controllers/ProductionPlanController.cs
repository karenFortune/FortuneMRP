using FortuneSystem.Models.Pedidos;
using FortuneSystem.Models.POSummary;
using FortuneSystem.Models.ProductionPlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FortuneSystem.Controllers
{
    public class ProductionPlanController : Controller
    {
        // GET: ProductionPlan
        public ActionResult Index()
        {
            return View();
        }
		public ActionResult Detalles()
		{
			ProductionPlanning planning = new ProductionPlanning();
			ListaHornos(planning);
			ListaPO(planning);			
			return View();
		}

		public ActionResult MenuHornos()
		{
			ProductionPlanning planning = new ProductionPlanning();
			//ListaHornos(planning);
			//ListaPO(planning);
			return View();
		}

		[HttpPost]
		public JsonResult Autocomplete_PO(string keyword)
		{
			ProductionPlanning planning = new ProductionPlanning();
			PedidosData objPedido = new PedidosData();
			planning.ListaPOs = objPedido.ListaOrdenCompraPorPO().ToList();
			var ordenes = (from N in planning.ListaPOs
						   where N.PO.StartsWith(keyword.ToUpper())
						   select new { N.PO, Orden = N.PO, Id = N.IdPedido });
			return Json(ordenes, JsonRequestBehavior.AllowGet);
		}

		public void ListaHornos(ProductionPlanning planning)
		{
			CatProdOverData objOver = new CatProdOverData();
			planning.ListHornos = objOver.ListaHornos().ToList();

			ViewBag.listHornos = new SelectList(planning.ListHornos, "IdProdCatOver", "Horno", planning.IdHorno);					   		
		}

		public JsonResult ListadoMaquinasPorHorno(int? id)
		{
			ProductionPlanningData objPlan = new ProductionPlanningData();
			List<ProdOverMachine> listaMaquinas = objPlan.ListaMaquinasHorno(id).ToList();
			var result = Json(new { listMaquinas = listaMaquinas });
			return Json(result, JsonRequestBehavior.AllowGet);
		}

		public JsonResult ListadoEstilosPOHorno(int? id)
		{
			DescripcionItemData objEstilo = new DescripcionItemData();
			List<POSummary> listSummary = objEstilo.ListaEstilosPorPOPP(id).ToList();
			var result = Json(new { listEstilos = listSummary });
			return Json(result, JsonRequestBehavior.AllowGet);
		}

		public void ListaPO(ProductionPlanning planning)
		{			
			PedidosData objPedido = new PedidosData();
			planning.ListaPOs = objPedido.ListaOrdenCompraPorPO().ToList();

			ViewBag.listPO = new SelectList(planning.ListaPOs, "IdPedido", "PO", planning.IdPedido);
		}

		[HttpPost]
		public JsonResult Obtener_Datos_PO_Oven(int? idPedido, int? idHorno)
		{
			ProductionPlanningData objPlan = new ProductionPlanningData();
			ProdOverMachine overMaquina = new ProdOverMachine();
			DescripcionItemData objEstilo = new DescripcionItemData();
			overMaquina.ListaMaquinas = objPlan.ListaMaquinasHorno(idHorno).ToList();
			List<POSummary> listSummary = objEstilo.ListaEstilosPorPOPP(idPedido).ToList();
			var result = Json(new
			{
				listaMaquinas = overMaquina.ListaMaquinas,
				listaEstilos = listSummary
			});
			return Json(result, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Obtener_Planeacion_General()
		{
			ProductionPlanningData objPlan = new ProductionPlanningData();
			List<ProductionPlanning> listaPlan = objPlan.ListaPlaneacionGeneral().ToList();
			ProdOverMachine overMaquina = new ProdOverMachine
			{
				ListaMaquinas = objPlan.ListaMaquinas().ToList()
			};
			var result = Json(new
			{
				listPlanGnl = listaPlan,
				listaMaquinas = overMaquina.ListaMaquinas
			});
			return Json(result, JsonRequestBehavior.AllowGet);
		}


		[HttpPost]
		public JsonResult Obtener_Informacion_Planeacion_Edit(int? id, int? idMaquina)
		{
			ProductionPlanningData objPlan = new ProductionPlanningData();
			ProductionPlanning planeacionGrl = new ProductionPlanning();

			planeacionGrl = objPlan.ObtenerInformacionPlaneacionPorId(id, idMaquina);

			var result = Json(new
			{				
				planeacion = planeacionGrl
			});
			return Json(result, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Obtener_Production_Planning([Bind] ProductionPlanning planning/*, List<string> ListEstilos*/)
		{
			ProductionPlanningData objPlan = new ProductionPlanningData();
			planning.IdUser = Convert.ToInt32(Session["id_Empleado"]);
			planning.Status = "ABIERTO";
			/*List<string> maquinas = ListEstilos[0].Split('*').ToList();
			List<string> estilos = ListEstilos[5].Split('*').ToList();
			List<string> fechas = ListEstilos[1].Split('*').ToList();
			List<string> metedores = ListEstilos[2].Split('*').ToList();
			List<string> sacadores = ListEstilos[3].Split('*').ToList();
			List<string> cachadores = ListEstilos[4].Split('*').ToList();
			int i = 1;
			foreach (var item in maquinas)
			{
				i++;
			}

			i -= 2;
			for (int v = 0; v < i; v++)
			{
				string maquina = maquinas[v];
				string estilo = estilos[v];
				string fecha = fechas[v];
				string metedor = metedores[v];
				string sacador = sacadores[v];
				string cachador = cachadores[v];

				if (fecha != "")
				{
					planning.IdProdOverMachine = Int32.Parse(maquina);
					planning.Fecha = Convert.ToDateTime(fecha);
					planning.IdSummary = Int32.Parse(estilo);
					planning.Metedor = metedor;
					planning.Sacador = sacador;
					planning.Cachador = cachador;
					
				}

			objPlan.AgregarPlaneacion(planning);

		}*/
		objPlan.AgregarPlaneacion(planning);
			return Json("0", JsonRequestBehavior.AllowGet);

		}

		[HttpPost]
		public JsonResult Actualizar_Production_Planning([Bind] ProductionPlanning planning/*, List<string> ListEstilos*/)
		{
			ProductionPlanningData objPlan = new ProductionPlanningData();
			planning.IdUser = Convert.ToInt32(Session["id_Empleado"]);
			if(planning.Status == "1")
			{
				planning.Status = "ABIERTO";
			}
			else
			{
				planning.Status = "CERRADO";
			}
			objPlan.ActualizarPlaneacion(planning);
			return Json("0", JsonRequestBehavior.AllowGet);

		}
	}
}