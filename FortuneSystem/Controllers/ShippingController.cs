using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FortuneSystem.Models.Almacen;
using FortuneSystem.Models.Trims;
using FortuneSystem.Models.Shipping;
using FortuneSystem.Models.Fantasy;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Rotativa;
using ZXing;
using OfficeOpenXml;
using System.Data;
using ClosedXML.Excel;
using ZXing.Common;
using ZXing.QrCode;
using System.Text.RegularExpressions;

namespace FortuneSystem.Controllers
{
    public class ShippingController : Controller
    {
        DatosInventario di = new DatosInventario();
        DatosShipping ds = new DatosShipping();        

        public ActionResult Index(){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            //Session["id_usuario"] = 2;
            //Session["id_usuario"] = consultas.buscar_id_usuario(Convert.ToString(Session["usuario"]));
            /*if (Session["usuario"] == null){
                return View();
            }else {
                return View();
            }*/
            int id_usuario = Convert.ToInt32(Session["id_Empleado"]);
            Session["id_usuario"] = id_usuario;
            ViewBag.usuario = id_usuario;
            int departamento = consultas.obtener_departamento_id_usuario(id_usuario);
            ViewBag.departamento = departamento;
            //Session["id_usuario"] = 2;
            return View();
        }

        public ActionResult new_pk(){ return View(); }
        public ActionResult new_packing_list(){ return View(); }
        public ActionResult new_packing_list_fantasy(){ return View(); }
        public ActionResult new_packing_list_samples(){ return View(); }
        public ActionResult new_packing_list_returns(){ return View(); }
        public ActionResult new_packing_list_fantasy_extras(){ return View(); }
        public ActionResult new_packing_list_directo_fantasy(){ return View(); }

        public JsonResult buscar_pedidos_inicio(string busqueda)
        {
            return Json(ds.lista_ordenes_mde(busqueda), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_estilos_pedido(string id_pedido)
        {
            Session["id_pedido"] = id_pedido;
            return Json(ds.lista_estilos(id_pedido), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Autocomplete_tallas(string term){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            var items = consultas.Lista_tallas();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_dcs(string term){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            var items = consultas.Lista_dcs();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_recibo_inventario_shipping(string estilos, string colores, string tallas, string piezas, string cajas){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string[] Estilos = estilos.Split('*'), Colores = colores.Split('*'), Tallas = tallas.Split('*'), Piezas = piezas.Split('*'), Cajas = cajas.Split('*');
            int total_piezas = 0, id_talla, id_color, existencia, total_cajas = 0, id_recibo, id_inventario;
            string descripcion, estilo;
            for (int i = 1; i < Estilos.Length; i++){
                total_piezas += Convert.ToInt32(Piezas[i]);
                total_cajas += Convert.ToInt32(Cajas[i]);
            }
            ds.guardar_recibo_fantasy(Convert.ToInt32(Session["id_pedido"]), Convert.ToInt32(Session["id_usuario"]), total_piezas, total_cajas);
            id_recibo = ds.obtener_ultimo_recibo();
            for (int i = 1; i < Estilos.Length; i++){
                id_talla = consultas.buscar_talla(Tallas[i]);
                id_color = consultas.buscar_color_codigo(Colores[i]);
                estilo = consultas.obtener_estilo(Convert.ToInt32(Estilos[i])) + " " + consultas.buscar_descripcion_estilo(Convert.ToInt32(Estilos[i]));
                descripcion = estilo + " " + Colores[i] + " " + Tallas[i];
                descripcion = Regex.Replace(descripcion, @"\s+", " ");
                existencia = ds.buscar_existencia_inventario(id_color, id_talla, Estilos[i]);
                if (existencia == 0){
                    ds.guardar_item_inventario(id_color, id_talla, Estilos[i], descripcion, Convert.ToInt32(Cajas[i]) * Convert.ToInt32(Piezas[i]));
                    id_inventario = ds.obtener_ultimo_item();
                }else{
                    id_inventario = existencia;
                    ds.aumentar_inventario(existencia, Convert.ToInt32(Cajas[i]), Convert.ToInt32(Piezas[i]));
                }
                ds.guardar_recibo_fantasy_item(id_recibo, id_inventario, Cajas[i], Piezas[i]);
            }
            return Json("0", JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_po(string term){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            var items = consultas.Lista_po_abiertos();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_estilos(string term){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            var items = consultas.Lista_styles();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_po_abiertos(string term)
        {
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            var items = consultas.Lista_po_abiertos();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public JsonResult obtener_estilos_dc(string po)
        {
            Session["po"] = po;
            return Json(ds.buscar_estilos_po(po), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_conductores()
        {
            return Json(ds.obtener_drivers(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_carriers()
        {
            return Json(ds.obtener_carriers(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult enviar_informacion_driver(string carrier, string nombre, string plates, string scac, string caat, string tractor)
        {
            ds.guardar_nuevo_conductor(carrier, nombre, plates, scac, caat, tractor);
            return Json(ds.obtener_carriers(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_informacion_driver(string id)
        {
            Session["id_driver_edit"] = id;
            return Json(ds.obtener_conductor_edicion(id), JsonRequestBehavior.AllowGet);
        }
        public JsonResult enviar_informacion_driver_edicion(string id, string carrier, string nombre, string plates, string scac, string caat, string tractor)
        {
            ds.guardar_conductor_edicion(id, carrier, nombre, plates, scac, caat, tractor);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_direcciones_envio()
        {
            return Json(ds.obtener_direcciones(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult enviar_informacion_direccion(string nombre, string direccion, string ciudad, string zip)
        {
            ds.guardar_nueva_direccion(nombre, direccion, ciudad, zip);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_modificar_edicion(string id)
        {
            return Json(ds.obtener_direccion_edicion(id), JsonRequestBehavior.AllowGet);
        }
        public JsonResult enviar_informacion_direccion_edicion(string id, string nombre, string direccion, string ciudad, string zip)
        {
            ds.guardar_direccion_edicion(id, nombre, direccion, ciudad, zip);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult eliminar_conductor(string id)
        {
            ds.borrar_conductor(id);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult eliminar_direccion(string id)
        {
            ds.borrar_direccion(id);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_contenedores_select()
        {
            return Json(ds.obtener_contenedores_select(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_conductores_select()
        {
            return Json(ds.obtener_conductores_select(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_direcciones_select()
        {
            return Json(ds.obtener_direcciones_select(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_estilos_pk(string pedido){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            int id_pedido = consultas.buscar_pedido(pedido);
            return Json(ds.obtener_lista_tarimas_estilos(id_pedido), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_pedidos(){
            return Json(ds.obtener_lista_po_shipping(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_pedidos_fantasy()
        {
            return Json(ds.obtener_lista_po_shipping(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_clientes(){
            DatosTrim dtrim = new DatosTrim();
            return Json(dtrim.lista_clientes(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_clientes_final(){
            DatosTrim dtrim = new DatosTrim();
            return Json(dtrim.obtener_lista_clientes(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult aumentar_contador_packing(){
            ds.incrementar_contador_packing_normal();
            return Json(ds.buscar_contador_packing_normal_configuracion(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult resetear_contadores_packing(){
            ds.reset_configuracion();
            ds.reset_year_configuracion();
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult guardar_pk(string pedido,string address, string driver, string container, string seal, string replacement, string tipo, string labels, string type_labels, string num_envio,string tipo_packing,string sin_partes,string pk_id)
        {//                                               address   + "','driver': + "','container': + "','seal'l + "','replacement':'" +   tipo':'" + ,'labels':'" "','type_labels':'+ "','num_envio':'" +,'tipo_packing':'7'}",
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            int usuario = Convert.ToInt32(Session["id_usuario"]);
            string[] pk_ant;
            string pk_nombre;
            //string pk_anterior = ds.obtener_ultimo_pk();
            int indice_pk = 0, ultimo_pk, id_customer, id_customer_po, id_pedido, year_pk;
            string parte ="";
            /*if (pk_anterior != ""){
                pk_ant = pk_anterior.Split('-');
                year_pk = pk_ant[1].Split(' ');
                if (year_pk[0] == (DateTime.Now.Year.ToString())){
                    indice_pk = (Convert.ToInt32(pk_ant[0]) + 1);
                }else { indice_pk = 1; }
            }else{
                indice_pk = 1;
            }   */
            if (pk_id == "0"){
                if (tipo_packing == "9") { indice_pk = ds.buscar_contador_packing_ext_configuracion(); } else { indice_pk = ds.buscar_contador_packing_normal_configuracion(); }
                indice_pk++;
                year_pk = ds.buscar_year_packing_configuracion();
                /*int current_year = DateTime.Now.Year;
                if (year_pk != current_year){
                    ds.reset_configuracion();
                    indice_pk = 1;
                    year_pk = current_year;
                } */               
                if (tipo_packing == "9"){
                    ds.incrementar_contador_packing_ext();
                    pk_nombre = ((Convert.ToString(indice_pk).PadLeft(4, '0')) + "-" + year_pk.ToString() + " (EXT-DMG) FFB");
                }else{
                    ds.incrementar_contador_packing_normal();
                    pk_nombre = ((Convert.ToString(indice_pk).PadLeft(4, '0')) + "-" + year_pk.ToString() + " FFB");
                }
            }else {
                pk_nombre = "NOT ASIGNED";
            }
            //2nd Part and final shipment
            id_pedido = Convert.ToInt32(pedido);
            id_customer = consultas.obtener_customer_po(id_pedido);
            if (tipo_packing == "0") {
                tipo_packing = Convert.ToString(id_customer);
            }
            id_customer_po = consultas.obtener_customer_final_po(id_pedido);
            if (id_customer_po == 27) { tipo_packing = "7"; }
            //PARTE 
            if (tipo_packing != "9" && sin_partes!="1"){
                if (tipo_packing != "3" && tipo_packing != "5"){
                    parte = ds.buscar_parte_packing(id_pedido);
                }else{
                    if (tipo_packing == "3"){
                        if (id_customer_po == 65 || id_customer_po == 66 || id_customer_po == 67){
                            parte = ds.buscar_parte_packing_walmart(id_pedido);
                        }
                    }
                }
                if (parte == "1") { parte = "i"; }
            } else { parte = "0"; }
            ds.guardar_pk_nuevo(id_customer,id_customer_po,id_pedido, pk_nombre, address, driver, container, seal, replacement, "LEONARDO ALBAÑEZ", tipo, usuario, num_envio, tipo_packing,parte);
            ultimo_pk = ds.obtener_ultimo_pk_registrado();
            if (labels != "N/A"){
                string[] label = labels.Split('*');
                for (int i = 1; i < label.Length; i++){
                    ds.guardar_pk_labels(label[i], ultimo_pk, type_labels);
                }
            }
            Session["pedido"] = id_pedido;
            Session["pedido_pk"] = id_pedido;
            Session["pk"] = ultimo_pk;
            //Pk p = new Pk();
            // List<estilo_shipping> e = ds.lista_estilos(Convert.ToString(id_pedido));
            /*var result = Json(new { dc = dcs,
                estilos =e,
                //cantidades_estilos = ds.obtener_cantidades_estilos(id_pedido),
                number_po = ds.obtener_number_po_pedido(id_pedido),
                assorts = ds.lista_assortments_pedido(id_pedido),
                tallas = ds.obtener_lista_tallas_pedido(e)
            });*/
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult obtener_estilos_orden(string pedido){
             Session["pedido"] = pedido;
             Session["pedido_pk"] = pedido;
            List<estilo_shipping> e = ds.lista_estilos(pedido);
            var result = Json(new {
                dc = Convert.ToString(Session["dcs"]),
                estilos =e,
                //cantidades_estilos = ds.obtener_cantidades_estilos(id_pedido),
                number_po = ds.obtener_number_po_pedido(Convert.ToInt32(pedido)),
                assorts = ds.lista_assortments_pedido(Convert.ToInt32(pedido)),
                tallas = ds.obtener_lista_tallas_pedido(e)
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult obtener_estilos_orden_envio(string pedido){
            List<estilo_shipping> e = ds.lista_estilos(pedido);
            List<int> indices = ds.buscar_indices_cantidades_enviadas(pedido);
            var result = Json(new{
                estilos = e,
                tallas = ds.obtener_lista_tallas_pedido(e),
                guardados = ds.obtener_cantidades_envio_anterior(Convert.ToInt32(pedido),indices),
                lista_indices=indices,
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult obtener_estilos_hottopic(string pedido){
            Session["pedido"] = pedido;
            Session["pedido_pk"] = pedido;
            List<estilo_shipping> e = ds.lista_estilos_hottopic(pedido);
            var result = Json(new{
                estilos = e,
                number_po = ds.obtener_number_po_pedido(Convert.ToInt32(pedido)),
                tallas = ds.obtener_lista_tallas_pedido(e)
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_pk_estilos(string summary,string box,string talla,string cantidad_size, string store,string type,string dc,string empaque,string indice,string label,string sobrante,string tipo_packing,string num_ppk){                                             
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string[] summarys = summary.Split('*'),  cajas = box.Split('*'), cantidades=cantidad_size.Split('*'), labels = label.Split('*'),tallas=talla.Split('*');
            string[] stores = store.Split('*'), tipos = type.Split('*'), empaques = empaque.Split('*'),dcs=dc.Split('*'),sobrantes=sobrante.Split('*');
            string[] indices=indice.Split('*'),nums_ppks=num_ppk.Split('&');
            int packing = Convert.ToInt32(Session["pk"]);
            int id_pedido = Convert.ToInt32(Session["pedido_pk"]);
            int total_enviado = ds.obtener_total_enviado_pedido(id_pedido),total_pedido= ds.obtener_total_pedido(id_pedido);
            string number_po = ds.obtener_number_po_pedido(id_pedido);
            int shipping_id = 0,total_piezas_pk=0;
            for (int i = 1; i < summarys.Length; i++){
                int id_estilo = consultas.obtener_estilo_summary(Convert.ToInt32(summarys[i]));
                if (tipos[i] != "DMG" && tipos[i] != "EXT"){
                    switch (empaques[i]){
                        case "1"://TIPO DE EMPAQUE BLPACK
                            if (empaques[i] == "1" && dcs[i] == "0"){//SIN DC
                                string[] cantidades_talla = cantidades[i].Split('&');
                                for (int j = 1; j < cantidades_talla.Length; j++){
                                    total_piezas_pk += (Convert.ToInt32(cantidades_talla[j]));
                                }
                            }
                            if (empaques[i] == "1" && dcs[i] != "0"){//CON DC
                                string[] cantidades_tallas = cantidades[i].Split('&');
                                for (int k = 1; k < tallas.Length; k++){
                                    int r = ds.buscar_piezas_empaque_bull(Convert.ToInt32(summarys[i]), Convert.ToInt32(tallas[k]));
                                    total_piezas_pk += (r * Convert.ToInt32(cantidades_tallas[k]));
                                }
                            }
                            break;
                        case "5"://TIPO DE EMPAQUE BLPACK
                            if (empaques[i] == "5" && dcs[i] == "0"){//SIN DC
                                string[] cantidades_talla = cantidades[i].Split('&');
                                for (int j = 1; j < cantidades_talla.Length; j++){
                                    total_piezas_pk += (Convert.ToInt32(cantidades_talla[j]));
                                }
                            }
                            if (empaques[i] == "5" && dcs[i] != "0"){//CON DC
                                string[] cantidades_tallas = cantidades[i].Split('&');
                                for (int k = 1; k < tallas.Length; k++){
                                    int r = ds.buscar_piezas_empaque_bulls(Convert.ToInt32(summarys[i]), Convert.ToInt32(tallas[k]), nums_ppks[i]);
                                    total_piezas_pk += (r * Convert.ToInt32(cantidades_tallas[k]));
                                }
                            }
                            break;
                        case "2":
                            List<ratio_tallas> ratios = ds.obtener_lista_ratio(Convert.ToInt32(summarys[i]), id_estilo, 2);
                            foreach (ratio_tallas r in ratios){
                                total_piezas_pk += (Convert.ToInt32(cajas[i]) * r.ratio);
                            }
                            break;
                        case "3":
                            Assortment a = ds.assortment_id(Convert.ToInt32(summarys[i]), id_pedido);
                            foreach (estilos e in a.lista_estilos){
                                List<ratio_tallas> ratios_a = ds.obtener_lista_ratio_assort_r(e.id_po_summary, e.id_estilo, a.nombre);
                                foreach (ratio_tallas r in ratios_a){
                                    total_piezas_pk += (Convert.ToInt32(cajas[i]) * r.ratio);
                                }
                            }
                            break;
                        case "4":
                            string[] ppks = nums_ppks[i].Split('*');
                            List<ratio_tallas> r4 = ds.obtener_lista_ratio_ppks(Convert.ToInt32(summarys[i]), id_estilo, 4,Convert.ToInt32(ppks[0]),ppks[1]);
                            foreach (ratio_tallas r in r4){
                                total_piezas_pk += (Convert.ToInt32(cajas[i]) * r.ratio);
                            }
                            break;

                    }
                }else {
                    string[] cantidades_talla = cantidades[i].Split('&');
                    for (int j = 1; j < cantidades_talla.Length; j++){
                        total_piezas_pk += (Convert.ToInt32(cantidades_talla[j]));
                    }
                }
            }
            if ((total_enviado + total_piezas_pk) > (total_pedido + 100)){//ERROR
                ds.eliminar_packing_normal(packing);
                return Json("1", JsonRequestBehavior.AllowGet);
            }else{
                for (int i = 1; i < summarys.Length; i++){
                    int id_estilo = consultas.obtener_estilo_summary(Convert.ToInt32(summarys[i]));
                    if (tipos[i] != "DMG" && tipos[i] != "EXT"){
                        switch (empaques[i]){
                            case "1"://TIPO DE EMPAQUE BLPACK
                                if (empaques[i] == "1" && dcs[i] == "0"){//SIN DC
                                    string[] cantidades_talla = cantidades[i].Split('&');
                                    for (int j = 1; j < cantidades_talla.Length; j++){
                                        if (cantidades_talla[j] != "0"){
                                            ds.guardar_estilos_paking("1", labels[i], cantidades_talla[j], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], tallas[j].ToString(), stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], nums_ppks[i],"0");
                                            shipping_id = ds.obtener_ultimo_shipping_registrado();
                                            ds.agregar_cantidades_enviadas(summarys[i], tallas[j], (Convert.ToInt32(cantidades_talla[j])).ToString(), shipping_id.ToString(), tipos[i], "0", "1");
                                        }
                                    }
                                }
                                if (empaques[i] == "1" && dcs[i] != "0"){//CON DC
                                    string[] cantidades_talla = cantidades[i].Split('&');
                                    for (int j = 1; j < cantidades_talla.Length; j++){
                                        int piezas = ds.buscar_piezas_empaque_bull(Convert.ToInt32(summarys[i]), Convert.ToInt32(tallas[j]));
                                        piezas = (piezas * Convert.ToInt32(cantidades_talla[j]));
                                        if (cantidades_talla[j] != "0"){
                                            ds.guardar_estilos_paking("1", labels[i], cantidades_talla[j], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], tallas[j].ToString(), stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], nums_ppks[i], "0");
                                            shipping_id = ds.obtener_ultimo_shipping_registrado();
                                            ds.agregar_cantidades_enviadas(summarys[i], tallas[j], piezas.ToString(), shipping_id.ToString(), tipos[i], "0", "1");
                                        }
                                    }
                                }
                                break;
                            case "5"://TIPO DE EMPAQUE BLPACKS
                                if (empaques[i] == "5" && dcs[i] == "0"){//SIN DC
                                    string[] cantidades_talla = cantidades[i].Split('&');
                                    for (int j = 1; j < cantidades_talla.Length; j++){
                                        if (cantidades_talla[j] != "0"){
                                            ds.guardar_estilos_paking("1", labels[i], cantidades_talla[j], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], tallas[j].ToString(), stores[i], tipos[i], empaques[i], indices[i], sobrantes[i],  "0",nums_ppks[i]);
                                            shipping_id = ds.obtener_ultimo_shipping_registrado();
                                            ds.agregar_cantidades_enviadas(summarys[i], tallas[j], (Convert.ToInt32(cantidades_talla[j])).ToString(), shipping_id.ToString(), tipos[i], "0", "1");
                                        }
                                    }
                                }
                                if (empaques[i] == "5" && dcs[i] != "0"){//CON DC
                                    string[] cantidades_talla = cantidades[i].Split('&');
                                    for (int j = 1; j < cantidades_talla.Length; j++){                                       
                                        int piezas = ds.buscar_piezas_empaque_bulls(Convert.ToInt32(summarys[i]), Convert.ToInt32(tallas[j]), nums_ppks[i]);
                                        piezas = (piezas * Convert.ToInt32(cantidades_talla[j]));
                                        if (cantidades_talla[j] != "0"){
                                            ds.guardar_estilos_paking("1", labels[i], cantidades_talla[j], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], tallas[j].ToString(), stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], "0", nums_ppks[i]);
                                            shipping_id = ds.obtener_ultimo_shipping_registrado();
                                            ds.agregar_cantidades_enviadas(summarys[i], tallas[j], piezas.ToString(), shipping_id.ToString(), tipos[i], "0", "1");
                                        }
                                    }
                                }
                                break;
                            case "2":
                                ds.guardar_estilos_paking("1", labels[i], cajas[i], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], "0", stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], nums_ppks[i], "0");
                                shipping_id = ds.obtener_ultimo_shipping_registrado();
                                List<ratio_tallas> ratios = ds.obtener_lista_ratio(Convert.ToInt32(summarys[i]), id_estilo, 2);
                                foreach (ratio_tallas r in ratios){
                                    ds.agregar_cantidades_enviadas(summarys[i], (r.id_talla).ToString(), ((Convert.ToInt32(cajas[i]) * r.ratio)).ToString(), shipping_id.ToString(), tipos[i], "0", "1");                                   
                                }
                                break;
                            case "3":
                                ds.guardar_estilos_paking("1", labels[i], cajas[i], "0", packing.ToString(), id_pedido.ToString(), "0", number_po.ToString(), dcs[i], summarys[i], "0", stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], nums_ppks[i], "0");
                                shipping_id = ds.obtener_ultimo_shipping_registrado();
                                Assortment a = ds.assortment_id(Convert.ToInt32(summarys[i]), id_pedido);
                                foreach (estilos e in a.lista_estilos){
                                    List<ratio_tallas> ratios_a = ds.obtener_lista_ratio_assort_r(e.id_po_summary, e.id_estilo, a.nombre);
                                    foreach (ratio_tallas r in ratios_a){
                                        ds.agregar_cantidades_enviadas((e.id_po_summary).ToString(), (r.id_talla).ToString(), ((Convert.ToInt32(cajas[i]) * r.ratio)).ToString(), shipping_id.ToString(), tipos[i], "1", "1");
                                    }
                                }
                                break;
                            case "4":
                                string[] ppks = nums_ppks[i].Split('*');
                                ds.guardar_estilos_paking("1", labels[i], cajas[i], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], "0", stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], ppks[0], ppks[1]);
                                shipping_id = ds.obtener_ultimo_shipping_registrado();
                                List<ratio_tallas> ra = ds.obtener_lista_ratio_ppks(Convert.ToInt32(summarys[i]), id_estilo, 4, Convert.ToInt32(ppks[0]), ppks[1]);
                                foreach (ratio_tallas r in ra){
                                    ds.agregar_cantidades_enviadas(summarys[i], (r.id_talla).ToString(), ((Convert.ToInt32(cajas[i]) * r.ratio)).ToString(), shipping_id.ToString(), tipos[i], "0", "1");
                                }
                                break;
                        }
                    }else {
                        string[] cantidades_talla = cantidades[i].Split('&');
                        for (int j = 1; j < cantidades_talla.Length; j++){
                            if (cantidades_talla[j] != "0"){
                                ds.guardar_estilos_paking(cajas[i], labels[i], cantidades_talla[j], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], tallas[j].ToString(), stores[i], tipos[i], "1", indices[i], sobrantes[i], "0",nums_ppks[i]);
                                shipping_id = ds.obtener_ultimo_shipping_registrado();
                                ds.agregar_cantidades_enviadas(summarys[i], tallas[j], (Convert.ToInt32(cantidades_talla[j])).ToString(), shipping_id.ToString(), tipos[i], "0", "1");
                            }
                        }
                    }
                }
                ds.revisar_totales_estilo(id_pedido);
                verificar_estado_pedido(id_pedido);
                ds.editar_nombre_packing_list(Convert.ToInt32(Session["pk"]));
                return Json("0", JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult UploadBreakdown(){
            int id_pedido = Convert.ToInt32(Session["pedido_pk"]);
            int ultimo_pk = Convert.ToInt32(Session["pk"]);
                        
            // Checking no of files injected in Request object 
            string archivo = "";
            if (Request.Files.Count > 0){
                try{
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
                    Session["pedido_pk"] = id_pedido;
                    Session["pk"] = ultimo_pk;
                    // Returns message that successfully uploaded  
                    /*if (System.IO.File.Exists(fullPath)){//PARA BORRAR
                        System.IO.File.Delete(fullPath);
                    }*/
                    guardar_informacion_breakdown(Convert.ToString(Session["archivo_comparacion"]));
                    
                    if (System.IO.File.Exists(archivo)) { System.IO.File.Delete(archivo); }
                    // return Json("File Uploaded Successfully! "+archivo);
                    
                    return Json("", JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex) { return Json("Error occurred. Error details: " + ex.Message); }
            }
            else { return Json("No files selected."); }
            
        }
        public void guardar_informacion_breakdown(string archivo) {
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            DatosTrim dtrim = new DatosTrim();
            DatosFantasy df = new DatosFantasy();
            List<Talla> lista_tallas = ds.obtener_tallas_fantasy();
            int id_pedido=Convert.ToInt32(Session["pedido_pk"]);
            int packing = Convert.ToInt32(Session["pk"]);
            //int number_po = ds.obtener_number_po_pedido(id_pedido);
            using (XLWorkbook libro_trabajo = new XLWorkbook(archivo)){
                var ws = libro_trabajo.Worksheet(1);
                var nonEmptyDataRows = libro_trabajo.Worksheet(1).RowsUsed();
                var nonEmptyDataCols = libro_trabajo.Worksheet(1).ColumnsUsed();
                int indice = 1;
                int column=2, total_columas=0, shipping_id;
                string estilo, dc, id_estilo;
                string po_numero= consultas.obtener_po_id_fantasy((id_pedido).ToString());
                List<Estilo> lista_estilos = df.obtener_lista_estilos();
                //BUSCAR_LOS_ESTILOS_DE_FANTASY
                foreach (var dataCol in nonEmptyDataCols) { total_columas++; }

                foreach (var dataRow in nonEmptyDataRows){                   
                    if (dataRow.RowNumber() >4){
                        column = 2;
                        while (column <= total_columas){
                            dc = Convert.ToString(dataRow.Cell(1).Value).Trim();
                            estilo = Convert.ToString(ws.Cell(3,column).Value);
                            id_estilo = "0";
                            foreach (Estilo e in lista_estilos){
                                if (((estilo).Trim()).ToLower() == ((e.descripcion).Trim()).ToLower()){
                                    //summary = Convert.ToString(e.id_po_summary);
                                    id_estilo = Convert.ToString(e.id_estilo);
                                }
                            }
                            string cantidad = Convert.ToString(dataRow.Cell(column).Value);
                            foreach (Talla t in lista_tallas){
                                if (Convert.ToString(dataRow.Cell(column).Value)!="" && Convert.ToString(dataRow.Cell(column).Value) != "0") {
                                     ds.guardar_estilos_paking("1", "NONE", Convert.ToString(dataRow.Cell(column).Value), "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), po_numero, dc, "0", (t.id_talla).ToString(), "N/A", "NONE", "1", indice.ToString(),"0","0","0");
                                     shipping_id = ds.obtener_ultimo_shipping_registrado();
                                     ds.agregar_cantidades_enviadas("0", t.id_talla.ToString(), Convert.ToInt32(dataRow.Cell(column).Value).ToString(), shipping_id.ToString(), "NONE", "0","1");
                                }
                                column++;
                            }
                            indice++;
                        }
                    }
                }//FOREACH ROWS
                ds.editar_nombre_packing_list(Convert.ToInt32(Session["pk"]));
            }            
            // ds.revisar_totales_estilo(Convert.ToInt32(Session["pedido_pk"]));
            //verificar_estado_pedido(Convert.ToInt32(Session["pedido_pk"]));
        }
        public JsonResult guardar_pk_samples(string cliente, string address, string driver, string container, string seal, string replacement, string tipo, string labels, string type_labels, string num_envio, string tipo_packing, string pk_id) {
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            int usuario = Convert.ToInt32(Session["id_usuario"]);
            string[] pk_ant;
            string pk_nombre;
            //string pk_anterior = ds.obtener_ultimo_pk();
            int indice_pk = 0, ultimo_pk, id_customer, id_customer_po, id_pedido, year_pk;
            string parte = "0";
            /*if (pk_anterior != ""){
                pk_ant = pk_anterior.Split('-');
                year_pk = pk_ant[1].Split(' ');
                if (year_pk[0] == (DateTime.Now.Year.ToString())){
                    indice_pk = (Convert.ToInt32(pk_ant[0]) + 1);
                }else { indice_pk = 1; }
            }else{
                indice_pk = 1;
            }*/
            if (pk_id == "0"){
                if (tipo_packing == "9") { indice_pk = ds.buscar_contador_packing_ext_configuracion(); } else { indice_pk = ds.buscar_contador_packing_normal_configuracion(); }
                indice_pk++;
                year_pk = ds.buscar_year_packing_configuracion();
                /*int current_year = DateTime.Now.Year;
                if (year_pk != current_year){
                    ds.reset_configuracion();
                    indice_pk = 1;
                    year_pk = current_year;
                } */               
                if (tipo_packing == "9"){
                    ds.incrementar_contador_packing_ext();
                    pk_nombre = ((Convert.ToString(indice_pk).PadLeft(4, '0')) + "-" + year_pk.ToString() + " (EXT-DMG) FFB");
                }else{
                    ds.incrementar_contador_packing_normal();
                    pk_nombre = ((Convert.ToString(indice_pk).PadLeft(4, '0')) + "-" + year_pk.ToString() + " FFB");
                }
            }else {
                pk_nombre = "NOT ASIGNED";
            }
            //ds.guardar_pk_nuevo(Convert.ToInt32(cliente),0,0, ((Convert.ToString(indice_pk).PadLeft(4, '0')) + "-" + DateTime.Now.Year.ToString() + " FFB"), address, driver, container, seal, replacement, "LEONARDO ALBAÑEZ", tipo, usuario, num_envio, tipo_packing,"0");
            ds.guardar_pk_nuevo(Convert.ToInt32(cliente), 0, 0, pk_nombre, address, driver, container, seal, replacement, "LEONARDO ALBAÑEZ", tipo, usuario, num_envio, tipo_packing, parte);
            ultimo_pk = ds.obtener_ultimo_pk_registrado();                 
            Session["pk"] = ultimo_pk;            
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_estilos_samples(string cliente, string summary, string caja, string attnto, string talla, string cantidad, string indice, string inicial, string tipo_sample, string id_new, string po_new, string estilo_new, string color_new, string descripcion_new, string origen_new, string porcentaje_new, string genero_new, string cabecera, string total_col_extras)
        {
            //                                          'cliente':'    'summary':'','  caja':,'    attnto':'     talla':,'    cantidad':,'    indice':,'    inicial'      tipo_sample''        id_new':'     po_new':'      estilo_new',      color_new''      descripcion_new''      origen_new'   '   porcentaje_new''      genero_new'
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string[] summarys = summary.Split('*'), cajas = caja.Split('*'), attntos = attnto.Split('*'), indices = indice.Split('*');
            string[] tallas = talla.Split('*'), cantidades = cantidad.Split('*'), iniciales = inicial.Split('*'), tipos_samples = tipo_sample.Split('*');
            // tipo_sample''        id_new':'     po_new':'      estilo_new',      color_new''      descripcion_new''      origen_new'   '   porcentaje_new''      genero_new'
            string[] ids_new = id_new.Split('*'), pos_new = po_new.Split('*'), estilos_new = estilo_new.Split('*'), colores_new = color_new.Split('*'), descripciones_new = descripcion_new.Split('*'), origenes_new = origen_new.Split('*');
            string[] porcentajes_new = porcentaje_new.Split('*'), generos_new = genero_new.Split('*'), cabeceras = cabecera.Split('*');
            int packing = Convert.ToInt32(Session["pk"]), examples_id, total_this_envio, exceso = 0, columnas_extra = Convert.ToInt32(total_col_extras);
            /*for (int i = 1; i < summarys.Length; i++){
                if (tipos_samples[i] == "0"){
                    int total_enviado = ds.obtener_total_enviado_samples_pedido_summary(Convert.ToInt32(summarys[i])), total_pedido = ds.obtener_total_samples_pedido_summary(Convert.ToInt32(summarys[i]));
                    string[] quantities = cantidades[i].Split('&');
                    total_this_envio = 0;
                    for (int j = 1; j < 6; j++){
                        if (quantities[j] != "0") { total_this_envio += Convert.ToInt32(quantities[j]); }
                    }
                    if ((total_enviado + total_this_envio) > (total_pedido + 100)) { exceso++; }
                }
            }
            if (exceso != 0){
                ds.eliminar_packing_list(packing);
                return Json("1", JsonRequestBehavior.AllowGet);
            }else{*/
            //int nuevo_ejemplo = 0; 

            //ds.guardar_cabeceras_ejemplos(packing, cabeceras);
            for (int i = 1; i < summarys.Length; i++){
                string[] quantities = cantidades[i].Split('&');
                string[] sizes = (tallas[i]).Split('&');
                if (tipos_samples[i] == "1"){
                    for (int c = 1; c < ids_new.Length; c++){
                        if (ids_new[c] == summarys[i]){
                            ds.guardar_nuevo_estilo_ejemplo(colores_new[c], porcentajes_new[c], generos_new[c], pos_new[c], estilos_new[c], descripciones_new[c], origenes_new[c]);
                            summarys[i] = Convert.ToString(ds.obtener_ultimo_nuevo_ejemplo_registrado());
                        }
                    }
                }
                ds.guardar_estilo_ejemplo(packing, summarys[i], quantities[1], quantities[2], quantities[3], quantities[4], quantities[5], quantities[6], quantities[7], cajas[i], attntos[i], cliente, indices[i], iniciales[i], tipos_samples[i]);
                examples_id = ds.obtener_ultimo_examples_registrado();
                for (int j = 1; j < sizes.Length; j++){
                    if (quantities[j] != "0"){
                        ds.agregar_cantidades_enviadas(summarys[i], (consultas.buscar_talla(sizes[j])).ToString(), quantities[j], examples_id.ToString(), "SAM", "0", "3");
                    }
                }
                int contador_extras = 1;
                if (quantities.Length > 8){
                    for (int j = 8; j < (8 + columnas_extra); j++){
                        if (quantities[j] != "0"){
                            ds.agregar_cantidades_extras(examples_id, contador_extras, Convert.ToInt32(quantities[j]), (consultas.buscar_talla(sizes[j])));
                        }
                        contador_extras++;
                    }
                }
                if (cabeceras[i] == "1") {
                    ds.guardar_cabeceras_ejemplos(packing, tallas[i],indices[i]);
                }
            }
            List<string> summarys_temp = new List<string>();
            for (int i = 1; i < summarys.Length; i++){
                if (tipos_samples[i] == "0"){
                    summarys_temp.Add(summarys[i]);
                }
            }
            // string[] po_summarys = summarys.Distinct().ToArray();
            string[] po_summarys = summarys_temp.Distinct().ToArray();
            for (int i = 1; i < po_summarys.Length; i++){
                int pedido = consultas.obtener_id_pedido_summary(Convert.ToInt32(po_summarys[i]));
                ds.revisar_totales_estilo(pedido);
                verificar_estado_pedido(pedido);
            }
            ds.editar_nombre_packing_list(Convert.ToInt32(Session["pk"]));
            return Json("0", JsonRequestBehavior.AllowGet);
            // }
        }
        public JsonResult guardar_items_retorno(string pedido,string inventario, string cantidad, string categoria, string talla, string item,string caja,string qty,string indice){
            DatosTrim dtrim = new DatosTrim();

            string[] inventarios = inventario.Split('*'), cantidades = cantidad.Split('*'), categorias = categoria.Split('*'), tallas = talla.Split('*'), items = item.Split('*'),cajas=caja.Split('*');
            string[] quantities = qty.Split('*'), indices = indice.Split('*');

            int packing = Convert.ToInt32(Session["pk"]), summary;
            for (int i = 1; i < inventarios.Length; i++){
                summary = ds.buscar_summary_inventario(Convert.ToInt32(inventarios[i]));
                string[] cantidades_tallas = cantidades[i].Split('&');
                if (categorias[i] == "1"){
                    for (int j = 1; j < cantidades_tallas.Length; j++) {
                        if (cantidades_tallas[j] != "0"){
                            int inv = ds.buscar_inventario_retornos(pedido, tallas[j], items[i]);
                            ds.agregar_retorno_envio(inv.ToString(), cantidades_tallas[j], categorias[i], tallas[j], items[i], summary, packing, cajas[i], indices[i]);
                            dtrim.actualizar_inventario(inv, cantidades_tallas[j]);
                        }
                    }
                }else {
                    int inv = ds.buscar_inventario_retornos(pedido, "0", items[i]);
                    ds.agregar_retorno_envio(inventarios[i], quantities[i], categorias[i], "0", items[i], summary, packing, cajas[i],indices[i]);
                    dtrim.actualizar_inventario(Convert.ToInt32(inventarios[i]), quantities[i]);
                    //dtrim.revisar_estados_cantidades_trim(Convert.ToInt32(inventarios[i]), Convert.ToInt32(quantities[i]));                    
                }                
            }
            ds.editar_nombre_packing_list(Convert.ToInt32(Session["pk"]));
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_pk_fantasy_extras(string address, string driver, string container, string seal, string replacement, string tipo, string labels, string type_labels, string num_envio, string tipo_packing, string pk_id)
        {
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            int usuario = Convert.ToInt32(Session["id_usuario"]);
            string[] pk_ant;
            string pk_nombre;
            //string pk_anterior = ds.obtener_ultimo_pk();
            int indice_pk = 0, ultimo_pk, year_pk, id_customer, id_customer_po, id_pedido;
            /*if (pk_anterior != ""){
                pk_ant = pk_anterior.Split('-');
                year_pk = pk_ant[1].Split(' ');
                if (year_pk[0] == (DateTime.Now.Year.ToString())){
                    indice_pk = (Convert.ToInt32(pk_ant[0]) + 1);
                }else { indice_pk = 1; }
            }else { indice_pk = 1; }**/
            if (pk_id == "0"){
                if (tipo_packing == "9") { indice_pk = ds.buscar_contador_packing_ext_configuracion(); } else { indice_pk = ds.buscar_contador_packing_normal_configuracion(); }
                indice_pk++;
                year_pk = ds.buscar_year_packing_configuracion();
                /*int current_year = DateTime.Now.Year;
                if (year_pk != current_year){
                    ds.reset_configuracion();
                    indice_pk = 1;
                    year_pk = current_year;
                } */               
                if (tipo_packing == "9"){
                    ds.incrementar_contador_packing_ext();
                    pk_nombre = ((Convert.ToString(indice_pk).PadLeft(4, '0')) + "-" + year_pk.ToString() + " (EXT-DMG) FFB");
                }else{
                    ds.incrementar_contador_packing_normal();
                    pk_nombre = ((Convert.ToString(indice_pk).PadLeft(4, '0')) + "-" + year_pk.ToString() + " FFB");
                }
            }else {
                pk_nombre = "NOT ASIGNED";
            }
            //ds.guardar_pk_nuevo(2, 0, 0, ((Convert.ToString(indice_pk).PadLeft(4, '0')) + "-" + DateTime.Now.Year.ToString() + " FFB"), address, driver, container, seal, replacement, "LEONARDO ALBAÑEZ", tipo, usuario, num_envio, tipo_packing, "0");
            ds.guardar_pk_nuevo(2, 0, 0, pk_nombre, address, driver, container, seal, replacement, "LEONARDO ALBAÑEZ", tipo, usuario, num_envio, tipo_packing, "0");
            ultimo_pk = ds.obtener_ultimo_pk_registrado();
            Session["pk"] = ultimo_pk;
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_estilos_fantasy_extras(string summary, string caja, string cantidad, string tipo)
        {
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string[] summarys = summary.Split('*'), cajas = caja.Split('*'), tipos = tipo.Split('*');
            string[] cantidades = cantidad.Split('*');
            string talla = "";
            int packing = Convert.ToInt32(Session["pk"]), extra_id;
            int exceso = 0,total_this_envio;
            for (int i = 1; i < summarys.Length; i++){
                int total_enviado = ds.obtener_total_enviado_extras_pedido_summary(Convert.ToInt32(summarys[i])), total_pedido = ds.obtener_total_extras_pedido_summary(Convert.ToInt32(summarys[i]));
                string[] quantities = cantidades[i].Split('&');
                total_this_envio = 0;
                for (int j = 1; j < 6; j++){
                    if (quantities[j] != "0"){ total_this_envio += Convert.ToInt32(quantities[j]); }
                }
                if ((total_enviado + total_this_envio) > (total_pedido + 100)) { exceso++; }
            }
            if (exceso != 0) {
                ds.eliminar_packing_list(packing);
                return Json("1", JsonRequestBehavior.AllowGet);
            } else{
                for (int i = 1; i < summarys.Length; i++){
                    string[] quantities = cantidades[i].Split('&');
                    ds.guardar_estilo_fantasy_extras(packing, summarys[i], quantities[1], quantities[2], quantities[3], quantities[4], quantities[5], cajas[i], tipos[i]);
                    extra_id = ds.obtener_ultimo_extra_fantasy_registrado();
                    int pedido = consultas.obtener_id_pedido_summary(Convert.ToInt32(summarys[i]));
                    ds.revisar_totales_estilo(pedido);
                    verificar_estado_pedido(pedido);
                    for (int j = 1; j < 6; j++){
                        if (quantities[j] != "0"){
                            switch (j){
                                case 1: talla = "SM"; break;
                                case 2: talla = "MD"; break;
                                case 3: talla = "LG"; break;
                                case 4: talla = "XL"; break;
                                case 5: talla = "2XL"; break;
                            }
                            ds.agregar_cantidades_enviadas(summarys[i], (consultas.buscar_talla(talla)).ToString(), quantities[j], extra_id.ToString(), "EXT", "0","6");
                        }
                    }
                }
                ds.editar_nombre_packing_list(Convert.ToInt32(Session["pk"]));
                return Json("0", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult guardar_pk_estilos_ht(string size,string summary, string ponumero, string talla, string ppk, string carton, string store, string tipo, string label, string dc, string empaque, string indice) { 
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string[] summarys = summary.Split('*'), ponumeros = ponumero.Split('*'), tallas = talla.Split('*'), ppks = ppk.Split('*'), cartones = carton.Split('*');
            string[] stores = store.Split('*'), tipos = tipo.Split('*'), labels = label.Split('*'), dcs = dc.Split('*'), empaques = empaque.Split('*'), indices = indice.Split('*');
            string[] sizes = size.Split('*');
            int packing = Convert.ToInt32(Session["pk"]);
            int id_pedido = Convert.ToInt32(Session["pedido_pk"]);
            int total_enviado = ds.obtener_total_enviado_pedido(id_pedido), total_pedido = ds.obtener_total_pedido(id_pedido);
            string number_po = ds.obtener_number_po_pedido(id_pedido);
            int shipping_id = 0, total_piezas_pk = 0, repeat = 0;
            for (int i = 1; i < summarys.Length; i++) {
                if (tipos[i] == "EXT" || tipos[i] == "DMG"){
                    string[] cantidades = tallas[i].Split('&');
                    for (int j = 1; j < cantidades.Length; j++){
                        total_piezas_pk += Convert.ToInt32(cantidades[j]);
                    }
                }else{
                    switch (empaques[i]){
                        case "1":
                            string[] cantidades = tallas[i].Split('&');
                            for (int j = 1; j < cantidades.Length; j++){
                                total_piezas_pk += Convert.ToInt32(cantidades[j]);
                            }
                            break;
                        case "2":
                            List<ratio_tallas> ratios = ds.obtener_lista_ratio_hottopic(Convert.ToInt32(summarys[i]), ponumeros[i], empaques[i]);
                            foreach (ratio_tallas r in ratios){
                                total_piezas_pk += (Convert.ToInt32(cartones[i]) * Convert.ToInt32(ppks[i]) * r.ratio);
                            }
                            break;
                        case "4":
                            List<ratio_tallas> ra = ds.obtener_lista_ratio_hottopic(Convert.ToInt32(summarys[i]), ponumeros[i], empaques[i]);
                            foreach (ratio_tallas r in ra){
                                total_piezas_pk += (Convert.ToInt32(cartones[i]) * Convert.ToInt32(ppks[i]) * r.ratio);
                            }
                            break;
                    }
                }
            }
            if (((total_enviado + total_piezas_pk) > (total_pedido + 100)) && total_piezas_pk != 0){//ERROR
                return Json("1", JsonRequestBehavior.AllowGet);
            }else {
                for (int i = 1; i < summarys.Length; i++){
                    int id_estilo = consultas.obtener_estilo_summary(Convert.ToInt32(summarys[i]));
                    if (tipos[i] == "EXT" || tipos[i] == "DMG" ){
                        string[] cantidades = tallas[i].Split('&');
                        for (int j = 1; j < cantidades.Length; j++){
                            if (cantidades[j] != "0"){
                                ds.guardar_estilos_paking(cartones[i], labels[i], cantidades[j], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), ponumeros[i], dcs[i], summarys[i], sizes[j], stores[i], tipos[i],"1", indices[i],"0","0","0");
                                shipping_id = ds.obtener_ultimo_shipping_registrado();
                                ds.agregar_cantidades_enviadas(summarys[i], sizes[j], cantidades[j], shipping_id.ToString(), tipos[i], "0","7");
                            }
                        }
                    }else{
                        switch (empaques[i]){
                            case "1":
                                string[] cantidades = tallas[i].Split('&');
                                for (int j = 1; j < cantidades.Length; j++){
                                    if (cantidades[j] != "0"){
                                        ds.guardar_estilos_paking("1", labels[i], cantidades[j], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), ponumeros[i], dcs[i], summarys[i], sizes[j], stores[i], tipos[i], empaques[i], indices[i], "0", "0", "0");
                                        shipping_id = ds.obtener_ultimo_shipping_registrado();
                                        ds.agregar_cantidades_enviadas(summarys[i], sizes[j], cantidades[j], shipping_id.ToString(), tipos[i], "0", "7");
                                    }
                                }
                                break;
                            case "2":
                                if (ppks[i] != "" && cartones[i] != "" && ppks[i] != "0" && cartones[i]!="0"){
                                    ds.guardar_estilos_paking(ppks[i], labels[i], cartones[i], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), ponumeros[i], dcs[i], summarys[i], "0", stores[i], tipos[i], empaques[i], indices[i], "0", "0", "0");
                                    shipping_id = ds.obtener_ultimo_shipping_registrado();
                                    List<ratio_tallas> ratios = ds.obtener_lista_ratio_hottopic(Convert.ToInt32(summarys[i]), ponumeros[i], empaques[i]);
                                    foreach (ratio_tallas r in ratios){
                                        if (((Convert.ToInt32(cartones[i]) * Convert.ToInt32(ppks[i]) * r.ratio)) != 0){
                                            ds.agregar_cantidades_enviadas(summarys[i], (r.id_talla).ToString(), ((Convert.ToInt32(cartones[i]) * Convert.ToInt32(ppks[i]) * r.ratio)).ToString(), shipping_id.ToString(), tipos[i], "0", "7");
                                        }
                                    }
                                }
                                break;
                            case "4":
                                if (ppks[i] != "" && cartones[i] != "" && ppks[i] != "0" && cartones[i] != "0"){
                                    ds.guardar_estilos_paking(ppks[i], labels[i], cartones[i], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), ponumeros[i], dcs[i], summarys[i], "0", stores[i], tipos[i], empaques[i], indices[i], "0", "0", "0");
                                    shipping_id = ds.obtener_ultimo_shipping_registrado();
                                    List<ratio_tallas> ratios = ds.obtener_lista_ratio_hottopic(Convert.ToInt32(summarys[i]), ponumeros[i], empaques[i]);
                                    foreach (ratio_tallas r in ratios){
                                        if (((Convert.ToInt32(cartones[i]) * Convert.ToInt32(ppks[i]) * r.ratio)) != 0)
                                        {
                                            ds.agregar_cantidades_enviadas(summarys[i], (r.id_talla).ToString(), ((Convert.ToInt32(cartones[i]) * Convert.ToInt32(ppks[i]) * r.ratio)).ToString(), shipping_id.ToString(), tipos[i], "0", "7");
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
                ds.revisar_totales_estilo(id_pedido);
                verificar_estado_pedido(id_pedido);
                ds.editar_nombre_packing_list(Convert.ToInt32(Session["pk"]));
                return Json("0", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult guardar_pk_directo_fantasy(string customer, string pedido, string address, string driver, string container, string seal, string replacement, string tipo, string labels, string type_labels, string num_envio, string tipo_packing, string pk_id) {
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            int usuario = Convert.ToInt32(Session["id_usuario"]);
            string[] pk_ant;
            string pk_nombre;
            string pk_anterior = ds.obtener_ultimo_pk();
            int indice_pk = 0, ultimo_pk, id_customer, id_customer_po, id_pedido, parte = 0, year_pk;
            /*if (pk_anterior != ""){
                pk_ant = pk_anterior.Split('-');
                year_pk = pk_ant[1].Split(' ');
                if (year_pk[0] == (DateTime.Now.Year.ToString())){
                    indice_pk = (Convert.ToInt32(pk_ant[0]) + 1);
                }else { indice_pk = 1; }
            }else{ indice_pk = 1; }*/
            if (pk_id == "0"){
                if (tipo_packing == "9") { indice_pk = ds.buscar_contador_packing_ext_configuracion(); } else { indice_pk = ds.buscar_contador_packing_normal_configuracion(); }
                indice_pk++;
                year_pk = ds.buscar_year_packing_configuracion();
                /*int current_year = DateTime.Now.Year;
                if (year_pk != current_year){
                    ds.reset_configuracion();
                    indice_pk = 1;
                    year_pk = current_year;
                } */               
                if (tipo_packing == "9"){
                    ds.incrementar_contador_packing_ext();
                    pk_nombre = ((Convert.ToString(indice_pk).PadLeft(4, '0')) + "-" + year_pk.ToString() + " (EXT-DMG) FFB");
                }else{
                    ds.incrementar_contador_packing_normal();
                    pk_nombre = ((Convert.ToString(indice_pk).PadLeft(4, '0')) + "-" + year_pk.ToString() + " FFB");
                }
            }else {
                pk_nombre = "NOT ASIGNED";
            }

            id_customer_po = Convert.ToInt32(customer);
            ds.guardar_pedido_fantasy(pedido, id_customer_po);
            id_pedido = ds.obtener_ultimo_pedido_fantasy();
            parte = 0;
            //ds.guardar_pk_nuevo(2, id_customer_po, id_pedido, ((Convert.ToString(indice_pk).PadLeft(4, '0')) + "-" + DateTime.Now.Year.ToString() + " FFB"), address, driver, container, seal, replacement, "LEONARDO ALBAÑEZ", "1", usuario, num_envio, tipo_packing, "0");
            ds.guardar_pk_nuevo(2, id_customer_po, id_pedido, pk_nombre, address, driver, container, seal, replacement, "LEONARDO ALBAÑEZ", tipo, usuario, num_envio, tipo_packing, "0");
            ultimo_pk = ds.obtener_ultimo_pk_registrado();
            if (labels != "N/A"){
                string[] label = labels.Split('*');
                for (int i = 1; i < label.Length; i++){
                    ds.guardar_pk_labels(label[i], ultimo_pk, type_labels);
                }
            }
            Session["pedido"] = id_pedido;
            Session["pedido_pk"] = id_pedido;
            Session["pk"] = ultimo_pk;
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_pk_estilos_directo_fantasy(string estilo, string po_sum, string index, string cantidad){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string[] estilos = estilo.Split('*'),posums= po_sum.Split('*'),indices=index.Split('*'),cantidades=cantidad.Split('*');
            string[] tallas = { "","SM", "MD", "LG", "XL", "2XL" };
            int id_pedido = Convert.ToInt32(Session["pedido_pk"]), shipping_id=0;
            int pk = Convert.ToInt32(Session["pk"]);
            for (int i = 1; i < estilos.Length; i++) {
                string[] totales = (cantidades[i]).Split('&');
                for (int jj= 1; jj < totales.Length; jj++){
                    if (totales[jj] != "0"){
                        int id_talla = consultas.buscar_talla(tallas[jj]);
                        ds.guardar_estilos_paking("1", "NONE", totales[jj], "0", pk.ToString(), id_pedido.ToString(),estilos[i], posums[i], "0", "0", id_talla.ToString(), "N/A", "NONE", "1", indices[i], "0", "0", "0");
                        shipping_id = ds.obtener_ultimo_shipping_registrado();
                        ds.agregar_cantidades_enviadas(estilos[i], id_talla.ToString(), totales[jj], shipping_id.ToString(), "NONE", "0", "8");
                    }
                }
            }
            ds.editar_nombre_packing_list(Convert.ToInt32(Session["pk"]));
            return Json("", JsonRequestBehavior.AllowGet);
        }
        //VERIFICAR SI YA SE MANDO TODO O NO PARA CERRAR EL PEDIDO
        public void verificar_estado_pedido(int pedido){           
            int total_enviado = ds.obtener_total_enviado_pedido(pedido), total_pedido = ds.obtener_total_pedido(pedido);
            if (total_enviado >= total_pedido){
                ds.eliminar_inventario_pedido(pedido);
                ds.cerrar_pedido(pedido);               
            }
        }
        public JsonResult cerrar_po(string po) {
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            int id_pedido = consultas.buscar_pedido(po);
            ds.cerrar_estilos_pedido(id_pedido);
            ds.cerrar_pedido(id_pedido);
            return Json("", JsonRequestBehavior.AllowGet);
        }
       
        public JsonResult buscar_estilos_packing(int id){
            return Json(Json(new { estilos = ds.lista_estilos_packing(id),
                                    returns=ds.lista_estilos_returns_pk(id),
                                    fantasy=ds.lista_estilos_extras_fantasy_pk(id)
            }), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_estilos_packing_inicio(int id){
            return Json(Json(new{
                estilos = ds.lista_estilos_packing_sin_tarima(id),
                returns = ds.lista_estilos_returns_pk_sin_tarima(id),
                fantasy = ds.lista_estilos_extras_fantasy_pk_sin_tarima(id)
            }), JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_ids_pk(string tarima, string shipping_id, string packing,string index, string empaque,string tipo){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string[] Shippings = shipping_id.Split('*'),Packings=packing.Split('*'),Indices=index.Split('*'),Empaques=empaque.Split('*'),Tipos=tipo.Split('*'),Tarimas=tarima.Split('*');
            for (int i = 1; i < Shippings.Length; i++){
                switch (Tipos[i]) {
                    case "1":
                    case "2":                                
                    case "7":
                    case "4":
                        int tipo_fila = ds.obtener_tipo_fila_estilo(Shippings[i]);
                        int pedido = ds.buscar_pedido_pk(Packings[i]);
                        int customer = consultas.obtener_customer_po(pedido);
                        int customer_final = consultas.obtener_customer_final_po(pedido);
                        if (customer == 2){
                            ds.guardar_ids_tarimas_bpdc(Tarimas[i], Packings[i], Indices[i]); 
                        }else {
                            if (customer == 29 || customer == 30){
                                int bbp = ds.buscar_bulpack_pedido(pedido);
                                int ucc = ds.buscar_labels_pedido(pedido);
                                if (bbp != 0 && ucc != 0){
                                    ds.guardar_ids_tarimas_bpdc(Tarimas[i], Packings[i], Indices[i]); 
                                }else{
                                    //AQUI ES NORMAL
                                    if (tipo_fila == 1){
                                        ds.guardar_ids_tarimas_bpdc(Tarimas[i], Packings[i], Indices[i]); 
                                    }else{
                                        ds.guardar_ids_tarimas(Tarimas[i], Shippings[i]);
                                    }
                                }
                            }else{
                                if (customer_final == 27){//HOT TOPIC
                                    ds.guardar_ids_tarimas_bpdc(Tarimas[i], Packings[i], Indices[i]); 
                                }else{
                                    if (tipo_fila == 1){
                                        ds.guardar_ids_tarimas_bpdc(Tarimas[i], Packings[i], Indices[i]); 
                                    }else{
                                        ds.guardar_ids_tarimas(Tarimas[i], Shippings[i]);
                                    }
                                }
                            }
                        }                                          
                        break;                    
                    case "5":
                        ds.guardar_ids_tarimas_returns(tarima, Shippings[i]);
                        break;
                    case "6":
                        ds.guardar_ids_tarimas_fantasy(tarima, Shippings[i]);
                        break;
                }                
            }         
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_ids_pk_individual(string tarima, string shipping_id, string packing, string index, string empaque, string tipo)
        {
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string[] Shippings = shipping_id.Split('*'), Packings = packing.Split('*'), Indices = index.Split('*'), Empaques = empaque.Split('*'), Tipos = tipo.Split('*');
            for (int i = 1; i < Shippings.Length; i++){
                switch (Tipos[i])
                {case "1":
                    case "2":
                    case "7":
                    case "4":
                        int tipo_fila = ds.obtener_tipo_fila_estilo(Shippings[i]);
                        int pedido = ds.buscar_pedido_pk(Packings[i]);
                        int customer = consultas.obtener_customer_po(pedido);
                        int customer_final = consultas.obtener_customer_final_po(pedido);
                        if (customer == 2){
                            ds.guardar_ids_tarimas_bpdc(tarima, Packings[i], Indices[i]); // -- REVISAR -- REVISAR -- REVISAR -- REVISAR
                        }else{
                            if (customer == 29 || customer == 30){
                                int bbp = ds.buscar_bulpack_pedido(pedido);
                                int ucc = ds.buscar_labels_pedido(pedido);
                                if (bbp != 0 && ucc != 0){
                                    ds.guardar_ids_tarimas_bpdc(tarima, Packings[i], Indices[i]); // ESTILO FANTASY
                                }else{
                                    //AQUI ES NORMAL
                                    if (tipo_fila == 1){
                                        ds.guardar_ids_tarimas_bpdc(tarima, Packings[i], Indices[i]); // -- REVISAR -- REVISAR -- REVISAR -- REVISAR
                                    }else{
                                        ds.guardar_ids_tarimas(tarima, Shippings[i]);
                                    }
                                }
                            }else{
                                if (customer_final == 27){//HOT TOPIC
                                    ds.guardar_ids_tarimas_bpdc(tarima, Packings[i], Indices[i]); // -- REVISAR -- REVISAR -- REVISAR -- REVISAR
                                }else{
                                    if (tipo_fila == 1){
                                        ds.guardar_ids_tarimas_bpdc(tarima, Packings[i], Indices[i]); // -- REVISAR -- REVISAR -- REVISAR -- REVISAR
                                    }else{
                                        ds.guardar_ids_tarimas(tarima, Shippings[i]);
                                    }
                                }
                            }
                        }
                        break;

                    case "5":
                        ds.guardar_ids_tarimas_returns(tarima, Shippings[i]);
                        break;
                    case "6":
                        ds.guardar_ids_tarimas_fantasy(tarima, Shippings[i]);
                        break;
                }
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_pk_tabla(string busqueda){
            /*string sourceDirectory = Server.MapPath("/") + "/Content/img/";
            var files = Directory.EnumerateFiles(Server.MapPath("/") + "/Content/img/", "cliente.*");
            foreach (string currentFile in files){
                string fileName = currentFile.Substring(sourceDirectory.Length);
            }*/           
            return Json(ds.lista_buscar_pk_inicio(busqueda), JsonRequestBehavior.AllowGet);
        }
        public JsonResult abrir_pk(string id){
            Session["pk"] = id;//BOL
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult eliminar_packing(string packing, string tipo){
            string pk = ds.obtener_clave_packing(Convert.ToInt32(packing));
            ds.guardar_packing_borrado( pk, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),tipo);
            switch (tipo){
                case "1":
                case "2":
                case "7":                   
                    ds.eliminar_estilos_packing_list(Convert.ToInt32(packing));
                    ds.eliminar_packing_list(Convert.ToInt32(packing));
                    break;
                case "9":
                    ds.eliminar_estilos_packing_list(Convert.ToInt32(packing));
                    ds.eliminar_packing_list(Convert.ToInt32(packing));
                    break;
                case "3":                    
                    ds.eliminar_packing_edicion_samples(Convert.ToInt32(packing));
                    ds.eliminar_packing_list(Convert.ToInt32(packing));
                    break;
                case "5":                   
                    ds.eliminar_returns_packing(Convert.ToInt32(packing));
                    ds.eliminar_packing_list(Convert.ToInt32(packing));
                    break;
                case "6":                    
                    ds.eliminar_estilos_extra_packing_fantasy(Convert.ToInt32(packing));
                    ds.eliminar_packing_list(Convert.ToInt32(packing));
                    break;
                case "4":                    
                    ds.eliminar_pedido_fantasy(Convert.ToInt32(packing));
                    ds.eliminar_estilos_packing_list(Convert.ToInt32(packing));
                    ds.eliminar_packing_list(Convert.ToInt32(packing));
                    break;
                case "8":                    
                    ds.eliminar_pedido_fantasy(Convert.ToInt32(packing));
                    ds.eliminar_estilos_packing_list(Convert.ToInt32(packing));
                    ds.eliminar_packing_list(Convert.ToInt32(packing));
                    break;
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult asignar_pk(string packing, string tipo){
            int indice_pk = 0, year_pk;
            string pk_nombre = "";
            if (tipo == "9") { indice_pk = ds.buscar_contador_packing_ext_configuracion(); } else { indice_pk = ds.buscar_contador_packing_normal_configuracion(); }
            indice_pk++;
            year_pk = ds.buscar_year_packing_configuracion();            
            if (tipo== "9"){
                ds.incrementar_contador_packing_ext();
                pk_nombre = ((Convert.ToString(indice_pk).PadLeft(4, '0')) + "-" + year_pk.ToString() + " (EXT-DMG) FFB");
            }else{
                ds.incrementar_contador_packing_normal();
                pk_nombre = ((Convert.ToString(indice_pk).PadLeft(4, '0')) + "-" + year_pk.ToString() + " FFB");
            }
            ds.cambiar_pk_packing_list(Convert.ToInt32(packing),pk_nombre);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult asignar_pk_borrado(string packing, string tipo,string id_pk_borrado){
            string nuevo_pk = ds.obtener_pk_borrado(Convert.ToInt32(id_pk_borrado));
            ds.cambiar_pk_packing_list(Convert.ToInt32(packing), nuevo_pk);
            ds.eliminar_packing_borrado(Convert.ToInt32(id_pk_borrado));
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public void excel_pk(){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();            
            int tipo_packing = ds.obtener_tipo_packing(Convert.ToInt32(Session["pk"]));
            switch(tipo_packing){
                case 1:                
                case 2:                
                case 9:                
                    int pedido = ds.buscar_pedido_pk(Convert.ToString(Session["pk"]));
                    int customer = consultas.obtener_customer_po(pedido);
                    //int customer = 2;
                    int customer_final = consultas.obtener_customer_final_po(pedido);
                    if (customer == 2){//FANTASY
                        crear_excel_fantasy(ds.obtener_packing_list(Convert.ToInt32(Session["pk"])));
                    }else {
                        if (customer == 29 || customer==30) {
                            int bbp = ds.buscar_bulpack_pedido(Convert.ToInt32(Session["pk"]));
                            int ucc = ds.buscar_labels_pedido(Convert.ToInt32(Session["pk"]));
                            if (bbp != 0 && ucc != 0) {
                                crear_excel_fantasy(ds.obtener_packing_list(Convert.ToInt32(Session["pk"])));
                            } else {
                                crear_excel(ds.obtener_packing_list(Convert.ToInt32(Session["pk"])));
                            }
                        } else {
                            if (customer_final == 27){//HOT TOPIC
                               crear_excel_hottopic(ds.obtener_packing_list(Convert.ToInt32(Session["pk"])));
                            }else {
                                crear_excel(ds.obtener_packing_list(Convert.ToInt32(Session["pk"])));
                            }
                        }
                    }                    
                    break;
                case 3:
                    crear_excel_samples(ds.obtener_packing_list(Convert.ToInt32(Session["pk"])));
                    break;
                case 4:
                    crear_excel_fantasy_breakdown(ds.obtener_packing_list(Convert.ToInt32(Session["pk"])));
                    break;
                case 5:
                    crear_excel_returns(ds.obtener_packing_list(Convert.ToInt32(Session["pk"])));
                    break;
                case 6:
                    crear_excel_fantasy_extras(ds.obtener_packing_list(Convert.ToInt32(Session["pk"])));
                    break;
                case 7:
                    crear_excel_hottopic(ds.obtener_packing_list(Convert.ToInt32(Session["pk"])));
                    break;
                case 8:
                    crear_excel_directo_fantasy(ds.obtener_packing_list(Convert.ToInt32(Session["pk"])));
                    break;
            }            
        }
        public void crear_excel(List<Pk> lista)
        {
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string clave_packing = "";
            using (XLWorkbook libro_trabajo = new XLWorkbook())
            {
                string archivo = "";
                int estilos_total = 0, rows = 0, tarima_contador = 0, total_ratio, contador, r, c, total_cajas = 0, filas = 0, columnas = 0, tallas_id;
                List<Talla> tallas = new List<Talla>();
                List<Talla> tallas_bpdc = new List<Talla>();
                var ws = libro_trabajo.Worksheets.Add("PK");
                foreach (Pk item in lista)
                {
                    clave_packing = item.packing;
                    string[] nombre_archivo = (item.packing).Split('-');
                    archivo = " PK " + nombre_archivo[0] + " ";
                    //item.parte = consultas.AddOrdinal(Convert.ToInt32(item.parte))+" Part";
                    /*****INICIO CON DIRECCIONES, LOGO, ETC******/
                    ws.Cell("A2").Value = "FORTUNE FASHIONS BAJA, S.R.L. DE C.V.";
                    ws.Cell("A3").Value = "CALLE TORTUGA No 313-A";
                    ws.Cell("A4").Value = "MANEADERO CP 22790";
                    ws.Cell("A5").Value = "BAJA CALIFORNIA";
                    ws.Style.Font.FontSize = 8;

                    ws.Range("A2:A10").Style.Font.Bold = true;
                    ws.Range("A7:A10").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    //DIRECCIÓN DE ORIGEN
                    ws.Cell("A7").Value = "CUSTOMER: ";
                    ws.Cell("B7").Value = item.customer;

                    int ex_label = ds.contar_labels(item.id_packing_list);
                    if (ex_label != 0)
                    {
                        List<Labels> lista_etiquetas = new List<Labels>();
                        lista_etiquetas = ds.obtener_etiquetas(item.id_packing_list);
                        ws.Cell("A8").Value = "P.O.: ";
                        string label = Regex.Replace(item.pedido, @"\s+", " ") + "(PO# ";
                        foreach (Labels l in lista_etiquetas) { label += " " + l.label; }
                        if (ex_label == 1) { label += " )" + " (With UCC Labels) " + item.parte; }
                        else { label += " )" + " (With TPM Labels) " + item.parte; }
                        // ws.Cell("B8").Value = label;
                        // archivo += label;
                    }
                    else
                    {
                        ws.Cell("A8").Value = "P.O.: ";
                        // ws.Cell("B8").Value = Regex.Replace(item.pedido, @"\s+", " ") + "(Without UCC Labels) " + item.parte;
                        archivo += Regex.Replace(item.pedido, @"\s+", " ") + "(Without UCC Labels) " + item.parte;
                    }
                    ws.Cell("B8").Value = item.nombre_archivo;
                    archivo = " PK " + nombre_archivo[0] + " ";
                    archivo += " " + item.nombre_archivo;

                    ws.Cell("A9").Value = "RETAILER: ";
                    ws.Cell("B9").Value = item.customer_po;
                    ws.Range("B2:B10").Style.Font.Bold = true;
                    //IMAGEN AL CENTRO
                    ws.Range(1, 7, 1, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    var imagePath = Server.MapPath("/") + "/Content/img/LOGOFORTUNEEXCEL.png";
                    //var imagePath = @"C:\Users\melissa\source\repos\FortuneSys----\FortuneSystem\Content\img\LOGO FORTUNE.png";
                    //var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1")).Scale(0.30);
                    var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1"));
                    //PK ABAJO DE LA IMAGEN
                    ws.Cell("D7").Value = "PK: ";
                    ws.Cell("D7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell("D7").Style.Font.Bold = true;
                    ws.Cell("E7").Value = item.packing;
                    ws.Cell("E7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range("E7:F7").Merge();
                    ws.Range("E7", "F7").Style.Font.Bold = true;
                    ws.Range("D7", "F7").Style.Font.FontSize = 15;
                    ws.Range("E7:F7").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range("E7:F7").Style.Border.BottomBorderColor = XLColor.Black;
                    //DIRECCION DE DESTINO
                    ws.Cell("L2").Value = "SHIP TO: ";
                    ws.Cell("L3").Value = item.destino.nombre;
                    ws.Cell("L4").Value = item.destino.direccion;
                    ws.Cell("L5").Value = item.destino.ciudad + " " + item.destino.zip;
                    ws.Cell("L8").Value = "DATE:" + item.fecha;
                    ws.Range("L2", "L10").Style.Font.Bold = true;
                    var columna_a = ws.Range("A2", "A10");
                    ws.Rows("6").Height = 30;
                    for (int zz = 1; zz <= 10; zz++) { ws.Row(zz).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    int contador_cabeceras = 0, contador_tallas = 0, pallets = 0, tiendas = 0, dc = 0, ppk = 0, bp = 0, ass = 0;
                    int suma_estilo, suma_cajas;
                    List<estilos> lista_descripciones_finales = new List<estilos>();
                    var headers = new List<String[]>();
                    List<String> titulos = new List<string>();//=  "ID", "P.O. NUM", "TYPE", "COLOR", "DESCRIPTION";
                    titulos.Add("ID"); titulos.Add("P.O. NUM"); titulos.Add("TYPE");
                    foreach (Tarima t in item.lista_tarimas)
                    { //REVISAN TIPOS DE EMPAQUE, DATOS, DC,ETC
                        foreach (estilos e in t.lista_estilos)
                        {
                            if (e.store != "N/A" && e.store != "NA") { tiendas++; }
                            if (e.dc != "0") { dc++; }
                            if (e.tipo_empaque == 1 || e.tipo_empaque == 5) { bp++; }
                            if (e.tipo_empaque == 2) { ppk++; }
                            if (e.tipo_empaque == 3) { ass++; ppk++; }
                        }
                    }
                    if (ass != 0) { titulos.Add("ASSORTMENT"); }
                    titulos.Add("STYLE"); titulos.Add("COLOR"); titulos.Add("DESCRIPTION");
                    if (tiendas != 0) { titulos.Add("STORE"); }
                    if (dc != 0) { titulos.Add("DC"); }
                    if (ppk != 0) { titulos.Add("RATIO"); }
                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                    //List<estilo_shipping> es = ds.lista_estilos(Convert.ToString(item.id_pedido));
                    //tallas = ds.obtener_lista_tallas_pedido(es);.
                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                    foreach (Tarima t in item.lista_tarimas)
                    {
                        foreach (estilos e in t.lista_estilos)
                        {
                            if (e.tipo_empaque != 3)
                            {
                                foreach (ratio_tallas ra in e.lista_ratio)
                                {
                                    bool isEmpty = !tallas.Any();
                                    if (isEmpty)
                                    {
                                        Talla ta = new Talla();
                                        ta.id_talla = ra.id_talla;
                                        ta.talla = ra.talla;
                                        tallas.Add(ta);
                                    }
                                    else
                                    {
                                        int existe = 0;
                                        foreach (Talla sizes in tallas)
                                        {
                                            if (sizes.id_talla == ra.id_talla) { existe++; }
                                        }
                                        if (existe == 0)
                                        {
                                            Talla ta = new Talla();
                                            ta.id_talla = ra.id_talla;
                                            ta.talla = ra.talla;
                                            tallas.Add(ta);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (estilos ea in e.assort.lista_estilos)
                                {
                                    foreach (ratio_tallas ras in ea.lista_ratio)
                                    {
                                        bool isEmpty = !tallas.Any();
                                        if (isEmpty)
                                        {
                                            Talla ta = new Talla();
                                            ta.id_talla = ras.id_talla;
                                            ta.talla = ras.talla;
                                            tallas.Add(ta);
                                        }
                                        else
                                        {
                                            int existe = 0;
                                            foreach (Talla sizes in tallas)
                                            {
                                                if (sizes.id_talla == ras.id_talla) { existe++; }
                                            }
                                            if (existe == 0)
                                            {
                                                Talla ta = new Talla();
                                                ta.id_talla = ras.id_talla;
                                                ta.talla = ras.talla;
                                                tallas.Add(ta);
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                    }
                    tallas = ds.obtener_tallas_pk(tallas);
                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS<-----TALLAS
                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS<-----TALLAS
                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS<-----TALLAS
                    List<int> tallas_id_temporal = new List<int>();
                    contador_tallas = 0;
                    foreach (Talla sizes in tallas)
                    {
                        titulos.Add(sizes.talla); //AQUI AGREGO LAS TALLAS A LA CABECERA
                        tallas_id_temporal.Add(sizes.id_talla);
                        contador_tallas++;
                    }
                    titulos.Add("PCS"); titulos.Add("BXS"); titulos.Add("PALLETS");
                    foreach (string s in titulos) { contador_cabeceras++; }
                    headers.Add(titulos.ToArray());
                    int total_titulos = (titulos.ToArray()).Length;
                    ws.Cell(11, 1).Value = headers;
                    ws.Column(2).AdjustToContents();
                    ws.Column(5).AdjustToContents();
                    for (int i = 1; i <= total_titulos; i++)
                    {
                        ws.Cell(11, i).Style.Font.Bold = true;
                        ws.Cell(11, i).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                        ws.Cell(11, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(11, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.LeftBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.RightBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.TopBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.BottomBorderColor = XLColor.Black;
                    }
                    ws.Rows("6").Height = 30;
                    /**********************************************************************************************************************************************************************************************/
                    /***********************C*A*B*E*C*E*R*A*S**D*E**L*A**T*A*B*L*A*********************************************************************************************************************************/
                    //int[] tallas_comparacion = tallas_id_temporal.ToArray(); 
                    /*foreach (Tarima tarimas in item.lista_tarimas) {  pallets++; }*///CONTEO DE TARIMAS!!!!!
                    //----------SUMA DE TALLAS AL FINAL------------------SUMA DE TALLAS AL FINAL
                    int[] sumas_tallas = new int[contador_tallas + 2];
                    //----------SUMA DE TALLAS AL FINAL------------------SUMA DE TALLAS AL FINAL
                    for (int i = 0; i < contador_tallas + 2; i++) { sumas_tallas[i] = 0; }
                    r = 12;
                    int estilos_extras = 0, estilos_normales = 0;
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************
                    foreach (Tarima tarimas in item.lista_tarimas)
                    {
                        estilos_total = 0;
                        estilos_extras = 0; estilos_normales = 0;
                        var celdas_estilos_i = new List<String[]>();
                        var celdas_estilos = new List<String[]>();
                        List<int> index = new List<int>();//BUSCO LOS INDEX DE LOS DC
                        List<int> index_bp = new List<int>();//BUSCO LOS INDEX DE LOS DC

                        foreach (estilos estilo in tarimas.lista_estilos)
                        {
                            if (estilo.tipo == "EXT" || estilo.tipo == "DMG")
                            {
                                index.Add(estilo.index_dc);
                            }
                            else
                            {
                                if (estilo.tipo_empaque == 1 || estilo.tipo_empaque == 5)
                                {
                                    if (estilo.dc != "0")
                                    {
                                        index.Add(estilo.index_dc);
                                    }
                                    else
                                    {
                                        index_bp.Add(estilo.index_dc);
                                    }
                                }
                                else
                                {
                                    //if (estilo.tipo_empaque == 2 || estilo.tipo_empaque == 4){
                                    index.Add(estilo.index_dc);
                                    /* }else {*/
                                    /*if (estilo.tipo_empaque == 3){
                                        foreach (estilos e in estilo.assort.lista_estilos) { estilos_total++; estilos_normales++; }
                                    }*/
                                    // }*/                                    
                                }
                            }
                        }
                        index = index.Distinct().ToList();
                        index_bp = index_bp.Distinct().ToList();
                        foreach (int i in index_bp)
                        {
                            foreach (estilos estilo in tarimas.lista_estilos)
                            {
                                if (estilo.index_dc == i)
                                {
                                    estilos_total++;
                                    if (estilo.tipo == "EXT" || estilo.tipo == "DMG")
                                    {
                                        estilos_extras++;
                                    }
                                    else
                                    {
                                        estilos_normales++;
                                    }
                                }
                            }
                        }
                        foreach (int i in index)
                        {
                            int assort_contador = 0;
                            foreach (estilos estilo in tarimas.lista_estilos)
                            {
                                if (estilo.index_dc == i)
                                {
                                    if (estilo.tipo == "EXT" || estilo.tipo == "DMG")
                                    {
                                        estilos_extras++;
                                    }
                                    else
                                    {
                                        estilos_normales++;
                                        if (estilo.tipo_empaque == 3)
                                        {
                                            assort_contador++;
                                            foreach (estilos e in estilo.assort.lista_estilos) { estilos_total++; }
                                        }
                                    }
                                }
                            }
                            if (assort_contador == 0) { estilos_total++; }
                        }

                        if (estilos_normales != 0) { pallets++; }

                        ws.Cell(r, 1).Value = tarimas.id_tarima;
                        ws.Range(r, 1, (r + (estilos_total - 1)), 1).Merge();
                        foreach (Talla sizes in tallas) { sizes.total = 0; }
                        tallas_bpdc = tallas;
                        foreach (Talla sizes in tallas_bpdc) { sizes.total = 0; }
                        int estilos_capturados = 0;
                        foreach (int indice in index)
                        { //ppks y bp
                            int es_otro = 0;

                            int contador_index = 0; suma_estilo = 0; suma_cajas = 0;
                            foreach (Talla sizes in tallas_bpdc) { sizes.total = 0; sizes.ratio = 0; }
                            foreach (estilos estilo in tarimas.lista_estilos)
                            {
                                if (estilo.index_dc == indice && estilo.tipo_empaque == 3)
                                {
                                    suma_estilo = 0; suma_cajas = 0;
                                    // List<String> datos = new List<string>(); //CREO LISTA PARA LOS DATOS
                                    if (estilo.usado == 0)
                                    {
                                        if (estilo.tipo_empaque == 3 && estilo.tipo != "EXT" && estilo.tipo != "DMG")
                                        {
                                            int estilos_assort = 0, estilos_tarima = 0;
                                            foreach (estilos e in estilo.assort.lista_estilos) { estilos_assort++; }
                                            estilos_tarima += estilos_assort;
                                            int contador_assort = 0;
                                            //ESTILOS DEL ASSORT//ESTILOS DEL ASSORT
                                            foreach (estilos e in estilo.assort.lista_estilos)
                                            {//ESTILOS DEL ASSORT

                                                List<String> datos_a = new List<string>();
                                                datos_a.Add(estilo.number_po); //AGREGO EL PO NUMBER
                                                string tipo = " ";
                                                if (estilo.tipo != "NONE") { tipo += estilo.tipo; }
                                                if (estilo.label != "NONE") { tipo += " " + estilo.label; }
                                                datos_a.Add(tipo);
                                                //if (estilo.tipo != "NONE") { datos_a.Add(estilo.tipo); } //HASTA AQUÍ VA LA COLUMNA DE "TIPO"
                                                //else { datos_a.Add(" "); }//TIPO SI NO LLEVA NADA
                                                datos_a.Add(Regex.Replace(estilo.assort_nombre, @"\s+", " "));
                                                datos_a.Add(Regex.Replace(e.estilo + estilo.ext + item.siglas_cliente, @"\s+", ""));//-----------------------------------------------------------------------------------------------------
                                                datos_a.Add(Regex.Replace(e.color, @"\s+", " "));
                                                datos_a.Add(Regex.Replace(e.descripcion, @"\s+", " "));
                                                suma_estilo = 0; suma_cajas = 0;
                                                if (tiendas != 0) { datos_a.Add(e.store); }
                                                if (dc != 0) { datos_a.Add(e.dc); }//DATOS ANTES DE LAS CANTIDADES
                                                                                   //if (ppk != 0) {  datos_a.Add(" "); }//RATIO 
                                                string ratio_af = "";
                                                //if (e.tipo_empaque == 2 || e.tipo_empaque == 4 && e.tipo != "DMG" && e.tipo != "EXT"){
                                                contador = 0; string ppk_ratio = ""; total_ratio = 0;
                                                foreach (Talla t in tallas)
                                                {
                                                    foreach (ratio_tallas ratio in e.lista_ratio)
                                                    {
                                                        if (ratio.id_talla == t.id_talla)
                                                        {
                                                            contador++; ppk_ratio += ratio.ratio;
                                                        }
                                                    }
                                                    if (contador < ((e.lista_ratio).Count)) { ppk_ratio += "-"; }
                                                }
                                                ratio_af = ppk_ratio;
                                                // }
                                                if (ppk != 0) { datos_a.Add(ratio_af); }
                                                int aa = 0;
                                                foreach (Talla sizes in tallas)
                                                {//CANTIDADES
                                                    int existio_a = 0;
                                                    foreach (ratio_tallas ratio in e.lista_ratio)
                                                    {
                                                        if (sizes.id_talla == ratio.id_talla)
                                                        {
                                                            existio_a++;
                                                            datos_a.Add(Convert.ToString(ratio.ratio * estilo.boxes));//SE MULTIPLICA POR EL TOTAL DE CARTONES
                                                            suma_estilo += (ratio.ratio * estilo.boxes);
                                                            //suma_cajas = estilo.boxes;
                                                            sumas_tallas[aa] += (ratio.ratio * estilo.boxes);
                                                        }
                                                    }
                                                    if (existio_a == 0) { datos_a.Add(" "); }
                                                    aa++;
                                                }
                                                int ct = contador_tallas;
                                                if (contador_assort == 0) { suma_cajas = estilo.boxes; }
                                                sumas_tallas[ct] += suma_estilo;
                                                sumas_tallas[ct + 1] += suma_cajas;
                                                datos_a.Add(Convert.ToString(suma_estilo));
                                                datos_a.Add(Convert.ToString(suma_cajas));
                                                celdas_estilos.Add(datos_a.ToArray());
                                                estilos_capturados++;
                                                if (contador_assort == 0)
                                                {//PONER LAS CAJAS / CARTONES DE EL ASSORT
                                                    ws.Cell(r + estilos_capturados - 1, (contador_cabeceras - 1)).Value = estilo.boxes;
                                                    ws.Range(r + estilos_capturados - 1, (contador_cabeceras - 1), ((r + estilos_capturados - 1) + estilos_assort - 1), (contador_cabeceras - 1)).Merge();
                                                    contador_assort++;
                                                    //sumas_tallas[ct + 1] += estilo.boxes;
                                                    ws.Cell(r + estilos_capturados - 1, 4).Value = Regex.Replace(estilo.assort_nombre, @"\s+", " ");
                                                    ws.Range(r + estilos_capturados - 1, 4, ((r + estilos_capturados - 1) + (estilos_assort - 1)), 4).Merge();
                                                }

                                            }//ESTILOS DEL ASSORT
                                        }
                                    }
                                }
                            }

                            string ponum_f = "", tipo_f = "", estilo_f = "", descripcion_f = "", ratio_f = "", color_f = "", store_f = "", dc_f = "", assort_f = "";
                            foreach (estilos estilo in tarimas.lista_estilos)
                            {
                                if (estilo.index_dc == indice && estilo.tipo_empaque != 3)
                                {
                                    es_otro++;
                                    ponum_f = estilo.number_po;
                                    string tipo = " ";
                                    if (estilo.tipo != "NONE") { tipo += estilo.tipo; }
                                    if (estilo.label != "NONE") { tipo += " " + estilo.label; }
                                    tipo_f = tipo;
                                    estilo_f = (estilo.estilo).Trim();
                                    color_f = (estilo.color).Trim();
                                    descripcion_f = (estilo.descripcion).Trim();
                                    if (tiendas != 0) { store_f = estilo.store; }
                                    if (dc != 0) { dc_f = estilo.dc; }
                                    if (estilo.tipo_empaque == 2 || estilo.tipo_empaque == 4 && estilo.tipo != "DMG" && estilo.tipo != "EXT")
                                    {
                                        contador = 0; string ppk_ratio = ""; total_ratio = 0;
                                        foreach (Talla t in tallas)
                                        {
                                            foreach (ratio_tallas ratio in estilo.lista_ratio)
                                            {
                                                if (ratio.id_talla == t.id_talla)
                                                {
                                                    contador++; ppk_ratio += ratio.ratio;
                                                }//if
                                            } //tallas                                               
                                            if (contador < ((estilo.lista_ratio).Count)) { ppk_ratio += "-"; }
                                        }//ratio
                                        ratio_f = ppk_ratio;
                                    }
                                    if (estilo.tipo == "DMG" || estilo.tipo == "EXT")
                                    {
                                        int ii = 0;
                                        foreach (Talla sizes in tallas_bpdc)
                                        {
                                            if (estilo.id_talla == sizes.id_talla)
                                            {
                                                sizes.total = estilo.boxes;
                                                suma_estilo += estilo.boxes;
                                                suma_cajas = estilo.repeticiones;
                                                sumas_tallas[ii] += estilo.boxes;
                                            }
                                            ii++;
                                        }
                                    }else{
                                        if (estilo.tipo_empaque == 1 || estilo.tipo_empaque == 5){
                                            int ii = 0;
                                            foreach (Talla sizes in tallas_bpdc){
                                                if (estilo.id_talla == sizes.id_talla){
                                                    //REVISAR SI TIENE DC O NO
                                                    if (estilo.dc != "0"){
                                                        int piezas = 0;
                                                        if (estilo.tipo_empaque == 1) { piezas = ds.buscar_piezas_empaque_bull(estilo.id_po_summary, estilo.id_talla); }
                                                        if (estilo.tipo_empaque == 5) { piezas = ds.buscar_piezas_empaque_bulls(estilo.id_po_summary, estilo.id_talla, estilo.packing_name); }
                                                        sizes.total = estilo.boxes * piezas;                                                        
                                                        sumas_tallas[ii] += estilo.boxes * piezas;
                                                        suma_estilo += estilo.boxes * piezas;
                                                        suma_cajas += estilo.boxes;
                                                    }else{
                                                        sizes.total = estilo.boxes;
                                                        sumas_tallas[ii] += estilo.boxes;
                                                        suma_estilo += estilo.boxes;
                                                        if (estilo.sobrantes == 1){
                                                            suma_cajas += estilo.repeticiones;
                                                        }else{
                                                            if (estilo.tipo_empaque == 1){
                                                                suma_cajas += (estilo.boxes / ds.buscar_piezas_empaque_bull(estilo.id_po_summary, estilo.id_talla));
                                                            }
                                                            if (estilo.tipo_empaque == 5){
                                                                suma_cajas += (estilo.boxes / ds.buscar_piezas_empaque_bulls(estilo.id_po_summary, estilo.id_talla, estilo.packing_name));
                                                            }
                                                        }
                                                    }//else
                                                    //REVISAR SI TIENE DC O NO
                                                }//if talla
                                                ii++;
                                            }
                                        }
                                        if (estilo.tipo_empaque == 2 || estilo.tipo_empaque == 4)
                                        {
                                            int ii = 0;
                                            foreach (Talla sizes in tallas_bpdc)
                                            {
                                                foreach (ratio_tallas ratio in estilo.lista_ratio)
                                                {
                                                    if (sizes.id_talla == ratio.id_talla)
                                                    {
                                                        sizes.total = (ratio.ratio * estilo.boxes);
                                                        suma_estilo += (ratio.ratio * estilo.boxes);
                                                        sumas_tallas[ii] += (ratio.ratio * estilo.boxes);
                                                        suma_cajas = estilo.boxes;
                                                    }
                                                }
                                                ii++;
                                            }
                                        }
                                    }//else                                    
                                }// IF INDICE
                            }//ESTILOS
                            if (es_otro != 0)
                            {
                                List<String> datos = new List<string>();
                                datos.Add(ponum_f);
                                datos.Add(tipo_f);
                                if (ass != 0) { datos.Add(" "); }
                                datos.Add(estilo_f);
                                datos.Add(color_f);
                                datos.Add(descripcion_f);
                                if (tiendas != 0) { datos.Add(store_f); }
                                if (dc != 0) { datos.Add(dc_f); }
                                if (ppk != 0) { datos.Add(ratio_f); }
                                int i = 0;
                                foreach (Talla sizes in tallas_bpdc)
                                {
                                    if (sizes.total == 0)
                                    {
                                        datos.Add(" ");
                                    }
                                    else
                                    {
                                        datos.Add(Convert.ToString(sizes.total));
                                    }
                                    i++;
                                }
                                datos.Add(Convert.ToString(suma_estilo));
                                datos.Add(Convert.ToString(suma_cajas));
                                sumas_tallas[i] += suma_estilo; i++;
                                sumas_tallas[i] += suma_cajas;
                                estilos_capturados++;
                                celdas_estilos.Add(datos.ToArray());
                                //r++;
                            }
                        }//INDICE

                        foreach (Talla sizes in tallas_bpdc) { sizes.total = 0; }

                        foreach (int indice in index_bp)
                        { //BP TALLA / FILA                            
                            int contador_index = 0; suma_estilo = 0; suma_cajas = 0;
                            string ponum_f = "", tipo_f = "", estilo_f = "", descripcion_f = "", ratio_f = "", color_f = "", store_f = "", dc_f = "";
                            foreach (estilos estilo in tarimas.lista_estilos)
                            {
                                if (estilo.index_dc == indice)
                                {
                                    List<String> datos = new List<string>();
                                    datos.Add(estilo.number_po);
                                    string tipo = " ";
                                    if (estilo.tipo != "NONE") { tipo += estilo.tipo; }
                                    if (estilo.label != "NONE") { tipo += " " + estilo.label; }
                                    datos.Add(tipo);
                                    if (ass != 0) { datos.Add(" "); }
                                    datos.Add((estilo.estilo).Trim());
                                    datos.Add((estilo.color).Trim());
                                    datos.Add((estilo.descripcion).Trim());
                                    if (tiendas != 0) { datos.Add(estilo.store); }
                                    if (dc != 0) { if (estilo.dc != "0") { datos.Add(estilo.dc); } else { datos.Add(" "); } }
                                    if (ppk != 0) { datos.Add(" "); }
                                    int ii = 0;
                                    foreach (Talla sizes in tallas_bpdc)
                                    {
                                        if (estilo.id_talla == sizes.id_talla)
                                        {
                                            datos.Add(Convert.ToString(estilo.boxes));
                                            suma_estilo = estilo.boxes;
                                            sumas_tallas[ii] += estilo.boxes;
                                            if (estilo.sobrantes == 1)
                                            {
                                                suma_cajas = estilo.repeticiones;
                                            }
                                            else
                                            {
                                                if (estilo.tipo_empaque == 1)
                                                {
                                                    suma_cajas = (estilo.boxes / ds.buscar_piezas_empaque_bull(estilo.id_po_summary, estilo.id_talla));
                                                }
                                                if (estilo.tipo_empaque == 5)
                                                {
                                                    suma_cajas = (estilo.boxes / ds.buscar_piezas_empaque_bulls(estilo.id_po_summary, estilo.id_talla, estilo.packing_name));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            datos.Add(" ");
                                        }
                                        ii++;
                                    }
                                    datos.Add(Convert.ToString(suma_estilo));
                                    datos.Add(Convert.ToString(suma_cajas));
                                    sumas_tallas[ii] += suma_estilo; ii++;
                                    sumas_tallas[ii] += suma_cajas;
                                    estilos_capturados++;
                                    celdas_estilos.Add(datos.ToArray());
                                }// IF INDICE
                            }//ESTILOS
                        }//INDICE


                        ws.Cell(r, 2).Value = celdas_estilos;//// <-------------THIS!!
                        if (estilos_normales != 0)
                        {
                            ws.Cell(r, contador_cabeceras).Value = "1";//PALLET
                        }
                        else
                        {
                            ws.Cell(r, contador_cabeceras).Value = "*";//PALLET
                        }
                        ws.Range(r, contador_cabeceras, (r + estilos_total - 1), contador_cabeceras).Merge();
                        r = r + (estilos_total);
                    }//************T*A*R*I*M*A*S*******************************************************************
                    //*************************************************************************T*A*R*I*M*A*S************************************************************************************
                    //***************************************************************************T*A*R*I*M*A*S********************************************************************************************************


                    /****************************************************************************************************************************************************************************************/
                    /****************************************************************************************************************************************************************************************/
                    /****************************************************************************************************************************************************************************************/
                    contador = 0;
                    string descripcion_final = "";
                    /*estilos desc = new estilos();
                    desc.descripcion = estilo.des
                    lista_descripciones_finales.Add();*/
                    foreach (Tarima tarimas in item.lista_tarimas){
                        foreach (estilos estilo in tarimas.lista_estilos){
                            if (estilo.tipo_empaque != 3)
                            {
                                bool isEmpty = !lista_descripciones_finales.Any();
                                if (isEmpty)
                                {
                                    estilos desc = new estilos();
                                    desc.id_estilo = estilo.id_estilo;
                                    desc.descripcion_final = estilo.descripcion_final;
                                    lista_descripciones_finales.Add(desc);
                                    descripcion_final += Regex.Replace(estilo.descripcion_final, @"\s+", " ") + " ";
                                }
                                else
                                {
                                    int existencia = 0;
                                    foreach (estilos e in lista_descripciones_finales)
                                    {
                                        if (e.descripcion_final == estilo.descripcion_final)
                                        {
                                            existencia++;
                                        }
                                    }
                                    if (existencia == 0)
                                    {
                                        estilos desc = new estilos();
                                        desc.id_estilo = estilo.id_estilo;
                                        desc.descripcion_final = estilo.descripcion_final;
                                        lista_descripciones_finales.Add(desc);
                                        descripcion_final += Regex.Replace(estilo.descripcion_final, @"\s+", " ") + " ";
                                    }
                                }
                            }
                            else
                            {//AQUI ASSORT
                                foreach (estilos ee in estilo.assort.lista_estilos) {
                                    bool isEmpty = !lista_descripciones_finales.Any();
                                    if (isEmpty){
                                        estilos desc = new estilos();
                                        desc.id_estilo = ee.id_estilo;
                                        desc.descripcion_final = ee.descripcion_final;
                                        lista_descripciones_finales.Add(desc);
                                        descripcion_final += Regex.Replace(ee.descripcion_final, @"\s+", " ") + " ";
                                    }else{
                                        int existencia = 0;
                                        foreach (estilos e in lista_descripciones_finales){
                                            if (e.descripcion_final == ee.descripcion_final){
                                                existencia++;
                                            }
                                        }
                                        if (existencia == 0){
                                            estilos desc = new estilos();
                                            desc.id_estilo = ee.id_estilo;
                                            desc.descripcion_final = ee.descripcion_final;
                                            lista_descripciones_finales.Add(desc);
                                            descripcion_final += Regex.Replace(ee.descripcion_final, @"\s+", " ") + " ";
                                        }
                                    }
                                }
                            }
                        }
                    }

                    ws.Cell(r, 1).Value = descripcion_final;
                    ws.Cell(r, 1).Style.Font.Bold = true;
                    ws.Cell(r, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(r, 1, r, 4).Merge();
                    ws.Range(r, 1, r, 4).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);

                    ws.Cell(r, 5).Value = "TOTAL";
                    ws.Cell(r, 5).Style.Font.Bold = true;
                    ws.Cell(r, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    c = 6;
                    if (dc != 0) { c++; }
                    if (ppk != 0) { c++; }//POR EL RATIO
                    if (ass != 0) { c++; }
                    if (tiendas != 0) { c++; }
                    for (int i = 12; i < r; i++)
                    {
                        for (int j = 1; j <= c; j++)
                        {
                            ws.Column(c).AdjustToContents(r, c);
                        }
                    }
                    ws.Range(r, 5, r, c).Merge();
                    ws.Range(r, 5, r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    c++;

                    for (int i = 0; i < contador_tallas + 2; i++)
                    {
                        ws.Cell(r, c).Value = sumas_tallas[i];
                        ws.Cell(r, c).Style.Font.Bold = true;
                        ws.Cell(r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                        ws.Cell(r, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        c++;
                    }
                    ws.Cell(r, c).Value = pallets;
                    ws.Cell(r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    ws.Cell(r, c).Style.Font.Bold = true;
                    ws.Cell(r, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    //for (int zz = 1; zz <= 10; zz++) { ws.Columns(zz.ToString()).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }
                    filas = r;
                    
                    ws.Column(2).AdjustToContents(12, 2);
                    ws.Column(3).AdjustToContents(12, 3);
                    ws.Column(4).AdjustToContents(12, 4);
                    for (int i = 12; i <= r; i++)
                    {
                        for (int j = 1; j <= c; j++)
                        {
                            ws.Cell(i, j).Style.Font.FontSize = 8;
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
                    for (int zz =(r+1); zz <= (r+30); zz++) { ws.Row(zz).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }

                    filas += 2;
                    ws.Range(filas, 1, (filas + 2), 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(filas, 1, (filas + 2), 1).Style.Font.Bold = true;
                    ws.Range(filas, 7, (filas + 2), 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(filas, 7, (filas + 2), 7).Style.Font.Bold = true;
                    ws.Range(filas, 2, (filas + 2), 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(filas, 8, (filas + 2), 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(filas, 1).Value = "DRIVER NAME:";
                    ws.Cell(filas, 2).Value = item.conductor.driver_name;
                    ws.Cell(filas, 7).Value = "SHIPPING MANAGER:";
                    ws.Cell(filas, 8).Value = item.shipping_manager;
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 1).Value = "TRAILER/PLATES:";
                    ws.Cell(filas, 2).Value = item.conductor.tractor + "/" + item.conductor.plates;
                    ws.Cell(filas, 7).Value = "SEAL:";
                    ws.Cell(filas, 8).SetValue(item.seal);
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 1).Value = "CONTAINER/PLATES:";
                    ws.Cell(filas, 2).Value = item.contenedor.eco + "/" + item.contenedor.plates;
                    ws.Cell(filas, 7).Value = "REPLACEMENT:";
                    ws.Cell(filas, 8).SetValue(item.replacement);
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 3).Value = "DOCUMENTO CONTROLADO. ÚNICAMENTE SE PUEDE MODIFICAR POR EL SUPERVISOR DE SHIPPING";
                    ws.Cell(filas, 3).Style.Font.FontColor = XLColor.FromArgb(100, 100, 100);
                    filas += 2;
                    columnas = 2;

                    var porcentajes = new List<String[]>();
                    List<String> p = new List<string>();
                    p.Add("ORIGIN"); p.Add("QTY"); p.Add("%");
                    porcentajes.Add(p.ToArray());
                    List<Fabricantes> totales_paises_estilo = new List<Fabricantes>();
                    List<Fabricantes> totales_paises = new List<Fabricantes>();
                    List<int> lista_estilos_stag = new List<int>();
                    int add = 0, total_paises = 0, iguales;
                    foreach (Tarima tarimas in item.lista_tarimas)
                    {
                        foreach (estilos e in tarimas.lista_estilos)
                        {
                            if (e.tipo_empaque != 3)
                            {
                                lista_estilos_stag.Add(e.id_po_summary);
                            }
                            else
                            {
                                foreach (estilos ee in e.assort.lista_estilos)
                                {
                                    lista_estilos_stag.Add(ee.id_po_summary);
                                }
                            }
                        }
                    }
                    lista_estilos_stag = lista_estilos_stag.Distinct().ToList();
                    totales_paises_estilo = ds.buscar_paises_estilos_stag_recibos(lista_estilos_stag);
                    foreach (Fabricantes fa in totales_paises_estilo)
                    {
                        iguales = 0;
                        if (add == 0)
                        {
                            Fabricantes nf = new Fabricantes();
                            nf.cantidad = fa.cantidad;
                            nf.pais = fa.pais;
                            nf.percent = fa.percent;
                            nf.id = fa.id;
                            totales_paises.Add(nf);
                            add++;
                        }
                        else
                        {
                            foreach (Fabricantes f in totales_paises.ToList())
                            {
                                if (f.pais == fa.pais && f.percent == fa.percent)
                                {
                                    f.cantidad += fa.cantidad;
                                    iguales++;
                                }
                            }
                            if (iguales == 0)
                            {
                                Fabricantes nf = new Fabricantes();
                                nf.cantidad = fa.cantidad;
                                nf.pais = fa.pais;
                                nf.id = fa.id;
                                nf.percent = fa.percent;
                                totales_paises.Add(nf);
                            }
                            add++;
                        }
                    }

                    foreach (Fabricantes f in totales_paises) { total_paises += f.cantidad; }
                    foreach (Fabricantes f in totales_paises)
                    {
                        double z = ((Convert.ToDouble(f.cantidad) * 100) / Convert.ToDouble(total_paises));
                        f.porcentaje = Math.Round(z, MidpointRounding.ToEven);
                    }
                    List<Fabricantes> totales_paises_envio = new List<Fabricantes>();
                    foreach (Fabricantes f in totales_paises)
                    {
                        Fabricantes nf = new Fabricantes();
                        double x = ((Convert.ToDouble(sumas_tallas[sumas_tallas.Length - 2]) * Convert.ToDouble(f.cantidad)) / Convert.ToDouble(total_paises));
                        nf.cantidad = Convert.ToInt32(Math.Round(x, MidpointRounding.ToEven));
                        nf.pais = f.pais;
                        nf.percent = f.percent;
                        totales_paises_envio.Add(nf);
                    }
                    foreach (Fabricantes f in totales_paises_envio)
                    {
                        porcentajes.Add(new string[] { f.pais, (f.cantidad).ToString(), f.percent });
                    }
                    ws.Cell(filas, 2).Value = "%";
                    ws.Cell(filas, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    filas++;



                    ws.Cell(filas, 2).Value = porcentajes;

                }
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

        public void crear_excel_fantasy(List<Pk> lista)
        {
            string clave_packing = "";
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            using (XLWorkbook libro_trabajo = new XLWorkbook())
            {
                string archivo = "";
                int estilos_total = 0, rows = 0, tarima_contador = 0, total_ratio, contador, r, c, total_cajas = 0, filas = 0, columnas = 0, tallas_id;
                List<Talla> tallas = new List<Talla>();
                List<Talla> tallas_bpdc = new List<Talla>();

                var ws = libro_trabajo.Worksheets.Add("PK");
                foreach (Pk item in lista)
                {
                    //item.tipo_empaque = 2;                    
                    clave_packing = item.packing;
                    string[] nombre_archivo = (item.packing).Split('-');
                    archivo = " PK" + nombre_archivo[0] + " ";
                    //item.parte = consultas.AddOrdinal(Convert.ToInt32(item.parte)) + " Part";
                    /*****INICIO CON DIRECCIONES, LOGO, ETC******/
                    ws.Cell("A2").Value = "FORTUNE FASHIONS BAJA, S.R.L. DE C.V.";
                    ws.Cell("A3").Value = "CALLE TORTUGA No 313-A";
                    ws.Cell("A4").Value = "MANEADERO CP 22790";
                    ws.Cell("A5").Value = "BAJA CALIFORNIA";
                    ws.Style.Font.FontSize = 8;

                    ws.Range("A2:A10").Style.Font.Bold = true;
                    ws.Range("A7:A10").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    //DIRECCIÓN DE ORIGEN
                    ws.Cell("A7").Value = "CUSTOMER: ";
                    ws.Cell("B7").Value = item.customer;

                    int ex_label = ds.contar_labels(item.id_packing_list);
                    if (ex_label != 0)
                    {
                        List<Labels> lista_etiquetas = new List<Labels>();
                        lista_etiquetas = ds.obtener_etiquetas(item.id_packing_list);
                        ws.Cell("A8").Value = "P.O.: ";
                        string label = Regex.Replace(item.pedido, @"\s+", " ") + "(PO# ";
                        foreach (Labels l in lista_etiquetas) { label += " " + l.label; }
                        if (ex_label == 1)
                        {
                            label += " )" + " (With UCC Labels) " + item.parte;
                        }
                        else
                        {
                            label += " )" + " (With TPM Labels) " + item.parte;
                        }
                        //ws.Cell("B8").Value = label;
                        //archivo += " "+label;
                    }
                    else
                    {
                        ws.Cell("A8").Value = "P.O.: ";
                        // ws.Cell("B8").Value = Regex.Replace(item.pedido, @"\s+", " ") + "(Without UCC Labels) " + item.parte;
                        archivo += " " + Regex.Replace(item.pedido, @"\s+", " ") + "(Without UCC Labels) " + item.parte;
                    }

                    ws.Cell("B8").Value = item.nombre_archivo;
                    //archivo = item.packing;.
                    archivo = " PK " + nombre_archivo[0] + " ";
                    archivo += " " + item.nombre_archivo;

                    ws.Cell("A9").Value = "RETAILER: ";
                    ws.Cell("B9").Value = item.customer_po;
                    ws.Range("B2:B10").Style.Font.Bold = true;
                    //IMAGEN AL CENTRO
                    ws.Range(1, 7, 1, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    var imagePath = Server.MapPath("/") + "/Content/img/LOGOFORTUNEEXCEL.png";
                    //var imagePath = @"C:\Users\melissa\source\repos\FortuneSys----\FortuneSystem\Content\img\LOGO FORTUNE.png";
                    //var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1")).Scale(0.30);
                    var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1"));
                    //PK ABAJO DE LA IMAGEN
                    ws.Cell("D7").Value = "PK: ";
                    ws.Cell("D7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell("D7").Style.Font.Bold = true;
                    ws.Cell("E7").Value = item.packing;
                    ws.Cell("E7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range("E7:F7").Merge();
                    ws.Range("E7", "F7").Style.Font.Bold = true;
                    ws.Range("D7", "F7").Style.Font.FontSize = 15;
                    ws.Range("E7:F7").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range("E7:F7").Style.Border.BottomBorderColor = XLColor.Black;
                    //DIRECCION DE DESTINO
                    ws.Cell("L2").Value = "SHIP TO: ";
                    ws.Cell("L3").Value = item.destino.nombre;
                    ws.Cell("L4").Value = item.destino.direccion;
                    ws.Cell("L5").Value = item.destino.ciudad + " " + item.destino.zip;
                    ws.Cell("L8").Value = "DATE:" + item.fecha;
                    ws.Range("L2", "L10").Style.Font.Bold = true;
                    var columna_a = ws.Range("A2", "A10");
                    ws.Rows("6").Height = 30;
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    int contador_cabeceras = 0, contador_tallas = 0, pallets = 0, tiendas = 0, dc = 0, ppk = 0, bp = 0, ass = 0;
                    int suma_estilo, suma_cajas;
                    List<estilos> lista_descripciones_finales = new List<estilos>();
                    var headers = new List<String[]>();
                    List<String> titulos = new List<string>();//=  "ID", "P.O. NUM", "TYPE", "COLOR", "DESCRIPTION";
                    titulos.Add("ID"); titulos.Add("P.O. NUM"); titulos.Add("TYPE");
                    foreach (Tarima t in item.lista_tarimas)
                    { //REVISAN TIPOS DE EMPAQUE, DATOS, DC,ETC
                        foreach (estilos e in t.lista_estilos)
                        {
                            if (e.store != "N/A" && e.store != "NA") { tiendas++; }
                            if (e.dc != "0") { dc++; }
                            if (e.tipo_empaque == 1 || e.tipo_empaque == 5) { bp++; }
                            if (e.tipo_empaque == 2 || e.tipo_empaque == 4) { ppk++; }
                            if (e.tipo_empaque == 3) { ass++; }
                        }
                    }
                    if (ass != 0) { titulos.Add("ASSORTMENT"); }
                    titulos.Add("STYLE"); titulos.Add("COLOR"); titulos.Add("DESCRIPTION");
                    if (tiendas != 0) { titulos.Add("STORE"); }
                    if (dc != 0) { titulos.Add("DC"); }
                    if (ppk != 0) { titulos.Add("RATIO"); }
                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                    foreach (Tarima t in item.lista_tarimas)
                    {
                        foreach (estilos e in t.lista_estilos)
                        {
                            if (e.tipo_empaque != 3)
                            {
                                foreach (ratio_tallas ra in e.lista_ratio)
                                {
                                    bool isEmpty = !tallas.Any();
                                    if (isEmpty)
                                    {
                                        Talla ta = new Talla();
                                        ta.id_talla = ra.id_talla;
                                        ta.talla = ra.talla;
                                        tallas.Add(ta);

                                    }
                                    else
                                    {
                                        int existe = 0;
                                        foreach (Talla sizes in tallas)
                                        {
                                            if (sizes.id_talla == ra.id_talla) { existe++; }
                                        }
                                        if (existe == 0)
                                        {
                                            Talla ta = new Talla();
                                            ta.id_talla = ra.id_talla;
                                            ta.talla = ra.talla;
                                            tallas.Add(ta);

                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (estilos ea in e.assort.lista_estilos)
                                {
                                    foreach (ratio_tallas ras in ea.lista_ratio)
                                    {
                                        bool isEmpty = !tallas.Any();
                                        if (isEmpty)
                                        {
                                            Talla ta = new Talla();
                                            ta.id_talla = ras.id_talla;
                                            ta.talla = ras.talla;
                                            tallas.Add(ta);

                                        }
                                        else
                                        {
                                            int existe = 0;
                                            foreach (Talla sizes in tallas)
                                            {
                                                if (sizes.id_talla == ras.id_talla) { existe++; }
                                            }
                                            if (existe == 0)
                                            {
                                                Talla ta = new Talla();
                                                ta.id_talla = ras.id_talla;
                                                ta.talla = ras.talla;
                                                tallas.Add(ta);

                                            }
                                        }
                                    }
                                }
                            }

                        }

                        //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                    } //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS<-----TALLAS
                    tallas = ds.obtener_tallas_pk(tallas);
                    List<int> tallas_id_temporal = new List<int>();
                    contador_tallas = 0;
                    foreach (Talla sizes in tallas)
                    {
                        titulos.Add(sizes.talla); //AQUI AGREGO LAS TALLAS A LA CABECERA
                        tallas_id_temporal.Add(sizes.id_talla);
                        contador_tallas++;
                    }
                    titulos.Add("PCS"); titulos.Add("BXS"); titulos.Add("PALLETS");
                    foreach (string s in titulos) { contador_cabeceras++; }
                    headers.Add(titulos.ToArray());
                    int total_titulos = (titulos.ToArray()).Length;
                    for (int zz = 1; zz <= 10; zz++) { ws.Row(zz).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }
                    ws.Cell(11, 1).Value = headers;
                    ws.Column(2).AdjustToContents();
                    ws.Column(5).AdjustToContents();
                    for (int i = 1; i <= total_titulos; i++)
                    {
                        ws.Cell(11, i).Style.Font.Bold = true;
                        ws.Cell(11, i).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                        ws.Cell(11, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(11, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.LeftBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.RightBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.TopBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.BottomBorderColor = XLColor.Black;
                    }
                    ws.Rows("6").Height = 30;
                    /**********************************************************************************************************************************************************************************************/
                    /***********************C*A*B*E*C*E*R*A*S**D*E**L*A**T*A*B*L*A*********************************************************************************************************************************/
                    //int[] tallas_comparacion = tallas_id_temporal.ToArray(); 
                    //foreach (Tarima tarimas in item.lista_tarimas) { pallets++; }
                    int[] sumas_tallas = new int[contador_tallas + 2];
                    for (int i = 0; i < contador_tallas + 2; i++) { sumas_tallas[i] = 0; }
                    r = 12;
                    int estilos_extras = 0, estilos_normales = 0;
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************
                    foreach (Tarima tarimas in item.lista_tarimas)
                    {
                        estilos_total = 0;
                        estilos_extras = 0; estilos_normales = 0;
                        var celdas_estilos_i = new List<String[]>();
                        var celdas_estilos = new List<String[]>();
                        List<int> index = new List<int>();//BUSCO LOS INDEX DE LOS DC
                        //List<int> index_bp = new List<int>();//BUSCO LOS INDEX DE LOS DC

                        foreach (estilos estilo in tarimas.lista_estilos) { index.Add(estilo.index_dc); }
                        index = index.Distinct().ToList();
                        foreach (int i in index)
                        {
                            int assort_contador = 0;
                            foreach (estilos estilo in tarimas.lista_estilos)
                            {
                                if (estilo.index_dc == i)
                                {
                                    if (estilo.tipo == "EXT" || estilo.tipo == "DMG") { estilos_extras++; }
                                    else
                                    {
                                        estilos_normales++;
                                        if (estilo.tipo_empaque == 3)
                                        {
                                            assort_contador++;
                                            foreach (estilos e in estilo.assort.lista_estilos) { estilos_total++; }
                                        }
                                    }
                                }
                            }
                            if (assort_contador == 0) { estilos_total++; }
                        }
                        if (estilos_normales != 0) { pallets++; }

                        ws.Cell(r, 1).Value = tarimas.id_tarima;
                        ws.Range(r, 1, (r + (estilos_total - 1)), 1).Merge();
                        foreach (Talla sizes in tallas) { sizes.total = 0; }
                        tallas_bpdc = tallas;
                        foreach (Talla sizes in tallas_bpdc) { sizes.total = 0; }
                        int estilos_capturados = 0;
                        foreach (int indice in index)
                        { //ppks y bp
                            int es_otro = 0;

                            int contador_index = 0; suma_estilo = 0; suma_cajas = 0;
                            foreach (Talla sizes in tallas_bpdc) { sizes.total = 0; sizes.ratio = 0; }
                            foreach (estilos estilo in tarimas.lista_estilos)
                            {

                                if (estilo.index_dc == indice && estilo.tipo_empaque == 3)
                                {
                                    suma_estilo = 0; suma_cajas = 0;
                                    // List<String> datos = new List<string>(); //CREO LISTA PARA LOS DATOS
                                    if (estilo.usado == 0)
                                    {
                                        if (estilo.tipo_empaque == 3 && estilo.tipo != "EXT" && estilo.tipo != "DMG")
                                        {
                                            int estilos_assort = 0, estilos_tarima = 0;
                                            foreach (estilos e in estilo.assort.lista_estilos) { estilos_assort++; }
                                            estilos_tarima += estilos_assort;
                                            int contador_assort = 0;
                                            //ESTILOS DEL ASSORT//ESTILOS DEL ASSORT
                                            foreach (estilos e in estilo.assort.lista_estilos)
                                            {//ESTILOS DEL ASSORT
                                                List<String> datos_a = new List<string>();
                                                datos_a.Add(estilo.number_po); //AGREGO EL PO NUMBER
                                                string tipo = " ";
                                                if (estilo.tipo != "NONE") { tipo += estilo.tipo; }
                                                if (estilo.label != "NONE") { tipo += " " + estilo.label; }
                                                datos_a.Add(tipo);
                                                //if (estilo.tipo != "NONE") { datos_a.Add(estilo.tipo); } //HASTA AQUÍ VA LA COLUMNA DE "TIPO"
                                                //else { datos_a.Add(" "); }//TIPO SI NO LLEVA NADA
                                                datos_a.Add(Regex.Replace(estilo.assort_nombre, @"\s+", " "));
                                                datos_a.Add(Regex.Replace(e.estilo + estilo.ext + item.siglas_cliente, @"\s+", ""));//-----------------------------------------------------------------------------------------------------
                                                datos_a.Add(Regex.Replace(e.color, @"\s+", " "));
                                                datos_a.Add(Regex.Replace(e.descripcion, @"\s+", " "));
                                                suma_estilo = 0; suma_cajas = 0;
                                                if (tiendas != 0) { datos_a.Add(e.store); }
                                                if (dc != 0) { if (e.dc != "0") { datos_a.Add(e.dc); } else { datos_a.Add(" "); } }//DATOS ANTES DE LAS CANTIDADES
                                                                                                                                   //if (ppk != 0) {  datos_a.Add(" "); }//RATIO 
                                                string ratio_af = "";
                                                //if (e.tipo_empaque == 2 || e.tipo_empaque == 4 && e.tipo != "DMG" && e.tipo != "EXT"){
                                                contador = 0; string ppk_ratio = ""; total_ratio = 0;
                                                foreach (Talla t in tallas)
                                                {
                                                    foreach (ratio_tallas ratio in e.lista_ratio)
                                                    {
                                                        if (ratio.id_talla == t.id_talla)
                                                        {
                                                            contador++; ppk_ratio += ratio.ratio;
                                                        }
                                                        //if (contador < total_ratio) { ppk_ratio += "-"; }

                                                    }
                                                    if (contador < ((e.lista_ratio).Count)) { ppk_ratio += "-"; }
                                                }
                                                ratio_af = ppk_ratio;
                                                // }
                                                if (ppk != 0) { datos_a.Add(ratio_af); }
                                                int aa = 0;
                                                foreach (Talla sizes in tallas)
                                                {//CANTIDADES
                                                    int existio_a = 0;
                                                    foreach (ratio_tallas ratio in e.lista_ratio)
                                                    {
                                                        if (sizes.id_talla == ratio.id_talla)
                                                        {
                                                            existio_a++;
                                                            datos_a.Add(Convert.ToString(ratio.ratio * estilo.boxes));//SE MULTIPLICA POR EL TOTAL DE CARTONES
                                                            suma_estilo += (ratio.ratio * estilo.boxes);
                                                            //suma_cajas = estilo.boxes;
                                                            sumas_tallas[aa] += (ratio.ratio * estilo.boxes);
                                                        }
                                                    }
                                                    if (existio_a == 0) { datos_a.Add(" "); }
                                                    aa++;
                                                }
                                                int ct = contador_tallas;
                                                if (contador_assort == 0) { suma_cajas = estilo.boxes; }
                                                sumas_tallas[ct] += suma_estilo;
                                                sumas_tallas[ct + 1] += suma_cajas;
                                                datos_a.Add(Convert.ToString(suma_estilo));
                                                datos_a.Add(Convert.ToString(suma_cajas));
                                                celdas_estilos.Add(datos_a.ToArray());
                                                estilos_capturados++;
                                                if (contador_assort == 0)
                                                {//PONER LAS CAJAS / CARTONES DE EL ASSORT
                                                    ws.Cell(r + estilos_capturados - 1, (contador_cabeceras - 1)).Value = estilo.boxes;
                                                    ws.Range(r + estilos_capturados - 1, (contador_cabeceras - 1), ((r + estilos_capturados - 1) + estilos_assort - 1), (contador_cabeceras - 1)).Merge();
                                                    contador_assort++;
                                                    //sumas_tallas[ct + 1] += estilo.boxes;
                                                    ws.Cell(r + estilos_capturados - 1, 4).Value = Regex.Replace(estilo.assort_nombre, @"\s+", " ");
                                                    ws.Range(r + estilos_capturados - 1, 4, ((r + estilos_capturados - 1) + (estilos_assort - 1)), 4).Merge();
                                                }

                                            }//ESTILOS DEL ASSORT
                                        }
                                    }
                                }
                            }

                            string ponum_f = "", tipo_f = "", estilo_f = "", descripcion_f = "", ratio_f = "", color_f = "", store_f = "", dc_f = "", assort_f = "";
                            foreach (estilos estilo in tarimas.lista_estilos)
                            {
                                if (estilo.index_dc == indice && estilo.tipo_empaque != 3)
                                {
                                    es_otro++;

                                    ponum_f = estilo.number_po;
                                    string tipo = " ";
                                    if (estilo.tipo != "NONE") { tipo += estilo.tipo; }
                                    if (estilo.label != "NONE") { tipo += " " + estilo.label; }
                                    tipo_f = tipo;

                                    estilo_f = (estilo.estilo).Trim();
                                    color_f = (estilo.color).Trim();
                                    descripcion_f = (estilo.descripcion).Trim();
                                    if (tiendas != 0) { store_f = estilo.store; }
                                    if (dc != 0) { dc_f = estilo.dc; }
                                    if (estilo.tipo_empaque == 2 || estilo.tipo_empaque == 4 && estilo.tipo != "DMG" && estilo.tipo != "EXT")
                                    {
                                        contador = 0; string ppk_ratio = ""; total_ratio = 0;
                                        foreach (Talla t in tallas)
                                        {
                                            foreach (ratio_tallas ratio in estilo.lista_ratio)
                                            {
                                                if (ratio.id_talla == t.id_talla)
                                                {
                                                    contador++; ppk_ratio += ratio.ratio;
                                                }
                                                //if (contador < total_ratio) { ppk_ratio += "-"; }
                                            }
                                            if (contador < ((estilo.lista_ratio).Count)) { ppk_ratio += "-"; }
                                        }
                                        ratio_f = ppk_ratio;
                                    }
                                    if (estilo.tipo == "DMG" || estilo.tipo == "EXT")
                                    {
                                        int ii = 0;
                                        foreach (Talla sizes in tallas_bpdc)
                                        {
                                            if (estilo.id_talla == sizes.id_talla)
                                            {
                                                sizes.total = estilo.boxes;
                                                suma_estilo += estilo.boxes;
                                                suma_cajas = estilo.repeticiones;
                                                sumas_tallas[ii] += estilo.boxes;
                                            }
                                            ii++;
                                        }
                                    }else{
                                        if (estilo.tipo_empaque == 5 || estilo.tipo_empaque == 1){
                                            int ii = 0;
                                            foreach (Talla sizes in tallas_bpdc){
                                                if (estilo.id_talla == sizes.id_talla){
                                                    //REVISAR SI TIENE DC O NO
                                                    if (estilo.dc != "0"){
                                                        int piezas = 0;
                                                        if (estilo.tipo_empaque == 1) { piezas = ds.buscar_piezas_empaque_bull(estilo.id_po_summary, estilo.id_talla); }
                                                        if (estilo.tipo_empaque == 5) { piezas = ds.buscar_piezas_empaque_bulls(estilo.id_po_summary, estilo.id_talla, estilo.packing_name); }
                                                        sizes.total = estilo.boxes * piezas;
                                                        //fsdgfh
                                                        sumas_tallas[ii] += estilo.boxes * piezas;
                                                        suma_estilo += estilo.boxes * piezas;
                                                        suma_cajas += estilo.boxes;
                                                    }else{
                                                        sizes.total = estilo.boxes;
                                                        sumas_tallas[ii] += estilo.boxes;
                                                        suma_estilo += estilo.boxes;
                                                        if (estilo.sobrantes == 1){
                                                            suma_cajas += estilo.repeticiones;
                                                        }else{
                                                            if (estilo.tipo_empaque == 1){
                                                                suma_cajas += (estilo.boxes / ds.buscar_piezas_empaque_bull(estilo.id_po_summary, estilo.id_talla));
                                                            }
                                                            if (estilo.tipo_empaque == 5){
                                                                suma_cajas += (estilo.boxes / ds.buscar_piezas_empaque_bulls(estilo.id_po_summary, estilo.id_talla, estilo.packing_name));
                                                            }
                                                        }
                                                    }//else
                                                    //REVISAR SI TIENE DC O NO                                                    
                                                }
                                                ii++;
                                            }
                                        }
                                        if (estilo.tipo_empaque == 2 || estilo.tipo_empaque == 4)
                                        {
                                            int ii = 0;
                                            foreach (Talla sizes in tallas_bpdc)
                                            {
                                                foreach (ratio_tallas ratio in estilo.lista_ratio)
                                                {
                                                    if (sizes.id_talla == ratio.id_talla)
                                                    {
                                                        sizes.total = (ratio.ratio * estilo.boxes);
                                                        suma_estilo += (ratio.ratio * estilo.boxes);
                                                        sumas_tallas[ii] += (ratio.ratio * estilo.boxes);
                                                        suma_cajas = estilo.boxes;
                                                    }
                                                }
                                                ii++;
                                            }
                                        }
                                    }//else

                                }// IF INDICE
                            }//ESTILOS
                            if (es_otro != 0)
                            {
                                List<String> datos = new List<string>();
                                datos.Add(ponum_f);
                                datos.Add(tipo_f);
                                if (ass != 0) { datos.Add(" "); }
                                datos.Add(estilo_f);
                                datos.Add(color_f);
                                datos.Add(descripcion_f);
                                if (tiendas != 0) { datos.Add(store_f); }
                                if (dc != 0) { if (dc_f != "0") { datos.Add(dc_f); } else { datos.Add(" "); } }
                                if (ppk != 0) { datos.Add(ratio_f); }
                                int i = 0;
                                foreach (Talla sizes in tallas_bpdc)
                                {
                                    if (sizes.total == 0)
                                    {
                                        datos.Add(" ");
                                    }
                                    else
                                    {
                                        datos.Add(Convert.ToString(sizes.total));
                                    }
                                    i++;
                                }
                                datos.Add(Convert.ToString(suma_estilo));
                                datos.Add(Convert.ToString(suma_cajas));
                                sumas_tallas[i] += suma_estilo; i++;
                                sumas_tallas[i] += suma_cajas;
                                estilos_capturados++;
                                celdas_estilos.Add(datos.ToArray());
                                //r++;
                            }


                        }//INDICE


                        ws.Cell(r, 2).Value = celdas_estilos;//// <-------------THIS!!
                        if (estilos_normales != 0)
                        {
                            ws.Cell(r, contador_cabeceras).Value = "1";//PALLET
                        }
                        else
                        {
                            ws.Cell(r, contador_cabeceras).Value = "*";//PALLET
                        }
                        ws.Range(r, contador_cabeceras, (r + estilos_total - 1), contador_cabeceras).Merge();
                        r = r + (estilos_total);
                    }//************T*A*R*I*M*A*S*******************************************************************
                    //*************************************************************************T*A*R*I*M*A*S************************************************************************************
                    //***************************************************************************T*A*R*I*M*A*S********************************************************************************************************


                    /****************************************************************************************************************************************************************************************/
                    /****************************************************************************************************************************************************************************************/
                    /****************************************************************************************************************************************************************************************/
                    contador = 0;
                    string descripcion_final = "";
                    /*estilos desc = new estilos();
                    desc.descripcion = estilo.des
                    lista_descripciones_finales.Add();*/
                    foreach (Tarima tarimas in item.lista_tarimas)
                    {
                        foreach (estilos estilo in tarimas.lista_estilos)
                        {
                            if (estilo.tipo_empaque != 3)
                            {
                                bool isEmpty = !lista_descripciones_finales.Any();
                                if (isEmpty)
                                {
                                    estilos desc = new estilos();
                                    desc.id_estilo = estilo.id_estilo;
                                    desc.descripcion_final = estilo.descripcion_final;
                                    lista_descripciones_finales.Add(desc);
                                    descripcion_final += Regex.Replace(estilo.descripcion_final, @"\s+", " ") + " ";
                                }
                                else
                                {
                                    int existencia = 0;
                                    foreach (estilos e in lista_descripciones_finales)
                                    {
                                        if (e.descripcion_final == estilo.descripcion_final)
                                        {
                                            existencia++;
                                        }
                                    }
                                    if (existencia == 0)
                                    {
                                        estilos desc = new estilos();
                                        desc.id_estilo = estilo.id_estilo;
                                        desc.descripcion_final = estilo.descripcion_final;
                                        lista_descripciones_finales.Add(desc);
                                        descripcion_final += Regex.Replace(estilo.descripcion_final, @"\s+", " ") + " ";
                                    }
                                }
                            }
                            else
                            {//AQUI ASSORT
                                foreach (estilos ee in estilo.assort.lista_estilos)
                                {
                                    bool isEmpty = !lista_descripciones_finales.Any();
                                    if (isEmpty)
                                    {
                                        estilos desc = new estilos();
                                        desc.id_estilo = ee.id_estilo;
                                        desc.descripcion_final = ee.descripcion_final;
                                        lista_descripciones_finales.Add(desc);
                                        descripcion_final += Regex.Replace(ee.descripcion_final, @"\s+", " ") + " ";
                                    }
                                    else
                                    {
                                        int existencia = 0;
                                        foreach (estilos e in lista_descripciones_finales)
                                        {
                                            if (e.descripcion_final == ee.descripcion_final)
                                            {
                                                existencia++;
                                            }
                                        }
                                        if (existencia == 0)
                                        {
                                            estilos desc = new estilos();
                                            desc.id_estilo = ee.id_estilo;
                                            desc.descripcion_final = ee.descripcion_final;
                                            lista_descripciones_finales.Add(desc);
                                            descripcion_final += Regex.Replace(ee.descripcion_final, @"\s+", " ") + " ";
                                        }
                                    }
                                }
                            }
                        }
                    }

                    ws.Cell(r, 1).Value = descripcion_final;
                    ws.Cell(r, 1).Style.Font.Bold = true;
                    ws.Cell(r, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(r, 1, r, 4).Merge();
                    ws.Range(r, 1, r, 4).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);

                    ws.Cell(r, 5).Value = "TOTAL";
                    ws.Cell(r, 5).Style.Font.Bold = true;
                    ws.Cell(r, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    c = 6;
                    if (dc != 0) { c++; }
                    if (ppk != 0) { c++; }//POR EL RATIO
                    if (ass != 0) { c++; }
                    if (tiendas != 0) { c++; }
                    for (int i = 12; i < r; i++)
                    {
                        for (int j = 1; j <= c; j++)
                        {
                            ws.Column(c).AdjustToContents(r, c);
                        }
                    }
                    ws.Range(r, 5, r, c).Merge();
                    ws.Range(r, 5, r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    c++;

                    for (int i = 0; i < contador_tallas + 2; i++)
                    {
                        ws.Cell(r, c).Value = sumas_tallas[i];
                        ws.Cell(r, c).Style.Font.Bold = true;
                        ws.Cell(r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                        ws.Cell(r, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;                        
                        c++;
                    }
                    ws.Cell(r, c).Value = pallets;
                    ws.Cell(r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    ws.Cell(r, c).Style.Font.Bold = true;
                    ws.Cell(r, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    for (int zz = (r + 1); zz <= (r + 30); zz++) { ws.Row(zz).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }
                    filas = r;

                    ws.Column(2).AdjustToContents(12, 2);
                    ws.Column(3).AdjustToContents(12, 3);
                    ws.Column(4).AdjustToContents(12, 4);
                    for (int i = 12; i <= r; i++)
                    {
                        for (int j = 1; j <= c; j++)
                        {
                            ws.Cell(i, j).Style.Font.FontSize = 8;
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


                    filas += 2;
                    ws.Range(filas, 1, (filas + 2), 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(filas, 1, (filas + 2), 1).Style.Font.Bold = true;
                    ws.Range(filas, 7, (filas + 2), 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(filas, 7, (filas + 2), 7).Style.Font.Bold = true;
                    ws.Range(filas, 2, (filas + 2), 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(filas, 8, (filas + 2), 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(filas, 1).Value = "DRIVER NAME:";
                    ws.Cell(filas, 2).Value = item.conductor.driver_name;
                    ws.Cell(filas, 7).Value = "SHIPPING MANAGER:";
                    ws.Cell(filas, 8).Value = item.shipping_manager;
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 1).Value = "TRAILER/PLATES:";
                    ws.Cell(filas, 2).Value = item.conductor.tractor + "/" + item.conductor.plates;
                    ws.Cell(filas, 7).Value = "SEAL:";
                    ws.Cell(filas, 8).SetValue(item.seal);
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 1).Value = "CONTAINER/PLATES:";
                    ws.Cell(filas, 2).Value = item.contenedor.eco + "/" + item.contenedor.plates;
                    ws.Cell(filas, 7).Value = "REPLACEMENT:";
                    ws.Cell(filas, 8).SetValue(item.replacement);
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 3).Value = "DOCUMENTO CONTROLADO. ÚNICAMENTE SE PUEDE MODIFICAR POR EL SUPERVISOR DE SHIPPING";
                    ws.Cell(filas, 3).Style.Font.FontColor = XLColor.FromArgb(100, 100, 100);
                    filas += 2;
                    columnas = 2;


                    var porcentajes = new List<String[]>();
                    List<String> p = new List<string>();
                    p.Add("ORIGIN"); p.Add("QTY"); p.Add("%");
                    porcentajes.Add(p.ToArray());
                    List<Fabricantes> totales_paises_estilo = new List<Fabricantes>();
                    List<Fabricantes> totales_paises = new List<Fabricantes>();
                    List<int> lista_estilos_stag = new List<int>();
                    int add = 0, total_paises = 0, iguales;
                    foreach (Tarima tarimas in item.lista_tarimas)
                    {
                        foreach (estilos e in tarimas.lista_estilos)
                        {
                            if (e.tipo_empaque != 3)
                            {
                                lista_estilos_stag.Add(e.id_po_summary);
                            }
                            else
                            {
                                foreach (estilos ee in e.assort.lista_estilos)
                                {
                                    lista_estilos_stag.Add(ee.id_po_summary);
                                }
                            }
                        }
                    }
                    lista_estilos_stag = lista_estilos_stag.Distinct().ToList();
                    totales_paises_estilo = ds.buscar_paises_estilos_stag_recibos(lista_estilos_stag);
                    foreach (Fabricantes fa in totales_paises_estilo)
                    {
                        iguales = 0;
                        if (add == 0)
                        {
                            Fabricantes nf = new Fabricantes();
                            nf.cantidad = fa.cantidad;
                            nf.pais = fa.pais;
                            nf.percent = fa.percent;
                            nf.id = fa.id;
                            totales_paises.Add(nf);
                            add++;
                        }
                        else
                        {
                            foreach (Fabricantes f in totales_paises.ToList())
                            {
                                if (f.pais == fa.pais && f.percent == fa.percent)
                                {
                                    f.cantidad += fa.cantidad;
                                    iguales++;
                                }
                            }
                            if (iguales == 0)
                            {
                                Fabricantes nf = new Fabricantes();
                                nf.cantidad = fa.cantidad;
                                nf.pais = fa.pais;
                                nf.id = fa.id;
                                nf.percent = fa.percent;
                                totales_paises.Add(nf);
                            }
                            add++;
                        }
                    }

                    foreach (Fabricantes f in totales_paises) { total_paises += f.cantidad; }
                    foreach (Fabricantes f in totales_paises)
                    {
                        double z = ((Convert.ToDouble(f.cantidad) * 100) / Convert.ToDouble(total_paises));
                        f.porcentaje = Math.Round(z, MidpointRounding.ToEven);
                    }
                    List<Fabricantes> totales_paises_envio = new List<Fabricantes>();
                    foreach (Fabricantes f in totales_paises)
                    {
                        Fabricantes nf = new Fabricantes();
                        double x = ((Convert.ToDouble(sumas_tallas[sumas_tallas.Length - 2]) * Convert.ToDouble(f.cantidad)) / Convert.ToDouble(total_paises));
                        nf.cantidad = Convert.ToInt32(Math.Round(x, MidpointRounding.ToEven));
                        nf.pais = f.pais;
                        nf.percent = f.percent;
                        totales_paises_envio.Add(nf);
                    }
                    foreach (Fabricantes f in totales_paises_envio)
                    {
                        porcentajes.Add(new string[] { f.pais, (f.cantidad).ToString(), f.percent });
                    }
                    ws.Cell(filas, 2).Value = "%";
                    ws.Cell(filas, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    filas++;
                    ws.Cell(filas, 2).Value = porcentajes;
                }
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

        public void crear_excel_samples(List<Pk> lista){
            string clave_packing = "";
            using (XLWorkbook libro_trabajo = new XLWorkbook()){
                string archivo = "";
                int estilos_total = 0, rows = 0, tarima_contador = 0, total_ratio, contador, r, c, total_cajas = 0, filas = 0, columnas = 0, tallas_id,total_columnas_extras=0;
                List<Talla> tallas = new List<Talla>();
                List<Talla> tallas_bpdc = new List<Talla>();
                List<int> columnas_extras = new List<int>();
                string cabeceras_temporal_tallas = "";
                string[] cabeceras_extras_tallas;
                var ws = libro_trabajo.Worksheets.Add("PK");
                foreach (Pk item in lista){
                    int cont_ext = 0;
                    foreach (Sample s in item.lista_samples){
                        if (cont_ext == 0){
                            cabeceras_temporal_tallas = s.cabeceras;
                            foreach (int cx in s.cols_extras){
                                columnas_extras.Add(cx);
                            }
                        }
                        cont_ext++;
                    }
                    total_columnas_extras = columnas_extras.Count();
                    string[] cab_temporal= cabeceras_temporal_tallas.Split('*');
                    cabeceras_extras_tallas = cab_temporal[0].Split('&');

                    clave_packing = item.packing;
                    string[] nombre = (item.packing).Split('-');
                    if (item.id_customer == 1) {
                        archivo = " PK "+ nombre[0]+ " " + item.nombre_archivo + " (SAMPLES)";
                    } else {
                        archivo = " PK " + nombre[0] + " (SAMPLES)";
                    }
                    
                    /*****INICIO CON DIRECCIONES, LOGO, ETC******/
                    ws.Cell("A2").Value = "FORTUNE FASHIONS BAJA, S.R.L. DE C.V.";
                    ws.Cell("A3").Value = "CALLE TORTUGA No 313-A";
                    ws.Cell("A4").Value = "MANEADERO CP 22790";
                    ws.Cell("A5").Value = "BAJA CALIFORNIA";
                    ws.Style.Font.FontSize = 8;

                    ws.Range("A2:A10").Style.Font.Bold = true;
                    ws.Range("A7:A10").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    //DIRECCIÓN DE ORIGEN
                    ws.Cell("A7").Value = "CUSTOMER: ";
                    ws.Cell("B7").Value = item.customer;
                    if (item.id_customer == 1) {
                        ws.Cell("A8").Value = "P.O.: ";
                        string pedidos="";
                        List<int> lista_idpedidos = new List<int>();
                        //foreach (Tarima t in item.lista_tarimas){
                        foreach (Sample s in item.lista_samples){
                            if (s.inicial == 1){
                                bool isEmpty = !lista_idpedidos.Any();
                                if (isEmpty){
                                    lista_idpedidos.Add(s.id_pedido);
                                    pedidos += " " + s.pedido;
                                }else{
                                    int existe = 0;
                                    foreach (int index in lista_idpedidos){
                                        if (index == s.id_pedido) { existe++; }
                                    }
                                    if (existe == 0){
                                        lista_idpedidos.Add(s.id_pedido);
                                        pedidos += " " + s.pedido;
                                    }
                                }
                            }
                        }//FOREACH
                        //}//TARIMAS
                        //ws.Cell("B8").Value = pedidos;
                    }
                    ws.Cell("B8").Value = item.nombre_archivo;
                    //ws.Cell("B8").Value = " SAMPLES";
                    ws.Range("B2:B10").Style.Font.Bold = true;
                    //IMAGEN AL CENTRO
                    ws.Range(1, 7, 1, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    var imagePath = Server.MapPath("/") + "/Content/img/LOGOFORTUNEEXCEL.png";
                    //var imagePath = @"C:\Users\melissa\source\repos\FortuneSys----\FortuneSystem\Content\img\LOGO FORTUNE.png";
                    //var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1")).Scale(0.30);
                    var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1"));
                    //PK ABAJO DE LA IMAGEN
                    ws.Cell("D7").Value = "PK: ";
                    ws.Cell("D7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell("D7").Style.Font.Bold = true;
                    ws.Cell("E7").Value = item.packing;
                    ws.Cell("E7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range("E7:F7").Merge();
                    ws.Range("E7", "F7").Style.Font.Bold = true;
                    ws.Range("D7", "F7").Style.Font.FontSize = 15;
                    ws.Range("E7:F7").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range("E7:F7").Style.Border.BottomBorderColor = XLColor.Black;
                    //DIRECCION DE DESTINO
                    ws.Cell("L2").Value = "SHIP TO: ";
                    ws.Cell("L3").Value = item.destino.nombre;
                    ws.Cell("L4").Value = item.destino.direccion;
                    ws.Cell("L5").Value = item.destino.ciudad + " " + item.destino.zip;
                    ws.Cell("L8").Value = "DATE:" + item.fecha;
                    ws.Range("L2", "L9").Style.Font.Bold = true;
                    var columna_a = ws.Range("A2", "A10");
                    ws.Rows("6").Height = 30;
                    for (int zz = 1; zz < 10; zz++) { ws.Row(zz).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/

                    ws.Cell("H10").Value = "B-BIG"; ws.Cell("I10").Value = "1XL"; ws.Cell("J10").Value = "2XL"; ws.Cell("K10").Value = "3XL"; ws.Cell("L10").Value = "4XL"; ws.Cell("M10").Value = "5XL"; ws.Cell("N10").Value = "6XL"; ws.Cell("O10").Value = "7XL";
                    ws.Cell("H11").Value = "C-TODDLER GIRLS"; ws.Cell("I11").Value = "2T"; ws.Cell("J11").Value = "3T"; ws.Cell("K11").Value = "4T"; ws.Cell("L11").Value = "5T";
                    ws.Cell("H12").Value = "D-TODDLER BOYS"; ws.Cell("I12").Value = "2T"; ws.Cell("J12").Value = "3T"; ws.Cell("K12").Value = "4T"; ws.Cell("L12").Value = "5T";
                    ws.Cell("H13").Value = "F-MISSY"; ws.Cell("I13").Value = "XS"; ws.Cell("J13").Value = "SM"; ws.Cell("K13").Value = "MD"; ws.Cell("L13").Value = "LG"; ws.Cell("M13").Value = "XL"; ws.Cell("N13").Value = "2XL"; ws.Cell("O13").Value = "3XL";
                    ws.Cell("H14").Value = "G-JUNIOR"; ws.Cell("I14").Value = "XS"; ws.Cell("J14").Value = "SM"; ws.Cell("K14").Value = "MD"; ws.Cell("L14").Value = "LG"; ws.Cell("M14").Value = "XL"; ws.Cell("N14").Value = "2XL"; ws.Cell("O14").Value = "3XL";
                    ws.Cell("H15").Value = "I-INFANT"; ws.Cell("I15").Value = "NB"; ws.Cell("J15").Value = "3M"; ws.Cell("K15").Value = "6M"; ws.Cell("L15").Value = "9M"; ws.Cell("M15").Value = "12M"; ws.Cell("N15").Value = "18M"; ws.Cell("O15").Value = "24M";
                    ws.Cell("H16").Value = "J-LITTLE BOYS"; ws.Cell("I16").Value = "'04";  ws.Cell("J16").Value = "'5/6"; ws.Cell("K16").Value = "'7";
                    ws.Cell("H17").Value = "M-YOUNG MENS"; ws.Cell("I17").Value = "XS"; ws.Cell("J17").Value = "SM"; ws.Cell("K17").Value = "MD"; ws.Cell("L17").Value = "LG"; ws.Cell("M17").Value = "XL"; ws.Cell("N17").Value = "2XL"; ws.Cell("O17").Value = "3XL";
                    ws.Cell("H18").Value = "Q-BIG GIRLS"; ws.Cell("I18").Value = "XS"; ws.Cell("J18").Value = "SM"; ws.Cell("K18").Value = "MD"; ws.Cell("L18").Value = "LG"; ws.Cell("M18").Value = "XL"; ws.Cell("N18").Value = "2XL"; ws.Cell("O18").Value = "3XL";
                    ws.Cell("H19").Value = "R-LITTLE GIRLS"; ws.Cell("I19").Value = "'04"; ws.Cell("J19").Value = "'5/6"; ws.Cell("K19").Value = "6x"; ws.Cell("L19").Value = "'07";
                    ws.Cell("H20").Value = "T-TALL"; ws.Cell("I20").Value = "LT"; ws.Cell("J20").Value = "1XLT"; ws.Cell("K20").Value = "2XLT"; ws.Cell("L20").Value = "3XLT"; ws.Cell("M20").Value = "4XLT"; ws.Cell("N20").Value = "5XLT"; ws.Cell("O20").Value = "6XLT";
                    ws.Cell("H21").Value = "W-WOMEN"; ws.Cell("I21").Value = "XS"; ws.Cell("J21").Value = "SM"; ws.Cell("K21").Value = "MD"; ws.Cell("L21").Value = "LG"; ws.Cell("M21").Value = "XL"; ws.Cell("N21").Value = "2XL"; ws.Cell("O21").Value = "3XL";
                    ws.Range("H10:O10").Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    ws.Range("H12:O12").Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    ws.Range("H14:O14").Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    ws.Range("H16:O16").Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    ws.Range("H18:O18").Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    ws.Range("H20:O20").Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    for (int j = 10; j <= 21; j++){
                        for (int i = 16; i <= (16 + total_columnas_extras - 1); i++) {
                            if (j % 2 ==0){
                                ws.Cell(j,i).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                            }
                        }
                    }
                    for (int i = 10; i <= 21; i++){                        
                        for (int j = 1; j <= (15 + total_columnas_extras); j++) {                           
                            ws.Cell(i, j).Style.Font.FontSize = 7;
                            ws.Cell(i, j).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(i, j).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            ws.Cell(i, j).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
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
                    ws.Cell("A10").Value = "SAMPLES";
                    ws.Cell("A10").Style.Font.FontSize = 40;
                    ws.Cell("A10").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell("A10").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Range("A10:G21").Merge();
                    ws.Range("A10:G21").Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    ws.Range("A10:G21").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range("A10:G21").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Range("A10:G21").Style.Border.LeftBorderColor = XLColor.Black;
                    ws.Range("A10:G21").Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    ws.Range("A10:G21").Style.Border.RightBorderColor = XLColor.Black;
                    ws.Range("A10:G21").Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Range("A10:G21").Style.Border.TopBorderColor = XLColor.Black;
                    ws.Range("A10:G21").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range("A10:G21").Style.Border.BottomBorderColor = XLColor.Black;
                    r = 22;
                    var headers = new List<String[]>();
                    List<String> titulos = new List<string>();//=  "ID", "P.O. NUM", "TYPE", "COLOR", "DESCRIPTION";
                    titulos.Add("P.O."); titulos.Add("P.O. NUM"); titulos.Add("STYLE"); titulos.Add("COLOR"); titulos.Add("DESCRIPTION"); titulos.Add("ORIGEN");
                    titulos.Add("%"); titulos.Add("GENDER");
                    
                    if (cabeceras_extras_tallas.Length > 1 ){
                        for (int i = 1; i < cabeceras_extras_tallas.Length; i++){
                            titulos.Add(cabeceras_extras_tallas[i]);
                        }
                    }else {
                        for (int i = 1; i <(8+ total_columnas_extras); i++){
                            titulos.Add(" ");
                        }
                    }
                   
                    titulos.Add("PCS"); titulos.Add("BXS"); titulos.Add("ATTN TO");
                    headers.Add(titulos.ToArray());
                    int total_titulos = (titulos.ToArray()).Length;
                    ws.Cell(r, 1).Value = headers;
                    for (int i = 1; i <= (18+total_columnas_extras); i++){
                        ws.Cell(22, i).Style.Font.Bold = true;
                        ws.Cell(22, i).Style.Font.FontSize = 8;
                        ws.Cell(22, i).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                        ws.Cell(22, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(22, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Cell(22, i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(22, i).Style.Border.LeftBorderColor = XLColor.Black;
                        ws.Cell(22, i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        ws.Cell(22, i).Style.Border.RightBorderColor = XLColor.Black;
                        ws.Cell(22, i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(22, i).Style.Border.TopBorderColor = XLColor.Black;
                        ws.Cell(22, i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(22, i).Style.Border.BottomBorderColor = XLColor.Black;
                    }
                    /**********************************************************************************************************************************************************************************************/
                    /***********************C*A*B*E*C*E*R*A*S**D*E**L*A**T*A*B*L*A*********************************************************************************************************************************/
                    //int[] tallas_comparacion = tallas_id_temporal.ToArray();                     
                    int[] sumas_tallas = new int[9+total_columnas_extras];
                    for (int i = 0; i < (9 + total_columnas_extras); i++) { sumas_tallas[i] = 0; }
                    r = 23;
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************
                   
                    List<int> tarimas = new List<int>();
                    foreach (Sample s in item.lista_samples) { tarimas.Add(s.id_tarima); }
                    tarimas = tarimas.Distinct().ToList();
                    
                    int total_estilos_tarima;
                    int total_total_todos = 0;
                    foreach (int t in tarimas){
                        total_estilos_tarima = 0;
                        total_total_todos = 0;
                        foreach (Sample s in item.lista_samples) { if (s.id_tarima == t) { total_estilos_tarima++; } }
                        foreach (Sample s in item.lista_samples) {
                            if (s.id_tarima == t) {
                                total_total_todos += s.talla_xs + s.talla_s + s.talla_m + s.talla_l + s.talla_xl + s.talla_2x + s.talla_3x;
                                if (s.total_extras != 0){
                                    foreach (Extra_sample xs in s.lista_extras){
                                        total_total_todos += xs.total;
                                    }
                                }
                            }
                        }
                        foreach (Sample s in item.lista_samples){
                            var fila = new List<String[]>();
                            if (s.id_tarima == t && s.inicial==1){
                                int total_fila = 0;
                                List<String> datos = new List<string>();
                                datos.Add((s.pedido).Trim()); datos.Add((s.number_po).Trim()); datos.Add(" " + s.estilo + " "); datos.Add(s.color); datos.Add(s.descripcion);
                                datos.Add(s.origen); datos.Add(s.porcentaje); datos.Add((s.genero).Trim());
                                if (s.talla_xs != 0) { datos.Add(Convert.ToString(s.talla_xs)); } else { datos.Add(" "); } sumas_tallas[0] += s.talla_xs; total_fila += s.talla_xs;
                                if (s.talla_s != 0) { datos.Add(Convert.ToString(s.talla_s)); } else { datos.Add(" "); } sumas_tallas[1] += s.talla_s; total_fila += s.talla_s;
                                if (s.talla_m != 0) { datos.Add(Convert.ToString(s.talla_m)); } else { datos.Add(" "); } sumas_tallas[2] += s.talla_m; total_fila += s.talla_m;
                                if (s.talla_l != 0) { datos.Add(Convert.ToString(s.talla_l)); } else { datos.Add(" "); } sumas_tallas[3] += s.talla_l; total_fila += s.talla_l;
                                if (s.talla_xl != 0) { datos.Add(Convert.ToString(s.talla_xl)); } else { datos.Add(" "); } sumas_tallas[4] += s.talla_xl; total_fila += s.talla_xl;
                                if (s.talla_2x != 0) { datos.Add(Convert.ToString(s.talla_2x)); } else { datos.Add(" "); } sumas_tallas[5] += s.talla_2x; total_fila += s.talla_2x;
                                if (s.talla_3x != 0) { datos.Add(Convert.ToString(s.talla_3x)); } else { datos.Add(" "); } sumas_tallas[6] += s.talla_3x; total_fila += s.talla_3x;
                                cont_ext = 7;
                                foreach (int extra in columnas_extras){
                                    int existe = 0;
                                    foreach (Extra_sample xs in s.lista_extras){
                                        if (extra == xs.columna) {
                                            existe++;
                                            datos.Add(Convert.ToString(xs.total));
                                            sumas_tallas[cont_ext] += xs.total;
                                            total_fila += xs.total;
                                        }
                                    }
                                    if (existe == 0) { datos.Add(" "); }
                                    cont_ext++;
                                }                                
                                //datos.Add(Convert.ToString(total_fila)); sumas_tallas[cont_ext] += total_fila;//s.total;
                                datos.Add(Convert.ToString(total_total_todos)); sumas_tallas[cont_ext] += total_fila;//s.total;
                                cont_ext++;
                                datos.Add(Convert.ToString(s.cajas)); sumas_tallas[cont_ext] += s.cajas;
                                datos.Add(s.attnto);
                                fila.Add(datos.ToArray());
                                ws.Cell(r, 1).Value = fila;
                                //ws.Range(r, 1, (r + (total_estilos_tarima - 1)), 1).Merge();
                                ws.Range(r, (17+total_columnas_extras-1), (r + (total_estilos_tarima - 1)), (17 + total_columnas_extras-1)).Merge();
                                ws.Range(r, (18 + total_columnas_extras - 1), (r + (total_estilos_tarima - 1)), (18 + total_columnas_extras - 1)).Merge();
                                ws.Range(r, (19 + total_columnas_extras - 1), (r + (total_estilos_tarima - 1)), (19 + total_columnas_extras - 1)).Merge();
                                r++;
                            }
                        }//FOREACH

                        foreach (Sample s in item.lista_samples){
                            var fila = new List<String[]>();
                            if (s.id_tarima == t && s.inicial == 0){
                                int total_fila = 0;
                                List<String> datos = new List<string>();
                                datos.Add((s.pedido).Trim()); datos.Add((s.number_po).Trim()); datos.Add("" + s.estilo + ""); datos.Add(s.color); datos.Add(s.descripcion);
                                datos.Add(s.origen); datos.Add(s.porcentaje); datos.Add((s.genero).Trim());
                                if (s.talla_xs != 0) { datos.Add(Convert.ToString(s.talla_xs)); } else { datos.Add(" "); } sumas_tallas[0] += s.talla_xs; total_fila += s.talla_xs;
                                if (s.talla_s != 0) { datos.Add(Convert.ToString(s.talla_s)); } else { datos.Add(" "); } sumas_tallas[1] += s.talla_s; total_fila += s.talla_s;
                                if (s.talla_m != 0) { datos.Add(Convert.ToString(s.talla_m)); } else { datos.Add(" "); } sumas_tallas[2] += s.talla_m; total_fila += s.talla_m;
                                if (s.talla_l != 0) { datos.Add(Convert.ToString(s.talla_l)); } else { datos.Add(" "); } sumas_tallas[3] += s.talla_l; total_fila += s.talla_l;
                                if (s.talla_xl != 0) { datos.Add(Convert.ToString(s.talla_xl)); } else { datos.Add(" "); } sumas_tallas[4] += s.talla_xl; total_fila += s.talla_xl;
                                if (s.talla_2x != 0) { datos.Add(Convert.ToString(s.talla_2x)); } else { datos.Add(" "); } sumas_tallas[5] += s.talla_2x; total_fila += s.talla_2x;
                                if (s.talla_3x != 0) { datos.Add(Convert.ToString(s.talla_3x)); } else { datos.Add(" "); } sumas_tallas[6] += s.talla_3x; total_fila += s.talla_3x;
                                cont_ext = 7;
                                foreach (int extra in columnas_extras){
                                    int existe = 0;
                                    foreach (Extra_sample xs in s.lista_extras){
                                        if (extra == xs.columna){
                                            existe++;
                                            datos.Add(Convert.ToString(xs.total));
                                            sumas_tallas[cont_ext] += xs.total;
                                            total_fila += xs.total;
                                        }
                                    }
                                    if (existe == 0) { datos.Add(" "); }
                                    cont_ext++;
                                }
                                datos.Add(Convert.ToString(total_fila)); sumas_tallas[cont_ext] += total_fila;//s.total;
                                //datos.Add(Convert.ToString(s.total)); sumas_tallas[7] += s.total;
                                fila.Add(datos.ToArray());
                                ws.Cell(r, 1).Value = fila;
                                r++;
                            }
                        }//FOREACH
                    }//tarimas   
                    
                    //ws.Cell(23, 2).Value = fila;//// <-------------THIS!!
                                                //*************************************************************************T*A*R*I*M*A*S************************************************************************************
                                                //***************************************************************************T*A*R*I*M*A*S********************************************************************************************************
                                                /****************************************************************************************************************************************************************************************/
                                                /****************************************************************************************************************************************************************************************/
                                                /****************************************************************************************************************************************************************************************/
                                                //r++;
                    ws.Range(r,1,r,18+total_columnas_extras).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    ws.Range(r, 1, r, 18 + total_columnas_extras).Style.Font.Bold = true;
                    for (int j=22; j <= r; j++) {
                        int i;
                        for (i = 1; i <= (18 + total_columnas_extras); i++){                           
                            ws.Cell(j, i).Style.Font.FontSize = 8;
                            ws.Cell(j, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            ws.Cell(j, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(j, i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            ws.Cell(j, i).Style.Border.LeftBorderColor = XLColor.Black;
                            ws.Cell(j, i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            ws.Cell(j, i).Style.Border.RightBorderColor = XLColor.Black;
                            ws.Cell(j, i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            ws.Cell(j, i).Style.Border.TopBorderColor = XLColor.Black;
                            ws.Cell(j, i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            ws.Cell(j, i).Style.Border.BottomBorderColor = XLColor.Black;
                        }
                        ws.Cell(j,i).Style.Font.Bold = true;
                    }
                    ws.Cell(r, 1).Value = " ";
                    ws.Cell(r, 6).Value = "TOTAL";                                      
                    ws.Range(r,6, r, 8).Merge();
                    ws.Range(r, 1, r, 5).Merge();
                    c = 9;
                    for (int i = 0; i < (9+total_columnas_extras); i++){
                        ws.Cell(r, c).Value = sumas_tallas[i];
                        c++;
                    }
                    r++;
                    for (int zz = (r + 1); zz <= (r + 30); zz++) { ws.Row(zz).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }
                    filas = r+2;
                    ws.Range(filas, 3, (filas + 2), 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(filas, 3, (filas + 2), 3).Style.Font.Bold = true;
                    ws.Range(filas, 9, (filas + 2), 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(filas, 9, (filas + 2), 9).Style.Font.Bold = true;
                    ws.Range(filas, 4, (filas + 2), 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(filas, 10, (filas + 2), 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    /*********************N*O**T*O*C*A*R***************************************/
                    ws.Cell(filas, 3).Value = "DRIVER NAME:";
                    ws.Cell(filas, 4).Value = item.conductor.driver_name;
                    ws.Cell(filas, 9).Value = "SHIPPING MANAGER:";
                    ws.Cell(filas, 10).Value = item.shipping_manager;
                    ws.Range(filas, 4, filas, 6).Merge();
                    ws.Range(filas, 4, filas, 6).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 4, filas, 6).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 10, filas, 12).Merge();
                    ws.Range(filas, 10, filas, 12).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 10, filas, 12).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 3).Value = "TRAILER/PLATES:";
                    ws.Cell(filas, 4).Value = item.conductor.tractor + "/" + item.conductor.plates;
                    ws.Cell(filas, 9).Value = "SEAL:";
                    ws.Cell(filas, 10).SetValue(item.seal);
                    ws.Range(filas, 4, filas, 6).Merge();
                    ws.Range(filas, 4, filas, 6).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 4, filas, 6).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 10, filas, 12).Merge();
                    ws.Range(filas, 10, filas, 12).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 10, filas, 12).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 3).Value = "CONTAINER/PLATES:";
                    ws.Cell(filas, 4).Value = item.contenedor.eco + "/" + item.contenedor.plates;
                    ws.Cell(filas, 9).Value = "REPLACEMENT:";
                    ws.Cell(filas, 10).SetValue(item.replacement);
                    ws.Range(filas, 4, filas, 6).Merge();
                    ws.Range(filas, 4, filas, 6).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 4, filas, 6).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 10, filas, 12).Merge();
                    ws.Range(filas, 10, filas, 12).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 10, filas, 12).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 4).Value = "DOCUMENTO CONTROLADO. ÚNICAMENTE SE PUEDE MODIFICAR POR EL SUPERVISOR DE SHIPPING";
                    ws.Cell(filas, 4).Style.Font.FontColor = XLColor.FromArgb(100, 100, 100);
                    filas += 2;
                    columnas = 2;
                }
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

        public void crear_excel_returns(List<Pk> lista){
            string clave_packing = "";

            using (XLWorkbook libro_trabajo = new XLWorkbook()){
                string archivo = "";
                int estilos_total = 0, rows = 0, tarima_contador = 0, total_ratio, contador, r, c, total_cajas = 0, filas = 0, columnas = 0, tallas_id;
                List<Talla> tallas = new List<Talla>();
                List<Talla> tallas_bpdc = new List<Talla>();

                var ws = libro_trabajo.Worksheets.Add("PK");
                foreach (Pk item in lista){
                    //item.tipo_empaque = 2;                    
                    clave_packing = item.packing;
                    string[] nombre = (item.packing).Split('-');
                    archivo =  "PK" + nombre[0]+ " " + item.nombre_archivo;                    
                    /*****INICIO CON DIRECCIONES, LOGO, ETC******/
                    ws.Cell("A2").Value = "FORTUNE FASHIONS BAJA, S.R.L. DE C.V.";
                    ws.Cell("A3").Value = "CALLE TORTUGA No 313-A";
                    ws.Cell("A4").Value = "MANEADERO CP 22790";
                    ws.Cell("A5").Value = "BAJA CALIFORNIA";
                    ws.Style.Font.FontSize = 8;

                    ws.Range("A2:A10").Style.Font.Bold = true;
                    ws.Range("A7:A10").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    //DIRECCIÓN DE ORIGEN
                    ws.Cell("A7").Value = "CUSTOMER: ";
                    ws.Cell("B7").Value = item.customer;

                    ws.Cell("A8").Value = "P.O.: " ;
                   // ws.Cell("B8").Value = Regex.Replace(item.pedido, @"\s+", " ") + "- RETURNS ";
                    //ws.Cell("B8").Value = item.packing;
                    ws.Cell("B8").Value = item.nombre_archivo;
                    ws.Cell("A9").Value = "RETAILER: ";
                    ws.Cell("B9").Value = item.customer_po;
                    ws.Range("B2:B10").Style.Font.Bold = true;
                    //IMAGEN AL CENTRO
                    ws.Range(1, 7, 1, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    var imagePath = Server.MapPath("/") + "/Content/img/LOGOFORTUNEEXCEL.png";
                    //var imagePath = @"C:\Users\melissa\source\repos\FortuneSys----\FortuneSystem\Content\img\LOGO FORTUNE.png";
                    //var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1")).Scale(0.30);
                    var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1"));
                    //PK ABAJO DE LA IMAGEN
                    ws.Cell("D7").Value = "PK: ";
                    ws.Cell("D7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell("D7").Style.Font.Bold = true;
                    ws.Cell("E7").Value = item.packing;
                    ws.Cell("E7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range("E7:F7").Merge();
                    ws.Range("E7", "F7").Style.Font.Bold = true;
                    ws.Range("D7", "F7").Style.Font.FontSize = 15;
                    ws.Range("E7:F7").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range("E7:F7").Style.Border.BottomBorderColor = XLColor.Black;
                    //DIRECCION DE DESTINO
                    ws.Cell("L2").Value = "SHIP TO: ";
                    ws.Cell("L3").Value = item.destino.nombre;
                    ws.Cell("L4").Value = item.destino.direccion;
                    ws.Cell("L5").Value = item.destino.ciudad + " " + item.destino.zip;
                    ws.Cell("L8").Value = "DATE:" + item.fecha;
                    ws.Range("L2", "L10").Style.Font.Bold = true;
                    var columna_a = ws.Range("A2", "A10");
                    ws.Rows("6").Height = 30;
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    int contador_cabeceras = 0, contador_tallas = 0, pallets = 0, tiendas = 0, dc = 0, ppk = 0, bp = 0, ass = 0;
                    int suma_estilo, suma_cajas;
                    List<estilos> lista_descripciones_finales = new List<estilos>();
                    var headers = new List<String[]>();
                    List<String> titulos = new List<string>();//=  "ID", "P.O. NUM", "TYPE", "COLOR", "DESCRIPTION";
                    titulos.Add("ID"); titulos.Add("P.O. NUM"); titulos.Add("TYPE");
                    titulos.Add("STYLE"); titulos.Add("COLOR"); titulos.Add("DESCRIPTION");
                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS                    
                    foreach (Tarima t in item.lista_tarimas){
                        foreach (Return ra in t.lista_returns){
                            bool isEmpty = !tallas.Any();
                            if (ra.id_categoria == 1){
                                if (isEmpty && ra.id_talla != 0){
                                    Talla ta = new Talla();
                                    ta.id_talla = ra.id_talla;
                                    ta.talla = ra.talla;
                                    tallas.Add(ta);
                                }else{
                                    int existe = 0;
                                    foreach (Talla sizes in tallas){
                                        if (sizes.id_talla == ra.id_talla && ra.id_talla != 0) { existe++; }
                                    }
                                    if (existe == 0){
                                        Talla ta = new Talla();
                                        ta.id_talla = ra.id_talla;
                                        ta.talla = ra.talla;
                                        tallas.Add(ta);
                                    }
                                }
                            }
                        }//OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                    } //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS<-----TALLAS
                    tallas = ds.obtener_tallas_pk(tallas);
                    List<int> tallas_id_temporal = new List<int>();
                    contador_tallas = 0;
                    foreach (Talla sizes in tallas){
                        titulos.Add(sizes.talla); //AQUI AGREGO LAS TALLAS A LA CABECERA
                        tallas_id_temporal.Add(sizes.id_talla);
                        contador_tallas++;
                    }
                    titulos.Add("PCS"); titulos.Add("BXS"); titulos.Add("PALLETS");
                    foreach (string s in titulos) { contador_cabeceras++; }
                    headers.Add(titulos.ToArray());
                    int total_titulos = (titulos.ToArray()).Length;
                    ws.Cell(11, 1).Value = headers;
                    ws.Column(2).AdjustToContents();
                    ws.Column(5).AdjustToContents();
                    for (int i = 1; i <= total_titulos; i++){
                        ws.Cell(11, i).Style.Font.Bold = true;
                        ws.Cell(11, i).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                        ws.Cell(11, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(11, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.LeftBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.RightBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.TopBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.BottomBorderColor = XLColor.Black;
                    }
                    ws.Rows("6").Height = 30;
                    /**********************************************************************************************************************************************************************************************/
                    /***********************C*A*B*E*C*E*R*A*S**D*E**L*A**T*A*B*L*A*********************************************************************************************************************************/
                    //int[] tallas_comparacion = tallas_id_temporal.ToArray(); 
                    foreach (Tarima tarimas in item.lista_tarimas) { pallets++; }
                    int[] sumas_tallas = new int[contador_tallas + 2];
                    for (int i = 0; i < contador_tallas + 2; i++) { sumas_tallas[i] = 0; }
                    for (int zz = 1; zz <= 10; zz++) { ws.Row(zz).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }
                    r = 12;
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************
                    foreach (Tarima tarimas in item.lista_tarimas){
                        int trims = 0, blanks = 0;
                        suma_estilo = 0; suma_cajas = 0; estilos_total = 0;
                        List<int> lista_contador_estilos = new List<int>();
                        var celdas_estilos_i = new List<String[]>();
                        var celdas_returns = new List<String[]>();
                        List<Tarima> index_dcs = new List<Tarima>();//BUSCO LOS INDEX DE LOS DC
                        //OBTENER TOTALES ESTILOS
                        if (item.id_customer == 1){//MAD ENGINE
                            foreach (Return ret in tarimas.lista_returns){
                                estilos_total++;
                                if (ret.id_categoria == 1){ //SI SON BLANKS
                                    blanks++;
                                }else {
                                    trims++;
                                }
                            }
                        }else{//FANTASY
                            foreach (Return ret in tarimas.lista_returns){
                                if (ret.id_categoria == 1){ //SI SON BLANKS
                                    blanks++;
                                    bool isEmpty = !lista_contador_estilos.Any();
                                    if (isEmpty){
                                        lista_contador_estilos.Add(ret.id_summary);
                                    }else{
                                        int existe = 0;
                                        foreach (int sizes in lista_contador_estilos){
                                            if (sizes == ret.id_summary) { existe++; }
                                        }
                                        if (existe == 0){
                                            lista_contador_estilos.Add(ret.id_summary);
                                        }
                                    }
                                }else{
                                    trims++;
                                    estilos_total++;
                                }
                            }
                            foreach (int x in lista_contador_estilos) { estilos_total++; }
                        }
                        if (blanks != 0 && trims != 0) {
                            archivo += " BLANKS & TRIMS RETURNS";
                        } else {
                            if (blanks != 0) { archivo += " BLANKS RETURNS"; }
                            if (trims != 0) { archivo += " TRIMS RETURNS"; }
                        }                            
                        
                        //OBTENER TOTALES ESTILOS
                        ws.Cell(r, 1).Value = tarimas.id_tarima;
                        ws.Range(r, 1, (r + (estilos_total - 1)), 1).Merge();
                        foreach (Talla sizes in tallas) { sizes.total = 0; }
                        //****************N*O*R*M*A*L*E*S**************************************************
                        if (item.id_customer == 1){//MAD ENGINE
                            foreach (Return ret in tarimas.lista_returns){
                                suma_estilo = 0; suma_cajas = 0;
                                if (ret.id_categoria == 1){
                                    List<String> datos = new List<string>(); //CREO LISTA PARA LOS DATOS   
                                    datos.Add((item.number_po).Trim());
                                    datos.Add(" ");
                                    datos.Add(ret.item);
                                    datos.Add(ret.color);
                                    datos.Add((ret.genero).Trim());
                                    
                                    int i = 0;
                                    foreach (Talla sizes in tallas){ //SI EL TIPO DE EMPAQUE ES 1 BP
                                        if (sizes.id_talla == ret.id_talla){ //BUSCO EN EL ORDEN DE LA LÍNEA DE TALLAS
                                            datos.Add(Convert.ToString(ret.total)); //SI COINCIDEN AGREGO LA CANTIDAD DE PIEZAS
                                            suma_estilo += ret.total; //SUMO EL TOTAL DE PIEZAS -ROW-                                                    
                                            suma_cajas += ret.cajas;
                                            sumas_tallas[i] += ret.total;//SUMO LAS PIEZAS POR TALLA / VA AL FINAL DE LA TABLA
                                        }else { datos.Add(" "); }
                                        i++;
                                    }                                    
                                    datos.Add(Convert.ToString(ret.total));
                                    datos.Add(Convert.ToString(ret.cajas));
                                    sumas_tallas[i] += ret.total;
                                    i++; sumas_tallas[i] += ret.cajas;
                                    celdas_returns.Add(datos.ToArray());
                                }//CATEGORIA BLANKS
                            }//FOREACH
                        }//MAD ENGINE


                        //FANTASY----FANTASY-----FANTASY----FANTASY----FANTASYFANTASY----FANTASY-----FANTASY----FANTASY----FANTASYFANTASY----FANTASY-----FANTASY----FANTASY----FANTASY
                        if (item.id_customer == 2){//FANTASY----FANTASY-----FANTASY----FANTASY----FANTASY---FANTASY-----FANTASY-----FANTASY-----FANTASY---FANTASY-----FANTASY---FANTASY---FANTASY---FANTASY----FANTASY
                            List<int> lista_summary = new List<int>();
                            //BUSCAR LOS DIFERENTES ESTILOS DE FANTASY
                            foreach (Return ret in tarimas.lista_returns){
                                if (ret.id_categoria == 1){ //SI SON BLANKS
                                    bool isEmpty = !lista_summary.Any();
                                    if (isEmpty){
                                        lista_summary.Add(ret.id_summary);
                                    }else{
                                        int existe = 0;
                                        foreach (int sizes in lista_summary){
                                            if (sizes == ret.id_summary) { existe++; }
                                        }
                                        if (existe == 0){
                                            lista_summary.Add(ret.id_summary);
                                        }
                                    }
                                }
                            }
                            List<Talla> tallas_fantasy = tallas;
                            foreach (int s in lista_summary){
                                int existe = 0;
                                suma_estilo = 0; suma_cajas = 0;
                                List<String> datos = new List<string>(); //CREO LISTA PARA LOS DATOS   

                                foreach (Talla t in tallas_fantasy) { t.total = 0; t.ratio = 0; } //ratio para cajas

                                foreach (Return ret in tarimas.lista_returns){
                                    if (s == ret.id_summary && ret.id_categoria==1){
                                        foreach (Talla t in tallas_fantasy){
                                            if (t.id_talla == ret.id_talla){
                                                t.total = ret.total;
                                                t.ratio += ret.cajas;
                                            }
                                        }
                                        if (existe == 0){
                                            datos.Add((item.number_po).Trim());
                                            datos.Add(" ");
                                            datos.Add(ret.item);
                                            datos.Add(ret.color);
                                            datos.Add((ret.genero).Trim());
                                            existe++;
                                        }//DATOS 
                                    }//COMPARACIÓN
                                }//FOREACH RETURNS
                                int i = 0;
                                foreach (Talla t in tallas_fantasy){
                                    datos.Add(Convert.ToString(t.total));
                                    suma_estilo += t.total; //SUMO EL TOTAL DE PIEZAS -ROW-                                                    
                                    suma_cajas += t.ratio;
                                    sumas_tallas[i] += t.total;//SUMO LAS PIEZAS POR TALLA / VA AL FINAL DE LA TABLA
                                    i++;
                                }
                                
                                datos.Add(Convert.ToString(suma_estilo));
                                datos.Add(Convert.ToString(suma_cajas));
                                sumas_tallas[i] += suma_estilo;
                                i++; sumas_tallas[i] += suma_cajas;
                                celdas_returns.Add(datos.ToArray());
                            }//FOREACH SUMMARY
                        }//FANTASY                        
                        foreach (Return ret in tarimas.lista_returns){ //ahora imprimir trims---TRIMS --TRIMS --TRIMS --TRIMS --TRIMS --TRIMS --TRIMS                                
                            if (ret.id_categoria == 2){
                                //List<String> datos = new List<string>(); //CREO LISTA PARA LOS DATOS //DATOS DEL ESTILO   
                                suma_estilo = 0; suma_cajas = 0;
                                List<String> datos = new List<string>(); //CREO LISTA PARA LOS DATOS   
                                datos.Add(" "); //po num
                                datos.Add(" ");//type
                                datos.Add(" ");//style
                                datos.Add(" ");//color
                                datos.Add(ret.descripcion_item);
                                int i = 0;
                                foreach (Talla sizes in tallas){ //SI EL TIPO DE EMPAQUE ES 1 BP                                    
                                    datos.Add(" ");
                                    i++;                                    
                                }
                                
                                datos.Add(Convert.ToString(ret.total));
                                datos.Add(Convert.ToString(ret.cajas));
                                sumas_tallas[i] += ret.total;
                                i++; sumas_tallas[i] += ret.cajas;
                                celdas_returns.Add(datos.ToArray());
                            }// categoria
                        }//lista returns
                        //}//TARIMAS
                        ws.Cell(r, 2).Value = celdas_returns;//// <-------------THIS!!
                        ws.Cell(r, contador_cabeceras).Value = "1";//PALLET
                        ws.Range(r, contador_cabeceras, (r + estilos_total - 1), contador_cabeceras).Merge();
                        r = r + (estilos_total);
                    }//************T*A*R*I*M*A*S*******************************************************************
                    //*************************************************************************T*A*R*I*M*A*S************************************************************************************
                    //***************************************************************************T*A*R*I*M*A*S********************************************************************************************************


                    /****************************************************************************************************************************************************************************************/
                    /****************************************************************************************************************************************************************************************/
                    /****************************************************************************************************************************************************************************************/
                    contador = 0;
                    string descripcion_final = "";
                    
                    ws.Cell(r, 1).Value = "BLANKS RETURNS";
                    ws.Cell(r, 1).Style.Font.Bold = true;
                    ws.Cell(r, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(r, 1, r, 4).Merge();
                    ws.Range(r, 1, r, 4).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);

                    ws.Cell(r, 5).Value = "TOTAL";
                    ws.Cell(r, 5).Style.Font.Bold = true;
                    ws.Cell(r, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    c = 6;
                    for (int i = 12; i < r; i++){
                        for (int j = 1; j <= c; j++){
                            ws.Column(c).AdjustToContents(r, c);
                        }
                    }
                    ws.Range(r, 5, r, c).Merge();
                    ws.Range(r, 5, r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    c++;

                    for (int i = 0; i < contador_tallas + 2; i++){
                        ws.Cell(r, c).Value = sumas_tallas[i];
                        ws.Cell(r, c).Style.Font.Bold = true;
                        ws.Cell(r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                        ws.Cell(r, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        c++;
                    }
                    ws.Cell(r, c).Value = pallets;
                    ws.Cell(r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    ws.Cell(r, c).Style.Font.Bold = true;
                    ws.Cell(r, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    for (int zz = (r + 1); zz <= (r + 30); zz++) { ws.Row(zz).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }
                    filas = r;

                    ws.Column(2).AdjustToContents(12, 2);
                    ws.Column(3).AdjustToContents(12, 3);
                    ws.Column(4).AdjustToContents(12, 4);
                    for (int i = 12; i <= r; i++){
                        for (int j = 1; j <= c; j++){
                            ws.Cell(i, j).Style.Font.FontSize = 8;
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

                    filas += 2;
                    ws.Range(filas, 1, (filas + 2), 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(filas, 1, (filas + 2), 1).Style.Font.Bold = true;
                    ws.Range(filas, 7, (filas + 2), 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(filas, 7, (filas + 2), 7).Style.Font.Bold = true;
                    ws.Range(filas, 2, (filas + 2), 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(filas, 8, (filas + 2), 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(filas, 1).Value = "DRIVER NAME:";
                    ws.Cell(filas, 2).Value = item.conductor.driver_name;
                    ws.Cell(filas, 7).Value = "SHIPPING MANAGER:";
                    ws.Cell(filas, 8).Value = item.shipping_manager;
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 1).Value = "TRAILER/PLATES:";
                    ws.Cell(filas, 2).Value = item.conductor.tractor + "/" + item.conductor.plates;
                    ws.Cell(filas, 7).Value = "SEAL:";
                    ws.Cell(filas, 8).SetValue(item.seal);
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 1).Value = "CONTAINER/PLATES:";
                    ws.Cell(filas, 2).Value = item.contenedor.eco + "/" + item.contenedor.plates;
                    ws.Cell(filas, 7).Value = "REPLACEMENT:";
                    ws.Cell(filas, 8).SetValue(item.replacement);
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 3).Value = "DOCUMENTO CONTROLADO. ÚNICAMENTE SE PUEDE MODIFICAR POR EL SUPERVISOR DE SHIPPING";
                    ws.Cell(filas, 3).Style.Font.FontColor = XLColor.FromArgb(100, 100, 100);
                    filas += 2;
                    columnas = 2;

                   
                }
                ws.Rows().AdjustToContents();
                //ws.Columns().AdjustToContents();

                /***********D*O*W*N*L*O*A*D*************************************************************************************************************************************************************************/
                HttpResponse httpResponse = System.Web.HttpContext.Current.Response;
                httpResponse.Clear();
                httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                httpResponse.AddHeader("content-disposition", "attachment;filename=\"" + archivo + ".xlsx\"");

                // Flush the workbook to the Response.OutputStream
                using (MemoryStream memoryStream = new MemoryStream()){
                    libro_trabajo.SaveAs(memoryStream);
                    memoryStream.WriteTo(httpResponse.OutputStream);
                    memoryStream.Close();
                }

                httpResponse.End();

            }
        }

        public void crear_excel_fantasy_breakdown(List<Pk> lista){
            string clave_packing = "";
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            using (XLWorkbook libro_trabajo = new XLWorkbook()){
                string archivo = "";
                int estilos_total = 0, rows = 0, tarima_contador = 0, total_ratio, contador, r, c, total_cajas = 0, filas = 0, columnas = 0, tallas_id;
                List<Talla> tallas = new List<Talla>();
                List<Talla> tallas_bpdc = new List<Talla>();

                var ws = libro_trabajo.Worksheets.Add("PK");
                foreach (Pk item in lista)
                {
                    //item.tipo_empaque = 2;                    
                    clave_packing = item.packing;
                    string[] nombre_archivo = (item.packing).Split('-');
                    archivo = " PK" + nombre_archivo[0] + " ";
                    //item.parte = consultas.AddOrdinal(Convert.ToInt32(item.parte)) + " Part";
                    /*****INICIO CON DIRECCIONES, LOGO, ETC******/
                    ws.Cell("A2").Value = "FORTUNE FASHIONS BAJA, S.R.L. DE C.V.";
                    ws.Cell("A3").Value = "CALLE TORTUGA No 313-A";
                    ws.Cell("A4").Value = "MANEADERO CP 22790";
                    ws.Cell("A5").Value = "BAJA CALIFORNIA";
                    ws.Style.Font.FontSize = 8;

                    ws.Range("A2:A10").Style.Font.Bold = true;
                    ws.Range("A7:A10").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    //DIRECCIÓN DE ORIGEN
                    ws.Cell("A7").Value = "CUSTOMER: ";
                    ws.Cell("B7").Value = item.customer;
                    
                    ws.Cell("B8").Value = item.nombre_archivo;
                    //archivo = item.packing;.
                    archivo = " PK " + nombre_archivo[0] + " ";
                    archivo += " " + item.nombre_archivo;

                    ws.Cell("A9").Value = "RETAILER: ";
                    ws.Cell("B9").Value = item.customer_po;
                    ws.Range("B2:B10").Style.Font.Bold = true;
                    //IMAGEN AL CENTRO
                    ws.Range(1, 7, 1, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    var imagePath = Server.MapPath("/") + "/Content/img/LOGOFORTUNEEXCEL.png";
                    //var imagePath = @"C:\Users\melissa\source\repos\FortuneSys----\FortuneSystem\Content\img\LOGO FORTUNE.png";
                    //var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1")).Scale(0.30);
                    var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1"));
                    //PK ABAJO DE LA IMAGEN
                    ws.Cell("D7").Value = "PK: ";
                    ws.Cell("D7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell("D7").Style.Font.Bold = true;
                    ws.Cell("E7").Value = item.packing;
                    ws.Cell("E7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range("E7:F7").Merge();
                    ws.Range("E7", "F7").Style.Font.Bold = true;
                    ws.Range("D7", "F7").Style.Font.FontSize = 15;
                    ws.Range("E7:F7").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range("E7:F7").Style.Border.BottomBorderColor = XLColor.Black;
                    //DIRECCION DE DESTINO
                    ws.Cell("L2").Value = "SHIP TO: ";
                    ws.Cell("L3").Value = item.destino.nombre;
                    ws.Cell("L4").Value = item.destino.direccion;
                    ws.Cell("L5").Value = item.destino.ciudad + " " + item.destino.zip;
                    ws.Cell("L8").Value = "DATE:" + item.fecha;
                    ws.Range("L2", "L10").Style.Font.Bold = true;
                    var columna_a = ws.Range("A2", "A10");
                    ws.Rows("6").Height = 30;
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    int contador_cabeceras = 0, contador_tallas = 0, pallets = 0, tiendas = 0, dc = 0, ppk = 0, bp = 0, ass = 0;
                    int suma_estilo, suma_cajas;
                    List<estilos> lista_descripciones_finales = new List<estilos>();
                    var headers = new List<String[]>();
                    List<String> titulos = new List<string>();//=  "ID", "P.O. NUM", "TYPE", "COLOR", "DESCRIPTION";
                    titulos.Add("ID"); titulos.Add("P.O. NUM"); titulos.Add("TYPE");
                    foreach (Tarima t in item.lista_tarimas){ //REVISAN TIPOS DE EMPAQUE, DATOS, DC,ETC
                        foreach (estilos e in t.lista_estilos){
                            if (e.store != "N/A" && e.store != "NA") { tiendas++; }
                            if (e.index_dc != 0) { dc++; }
                            if (e.tipo_empaque == 1) { bp++; }
                            if (e.tipo_empaque == 2) { ppk++; }
                            if (e.tipo_empaque == 3) { ass++; }
                        }
                    }
                    if (ass != 0) { titulos.Add("ASSORTMENT"); }
                    titulos.Add("STYLE"); titulos.Add("COLOR"); titulos.Add("DESCRIPTION");
                    if (tiendas != 0) { titulos.Add("STORE"); }
                    if (dc != 0) { titulos.Add("DC"); }
                    if (ppk != 0) { titulos.Add("RATIO"); }
                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS

                   string[] tallasS = { "SM", "MD", "LG", "XL", "2XL" };
                   for (int i = 0; i < 5; i++) {
                        Talla t = new Talla();
                        t.id_talla = consultas.buscar_talla(tallasS[i]);
                        t.talla = tallasS[i];
                        tallas.Add(t);
                   }                   

                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS<-----TALLAS

                    List<int> tallas_id_temporal = new List<int>();
                    contador_tallas = 0;
                    foreach (Talla sizes in tallas){
                        titulos.Add(sizes.talla); //AQUI AGREGO LAS TALLAS A LA CABECERA
                        tallas_id_temporal.Add(sizes.id_talla);
                        contador_tallas++;
                    }
                    titulos.Add("PCS"); titulos.Add("BXS"); titulos.Add("PALLETS");
                    foreach (string s in titulos) { contador_cabeceras++; }
                    headers.Add(titulos.ToArray());
                    int total_titulos = (titulos.ToArray()).Length;
                    ws.Cell(11, 1).Value = headers;
                    ws.Column(2).AdjustToContents();
                    ws.Column(5).AdjustToContents();
                    for (int i = 1; i <= total_titulos; i++){
                        ws.Cell(11, i).Style.Font.Bold = true;
                        ws.Cell(11, i).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                        ws.Cell(11, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(11, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.LeftBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.RightBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.TopBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.BottomBorderColor = XLColor.Black;
                    }
                    ws.Rows("6").Height = 30;
                    /**********************************************************************************************************************************************************************************************/
                    /***********************C*A*B*E*C*E*R*A*S**D*E**L*A**T*A*B*L*A*********************************************************************************************************************************/
                    //int[] tallas_comparacion = tallas_id_temporal.ToArray(); 
                    //foreach (Tarima tarimas in item.lista_tarimas) { pallets++; }
                    int[] sumas_tallas = new int[contador_tallas + 2];
                    for (int i = 0; i < contador_tallas + 2; i++) { sumas_tallas[i] = 0; }
                    for (int zz = 1; zz <= 10; zz++) { ws.Row(zz).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }
                    r = 12;
                    int estilos_extras = 0, estilos_normales = 0;
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************
                    foreach (Tarima tarimas in item.lista_tarimas)
                    {
                        estilos_total = 0;
                        estilos_extras = 0; estilos_normales = 0;
                        var celdas_estilos_i = new List<String[]>();
                        var celdas_estilos = new List<String[]>();
                        List<int> index = new List<int>();//BUSCO LOS INDEX DE LOS DC
                        //List<int> index_bp = new List<int>();//BUSCO LOS INDEX DE LOS DC

                        foreach (estilos estilo in tarimas.lista_estilos) { index.Add(estilo.index_dc); }
                        index = index.Distinct().ToList();
                        foreach (int i in index){ estilos_total++; }
                        if (estilos_normales != 0) { pallets++; }

                        ws.Cell(r, 1).Value = tarimas.id_tarima;
                        ws.Range(r, 1, (r + (estilos_total - 1)), 1).Merge();
                        foreach (Talla sizes in tallas) { sizes.total = 0; }
                        tallas_bpdc = tallas;
                        foreach (Talla sizes in tallas_bpdc) { sizes.total = 0; }
                        int estilos_capturados = 0;
                        foreach (int indice in index){ //ppks y bp
                            int es_otro = 0;

                            int contador_index = 0; suma_estilo = 0; suma_cajas = 0;
                            foreach (Talla sizes in tallas_bpdc) { sizes.total = 0; sizes.ratio = 0; }

                            string ponum_f = "", tipo_f = "", estilo_f = "", descripcion_f = "", ratio_f = "", color_f = "", store_f = "", dc_f = "", assort_f = "";
                            foreach (estilos estilo in tarimas.lista_estilos){
                                if (estilo.index_dc == indice ){
                                    ponum_f = estilo.number_po;
                                    string tipo = " ";
                                    if (estilo.tipo != "NONE") { tipo += estilo.tipo; }
                                    if (estilo.label != "NONE") { tipo += " " + estilo.label; }
                                    //tipo_f = tipo;
                                    estilo_f = (estilo.estilo).Trim();
                                    color_f =  (ds.obtener_color_estilo_fantasy(estilo.id_estilo)).Trim();
                                    descripcion_f = (estilo.descripcion).Trim();
                                    if (tiendas != 0) { store_f = estilo.store; }
                                    if (dc != 0) { dc_f = estilo.dc; }
                                    //int ii = 0;
                                    foreach (Talla sizes in tallas_bpdc){
                                        if (estilo.id_talla == sizes.id_talla){
                                            sizes.total = estilo.boxes;
                                            //suma_estilo += estilo.boxes;
                                           // sumas_tallas[ii] += estilo.boxes;
                                        }
                                     //   ii++;
                                    }
                                }// IF INDICE
                            }//ESTILOS
                            
                                List<String> datos = new List<string>();
                                datos.Add(ponum_f);
                                datos.Add(tipo_f);
                                if (ass != 0) { datos.Add(" "); }
                                datos.Add(estilo_f);
                                datos.Add(color_f);
                                datos.Add(descripcion_f);
                                if (tiendas != 0) { datos.Add(store_f); }
                                if (dc != 0) { if (dc_f != "0") { datos.Add(dc_f); } else { datos.Add(" "); } }
                                if (ppk != 0) { datos.Add(ratio_f); }
                                int i = 0;
                                foreach (Talla sizes in tallas_bpdc){
                                    if (sizes.total == 0){
                                        datos.Add(" ");
                                    }else {
                                        datos.Add(Convert.ToString(sizes.total));
                                    }
                                    suma_estilo += sizes.total;
                                    sumas_tallas[i] += sizes.total;
                                i++;
                                }
                                suma_cajas += (suma_estilo/6);
                                datos.Add(Convert.ToString(suma_estilo));
                                datos.Add(Convert.ToString(suma_cajas));
                                sumas_tallas[i] += suma_estilo; i++;
                                sumas_tallas[i] += suma_cajas;
                                estilos_capturados++;
                                celdas_estilos.Add(datos.ToArray());
                                //r++;
                           

                        }//INDICE


                        ws.Cell(r, 2).Value = celdas_estilos;//// <-------------THIS!!
                        ws.Cell(r, contador_cabeceras).Value = "1";//PALLET
                        
                        ws.Range(r, contador_cabeceras, (r + estilos_total - 1), contador_cabeceras).Merge();
                        r = r + (estilos_total);
                        pallets++;
                    }//************T*A*R*I*M*A*S*******************************************************************
                    //*************************************************************************T*A*R*I*M*A*S************************************************************************************
                    //***************************************************************************T*A*R*I*M*A*S********************************************************************************************************


                    /****************************************************************************************************************************************************************************************/
                    /****************************************************************************************************************************************************************************************/
                    /****************************************************************************************************************************************************************************************/
                    contador = 0;
                    string descripcion_final = "";
                    /*estilos desc = new estilos();
                    desc.descripcion = estilo.des
                    lista_descripciones_finales.Add();*/
                    foreach (Tarima tarimas in item.lista_tarimas)
                    {
                        foreach (estilos estilo in tarimas.lista_estilos)
                        {
                            bool isEmpty = !lista_descripciones_finales.Any();
                            if (isEmpty)
                            {
                                estilos desc = new estilos();
                                desc.id_estilo = estilo.id_estilo;
                                desc.descripcion = estilo.descripcion_final;
                                lista_descripciones_finales.Add(desc);
                                descripcion_final += Regex.Replace(estilo.descripcion_final, @"\s+", " ") + " ";
                            }
                            else
                            {
                                int existencia = 0;
                                foreach (estilos e in lista_descripciones_finales)
                                {
                                    if (e.id_estilo == estilo.id_estilo)
                                    {
                                        existencia++;
                                    }
                                }
                                if (existencia == 0)
                                {
                                    estilos desc = new estilos();
                                    desc.id_estilo = estilo.id_estilo;
                                    desc.descripcion = estilo.descripcion_final;
                                    lista_descripciones_finales.Add(desc);
                                    descripcion_final += Regex.Replace(estilo.descripcion_final, @"\s+", " ") + " ";
                                }
                            }
                        }
                    }

                    ws.Cell(r, 1).Value = descripcion_final;
                    ws.Cell(r, 1).Style.Font.Bold = true;
                    ws.Cell(r, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(r, 1, r, 4).Merge();
                    ws.Range(r, 1, r, 4).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);

                    ws.Cell(r, 5).Value = "TOTAL";
                    ws.Cell(r, 5).Style.Font.Bold = true;
                    ws.Cell(r, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    c = 6;
                    if (dc != 0) { c++; }
                    if (ppk != 0) { c++; }//POR EL RATIO
                    if (ass != 0) { c++; }
                    if (tiendas != 0) { c++; }
                    for (int i = 12; i < r; i++){
                        for (int j = 1; j <= c; j++){
                            ws.Column(c).AdjustToContents(r, c);
                        }
                    }
                    ws.Range(r, 5, r, c).Merge();
                    ws.Range(r, 5, r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    c++;

                    for (int i = 0; i < contador_tallas + 2; i++){
                        ws.Cell(r, c).Value = sumas_tallas[i];
                        ws.Cell(r, c).Style.Font.Bold = true;
                        ws.Cell(r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                        ws.Cell(r, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        c++;
                    }
                    ws.Cell(r, c).Value = pallets;
                    ws.Cell(r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    ws.Cell(r, c).Style.Font.Bold = true;
                    ws.Cell(r, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    for (int zz = (r + 1); zz <= (r + 30); zz++) { ws.Row(zz).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }
                    filas = r;

                    ws.Column(2).AdjustToContents(12, 2);
                    ws.Column(3).AdjustToContents(12, 3);
                    ws.Column(4).AdjustToContents(12, 4);
                    for (int i = 12; i <= r; i++){
                        for (int j = 1; j <= c; j++){
                            ws.Cell(i, j).Style.Font.FontSize = 8;
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


                    filas += 2;
                    ws.Range(filas, 1, (filas + 2), 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(filas, 1, (filas + 2), 1).Style.Font.Bold = true;
                    ws.Range(filas, 7, (filas + 2), 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(filas, 7, (filas + 2), 7).Style.Font.Bold = true;
                    ws.Range(filas, 2, (filas + 2), 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(filas, 8, (filas + 2), 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(filas, 1).Value = "DRIVER NAME:";
                    ws.Cell(filas, 2).Value = item.conductor.driver_name;
                    ws.Cell(filas, 7).Value = "SHIPPING MANAGER:";
                    ws.Cell(filas, 8).Value = item.shipping_manager;
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 1).Value = "TRAILER/PLATES:";
                    ws.Cell(filas, 2).Value = item.conductor.tractor + "/" + item.conductor.plates;
                    ws.Cell(filas, 7).Value = "SEAL:";
                    ws.Cell(filas, 8).SetValue(item.seal);
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 1).Value = "CONTAINER/PLATES:";
                    ws.Cell(filas, 2).Value = item.contenedor.eco + "/" + item.contenedor.plates;
                    ws.Cell(filas, 7).Value = "REPLACEMENT:";
                    ws.Cell(filas, 8).SetValue(item.replacement);
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 3).Value = "DOCUMENTO CONTROLADO. ÚNICAMENTE SE PUEDE MODIFICAR POR EL SUPERVISOR DE SHIPPING";
                    ws.Cell(filas, 3).Style.Font.FontColor = XLColor.FromArgb(100, 100, 100);
                    filas += 2;
                    columnas = 2;

                    var porcentajes = new List<String[]>();
                    List<String> p = new List<string>();
                    p.Add("ORIGIN"); p.Add("QTY"); p.Add("%");
                    porcentajes.Add(p.ToArray());
                    /*List<Fabricantes> totales_paises_estilo = new List<Fabricantes>();
                    List<Fabricantes> totales_paises = new List<Fabricantes>();
                    int add = 0, total_paises = 0, iguales;
                    foreach (Tarima tarimas in item.lista_tarimas){
                        totales_paises_estilo = ds.buscar_paises_estilos(tarimas.lista_estilos);
                        foreach (Fabricantes fa in totales_paises_estilo){
                            iguales = 0;
                            if (add == 0){
                                Fabricantes nf = new Fabricantes();
                                nf.cantidad = fa.cantidad;
                                nf.pais = fa.pais;
                                nf.percent = fa.percent;
                                totales_paises.Add(nf);
                                add++;
                            }else{
                                foreach (Fabricantes f in totales_paises.ToList()){
                                    if (f.pais == fa.pais){
                                        f.cantidad = fa.cantidad;
                                        iguales++;
                                    }
                                }
                                if (iguales == 0){
                                    Fabricantes nf = new Fabricantes();
                                    nf.cantidad = fa.cantidad;
                                    nf.pais = fa.pais;
                                    nf.percent = fa.percent;
                                    totales_paises.Add(nf);
                                }
                                add++;
                            }
                        }
                    }
                    foreach (Fabricantes f in totales_paises) { total_paises += f.cantidad; }
                     foreach (Fabricantes f in totales_paises){
                        double z = ((Convert.ToDouble(f.cantidad) * 100) / Convert.ToDouble(total_paises));
                        f.porcentaje = Math.Round(z, MidpointRounding.ToEven);
                    }
                    List<Fabricantes> totales_paises_envio = new List<Fabricantes>();
                    foreach (Fabricantes f in totales_paises){
                        Fabricantes nf = new Fabricantes();
                        double x = ((Convert.ToDouble(sumas_tallas[sumas_tallas.Length - 2]) * Convert.ToDouble(f.cantidad)) / Convert.ToDouble(total_paises));
                        nf.cantidad = Convert.ToInt32(Math.Round(x, MidpointRounding.ToEven));
                        nf.pais = f.pais;
                        nf.percent = f.percent;
                        totales_paises_envio.Add(nf);
                    }
                    foreach (Fabricantes f in totales_paises_envio){
                        porcentajes.Add(new string[] { f.pais, (f.cantidad).ToString(), f.percent });
                    }
                    ws.Cell(filas, 2).Value = "%";
                    ws.Cell(filas, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    filas++;*/
                    ws.Cell(filas, 2).Value = porcentajes;

                }
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

        public void crear_excel_fantasy_extras(List<Pk> lista)
        {
            string clave_packing = "";
            using (XLWorkbook libro_trabajo = new XLWorkbook())
            {
                string archivo = "";
                int estilos_total = 0, rows = 0, tarima_contador = 0, total_ratio, contador, r, c, total_cajas = 0, filas = 0, columnas = 0, tallas_id,pallets=0;
                List<Talla> tallas = new List<Talla>();
                List<Talla> tallas_bpdc = new List<Talla>();
               
                List<generos> lista_generos = new List<generos>();
                var ws = libro_trabajo.Worksheets.Add("PK");
                foreach (Pk item in lista)
                {
                    clave_packing = item.packing;
                    //archivo = " PK" + item.packing + " DAMAGES-EXTRAS FANTASY ";
                    archivo = item.nombre_archivo;
                    /*****INICIO CON DIRECCIONES, LOGO, ETC******/
                    ws.Cell("A2").Value = "FORTUNE FASHIONS BAJA, S.R.L. DE C.V.";
                    ws.Cell("A3").Value = "CALLE TORTUGA No 313-A";
                    ws.Cell("A4").Value = "MANEADERO CP 22790";
                    ws.Cell("A5").Value = "BAJA CALIFORNIA";
                    ws.Style.Font.FontSize = 8;

                    ws.Range("A2:A10").Style.Font.Bold = true;
                    ws.Range("A7:A10").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    //DIRECCIÓN DE ORIGEN
                    ws.Cell("A7").Value = "CUSTOMER: ";
                    ws.Cell("B7").Value = item.customer;
                    ws.Cell("A8").Value = "PO: ";
                    ws.Cell("B8").Value = " DAMAGES-EXTRAS FANTASY";
                    ws.Cell("A9").Value = "RETAILER: ";
                    ws.Range("B2:B10").Style.Font.Bold = true;
                    //IMAGEN AL CENTRO
                    ws.Range(1, 7, 1, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    var imagePath = Server.MapPath("/") + "/Content/img/LOGOFORTUNEEXCEL.png";
                    //var imagePath = @"C:\Users\melissa\source\repos\FortuneSys----\FortuneSystem\Content\img\LOGO FORTUNE.png";
                    //var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1")).Scale(0.30);
                    var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1"));
                    //PK ABAJO DE LA IMAGEN
                    ws.Cell("D7").Value = "PK: ";
                    ws.Cell("D7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell("D7").Style.Font.Bold = true;
                    ws.Cell("E7").Value = item.packing;

                    ws.Cell("E7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range("E7:F7").Merge();
                    ws.Range("E7", "F7").Style.Font.Bold = true;
                    ws.Range("D7", "F7").Style.Font.FontSize = 15;
                    ws.Range("E7:F7").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range("E7:F7").Style.Border.BottomBorderColor = XLColor.Black;
                    //DIRECCION DE DESTINO
                    ws.Cell("L2").Value = "SHIP TO: ";
                    ws.Cell("L3").Value = item.destino.nombre;
                    ws.Cell("L4").Value = item.destino.direccion;
                    ws.Cell("L5").Value = item.destino.ciudad + " " + item.destino.zip;
                    ws.Cell("L8").Value = "DATE:" + item.fecha;
                    ws.Range("L2", "L9").Style.Font.Bold = true;
                    var columna_a = ws.Range("A2", "A10");
                    ws.Rows("6").Height = 30;
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/

                    r = 11;
                    var headers = new List<String[]>();
                    List<String> titulos = new List<string>();//=  "ID", "P.O. NUM", "TYPE", "COLOR", "DESCRIPTION";
                    titulos.Add("ID"); titulos.Add("P.O."); titulos.Add("TYPE"); titulos.Add("STYLE"); titulos.Add("COLOR"); titulos.Add("DESCRIPTION"); titulos.Add("ORIGEN");
                    titulos.Add("%"); titulos.Add("SM"); titulos.Add("MD"); titulos.Add("LG"); titulos.Add("XL"); titulos.Add("2XL"); 
                    titulos.Add("PCS"); titulos.Add("BXS"); titulos.Add("PALLETS");
                    headers.Add(titulos.ToArray());
                    int total_titulos = (titulos.ToArray()).Length;
                    ws.Cell(r, 1).Value = headers;
                    for (int i = 1; i <= 16; i++)
                    {
                        ws.Cell(r, i).Style.Font.Bold = true;
                        ws.Cell(r, i).Style.Font.FontSize = 8;
                        ws.Cell(r, i).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                        ws.Cell(r, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(r, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Cell(r, i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(r, i).Style.Border.LeftBorderColor = XLColor.Black;
                        ws.Cell(r, i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        ws.Cell(r, i).Style.Border.RightBorderColor = XLColor.Black;
                        ws.Cell(r, i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(r, i).Style.Border.TopBorderColor = XLColor.Black;
                        ws.Cell(r, i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(r, i).Style.Border.BottomBorderColor = XLColor.Black;
                    }
                    /**********************************************************************************************************************************************************************************************/
                    /***********************C*A*B*E*C*E*R*A*S**D*E**L*A**T*A*B*L*A*********************************************************************************************************************************/
                    //int[] tallas_comparacion = tallas_id_temporal.ToArray(); 
                    foreach (Tarima t in item.lista_tarimas){pallets++;  }                    
                    int[] sumas_tallas = new int[7];
                    for (int i = 0; i < 7; i++) { sumas_tallas[i] = 0; }
                    for (int zz = 1; zz <= 10; zz++) { ws.Row(zz).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }
                    r = 12;
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************
                    var fila = new List<String[]>();
                    foreach (Tarima t in item.lista_tarimas)
                    {
                        estilos_total = 0;
                        foreach (Sample s in t.lista_fantasy){ estilos_total++; }
                        ws.Cell(r, 1).Value = t.id_tarima;
                        ws.Range(r, 1, (r + (estilos_total - 1)), 1).Merge();
                        var celdas_estilos = new List<String[]>();

                        foreach (Sample s in t.lista_fantasy){
                            List<String> datos = new List<string>();
                            datos.Add((s.pedido).Trim());
                            datos.Add(s.tipo);
                            datos.Add(s.estilo);
                            datos.Add(s.color);
                            datos.Add((s.descripcion).Trim());
                            datos.Add(s.origen);
                            datos.Add(s.porcentaje);
                            //if (s.talla_xs != 0) { datos.Add(Convert.ToString(s.talla_xs)); } else { datos.Add(" "); } sumas_tallas[0] += s.talla_xs; 
                            if (s.talla_s != 0) { datos.Add(Convert.ToString(s.talla_s)); } else { datos.Add(" "); } sumas_tallas[0] += s.talla_s;
                            if (s.talla_m != 0) { datos.Add(Convert.ToString(s.talla_m)); } else { datos.Add(" "); }sumas_tallas[1] += s.talla_m;
                            if (s.talla_l != 0) { datos.Add(Convert.ToString(s.talla_l)); } else { datos.Add(" "); }sumas_tallas[2] += s.talla_l;
                            if (s.talla_xl != 0) { datos.Add(Convert.ToString(s.talla_xl)); } else { datos.Add(" "); }sumas_tallas[3] += s.talla_xl;
                            if (s.talla_2x != 0) { datos.Add(Convert.ToString(s.talla_2x)); } else { datos.Add(" "); }sumas_tallas[4] += s.talla_2x;
                            //if (s.talla_3x != 0) { datos.Add(Convert.ToString(s.talla_3x)); } else { datos.Add(" "); }sumas_tallas[6] += s.talla_3x;
                            /*datos.Add(Convert.ToString(s.talla_s)); sumas_tallas[0] += s.talla_s;
                            datos.Add(Convert.ToString(s.talla_m)); sumas_tallas[1] += s.talla_m;
                            datos.Add(Convert.ToString(s.talla_l)); sumas_tallas[2] += s.talla_l;
                            datos.Add(Convert.ToString(s.talla_xl)); sumas_tallas[3] += s.talla_xl;
                            datos.Add(Convert.ToString(s.talla_2x)); sumas_tallas[4] += s.talla_2x;*/
                            datos.Add(Convert.ToString(s.total)); sumas_tallas[5] += s.total;
                            datos.Add(Convert.ToString(s.cajas)); sumas_tallas[6] += s.cajas;
                            datos.Add(s.attnto);
                            fila.Add(datos.ToArray());                            
                            bool isEmpty = !lista_generos.Any();
                            if (isEmpty){
                                generos ta = new generos();
                                ta.id_genero =s.id_genero;
                                ta.genero = (s.genero).Trim();
                                lista_generos.Add(ta);                                
                            }else{
                                int existe = 0;
                                foreach (generos index in lista_generos){
                                    if (index.id_genero == s.id_genero) { existe++; }
                                }
                                if (existe == 0){
                                    generos ta = new generos();
                                    ta.id_genero = s.id_genero;
                                    ta.genero = (s.genero).Trim();
                                    lista_generos.Add(ta);
                                }
                            }
                            celdas_estilos.Add(datos.ToArray());
                        }//FOREACH 
                        ws.Cell(r, 2).Value = celdas_estilos;//// <-------------THIS!!
                        ws.Cell(r, 16).Value = "*";//PALLET
                        ws.Range(r, 16, (r + estilos_total - 1), 16).Merge();
                        r = r + (estilos_total);
                    }//TARIMAS                              

                    string genero = "";
                    foreach (generos index in lista_generos){
                        genero += " "+index.genero;
                    }
                    ws.Cell(r, 1).Value = genero;
                    ws.Cell(r, 1).Style.Font.Bold = true;
                    ws.Cell(r, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(r, 1, r, 6).Merge();
                    ws.Range(r, 1, r, 16).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    ws.Cell(r,7).Value = "TOTAL";
                    ws.Cell(r, 7).Style.Font.Bold = true;
                    ws.Cell(r, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(r, 7, r, 8).Merge();

                    c = 9;
                    for (int i = 0; i < 7; i++){
                        ws.Cell(r, c).Value = sumas_tallas[i];
                        ws.Cell(r, c).Style.Font.Bold = true;
                        ws.Cell(r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                        ws.Cell(r, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        c++;
                    }
                   // ws.Cell(r, c).Value = pallets;
                    ws.Cell(r, c).Value = "*";
                    ws.Cell(r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    ws.Cell(r, c).Style.Font.Bold = true;
                    ws.Cell(r, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    for (int zz = (r + 1); zz <= (r + 30); zz++) { ws.Row(zz).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }
                    filas = r;
                    for (int j = 12; j <= r; j++)
                    {
                        for (int i = 1; i <= 16; i++)
                        {
                            ws.Cell(j, i).Style.Font.FontSize = 8;
                            ws.Cell(j, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            ws.Cell(j, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(j, i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            ws.Cell(j, i).Style.Border.LeftBorderColor = XLColor.Black;
                            ws.Cell(j, i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            ws.Cell(j, i).Style.Border.RightBorderColor = XLColor.Black;
                            ws.Cell(j, i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            ws.Cell(j, i).Style.Border.TopBorderColor = XLColor.Black;
                            ws.Cell(j, i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            ws.Cell(j, i).Style.Border.BottomBorderColor = XLColor.Black;
                            ws.Cell(j, i).Style.Alignment.WrapText = true;
                            ws.Rows(j, i).AdjustToContents();
                        }
                    }

                    filas += 2;
                    ws.Range(filas, 3, (filas + 2), 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(filas, 3, (filas + 2), 3).Style.Font.Bold = true;
                    ws.Range(filas, 9, (filas + 2), 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(filas, 9, (filas + 2), 9).Style.Font.Bold = true;
                    ws.Range(filas, 4, (filas + 2), 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(filas, 10, (filas + 2), 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    /*********************N*O**T*O*C*A*R***************************************/
                    ws.Cell(filas, 3).Value = "DRIVER NAME:";
                    ws.Cell(filas, 4).Value = item.conductor.driver_name;
                    ws.Cell(filas, 9).Value = "SHIPPING MANAGER:";
                    ws.Cell(filas, 10).Value = item.shipping_manager;
                    ws.Range(filas, 4, filas, 6).Merge();
                    ws.Range(filas, 4, filas, 6).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 4, filas, 6).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 10, filas, 12).Merge();
                    ws.Range(filas, 10, filas, 12).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 10, filas, 12).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 3).Value = "TRAILER/PLATES:";
                    ws.Cell(filas, 4).Value = item.conductor.tractor + "/" + item.conductor.plates;
                    ws.Cell(filas, 9).Value = "SEAL:";
                    ws.Cell(filas, 10).SetValue(item.seal);
                    ws.Range(filas, 4, filas, 6).Merge();
                    ws.Range(filas, 4, filas, 6).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 4, filas, 6).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 10, filas, 12).Merge();
                    ws.Range(filas, 10, filas, 12).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 10, filas, 12).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 3).Value = "CONTAINER/PLATES:";
                    ws.Cell(filas, 4).Value = item.contenedor.eco + "/" + item.contenedor.plates;
                    ws.Cell(filas, 9).Value = "REPLACEMENT:";
                    ws.Cell(filas, 10).SetValue(item.replacement);
                    ws.Range(filas, 4, filas, 6).Merge();
                    ws.Range(filas, 4, filas, 6).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 4, filas, 6).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 10, filas, 12).Merge();
                    ws.Range(filas, 10, filas, 12).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 10, filas, 12).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 4).Value = "DOCUMENTO CONTROLADO. ÚNICAMENTE SE PUEDE MODIFICAR POR EL SUPERVISOR DE SHIPPING";
                    ws.Cell(filas, 4).Style.Font.FontColor = XLColor.FromArgb(100, 100, 100);
                    filas += 2;
                    columnas = 2;

                    
                    var porcentajes = new List<String[]>();
                    List<String> p = new List<string>();
                    p.Add("ORIGIN"); p.Add("QTY"); p.Add("%");
                    porcentajes.Add(p.ToArray());
                    List<Fabricantes> totales_paises_estilo = new List<Fabricantes>();
                    List<Fabricantes> totales_paises = new List<Fabricantes>();
                    List<int> lista_estilos_stag = new List<int>();
                    int add = 0, total_paises = 0, iguales;
                    foreach (Tarima tarimas in item.lista_tarimas){
                        foreach (Sample s in tarimas.lista_fantasy){                           
                                lista_estilos_stag.Add(s.id_summary);                            
                        }
                    }
                    lista_estilos_stag = lista_estilos_stag.Distinct().ToList();
                    totales_paises_estilo = ds.buscar_paises_estilos_stag_recibos(lista_estilos_stag);
                    foreach (Fabricantes fa in totales_paises_estilo){
                        iguales = 0;
                        if (add == 0){
                            Fabricantes nf = new Fabricantes();
                            nf.cantidad = fa.cantidad;
                            nf.pais = fa.pais;
                            nf.percent = fa.percent;
                            nf.id = fa.id;
                            totales_paises.Add(nf);
                            add++;
                        }
                        else
                        {
                            foreach (Fabricantes f in totales_paises.ToList())
                            {
                                if (f.pais == fa.pais && f.percent == fa.percent)
                                {
                                    f.cantidad += fa.cantidad;
                                    iguales++;
                                }
                            }
                            if (iguales == 0)
                            {
                                Fabricantes nf = new Fabricantes();
                                nf.cantidad = fa.cantidad;
                                nf.pais = fa.pais;
                                nf.id = fa.id;
                                nf.percent = fa.percent;
                                totales_paises.Add(nf);
                            }
                            add++;
                        }
                    }

                    foreach (Fabricantes f in totales_paises) { total_paises += f.cantidad; }
                    foreach (Fabricantes f in totales_paises)
                    {
                        double z = ((Convert.ToDouble(f.cantidad) * 100) / Convert.ToDouble(total_paises));
                        f.porcentaje = Math.Round(z, MidpointRounding.ToEven);
                    }
                    List<Fabricantes> totales_paises_envio = new List<Fabricantes>();
                    foreach (Fabricantes f in totales_paises)
                    {
                        Fabricantes nf = new Fabricantes();
                        double x = ((Convert.ToDouble(sumas_tallas[sumas_tallas.Length - 2]) * Convert.ToDouble(f.cantidad)) / Convert.ToDouble(total_paises));
                        nf.cantidad = Convert.ToInt32(Math.Round(x, MidpointRounding.ToEven));
                        nf.pais = f.pais;
                        nf.percent = f.percent;
                        totales_paises_envio.Add(nf);
                    }
                    foreach (Fabricantes f in totales_paises_envio)
                    {
                        porcentajes.Add(new string[] { f.pais, (f.cantidad).ToString(), f.percent });
                    }
                    ws.Cell(filas, 2).Value = "%";
                    ws.Cell(filas, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    filas++;
                    ws.Cell(filas, 2).Value = porcentajes;
                }
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

        public void crear_excel_hottopic(List<Pk> lista)
        {
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string clave_packing = "";
            using (XLWorkbook libro_trabajo = new XLWorkbook())
            {
                string archivo = "";
                int estilos_total = 0, rows = 0, tarima_contador = 0, total_ratio, contador, r, c, total_cajas = 0, filas = 0, columnas = 0, tallas_id,renglones;
                List<Talla> tallas = new List<Talla>();
                List<Talla> tallas_bpdc = new List<Talla>();
                List<Talla> tallas_bpdc_restante = new List<Talla>();
                var ws = libro_trabajo.Worksheets.Add("PK");
                foreach (Pk item in lista)
                {
                    clave_packing = item.packing;
                    string[] nombre_archivo = (item.packing).Split('-');
                    archivo = " PK" + nombre_archivo[0] + " ";
                   //item.parte = consultas.AddOrdinal(Convert.ToInt32(item.parte)) + " Part";
                    /*****INICIO CON DIRECCIONES, LOGO, ETC******/
                    ws.Cell("A2").Value = "FORTUNE FASHIONS BAJA, S.R.L. DE C.V.";
                    ws.Cell("A3").Value = "CALLE TORTUGA No 313-A";
                    ws.Cell("A4").Value = "MANEADERO CP 22790";
                    ws.Cell("A5").Value = "BAJA CALIFORNIA";
                    ws.Style.Font.FontSize = 8;

                    ws.Range("A2:A10").Style.Font.Bold = true;
                    ws.Range("A7:A10").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    //DIRECCIÓN DE ORIGEN
                    ws.Cell("A7").Value = "CUSTOMER: ";
                    ws.Cell("B7").Value = item.customer;

                    int ex_label = ds.contar_labels(item.id_packing_list);
                    if (ex_label != 0){
                        List<Labels> lista_etiquetas = new List<Labels>();
                        lista_etiquetas = ds.obtener_etiquetas(item.id_packing_list);
                        ws.Cell("A8").Value = "P.O.: ";
                        string label = Regex.Replace(item.pedido, @"\s+", " ") + "(PO# ";
                        foreach (Labels l in lista_etiquetas) { label += " " + l.label; }
                        if (ex_label == 1){ label += " )" + " (With UCC Labels) " + item.parte; }
                        else { label += " )" + " (With TPM Labels) " + item.parte; }
                        //ws.Cell("B8").Value = label;
                        //archivo += label;
                    }else{
                        ws.Cell("A8").Value = "P.O.: ";
                        //ws.Cell("B8").Value = Regex.Replace(item.pedido, @"\s+", " ") + "(Without TPM Labels) " + item.parte;
                        archivo += Regex.Replace(item.pedido, @"\s+", " ") + "(Without TPM Labels) " + item.parte;
                    }
                    ws.Cell("B8").Value = item.nombre_archivo;
                    archivo = " PK " + nombre_archivo[0] + " ";
                    archivo += " "+item.nombre_archivo;

                    ws.Cell("A9").Value = "RETAILER: ";
                    ws.Cell("B9").Value = item.customer_po;
                    ws.Range("B2:B10").Style.Font.Bold = true;
                    //IMAGEN AL CENTRO
                    ws.Range(1, 7, 1, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    var imagePath = Server.MapPath("/") + "/Content/img/LOGOFORTUNEEXCEL.png";
                    //var imagePath = @"C:\Users\melissa\source\repos\FortuneSys----\FortuneSystem\Content\img\LOGO FORTUNE.png";
                    //var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1")).Scale(0.30);
                    var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1"));
                    //PK ABAJO DE LA IMAGEN
                    ws.Cell("D7").Value = "PK: ";
                    ws.Cell("D7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell("D7").Style.Font.Bold = true;
                    ws.Cell("E7").Value = item.packing;
                    ws.Cell("E7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range("E7:F7").Merge();
                    ws.Range("E7", "F7").Style.Font.Bold = true;
                    ws.Range("D7", "F7").Style.Font.FontSize = 15;
                    ws.Range("E7:F7").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range("E7:F7").Style.Border.BottomBorderColor = XLColor.Black;
                    //DIRECCION DE DESTINO
                    ws.Cell("L2").Value = "SHIP TO: ";
                    ws.Cell("L3").Value = item.destino.nombre;
                    ws.Cell("L4").Value = item.destino.direccion;
                    ws.Cell("L5").Value = item.destino.ciudad + " " + item.destino.zip;
                    ws.Cell("L8").Value = "DATE:" + item.fecha;
                    ws.Range("L2", "L10").Style.Font.Bold = true;
                    var columna_a = ws.Range("A2", "A10");
                    ws.Rows("6").Height = 30;
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    int contador_cabeceras = 0, contador_tallas = 0, pallets = 0, tiendas = 0, dc = 0, ppk = 0, bp = 0, ass = 0;
                    int suma_estilo, suma_cajas;
                    List<estilos> lista_descripciones_finales = new List<estilos>();
                    var headers = new List<String[]>();
                    List<String> titulos = new List<string>();//=  "ID", "P.O. NUM", "TYPE", "COLOR", "DESCRIPTION";
                    titulos.Add("ID"); titulos.Add("P.O. NUM"); titulos.Add("TYPE");
                    foreach (Tarima t in item.lista_tarimas)
                    { //REVISAN TIPOS DE EMPAQUE, DATOS, DC,ETC
                        foreach (estilos e in t.lista_estilos)
                        {
                            if (e.store != "N/A" && e.store != "NA") { tiendas++; }
                            if (e.dc != "0") { dc++; }
                            if (e.tipo_empaque == 5||e.tipo_empaque == 1) { bp++; }
                            if (e.tipo_empaque == 4||e.tipo_empaque == 2) { ppk++; }
                            //if (e.tipo_empaque == 3) { ass++; }
                        }
                    }
                    //if (ass != 0) { titulos.Add("ASSORTMENT"); }
                    titulos.Add("STYLE"); titulos.Add("COLOR"); titulos.Add("DESCRIPTION");
                    if (tiendas != 0) { titulos.Add("STORE"); }
                    if (dc != 0) { titulos.Add("DC"); }
                    if (ppk != 0) { titulos.Add("RATIO"); }
                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                    foreach (Tarima t in item.lista_tarimas)
                    {
                        foreach (estilos e in t.lista_estilos)
                        {
                            if (e.tipo_empaque != 3)
                            {
                                foreach (ratio_tallas ra in e.lista_ratio)
                                {
                                    bool isEmpty = !tallas.Any();
                                    if (isEmpty)
                                    {
                                        Talla ta = new Talla();
                                        ta.id_talla = ra.id_talla;
                                        ta.talla = ra.talla;
                                        tallas.Add(ta);
                                    }
                                    else
                                    {
                                        int existe = 0;
                                        foreach (Talla sizes in tallas)
                                        {
                                            if (sizes.id_talla == ra.id_talla) { existe++; }
                                        }
                                        if (existe == 0)
                                        {
                                            Talla ta = new Talla();
                                            ta.id_talla = ra.id_talla;
                                            ta.talla = ra.talla;
                                            tallas.Add(ta);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (estilos ea in e.assort.lista_estilos)
                                {
                                    foreach (ratio_tallas ras in ea.lista_ratio)
                                    {
                                        bool isEmpty = !tallas.Any();
                                        if (isEmpty)
                                        {
                                            Talla ta = new Talla();
                                            ta.id_talla = ras.id_talla;
                                            ta.talla = ras.talla;
                                            tallas.Add(ta);
                                        }
                                        else
                                        {
                                            int existe = 0;
                                            foreach (Talla sizes in tallas)
                                            {
                                                if (sizes.id_talla == ras.id_talla) { existe++; }
                                            }
                                            if (existe == 0)
                                            {
                                                Talla ta = new Talla();
                                                ta.id_talla = ras.id_talla;
                                                ta.talla = ras.talla;
                                                tallas.Add(ta);
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                    }
                    tallas = ds.obtener_tallas_pk(tallas);
                    //OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS<-----TALLAS
                    List<int> tallas_id_temporal = new List<int>();
                    contador_tallas = 0;
                    foreach (Talla sizes in tallas)
                    {
                        titulos.Add(sizes.talla); //AQUI AGREGO LAS TALLAS A LA CABECERA
                        tallas_id_temporal.Add(sizes.id_talla);
                        contador_tallas++;
                    }
                    titulos.Add("PCS"); titulos.Add("BXS"); titulos.Add("PALLETS");
                    foreach (string s in titulos) { contador_cabeceras++; }
                    headers.Add(titulos.ToArray());
                    int total_titulos = (titulos.ToArray()).Length;
                    ws.Cell(11, 1).Value = headers;
                    ws.Column(2).AdjustToContents();
                    ws.Column(5).AdjustToContents();
                    for (int i = 1; i <= total_titulos; i++)
                    {
                        ws.Cell(11, i).Style.Font.Bold = true;
                        ws.Cell(11, i).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                        ws.Cell(11, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(11, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.LeftBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.RightBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.TopBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.BottomBorderColor = XLColor.Black;
                    }
                    ws.Rows("6").Height = 30;
                    /**********************************************************************************************************************************************************************************************/
                    /***********************C*A*B*E*C*E*R*A*S**D*E**L*A**T*A*B*L*A*********************************************************************************************************************************/
                    //int[] tallas_comparacion = tallas_id_temporal.ToArray(); 
                    //foreach (Tarima tarimas in item.lista_tarimas) { pallets++; }
                    int[] sumas_tallas = new int[contador_tallas + 2];
                    for (int i = 0; i < contador_tallas + 2; i++) { sumas_tallas[i] = 0; }
                    for (int zz = 1; zz <= 10; zz++) { ws.Row(zz).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }

                    r = 12;

                    int estilos_extras = 0, estilos_normales = 0;
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************
                    foreach (Tarima tarimas in item.lista_tarimas)
                    {
                        suma_estilo = 0; suma_cajas = 0; estilos_total = 0; renglones = 0;
                        estilos_extras = 0; estilos_normales = 0;
                        var celdas_estilos_i = new List<String[]>();
                        var celdas_estilos = new List<String[]>();
                        List<int> indices = new List<int>();//BUSCO LOS INDEX DE LOS DC
                        //BUSCO LOS INDEX DE LOS DC Y CUENTO ESTILOS DE LA TARIMA
                        foreach (estilos estilo in tarimas.lista_estilos){//BUSCO LOS INDEX DE LOS DC Y CUENTO ESTILOS DE LA TARIMA
                            bool isEmpty = !indices.Any();
                            if (isEmpty){
                                indices.Add(estilo.index_dc);
                                estilos_total++;
                            }else{
                                int existe = 0;
                                foreach (int i in indices){
                                    if (i == estilo.index_dc) { existe++; }
                                }
                                if (existe == 0){
                                    indices.Add(estilo.index_dc);
                                    estilos_total++;
                                }
                            }
                        }
                        ws.Cell(r, 1).Value = tarimas.id_tarima; //<------------------------------------------------THIS! ATTENTION!!
                        //ws.Range(r, 1, (r + (estilos_total - 1)), 1).Merge();
                        foreach (Talla sizes in tallas) { sizes.total = 0; }

                        foreach (int i in indices){
                            foreach (estilos estilo in tarimas.lista_estilos){
                                if (estilo.index_dc == i){
                                    if (estilo.tipo == "EXT" || estilo.tipo == "DMG"){
                                        estilos_extras++;
                                    }else{
                                        estilos_normales++;
                                    }
                                }
                            }
                        }
                        if (estilos_normales != 0) { pallets++; }

                        tallas_bpdc = tallas;
                        tallas_bpdc_restante = tallas;
                        foreach (int i in indices)//INDICES
                        {
                            int contador_index = 0, bullpack = 0,cajas_t=0; suma_estilo = 0; suma_cajas = 0; 
                            
                            foreach (Talla sizes in tallas_bpdc) { sizes.total = 0; sizes.ratio = 0; }
                            foreach (Talla sizes in tallas_bpdc_restante) { sizes.total = 0; sizes.ratio = 0; }
                            string tipo_t=" ", estilo_t="", color_t="", descripcion_t="", ppk_t="",label_t=" ",store_t="",dc_t="",po_num_t="";
                            int cajas_extras=0;
                            foreach (estilos e in tarimas.lista_estilos){
                                if(i==e.index_dc){
                                    if (e.tipo != "NONE") { tipo_t = e.tipo; }
                                    if (e.label != "NONE") { label_t = " " + e.label; }
                                    if (e.tipo == "EXT" || e.tipo == "DMG") {
                                        label_t = " ";
                                        po_num_t =" ";
                                    }else{
                                        po_num_t = e.number_po;
                                    }
                                    estilo_t = (e.estilo).Trim();
                                    color_t = (e.color).Trim();
                                    descripcion_t = (e.descripcion).Trim();
                                    if (tiendas != 0) { store_t = e.store; }
                                    if (dc != 0) { dc_t = e.dc; }

                                    if (e.tipo == "EXT" || e.tipo == "DMG") {
                                        bullpack++;
                                        cajas_t = e.repeticiones;
                                        ppk_t = " ";
                                        foreach (Talla sizes in tallas_bpdc){
                                            if (sizes.id_talla == e.id_talla){
                                                sizes.total = e.boxes;
                                            }
                                        }
                                        ppk_t = "N/A";
                                    }
                                    else{
                                        if (e.tipo_empaque == 1){
                                            bullpack++;
                                            foreach (Talla sizes in tallas_bpdc){
                                                if (sizes.id_talla == e.id_talla){
                                                    sizes.total = e.boxes;
                                                }
                                            }
                                            ppk_t = "BULK";
                                        }else{
                                            List<ratio_tallas> ratios = ds.obtener_lista_ratio_hottopic(e.id_po_summary, e.number_po_ht, "2");
                                            contador = 0; string ppk_ratio = ""; total_ratio = 0;
                                            foreach (ratio_tallas ratio in ratios) { if (ratio.ratio != 0) { contador++; } }
                                            int contador2 = 0;
                                            foreach (Talla t in tallas)
                                            {
                                                foreach (ratio_tallas ratio in ratios)
                                                {
                                                    if (ratio.ratio != 0 && ratio.id_talla == t.id_talla)
                                                    {
                                                        contador2++; ppk_ratio += ratio.ratio;
                                                        
                                                    }
                                                }if (contador2 != contador) { ppk_ratio += "-"; }
                                            }
                                            cajas_t = e.boxes;
                                            ppk_t = ppk_ratio;
                                            foreach (Talla sizes in tallas_bpdc){
                                                foreach (ratio_tallas ratio in ratios){
                                                    if (sizes.id_talla == ratio.id_talla){
                                                        sizes.total = ratio.ratio * e.repeticiones * e.boxes;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }  
                            }//*****************************E*S*T*I*L*O*S*****I*N*D*I*C*E*S*****************************
                            if (bullpack!=0){
                                int llave = 0, primer_vuelta = 0, extras = 0;
                                do{                                    
                                    List<String> datos_dc_bp = new List<string>();
                                    suma_estilo = 0; suma_cajas = 0;
                                    datos_dc_bp.Add((item.number_po).Trim());
                                    datos_dc_bp.Add((tipo_t+" "+label_t +" "+ po_num_t).Trim());
                                    datos_dc_bp.Add(estilo_t);
                                    datos_dc_bp.Add(color_t);
                                    datos_dc_bp.Add(descripcion_t);
                                    if (tiendas != 0) { datos_dc_bp.Add(store_t); }
                                    if (dc != 0) { datos_dc_bp.Add(dc_t); }
                                    if (ppk != 0) { datos_dc_bp.Add(ppk_t); }
                                    int ii = 0;
                                    foreach (Talla sizes in tallas_bpdc){
                                        if (sizes.total != 0){
                                            int total = 0;
                                            if (tipo_t == "DMG" || tipo_t == "EXT"){
                                                total = sizes.total;
                                                if (extras == 0) {
                                                    suma_cajas= cajas_t;
                                                    extras++;                                                    
                                                }
                                            }else{
                                                if (primer_vuelta == 0) { total = sizes.total - (sizes.total % 50); }
                                                else { total = sizes.total; }
                                                if (total != 0)
                                                {
                                                    if (total > 50) { suma_cajas += (total / 50); }
                                                    else { suma_cajas++; }
                                                }
                                            }
                                            sumas_tallas[ii] += total;
                                            suma_estilo += total;
                                            if (total == 0){ datos_dc_bp.Add(" "); }
                                            else { datos_dc_bp.Add(Convert.ToString(total)); }
                                            sizes.total = sizes.total - total;
                                        }else {
                                            datos_dc_bp.Add(" ");
                                        }
                                        ii++;
                                    }
                                    primer_vuelta++;
                                    int ct = contador_tallas;
                                    sumas_tallas[ct] += suma_estilo;
                                    /*if (tipo_t == "DMG" || tipo_t == "EXT"){
                                        suma_cajas = 1;
                                    }else {
                                        suma_cajas += suma_estilo / 50;
                                        if ((suma_estilo % 50) != 0) { suma_cajas++; }
                                    }  */                                  
                                    sumas_tallas[ct + 1] += suma_cajas;
                                    datos_dc_bp.Add(Convert.ToString(suma_estilo));
                                    datos_dc_bp.Add(Convert.ToString(suma_cajas));
                                    if (suma_estilo != 0){
                                        celdas_estilos.Add(datos_dc_bp.ToArray());
                                        renglones++;
                                    }
                                    int existe = 0;
                                    foreach (Talla sizes in tallas_bpdc) {
                                        if (sizes.total != 0) {
                                            existe++;
                                        }
                                    }
                                    if (existe == 0) { llave++; }
                                } while (llave==0);
                            }else {
                                List<String> datos_dc_bp = new List<string>();
                                suma_estilo = 0; suma_cajas = 0;
                                datos_dc_bp.Add((item.number_po).Trim());
                                datos_dc_bp.Add((tipo_t + " " + label_t).Trim());
                                datos_dc_bp.Add(estilo_t);
                                datos_dc_bp.Add(color_t);
                                datos_dc_bp.Add(descripcion_t);
                                if (tiendas != 0) { datos_dc_bp.Add(store_t); }
                                if (dc != 0) { datos_dc_bp.Add(dc_t); }
                                datos_dc_bp.Add(ppk_t); 
                                int ii = 0;
                                foreach (Talla sizes in tallas_bpdc){
                                    if (sizes.total == 0) { datos_dc_bp.Add(" "); }
                                    else{
                                        sumas_tallas[ii] += sizes.total;
                                        suma_estilo += sizes.total;
                                        datos_dc_bp.Add(Convert.ToString(sizes.total));
                                    }
                                    ii++;
                                }
                                int ct = contador_tallas;
                                sumas_tallas[ct] += suma_estilo;
                                suma_cajas = cajas_t;                                
                                sumas_tallas[ct + 1] += suma_cajas;
                                datos_dc_bp.Add(Convert.ToString(suma_estilo));
                                datos_dc_bp.Add(Convert.ToString(suma_cajas));
                                if (suma_estilo != 0)
                                {
                                    celdas_estilos.Add(datos_dc_bp.ToArray());
                                    renglones++;
                                }
                            } 
                        }//*************I*N*D*I*C*E*S*******************************************************************                        
                        ws.Range(r, 1, (r + (renglones - 1)), 1).Merge();
                        ws.Cell(r, 2).Value = celdas_estilos;//// <-------------THIS!!
                                                             //ws.Cell(r, contador_cabeceras).Value = "1";//PALLET
                         if (estilos_normales != 0){
                                ws.Cell(r, contador_cabeceras).Value = "1";//PALLET
                         }else{
                                ws.Cell(r, contador_cabeceras).Value = "*";//PALLET
                         }
                            ws.Range(r, contador_cabeceras, (r + renglones - 1), contador_cabeceras).Merge();
                        r = r + (renglones);
                    }//************T*A*R*I*M*A*S*******************************************************************
                    //*************************************************************************T*A*R*I*M*A*S************************************************************************************
                    //***************************************************************************T*A*R*I*M*A*S********************************************************************************************************


                    /****************************************************************************************************************************************************************************************/
                    /****************************************************************************************************************************************************************************************/
                    /****************************************************************************************************************************************************************************************/
                    contador = 0;
                    string descripcion_final = "";
                    /*estilos desc = new estilos();
                    desc.descripcion = estilo.des
                    lista_descripciones_finales.Add();*/
                    foreach (Tarima tarimas in item.lista_tarimas)
                    {
                        foreach (estilos estilo in tarimas.lista_estilos)
                        {
                            if (estilo.tipo_empaque != 3)
                            {
                                bool isEmpty = !lista_descripciones_finales.Any();
                                if (isEmpty)
                                {
                                    estilos desc = new estilos();
                                    desc.id_estilo = estilo.id_estilo;
                                    desc.descripcion_final = estilo.descripcion_final;
                                    lista_descripciones_finales.Add(desc);
                                    descripcion_final += Regex.Replace(estilo.descripcion_final, @"\s+", " ") + " ";
                                }
                                else
                                {
                                    int existencia = 0;
                                    foreach (estilos e in lista_descripciones_finales)
                                    {
                                        if (e.descripcion_final == estilo.descripcion_final)
                                        {
                                            existencia++;
                                        }
                                    }
                                    if (existencia == 0)
                                    {
                                        estilos desc = new estilos();
                                        desc.id_estilo = estilo.id_estilo;
                                        desc.descripcion_final = estilo.descripcion_final;
                                        lista_descripciones_finales.Add(desc);
                                        descripcion_final += Regex.Replace(estilo.descripcion_final, @"\s+", " ") + " ";
                                    }
                                }
                            }
                            else
                            {//AQUI ASSORT
                                foreach (estilos ee in estilo.assort.lista_estilos)
                                {
                                    bool isEmpty = !lista_descripciones_finales.Any();
                                    if (isEmpty)
                                    {
                                        estilos desc = new estilos();
                                        desc.id_estilo = ee.id_estilo;
                                        desc.descripcion_final = ee.descripcion_final;
                                        lista_descripciones_finales.Add(desc);
                                        descripcion_final += Regex.Replace(ee.descripcion_final, @"\s+", " ") + " ";
                                    }
                                    else
                                    {
                                        int existencia = 0;
                                        foreach (estilos e in lista_descripciones_finales)
                                        {
                                            if (e.descripcion_final == ee.descripcion_final)
                                            {
                                                existencia++;
                                            }
                                        }
                                        if (existencia == 0)
                                        {
                                            estilos desc = new estilos();
                                            desc.id_estilo = ee.id_estilo;
                                            desc.descripcion_final = ee.descripcion_final;
                                            lista_descripciones_finales.Add(desc);
                                            descripcion_final += Regex.Replace(ee.descripcion_final, @"\s+", " ") + " ";
                                        }
                                    }
                                }
                            }
                        }
                    }

                    ws.Cell(r, 1).Value = descripcion_final;
                    ws.Cell(r, 1).Style.Font.Bold = true;
                    ws.Cell(r, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(r, 1, r, 4).Merge();
                    ws.Range(r, 1, r, 4).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);

                    ws.Cell(r, 5).Value = "TOTAL";
                    ws.Cell(r, 5).Style.Font.Bold = true;
                    ws.Cell(r, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    c = 6;
                    if (dc != 0) { c++; }
                    if (ppk != 0) { c++; }//POR EL RATIO
                    if (ass != 0) { c++; }
                    if (tiendas != 0) { c++; }
                    for (int i = 12; i < r; i++){
                        for (int j = 1; j <= c; j++){
                            ws.Column(c).AdjustToContents(r, c);
                        }
                    }
                    ws.Range(r, 5, r, c).Merge();
                    ws.Range(r, 5, r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    c++;

                    for (int i = 0; i < contador_tallas + 2; i++){
                        ws.Cell(r, c).Value = sumas_tallas[i];
                        ws.Cell(r, c).Style.Font.Bold = true;
                        ws.Cell(r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                        ws.Cell(r, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(r, c).Style.Alignment.WrapText = true;
                        c++;
                    }
                    ws.Cell(r, c).Value = pallets;
                    ws.Cell(r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    ws.Cell(r, c).Style.Font.Bold = true;
                    ws.Cell(r, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    for (int zz = (r + 1); zz <= (r + 30); zz++) { ws.Row(zz).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }
                    filas = r;

                   // ws.Column(2).AdjustToContents(12, 2);
                    ws.Column(3).AdjustToContents(12, 3);
                    ws.Column(4).AdjustToContents(12, 4);
                    for (int i = 12; i <= r; i++){
                        for (int j = 1; j <= c; j++){
                            ws.Cell(i, j).Style.Font.FontSize = 8;
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
                    filas += 2;
                    ws.Range(filas, 1, (filas + 2), 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(filas, 1, (filas + 2), 1).Style.Font.Bold = true;
                    ws.Range(filas, 7, (filas + 2), 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(filas, 7, (filas + 2), 7).Style.Font.Bold = true;
                    ws.Range(filas, 2, (filas + 2), 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(filas, 8, (filas + 2), 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(filas, 1).Value = "DRIVER NAME:";
                    ws.Cell(filas, 2).Value = item.conductor.driver_name;
                    ws.Cell(filas, 7).Value = "SHIPPING MANAGER:";
                    ws.Cell(filas, 8).Value = item.shipping_manager;
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 1).Value = "TRAILER/PLATES:";
                    ws.Cell(filas, 2).Value = item.conductor.tractor + "/" + item.conductor.plates;
                    ws.Cell(filas, 7).Value = "SEAL:";
                    ws.Cell(filas, 8).SetValue(item.seal);
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 1).Value = "CONTAINER/PLATES:";
                    ws.Cell(filas, 2).Value = item.contenedor.eco + "/" + item.contenedor.plates;
                    ws.Cell(filas, 7).Value = "REPLACEMENT:";
                    ws.Cell(filas, 8).SetValue(item.replacement);
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 3).Value = "DOCUMENTO CONTROLADO. ÚNICAMENTE SE PUEDE MODIFICAR POR EL SUPERVISOR DE SHIPPING";
                    ws.Cell(filas, 3).Style.Font.FontColor = XLColor.FromArgb(100, 100, 100);
                    filas += 2;
                    columnas = 2;
                                       
                    var porcentajes = new List<String[]>();
                    List<String> p = new List<string>();
                    p.Add("ORIGIN"); p.Add("QTY"); p.Add("%");
                    porcentajes.Add(p.ToArray());
                    List<Fabricantes> totales_paises_estilo = new List<Fabricantes>();
                    List<Fabricantes> totales_paises = new List<Fabricantes>();
                    List<int> lista_estilos_stag = new List<int>();
                    int add = 0, total_paises = 0, iguales;
                    foreach (Tarima tarimas in item.lista_tarimas)
                    {
                        foreach (estilos e in tarimas.lista_estilos)
                        {
                            if (e.tipo_empaque != 3)
                            {
                                lista_estilos_stag.Add(e.id_po_summary);
                            }
                            else
                            {
                                foreach (estilos ee in e.assort.lista_estilos)
                                {
                                    lista_estilos_stag.Add(ee.id_po_summary);
                                }
                            }
                        }
                    }
                    lista_estilos_stag = lista_estilos_stag.Distinct().ToList();
                    totales_paises_estilo = ds.buscar_paises_estilos_stag_recibos(lista_estilos_stag);
                    foreach (Fabricantes fa in totales_paises_estilo)
                    {
                        iguales = 0;
                        if (add == 0)
                        {
                            Fabricantes nf = new Fabricantes();
                            nf.cantidad = fa.cantidad;
                            nf.pais = fa.pais;
                            nf.percent = fa.percent;
                            nf.id = fa.id;
                            totales_paises.Add(nf);
                            add++;
                        }
                        else
                        {
                            foreach (Fabricantes f in totales_paises.ToList())
                            {
                                if (f.pais == fa.pais && f.percent == fa.percent)
                                {
                                    f.cantidad += fa.cantidad;
                                    iguales++;
                                }
                            }
                            if (iguales == 0)
                            {
                                Fabricantes nf = new Fabricantes();
                                nf.cantidad = fa.cantidad;
                                nf.pais = fa.pais;
                                nf.id = fa.id;
                                nf.percent = fa.percent;
                                totales_paises.Add(nf);
                            }
                            add++;
                        }
                    }

                    foreach (Fabricantes f in totales_paises) { total_paises += f.cantidad; }
                    foreach (Fabricantes f in totales_paises){
                        double z = ((Convert.ToDouble(f.cantidad) * 100) / Convert.ToDouble(total_paises));
                        f.porcentaje = Math.Round(z, MidpointRounding.ToEven);
                    }
                    List<Fabricantes> totales_paises_envio = new List<Fabricantes>();
                    foreach (Fabricantes f in totales_paises){
                        Fabricantes nf = new Fabricantes();
                        double x = ((Convert.ToDouble(sumas_tallas[sumas_tallas.Length - 2]) * Convert.ToDouble(f.cantidad)) / Convert.ToDouble(total_paises));
                        nf.cantidad = Convert.ToInt32(Math.Round(x, MidpointRounding.ToEven));
                        nf.pais = f.pais;
                        nf.percent = f.percent;
                        totales_paises_envio.Add(nf);
                    }
                    foreach (Fabricantes f in totales_paises_envio){
                        porcentajes.Add(new string[] { f.pais, (f.cantidad).ToString(), f.percent });
                    }
                    ws.Cell(filas, 2).Value = "%";
                    ws.Cell(filas, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    filas++;
                    ws.Cell(filas, 2).Value = porcentajes;
                }
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

        public void crear_excel_directo_fantasy(List<Pk> lista)
        {
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string clave_packing = "";
            using (XLWorkbook libro_trabajo = new XLWorkbook()){
                string archivo = "";
                int estilos_total = 0, rows = 0, tarima_contador = 0, total_ratio, contador, r, c, total_cajas = 0, filas = 0,columnas=0;

                var ws = libro_trabajo.Worksheets.Add("PK");
                foreach (Pk item in lista){
                    clave_packing = item.packing;
                    string[] nombre_archivo = (item.packing).Split('-');
                    archivo = " PK " + nombre_archivo[0] + " ";
                    //item.parte = consultas.AddOrdinal(Convert.ToInt32(item.parte))+" Part";
                    /*****INICIO CON DIRECCIONES, LOGO, ETC******/
                    ws.Cell("A2").Value = "FORTUNE FASHIONS BAJA, S.R.L. DE C.V.";
                    ws.Cell("A3").Value = "CALLE TORTUGA No 313-A";
                    ws.Cell("A4").Value = "MANEADERO CP 22790";
                    ws.Cell("A5").Value = "BAJA CALIFORNIA";
                    ws.Style.Font.FontSize = 8;

                    ws.Range("A2:A10").Style.Font.Bold = true;
                    
                    ws.Range("A7:A10").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    //DIRECCIÓN DE ORIGEN
                    ws.Cell("A7").Value = "CUSTOMER: ";
                    ws.Cell("B7").Value = item.customer;

                    int ex_label = ds.contar_labels(item.id_packing_list);
                    if (ex_label != 0){
                        List<Labels> lista_etiquetas = new List<Labels>();
                        lista_etiquetas = ds.obtener_etiquetas(item.id_packing_list);
                        ws.Cell("A8").Value = "P.O.: ";
                        string label = Regex.Replace(item.pedido, @"\s+", " ") + "(PO# ";
                        foreach (Labels l in lista_etiquetas) { label += " " + l.label; }
                        if (ex_label == 1) { label += " )" + " (With UCC Labels) " + item.parte; }
                        else { label += " )" + " (With TPM Labels) " + item.parte; }
                        // ws.Cell("B8").Value = label;
                        // archivo += label;
                    }else{
                        ws.Cell("A8").Value = "P.O.: ";
                        // ws.Cell("B8").Value = Regex.Replace(item.pedido, @"\s+", " ") + "(Without UCC Labels) " + item.parte;
                        archivo += Regex.Replace(item.pedido, @"\s+", " ") + "(Without UCC Labels) " + item.parte;
                    }
                    ws.Cell("B8").Value = item.nombre_archivo;
                    archivo = " PK " + nombre_archivo[0] + " ";
                    archivo += " " + item.nombre_archivo;

                    ws.Cell("A9").Value = "RETAILER: ";
                    ws.Cell("B9").Value = item.customer_po;
                    ws.Range("B2:B10").Style.Font.Bold = true;
                    //IMAGEN AL CENTRO
                    ws.Range(1, 7, 1, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    var imagePath = Server.MapPath("/") + "/Content/img/LOGOFORTUNEEXCEL.png";
                    //var imagePath = @"C:\Users\melissa\source\repos\FortuneSys----\FortuneSystem\Content\img\LOGO FORTUNE.png";
                    //var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1")).Scale(0.30);
                    var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1"));
                    //PK ABAJO DE LA IMAGEN
                    ws.Cell("D7").Value = "PK: ";
                    ws.Cell("D7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell("D7").Style.Font.Bold = true;
                    ws.Cell("E7").Value = item.packing;
                    ws.Cell("E7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range("E7:F7").Merge();
                    ws.Range("E7", "F7").Style.Font.Bold = true;
                    ws.Range("D7", "F7").Style.Font.FontSize = 15;
                    ws.Range("E7:F7").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range("E7:F7").Style.Border.BottomBorderColor = XLColor.Black;
                    //DIRECCION DE DESTINO
                    ws.Cell("L2").Value = "SHIP TO: ";
                    ws.Cell("L3").Value = item.destino.nombre;
                    ws.Cell("L4").Value = item.destino.direccion;
                    ws.Cell("L5").Value = item.destino.ciudad + " " + item.destino.zip;
                    ws.Cell("L8").Value = "DATE:" + item.fecha;
                    ws.Range("L2", "L10").Style.Font.Bold = true;
                    var columna_a = ws.Range("A2", "A10");
                    ws.Rows("6").Height = 30;
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    /****************T*A*B*L*A********************************************************************************************************************************************************/
                    int contador_cabeceras = 0, contador_tallas = 0, pallets = 0, tiendas = 0, dc = 0, ppk = 0, bp = 0, ass = 0;
                    int suma_estilo, suma_cajas;
                    List<estilos> lista_descripciones_finales = new List<estilos>();
                    var headers = new List<String[]>();
                    List<String> titulos = new List<string>();//=  "ID", "P.O. NUM", "TYPE", "COLOR", "DESCRIPTION";
                    titulos.Add("ID"); titulos.Add("P.O. NUM"); titulos.Add("TYPE"); titulos.Add("STYLE"); titulos.Add("COLOR"); titulos.Add("DESCRIPTION");

                    string[] letras_tallas = { "SM", "MD", "LG", "XL", "2XL" };
                    List<int> ids_tallas = new List<int>();
                    for (int i = 0; i < 5; i++){
                        ids_tallas.Add(consultas.buscar_talla(letras_tallas[i]));
                        titulos.Add(letras_tallas[i]);
                    }
                    contador_tallas = 5;
                    titulos.Add("PCS"); titulos.Add("BXS"); titulos.Add("PALLETS");
                    foreach (string s in titulos) { contador_cabeceras++; }
                    headers.Add(titulos.ToArray());

                    int total_titulos = (titulos.ToArray()).Length;
                    ws.Cell(11, 1).Value = headers;
                    ws.Column(2).AdjustToContents();
                    ws.Column(5).AdjustToContents();
                    for (int i = 1; i <= total_titulos; i++){
                        ws.Cell(11, i).Style.Font.Bold = true;
                        ws.Cell(11, i).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                        ws.Cell(11, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(11, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.LeftBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.RightBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.TopBorderColor = XLColor.Black;
                        ws.Cell(11, i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(11, i).Style.Border.BottomBorderColor = XLColor.Black;
                    }
                    ws.Rows("6").Height = 30;
                    /**********************************************************************************************************************************************************************************************/
                    /***********************C*A*B*E*C*E*R*A*S**D*E**L*A**T*A*B*L*A*********************************************************************************************************************************/
                    //int[] tallas_comparacion = tallas_id_temporal.ToArray(); 
                    /*foreach (Tarima tarimas in item.lista_tarimas) {  pallets++; }*///CONTEO DE TARIMAS!!!!!
                    //----------SUMA DE TALLAS AL FINAL------------------SUMA DE TALLAS AL FINAL
                    int[] sumas_tallas = new int[7];
                    //----------SUMA DE TALLAS AL FINAL------------------SUMA DE TALLAS AL FINAL
                    for (int i = 0; i < 7; i++) { sumas_tallas[i] = 0; }
                    for (int zz = 1; zz <= 10; zz++) { ws.Row(zz).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }
                    r = 12;
                    int estilos_extras = 0, estilos_normales = 0;
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************
                    //*************************************************************************T*A*R*I*M*A*S********************************************************************************************************

                    estilos_total = 0;
                    estilos_extras = 0; estilos_normales = 0;
                    var celdas_estilos_i = new List<String[]>();
                    var celdas_estilos = new List<String[]>();
                    List<int> index = new List<int>();//BUSCO LOS INDEX DE LOS DC

                    foreach (estilos estilo in item.lista_estilos) { index.Add(estilo.index_dc); }
                    index = index.Distinct().ToList();

                    foreach (int i in index) { estilos_normales++; estilos_total++; pallets++; }

                    ws.Cell(r, 1).Value = " ";
                    ws.Cell(r, 14).Value = pallets;
                    ws.Range(r, 1, (r + (estilos_total - 1)), 1).Merge();
                    ws.Range(r, 3, (r + (estilos_total - 1)), 3).Merge();
                    ws.Range(r, 14, (r + (estilos_total - 1)), 14).Merge();
                    string ponum_f = "", estilo_f = "", descripcion_f = "", color_f = "";
                    int estilos_capturados = 0;
                    foreach (int indice in index){ //ppks y bp
                        int es_otro = 0;
                        int contador_index = 0; suma_estilo = 0; suma_cajas = 0;
                        foreach (estilos estilo in item.lista_estilos){
                            if (estilo.index_dc == indice){
                                ponum_f = estilo.number_po;
                                estilo_f = (estilo.estilo).Trim();
                                color_f = (ds.obtener_color_estilo_fantasy(estilo.id_estilo)).Trim();
                                descripcion_f = (estilo.descripcion).Trim();
                            }
                        }
                        List<String> datos = new List<string>();
                        datos.Add(ponum_f);
                        datos.Add(" ");
                        datos.Add(estilo_f);
                        datos.Add(color_f);
                        datos.Add(descripcion_f);
                        //ids_tallas
                        int i = 0; suma_estilo = 0; suma_cajas = 0;
                        foreach (int t in ids_tallas){
                            foreach (estilos e in item.lista_estilos){
                                if (e.index_dc == indice && e.id_talla == t){
                                    if (e.boxes != 0) { datos.Add(" "); }
                                    else {datos.Add(Convert.ToString(e.boxes)); }
                                    suma_estilo +=e.boxes;
                                    sumas_tallas[i] += e.boxes;
                                }// IF INDICE
                            }//ESTILOS  
                            i++;
                        }
                        suma_cajas = suma_estilo / 6;
                        datos.Add(Convert.ToString(suma_estilo));
                        datos.Add(Convert.ToString(suma_cajas));
                        sumas_tallas[i] += suma_estilo; i++;
                        sumas_tallas[i] += suma_cajas;
                        estilos_capturados++;
                        celdas_estilos.Add(datos.ToArray());
                    }//INDICES



                    ws.Cell(r, 2).Value = celdas_estilos;//// <-------------THIS!!
                    /*
                    if (estilos_normales != 0){
                        ws.Cell(r, contador_cabeceras).Value = "1";//PALLET
                    }else{
                        ws.Cell(r, contador_cabeceras).Value = "*";//PALLET
                    }*/
                    ws.Range(r, contador_cabeceras, (r + estilos_total - 1), contador_cabeceras).Merge();
                    r = r + (estilos_total);
                    //************T*A*R*I*M*A*S*******************************************************************
                    //*************************************************************************T*A*R*I*M*A*S************************************************************************************
                    //***************************************************************************T*A*R*I*M*A*S********************************************************************************************************


                    /****************************************************************************************************************************************************************************************/
                    /****************************************************************************************************************************************************************************************/
                    /****************************************************************************************************************************************************************************************/
                    contador = 0;
                    string descripcion_final = "";
                    /*estilos desc = new estilos();
                    desc.descripcion = estilo.des
                    lista_descripciones_finales.Add();*/
                    foreach (estilos estilo in item.lista_estilos)
                    {
                        bool isEmpty = !lista_descripciones_finales.Any();
                        if (isEmpty)
                        {
                            estilos desc = new estilos();
                            desc.id_estilo = estilo.id_estilo;
                            desc.descripcion = estilo.descripcion_final;
                            lista_descripciones_finales.Add(desc);
                            descripcion_final += Regex.Replace(estilo.descripcion_final, @"\s+", " ") + " ";
                        }
                        else
                        {
                            int existencia = 0;
                            foreach (estilos e in lista_descripciones_finales)
                            {
                                if (e.id_estilo == estilo.id_estilo)
                                {
                                    existencia++;
                                }
                            }
                            if (existencia == 0)
                            {
                                estilos desc = new estilos();
                                desc.id_estilo = estilo.id_estilo;
                                desc.descripcion = estilo.descripcion_final;
                                lista_descripciones_finales.Add(desc);
                                descripcion_final += Regex.Replace(estilo.descripcion_final, @"\s+", " ") + " ";
                            }
                        }
                    }


                    ws.Cell(r, 1).Value = descripcion_final;
                    ws.Cell(r, 1).Style.Font.Bold = true;
                    ws.Cell(r, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(r, 1, r, 4).Merge();
                    ws.Range(r, 1, r, 4).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);

                    ws.Cell(r, 5).Value = "TOTAL";
                    ws.Cell(r, 5).Style.Font.Bold = true;
                    ws.Cell(r, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    c = 6;
                    for (int i = 12; i < r; i++)
                    {
                        for (int j = 1; j <= c; j++)
                        {
                            ws.Column(c).AdjustToContents(r, c);
                        }
                    }
                    ws.Range(r, 5, r, c).Merge();
                    ws.Range(r, 5, r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    c++;

                    for (int i = 0; i < contador_tallas + 2; i++)
                    {
                        ws.Cell(r, c).Value = sumas_tallas[i];
                        ws.Cell(r, c).Style.Font.Bold = true;
                        ws.Cell(r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                        ws.Cell(r, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        c++;
                    }
                    ws.Cell(r, c).Value = pallets;
                    ws.Cell(r, c).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    ws.Cell(r, c).Style.Font.Bold = true;
                    ws.Cell(r, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    for (int zz = (r + 1); zz <= (r + 30); zz++) { ws.Row(zz).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255); }
                    filas = r;

                    ws.Column(2).AdjustToContents(12, 2);
                    ws.Column(3).AdjustToContents(12, 3);
                    ws.Column(4).AdjustToContents(12, 4);
                    for (int i = 12; i <= r; i++)
                    {
                        for (int j = 1; j <= c; j++)
                        {
                            ws.Cell(i, j).Style.Font.FontSize = 8;
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


                    filas += 2;
                    ws.Range(filas, 1, (filas + 2), 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(filas, 1, (filas + 2), 1).Style.Font.Bold = true;
                    ws.Range(filas, 7, (filas + 2), 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(filas, 7, (filas + 2), 7).Style.Font.Bold = true;
                    ws.Range(filas, 2, (filas + 2), 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(filas, 8, (filas + 2), 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(filas, 1).Value = "DRIVER NAME:";
                    ws.Cell(filas, 2).Value = item.conductor.driver_name;
                    ws.Cell(filas, 7).Value = "SHIPPING MANAGER:";
                    ws.Cell(filas, 8).Value = item.shipping_manager;
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 1).Value = "TRAILER/PLATES:";
                    ws.Cell(filas, 2).Value = item.conductor.tractor + "/" + item.conductor.plates;
                    ws.Cell(filas, 7).Value = "SEAL:";
                    ws.Cell(filas, 8).SetValue(item.seal);
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 1).Value = "CONTAINER/PLATES:";
                    ws.Cell(filas, 2).Value = item.contenedor.eco + "/" + item.contenedor.plates;
                    ws.Cell(filas, 7).Value = "REPLACEMENT:";
                    ws.Cell(filas, 8).SetValue(item.replacement);
                    ws.Range(filas, 2, filas, 3).Merge();
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 2, filas, 3).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(filas, 8, filas, 11).Merge();
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(filas, 8, filas, 11).Style.Border.BottomBorderColor = XLColor.Black;
                    filas++;
                    ws.Cell(filas, 3).Value = "DOCUMENTO CONTROLADO. ÚNICAMENTE SE PUEDE MODIFICAR POR EL SUPERVISOR DE SHIPPING";
                    ws.Cell(filas, 3).Style.Font.FontColor = XLColor.FromArgb(100, 100, 100);
                    filas += 2;
                    columnas = 2;
                    var porcentajes = new List<String[]>();
                    List<String> p = new List<string>();
                    p.Add("ORIGIN"); p.Add("QTY"); p.Add("%");
                    porcentajes.Add(p.ToArray());
                    /*List<Fabricantes> totales_paises_estilo = new List<Fabricantes>();
                    List<Fabricantes> totales_paises = new List<Fabricantes>();
                    int add = 0, total_paises = 0, iguales;
                    foreach (Tarima tarimas in item.lista_tarimas){
                        totales_paises_estilo = ds.buscar_paises_estilos(tarimas.lista_estilos);
                        foreach (Fabricantes fa in totales_paises_estilo){
                            iguales = 0;
                            if (add == 0){
                                Fabricantes nf = new Fabricantes();
                                nf.cantidad = fa.cantidad;
                                nf.pais = fa.pais;
                                nf.percent = fa.percent;
                                totales_paises.Add(nf);
                                add++;
                            }else{
                                foreach (Fabricantes f in totales_paises.ToList()){
                                    if (f.pais == fa.pais){
                                        f.cantidad = fa.cantidad;
                                        iguales++;
                                    }
                                }
                                if (iguales == 0){
                                    Fabricantes nf = new Fabricantes();
                                    nf.cantidad = fa.cantidad;
                                    nf.pais = fa.pais;
                                    nf.percent = fa.percent;
                                    totales_paises.Add(nf);
                                }
                                add++;
                            }
                        }
                    }
                    foreach (Fabricantes f in totales_paises) { total_paises += f.cantidad; }
                     foreach (Fabricantes f in totales_paises){
                        double z = ((Convert.ToDouble(f.cantidad) * 100) / Convert.ToDouble(total_paises));
                        f.porcentaje = Math.Round(z, MidpointRounding.ToEven);
                    }
                    List<Fabricantes> totales_paises_envio = new List<Fabricantes>();
                    foreach (Fabricantes f in totales_paises){
                        Fabricantes nf = new Fabricantes();
                        double x = ((Convert.ToDouble(sumas_tallas[sumas_tallas.Length - 2]) * Convert.ToDouble(f.cantidad)) / Convert.ToDouble(total_paises));
                        nf.cantidad = Convert.ToInt32(Math.Round(x, MidpointRounding.ToEven));
                        nf.pais = f.pais;
                        nf.percent = f.percent;
                        totales_paises_envio.Add(nf);
                    }
                    foreach (Fabricantes f in totales_paises_envio){
                        porcentajes.Add(new string[] { f.pais, (f.cantidad).ToString(), f.percent });
                    }
                    ws.Cell(filas, 2).Value = "%";
                    ws.Cell(filas, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    filas++;*/
                    ws.Cell(filas, 2).Value = porcentajes;

                }
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
               


        public JsonResult eliminar_packing_pk(string packing,string tipo){
            //obtener_el pk
            string pk = ds.obtener_clave_packing(Convert.ToInt32(packing));
            //cambiarlo
            ds.cambiar_pk_packing_list(Convert.ToInt32(packing),"NOT ASIGNED");
            //guardarlo
            ds.guardar_packing_borrado(pk, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),tipo);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_pks_disponibles(string tipo){
            int indice_pk = 0, year_pk = ds.buscar_year_packing_configuracion(); ;
            string pk_nombre;
            if (tipo == "9") {
                indice_pk = ds.buscar_contador_packing_ext_configuracion();
                indice_pk++;
                pk_nombre = ((Convert.ToString(indice_pk).PadLeft(4, '0')) + "-" + year_pk.ToString() + " (EXT-DMG) FFB");
            } else {
                indice_pk = ds.buscar_contador_packing_normal_configuracion();
                indice_pk++;
                pk_nombre = ((Convert.ToString(indice_pk).PadLeft(4, '0')) + "-" + year_pk.ToString() + " FFB");
            }
            return Json(Json(new{
                pks= ds.obtener_pks_borrados(tipo),
                next_pk=pk_nombre,
            }), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_informacion_edicion_pk(string pk){
            return Json(ds.obtener_informacion_editar_pk(Convert.ToInt32(pk)), JsonRequestBehavior.AllowGet);
        }

        public JsonResult guardar_informacion_edicion_pk(string id, string sello, string replacement, string conductor, string contenedor,string direccion)
        {
            ds.actualizar_datos_pk(id, sello, replacement, conductor, contenedor,direccion,"1");
            return Json("", JsonRequestBehavior.AllowGet);
        }
        //****************************************************************************************************************************************************************
        //*********REPORTES***********************************************************************************************************************************************
        //****************************************************************************************************************************************************************
        //string fecha_inicio, fecha_final;
        public JsonResult fechas_reporte(string inicio, string final){
            Session["fechas"] = inicio + "*" + final;
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public void excel_reporte_po_abierto(){
            string[] fechas = (Convert.ToString(Session["fechas"])).Split('*');
            
            List<Pk> lista = ds.obtener_pedido_cantidades(fechas[0], fechas[1]);
            int row = 1, column = 2, suma_totales_talla = 0, total_talla, suma_estilo, existe_talla, total_cabeceras = 4;
            using (XLWorkbook libro_trabajo = new XLWorkbook()){
                var ws = libro_trabajo.Worksheets.Add("Open orders");
                ws.Cell(row, column).Value = "OPEN WIP";
                ws.Cell(row, column).Style.Font.FontSize = 22;
                ws.Cell(row, column).Style.Font.Bold= true;
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(230, 184, 183);
                row++;

                foreach (Pk p in lista){
                    total_cabeceras = 4;
                    List<string> tallas_letras = new List<string>();
                    List<Int32> talla_tempo = new List<Int32>();
                    int[] tallas_ids;
                    var headers = new List<String[]>();
                    List<String> titulos = new List<string>();
                    titulos.Add("CANCEL DATE"); titulos.Add("STYLE"); titulos.Add("ITEM DESCRIPTION"); titulos.Add("COLOR");
                    foreach (estilos e in p.lista_estilos){
                        foreach (ratio_tallas r in e.lista_ratio){
                            bool isEmpty = !tallas_letras.Any();
                            if (isEmpty){
                                talla_tempo.Add(r.id_talla);
                                tallas_letras.Add(r.talla);
                            }else{
                                existe_talla = 0;
                                foreach (string s in tallas_letras){
                                    if (s == r.talla){
                                        existe_talla++;
                                    }
                                }
                                if (existe_talla == 0){
                                    talla_tempo.Add(r.id_talla);
                                    tallas_letras.Add(r.talla);
                                }
                            }
                        }
                    }
                    foreach (string s in tallas_letras){
                        titulos.Add(s);
                        total_cabeceras++;
                    }
                    tallas_ids = talla_tempo.ToArray();
                    titulos.Add("(+/-)"); total_cabeceras++;
                    headers.Add(titulos.ToArray());
                   
                    int tempo = 0; suma_totales_talla = 0;
                    foreach (estilos e in p.lista_estilos){
                        tempo++;
                        foreach (ratio_tallas r in e.lista_ratio){
                            suma_totales_talla += r.total_talla;
                        }
                    }
                    if (tempo != 0) {
                        if (suma_totales_talla >= 5) {
                            row++;
                            ws.Cell(row, 2).Value = p.pedido;
                            ws.Cell(row, 2).Style.Fill.BackgroundColor = XLColor.FromArgb(230, 184, 183);
                            ws.Cell(row, 2).Style.Font.Bold = true;
                            ws.Cell(row, 2).Style.Font.FontSize = 14;
                            row++;
                            ws.Cell(row, 1).Value = headers;
                            ws.Range(row, 1, row, total_cabeceras).Style.Font.Bold = true;
                            ws.Range(row, 1, row, total_cabeceras).Style.Font.FontSize = 14;
                            ws.Range(row, 1, row, total_cabeceras).Style.Fill.BackgroundColor = XLColor.FromArgb(230, 184, 183);
                            row++;
                        }                        
                    }
                    suma_totales_talla = 0;
                    foreach (estilos e in p.lista_estilos){
                        foreach (ratio_tallas r in e.lista_ratio){
                            suma_totales_talla += r.total_talla;
                        }
                        if (suma_totales_talla >= 5) {                            
                            var celdas_estilos = new List<String[]>();
                            List<String> datos = new List<string>();
                            datos.Add(p.cancel_date);
                            datos.Add(e.estilo);
                            datos.Add(e.descripcion);
                            datos.Add(e.color);
                            suma_estilo = 0;
                            foreach (int i in tallas_ids){
                                total_talla = 0;
                                foreach (ratio_tallas r in e.lista_ratio){
                                    if (r.id_talla == i){
                                        total_talla = r.total_talla;
                                    }
                                }
                                suma_estilo += total_talla;
                                if (total_talla == 0){
                                    datos.Add("0");
                                }else{
                                    datos.Add("-" + (total_talla).ToString());
                                }
                            }
                            datos.Add("-" + (suma_estilo).ToString());
                            celdas_estilos.Add(datos.ToArray());
                            ws.Cell(row, 1).Value = celdas_estilos;
                            ws.Range(row, 1, row, total_cabeceras).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                            ws.Range(row, 1, row, total_cabeceras).Style.Border.LeftBorderColor = XLColor.White;
                            ws.Range(row, 1, row, total_cabeceras).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                            ws.Range(row, 1, row, total_cabeceras).Style.Border.RightBorderColor = XLColor.White;
                            ws.Range(row, 1, row, total_cabeceras).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                            ws.Range(row, 1, row, total_cabeceras).Style.Border.TopBorderColor = XLColor.White;
                            ws.Range(row, 1, row, total_cabeceras).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                            ws.Range(row, 1, row, total_cabeceras).Style.Border.BottomBorderColor = XLColor.White;
                            row++;
                        }
                       
                    }
                    
                }
                ws.Rows().AdjustToContents();
                ws.Columns().AdjustToContents();
                ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                /***********D*O*W*N*L*O*A*D*************************************************************************************************************************************************************************/
                HttpResponse httpResponse = System.Web.HttpContext.Current.Response;
                httpResponse.Clear();
                httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                httpResponse.AddHeader("content-disposition", "attachment;filename=\"Open orders " + fechas[0] + "-" + fechas[1] + ".xlsx\"");
                // Flush the workbook to the Response.OutputStream
                using (MemoryStream memoryStream = new MemoryStream()){
                    libro_trabajo.SaveAs(memoryStream);
                    memoryStream.WriteTo(httpResponse.OutputStream);
                    memoryStream.Close();
                }
                httpResponse.End();
            }
        }
        public JsonResult po_reporte(string po){
            Session["po_reporte"] = po ;
            return Json("", JsonRequestBehavior.AllowGet);
        }      
        public void excel_reporte_status(){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string pedido = Convert.ToString(Session["po_reporte"]);
            int id_pedido = consultas.buscar_pedido(pedido);
            List<Estilo_Pedido> lista_estilos = ds.obtener_estilos_pedido_status(id_pedido);
            int row = 1, aux,cabeceras;
            using (XLWorkbook libro_trabajo = new XLWorkbook()){ //Regex.Replace(pedido, @"\s+", " "); 
                var ws = libro_trabajo.Worksheets.Add("Report");

                ws.Cell(row, 3).Value = Regex.Replace("ORDER "+pedido, @"\s+", " "); 
                ws.Cell(row, 3).Style.Font.FontSize = 22;
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(230, 184, 183);
                row+=2;
                foreach (Estilo_Pedido ep in lista_estilos){
                    cabeceras = 7;
                    List<Int32> id_tallas_tempo = new List<Int32>();
                    List<Int32> cantidades_tallas_tempo = new List<Int32>();
                    List<string> tallas_tempo = new List<string>();
                    foreach (Talla t in ep.totales_pedido){
                        bool isEmpty = !id_tallas_tempo.Any();
                        if (isEmpty){
                            id_tallas_tempo.Add(t.id_talla);
                            tallas_tempo.Add(t.talla);
                            cantidades_tallas_tempo.Add(t.total);
                            cabeceras++;
                        }else{
                            aux = 0;
                            foreach (int i in id_tallas_tempo){
                                if (i == t.id_talla) { aux++; }
                            }
                            if (aux == 0){
                                id_tallas_tempo.Add(t.id_talla);
                                tallas_tempo.Add(t.talla);
                                cantidades_tallas_tempo.Add(t.total);
                                cabeceras++;
                            }
                        }
                    }//OBTENER TALLAS DE EL ESTILO
                    var headers = new List<String[]>();
                    List<String> titulos = new List<string>();
                    titulos.Add("ID"); titulos.Add("STYLE"); titulos.Add("ITEM DESCRIPTION"); titulos.Add("COLOR"); titulos.Add("TYPE");
                    int titulo_c = 0;
                    foreach (string s in tallas_tempo) { titulo_c++; titulos.Add(s); }
                    titulos.Add("PACKING"); titulos.Add("DATE");
                    if (titulo_c != 0) { 
                        headers.Add(titulos.ToArray());
                        ws.Cell(row, 1).Value = headers;
                        ws.Range(row,1,row, cabeceras).Style.Fill.BackgroundColor = XLColor.FromArgb(230, 184, 183);
                        ws.Range(row, 1, row, cabeceras).Style.Font.FontSize = 13;
                        ws.Range(row, 1, row, cabeceras).Style.Font.Bold = true;
                        row++; //AGHREGAR CABECERA TABLA
                        //AGREGAR ROWS DE INFORMACIÓN POR PK DE CADA ESTILO
                        foreach (Packing_Estilo pe in ep.lista_pk){
                            var celdas = new List<String[]>();
                            List<String> datos = new List<string>();
                            datos.Add((ep.id_estilo).ToString());
                            datos.Add(ep.estilo);
                            datos.Add(ep.descripcion);
                            datos.Add(ep.color);
                            datos.Add(pe.tipo);
                            int j = 0;
                            foreach (int i in id_tallas_tempo){
                                aux = 0;
                                foreach (Talla te in pe.lista_enviados){
                                    if (te.id_talla == i){
                                        aux = te.total;
                                    }
                                }
                                if (aux == 0){
                                    datos.Add(" ");
                                }else{
                                    datos.Add((aux).ToString());
                                    cantidades_tallas_tempo[j] = cantidades_tallas_tempo[j] - aux;
                                }
                                j++;
                            }
                            datos.Add(pe.package);
                            datos.Add(pe.fecha);
                            celdas.Add(datos.ToArray());
                            ws.Cell(row, 1).Value = celdas;
                            ws.Range(row, 1, row, cabeceras).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                            ws.Range(row, 1, row, cabeceras).Style.Border.LeftBorderColor = XLColor.White;
                            ws.Range(row, 1, row, cabeceras).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                            ws.Range(row, 1, row, cabeceras).Style.Border.RightBorderColor = XLColor.White;
                            ws.Range(row, 1, row, cabeceras).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                            ws.Range(row, 1, row, cabeceras).Style.Border.TopBorderColor = XLColor.White;
                            ws.Range(row, 1, row, cabeceras).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                            ws.Range(row, 1, row, cabeceras).Style.Border.BottomBorderColor = XLColor.White;
                            row++;                            
                        }

                        var celdas2 = new List<String[]>();
                        List<String> datos2 = new List<string>();
                        datos2.Add("+/-");
                        foreach (int i in cantidades_tallas_tempo){
                            if (i < 0){
                                datos2.Add("+" + (i * -1).ToString());
                            }else{
                                datos2.Add("-" + i.ToString());
                            }
                        }
                        celdas2.Add(datos2.ToArray());
                        ws.Cell(row, 5).Value = celdas2;
                        ws.Range(row, 1, row, cabeceras).Style.Font.Bold = true;
                        ws.Range(row, 1, row, cabeceras).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                        ws.Range(row, 1, row, cabeceras).Style.Border.LeftBorderColor = XLColor.White;
                        ws.Range(row, 1, row, cabeceras).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                        ws.Range(row, 1, row, cabeceras).Style.Border.RightBorderColor = XLColor.White;
                        ws.Range(row, 1, row, cabeceras).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                        ws.Range(row, 1, row, cabeceras).Style.Border.TopBorderColor = XLColor.White;
                        ws.Range(row, 1, row, cabeceras).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                        ws.Range(row, 1, row, cabeceras).Style.Border.BottomBorderColor = XLColor.White;
                        row++; row++;
                    }
                }
                ws.Rows().AdjustToContents();
                ws.Columns().AdjustToContents();                
                ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    /***********D*O*W*N*L*O*A*D*************************************************************************************************************************************************************************/
                HttpResponse httpResponse = System.Web.HttpContext.Current.Response;
                httpResponse.Clear();
                httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                httpResponse.AddHeader("content-disposition", "attachment;filename=\"Order status " +pedido+ ".xlsx\"");
                // Flush the workbook to the Response.OutputStream
                using (MemoryStream memoryStream = new MemoryStream()){
                    libro_trabajo.SaveAs(memoryStream);
                    memoryStream.WriteTo(httpResponse.OutputStream);
                    memoryStream.Close();
                }
                httpResponse.End();
            }
        }
        public JsonResult estilo_reporte(string estilo){
            Session["estilo_reporte"] = estilo;
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public void excel_reporte_por_estilos(){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string estilo = Convert.ToString(Session["estilo_reporte"]);
            int id_estilo = consultas.obtener_estilo_id(estilo);
            List<Estilo_PO> lista_estilos = ds.obtener_pedidos_po_estilo(id_estilo);
            int row = 1;
            using (XLWorkbook libro_trabajo = new XLWorkbook()){ //Regex.Replace(pedido, @"\s+", " "); 
                var ws = libro_trabajo.Worksheets.Add("Report");
                ws.Cell(row, 2).Value = Regex.Replace("STYLE: "+estilo, @"\s+", " ");
                ws.Cell(row, 2).Style.Font.FontSize = 22;
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(196, 215, 155);
                row++;
                //CABECERAS TABLA
                var headers = new List<String[]>();
                List<String> titulos = new List<string>();
                titulos.Add("PO"); titulos.Add("STYLE"); titulos.Add("ITEM DESCRIPTION"); titulos.Add("COLOR"); titulos.Add("PCS"); titulos.Add("STATUS");
                headers.Add(titulos.ToArray());
                ws.Cell(row, 1).Value = headers;
                ws.Range(row, 1,row,6).Style.Fill.BackgroundColor = XLColor.FromArgb(196, 215, 155);
                ws.Range(row, 1, row, 6).Style.Font.Bold = true;
                row++; //AGHREGAR CABECERA TABLA
                foreach (Estilo_PO e in lista_estilos) {
                    var celdas = new List<String[]>();
                    List<String> datos = new List<string>();
                    datos.Add(e.pedido);
                    datos.Add(e.estilo);
                    datos.Add(e.descripcion);
                    datos.Add(e.color);
                    if (e.total < 0){
                        datos.Add((e.total * -1).ToString());
                    }else{
                        datos.Add(e.total.ToString());
                    }
                    //datos.Add((e.total).ToString());
                    datos.Add(e.estado);
                    celdas.Add(datos.ToArray());
                    ws.Cell(row, 1).Value = celdas;
                    ws.Range(row, 1, row, 6).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                    ws.Range(row, 1, row, 6).Style.Border.LeftBorderColor = XLColor.White;
                    ws.Range(row, 1, row, 6).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                    ws.Range(row, 1, row, 6).Style.Border.RightBorderColor = XLColor.White;
                    ws.Range(row, 1, row, 6).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                    ws.Range(row, 1, row, 6).Style.Border.TopBorderColor = XLColor.White;
                    ws.Range(row, 1, row, 6).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                    ws.Range(row, 1, row, 6).Style.Border.BottomBorderColor = XLColor.White;
                    row++;
                }              
                ws.Rows().AdjustToContents();
                ws.Columns().AdjustToContents();
                ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                /***********D*O*W*N*L*O*A*D*************************************************************************************************************************************************************************/
                HttpResponse httpResponse = System.Web.HttpContext.Current.Response;
                httpResponse.Clear();
                httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                httpResponse.AddHeader("content-disposition", "attachment;filename=\"Orders with " + estilo + ".xlsx\"");
                // Flush the workbook to the Response.OutputStream
                using (MemoryStream memoryStream = new MemoryStream()){
                    libro_trabajo.SaveAs(memoryStream);
                    memoryStream.WriteTo(httpResponse.OutputStream);
                    memoryStream.Close();
                }
                httpResponse.End();

            }

        }
        public JsonResult year_report(string year){
            Session["year_reporte"] = year;
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public void listado_year() {
            string year = Convert.ToString(Session["year_reporte"]);
            List<Shipping_pk> lista_pk = ds.obtener_listado_packing_year(year);
            excel_listado_packing(lista_pk, "SHIPMENT REPORTS "+year);
        }
        public void listado_diario(){
            List<Shipping_pk> lista_pk = ds.obtener_listado_packing_diario();
            excel_listado_packing(lista_pk,"SHIPMENT REPORTS "+DateTime.Now.ToString("yyyy-MM-dd"));
        }
        public void excel_listado_packing(List<Shipping_pk> lista_pk,string titulo){
            //string year = Convert.ToString(Session["year_reporte"]);
            //List<Shipping_pk> lista_pk = ds.obtener_listado_packing_year(year);
            int row = 1, column = 1;
            using (XLWorkbook libro_trabajo = new XLWorkbook()){ //Regex.Replace(pedido, @"\s+", " "); 
                var ws = libro_trabajo.Worksheets.Add("Report");
                ws.Cell(row, column).Value = titulo;
                ws.Row(1).Style.Font.FontSize = 20;
                ws.Range(1, 1, 1, 7).Merge();
                ws.Row(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Row(1).Style.Fill.BackgroundColor = XLColor.FromArgb(196, 215, 155);
                ws.Row(1).Style.Font.Bold = true;
                row++;
                //CABECERAS TABLA
                var headers = new List<String[]>();
                List<String> titulos = new List<string>();
                titulos.Add("PK"); titulos.Add("PO"); titulos.Add("SHIP TO"); titulos.Add("PCS"); titulos.Add("BXS"); titulos.Add("PALLETS");titulos.Add("# SHIPPING");
                headers.Add(titulos.ToArray());
                ws.Cell(row, 1).Value = headers;
                ws.Row(row).Style.Font.FontSize = 13;
                ws.Row(row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(196, 215, 155);
                ws.Row(row).Style.Font.Bold = true;
                row++; //AGHREGAR CABECERA TABLA
                foreach (Shipping_pk e in lista_pk){
                    var celdas = new List<String[]>();
                    List<String> datos = new List<string>();
                    datos.Add(e.packing);
                    datos.Add(e.pedido);
                    datos.Add(e.destino);
                    datos.Add((e.piezas).ToString());
                    datos.Add((e.cajas).ToString());
                    datos.Add((e.pallets).ToString());
                    datos.Add((e.num_envio).ToString());
                    celdas.Add(datos.ToArray());
                    ws.Cell(row, 1).Value = celdas;
                    ws.Range(row, 1, row, 7).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                    ws.Range(row, 1, row, 7).Style.Border.LeftBorderColor = XLColor.White;
                    ws.Range(row, 1, row, 7).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                    ws.Range(row, 1, row, 7).Style.Border.RightBorderColor = XLColor.White;
                    ws.Range(row, 1, row, 7).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                    ws.Range(row, 1, row, 7).Style.Border.TopBorderColor = XLColor.White;
                    ws.Range(row, 1, row, 7).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                    ws.Range(row, 1, row, 7).Style.Border.BottomBorderColor = XLColor.White;
                    row++;
                }
                ws.Rows().AdjustToContents();
                ws.Columns().AdjustToContents();
                ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                /***********D*O*W*N*L*O*A*D*************************************************************************************************************************************************************************/
                HttpResponse httpResponse = System.Web.HttpContext.Current.Response;
                httpResponse.Clear();
                httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                httpResponse.AddHeader("content-disposition", "attachment;filename=\"Shipping Report.xlsx\"");
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
        public void excel_reporte_status_orden(){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string pedido = Convert.ToString(Session["po_reporte"]);
            int id_pedido = consultas.buscar_pedido(pedido);
            List<Pk> lista = ds.obtener_pedido_cantidades_orden(id_pedido);
            int row = 1, column = 1, suma_totales_talla = 0, total_talla, suma_estilo, existe_talla, total_cabeceras ;
            using (XLWorkbook libro_trabajo = new XLWorkbook()){
                var ws = libro_trabajo.Worksheets.Add("Report");
                List<Int32> talla_tempo = new List<Int32>();
                int[] tallas_ids;
                
                foreach (Pk p in lista){
                    total_cabeceras = 5;
                    List<string> tallas_letras = new List<string>();
                    var headers = new List<String[]>();
                    List<String> titulos = new List<string>();
                    titulos.Add("CANCEL DATE"); titulos.Add("STYLE"); titulos.Add("ITEM DESCRIPTION"); titulos.Add("COLOR");
                    foreach (estilos e in p.lista_estilos){
                        foreach (ratio_tallas r in e.lista_ratio){
                            bool isEmpty = !tallas_letras.Any();
                            if (isEmpty){
                                talla_tempo.Add(r.id_talla);
                                tallas_letras.Add(r.talla);
                            }else{
                                existe_talla = 0;
                                foreach (string s in tallas_letras){
                                    if (s == r.talla){
                                        existe_talla++;
                                    }
                                }
                                if (existe_talla == 0){
                                    talla_tempo.Add(r.id_talla);
                                    tallas_letras.Add(r.talla);
                                }
                            }
                        }
                    }
                    foreach (string s in tallas_letras){
                        titulos.Add(s);
                        total_cabeceras++;
                    }
                    tallas_ids = talla_tempo.ToArray();
                    titulos.Add("(+/-)");
                    headers.Add(titulos.ToArray());
                    row++;
                    ws.Cell(row, 2).Value ="MISSING PIECES "+ p.pedido;
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(196, 215, 155);
                    ws.Row(row).Style.Font.Bold = true;
                    ws.Row(row).Style.Font.FontSize = 14;
                    row++;
                    ws.Cell(row, 1).Value = headers;
                    ws.Range(row, 1, row, total_cabeceras).Style.Font.Bold = true;
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(196, 215, 155);
                    ws.Range(row, 1, row, total_cabeceras).Style.Font.FontSize = 14;
                    row++;
                    foreach (estilos e in p.lista_estilos){
                        foreach (ratio_tallas r in e.lista_ratio){
                            suma_totales_talla += r.total_talla;
                        }
                        //if (suma_totales_talla >= 1){
                            var celdas_estilos = new List<String[]>();
                            List<String> datos = new List<string>();
                            datos.Add(p.cancel_date);
                            datos.Add(e.estilo);
                            datos.Add(e.descripcion);
                            datos.Add(e.color);
                            suma_estilo = 0;
                            foreach (int i in tallas_ids){
                                total_talla = 0;
                                foreach (ratio_tallas r in e.lista_ratio){
                                    if (r.id_talla == i){
                                        total_talla = r.total_talla;
                                    }
                                }
                                suma_estilo += total_talla;
                                if (total_talla == 0){
                                    datos.Add("0");
                                }else{
                                    datos.Add("-" + (total_talla).ToString());
                                }
                            }
                            datos.Add("-" + (suma_estilo).ToString());
                            celdas_estilos.Add(datos.ToArray());
                            ws.Cell(row, 1).Value = celdas_estilos;
                            ws.Range(row, 1, row, total_cabeceras).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                            ws.Range(row, 1, row, total_cabeceras).Style.Border.LeftBorderColor = XLColor.White;
                            ws.Range(row, 1, row, total_cabeceras).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                            ws.Range(row, 1, row, total_cabeceras).Style.Border.RightBorderColor = XLColor.White;
                            ws.Range(row, 1, row, total_cabeceras).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                            ws.Range(row, 1, row, total_cabeceras).Style.Border.TopBorderColor = XLColor.White;
                            ws.Range(row, 1, row, total_cabeceras).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                            ws.Range(row, 1, row, total_cabeceras).Style.Border.BottomBorderColor = XLColor.White;
                        //}
                        row++;
                    }                    
                    ws.Cell(row, 2).Value = "TOTAL PO";
                    ws.Cell(row, 3).Value = p.total_po;
                    ws.Cell(row, 4).Value = "SHIPPED";
                    ws.Cell(row, 5).Value = p.total_enviado;
                    ws.Cell(row, 6).Value = "TOTAL";
                    if (p.total_po - p.total_enviado > 0){
                        ws.Cell(row, 7).Value = "-" + (p.total_po - p.total_enviado).ToString();
                    }else {
                        ws.Cell(row, 7).Value = ((p.total_po - p.total_enviado)*-1).ToString();
                    }
                    
                    ws.Range(row,1,row,10).Style.Font.FontSize = 13;
                    ws.Row(row).Style.Font.Bold = true;
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(196, 215, 155);
                    row++;
                }
                ws.Rows().AdjustToContents();
                ws.Columns().AdjustToContents();
                ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                /***********D*O*W*N*L*O*A*D*************************************************************************************************************************************************************************/
                HttpResponse httpResponse = System.Web.HttpContext.Current.Response;
                httpResponse.Clear();
                httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                httpResponse.AddHeader("content-disposition", "attachment;filename=\"Open orders " + pedido + ".xlsx\"");
                // Flush the workbook to the Response.OutputStream
                using (MemoryStream memoryStream = new MemoryStream()){
                    libro_trabajo.SaveAs(memoryStream);
                    memoryStream.WriteTo(httpResponse.OutputStream);
                    memoryStream.Close();
                }
                httpResponse.End();
            }
        }
        
        public void estados_pedidos(List<Shipping_pk> lista_pk, string titulo){
            //string year = Convert.ToString(Session["year_reporte"]);
            List<Po> lista_pedidos = ds.obtener_lista_pedidos();
            int row = 1, column = 1;
            using (XLWorkbook libro_trabajo = new XLWorkbook())
            { //Regex.Replace(pedido, @"\s+", " "); 
                var ws = libro_trabajo.Worksheets.Add("Report");
                
                //CABECERAS TABLA
                var headers = new List<String[]>();
                List<String> titulos = new List<string>();
                titulos.Add("PO"); titulos.Add("CUSTOMER PO"); titulos.Add("STYLE"); titulos.Add("CANCEL DATE"); titulos.Add("TOTAL UNITS"); titulos.Add("STATUS"); titulos.Add("CUSTOMER");
                headers.Add(titulos.ToArray());
                ws.Cell(row, 1).Value = headers;
                ws.Range(row,1,row,7).Style.Fill.BackgroundColor = XLColor.FromArgb(196, 215, 155);
                row++; //AGREGAR CABECERA TABLA
                foreach (Po e in lista_pedidos){
                    var celdas = new List<String[]>();
                    List<String> datos = new List<string>();
                    datos.Add(e.pedido);
                    datos.Add(e.customer_po);
                    datos.Add((e.estilos).ToString());
                    datos.Add(e.fecha_cancelacion);
                    datos.Add((e.total).ToString());
                    datos.Add(e.estado);
                    datos.Add(e.customer);
                    celdas.Add(datos.ToArray());
                    ws.Cell(row, 1).Value = celdas;
                    if (e.estado == "CANCELLED") {
                        ws.Row(row).Style.Font.FontColor= XLColor.FromArgb(150, 54, 52);
                    }
                    if (e.estado == "COMPLETED") {
                        ws.Row(row).Style.Font.FontColor= XLColor.FromArgb(57, 71, 29);
                    }
                    if (e.estado == "INCOMPLETE") {
                        ws.Row(row).Style.Font.FontColor= XLColor.FromArgb(54, 96, 146);
                    }
                    ws.Range(row, 1, row, 7).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 7).Style.Border.LeftBorderColor = XLColor.FromArgb(178, 178, 178); 
                    ws.Range(row, 1, row, 7).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 7).Style.Border.RightBorderColor = XLColor.FromArgb(178, 178, 178);
                    ws.Range(row, 1, row, 7).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 7).Style.Border.TopBorderColor = XLColor.FromArgb(178, 178, 178);
                    ws.Range(row, 1, row, 7).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 7).Style.Border.BottomBorderColor = XLColor.FromArgb(178, 178, 178);
                    row++;
                }
                ws.Rows().AdjustToContents();
                ws.Columns().AdjustToContents();
                //ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                /***********D*O*W*N*L*O*A*D*************************************************************************************************************************************************************************/
                HttpResponse httpResponse = System.Web.HttpContext.Current.Response;
                httpResponse.Clear();
                httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                httpResponse.AddHeader("content-disposition", "attachment;filename=\"Shipping Report Orders.xlsx\"");
                // Flush the workbook to the Response.OutputStream
                using (MemoryStream memoryStream = new MemoryStream()){
                    libro_trabajo.SaveAs(memoryStream);
                    memoryStream.WriteTo(httpResponse.OutputStream);
                    memoryStream.Close();
                }
                httpResponse.End();
            }
        }
        public JsonResult buscar_contenedores(){
            return Json(ds.obtener_contenedores(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_modificar_contenedor(string id){
            return Json(ds.obtener_contenedor_edicion(id), JsonRequestBehavior.AllowGet);
        }
        public JsonResult eliminar_contenedor(string id){
            ds.borrar_contenedor(id);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult enviar_informacion_contenedor(string eco, string plates){
            ds.guardar_nuevo_contenedor(eco, plates);
            return Json(ds.obtener_carriers(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult enviar_informacion_contenedor_edicion(string id,string eco, string plates){
            ds.guardar_contenedor_edicion(id,eco, plates);
            return Json(ds.obtener_carriers(), JsonRequestBehavior.AllowGet);
        }
        //************E*D*I*C*I*O*N***************************
        //************E*D*I*C*I*O*N***************************
        public JsonResult edicion_pk_completo(string id){
             Session["pk_edicion"] = id;
             return Json("", JsonRequestBehavior.AllowGet);            
        }
        public ActionResult abrir_edicion_packing_list() {
            int tipo_packing = ds.obtener_tipo_packing(Convert.ToInt32(Session["pk_edicion"]));
            string vista = "";
            switch (tipo_packing){
                case 1:
                case 2:                
                case 7:
                case 9:
                    vista ="edicion_pk";
                    break;
                case 3:
                    vista = "edicion_pk_samples";
                    break;                    
                case 5:
                    vista = "edicion_pk_returns";
                    break;
                case 6:
                    vista = "edicion_pk_fantasy_extras";
                    break;
                case 8:
                    vista = "edicion_pk_directo_fantasy";
                    break;
                case 4:
                    vista = "edicion_pk_semanal_fantasy";
                    break;
            }
           //return View("Index");
           return View(vista);
        }        
        public JsonResult buscar_informacion_packing(){
            int pedido = ds.obtener_pedido_packing(Convert.ToInt32(Session["pk_edicion"]));
            List<estilo_shipping> e = ds.lista_estilos(Convert.ToString(pedido));
            var result = Json(new{
                estilos=e,
                packing = ds.obtener_informacion_editar_packing_completo(Convert.ToInt32(Session["pk_edicion"])),                
                assorts = ds.lista_assortments_pedido(pedido),
                tallas = ds.obtener_lista_tallas_pedido(e),
                estilos_packing=ds.lista_estilos_packing_edicion(Convert.ToInt32(Session["pk_edicion"]))
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_informacion_packing_fantasy_extdmg(){
            int pedido = ds.obtener_pedido_packing(Convert.ToInt32(Session["pk_edicion"]));
            List<estilo_shipping> e = ds.lista_estilos(Convert.ToString(pedido));
            var result = Json(new{                
                packing = ds.obtener_informacion_editar_packing_completo(Convert.ToInt32(Session["pk_edicion"])),
                extras_packing = ds.obtener_lista_extras_fantasy_packing(Convert.ToInt32(Session["pk_edicion"]))
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_informacion_packing_samples(){
            //int pedido = ds.obtener_pedido_packing(Convert.ToInt32(Session["pk_edicion"]));
            //List<estilo_shipping> e = ds.lista_estilos(Convert.ToString(pedido));
            var result = Json(new{
                packing = ds.obtener_informacion_editar_packing_completo(Convert.ToInt32(Session["pk_edicion"])),
                samples_packing = ds.lista_estilos_samples_pk(Convert.ToInt32(Session["pk_edicion"])),
                indices_pk = ds.lista_indices_samples_pk(Convert.ToInt32(Session["pk_edicion"])),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_informacion_packing_returns(){
            int pedido = ds.obtener_pedido_packing(Convert.ToInt32(Session["pk_edicion"]));
            List<estilo_shipping> e = ds.lista_estilos(Convert.ToString(pedido));
            var result = Json(new{
                packing = ds.obtener_informacion_editar_packing_completo(Convert.ToInt32(Session["pk_edicion"])),
                returns_packing = ds.obtener_lista_returns_pk(Convert.ToInt32(Session["pk_edicion"])),
                indices_pk = ds.lista_indices_returns_pk(Convert.ToInt32(Session["pk_edicion"])),
                tallas = ds.obtener_lista_tallas_pedido(e),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_informacion_packing_directo_fantasy(){
            DatosFantasy df = new DatosFantasy();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            /*string[] tallas = { "", "SM", "MD", "LG", "XL", "2XL" };
            List<int> ids_tallas = new List<int>();
            for (int i = 1; i < 6; i++) {
                ids_tallas.Add(consultas.buscar_talla(tallas[i]));
            }*/
            //List<Talla> lista_tallas = ds.obtener_tallas_fantasy();
            var result = Json(new{
                packing = ds.obtener_informacion_editar_packing_completo(Convert.ToInt32(Session["pk_edicion"])),
                estilos_packing = ds.buscar_lista_estilos_pedido(Convert.ToInt32(Session["pk_edicion"])),
                tallas_ids = ds.obtener_tallas_fantasy(),
                indices = ds.lista_indices_directo_fantasy(Convert.ToInt32(Session["pk_edicion"])),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult guardar_edicion_pk_estilos(string pedido,string packing, string summary, string box, string talla, string cantidad_size,string store, string type, string dc, string empaque, string sobrante, string label, string num_ppk, string indice, string tipo_packing, string shipping)
        {   //                                              'pedido':'  packing':',   'summary':     'box':'    ,'id_talla     'cantidad_size','store''      type': 'dc_summary'             empaque'      'sobrante'    'label'           'num_ppk':   ''indice':'tipo_packing''shipping'
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string[] summarys = summary.Split('*'), cajas = box.Split('*'), cantidades = cantidad_size.Split('*'), labels = label.Split('*'), tallas = talla.Split('*');
            string[] stores = store.Split('*'), tipos = type.Split('*'), empaques = empaque.Split('*'), dcs = dc.Split('*'), sobrantes = sobrante.Split('*');
            string[] indices = indice.Split('*'), nums_ppks = num_ppk.Split('&'),shippings=shipping.Split('&');
            int repeat=0;
            int packing_id = Convert.ToInt32(packing);
            int id_pedido = Convert.ToInt32(pedido);
            //OBTENER EL TOTAL DE PIEZAS ENVIADAS SIN INCLUIR LO DE ESTE ENVIO (LO QUE YA ESTA GUARDADO) PARA PODER HACER CUENTAS COMO SE DEBE
            int total_enviado = ds.obtener_total_enviado_pedido_exclusivo(id_pedido, packing_id), total_pedido = ds.obtener_total_pedido(id_pedido);
            string number_po = ds.obtener_number_po_pedido(id_pedido);
            int shipping_id=0,total_piezas_pk = 0;
            for (int i = 1; i < summarys.Length; i++){
                int id_estilo = consultas.obtener_estilo_summary(Convert.ToInt32(summarys[i]));
                if (tipos[i] != "DMG" && tipos[i] != "EXT"){
                    switch (empaques[i]){
                        case "1"://TIPO DE EMPAQUE BLPACK
                            if (empaques[i] == "1" && dcs[i] == "0"){//SIN DC
                                string[] cantidades_talla = cantidades[i].Split('&');
                                for (int j = 1; j < cantidades_talla.Length; j++){
                                    total_piezas_pk += (Convert.ToInt32(cantidades_talla[j]));
                                }
                            }
                            if (empaques[i] == "1" && dcs[i] != "0"){//CON DC
                                string[] cantidades_tallas = cantidades[i].Split('&');
                                for (int k = 1; k < tallas.Length; k++){
                                    int r = ds.buscar_piezas_empaque_bull(Convert.ToInt32(summarys[i]), Convert.ToInt32(tallas[k]));
                                    total_piezas_pk += (r * Convert.ToInt32(cantidades_tallas[k]));
                                }
                            }
                            break;
                        case "5"://TIPO DE EMPAQUE BLPACK
                            if (empaques[i] == "5" && dcs[i] == "0"){//SIN DC
                                string[] cantidades_talla = cantidades[i].Split('&');
                                for (int j = 1; j < cantidades_talla.Length; j++){
                                    total_piezas_pk += (Convert.ToInt32(cantidades_talla[j]));
                                }
                            }
                            if (empaques[i] == "5" && dcs[i] != "0"){//CON DC
                                string[] cantidades_tallas = cantidades[i].Split('&');
                                for (int k = 1; k < tallas.Length; k++){
                                    int r = ds.buscar_piezas_empaque_bulls(Convert.ToInt32(summarys[i]), Convert.ToInt32(tallas[k]),nums_ppks[i]);
                                    total_piezas_pk += (r * Convert.ToInt32(cantidades_tallas[k]));
                                }
                            }
                            break;
                        case "2":
                            List<ratio_tallas> ratios = ds.obtener_lista_ratio(Convert.ToInt32(summarys[i]), id_estilo, 2);
                            foreach (ratio_tallas r in ratios){
                                total_piezas_pk += (Convert.ToInt32(cajas[i]) * r.ratio);
                            }
                            break;
                        case "3":
                            Assortment a = ds.assortment_id(Convert.ToInt32(summarys[i]), id_pedido);
                            foreach (estilos e in a.lista_estilos){
                                List<ratio_tallas> ratios_a = ds.obtener_lista_ratio_assort_r(e.id_po_summary, e.id_estilo, a.nombre);
                                foreach (ratio_tallas r in ratios_a){
                                    total_piezas_pk += (Convert.ToInt32(cajas[i]) * r.ratio);
                                }
                            }
                            break;
                        case "4":
                            string[] ppks = nums_ppks[i].Split('*');
                            List<ratio_tallas> r4 = ds.obtener_lista_ratio_ppks(Convert.ToInt32(summarys[i]), id_estilo, 4, Convert.ToInt32(ppks[0]), ppks[1]);
                            foreach (ratio_tallas r in r4){
                                total_piezas_pk += (Convert.ToInt32(cajas[i]) * r.ratio);
                            }
                            break;
                    }
                }else{
                    string[] cantidades_talla = cantidades[i].Split('&');
                    for (int j = 1; j < cantidades_talla.Length; j++){ total_piezas_pk += (Convert.ToInt32(cantidades_talla[j])); }
                }
            }

            if ((total_enviado + total_piezas_pk) > (total_pedido + 100)){//ERROR
                return Json("1", JsonRequestBehavior.AllowGet);
            }else{
                List<int> originales=ds.obtener_shipping_ids_packing(packing_id);
                List<int> temporales = new List<int>();
                List<int> nueva = new List<int>();
                for (int j=1; j<shippings.Length; j++) {
                    string[] ship_ids = shippings[j].Split('*');
                    for (int i = 1; i < ship_ids.Length; i++) {
                        if (ship_ids[i] != "0"){ temporales.Add(Convert.ToInt32(ship_ids[i])); }
                    }
                }
                nueva = originales.Except(temporales).ToList();
                foreach (int i in nueva) { ds.eliminar_totales_envios(i); ds.eliminar_estilo_shipping_id(i,ds.buscar_tipo_empaque_shipid(i)); }
                //AHORA A EDITAR -_-
                for (int i = 1; i < summarys.Length; i++) {
                    string[] ship_ids = shippings[i].Split('*');
                    int id_estilo = consultas.obtener_estilo_summary(Convert.ToInt32(summarys[i]));
                    string[] cantidades_talla = cantidades[i].Split('&');
                    //BUSCO TALLAS NUEVAS EN ESTILOS AGREGADOS ANTERIORMENTE
                    //SOLO PARA BP Y EXT/DMG PORQUE AUNQUE AGREGUE TALLAS, NO SE NOTA EN EL ARRAY
                    //EJEM: [65,95,68,45] Y SE LE AGREGA CANTIDAD DE 5TA TALLA
                    //A LA PRÓXIMA  EDICIÓN QUEDARÍA: [65,95,68,45,*NEW*]
                    //EL DE ABAJO ES PARA CUANDO SON 100% NUEVOS O VIEJOS CON LAS MISMAS TALLAS :P
                    int existe = 0, old = 0;
                    for (int j = 1; j < cantidades_talla.Length; j++){
                        existe = 0; old = 0;
                        if (cantidades_talla[j] != "0"){
                            for (int z = 1; z < ship_ids.Length; z++){
                                if (ship_ids[z] != "0") { old++; }
                            }
                            if (old != 0){
                                //AQUI BUSCO QUE TODAS LAS TALLAS EXISTAN
                                for (int z = 1; z < ship_ids.Length; z++){
                                    int talla_temporal = ds.buscar_talla_shipping_id(ship_ids[z]);
                                    if (talla_temporal == Convert.ToInt32(tallas[j])){
                                        existe++;
                                    }
                                }//SI NO EXISTE Y EL ESTILO YA SE GUARDÓ ANTES, SE AGREGA LA NUEVA TALLA :P
                                if (existe == 0 && old != 0){
                                    if (tipos[i] == "EXT" || tipos[i] == "DMG"){
                                        ds.guardar_estilos_paking(cajas[i], labels[i], cantidades_talla[j], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], tallas[j], stores[i], tipos[i], "1", indices[i], "0", "0","0");
                                        shipping_id = ds.obtener_ultimo_shipping_registrado();
                                        ds.agregar_cantidades_enviadas(summarys[i], tallas[j], cantidades_talla[j], shipping_id.ToString(), tipos[i], "0", "0");
                                    }else{
                                        if (empaques[i] == "1"||empaques[i] == "5"){
                                            int piezas = 0;
                                            ds.guardar_estilos_paking("1", labels[i], cantidades_talla[j], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], tallas[j], stores[i], tipos[i], "1", indices[i], "0", "0", nums_ppks[i]);
                                            shipping_id = ds.obtener_ultimo_shipping_registrado();
                                            if (dcs[i] != "0"){
                                                if (empaques[i] == "1") { piezas = ds.buscar_piezas_empaque_bull(Convert.ToInt32(summarys[i]), Convert.ToInt32(tallas[j])); }
                                                if (empaques[i] == "5") { piezas = ds.buscar_piezas_empaque_bulls(Convert.ToInt32(summarys[i]), Convert.ToInt32(tallas[j]), nums_ppks[i]); }
                                                piezas = piezas * Convert.ToInt32(cantidades_talla[j]);
                                                ds.agregar_cantidades_enviadas(summarys[i], tallas[j], piezas.ToString(), shipping_id.ToString(), tipos[i], "0", "0");
                                            }else{
                                                ds.agregar_cantidades_enviadas(summarys[i], tallas[j], cantidades_talla[j], shipping_id.ToString(), tipos[i], "0", "0");
                                            }
                                        }
                                    }
                                    //
                                    //
                                }
                            }
                        }
                    }

                    for (int j = 1; j < ship_ids.Length; j++){
                        if (ship_ids[j] != "0"){//editar lo que no es nuevo duh
                            ds.eliminar_totales_envios(Convert.ToInt32(ship_ids[j]));
                            if (empaques[i] != "1" && empaques[i] != "5" && tipos[i] != "EXT" && tipos[i] != "DMG"){
                                if (empaques[i] == "4"){
                                    string[] ppks = nums_ppks[i].Split('*');
                                    ds.editar_estilos_paking(ship_ids[j], "1", labels[i], cajas[i], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], "0", stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], nums_ppks[i],"0");
                                    List<ratio_tallas> ra = ds.obtener_lista_ratio_ppks(Convert.ToInt32(summarys[i]), id_estilo, 4, Convert.ToInt32(ppks[0]), ppks[1]);
                                    foreach (ratio_tallas r in ra){
                                        ds.agregar_cantidades_enviadas(summarys[i], (r.id_talla).ToString(), ((Convert.ToInt32(cajas[i]) * r.ratio)).ToString(), ship_ids[j], tipos[i], "0", "1");
                                    }
                                }else{
                                    if (empaques[i] == "3"){
                                        ds.editar_estilos_paking(ship_ids[j], "1", labels[i], cajas[i], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], "0", stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], nums_ppks[i], "0");
                                        Assortment a = ds.assortment_id(Convert.ToInt32(summarys[i]), id_pedido);
                                        foreach (estilos e in a.lista_estilos){
                                            List<ratio_tallas> ratios_a = ds.obtener_lista_ratio_assort_r(e.id_po_summary, e.id_estilo, a.nombre);
                                            foreach (ratio_tallas r in ratios_a){
                                                ds.agregar_cantidades_enviadas((e.id_po_summary).ToString(), (r.id_talla).ToString(), ((Convert.ToInt32(cajas[i]) * r.ratio)).ToString(), ship_ids[j], tipos[i], "1", "1");
                                            }
                                        }
                                    }else{//2---PPK
                                        ds.editar_estilos_paking(ship_ids[j], "1", labels[i], cajas[i], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], "0", stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], nums_ppks[i], "0");
                                        List<ratio_tallas> ratios = ds.obtener_lista_ratio(Convert.ToInt32(summarys[i]), id_estilo, 2);
                                        foreach (ratio_tallas r in ratios){
                                            ds.agregar_cantidades_enviadas(summarys[i], (r.id_talla).ToString(), ((Convert.ToInt32(cajas[i]) * r.ratio)).ToString(), ship_ids[j], tipos[i], "0", "1");
                                        }
                                    }
                                }
                            }else{//EMPAQUE =1
                                if (tipos[i] != "EXT" && tipos[i] != "DMG"){                                    
                                    if (dcs[i] == "0"){//SIN DC
                                        for (int k = 1; k < cantidades_talla.Length; k++){
                                            int talla_temporal = ds.buscar_talla_shipping_id(ship_ids[j]);
                                            if (cantidades_talla[k] != "0" && talla_temporal == Convert.ToInt32(tallas[k])){
                                                ds.editar_estilos_paking(ship_ids[j], cajas[i], labels[i], cantidades_talla[k], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], tallas[k], stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], "0", nums_ppks[i]);
                                                ds.agregar_cantidades_enviadas(summarys[i], tallas[k], (Convert.ToInt32(cantidades_talla[k])).ToString(), ship_ids[j], tipos[i], "0", "1");
                                            }
                                            if (cantidades_talla[k] == "0" && talla_temporal == Convert.ToInt32(tallas[k])){
                                                ds.eliminar_estilo_shipping_id(Convert.ToInt32(ship_ids[j]),1);
                                            }
                                        }
                                    }else{//CON DC                                        
                                        for (int k = 1; k < cantidades_talla.Length; k++){
                                            int piezas = ds.buscar_piezas_empaque_bull(Convert.ToInt32(summarys[i]), Convert.ToInt32(tallas[k]));
                                            piezas = (piezas * Convert.ToInt32(cantidades_talla[k]));
                                            int talla_temporal = ds.buscar_talla_shipping_id(ship_ids[j]);
                                            if (cantidades_talla[k] != "0" && talla_temporal == Convert.ToInt32(tallas[k])){
                                                ds.editar_estilos_paking(ship_ids[j], cajas[i], labels[i], cantidades_talla[k], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], tallas[k], stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], "0", nums_ppks[i]);
                                                ds.agregar_cantidades_enviadas(summarys[i], tallas[k], piezas.ToString(), ship_ids[j], tipos[i], "0", "1");
                                            }
                                            if (cantidades_talla[k] == "0" && talla_temporal == Convert.ToInt32(tallas[k])){
                                                ds.eliminar_estilo_shipping_id(Convert.ToInt32(ship_ids[j]),1);
                                            }
                                        }
                                    }
                                }else{                                    
                                    for (int k = 1; k < cantidades_talla.Length; k++){
                                        int talla_temporal = ds.buscar_talla_shipping_id(ship_ids[j]);
                                        if (cantidades_talla[k] != "0" && talla_temporal == Convert.ToInt32(tallas[k])){
                                            ds.editar_estilos_paking(ship_ids[j], cajas[i], labels[i], cantidades_talla[k], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], tallas[k], stores[i], tipos[i], "1", indices[i], sobrantes[i],"0", nums_ppks[i]);
                                            ds.agregar_cantidades_enviadas(summarys[i], tallas[j], (Convert.ToInt32(cantidades_talla[k])).ToString(), ship_ids[j], tipos[i], "0", "1");
                                        }
                                        if (cantidades_talla[k] == "0" && talla_temporal == Convert.ToInt32(tallas[k])){
                                            ds.eliminar_estilo_shipping_id(Convert.ToInt32(ship_ids[j]),1);
                                        }
                                    }
                                }
                            }
                        }else {
                            id_estilo = consultas.obtener_estilo_summary(Convert.ToInt32(summarys[i]));
                            if (tipos[i] != "DMG" && tipos[i] != "EXT"){
                                switch (empaques[i]){
                                    case "1"://TIPO DE EMPAQUE BLPACK
                                        if (empaques[i] == "1" && dcs[i] == "0"){//SIN DC                                           
                                            for (int k = 1; k < cantidades_talla.Length; k++){
                                                if (cantidades_talla[k] != "0"){
                                                    ds.guardar_estilos_paking(cajas[i], labels[i], cantidades_talla[k], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], tallas[k].ToString(), stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], nums_ppks[i],"0");
                                                    shipping_id = ds.obtener_ultimo_shipping_registrado();
                                                    ds.agregar_cantidades_enviadas(summarys[i], tallas[k], (Convert.ToInt32(cantidades_talla[k])).ToString(), shipping_id.ToString(), tipos[i], "0", "1");
                                                }
                                            }
                                        }
                                        if (empaques[i] == "1" && dcs[i] != "0"){//CON DC                                            
                                            for (int k = 1; k < cantidades_talla.Length; k++){
                                                int piezas = ds.buscar_piezas_empaque_bull(Convert.ToInt32(summarys[i]), Convert.ToInt32(tallas[k]));
                                                piezas = (piezas * Convert.ToInt32(cantidades_talla[k]));
                                                if (cantidades_talla[k] != "0"){
                                                    ds.guardar_estilos_paking(cajas[i], labels[i], cantidades_talla[k], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], tallas[k].ToString(), stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], nums_ppks[i],"0");
                                                    shipping_id = ds.obtener_ultimo_shipping_registrado();
                                                    ds.agregar_cantidades_enviadas(summarys[i], tallas[k], piezas.ToString(), shipping_id.ToString(), tipos[i], "0", "1");
                                                }
                                            }
                                        }
                                        break;
                                    case "5"://TIPO DE EMPAQUE BLPACK
                                        if (empaques[i] == "5" && dcs[i] == "0"){//SIN DC                                           
                                            for (int k = 1; k < cantidades_talla.Length; k++)                                            {
                                                if (cantidades_talla[k] != "0"){
                                                    ds.guardar_estilos_paking(cajas[i], labels[i], cantidades_talla[k], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], tallas[k].ToString(), stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], "0", nums_ppks[i]);
                                                    shipping_id = ds.obtener_ultimo_shipping_registrado();
                                                    ds.agregar_cantidades_enviadas(summarys[i], tallas[k], (Convert.ToInt32(cantidades_talla[k])).ToString(), shipping_id.ToString(), tipos[i], "0", "1");
                                                }
                                            }
                                        }
                                        if (empaques[i] == "5" && dcs[i] != "0"){//CON DC
                                            for (int k = 1; k < cantidades_talla.Length; k++){
                                                int piezas = ds.buscar_piezas_empaque_bulls(Convert.ToInt32(summarys[i]), Convert.ToInt32(tallas[k]),nums_ppks[i]);
                                                piezas = (piezas * Convert.ToInt32(cantidades_talla[k]));
                                                if (cantidades_talla[k] != "0"){
                                                    ds.guardar_estilos_paking(cajas[i], labels[i], piezas.ToString(), "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], tallas[k].ToString(), stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], "0", nums_ppks[i]);
                                                    shipping_id = ds.obtener_ultimo_shipping_registrado();
                                                    ds.agregar_cantidades_enviadas(summarys[i], tallas[k], piezas.ToString(), shipping_id.ToString(), tipos[i], "0", "1");
                                                }
                                            }
                                        }
                                        break;
                                    case "2":
                                        ds.guardar_estilos_paking("1", labels[i], cajas[i], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], "0", stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], nums_ppks[i], "0");
                                        shipping_id = ds.obtener_ultimo_shipping_registrado();
                                        List<ratio_tallas> ratios = ds.obtener_lista_ratio(Convert.ToInt32(summarys[i]), id_estilo, 2);
                                        foreach (ratio_tallas r in ratios){
                                            ds.agregar_cantidades_enviadas(summarys[i], (r.id_talla).ToString(), ((Convert.ToInt32(cajas[i]) * r.ratio)).ToString(), shipping_id.ToString(), tipos[i], "0", "1");
                                        }
                                        break;
                                    case "3":
                                        ds.guardar_estilos_paking("1", labels[i], cajas[i], "0", packing.ToString(), id_pedido.ToString(), "0", number_po.ToString(), dcs[i], summarys[i], "0", stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], nums_ppks[i], "0");
                                        shipping_id = ds.obtener_ultimo_shipping_registrado();
                                        Assortment a = ds.assortment_id(Convert.ToInt32(summarys[i]), id_pedido);
                                        foreach (estilos e in a.lista_estilos){
                                            List<ratio_tallas> ratios_a = ds.obtener_lista_ratio_assort_r(e.id_po_summary, e.id_estilo, a.nombre);
                                            foreach (ratio_tallas r in ratios_a){
                                                ds.agregar_cantidades_enviadas((e.id_po_summary).ToString(), (r.id_talla).ToString(), ((Convert.ToInt32(cajas[i]) * r.ratio)).ToString(), shipping_id.ToString(), tipos[i], "1", "1");
                                            }
                                        }
                                        break;
                                    case "4":
                                        string[] ppks = nums_ppks[i].Split('*');
                                        ds.guardar_estilos_paking("1", labels[i], cajas[i], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], "0", stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], ppks[0], ppks[1]);
                                        shipping_id = ds.obtener_ultimo_shipping_registrado();
                                        List<ratio_tallas> ra = ds.obtener_lista_ratio_ppks(Convert.ToInt32(summarys[i]), id_estilo, 4, Convert.ToInt32(ppks[0]), ppks[1]);
                                        foreach (ratio_tallas r in ra){
                                            ds.agregar_cantidades_enviadas(summarys[i], (r.id_talla).ToString(), ((Convert.ToInt32(cajas[i]) * r.ratio)).ToString(), shipping_id.ToString(), tipos[i], "0", "1");
                                        }
                                        break;
                                }
                            }else{                                
                                for (int k = 1; k < cantidades_talla.Length; k++){
                                    if (cantidades_talla[k] != "0"){
                                        ds.guardar_estilos_paking(cajas[i], labels[i], cantidades_talla[k], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), number_po.ToString(), dcs[i], summarys[i], tallas[k].ToString(), stores[i], tipos[i], empaques[i], indices[i], sobrantes[i], "0", nums_ppks[i]);
                                        shipping_id = ds.obtener_ultimo_shipping_registrado();
                                        ds.agregar_cantidades_enviadas(summarys[i], tallas[j], cantidades_talla[k], shipping_id.ToString(), tipos[i], "0", "1");
                                    }
                                }
                            }
                        }
                    }
                }
                ds.editar_nombre_packing_list(packing_id);
                return Json("0", JsonRequestBehavior.AllowGet);
            }//ELSE    
        }
        public JsonResult guardar_edicion_pk(string packing,string po, string address, string driver, string container, string seal, string replacement, string manager, string tipo, string labels, string type_labels, string num_envio,string sin_partes){
            int usuario = Convert.ToInt32(Session["id_usuario"]);            
            ds.actualizar_datos_pk(packing, seal, replacement, driver, container,address,num_envio);
            if (labels != "N/A"){
                ds.eliminar_labels(Convert.ToInt32(packing));
                string[] label = labels.Split('*');
                for (int i = 1; i < label.Length; i++){
                    ds.guardar_pk_labels(label[i], Convert.ToInt32(packing), type_labels);
                }
            }

            int tipo_packing = ds.obtener_tipo_packing_packing(Convert.ToInt32(packing));
            if (tipo_packing == 1 || tipo_packing == 2 || tipo_packing == 7 || tipo_packing == 9){
                if (sin_partes == "1"){
                    ds.actualizar_packing_parte(Convert.ToInt32(packing), "0");
                }else{
                    string parte_vieja = ds.buscar_consecutivo(Convert.ToInt32(packing)); //OBTENER PARTE DE ESTE PACKING
                    if (parte_vieja == "0"){
                        int total_enviado = ds.obtener_total_enviado_pedido(Convert.ToInt32(po)), total_pedido = ds.obtener_total_pedido(Convert.ToInt32(po));
                        int total_anterior = ds.obtener_total_enviado_pedido_exclusivo(Convert.ToInt32(po), Convert.ToInt32(packing));
                        int total_pk = ds.obtener_total_enviado_pk(Convert.ToInt32(packing));
                        if (total_anterior != 0){
                            if (total_enviado >= total_pedido){
                                ds.actualizar_packing_parte(Convert.ToInt32(packing), (ds.buscar_parte_packing_anteriores(Convert.ToInt32(po), Convert.ToInt32(packing))).ToString() + ",f");
                            }
                            else { ds.actualizar_packing_parte(Convert.ToInt32(packing), ds.buscar_parte_packing_anteriores(Convert.ToInt32(po), Convert.ToInt32(packing))); }
                        }else{
                            if (total_pk >= total_pedido) { ds.actualizar_packing_parte(Convert.ToInt32(packing), "u"); }
                            else { ds.actualizar_packing_parte(Convert.ToInt32(packing), "1"); }
                        }
                    }
                }
            }
            
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_edicion_pk_directo_fantasy(string customer,string packing,string id_pedido, string po, string address, string driver, string container, string seal, string replacement,string labels, string manager, string tipo, string type_labels, string num_envio)
        {
            int usuario = Convert.ToInt32(Session["id_usuario"]);
            ds.editar_pedido_fantasy(id_pedido,po,customer);

            ds.actualizar_datos_pk_df(customer,packing, seal, replacement, driver, container, address, num_envio);
            if (labels != "N/A"){
                ds.eliminar_labels(Convert.ToInt32(packing));
                string[] label = labels.Split('*');
                for (int i = 1; i < label.Length; i++){
                    ds.guardar_pk_labels(label[i], Convert.ToInt32(packing), type_labels);
                }
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_edicion_estilos_directo_fantasy(string idpedido,string packing,string estilo, string po_sum, string index, string cantidad)
        {
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string[] estilos = estilo.Split('*'), posums = po_sum.Split('*'), indices = index.Split('*'), cantidades = cantidad.Split('*');
            string[] tallas = { "", "SM", "MD", "LG", "XL", "2XL" };
            int id_pedido = Convert.ToInt32(idpedido), shipping_id = 0;
            int pk = Convert.ToInt32(packing);
            ds.eliminar_packing_edicion_directo_fantasy(pk);

            for (int i = 1; i < estilos.Length; i++){
                string[] totales = (cantidades[i]).Split('&');
                for (int jj = 1; jj < totales.Length; jj++){
                    if (totales[jj] != "0")
                    {
                        int id_talla = consultas.buscar_talla(tallas[jj]);
                        ds.guardar_estilos_paking("1", "NONE", totales[jj], "0", pk.ToString(), id_pedido.ToString(), estilos[i], posums[i], "0", "0", id_talla.ToString(), "N/A", "NONE", "1", indices[i], "0", "0","0");
                        shipping_id = ds.obtener_ultimo_shipping_registrado();
                        ds.agregar_cantidades_enviadas(estilos[i], id_talla.ToString(), totales[jj], shipping_id.ToString(), "NONE", "0", "8");
                    }
                }
            }
            ds.editar_nombre_packing_list(pk);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_edicion_estilos_semanal_fantasy(string idpedido, string packing, string estilo, string po_sum, string index, string cantidad,string pallet,string dc){        
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string[] estilos = estilo.Split('*'), posums = po_sum.Split('*'), indices = index.Split('*'), cantidades = cantidad.Split('*'),pallets=pallet.Split('*'),dcs=dc.Split('*');
            string[] tallas = { "", "SM", "MD", "LG", "XL", "2XL" };
            int id_pedido = Convert.ToInt32(idpedido), shipping_id = 0;
            int pk = Convert.ToInt32(packing);
            //ds.eliminar_packing_edicion_directo_fantasy(pk);
            ds.eliminar_estilos_packing_list(pk);
            for (int i = 1; i < estilos.Length; i++){
                string[] totales = (cantidades[i]).Split('&');
                for (int jj = 1; jj < totales.Length; jj++){
                    if (totales[jj] != "0"){
                        int id_talla = consultas.buscar_talla(tallas[jj]);
                        ds.guardar_estilos_paking_tarima("1", "NONE", totales[jj], pallets[i], pk.ToString(), id_pedido.ToString(), estilos[i], posums[i], dcs[i], "0", id_talla.ToString(), "N/A", "NONE", "1", indices[i], "0", "0");
                        shipping_id = ds.obtener_ultimo_shipping_registrado();
                        ds.agregar_cantidades_enviadas(estilos[i], id_talla.ToString(), totales[jj], shipping_id.ToString(), "NONE", "0", "1");
                    }
                }
            }
            ds.editar_nombre_packing_list(pk);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult guardar_edicion_pk_estilos_ht(string pedido, string packing, string size, string summary, string ponumero, string talla, string ppk, string carton, string store, string tipo, string label, string dc, string empaque, string indice,string shipping)
        {
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string[] summarys = summary.Split('*'), ponumeros = ponumero.Split('*'), tallas = talla.Split('*'), ppks = ppk.Split('*'), cartones = carton.Split('*');
            string[] stores = store.Split('*'), tipos = tipo.Split('*'), labels = label.Split('*'), dcs = dc.Split('*'), empaques = empaque.Split('*'), indices = indice.Split('*');
            string[] sizes = size.Split('*'), shippings = shipping.Split('&'); 
            int id_packing = Convert.ToInt32(packing);
            int id_pedido = Convert.ToInt32(pedido);
            int total_enviado = ds.obtener_total_enviado_pedido_exclusivo(id_pedido,id_packing), total_pedido = ds.obtener_total_pedido(id_pedido);
            string number_po = ds.obtener_number_po_pedido(id_pedido);
            int shipping_id = 0, total_piezas_pk = 0;
            for (int i = 1; i < summarys.Length; i++){
                if (tipos[i] == "EXT" || tipos[i] == "DMG"){
                    string[] cantidades = tallas[i].Split('&');
                    for (int j = 1; j < cantidades.Length; j++){
                        total_piezas_pk += Convert.ToInt32(cantidades[j]);
                    }
                }else{
                    switch (empaques[i]){
                        case "1":
                            string[] cantidades = tallas[i].Split('&');
                            for (int j = 1; j < cantidades.Length; j++){
                                total_piezas_pk += Convert.ToInt32(cantidades[j]);
                            }
                            break;
                        case "2":
                            List<ratio_tallas> ratios = ds.obtener_lista_ratio_hottopic(Convert.ToInt32(summarys[i]), ponumeros[i], empaques[i]);
                            foreach (ratio_tallas r in ratios){
                                total_piezas_pk += (Convert.ToInt32(cartones[i]) * Convert.ToInt32(ppks[i]) * r.ratio);
                            }
                            break;
                    }
                }
            }
            if (((total_enviado + total_piezas_pk) > (total_pedido + 100)) && total_piezas_pk != 0){//ERROR
                return Json("1", JsonRequestBehavior.AllowGet);
            }else{
                //ds.eliminar_estilos_packing_list(id_packing);
                List<int> originales = ds.obtener_shipping_ids_packing(id_packing);
                List<int> temporales = new List<int>();
                List<int> nueva = new List<int>();
                for (int j = 1; j < shippings.Length; j++){
                    string[] ship_ids = shippings[j].Split('*');
                    for (int i = 1; i < ship_ids.Length; i++){
                        if (ship_ids[i] != "0") { temporales.Add(Convert.ToInt32(ship_ids[i])); }
                    }
                }
                nueva = originales.Except(temporales).ToList();
                foreach (int i in nueva) { ds.eliminar_totales_envios(i); ds.eliminar_estilo_shipping_id(i,7); }
                //AHORA A EDITAR -_-

                for (int i = 1; i < summarys.Length; i++) {
                    string[] ships = shippings[i].Split('*');
                    int id_estilo = consultas.obtener_estilo_summary(Convert.ToInt32(summarys[i]));
                    string[] cantidades = tallas[i].Split('&');

                    int existe=0,old=0;
                    for (int j = 1; j < cantidades.Length; j++){
                        existe = 0; old = 0;
                        if (cantidades[j] != "0"){
                            for (int z = 1; z < ships.Length; z++) {
                                if (ships[z] != "0") { old++; }
                            }
                            if (old != 0){
                                for (int z = 1; z < ships.Length; z++){
                                    int talla_temporal = ds.buscar_talla_shipping_id(ships[z]);
                                    if (talla_temporal == Convert.ToInt32(sizes[j])){
                                        existe++;
                                    }
                                }
                                if (existe == 0 && old != 0){
                                    if (tipos[i] == "EXT" || tipos[i] == "DMG"){
                                        ds.guardar_estilos_paking(cartones[i], labels[i], cantidades[j], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), ponumeros[i], dcs[i], summarys[i], sizes[j], stores[i], tipos[i], "1", indices[i], "0", "0", "0");
                                    }else{
                                        if (empaques[i] == "1"){
                                            ds.guardar_estilos_paking("1", labels[i], cantidades[j], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), ponumeros[i], dcs[i], summarys[i], sizes[j], stores[i], tipos[i], "1", indices[i], "0", "0", "0");
                                        }
                                    }
                                    shipping_id = ds.obtener_ultimo_shipping_registrado();
                                    ds.agregar_cantidades_enviadas(summarys[i], sizes[j], cantidades[j], shipping_id.ToString(), tipos[i], "0", "7");
                                }
                            }
                        }
                    }                  
                   
                    for (int z = 1; z < ships.Length; z++) {
                        if (ships[z]!="0") {
                            ds.eliminar_totales_envios(Convert.ToInt32(ships[z]));
                            if (tipos[i] == "EXT" || tipos[i] == "DMG"){
                                for (int j = 1; j < cantidades.Length; j++){
                                    int talla_temporal = ds.buscar_talla_shipping_id(ships[z]);
                                    if (cantidades[j] != "0" && talla_temporal == Convert.ToInt32(sizes[j])) {
                                        ds.editar_estilos_paking(ships[z], cartones[i], labels[i], cantidades[j], "0", packing.ToString(), id_pedido.ToString(), "0", ponumeros[i], dcs[i], summarys[i], sizes[j], stores[i], tipos[i], "1", indices[i],"0","0","0");
                                        ds.agregar_cantidades_enviadas(summarys[i], sizes[j], cantidades[j], ships[z], tipos[i], "0", "7");
                                    }
                                    if (cantidades[j] == "0" && talla_temporal == Convert.ToInt32(sizes[j])){
                                        ds.eliminar_estilo_shipping_id(Convert.ToInt32(ships[z]),7);
                                    }
                                }
                            }else{
                                switch (empaques[i]){
                                    case "1":
                                        int talla_temporal = ds.buscar_talla_shipping_id(ships[z]);
                                        for (int j = 1; j < cantidades.Length; j++){                                            
                                            if (cantidades[j] != "0" && talla_temporal == Convert.ToInt32(sizes[j])){
                                                ds.editar_estilos_paking(ships[z], "1", labels[i], cantidades[j], "0", packing.ToString(), id_pedido.ToString(), "0", ponumeros[i], dcs[i], summarys[i], sizes[j], stores[i], tipos[i], empaques[i], indices[i], "0", "0", "0");
                                                ds.agregar_cantidades_enviadas(summarys[i], sizes[j], cantidades[j], ships[z], tipos[i], "0", "7");
                                            }
                                            if (cantidades[j] == "0" && talla_temporal == Convert.ToInt32(sizes[j])){
                                                ds.eliminar_estilo_shipping_id(Convert.ToInt32(ships[z]),7);
                                            }                                            
                                        }                                        
                                        break;
                                    case "2":
                                        if (ppks[i] != "" && cartones[i] != "" && ppks[i] != "0" && cartones[i] != "0"){
                                            ds.editar_estilos_paking(ships[z],ppks[i], labels[i], cartones[i], "0", packing.ToString(), id_pedido.ToString(), "0", ponumeros[i], dcs[i], summarys[i], "0", stores[i], tipos[i], empaques[i], indices[i], "0", "0", "0");
                                            List<ratio_tallas> ratios = ds.obtener_lista_ratio_hottopic(Convert.ToInt32(summarys[i]), ponumeros[i], empaques[i]);
                                            foreach (ratio_tallas r in ratios){
                                                if (((Convert.ToInt32(cartones[i]) * Convert.ToInt32(ppks[i]) * r.ratio)) != 0){
                                                    ds.agregar_cantidades_enviadas(summarys[i], (r.id_talla).ToString(), ((Convert.ToInt32(cartones[i]) * Convert.ToInt32(ppks[i]) * r.ratio)).ToString(), ships[z], tipos[i], "0", "7");
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                        }else {                            
                            if (tipos[i] == "EXT" || tipos[i] == "DMG") {
                                for (int j = 1; j < cantidades.Length; j++) {
                                    if (cantidades[j] != "0") {
                                        ds.guardar_estilos_paking(cartones[i], labels[i], cantidades[j], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), ponumeros[i], dcs[i], summarys[i], sizes[j], stores[i], tipos[i], "1", indices[i], "0", "0", "0");
                                        shipping_id = ds.obtener_ultimo_shipping_registrado();
                                        ds.agregar_cantidades_enviadas(summarys[i], sizes[j], cantidades[j], shipping_id.ToString(), tipos[i], "0", "7");
                                    }
                                }
                            } else {
                                switch (empaques[i]) {
                                    case "1":
                                        for (int j = 1; j < cantidades.Length; j++) {
                                            if (cantidades[j] != "0") {
                                                ds.guardar_estilos_paking("1", labels[i], cantidades[j], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), ponumeros[i], dcs[i], summarys[i], sizes[j], stores[i], tipos[i], empaques[i], indices[i], "0", "0","0");
                                                shipping_id = ds.obtener_ultimo_shipping_registrado();
                                                ds.agregar_cantidades_enviadas(summarys[i], sizes[j], cantidades[j], shipping_id.ToString(), tipos[i], "0", "7");
                                            }
                                        }
                                        break;
                                    case "2":
                                        if (ppks[i] != "" && cartones[i] != "" && ppks[i] != "0" && cartones[i] != "0") {
                                            ds.guardar_estilos_paking(ppks[i], labels[i], cartones[i], "0", packing.ToString(), id_pedido.ToString(), id_estilo.ToString(), ponumeros[i], dcs[i], summarys[i], "0", stores[i], tipos[i], empaques[i], indices[i], "0", "0", "0");
                                            shipping_id = ds.obtener_ultimo_shipping_registrado();
                                            List<ratio_tallas> ratios = ds.obtener_lista_ratio_hottopic(Convert.ToInt32(summarys[i]), ponumeros[i], empaques[i]);
                                            foreach (ratio_tallas r in ratios) {
                                                if (((Convert.ToInt32(cartones[i]) * Convert.ToInt32(ppks[i]) * r.ratio)) != 0) {
                                                    ds.agregar_cantidades_enviadas(summarys[i], (r.id_talla).ToString(), ((Convert.ToInt32(cartones[i]) * Convert.ToInt32(ppks[i]) * r.ratio)).ToString(), shipping_id.ToString(), tipos[i], "0", "0");
                                                }
                                            }
                                        }
                                        break;
                                    
                                }
                            }
                        }
                    }
                }

                ds.revisar_totales_estilo(id_pedido);
                verificar_estado_pedido(id_pedido);
                ds.editar_nombre_packing_list(id_packing);
                return Json("0", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult guardar_edicion_fantasy_extras(string packing, string summary, string caja, string cantidad, string tipo,string id)
        {
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string[] summarys = summary.Split('*'),  cajas = caja.Split('*'), tipos = tipo.Split('*');
            string[] cantidades = cantidad.Split('*'),ids=id.Split('*');
            string talla = "";
            int id_packing = Convert.ToInt32(packing), extra_id;
            int exceso = 0, total_this_envio;
            for (int i = 1; i < summarys.Length; i++){
                int total_enviado = ds.obtener_total_enviado_extras_pedido_summary_exclusivo(Convert.ToInt32(summarys[i]),id_packing), total_pedido = ds.obtener_total_extras_pedido_summary(Convert.ToInt32(summarys[i]));
                string[] quantities = cantidades[i].Split('&');
                total_this_envio = 0;
                for (int j = 1; j < 6; j++){
                    if (quantities[j] != "0") { total_this_envio += Convert.ToInt32(quantities[j]); }
                }
                if ((total_enviado + total_this_envio) > (total_pedido + 100)) { exceso++; }
            }
            if (exceso != 0){                
                return Json("1", JsonRequestBehavior.AllowGet);
            }else{
                //ds.eliminar_packing_edicion_extras_fantasy(id_packing);
                List<int> originales = ds.obtener_ids_packing_extras_fantasy(id_packing);
                List<int> temporales = new List<int>();
                List<int> nueva = new List<int>();
                for (int j = 1; j < summarys.Length; j++){
                    if (ids[j] != "0") { temporales.Add(Convert.ToInt32(ids[j])); }
                }
                nueva = originales.Except(temporales).ToList();
                foreach (int i in nueva){
                    ds.eliminar_totales_extra_packing_fantasy(i);
                    ds.eliminar_extras_packing_fantasy(i);
                }

                for (int i = 1; i < summarys.Length; i++){
                    ds.eliminar_totales_extra_packing_fantasy(Convert.ToInt32(ids[i]));
                    string[] quantities = cantidades[i].Split('&');
                    if (ids[i] == "0"){
                        ds.guardar_estilo_fantasy_extras(id_packing, summarys[i], quantities[1], quantities[2], quantities[3], quantities[4], quantities[5], cajas[i], tipos[i]);
                        extra_id = ds.obtener_ultimo_extra_fantasy_registrado();                       
                    }else {
                        ds.editar_estilo_fantasy_extras(ids[i], summarys[i], quantities[1], quantities[2], quantities[3], quantities[4], quantities[5], cajas[i], tipos[i]);
                        extra_id = Convert.ToInt32(ids[i]);
                    }
                    int pedido = consultas.obtener_id_pedido_summary(Convert.ToInt32(summarys[i]));
                    ds.revisar_totales_estilo(pedido);
                    verificar_estado_pedido(pedido);
                    for (int j = 1; j < 6; j++){
                        if (quantities[j] != "0"){
                            switch (j){
                                case 1: talla = "SM"; break;
                                case 2: talla = "MD"; break;
                                case 3: talla = "LG"; break;
                                case 4: talla = "XL"; break;
                                case 5: talla = "2XL"; break;
                            }
                            ds.agregar_cantidades_enviadas(summarys[i], (consultas.buscar_talla(talla)).ToString(), quantities[j], extra_id.ToString(), "EXT", "0", "6");
                        }
                    }
                }
                ds.editar_nombre_packing_list(id_packing);
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult guardar_edicion_estilos_samples(string packing, string cliente, string summary, string caja, string attnto, string talla, string cantidad, string indice, string inicial, string tipo_sample, string id_new, string po_new, string estilo_new, string color_new, string descripcion_new, string origen_new, string porcentaje_new, string genero_new, string cabecera, string total_col_extras)
        {
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string[] summarys = summary.Split('*'), cajas = caja.Split('*'), attntos = attnto.Split('*'), iniciales = inicial.Split('*');
            string[] tallas = talla.Split('*'), cantidades = cantidad.Split('*'), indices = indice.Split('*'), tipos_samples = tipo_sample.Split('*');
            int id_packing = Convert.ToInt32(packing), examples_id, total_this_envio, exceso = 0, columnas_extra = Convert.ToInt32(total_col_extras);
            string[] ids_new = id_new.Split('*'), pos_new = po_new.Split('*'), estilos_new = estilo_new.Split('*'), colores_new = color_new.Split('*'), descripciones_new = descripcion_new.Split('*'), origenes_new = origen_new.Split('*');
            string[] porcentajes_new = porcentaje_new.Split('*'), generos_new = genero_new.Split('*'), cabeceras = cabecera.Split('*');
            /*for (int i = 1; i < summarys.Length; i++){
                int total_enviado = ds.obtener_total_enviado_samples_pedido_summary_exclusivo(Convert.ToInt32(summarys[i]),id_packing), total_pedido = ds.obtener_total_samples_pedido_summary(Convert.ToInt32(summarys[i]));
                string[] quantities = cantidades[i].Split('&');
                total_this_envio = 0;
                for (int j = 1; j < 6; j++){
                    if (quantities[j] != "0") { total_this_envio += Convert.ToInt32(quantities[j]); }
                }
                if ((total_enviado + total_this_envio) > (total_pedido + 100)) { exceso++; }
            }*/
            /*if (exceso != 0){
                return Json("1", JsonRequestBehavior.AllowGet);
            }else{9*/
            ds.eliminar_packing_edicion_samples(id_packing);
            for (int i = 1; i < summarys.Length; i++)
            {
                string[] quantities = cantidades[i].Split('&');
                string[] sizes = (tallas[i]).Split('&');
                int contador = 0;
                if (tipos_samples[i] == "1"){
                    for (int c = 1; c < ids_new.Length; c++){
                        if (ids_new[c] == summarys[i] && contador == 0){
                            ds.guardar_nuevo_estilo_ejemplo(colores_new[c], porcentajes_new[c], generos_new[c], pos_new[c], estilos_new[c], descripciones_new[c], origenes_new[c]);
                            summarys[i] = Convert.ToString(ds.obtener_ultimo_nuevo_ejemplo_registrado());
                            contador++;
                        }
                    }
                }
                ds.guardar_estilo_ejemplo(id_packing, summarys[i], quantities[1], quantities[2], quantities[3], quantities[4], quantities[5], quantities[6], quantities[7], cajas[i], attntos[i], cliente, indices[i], iniciales[i], tipos_samples[i]);
                examples_id = ds.obtener_ultimo_examples_registrado();
                for (int j = 1; j < sizes.Length; j++){
                    if (quantities[j] != "0"){
                        ds.agregar_cantidades_enviadas(summarys[i], (consultas.buscar_talla(sizes[j])).ToString(), quantities[j], examples_id.ToString(), "SAM", "0", "3");
                    }
                }
                int contador_extras = 1;
                if (quantities.Length > 8){
                    for (int j = 8; j < (8 + columnas_extra); j++){
                        if (quantities[j] != "0"){
                            ds.agregar_cantidades_extras(examples_id, contador_extras, Convert.ToInt32(quantities[j]), (consultas.buscar_talla(sizes[j])));
                        }
                        contador_extras++;
                    }
                }
                if (cabeceras[i] == "1"){
                    ds.guardar_cabeceras_ejemplos(id_packing, tallas[i], indices[i]);
                }
            }
            string[] po_summarys = summarys.Distinct().ToArray();
            for (int i = 1; i < po_summarys.Length; i++){
                int pedido = consultas.obtener_id_pedido_summary(Convert.ToInt32(po_summarys[i]));
                ds.revisar_totales_estilo(pedido);
                verificar_estado_pedido(pedido);
            }
            ds.editar_nombre_packing_list(id_packing);
            return Json("0", JsonRequestBehavior.AllowGet);
            //}
        }
        public JsonResult guardar_edicion_items_retorno(string packing,string inventario, string cantidad, string categoria, string talla, string item, string caja,string qty,string indice,string inventario_registro,string packing_return){
            DatosTrim dtrim = new DatosTrim();
            string[] inventarios = inventario.Split('*'), cantidades = cantidad.Split('*'), categorias = categoria.Split('*'), tallas = talla.Split('*'), items = item.Split('*'), cajas = caja.Split('*');
            string[] quantities = qty.Split('*'), indices = indice.Split('*'),inventarios_old=inventario_registro.Split('&'),packings_old=packing_return.Split('&');
            int pedido = ds.obtener_pedido_packing(Convert.ToInt32(packing));
            int id_packing = Convert.ToInt32(packing), summary;
            //ELIMINAR RETURNS QUE YA NO ESTAN EN LA LISTA NUEVA
            List<int> originales = ds.obtener_shipping_ids_packing_return(id_packing);
            List<int> temporales = new List<int>();
            List<int> nueva = new List<int>();
            for (int j = 1; j < packings_old.Length; j++){
                string[] returns_ids = packings_old[j].Split('*');
                for (int i = 1; i < returns_ids.Length; i++){
                    if (returns_ids[i] != "0") { temporales.Add(Convert.ToInt32(returns_ids[i])); }
                }
            }
            nueva = originales.Except(temporales).ToList();
            foreach (int i in nueva) {
                ds.actualizar_inventario_retorno_id(i);
                ds.eliminar_returns_packing(i);
            }
            //ELIMINAR RETURNS QUE YA NO ESTAN EN LA LISTA NUEVA
            //AGREGAR TALLAS DE ELEMENTOS EXISTENTES
            int existe = 0, old = 0;
            for (int i = 1; i < categorias.Length; i++) { //POR CADA INDICE-LINEA-ITEM
                summary = ds.buscar_summary_inventario(Convert.ToInt32(inventarios[i]));
                if (categorias[i] == "1") {
                    string[] returns_ids = packings_old[i].Split('*');
                    string[] cantidades_talla = cantidades[i].Split('&');
                    existe = 0; old = 0;
                    for (int j = 1; j < cantidades_talla.Length; j++) {
                        if (cantidades_talla[j] != "0") {
                            for (int z = 1; z < returns_ids.Length; z++){
                                if (returns_ids[z] != "0") { old++; }
                            }//VERIFICAR QUE NO SEA UN REGISTRO "NUEVO"
                            if (old != 0) {
                                for (int z = 1; z < returns_ids.Length; z++){
                                    int talla_temporal = ds.buscar_talla_shipping_id_returns(returns_ids[z]);
                                    if (talla_temporal == Convert.ToInt32(tallas[j])){
                                        existe++;
                                    }
                                }
                                if (existe == 0 && old != 0) {
                                    int inv = ds.buscar_inventario_retornos(pedido.ToString(), tallas[j], items[i]);
                                    ds.agregar_retorno_envio(inv.ToString(), cantidades_talla[j], categorias[i], tallas[j], items[i], summary, id_packing, cajas[i], indices[i]);
                                    dtrim.actualizar_inventario(inv, cantidades_talla[j]);
                                }//IF
                            }//IF OLD
                        }//IF CANTIDAD_TALLA==0
                    }//FOR
                }//CATEGORIA BLANKS
            }//FOR
            //AGREGAR TALLAS DE ELEMENTOS EXISTENTES
            //EDICION DE ITEMS Y NUEVOS ITEMS
            for (int i = 1; i < categorias.Length; i++){ //POR CADA INDICE-LINEA-ITEM
                summary = ds.buscar_summary_inventario(Convert.ToInt32(inventarios[i]));
                string[] cantidades_talla = cantidades[i].Split('&');
                string[] returns_ids = packings_old[i].Split('*');
                string[] invs_ids = inventarios_old[i].Split('*');
                if (categorias[i] == "1"){                                
                    for (int z = 1; z < returns_ids.Length; z++){
                        if (returns_ids[z] != "0"){//edicion
                            int talla_temporal = ds.buscar_talla_shipping_id_returns(returns_ids[z]);
                            for (int j = 1; j < cantidades_talla.Length; j++){
                                if (talla_temporal == Convert.ToInt32(tallas[j]) && cantidades_talla[j] != "0"){
                                    ds.actualizar_inventario_retorno_id(Convert.ToInt32(returns_ids[z]));
                                    ds.editar_retorno_envio(returns_ids[z], cantidades_talla[j], cajas[i]);
                                    dtrim.actualizar_inventario(Convert.ToInt32(invs_ids[z]), cantidades_talla[j]);
                                }
                                if (cantidades_talla[j] == "0" && talla_temporal == Convert.ToInt32(tallas[j])){
                                    ds.actualizar_inventario_retorno_id(Convert.ToInt32(returns_ids[z]));
                                    ds.eliminar_returns_packing(Convert.ToInt32(returns_ids[z]));
                                }
                            }
                        }else {//SON NUEVOS
                            for (int j = 1; j < cantidades_talla.Length; j++){
                                if (cantidades_talla[j] != "0"){
                                    int inv = ds.buscar_inventario_retornos(pedido.ToString(), tallas[j], items[i]);
                                    ds.agregar_retorno_envio(inv.ToString(), cantidades_talla[j], categorias[i], tallas[j], items[i], summary, id_packing, cajas[i], indices[i]);
                                    dtrim.actualizar_inventario(inv, cantidades_talla[j]);
                                }
                            }
                        }                        
                    }                    
                }else{
                    for (int z = 1; z < returns_ids.Length; z++) {
                        if (returns_ids[z] != "0"){
                            ds.actualizar_inventario_retorno_id(Convert.ToInt32(returns_ids[z]));
                            ds.editar_retorno_envio(returns_ids[z], quantities[i], cajas[i]);
                            dtrim.actualizar_inventario(Convert.ToInt32(invs_ids[z]), quantities[i]);
                        }else {                           
                            ds.agregar_retorno_envio(inventarios[i], quantities[i], categorias[i], "0", items[i], summary, id_packing, cajas[i], indices[i]);
                            dtrim.actualizar_inventario(Convert.ToInt32(inventarios[i]), quantities[i]);
                            //dtrim.revisar_estados_cantidades_trim(Convert.ToInt32(inventarios[i]), Convert.ToInt32(quantities[i]));
                        }
                    }
                }
            }
            //EDICION DE ITEMS Y NUEVOS ITEMS
            ds.editar_nombre_packing_list(id_packing);
            return Json("", JsonRequestBehavior.AllowGet);
        }


        /***************************E*X*T*R*A*S**Y**D*A*Ñ*A*D*A*S***********************************************************************/

        public JsonResult buscar_estilos_pk_otros(string id){
            Session["pedido"] = ds.buscar_pedido_pk(id);
            Session["pk"] = id;
            List<estilo_shipping> e = ds.lista_estilos_extras(Convert.ToString(Session["pedido"]),"0");
            var result = Json(new { po_number = ds.buscar_po_number_pk(id),
                estilos = e,
                tallas = ds.obtener_lista_tallas_pedido(e),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscar_estilos_pk_extras(string id){           
            List<estilo_shipping> e = ds.lista_estilos_extras(Convert.ToString(Session["pedido"]), id);
            var result = Json(new {           
                estilos_busqueda=e,
                tallas = ds.obtener_lista_tallas_pedido(e),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }        
        public ActionResult customer_shipping() {
            return View();
        }
        public ActionResult departamentos_shipping() {
            return View();
        }
        public JsonResult buscar_informacion_shipping(string estilo){
            List<Talla> lista_tallas = di.obtener_lista_tallas_summary(Convert.ToInt32(estilo));
            List<Pk> primera_calidad = ds.obtener_lista_shipping_summary(Convert.ToInt32(estilo), "NONE", lista_tallas);
            primera_calidad.AddRange(ds.obtener_lista_shipping_summary_cantidades_extra(Convert.ToInt32(estilo), "NONE", lista_tallas));
            List<Pk> extras = ds.obtener_lista_shipping_summary(Convert.ToInt32(estilo), "EXT", lista_tallas);
            extras.AddRange(ds.obtener_lista_shipping_summary_cantidades_extra(Convert.ToInt32(estilo), "EXT", lista_tallas));
            List<Pk> samples = ds.obtener_lista_shipping_summary_samples(Convert.ToInt32(estilo), "SAM", lista_tallas);
            var result = Json(new{
                tallas = di.obtener_lista_tallas_summary(Convert.ToInt32(estilo)),
                tallas_primera_calidad= primera_calidad,
                tallas_extras= extras,
                tallas_samples= samples,
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_inventario_orden(string pedido){
            List<estilo_shipping> e = ds.lista_estilos(pedido);
            var result = Json(new{
                inventario = ds.obtener_inventario_orden(Convert.ToInt32(pedido)),
                tallas = ds.obtener_lista_tallas_pedido(e),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_lista_packings(){
            return Json(Json(new{
                packings = ds.obtener_lista_packings()
            }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult guardar_id_reporte_mde(string packing){
            Session["packings_reporte"] = packing;
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public void generar_reporte_mde(){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string[] packings = (Convert.ToString(Session["packings_reporte"])).Split('*');
            List<Pk> lista = new List<Pk>();
            int total_packings = 0;
            string contenedor = "";
            for (int i = 1; i < packings.Length; i++){
                Pk packing = ds.obtener_packing_list_individual(Convert.ToInt32(packings[i]));
                if (packing.id_tipo != 8){
                    if (packing.id_tipo != 3){
                        if (packing.lista_tarimas.Count != 0){
                            lista.Add(packing);
                            total_packings++;
                        }
                    }else{
                        List<int> index_samplesi = new List<int>();
                        int total_temporal_samples = 0;
                        foreach (Sample s in packing.lista_samples) { index_samplesi.Add(s.id_tarima); }
                        index_samplesi = index_samplesi.Distinct().ToList();
                        foreach (int s in index_samplesi) { total_temporal_samples++; }
                        if (total_temporal_samples != 0){
                            lista.Add(packing);
                            total_packings++;
                        }
                    }
                }
            }
            foreach (Pk ppp in lista) { contenedor = ppp.contenedor.eco + "/" + ppp.contenedor.plates; }
            using (XLWorkbook libro_trabajo = new XLWorkbook()){
                var ws = libro_trabajo.Worksheets.Add("LOAD SHEET");
                //IMAGEN AL CENTRO
                ws.Range(1, 7, 1, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                var imagePath = Server.MapPath("/") + "/Content/img/LOGOFORTUNEEXCEL.png";

                var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1"));
                ws.Cell("J1").Value = "DATE: ";
                ws.Cell("J2").Value = "DEPARTMENT: ";
                ws.Cell("J3").Value = "CONTAINER #: ";
                ws.Cell("J4").Value = "LOAD #: ";
                ws.Cell("N1").Value = DateTime.Now.ToString("MM/dd/yyyy");
                ws.Cell("N2").Value = "SHIPPING";
                ws.Cell("N3").Value = contenedor;
                ws.Cell("N4").Value = " ";

                ws.Range("J1", "M1").Merge();
                ws.Range("J2", "M2").Merge();
                ws.Range("J3", "M3").Merge();
                ws.Range("J4", "M4").Merge();
                ws.Range("J1", "M4").Style.Font.Bold = true;
                ws.Range("N1", "O1").Merge();
                ws.Range("N2", "O2").Merge();
                ws.Range("N3", "O3").Merge();
                ws.Range("N4", "O4").Merge();
                
                ws.Range("N1", "O4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Range("N1", "O1").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Range("N1", "O1").Style.Border.BottomBorderColor = XLColor.Black;
                ws.Range("N2", "O2").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Range("N2", "O2").Style.Border.BottomBorderColor = XLColor.Black;
                ws.Range("N3", "O3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Range("N3", "O3").Style.Border.BottomBorderColor = XLColor.Black;
                ws.Range("N4", "O4").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Range("N4", "O4").Style.Border.BottomBorderColor = XLColor.Black;

                var headers = new List<String[]>();
                List<String> titulos = new List<string>();
                titulos.Add("PLT"); titulos.Add("FOLIO"); titulos.Add("COMPANY"); titulos.Add("PO#"); titulos.Add("STYLE #");
                titulos.Add("COLOR"); titulos.Add("DESCRIPCION"); titulos.Add("DC#"); titulos.Add("RATIO"); titulos.Add("SIZE");
                titulos.Add("PCS"); titulos.Add("BXS"); titulos.Add("PL"); titulos.Add("BATCH# REC#"); titulos.Add("SHIP TO");
                headers.Add(titulos.ToArray());
                ws.Cell(5, 1).Value = headers;
                for (int i = 1; i <= 15; i++){
                    ws.Cell(5, i).Style.Font.Bold = true;
                    ws.Cell(5, i).Style.Fill.BackgroundColor = XLColor.FromArgb(209, 245, 253);
                    ws.Cell(5, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(5, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Cell(5, i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Cell(5, i).Style.Border.LeftBorderColor = XLColor.Black;
                    ws.Cell(5, i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    ws.Cell(5, i).Style.Border.RightBorderColor = XLColor.Black;
                    ws.Cell(5, i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Cell(5, i).Style.Border.TopBorderColor = XLColor.Black;
                    ws.Cell(5, i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Cell(5, i).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Cell(5, i).Style.Alignment.WrapText = true;
                }
                int row = 6, num_tarimas = 0, primer_fila_pk; //total = PLT
                int suma_piezas, suma_cajas, piezas, cajas, e_extras, e_normal, filas_total = 0;
                int filas_agregadas, ejemplos = 0;
                int rr = 5, ass_counter = 0, row_assort = 0;
                foreach (Pk p in lista){
                    ejemplos = 0;
                    primer_fila_pk = row;
                    suma_piezas = 0; suma_cajas = 0;
                    //p.parte = consultas.AddOrdinal(Convert.ToInt32(p.parte)) + " Part";

                    if ((p.parte).Contains(",")){
                        string[] parte_temporal = (p.parte).Split(',');
                        p.parte = consultas.AddOrdinal(Convert.ToInt32(parte_temporal[0])) + " Part and Final shipment";
                    }else{
                        switch (p.parte){
                            case "u":
                            case "0":
                                p.parte = " ";
                                break;
                            default:
                                p.parte = consultas.AddOrdinal(Convert.ToInt32(p.parte)) + " Part";
                                break;
                        }
                    }
                    string po = "", batch = "", packing_list_number, company = "";
                    int ex_label = ds.contar_labels(p.id_packing_list);
                    if (ex_label != 0){
                        List<Labels> lista_etiquetas = new List<Labels>();
                        lista_etiquetas = ds.obtener_etiquetas(p.id_packing_list);
                        //po = Regex.Replace(p.pedido, @"\s+", " ") + "(PO# ";
                        foreach (Labels l in lista_etiquetas) { /*po += " " + l.label;*/ batch += " " + l.label; }
                        if (ex_label == 1){
                            if (p.id_tipo == 27) {
                                batch = " (With TPM Labels) " + p.parte;
                            } else {
                                batch = " (With UCC Labels) " + p.parte;
                            }
                        }else{
                            batch = " (With TPM Labels) " + p.parte;
                        }
                    }else{
                        if (p.id_tipo == 7){
                            batch = "(Without TPM Labels) " + p.parte;
                        }else{
                            batch = "(Without UCC Labels) " + p.parte;
                        }
                    }
                    po = p.nombre_archivo;
                    string[] p_temporal = (p.packing).Split('-');
                    packing_list_number = "'" + p_temporal[0];
                    if (p.id_customer == 1) { company = "M.E."; } else { company = "FANTASY"; }
                    string estilo = "", color = "", dc = "", ratio = "", size = "", descripcion = "";
                    int estilos_total = 0;

                    switch (p.id_tipo){
                        case 1:
                        case 2:
                        case 9:
                            int mayor_1 = 0, menor_1 = 0;
                            List<Talla> tallas = new List<Talla>();
                            List<int> indices = new List<int>();
                            //TALLAS
                            foreach (Tarima t in p.lista_tarimas){//OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                                foreach (estilos e in t.lista_estilos){
                                    if (e.tipo_empaque != 3){
                                        foreach (ratio_tallas ra in e.lista_ratio){
                                            bool isEmpty = !tallas.Any();
                                            if (isEmpty){
                                                Talla ta = new Talla();
                                                ta.id_talla = ra.id_talla;
                                                ta.talla = ra.talla;
                                                tallas.Add(ta);
                                            }else{
                                                int existe = 0;
                                                foreach (Talla sizes in tallas){
                                                    if (sizes.id_talla == ra.id_talla) { existe++; }
                                                }
                                                if (existe == 0){
                                                    Talla ta = new Talla();
                                                    ta.id_talla = ra.id_talla;
                                                    ta.talla = ra.talla;
                                                    tallas.Add(ta);
                                                }
                                            }
                                        }
                                    }else{
                                        foreach (estilos ea in e.assort.lista_estilos){
                                            foreach (ratio_tallas ras in ea.lista_ratio){
                                                bool isEmpty = !tallas.Any();
                                                if (isEmpty){
                                                    Talla ta = new Talla();
                                                    ta.id_talla = ras.id_talla;
                                                    ta.talla = ras.talla;
                                                    tallas.Add(ta);
                                                }else{
                                                    int existe = 0;
                                                    foreach (Talla sizes in tallas){
                                                        if (sizes.id_talla == ras.id_talla) { existe++; }
                                                    }
                                                    if (existe == 0){
                                                        Talla ta = new Talla();
                                                        ta.id_talla = ras.id_talla;
                                                        ta.talla = ras.talla;
                                                        tallas.Add(ta);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }//OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                            }
                            //TALLAS
                            int primero_1 = 0;

                            foreach (Talla tt in tallas){
                                if (primero_1 == 0) { menor_1 = tt.id_talla; }
                                primero_1++;
                                mayor_1 = tt.id_talla;
                            }
                            size = consultas.obtener_size_id(menor_1.ToString()) + "-" + consultas.obtener_size_id(mayor_1.ToString());

                            foreach (Tarima t in p.lista_tarimas){//TARIMA --- TARIMA ---TARIMA --- TARIMA --- TARIMA
                                var filas = new List<String[]>();
                                e_extras = 0; e_normal = 0; filas_agregadas = 0;
                                //BUSCO LOS INDEX DE LOS DC Y CUENTO ESTILOS DE LA TARIMA
                                List<int> index = new List<int>();//BUSCO LOS INDEX DE LOS DC
                                List<int> index_bp = new List<int>();//BUSCO LOS INDEX DE LOS DC

                                foreach (estilos e in t.lista_estilos){
                                    if (e.tipo == "EXT" || e.tipo == "DMG"){
                                        index.Add(e.index_dc);
                                    }else{
                                        if (e.tipo_empaque == 1||e.tipo_empaque == 5){
                                            if (p.id_customer == 2){
                                                index.Add(e.index_dc);
                                            }else{
                                                if (p.id_tipo == 1){
                                                    if (e.dc != "0") { index.Add(e.index_dc); }
                                                    else { index_bp.Add(e.index_dc); }
                                                }else {
                                                    index.Add(e.index_dc);
                                                }
                                            }
                                        }else{ index.Add(e.index_dc); }
                                    }
                                }
                                index = index.Distinct().ToList();
                                index_bp = index_bp.Distinct().ToList();
                                foreach (int i in index_bp){
                                    foreach (estilos e in t.lista_estilos){
                                        if (e.index_dc == i){
                                            estilos_total++;
                                            if (e.tipo == "EXT" || e.tipo == "DMG") { e_extras++; }
                                            else { e_normal++; }
                                        }
                                    }
                                }
                                foreach (int i in index){
                                    int assort_contador = 0;
                                    foreach (estilos e in t.lista_estilos){
                                        if (e.index_dc == i){
                                            if (e.tipo == "EXT" || e.tipo == "DMG"){
                                                e_extras++;
                                            }else{
                                                e_normal++;
                                                if (e.tipo_empaque == 3){
                                                    assort_contador++;
                                                    foreach (estilos ee in e.assort.lista_estilos) { estilos_total++; }
                                                }
                                            }
                                        }
                                    }
                                    if (assort_contador == 0) { estilos_total++; }
                                }

                                foreach (int indice in index){ //ppks y bp
                                    int es_otro = 0;
                                    int contador_index = 0; piezas = 0; cajas = 0;
                                    foreach (estilos e in t.lista_estilos){
                                        if (e.index_dc == indice && e.tipo_empaque == 3 && e.tipo != "EXT" && e.tipo != "DMG"){
                                            piezas = 0; cajas = 0;
                                            if (e.usado == 0){
                                                if (e.tipo_empaque == 3 && e.tipo != "EXT" && e.tipo != "DMG"){
                                                    int estilos_assort = 0, estilos_tarima = 0;
                                                    foreach (estilos ee in e.assort.lista_estilos) { estilos_assort++; }
                                                    estilos_tarima += estilos_assort;
                                                    int contador_assort = 0;
                                                    //ESTILOS DEL ASSORT//ESTILOS DEL ASSORT
                                                    ass_counter = 0;
                                                    foreach (estilos ee in e.assort.lista_estilos){//ESTILOS DEL ASSORT
                                                        rr++;
                                                        row_assort = rr;
                                                        List<String> datos_a = new List<string>();
                                                        datos_a.Add(company);//-----------------------------------------------------------------------------------------------------
                                                        datos_a.Add(po);//-----------------------------------------------------------------------------------------------------
                                                        datos_a.Add(Regex.Replace(ee.estilo, @"\s+", ""));//-----------------------------------------------------------------------------------------------------
                                                        datos_a.Add(Regex.Replace(ee.color, @"\s+", " "));
                                                        datos_a.Add(Regex.Replace(ee.descripcion, @"\s+", " "));
                                                        cajas = 0; piezas = 0;
                                                        if (e.dc != "0") { datos_a.Add(e.dc); } else { datos_a.Add("N/A"); }
                                                        int contador = 0; string ppk_ratio = "";
                                                        foreach (Talla tt in tallas){
                                                            foreach (ratio_tallas r in ee.lista_ratio){
                                                                if (r.id_talla == tt.id_talla){
                                                                    contador++; ppk_ratio += r.ratio;
                                                                }
                                                            }
                                                            if (contador < ((ee.lista_ratio).Count)) { ppk_ratio += "-"; }
                                                        }
                                                        datos_a.Add(ppk_ratio);
                                                        datos_a.Add(size);
                                                        foreach (ratio_tallas r in ee.lista_ratio) { piezas += (r.ratio * e.boxes); }

                                                        datos_a.Add(Convert.ToString(piezas));
                                                        datos_a.Add(Convert.ToString(e.boxes));
                                                        cajas = e.boxes;
                                                        suma_piezas += piezas;
                                                        ass_counter++;
                                                        filas.Add(datos_a.ToArray());
                                                        filas_agregadas++; filas_total++;
                                                    }//ESTILOS DEL ASSORT
                                                }
                                            }
                                            ws.Range(row_assort, 12, (row_assort + ass_counter - 1), 12).Merge();
                                            ws.Range(row_assort, 12, (row_assort + ass_counter - 1), 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                            ws.Range(row_assort, 12, (row_assort + ass_counter - 1), 12).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                            suma_cajas += cajas;
                                        }
                                    }

                                    cajas = 0; piezas = 0;
                                    string estilo_f = "", descripcion_f = "", ratio_f = "", color_f = "", dc_f = "";
                                    foreach (estilos e in t.lista_estilos){
                                        if (e.index_dc == indice && e.tipo_empaque != 3){
                                            es_otro++;
                                            estilo_f = (e.estilo).Trim();
                                            color_f = (e.color).Trim();
                                            descripcion_f = (e.descripcion).Trim();
                                            if (e.dc != "0") { dc_f = e.dc; } else { dc_f = "N/A"; }
                                            int contador = 0;
                                            if (e.tipo_empaque == 2 || e.tipo_empaque == 4 && e.tipo != "DMG" && e.tipo != "EXT"){
                                                contador = 0; string ppk_ratio = "";
                                                foreach (Talla t2 in tallas){
                                                    foreach (ratio_tallas r in e.lista_ratio){
                                                        if (r.id_talla == t2.id_talla){
                                                            contador++; ppk_ratio += r.ratio;
                                                        }
                                                    }
                                                    if (contador < ((e.lista_ratio).Count)) { ppk_ratio += "-"; }
                                                }
                                                ratio_f = ppk_ratio;
                                            }else { ratio_f = "N/A"; }
                                            if (e.tipo == "DMG" || e.tipo == "EXT"){
                                                piezas += e.boxes;
                                                cajas = e.repeticiones;
                                            }else{
                                                if (e.tipo_empaque == 1 || e.tipo_empaque == 5){
                                                    if (e.dc != "0") {
                                                        if (e.tipo_empaque == 1){
                                                            piezas += (e.boxes * ds.buscar_piezas_empaque_bull(e.id_po_summary, e.id_talla));
                                                        }
                                                        if (e.tipo_empaque == 5){
                                                            piezas += (e.boxes * ds.buscar_piezas_empaque_bulls(e.id_po_summary, e.id_talla, e.packing_name));
                                                        }
                                                        cajas += e.boxes;
                                                    }else{
                                                        piezas += e.boxes;
                                                        if (e.sobrantes == 1){
                                                            cajas = e.repeticiones;
                                                        }else{
                                                            if (e.tipo_empaque == 1){
                                                                cajas += (e.boxes / ds.buscar_piezas_empaque_bull(e.id_po_summary, e.id_talla));
                                                            }
                                                            if (e.tipo_empaque == 5){
                                                                cajas += (e.boxes / ds.buscar_piezas_empaque_bulls(e.id_po_summary, e.id_talla, e.packing_name));
                                                            }
                                                        }
                                                    }
                                                }
                                                if (e.tipo_empaque == 2 || e.tipo_empaque == 4){
                                                    foreach (ratio_tallas r in e.lista_ratio){
                                                        piezas += (r.ratio * e.boxes);
                                                    }
                                                    cajas = e.boxes;
                                                }
                                            }//else
                                        }//if
                                    }

                                    if (es_otro != 0){
                                        rr++;
                                        List<String> datos = new List<string>();
                                        datos.Add(company);
                                        datos.Add(po);
                                        datos.Add(estilo_f);
                                        datos.Add(color_f);
                                        datos.Add(descripcion_f);
                                        datos.Add(dc_f);
                                        datos.Add(ratio_f);
                                        datos.Add(size);
                                        datos.Add(Convert.ToString(piezas));
                                        datos.Add(Convert.ToString(cajas));
                                        suma_cajas += cajas;
                                        suma_piezas += piezas;
                                        filas.Add(datos.ToArray());
                                        filas_agregadas++; filas_total++;
                                    }
                                }//INDICE

                                foreach (int indice in index_bp){ //BP TALLA / FILA                            
                                    int contador_index = 0;
                                    foreach (estilos e in t.lista_estilos){
                                        if (e.index_dc == indice){
                                            cajas = 0;
                                            piezas = 0;
                                            rr++;
                                            List<String> datos = new List<string>();
                                            datos.Add((company).Trim());
                                            datos.Add((po).Trim());
                                            datos.Add((e.estilo).Trim());
                                            datos.Add((e.color).Trim());
                                            datos.Add((e.descripcion).Trim());
                                            datos.Add("N/A");
                                            datos.Add("N/A");
                                            datos.Add(size);
                                            if (e.sobrantes == 1){
                                                cajas = e.repeticiones;
                                            }else{
                                                if (e.tipo_empaque == 1)
                                                {
                                                    cajas += (e.boxes / ds.buscar_piezas_empaque_bull(e.id_po_summary, e.id_talla));
                                                }
                                                if (e.tipo_empaque == 5)
                                                {
                                                    cajas += (e.boxes / ds.buscar_piezas_empaque_bulls(e.id_po_summary, e.id_talla, e.packing_name));
                                                }
                                            }
                                            piezas = e.boxes;
                                            datos.Add(Convert.ToString(piezas));
                                            datos.Add(Convert.ToString(cajas));
                                            suma_cajas += cajas;
                                            suma_piezas += piezas;
                                            filas.Add(datos.ToArray());
                                            filas_agregadas++; filas_total++;
                                        }// IF INDICE
                                    }//ESTILOS
                                }//INDICE

                                if (e_extras != 0 && e_normal == 0){
                                    ws.Cell(row, 1).Value = "*";
                                }else{
                                    num_tarimas++;
                                    ws.Cell(row, 1).Value = num_tarimas;
                                }
                                ws.Cell(row, 2).Value = t.id_tarima;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Merge();
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.LeftBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.RightBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.TopBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.BottomBorderColor = XLColor.Black;

                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Merge();
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.LeftBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.RightBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.TopBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.BottomBorderColor = XLColor.Black;

                                ws.Cell(row, 3).Value = filas;//// <-------------THIS!!

                                for (int r = row; r < filas_agregadas; r++){
                                    for (int c = 3; c <= 12; c++){
                                        ws.Cell(r, c).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.LeftBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.RightBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.TopBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.BottomBorderColor = XLColor.Black;
                                    }
                                }
                                row += filas_agregadas;
                            }//tarimatarimatarimatarimatarimatarimatarimatarimatarimatarimatarimatarimatarimatarimatarimatarimatarimatarimatarimatarimatarimatarima            

                            break;
                        case 3:
                            List<int> index_samples = new List<int>();
                            foreach (Sample s in p.lista_samples) { index_samples.Add(s.id_tarima); }
                            filas_agregadas = 0;
                            index_samples = index_samples.Distinct().ToList();

                            foreach (int i in index_samples){
                                var filas = new List<String[]>();
                                filas_agregadas = 0;
                                cajas = 0;
                                int sampler_counter = 0, sampler_row = 0;
                                foreach (Sample s in p.lista_samples){
                                    if (s.id_tarima == i && s.inicial == 1){
                                        piezas = 0;
                                        rr++;
                                        sampler_row = rr;
                                        List<String> datos = new List<string>();
                                        datos.Add(company);
                                        datos.Add(po);
                                        datos.Add(s.estilo);
                                        datos.Add(s.color);
                                        datos.Add(s.descripcion);
                                        datos.Add("N/A");
                                        datos.Add("N/A");
                                        datos.Add("0-5XT");

                                        piezas += s.talla_xs;
                                        piezas += s.talla_s;
                                        piezas += s.talla_m;
                                        piezas += s.talla_l;
                                        piezas += s.talla_xl;
                                        piezas += s.talla_2x;
                                        piezas += s.talla_3x;
                                        if (s.total_extras != 0){
                                            foreach (Extra_sample xs in s.lista_extras){
                                                piezas += xs.total;
                                            }
                                        }
                                        datos.Add(Convert.ToString(piezas));
                                        datos.Add(Convert.ToString(s.cajas));
                                        suma_piezas += piezas;
                                        suma_cajas += s.cajas;
                                        filas.Add(datos.ToArray());
                                        sampler_counter++;
                                        filas_agregadas++; filas_total++;
                                    }//FOREACH
                                }//iniciales =1
                                foreach (Sample s in p.lista_samples){
                                    if (s.id_tarima == i && s.inicial == 0){
                                        piezas = 0;
                                        rr++;
                                        List<String> datos = new List<string>();
                                        datos.Add(company);
                                        datos.Add(po);
                                        datos.Add(s.estilo);
                                        datos.Add(s.color);
                                        datos.Add(s.descripcion);
                                        datos.Add("N/A");
                                        datos.Add("N/A");
                                        datos.Add("0-5XT");

                                        piezas += s.talla_xs;
                                        piezas += s.talla_s;
                                        piezas += s.talla_m;
                                        piezas += s.talla_l;
                                        piezas += s.talla_xl;
                                        piezas += s.talla_2x;
                                        piezas += s.talla_3x;
                                        if (s.total_extras != 0){
                                            foreach (Extra_sample xs in s.lista_extras){
                                                piezas += xs.total;
                                            }
                                        }
                                        datos.Add(Convert.ToString(piezas));
                                        datos.Add(Convert.ToString(s.cajas));
                                        suma_piezas += piezas;
                                        suma_cajas += s.cajas;
                                        filas.Add(datos.ToArray());
                                        sampler_counter++;
                                        filas_agregadas++; filas_total++;
                                    }//FOREACH                                        
                                }//iniciales 0
                                ws.Range(sampler_row, 12, (sampler_row + sampler_counter - 1), 12).Merge();
                                ws.Range(sampler_row, 12, (sampler_row + sampler_counter - 1), 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                ws.Range(sampler_row, 12, (sampler_row + sampler_counter - 1), 12).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                                ws.Cell(row, 1).Value = "*";
                                ws.Cell(row, 2).Value = " ";
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Merge();
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Merge();
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.LeftBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.RightBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.TopBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.BottomBorderColor = XLColor.Black;

                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Merge();
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.LeftBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.RightBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.TopBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.BottomBorderColor = XLColor.Black;
                                ws.Cell(row, 3).Value = filas;//// <-------------THIS!!
                                for (int r = row; r < filas_agregadas; r++){
                                    for (int c = 3; c <= 12; c++){
                                        ws.Cell(r, c).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.LeftBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.RightBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.TopBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.BottomBorderColor = XLColor.Black;
                                    }
                                }
                                row += filas_agregadas;
                            }//TARIMAS
                            break;
                        case 5:
                            size = "";
                            estilos_total = 0;
                            int blanks = 0, trims = 0;
                            batch += " RETURNS";
                            List<int> lista_contador_estilos = new List<int>();
                            int mayor = 0, menor = 0;
                            List<int> lista_tallas = new List<int>();
                            foreach (Tarima t in p.lista_tarimas){
                                foreach (Return ret in t.lista_returns){
                                    bool isEmptyt = !lista_tallas.Any();
                                    if (isEmptyt) { lista_tallas.Add(ret.id_talla); }
                                    else{
                                        int existe = 0;
                                        foreach (int sizes in lista_tallas) { if (sizes == ret.id_talla) { existe++; } }
                                        if (existe == 0) { lista_tallas.Add(ret.id_talla); }
                                    }
                                }
                            }
                            int primero = 0;
                            foreach (int t in lista_tallas){
                                if (primero == 0) { menor = t; }
                                primero++;
                                mayor = t;
                            }
                            size = consultas.obtener_size_id(menor.ToString()) + "-" + consultas.obtener_size_id(mayor.ToString());
                            foreach (Tarima t in p.lista_tarimas){
                                var filas = new List<String[]>();
                                filas_agregadas = 0;
                                //**************************************************************************************************
                                if (p.id_customer == 1){//MAD ENGINE
                                    foreach (Return ret in t.lista_returns){
                                        estilos_total++;
                                        if (ret.id_categoria == 1){ //SI SON BLANKS
                                            blanks++;
                                        }else{
                                            trims++;
                                        }
                                    }
                                }else{//FANTASY
                                    foreach (Return ret in t.lista_returns){
                                        if (ret.id_categoria == 1){
                                            blanks++;
                                            bool isEmpty = !lista_contador_estilos.Any();
                                            if (isEmpty) { lista_contador_estilos.Add(ret.id_summary); }
                                            else{
                                                int existe = 0;
                                                foreach (int sizes in lista_contador_estilos) { if (sizes == ret.id_summary) { existe++; } }
                                                if (existe == 0) { lista_contador_estilos.Add(ret.id_summary); }
                                            }
                                        }else{
                                            trims++;
                                            estilos_total++;
                                        }
                                    }
                                    foreach (int x in lista_contador_estilos) { estilos_total++; }
                                }

                                if (p.id_customer == 1){//MAD ENGINE
                                    foreach (Return ret in t.lista_returns){
                                        if (ret.id_categoria == 1){
                                            rr++;
                                            List<String> datos = new List<string>(); //CREO LISTA PARA LOS DATOS   
                                            datos.Add(company);
                                            datos.Add(po);
                                            datos.Add(ret.estilo);
                                            datos.Add(ret.color);
                                            datos.Add(ret.descripcion);
                                            datos.Add("N/A");
                                            datos.Add("N/A");
                                            datos.Add(size);
                                            datos.Add(Convert.ToString(ret.total));
                                            datos.Add(Convert.ToString(ret.cajas));
                                            suma_piezas += ret.total;
                                            suma_cajas += ret.cajas;
                                            filas.Add(datos.ToArray());
                                            filas_total++; filas_agregadas++;
                                        }//CATEGORIA BLANKS
                                    }//FOREACH
                                }//MAD ENGINE
                                if (p.id_customer == 2){//FANTASY----FANTASY-----FANTASY----FANTASY----FANTASY---FANTASY-----FANTASY-----FANTASY-----FANTASY---FANTASY-----FANTASY---FANTASY---FANTASY---FANTASY----FANTASY
                                    List<int> lista_summary = new List<int>();
                                    //BUSCAR LOS DIFERENTES ESTILOS DE FANTASY
                                    foreach (Return ret in t.lista_returns){
                                        if (ret.id_categoria == 1){ //SI SON BLANKS
                                            bool isEmpty = !lista_summary.Any();
                                            if (isEmpty) { lista_summary.Add(ret.id_summary); }
                                            else{
                                                int existe = 0;
                                                foreach (int sizes in lista_summary) { if (sizes == ret.id_summary) { existe++; } }
                                                if (existe == 0) { lista_summary.Add(ret.id_summary); }
                                            }
                                        }
                                    }
                                    foreach (int s in lista_summary){
                                        int existe = 0;
                                        cajas = 0; piezas = 0;
                                        List<String> datos = new List<string>(); //CREO LISTA PARA LOS DATOS  
                                        foreach (Return ret in t.lista_returns){
                                            if (s == ret.id_summary && ret.id_categoria == 1){
                                                piezas += ret.total;
                                                cajas += ret.cajas;
                                                if (existe == 0){
                                                    estilo = ret.estilo;
                                                    color = ret.color;
                                                    descripcion = ret.descripcion;
                                                }//DATOS 
                                            }//COMPARACIÓN
                                        }//FOREACH RETURNS
                                        rr++;
                                        List<String> datos2 = new List<string>(); //CREO LISTA PARA LOS DATOS   
                                        datos2.Add(company);
                                        datos2.Add(po);
                                        datos2.Add(estilo);
                                        datos2.Add(color);
                                        datos2.Add(descripcion);
                                        datos2.Add("N/A");
                                        datos2.Add("N/A");
                                        datos2.Add(size);
                                        datos2.Add(Convert.ToString(piezas));
                                        datos2.Add(Convert.ToString(cajas));
                                        suma_piezas += piezas;
                                        suma_cajas += cajas;
                                        filas.Add(datos2.ToArray());
                                        filas_total++; filas_agregadas++;
                                    }//FOREACH SUMMARY
                                }//FANTASY   


                                foreach (Return ret in t.lista_returns){ //ahora imprimir trims---TRIMS --TRIMS --TRIMS --TRIMS --TRIMS --TRIMS --TRIMS   
                                    cajas = 0; piezas = 0;
                                    if (ret.id_categoria == 2){
                                        rr++;
                                        List<String> datos = new List<string>(); //CREO LISTA PARA LOS DATOS   
                                        datos.Add(company);
                                        datos.Add(po);
                                        datos.Add(ret.amt);
                                        datos.Add(ret.color);
                                        datos.Add(ret.descripcion_item);
                                        datos.Add("N/A");
                                        datos.Add("N/A");
                                        datos.Add(size);
                                        datos.Add(Convert.ToString(ret.total));
                                        datos.Add(Convert.ToString(ret.cajas));
                                        suma_piezas += ret.total;
                                        suma_cajas += ret.cajas;
                                        filas.Add(datos.ToArray());
                                        filas_total++; filas_agregadas++;
                                    }// categoria
                                }//lista returns

                                num_tarimas++;
                                ws.Cell(row, 1).Value = num_tarimas;
                                ws.Cell(row, 2).Value = t.id_tarima;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Merge();
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.LeftBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.RightBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.TopBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.BottomBorderColor = XLColor.Black;

                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Merge();
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.LeftBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.RightBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.TopBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.BottomBorderColor = XLColor.Black;
                                ws.Cell(row, 3).Value = filas;//// <-------------THIS!!
                                for (int r = row; r < filas_agregadas; r++){
                                    for (int c = 3; c <= 12; c++){
                                        ws.Cell(r, c).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.LeftBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.RightBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.TopBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.BottomBorderColor = XLColor.Black;
                                    }
                                }
                                row += filas_agregadas;
                            }//TARIMAS
                            break;
                        case 6:
                            filas_agregadas = 0;
                            List<string> lista_pos_fantasy = new List<string>();
                            foreach (Tarima t in p.lista_tarimas){
                                foreach (Sample s in t.lista_fantasy){
                                    bool isEmpty = !lista_pos_fantasy.Any();
                                    if (isEmpty) { lista_pos_fantasy.Add(s.pedido); }
                                    else{
                                        int existe = 0;
                                        foreach (string ss in lista_pos_fantasy) { if (ss == s.pedido) { existe++; } }
                                        if (existe == 0) { lista_pos_fantasy.Add(s.pedido); }
                                    }
                                }
                            }
                            string po_extras_fa = "";
                            int contador_po_extras_fa = 0, contador_po_extras_fa2 = 0;
                            foreach (string s in lista_pos_fantasy) { contador_po_extras_fa++; }
                            foreach (string s in lista_pos_fantasy){
                                contador_po_extras_fa2++;
                                po_extras_fa += " " + s;
                                if (contador_po_extras_fa2 != contador_po_extras_fa){
                                    po_extras_fa += ",";
                                }
                            }
                            foreach (Tarima t in p.lista_tarimas){
                                var filas = new List<String[]>();
                                estilos_total = 0;
                                foreach (Sample s in t.lista_fantasy) { estilos_total++; }
                                foreach (Sample s in t.lista_fantasy){
                                    piezas = 0; cajas = 0;
                                    rr++;
                                    List<String> datos = new List<string>();
                                    datos.Add(company);
                                    datos.Add((po_extras_fa).Trim());
                                    datos.Add(s.estilo);
                                    datos.Add(s.color);
                                    datos.Add(s.descripcion);
                                    datos.Add("N/A");
                                    datos.Add("N/A");
                                    datos.Add("SM-2X");
                                    piezas += s.talla_s;
                                    piezas += s.talla_m;
                                    piezas += s.talla_l;
                                    piezas += s.talla_xl;
                                    piezas += s.talla_2x;
                                    cajas += s.cajas;
                                    datos.Add(Convert.ToString(piezas));
                                    datos.Add(Convert.ToString(cajas));
                                    suma_piezas += piezas;
                                    suma_cajas += cajas;
                                    filas.Add(datos.ToArray());
                                    filas_total++; filas_agregadas++;
                                }//FOREACH
                                ws.Cell(row, 1).Value = "*";
                                ws.Cell(row, 2).Value = t.id_tarima;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Merge();
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.LeftBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.RightBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.TopBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.BottomBorderColor = XLColor.Black;

                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Merge();
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.LeftBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.RightBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.TopBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.BottomBorderColor = XLColor.Black;
                                ws.Cell(row, 3).Value = filas;//// <-------------THIS!!
                                for (int r = row; r < filas_agregadas; r++){
                                    for (int c = 3; c <= 12; c++){
                                        ws.Cell(r, c).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.LeftBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.RightBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.TopBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.BottomBorderColor = XLColor.Black;
                                    }
                                }
                                row += filas_agregadas;

                            }//TARIMAS  
                            break;
                        case 4:
                            size = "";
                            filas_agregadas = 0;
                            int mayor_4 = 0, menor_4 = 0;
                            List<int> lista_t = new List<int>();

                            size = "SM-2XL";
                            foreach (Tarima t in p.lista_tarimas){
                                var filas = new List<String[]>();
                                filas_agregadas = 0;
                                //****************N*O*R*M*A*L*E*S************************************************

                                List<int> index = new List<int>();//BUSCO LOS INDEX DE LOS DC
                                foreach (estilos e in t.lista_estilos) { index.Add(e.index_dc); }
                                index = index.Distinct().ToList();
                                foreach (int i in index) { estilos_total++; }
                                foreach (int indice in index){ //ppks y bp
                                    cajas = 0; piezas = 0;
                                    string estilo_f = "", descripcion_f = "", ratio_f = "", color_f = "", dc_f = "";
                                    foreach (estilos e in t.lista_estilos){
                                        if (e.index_dc == indice){
                                            estilo_f = (e.estilo).Trim();
                                            color_f = (ds.obtener_color_estilo_fantasy(e.id_estilo)).Trim();
                                            descripcion_f = (e.descripcion).Trim();
                                            if (e.dc != "0") { dc_f = e.dc; } else { dc_f = "N/A"; }
                                            ratio_f = "N/A";
                                            piezas += e.boxes;
                                        }//if
                                    }
                                    cajas = (piezas / 6);
                                    rr++;
                                    List<String> datos = new List<string>();
                                    datos.Add(company);
                                    datos.Add(po);
                                    datos.Add(estilo_f);
                                    datos.Add(color_f);
                                    datos.Add(descripcion_f);
                                    datos.Add(dc_f);
                                    datos.Add(ratio_f);
                                    datos.Add(size);
                                    datos.Add(Convert.ToString(piezas));
                                    datos.Add(Convert.ToString(cajas));
                                    suma_cajas += cajas;
                                    suma_piezas += piezas;
                                    filas.Add(datos.ToArray());
                                    filas_agregadas++; filas_total++;
                                }//INDICE
                                num_tarimas++;
                                ws.Cell(row, 1).Value = num_tarimas;
                                ws.Cell(row, 2).Value = t.id_tarima;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Merge();
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.LeftBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.RightBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.TopBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.BottomBorderColor = XLColor.Black;

                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Merge();
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.LeftBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.RightBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.TopBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.BottomBorderColor = XLColor.Black;
                                ws.Cell(row, 3).Value = filas;//// <-------------THIS!!
                                for (int r = row; r < filas_agregadas; r++){
                                    for (int c = 3; c <= 12; c++){
                                        ws.Cell(r, c).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.LeftBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.RightBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.TopBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.BottomBorderColor = XLColor.Black;
                                    }
                                }
                                row += filas_agregadas;
                            }
                            break;
                        case 7:
                            List<Talla> tallas_ht = new List<Talla>();
                            List<Talla> tallas_bpdc = new List<Talla>();
                            List<Talla> tallas_bpdc_restante = new List<Talla>();

                            suma_cajas = 0; suma_piezas = 0;
                            foreach (Tarima t in p.lista_tarimas){//OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                                foreach (estilos e in t.lista_estilos){
                                    if (e.tipo_empaque != 3){
                                        foreach (ratio_tallas ra in e.lista_ratio){
                                            bool isEmpty = !tallas_ht.Any();
                                            if (isEmpty){
                                                Talla ta = new Talla();
                                                ta.id_talla = ra.id_talla;
                                                ta.talla = ra.talla;
                                                tallas_ht.Add(ta);
                                            }else{
                                                int existe = 0;
                                                foreach (Talla sizes in tallas_ht){
                                                    if (sizes.id_talla == ra.id_talla) { existe++; }
                                                }
                                                if (existe == 0){
                                                    Talla ta = new Talla();
                                                    ta.id_talla = ra.id_talla;
                                                    ta.talla = ra.talla;
                                                    tallas_ht.Add(ta);
                                                }
                                            }
                                        }
                                    }else{
                                        foreach (estilos ea in e.assort.lista_estilos){
                                            foreach (ratio_tallas ras in ea.lista_ratio){
                                                bool isEmpty = !tallas_ht.Any();
                                                if (isEmpty){
                                                    Talla ta = new Talla();
                                                    ta.id_talla = ras.id_talla;
                                                    ta.talla = ras.talla;
                                                    tallas_ht.Add(ta);
                                                }else{
                                                    int existe = 0;
                                                    foreach (Talla sizes in tallas_ht){
                                                        if (sizes.id_talla == ras.id_talla) { existe++; }
                                                    }
                                                    if (existe == 0){
                                                        Talla ta = new Talla();
                                                        ta.id_talla = ras.id_talla;
                                                        ta.talla = ras.talla;
                                                        tallas_ht.Add(ta);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }//OBTENER_TODAS LAS TALLAS DE TODOS LOS ESTILOS
                            }
                            tallas_ht = ds.obtener_tallas_pk(tallas_ht);
                            int primero_ht = 0, menor_ht = 0, mayor_ht = 0;
                            foreach (Talla tt in tallas_ht){
                                if (primero_ht == 0) { menor_ht = tt.id_talla; }
                                primero_ht++;
                                mayor_ht = tt.id_talla;
                            }
                            size = consultas.obtener_size_id(menor_ht.ToString()) + "-" + consultas.obtener_size_id(mayor_ht.ToString());

                            foreach (Tarima t in p.lista_tarimas){
                                List<int> indices_ht = new List<int>();//BUSCO LOS INDEX DE LOS DC
                                var filas = new List<String[]>();
                                e_extras = 0; e_normal = 0;
                                estilos_total = 0; filas_agregadas = 0;
                                var celdas_estilos_i = new List<String[]>();
                                var celdas_estilos = new List<String[]>();
                                //BUSCO LOS INDEX DE LOS DC Y CUENTO ESTILOS DE LA TARIMA
                                foreach (estilos e in t.lista_estilos){//BUSCO LOS INDEX DE LOS DC Y CUENTO ESTILOS DE LA TARIMA
                                    bool isEmpty = !indices_ht.Any();
                                    if (isEmpty){
                                        indices_ht.Add(e.index_dc);
                                        estilos_total++;
                                    }else{
                                        int existe = 0;
                                        foreach (int i in indices_ht){
                                            if (i == e.index_dc) { existe++; }
                                        }
                                        if (existe == 0){
                                            indices_ht.Add(e.index_dc);
                                            estilos_total++;
                                        }
                                    }
                                    if (e.tipo == "EXT" || e.tipo == "DMG") { e_extras++; }
                                    else { e_normal++; }
                                }
                                foreach (Talla sizes in tallas_ht) { sizes.total = 0; }
                                tallas_bpdc = tallas_ht;
                                tallas_bpdc_restante = tallas_ht;
                                foreach (int i in indices_ht){//INDICES
                                    int contador_index = 0, bullpack = 0, cajas_t = 0;
                                    piezas = 0; cajas = 0;
                                    foreach (Talla sizes in tallas_bpdc) { sizes.total = 0; sizes.ratio = 0; }
                                    foreach (Talla sizes in tallas_bpdc_restante) { sizes.total = 0; sizes.ratio = 0; }
                                    string tipo_t = "", estilo_t = "", color_t = "", descripcion_t = "", ppk_t = "", dc_t = "";
                                    foreach (estilos e in t.lista_estilos){
                                        if (i == e.index_dc){
                                            if (e.tipo != "NONE") { tipo_t = e.tipo; }
                                            estilo_t = (e.estilo).Trim();
                                            color_t = (e.color).Trim();
                                            descripcion_t = (e.descripcion).Trim();
                                            if (e.dc != "0") { dc_t = e.dc; } else { dc_t = "N/A"; }
                                            if (e.tipo_empaque == 1){
                                                bullpack++;
                                                ppk_t = "N/A";
                                                foreach (Talla sizes in tallas_bpdc){
                                                    if (sizes.id_talla == e.id_talla) { sizes.total = e.boxes; }
                                                }
                                            }else{
                                                List<ratio_tallas> ratios = ds.obtener_lista_ratio_hottopic(e.id_po_summary, e.number_po, "2");
                                                int contador = 0; string ppk_ratio = "";
                                                foreach (ratio_tallas r in ratios) { if (r.ratio != 0) { contador++; } }
                                                int contador2 = 0;
                                                foreach (Talla t7 in tallas_ht){
                                                    foreach (ratio_tallas r in ratios){
                                                        if (r.ratio != 0 && r.id_talla == t7.id_talla){
                                                            contador2++; ppk_ratio += r.ratio;
                                                        }
                                                    }
                                                    if (contador2 != contador) { ppk_ratio += "-"; }
                                                }
                                                cajas_t = e.boxes;
                                                ppk_t = ppk_ratio;
                                                foreach (Talla sizes in tallas_bpdc){
                                                    foreach (ratio_tallas r in ratios){
                                                        if (sizes.id_talla == r.id_talla){
                                                            sizes.total = r.ratio * e.repeticiones * e.boxes;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }//*****************************E*S*T*I*L*O*S*****I*N*D*I*C*E*S*****************************
                                    if (bullpack != 0){
                                        int llave = 0, primer_vuelta = 0, extras = 0;
                                        do{
                                            rr++;
                                            List<String> datos_dc_bp = new List<string>();
                                            piezas = 0; cajas = 0;
                                            datos_dc_bp.Add(company);
                                            datos_dc_bp.Add(po);
                                            datos_dc_bp.Add(estilo_t);
                                            datos_dc_bp.Add(color_t);
                                            datos_dc_bp.Add(descripcion_t);
                                            datos_dc_bp.Add(dc_t);
                                            datos_dc_bp.Add(ppk_t);
                                            datos_dc_bp.Add(size);
                                            foreach (Talla sizes in tallas_bpdc){
                                                if (sizes.total != 0){
                                                    int total = 0;
                                                    if (tipo_t == "DMG" || tipo_t == "EXT"){
                                                        total = sizes.total;
                                                        piezas += sizes.total;
                                                        if (extras == 0) { cajas++; }
                                                        extras++;
                                                    }else{
                                                        if (primer_vuelta == 0){
                                                            total = sizes.total - (sizes.total % 50);
                                                            piezas += sizes.total - (sizes.total % 50);
                                                        }else{
                                                            piezas += sizes.total;
                                                            total = sizes.total;
                                                        }
                                                        if (total != 0){
                                                            if (total > 50){
                                                                cajas += (total / 50);
                                                            }else{
                                                                cajas++;
                                                            }
                                                        }
                                                    }
                                                    sizes.total = sizes.total - total;
                                                }
                                            }
                                            primer_vuelta++;
                                            datos_dc_bp.Add(Convert.ToString(piezas));
                                            datos_dc_bp.Add(Convert.ToString(cajas));
                                            suma_piezas += piezas;
                                            suma_cajas += cajas;
                                            if (piezas != 0 && cajas != 0){
                                                filas.Add(datos_dc_bp.ToArray());
                                                filas_total++; filas_agregadas++;
                                            }
                                            int existe = 0;
                                            foreach (Talla sizes in tallas_bpdc) { if (sizes.total != 0) { existe++; } }
                                            if (existe == 0) { llave++; }
                                        } while (llave == 0);
                                    }else{//PPK
                                        piezas = 0; cajas = 0;
                                        rr++;
                                        List<String> datos_dc_bp = new List<string>();
                                        datos_dc_bp.Add(company);
                                        datos_dc_bp.Add(po);
                                        datos_dc_bp.Add(estilo_t);
                                        datos_dc_bp.Add(color_t);
                                        datos_dc_bp.Add(descripcion_t);
                                        datos_dc_bp.Add(dc_t);
                                        datos_dc_bp.Add(ppk_t);
                                        datos_dc_bp.Add(size);
                                        foreach (Talla sizes in tallas_bpdc) { piezas += sizes.total; }
                                        cajas = cajas_t;
                                        datos_dc_bp.Add(Convert.ToString(piezas));
                                        datos_dc_bp.Add(Convert.ToString(cajas));
                                        suma_piezas += piezas;
                                        suma_cajas += cajas;
                                        if (piezas != 0 && cajas != 0){
                                            filas.Add(datos_dc_bp.ToArray());
                                            filas_total++; filas_agregadas++;
                                        }
                                    }
                                }//*************I*N*D*I*C*E*S*******************************************************************                        
                                if (e_normal == 0) { ws.Cell(row, 1).Value = "*"; }
                                else{
                                    num_tarimas++;
                                    ws.Cell(row, 1).Value = num_tarimas;
                                }
                                ws.Cell(row, 2).Value = t.id_tarima;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Merge();
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.LeftBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.RightBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.TopBorderColor = XLColor.Black;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 1, (row + filas_agregadas - 1), 1).Style.Border.BottomBorderColor = XLColor.Black;

                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Merge();
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.LeftBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.RightBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.TopBorderColor = XLColor.Black;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                ws.Range(row, 2, (row + filas_agregadas - 1), 2).Style.Border.BottomBorderColor = XLColor.Black;

                                ws.Cell(row, 3).Value = filas;//// <-------------THIS!!

                                for (int r = row; r < filas_agregadas; r++){
                                    for (int c = 3; c <= 12; c++){
                                        ws.Cell(r, c).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.LeftBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.RightBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.TopBorderColor = XLColor.Black;
                                        ws.Cell(r, c).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                        ws.Cell(r, c).Style.Border.BottomBorderColor = XLColor.Black;
                                    }
                                }
                                row += filas_agregadas;
                            }
                            break;
                    }
                    if (ejemplos != 0){
                        ws.Cell(primer_fila_pk, 14).Value = "SAMPLES";
                    }else{
                        ws.Cell(primer_fila_pk, 14).Value = batch;
                    }

                    ws.Range(primer_fila_pk, 14, (row - 1), 14).Style.Alignment.WrapText = true;
                    ws.Range(primer_fila_pk, 14, (row - 1), 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(primer_fila_pk, 14, (row - 1), 14).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Range(primer_fila_pk, 14, (row - 1), 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(primer_fila_pk, 14, (row - 1), 14).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Range(primer_fila_pk, 14, (row - 1), 14).Style.Border.LeftBorderColor = XLColor.Black;
                    ws.Range(primer_fila_pk, 14, (row - 1), 14).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    ws.Range(primer_fila_pk, 14, (row - 1), 14).Style.Border.RightBorderColor = XLColor.Black;
                    ws.Range(primer_fila_pk, 14, (row - 1), 14).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Range(primer_fila_pk, 14, (row - 1), 14).Style.Border.TopBorderColor = XLColor.Black;
                    ws.Range(primer_fila_pk, 14, (row - 1), 14).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(primer_fila_pk, 14, (row - 1), 14).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(primer_fila_pk, 14, (row - 1), 14).Merge();
                    ws.Range(primer_fila_pk, 14, (row - 1), 14).Style.Alignment.WrapText = true;

                    ws.Cell(primer_fila_pk, 13).Value = packing_list_number;

                    ws.Range(primer_fila_pk, 13, (row - 1), 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(primer_fila_pk, 13, (row - 1), 13).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Range(primer_fila_pk, 13, (row - 1), 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(primer_fila_pk, 13, (row - 1), 13).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Range(primer_fila_pk, 13, (row - 1), 13).Style.Border.LeftBorderColor = XLColor.Black;
                    ws.Range(primer_fila_pk, 13, (row - 1), 13).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    ws.Range(primer_fila_pk, 13, (row - 1), 13).Style.Border.RightBorderColor = XLColor.Black;
                    ws.Range(primer_fila_pk, 13, (row - 1), 13).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Range(primer_fila_pk, 13, (row - 1), 13).Style.Border.TopBorderColor = XLColor.Black;
                    ws.Range(primer_fila_pk, 13, (row - 1), 13).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(primer_fila_pk, 13, (row - 1), 13).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(primer_fila_pk, 13, (row - 1), 13).Merge();

                    ws.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Column(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Column(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Column(2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    ws.Range(row, 1, row, 7).Merge();
                    ws.Cell(row, 8).Value = "TOTAL";
                    ws.Range(row, 8, row, 10).Merge();
                    ws.Range(row, 8, row, 12).Style.Fill.BackgroundColor = XLColor.FromArgb(221, 255, 227);
                    ws.Range(row, 8, row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(row, 1, row, 7).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 7).Style.Border.LeftBorderColor = XLColor.Black;
                    ws.Range(row, 1, row, 7).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 7).Style.Border.RightBorderColor = XLColor.Black;
                    ws.Range(row, 1, row, 7).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 7).Style.Border.TopBorderColor = XLColor.Black;
                    ws.Range(row, 1, row, 7).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 7).Style.Border.BottomBorderColor = XLColor.Black;

                    ws.Range(row, 8, row, 10).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 8, row, 10).Style.Border.LeftBorderColor = XLColor.Black;
                    ws.Range(row, 8, row, 10).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 8, row, 10).Style.Border.RightBorderColor = XLColor.Black;
                    ws.Range(row, 8, row, 10).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 8, row, 10).Style.Border.TopBorderColor = XLColor.Black;
                    ws.Range(row, 8, row, 10).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 8, row, 10).Style.Border.BottomBorderColor = XLColor.Black;

                    ws.Range(row, 8, row, 11).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 8, row, 11).Style.Border.LeftBorderColor = XLColor.Black;
                    ws.Range(row, 8, row, 11).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 8, row, 11).Style.Border.RightBorderColor = XLColor.Black;
                    ws.Range(row, 8, row, 11).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 8, row, 11).Style.Border.TopBorderColor = XLColor.Black;
                    ws.Range(row, 8, row, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 8, row, 11).Style.Border.BottomBorderColor = XLColor.Black;

                    ws.Range(row, 8, row, 12).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 8, row, 12).Style.Border.LeftBorderColor = XLColor.Black;
                    ws.Range(row, 8, row, 12).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 8, row, 12).Style.Border.RightBorderColor = XLColor.Black;
                    ws.Range(row, 8, row, 12).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 8, row, 12).Style.Border.TopBorderColor = XLColor.Black;
                    ws.Range(row, 8, row, 12).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 8, row, 12).Style.Border.BottomBorderColor = XLColor.Black;

                    ws.Cell(row, 11).Value = suma_piezas;
                    ws.Cell(row, 12).Value = suma_cajas;
                    ws.Range(row, 13, row, 14).Merge();
                    row++; rr++;
                    ws.Range(row, 1, row, 14).Merge();
                    ws.Range(row, 1, row, 14).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                    ws.Range(row, 1, row, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 14).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 14).Style.Border.LeftBorderColor = XLColor.Black;
                    ws.Range(row, 1, row, 14).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 14).Style.Border.RightBorderColor = XLColor.Black;
                    ws.Range(row, 1, row, 14).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 14).Style.Border.TopBorderColor = XLColor.Black;
                    ws.Range(row, 1, row, 14).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 14).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Row(row).Height = 30;
                    row++;
                    rr++;
                }//PK --- PK --- PK --- PK --- PK --- PK ---PK --- PK --- PK --- PK --- PK --- PK --- PK --- PK --- PK --- PK --- PK --- PKif()
                if (total_packings != 0){
                    ws.Cell(6, 15).Value = "M E O T Y - K E R N S ";
                    //ws.Cell(6, 15).Style.Font.FontSize = 26;                
                    ws.Cell(6, 15).Style.Font.FontColor = XLColor.FromArgb(0, 32, 96);
                    ws.Range(6, 15, row - 1, 15).Style.Alignment.WrapText = true;
                    ws.Range(6, 15, row - 1, 15).Style.Fill.BackgroundColor = XLColor.FromArgb(221, 255, 227);
                    ws.Range(6, 15, row - 1, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(6, 15, row - 1, 15).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    ws.Range(6, 15, row - 1, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(6, 15, row - 1, 15).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Range(6, 15, row - 1, 15).Style.Border.LeftBorderColor = XLColor.Black;
                    ws.Range(6, 15, row - 1, 15).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    ws.Range(6, 15, row - 1, 15).Style.Border.RightBorderColor = XLColor.Black;
                    ws.Range(6, 15, row - 1, 15).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Range(6, 15, row - 1, 15).Style.Border.TopBorderColor = XLColor.Black;
                    ws.Range(6, 15, row - 1, 15).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Range(6, 15, row - 1, 15).Style.Border.BottomBorderColor = XLColor.Black;
                    ws.Range(6, 15, row - 1, 15).Merge();

                    ws.Style.Font.FontSize = 10;
                    ws.Columns().AdjustToContents();
                    ws.Rows().AdjustToContents();

                    ws.Range("J1", "M4").Style.Font.FontSize = 15;
                    ws.Range("J1", "M4").Style.Font.Bold = true;
                    ws.Rows().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range("J1", "M4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(row + 2, 2).Value = "DOCUMENTO CONTROLADO- UNICAMENTE SE PUEDE MODIFICAR POR EL SUPERVISOR DE SHIPPING";
                    ws.Cell(row + 2, 2).Style.Font.FontColor = XLColor.FromArgb(128, 128, 128);
                    ws.Range(row + 2, 2, row + 2, 14).Merge();

                    ws.Range("A1", "R4").Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255);
                    ws.Columns("P").Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255);
                    ws.Columns("Q").Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255);
                    ws.Columns("R").Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255);
                    ws.Range(row, 1, row + 5, 18).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255);

                    ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }
                // ws.Columns().AdjustToContents();
                // ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                /***********D*O*W*N*L*O*A*D*************************************************************************************************************************************************************************/
                HttpResponse httpResponse = System.Web.HttpContext.Current.Response;
                httpResponse.Clear();
                httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                httpResponse.AddHeader("content-disposition", "attachment;filename=\"LOAD SHEET.xlsx\"");
                // Flush the workbook to the Response.OutputStream

                using (MemoryStream memoryStream = new MemoryStream()){
                    libro_trabajo.SaveAs(memoryStream);
                    memoryStream.WriteTo(httpResponse.OutputStream);
                    memoryStream.Close();
                }
                httpResponse.End();
            }

        }
        public JsonResult buscar_estilos_summary(string summary){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            if (summary != "0"){
                int pedido = consultas.obtener_id_pedido_summary(Convert.ToInt32(summary));
                return Json(ds.lista_estilos(pedido.ToString()), JsonRequestBehavior.AllowGet);
            }else {
                return Json(ds.lista_estilos_abiertos(), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult buscar_clientes_select(){
            DatosTrim dtrim = new DatosTrim();
            return Json(dtrim.obtener_lista_clientes(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_estilos_fantasy_select(){
            DatosFantasy df = new DatosFantasy();
            return Json(df.obtener_lista_estilos(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_paises(string term){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            var items = consultas.Lista_paises();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_colores(string term){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            var items = consultas.Lista_colores();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_percents(string term){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            var items = consultas.Lista_porcentajes();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_generos(string term){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            var items = consultas.Lista_generos();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_product_type(string term){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            var items = consultas.Lista_tipos_productos();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult imprimir_bol(){
            //DatosShipping dsh = new DatosShipping();
            //return View("bol", dsh.obtener_packing_list(Convert.ToInt32(Session["pk"])));
            /* return new ViewAsPdf("bol", dsh.obtener_packing_list(Convert.ToInt32(Session["pk"]))){
                 FileName = filename,
                 PageOrientation = Rotativa.Options.Orientation.Portrait,
                 PageSize = Rotativa.Options.Size.Letter,
                 PageMargins = new Rotativa.Options.Margins(8, 10, 15, 10),
                 CustomSwitches = "--page-offset 0 --print-media-type ",
             };*/
            Dictionary<string, string> cookieCollection = new Dictionary<string, string>();
            foreach (var key in Request.Cookies.AllKeys){
                cookieCollection.Add(key, Request.Cookies.Get(key).Value);
            }
            return new ViewAsPdf("bol", ds.obtener_packing_list_bol(Convert.ToInt32(Session["pk"]))){
                FileName = "BOL-PK" + Convert.ToString(Session["pk"]) + ".pdf",
                Cookies = cookieCollection,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                PageSize = Rotativa.Options.Size.Letter,
                FormsAuthenticationCookieName = System.Web.Security.FormsAuthentication.FormsCookieName
            };
            /* var byteArray = abc.BuildPdf(ControllerContext);
             var fileStream = new System.IO.FileStream(Server.MapPath(subPath) + "/abc.pdf", FileMode.Create, FileAccess.Write);
             fileStream.Write(byteArray, 0, byteArray.Length);
             fileStream.Close();*/
        }

        public JsonResult guardar_cantidades_envio(string pedido,string id_talla,string estilo,string cant_talla,string tipo,string indice,string anterior){
            string[] ids_tallas = id_talla.Split('*'),estilos=estilo.Split('*'),cantidades_tallas=cant_talla.Split('*'),tipos=tipo.Split('*'),indices=indice.Split('*'),anteriores=anterior.Split('*');
            string fecha_anterior = "";
            for (int i = 1; i < estilos.Length; i++) {
                if (anteriores[i] == "1"){
                    fecha_anterior += "*" + ds.buscar_fechas_envio_anterior_extra(indices[i], estilos[i]);
                }else {
                    fecha_anterior += "*0";
                }
            }
            string[] fechas_anteriores = fecha_anterior.Split('*');
            ds.eliminar_cantidades_enviadas_extra(pedido);
            for (int i = 1; i < estilos.Length; i++) {
                string[] cantidades = cantidades_tallas[i].Split('&');
                string fecha;
                if (fechas_anteriores[i] != "0") {
                    fecha = fechas_anteriores[i];
                }else{
                    fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                for (int j = 1; j < ids_tallas.Length; j++){
                    if (Convert.ToInt32(cantidades[j]) != 0){
                        ds.guardar_cantidades_enviadas_extra(pedido,estilos[i], ids_tallas[j], cantidades[j],tipos[i],indices[i],fecha);
                    }
                }
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }




























































    }
}