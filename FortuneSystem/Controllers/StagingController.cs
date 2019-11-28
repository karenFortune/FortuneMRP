using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using Rotativa;
using System.Web.Routing;
using FortuneSystem.Models.Staging;
using FortuneSystem.Models.Almacen;
using System.IO;
using ClosedXML.Excel;
namespace FortuneSystem.Controllers
{
    public class StagingController : Controller
    {
        // GET: Staging
        StagingGeneral stag = new StagingGeneral();
        FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
        DatosStaging ds = new DatosStaging();
        DatosInventario di= new DatosInventario();

        public ActionResult Index(){
            //Session["id_usuario"] = consultas.buscar_id_usuario(Convert.ToString(Session["usuario"]));
            //Session["id_usuario"] = 2;
            //Session["id_sucursal"] = consultas.obtener_sucursal_id_usuario(Convert.ToInt32(Session["id_usuario"]));
            
            //if (Session["id_Empleado"] != null){
                int id_usuario = Convert.ToInt32(Session["id_Empleado"]);
                Session["id_usuario"] = id_usuario;                
                Session["id_sucursal"] = consultas.obtener_sucursal_id_usuario(Convert.ToInt32(Session["id_usuario"]));
                Session["turno"] = consultas.obtener_turno_usuario(Convert.ToInt32(Session["id_usuario"]));
            int departamento = consultas.obtener_departamento_id_usuario(id_usuario);
            ViewBag.departamento = departamento;
            return View();
           /* }else{
                return RedirectToAction("Index", "Login");
            }*/
        }
        public ActionResult Autocomplete_paises(string term)
        {
            var items = consultas.Lista_paises();
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
        public ActionResult Autocomplete_tallas(string term)
        {
            var items = consultas.Lista_tallas();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_percents(string term)
        {
            var items = consultas.Lista_porcentajes();
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Autocomplete_empleados(string term)
        {
            var items = consultas.Lista_empleados(Convert.ToInt32(Session["turno"]),1);
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }

        
       
        [HttpPost]
        public JsonResult imprimir_papeleta_vacia_staging(string datos)
        {
            string[] data = datos.Split('&');
            //inventario//estilo//pedido
            Session["id_inventario"] = data[0];
            Session["id_estilo"] = data[1];
            Session["id_pedido"] = data[2];

            return Json("0", JsonRequestBehavior.AllowGet);
        }

       
        //buscar_staging_inicio
        public JsonResult buscar_staging_inicio(){
            return Json(ds.obtener_staging_inicio(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_staging_inicio_busqueda(string busqueda){
            return Json(ds.obtener_staging_inicio_busqueda(busqueda), JsonRequestBehavior.AllowGet);
        }
        //abrir_papeleta_stag
        public JsonResult abrir_papeleta_stag(string id){
            Session["id_staging"] = id;
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscar_datos_grafica() {
            return Json(stag.obtener_lista_staging_grafica(), JsonRequestBehavior.AllowGet);
        }
/***********************************************************************************************************************************************************************/
        public JsonResult buscar_pedidos_inicio(string busqueda){
            return Json(ds.buscar_pedidos_recibo(Convert.ToInt32(Session["id_sucursal"]),busqueda), JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_pedido_estilo_tallas(string datos) {
            string[] data = datos.Split('*');
            //summary//estilo//pedido
            Session["id_estilo_count"] = data[0];
            Session["id_pedido_count"] = data[1];
            Session["id_summary_count"] = data[2];           
            var resultado = Json(new {
                result = ds.lista_papeleta(Convert.ToInt32(Session["id_estilo_count"]), Convert.ToInt32(Session["id_pedido_count"]), Convert.ToInt32(Session["turno"])),
            });
            return Json(resultado, JsonRequestBehavior.AllowGet);           
        }

        public JsonResult datos_sesion_summary(string datos){
            string[] data = datos.Split('*');
            //summary//estilo//pedido
            Session["id_estilo_count"] = data[0];
            Session["id_pedido_count"] = data[1];
            Session["id_summary_count"] = data[2];
           
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult datos_sesion_summary_customer(string summary){
            Session["id_estilo_count"] = consultas.obtener_estilo_summary(Convert.ToInt32(summary));
            Session["id_pedido_count"] = consultas.obtener_id_pedido_summary(Convert.ToInt32(summary));
            Session["id_summary_count"] = summary;
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscar_conteos_estilo(){
            if (Session["id_Empleado"] != null){
                List<Talla_staging> lista_tallas = ds.obtener_cantidades_tallas_estilo(Convert.ToInt32(Session["id_summary_count"]));
                List<Talla_staging> totales_orden = ds.obtener_cantidades_tallas_estilo(Convert.ToInt32(Session["id_summary_count"]));
                List<stag_conteo> totales_stagin = ds.obtener_lista_staging_summary(Convert.ToInt32(Session["id_summary_count"]));
                List<recibos_item> recibos_staging = ds.obtener_lista_recibos_staging(Convert.ToInt32(Session["id_summary_count"]), lista_tallas);
                string nombre = consultas.obtener_estilo(Convert.ToInt32(Session["id_estilo_count"])) + " " + consultas.buscar_descripcion_estilo(Convert.ToInt32(Session["id_estilo_count"]));
                List<pedido_staging> orden = ds.obtener_informacion_pedido_summary(Convert.ToInt32(Session["id_pedido_count"]), Convert.ToInt32(Session["id_estilo_count"]), Convert.ToInt32(Session["id_summary_count"]));

                var resultado = Json(new
                {
                    result = ds.lista_papeleta(Convert.ToInt32(Session["id_estilo_count"]), Convert.ToInt32(Session["id_pedido_count"]), Convert.ToInt32(Session["turno"])),
                    lista_totales_orden = totales_orden,
                    lista_staging = totales_stagin,
                    estilo = nombre,
                    recibos = recibos_staging,
                    pedido=orden,
                    
                });
            return Json(resultado, JsonRequestBehavior.AllowGet);
            }else{
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult customer_stagging(){
            return View();
        }public ActionResult departamentos_stagging(){
            return View();
        }
        public JsonResult buscar_informacion_staging(string estilo){
            var result = Json(new{
                tallas = di.obtener_lista_tallas_summary(Convert.ToInt32(estilo)),
                staging_tallas = ds.obtener_lista_staging_summary(Convert.ToInt32(estilo)),
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult guardar_edicion_stag_bd(string staging,string pedido, string estilo, string summary, string size, string quantity, string employee, string fabric_percent, string country, string color, string comentario)
        {
            if (Session["id_Empleado"] != null)
            {
                int id_usuario = Convert.ToInt32(Session["id_Empleado"]);
                Session["id_usuario"] = id_usuario;
                Session["id_sucursal"] = consultas.obtener_sucursal_id_usuario(Convert.ToInt32(Session["id_usuario"]));
                Session["turno"] = consultas.obtener_turno_usuario(Convert.ToInt32(Session["id_usuario"]));                
            
                string[] tallas = size.Split('*'), cantidades = quantity.Split('*'), empleados = employee.Split('*'), porcentajes = fabric_percent.Split('*'), paises = country.Split('*'), colores = color.Split('*');
                int total = 0, id_size, id_color, id_pais, id_percent;
                for (int i = 1; i < cantidades.Length; i++){
                    total += Convert.ToInt32(cantidades[i]);
                }
                int id_staging = Convert.ToInt32(staging);

                ds.eliminar_conteos_staging(Convert.ToInt32(staging));
                ds.editar_staging(id_staging,total,comentario);

                for (int i = 1; i < cantidades.Length; i++){
                    // id_empleado = stag.obtener_id_empleado(empleados[i]);
                    id_size = consultas.buscar_talla(tallas[i]);
                    id_color = consultas.buscar_color(colores[i]);
                    id_pais = consultas.buscar_id_pais(paises[i]);
                    id_percent = consultas.buscar_percent(porcentajes[i]);
                    ds.guardar_stag_conteos(id_staging, id_size, id_pais, id_color, id_percent, Convert.ToInt32(cantidades[i]), empleados[i]);

                    int inventario = ds.buscar_inventario_stagging(Convert.ToInt32(summary), id_size);
                    recibos_item recibo = ds.buscar_recibo_stag(inventario);
                    ds.actualizar_cantidad_inventario(inventario, Convert.ToInt32(cantidades[i]), recibo.total);
                    ds.actualizar_cantidad_recibos_item(recibo.id_recibo_item, Convert.ToInt32(cantidades[i]), recibo.total);
                    ds.actualizar_cantidad_recibos(recibo.id_recibo);

            }
            Session["id_staging"] = id_staging;
            }           
            return Json("", JsonRequestBehavior.AllowGet);

        }
        public JsonResult guardar_staging(string id,string talla,string pedido, string estilo, string summary, string cantidad, string porcentaje, string pais, string color, string comentario){
            // " + pedido + "','':'" + estilo + "','':'" + summary + "','':'" + cantidad + "','':'" + porcentaje + "','':'" + pais + "','color':'" + color + "','comentario':'" + comentarios + "'}",
            int id_pedido = Convert.ToInt32(pedido), id_summary = Convert.ToInt32(summary), id_estilo = Convert.ToInt32(estilo);
            string[] ids= id.Split('*'),cantidades = cantidad.Split('*'),porcentajes=porcentaje.Split('*'),paises=pais.Split('*'),colores=color.Split('*'),tallas=talla.Split('*');
            int  id_color, id_pais, id_percent;
            string tempo_fechas = "";
            for (int i = 1; i < ids.Length; i++) {
                if (ids[i] != "0"){
                    tempo_fechas += "*" + ds.obtener_fecha_staging(ids[i]);
                }else {
                    tempo_fechas += "*" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            string[] fechas = tempo_fechas.Split('*');
            List<stag_conteo> totales_stagin = ds.obtener_lista_staging_summary(Convert.ToInt32(summary));
            foreach (stag_conteo stag in totales_stagin) {
                foreach (Talla_staging s in stag.lista_staging){
                    ds.eliminar_conteos_staging(s.id_staging_count);
                    //ELIMINAR TODO
                }
                ds.eliminar_staging(stag.id_staging);
            }
            for (int i = 1; i < ids.Length; i++) {
                var sum_total = 0;
                id_color = consultas.buscar_color(colores[i]);
                id_pais = consultas.buscar_id_pais(paises[i]);
                if (id_pais == 0){
                    consultas.crear_pais(paises[i]);
                    id_pais = consultas.buscar_id_pais(paises[i]);
                }
                id_percent = consultas.buscar_percent(porcentajes[i]);
               
                    string[] cantidad_tallas = cantidades[i].Split('&');
                    for (int j = 1; j < cantidad_tallas.Length; j++) { sum_total += Convert.ToInt32(cantidad_tallas[j]); }
                    ds.guardar_stag_bd(pedido, estilo, sum_total, Convert.ToInt32(Session["id_usuario"]), Convert.ToInt32(summary), comentario, fechas[i]);
                    int id_stag = ds.obtener_ultimo_stag();
                    for (int j = 1; j < cantidad_tallas.Length; j++){
                        if (Convert.ToInt32(cantidad_tallas[j]) != 0){
                            ds.guardar_stag_conteos(id_stag, Convert.ToInt32(tallas[j]), id_pais, id_color, id_percent, Convert.ToInt32(cantidad_tallas[j]), "ALL");                            
                        }
                    }
                //}
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscar_ordenes_reportes_recibos(string busqueda){
            return Json(ds.obtener_pedidos_reportes(busqueda), JsonRequestBehavior.AllowGet);
        }

        public JsonResult sesion_reporte(string estilo)
        {
            Session["estilos_reporte"] = estilo;
            return Json("", JsonRequestBehavior.AllowGet);
        }


        public void reporte_staging()
        {

            string archivo = "Staging " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            String[] lista_estilos = (Convert.ToString(Session["estilos_reporte"]).Split('*'));
            List<int> lista_pedidos = ds.buscar_lista_pedidos_estilos(lista_estilos);
            int row = 1;
            using (XLWorkbook libro_trabajo = new XLWorkbook())
            { //Regex.Replace(pedido, @"\s+", " "); 
                var ws = libro_trabajo.Worksheets.Add("Staging");

                var imagePath = Server.MapPath("/") + "/Content/img/LOGOFORTUNEEXCEL.png";
                //var imagePath = @"C:\Users\melissa\source\repos\FortuneSys----\FortuneSystem\Content\img\LOGO FORTUNE.png";
                //var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("E1")).Scale(0.30);
                var image = ws.AddPicture(imagePath).MoveTo(ws.Cell("A1"));

                ws.Cell("H1").Value = "Fortune Fashions Baja ";
                ws.Cell("H2").Value = "Calle Tortuga 313-A, Col. Maneadero,";
                ws.Cell("H3").Value = "Ensenada B.C. 22790";
                ws.Cell("H4").Value = "Phone:+52 (646) 152 52 77";
                ws.Range("H1:H4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                //CABECERAS TABLA
                row = 6; //AGREGAR CABECERA TABLA

                foreach (int p in lista_pedidos)
                {

                    ws.Cell(row, 5).Value = (consultas.obtener_po_id(Convert.ToString(p))).Trim();
                    ws.Cell(row, 5).Style.Font.Underline = XLFontUnderlineValues.Single;
                    ws.Range(row, 5, row + 1, 7).Merge();
                    ws.Range(row, 5, row + 1, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(row, 5, row + 1, 7).Style.Font.Bold = true;
                    ws.Range(row, 5, row + 1, 7).Style.Font.FontColor = XLColor.FromArgb(49, 112, 135); //AZUL-TURQUESA-NO SE QUE COLOR ES
                    ws.Range(row, 5, row + 1, 7).Style.Font.FontSize = 24;
                    row = row + 3;
                    for (int i = 1; i < lista_estilos.Length; i++)
                    {
                        int id_pedido = consultas.obtener_id_pedido_summary(Convert.ToInt32(lista_estilos[i]));
                        if (id_pedido == p)
                        {
                            ws.Cell(row, 2).Value = "ID";
                            ws.Cell(row, 5).Value = "STYLE";
                            ws.Cell(row, 8).Value = "COLOR DESCRIPTION";
                            ws.Range(row, 2, row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Range(row, 2, row, 8).Style.Font.Bold = true;
                            row++;
                            int id_estilo = consultas.obtener_estilo_summary(Convert.ToInt32(lista_estilos[i]));
                            List<Talla_staging> lista_tallas = ds.obtener_cantidades_tallas_estilo(Convert.ToInt32(lista_estilos[i]));//TOTALES Y TALLAS DEL ESTILO
                            List<Talla_staging> totales_orden = ds.obtener_cantidades_tallas_estilo(Convert.ToInt32(lista_estilos[i]));//TOTALES Y TALLAS DEL ESTILO
                            List<stag_conteo> totales_stagin = ds.obtener_lista_staging_summary(Convert.ToInt32(lista_estilos[i]));//REGISTROS DE STAGING DEL ESTILO
                            // List<recibos_item> recibos_staging = ds.obtener_lista_recibos_staging(Convert.ToInt32(summary), lista_tallas);
                            //List<pedido_staging> orden = ds.obtener_informacion_pedido_summary(Convert.ToInt32(Session["id_pedido_count"]), Convert.ToInt32(Session["id_estilo_count"]), Convert.ToInt32(Session["id_summary_count"]));

                            ws.Cell(row, 2).Value = id_estilo.ToString();
                            ws.Cell(row, 5).Value = (consultas.obtener_estilo(id_estilo)).Trim();
                            ws.Cell(row, 8).Value = (consultas.obtener_descripcion_color_id(Convert.ToString(consultas.obtener_color_id_item_cat(Convert.ToInt32(lista_estilos[i]))))).Trim();
                            ws.Range(row, 2, row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Range(row, 2, row, 8).Style.Font.Bold = true;
                            ws.Range(row, 2, row, 8).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0); //ROJO
                            row++;

                            ws.Cell(row, 5).Value = (consultas.buscar_descripcion_estilo(id_estilo)).Trim();
                            ws.Cell(row, 5).Style.Font.FontColor = XLColor.FromArgb(49, 112, 135); //AZUL-TURQUESA-NO SE QUE COLOR ES
                            ws.Cell(row, 5).Style.Font.FontSize = 14;
                            ws.Cell(row, 5).Style.Font.Bold = true;
                            ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            row++;

                            string tallas_tempo = "";
                            foreach (Talla_staging t in lista_tallas) { tallas_tempo += " " + t.talla; }
                            ws.Cell(row, 5).Value = tallas_tempo;
                            ws.Cell(row, 5).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0); //AZUL-TURQUESA-NO SE QUE COLOR ES
                            ws.Cell(row, 5).Style.Font.Bold = true;
                            ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            row++;
                            List<Talla_staging> lista_tallas_suma = lista_tallas;
                            List<Talla_staging> porcentaje_pais = new List<Talla_staging>();
                            //foreach (Talla_staging t in lista_tallas_suma) { t.total = 0; }
                            //AQUI EMPIEZA LA TABLA
                            int fila = 3, total_estilo = 0, inicio_tabla = row;
                            foreach (Talla_staging t in totales_orden)
                            {
                                ws.Cell(row, fila).Value = "'"+t.talla;
                                ws.Cell(row, fila).Style.Font.Bold = true;
                                ws.Cell(row, fila).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                total_estilo += t.total;
                                fila++;
                            }
                            fila++;
                            ws.Cell(row, fila).Value = "TOTAL";
                            ws.Cell(row, fila).Style.Font.Bold = true;
                            int columna_tabla = fila;
                            row++;
                            ws.Cell(row, fila).Value = total_estilo;
                            //row++;
                            foreach (stag_conteo stag in totales_stagin)
                            {
                                fila = 3;
                                foreach (Talla_staging t in totales_orden)
                                {
                                    foreach (Talla_staging s in stag.lista_staging)
                                    {
                                        if (s.id_talla == t.id_talla)
                                        {
                                            ws.Cell(row, fila).Value = s.total;
                                            ws.Cell(row, fila).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                            t.total = t.total - s.total;
                                        }
                                        bool isEmpty = !porcentaje_pais.Any();
                                        if (isEmpty)
                                        {
                                            Talla_staging nuevo = new Talla_staging();
                                            nuevo.pais = s.pais;
                                            nuevo.porcentaje = s.porcentaje;
                                            porcentaje_pais.Add(nuevo);
                                        }
                                        else
                                        {
                                            int existe = 0;
                                            foreach (Talla_staging st in porcentaje_pais)
                                            {
                                                if (s.pais == st.pais && s.porcentaje == st.porcentaje)
                                                {
                                                    existe++;
                                                }
                                            }
                                            if (existe == 0)
                                            {
                                                Talla_staging nuevo = new Talla_staging();
                                                nuevo.pais = s.pais;
                                                nuevo.porcentaje = s.porcentaje;
                                                porcentaje_pais.Add(nuevo);
                                            }
                                        }
                                    }
                                    fila++;
                                }
                                row++;
                            }
                            fila = 3;

                            foreach (Talla_staging t in totales_orden)
                            {

                                if (t.total == 0)
                                {
                                    ws.Cell(row, fila).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                                    ws.Cell(row, fila).Value = t.total;
                                    ws.Cell(row, fila).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                }
                                else
                                {
                                    if (t.total > 0)
                                    {
                                        ws.Cell(row, fila).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                                        ws.Cell(row, fila).Value = "-" + t.total;
                                        ws.Cell(row, fila).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    }
                                    else
                                    {
                                        ws.Cell(row, fila).Style.Font.FontColor = XLColor.FromArgb(12, 17, 228);
                                        ws.Cell(row, fila).Value = "+" + (t.total * -1);
                                        ws.Cell(row, fila).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    }
                                }
                                ws.Cell(row, fila).Style.Font.Bold = true;
                                fila++;
                            }
                            for (int f = inicio_tabla; f <= row; f++)
                            {
                                for (int c = 3; c <= columna_tabla; c++)
                                {
                                    ws.Cell(f, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                    ws.Cell(f, c).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                    ws.Cell(f, c).Style.Border.LeftBorderColor = XLColor.FromArgb(191, 191, 191);
                                    ws.Cell(f, c).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                    ws.Cell(f, c).Style.Border.RightBorderColor = XLColor.FromArgb(191, 191, 191);
                                    ws.Cell(f, c).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                    ws.Cell(f, c).Style.Border.TopBorderColor = XLColor.FromArgb(191, 191, 191);
                                    ws.Cell(f, c).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                    ws.Cell(f, c).Style.Border.BottomBorderColor = XLColor.FromArgb(191, 191, 191);
                                }
                            }
                            //AQUI TERMINA LA TABLA
                            row++;
                            foreach (Talla_staging pp in porcentaje_pais)
                            {
                                ws.Cell(row, 3).Value = pp.pais;
                                ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                ws.Cell(row, 6).Value = pp.porcentaje;
                                ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                row++;
                            }
                            row++;
                        }
                    }
                }
                ws.Range(1, 1, row, 80).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255);

                //ws.Rows().AdjustToContents();
                //ws.Columns().AdjustToContents();
                //ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
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