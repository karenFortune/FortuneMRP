using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FortuneSystem.Models.Almacen;
using FortuneSystem.Models.Trims;
using FortuneSystem.Models.Shipping;
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
using Rotativa;
using System.Text.RegularExpressions;
using System.Drawing.Printing;
using System.Net.Sockets;

namespace FortuneSystem.Controllers
{
    public class TrimsController : Controller {
        DatosInventario di = new DatosInventario();
        DatosTrim dtrim = new DatosTrim();
        //DatosReportes dr = new DatosReportes();
        FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
        DatosTransferencias dt = new DatosTransferencias();
        QRCodeController qr = new QRCodeController();
        PDFController pdf = new PDFController();
        DatosShipping ds = new DatosShipping();
        string filename, footer_alineacion, footer_size, vista;
        private object DialogResult;
        /*******************************************************************************************************************************************************/
        /*INICIO*/
        public ActionResult Index(){
            int id_usuario = Convert.ToInt32(Session["id_Empleado"]);
            Session["id_usuario"] = id_usuario;
            ViewBag.usuario = id_usuario;
            Session["id_sucursal"] = Convert.ToInt32(consultas.obtener_sucursal_id_usuario(Convert.ToInt32(Session["id_usuario"])));
            ViewBag.sucursal = Convert.ToInt32(Session["id_sucursal"]);
            return View();
        }
        public JsonResult buscar_familias_trims(){
            var result = Json(new{
                familias = dtrim.lista_familias_trims(),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_ordenes_tabla_inicio(){
            return Json(dtrim.obtener_lista_ordenes_estados("0"), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_inventario_items_trim(){
            return Json(dtrim.lista_trim_items(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_trims_recibidos_tabla_inicio(string busqueda, string fecha){
            var result = Json(new
            {
                tabla = dtrim.obtener_lista_trims_inicio(busqueda, fecha),
                mps = dtrim.buscar_mp_recibos_hoy()
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult customer_trims(){
            return View();
        }
        public ActionResult trims_entregados(){
            return View();
        }
        public JsonResult StayingAlive(){
            int id_usuario = Convert.ToInt32(Session["id_Empleado"]);
            Session["id_usuario"] = id_usuario;
            return Json(id_usuario, JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_sucursales(){
            return Json(dtrim.lista_sucursales(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_solo_estilos(string pedido){
            var result = Json(new{
                estilos = dtrim.lista_estilos_dropdown(pedido)
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_trims_tabla_inicio(){
            return Json(dtrim.lista_trims_tabla_inicio(Convert.ToInt32(Session["id_sucursal"])), JsonRequestBehavior.AllowGet);
        }
        /*INICIO*/
        /*******************************************************************************************************************************************************/
        /*******************************************************************************************************************************************************/
        /*ITEMS*/
        public JsonResult buscar_items_trims(){
            var result = Json(new{
                trim = dtrim.lista_descripciones_trims(),
                familias = dtrim.lista_familias_trims(),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_informacion_trim(string id){
            var result = Json(new{
                trim = dtrim.informacion_editar_item_trim(id),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult eliminar_trim_catalogo(string id){
            dtrim.eliminar_item_catalogo(Convert.ToInt32(id));
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult editar_trim_catalogo(string id, string item, string minimo, string descripcion, string family, string unidad){
            dtrim.editar_informacion_trim(id, item, minimo, descripcion, family, unidad);
            return Json("", JsonRequestBehavior.AllowGet);
        }
       
        /*ITEMS*/
        /*******************************************************************************************************************************************************/
        /*******************************************************************************************************************************************************/
        /*RECIBOS*/
        public JsonResult buscar_pedidos(){
            return Json(dtrim.lista_ordenes(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_items_recibos_trims(string busqueda){
            return Json(dtrim.busqueda_items_trims(busqueda), JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_recibo_trims(string pedido, string mp, string millpo, string po_number, string item, string estilo, string cantidad, string packing_number, string cliente, string sucursal, string fecha, string comentarios, string ubicacion){
            string[] items = item.Split('*'), estilos = estilo.Split('*'), cantidades = cantidad.Split('*'), ubicaciones = ubicacion.Split('*');//, pedidos= pedido.Split('*');
            int total = 0, existencia = 0;
            string ids_inventario = "", qty_item = "";
            //int customer = consultas.obtener_customer_final_po(Convert.ToInt32(pedido));
            for (int i = 1; i < items.Length; i++){
                total += Convert.ToInt32(cantidades[i]);
                qty_item += "*" + cantidades[i];
            }
            di.id_usuario = Convert.ToInt32(Session["id_usuario"]);
            di.id_sucursal = Convert.ToInt32(sucursal);
            di.id_company = Convert.ToInt32(cliente);
            di.id_customer = Convert.ToInt32(cliente);
            di.guardar_recibo_trims(total, Convert.ToInt32(cliente), millpo, po_number, packing_number, comentarios, fecha + DateTime.Now.ToString(" HH:mm:ss"));
            di.id_recibo = di.obtener_ultimo_recibo();
            dtrim.update_tipo_recibo(di.id_recibo);
            di.agregar_mp_recibo(Convert.ToString(di.id_recibo), mp);
            for (int i = 1; i < items.Length; i++){
                if (pedido == "0"){
                    consultas.buscar_informacion_trim_item(items[i]);
                    di.cantidad = Convert.ToInt32(cantidades[i]);
                    //di.obtener_datos_trim(Convert.ToInt32(items[i]), Convert.ToInt32(Session["id_usuario"]), estilos[i], "Trims", pos[i], consultas.unit, customers[i], total_item.ToString(), consultas.descripcion, consultas.family, "0");
                    di.id_item = Convert.ToInt32(items[i]);
                    di.id_ubicacion = Convert.ToInt32(ubicaciones[i]);
                    di.id_estilo = Convert.ToInt32(estilos[i]);
                    di.id_tipo = consultas.buscar_tipo_inventario("Trims");
                    di.id_unit = consultas.buscar_unit(consultas.unit);
                    di.id_customer = Convert.ToInt32(cliente);
                    di.id_familia = consultas.buscar_familia_trim(dtrim.obtener_family_trim_item(items[i]));
                    di.id_pedido = Convert.ToInt32(pedido);
                    di.id_trim = 0;
                    di.descripcion = Regex.Replace(dtrim.obtener_descripcion_item(Convert.ToInt32(items[i])) + " " + dtrim.obtener_family_trim_item(items[i]), @"\s+", " ");
                    di.total = Convert.ToInt32(cantidades[i]);
                    di.minimo_trim = 0;
                    existencia = di.buscar_existencia_trim_inventario();
                    if (existencia == 0){
                        di.guardar_trim_po();
                        di.id_inventario = di.obtener_ultimo_inventario();
                        ids_inventario += "*" + di.id_inventario.ToString();
                    }else{
                        di.update_stock(existencia, Convert.ToInt32(cantidades[i]), Convert.ToInt32(sucursal));
                        ids_inventario += "*" + existencia.ToString();
                    }                   
                }else{
                    ids_inventario += "*" + items[i];
                    dtrim.guardar_stock_inventario(pedido, items[i], cantidades[i]);
                }
            }   
            string[] inventarios = ids_inventario.Split('*'), totales_items = qty_item.Split('*');
            for (int j = 1; j < inventarios.Length; j++){
                dtrim.guardar_recibo_item(di.id_recibo, inventarios[j], totales_items[j], pedido, ubicaciones[j]);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_informacion_recibos_trims(string busqueda){
            return Json(dtrim.obtener_lista_recibos_trim(busqueda), JsonRequestBehavior.AllowGet);
        }
        public JsonResult obtener_informacion_recibos_edicion(string recibo){
            string query = "SELECT r.id_origen,r.comentarios,r.id_recibo,r.fecha,r.total,r.mp_number,r.mill_po,r.po_reference,r.id_usuario,r.id_sucursal,r.packing_number from recibos r where r.id_recibo=" + recibo;
            return Json(dtrim.obtener_lista_recibos_trim_query(query), JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_edicion_recibo_trims(string recibo, string pedido, string mp, string millpo, string po_number, string item, string estilo, string cantidad, string packing_number, string cliente, string sucursal, string fecha, string comentarios, string ubicacion)
        {
            string[] items = item.Split('*'), estilos = estilo.Split('*'), cantidades = cantidad.Split('*'), ubicaciones = ubicacion.Split('*');//,pedidos=pedido.Split('*');
            int total = 0, existencia = 0;
            string ids_inventario = "", qty_item = "";
            for (int i = 1; i < items.Length; i++){
                total += Convert.ToInt32(cantidades[i]);
                qty_item += "*" + cantidades[i];
            }
            dtrim.eliminar_items_recibo(Convert.ToInt32(recibo));

            di.id_usuario = Convert.ToInt32(Session["id_usuario"]);
            di.id_sucursal = Convert.ToInt32(sucursal);
            di.id_company = Convert.ToInt32(cliente);
            di.id_customer = Convert.ToInt32(cliente);            
            di.id_recibo = Convert.ToInt32(recibo);
            for (int i = 1; i < items.Length; i++){
                if (pedido == "0"){
                    consultas.buscar_informacion_trim_item(items[i]);
                    di.cantidad = Convert.ToInt32(cantidades[i]);
                    //di.obtener_datos_trim(Convert.ToInt32(items[i]), Convert.ToInt32(Session["id_usuario"]), estilos[i], "Trims", pos[i], consultas.unit, customers[i], total_item.ToString(), consultas.descripcion, consultas.family, "0");
                    di.id_item = Convert.ToInt32(items[i]);
                    di.id_estilo = Convert.ToInt32(estilos[i]);
                    di.id_tipo = consultas.buscar_tipo_inventario("Trims");
                    di.id_unit = consultas.buscar_unit(consultas.unit);
                    di.id_customer = Convert.ToInt32(cliente);
                    di.id_familia = consultas.buscar_familia_trim(dtrim.obtener_family_trim_item(items[i]));
                    di.id_pedido = Convert.ToInt32(pedido);
                    di.id_trim = 0;
                    di.descripcion = dtrim.obtener_descripcion_item(Convert.ToInt32(items[i])) + " " + dtrim.obtener_family_trim_item(items[i]);
                    di.descripcion = Regex.Replace(di.descripcion, @"\s+", " ");
                    di.total = Convert.ToInt32(cantidades[i]);
                    di.minimo_trim = 0;
                    di.id_ubicacion = Convert.ToInt32(ubicaciones[i]);
                    existencia = di.buscar_existencia_trim_inventario();                   
                    if (existencia == 0){
                        di.guardar_trim_po();
                        di.id_inventario = di.obtener_ultimo_inventario();                        
                        ids_inventario += "*" + di.id_inventario.ToString();
                    }else{                        
                        di.update_stock(existencia, Convert.ToInt32(cantidades[i]), Convert.ToInt32(sucursal));                                              
                        ids_inventario += "*" + existencia.ToString();
                    }
                }else {
                    ids_inventario += "*" + items[i];
                    dtrim.guardar_stock_inventario(pedido, items[i], cantidades[i]);
                }
            }            
            string[] inventarios = ids_inventario.Split('*'), totales_items = qty_item.Split('*');
            for (int j = 1; j < inventarios.Length; j++){
                dtrim.guardar_recibo_item(di.id_recibo, inventarios[j], totales_items[j], pedido, ubicaciones[j]);
            }//EDITAR TOTAL RECIBO
            dtrim.actualizar_recibo_trim(recibo, total, mp, millpo, po_number, packing_number, sucursal, Convert.ToInt32(Session["id_usuario"]), fecha, comentarios, cliente);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult eliminar_recibo(string recibo){
            dtrim.eliminar_items_recibo(Convert.ToInt32(recibo));
            dtrim.delete_recibo(Convert.ToInt32(recibo));
            return Json("", JsonRequestBehavior.AllowGet);
        }
        /*RECIBOS*/
        /*******************************************************************************************************************************************************/
        /*******************************************************************************************************************************************************/
        /*TRIM REQUEST*/
        public JsonResult buscar_estilos(string pedido){
            Session["pedido_trim_request"] = pedido;
            var result = Json(new{
                trims_anteriores = dtrim.obtener_trims_anteriores_orden(pedido),
                estilos = dtrim.lista_estilos_dropdown(pedido),
                empaque = dtrim.buscar_estado_instruccion_empaque(Convert.ToInt32(Session["pedido_trim_request"])),
                fold_size = dtrim.buscar_fold_size_pedido(Convert.ToInt32(Session["pedido_trim_request"])),
                hang_size = dtrim.buscar_hang_size_pedido(Convert.ToInt32(Session["pedido_trim_request"])),
                estado = dtrim.buscar_estado_trim_pedido(Convert.ToInt32(Session["pedido_trim_request"])),
                tallas = dtrim.lista_tallas_pedido(pedido),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_total_estilo(string pedido, string summary, string talla, string item){
            string familia = dtrim.obtener_family_trim_item(item);
            int cartonlabel = 0;
            if (familia == "CARTON LABEL") { cartonlabel = 1; }
            var result = Json(new{
                total = dtrim.obtener_total_estilo(summary, talla),
                categoria = dtrim.obtener_family_trim_item(item),
                cajas = dtrim.obtener_cajas_estilo(summary, pedido),
                carton_label = cartonlabel,
                //trims_anteriores=dtrim.obtener_trims_anteriores(summary)//buscar las tallas del esa
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult enviar_datos_request(string pedido, string total, string estilo, string item, string talla, string cantidad, string blank, string request, string request_original, string instruccion, string fold_size, string hang_size, string estado)
        {
            string[] totales = total.Split('*'), estilos = estilo.Split('&'), items = item.Split('*'), tallas = talla.Split('&');
            string[] cantidades = cantidad.Split('*'), blanks = blank.Split('*'), requests = request.Split('*'), requests_originales = request_original.Split('*');
            int id_pedido = Convert.ToInt32(pedido);
            //buscar request a eliminar
            Session["cantidades_revisiones"] = cantidad;
            Session["items_revisiones"] = item;
            int cliente = consultas.obtener_customer_po(Convert.ToInt32(Session["pedido_trim_request"]));
            //Session["pedido_trim_request"])
            List<int> lista_original = new List<int>();
            for (int i = 1; i < requests_originales.Length; i++) { lista_original.Add(Convert.ToInt32(requests_originales[i])); }
            List<int> lista_nuevos = new List<int>();
            for (int i = 1; i < requests.Length; i++) { lista_nuevos.Add(Convert.ToInt32(requests[i])); }
            List<int> lista_final = lista_original.Except(lista_nuevos).ToList();
            foreach (int t in lista_final) { dtrim.eliminar_trim_request(Convert.ToString(t)); }
            List<Trim_requests> lista_existencias = new List<Trim_requests>();
            List<int> lista_request_id = new List<int>();

            for (int i = 1; i < totales.Length; i++){
                if (requests[i] != "0"){
                    dtrim.modificar_request(requests[i], totales[i], Convert.ToInt32(Session["id_usuario"]), cantidades[i], blanks[i]);                    
                    lista_request_id.Add(Convert.ToInt32(requests[i]));
                }else{
                    dtrim.guardar_request(pedido, totales[i], "0", items[i], "0", Convert.ToInt32(Session["id_usuario"]), cantidades[i], blanks[i]);
                    int ultimo_request = dtrim.obtener_ultimo_request();
                    string[] summarys_estilos = estilos[i].Split('*');
                    for (int re = 1; re < summarys_estilos.Length; re++){
                        string[] tallas_estilos = tallas[i].Split('*');
                        dtrim.guardar_request_estilo(summarys_estilos[re], ultimo_request);
                        int ultimo_estilo_request = dtrim.obtener_ultimo_request_estilo();
                        for (int te = 1; te < tallas_estilos.Length; te++){
                            dtrim.guardar_request_estilo_talla(ultimo_estilo_request, tallas_estilos[te]);
                        }
                    }
                    string familia = dtrim.obtener_family_trim_item(items[i]);
                    if (familia == "PRICE TICKETS" || familia == "UPC STICKERS" || familia == "HOUSE LABEL" || familia == "STICKERS" || familia == "CARTON LABEL"){
                        List<Impresion> impresiones = dtrim.obtener_impresiones_orden(pedido);
                        foreach (Impresion p in impresiones){
                            Trim_requests tr = dtrim.obtener_trim_request(ultimo_request);
                            if (familia == "PRICE TICKETS" && p.tipo == "PRICE TICKETS"){
                                dtrim.cambiar_estado_trim_request(ultimo_request, 3);
                                dtrim.marcar_price_ticket_orden_impreso(pedido);
                                dtrim.marcar_pt_impreso(ultimo_request, 1);
                                foreach (Trim_estilo te in tr.lista_estilos){
                                    foreach (Talla_t t in te.lista_tallas){
                                        dtrim.marcar_talla_impresa(t.id, 1);
                                    }
                                }
                            }else{
                                if (familia != "PRICE TICKETS" && p.tipo == "0"){
                                    foreach (Trim_estilo te in tr.lista_estilos){
                                        foreach (Talla_t tt in te.lista_tallas){
                                            dtrim.marcar_talla_impresa(tt.id, 1);
                                        }
                                    }
                                    dtrim.marcar_pt_impreso(tr.id_request, 1);
                                }
                            }                            
                        }
                    }
                    lista_request_id.Add(ultimo_request);
                }
            }
            int instruccion_empaque = dtrim.buscar_instruccion_empaque(Convert.ToInt32(Session["pedido_trim_request"]));
            if (instruccion_empaque == 0){
                dtrim.agregar_instruccion_empaque(Convert.ToInt32(Session["id_usuario"]), Convert.ToInt32(Session["pedido_trim_request"]), instruccion);
            }else{
                dtrim.update_instruccion_empaque(instruccion_empaque, Convert.ToInt32(Session["id_usuario"]), instruccion);
            }
            dtrim.ingresar_fold_size(Convert.ToInt32(Session["pedido_trim_request"]), fold_size, hang_size, estado);
            /******************************************************************************************************************************/
            return Json(lista_existencias, JsonRequestBehavior.AllowGet);
        }
        /*TRIM REQUEST*/
        /*******************************************************************************************************************************************************/
        /*******************************************************************************************************************************************************/
        /*COLORES*/
        public JsonResult buscar_todos_pedidos(){
            return Json(dtrim.lista_ordenes_todas(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult datos_reporte_colores(string orden, string estado, string customer, string inicio, string final){
            Session["orden_colores"] = orden;
            Session["estado_colores"] = estado;
            Session["customer_colores"] = customer;
            Session["inicio_colores"] = inicio;
            Session["final_colores"] = final;
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public void excel_reporte_colores()
        {
            List<Pedidos_trim> lista_pedidos = new List<Pedidos_trim>();
            List<Family_trim> lista_familias = dtrim.obtener_familias();
            string orden_colores = Convert.ToString(Session["orden_colores"]);
            string estado_colores = Convert.ToString(Session["estado_colores"]);
            string customer_colores = Convert.ToString(Session["customer_colores"]);
            string inicio_colores = Convert.ToString(Session["inicio_colores"]);
            string final_colores = Convert.ToString(Session["final_colores"]);
            if (orden_colores != "0"){//SI ES DE UNA ORDEN EN ESPECÍFICO
                lista_pedidos = dtrim.obtener_pedidos_reporte_colores_orden(orden_colores);
            }else{//SIN ORDEN EN ESPECÍFICO
                lista_pedidos = dtrim.obtener_pedidos_reporte_estado_customer(estado_colores, customer_colores, inicio_colores, final_colores);
            }
            int row = 1, col = 0;
            using (XLWorkbook libro_trabajo = new XLWorkbook()){ //Regex.Replace(pedido, @"\s+", " "); 
                var ws = libro_trabajo.Worksheets.Add("Trims");
                //CABECERAS TABLA
                //ws.Worksheets.Add("AutoFilter");
                int total_familias = 0;
                ws.Cell(row, 1).Value = "IDENTIFICATION OF COLORS";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Range(row, 1, row, 4).Merge();
                ws.Cell(row, 5).Value = "PENDING";
                ws.Cell(row, 5).Style.Font.Bold = true;
                ws.Range(row, 5, row, 7).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 122);//AMARILLO
                //ws.Range(row, 5, row, 7).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 122);//AMARILLO
                ws.Cell(row, 8).Value = "IN TRANSIT";
                ws.Cell(row, 8).Style.Font.Bold = true;
                ws.Range(row, 8, row, 11).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 133, 133);//ROJO
                //ws.Range(row, 8, row, 11).Style.Fill.BackgroundColor = XLColor.FromArgb(174, 252, 174);//VERDE
                ws.Cell(row, 12).Value = "RECEIVED -DATE AND QTY'S";
                ws.Cell(row, 12).Style.Font.Bold = true;
                ws.Range(row, 12, row, 15).Style.Fill.BackgroundColor = XLColor.FromArgb(174, 252, 174);//ROJO
                //ws.Range(row, 12, row, 15).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 133, 133);//ROJO

                row++;
                var headers = new List<String[]>();
                List<String> titulos = new List<string>();
                titulos.Add("PO"); titulos.Add("CUSTOMER"); titulos.Add("SHIP DATE"); titulos.Add("GENDER"); titulos.Add("BULK/PPK/BREAKDOWN"); titulos.Add("PACKING INST.");
                foreach (Family_trim f in lista_familias) { titulos.Add(f.family_trim); total_familias++; }
                headers.Add(titulos.ToArray());
                ws.Cell(row, 1).Value = headers;
                ws.Range(row, 1, row, (total_familias + 6)).Style.Fill.BackgroundColor = XLColor.FromArgb(52, 121, 191);
                ws.Range(row, 1, row, (total_familias + 6)).SetAutoFilter();
                ws.Range(row, 1, row, (total_familias + 6)).Style.Font.FontSize = 12;
                ws.Range(row, 1, row, (total_familias + 6)).Style.Font.Bold = true;
                for (int i = 1; i <= (total_familias + 6); i++){
                    ws.Cell(row, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }
                row++; //AGREGAR CABECERA TABLA
                int template_pt = 0;
                foreach (Pedidos_trim p in lista_pedidos){
                    var celdas = new List<String[]>();
                    List<String> datos = new List<string>();
                    datos.Add(Regex.Replace(p.pedido + " " + p.estado, @"\s+", " "));
                    datos.Add(Regex.Replace(p.customer, @"\s+", " "));
                    datos.Add(p.ship_date);
                    datos.Add(p.gender);
                    //AGREGAR AQUI EL PACKING                   
                    var d = ws.Cell(row, 5);
                    int cont_emp = 0;
                    foreach (Empaque e in p.lista_empaque){
                        //d.RichText.AddText((e.estilo).Trim());
                        if (cont_emp == 0){
                            d.RichText.AddText(Environment.NewLine);
                            if (e.tipo_empaque == 1 || e.tipo_empaque == 5){
                                d.RichText.AddText("BP ");
                                foreach (ratio_tallas rt in e.lista_ratio){
                                    d.RichText.AddText(rt.talla + ": " + rt.piezas + " ");
                                }
                            }else{
                                d.RichText.AddText("PPK ");
                                foreach (ratio_tallas rt in e.lista_ratio){
                                    d.RichText.AddText(" " + rt.ratio);
                                }
                            }
                            d.RichText.AddText(Environment.NewLine);
                        }
                        cont_emp++;
                    }
                    foreach (Assortment a in p.lista_assort){
                        d.RichText.AddText(" ASSORT " + a.cartones);
                        d.RichText.AddText(Environment.NewLine);
                    }
                    ws.Cell(row, 5).Style.Alignment.WrapText = true;
                    if ((p.pedido).Contains("HOT")) { ws.Cell(row, 5).Value = "PCS IN BREAKDOWN"; }
                    if ((p.pedido).Contains("JCP") && (p.pedido).Contains("ECOM")) { ws.Cell(row, 5).Value = "PENDING BREAKDOWN"; }
                    //AGREGAR AQUI EL PACKING
                    ws.Cell(row, 6).Value = p.fold_size;

                    if (p.id_customer == 1) { ws.Cell(row, 2).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 203, 229); }
                    col = 7;
                    int pt_impresos = 0, price_ticket = 0, tomado_stock, impresos_otros = 0;
                    foreach (Family_trim f in p.lista_families){
                        template_pt = 0; pt_impresos = 0; price_ticket = 0; tomado_stock = 0; impresos_otros = 0;
                        string texto = "", comentario = " Receives:\n ";
                        var c = ws.Cell(row, col); //C de CELDA
                        int estado_1 = 0, estado_2 = 0, estado_3 = 0, recibo = 0, total_total = 0;
                        List<int> lista_estados = new List<int>();
                        foreach (Trim_requests t in f.lista_requests){
                            texto = t.item + " "; //+t.cantidad+"/"+t.total;
                            c.RichText.AddText(Regex.Replace(texto + ":" + t.total, @"\s+", " "));
                            c.RichText.AddText(Environment.NewLine);
                            total_total += t.total;
                            lista_estados.Add(t.id_estado);
                            /* foreach (Trim_estilo te in t.lista_estilos){
                                 c.RichText.AddText(te.descripcion + " ");
                                 c.RichText.AddText(Environment.NewLine);
                             }
                             int conta_tallas = 0;
                             estilos_trim = "";
                             foreach (Trim_estilo te in t.lista_estilos){
                                 if (conta_tallas == 0){
                                     foreach (Talla_t ta in te.lista_tallas){
                                         estilos_trim += " " + ta.talla;
                                         conta_tallas++;
                                     }
                                 }
                             }*/
                            //c.RichText.AddText(estilos_trim);
                            // c.RichText.AddText(Environment.NewLine);
                            if (f.family_trim == "PRICE TICKETS"){
                                price_ticket++;
                                if (t.templates_pt != 0) { template_pt++; }
                                if (t.impreso == 1) { pt_impresos++; }
                            }
                            if (f.family_trim == "PRICE TICKETS" || f.family_trim == "UPC STICKERS" || f.family_trim == "HOUSE LABEL" || f.family_trim == "STICKERS" || f.family_trim == "CARTON LABEL"){
                                price_ticket++;
                                if (t.impreso == 1){
                                    impresos_otros++;
                                    c.RichText.AddText(" IMPRESO");
                                }else{
                                    c.RichText.AddText(" NO IMPRESO");
                                }
                            }
                            c.RichText.AddText(Environment.NewLine);
                            /*if (t.recibo != 0){
                                recibo++;
                                comentario += t.recibo_item.fecha + " MP " + t.recibo_item.mp_number + "\n";
                            }else{
                                if (t.stock == 1){
                                    tomado_stock++;
                                    comentario = "TOMADO DE STOCK";
                                }
                            }*/
                        }
                        //c.RichText.AddText(Regex.Replace(total_cantidad + "/" + total_total, @"\s+", " "));
                        /*if (total_total != 0){
                            // c.RichText.AddText("TOTAL"+total_total.ToString());
                        }*/
                        foreach (int i in lista_estados) { if (i == 1) { estado_1++; } if (i == 2) { estado_2++; } if (i == 3) { estado_3++; } }
                        if (estado_1 != 0) { ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 122); }
                        if (estado_3 != 0) { ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(174, 252, 174); }
                        if (estado_2 != 0) { ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 133, 133); }
                        if (estado_1 != 0 && estado_2 != 0 && estado_3 != 0) { ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }
                        //datos.Add(texto);
                        if (price_ticket != 0){
                            if (pt_impresos != 0){
                                ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(174, 252, 174);//VERDE
                            }else{
                                if (template_pt != 0 && pt_impresos == 0){
                                    ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(14, 96, 228);//azul
                                }else{
                                    if (template_pt != 0 && pt_impresos != 0){
                                        ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(174, 252, 174);//VERDE
                                    }else{
                                        if (template_pt == 0){
                                            ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 122);//AMARILLO
                                        }
                                    }
                                }
                            }
                        }
                        if (impresos_otros != 0){
                            ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(174, 252, 174);//VERDE
                        }
                        if (recibo != 0 || tomado_stock != 0) { ws.Cell(row, col).Comment.AddText(comentario); }
                        ws.Cell(row, col).Style.Alignment.WrapText = true;
                        ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        col++;
                    }//FAMILIAS
                    celdas.Add(datos.ToArray());
                    ws.Cell(row, 1).Value = celdas;
                    for (int i = 1; i <= total_familias; i++){
                        ws.Cell(row, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(row, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }
                    row++;
                }//PEDIDOS
                ws.SheetView.FreezeColumns(5);
                for (int cc = 1; cc < col; cc++){
                    var col2 = ws.Column(cc);
                    col2.Width = 20;
                }
                for (int cc = 3; cc < row; cc++){
                    var col2 = ws.Row(cc);
                    col2.Height = 100;
                }
                //ws.Rows().AdjustToContents();
                //ws.Columns().AdjustToContents();
                //ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                /***********D*O*W*N*L*O*A*D*************************************************************************************************************************************************************************/
                HttpResponse httpResponse = System.Web.HttpContext.Current.Response;
                httpResponse.Clear();
                httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                httpResponse.AddHeader("content-disposition", "attachment;filename=\"TRIM " + DateTime.Now.ToString("MMMM dd HH:mm:ss") + ".xlsx\"");
                // Flush the workbook to the Response.OutputStream
                using (MemoryStream memoryStream = new MemoryStream()){
                    libro_trabajo.SaveAs(memoryStream);
                    memoryStream.WriteTo(httpResponse.OutputStream);
                    memoryStream.Close();
                }
                httpResponse.End();

            }
        }
        /*COLORES*/
        /*******************************************************************************************************************************************************/
        /*******************************************************************************************************************************************************/
        /*PRICE TICKETS*/
        public JsonResult guardar_pedido_sesion(string pedido){
            Session["pedido_comparacion"] = pedido;
            return Json("", JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult UploadFiles(){
            // Checking no of files injected in Request object 
            string archivo = "";
            //if (Request.Files.Count > 0){
            // try{
            //  Get all files from Request object  
            HttpFileCollectionBase files = Request.Files;
            for (int i = 0; i < files.Count; i++){
                //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                //string filename = Path.GetFileName(Request.Files[i].FileName);  
                HttpPostedFileBase file = files[i];
                string fname;
                // Checking for Internet Explorer  
                if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER"){
                    string[] testfiles = file.FileName.Split(new char[] { '\\' });
                    fname = testfiles[testfiles.Length - 1];
                }else{
                    fname = file.FileName;
                }
                // Get the complete folder path and store the file inside it.  
                fname = Path.Combine("C:\\Uploads\\", fname);
                file.SaveAs(fname);
                archivo = fname;
            }
            Session["archivo_comparacion"] = archivo;
            // Returns message that successfully uploaded  
            /* if (System.IO.File.Exists(fullPath)){//PARA BORRAR
                 System.IO.File.Delete(fullPath);
             }*/
            //OBTENER INFORMACIÓN DEL PEDIDO
            List<registro_price_tickets> lista_sistema = dtrim.buscar_registro_price_ticket_pedido(Convert.ToInt32(Session["pedido_comparacion"]));
            //GUARDAR INFORMACIÓN EXCEL
            List<registro_price_tickets> lista_excel = obtener_registros_excel(archivo);
            //COMPARAR ¿¿??
            List<registro_price_tickets> lista_final = new List<registro_price_tickets>();
            foreach (registro_price_tickets e in lista_excel){
                int existe = 0;
                registro_price_tickets rpt = new registro_price_tickets();
                foreach (registro_price_tickets s in lista_sistema){
                    if (s.talla == e.talla && s.estilo == e.estilo){
                        rpt.estado = 0;
                        existe++;
                        if (e.total == s.total) { rpt.total = s.total; } else { rpt.total = e.total + "*" + s.total; }
                        if (e.estilo == s.estilo) { rpt.estilo = s.estilo; } else { rpt.estilo = e.estilo + "*" + s.estilo; }
                        if (e.upc == s.upc) { rpt.upc = s.upc; } else { rpt.upc = e.upc + "*" + s.upc; }
                        if (e.descripcion_estilo == s.descripcion_estilo) { rpt.descripcion_estilo = s.descripcion_estilo; } else { rpt.descripcion_estilo = e.descripcion_estilo + "*" + s.descripcion_estilo; }
                        if (e.color == s.color) { rpt.color = s.color; } else { rpt.color = e.color + "*" + s.color; }
                        if (e.talla == s.talla) { rpt.talla = s.talla; } else { rpt.talla = e.talla + "*" + s.talla; }
                    }
                }
                if (existe == 0){
                    rpt.estado = 1;
                    rpt.total = e.total;
                    rpt.estilo = e.estilo;
                    rpt.upc = e.upc;
                    rpt.descripcion_estilo = e.descripcion_estilo;
                    rpt.color = e.color;
                    rpt.talla = e.talla;

                    foreach (registro_price_tickets s in lista_sistema){
                        rpt.total += "*" + s.total;
                        rpt.estilo += "*" + s.estilo;
                        rpt.upc += "*" + s.upc;
                        rpt.descripcion_estilo += "*" + s.descripcion_estilo;
                        rpt.color += "*" + s.color;
                        rpt.talla += "*" + s.talla;
                    }
                }
                if (e.tickets != "") { rpt.tickets = e.tickets; } else { rpt.tickets = ""; }
                if (e.dept != "") { rpt.dept = e.dept; } else { rpt.dept = ""; }
                if (e.clas != "") { rpt.clas = e.clas; } else { rpt.clas = ""; }
                if (e.sub != "") { rpt.sub = e.sub; } else { rpt.sub = ""; }
                if (e.retail != "") { rpt.retail = e.retail; } else { rpt.retail = ""; }
                if (e.cl != "") { rpt.cl = e.cl; } else { rpt.cl = ""; }
                lista_final.Add(rpt);
            }
            if (System.IO.File.Exists(archivo)) { System.IO.File.Delete(archivo); }
            // return Json("File Uploaded Successfully! "+archivo);
            var result = Json(new{
                id_pedido = Convert.ToString(Session["pedido_comparacion"]),
                pedido = consultas.obtener_po_id(Convert.ToString(Session["pedido_comparacion"])),
                lista = lista_final,
            });
            return Json(result, JsonRequestBehavior.AllowGet);
            /* }
             catch (Exception ex){ return Json("Error occurred. Error details: " + ex.Message); }
         }else { return Json("No files selected."); }*/
        }
        public List<registro_price_tickets> obtener_registros_excel(string archivo){
            List<registro_price_tickets> lista = new List<registro_price_tickets>();
            using (XLWorkbook libro_trabajo = new XLWorkbook(archivo)){
                //Regex.Replace(pedido, @"\s+", " "); 
                var ws = libro_trabajo.Worksheet(1);
                var nonEmptyDataRows = libro_trabajo.Worksheet(1).RowsUsed();
                foreach (var dataRow in nonEmptyDataRows){
                    //for row number check
                    if (dataRow.RowNumber() > 1)
                    {//&& dataRow.RowNumber() <= 20
                        //to get column # 3's data
                        //var cell = dataRow.Cell(3).Value;
                        registro_price_tickets r = new registro_price_tickets();
                        r.estado = 0;
                        r.total = Convert.ToString(dataRow.Cell(1).Value).Trim();
                        r.estilo = (Convert.ToString(dataRow.Cell(2).Value)).Trim();
                        r.upc = (Convert.ToString(dataRow.Cell(3).Value)).Trim();
                        r.descripcion_estilo = (Convert.ToString(dataRow.Cell(4).Value)).Trim();
                        r.color = (Convert.ToString(dataRow.Cell(5).Value)).Trim();
                        r.talla = (Convert.ToString(dataRow.Cell(6).Value)).Trim();
                        /*************************************************************************************************/
                        r.tickets = (Convert.ToString(dataRow.Cell(7).Value)).Trim();
                        r.dept = (Convert.ToString(dataRow.Cell(8).Value)).Trim();
                        r.clas = (Convert.ToString(dataRow.Cell(9).Value)).Trim();
                        r.sub = (Convert.ToString(dataRow.Cell(10).Value)).Trim();
                        r.retail = (Convert.ToString(dataRow.Cell(11).Value)).Trim();
                        r.cl = (Convert.ToString(dataRow.Cell(12).Value)).Trim();
                        lista.Add(r);
                    }
                }
            }
            return lista;
        }
        public JsonResult guardar_price_tickets(string pedido, string total, string estilo, string upc, string descripcion, string color, string talla, string ticket, string dept, string clas, string sub, string retail, string cl, string impreso){
            string[] totales = total.Split('*'), estilos = estilo.Split('*'), upcs = upc.Split('*'), descripciones = descripcion.Split('*');
            string[] colores = color.Split('*'), tallas = talla.Split('*'), tickets = ticket.Split('*'), depts = dept.Split('*'), clases = clas.Split('*');
            string[] subs = sub.Split('*'), retails = retail.Split('*'), cls = cl.Split('*');
            for (int i = 1; i < totales.Length; i++){
                dtrim.guardar_price_tickets(pedido, totales[i], estilos[i], upcs[i], descripciones[i], colores[i], tallas[i], tickets[i], depts[i], clases[i], subs[i], retails[i], cls[i], Convert.ToString(Session["id_usuario"]), impreso);
            }
            if (impreso == "1"){
                dtrim.guardar_impresion("PRICE TICKETS", pedido, "0", "0");
                dtrim.marcar_price_tickets_pedido_impreso(Convert.ToInt32(pedido));
                int request = dtrim.buscar_request_pt_pedido(pedido);
                if (request != 0){
                    Trim_requests tr = dtrim.obtener_trim_request(request);
                    dtrim.marcar_pt_impreso(request, 1);
                    dtrim.cambiar_estado_trim_request(request, 3);
                    foreach (Trim_estilo te in tr.lista_estilos){
                        int summary = te.id_summary;
                        foreach (Talla_t t in te.lista_tallas){
                            dtrim.marcar_talla_impresa(t.id, 1);
                            /*int talla_pt = t.id_talla;
                            int total_pt = dtrim.buscar_total_talla(summary, talla_pt);
                            dtrim.actualizar_inventario(tr.id_inventario, total_pt.ToString());*/
                        }
                    }                    
                }
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        /////////////////////////////////////////////////////////////////////////////////////
        public JsonResult buscar_price_tickets(string pedido){
            return Json(dtrim.buscar_price_tickets_pedido(Convert.ToInt32(pedido)), JsonRequestBehavior.AllowGet);
        }
        public JsonResult cambiar_estado_pt(string datos, string estado){
            string[] data = datos.Split('*');//+ item.id_price_ticket +'*'+item.id_summary+ +'*'+item.id_talla+pedido
            dtrim.marcar_price_ticket_impreso(data[0]);
            //int pedido = consultas.obtener_id_pedido_summary(Convert.ToInt32(data[1]));
            dtrim.guardar_impresion("PRICE TICKETS", data[3], data[1], data[2]);
            int request = dtrim.buscar_request_pt_pedido(data[3]);
            Trim_requests tr = dtrim.obtener_trim_request(request);            
            if (request != 0){
                foreach (Trim_estilo te in tr.lista_estilos){
                    if (te.id_summary == Convert.ToInt32(data[1])){
                        foreach (Talla_t t in te.lista_tallas){
                            if (t.id_talla == Convert.ToInt32(data[2])){
                                dtrim.marcar_talla_impresa(t.id, Convert.ToInt32(estado));
                            }
                        }
                    }
                }
                tr = dtrim.obtener_trim_request(request);
                int no_impreso = 0;
                foreach (Trim_estilo te in tr.lista_estilos){
                    foreach (Talla_t t in te.lista_tallas){
                        if (t.impreso == 0){
                            no_impreso++;
                        }
                    }
                }
                if (no_impreso == Convert.ToInt32(estado)){
                    dtrim.marcar_pt_impreso(request, Convert.ToInt32(estado));
                    dtrim.cambiar_estado_trim_request(request, 3);
                }                
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult marcar_pt_orden_impresos(string pedido){
            // dtrim.marcar_price_tickets_pedido_impreso(Convert.ToInt32(pedido));
            dtrim.guardar_impresion("PRICE TICKETS", pedido, "0", "0");
            dtrim.marcar_price_ticket_orden_impreso(pedido);
            int request = dtrim.buscar_request_pt_pedido(pedido);
            Trim_requests tr = dtrim.obtener_trim_request(request);
            if (request != 0){
                dtrim.marcar_pt_impreso(request, 1);
                dtrim.cambiar_estado_trim_request(request, 3);
                foreach (Trim_estilo te in tr.lista_estilos){
                    int summary = te.id_summary;
                    foreach (Talla_t t in te.lista_tallas){
                        dtrim.marcar_talla_impresa(t.id, 1);
                        /*int talla_pt = t.id_talla;
                        int total_pt = dtrim.buscar_total_talla(summary, talla_pt);
                        dtrim.actualizar_inventario(tr.id_inventario, total_pt.ToString());*/
                    }
                }
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult eliminar_pt(string pt){
            dtrim.eliminar_price_ticket(pt);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        ////////////////////////////////////////////////////////////////////////////////////        
        public JsonResult buscar_ordenes_pt(){
            return Json(dtrim.obtener_lista_pedidos_pt(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult marcar_pt_impreso(string informacion){//EDITAR ESTOOOOO
            string[] datos = informacion.Split('*');
            int pedido;
            for (int i = 1; i < datos.Length; i++){
                //\'' + item.id_price_ticket + '*' + item.id_summary + '*' + item.id_talla + '+
                //d="' + t.id_request + '_' + t.id_inventario + '_' +lt.id_talla+'_'+ r.id_summary+
                string[] data = datos[i].Split('_');
                int id_estilo = consultas.obtener_estilo_summary(Convert.ToInt32(data[3]));
                string estilo = (consultas.obtener_estilo(id_estilo)).Trim();
                string talla = "";
                //pedido = consultas.obtener_id_pedido_summary(Convert.ToInt32(data[3]));
                //int request = dtrim.buscar_request_pt_pedido(pedido.ToString());
                int request = Convert.ToInt32(data[0]);
                if (request != 0){
                    Trim_requests tr = dtrim.obtener_trim_request(request);
                    pedido = tr.id_pedido;
                    //Trim_requests tr = dtrim.obtener_trim_request(Convert.ToInt32(data[0]));
                    int total = dtrim.buscar_total_talla(Convert.ToInt32(data[3]), Convert.ToInt32(data[2]));
                    foreach (Trim_estilo te in tr.lista_estilos){
                        if (te.id_summary == Convert.ToInt32(data[3])){
                            foreach (Talla_t t in te.lista_tallas){
                                if (t.id_talla == Convert.ToInt32(data[2])){
                                    dtrim.marcar_talla_impresa(t.id, 1);
                                    talla = t.talla;
                                }
                            }
                        }
                    }
                    //int pedido = consultas.obtener_id_pedido_summary(Convert.ToInt32(data[3]));
                    int pt = dtrim.buscar_pt_orden_estilo(pedido, estilo, talla);
                    dtrim.marcar_price_ticket_impreso((pt).ToString());
                    //dtrim.marcar_pt_impreso(Convert.ToInt32(data[0]), 1);
                    //dtrim.actualizar_inventario(Convert.ToInt32(data[1]), total.ToString());
                    tr = dtrim.obtener_trim_request(request);
                    int no_impreso = 0;
                    foreach (Trim_estilo te in tr.lista_estilos){
                        foreach (Talla_t t in te.lista_tallas){
                            if (t.impreso == 0){
                                no_impreso++;
                            }
                        }
                    }
                    if (no_impreso == 0){
                        dtrim.marcar_pt_impreso(request, 1);
                        //dtrim.actualizar_inventario(Convert.ToInt32(data[1]), total.ToString());
                        dtrim.cambiar_estado_trim_request(request, 3);
                        //dtrim.restar_cantidad_request(request, 0);

                    }
                }//request
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        /*PRICE TICKETS*/
        /*******************************************************************************************************************************************************/
        /***************************************************************************************************************************************************************/
        /*STICKERS */
        public JsonResult buscar_stickers(string pedido){
            return Json(dtrim.obtener_trims_stickers(Convert.ToInt32(pedido)), JsonRequestBehavior.AllowGet);
        }
        public JsonResult marcar_sticker_orden_impresos(string pedido){
            dtrim.guardar_impresion("0", pedido, "0", "0");
            List<Trim_requests> lista_tr = dtrim.obtener_trims_stickers(Convert.ToInt32(pedido));
            foreach (Trim_requests trim in lista_tr){
                foreach (Trim_estilo te in trim.lista_estilos){
                    foreach (Talla_t tt in te.lista_tallas){
                        dtrim.marcar_talla_impresa_sticker(tt.id, 1);
                    }
                }
                /*Trim_inventario inventario = dtrim.buscar_inventario_trim(trim.id_item, trim.id_pedido);
                if (inventario.id_inventario != 0){
                    dtrim.actualizar_inventario(inventario.id_inventario, trim.total.ToString());
                }*/
                dtrim.marcar_pt_impreso(trim.id_request, 1);
                dtrim.cambiar_estado_trim_request(trim.id_request, 3);
                //dtrim.restar_cantidad_request(trim.id_request, 0);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult cambiar_estado_stickers_individual(string talla, string request, string estado){
            //'talla':'" + datos[0] + "','request':'"+datos[1]+"','estado':'"+datos[2]+"'
            Trim_requests tr = dtrim.obtener_trim_request(Convert.ToInt32(request));
            dtrim.marcar_talla_impresa_sticker(Convert.ToInt32(talla), Convert.ToInt32(estado));
            int impreso = 0;
            foreach (Trim_estilo te in tr.lista_estilos){
                foreach (Talla_t tt in te.lista_tallas){
                    if (tt.impreso == 0){
                        impreso++;
                    }
                }
            }
            if (impreso == 0){
                /*Trim_inventario inventario = dtrim.buscar_inventario_trim(tr.id_item, tr.id_pedido);
                if (inventario.id_inventario == 0){
                    dtrim.actualizar_inventario(inventario.id_inventario, tr.total.ToString());
                }*/
                dtrim.marcar_pt_impreso(tr.id_request, 1);
            }else{
                dtrim.marcar_pt_impreso(tr.id_request, 0);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        /*STICKERS */
        /***************************************************************************************************************************************************************/
        /***************************************************************************************************************************************************************/
        /*TRIM CARD*/
        public ActionResult packing_instruction(){
            return View();
        }
        public JsonResult buscar_clientes_generos_familias(){
            var result = Json(new{
                clientes = dtrim.obtener_lista_clientes(),
                generos = dtrim.obtener_lista_generos(),
                familias = dtrim.obtener_familias(),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_trim_card(string busqueda){
            return Json(dtrim.obtener_lista_trim_cards(busqueda), JsonRequestBehavior.AllowGet);
        }
        public JsonResult eliminar_trim_card(string id){
            dtrim.eliminar_trim_card(id);
            dtrim.eliminar_trim_card_spec(id);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult upload_imagen_trim(){
            for (int i = 0; i < Request.Files.Count; i++){
                HttpPostedFileBase file = Request.Files[i]; //Uploaded file
                //Use the following properties to get file's name, size and MIMEType
                int fileSize = file.ContentLength;
                string fileName = file.FileName;
                string mimeType = file.ContentType;
                System.IO.Stream fileContent = file.InputStream;
                //To save file, use SaveAs method
                Session["imagen_nueva"] = fileName;
                file.SaveAs("C:\\Trims\\" + fileName); //File will be saved in application root
            }
            return Json("Uploaded " + Request.Files.Count + " files");
        }
        public JsonResult guardar_nueva_imagen(string familia, string cliente, string genero){
            string[] clientes = cliente.Split('*'), generos = genero.Split('*');
            string imagen = Convert.ToString(Session["imagen_nueva"]);
            dtrim.guardar_nueva_imagen(imagen, familia, Convert.ToInt32(Session["id_usuario"]));
            int id_imagen = dtrim.obtener_ultimo_id_imagen();
            for (int i = 1; i < clientes.Length; i++){
                dtrim.guardar_imagen_genero_cliente(id_imagen, clientes[i], generos[i]);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_imagenes_familia(string familia){
            return Json(dtrim.obtener_lista_imagenes_familia(Convert.ToInt32(familia)), JsonRequestBehavior.AllowGet);
        }
        public ActionResult ConvertirImagen(string id){
            string nombre = dtrim.buscar_imagen(Convert.ToInt32(id));
            return new FilePathResult("C:\\Trims\\" + nombre, System.Net.Mime.MediaTypeNames.Application.Octet);
        }
        public JsonResult buscar_genero_cliente_trim(string trim){
            Session["trim_edicion"] = trim;
            return Json(dtrim.obtener_lista_generos_clientes_imagen(Convert.ToInt32(trim)), JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_edicion_imagen(string trim, string cliente, string genero, string cambio, string imagen){
            string[] clientes = cliente.Split('*'), generos = genero.Split('*');
            if (cambio == "1"){
                dtrim.edicion_imagen_trim(trim, imagen, Convert.ToInt32(Session["id_usuario"]));
            }
            dtrim.borrar_genero_clientes_imagen(trim);
            for (int i = 1; i < clientes.Length; i++){
                dtrim.guardar_imagen_genero_cliente(Convert.ToInt32(trim), clientes[i], generos[i]);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult obtener_datos_pedido_tc(string pedido){
            List<estilo_shipping> e = ds.lista_estilos(pedido);
            List<int> lista_generos = (consultas.Lista_generos_po(Convert.ToInt32(pedido))).Distinct().ToList();
            clientes customer = new clientes();
            customer.id_customer = consultas.obtener_customer_final_po(Convert.ToInt32(pedido));
            customer.customer = consultas.obtener_customer_final_id(Convert.ToString(customer.id_customer));
            List<Familias_trim_card> lista_familias = dtrim.obtener_datos_familias_pedido(Convert.ToInt32(pedido), lista_generos, customer.id_customer);
            var result = Json(new{
                estilos = e,
                assorts = ds.lista_assortments_pedido(Convert.ToInt32(pedido)),
                familias_imagenes = lista_familias,
                cliente = customer,
                generos = lista_generos,
                cajas=dtrim.obtener_cajas_pedido(pedido),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_tc(string pedido, string customer, string tipo_empaque, string ratio, string familia, string imagen, string nota, string especial, string generos, string item,string comentarios,string estado){
            string[] familias = familia.Split('*'), imagenes = imagen.Split('*'), notas = nota.Split('*'), especiales = especial.Split('*'), items = item.Split('*');
            
            string pedido_po = (consultas.obtener_po_id(pedido)).Trim();
            if (pedido_po.Contains("HOT")|| pedido_po.Contains("URB")|| (pedido_po.Contains("JCP") && pedido_po.Contains("ECOM")) ) {
                ratio = "PIEZAS EN BREAKDOWN";
                tipo_empaque = "2";
            }
            dtrim.guardar_nuevo_trim_card(pedido, customer, tipo_empaque, ratio, Convert.ToInt32(Session["id_usuario"]), generos,comentarios,estado);
            int id_trim_card = dtrim.obtener_ultimo_trim_card();

            for (int i = 1; i < familias.Length; i++){
                dtrim.guardar_trim_card_familia(id_trim_card, imagenes[i], especiales[i], notas[i], familias[i], items[i]);
            }
            dtrim.agregar_trim_card_fold_size(pedido, id_trim_card);
            return Json(id_trim_card, JsonRequestBehavior.AllowGet);
        }
        public JsonResult session_tc(string id){
            Session["trim_card_id_print"] = id;
            return Json("", JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult imprimir_tc(){
            Dictionary<string, string> cookieCollection = new Dictionary<string, string>();
            foreach (var key in Request.Cookies.AllKeys){
                cookieCollection.Add(key, Request.Cookies.Get(key).Value);
            }
            return new ViewAsPdf("trim_card_nuevo", dtrim.obtener_trim_card_pdf(Convert.ToInt32(Session["trim_card_id_print"]))){
                FileName = "Trim Card #" + Convert.ToString(Session["trim_card_id_print"]) + ".pdf",
                Cookies = cookieCollection,
                PageOrientation = Rotativa.Options.Orientation.Landscape,
                PageSize = Rotativa.Options.Size.Letter,
                PageMargins = new Rotativa.Options.Margins(5, 5, 5, 5),
                CustomSwitches = "--page-offset 0 --footer-right [page]/[toPage] --footer-font-size 9 ",
                FormsAuthenticationCookieName = System.Web.Security.FormsAuthentication.FormsCookieName
            };
        }
        public JsonResult obtener_datos_edicion_trim_card(string trim_card){
            int pedido = dtrim.buscar_pedido_trim_card(Convert.ToInt32(trim_card));
            List<estilo_shipping> e = ds.lista_estilos(pedido.ToString());
            List<int> lista_generos = (consultas.Lista_generos_po(pedido)).Distinct().ToList();
            clientes customer = new clientes();
            customer.id_customer = consultas.obtener_customer_final_po(pedido);
            customer.customer = consultas.obtener_customer_final_id(Convert.ToString(customer.id_customer));
            List<Familias_trim_card> lista_familias = dtrim.obtener_datos_familias_pedido(pedido, lista_generos, customer.id_customer);
            var result = Json(new{
                estilos = e,
                assorts = ds.lista_assortments_pedido(pedido),
                familias_imagenes = lista_familias,
                cliente = customer,
                generos = lista_generos,
                trim_card = dtrim.obtener_trim_card(Convert.ToInt32(trim_card)),
                id_pedido = pedido,
                cajas = dtrim.obtener_cajas_pedido(pedido.ToString()),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_edicion_tc(string id_trim_card, string customer, string tipo_empaque, string ratio, string familia, string imagen, string nota, string especial, string generos, string item,string comentarios,string estado){
            string[] familias = familia.Split('*'), imagenes = imagen.Split('*'), notas = nota.Split('*'), especiales = especial.Split('*'), items = item.Split('*');
            int id_pedido = dtrim.obtener_pedido_trim_card(id_trim_card);
            string pedido_po = (consultas.obtener_po_id(id_pedido.ToString())).Trim();
            if (pedido_po.Contains("HOT") || pedido_po.Contains("URB") || (pedido_po.Contains("JCP") && pedido_po.Contains("ECOM"))){
                ratio = "PIEZAS EN BREAKDOWN";
                tipo_empaque = "2";
            }
            dtrim.editar_trim_card(Convert.ToInt32(id_trim_card), tipo_empaque, ratio,comentarios,estado);
            dtrim.borrar_trim_card_spec(Convert.ToInt32(id_trim_card));
            for (int i = 1; i < familias.Length; i++){
                dtrim.guardar_trim_card_familia(Convert.ToInt32(id_trim_card), imagenes[i], especiales[i], notas[i], familias[i], items[i]);
            }
            return Json(id_trim_card, JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_entrega_trim_card(string id,string entrega,string recibe,string fecha){
            dtrim.agregar_entrega_trim_card(id,entrega,recibe,fecha);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        /*TRIM CARD*/
        /***************************************************************************************************************************************************************/
        /***************************************************************************************************************************************************************/
        /*STOCK*/
        public JsonResult buscar_inventario_trims(string descripcion, string familia){
            List<Inventario> lista = new List<Inventario>();
            if (Convert.ToInt32(Session["id_usuario"]) == 89 || Convert.ToInt32(Session["id_usuario"]) == 90|| Convert.ToInt32(Session["id_usuario"]) == 91){
                if (descripcion == "0"  && familia == "0") { lista = dtrim.obtener_lista_trims_inventario_stock_bodega(); }
                if (descripcion == "0"  && familia != "0") { lista = dtrim.obtener_lista_trims_inventario_familia_bodega(familia); }               
                if (descripcion != "0"  && familia == "0") { lista = dtrim.obtener_lista_trims_inventario_descripcion_bodega(descripcion); }
            }else{
                if (descripcion == "0"  && familia == "0") { lista = dtrim.obtener_lista_trims_inventario_stock(); }
                if (descripcion == "0"  && familia != "0") { lista = dtrim.obtener_lista_trims_inventario_familia(familia); }
                if (descripcion != "0"  && familia == "0") { lista = dtrim.obtener_lista_trims_inventario_descripcion(descripcion); }
            }
            return Json(lista, JsonRequestBehavior.AllowGet);
        }
        public JsonResult editar_cantidades_inventario_trim(string inventario, string cantidad){
            dtrim.actualizar_cantidad_inventario(Convert.ToInt32(inventario), Convert.ToInt32(cantidad));
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_stock_orden(string pedido){
            int sucursal = Convert.ToInt32(consultas.obtener_sucursal_id_usuario(Convert.ToInt32(Session["id_usuario"])));
            int customer = consultas.obtener_customer_po(Convert.ToInt32(pedido));
            List<Trim_requests> lista_requests = dtrim.obtener_trims_anteriores_orden(pedido);
            List<Trim_requests> lista_stock = dtrim.obtener_stock_para_orden(pedido, lista_requests, customer, sucursal);
            var result = Json(new{
                stock = lista_stock,
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult obtener_stock(){
            List<Inventario> lista = new List<Inventario>();
            int sucursal = Convert.ToInt32(Session["id_sucursal"]), location;
            if (sucursal == 1){
                if (Convert.ToInt32(Session["id_usuario"]) == 89 || Convert.ToInt32(Session["id_usuario"]) == 90 || Convert.ToInt32(Session["id_usuario"]) == 91){
                    location = 72;
                }else{
                    if (Convert.ToInt32(Session["id_usuario"]) == 64){
                        location = 77;
                    }else{
                        location = 73;
                    }                    
                }
            }else{
                location = 72;
            }
            return Json(dtrim.obtener_trim_stock_sucursal_ubicacion(sucursal, location), JsonRequestBehavior.AllowGet);
        }
        public JsonResult cambiar_totales_stock(string inventario, string total){
            string[] inventarios = inventario.Split('*'), totales = total.Split('*');
            for (int i = 1; i < inventarios.Length; i++){
                dtrim.actualizar_cantidad_inventario(Convert.ToInt32(inventarios[i]), Convert.ToInt32(totales[i]));
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        //TRANSFERENCIA/////////////////////////////////////////////////////////////////////////////////////////////////////
        public JsonResult guardar_transferencia_trims(string inventario, string total, string sucursal, string location){
            string[] inventarios = inventario.Split('*'), totales = total.Split('*'), sucursales = sucursal.Split('*'), locaciones = location.Split('*');
            dtrim.guardar_transferencia_trim(Convert.ToInt32(Session["id_usuario"]));
            int transferencia = dtrim.obtener_ultima_transferencia();
            for (int x = 1; x < inventarios.Length; x++){
                Inventario i = dtrim.obtener_item_editar(Convert.ToInt32(inventarios[x]));
                di.id_usuario = Convert.ToInt32(Session["id_usuario"]);
                //di.id_sucursal = Convert.ToInt32(consultas.obtener_sucursal_id_usuario(Convert.ToInt32(Session["id_usuario"])));
                di.id_familia = dtrim.buscar_family_trim_inventario(Convert.ToInt32(inventarios[x]));
                di.id_item = i.id_item; //dtrim.buscar_id_item(Convert.ToInt32(inventarios[x]));
                consultas.buscar_informacion_trim_item((di.id_item).ToString());
                di.cantidad = Convert.ToInt32(totales[x]);
                di.id_pedido = 0;
                di.id_tipo = 2;
                di.id_trim = 0;
                di.id_unit = Convert.ToInt32(i.id_unit);
                di.id_customer = i.id_customer;
                di.id_trim = 0;
                di.descripcion = i.descripcion;
                di.total = Convert.ToInt32(totales[x]);
                di.minimo_trim = i.minimo;
                if (sucursales[x] == "2" || sucursales[x] == "3"){
                    di.id_ubicacion = 72;
                }else{
                    di.id_ubicacion = Convert.ToInt32(locaciones[x]);
                }
                di.id_sucursal = Convert.ToInt32(sucursales[x]);
                dtrim.guardar_transferencia_trim_item(transferencia, i.id_item, Convert.ToInt32(totales[x]), Convert.ToInt32(locaciones[x]), Convert.ToInt32(sucursales[x]),"0","0");
                int nuevo_inventario = di.buscar_existencia_trim_inventario();
                if (nuevo_inventario != 0){
                    di.update_inventario(nuevo_inventario, Convert.ToInt32(totales[x]));
                }else{
                    di.guardar_trim_po();
                }
                dtrim.actualizar_inventario(Convert.ToInt32(inventarios[x]), totales[x]);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_transferencias(string inicio, string final){
            return Json(dtrim.obtener_transferencias(inicio, final), JsonRequestBehavior.AllowGet);
        }
        public JsonResult sesiones_lista_stock(string sucursal, string location){
            Session["sucursal_lista_stock"] = sucursal;
            Session["locacion_lista_stock"] = location;
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public void lista_stock(){
            //string year = Convert.ToString(Session["year_reporte"]);
            List<Inventario> lista = dtrim.lista_reporte_stock(Convert.ToInt32(Session["sucursal_lista_stock"]), Convert.ToInt32(Session["locacion_lista_stock"]));
            int row = 1;
            using (XLWorkbook libro_trabajo = new XLWorkbook()){ //Regex.Replace(pedido, @"\s+", " "); 
                var ws = libro_trabajo.Worksheets.Add("STOCK");
                //CABECERAS TABLA
                var headers = new List<String[]>();
                List<String> titulos = new List<string>();
                titulos.Add("AMT"); titulos.Add("ITEM"); titulos.Add("TOTAL"); titulos.Add("FAMILY");
                headers.Add(titulos.ToArray());
                ws.Cell(row, 1).Value = headers;
                ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.FromArgb(196, 215, 155);
                ws.Range(row, 1, row, 4).Style.Font.FontSize = 13;
                ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                ws.Range(row, 1, row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                row++; //AGREGAR CABECERA TABLA
                foreach (Inventario i in lista){
                    ws.Cell(row, 1).Value = "'" + i.item;
                    ws.Cell(row, 2).Value = i.descripcion;
                    ws.Cell(row, 3).Value = "'" + i.total;
                    ws.Cell(row, 4).Value = i.family_trim;
                    ws.Range(row, 1, row, 4).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 4).Style.Border.LeftBorderColor = XLColor.FromArgb(178, 178, 178);
                    ws.Range(row, 1, row, 4).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 4).Style.Border.RightBorderColor = XLColor.FromArgb(178, 178, 178);
                    ws.Range(row, 1, row, 4).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 4).Style.Border.TopBorderColor = XLColor.FromArgb(178, 178, 178);
                    ws.Range(row, 1, row, 4).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 4).Style.Border.BottomBorderColor = XLColor.FromArgb(178, 178, 178);
                    ws.Range(row, 1, row, 4).Style.Font.FontSize = 13;
                    ws.Range(row, 1, row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    row++;
                }
                ws.Rows().AdjustToContents();
                ws.Columns().AdjustToContents();
                /***********D*O*W*N*L*O*A*D*************************************************************************************************************************************************************************/
                HttpResponse httpResponse = System.Web.HttpContext.Current.Response;
                httpResponse.Clear();
                httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                httpResponse.AddHeader("content-disposition", "attachment;filename=\"Inventory-" + DateTime.Now.ToString("MMMM dd HH:mm:ss") + ".xlsx\"");
                // Flush the workbook to the Response.OutputStream
                using (MemoryStream memoryStream = new MemoryStream()){
                    libro_trabajo.SaveAs(memoryStream);
                    memoryStream.WriteTo(httpResponse.OutputStream);
                    memoryStream.Close();
                }
                httpResponse.End();
            }
        }
        /*STOCK*/
        /***************************************************************************************************************************************************************/
        /***************************************************************************************************************************************************************/
        /*TSO*/
        public JsonResult guardar_transferencia_stock_orden(string inventario, string total,string orden){
            string[] inventarios = inventario.Split('*'),totales=total.Split('*'),ordenes=orden.Split('*');
            dtrim.guardar_transferencia_trim(Convert.ToInt32(Session["id_usuario"]));
            int transferencia = dtrim.obtener_ultima_transferencia();
            for (int i=1; i<inventarios.Length;i++) {
                int item = dtrim.buscar_item_inventario(Convert.ToInt32(inventarios[i]));
                dtrim.actualizar_inventario(Convert.ToInt32(inventarios[i]), totales[i]);
                dtrim.guardar_stock_inventario(ordenes[i],Convert.ToString(item),totales[i]);
                dtrim.guardar_transferencia_trim_item(transferencia, item, Convert.ToInt32(totales[i]), 0, Convert.ToInt32(Session["id_sucursal"]), ordenes[i],"1");
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        /*TSO*/
        /***************************************************************************************************************************************************************/
        /***************************************************************************************************************************************************************/
        /*TOS*/
        public JsonResult obtener_stock_orden(string orden){
            return Json(dtrim.obtener_stock_orden(orden), JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_transferencia_orden_stock(string item, string orden,string total,string sucursal,string locacion){            
            string[] items = item.Split('*'),totales=total.Split('*'),ordenes=orden.Split('*'),sucursales=sucursal.Split('*'),locaciones=locacion.Split('*');
            dtrim.guardar_transferencia_trim(Convert.ToInt32(Session["id_usuario"]));
            int transferencia = dtrim.obtener_ultima_transferencia();
            for (int i=1; i<items.Length;i++) {
                consultas.buscar_informacion_trim_item(items[i]);
                int customer = consultas.obtener_customer_po(Convert.ToInt32(ordenes[i]));                
                di.id_usuario = Convert.ToInt32(Session["id_usuario"]);                
                di.id_familia = dtrim.obtener_familias_item_id(Convert.ToInt32(items[i]));
                //di.id_familia = consultas.buscar_familia_trim(dtrim.obtener_family_trim_item(items[i]));
                di.id_item = Convert.ToInt32(items[i]); //dtrim.buscar_id_item(Convert.ToInt32(inventarios[x]));
                di.cantidad = Convert.ToInt32(totales[i]);
                di.id_pedido = 0;
                di.id_tipo = consultas.buscar_tipo_inventario("Trims");
                di.id_unit = consultas.buscar_unit(consultas.unit);
                di.id_trim = 0;
                di.id_customer = customer;
                di.id_trim = 0;
                di.descripcion = dtrim.obtener_descripcion_item(Convert.ToInt32(items[i])); 
                di.total = Convert.ToInt32(totales[i]);
                di.minimo_trim = 0;
                di.id_ubicacion= Convert.ToInt32(locaciones[i]);
                di.id_sucursal = Convert.ToInt32(sucursales[i]);                
                int nuevo_inventario = di.buscar_existencia_trim_inventario();
                if (nuevo_inventario != 0){
                    di.update_inventario(nuevo_inventario, Convert.ToInt32(totales[i]));
                }else{
                    di.guardar_trim_po();
                }
                dtrim.restar_stock_orden(ordenes[i], items[i], totales[i]);
                dtrim.guardar_transferencia_trim_item(transferencia, Convert.ToInt32(items[i]), Convert.ToInt32(totales[i]), 0, Convert.ToInt32(Session["id_sucursal"]), ordenes[i], "2");
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        /*TOS*/
        /***************************************************************************************************************************************************************/
        /***************************************************************************************************************************************************************/
        /*AUDITORIA*/
        public ActionResult Autocomplete_ubicacion(string term){
            var items = consultas.Lista_ubicaciones();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_trims_orden(string orden){
            var result = Json(new{
                trims_request = dtrim.obtener_trims_anteriores_orden(orden),
                trims_orden = dtrim.obtener_trims_auditados_orden(orden),
            });
            return Json(result, JsonRequestBehavior.AllowGet); 
        }
        public JsonResult buscar_pos_ht(string orden){
            return Json(dtrim.obtener_po_ht(orden), JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_auditoria_trims_orden(string orden,string item,string estilo,string talla,string total,string sucursal,string locacion,string anterior,string po){
            string[] items = item.Split('*'),estilos=estilo.Split('*'),tallas=talla.Split('*'),totales=total.Split('*'),sucursales=sucursal.Split('*');
            string[] locaciones = locacion.Split('*'), anteriores = anterior.Split('*'),pos=po.Split('*');
            List<int> nuevos = new List<int>();
            List<int> viejos = new List<int>();
            List<Trim_orden> trims_orden = dtrim.obtener_trims_auditados_orden(orden);
            foreach (Trim_orden t in trims_orden) { viejos.Add(t.id_trim_order); }
            for (int i = 1; i < items.Length; i++) { if (anteriores[i] != "0") { nuevos.Add(Convert.ToInt32(anteriores[i])); } }
            viejos= viejos.Except(nuevos).ToList();
            foreach (int i in viejos) { dtrim.eliminar_trim_auditado(i.ToString()); }
            for (int i=1; i<items.Length;i++) {
                int id_ubicacion = consultas.buscar_ubicacion(locaciones[i]);
                if (id_ubicacion == 0){
                    consultas.crear_ubicacion(locaciones[i]);
                    id_ubicacion = consultas.buscar_ubicacion(locaciones[i]);
                }
                string[] item_request = items[i].Split('_');// i.id_request + '_'+i.id_item
                if (anteriores[i] == "0"){                    
                    dtrim.insertar_trim_orden(item_request[1], orden, estilos[i], tallas[i], sucursales[i], id_ubicacion.ToString(), Convert.ToString(Session["id_usuario"]), totales[i], DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), item_request[0],pos[i]);
                }else {
                    Trim_orden to = dtrim.obtener_trim_auditado_orden(anteriores[i]);
                    dtrim.eliminar_trim_auditado(anteriores[i]);
                    dtrim.insertar_trim_orden(item_request[1], orden, estilos[i], tallas[i], sucursales[i], id_ubicacion.ToString(), (to.id_usuario).ToString(), totales[i], to.fecha_completa, item_request[0], pos[i]);
                }
            }
            return Json("", JsonRequestBehavior.AllowGet); 
        }
        public JsonResult buscar_pedidos_dos_semanas(){
            return Json(dtrim.lista_ordenes_dos_semanas(), JsonRequestBehavior.AllowGet);
        }
        /*AUDITORIA*/
        /***************************************************************************************************************************************************************/
        /***************************************************************************************************************************************************************/
        /*ENTREGAS*/
        public JsonResult buscar_trims_pedido(string pedido){
            List<Trim_orden> lista = new List<Trim_orden>();
            int usuario_trim = Convert.ToInt32(Session["id_usuario"]);
            if (usuario_trim == 89 || usuario_trim == 90 || usuario_trim == 91 ){
                lista = dtrim.obtener_trims_entrega_pedido_almacen(pedido);
            }else{
                lista = dtrim.obtener_trims_entrega_pedido(pedido);
            }
            var result = Json(new { trims = lista, });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_inventario_stock_entrega(){
            List<Inventario> lista = new List<Inventario>();
            if (Convert.ToInt32(Session["id_usuario"]) == 89 || Convert.ToInt32(Session["id_usuario"]) == 90 || Convert.ToInt32(Session["id_usuario"]) == 91){
                lista = dtrim.obtener_stock_entrega_bodega(Convert.ToInt32(Session["id_sucursal"]));
            }else{
                if (Convert.ToInt32(Session["id_usuario"]) == 64){
                    lista = dtrim.obtener_stock_entrega_impresiones(Convert.ToInt32(Session["id_sucursal"]));
                }else{
                    lista = dtrim.obtener_stock_entrega(Convert.ToInt32(Session["id_sucursal"]));
                }
            }
            return Json(lista, JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_entrega(string orden, string entrega, string recibe, string total, string comentario,  string inventario, string pedido,string trim_orden,string request){
            string[] totales = total.Split('*'), comentarios = comentario.Split('*'), inventarios = inventario.Split('*'), pedidos = pedido.Split('*'),trims_ordenes=trim_orden.Split('*');
            string[] requests = request.Split('*');
            dtrim.guardar_entrega(entrega, recibe, orden);
            int id_entrega = dtrim.obtener_ultima_entrega();
            for (int i = 1; i < requests.Length; i++) {
                dtrim.guardar_entrega_item(id_entrega, trims_ordenes[i], requests[i], inventarios[i], totales[i], comentarios[i]);
                if (inventarios[i]!="0") {
                    dtrim.actualizar_inventario(Convert.ToInt32(inventarios[i]), totales[i]);
                }
                if (requests[i] != "0") {
                    Trim_requests tr = dtrim.obtener_trim_request(Convert.ToInt32(requests[i]));
                    if (Convert.ToInt32(Session["id_sucursal"]) != 1){
                        dtrim.actualizar_request_inventario(tr.id_item,totales[i], Convert.ToString(Session["id_sucursal"]),72);
                    }else{
                        if (Convert.ToInt32(Session["id_usuario"]) == 89 || Convert.ToInt32(Session["id_usuario"]) == 90 || Convert.ToInt32(Session["id_usuario"]) == 91){
                            dtrim.actualizar_request_inventario(tr.id_item, totales[i], Convert.ToString(Session["id_sucursal"]), 72);
                        }else{
                            dtrim.actualizar_request_inventario(tr.id_item, totales[i], Convert.ToString(Session["id_sucursal"]), 73);
                        }
                    }
                }
                if (trims_ordenes[i] != "0"){
                    Trim_orden to = dtrim.obtener_trim_auditado_orden(trims_ordenes[i]);
                    dtrim.restar_stock_orden(orden, Convert.ToString(to.id_item), totales[i]); 
                }
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        /*********************************************************************************/
        public JsonResult buscar_entregas_orden(string orden){
            return Json(dtrim.obtener_entregas_orden(orden), JsonRequestBehavior.AllowGet);
        }
        public JsonResult obtener_entregas_fechas(string inicio,string final){
            return Json(dtrim.obtener_entregas_fechas(inicio,final), JsonRequestBehavior.AllowGet);
        }
        public JsonResult regresar_item_entregado(string entrega, string total_original, string total_nuevo){
            DatosShipping ds = new DatosShipping();//TOTAL_NUEVO
            Trim_entregas_items e = dtrim.obtener_entrega_item(Convert.ToInt32(entrega));
            regresar_trim_entrega(e,total_nuevo);
            if (total_original == total_nuevo){
                dtrim.eliminar_item_entrega(entrega);
                int cont = dtrim.contar_items_entrega((e.id_entrega).ToString());
                if (cont == 0){
                    dtrim.eliminar_entrega((e.id_entrega).ToString());
                }
            }else{
                dtrim.restar_item_entrega(entrega, Convert.ToInt32(total_nuevo));
            }            
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult eliminar_entrega_individual(string entrega){
            Trim_entregas_items e = dtrim.obtener_entrega_item(Convert.ToInt32(entrega));
            regresar_trim_entrega(e, (e.total).ToString());
            dtrim.eliminar_item_entrega(entrega);
            int cont = dtrim.contar_items_entrega((e.id_entrega).ToString());
            if (cont == 0){
                dtrim.eliminar_entrega((e.id_entrega).ToString());
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public void regresar_trim_entrega(Trim_entregas_items e,string total_nuevo) {
            if (e.id_trim_request != 0){//REQUEST                   
                Trim_requests tr = dtrim.obtener_trim_request(e.id_trim_request);
                if (Convert.ToInt32(Session["id_sucursal"]) != 1){
                    dtrim.sumar_request_inventario(tr.id_item, total_nuevo, Convert.ToString(Session["id_sucursal"]), 72);
                }else{
                    if (Convert.ToInt32(Session["id_usuario"]) == 89 || Convert.ToInt32(Session["id_usuario"]) == 90|| Convert.ToInt32(Session["id_usuario"]) == 91){
                        dtrim.sumar_request_inventario(tr.id_item, total_nuevo, Convert.ToString(Session["id_sucursal"]), 72);
                    }else{
                        dtrim.sumar_request_inventario(tr.id_item, total_nuevo, Convert.ToString(Session["id_sucursal"]), 73);
                    }
                }
            }else{
                if (e.id_trim_orden != 0){//ORDEN
                    Trim_orden to = dtrim.obtener_trim_auditado_orden(Convert.ToString(e.id_trim_orden));
                    int id_pedido = consultas.obtener_id_pedido_summary(to.id_summary);
                    dtrim.sumar_stock_orden(Convert.ToString(to.id_pedido), Convert.ToString(to.id_item), total_nuevo);
                }else{//INVENTARIO
                    dtrim.sumar_inventario(e.id_inventario, total_nuevo);
                }
            }
        }
        public JsonResult eliminar_entrega_completo(string entrega){
            Trim_entregas e = dtrim.obtener_entrega(entrega);
            foreach (Trim_entregas_items i in e.lista_entregas){
                regresar_trim_entrega(i, (i.total).ToString());
                dtrim.eliminar_item_entrega(entrega);
            }
            dtrim.eliminar_entrega(entrega);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        /*ENTREGAS*/
        /******************************************************************************************************************************************************************************************************/
        /******************************************************************************************************************************************************************************************************/
        /******************************************************************************************************************************************************************************************************/
        public JsonResult buscar_especifico_ordenes_tabla_inicio(string busqueda){
            return Json(dtrim.obtener_lista_ordenes_estados(busqueda), JsonRequestBehavior.AllowGet);
        }
        //FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
        public ActionResult auditoria(){
            return View();
        }
        public JsonResult obtener_estilos_orden(string pedido){
            List<estilo_shipping> e = dtrim.lista_estilos(pedido);
            var result = Json(new{               
                estilos = e,
                tallas = ds.obtener_lista_tallas_pedido(e),
                trims_request = dtrim.obtener_trims_anteriores_orden(pedido), //ESTE JALA LOS TRIMS DE REQUEST
                trims_orden = dtrim.obtener_trims_auditados_orden(pedido),
                locaciones = dtrim.buscar_locaciones_orden(pedido),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_auditoria(string pedido,string dato,string locacion,string comentario_locacion){            
            string[] locaciones = locacion.Split('*'),comentarios_locacion=comentario_locacion.Split('*');
            List<Trim_orden> trims_orden = dtrim.obtener_trims_auditados_orden(pedido);
            foreach (Trim_orden t in trims_orden) {
                dtrim.eliminar_trim_auditado(t.id_trim_order.ToString());
            }
            dtrim.eliminar_locacion_trim_pedido(pedido);
            string[] datos = dato.Split('*');
            for (int i=1;i<datos.Length;i++){
                //[ 0-total | 1-trim_orden | 2-item | 3-estilo | 4-talla ]
                string[] info = datos[i].Split('_');
                dtrim.insertar_trim_orden(info[2], pedido, info[3], info[4], "1", "0", Convert.ToString(Session["id_usuario"]), info[0], DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "0", "0");
                //**********************************************************
                if ( info[3]=="0" && info[4] == "0") {
                    int total = dtrim.obtener_total_trim_recibido_pedido(Convert.ToInt32(pedido), Convert.ToInt32(info[2]));
                    int total_nuevo = Convert.ToInt32(info[0]);
                    int customer = consultas.obtener_customer_po(Convert.ToInt32(pedido));
                    int inventario = dtrim.buscar_inventario_auditoria(Convert.ToInt32(info[2]), customer,Convert.ToInt32(Session["id_sucursal"]) );

                    if ((total_nuevo - total) >= 1){
                        dtrim.actualizar_inventario(inventario, Convert.ToString(total_nuevo - total));
                        dtrim.guardar_stock_inventario(pedido, info[2], Convert.ToString(total_nuevo - total));
                    }
                }
                //**********************************************************
            }
            for (int i = 1; i < locaciones.Length; i++){
                int id_ubicacion = consultas.buscar_ubicacion(locaciones[i]);
                if (id_ubicacion == 0){
                    consultas.crear_ubicacion(locaciones[i]);
                    id_ubicacion = consultas.buscar_ubicacion(locaciones[i]);
                }
                dtrim.insertar_locacion_pedido(id_ubicacion.ToString(),pedido, comentarios_locacion[i]);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        /*ENTREGAS*/
        public ActionResult entrega(){
            return View();
        }
        public JsonResult guardar_entrega_pedido(string pedido, string dato, string entrega, string recibe,string comentarios){
            string[] datos = dato.Split('*');
            dtrim.guardar_entrega_pedido(entrega, recibe, pedido,comentarios);
            int id_entrega = dtrim.obtener_ultima_entrega();
            //[0-total | 1-trim_orden | 2-item | 3-estilo | 4-talla]
            for (int i = 1; i < datos.Length; i++){
                string[] info = datos[i].Split('_');
                dtrim.guardar_entrega_item(id_entrega, info[1],"0", "0", info[0], "N/A");
                Trim_orden to = dtrim.obtener_trim_auditado_orden(info[1]);
                dtrim.restar_stock_orden(pedido, Convert.ToString(to.id_item), info[0]);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        /******************************************************************************************************************************************************************************************************/
        /******************************************************************************************************************************************************************************************************/
        public JsonResult obtener_estado_total_pedido(string pedido){
            var result = Json(new{
                pedido = dtrim.buscar_estado_total_pedido(Convert.ToInt32(pedido)),
                estado = dtrim.buscar_estado_trim_pedido(Convert.ToInt32(pedido)),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /******************************************************************************************************************************************************************************************************/
        /******************************************************************************************************************************************************************************************************/
        public JsonResult guardar_cambios_stock_orden(string pedido, string inventario, string request, string item, string total){
            string[] inventarios = inventario.Split('*'), requests = request.Split('*'), items = item.Split('*'), totales = total.Split('*');
            for (int x = 1; x < inventarios.Length; x++){
                dtrim.actualizar_inventario(Convert.ToInt32(inventarios[x]), totales[x]);
                dtrim.guardar_stock_inventario(pedido,items[x],totales[x]);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        /******************************************************************************************************************************************************************************************************/
        /******************************************************************************************************************************************************************************************************/
        //FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();


































































    }

    internal class PrintDialog
    {
        public PrinterSettings PrinterSettings { get; internal set; }
    }
}