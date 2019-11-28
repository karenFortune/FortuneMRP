using FortuneSystem.Models;
using FortuneSystem.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FortuneSystem.Controllers.Catalogos
{
    public class ItemDescripcionController : Controller
    {
		// GET: CatItemDescripcion
		readonly ItemDescripcionData objItem = new ItemDescripcionData();
		private MyDbContext db = new MyDbContext();
		public ActionResult Index()
        {
			List<ItemDescripcion>  listaItems = objItem.ListaItems().ToList();
			return View(listaItems);
		}

		[HttpGet]
		public ActionResult CrearItemDesc()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CrearItemDesc([Bind] ItemDescripcion items)
		{
			if (items.ItemId == 0)
			{
				objItem.AgregarItemDescripcion(items);
				TempData["itemOK"] = "The style was registered correctly.";
				return RedirectToAction("Index");
			}
			else
			{
				TempData["itemError"] = "The style can not be registered, try it later.";
			}
			return View(items);
		}

		[HttpGet]
		public ActionResult Detalles(int? id)
		{
			if (id == null)
			{
				return View();
			}
			
			ItemDescripcion items = objItem.ConsultarListaItemDesc(id);
			if (items == null)
			{
				return View();
			}
			return View(items);
		}


		[HttpGet]
		public ActionResult Editar(int? id)
		{
			if (id == null)
			{
				return View();
			}

			ItemDescripcion items = objItem.ConsultarListaItemDesc(id);
			if (items == null)
			{
				return View();
			}

			return View(items);

		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Editar(int id, [Bind] ItemDescripcion items)
		{
			if (id != items.ItemId)
			{
				return View();
			}
			if (ModelState.IsValid)
			{
				objItem.ActualizarItemDesc(items);
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
		public ActionResult Eliminar(int? id)
		{
			if (id == null)
			{
				return View();
			}

			ItemDescripcion items = objItem.ConsultarListaItemDesc(id);


			if (items == null)
			{
				return View();
			}
			return View(items);

		}

		[HttpPost, ActionName("Eliminar")]
		[ValidateAntiForgeryToken]
		public ActionResult ConfimacionEliminar(int? id)
		{
			IMAGEN_ARTE art = db.ImagenArte.Where(x => x.IdEstilo == id).FirstOrDefault();
			if(art != null)
			{
				TempData["itemEliminarError"] = "The style can not be removed, it has an associated art image.";
				return RedirectToAction("Index");
			}
			else
			{
				objItem.EliminarItemDesc(id);
				TempData["itemEliminar"] = "The style was removed correctly.";
			}
		
			return RedirectToAction("Index");
		}

	}
}