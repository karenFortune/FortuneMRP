using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FortuneSystem.Models.Almacen;
using FortuneSystem.Models.Trims;

namespace FortuneSystem.Controllers
{
    public class KeepSessionAliveController : Controller
    {

        // GET: KeepSessionAlive
        public JsonResult StayingAlive()
        {
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            int id_usuario = Convert.ToInt32(Session["id_Empleado"]);
            Session["id_usuario"] = id_usuario;
            Session["sucursal"] = consultas.obtener_sucursal_id_usuario(Convert.ToInt32(Session["id_usuario"]));
            return Json(id_usuario, JsonRequestBehavior.AllowGet);
        }
    }
}