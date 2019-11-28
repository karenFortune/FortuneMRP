using FortuneSystem.Models;
using FortuneSystem.Models.Catalogos;
using FortuneSystem.Models.Pedidos;
using FortuneSystem.Models.POSummary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace FortuneSystem.Controllers
{
    public class WIPController : Controller
    {  
        public ActionResult Index()
        {
            OrdenesCompra pedido = new OrdenesCompra();
            ListaPeriodos(pedido);
            /*List<OrdenesCompra> listaPedidos = new List<OrdenesCompra>();
             listaPedidos = objPedido.ListaOrdenCompraWIP().ToList();*/
            return View();
        }
        public JsonResult ListadoPedido()
        {
            int estadoTab = 1;
            List<OrdenesCompra> listaPedidos = CatalagoWIP(estadoTab);           
            var jsonResult = Json(listaPedidos, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public void ListaPeriodos(OrdenesCompra pedido)
        {
            PedidosData objPedido = new PedidosData();
            List<Periodo> listaYear = objPedido.ListadoPeriodos().ToList();
            List<SelectListItem> items = new SelectList(listaYear, "NumPeriodo", "NumPeriodo", pedido.IdPeriodo).ToList();
            items.Insert(0, (new SelectListItem { Text = "ALL", Value = "0" }));
            ViewBag.listPeriodo = items;
        }

        public JsonResult ListadoPedidoShipping()
        {
            int estadoTab = 2;
            List<OrdenesCompra> listaPedidos = CatalagoWIP(estadoTab);
            var jsonResult = Json(listaPedidos, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult ListadoPedidoCancelled()
        {
            int estadoTab = 3;
            List<OrdenesCompra> listaPedidos = CatalagoWIP(estadoTab);
            var jsonResult = Json(listaPedidos, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public List<OrdenesCompra> CatalagoWIP(int estadoTab)
        {
            PedidosData objPedido = new PedidosData();
            List<OrdenesCompra> listaPedidos = objPedido.ListaOrdenCompraWIP(estadoTab).ToList();
            return listaPedidos;
        }

        public JsonResult ListadoComentariosWIP()
        {
            CatComentariosData objComent = new CatComentariosData();
            string tipoArchivo = "WIP";
            List<CatComentarios> listaComentarios = objComent.ListadoAllWIPComentarios(tipoArchivo).ToList();
            var jsonResult = Json(listaComentarios, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult ListadoComentariosShipped()
        {
            CatComentariosData objComent = new CatComentariosData();
            string tipoArchivo = "SHIPPED";
            List<CatComentarios> listaComentarios = objComent.ListadoAllWIPComentarios(tipoArchivo).ToList();
            var jsonResult = Json(listaComentarios, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult ListadoComentariosCancelled()
        {
            CatComentariosData objComent = new CatComentariosData();
            string tipoArchivo = "CANCELLED";
            List<CatComentarios> listaComentarios = objComent.ListadoAllWIPComentarios(tipoArchivo).ToList();
            var jsonResult = Json(listaComentarios, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }


        [HttpPost]
        public void RegistrarCometarioWIP(string Comentario, int IdSummary, string TipoArchivo)
        {
            CatComentariosData objComent = new CatComentariosData();
            DateTime fecha = DateTime.Now;
            int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            CatComentarios catComentario = new CatComentarios()
            {
                FechaComentario = fecha,
                IdSummary = IdSummary,
                Comentario = Comentario,
                IdUsuario = noEmpleado,
                TipoArchivo = TipoArchivo
            };
            objComent.AgregarComentario(catComentario);

        }
        [HttpPost]
        public void RegistrarFechaUCC(DateTime FechaUCC, int IdSummary)
        {
            // DateTime fecha = DateTime.Now;
            //  int noEmpleado = Convert.ToInt32(Session["id_Empleado"]);
            DescripcionItemData objSummary = new DescripcionItemData();
            POSummary poSummary = new POSummary()
            {
                IdItems = IdSummary,
                FechaUCC = FechaUCC
            };
            objSummary.AgregarFechaUCC(poSummary);

        }

        public void ActualizarSucursalIdSummary(int IdSucursal, int IdSummary)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "UPDATE PO_SUMMARY SET ID_SUCURSAL='" + IdSucursal + "' WHERE ID_PO_SUMMARY='" + IdSummary + "'";
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

        public int ConsultarSucursalIdSummary(int IdSummary)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "SELECT ID_SUCURSAL FROM PO_SUMMARY WHERE ID_PO_SUMMARY='" + IdSummary + "'";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    return Convert.ToInt32(reader["ID_SUCURSAL"]);
                }
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return 0;
        }
    }
}