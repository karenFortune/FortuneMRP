using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FortuneSystem.Models.Almacen;
using FortuneSystem.Models.Shipping;
using FortuneSystem.Models.Trims;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ZXing;
using OfficeOpenXml;
using System.Data;
using ClosedXML.Excel;
using ZXing.Common;
using ZXing.QrCode;
using System.Data.SqlClient;


namespace FortuneSystem.Controllers.Catalogos
{
    public class AlmacenController : Controller
    {
        // GET: Almacen
        DatosInventario di = new DatosInventario();
        DatosReportes dr = new DatosReportes();
        FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
        DatosTransferencias dt = new DatosTransferencias();
        QRCodeController qr = new QRCodeController();
        PDFController pdf = new PDFController();
        DatosShipping ds = new DatosShipping();

        int id_sucursal;
        public ActionResult Index(){
            int id_usuario = Convert.ToInt32(Session["id_Empleado"]);
            Session["id_usuario"] = id_usuario;
            Session["id_sucursal"] = consultas.obtener_sucursal_id_usuario(Convert.ToInt32(Session["id_usuario"]));
            return View();
        }
        public ActionResult recibo_items(){
            return View();
        }
        public ActionResult formulario_nuevo_item(){
            return View();
        }
        public ActionResult customer_recibos(){
            return View();
        }
        public ActionResult departamentos_recibos(){
            return View();
        }
        public JsonResult buscar_items_tabla_inventario(string busqueda){
            return Json(di.ListaInventario(busqueda), JsonRequestBehavior.AllowGet);
        }
        public ActionResult nueva_transferencia(){
            return View();
        }
        public ActionResult nueva_transferencia_qr(){
            return View();
        }
        public ActionResult editar_recibo_items(){
            return View();
        }
        //AUTOCOMPLETADOS
        public ActionResult Autocomplete_po(string term) {
            var items = consultas.Lista_po_abiertos();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_estilos(string term)
        {
            var items = consultas.Lista_styles();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_paises(string term)
        {
            var items = consultas.Lista_paises();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_fabricantes(string term)
        {
            var items = consultas.Lista_fabricantes();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_colores(string term)
        {
            var items = consultas.Lista_colores();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_colores_codigos(string term)
        {
            var items = consultas.Lista_colores_codigos();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_body_type(string term)
        {
            var items = consultas.Lista_body_types();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_tallas(string term)
        {
            var items = consultas.Lista_tallas();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_generos(string term)
        {
            var items = consultas.Lista_generos();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_telas(string term)
        {
            var items = consultas.Lista_telas();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_percents(string term)
        {
            var items = consultas.Lista_porcentajes();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_customer_name(string term)
        {
            var items = consultas.Lista_customers();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_cliente_final(string term)
        {
            var items = consultas.Lista_clientes_finales();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_ubicacion(string term)
        {
            var items = consultas.Lista_ubicaciones();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult obtener_locaciones(){
            return Json(consultas.Lista_locaciones(), JsonRequestBehavior.AllowGet);
        }
         public ActionResult obtener_paises(){
            return Json(consultas.Lista_paises_completa(), JsonRequestBehavior.AllowGet);
        } public ActionResult obtener_customer(){
            return Json(consultas.Lista_customers_completo(), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Autocomplete_unidades(string term)
        {
            var items = consultas.Lista_units();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_locacion(string term)
        {
            var items = consultas.Lista_customers();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_family(string term)
        {
            var items = consultas.Lista_family();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_trims(string term)
        {
            var items = consultas.Lista_trims();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_yarns(string term)
        {
            var items = consultas.Lista_yarn();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_units(string term)
        {
            var items = consultas.Lista_units();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        //AUTOCOMPLETADOS
        public ActionResult lista_pos(string term) {
            return Json(consultas.Lista_po_abiertos(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult lista_estilos_dropdown(string ID) {
            //var items = di.Lista_po();
            //var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            Session["po"] = ID;
            return Json(consultas.Lista_estilos_dropdown(ID), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult lista_lugares(){
            List<int> lista = dt.qrs();
            foreach (int i in lista) {
               // GenerateMyQCCode("ubicacion_"+Convert.ToString(i));
            }
            return Json(dt.lista_lugares_transfer(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult lista_lugares_destino(string ID){
            return Json(dt.lista_lugares_transfer_destino(ID), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult inventario_sucursal(string ID, string busqueda){
            return Json(dt.obtener_inventario_sucursal(ID, busqueda), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult guardar_nuevo_sello(string inicio, string final, string sucursal) {
            di.guardar_sello(inicio, final, sucursal);
            return Json("0", JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult lista_sellos(string ID) {
            return Json(consultas.buscar_sello_nuevo(Convert.ToInt32(ID)), JsonRequestBehavior.AllowGet);
        }
        //[HttpPost]
        public JsonResult guardar_transferencia(string ids, string cantidades, string fecha, string persona, string sello, string origen, string destino, string driver, string pallet, string envio, string id_sello, string pos, string estilos,string caja,string carro,string placas,string codigos,string num_pallet)
        {
            int id_transferencia = 0, total = 0, id_sucursal = 0,total_pallets=0;
            string[] Cantidades, Ids, Pos, Estilos,Cajas,Codigo,Pallets;
            Cantidades = cantidades.Split('*');
            Ids = ids.Split('*');
            Pos = pos.Split('*');
            Estilos = estilos.Split('*');
            Cajas = caja.Split('*');
            Codigo= codigos.Split('*');
            Pallets= num_pallet.Split('*');
            for (int i = 1; i < Cantidades.Length; i++){
                total += Convert.ToInt32(Cantidades[i]);
                total_pallets += Convert.ToInt32(Pallets[i]);
            }
            dt.guardar_transferencia_inventario(fecha, persona, sello, origen, destino, driver, pallet, envio, total, Convert.ToInt32(Session["id_usuario"]), id_sello,carro,placas);
            id_transferencia = dt.obtener_ultima_transferencia();
            id_sucursal = consultas.buscar_id_sucursal_usuario(Convert.ToInt32(Session["id_usuario"]));
            if (sello != "0"){
                dt.aumentar_sellos(sello, id_sucursal);
            }
            dt.revisar_sellos(id_sello, sello);
            for (int i = 1; i < Cantidades.Length; i++) {
                int pedido = consultas.buscar_pedido(Pos[i]);
                int estilo = consultas.obtener_estilo_id(Estilos[i]);
                int summary = consultas.obtener_po_summary(pedido, estilo);
                dt.guardar_items_inventario(id_transferencia, Ids[i], Cantidades[i], pedido, estilo,Cajas[i],Codigo[i],summary, Pallets[i]);
            }
            return Json("0", JsonRequestBehavior.AllowGet);
        }

        [ChildActionOnly]
        public ActionResult lista_transferencias() {
            return PartialView(dt.ListaTransferencias());
        }
        [HttpGet]
        public ActionResult lista_recibos() {
            string id_sucursal = consultas.obtener_sucursal_id_usuario(Convert.ToInt32(Session["id_usuario"]));
            Session["id_sucursal"] = id_sucursal;
            return PartialView();
        }
        [HttpPost]
        public JsonResult aprobar_transferencia(string ID){
            dt.aprobar_transferencia_inventario(ID);
            //GenerateMyQCCode("transferencia_" + ID);
            dt.aprobar_transferencia_items(ID);
            return Json(dt.ListaTransferencias(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult desaprobar_transferencias(string ID) {
            dt.negar_transferencia_inventario(ID);
            return Json(ID, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult buscar_transferencia(string ID) {
            return Json(dt.obtener_informacion_transferencia(ID), JsonRequestBehavior.AllowGet);
        }        
        [HttpGet]
        public ActionResult edicion_inventario(int id)
        {
            Session["id_inventario_editar"] = id;
           return PartialView(dt.obtener_informacion_inventario(id));
           // return Json(dt.obtener_informacion_inventario(id), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult get_item_data() {
            return Json(dt.obtener_item_editar(Convert.ToInt32(Session["id_inventario_editar"])), JsonRequestBehavior.AllowGet);
        }
        /****************************************************************************************************************************************/
        [HttpPost]
        public JsonResult guardar_edicion_inventario_trim(string id, string estilo, string tipo, string po,  string unit, string company, string cantidad, string descripcion, string familia, string minimo)
        {//
            if (estilo == "undefined") estilo = "0";
            di.obtener_datos_trim(0,Convert.ToInt32(Session["id_usuario"]), estilo, tipo, po,  unit, company, cantidad, descripcion, familia, minimo);
            di.cantidad = Convert.ToInt32(cantidad); 
            di.id_inventario = Convert.ToInt32(id);
            di.guardar_edicion_trim_po();
            return Json("0", JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult guardar_edicion_inventario_blank(string datos,string total)
        {
           /* string[] data = datos.Split('*');
            if (data[0] == "undefined") data[0] = "0"; // Estilo
            di.id_inventario = Convert.ToInt32(data[17]);
            int customer_final = consultas.buscar_customer_final(data[13]);
            di.color_aux = data[5];
            //"                                                           + estilo +tipo_reporte  po +    pais + fabricante color + body_type   size +  gender +fabric_type percent customer  locacion +customer_final  date_comment comment  notas +  + inventario 
            di.obtener_datos_blank(0,Convert.ToInt32(Session["id_usuario"]), data[0], "Blanks", data[2], data[3], data[4],  data[5], data[6],  data[7], data[8], data[9],  data[10], data[11], data[12], customer_final, data[14], data[15], data[16], "0", "0");
                          //int item, int usuario,                    string estilo, s tipo,    po,       pais,  fabricante, color,  body_type,  size,    gender, abric_type, percent, customer, location, customer_final, datecoment,comments,  notas, cajas, cantidades)
            di.total = Convert.ToInt32(total);
            di.guardar_edicion_blank();*/
            return Json("0", JsonRequestBehavior.AllowGet);
        }
        public JsonResult cambiar_sello_transferencia(string ID)
        {
            salidas salida = new salidas();
            salidas salida_sello_nuevo = new salidas();
            salida = dt.obtener_datos_cambio_sello(ID);//SUCURSAL, ID_SELLO, SELLO
            salida_sello_nuevo = consultas.buscar_sello_nuevo(salida.id_sucursal); //SELLO ID_SELLO
            dt.cambiar_sello(Convert.ToInt32(ID), salida_sello_nuevo.sello, salida_sello_nuevo.id_sello);
            dt.aumentar_sellos(Convert.ToString(salida_sello_nuevo.sello), salida.id_sucursal);
            dt.revisar_sellos(Convert.ToString(salida_sello_nuevo.id_sello), Convert.ToString(salida_sello_nuevo.sello));
            return Json("", JsonRequestBehavior.AllowGet);
        }
        //*****************************************************************************************************************************************************
        [HttpGet]
        public ActionResult lista_recepcion_transferencias(){
            return PartialView(dt.lista_transferencias_por_recibir(Convert.ToString(Session["id_sucursal"])));
        }      

        

        public JsonResult obtener_categorias_inventario() {
            return Json(consultas.buscar_categorias_inventario(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult eliminar_categoria(string id) {
            dt.eliminar_categorias_inventario(id);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        /******************************************************************************************************************************************/
        [HttpPost]
        public JsonResult agregar_item_catalogo(string actionData,string unit,string minimo){
            string[] datos = actionData.Split('*');
            string[] tallas = datos[3].Split('+');
            
            for (int i = 0; i < tallas.Length; i++) {
                int existencia = di.buscar_existencia_item(actionData,tallas[i]);
                if (existencia == 0){
                    di.guardar_item_nuevo(actionData,tallas[i],unit,minimo);
                    //if () {
                    //}
                }
            }            
            
            return Json("0", JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult formulario_recibo() {
            string id_sucursal = consultas.obtener_sucursal_id_usuario(Convert.ToInt32(Session["id_usuario"]));
            Session["id_sucursal"] = id_sucursal;
            return PartialView();
        }
        //BUSQUEDA DINAMICA
        public JsonResult busqueda_dinamica_items(string item,string descripcion,string color, string size,string gender) {
            return Json(di.lista_items_para_recibir(item,descripcion,color,size,gender), JsonRequestBehavior.AllowGet);
        }
        //G U A R D A R --- R E C I B O 
        public int obtener_cantidad_item(string caja, string cantidad) {
            string[] cajas = caja.Split('&'), cantidades = cantidad.Split('&');
            int total = 0;
            for (int i = 1; i < cajas.Length; i++) {
                total += Convert.ToInt32(cajas[i]) * Convert.ToInt32(cantidades[i]);
            }
            return total;
        }
                               //var actionData = "{'id':'" + id + "','item':'" + item + "','po':'" + po'mill':'" + mill + "','po_r':'" + po_r + "','locacion':'" + locacion + "','country':'" + country + "','customer':'" + customer + "','packing_number':'" + packing_number + "','sucursal':'" + $("#caja_sucursales_recibo").val()+"','comentarios':'" + comentarios+"','total':'" + quantity+"'}";
      
        [HttpPost]
        public JsonResult guardar_recibo_inventario(string id, string item, string po, string style, string mill, string po_r, string locacion, string country, string customer, string packing_number, string sucursal, string comentarios, string total)
        {
            //SEPARAR INFORMACIÓN
            //Session["id_usuario"] = 2;
            string[] ids = id.Split('*'), items = item.Split('*'), pos = po.Split('*'), styles = style.Split('*'), locaciones = locacion.Split('*');
            string[] countries = country.Split('*'), customers = customer.Split('*'), totales = total.Split('*');//, cajas_item = caja.Split('*'), cantidades_item = cantidad.Split('*');
            int existencia = 0;
            //POR CADA ITEM
            int total_recibo = 0;
            for (int i = 1; i < ids.Length; i++) { total_recibo += Convert.ToInt32(totales[i]); }
            di.guardar_recibo(total_recibo, mill, po_r, packing_number, comentarios, Convert.ToString(Session["id_usuario"]),sucursal,customers[1]);
            di.id_recibo = di.obtener_ultimo_recibo();

            for (int i = 1; i < ids.Length; i++){
                di.id_tipo = consultas.buscar_tipo_inventario_item(items[i]);
                di.mill_po = mill; di.po_referencia = po_r;
                
                switch (di.id_tipo){
                    case 1:
                        consultas.buscar_informacion_blank_item(items[i]);
                        int estilo = consultas.obtener_estilo_summary(Convert.ToInt32(styles[i]));
                        int customer_final = consultas.buscar_cliente_final_po(pos[i]);
                        di.obtener_datos_blank(Convert.ToInt32(items[i]), Convert.ToInt32(Session["id_usuario"]), styles[i],estilo, "Blanks", pos[i], countries[i], consultas.fabricante, consultas.color, consultas.body_type, consultas.size, consultas.gender, consultas.fabric_type, consultas.fabric_percent, customers[i], locaciones[i], customer_final, "N/A", "N/A", "N/A");
                        di.id_sucursal = Convert.ToInt32(sucursal);
                        di.id_customer_final = customer_final;
                        existencia = di.buscar_existencia_blank_inventario();
                        di.id_sucursal = Convert.ToInt32(sucursal);
                        di.quantity = Convert.ToInt32(totales[i]);
                        di.id_usuario = Convert.ToInt32(Session["id_usuario"]);
                        if (existencia == 0){
                            di.guardar_blank();
                            di.id_inventario = di.obtener_ultimo_inventario();                            
                            di.guardar_recibo_item(di.id_recibo, (di.id_inventario).ToString(), totales[i], styles[i]);
                        }else{
                            di.sumar_existencia_blank(existencia);                           
                            di.guardar_recibo_item(di.id_recibo, existencia.ToString(), totales[i], styles[i]);
                        }
                        break;
                }//switch
            }//FOR
            
            Session["id_recibo_nuevo"] = di.id_recibo;
            return Json("0", JsonRequestBehavior.AllowGet);
        }



        /* public JsonResult guardar_recibo_inventario(string id, string item, string po, string style, string mill, string po_r, string locacion, string country, string customer, string caja, string cantidad, string packing_number, string sucursal, string comentarios)
         {
             //SEPARAR INFORMACIÓN
             //Session["id_usuario"] = 2;
             string[] ids = id.Split('*'), items = item.Split('*'), pos = po.Split('*'), styles = style.Split('*'), locaciones = locacion.Split('*');
             string[] countries = country.Split('*'), customers = customer.Split('*'), cajas_item = caja.Split('*'), cantidades_item = cantidad.Split('*');
             string qty_item = "", ids_inventario = "", trims_inventario = "", trims_cantidades = "", trims_item = "", ids_summary = "";
             int existencia = 0, summary = 0;
             //POR CADA ITEM
             int total_item = 0, total_recibo = 0;
             for (int i = 1; i < ids.Length; i++)
             {
                 di.id_tipo = consultas.buscar_tipo_inventario_item(ids[i]);
                 di.mill_po = mill; di.po_referencia = po_r;
                 summary = 0;
                 switch (di.id_tipo)
                 {
                     case 2:
                         //int id_usuario, string estilo, string tipo, string po, string mill_po, string amt, string unit, string company, string cantidad, string descripcion_trim, string familia, string minimo,string referencia
                         consultas.buscar_informacion_trim_item(ids[i]);
                         total_item = obtener_cantidad_item(cajas_item[i], cantidades_item[i]);
                         qty_item += "*" + total_item.ToString();
                         total_recibo += total_item;
                         di.cantidad = total_item;
                         di.obtener_datos_trim(Convert.ToInt32(ids[i]), Convert.ToInt32(Session["id_usuario"]), styles[i], "Trims", pos[i], consultas.unit, customers[i], total_item.ToString(), consultas.descripcion, consultas.family, "0");
                         di.id_sucursal = Convert.ToInt32(sucursal);
                         existencia = di.buscar_existencia_trim_inventario();
                         summary = consultas.obtener_po_summary(di.id_pedido, di.id_estilo);
                         ids_summary += "*" + summary.ToString();
                         if (existencia == 0)
                         {
                             di.guardar_trim_po();
                             di.id_inventario = di.obtener_ultimo_inventario();
                             ids_inventario += "*" + di.id_inventario.ToString();
                         }
                         else
                         {
                             di.sumar_existencia_trim(existencia);
                             ids_inventario += "*" + existencia.ToString();
                         }
                         trims_inventario += "*" + consultas.obtener_po_summary(di.id_pedido, di.id_estilo);
                         trims_cantidades += "*" + total_item.ToString();
                         trims_item += "*" + ids[i];
                         break;
                     case 1:
                         consultas.buscar_informacion_blank_item(ids[i]);
                         int customer_final = consultas.buscar_cliente_final_po(pos[i]);
                         di.obtener_datos_blank(Convert.ToInt32(ids[i]), Convert.ToInt32(Session["id_usuario"]), styles[i], "Blanks", pos[i], countries[i], consultas.fabricante, consultas.color, consultas.body_type, consultas.size, consultas.gender, consultas.fabric_type, consultas.fabric_percent, customers[i], locaciones[i], customer_final, "N/A", "N/A", "N/A", cajas_item[i], cantidades_item[i]);
                         di.id_sucursal = Convert.ToInt32(sucursal);
                         qty_item += "*" + di.quantity.ToString();
                         total_recibo += di.quantity;

                         di.id_summary = consultas.obtener_po_summary(di.id_pedido, di.id_estilo);
                         existencia = di.buscar_existencia_blank_inventario();
                         ids_summary += "*" + (di.id_summary).ToString();
                         if (existencia == 0)
                         {
                             di.guardar_blank();
                             di.id_inventario = di.obtener_ultimo_inventario();
                             ids_inventario += "*" + di.id_inventario.ToString();
                         }
                         else
                         {
                             di.sumar_existencia_blank(existencia);
                             ids_inventario += "*" + existencia.ToString();
                         }
                         break;
                 }//switch
             }//FOR
             di.guardar_recibo(total_recibo, 0, mill, po_r, packing_number, comentarios);
             di.id_recibo = di.obtener_ultimo_recibo();
             //GenerateMyQCCode("recibo_" + di.id_recibo.ToString());
             string[] trimsInventario = trims_inventario.Split('*'), trimsCantidad = trims_cantidades.Split('*'), trimsItem = trims_item.Split('*');

             string[] inventarios = ids_inventario.Split('*'), totales_items = qty_item.Split('*'), summarys = ids_summary.Split('*');
             for (int j = 1; j < inventarios.Length; j++)
             {
                 di.guardar_recibo_item(di.id_recibo, inventarios[j], totales_items[j], summarys[j]);
                 di.id_recibo_item = di.obtener_ultimo_recibo_item();
                 string[] cajas = cajas_item[j].Split('&'), cantidades = cantidades_item[j].Split('&');
                 for (int k = 1; k < cajas.Length; k++)
                 {
                     for (int h = 0; h < Convert.ToInt32(cajas[k]); h++)
                     {
                         di.guardar_caja(di.id_recibo_item, inventarios[j], cantidades[k], cantidades[k]);
                         di.id_caja = di.obtener_id_ultima_caja();
                         //GenerateMyQCCode("caja_" + Convert.ToString(di.id_caja));
                     }
                 }
             }
             Session["id_recibo_nuevo"] = di.id_recibo;
             return Json("0", JsonRequestBehavior.AllowGet);
         }*/


        private void GenerateMyQCCode(string QCText) {
            var QCwriter = new BarcodeWriter();
            QCwriter.Format = BarcodeFormat.QR_CODE;
            QrCodeEncodingOptions options = new QrCodeEncodingOptions()
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
            };
            var result = QCwriter.Write(QCText);
            string path = Server.MapPath("~/Content/img/QR/" + QCText + ".jpg");
            //string nombre = "caja_" + QCText + ".jpg";
            var barcodeBitmap = new Bitmap(result, new Size(200, 200));
            using (MemoryStream memory = new MemoryStream()) {
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite)) {
                    barcodeBitmap.Save(memory, ImageFormat.Jpeg);
                    byte[] bytes = memory.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }
        [HttpPost]
        public JsonResult imprimir_etiquetas_cajas(string ID) {
            Session["id_recibo_nuevo"] = ID;
            return Json("0", JsonRequestBehavior.AllowGet);
        }

        private void ReadQRCode(string imagen) {
            var QCreader = new BarcodeReader();
            string QCfilename = Path.Combine(Request.MapPath
               ("~/Content/img/QR/"), imagen);
            var QCresult = QCreader.Decode(new Bitmap(QCfilename));
            if (QCresult != null) {
                string x = "My QR Code: " + QCresult.Text;
            }
        }
        public JsonResult obtener_recibos_lista() {
            return Json(di.Listarecibos(), JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult guardar_mp_recibo(string recibo, string mp)
        {
            di.agregar_mp_recibo(recibo, mp);
            return Json("0", JsonRequestBehavior.AllowGet);
        }
        public JsonResult obtener_recibo(string recibo)
        {
            return Json(di.lista_recibo_detalles(recibo), JsonRequestBehavior.AllowGet);
        }

        public void excel_diario(){            
            using (XLWorkbook libro_trabajo = new XLWorkbook())
            {
                var ws = libro_trabajo.Worksheets.Add("FORTUNE").SetTabColor(XLColor.FromArgb(146, 208, 80));
                ws.Range(1, 1, 1, 27).Style.Fill.BackgroundColor = XLColor.FromArgb(146, 208, 80);
                ws.Cell(1,28).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 0);
                ws.Range(1, 1, 1, 30).SetAutoFilter();                                
                var headers = new List<String[]>();
                headers.Add(new String[] { "MANUFACTURER   (2 LETTER CODE) FABRICANTE DE CAMISA","COUNTRY OF ORIGIN","AMT STYLE","AMT COLOR ","BODY TYPE","COLOR","SIZE","GENDER",
                    "BLANKS (B) NORMAL (N) TRIM (T) DAMAGES (D)","FABRIC TYPE","FABRIC %","WAREHOUSE","LOCATION","QTY","FISHBOWL","ORDER REF #","DATE OF RECEIPT (AGING)","AGE (days)",
                    "CANCEL DATE W.O","MILL PO","CUSTOMER NAME","# PALLET","AMT STYLE DESCRIPTION","DATE OF COMMENT","COMMENTS","PURCHASED FOR"});
                ws.Cell(1, 1).AsRange().AddToNamed("Titles");
                ws.Cell(1, 1).Value = headers;
                var row1 = ws.Row(1);
                row1.Height = 80;
                row1.Style.Font.Bold = true;
                row1.Style.Alignment.WrapText = true;
                ws.Cell("R1").Style.Font.FontColor = XLColor.FromArgb(255,0,102);
                ws.Cell("S1").Style.Font.FontColor = XLColor.FromArgb(112,48,160);
                row1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                row1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Style.Font.FontSize = 10;
                ws.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                ws.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell(1,1).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                ws.Cell(1,1).Style.Border.LeftBorderColor = XLColor.Blue;

                var inventario_stock = new List<string[]>();
                List<Inventario> lista_inventario_stock = new List<Inventario>();
                lista_inventario_stock=dr.obtener_item_diario(1);//AQUI VA EL ID DE LA SUCURSAL
                foreach(Inventario i in lista_inventario_stock) {
                    string fecha_recibo = consultas.obtener_fecha_recibo(i.id_inventario);
                    inventario_stock.Add(new string[] {i.fabricante,i.pais,i.amt_item,i.codigo_color,i.body_type,i.color,i.size,i.genero,i.categoria_inventario,
                        i.fabric_type,i.fabric_percent,"FORT",i.location,Convert.ToString(i.total),"ADD",i.po,fecha_recibo,consultas.item_age.ToString(),"",consultas.mill_po,
                        i.customer,i.date_comment,i.comment,"STOCK",i.notas });
                }
                ws.Cell(2, 1).Value = inventario_stock;

        /***********F*I*R*S*T**T*A*B***E*N*D****************************************************************************************************************************************************************/

                var mac = libro_trabajo.Worksheets.Add("MOVE AND CHANGE").SetTabColor(XLColor.FromArgb(255, 0, 0));
                mac.Range(1, 1, 1,30).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 0, 0);
                mac.Range(1, 1, 1, 30).SetAutoFilter();
                var headers_MAC = new List<String[]>();
                headers_MAC.Add(new String[] { "MANUFACTURER   (2 LETTER CODE) FABRICANTE DE CAMISA","COUNTRY OF ORIGIN","AMT STYLE","AMT COLOR ","BODY TYPE","COLOR","SIZE","GENDER",
                    "BLANKS (B) NORMAL (N) TRIM (T) DAMAGES (D)","FABRIC TYPE","FABRIC %","WAREHOUSE","LOCATION","QTY","","ORDER REF #","DATE OF RECEIPT (AGING)","AGE (days)",
                    "CANCEL DATE W.O","MILL PO","CUSTOMER NAME","AMT STYLE # (16 DIGITS)","AMT STYLE DESCRIPTION","DATE OF COMMENT","COMMENTS","","CODIGO DE IDENTIFICACION","MOVEMENT DATE","NOTES","MOVEMENT TYPE"});
                mac.Cell(1, 1).AsRange().AddToNamed("Titles");
                mac.Cell(1, 1).Value = headers;
                var row1_mac = mac.Row(1);
                row1_mac.Height = 80;
                row1_mac.Style.Font.Bold = true;
                row1_mac.Style.Alignment.WrapText = true;
                row1_mac.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                row1_mac.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                mac.Style.Font.FontSize = 10;
                mac.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                mac.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                mac.Cell(1, 1).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                mac.Cell(1, 1).Style.Border.LeftBorderColor = XLColor.Blue;
                mac.Column("AA").Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 0);
                var move_change = new List<string[]>();//TRANSFERENCIAS POR MES
                List<Inventario> lista_move_change = new List<Inventario>();
                lista_move_change = dr.obtener_inventario_transferencia_mes(Convert.ToInt32(Session["id_sucursal"]));//AQUI VA EL ID DE LA SUCURSAL
                if (dr.fecha_salidas != null)
                {
                    string[] fecha_salida = dr.fecha_salidas.Split('*'), id_salidas = dr.ids_salidas.Split('*');
                    int x = 1;
                    foreach (Inventario i in lista_move_change)
                    {
                        string fecha_recibo = consultas.obtener_fecha_recibo(i.id_inventario);
                        move_change.Add(new string[] {i.fabricante,i.pais,i.amt_item,i.codigo_color,i.body_type,i.color,i.size,i.genero,i.categoria_inventario,
                        i.fabric_type,i.fabric_percent,"FORT",i.location,Convert.ToString(i.total),"ADD",i.po,fecha_recibo,consultas.item_age.ToString(),"",consultas.mill_po,
                        i.customer,i.date_comment,i.comment,"HOUSE","STOCK",fecha_salida[x],dr.buscar_datos_transferencia(id_salidas[x]),"MOVE" });
                        x++;
                    }
                }                
                mac.Cell(2, 1).Value = move_change;
                dr.fecha_salidas = null;
                dr.ids_salidas = null;
        /***********S*E*C*O*N*D**T*A*B**E*N*D****************************************************************************************************************************************************************/

  
                

        /***********D*O*W*N*L*O*A*D*************************************************************************************************************************************************************************/
                HttpResponse httpResponse = System.Web.HttpContext.Current.Response;
                httpResponse.Clear();
                httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                httpResponse.AddHeader("content-disposition", "attachment;filename=\"Inventory MDE "+ DateTime.Now.ToString("MMMM dd, yyyy")+".xlsx\"");
                // Flush the workbook to the Response.OutputStream
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    libro_trabajo.SaveAs(memoryStream);
                    memoryStream.WriteTo(httpResponse.OutputStream);
                    memoryStream.Close();
                }
                httpResponse.End();
            }            
        }

        public void camreader() {
        }
        public JsonResult Scan(HttpPostedFileBase file){
            /*string barcode = "";
            try{
                string path = "";
                if (file.ContentLength > 0){
                    var fileName = Path.GetFileName(file.FileName);
                    path = Path.Combine(Server.MapPath("~/App_Data"), fileName);
                    file.SaveAs(path);
                }
                // Now we try to read the barcode
                // Instantiate BarCodeReader object
                BarCodeReader reader = new BarCodeReader(path, BarCodeReadType.Code39Standard);
                System.Drawing.Image img = System.Drawing.Image.FromFile(path);
                System.Diagnostics.Debug.WriteLine("Width:" + img.Width + " - Height:" + img.Height);
                try{
                    // read Code39 bar code
                    while (reader.Read()){
                        // detect bar code orientation
                        ViewBag.Title = reader.GetCodeText();
                        barcode = reader.GetCodeText();
                    }
                    reader.Close();
                }catch (Exception exp){
                    System.Console.Write(exp.Message);
                }
            }catch (Exception ex){
                ViewBag.Title = ex.Message;
            }*/
            //return Json(barcode);
            return Json("0");
        }

        //[HttpPost]
        public JsonResult datos_imprimir_transfer(string ID)
        {
            Session["id_transfer_ticket"] = ID;            
            return Json("0", JsonRequestBehavior.AllowGet);
        }
        //agregar_nuevo_lugar
        public JsonResult agregar_nuevo_lugar(string nombre, string direccion, string tipo)
        {
            int existencia=dt.revisar_existencia_categoria(nombre);
            if (existencia == 0){
                dt.guardar_nuevo_lugar(nombre, direccion, tipo);
            }            
            return Json(existencia, JsonRequestBehavior.AllowGet);
        }
       public JsonResult agregar_nueva_categoria(string nombre){
            int existencia=dt.revisar_existencia_categoria(nombre);
            if (existencia == 0){
                dt.guardar_nueva_categoria(nombre);
            }            
            return Json(existencia, JsonRequestBehavior.AllowGet);
        }
        public JsonResult edicion_categoria(string id,string nombre){
            int existencia=dt.revisar_existencia_categoria(nombre);
            if (existencia == 0){
                dt.editar_categorias_inventario(id, nombre);
            }            
            return Json(existencia, JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_datos_grafica(){
            return Json(dt.obtener_lista_grafica(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_datos_grafica_transferencias()
        {
            return Json(dt.obtener_lista_grafica_transferencias(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_productos_ubicacion(string ID){
            string[] cadena = ID.Split('_');
            return Json(dt.buscar_lista_productos_ubicacion(cadena[1]), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_productos_caja(string ID){
            string[] cadena = ID.Split('_');
            return Json(dt.buscar_lista_productos_cajas(cadena[1]), JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscar_estilo_cajas(string inventario){
            return Json(dt.buscar_estilo_inventario(Convert.ToInt32(inventario))+"*"+ dt.buscar_cajas_inventario(Convert.ToInt32(inventario)), JsonRequestBehavior.AllowGet);
        }

        public JsonResult aceptar_transferencia(string id){
            int id_destino = 0, existencia = 0;
            //int cantidad_temporal, total_caja, identificador_caja;
            List<salidas> transferencia = dt.obtener_informacion_transferencia(id);
            foreach (salidas item in transferencia){
                id_destino = item.id_destino;
                foreach (salidas_item i in item.lista_salidas_item){
                    Inventario inventario = new Inventario();
                    inventario = dt.consultar_item(i.id_inventario);
                    //buscar si en el destino de la transferencia ya existen esos item
                    existencia = dt.comparar_inventario(inventario, id_destino);
                    if (existencia != 0){
                        //si existen se suma 
                        di.update_stock(existencia, i.cantidad, id_destino);
                    }else{
                        //si no se agregan con la  sucursal de destino
                        dt.agregar_inventario_desde_transferencia(inventario, id_destino, i.cantidad);
                        existencia = di.obtener_ultimo_inventario();
                    }
                    //
                    //int summary = di.buscar_po_summary_inventario(Convert.ToString(existencia));
                    //BUSCAR EL SUMMARY DEL INVENTARIO QUE SE TRANSFIERE
                    dt.cambiar_sucursal_estilo(inventario.id_summary,id_destino);
                    //CAMBIAR LA SUCURSAL DE ESE ESTILO (CON EL SUMMARY)
                    dt.agregar_id_inventario_nuevo_transferencia(i.id_salida_item, existencia);
                    string[] codigo = (i.codigo).Split('_');
                    if ((i.codigo).Contains("caja")){
                        int cantidad_caja = dt.obtener_contenido_caja(Convert.ToInt32(codigo[1]));
                        if ((i.codigo).Contains("caja") && (cantidad_caja == i.cantidad)){
                            dt.cambiar_id_inventario_caja(existencia, codigo[1]);
                        }
                    }else{
                        //crear una nueva caja con ese id de inventario y cantidad
                        di.guardar_caja(i.id_salida_item, (existencia).ToString(), (i.cantidad).ToString(), (i.cantidad).ToString());
                    }
                }
            }
            dt.actualizar_transferencia(Convert.ToInt32(Session["id_usuario"]), Convert.ToInt32(id));
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult guardar_id_salida(string id){
            Session["id_salida_editar"] = id;
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult obtener_informacion_salida(){
            return Json(dt.obtener_informacion_transferencia(Convert.ToString(Session["id_salida_editar"])), JsonRequestBehavior.AllowGet);
        }
        public ActionResult abrir_edicion(){
            return View("editar_transferencia");
        }
        public JsonResult guardar_edicion_transferencia(string informacion,string estado,string items_salida,string pallet)
        {
            // 0      1            2     3       4       5       6       7       8       9      10       11    12      13     14     15       16
            //ids, cantidades, fecha, persona, sello, origen, destino, driver, pallet, envio, id_sello, pos, estilos, caja, carro, placas, codigos
            string[] datos= informacion.Split('+');
            int id_transferencia = Convert.ToInt32(Session["id_salida_editar"]), total = 0, id_sucursal = 0;
            string[] Cantidades, Ids, Pos, Estilos, Cajas, Codigo,Pallets;
            Cantidades = datos[1].Split('*');
            Ids = datos[0].Split('*');
            Pos = datos[11].Split('*');
            Estilos = datos[12].Split('*');
            Cajas = datos[13].Split('*');
            Codigo = datos[16].Split('*');
            Pallets = pallet.Split('*');
            for (int i = 1; i < Cantidades.Length; i++){
                total += Convert.ToInt32(Cantidades[i]);
            }
            dt.guardar_edicion_transferencia_inventario(id_transferencia,datos[2], datos[3], datos[4], datos[5], datos[6], datos[7], datos[8], datos[9], total, Convert.ToInt32(Session["id_usuario"]), datos[10], datos[14], datos[15]);
            id_sucursal = consultas.buscar_id_sucursal_usuario(Convert.ToInt32(Session["id_usuario"]));
            
            if (datos[4] != "0"){
                dt.aumentar_sellos(datos[4], id_sucursal);
            }
            string[] salidasitems = items_salida.Split('*');
            if (estado == "1") {//si esta aprobada                
                for(int ii= 1; ii < salidasitems.Length; ii++){
                    salidas_item sa = dt.obtener_datos_salida_item(Convert.ToInt32(salidasitems[ii]));
                    //regresar_inventario
                    di.update_inventario(sa.id_inventario, sa.cantidad);
                    //regresar cajas
                    dt.regresar_datos_cajas(sa.id_inventario, sa.cantidad);                   
                }
            }
            //borrar
            for (int ii = 1; ii < salidasitems.Length; ii++){
                dt.eliminar_salida_item(Convert.ToInt32(salidasitems[ii]));
            }

            for (int i = 1; i < Cantidades.Length; i++){
                int pedido = consultas.buscar_pedido(Pos[i]);
                int estilo = consultas.obtener_estilo_id(Estilos[i]);
                int summary = consultas.obtener_po_summary(pedido, estilo);
                dt.guardar_items_inventario(id_transferencia, Ids[i], Cantidades[i], pedido, estilo, Cajas[i], Codigo[i],summary,Pallets[i]);
            }
            return Json("0", JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_mill_po_pedido(string pedido){
            int id_pedido = consultas.buscar_pedido(pedido);
            return Json(di.obtener_mill_po_pedido(id_pedido), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_ordenes_customer_recibos(string busqueda){
            return Json(di.obtener_pedidos_customer(busqueda), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_estilos_customer_recibos(string pedido){
            Session["pedido_customer"] = pedido;
            return Json(di.obtener_estilos_customer(pedido), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_informacion_recibos(string estilo){            
            var result = Json(new{
                tallas = di.obtener_lista_tallas_summary(Convert.ToInt32(estilo)),   
                recibos_tallas=di.obtener_lista_recibos_summary(Convert.ToInt32(estilo)),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscar_informacion_item_pedido(string pedido,string estilo,string item){            
            estilos e = di.lista_summary(estilo);
            Item i = di.obtener_item(item);
            int id_cliente = consultas.obtener_customer_po(Convert.ToInt32(pedido));
            List<Item> listaItems = di.buscar_items_tallas(i,e.lista_ratio,estilo);
            var result = Json(new{
                estilo = e,
                lista_items = listaItems,
                customer = consultas.obtener_customer_id(Convert.ToString(id_cliente)),
            }); 
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult guardar_session_editar_recibo(string recibo){
            Session["recibo_edicion"] = recibo;
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_informacion_editar_recibo(){
            return Json(di.obtener_recibo_edicion(Convert.ToString(Session["recibo_edicion"])), JsonRequestBehavior.AllowGet);
        }

        public JsonResult guardar_edicion_recibo(string recibo, string mp,string id, string item, string po, string style, string mill, string po_r, string locacion, string country, string customer,  string packing_number,string sucursal,string comentarios,string total)
        {

            string[] ids = id.Split('*'), items = item.Split('*'), pos = po.Split('*'), styles = style.Split('*'), locaciones = locacion.Split('*');
            string[] countries = country.Split('*'), customers = customer.Split('*'), totales = total.Split('*');// cajas_item = caja.Split('*'), cantidades_item = cantidad.Split('*');            
            int existencia = 0, id_recibo=Convert.ToInt32(recibo);
            //POR CADA ITEM

            //BORRAR COSAS DEL 
            di.borrar_recibo_items(id_recibo);

            int total_recibo = 0;
            for (int i = 1; i < ids.Length; i++) { total_recibo += Convert.ToInt32(totales[i]); }
            di.guardar_edicion_recibo(id_recibo, total_recibo, Convert.ToInt32(Session["id_usuario"]), Convert.ToInt32(sucursal), Convert.ToInt32(sucursal), mp, mill, po_r, packing_number, comentarios);
            di.id_recibo = id_recibo;

            for (int i = 1; i < ids.Length; i++){
                di.id_tipo = consultas.buscar_tipo_inventario_item(items[i]);
                di.mill_po = mill; di.po_referencia = po_r;                
                switch (di.id_tipo){                   
                    case 1:
                        consultas.buscar_informacion_blank_item(items[i]);
                        int estilo = consultas.obtener_estilo_summary(Convert.ToInt32(styles[i]));
                        int customer_final = consultas.buscar_cliente_final_po(pos[i]);
                        di.obtener_datos_blank(Convert.ToInt32(items[i]), Convert.ToInt32(Session["id_usuario"]), styles[i], estilo, "Blanks", pos[i], countries[i], consultas.fabricante, consultas.color, consultas.body_type, consultas.size, consultas.gender, consultas.fabric_type, consultas.fabric_percent, customers[i], locaciones[i], customer_final, "N/A", "N/A", "N/A");
                        di.id_sucursal = Convert.ToInt32(sucursal);
                        di.id_sucursal = Convert.ToInt32(sucursal);
                        di.quantity = Convert.ToInt32(totales[i]);
                        di.id_usuario = Convert.ToInt32(Session["id_usuario"]);
                        existencia = di.buscar_existencia_blank_inventario();
                        if (existencia == 0){
                            di.guardar_blank();
                            di.id_inventario = di.obtener_ultimo_inventario();
                            di.guardar_recibo_item(di.id_recibo, (di.id_inventario).ToString(), totales[i], styles[i]);
                        }else{
                            di.sumar_existencia_blank(existencia);
                            di.guardar_recibo_item(di.id_recibo, existencia.ToString(), totales[i], styles[i]);
                        }
                        break;
                }//switch
            }//FOR
           
            Session["id_recibo_nuevo"] = di.id_recibo;
            return Json("0", JsonRequestBehavior.AllowGet);
        }
       /* public JsonResult guardar_edicion_recibo(string recibo, string mp, string id, string item, string po, string style, string mill, string po_r, string locacion, string country, string customer, string caja, string cantidad, string packing_number, string sucursal, string comentarios)
        {

            string[] ids = id.Split('*'), items = item.Split('*'), pos = po.Split('*'), styles = style.Split('*'), locaciones = locacion.Split('*');
            string[] countries = country.Split('*'), customers = customer.Split('*'), cajas_item = caja.Split('*'), cantidades_item = cantidad.Split('*');
            string qty_item = "", ids_inventario = "", trims_inventario = "", trims_cantidades = "", trims_item = "", ids_summary = "";
            int existencia = 0, summary = 0,id_recibo=Convert.ToInt32(recibo);
            //POR CADA ITEM

            //BORRAR COSAS DEL 
            di.borrar_recibo_items(id_recibo);

            int total_item = 0, total_recibo = 0;
            for (int i = 1; i < ids.Length; i++){
                di.id_tipo = consultas.buscar_tipo_inventario_item(ids[i]);
                di.mill_po = mill; di.po_referencia = po_r;
                summary = 0;
                switch (di.id_tipo){
                    case 2:
                        //int id_usuario, string estilo, string tipo, string po, string mill_po, string amt, string unit, string company, string cantidad, string descripcion_trim, string familia, string minimo,string referencia
                        consultas.buscar_informacion_trim_item(ids[i]);
                        total_item = obtener_cantidad_item(cajas_item[i], cantidades_item[i]);
                        qty_item += "*" + total_item.ToString();
                        total_recibo += total_item;
                        di.cantidad = total_item;
                        di.obtener_datos_trim(Convert.ToInt32(ids[i]), Convert.ToInt32(Session["id_usuario"]), styles[i], "Trims", pos[i], consultas.unit, customers[i], total_item.ToString(), consultas.descripcion, consultas.family, "0");
                        di.id_sucursal = Convert.ToInt32(sucursal);
                        existencia = di.buscar_existencia_trim_inventario();
                        summary = consultas.obtener_po_summary(di.id_pedido, di.id_estilo);
                        ids_summary += "*" + summary.ToString();
                        if (existencia == 0){
                            di.guardar_trim_po();
                            di.id_inventario = di.obtener_ultimo_inventario();
                            ids_inventario += "*" + di.id_inventario.ToString();
                        }else{
                            di.sumar_existencia_trim(existencia);
                            ids_inventario += "*" + existencia.ToString();
                        }
                        trims_inventario += "*" + consultas.obtener_po_summary(di.id_pedido, di.id_estilo);
                        trims_cantidades += "*" + total_item.ToString();
                        trims_item += "*" + ids[i];
                        break;
                    case 1:
                        consultas.buscar_informacion_blank_item(ids[i]);
                        int customer_final = consultas.buscar_cliente_final_po(pos[i]);
                        di.obtener_datos_blank(Convert.ToInt32(ids[i]), Convert.ToInt32(Session["id_usuario"]), styles[i], "Blanks", pos[i], countries[i], consultas.fabricante, consultas.color, consultas.body_type, consultas.size, consultas.gender, consultas.fabric_type, consultas.fabric_percent, customers[i], locaciones[i], customer_final, "N/A", "N/A", "N/A", cajas_item[i], cantidades_item[i]);
                        di.id_sucursal = Convert.ToInt32(sucursal);
                        qty_item += "*" + di.quantity.ToString();
                        total_recibo += di.quantity;

                        
                        summary = consultas.obtener_po_summary(di.id_pedido, di.id_estilo);
                        di.id_summary = summary;
                        ids_summary += "*" + summary.ToString();
                        existencia = di.buscar_existencia_blank_inventario();
                        if (existencia == 0){
                            di.guardar_blank();
                            di.id_inventario = di.obtener_ultimo_inventario();
                            ids_inventario += "*" + di.id_inventario.ToString();
                        }else{
                            di.sumar_existencia_blank(existencia);
                            ids_inventario += "*" + existencia.ToString();
                        }
                        break;
                }//switch
            }//FOR
            di.guardar_edicion_recibo(id_recibo,total_recibo,Convert.ToInt32(Session["id_usuario"]),Convert.ToInt32(Session["id_sucursal"]), Convert.ToInt32(Session["id_sucursal"]),mp,mill, po_r, packing_number,comentarios);
            di.id_recibo = id_recibo;
            //GenerateMyQCCode("recibo_" + di.id_recibo.ToString());
            string[] trimsInventario = trims_inventario.Split('*'), trimsCantidad = trims_cantidades.Split('*'), trimsItem = trims_item.Split('*');
                        
            string[] inventarios = ids_inventario.Split('*'), totales_items = qty_item.Split('*'), summarys = ids_summary.Split('*');
            for (int j = 1; j < inventarios.Length; j++){
                di.guardar_recibo_item(di.id_recibo, inventarios[j], totales_items[j], summarys[j]);
                di.id_recibo_item = di.obtener_ultimo_recibo_item();
                string[] cajas = cajas_item[j].Split('&'), cantidades = cantidades_item[j].Split('&');
                for (int k = 1; k < cajas.Length; k++){
                    for (int h = 0; h < Convert.ToInt32(cajas[k]); h++){
                        di.guardar_caja(di.id_recibo_item, inventarios[j], cantidades[k], cantidades[k]);
                        di.id_caja = di.obtener_id_ultima_caja();
                        //GenerateMyQCCode("caja_" + Convert.ToString(di.id_caja));
                    }
                }
            }
            Session["id_recibo_nuevo"] = di.id_recibo;
            return Json("0", JsonRequestBehavior.AllowGet);
        }*/


        public JsonResult eliminar_recibo(string recibo){
            DatosTrim dtrim = new DatosTrim();            
            di.borrar_recibo_items(Convert.ToInt32(recibo));
            dtrim.delete_recibo(Convert.ToInt32(recibo));
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscar_informacion_item(string id){
            return Json(di.obtener_item(id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult guardar_edicion_items(string id,string item,string manufacturer,string size,string color, string body_type,string gender,string fabric_type,string fabric_percent,string yarn,string descripcion){
            // data: "{'id':'",'':'"+item+"','':'"+manufacturer+"','':'"+size+"','':'"+color+"','':'"+body_type+"','':'"+gender+"','':'"+fabric_type+"','':'"+fabric_percent+"','':'"+yarn+"','':'"+descripcion+"'}",
            int existencia = di.buscar_existencia_item_edicion( id,  item,  manufacturer,  size,  color,  body_type,  gender,  fabric_type,  fabric_percent,  yarn);
            if (existencia == 0) {
                di.guardar_item_edicion(id, item, manufacturer, size, color, body_type, gender, fabric_type, fabric_percent, yarn,descripcion);
            }
            return Json(existencia, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscar_inventario_ubicacion(string id){
            var result = Json(new{
                item = dt.obtener_item_editar(Convert.ToInt32(id)),
                locations = consultas.buscar_lista_ubicaciones(),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult cambiar_ubicacion(string id,string old_total,string new_total,string ubicacion){
            int viejo_total = Convert.ToInt32(old_total), nuevo_total = Convert.ToInt32(new_total),existencia=0;
            if (viejo_total == nuevo_total){
                di.cambiar_ubicacion_inventario(id, ubicacion);
            }else {
                Inventario inventario = new Inventario();
                inventario = dt.consultar_item(Convert.ToInt32(id));
                //buscar si en el destino de la transferencia ya existen esos item
                existencia = dt.comparar_inventario_ubicacion(inventario, ubicacion);
                if (existencia != 0){//si existen se suma 
                    di.update_inventario(existencia, Convert.ToInt32(new_total));
                    dt.restar_inventario(Convert.ToInt32(id), Convert.ToInt32(new_total));
                }else{
                    //si no se agregan con la  sucursal de destino
                    dt.restar_inventario(Convert.ToInt32(id), Convert.ToInt32(new_total));
                    dt.agregar_inventario_cambio_ubicacion(inventario, Convert.ToInt32(ubicacion), Convert.ToInt32(new_total));                  
                }
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }



        public JsonResult sesion_transferencia(string id){
            Session["id_impresion_transferencia"] = id;
            //excel_transferencia(lista_salidas);
            return Json("", JsonRequestBehavior.AllowGet);
        }


        public void excel_transferencia()
        {
            List<salidas> lista_salidas = dt.lista_transfer_ticket_excel(Convert.ToInt32(Session["id_impresion_transferencia"]));
            
            using (XLWorkbook libro_trabajo = new XLWorkbook())
            {
                string archivo = "Transferencia "+ Convert.ToString(Session["id_impresion_transferencia"]);
                var ws = libro_trabajo.Worksheets.Add("TL");
                int trims = 0, blanks = 0, printed = 0, r;
                foreach (salidas item in lista_salidas)
                {
                    ws.Style.Font.FontSize =9;
                    ws.Cell("A1").Value = "***FORTUNE WHSE INVENTORY TRANSFER TICKET***";
                    ws.Cell("A1").Style.Font.FontSize = 11;
                    ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell("A1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Range("A1:T2").Merge();


                    var imagePath = Server.MapPath("/") + "/Content/img/LOGOFORTUNEEXCEL.png";
                    //var imagePath = @"C:\Users\melissa\source\repos\FortuneSys----\FortuneSystem\Content\img\LOGO FORTUNE.png";
                    //var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1")).Scale(0.30);
                    var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("A8"));

                    /*****INICIO CON DIRECCIONES, LOGO, ETC******/
                    ws.Cell("B3").Value = "REQUESTED BY:"; //ws.Range("B3:B4").Merge();
                    ws.Cell("B4").Value = "DATE REQUESTED:"; //ws.Range("B5:B6").Merge();
                    ws.Cell("B5").Value = "WHSE FROM"; //ws.Range("B7:B8").Merge();
                    ws.Cell("D5").Value = "WHSE TO"; //ws.Range("D7:D8").Merge();
                    ws.Cell("B6").Value = "# ENVIO"; //ws.Range("B9:B10").Merge();
                    ws.Cell("B7").Value = "N. SEAL"; //ws.Range("B11:B12").Merge();
                    ws.Style.Font.FontSize = 11;
                    ws.Range("B3:H12").Style.Font.Bold = true;
                    ws.Range("B3:B12").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range("C1:H16").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


                    ws.Cell("C3").Value = item.responsable; //ws.Range("C3:D4").Merge();
                    ws.Range("C3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range("C3").Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Cell("C4").Value = item.fecha_solicitud; //ws.Range("C5:D6").Merge();
                    ws.Range("C4").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range("C4").Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Cell("C5").Value = item.origen; //ws.Range("C7:D8").Merge();
                    ws.Range("C5").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range("C5").Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Cell("E5").Value = item.destino; //ws.Range("E7:H8").Merge();
                    ws.Range("E5").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range("E5").Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Cell("C6").Value = item.id_envio; //ws.Range("C9:F10").Merge();
                    ws.Range("C6").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range("C6").Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Cell("C7").Value = item.sello;// ws.Range("C11:F12").Merge();
                    ws.Range("C7").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range("C7").Style.Border.BottomBorderColor = XLColor.Black;

                    ws.Cell("D9").Value = "PRINTED";
                    ws.Cell("D10").Value = "BLANKS";
                    ws.Cell("D11").Value = "TRIM";
                    foreach (salidas_item si in item.lista_salidas_item){
                        if (si.id_categoria == 1) { blanks++; }
                        if (si.id_categoria == 2) { trims++; }
                        if (si.id_categoria == 3) { printed++; }
                    }
                    if (printed != 0) { ws.Cell("E9").Value = "X"; }
                    if (blanks != 0) { ws.Cell("E10").Value = "X"; }
                    if (trims != 0) { ws.Cell("E11").Value = "X"; }

                    ws.Range("D14:D16").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    for (int i = 9; i < 12; i++)
                    {
                        ws.Cell(i, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(i, 5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell(i, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Cell(i, 5).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(i, 5).Style.Border.LeftBorderColor = XLColor.Black;
                        ws.Cell(i, 5).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        ws.Cell(i, 5).Style.Border.RightBorderColor = XLColor.Black;
                        ws.Cell(i, 5).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(i, 5).Style.Border.TopBorderColor = XLColor.Black;
                        ws.Cell(i, 5).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(i, 5).Style.Border.BottomBorderColor = XLColor.Black;
                    }
                    ws.Range("D9:E11").Style.Font.Bold = true;

                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    r = 14;
                    var headers = new List<String[]>();
                    List<String> titulos = new List<string>();//=  "ID", "P.O. NUM", "TYPE", "COLOR", "DESCRIPTION";
                    titulos.Add("CNT"); titulos.Add("P.O. NUMBER"); titulos.Add("DESCRIPTION"); titulos.Add("FABRIC TYPE"); titulos.Add("STYLE NO."); titulos.Add("SIZE");
                    titulos.Add("COLOR"); titulos.Add("GENDER"); titulos.Add("TOTAL"); titulos.Add("PALLETS");
                    headers.Add(titulos.ToArray());
                    int total_titulos = (titulos.ToArray()).Length;
                    ws.Cell(r, 1).Value = headers;
                    for (int i = 1; i <= 10; i++)
                    {
                        ws.Cell(14, i).Style.Font.Bold = true;
                        ws.Cell(14, i).Style.Font.FontSize = 10;
                        ws.Cell(14, i).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                        ws.Cell(14, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(14, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Cell(14, i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(14, i).Style.Border.LeftBorderColor = XLColor.Black;
                        ws.Cell(14, i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        ws.Cell(14, i).Style.Border.RightBorderColor = XLColor.Black;
                        ws.Cell(14, i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(14, i).Style.Border.TopBorderColor = XLColor.Black;
                        ws.Cell(14, i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(14, i).Style.Border.BottomBorderColor = XLColor.Black;
                        //ws.Range(18, i, 19, i).Merge();
                    }
                    /**********************************************************************************************************************************************************************************************/
                    /***********************C*A*B*E*C*E*R*A*S**D*E**L*A**T*A*B*L*A*********************************************************************************************************************************/
                    //int[] tallas_comparacion = tallas_id_temporal.ToArray();                     
                    r = 15;
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************

                    List<int> tarimas = new List<int>();

                    int total_pallets_transferencia = 0;
                    int articulos_pallet = 0;
                    List<int> pallets = new List<int>();
                    foreach (var item_trans in item.lista_salidas_item){
                        pallets.Add(item_trans.total_pallets);
                    }
                    pallets = pallets.Distinct().ToList();
                    total_pallets_transferencia = pallets.Count();

                    foreach (int p in pallets)//////////////////////////////////////////////TARIMAS TRANSFER
                    {
                        var fila = new List<String[]>();
                        articulos_pallet = 0;
                        foreach (var item_trans in item.lista_salidas_item)
                        {
                            if (p == item_trans.total_pallets)
                            {
                                articulos_pallet++;
                            }
                        }

                        ws.Cell(r, 10).Value = p;
                        ws.Range(r, 10, (r + articulos_pallet - 1), 10).Merge();

                        foreach (var item_trans in item.lista_salidas_item)
                        {
                            if (p == item_trans.total_pallets)
                            {
                                foreach (Inventario inventario in item_trans.lista_inventario) {
                                    List<String> datos = new List<string>();
                                    datos.Add((item_trans.id_inventario).ToString());
                                    datos.Add((item_trans.po).Trim());
                                    datos.Add((item_trans.descripcion).Trim());
                                    datos.Add((inventario.fabric_type).Trim());
                                    datos.Add((item_trans.estilo).Trim());
                                    datos.Add(inventario.size);
                                    datos.Add((inventario.color).Trim());
                                    datos.Add((item_trans.genero).Trim());
                                    datos.Add((item_trans.cantidad).ToString());
                                    fila.Add(datos.ToArray());
                                    ws.Cell(r, 1).Value = fila;
                                    r++;
                                }
                            }
                        }

                    }//////////////////////////////////////////////TARIMAS TRANSFER

                    for (int i = 15; i <= (r-1); i++)
                    {
                        for (int j = 1; j <= 10; j++)
                        {
                            ws.Cell(i, j).Style.Font.FontSize = 9;
                            ws.Cell(i, j).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            ws.Cell(i, j).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(i, j).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            ws.Cell(i, j).Style.Border.LeftBorderColor = XLColor.Black;
                            ws.Cell(i, j).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            ws.Cell(i, j).Style.Border.RightBorderColor = XLColor.Black;
                            ws.Cell(i, j).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            ws.Cell(i, j).Style.Border.TopBorderColor = XLColor.Black;
                            ws.Cell(i, j).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            ws.Cell(i, j).Style.Border.BottomBorderColor = XLColor.Black;

                        }
                    }

                    r++;
                    int row_abajo = r;
                    ws.Cell(r,1).Value = "*********BELOW IS FOR INVENTORY CONTROL USE ONLY*********";
                    ws.Cell(r,1).Style.Font.FontSize = 11;
                    ws.Cell(r,1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(r,1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Range(r,1,17,1).Merge();
                    r++;
                    ws.Cell(r, 5).Value = "# OF PALLET";
                    ws.Cell(r,6).Value = "SHIPPED";
                    ws.Cell(r, 5).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Cell(r, 5).Style.Border.LeftBorderColor = XLColor.Black;
                    ws.Cell(r, 5).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    ws.Cell(r, 5).Style.Border.RightBorderColor = XLColor.Black;
                    ws.Cell(r, 5).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Cell(r, 5).Style.Border.TopBorderColor = XLColor.Black;
                    ws.Cell(r, 5).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Cell(r, 5).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Cell(r,6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(r,7).Value = item.usuario;
                    ws.Cell(r,7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(r, 7, r, 9).Style.Border.BottomBorderColor = XLColor.Black;
                    r++;
                    ws.Cell(r, 5).Value = total_pallets_transferencia;
                    ws.Cell(r, 5).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Cell(r, 5).Style.Border.LeftBorderColor = XLColor.Black;
                    ws.Cell(r, 5).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    ws.Cell(r, 5).Style.Border.RightBorderColor = XLColor.Black;
                    ws.Cell(r, 5).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Cell(r, 5).Style.Border.TopBorderColor = XLColor.Black;
                    ws.Cell(r, 5).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Cell(r, 5).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Cell(r, 7).Value = "NAME";
                    ws.Cell(r, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    r++;
                    ws.Cell(r, 6).Value = "DRIVER";
                    ws.Cell(r, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(r, 7).Value = item.driver;
                    ws.Cell(r, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(r, 7, r, 9).Style.Border.BottomBorderColor = XLColor.Black;
                    r++;
                    ws.Cell(r, 7).Value = "NAME";
                    r++;
                    ws.Cell(r, 5).Value = "CARRIER";
                    ws.Cell(r, 5).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Cell(r, 5).Style.Border.LeftBorderColor = XLColor.Black;
                    ws.Cell(r, 5).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    ws.Cell(r, 5).Style.Border.RightBorderColor = XLColor.Black;
                    ws.Cell(r, 5).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Cell(r, 5).Style.Border.TopBorderColor = XLColor.Black;
                    ws.Cell(r, 5).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Cell(r, 5).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Cell(r, 6).Value = "RECEIVED";
                    ws.Cell(r, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(r, 7, r, 9).Style.Border.BottomBorderColor = XLColor.Black;
                    r++;
                    ws.Cell(r, 7).Value = "NAME";
                    ws.Cell(r, 5).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Cell(r, 5).Style.Border.LeftBorderColor = XLColor.Black;
                    ws.Cell(r, 5).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    ws.Cell(r, 5).Style.Border.RightBorderColor = XLColor.Black;
                    ws.Cell(r, 5).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Cell(r, 5).Style.Border.TopBorderColor = XLColor.Black;
                    ws.Cell(r, 5).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Cell(r, 5).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(row_abajo,5,r,10).Style.Font.Bold = true;




                }//SALIDAS
                ws.Rows().AdjustToContents();
                //ws.Columns().AdjustToContents();

                /***********D*O*W*N*L*O*A*D*************************************************************************************************************************************************************************/
                HttpResponse httpResponse = System.Web.HttpContext.Current.Response;
                httpResponse.Clear();
                httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                httpResponse.AddHeader("content-disposition", "attachment;filename=\"" + archivo + ".xlsx\"");

                // Flush the workbook to the Response.OutputStream
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    libro_trabajo.SaveAs(memoryStream);
                    memoryStream.WriteTo(httpResponse.OutputStream);
                    memoryStream.Close();
                }
                httpResponse.End();

            }
        }

































    }
}