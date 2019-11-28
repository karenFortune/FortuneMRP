using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FortuneSystem.Models.Almacen;
using FortuneSystem.Models.Shipping;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FortuneSystem.Models.Trims
{
    public class DatosTrim {
        public List<Trim_item> lista_familias_trims(){
            List<Trim_item> lista = new List<Trim_item>();
            Link con_led = new Link();
            try{
                SqlCommand com_led = new SqlCommand();
                SqlDataReader leer_led = null;
                com_led.Connection = con_led.AbrirConexion();
                com_led.CommandText = " SELECT  distinct fabric_type from items_catalogue where tipo=2 ";
                leer_led = com_led.ExecuteReader();
                while (leer_led.Read()){
                    Trim_item ti = new Trim_item();
                    ti.family = Convert.ToString(leer_led["fabric_type"]);
                    lista.Add(ti);
                }leer_led.Close();
            }finally { con_led.CerrarConexion(); con_led.Dispose(); }
            return lista;
        }
        public List<Pedido_t> obtener_lista_ordenes_estados(string busqueda){
            string query = "";
            if (busqueda == "0"){
                query = "SELECT TOP 10 ID_PEDIDO,PO FROM PEDIDO WHERE ID_STATUS!=5 AND ID_STATUS!=6 AND ID_STATUS!=5  ORDER BY ID_PEDIDO DESC";
            }else{
                query = "SELECT TOP 10 ID_PEDIDO,PO FROM PEDIDO WHERE  PO LIKE'%" + busqueda + "%'";
            }
            List<Pedido_t> lista = new List<Pedido_t>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                //com.CommandText = "SELECT TOP 25 ID_PEDIDO,PO FROM PEDIDO WHERE ID_STATUS!=6 AND ID_STATUS!=7";
                com.CommandText = query;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Pedido_t p = new Pedido_t();
                    p.id_pedido = Convert.ToInt32(leer["ID_PEDIDO"]);
                    p.pedido = (Convert.ToString(leer["PO"])).Trim();
                    //p.lista_estilos= obtener_trims_anteriores_pedido(Convert.ToString(leer["ID_PEDIDO"]));
                    //p.lista_requests = obtener_trims_anteriores_orden((p.id_pedido).ToString());
                    p.total = obtener_total_trim_request(p.id_pedido);
                    p.cantidad = obtener_total_trim_recibido(p.id_pedido);
                    p.estado=Convert.ToInt32(buscar_estado_trim_pedido(p.id_pedido));
                    lista.Add(p);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public int obtener_total_trim_request(int id){
            int lista = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = " SELECT total FROM trim_requests WHERE id_pedido='" + id + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    lista += Convert.ToInt32(leer["total"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public int obtener_total_trim_recibido(int id){
            int lista = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = " SELECT total FROM stock_ordenes WHERE id_pedido='" + id + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    lista += Convert.ToInt32(leer["total"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }

        public List<Item_t> lista_trim_items(){
            List<Item_t> lista = new List<Item_t>();
            Link con_led = new Link();
            try{
                SqlCommand com_led = new SqlCommand();
                SqlDataReader leer_led = null;
                com_led.Connection = con_led.AbrirConexion();
                com_led.CommandText = " SELECT item_id,descripcion FROM items_catalogue where tipo=2 ";
                leer_led = com_led.ExecuteReader();
                while (leer_led.Read()){
                    Item_t i = new Item_t();
                    i.id_item = Convert.ToInt32(leer_led["item_id"]);
                    i.descripcion = Convert.ToString(leer_led["descripcion"]);
                    lista.Add(i);
                }leer_led.Close();
            }finally { con_led.CerrarConexion(); con_led.Dispose(); }
            return lista;
        }
        public List<Inventario> obtener_lista_trims_inicio(string busqueda, string fecha){
            List<Inventario> lista = new List<Inventario>();
            if (busqueda == "0" && fecha == "0"){
                lista.AddRange(lista_trims_inicio_default());
            }else{
                if (busqueda == "0" && fecha != "0"){
                    lista.AddRange(lista_trims_inicio_fecha(busqueda, fecha));
                }else{
                    lista.AddRange(lista_trims_inicio_trim(busqueda, fecha));
                    lista.AddRange(lista_trims_inicio_mp(busqueda, fecha));
                    lista.AddRange(lista_trims_inicio_po(busqueda, fecha));
                }
            }
            return lista;
        }
        public List<Inventario> lista_trims_inicio_default(){
            List<Inventario> lista = new List<Inventario>();
            Link con = new Link();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT DISTINCT r.fecha,i.id_inventario,i.id_pedido,i.id_estilo,i.total,i.id_item,i.auditoria,ic.descripcion,ic.fabric_type " +
                    "  FROM recibos r,inventario i,recibos_items ri,items_catalogue ic" +
                    " WHERE i.id_inventario=ri.id_inventario AND ic.item_id=i.id_item AND ri.id_recibo=r.id_recibo " +
                    " AND r.fecha between '" + DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00' AND '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' " +//AND r.fecha<'2019-03-01 12:00:00'
                    "AND ic.tipo=2 ";
                leer = com.ExecuteReader();
                while (leer.Read())
                {
                    Inventario i = new Inventario();
                    i.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    i.po = consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    i.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"])) + " " + consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    i.total = Convert.ToInt32(leer["total"]);
                    i.auditoria = Convert.ToInt32(leer["auditoria"]);
                    i.descripcion = Convert.ToString(leer["descripcion"]) + " " + Convert.ToString(leer["fabric_type"]);
                    i.fecha = Convert.ToDateTime(leer["fecha"]).ToString("dd-MM-yyyy");
                    lista.Add(i);
                }
                leer.Close();
            }
            finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Inventario> lista_trims_inicio_trim(string busqueda, string fecha)
        {
            string query = "SELECT DISTINCT r.fecha,i.id_inventario,i.id_pedido,i.id_estilo,i.total,i.id_item,i.auditoria,ic.descripcion,ic.fabric_type " +
                    "  FROM recibos r,inventario i,recibos_items ri,items_catalogue ic" +
                    " WHERE i.id_inventario=ri.id_inventario AND ic.item_id=i.id_item AND ri.id_recibo=r.id_recibo AND ic.tipo=2 ";
            if (busqueda != "0") { query += " AND i.descripcion like '%" + busqueda + "%'  "; }
            if (fecha != "0") { query += " AND r.fecha between '" + fecha + " 00:00:00' and  '" + fecha + " 23:59:59'  "; }
            List<Inventario> lista = new List<Inventario>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con = new Link();
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = query;
                leer = com.ExecuteReader();
                while (leer.Read())
                {
                    Inventario i = new Inventario();
                    i.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    i.po = consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    i.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"])) + " " + consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    i.total = Convert.ToInt32(leer["total"]);
                    i.auditoria = Convert.ToInt32(leer["auditoria"]);
                    i.descripcion = Convert.ToString(leer["descripcion"]) + " " + Convert.ToString(leer["fabric_type"]);
                    i.fecha = Convert.ToDateTime(leer["fecha"]).ToString("dd-MM-yyyy");
                    lista.Add(i);
                }
                leer.Close();
            }
            finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Inventario> lista_trims_inicio_mp(string busqueda, string fecha)
        {
            string query = "SELECT DISTINCT r.fecha,i.id_inventario,i.id_pedido,i.id_estilo,i.total,i.id_item,i.auditoria,ic.descripcion,ic.fabric_type " +
                    "  FROM recibos r,inventario i,recibos_items ri,items_catalogue ic" +
                    " WHERE i.id_inventario=ri.id_inventario AND ic.item_id=i.id_item AND ri.id_recibo=r.id_recibo AND ic.tipo=2  ";
            if (busqueda != "0") { query += " AND r.mp_number like '%" + busqueda + "%'  "; }
            if (fecha != "0") { query += " AND r.fecha between '" + fecha + " 00:00:00' and  '" + fecha + " 23:59:59'  "; }
            List<Inventario> lista = new List<Inventario>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con = new Link();
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = query;
                leer = com.ExecuteReader();
                while (leer.Read())
                {
                    Inventario i = new Inventario();
                    i.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    i.po = consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    i.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"])) + " " + consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    i.total = Convert.ToInt32(leer["total"]);
                    i.auditoria = Convert.ToInt32(leer["auditoria"]);
                    i.descripcion = Convert.ToString(leer["descripcion"]) + " " + Convert.ToString(leer["fabric_type"]);
                    i.fecha = Convert.ToDateTime(leer["fecha"]).ToString("dd-MM-yyyy");
                    lista.Add(i);
                }
                leer.Close();
            }
            finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Inventario> lista_trims_inicio_po(string busqueda, string fecha)
        {
            string query = "SELECT DISTINCT r.fecha,i.id_inventario,i.id_pedido,i.id_estilo,i.total,i.id_item,i.auditoria,ic.descripcion,ic.fabric_type " +
                    "  FROM recibos r,inventario i,recibos_items ri,items_catalogue ic,PEDIDO p" +
                    " WHERE i.id_inventario=ri.id_inventario AND ic.item_id=i.id_item AND ri.id_recibo=r.id_recibo AND i.id_pedido=p.ID_PEDIDO AND ic.tipo=2 ";
            if (busqueda != "0") { query += "  AND p.PO like '%" + busqueda + "%' "; }
            if (fecha != "0") { query += " AND r.fecha between '" + fecha + " 00:00:00' and  '" + fecha + " 23:59:59'  "; }
            List<Inventario> lista = new List<Inventario>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con = new Link();
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = query;
                leer = com.ExecuteReader();
                while (leer.Read())
                {
                    Inventario i = new Inventario();
                    i.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    i.po = consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    i.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"])) + " " + consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    i.total = Convert.ToInt32(leer["total"]);
                    i.auditoria = Convert.ToInt32(leer["auditoria"]);
                    i.descripcion = Convert.ToString(leer["descripcion"]) + " " + Convert.ToString(leer["fabric_type"]);
                    i.fecha = Convert.ToDateTime(leer["fecha"]).ToString("dd-MM-yyyy");
                    lista.Add(i);
                }
                leer.Close();
            }
            finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Inventario> lista_trims_inicio_fecha(string busqueda, string fecha)
        {
            List<Inventario> lista = new List<Inventario>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con = new Link();
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT DISTINCT r.fecha,i.id_inventario,i.id_pedido,i.id_estilo,i.total,i.id_item,i.auditoria,ic.descripcion,ic.fabric_type " +
                    "  FROM recibos r,inventario i,recibos_items ri,items_catalogue ic " +
                    " WHERE i.id_inventario=ri.id_inventario AND ic.item_id=i.id_item AND ri.id_recibo=r.id_recibo " +
                    " AND r.fecha between '" + fecha + " 00:00:00' and  '" + fecha + " 23:59:59'  " +
                    "AND ic.tipo=2 ";
                leer = com.ExecuteReader();
                while (leer.Read())
                {
                    Inventario i = new Inventario();
                    i.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    i.po = consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    i.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"])) + " " + consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    i.total = Convert.ToInt32(leer["total"]);
                    i.auditoria = Convert.ToInt32(leer["auditoria"]);
                    i.descripcion = Convert.ToString(leer["descripcion"]) + " " + Convert.ToString(leer["fabric_type"]);
                    i.fecha = Convert.ToDateTime(leer["fecha"]).ToString("dd-MM-yyyy");
                    lista.Add(i);
                }
                leer.Close();
            }
            finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<recibo> buscar_mp_recibos_hoy()
        {
            List<recibo> lista = new List<recibo>();
            Link con = new Link();
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT distinct r.mp_number from recibos r,recibos_items ri,inventario i,items_catalogue ic where" +
                    " r.id_recibo=ri.id_recibo AND ri.id_inventario=i.id_inventario AND i.id_item=ic.item_id " +
                    " AND r.fecha between '" + DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00' AND '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' " +//AND r.fecha<'2019-03-01 12:00:00'
                    "AND ic.tipo=2 ";
                leer = com.ExecuteReader();
                while (leer.Read())
                {
                    recibo i = new recibo();
                    i.mp_number = Convert.ToString(leer["mp_number"]);
                    lista.Add(i);
                }
                leer.Close();
            }
            finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Trim_requests> obtener_trims_anteriores_orden(string pedido){
            List<Trim_requests> lista = new List<Trim_requests>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT entregado,id_request,id_item,total,usuario,fecha,cantidad,blanks from trim_requests where id_pedido=" + pedido;
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Trim_requests tr = new Trim_requests();
                    tr.id_request = Convert.ToInt32(leer_ltd["id_request"]);
                    tr.id_item = Convert.ToInt32(leer_ltd["id_item"]);
                    tr.item = consultas.buscar_descripcion_item(Convert.ToString(tr.id_item));
                    tr.tipo_item = consultas.buscar_tipo_trim_item(Convert.ToString(tr.id_item));
                    tr.total = Convert.ToInt32(leer_ltd["total"]);
                    tr.cantidad = Convert.ToInt32(leer_ltd["cantidad"]);
                    tr.blanks = Convert.ToInt32(leer_ltd["blanks"]);                    
                    tr.fecha = Convert.ToDateTime(leer_ltd["fecha"]).ToString("dd-MM-yyyy");
                    tr.id_usuario = Convert.ToInt32(leer_ltd["usuario"]);
                    tr.usuario = consultas.buscar_nombre_usuario(Convert.ToString(tr.id_usuario));
                    tr.lista_estilos = obtener_trims_estilos(tr.id_request);
                    tr.entregado = Convert.ToInt32(leer_ltd["entregado"]);                   
                    lista.Add(tr);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public List<Trim_estilo> obtener_trims_estilos(int request){
            List<Trim_estilo> lista = new List<Trim_estilo>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT id,id_summary,id_request FROM trim_estilos WHERE id_request='" + request + "'  ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Trim_estilo tr = new Trim_estilo();
                    tr.id = Convert.ToInt32(leer_ltd["id"]);
                    tr.id_summary = Convert.ToInt32(leer_ltd["id_summary"]);
                    tr.id_estilo = consultas.obtener_estilo_summary(tr.id_summary);
                    tr.estilo = consultas.obtener_estilo((tr.id_estilo));
                    tr.descripcion = consultas.buscar_descripcion_estilo(tr.id_estilo);
                    tr.lista_tallas = obtener_tallas_request(Convert.ToInt32(leer_ltd["id"]));
                    lista.Add(tr);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }        
        public List<Talla_t> obtener_tallas_request(int id){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<Talla_t> lista = new List<Talla_t>();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT tet.id,tet.id_trim_estilos,tet.id_talla,tet.impreso FROM trim_estilos_tallas tet,CAT_ITEM_SIZE CS  " +
                    "WHERE id_trim_estilos='" + id + "' and CS.ID=tet.id_talla ORDER BY CAST(CS.ORDEN AS int) ASC  ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Talla_t tr = new Talla_t();
                    tr.id = Convert.ToInt32(leer_ltd["id"]);
                    tr.id_talla = Convert.ToInt32(leer_ltd["id_talla"]);
                    tr.talla = consultas.obtener_size_id(Convert.ToString(tr.id_talla));
                    tr.impreso = Convert.ToInt32(leer_ltd["impreso"]);
                    lista.Add(tr);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public List<Trim_item> lista_descripciones_trims(){
            List<Trim_item> lista = new List<Trim_item>();
            Link con_led = new Link();
            try{
                SqlCommand com_led = new SqlCommand();
                SqlDataReader leer_led = null;
                com_led.Connection = con_led.AbrirConexion();
                com_led.CommandText = " SELECT item_id,item,descripcion,fabric_type from items_catalogue where tipo=2 ";
                leer_led = com_led.ExecuteReader();
                while (leer_led.Read()){
                    Trim_item ti = new Trim_item();
                    ti.id_item = Convert.ToInt32(leer_led["item_id"]);
                    ti.descripcion = Convert.ToString(leer_led["descripcion"]);
                    ti.item = Convert.ToString(leer_led["item"]);
                    ti.family = Convert.ToString(leer_led["fabric_type"]);
                    lista.Add(ti);
                }leer_led.Close();
            }finally { con_led.CerrarConexion(); con_led.Dispose(); }
            return lista;
        }
        public List<Trim_item> informacion_editar_item_trim(string id){
            List<Trim_item> lista = new List<Trim_item>();
            Link con_led = new Link();
            try{
                SqlCommand com_led = new SqlCommand();
                SqlDataReader leer_led = null;
                com_led.Connection = con_led.AbrirConexion();
                com_led.CommandText = " SELECT item_id,item,body_type,descripcion,fabric_type,unit,minimo from items_catalogue where item_id='" + id + "' ";
                leer_led = com_led.ExecuteReader();
                while (leer_led.Read()){
                    Trim_item ti = new Trim_item();
                    ti.id_item = Convert.ToInt32(leer_led["item_id"]);
                    ti.item = Convert.ToString(leer_led["item"]);
                    ti.descripcion = Convert.ToString(leer_led["descripcion"]);
                    ti.family = Convert.ToString(leer_led["fabric_type"]);
                    ti.unit = Convert.ToString(leer_led["unit"]);
                    ti.minimo = Convert.ToInt32(leer_led["minimo"]);
                    lista.Add(ti);
                }leer_led.Close();
            }finally { con_led.CerrarConexion(); con_led.Dispose(); }
            return lista;
        }
        public void eliminar_item_catalogo(int id){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE FROM items_catalogue WHERE item_id='" + id + "'  ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void editar_informacion_trim(string id, string item, string minimo, string descripcion, string family, string unidad){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE items_catalogue SET item='" + item + "', descripcion='" + descripcion + "',body_type='" + unidad + "', " +
                    " unit='" + unidad + "', fabric_type='" + family + "', minimo='" + minimo + "' " +
                    " WHERE item_id='" + id + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public List<Pedido_t> lista_ordenes(){
            List<Pedido_t> listar = new List<Pedido_t>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT ID_PEDIDO,PO from PEDIDO where ID_STATUS!=7 AND ID_STATUS!=6 AND ID_STATUS!=5 ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    Pedido_t l = new Pedido_t();
                    l.id_pedido = Convert.ToInt32(leerFilas["ID_PEDIDO"]);
                    l.pedido = leerFilas["PO"].ToString();
                    listar.Add(l);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return listar;
        }
        public List<Item_t> busqueda_items_trims(string busqueda){
            string query = "";
            if (busqueda != "0"){
                query = "SELECT top 30  item_id,item,descripcion,fabric_type FROM items_catalogue WHERE tipo=2 and " +
                    " (descripcion like '%" + busqueda + "%' or item  like '%" + busqueda + "%' ) ";
            }else{
                query = "SELECT top 30  item_id,item,descripcion,fabric_type FROM items_catalogue WHERE tipo=2  ";
            }
            List<Item_t> lista = new List<Item_t>();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = query;
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Item_t i = new Item_t();
                    i.id_item = Convert.ToInt32(leer_ltd["item_id"]);
                    i.componente = Convert.ToString(leer_ltd["item"]);
                    i.descripcion = Convert.ToString(leer_ltd["descripcion"]);
                    i.familia = Convert.ToString(leer_ltd["fabric_type"]);
                    lista.Add(i);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public string obtener_family_trim_item(string item){
            string trim = "";
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT fabric_type from items_catalogue where item_id='" + item + "'";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    trim = Convert.ToString(leer_ltd["fabric_type"]);
                }
                leer_ltd.Close();
            }
            finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return trim;
        }
        public string obtener_descripcion_item(int item){
            string lista = "";
            Link con_led = new Link();
            try{
                SqlCommand com_led = new SqlCommand();
                SqlDataReader leer_led = null;
                com_led.Connection = con_led.AbrirConexion();
                com_led.CommandText = " SELECT descripcion FROM items_catalogue where item_id='" + item + "' ";
                leer_led = com_led.ExecuteReader();
                while (leer_led.Read()){
                    lista = Convert.ToString(leer_led["descripcion"]);
                }leer_led.Close();
            }finally { con_led.CerrarConexion(); con_led.Dispose(); }
            return lista;
        }
        public void update_tipo_recibo(int id){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE recibos SET tipo=2 WHERE id_recibo='" + id + "'  ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void guardar_recibo_item(int id_recibo, string id_inventario, string cantidad, string summary,string ubicacion){
            Link con_r_i = new Link();
            try{
                SqlCommand com_r_i = new SqlCommand();
                com_r_i.Connection = con_r_i.AbrirConexion();
                com_r_i.CommandText = "INSERT INTO recibos_items(id_recibo,id_inventario,total,id_summary,id_ubicacion) " +
                    " values ('" + id_recibo + "','" + id_inventario + "','" + cantidad + "','" + summary + "','" + ubicacion + "') ";
                com_r_i.ExecuteNonQuery();
            }finally { con_r_i.CerrarConexion(); con_r_i.Dispose(); }
        }
        public List<recibo> obtener_lista_recibos_trim(string busqueda){
            //data[0]                            data[1]                       data[2]
            //($("#caja_inicio").val() + "*" + $("#caja_final").val()+ "*" + $("#caja_busqueda_recibo_trim").val());
            List<recibo> lista = new List<recibo>();
            string query = "";
            string[] datos = busqueda.Split('*');
            if (busqueda != ""){
                if (datos[0] != "" && datos[1] != "" && datos[2] != ""){
                    query = "SELECT top 50 r.id_origen,r.comentarios,r.id_recibo,r.fecha,r.total,r.mp_number,r.mill_po,r.po_reference,r.id_usuario,r.id_sucursal,r.packing_number  " +
                     "  from recibos r WHERE r.fecha between '" + datos[0] + " 00:00:00' and '" + datos[1] + " 23:59:59' " +
                     " AND r.mill_po like'%" + datos[2] + "%' AND r.tipo=2 order by r.id_recibo desc ";
                    lista.AddRange(obtener_lista_recibos_trim_query(query));
                    query = "SELECT top 50 r.id_origen,r.comentarios,r.id_recibo,r.fecha,r.total,r.mp_number,r.mill_po,r.po_reference,r.id_usuario,r.id_sucursal,r.packing_number " +
                     " from recibos r WHERE  r.fecha between '" + datos[0] + " 00:00:00' and '" + datos[1] + " 23:59:59' " +
                     " AND r.mp_number like'%" + datos[2] + "%' AND r.tipo=2  order by r.id_recibo desc ";
                    lista.AddRange(obtener_lista_recibos_trim_query(query));
                    query = " SELECT distinct r.id_origen,r.comentarios,r.id_recibo,r.fecha,r.total,r.mp_number,r.mill_po,r.po_reference,r.id_usuario,r.id_sucursal,r.packing_number " +
                          " from recibos r,recibos_items ri, items_catalogue i, pedido p " +
                          " WHERE r.id_recibo = ri.id_recibo and ri.id_inventario = i.item_id and ri.id_summary = p.id_pedido and " +
                           "  r.fecha between '" + datos[0] + " 00:00:00' and '" + datos[1] + " 23:59:59' " +
                          "and p.po like'%" + datos[2] + "%' and i.tipo = 2 AND r.tipo=2  order by r.id_recibo desc";
                    lista.AddRange(obtener_lista_recibos_trim_query(query));
                    query = " SELECT distinct r.id_origen,r.comentarios,r.id_recibo,r.fecha,r.total,r.mp_number,r.mill_po,r.po_reference,r.id_usuario,r.id_sucursal,r.packing_number " +
                        " from recibos r,recibos_items ri, items_catalogue i " +
                        " WHERE r.id_recibo = ri.id_recibo and ri.id_inventario = i.item_id and  " +
                         "  r.fecha between '" + datos[0] + " 00:00:00' and '" + datos[1] + " 23:59:59' " +
                        "and  i.descripcion like'%" + datos[2] + "%' and i.tipo = 2 AND r.tipo=2  order by r.id_recibo desc";
                    lista.AddRange(obtener_lista_recibos_trim_query(query));

                    query = " SELECT distinct r.id_origen,r.comentarios,r.id_recibo,r.fecha,r.total,r.mp_number,r.mill_po,r.po_reference,r.id_usuario,r.id_sucursal,r.packing_number " +
                        " from recibos r,recibos_items ri, inventario i " +
                        " WHERE r.id_recibo = ri.id_recibo and ri.id_inventario = i.id_inventario AND  " +
                         "  r.fecha between '" + datos[0] + " 00:00:00' and '" + datos[1] + " 23:59:59' " +
                        "and  i.descripcion like'%" + datos[2] + "%'  AND r.tipo=2  order by r.id_recibo desc";
                    lista.AddRange(obtener_lista_recibos_trim_query(query));
                }else{
                    if (datos[2] != ""){
                        query = "SELECT distinct r.id_origen,r.comentarios,r.id_recibo,r.fecha,r.total,r.mp_number,r.mill_po,r.po_reference,r.id_usuario,r.id_sucursal,r.packing_number " +
                         "  from recibos r  WHERE  r.mill_po like'%" + datos[2] + "%' AND r.tipo=2  order by r.id_recibo desc ";
                        lista.AddRange(obtener_lista_recibos_trim_query(query));
                        query = "SELECT distinct r.id_origen,r.comentarios,r.id_recibo,r.fecha,r.total,r.mp_number,r.mill_po,r.po_reference,r.id_usuario,r.id_sucursal,r.packing_number " +
                         "  from recibos r  WHERE  r.mp_number like'%" + datos[2] + "%' AND r.tipo=2  order by r.id_recibo desc ";
                        lista.AddRange(obtener_lista_recibos_trim_query(query));
                        query = " SELECT distinct r.id_origen,r.comentarios,r.id_recibo,r.fecha,r.total,r.mp_number,r.mill_po,r.po_reference,r.id_usuario,r.id_sucursal,r.packing_number " +
                            " from recibos r,recibos_items ri, items_catalogue i, pedido p " +
                            " WHERE r.id_recibo = ri.id_recibo and ri.id_inventario = i.item_id and ri.id_summary = p.id_pedido and " +
                            " p.po like'%" + datos[2] + "%' and i.tipo = 2 AND r.tipo=2  order by r.id_recibo desc";
                        lista.AddRange(obtener_lista_recibos_trim_query(query));
                        query = " SELECT distinct r.id_origen,r.comentarios,r.id_recibo,r.fecha,r.total,r.mp_number,r.mill_po,r.po_reference,r.id_usuario,r.id_sucursal,r.packing_number " +
                            " from recibos r,recibos_items ri, items_catalogue i " +
                            " WHERE r.id_recibo = ri.id_recibo and ri.id_inventario = i.item_id and  " +
                            " i.descripcion like'%" + datos[2] + "%' and i.tipo = 2 AND r.tipo=2  order by r.id_recibo desc";
                        lista.AddRange(obtener_lista_recibos_trim_query(query));
                        query = " SELECT distinct r.id_origen,r.comentarios,r.id_recibo,r.fecha,r.total,r.mp_number,r.mill_po,r.po_reference,r.id_usuario,r.id_sucursal,r.packing_number " +
                            " from recibos r,recibos_items ri, inventario i " +
                            " WHERE r.id_recibo = ri.id_recibo and ri.id_inventario = i.id_inventario and  " +
                            " i.descripcion like'%" + datos[2] + "%' AND r.tipo=2 order by r.id_recibo desc";
                        lista.AddRange(obtener_lista_recibos_trim_query(query));
                    }else{
                        if (datos[0] != "" && datos[1] != ""){
                            query = "SELECT top 50 r.id_origen,r.comentarios,r.id_recibo,r.fecha,r.total,r.mp_number,r.mill_po,r.po_reference,r.id_usuario,r.id_sucursal,r.packing_number " +
                                "  from recibos r  WHERE  r.fecha between '" + datos[0] + " 00:00:00' and '" + datos[1] + " 23:59:59' AND r.tipo=2 order by r.id_recibo desc ";
                            lista.AddRange(obtener_lista_recibos_trim_query(query));
                        }else{
                            query = "SELECT top 30 r.id_origen,r.comentarios,r.id_recibo,r.fecha,r.total,r.mp_number,r.mill_po,r.po_reference,r.id_usuario,r.id_sucursal,r.packing_number " +
                                " from recibos r WHERE  r.tipo=2  order by r.id_recibo desc ";
                            lista.AddRange(obtener_lista_recibos_trim_query(query));
                        }
                    }
                }
            }else{
                query = "SELECT top 30 r.id_origen,r.comentarios,r.id_recibo,r.fecha,r.total,r.mp_number,r.mill_po,r.po_reference,r.id_usuario,r.id_sucursal,r.packing_number " +
                        " from recibos r WHERE r.tipo=2  order by r.id_recibo desc ";
                lista.AddRange(obtener_lista_recibos_trim_query(query));
            }
            return lista;
        }
        public List<recibo> obtener_lista_recibos_trim_query(string busqueda){
            List<recibo> lista = new List<recibo>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con_ltf = new Link();
            try{
                SqlCommand com_ltf = new SqlCommand();
                SqlDataReader leer_ltf = null;
                com_ltf.Connection = con_ltf.AbrirConexion();
                com_ltf.CommandText = busqueda;
                leer_ltf = com_ltf.ExecuteReader();
                while (leer_ltf.Read()){
                    recibo l = new recibo();
                    l.id_recibo = Convert.ToInt32(leer_ltf["id_recibo"]);
                    l.fecha = (Convert.ToDateTime(leer_ltf["fecha"])).ToString("yyyy-MM-dd");
                    l.usuario = consultas.buscar_nombre_usuario(leer_ltf["id_usuario"].ToString());
                    l.total = Convert.ToInt32(leer_ltf["total"]);
                    l.id_sucursal = Convert.ToInt32(leer_ltf["id_sucursal"]);
                    l.sucursal = consultas.obtener_sucursal_id(leer_ltf["id_sucursal"].ToString());
                    l.lista_recibos_item = obtener_lista_items_trims(l.id_recibo);
                    l.mp_number = leer_ltf["mp_number"].ToString();
                    l.mill_po = leer_ltf["mill_po"].ToString();
                    l.po_referencia = leer_ltf["po_reference"].ToString();
                    l.packing_number = Convert.ToString(leer_ltf["packing_number"]);
                    foreach (recibos_item ri in l.lista_recibos_item){
                        l.id_pedido = ri.id_pedido;
                    }
                    if (l.id_pedido == 0){
                        l.id_customer = buscar_cliente_recibo_trim(l.id_recibo);
                    }else{
                        l.id_customer = consultas.obtener_customer_po(l.id_pedido);
                    }
                    l.comentarios = Convert.ToString(leer_ltf["comentarios"]);
                    l.customer = consultas.obtener_customer_id(Convert.ToString(leer_ltf["id_origen"]));
                    lista.Add(l);
                }leer_ltf.Close();
            }finally { con_ltf.CerrarConexion(); con_ltf.Dispose(); }
            return lista;
        }        
        public List<recibos_item> obtener_lista_items_trims(int id_recibo){
            DatosInventario di = new DatosInventario();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<recibos_item> lista = new List<recibos_item>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "Select  ri.id_inventario,ri.total,ri.id_recibo_item,ri.id_summary,ri.id_ubicacion from recibos_items ri where ri.id_recibo='" + id_recibo + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    recibos_item ri = new recibos_item();
                    ri.id_recibo_item = Convert.ToInt32(leer["id_recibo_item"]);
                    ri.id_inventario = Convert.ToInt32(leer["id_inventario"]);                   
                    ri.id_summary = Convert.ToInt32(leer["id_summary"]);
                    ri.id_pedido = ri.id_summary;
                    ri.total = Convert.ToInt32(leer["total"]);
                    if (ri.id_summary==0) {
                        ri.item_description = buscar_descripcion_inventario(ri.id_inventario);
                        ri.id_item = buscar_item_inventario(ri.id_inventario);
                    } else {
                        ri.item_description = obtener_descripcion_item(ri.id_inventario);
                        ri.id_item = ri.id_inventario;
                    }                                    
                    ri.pedido = consultas.obtener_po_id(Convert.ToString(leer["id_summary"]));
                    ri.id_ubicacion = Convert.ToInt32(leer["id_ubicacion"]);
                    ri.id_pedido = ri.id_summary;
                    lista.Add(ri);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public string buscar_descripcion_inventario(int inventario){
            string temp = "";
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT descripcion FROM inventario WHERE id_inventario='" + inventario + "' ";
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    temp = Convert.ToString(leer_u_i["descripcion"]);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            return temp;
        }
        public int buscar_item_inventario(int inventario){
            int temp = 0;
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT id_item FROM inventario WHERE id_inventario='" + inventario + "' ";
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    temp = Convert.ToInt32(leer_u_i["id_item"]);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            return temp;
        }
        public int buscar_cliente_recibo_trim(int recibo){
            int trim = 0;
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "select distinct i.id_customer from inventario i, recibos_items ri where ri.id_inventario=i.id_inventario and ri.id_recibo='" + recibo + "'   ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    trim = Convert.ToInt32(leer_ltd["id_customer"]);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return trim;
        }
        public void eliminar_items_recibo(int recibo){
            DatosTransferencias dt = new DatosTransferencias();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_inventario,total,id_summary FROM recibos_items WHERE id_recibo='" + recibo + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    int inventario = Convert.ToInt32(leer["id_inventario"]);
                    int total = Convert.ToInt32(leer["total"]);
                    if (Convert.ToInt32(leer["id_summary"]) == 0){
                        actualizar_inventario(inventario, total.ToString());
                    }else {
                        restar_stock_orden( Convert.ToString(leer["id_summary"]), Convert.ToString(leer["id_inventario"]), Convert.ToString(leer["total"]));
                    }
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            delete_items_recibo(recibo);
        }
        public void actualizar_inventario(int inventario, string cantidad){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE inventario SET total=total-" + cantidad + " WHERE id_inventario=" + inventario + " AND total!=0  ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void sumar_inventario(int inventario, string cantidad){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE inventario SET total=(total+" + cantidad + ") WHERE id_inventario=" + inventario;
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void delete_items_recibo(int recibo){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE FROM recibos_items WHERE id_recibo='" + recibo + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void actualizar_recibo_trim(string recibo, int total, string mp, string millpo, string po_number, string packing_number, string sucursal, int usuario, string fecha, string comentarios, string cliente)
        {
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE recibos SET total='" + total + "',mp_number='" + mp + "',mill_po='" + millpo + "',po_reference='" + po_number + "'," +
                    " packing_number='" + packing_number + "',id_sucursal='" + sucursal + "',id_usuario='" + usuario + "',fecha_modificacion='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                    " fecha='" + fecha + DateTime.Now.ToString(" HH:mm:ss") + "', comentarios='" + comentarios + "',id_origen='" + cliente + "' " +
                    " WHERE id_recibo='" + recibo + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void delete_recibo(int recibo){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE FROM recibos WHERE id_recibo='" + recibo + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public List<Estilos_t> lista_estilos_dropdown(string po){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<Estilos_t> lista = new List<Estilos_t>();
            Link con_led = new Link();
            try{
                SqlCommand com_led = new SqlCommand();
                SqlDataReader leer_led = null;
                com_led.Connection = con_led.AbrirConexion();
                com_led.CommandText = " SELECT ID_PO_SUMMARY,ITEM_ID FROM PO_SUMMARY WHERE ID_PEDIDOS='" + po + "'  ";
                leer_led = com_led.ExecuteReader();
                while (leer_led.Read()){
                    Estilos_t i = new Estilos_t();
                    i.id_po_summary = Convert.ToInt32(leer_led["ID_PO_SUMMARY"]);
                    i.id_estilo = Convert.ToInt32(leer_led["ITEM_ID"]);
                    i.estilo = (consultas.obtener_estilo(i.id_estilo)).Trim();
                    i.descripcion = (consultas.buscar_descripcion_estilo(i.id_estilo)).Trim();
                    lista.Add(i);
                }leer_led.Close();
            }finally { con_led.CerrarConexion(); con_led.Dispose(); }
            return lista;
        }
        public int buscar_estado_instruccion_empaque(int pedido){
            int temp = 0;
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT estado FROM instrucciones_empaque WHERE id_pedido='" + pedido + "' ";
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    temp = Convert.ToInt32(leer_u_i["estado"]);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            return temp;
        }
        public string buscar_fold_size_pedido(int pedido){
            string temp = "";
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT fold_size FROM trims_fold_sizes WHERE id_pedido='" + pedido + "' ";
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    temp = Convert.ToString(leer_u_i["fold_size"]);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            return temp;
        }
        public string buscar_hang_size_pedido(int pedido){
            string temp = "";
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT hang_size FROM trims_fold_sizes WHERE id_pedido='" + pedido + "' ";
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    temp = Convert.ToString(leer_u_i["hang_size"]);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            return temp;
        }
        public string buscar_estado_trim_pedido(int pedido){
            string temp = "";
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT estado FROM trims_fold_sizes WHERE id_pedido='" + pedido + "' ";
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    temp = Convert.ToString(leer_u_i["estado"]);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            if (temp == "") { return "0"; } else { return temp; }            
        }
        public List<Talla_t> lista_tallas_pedido(string pedido){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<Talla_t> lista = new List<Talla_t>();
            Link con_led = new Link();
            try{
                SqlCommand com_led = new SqlCommand();
                SqlDataReader leer_led = null;
                com_led.Connection = con_led.AbrirConexion();
                com_led.CommandText = " select DISTINCT T.TALLA_ITEM from ITEM_SIZE T,PO_SUMMARY PS " +
                    " WHERE PS.ID_PO_SUMMARY=T.ID_SUMMARY AND PS.ID_PEDIDOS='" + pedido + "' AND T.TALLA_ITEM IS NOT NULL";
                leer_led = com_led.ExecuteReader();
                while (leer_led.Read()){
                    Talla_t i = new Talla_t();
                    i.id_talla = Convert.ToInt32(leer_led["TALLA_ITEM"]);
                    i.talla = consultas.obtener_size_id(Convert.ToString(leer_led["TALLA_ITEM"]));
                    lista.Add(i);
                }leer_led.Close();
            }finally { con_led.CerrarConexion(); con_led.Dispose(); }
            return (lista).Distinct().ToList();
        }
        public int obtener_total_estilo(string summary, string talla){
            string query = "";
            string[] Estilos = summary.Split('*');
            string[] Tallas = talla.Split('*');
            int total = 0;
            for (int ii = 1; ii < Estilos.Length; ii++){
                for (int i = 1; i < Tallas.Length; i++){
                    query = "SELECT CANTIDAD,\"1RST_CALIDAD\",EXTRAS,EJEMPLOS FROM ITEM_SIZE WHERE ID_SUMMARY='" + Estilos[ii] + "' and " +
                        "TALLA_ITEM='" + Tallas[i] + "'  AND TALLA_ITEM IS NOT NULL";
                    Link con_ltd = new Link();
                    try{
                        SqlCommand com_ltd = new SqlCommand();
                        SqlDataReader leer_ltd = null;
                        com_ltd.Connection = con_ltd.AbrirConexion();
                        com_ltd.CommandText = query;
                        leer_ltd = com_ltd.ExecuteReader();
                        while (leer_ltd.Read()){
                            total += Convert.ToInt32(leer_ltd["CANTIDAD"]);// + Convert.ToInt32(leer_ltd["EXTRAS"]) + Convert.ToInt32(leer_ltd["EJEMPLOS"]);
                        }leer_ltd.Close();
                    }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
                }
            }
            return total;
        }
        public int obtener_cajas_estilo(string summary, string pedido){
            DatosShipping ds = new DatosShipping();
            int cajas = 0;
            string[] Estilos = summary.Split('*');
            /************************************************************************************************/
            List<Estilos_t> estilos_orden = new List<Estilos_t>();            
            for (int i = 1; i < Estilos.Length; i++){
                Estilos_t e = new Estilos_t();
                e.id_po_summary = Convert.ToInt32(Estilos[i]);
                estilos_orden.Add(e);
            }
            foreach (Estilos_t e in estilos_orden){
                int tipo_empaque = buscar_tipo_empaque(e.id_po_summary);
                if (tipo_empaque == 1){
                    List<ratio_tallas> lista_ratio = ds.obtener_lista_ratio(e.id_po_summary, e.id_estilo, 1);
                    foreach (ratio_tallas r in lista_ratio){
                        int total = buscar_total_talla_primera_calidad(e.id_po_summary, r.id_talla);
                        if (r.piezas == 0){//hottopic
                            cajas += (total / 50);
                            if ((total % 50) > 0) { cajas++; }
                        }else{
                            cajas += (total / r.piezas);
                            if ((total % r.piezas) > 0) { cajas++; }
                        }
                    }
                }
                if (tipo_empaque == 2){
                    List<ratio_tallas> lista_ratio = ds.obtener_lista_ratio(e.id_po_summary, e.id_estilo, 2);
                    int i = 0;
                    foreach (ratio_tallas r in lista_ratio){
                        if (i == 0){
                            int total = buscar_total_talla_primera_calidad(e.id_po_summary, r.id_talla);
                            if (r.ratio == 0){//hottopic
                                cajas += (total / 50);
                                if ((total % 50) > 0) { cajas++; }
                            }else{
                                cajas += (total / r.ratio);
                            }
                        }
                        i++;
                    }
                }
                if (tipo_empaque == 5){
                    List<ratio_tallas> lista_ratio = ds.obtener_lista_ratio(e.id_po_summary, e.id_estilo, 5);
                    foreach (ratio_tallas r in lista_ratio){
                        cajas++;
                    }
                }
                if (tipo_empaque == 4){
                    List<int> ppks = ds.obtener_number_ppks_estilo(e.id_po_summary);
                    foreach (int p in ppks){
                        List<string> packings = ds.obtener_number_ppks_estilo(e.id_po_summary, p);
                        foreach (string packing_pks in packings){
                            cajas += obtener_total_cartones_ppks(e.id_po_summary, packing_pks);
                        }
                    }
                }
            }
            List<Assortment> lista_assorts_pedido = ds.lista_assortments_pedido(Convert.ToInt32(pedido));
            foreach (Assortment a in lista_assorts_pedido){
                cajas += a.cartones;
            }
            return cajas;
        }
        public int buscar_tipo_empaque(int summary){
            int temp = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT TYPE_PACKING from PACKING_TYPE_SIZE where ID_SUMMARY='" + summary + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temp = Convert.ToInt32(leer["TYPE_PACKING"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }
        public int buscar_total_talla_primera_calidad(int summary, int talla){
            int temp = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT CANTIDAD,EXTRAS,EJEMPLOS from ITEM_SIZE where " +
                    " ID_SUMMARY='" + summary + "' and TALLA_ITEM='" + talla + "'   AND TALLA_ITEM IS NOT NULL";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temp = Convert.ToInt32(leer["CANTIDAD"]) - Convert.ToInt32(leer["EXTRAS"]) - Convert.ToInt32(leer["EJEMPLOS"]);//\"1RST_CALIDAD\",
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }
        public int obtener_total_cartones_ppks(int summary, string packing){
            int temp = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT TOTAL_CARTONS from PACKING_TYPE_SIZE where ID_SUMMARY='" + summary + "' AND " +
                    " PACKING_NAME='" + packing + "' AND TYPE_PACKING=4";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temp = Convert.ToInt32(leer["TOTAL_CARTONS"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }
        public void eliminar_trim_request(string request){
            buscar_id_trim_estilos(request);
            eliminar_trim_request_estilos(request);
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "DELETE FROM trim_requests WHERE id_request='" + request + "'";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void buscar_id_trim_estilos(string request){
            Link con_led = new Link();
            try{
                SqlCommand com_led = new SqlCommand();
                SqlDataReader leer_led = null;
                com_led.Connection = con_led.AbrirConexion();
                com_led.CommandText = " SELECT id FROM trim_estilos where id_request='" + request + "' ";
                leer_led = com_led.ExecuteReader();
                while (leer_led.Read()){
                    eliminar_trim_request_estilos_tallas(Convert.ToString(leer_led["id"]));
                }leer_led.Close();
            }finally { con_led.CerrarConexion(); con_led.Dispose(); }
        }
        public void eliminar_trim_request_estilos_tallas(string id){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "DELETE FROM trim_estilos_tallas WHERE id_trim_estilos='" + id + "'";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void eliminar_trim_request_estilos(string request){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "DELETE FROM trim_estilos WHERE id_request='" + request + "'";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void modificar_request(string request, string total, int usuario, string cantidad, string blank){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "UPDATE trim_requests SET usuario='" + usuario + "',cantidad='" + cantidad + "',blanks='" + blank + "'," +
                    "total='" + total + "',fecha='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                    " WHERE id_request='" + request + "' ";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void guardar_request(string pedido, string total, string estilo, string item, string talla, int usuario, string cantidad, string blank){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO trim_requests(id_item,total,usuario,fecha,cantidad,blanks,id_status,id_pedido) VALUES " +
                    "('" + item + "','" + total + "','" + usuario + "'," +
                    "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + cantidad + "','" + blank + "','1','" + pedido + "')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public int obtener_ultimo_request(){
            int temp = 0;
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT TOP 1 id_request FROM trim_requests order by id_request desc ";
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    temp = Convert.ToInt32(leer_u_i["id_request"]);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            return temp;
        }
        public void guardar_request_estilo(string summary, int request){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO trim_estilos(id_summary,id_request) VALUES ('" + summary + "','" + request + "')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public int obtener_ultimo_request_estilo(){
            int temp = 0;
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT TOP 1 id FROM trim_estilos order by id desc ";
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    temp = Convert.ToInt32(leer_u_i["id"]);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            return temp;
        }
        public void guardar_request_estilo_talla(int estilo_request, string talla){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO trim_estilos_tallas(id_trim_estilos,id_talla) VALUES ('" + estilo_request + "','" + talla + "')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public List<Impresion> obtener_impresiones_orden(string pedido){
            List<Impresion> lista = new List<Impresion>();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT tipo,id_pedido,id_summary,id_talla FROM trims_impresiones WHERE id_pedido=" + pedido;
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Impresion tr = new Impresion();
                    tr.id_pedido = Convert.ToInt32(leer_ltd["id_pedido"]);
                    tr.id_summary = Convert.ToInt32(leer_ltd["id_summary"]);
                    tr.id_talla = Convert.ToInt32(leer_ltd["id_talla"]);
                    tr.tipo = Convert.ToString(leer_ltd["tipo"]);
                    lista.Add(tr);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public Trim_requests obtener_trim_request(int trim_request){
            //Trim_requests lista = new Trim_requests();
            Trim_requests tr = new Trim_requests();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT id_request,id_item,total,usuario,id_pedido,fecha,cantidad,blanks from trim_requests where id_request='" + trim_request + "'";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    tr.id_request = Convert.ToInt32(leer_ltd["id_request"]);
                    tr.id_item = Convert.ToInt32(leer_ltd["id_item"]);
                    tr.id_pedido = Convert.ToInt32(leer_ltd["id_pedido"]);
                    tr.item = consultas.buscar_amt_item(Convert.ToString(tr.id_item)) + " " + consultas.buscar_descripcion_item(Convert.ToString(tr.id_item));
                    tr.tipo_item = consultas.buscar_tipo_trim_item(Convert.ToString(tr.id_item));
                    tr.total = Convert.ToInt32(leer_ltd["total"]);
                    tr.blanks = Convert.ToInt32(leer_ltd["blanks"]);                    
                    tr.id_usuario = Convert.ToInt32(leer_ltd["usuario"]);
                    tr.usuario = consultas.buscar_nombre_usuario(Convert.ToString(tr.id_usuario));
                    tr.lista_estilos = obtener_trims_estilos(tr.id_request);
                    tr.pedido = consultas.obtener_po_id(Convert.ToString(leer_ltd["id_pedido"]));                    
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return tr;
        }
        public void cambiar_estado_trim_request(int request, int estado){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE trim_requests SET id_status='" + estado + "' WHERE id_request='" + request + "'  ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void marcar_price_ticket_orden_impreso(string pt){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE price_tickets SET impreso=1 WHERE id_pedido='" + pt + "'  ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void marcar_pt_impreso(int request, int estado){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "UPDATE trim_requests SET impreso='" + estado + "' WHERE id_request='" + request + "' ";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void marcar_talla_impresa(int pt, int estado){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE trim_estilos_tallas SET impreso='" + estado + "' WHERE id='" + pt + "'  ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public int buscar_instruccion_empaque(int pedido){
            int temp = 0;
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT id_instruccion FROM instrucciones_empaque WHERE id_pedido='" + pedido + "' ";
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    temp = Convert.ToInt32(leer_u_i["id_instruccion"]);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            return temp;
        }
        public void agregar_instruccion_empaque(int usuario, int pedido, string instruccion){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO instrucciones_empaque(id_pedido,estado,fecha,id_usuario) VALUES " +
                    "('" + pedido + "','" + instruccion + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + usuario + "')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void update_instruccion_empaque(int id_instruccion, int usuario, string instruccion){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "UPDATE instrucciones_empaque SET id_usuario='" + usuario + "',estado='" + instruccion + "'" +
                    ",fecha='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE id_instruccion='" + id_instruccion + "' ";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void ingresar_fold_size(int pedido, string fold_size, string hang_size, string estado){
            int existe = buscar_fold_size(pedido);
            if (existe != 0){
                actualizar_fold_size(existe, fold_size, hang_size, estado);
            }else{
                insertar_fold_size(pedido, fold_size, hang_size, estado);
            }
        }
        public int buscar_fold_size(int pedido){
            int temp = 0;
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT id_fold_size FROM trims_fold_sizes WHERE id_pedido='" + pedido + "' ";
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    temp = Convert.ToInt32(leer_u_i["id_fold_size"]);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            return temp;
        }
        public void actualizar_fold_size(int id_fold_size, string fold_size, string hang_size, string estado){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "UPDATE trims_fold_sizes SET fold_size='" + fold_size + "',hang_size='" + hang_size + "',estado=" + estado +
                    " WHERE id_fold_size='" + id_fold_size + "' ";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void insertar_fold_size(int pedido, string fold_size, string hang_size, string estado){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO trims_fold_sizes(id_pedido,id_trim_card,fold_size,hang_size,estado) VALUES " +
                    "('" + pedido + "','0','" + fold_size + "','" + hang_size + "','" + estado + "')  ";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public List<Family_trim> obtener_familias(){
            List<Family_trim> lista = new List<Family_trim>();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT id_family_trim,family_trim FROM family_trims ORDER BY orden ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Family_trim ft = new Family_trim();
                    ft.id_family_trim = Convert.ToInt32(leer_ltd["id_family_trim"]);
                    ft.family_trim = Convert.ToString(leer_ltd["family_trim"]);
                    lista.Add(ft);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public List<Pedidos_trim> obtener_pedidos_reporte_colores_orden(string pedido){
            List<Pedidos_trim> lista = new List<Pedidos_trim>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            DatosShipping ds = new DatosShipping();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT P.ID_PEDIDO,P.PO,P.CUSTOMER,P.DATE_CANCEL,CC.NAME from " +
                    " PEDIDO P, CAT_CUSTOMER CC where CC.CUSTOMER=P.CUSTOMER and P.ID_PEDIDO='" + pedido + "' ORDER BY P.DATE_CANCEL ";
                leer = com.ExecuteReader();
                while (leer.Read())                {
                    Pedidos_trim p = new Pedidos_trim();
                    p.id_pedido = Convert.ToInt32(leer["ID_PEDIDO"]);
                    p.pedido = Convert.ToString(leer["PO"]);
                    p.id_customer = Convert.ToInt32(leer["CUSTOMER"]);
                    p.customer = Convert.ToString(leer["NAME"]);
                    p.ship_date = Convert.ToDateTime(leer["DATE_CANCEL"]).ToString("dd MMM yyyy");
                    List<int> temporal = consultas.Lista_generos_po(p.id_pedido);
                    p.gender = "";
                    foreach (int x in temporal){
                        p.gender += consultas.obtener_genero_id(Convert.ToString(x));
                    }
                    p.gender = Regex.Replace(p.gender, @"\s+", " ");
                    p.lista_families = obtener_lista_familias(p.id_pedido);
                    p.lista_empaque = lista_tipos_empaque(Convert.ToString(p.id_pedido));
                    p.lista_assort = ds.lista_assortments_pedido(p.id_pedido);
                    p.fold_size = buscar_fold_size_pedido(p.id_pedido);
                    lista.Add(p);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Empaque> lista_tipos_empaque(string id_pedido){
            List<Empaque> lista = new List<Empaque>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            DatosShipping ds = new DatosShipping();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct ITEM_ID from PO_SUMMARY where ID_PEDIDOS='" + id_pedido + "'  AND ID_ESTADO!=6 AND ID_ESTADO!=7  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    estilo_shipping l = new estilo_shipping();
                    l.id_estilo = Convert.ToInt32(leerFilas["ITEM_ID"]);
                    l.id_summary = consultas.obtener_po_summary(Convert.ToInt32(id_pedido), l.id_estilo);
                    l.descripcion = consultas.buscar_descripcion_estilo(l.id_estilo); ;
                    List<Empaque> lista_e = new List<Empaque>();
                    List<string> tipo_empaque_temporal = consultas.buscar_tipo_empaque(l.id_summary);
                    foreach (string s in tipo_empaque_temporal){
                        if (s == "1"){//e.lista_ratio = ds.obtener_lista_tallas_estilo(l.id_summary, l.id_estilo);
                            Empaque e = new Empaque();
                            e.estilo = l.descripcion;
                            e.tipo_empaque = Convert.ToInt32(s);
                            e.lista_ratio = ds.obtener_lista_ratio(l.id_summary, l.id_estilo, 1);
                            lista.Add(e);
                        }
                        if (s == "2"){
                            Empaque e = new Empaque();
                            e.estilo = l.descripcion;
                            e.tipo_empaque = Convert.ToInt32(s);
                            e.lista_ratio = ds.obtener_lista_ratio(l.id_summary, l.id_estilo, 2);
                            lista.Add(e);
                        }
                        if (s == "4"){ //e.lista_ratio = ds.obtener_lista_ratio(l.id_summary, l.id_estilo, 2);
                            List<int> ppks = ds.obtener_number_ppks_estilo(l.id_summary);
                            foreach (int p in ppks){
                                List<string> packings = ds.obtener_number_ppks_estilo(l.id_summary, p);
                                foreach (string packing_pks in packings){
                                    Empaque e4 = new Empaque();
                                    e4.estilo = l.descripcion;
                                    e4.tipo_empaque = Convert.ToInt32(s);
                                    e4.number_ppk = p;
                                    e4.packing_name = packing_pks;
                                    e4.lista_ratio = ds.obtener_lista_ratio_ppks(l.id_summary, l.id_estilo, 4, p, packing_pks);
                                    lista.Add(e4);
                                }
                            }
                        }
                        if (s == "5"){//e.lista_ratio = ds.obtener_lista_ratio(l.id_summary, l.id_estilo, 2); 
                            List<string> bps = ds.obtener_number_bps_estilo(l.id_summary);
                            foreach (string b in bps)
                            {
                                Empaque e5 = new Empaque();
                                e5.estilo = l.descripcion;
                                e5.tipo_empaque = Convert.ToInt32(s);
                                e5.packing_name = b;
                                e5.lista_ratio = ds.obtener_lista_ratio_bps(l.id_summary, l.id_estilo, 5, b);
                                lista.Add(e5);
                            }
                        }
                    }
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<Family_trim> obtener_lista_familias(int pedido){
            List<Family_trim> lista = new List<Family_trim>();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT id_family_trim,family_trim FROM family_trims ORDER BY orden ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Family_trim ft = new Family_trim();
                    ft.id_family_trim = Convert.ToInt32(leer_ltd["id_family_trim"]);
                    ft.family_trim = Convert.ToString(leer_ltd["family_trim"]);
                    ft.lista_requests = buscar_trims_pedido_familia(pedido, ft.family_trim);
                    lista.Add(ft);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public List<Trim_requests> buscar_trims_pedido_familia(int pedido, string familia){
            List<Trim_requests> lista = new List<Trim_requests>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT tr.entregado,tr.id_request,tr.id_item,tr.total,tr.usuario,tr.fecha,tr.id_status,ts.status," +
                    "tr.impreso,tr.stock FROM trim_requests tr,items_catalogue ic,trim_status ts WHERE tr.id_pedido='" + pedido + "' AND ic.item_id=tr.id_item " +
                    " AND ic.fabric_type='" + familia + "' AND ts.id_status=tr.id_status ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Trim_requests tr = new Trim_requests();
                    tr.id_request = Convert.ToInt32(leer_ltd["id_request"]);
                    tr.id_item = Convert.ToInt32(leer_ltd["id_item"]);
                    tr.item = consultas.buscar_descripcion_item(Convert.ToString(tr.id_item));
                    tr.tipo_item = consultas.buscar_tipo_trim_item(Convert.ToString(tr.id_item));
                    tr.total = Convert.ToInt32(leer_ltd["total"]);                                        
                    tr.fecha = Convert.ToDateTime(leer_ltd["fecha"]).ToString("dd-MM");
                    tr.lista_estilos = obtener_trims_estilos(tr.id_request);
                    tr.entregado = Convert.ToInt32(leer_ltd["entregado"]);                    
                    tr.id_estado = Convert.ToInt32(leer_ltd["id_status"]);
                    tr.impreso = Convert.ToInt32(leer_ltd["impreso"]);
                    tr.stock = Convert.ToInt32(leer_ltd["stock"]);
                    tr.estado = Convert.ToString(leer_ltd["status"]);
                    tr.mill_po= obtener_lista_recibos_millpo(pedido, tr.id_item);
                    tr.cantidad=obtener_total_trim_recibido_pedido(pedido, tr.id_item);

                    tr.usuario = consultas.buscar_nombre_usuario(Convert.ToString(leer_ltd["usuario"]));
                    if (familia == "PRICE TICKETS"){
                        tr.templates_pt = buscar_templates_price_tickets(pedido);
                    }lista.Add(tr);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public string obtener_lista_recibos_millpo(int pedido,int item){
           string lista = "";
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT r.mp_number,r.mill_po,r.fecha FROM recibos r Join recibos_items ri ON ri.id_recibo=r.id_recibo " +
                    " WHERE ri.id_inventario="+item+" AND id_summary="+pedido;                    
                leer = com.ExecuteReader();
                while (leer.Read()){
                    lista+="*"+Convert.ToString(leer["mp_number"]) + " " + Convert.ToString(leer["mill_po"])+"_"+ (Convert.ToDateTime(leer["fecha"])).ToString("MM/dd/yyyy"); ;
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public int obtener_total_trim_recibido_pedido(int pedido,int item){
            int lista = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = " SELECT total FROM stock_ordenes WHERE id_pedido="+pedido+" AND id_item="+item;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    lista += Convert.ToInt32(leer["total"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }







        public int buscar_templates_price_tickets(int pedido){
            int trim = 0;
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT id_price_ticket FROM price_tickets WHERE id_pedido='" + pedido + "'   ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    trim++;
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return trim;
        }
        public List<Pedidos_trim> obtener_pedidos_reporte_estado_customer(string estado, string customer, string inicio, string final){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            DatosShipping ds = new DatosShipping();
            string query = "";
            if (estado != "0" && customer == "0" && inicio == "0" && final == "0"){
                query = "SELECT P.ID_PEDIDO,P.PO,P.CUSTOMER,P.DATE_CANCEL,CC.NAME,tfs.estado from PEDIDO P, CAT_CUSTOMER CC, trims_fold_sizes tfs " +
                        " where CC.CUSTOMER=P.CUSTOMER and tfs.id_pedido=P.ID_PEDIDO and tfs.estado=" + estado + " ORDER BY P.DATE_CANCEL ";
            }
            if (estado != "0" && customer != "0" && inicio == "0" && final == "0"){
                query = "SELECT P.ID_PEDIDO,P.PO,P.CUSTOMER,P.DATE_CANCEL,CC.NAME,tfs.estado from PEDIDO P, CAT_CUSTOMER CC, trims_fold_sizes tfs " +
                    " where CC.CUSTOMER=P.CUSTOMER and tfs.id_pedido=P.ID_PEDIDO and " +
                    " tfs.estado=" + estado + "' and P.CUSTOMER='" + customer + "'" + " ORDER BY P.DATE_CANCEL ";
            }
            if (estado != "0" && customer != "0" && inicio != "0" && final != "0"){
                query = "SELECT P.ID_PEDIDO,P.PO,P.CUSTOMER,P.DATE_CANCEL,CC.NAME,tfs.estado from PEDIDO P, CAT_CUSTOMER CC, trims_fold_sizes tfs " +
                    " where CC.CUSTOMER=P.CUSTOMER and tfs.id_pedido=P.ID_PEDIDO and tfs.estado='" + estado + "' and " +
                    " P.CUSTOMER='" + customer + "'  and " +
                    " P.DATE_CANCEL BETWEEN '" + inicio + " 00:00:00' AND '" + final + " 23:59:59' " + " ORDER BY P.DATE_CANCEL ";
            }
            if (estado == "0" && customer != "0" && inicio != "0" && final != "0"){
                query = "SELECT P.ID_PEDIDO,P.PO,P.CUSTOMER,P.DATE_CANCEL,CC.NAME,tfs.estado from PEDIDO P, CAT_CUSTOMER CC, trims_fold_sizes tfs " +
                    " where CC.CUSTOMER=P.CUSTOMER and tfs.id_pedido=P.ID_PEDIDO and " +
                    " P.CUSTOMER='" + customer + "'  and " +
                    " P.DATE_CANCEL BETWEEN '" + inicio + " 00:00:00' AND '" + final + " 23:59:59' " + " ORDER BY P.DATE_CANCEL ";
            }
            if (estado == "0" && customer == "0" && inicio != "0" && final != "0"){
                query = "SELECT P.ID_PEDIDO,P.PO,P.CUSTOMER,P.DATE_CANCEL,CC.NAME,tfs.estado from PEDIDO P, CAT_CUSTOMER CC, trims_fold_sizes tfs " +
                    " where CC.CUSTOMER=P.CUSTOMER and tfs.id_pedido=P.ID_PEDIDO and" +
                    " P.DATE_CANCEL BETWEEN '" + inicio + " 00:00:00' AND '" + final + " 23:59:59' " + " ORDER BY P.DATE_CANCEL ";
            }
            if (estado == "0" && customer != "0" && inicio == "0" && final == "0"){
                query = "SELECT P.ID_PEDIDO,P.PO,P.CUSTOMER,P.DATE_CANCEL,CC.NAME,tfs.estado from PEDIDO P, CAT_CUSTOMER CC , trims_fold_sizes tfs" +
                    " where CC.CUSTOMER=P.CUSTOMER and tfs.id_pedido=P.ID_PEDIDO and P.CUSTOMER='" + customer + "'  " + " ORDER BY P.DATE_CANCEL ";
            }
            if (estado == "0" && customer == "0" && inicio == "0" && final == "0"){
                query = "SELECT P.ID_PEDIDO,P.PO,P.CUSTOMER,P.DATE_CANCEL,CC.NAME,tfs.estado from PEDIDO P, CAT_CUSTOMER CC, trims_fold_sizes tfs " +
                    " where CC.CUSTOMER=P.CUSTOMER and tfs.id_pedido=P.ID_PEDIDO  " + " ORDER BY P.DATE_CANCEL ";
            }
            List<Pedidos_trim> lista = new List<Pedidos_trim>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = query;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Pedidos_trim p = new Pedidos_trim();
                    p.id_pedido = Convert.ToInt32(leer["ID_PEDIDO"]);
                    p.pedido = Convert.ToString(leer["PO"]);
                    p.id_customer = Convert.ToInt32(leer["CUSTOMER"]);
                    p.customer = Convert.ToString(leer["NAME"]);
                    p.ship_date = Convert.ToDateTime(leer["DATE_CANCEL"]).ToString("dd MMM yyyy");
                    if (Convert.ToInt32(leer["estado"]) == 1){
                        p.estado = "OPEN";
                    }else{
                        if (Convert.ToInt32(leer["estado"]) == 2){
                            p.estado = "CLOSED";
                        }else{
                            p.estado = "CANCELLED";
                        }
                    }
                    List<int> temporal = consultas.Lista_generos_po(p.id_pedido);
                    p.gender = "";
                    foreach (int x in temporal) { p.gender += consultas.obtener_genero_id(Convert.ToString(x)); }
                    p.gender = Regex.Replace(p.gender, @"\s+", " ");
                    p.lista_families = obtener_lista_familias(p.id_pedido);
                    p.lista_empaque = lista_tipos_empaque(Convert.ToString(p.id_pedido));
                    p.lista_assort = ds.lista_assortments_pedido(p.id_pedido);
                    p.fold_size = buscar_fold_size_pedido(p.id_pedido);
                    lista.Add(p);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<registro_price_tickets> buscar_registro_price_ticket_pedido(int pedido){
            List<registro_price_tickets> lista = new List<registro_price_tickets>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select PS.ID_PO_SUMMARY,PS.ID_COLOR,PS.ITEM_ID,IZ.TALLA_ITEM,IZ.CANTIDAD from PO_SUMMARY PS, ITEM_SIZE IZ " +
                    " where PS.ID_PEDIDOS='" + pedido + "' AND  PS.ID_PO_SUMMARY=IZ.ID_SUMMARY AND IZ.TALLA_ITEM IS NOT NULL";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    registro_price_tickets rpt = new registro_price_tickets();
                    rpt.id_pedido = pedido;
                    rpt.id_estilo = Convert.ToInt32(leer["ITEM_ID"]);
                    rpt.id_summary = Convert.ToInt32(leer["ID_PO_SUMMARY"]);
                    rpt.id_color = Convert.ToInt32(leer["ID_COLOR"]);
                    rpt.estilo = (consultas.obtener_estilo((rpt.id_estilo))).Trim();
                    rpt.descripcion_estilo = (consultas.buscar_descripcion_estilo(rpt.id_estilo)).Trim();
                    rpt.color = ((consultas.obtener_color_id((rpt.id_color).ToString())).Trim()).Remove(0, 2);
                    rpt.id_talla = Convert.ToInt32(leer["TALLA_ITEM"]);
                    rpt.talla = (consultas.obtener_size_id(Convert.ToString(leer["TALLA_ITEM"]))).Trim();
                    rpt.total = (Convert.ToString(leer["CANTIDAD"])).Trim();
                    rpt.upc = (buscar_upc(rpt.id_summary, rpt.id_talla)).Trim();
                    lista.Add(rpt);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public string buscar_upc(int summary, int talla){
            string temp = "";
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "select UPC from UPC where IdSummary='" + summary + "' and IdTalla='" + talla + "'  ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    temp = Convert.ToString(leer_ltd["UPC"]);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return temp;
        }
        public void guardar_price_tickets(string pedido, string total, string estilo, string upc, string descripcion, string color, string talla, string ticket, string dept, string clas, string sub, string retail, string cl, string usuario, string impreso){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO price_tickets(id_pedido,total, estilo, upc, descripcion, color, talla, ticket, dept,class, sub, retail, cl,id_usuario,fecha,impreso) VALUES " +
                    "('" + pedido + "','" + total + "','" + estilo + "','" + upc + "','" + descripcion + "','" + color + "','" + talla + "'," +
                    "'" + ticket + "','" + dept + "','" + clas + "','" + sub + "','" + retail + "','" + cl + "','" + usuario + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + impreso + "')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void guardar_impresion(string tipo, string pedido, string estilo, string talla){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO trims_impresiones(tipo,id_pedido,id_summary,id_talla) VALUES ('" + tipo + "','" + pedido + "','" + estilo + "','" + talla + "')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void marcar_price_tickets_pedido_impreso(int pedido){
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select ps.ID_PO_SUMMARY,iz.TALLA_ITEM,iz.CANTIDAD,iz.\"1RST_CALIDAD\",iz.EXTRAS,iz.EJEMPLOS from ITEM_SIZE iz," +
                    " PO_SUMMARY ps where ps.ID_PEDIDOS='" + pedido + "'  and ps.ID_PO_SUMMARY=iz.ID_SUMMARY  AND iz.TALLA_ITEM IS NOT NULL ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    int summary = Convert.ToInt32(leer["ID_PO_SUMMARY"]);
                    int total = Convert.ToInt32(leer["CANTIDAD"]);// + Convert.ToInt32(leer["EXTRAS"])+ Convert.ToInt32(leer["EJEMPLOS"]);
                    List<Trim_requests> lista_items = obtener_total_inventario_pt(pedido);
                    foreach (Trim_requests t in lista_items){
                        foreach (Trim_estilo te in t.lista_estilos){
                            foreach (Talla_t ta in te.lista_tallas){
                                marcar_talla_impresa(ta.id, 1);
                            }
                        }
                    }
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
        }
        public List<Trim_requests> obtener_total_inventario_pt(int pedido){
            List<Trim_requests> lista = new List<Trim_requests>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT tr.id_request,tr.total,tr.id_item FROM trim_requests tr,items_catalogue ic " +
                    " WHERE ic.item_id=tr.id_item AND tr.id_pedido='" + pedido + "' AND ic.fabric_type='PRICE TICKETS' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Trim_requests tr = new Trim_requests();
                    tr.id_request = Convert.ToInt32(leer["id_request"]);                   
                    tr.id_item = Convert.ToInt32(leer["id_item"]);
                    tr.total = Convert.ToInt32(leer["total"]);
                    tr.lista_estilos = obtener_trims_estilos(tr.id_request);
                    lista.Add(tr);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public int buscar_request_pt_pedido(string pedido){
            int temp = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_request from trim_requests where id_pedido='" + pedido + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    string familia = buscar_familia_item_request(Convert.ToString(leer["id_request"]));
                    if (familia == "PRICE TICKETS"){
                        temp = Convert.ToInt32(leer["id_request"]);
                    }
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }
        public string buscar_familia_item_request(string request){
            string temp = "";
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT ic.fabric_type FROM items_catalogue ic,trim_requests tr " +
                    " where tr.id_item=ic.item_id and  tr.id_request='" + request + "'   ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temp = Convert.ToString(leer["fabric_type"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }
        public List<registro_price_tickets> buscar_price_tickets_pedido(int pedido){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<registro_price_tickets> lista = new List<registro_price_tickets>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_price_ticket,total,estilo,upc,descripcion,color,talla,ticket,dept,class,sub,retail,cl,id_usuario,fecha,impreso  " +
                    " FROM price_tickets WHERE id_pedido='" + pedido + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    registro_price_tickets rpt = new registro_price_tickets();
                    rpt.id_pedido = pedido;
                    rpt.pedido = consultas.obtener_po_id(Convert.ToString(pedido));
                    rpt.id_price_ticket = Convert.ToInt32(leer["id_price_ticket"]);
                    rpt.impreso = Convert.ToInt32(leer["impreso"]);
                    rpt.total = Convert.ToString(leer["total"]);
                    rpt.estilo = Convert.ToString(leer["estilo"]);
                    rpt.upc = Convert.ToString(leer["upc"]);
                    rpt.descripcion_estilo = Convert.ToString(leer["descripcion"]);
                    rpt.color = Convert.ToString(leer["color"]);
                    rpt.talla = Convert.ToString(leer["talla"]);
                    rpt.id_talla = consultas.buscar_talla(rpt.talla);
                    rpt.tickets = Convert.ToString(leer["ticket"]);
                    rpt.dept = Convert.ToString(leer["dept"]);
                    rpt.clas = Convert.ToString(leer["class"]);
                    rpt.sub = Convert.ToString(leer["sub"]);
                    rpt.retail = Convert.ToString(leer["retail"]);
                    rpt.cl = Convert.ToString(leer["cl"]);
                    rpt.fecha = (Convert.ToDateTime(leer["fecha"])).ToString("MMM dd yyyy");
                    rpt.usuario = consultas.buscar_nombre_usuario(Convert.ToString(leer["id_usuario"]));
                    rpt.id_summary = obtener_estilo_id(rpt.estilo, pedido);
                    lista.Add(rpt);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public int obtener_estilo_id(string estilo, int pedido){
            int temp = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT PS.ID_PO_SUMMARY,I.ITEM_STYLE from ITEM_DESCRIPTION I,PO_SUMMARY PS where " +
                    " PS.ID_PEDIDOS='" + pedido + "' AND PS.ITEM_ID=I.ITEM_ID ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    string e = (Convert.ToString(leer["ITEM_STYLE"])).Trim();
                    estilo = (estilo).Trim();
                    if (e == estilo){
                        temp = Convert.ToInt32(leer["ID_PO_SUMMARY"]);
                    }
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }
        public void marcar_price_ticket_impreso(string pt){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE price_tickets SET impreso=1 WHERE id_price_ticket='" + pt + "'  ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public int buscar_total_talla(int summary, int talla){
            int temp = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT CANTIDAD,\"1RST_CALIDAD\",EXTRAS,EJEMPLOS from ITEM_SIZE " +
                    "where ID_SUMMARY='" + summary + "' and TALLA_ITEM='" + talla + "'   AND TALLA_ITEM IS NOT NULL";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temp = Convert.ToInt32(leer["CANTIDAD"]);// + Convert.ToInt32(leer["EXTRAS"]) + Convert.ToInt32(leer["EJEMPLOS"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }
        public void eliminar_price_ticket(string pt){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "DELETE FROM price_tickets WHERE id_price_ticket='" + pt + "' ";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public List<Pedidos_trim> obtener_lista_pedidos_pt(){
            List<Pedidos_trim> listar = new List<Pedidos_trim>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT P.ID_PEDIDO,P.PO,P.CUSTOMER,P.DATE_CANCEL,CC.NAME from PEDIDO P, CAT_CUSTOMER CC,trims_fold_sizes t " +
                    " where ID_STATUS!=7 AND ID_STATUS!=6 AND ID_STATUS!=5  and CC.CUSTOMER=P.CUSTOMER AND P.ID_PEDIDO=t.id_pedido " +
                    " AND t.estado=1 ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    Pedidos_trim p = new Pedidos_trim();
                    p.id_pedido = Convert.ToInt32(leerFilas["ID_PEDIDO"]);
                    p.pedido = leerFilas["PO"].ToString();
                    p.id_customer = Convert.ToInt32(leerFilas["CUSTOMER"]);
                    p.customer = Convert.ToString(leerFilas["NAME"]);                    
                    p.lista_trims = obtener_trims_pt(p.id_pedido);
                    listar.Add(p);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return listar;
        }
        public List<Trim_requests> obtener_trims_pt(int pedido){
            List<Trim_requests> lista = new List<Trim_requests>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT tr.id_request,tr.total,tr.impreso FROM trim_requests tr,items_catalogue ic " +
                    "WHERE ic.item_id=tr.id_item  AND ic.fabric_type='PRICE TICKETS' AND tr.id_pedido='" + pedido + "'";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Trim_requests tr = new Trim_requests();
                    tr.id_request = Convert.ToInt32(leer["id_request"]);                   
                    tr.total = Convert.ToInt32(leer["total"]);                    
                    tr.impreso = Convert.ToInt32(leer["impreso"]);                    
                    tr.lista_estilos = obtener_trims_estilos(tr.id_request);
                    lista.Add(tr);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public int buscar_pt_orden_estilo(int pedido, string estilo, string talla){
            int temp = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_price_ticket from price_tickets where id_pedido='" + pedido + "' " +
                    " and estilo='" + estilo + "'  and talla='" + talla + "'  ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temp = Convert.ToInt32(leer["id_price_ticket"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }
        public List<Trim_requests> obtener_trims_stickers(int pedido){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<Trim_requests> lista = new List<Trim_requests>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT tr.id_request,tr.id_item,tr.total,tr.impreso FROM trim_requests tr,items_catalogue ic " +
                    " WHERE ic.item_id=tr.id_item AND (ic.fabric_type='STICKERS' or ic.fabric_type='UPC STICKERS' or " +
                    " ic.fabric_type='CARTON LABEL' or ic.fabric_type='HOUSE LABEL') AND tr.id_pedido='" + pedido + "'";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Trim_requests tr = new Trim_requests();
                    tr.id_request = Convert.ToInt32(leer["id_request"]);
                    tr.total = Convert.ToInt32(leer["total"]);
                    tr.impreso = Convert.ToInt32(leer["impreso"]);                    
                    tr.id_item = Convert.ToInt32(leer["id_item"]);
                    tr.item = consultas.buscar_descripcion_item(Convert.ToString(tr.id_item));
                    tr.tipo_item = consultas.buscar_tipo_trim_item(Convert.ToString(tr.id_item));
                    tr.lista_estilos = obtener_trims_estilos(tr.id_request);
                    lista.Add(tr);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public void marcar_talla_impresa_sticker(int pt, int estado){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE trim_estilos_tallas SET impreso='" + estado + "' WHERE id='" + pt + "'  ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public List<clientes> obtener_lista_clientes(){
            List<clientes> lista = new List<clientes>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT CUSTOMER_FINAL,NAME_FINAL from CAT_CUSTOMER_PO order by NAME_FINAL  ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    clientes c = new clientes();
                    c.id_customer = Convert.ToInt32(leer["CUSTOMER_FINAL"]);
                    c.customer = Convert.ToString(leer["NAME_FINAL"]);
                    lista.Add(c);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<generos> obtener_lista_generos(){
            Link con = new Link();
            List<generos> lista = new List<generos>();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT ID_GENDER,GENERO FROM CAT_GENDER ORDER BY GENERO";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    generos g = new generos();
                    g.id_genero = Convert.ToInt32(leer["ID_GENDER"]);
                    g.genero = Convert.ToString(leer["GENERO"]);
                    lista.Add(g);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Pedidos_trim_card> obtener_lista_trim_cards(string busqueda){
            List<Pedidos_trim_card> lista = new List<Pedidos_trim_card>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string query = "";
            if (busqueda == "0"){
                query = "SELECT TOP 50 id_trim_card,id_pedido,id_customer,id_usuario,fecha,estado,comentarios,entrega,recibe,fecha_entrega FROM trim_card ORDER BY id_trim_card DESC";
            }else{
                query = "SELECT TOP 50 tc.id_trim_card,tc.id_pedido,tc.id_customer,tc.id_usuario,tc.fecha,estado,comentarios,entrega,recibe,fecha_entrega FROM trim_card tc, PEDIDO p " +
                    " WHERE tc.id_pedido=p.ID_PEDIDO AND p.PO like'%" + busqueda + "%' ORDER BY tc.id_trim_card DESC";
            }
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = query;
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Pedidos_trim_card p = new Pedidos_trim_card();
                    p.id_trim_card = Convert.ToInt32(leer_ltd["id_trim_card"]);
                    p.estado = Convert.ToInt32(leer_ltd["estado"]);
                    p.comentarios = Convert.ToString(leer_ltd["comentarios"]);
                    p.pedido = consultas.obtener_po_id(Convert.ToString(leer_ltd["id_pedido"]));
                    p.customer = consultas.obtener_customer_final_id(Convert.ToString(leer_ltd["id_customer"]));
                    p.usuario = consultas.buscar_nombre_usuario(Convert.ToString(leer_ltd["id_usuario"]));
                    p.fecha = (Convert.ToDateTime(leer_ltd["fecha"])).ToString("MMM dd yyyy");
                    p.entrega = Convert.ToString(leer_ltd["entrega"]);
                    if (p.entrega != "0") {
                        p.recibe = Convert.ToString(leer_ltd["recibe"]);
                        p.fecha_entrega = (Convert.ToDateTime(leer_ltd["fecha_entrega"])).ToString("MMM dd yyyy");
                    }
                    lista.Add(p);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public void eliminar_trim_card(string id){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "DELETE FROM trim_card WHERE id_trim_card='" + id + "'  ";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void eliminar_trim_card_spec(string id){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "DELETE FROM trim_card_spec WHERE id_trim_card='" + id + "'  ";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void guardar_nueva_imagen(string imagen, string familia, int usuario){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO trims_imagenes_archivo(imagen,id_familia,fecha,id_usuario) VALUES " +
                    "('" + imagen + "','" + familia + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + usuario + "')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public int obtener_ultimo_id_imagen(){
            int temp = 0;
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT TOP 1 id_imagen FROM trims_imagenes_archivo ORDER BY  id_imagen DESC  ";
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    temp = Convert.ToInt32(leer_u_i["id_imagen"]);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            return temp;
        }
        public int obtener_pedido_trim_card(string id ){
            int temp = 0;
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT id_pedido FROM trim_card WHERE id_trim_card="+id;
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    temp = Convert.ToInt32(leer_u_i["id_pedido"]);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            return temp;
        }
        public void guardar_imagen_genero_cliente(int imagen, string cliente, string genero){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO trims_imagenes(id_imagen,id_cliente,id_genero) VALUES ('" + imagen + "','" + cliente + "','" + genero + "')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public List<imagenes_trim> obtener_lista_imagenes_familia(int familia){
            Link con = new Link();
            List<imagenes_trim> lista = new List<imagenes_trim>();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_imagen,imagen FROM trims_imagenes_archivo WHERE id_familia='" + familia + "'";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    imagenes_trim i = new imagenes_trim();
                    i.id_imagen = Convert.ToInt32(leer["id_imagen"]);
                    //i.imagen = Convert.ToString(leer["imagen"]);
                    i.imagen = Convert.ToString(leer["imagen"]);
                    i.lista_datos = obtener_lista_generos_clientes_imagen(i.id_imagen);
                    lista.Add(i);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<imagen_datos> obtener_lista_generos_clientes_imagen(int imagen){
            List<imagen_datos> lista = new List<imagen_datos>();
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT id_cliente,id_genero FROM trims_imagenes WHERE id_imagen='" + imagen + "'  ";
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    imagen_datos i = new imagen_datos();
                    i.id_genero = Convert.ToInt32(leer_u_i["id_genero"]);
                    i.id_customer = Convert.ToInt32(leer_u_i["id_cliente"]);
                    lista.Add(i);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            return lista;
        }
        public string buscar_imagen(int id){
            string nombre = "";
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT imagen FROM trims_imagenes_archivo WHERE id_imagen='" + id + "'  ";
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    nombre = Convert.ToString(leer_u_i["imagen"]);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            return nombre;
        }
        public void edicion_imagen_trim(string trim, string imagen, int usuario){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "UPDATE trims_imagenes_archivo SET imagen='" + imagen + "'," +
                    " fecha='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',id_usuario='" + usuario + "' " +
                    " WHERE id_imagen='" + trim + "' ";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void borrar_genero_clientes_imagen(string id){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "DELETE FROM trims_imagenes WHERE id_imagen='" + id + "' ";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public List<Familias_trim_card> obtener_datos_familias_pedido(int pedido, List<int> lista_generos, int customer){
            List<Familias_trim_card> lista = new List<Familias_trim_card>();
            List<int> lista_items = obtener_lista_items_pedido(pedido);
            List<int> lista_familias_pedido = (obtener_familias_items(lista_items)).Distinct().ToList();
            List<Familias_trim_card> lista_familias_imagen = obtener_imagenes_familia_cliente_genero(lista_familias_pedido, customer, lista_generos, pedido);
            return lista_familias_imagen;
        }
        public List<int> obtener_lista_items_pedido(int pedido){
            List<int> lista = new List<int>();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT distinct id_item FROM trim_requests WHERE id_pedido='" + pedido + "'   ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    lista.Add(Convert.ToInt32(leer_ltd["id_item"]));
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public List<int> obtener_familias_items(List<int> lista_items){
            List<int> lista = new List<int>();
            foreach (int i in lista_items){
                Link con_ltd = new Link();
                try{
                    SqlCommand com_ltd = new SqlCommand();
                    SqlDataReader leer_ltd = null;
                    com_ltd.Connection = con_ltd.AbrirConexion();
                    com_ltd.CommandText = "SELECT ft.id_family_trim FROM family_trims ft,items_catalogue ic WHERE " +
                        "  ic.item_id='" + i + "' AND ic.fabric_type=ft.family_trim ";
                    leer_ltd = com_ltd.ExecuteReader();
                    while (leer_ltd.Read()){
                        lista.Add(Convert.ToInt32(leer_ltd["id_family_trim"]));
                    }leer_ltd.Close();
                }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            }
            return lista;
        }
        public List<Familias_trim_card> obtener_imagenes_familia_cliente_genero(List<int> lista_familias_pedido, int customer, List<int> lista_generos, int pedido)
        {
            List<Familias_trim_card> lista_final = new List<Familias_trim_card>();
            foreach (int f in lista_familias_pedido){
                Familias_trim_card familia = new Familias_trim_card();
                familia.id_family_trim = f;
                familia.family_trim = obtener_nombre_familia(f);
                List<imagenes_trim> lista_temporal_imagen = new List<imagenes_trim>();
                int contador = 0;
                foreach (int g in lista_generos){
                    contador = 0;
                    Link con_ltd = new Link();
                    try{
                        SqlCommand com_ltd = new SqlCommand();
                        SqlDataReader leer_ltd = null;
                        com_ltd.Connection = con_ltd.AbrirConexion();
                        com_ltd.CommandText = "SELECT ti.id_imagen,tia.imagen FROM trims_imagenes ti,trims_imagenes_archivo tia WHERE " +
                            " tia.id_familia='" + f + "' AND ti.id_imagen=tia.id_imagen AND " +
                            " ti.id_cliente='" + customer + "' AND ti.id_genero='" + g + "' ";
                        leer_ltd = com_ltd.ExecuteReader();
                        while (leer_ltd.Read()){
                            imagenes_trim img = new imagenes_trim();
                            img.id_imagen = Convert.ToInt32(leer_ltd["id_imagen"]);
                            img.imagen = Convert.ToString(leer_ltd["imagen"]);
                            lista_temporal_imagen.Add(img);
                            contador++;
                        }leer_ltd.Close();
                    }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
                }
                if (contador == 0){
                    familia.lista_imagenes = obtener_imagenes_familia_cliente(f, customer);
                }else{
                    familia.lista_imagenes = lista_temporal_imagen;
                }
                familia.item = obtener_item_pedido_familia(familia.id_family_trim, pedido);
                lista_final.Add(familia);
            }
            return lista_final;
        }
        public string obtener_nombre_familia(int familia){
            string temporal = "";
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT family_trim FROM family_trims WHERE id_family_trim='" + familia + "'  ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    temporal = Convert.ToString(leer_ltd["family_trim"]);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return temporal;
        }
        public List<imagenes_trim> obtener_imagenes_familia_cliente(int familia, int customer){
            List<imagenes_trim> lista_temporal_imagen = new List<imagenes_trim>();
            int contador = 0;
            Link con_ltd = new Link();
            try{
                contador = 0;
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT distinct ti.id_imagen,tia.imagen FROM trims_imagenes ti,trims_imagenes_archivo tia WHERE " +
                            " tia.id_familia='" + familia + "' AND ti.id_imagen=tia.id_imagen AND ti.id_cliente='" + customer + "' ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    imagenes_trim img = new imagenes_trim();
                    img.id_imagen = Convert.ToInt32(leer_ltd["id_imagen"]);
                    img.imagen = Convert.ToString(leer_ltd["imagen"]);
                    lista_temporal_imagen.Add(img);
                    contador++;
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            if (contador == 0){
                return obtener_imagenes_familia(familia);
            }else{
                return lista_temporal_imagen;
            }
        }
        public string obtener_item_pedido_familia(int familia, int pedido){
            string temporal = "";
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = " select ic.descripcion from trim_requests tr, items_catalogue ic,family_trims ft where tr.id_pedido='" + pedido + "' " +
                    "   and ic.item_id=tr.id_item and ft.family_trim=ic.fabric_type and ft.id_family_trim='" + familia + "'";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    temporal = Convert.ToString(leer_ltd["descripcion"]);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return temporal;
        }
        public List<imagenes_trim> obtener_imagenes_familia(int familia){
            List<imagenes_trim> lista_temporal_imagen = new List<imagenes_trim>();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT distinct ti.id_imagen,tia.imagen FROM trims_imagenes ti,trims_imagenes_archivo tia WHERE " +
                        " tia.id_familia='" + familia + "' AND ti.id_imagen=tia.id_imagen ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    imagenes_trim img = new imagenes_trim();
                    img.id_imagen = Convert.ToInt32(leer_ltd["id_imagen"]);
                    img.imagen = Convert.ToString(leer_ltd["imagen"]);
                    lista_temporal_imagen.Add(img);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista_temporal_imagen;
        }
        public void guardar_nuevo_trim_card(string pedido, string customer, string tipo_empaque, string ratio, int usuario, string generos,string comentarios,string estado){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO trim_card(id_pedido,ratio,tipo_empaque,id_usuario,id_customer,generos,fecha,comentarios,estado) VALUES " +
                "('" + pedido + "','" + ratio + "','" + tipo_empaque + "','" + usuario + "','" + customer + "','" + generos + "','" +
                "" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + comentarios + "','" + estado + "')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public int obtener_ultimo_trim_card(){
            int temp = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT TOP 1 id_trim_card FROM trim_card  ORDER BY id_trim_card DESC ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temp = Convert.ToInt32(leer["id_trim_card"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }
        public void guardar_trim_card_familia(int id_trim_card, string imagen, string especial, string notas, string familia, string item){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO trim_card_spec(id_trim_card,id_imagen,especial,notas,id_familia,item) VALUES " +
                "('" + id_trim_card + "','" + imagen + "','" + especial + "','" + notas + "','" + familia + "','" + item + "')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void agregar_trim_card_fold_size(string pedido, int trim_card){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "UPDATE trims_fold_sizes SET id_trim_card='" + trim_card + "' WHERE id_pedido='" + pedido + "' ";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public List<Pedidos_trim_card> obtener_trim_card_pdf(int id){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<Pedidos_trim_card> lista = new List<Pedidos_trim_card>();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT id_trim_card,id_pedido,id_customer,id_usuario,fecha,generos,ratio,tipo_empaque FROM trim_card WHERE id_trim_card='" + id + "' ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Pedidos_trim_card p = new Pedidos_trim_card();
                    p.id_pedido = Convert.ToInt32(leer_ltd["id_pedido"]);
                    p.id_trim_card = Convert.ToInt32(leer_ltd["id_trim_card"]);
                    p.pedido = consultas.obtener_po_id(Convert.ToString(leer_ltd["id_pedido"]));
                    p.customer = consultas.obtener_customer_final_id(Convert.ToString(leer_ltd["id_customer"]));
                    p.usuario = consultas.buscar_nombre_usuario(Convert.ToString(leer_ltd["id_usuario"]));
                    p.fecha = (Convert.ToDateTime(leer_ltd["fecha"])).ToString("MMM dd yyyy");
                    p.gender = Convert.ToString(leer_ltd["generos"]);
                    p.ratio = Convert.ToString(leer_ltd["ratio"]);
                    p.fold_size = buscar_fold_size_trim_card(id);
                    p.tipo_empaque = Convert.ToInt32(leer_ltd["tipo_empaque"]);
                    p.lista_estilos = lista_estilos_dropdown(Convert.ToString(Convert.ToInt32(leer_ltd["id_pedido"])));
                    p.lista_familias = obtener_datos_trim_card_pdf(p.id_trim_card);
                    p.lista_generos = obtener_generos(p.gender);
                    lista.Add(p);
                }
                leer_ltd.Close();
            }
            finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public int buscar_pedido_trim_card(int trim_card){
            int temp = 0;
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT id_pedido FROM trim_card WHERE id_trim_card='" + trim_card + "' ";
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    temp = Convert.ToInt32(leer_u_i["id_pedido"]);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            return temp;
        }
        public string buscar_fold_size_trim_card(int trim_card){
            string temp = "";
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT fold_size FROM trims_fold_sizes WHERE id_trim_card='" + trim_card + "' ";
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    temp = Convert.ToString(leer_u_i["fold_size"]);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            return temp;
        }
        public List<Familias_trim_card> obtener_datos_trim_card_pdf(int id){
            List<Familias_trim_card> lista = new List<Familias_trim_card>();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT id_familia,id_imagen,especial,notas,item FROM trim_card_spec WHERE id_trim_Card='" + id + "' ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Familias_trim_card f = new Familias_trim_card();
                    f.id_family_trim = Convert.ToInt32(leer_ltd["id_familia"]);
                    f.family_trim = obtener_nombre_familia(f.id_family_trim);
                    f.id_imagen = Convert.ToInt32(leer_ltd["id_imagen"]);
                    f.imagen = "C:\\Trims\\" + obtener_imagen(f.id_imagen);
                    f.id_especial = Convert.ToInt32(leer_ltd["especial"]);
                    f.notas = Convert.ToString(leer_ltd["notas"]);
                    f.item = Convert.ToString(leer_ltd["item"]);
                    f.especial = obtener_especial(f.id_especial);
                    lista.Add(f);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public List<generos> obtener_generos(string genero){
            string[] generos = genero.Split('*');
            List<generos> lista = new List<generos>();
            for (int i = 1; i < generos.Length; i++){
                Link con_ltd = new Link();
                try{
                    SqlCommand com_ltd = new SqlCommand();
                    SqlDataReader leer_ltd = null;
                    com_ltd.Connection = con_ltd.AbrirConexion();
                    com_ltd.CommandText = "SELECT ID_GENDER,GENERO FROM CAT_GENDER WHERE ID_GENDER='" + generos[i] + "' ";
                    leer_ltd = com_ltd.ExecuteReader();
                    while (leer_ltd.Read()){
                        generos g = new generos();
                        g.id_genero = Convert.ToInt32(leer_ltd["ID_GENDER"]);
                        g.genero = Convert.ToString(leer_ltd["GENERO"]);
                        lista.Add(g);
                    }leer_ltd.Close();
                }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            }
            return lista;
        }
        public string obtener_imagen(int id){
            string temporal = "";
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT imagen FROM trims_imagenes_archivo WHERE id_imagen='" + id + "'";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temporal = Convert.ToString(leer["imagen"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temporal;
        }
        public string obtener_especial(int id){
            string temporal = "";
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT especial FROM trims_especiales WHERE id_especial='" + id + "'  ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    temporal = Convert.ToString(leer_ltd["especial"]);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return temporal;
        }
        public List<Pedidos_trim_card> obtener_trim_card(int id){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<Pedidos_trim_card> lista = new List<Pedidos_trim_card>();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT id_trim_card,id_pedido,id_customer,id_usuario,fecha,generos,ratio,tipo_empaque,comentarios,estado FROM trim_card WHERE id_trim_card='" + id + "' ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Pedidos_trim_card p = new Pedidos_trim_card();
                    p.id_pedido = Convert.ToInt32(leer_ltd["id_pedido"]);
                    p.id_trim_card = Convert.ToInt32(leer_ltd["id_trim_card"]);
                    p.pedido = consultas.obtener_po_id(Convert.ToString(leer_ltd["id_pedido"]));
                    p.customer = consultas.obtener_customer_final_id(Convert.ToString(leer_ltd["id_customer"]));
                    p.usuario = consultas.buscar_nombre_usuario(Convert.ToString(leer_ltd["id_usuario"]));
                    p.fecha = (Convert.ToDateTime(leer_ltd["fecha"])).ToString("MMM dd yyyy");
                    p.gender = Convert.ToString(leer_ltd["generos"]);
                    p.ratio = Convert.ToString(leer_ltd["ratio"]);
                    p.comentarios = Convert.ToString(leer_ltd["comentarios"]);
                    p.fold_size = buscar_fold_size_trim_card(id);
                    p.tipo_empaque = Convert.ToInt32(leer_ltd["tipo_empaque"]);
                    p.estado = Convert.ToInt32(leer_ltd["estado"]);
                    p.lista_estilos = lista_estilos_dropdown(Convert.ToString(Convert.ToInt32(leer_ltd["id_pedido"])));
                    p.lista_familias = obtener_datos_trim_card(p.id_trim_card);
                    p.lista_generos = obtener_generos(p.gender);
                    lista.Add(p);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public List<Familias_trim_card> obtener_datos_trim_card(int id){
            List<Familias_trim_card> lista = new List<Familias_trim_card>();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT id_familia,id_imagen,especial,notas,item FROM trim_card_spec WHERE id_trim_Card='" + id + "' ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Familias_trim_card f = new Familias_trim_card();
                    f.id_family_trim = Convert.ToInt32(leer_ltd["id_familia"]);
                    f.family_trim = obtener_nombre_familia(f.id_family_trim);
                    f.id_imagen = Convert.ToInt32(leer_ltd["id_imagen"]);
                    f.imagen = obtener_imagen(f.id_imagen);
                    f.id_especial = Convert.ToInt32(leer_ltd["especial"]);
                    f.notas = Convert.ToString(leer_ltd["notas"]);
                    f.item = Convert.ToString(leer_ltd["item"]);
                    f.especial = obtener_especial(f.id_especial);
                    lista.Add(f);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public void editar_trim_card(int trim_card, string tipo_empaque, string ratio,string comentarios,string estado){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "UPDATE trim_card SET tipo_empaque='" + tipo_empaque + "',ratio='" + ratio + "', " +
                    "fecha='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',comentarios='" + comentarios + "',estado='" + estado + "' " +
                    " WHERE id_trim_card='" + trim_card + "' ";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void borrar_trim_card_spec(int trim_card){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "Delete from trim_card_spec WHERE id_trim_card='" + trim_card + "' ";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public List<Inventario> obtener_lista_trims_inventario_stock_bodega(){
            List<Inventario> lista = new List<Inventario>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT  id_location,id_item,id_inventario,id_pedido,total,id_customer,id_family_trim,descripcion,id_estilo,id_sucursal FROM inventario " +
                    " WHERE id_categoria_inventario=2 and id_pedido=0 and id_location=72 and id_sucursal=1 ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Inventario i = new Inventario();
                    i.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    i.id_item = Convert.ToInt32(leer["id_item"]);
                    i.id_location = Convert.ToInt32(leer["id_location"]);
                    i.id_pedido = Convert.ToInt32(leer["id_pedido"]);
                    i.id_sucursal = Convert.ToInt32(leer["id_sucursal"]);
                    i.sucursal = consultas.obtener_sucursal_id(Convert.ToString(leer["id_sucursal"]));
                    i.po = consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    i.id_estilo = Convert.ToInt32(leer["id_estilo"]);
                    i.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"])) + " " + consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    i.total = Convert.ToInt32(leer["total"]);
                    i.descripcion = Convert.ToString(leer["descripcion"]);
                    i.id_customer = Convert.ToInt32(leer["id_customer"]);
                    i.customer = consultas.obtener_customer_id(Convert.ToString(i.id_customer));
                    i.family_trim = consultas.obtener_family_id(Convert.ToString(leer["id_family_trim"]));
                    //string familia = buscar_familia_item_request(Convert.ToString(tr.id_request));
                    /*string familia = (i.family_trim).Trim();
                    if (familia == "BOXES" || familia == "POLYBAG" || familia == "MASTER BAG" || familia == "BAG" || familia == "GARMENTBAG" || familia == "TAPE" || familia == "HANGERS" || familia == "HANGTAGS" || familia == "SIZE STRIP" || familia == "SIZE STRIP" || familia == "")
                    {
                        lista.Add(i);
                    }*/
                    lista.Add(i);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Inventario> obtener_lista_trims_inventario_stock(){
            List<Inventario> lista = new List<Inventario>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT  id_location,id_sucursal,id_item,id_inventario,id_pedido,total,id_customer,id_family_trim,descripcion,id_estilo FROM inventario " +
                    " WHERE id_categoria_inventario=2 and id_pedido=0 ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Inventario i = new Inventario();
                    i.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    i.id_location = Convert.ToInt32(leer["id_location"]);
                    i.id_item = Convert.ToInt32(leer["id_item"]);
                    i.id_pedido = Convert.ToInt32(leer["id_pedido"]);
                    i.id_sucursal = Convert.ToInt32(leer["id_sucursal"]);
                    i.sucursal = consultas.obtener_sucursal_id(Convert.ToString(leer["id_sucursal"]));
                    i.po = consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    i.id_estilo = Convert.ToInt32(leer["id_estilo"]);
                    i.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"])) + " " + consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    i.total = Convert.ToInt32(leer["total"]);
                    i.descripcion = Convert.ToString(leer["descripcion"]);
                    i.id_customer = Convert.ToInt32(leer["id_customer"]);
                    i.customer = consultas.obtener_customer_id(Convert.ToString(i.id_customer));
                    i.family_trim = consultas.obtener_family_id(Convert.ToString(leer["id_family_trim"]));
                    lista.Add(i);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Inventario> obtener_lista_trims_inventario_familia_bodega(string busqueda){
            List<Inventario> lista = new List<Inventario>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT  i.id_location,i.id_item,i.id_inventario,i.id_pedido,i.total,i.id_customer,i.id_family_trim,i.descripcion,i.id_estilo,i.id_sucursal " +
                    " FROM inventario i, family_trims ft WHERE ft.id_family_trim=i.id_family_trim AND " +
                    "  id_categoria_inventario=2 AND ft.family_trim like'%" + busqueda + "%' AND i.id_pedido=0  and id_location=72 and i.id_sucursal=1  ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Inventario i = new Inventario();
                    i.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    i.id_item = Convert.ToInt32(leer["id_item"]);
                    i.id_pedido = Convert.ToInt32(leer["id_pedido"]);
                    i.id_sucursal = Convert.ToInt32(leer["id_sucursal"]);
                    i.sucursal = consultas.obtener_sucursal_id(Convert.ToString(leer["id_sucursal"]));
                    i.po = consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    i.id_estilo = Convert.ToInt32(leer["id_estilo"]);
                    i.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"])) + " " + consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    i.total = Convert.ToInt32(leer["total"]);
                    i.descripcion = Convert.ToString(leer["descripcion"]);
                    i.id_customer = Convert.ToInt32(leer["id_customer"]);
                    i.customer = consultas.obtener_customer_id(Convert.ToString(i.id_customer));
                    i.family_trim = consultas.obtener_family_id(Convert.ToString(leer["id_family_trim"]));
                    i.id_location = Convert.ToInt32(leer["id_location"]);
                    //string familia = (i.family_trim).Trim();
                    /*if (familia == "BOXES" || familia == "POLYBAG" || familia == "MASTER BAG" || familia == "BAG" || familia == "GARMENTBAG" || familia == "TAPE" || familia == "HANGERS" || familia == "HANGTAGS")
                    {
                        lista.Add(i);
                    }*/
                    lista.Add(i);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Inventario> obtener_lista_trims_inventario_descripcion_bodega(string busqueda){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<Inventario> lista = new List<Inventario>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT  id_location,id_sucursal,id_item,id_inventario,id_pedido,total,id_customer,id_family_trim,descripcion,id_estilo FROM inventario " +
                    " WHERE id_categoria_inventario=2 AND descripcion like'%" + busqueda + "%'  AND id_pedido=0  and id_location=72  and id_sucursal=1";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Inventario i = new Inventario();
                    i.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    i.id_item = Convert.ToInt32(leer["id_item"]);
                    i.id_location = Convert.ToInt32(leer["id_location"]);
                    i.id_pedido = Convert.ToInt32(leer["id_pedido"]);
                    i.id_sucursal = Convert.ToInt32(leer["id_sucursal"]);
                    i.sucursal = consultas.obtener_sucursal_id(Convert.ToString(leer["id_sucursal"]));
                    i.po = consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    i.id_estilo = Convert.ToInt32(leer["id_estilo"]);
                    i.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"])) + " " + consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    i.total = Convert.ToInt32(leer["total"]);
                    i.descripcion = Convert.ToString(leer["descripcion"]);
                    i.id_customer = Convert.ToInt32(leer["id_customer"]);
                    i.customer = consultas.obtener_customer_id(Convert.ToString(i.id_customer));
                    i.family_trim = consultas.obtener_family_id(Convert.ToString(leer["id_family_trim"]));                    
                    lista.Add(i);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Inventario> obtener_lista_trims_inventario_familia(string busqueda){
            List<Inventario> lista = new List<Inventario>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT  i.id_location,i.id_sucursal,i.id_item,i.id_inventario,i.id_pedido,i.total,i.id_customer,i.id_family_trim,i.descripcion,i.id_estilo " +
                    " FROM inventario i, family_trims ft WHERE ft.id_family_trim=i.id_family_trim AND " +
                    "  id_categoria_inventario=2 AND ft.family_trim like'%" + busqueda + "%' AND i.id_pedido=0 ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Inventario i = new Inventario();
                    i.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    i.id_item = Convert.ToInt32(leer["id_item"]);
                    i.id_location = Convert.ToInt32(leer["id_location"]);
                    i.id_pedido = Convert.ToInt32(leer["id_pedido"]);
                    i.id_sucursal = Convert.ToInt32(leer["id_sucursal"]);
                    i.sucursal = consultas.obtener_sucursal_id(Convert.ToString(leer["id_sucursal"]));
                    i.po = consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    i.id_estilo = Convert.ToInt32(leer["id_estilo"]);
                    i.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"])) + " " + consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    i.total = Convert.ToInt32(leer["total"]);
                    i.descripcion = Convert.ToString(leer["descripcion"]);
                    i.id_customer = Convert.ToInt32(leer["id_customer"]);
                    i.customer = consultas.obtener_customer_id(Convert.ToString(i.id_customer));
                    i.family_trim = consultas.obtener_family_id(Convert.ToString(leer["id_family_trim"]));
                    lista.Add(i);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Inventario> obtener_lista_trims_inventario_descripcion(string busqueda){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<Inventario> lista = new List<Inventario>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT  id_location,id_sucursal,id_item,id_inventario,id_pedido,total,id_customer,id_family_trim,descripcion,id_estilo FROM inventario " +
                    " WHERE id_categoria_inventario=2 AND descripcion like'%" + busqueda + "%'  AND id_pedido=0 ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Inventario i = new Inventario();
                    i.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    i.id_item = Convert.ToInt32(leer["id_item"]);
                    i.id_location = Convert.ToInt32(leer["id_location"]);
                    i.id_pedido = Convert.ToInt32(leer["id_pedido"]);
                    i.id_sucursal = Convert.ToInt32(leer["id_sucursal"]);
                    i.sucursal = consultas.obtener_sucursal_id(Convert.ToString(leer["id_sucursal"]));
                    i.po = consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    i.id_estilo = Convert.ToInt32(leer["id_estilo"]);
                    i.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"])) + " " + consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    i.total = Convert.ToInt32(leer["total"]);
                    i.descripcion = Convert.ToString(leer["descripcion"]);
                    i.id_customer = Convert.ToInt32(leer["id_customer"]);
                    i.customer = consultas.obtener_customer_id(Convert.ToString(i.id_customer));
                    i.family_trim = consultas.obtener_family_id(Convert.ToString(leer["id_family_trim"]));
                    lista.Add(i);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public void actualizar_cantidad_inventario(int inventario, int cantidad){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE inventario SET total='" + cantidad + "' WHERE id_inventario='" + inventario + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public List<Trim_requests> obtener_stock_para_orden(string pedido, List<Trim_requests> requests, int customer, int sucursal){
            List<Trim_requests> lista = new List<Trim_requests>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            foreach (Trim_requests r in requests){
                if (r.auditado == 0){
                    Link con_ltd = new Link();
                    try{
                        SqlCommand com_ltd = new SqlCommand();
                        SqlDataReader leer_ltd = null;
                        com_ltd.Connection = con_ltd.AbrirConexion();
                        com_ltd.CommandText = "SELECT id_sucursal,id_location,id_inventario,total,id_item FROM inventario where id_item='" + r.id_item + "' and " +
                            " id_pedido=0  AND id_customer=" + customer + " AND id_sucursal=" + sucursal + " AND id_location=72"; //AND id_location=72
                        leer_ltd = com_ltd.ExecuteReader();
                        while (leer_ltd.Read()){
                            Trim_requests tr = new Trim_requests();
                            tr.id_request = r.id_request;
                            tr.id_inventario = Convert.ToInt32(leer_ltd["id_inventario"]);
                            tr.id_sucursal = Convert.ToInt32(leer_ltd["id_sucursal"]);
                            tr.id_location = Convert.ToInt32(leer_ltd["id_location"]);
                            tr.id_item = Convert.ToInt32(leer_ltd["id_item"]);
                            tr.item = consultas.buscar_descripcion_item(Convert.ToString(tr.id_item));//consultas.buscar_amt_item(Convert.ToString(tr.id_item)) + " " +
                            tr.total = Convert.ToInt32(leer_ltd["total"]);//INVENTARIO
                            tr.cantidad = r.total;//ORDEN
                            tr.restante = obtener_total_trim_recibido_pedido(Convert.ToInt32(pedido), tr.id_item); //INVENTARIO DE LA ORDEN
                            tr.restante = tr.cantidad - tr.restante;
                            lista.Add(tr);
                        }leer_ltd.Close();
                    }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
                }//IF
            }//FOREACH
            return lista;
        }
        public List<Inventario> obtener_trim_stock_sucursal_ubicacion(int sucursal, int location){
            List<Inventario> lista = new List<Inventario>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT  id_location,id_sucursal,id_item,id_inventario,id_pedido,total,id_customer,id_family_trim,descripcion,id_estilo FROM inventario " +
                    " WHERE id_categoria_inventario=2 AND id_pedido=0 AND id_location=" + location + " AND id_sucursal=" + sucursal;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Inventario i = new Inventario();
                    i.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    i.id_location = Convert.ToInt32(leer["id_location"]);
                    if (i.id_location == 72){
                        i.location = "BODEGA";
                    }
                    if (i.id_location == 73){
                        i.location = "TRIM OFFICE";
                    }
                    i.id_item = Convert.ToInt32(leer["id_item"]);
                    i.id_pedido = Convert.ToInt32(leer["id_pedido"]);
                    i.id_sucursal = Convert.ToInt32(leer["id_sucursal"]);
                    i.sucursal = consultas.obtener_sucursal_id(Convert.ToString(leer["id_sucursal"]));
                    //i.po = consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    //i.id_estilo = Convert.ToInt32(leer["id_estilo"]);
                    //i.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"])) + " " + consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    i.total = Convert.ToInt32(leer["total"]);
                    i.descripcion = Convert.ToString(leer["descripcion"]);
                    /*i.id_customer = Convert.ToInt32(leer["id_customer"]);
                    i.customer = consultas.obtener_customer_id(Convert.ToString(i.id_customer));
                    i.family_trim = consultas.obtener_family_id(Convert.ToString(leer["id_family_trim"]));*/
                    lista.Add(i);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public void guardar_transferencia_trim(int usuario){
            Link conTrim = new Link();
            try{
                SqlCommand comTrim = new SqlCommand();
                comTrim.Connection = conTrim.AbrirConexion();
                comTrim.CommandText = "INSERT INTO trims_transferencias(id_usuario,fecha) values ('" + usuario + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                comTrim.ExecuteNonQuery();
            }finally { conTrim.CerrarConexion(); conTrim.Dispose(); }
        }
        public Inventario obtener_item_editar(int id){
            Inventario i = new Inventario();
            Link con_oie = new Link();
            try{
                SqlCommand com_oie = new SqlCommand();
                SqlDataReader leer_oie = null;
                com_oie.Connection = con_oie.AbrirConexion();
                com_oie.CommandText = "SELECT id_estilo,id_inventario,id_pedido,id_pais,id_fabricante,id_categoria_inventario,id_color,id_body_type,id_genero,id_fabric_type," +
                    "id_location,total,id_size,id_customer,id_customer_final,minimo,notas,id_fabric_percent,date_comment,comment,id_family_trim,id_unit,id_trim," +
                    "descripcion,auditoria,id_summary,id_item FROM inventario WHERE id_inventario='" + id + "' ";
                leer_oie = com_oie.ExecuteReader();
                while (leer_oie.Read()){
                    i.id_inventario = Convert.ToInt32(leer_oie["id_inventario"]);
                    i.id_pedido = Convert.ToInt32(leer_oie["id_pedido"]);
                    i.id_categoria_inventario = Convert.ToInt32(leer_oie["id_categoria_inventario"]);
                    i.minimo = Convert.ToInt32(leer_oie["minimo"]);
                    i.notas = Convert.ToString(leer_oie["notas"]);
                    i.id_pais = Convert.ToInt32(leer_oie["id_pais"]);
                    i.id_fabricante = Convert.ToInt32(leer_oie["id_fabricante"]);
                    i.id_color = Convert.ToInt32(leer_oie["id_color"]);
                    i.id_body_type = Convert.ToInt32(leer_oie["id_body_type"]);
                    i.id_genero = Convert.ToInt32(leer_oie["id_genero"]);
                    i.id_fabric_type = Convert.ToInt32(leer_oie["id_fabric_type"]);
                    i.id_location = Convert.ToInt32(leer_oie["id_location"]);
                    i.total = Convert.ToInt32(leer_oie["total"]);
                    i.id_size = Convert.ToInt32(leer_oie["id_size"]);
                    i.id_customer = Convert.ToInt32(leer_oie["id_customer"]);
                    i.id_final_customer = Convert.ToInt32(leer_oie["id_customer_final"]);
                    i.id_fabric_percent = Convert.ToInt32(leer_oie["id_fabric_percent"]);
                    i.comment = Convert.ToString(leer_oie["comment"]);
                    i.id_family_trim = Convert.ToString(leer_oie["id_family_trim"]);
                    i.id_unit = Convert.ToString(leer_oie["id_unit"]);
                    i.descripcion = Convert.ToString(leer_oie["descripcion"]);
                    i.id_trim = Convert.ToInt32(leer_oie["id_trim"]);
                    i.id_estilo = Convert.ToInt32(leer_oie["id_estilo"]);
                    i.auditoria = Convert.ToInt32(leer_oie["auditoria"]);
                    i.id_summary = Convert.ToInt32(leer_oie["id_summary"]);
                    i.id_item = Convert.ToInt32(leer_oie["id_item"]);
                }leer_oie.Close();
            }finally { con_oie.CerrarConexion(); con_oie.Dispose(); }
            return i;
        }
        public int obtener_ultima_transferencia(){
            int temp = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT TOP 1 id_transferencia FROM trims_transferencias ORDER BY id_transferencia DESC ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temp = Convert.ToInt32(leer["id_transferencia"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }
        public int buscar_family_trim_inventario(int inventario){
            int temp = 0;
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT id_family_trim FROM inventario WHERE id_inventario='" + inventario + "'  ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    temp = Convert.ToInt32(leer_ltd["id_family_trim"]);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return temp;
        }
        public void guardar_transferencia_trim_item(int transferencia, int item, int total, int ubicacion, int sucursal,string pedido,string tipo){
            Link conTrim = new Link();
            try{
                SqlCommand comTrim = new SqlCommand();
                comTrim.Connection = conTrim.AbrirConexion();
                comTrim.CommandText = "INSERT INTO trims_transferencias_items(id_transferencia,id_item,total,id_ubicacion,id_sucursal,id_pedido,stock_orden) " +
                " values ('" + transferencia + "','" + item + "','" + total + "','" + ubicacion + "','" + sucursal + "','" + pedido + "','" + tipo + "') ";
                comTrim.ExecuteNonQuery();
            }finally { conTrim.CerrarConexion(); conTrim.Dispose(); }
        }
        public List<Transferencia_trim> obtener_transferencias(string inicio, string final){
            List<Transferencia_trim> lista = new List<Transferencia_trim>();
            FuncionesInventarioGeneral consulta = new FuncionesInventarioGeneral();
            string query = "";
            if (inicio == "0" && final == "0"){
                query = "SELECT TOP 30 id_transferencia,id_usuario,fecha FROM trims_transferencias ORDER BY id_transferencia DESC";
            }else{
                query = "SELECT TOP 30 id_transferencia,id_usuario,fecha FROM trims_transferencias " +
                    " WHERE fecha BETWEEN '" + inicio + "' and '" + final + " 23:59:59' ORDER BY id_transferencia DESC";
            }Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = query;
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Transferencia_trim tt = new Transferencia_trim();
                    tt.id_usuario = Convert.ToInt32(leer_ltd["id_usuario"]);
                    tt.usuario = consulta.buscar_nombre_usuario((tt.id_usuario).ToString());
                    tt.id_transferencia = Convert.ToInt32(leer_ltd["id_transferencia"]);
                    tt.fecha = (Convert.ToDateTime(leer_ltd["fecha"])).ToString("MM/dd/yyyy");
                    tt.lista_items = buscar_items_transferencias(tt.id_transferencia);
                    lista.Add(tt);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public List<Transferencia_item> buscar_items_transferencias(int transfer){
            List<Transferencia_item> lista = new List<Transferencia_item>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT TOP 30 id_transferencia_item,id_item,total,id_ubicacion,id_sucursal,id_pedido,stock_orden FROM trims_transferencias_items WHERE id_transferencia=" + transfer;
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Transferencia_item ti = new Transferencia_item();
                    ti.id_transferencia = transfer;
                    ti.id_transferencia_item = Convert.ToInt32(leer_ltd["id_transferencia_item"]);
                    ti.total = Convert.ToInt32(leer_ltd["total"]);
                    ti.id_ubicacion = Convert.ToInt32(leer_ltd["id_ubicacion"]);
                    ti.id_sucursal = Convert.ToInt32(leer_ltd["id_sucursal"]);
                    ti.id_item = Convert.ToInt32(leer_ltd["id_item"]);
                    ti.id_pedido = Convert.ToInt32(leer_ltd["id_pedido"]);
                    if (ti.id_pedido == 0){
                        ti.pedido = " ";
                    }else {
                        ti.pedido = consultas.obtener_po_id(Convert.ToString(leer_ltd["id_pedido"]));
                    }
                    if (Convert.ToString(leer_ltd["stock_orden"])=="0") {
                        ti.tipo = " ";
                    } else {
                        if (Convert.ToString(leer_ltd["stock_orden"]) == "1"){
                            ti.tipo = "STOCK-ORDER";
                        }else{
                            ti.tipo = "ORDER-STOCK";
                        }
                    }
                    ti.item = consultas.buscar_descripcion_item(Convert.ToString(ti.id_item));
                    if (ti.id_ubicacion == 72) {
                        ti.ubicacion = "BODEGA";
                    } else {
                        if (ti.id_ubicacion == 77){
                            ti.ubicacion = "STOCK IMPRESIONES";
                        }else{
                            ti.ubicacion = "TRIMS OFFICE";
                        }
                    }
                    if (ti.id_sucursal == 1) { ti.sucursal = "FORTUNE"; } else { if (ti.id_sucursal == 2) { ti.sucursal = "LUCKY"; } else { ti.sucursal = "LUCKY 2"; } }
                    lista.Add(ti);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public List<Inventario> lista_reporte_stock(int sucursal, int locacion){
            List<Inventario> lista = new List<Inventario>();
            List<int> lista_items = new List<int>();
            string query = "";
            if (sucursal == 0 && locacion == 0){
                //query = "SELECT DISTINCT id_item FROM inventario WHERE id_categoria_inventario=2 AND id_pedido=0 ";
                query = "SELECT  id_item FROM inventario WHERE id_categoria_inventario=2 AND id_pedido=0 ORDER by descripcion ";
                lista_items = buscar_items_id_stock(query);
                lista = obtener_inventario_stock(lista_items, "SELECT SUM(total) as 'totalillo' FROM inventario WHERE id_categoria_inventario=2 AND id_pedido=0 AND id_item=");
            }
            if (sucursal == 1 && locacion == 0){
                query = "SELECT  id_item FROM inventario WHERE id_categoria_inventario=2 AND  id_pedido=0 AND id_sucursal=1 ORDER by descripcion ";
                lista_items = buscar_items_id_stock(query);
                lista = obtener_inventario_stock(lista_items, "SELECT SUM(total) as 'totalillo' FROM inventario WHERE id_categoria_inventario=2 AND id_pedido=0 AND id_sucursal=1 AND id_item=");
            }
            if (sucursal == 1 && locacion == 72){
                query = "SELECT  id_item FROM inventario WHERE id_categoria_inventario=2 AND  id_pedido=0 AND id_sucursal=1 AND id_location=72 ORDER by descripcion ";
                lista_items = buscar_items_id_stock(query);
                lista = obtener_inventario_stock(lista_items, "SELECT SUM(total) as 'totalillo' FROM  inventario WHERE id_categoria_inventario=2 AND id_pedido=0 AND id_sucursal=1 AND id_location=72 AND id_item=");
            }
            if (sucursal == 1 && locacion == 73){
                query = "SELECT  id_item FROM inventario WHERE id_categoria_inventario=2 AND id_pedido=0 AND id_sucursal=1 AND id_location=73 ORDER by descripcion ";
                lista_items = buscar_items_id_stock(query);
                lista = obtener_inventario_stock(lista_items, "SELECT SUM(total) as 'totalillo' FROM inventario WHERE id_categoria_inventario=2 AND  id_pedido=0 AND id_sucursal=1 AND id_location=73 AND id_item=");
            }
            if (sucursal == 2){
                query = "SELECT  id_item FROM inventario WHERE id_categoria_inventario=2 AND  id_pedido=0 AND id_sucursal=2 AND id_location=72 ORDER by descripcion";
                lista_items = buscar_items_id_stock(query);
                lista = obtener_inventario_stock(lista_items, "SELECT SUM(total) as 'totalillo' FROM inventario WHERE id_categoria_inventario=2 AND id_pedido=0 AND id_sucursal=2 AND id_location=72 AND id_item=");
            }
            if (sucursal == 3){
                query = "SELECT  id_item FROM inventario WHERE id_categoria_inventario=2 AND  id_pedido=0 AND id_sucursal=3 AND id_location=72 ORDER by descripcion";
                lista_items = buscar_items_id_stock(query);
                lista = obtener_inventario_stock(lista_items, "SELECT SUM(total) as 'totalillo' FROM inventario WHERE id_categoria_inventario=2 AND  id_pedido=0 AND id_sucursal=3 AND id_location=72 AND id_item=");
            }
            return lista;
        }
        public List<int> buscar_items_id_stock(string query){
            List<int> lista_items = new List<int>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = query;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    lista_items.Add(Convert.ToInt32(leer["id_item"]));
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            lista_items = lista_items.Distinct().ToList();
            return lista_items;
        }
        public List<Inventario> obtener_inventario_stock(List<int> lista_ids, string query){
            List<Inventario> lista = new List<Inventario>();
            foreach (int ii in lista_ids){
                Link con_ltd = new Link();
                try{
                    SqlCommand com_ltd = new SqlCommand();
                    SqlDataReader leer_ltd = null;
                    com_ltd.Connection = con_ltd.AbrirConexion();
                    com_ltd.CommandText = query + ii;
                    leer_ltd = com_ltd.ExecuteReader();
                    while (leer_ltd.Read()){
                        Inventario i = new Inventario();
                        i.id_inventario = ii;
                        i.total = Convert.ToInt32(leer_ltd["totalillo"]);
                        i.item = obtener_componente_item(ii);//AMT
                        i.descripcion = obtener_descripcion_item(ii);//AMT
                        i.family_trim = obtener_family_trim_item((ii).ToString());
                        lista.Add(i);
                    }leer_ltd.Close();
                }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            }
            return lista;
        }
        public string obtener_componente_item(int item){
            string lista = "";
            Link con_led = new Link();
            try{
                SqlCommand com_led = new SqlCommand();
                SqlDataReader leer_led = null;
                com_led.Connection = con_led.AbrirConexion();
                com_led.CommandText = " SELECT item FROM items_catalogue where item_id='" + item + "' ";
                leer_led = com_led.ExecuteReader();
                while (leer_led.Read()){
                    lista = Convert.ToString(leer_led["item"]);
                }leer_led.Close();
            }finally { con_led.CerrarConexion(); con_led.Dispose(); }
            return lista;
        }
        public List<clientes> lista_clientes(){
            List<clientes> lista = new List<clientes>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT CUSTOMER,NAME from CAT_CUSTOMER  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    clientes c = new clientes();
                    c.id_customer = Convert.ToInt32(leerFilas["CUSTOMER"]);
                    c.customer = leerFilas["NAME"].ToString();
                    lista.Add(c);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<Pedido_t> lista_ordenes_todas(){
            List<Pedido_t> listar = new List<Pedido_t>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT ID_PEDIDO,PO from PEDIDO ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    Pedido_t l = new Pedido_t();
                    l.id_pedido = Convert.ToInt32(leerFilas["ID_PEDIDO"]);
                    l.pedido = leerFilas["PO"].ToString();
                    listar.Add(l);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return listar;
        }
        public void guardar_stock_inventario(string pedido,string item,string total){
            int temporal = buscar_stock_inventario(pedido,item);
            if (temporal==0) {
                insertar_stock_orden(pedido,item,total);
            } else {
                sumar_stock_orden(pedido,item,total);
            }
        }
        public int buscar_stock_inventario(string pedido,string item){
            int temporal = 0;
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_stock FROM stock_ordenes WHERE id_pedido="+pedido+" AND id_item="+item;
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    temporal = Convert.ToInt32(leerFilas["id_stock"]);                    
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return temporal;
        }        
        public void insertar_stock_orden(string pedido, string item, string total) {
            Link conTrim = new Link();
            try{
                SqlCommand comTrim = new SqlCommand();
                comTrim.Connection = conTrim.AbrirConexion();
                comTrim.CommandText = "INSERT INTO stock_ordenes(id_pedido,id_item,total) values ('" + pedido + "','" + item + "','" + total + "')";
                comTrim.ExecuteNonQuery();
            }finally { conTrim.CerrarConexion(); conTrim.Dispose(); }
        }
        public void sumar_stock_orden(string pedido, string item, string total){
            Link conTrim = new Link();
            try{
                SqlCommand comTrim = new SqlCommand();
                comTrim.Connection = conTrim.AbrirConexion();
                comTrim.CommandText = "UPDATE stock_ordenes SET total=total+" + total + " WHERE id_pedido=" + pedido + " AND id_item=" + item ;
                comTrim.ExecuteNonQuery();
            }finally { conTrim.CerrarConexion(); conTrim.Dispose(); }
        }
        public void restar_stock_orden(string pedido, string item, string total){
            Link conTrim = new Link();
            try{
                SqlCommand comTrim = new SqlCommand();
                comTrim.Connection = conTrim.AbrirConexion();
                comTrim.CommandText = "UPDATE stock_ordenes SET total=total-" + total + " WHERE id_pedido=" + pedido + " AND id_item=" + item+" AND total>=1";
                comTrim.ExecuteNonQuery();
            }finally { conTrim.CerrarConexion(); conTrim.Dispose(); }
        }
        public List<Inventario> obtener_stock_orden(string orden){
            List<Inventario> lista = new List<Inventario>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT id_pedido,id_item,total FROM stock_ordenes WHERE id_pedido="+orden;
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Inventario i = new Inventario();
                    i.id_pedido= Convert.ToInt32(leer_ltd["id_pedido"]); ;
                    i.total = Convert.ToInt32(leer_ltd["total"]);
                    i.id_item = Convert.ToInt32(leer_ltd["id_item"]);
                    i.item = obtener_componente_item(i.id_item);//AMT
                    i.po = consultas.obtener_po_id(Convert.ToString(leer_ltd["id_pedido"]));
                    i.descripcion = obtener_descripcion_item(i.id_item);//AMT
                    i.family_trim = obtener_family_trim_item((i.id_item).ToString());
                    lista.Add(i);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public int obtener_familias_item_id(int item){
            int temporal = 0;
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT ft.id_family_trim FROM family_trims ft,items_catalogue ic WHERE " +
                "  ic.item_id='" + item+ "' AND ic.fabric_type=ft.family_trim ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    temporal=Convert.ToInt32(leer_ltd["id_family_trim"]);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return temporal;
        }
        public List<Trim_orden> obtener_trims_auditados_orden(string pedido){
            List<Trim_orden> lista = new List<Trim_orden>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT id_trim_orden,id_item,id_pedido,id_summary,id_talla,id_sucursal,id_usuario,total,fecha,id_request " +
                    " FROM trims_ordenes WHERE id_pedido='" + pedido + "'";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Trim_orden to = new Trim_orden();
                    to.id_trim_order = Convert.ToInt32(leer_ltd["id_trim_orden"]);
                    to.id_item = Convert.ToInt32(leer_ltd["id_item"]);
                    to.id_pedido = Convert.ToInt32(leer_ltd["id_pedido"]);
                    to.item = consultas.buscar_descripcion_item(Convert.ToString(to.id_item));
                    to.id_summary = Convert.ToInt32(leer_ltd["id_summary"]);
                    int id_estilo = consultas.obtener_estilo_summary(to.id_summary);
                    to.estilo = consultas.buscar_descripcion_estilo(id_estilo);
                    to.id_talla = Convert.ToInt32(leer_ltd["id_talla"]);
                    to.talla = consultas.obtener_size_id(Convert.ToString(to.id_talla));
                    //to.id_sucursal = Convert.ToInt32(leer_ltd["id_sucursal"]);
                    //to.sucursal = consultas.obtener_sucursal_id(Convert.ToString(to.id_sucursal));                    
                    to.total = Convert.ToInt32(leer_ltd["total"]);
                    to.fecha = Convert.ToDateTime(leer_ltd["fecha"]).ToString("dd-MM-yyyy");
                    to.id_usuario = Convert.ToInt32(leer_ltd["id_usuario"]);
                    to.usuario = consultas.buscar_nombre_usuario(Convert.ToString(to.id_usuario));
                    to.id_request = Convert.ToInt32(leer_ltd["id_request"]);
                    to.anterior = obtener_total_entregado_anteriormente(to.id_trim_order, 0,0);
                    to.restante = to.total - to.anterior;
                    lista.Add(to);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public void insertar_trim_orden(string item, string pedido, string estilo,string talla,string sucursal,string locacion,string usuario,string total,string fecha,string request,string po){
            Link conTrim = new Link();
            try{
                SqlCommand comTrim = new SqlCommand();
                comTrim.Connection = conTrim.AbrirConexion();
                comTrim.CommandText = "INSERT INTO trims_ordenes(id_item,id_pedido,id_summary,id_talla,id_sucursal,id_locacion,id_usuario,total,fecha,id_request,po) " +
                    " values ('" + item + "','" + pedido + "','" + estilo + "','" + talla + "','" + sucursal + "','" + locacion + "','" + usuario + "','" + total + "','" + fecha + "','" + request + "','" + po + "')";
                comTrim.ExecuteNonQuery();
            }finally { conTrim.CerrarConexion(); conTrim.Dispose(); }
        }
        public Trim_orden obtener_trim_auditado_orden(string id){            
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Trim_orden to = new Trim_orden();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT id_trim_orden,id_item,id_pedido,id_summary,id_talla,id_sucursal,id_locacion,id_usuario,total,fecha,id_request,po " +
                    " FROM trims_ordenes WHERE id_trim_orden='" + id+ "'";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){                    
                    to.id_trim_order = Convert.ToInt32(leer_ltd["id_trim_orden"]);
                    to.id_item = Convert.ToInt32(leer_ltd["id_item"]);
                    to.id_pedido = Convert.ToInt32(leer_ltd["id_pedido"]);
                    to.item = consultas.buscar_descripcion_item(Convert.ToString(to.id_item));
                    to.id_summary = Convert.ToInt32(leer_ltd["id_summary"]);
                    int id_estilo = consultas.obtener_estilo_summary(to.id_summary);
                    to.estilo = consultas.buscar_descripcion_estilo(id_estilo);
                    to.id_talla = Convert.ToInt32(leer_ltd["id_talla"]);
                    to.talla = consultas.obtener_size_id(Convert.ToString(to.id_talla));
                    to.id_sucursal = Convert.ToInt32(leer_ltd["id_sucursal"]);
                    to.sucursal = consultas.obtener_sucursal_id(Convert.ToString(to.id_sucursal));
                    to.id_locacion = Convert.ToInt32(leer_ltd["id_locacion"]);
                    to.locacion = consultas.obtener_ubicacion_id(Convert.ToString(to.id_locacion));
                    to.total = Convert.ToInt32(leer_ltd["total"]);
                    to.fecha = Convert.ToDateTime(leer_ltd["fecha"]).ToString("dd-MM-yyyy");
                    to.fecha_completa = Convert.ToString(leer_ltd["fecha"]);
                    to.id_usuario = Convert.ToInt32(leer_ltd["id_usuario"]);
                    to.usuario = consultas.buscar_nombre_usuario(Convert.ToString(to.id_usuario));
                    to.id_request = Convert.ToInt32(leer_ltd["id_request"]);
                    to.po = Convert.ToString(leer_ltd["po"]);
                }
                leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return to;
        }
        public void eliminar_trim_auditado(string id){
            Link conTrim = new Link();
            try{
                SqlCommand comTrim = new SqlCommand();
                comTrim.Connection = conTrim.AbrirConexion();
                comTrim.CommandText = "DELETE FROM trims_ordenes WHERE id_trim_orden=" + id;
                comTrim.ExecuteNonQuery();
            }finally { conTrim.CerrarConexion(); conTrim.Dispose(); }
        }
        public List<int> obtener_po_ht(string pedido){
            List<int> lista = new List<int>();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT distinct PTS.NUMBER_PO FROM PACKING_TYPE_SIZE PTS JOIN PO_SUMMARY PS ON PTS.ID_SUMMARY=PS.ID_PO_SUMMARY WHERE PS.ID_PEDIDOS=" + pedido ;
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    lista.Add(Convert.ToInt32(leer_ltd["NUMBER_PO"]));
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public List<Pedido_t> lista_ordenes_dos_semanas(){
            List<Pedido_t> listar = new List<Pedido_t>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT ID_PEDIDO,PO from PEDIDO where DATE_CANCEL BETWEEN GETDATE() AND DATEADD(DAY, +14, GETDATE()) order by DATE_CANCEL ASC";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    Pedido_t l = new Pedido_t();
                    l.id_pedido = Convert.ToInt32(leerFilas["ID_PEDIDO"]);
                    l.pedido = leerFilas["PO"].ToString();
                    listar.Add(l);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return listar;
        }
        public List<Trim_orden> obtener_trims_entrega_pedido_almacen(string pedido){
            List<Trim_orden> lista = new List<Trim_orden>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT id_request,id_item,total,id_pedido FROM trim_requests WHERE id_pedido=" + pedido;
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Trim_orden to = new Trim_orden();
                    to.id_trim_order = 0;
                    to.id_request = Convert.ToInt32(leer_ltd["id_request"]);
                    to.id_item = Convert.ToInt32(leer_ltd["id_item"]);
                    to.id_pedido = Convert.ToInt32(leer_ltd["id_pedido"]);
                    to.item = consultas.buscar_descripcion_item(Convert.ToString(to.id_item));
                    to.id_summary = 0;
                    //int id_estilo = consultas.obtener_estilo_summary(to.id_summary);
                    to.estilo = "0";
                    to.id_talla = 0;
                    to.talla = "0";
                    to.id_sucursal = 0;
                    to.sucursal = "0";
                    to.id_locacion = 0;
                    to.locacion = "0";
                    to.total = Convert.ToInt32(leer_ltd["total"]);
                    to.total = to.total - obtener_total_entregado_anteriormente(0, to.id_request, 0);
                    //to.fecha = Convert.ToDateTime(leer_ltd["fecha"]).ToString("dd-MM-yyyy");
                    //to.id_usuario = Convert.ToInt32(leer_ltd["id_usuario"]);
                    //to.usuario = consultas.buscar_nombre_usuario(Convert.ToString(to.id_usuario));
                    //to.id_request = Convert.ToInt32(leer_ltd["id_request"]);
                    //to.po = Convert.ToString(leer_ltd["po"]);
                    to.familia = consultas.buscar_tipo_trim_item(Convert.ToString(to.id_item));
                    if (to.familia == "BOXES" || to.familia == "POLYBAG" || to.familia == "MASTER BAG" || to.familia == "BAG" || to.familia == "GARMENTBAG" || to.familia == "HANGERS"){
                        lista.Add(to);
                    }
                    //lista.Add(to);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public List<Trim_orden> obtener_trims_entrega_pedido(string pedido){
            List<Trim_orden> lista = new List<Trim_orden>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT id_trim_orden,id_item,id_pedido,id_summary,id_talla,id_sucursal,id_locacion,id_usuario,total,fecha,id_request,po " +
                    " FROM trims_ordenes WHERE id_pedido='" + pedido + "'";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Trim_orden to = new Trim_orden();
                    to.id_trim_order = Convert.ToInt32(leer_ltd["id_trim_orden"]);
                    to.id_item = Convert.ToInt32(leer_ltd["id_item"]);
                    to.id_pedido = Convert.ToInt32(leer_ltd["id_pedido"]);
                    to.item = consultas.buscar_descripcion_item(Convert.ToString(to.id_item));
                    to.id_summary = Convert.ToInt32(leer_ltd["id_summary"]);
                    int id_estilo = consultas.obtener_estilo_summary(to.id_summary);
                    to.estilo = consultas.buscar_descripcion_estilo(id_estilo);
                    to.id_talla = Convert.ToInt32(leer_ltd["id_talla"]);
                    to.talla = consultas.obtener_size_id(Convert.ToString(to.id_talla));
                    to.id_sucursal = Convert.ToInt32(leer_ltd["id_sucursal"]);
                    to.sucursal = consultas.obtener_sucursal_id(Convert.ToString(to.id_sucursal));
                    to.id_locacion = Convert.ToInt32(leer_ltd["id_locacion"]);
                    to.locacion = consultas.obtener_ubicacion_id(Convert.ToString(to.id_locacion));
                    to.total = Convert.ToInt32(leer_ltd["total"]);
                    to.total=to.total- obtener_total_entregado_anteriormente(to.id_trim_order, 0, 0);
                    to.fecha = Convert.ToDateTime(leer_ltd["fecha"]).ToString("dd-MM-yyyy");
                    to.id_usuario = Convert.ToInt32(leer_ltd["id_usuario"]);
                    to.usuario = consultas.buscar_nombre_usuario(Convert.ToString(to.id_usuario));
                    to.id_request = 0;
                    to.po = Convert.ToString(leer_ltd["po"]);
                    to.familia = consultas.buscar_tipo_trim_item(Convert.ToString(to.id_item));
                    if (to.familia != "BOXES" || to.familia != "POLYBAG" || to.familia != "MASTER BAG" || to.familia != "BAG" || to.familia != "GARMENTBAG" || to.familia != "HANGERS"){
                        lista.Add(to);
                    }
                    //lista.Add(to);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public List<Inventario> obtener_stock_entrega_bodega(int sucursal){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<Inventario> lista = new List<Inventario>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT  id_item,id_inventario,id_pedido,total,id_customer,id_family_trim,descripcion,id_estilo,id_sucursal FROM inventario " +
                    " WHERE id_categoria_inventario=2 and id_pedido=0 AND total>0 AND id_sucursal=" + sucursal+" AND id_location=72";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Inventario i = new Inventario();
                    i.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    i.id_item = Convert.ToInt32(leer["id_item"]);
                    i.id_pedido = Convert.ToInt32(leer["id_pedido"]);
                    i.id_sucursal = Convert.ToInt32(leer["id_sucursal"]);
                    i.sucursal = consultas.obtener_sucursal_id(Convert.ToString(leer["id_sucursal"]));
                    i.po = consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    i.id_estilo = Convert.ToInt32(leer["id_estilo"]);
                    i.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"])) + " " + consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    i.total = Convert.ToInt32(leer["total"]);
                    i.descripcion = Convert.ToString(leer["descripcion"]);
                    i.id_customer = Convert.ToInt32(leer["id_customer"]);
                    i.customer = consultas.obtener_customer_id(Convert.ToString(i.id_customer));
                    i.family_trim = consultas.obtener_family_id(Convert.ToString(leer["id_family_trim"]));
                    //string familia = (i.family_trim).Trim();
                    /*if (familia == "BOXES" || familia == "POLYBAG" || familia == "MASTER BAG" || familia == "BAG" || familia == "GARMENTBAG" || familia == "TAPE" || familia == "HANGERS" || familia == "HANGTAGS")
                    {*/
                    lista.Add(i);
                    //}
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Inventario> obtener_stock_entrega(int sucursal){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<Inventario> lista = new List<Inventario>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT  id_item,id_inventario,id_pedido,total,id_customer,id_family_trim,descripcion,id_estilo,id_sucursal FROM inventario " +
                    " WHERE id_categoria_inventario=2 and id_pedido=0 AND total>0 AND id_sucursal=" + sucursal + " AND id_location=73"; 
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Inventario i = new Inventario();
                    i.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    i.id_item = Convert.ToInt32(leer["id_item"]);
                    i.id_pedido = Convert.ToInt32(leer["id_pedido"]);
                    i.id_sucursal = Convert.ToInt32(leer["id_sucursal"]);
                    i.sucursal = consultas.obtener_sucursal_id(Convert.ToString(leer["id_sucursal"]));
                    i.po = consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    i.id_estilo = Convert.ToInt32(leer["id_estilo"]);
                    i.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"])) + " " + consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    i.total = Convert.ToInt32(leer["total"]);
                    i.descripcion = Convert.ToString(leer["descripcion"]);
                    i.id_customer = Convert.ToInt32(leer["id_customer"]);
                    i.customer = consultas.obtener_customer_id(Convert.ToString(i.id_customer));
                    i.family_trim = consultas.obtener_family_id(Convert.ToString(leer["id_family_trim"]));
                    lista.Add(i);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Inventario> obtener_stock_entrega_impresiones(int sucursal){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<Inventario> lista = new List<Inventario>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT  id_item,id_inventario,id_pedido,total,id_customer,id_family_trim,descripcion,id_estilo,id_sucursal FROM inventario " +
                    " WHERE id_categoria_inventario=2 and id_pedido=0 AND total>0 AND id_sucursal=" + sucursal + " AND id_location=77";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Inventario i = new Inventario();
                    i.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    i.id_item = Convert.ToInt32(leer["id_item"]);
                    i.id_pedido = Convert.ToInt32(leer["id_pedido"]);
                    i.id_sucursal = Convert.ToInt32(leer["id_sucursal"]);
                    i.sucursal = consultas.obtener_sucursal_id(Convert.ToString(leer["id_sucursal"]));
                    i.po = consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    i.id_estilo = Convert.ToInt32(leer["id_estilo"]);
                    i.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"])) + " " + consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    i.total = Convert.ToInt32(leer["total"]);
                    i.descripcion = Convert.ToString(leer["descripcion"]);
                    i.id_customer = Convert.ToInt32(leer["id_customer"]);
                    i.customer = consultas.obtener_customer_id(Convert.ToString(i.id_customer));
                    i.family_trim = consultas.obtener_family_id(Convert.ToString(leer["id_family_trim"]));
                    lista.Add(i);}
                leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public void guardar_entrega(string entrega, string recibe,string pedido){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO trim_entregas(entrega,recibe,fecha,id_pedido) " +
                    "VALUES ('" + entrega + "','" + recibe + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + pedido + "')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void guardar_entrega_pedido(string entrega, string recibe, string pedido,string comentarios){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO trim_entregas(entrega,recibe,fecha,id_pedido,comentarios) " +
                    "VALUES ('" + entrega + "','" + recibe + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + pedido + "','" + comentarios + "')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public int obtener_ultima_entrega(){
            int temp = 0;
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT TOP 1 id_entrega FROM trim_entregas order by id_entrega desc ";
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    temp = Convert.ToInt32(leer_u_i["id_entrega"]);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            return temp;
        }
        public void guardar_entrega_item(int entrega, string trim_orden, string request, string inventario, string total, string comentarios){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO trim_entrega_items(id_entrega,id_trim_orden,id_trim_request,id_inventario,total,comentarios) VALUES " +
                    "('" + entrega + "','" + trim_orden + "','" + request + "','" + inventario + "','" + total + "','" + comentarios + "')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }

        public void actualizar_request_inventario(int item, string total,string sucursal,int locacion){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE inventario SET total=total-" + total + " WHERE id_item=" + item + " AND id_pedido=0 AND id_sucursal="+sucursal+" AND id_location="+locacion+" AND total>=1";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void sumar_request_inventario(int item, string total, string sucursal, int locacion){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE inventario SET total=total+" + total + " WHERE id_item=" + item + " AND id_pedido=0 AND id_sucursal=" + sucursal + " AND id_location=" + locacion;
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public List<Trim_entregas> obtener_entregas_orden(string orden){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<Trim_entregas> lista = new List<Trim_entregas>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT entrega,recibe,fecha,id_entrega,id_pedido,comentarios FROM trim_entregas WHERE id_pedido=" + orden ;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Trim_entregas e = new Trim_entregas();
                    e.id_entrega = Convert.ToInt32(leer["id_entrega"]);
                    e.comentarios = Convert.ToString(leer["comentarios"]);
                    e.entrega = Convert.ToString(leer["entrega"]);
                    e.recibe = Convert.ToString(leer["recibe"]);
                    e.fecha = Convert.ToDateTime(leer["fecha"]).ToString("dd-MM-yyyy");
                    e.id_pedido= Convert.ToInt32(leer["id_pedido"]);
                    e.pedido= consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    e.lista_entregas = obtener_entregas_items(e.id_entrega);
                    lista.Add(e);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public Trim_entregas obtener_entrega(string id){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Trim_entregas e = new Trim_entregas();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT entrega,recibe,fecha,id_entrega,id_pedido,comentarios FROM trim_entregas WHERE id_entrega=" + id;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    e.id_entrega = Convert.ToInt32(leer["id_entrega"]);
                    e.comentarios = Convert.ToString(leer["comentarios"]);
                    e.entrega = Convert.ToString(leer["entrega"]);
                    e.recibe = Convert.ToString(leer["recibe"]);
                    e.fecha = Convert.ToDateTime(leer["fecha"]).ToString("dd-MM-yyyy");
                    e.id_pedido = Convert.ToInt32(leer["id_pedido"]);
                    e.pedido = consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    e.lista_entregas = obtener_entregas_items(e.id_entrega);                    
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return e;
        }
        public List<Trim_entregas> obtener_entregas_fechas(string inicio,string final){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<Trim_entregas> lista = new List<Trim_entregas>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                if (inicio != "0" && final != "0"){
                    com.CommandText = "SELECT entrega,recibe,fecha,id_entrega,id_pedido,comentarios FROM trim_entregas WHERE fecha BETWEEN '" + inicio + "' AND '" + final + "' ";
                }else {
                    com.CommandText = "SELECT TOP 15 entrega,recibe,fecha,id_entrega,id_pedido,comentarios FROM trim_entregas ORDER BY id_entrega DESC ";
                }
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Trim_entregas e = new Trim_entregas();
                    e.id_entrega = Convert.ToInt32(leer["id_entrega"]);
                    e.comentarios = Convert.ToString(leer["comentarios"]);
                    e.entrega = Convert.ToString(leer["entrega"]);
                    e.recibe = Convert.ToString(leer["recibe"]);
                    e.fecha = Convert.ToDateTime(leer["fecha"]).ToString("dd-MM-yyyy");
                    e.id_pedido = Convert.ToInt32(leer["id_pedido"]);
                    e.pedido = consultas.obtener_po_id(Convert.ToString(leer["id_pedido"]));
                    e.lista_entregas = obtener_entregas_items(e.id_entrega);
                    lista.Add(e);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Trim_entregas_items> obtener_entregas_items(int entrega){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<Trim_entregas_items> lista = new List<Trim_entregas_items>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_entrega_item,id_entrega,id_trim_orden,id_trim_request,id_inventario,total,comentarios " +
                    " FROM trim_entrega_items WHERE id_entrega=" + entrega;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Trim_entregas_items e = new Trim_entregas_items();
                    e.id_entrega = Convert.ToInt32(leer["id_entrega"]);
                    e.id_entrega_item = Convert.ToInt32(leer["id_entrega_item"]);
                    e.id_trim_orden = Convert.ToInt32(leer["id_trim_orden"]);
                    e.id_trim_request = Convert.ToInt32(leer["id_trim_request"]);
                    e.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    e.total = Convert.ToInt32(leer["total"]);
                    e.comentarios = Convert.ToString(leer["comentarios"]);
                    e.total_anterior = obtener_total_entregado_anteriormente(e.id_trim_orden, e.id_trim_request, e.id_inventario);
                    if (e.id_trim_request != 0){//REQUEST                                              
                        Trim_requests tr = obtener_trim_request(e.id_trim_request);
                        e.item = tr.item;
                        e.total_orden = tr.total;                       
                    }else {
                        if (e.id_trim_orden!= 0) {//ORDEN
                            Trim_orden to = obtener_trim_auditado_orden(Convert.ToString(e.id_trim_orden));
                            e.item = to.item;
                            e.total_orden = to.total;
                            e.estilo = to.estilo;
                            e.talla = to.talla;
                        }else {//INVENTARIO
                            e.item = buscar_descripcion_inventario(e.id_inventario);
                            e.total_orden = 0;
                        }
                    }
                    e.restante = e.total_orden - e.total_anterior;
                    lista.Add(e);}
                leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public int obtener_total_entregado_anteriormente(int trim_orden,int request,int inventario){
            int temp = 0;
            Link con= new Link();
            try{
                SqlCommand com= new SqlCommand();
                SqlDataReader leer= null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT total FROM trim_entrega_items WHERE id_trim_orden="+trim_orden+" " +
                    " AND id_trim_request="+request+" AND id_inventario="+inventario;
                leer= com.ExecuteReader();
                while (leer.Read()){
                    temp += Convert.ToInt32(leer["total"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }
        public Trim_entregas_items obtener_entrega_item(int id){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Trim_entregas_items e = new Trim_entregas_items();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_entrega_item,id_entrega,id_trim_orden,id_trim_request,id_inventario,total,comentarios " +
                    " FROM trim_entrega_items WHERE id_entrega_item=" + id;
                leer = com.ExecuteReader();
                while (leer.Read()){                    
                    e.id_entrega = Convert.ToInt32(leer["id_entrega"]);
                    e.id_entrega_item = Convert.ToInt32(leer["id_entrega_item"]);
                    e.id_trim_orden = Convert.ToInt32(leer["id_trim_orden"]);
                    e.id_trim_request = Convert.ToInt32(leer["id_trim_request"]);
                    e.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    e.total = Convert.ToInt32(leer["total"]);
                    e.comentarios = Convert.ToString(leer["comentarios"]);
                    e.total_anterior = obtener_total_entregado_anteriormente(e.id_trim_orden, e.id_trim_request, e.id_inventario);
                    if (e.id_trim_request != 0){//REQUEST                                              
                        Trim_requests tr = obtener_trim_request(e.id_trim_request);
                        e.item = tr.item;
                        e.total_orden = tr.total;
                    }else{
                        if (e.id_trim_orden != 0){//ORDEN
                            Trim_orden to = obtener_trim_auditado_orden(Convert.ToString(e.id_trim_orden));
                            e.item = to.item;
                            e.total_orden = to.total;
                            e.estilo = to.estilo;
                            e.talla = to.talla;
                        }else{//INVENTARIO
                            e.item = buscar_descripcion_inventario(e.id_inventario);
                            e.total_orden = 0;
                        }
                    }
                    e.restante = e.total_orden - e.total_anterior;
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return e;
        }
        public void eliminar_item_entrega(string entrega){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "DELETE FROM trim_entrega_items WHERE id_entrega_item="+ entrega;
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public int contar_items_entrega(string entrega){
            int temp = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_entrega FROM trim_entrega_items WHERE id_entrega=" + entrega;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temp++;
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }
        public void eliminar_entrega(string entrega){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "DELETE FROM trim_entregas WHERE id_entrega=" + entrega;
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void restar_item_entrega(string entrega,int total){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "UPDATE trim_entrega_items SET total=(total-" + total + ") WHERE id_entrega_item=" + entrega;
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public List<Sucursal_trims> lista_sucursales(){
            List<Sucursal_trims> lista = new List<Sucursal_trims>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_sucursal,sucursal FROM sucursales  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    Sucursal_trims s = new Sucursal_trims();
                    s.id_sucursal = Convert.ToInt32(leerFilas["id_sucursal"]);
                    s.sucursal = leerFilas["sucursal"].ToString();
                    lista.Add(s);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<Trim_orden> obtener_cajas_pedido(string pedido){
            List<Trim_orden> lista = new List<Trim_orden>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con_ltd = new Link();
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "SELECT id_item FROM trim_requests WHERE id_pedido=" + pedido;
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Trim_orden to = new Trim_orden();
                    to.id_item = Convert.ToInt32(leer_ltd["id_item"]);                    
                    to.item = consultas.buscar_descripcion_item(Convert.ToString(to.id_item));                                      
                    to.familia = consultas.buscar_tipo_trim_item(Convert.ToString(to.id_item));
                    if (to.familia == "BOXES" ){
                        lista.Add(to);
                    }
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public List<Trim_location_item> buscar_locaciones_orden(string pedido){
            List<Trim_location_item> lista = new List<Trim_location_item>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link con= new Link();
            try{
                SqlCommand com= new SqlCommand();
                SqlDataReader leer= null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_trim_location,id_ubicacion,comentarios FROM trim_locations WHERE id_pedido=" + pedido;
                leer= com.ExecuteReader();
                while (leer.Read()){
                    Trim_location_item t = new Trim_location_item();
                    t.id_trim_location = Convert.ToInt32(leer["id_trim_location"]);
                    t.id_ubicacion = Convert.ToInt32(leer["id_ubicacion"]);
                    t.ubicacion = consultas.obtener_ubicacion_id(Convert.ToString(leer["id_ubicacion"]));
                    t.comentarios = Convert.ToString(leer["comentarios"]);
                    lista.Add(t);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public void eliminar_locacion_trim_pedido(string id){
            Link conTrim = new Link();
            try{
                SqlCommand comTrim = new SqlCommand();
                comTrim.Connection = conTrim.AbrirConexion();
                comTrim.CommandText = "DELETE FROM trim_locations WHERE id_pedido=" + id;
                comTrim.ExecuteNonQuery();
            }finally { conTrim.CerrarConexion(); conTrim.Dispose(); }
        }
        public void insertar_locacion_pedido(string locacion,string pedido,string comentarios){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO trim_locations(id_ubicacion,id_pedido,comentarios) VALUES ('" + locacion + "','" + pedido + "','" + comentarios + "')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }

        public List<estilo_shipping> lista_estilos(string id_pedido){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<estilo_shipping> listar = new List<estilo_shipping>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select ID_PO_SUMMARY from PO_SUMMARY where ID_PEDIDOS='" + id_pedido + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    estilo_shipping l = new estilo_shipping();
                    l.id_summary = Convert.ToInt32(leerFilas["ID_PO_SUMMARY"]);
                    l.id_estilo = consultas.obtener_estilo_summary(l.id_summary);
                    l.estilo = consultas.obtener_estilo(l.id_estilo);
                    l.descripcion = consultas.buscar_descripcion_estilo(l.id_estilo);
                    listar.Add(l);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return listar;
        }
        public Pedidos_trim buscar_estado_total_pedido(int pedido){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            DatosShipping ds = new DatosShipping();
            Pedidos_trim p = new Pedidos_trim();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT P.PO,P.CUSTOMER,P.DATE_CANCEL,CC.NAME from " +
                    " PEDIDO P, CAT_CUSTOMER CC where P.ID_PEDIDO='" + pedido + "' and CC.CUSTOMER=P.CUSTOMER ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    p.id_pedido = pedido;
                    p.pedido = Convert.ToString(leer["PO"]);
                    p.id_customer = Convert.ToInt32(leer["CUSTOMER"]);
                    p.customer = Convert.ToString(leer["NAME"]);
                    p.ship_date = Convert.ToDateTime(leer["DATE_CANCEL"]).ToString("dd-MMM");
                    List<int> temporal = consultas.Lista_generos_po(pedido);
                    p.gender = "";
                    foreach (int x in temporal){
                        p.gender += consultas.obtener_genero_id(Convert.ToString(x));
                    }
                    p.gender = Regex.Replace(p.gender, @"\s+", " ");
                    p.lista_families = obtener_lista_familias(pedido);
                    p.lista_empaque = lista_tipos_empaque(Convert.ToString(pedido));
                    p.lista_assort = ds.lista_assortments_pedido(pedido);
                    p.fold_size = buscar_fold_size_pedido(pedido);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return p;
        }
        public void agregar_entrega_trim_card(string id, string entrega,string recibe,string fecha){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "UPDATE trim_card SET entrega='"+entrega+ "',recibe='" + recibe + "',fecha_entrega='"+fecha+" 00:00:00' WHERE id_trim_card=" + id;
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public int buscar_inventario_auditoria(int item,int customer,int sucursal){
            int temp = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_inventario FROM inventario WHERE id_item=" + item+" AND id_customer="+customer+
                    " AND id_sucursal="+sucursal+" AND id_location=72";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temp = Convert.ToInt32(leer["id_inventario"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }
        public List<Trim_item> lista_trims_tabla_inicio(int sucursal){
            List<Trim_item> lista = new List<Trim_item>();
            Link con_led = new Link();
            try{
                SqlCommand com_led = new SqlCommand();
                SqlDataReader leer_led = null;
                com_led.Connection = con_led.AbrirConexion();
                com_led.CommandText = " SELECT item_id,item,body_type,descripcion,fabric_type,unit,minimo from items_catalogue where minimo!=0 and tipo=2 ";
                leer_led = com_led.ExecuteReader();
                while (leer_led.Read()){
                    Trim_item ti = new Trim_item();
                    ti.id_item = Convert.ToInt32(leer_led["item_id"]);
                    ti.item = Convert.ToString(leer_led["item"]);
                    ti.descripcion = Convert.ToString(leer_led["descripcion"]);
                    ti.family = Convert.ToString(leer_led["fabric_type"]);
                    ti.unit = Convert.ToString(leer_led["unit"]);
                    ti.minimo = Convert.ToInt32(leer_led["minimo"]);
                    ti.total = buscar_totales_item(ti.id_item,sucursal);
                    if (ti.total <= ti.minimo){
                        lista.Add(ti);
                    }
                }leer_led.Close();
            }finally { con_led.CerrarConexion(); con_led.Dispose(); }
            return lista;
        }
        public int buscar_totales_item(int item,int sucursal){
            int temp = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT total FROM inventario WHERE id_item=" + item + " AND id_sucursal="+sucursal+" AND id_location=72"; //AQUI QUEDA PENDIENTE QUE INVENTARIO VOY A REVISAR
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temp += Convert.ToInt32(leer["total"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }

































































































    }
}