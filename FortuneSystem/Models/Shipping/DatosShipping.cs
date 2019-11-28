using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.IO;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using FortuneSystem.Models.Shipping;
using FortuneSystem.Models.Catalogos;
using FortuneSystem.Models.Almacen;
using FortuneSystem.Models.Staging;
using FortuneSystem.Models.Trims;
using System.Text.RegularExpressions;


namespace FortuneSystem.Models.Shipping
{
    public class DatosShipping{

        FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();       

        public List<recibo_fantasy> lista_ordenes_mde(string busqueda) {
            List<recibo_fantasy> listar = new List<recibo_fantasy>();
            Link conn = new Link();
            string query;
            if (busqueda != "") {
                query = "SELECT TOP 50 ID_PEDIDO,PO from PEDIDO where PO like'%" + busqueda + "%' where ID_STATUS!=7 AND ID_STATUS!=6 AND ID_STATUS!=5  AND CUSTOMER=1";
            } else {
                query = "SELECT TOP 50 ID_PEDIDO,PO from PEDIDO where ID_STATUS!=7 AND ID_STATUS!=6 AND ID_STATUS!=5  where CUSTOMER=1";
            }
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = query;
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    recibo_fantasy l = new recibo_fantasy();
                    l.id_pedido = Convert.ToInt32(leerFilas["ID_PEDIDO"]);
                    l.po = leerFilas["PO"].ToString();
                    listar.Add(l);
                }
                leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return listar;
        }
        public List<estilo_shipping> lista_estilos(string id_pedido) {         
            List<estilo_shipping> listar = new List<estilo_shipping>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct ITEM_ID from PO_SUMMARY where ID_PEDIDOS='" + id_pedido + "' "; 
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    estilo_shipping l = new estilo_shipping();
                    l.id_estilo = Convert.ToInt32(leerFilas["ITEM_ID"]);
                    l.id_summary = consultas.obtener_po_summary(Convert.ToInt32(id_pedido), l.id_estilo);                   
                    l.id_color = consultas.obtener_color_id_item(Convert.ToInt32(id_pedido), l.id_estilo);
                    l.color = consultas.obtener_color_id(Convert.ToString(l.id_color));
                    l.estilo = consultas.obtener_estilo(l.id_estilo);
                    l.descripcion = consultas.buscar_descripcion_estilo(l.id_estilo);
                    List<Empaque> lista_e = new List<Empaque>();
                    List<string> tipo_empaque_temporal= consultas.buscar_tipo_empaque(l.id_summary);
                    int bulls = 0;
                    foreach(string s in tipo_empaque_temporal) {                       
                        //if (s=="1"){ e.lista_ratio = obtener_lista_tallas_estilo(l.id_summary, l.id_estilo); }
                        switch (s) {
                            case "1":
                                Empaque e = new Empaque();
                                e.tipo_empaque = Convert.ToInt32(s);
                                e.lista_ratio = obtener_lista_ratio(l.id_summary, l.id_estilo, 1);
                                lista_e.Add(e);
                                bulls++;
                                break;
                            case "2":
                                Empaque e2 = new Empaque();
                                e2.tipo_empaque = Convert.ToInt32(s);
                                e2.lista_ratio = obtener_lista_ratio(l.id_summary, l.id_estilo, 2);
                                lista_e.Add(e2);
                                break;
                            case "4":
                                List<int> ppks = obtener_number_ppks_estilo(l.id_summary);
                                foreach (int p in ppks) {
                                    List<string> packings = obtener_number_ppks_estilo(l.id_summary,p);
                                    foreach (string packing_pks in packings) {
                                        Empaque e4 = new Empaque();
                                        e4.tipo_empaque = Convert.ToInt32(s);
                                        e4.number_ppk = p;
                                        e4.packing_name = packing_pks;
                                        e4.lista_ratio = obtener_lista_ratio_ppks(l.id_summary, l.id_estilo, 4, p, packing_pks);
                                        lista_e.Add(e4);
                                    }
                                }
                                break;
                            case "5":
                                List<string> bps = obtener_number_bps_estilo(l.id_summary);
                                foreach (string b in bps){
                                    Empaque e5 = new Empaque();
                                    e5.tipo_empaque = Convert.ToInt32(s);
                                    e5.packing_name =b;
                                    e5.lista_ratio = obtener_lista_ratio_bps(l.id_summary, l.id_estilo, 5, b);
                                    lista_e.Add(e5);
                                }
                                bulls++;
                                break;
                        }                        
                    }
                    if (bulls == 0) {
                       /* Empaque eb = new Empaque();
                        eb.tipo_empaque =1;
                        eb.lista_ratio = obtener_lista_ratio(l.id_summary, l.id_estilo, 1);
                        lista_e.Add(eb);*/
                    }
                    l.lista_empaque = lista_e;
                    listar.Add(l);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return listar;
        }

        public List<estilo_shipping> lista_estilos_abiertos(){
            List<estilo_shipping> listar = new List<estilo_shipping>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct ITEM_ID from PO_SUMMARY where ID_ESTADO!=7 AND ID_ESTADO!=6 AND ID_ESTADO!=5 ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    estilo_shipping l = new estilo_shipping();
                    l.id_estilo = Convert.ToInt32(leerFilas["ITEM_ID"]);
                    l.id_summary = consultas.obtener_po_summary(Convert.ToInt32(id_pedido), l.id_estilo);
                    l.id_color = consultas.obtener_color_id_item(Convert.ToInt32(id_pedido), l.id_estilo);
                    l.color = consultas.obtener_color_id(Convert.ToString(l.id_color));
                    l.estilo = consultas.obtener_estilo(l.id_estilo);
                    l.descripcion = consultas.buscar_descripcion_estilo(l.id_estilo);
                   /* List<Empaque> lista_e = new List<Empaque>();
                    List<string> tipo_empaque_temporal = consultas.buscar_tipo_empaque(l.id_summary);
                    foreach (string s in tipo_empaque_temporal){
                        //if (s=="1"){ e.lista_ratio = obtener_lista_tallas_estilo(l.id_summary, l.id_estilo); }
                        switch (s){
                            case "1":
                                Empaque e = new Empaque();
                                e.tipo_empaque = Convert.ToInt32(s);
                                e.lista_ratio = obtener_lista_ratio(l.id_summary, l.id_estilo, 1);
                                lista_e.Add(e);
                                break;
                            case "2":
                                Empaque e2 = new Empaque();
                                e2.tipo_empaque = Convert.ToInt32(s);
                                e2.lista_ratio = obtener_lista_ratio(l.id_summary, l.id_estilo, 2);
                                lista_e.Add(e2);
                                break;
                            case "4":
                                List<int> ppks = obtener_number_ppks_estilo(l.id_summary);
                                foreach (int p in ppks){
                                    Empaque e4 = new Empaque();
                                    e4.tipo_empaque = Convert.ToInt32(s);
                                    e4.number_ppk = p;
                                    e4.lista_ratio = obtener_lista_ratio_ppks(l.id_summary, l.id_estilo, 4, p);
                                    lista_e.Add(e4);
                                }
                                break;
                        }
                    }
                    l.lista_empaque = lista_e;*/
                    listar.Add(l);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return listar;
        }
        public List<ratio_tallas> obtener_lista_ratio_ppks(int posummary, int estilo, int tipo_empaque,int ppk,string packing){
            List<ratio_tallas> lista = new List<ratio_tallas>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select PTZ.ID_TALLA,PTZ.RATIO,PTZ.PIECES,CS.ORDEN,CS.TALLA from PACKING_TYPE_SIZE PTZ,CAT_ITEM_SIZE CS " +
                    " where PTZ.ID_SUMMARY='" + posummary + "' and PTZ.TYPE_PACKING='" + tipo_empaque + "' AND PTZ.NUMBER_PPKS='" + ppk + "'  " +
                    " AND PTZ.PACKING_NAME='" + packing + "' AND " +
                    " CS.ID=PTZ.ID_TALLA ORDER BY CAST(CS.ORDEN AS int) ASC ";                
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    ratio_tallas e = new ratio_tallas();
                    e.id_estilo = estilo; //Regex.Replace(color, @"\s+", " ");
                    e.id_talla = Convert.ToInt32(leerFilas["ID_TALLA"]);
                    e.talla = Regex.Replace(consultas.obtener_size_id(Convert.ToString(leerFilas["ID_TALLA"])), @"\s+", " ");
                    e.ratio = Convert.ToInt32(leerFilas["RATIO"]);
                    e.piezas = Convert.ToInt32(leerFilas["PIECES"]);
                    lista.Add(e);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<ratio_tallas> obtener_lista_ratio_bps(int posummary, int estilo, int tipo_empaque, string packing_name){
            List<ratio_tallas> lista = new List<ratio_tallas>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select ID_TALLA,RATIO,PIECES from PACKING_TYPE_SIZE where ID_SUMMARY='" + posummary + "' and TYPE_PACKING='" + tipo_empaque + "' and PACKING_NAME='" + packing_name + "'  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    ratio_tallas e = new ratio_tallas();
                    e.id_estilo = estilo; //Regex.Replace(color, @"\s+", " ");
                    e.id_talla = Convert.ToInt32(leerFilas["ID_TALLA"]);
                    e.talla = Regex.Replace(consultas.obtener_size_id(Convert.ToString(leerFilas["ID_TALLA"])), @"\s+", " ");
                    e.ratio = Convert.ToInt32(leerFilas["RATIO"]);
                    e.piezas = Convert.ToInt32(leerFilas["PIECES"]);
                    lista.Add(e);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<int> obtener_number_ppks_estilo(int summary) {
            List<int> lista = new List<int>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct NUMBER_PPKS from PACKING_TYPE_SIZE where ID_SUMMARY='" + summary + "' and TYPE_PACKING=4  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista.Add(Convert.ToInt32(leerFilas["NUMBER_PPKS"]));
                }
                leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<string> obtener_number_bps_estilo(int summary){
            List<string> lista = new List<string>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct PACKING_NAME from PACKING_TYPE_SIZE where ID_SUMMARY='" + summary + "' and TYPE_PACKING=5  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista.Add(Convert.ToString(leerFilas["PACKING_NAME"]));
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<string> obtener_number_ppks_estilo(int summary,int ppks){
            List<string> lista = new List<string>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct PACKING_NAME from PACKING_TYPE_SIZE where ID_SUMMARY='" + summary + "' and TYPE_PACKING=4 and NUMBER_PPKS='"+ppks+"'  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista.Add(Convert.ToString(leerFilas["PACKING_NAME"]));
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<estilo_shipping> lista_estilos_hottopic(string id_pedido){
            List<estilo_shipping> listar = new List<estilo_shipping>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct pk.type_packing,pk.number_po,pk.id_summary,p.item_id from po_summary p,packing_type_size pk where " +
                    " p.id_pedidos='"+id_pedido+"' and pk.id_summary=p.id_po_summary ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read())
                {
                    estilo_shipping l = new estilo_shipping();
                    l.id_estilo = Convert.ToInt32(leerFilas["item_id"]);
                    l.id_summary = Convert.ToInt32(leerFilas["id_summary"]);
                    l.id_color = consultas.obtener_color_id_item(Convert.ToInt32(id_pedido), l.id_estilo);
                    l.color = consultas.obtener_color_id(Convert.ToString(l.id_color));
                    l.estilo = consultas.obtener_estilo(l.id_estilo);
                    l.descripcion = consultas.buscar_descripcion_estilo(l.id_estilo);
                    l.number_po = Convert.ToString(leerFilas["number_po"]);
                    l.tipo_empaque = Convert.ToInt32(leerFilas["type_packing"]);
                    /*List<Empaque> lista_e = new List<Empaque>();
                    List<string> tipo_empaque_temporal = consultas.buscar_tipo_empaque(l.id_summary);
                    foreach (string s in tipo_empaque_temporal){
                        Empaque e = new Empaque();
                        e.tipo_empaque = Convert.ToInt32(s);
                        if (s == "1") { e.lista_ratio = obtener_lista_tallas_estilo(l.id_summary, l.id_estilo); }
                        if (s == "2") { e.lista_ratio = obtener_lista_ratio(l.id_summary, l.id_estilo, 2); }
                        lista_e.Add(e);
                    }
                    l.lista_empaque = lista_e;*/
                    listar.Add(l);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return listar;
        }
        public List<ratio_tallas> obtener_lista_tallas_estilo(int posummary, int estilo) {
            List<ratio_tallas> lista = new List<ratio_tallas>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select TALLA_ITEM from ITEM_SIZE where ID_SUMMARY='" + posummary + "'  and TALLA_ITEM IS NOT NULL ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    ratio_tallas e = new ratio_tallas();
                    e.id_estilo = estilo;
                    e.id_talla = Convert.ToInt32(leerFilas["TALLA_ITEM"]);
                    e.talla = consultas.obtener_size_id(Convert.ToString(leerFilas["TALLA_ITEM"]));
                    e.ratio = buscar_piezas_empaque_bull(posummary, e.id_talla);
                    lista.Add(e);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public int buscar_piezas_empaque_bull(int summary,int talla){
            int temp = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT PIECES FROM PACKING_TYPE_SIZE WHERE ID_SUMMARY='" + summary + "' AND ID_TALLA='"+talla+"' and TYPE_PACKING=1 ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temp = Convert.ToInt32(leer["PIECES"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }
        public int buscar_piezas_empaque_bulls(int summary, int talla,string packing_name){
            int temp = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT PIECES FROM PACKING_TYPE_SIZE WHERE ID_SUMMARY='" + summary + "' AND ID_TALLA='" + talla + "' and TYPE_PACKING=5 and PACKING_NAME='"+packing_name+"' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temp = Convert.ToInt32(leer["PIECES"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }

        public string obtener_nombre_packing(int summary){
            string temp = "";
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT PACKING_NAME FROM PACKING_TYPE_SIZE WHERE ID_SUMMARY='" + summary + "' AND TYPE_PACKING=3 ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temp = Convert.ToString(leer["PACKING_NAME"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }
        public int obtener_id_assort(string name){
            int temp = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT ID_PACKING_ASSORT FROM PACKING_ASSORT WHERE PACKING_NAME='" + name + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temp = Convert.ToInt32(leer["ID_PACKING_ASSORT"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }
        public int buscar_existencia_inventario(int id_color, int id_talla, string id_estilo) {
            int temp = 0;
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_inventario from inventario_fantasy where id_color='" + id_color + "' and id_talla='" + id_talla + "' and id_estilo='" + id_estilo + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    temp = Convert.ToInt32(leer["id_inventario"]);
                }
                leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }

        public void guardar_recibo_fantasy(int pedido, int usuario, int piezas, int cajas) {
            Link con_c = new Link();
            try {
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO recibos_fantasy(id_pedido,fecha,id_usuario,total,total_cajas) VALUES " +
                    " ('" + pedido + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + usuario + "','" + piezas + "','" + cajas + "')";
                com_c.ExecuteNonQuery();
            } finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }

        public int obtener_ultimo_recibo() {
            int id_recibo = 0;
            Link con_u_r = new Link();
            try {
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT TOP 1 id_recibo FROM recibos_fantasy order by id_recibo desc ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()) {
                    id_recibo = Convert.ToInt32(leer_u_r["id_recibo"]);
                }
                leer_u_r.Close();
            } finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return id_recibo;
        }

        public void guardar_item_inventario(int color, int talla, string estilo, string descripcion, int total)
        {
            Link con_c = new Link();
            try
            {
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO inventario_fantasy(id_color,id_talla,id_estilo,descripcion,total) VALUES " +
                    " ('" + color + "','" + talla + "','" + estilo + "','" + descripcion + "','" + total + "')";
                com_c.ExecuteNonQuery();
            }
            finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }

        public int obtener_ultimo_item() {
            int id = 0;
            Link con_u_r = new Link();
            try {
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT TOP 1 id_inventario FROM inventario_fantasy order by id_inventario desc ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()) {
                    id = Convert.ToInt32(leer_u_r["id_inventario"]);
                }
                leer_u_r.Close();
            } finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return id;
        }

        public void aumentar_inventario(int inventario, int cajas, int piezas) {
            Link con_s = new Link();
            try {
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE inventario_fantasy SET total=(total+'" + cajas * piezas + "') WHERE id_inventario='" + inventario + "' ";
                com_s.ExecuteNonQuery();
            } finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        //ds.guardar_recibo_fantasy_item(id_recibo,id_inventario,Cajas[i],Piezas[i]);
        public void guardar_recibo_fantasy_item(int recibo, int inventario, string cajas, string piezas) {
            Link con_c = new Link();
            try {
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO recibos_fantasy_items(id_recibo,id_inventario,cajas,piezas) VALUES " +
                    " ('" + recibo + "','" + inventario + "','" + cajas + "','" + piezas + "')";
                com_c.ExecuteNonQuery();
            } finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        //buscar_estilos_po
        public List<estilos> buscar_estilos_po(string po) {
            List<estilos> lista = new List<estilos>();
            int id_pedido = consultas.buscar_pedido(po);
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct ITEM_ID from PO_SUMMARY where ID_PEDIDOS='" + id_pedido + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    estilos e = new estilos();
                    e.id_estilo = Convert.ToInt32(leerFilas["ITEM_ID"]);
                    e.estilo = consultas.buscar_descripcion_estilo(e.id_estilo);
                    e.estilo = Regex.Replace(e.estilo, @"\s+", " ");
                    lista.Add(e);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
             
        public void guardar_estilos_paking(string repeticion,string label,string cantidad, string id_tarima, string used, string id_pedido, string id_estilo, string number_po, string dc, string id_po_summary, string id_talla, string store, string tipo,string tipo_empaque, string index_dc,string sobrantes,string numberppk,string packingname)
        {
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO shipping_ids(cantidad,id_tarima,used,id_pedido,id_estilo,number_po,dc,id_po_summary,id_talla,store,tipo,tipo_empaque,index_dc,label,repeticiones,sobrantes,number_ppk,packing_name)" +
                    " VALUES ('"+cantidad+ "','0','"+used+ "','"+id_pedido+ "','"+id_estilo+ "','"+number_po+ "','"+dc+ "','"+id_po_summary+ "','"+id_talla+"'," +
                    " '"+store+ "','"+tipo+ "','"+tipo_empaque+ "','"+index_dc+ "','" + label + "','" + repeticion + "','" + sobrantes + "','" + numberppk + "','" + packingname + "')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void guardar_estilos_paking_tarima(string repeticion, string label, string cantidad, string id_tarima, string used, string id_pedido, string id_estilo, string number_po, string dc, string id_po_summary, string id_talla, string store, string tipo, string tipo_empaque, string index_dc, string sobrantes, string numberppk){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO shipping_ids(cantidad,id_tarima,used,id_pedido,id_estilo,number_po,dc,id_po_summary,id_talla,store,tipo,tipo_empaque,index_dc,label,repeticiones,sobrantes,number_ppk)" +
                    " VALUES ('" + cantidad + "','"+id_tarima+"','" + used + "','" + id_pedido + "','" + id_estilo + "','" + number_po + "','" + dc + "','" + id_po_summary + "','" + id_talla + "'," +
                    " '" + store + "','" + tipo + "','" + tipo_empaque + "','" + index_dc + "','" + label + "','" + repeticion + "','" + sobrantes + "','" + numberppk + "')";
                com_c.ExecuteNonQuery();
            }
            finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }

        public void editar_estilos_paking(string shipping,string repeticion, string label, string cantidad, string id_tarima, string used, string id_pedido, string id_estilo, string number_po, string dc, string id_po_summary, string id_talla, string store, string tipo, string tipo_empaque, string index_dc, string sobrantes, string numberppk,string packing_name){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "UPDATE shipping_ids SET cantidad='" + cantidad + "',dc='" + dc + "',id_talla='" + id_talla + "'," +
                    " store='" + store + "',tipo='" + tipo + "',label='" + label + "',repeticiones='" + repeticion + "'," +
                    "sobrantes='" + sobrantes + "', packing_name='" + packing_name + "' " +
                    " WHERE id_shipping_id='" + shipping + "'  ";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }


        public void agregar_cantidades_enviadas(string summary, string talla, string cantidad, string shipping_id, string tipo, string assort,string tipo_packing) {
            Link con_c = new Link();
            try {
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO totales_envios(id_summary,id_talla,total,id_shipping_id,tipo,assort,tipo_packing) VALUES " +
                    " ('" + summary + "','" + talla + "','" + cantidad + "','" + shipping_id + "','" + tipo + "','" + assort + "','"+tipo_packing+"')";
                com_c.ExecuteNonQuery();
            } finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }

        public void agregar_cantidades_extras(int sample,int columna,int total,int talla){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO extras_samples(id_packing_sample,columna,total,id_talla) VALUES " +
                    " ('" + sample + "','" + columna + "','" + total + "','" + talla + "')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }



        //obtener_lista_dc_estilos_id
        public List<Breakdown> obtener_lista_dc_id(string po_number) {
            List<Breakdown> lista = new List<Breakdown>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct dc from shipping_ids where number_po='" + po_number + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    Breakdown e = new Breakdown();
                    e.dc = Convert.ToInt32(leerFilas["dc"]);
                    lista.Add(e);
                }
                leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<Breakdown> obtener_lista_estilos_id(string po_number) {
            List<Breakdown> lista = new List<Breakdown>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct id_estilo from shipping_ids where number_po='" + po_number + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    Breakdown e = new Breakdown();
                    e.id_estilo = Convert.ToInt32(leerFilas["id_estilo"]);
                    e.estilo = consultas.obtener_estilo(e.id_estilo);
                    e.descripcion = consultas.buscar_descripcion_estilo(e.id_estilo);
                    e.descripcion = Regex.Replace(e.descripcion, @"\s+", " ");
                    e.id_pedido = buscar_pedido_dc(e.id_estilo, po_number);
                    e.id_color = consultas.obtener_color_id_item(e.id_pedido, e.id_estilo);
                    e.codigo_color = consultas.obtener_color_id(Convert.ToString(e.id_color));
                    e.codigo_color = Regex.Replace(e.codigo_color, @"\s+", " ");
                    lista.Add(e);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public int buscar_pedido_dc(int estilo, string po_number) {
            int id = 0;
            Link con_u_r = new Link();
            try {
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT id_pedido  FROM shipping_ids where id_estilo='" + estilo + "' and number_po='" + po_number + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()) {
                    id = Convert.ToInt32(leer_u_r["id_pedido"]);
                } leer_u_r.Close();
            } finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return id;
        }

        public void agregar_id_tarima_estilo_dc(string number_po, string dc, string id, string estilo) {
            Link con_s = new Link();
            try {
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE shipping_ids set id_tarima='" + id + "' where number_po='" + number_po + "' and dc='" + dc + "' and id_estilo='" + estilo + "' ";
                com_s.ExecuteNonQuery();
            } finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }

        public void agregar_id_tarima_estilo_ppk(string cajas, string tarima, int pedido, string estilo, int summary) {
            Link con_s = new Link();
            try {
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "INSERT INTO shipping_ids(cantidad,id_tarima,id_pedido,id_estilo,id_po_summary,id_talla)values('" + cajas + "','" + tarima + "','" + pedido + "','" + estilo + "','" + summary + "','0') ";
                com_s.ExecuteNonQuery();
            } finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void agregar_id_tarima_estilo_bulpack(string tarima, int pedido, string estilo, int summary, int talla, string total) {
            Link con_s = new Link();
            try {
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "INSERT INTO shipping_ids(cantidad,id_tarima,id_pedido,id_estilo,id_po_summary,id_talla)values('" + total + "','" + tarima + "','" + pedido + "','" + estilo + "','" + summary + "','" + talla + "') ";
                com_s.ExecuteNonQuery();
            } finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public List<Drivers> obtener_drivers() {
            List<Drivers> lista = new List<Drivers>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select id_driver,carrier,driver_name,plates,scac,caat,tractor from drivers";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    Drivers e = new Drivers();
                    e.id_driver = Convert.ToInt32(leerFilas["id_driver"]);
                    e.carrier = Convert.ToString(leerFilas["carrier"]);
                    e.driver_name = Convert.ToString(leerFilas["driver_name"]);
                    e.plates = Convert.ToString(leerFilas["plates"]);
                    e.scac = Convert.ToString(leerFilas["scac"]);
                    e.caat = Convert.ToString(leerFilas["caat"]);
                    e.tractor = Convert.ToString(leerFilas["tractor"]);
                    lista.Add(e);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<Drivers> obtener_carriers()
        {
            List<Drivers> lista = new List<Drivers>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct carrier from drivers";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    Drivers e = new Drivers();
                    e.carrier = Convert.ToString(leerFilas["carrier"]);
                    lista.Add(e);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public void guardar_nuevo_conductor(string carrier, string nombre, string plates, string scac, string caat, string tractor) {
            Link con_s = new Link();
            try {
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "INSERT INTO drivers(carrier,driver_name,plates,scac,caat,tractor)values" +
                    "('" + carrier + "','" + nombre + "','" + plates + "','" + scac + "','" + caat + "','" + tractor + "') ";
                com_s.ExecuteNonQuery();
            } finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public List<Drivers> obtener_conductor_edicion(string id)
        {
            List<Drivers> lista = new List<Drivers>();
            Link conn = new Link();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select id_driver,carrier,driver_name,plates,scac,caat,tractor from drivers where id_driver='" + id + "'";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read())
                {
                    Drivers e = new Drivers();
                    e.id_driver = Convert.ToInt32(leerFilas["id_driver"]);
                    e.carrier = Convert.ToString(leerFilas["carrier"]);
                    e.driver_name = Convert.ToString(leerFilas["driver_name"]);
                    e.plates = Convert.ToString(leerFilas["plates"]);
                    e.scac = Convert.ToString(leerFilas["scac"]);
                    e.caat = Convert.ToString(leerFilas["caat"]);
                    e.tractor = Convert.ToString(leerFilas["tractor"]);
                    lista.Add(e);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public void guardar_conductor_edicion(string id, string carrier, string nombre, string plates, string scac, string caat, string tractor)
        {
            Link con_s = new Link();
            try
            {
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE drivers SET carrier='" + carrier + "', driver_name='" + nombre + "',plates='" + plates + "',scac='" + scac + "',caat='" + caat + "'," +
                    " tractor='" + tractor + "' where id_driver='" + id + "' ";
                com_s.ExecuteNonQuery();
            }
            finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }

        public List<Direccion> obtener_direcciones() {
            List<Direccion> lista = new List<Direccion>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select id_direccion,nombre,direccion,ciudad,codigo_postal from direcciones_envio ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    Direccion e = new Direccion();
                    e.id_direccion = Convert.ToInt32(leerFilas["id_direccion"]);
                    e.nombre = Convert.ToString(leerFilas["nombre"]);
                    e.direccion = Convert.ToString(leerFilas["direccion"]);
                    e.ciudad = Convert.ToString(leerFilas["ciudad"]);
                    e.zip = Convert.ToString(leerFilas["codigo_postal"]);
                    lista.Add(e);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public void guardar_nueva_direccion(string nombre, string direccion, string ciudad, string zip) {
            Link con_s = new Link();
            try {
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "INSERT INTO direcciones_envio(nombre,direccion,ciudad,codigo_postal)values" +
                    "('" + nombre + "','" + direccion + "','" + ciudad + "','" + zip + "') ";
                com_s.ExecuteNonQuery();
            } finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }

        public List<Direccion> obtener_direccion_edicion(string id) {
            List<Direccion> lista = new List<Direccion>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select id_direccion,nombre,direccion,ciudad,codigo_postal from direcciones_envio where id_direccion='" + id + "'";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    Direccion e = new Direccion();
                    e.id_direccion = Convert.ToInt32(leerFilas["id_direccion"]);
                    e.nombre = Convert.ToString(leerFilas["nombre"]);
                    e.direccion = Convert.ToString(leerFilas["direccion"]);
                    e.ciudad = Convert.ToString(leerFilas["ciudad"]);
                    e.zip = Convert.ToString(leerFilas["codigo_postal"]);
                    lista.Add(e);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public void guardar_direccion_edicion(string id, string nombre, string direccion, string ciudad, string zip) {
            Link con_s = new Link();
            try {
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE direcciones_envio SET nombre='" + nombre + "', direccion='" + direccion + "',ciudad='" + ciudad + "',codigo_postal='" + zip + "' " +
                    "  where id_direccion='" + id + "' ";
                com_s.ExecuteNonQuery();
            } finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }

        public void borrar_conductor(string id)
        {
            Link con_s = new Link();
            try
            {
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE FROM drivers  where id_driver='" + id + "' ";
                com_s.ExecuteNonQuery();
            }
            finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }

        public void borrar_direccion(string id)
        {
            Link con_s = new Link();
            try
            {
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE FROM direcciones_envio  where id_direccion='" + id + "' ";
                com_s.ExecuteNonQuery();
            }
            finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public List<Container> obtener_contenedores_select() {
            List<Container> lista = new List<Container>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select id_container,eco,plates from containers ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    Container e = new Container();
                    e.id_container = Convert.ToInt32(leerFilas["id_container"]);
                    e.eco = Convert.ToString(leerFilas["eco"]) + " " + Convert.ToString(leerFilas["plates"]);
                    lista.Add(e);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        
        public List<Drivers> obtener_conductores_select() {
            List<Drivers> lista = new List<Drivers>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select id_driver,driver_name,plates from drivers";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    Drivers e = new Drivers();
                    e.id_driver = Convert.ToInt32(leerFilas["id_driver"]);
                    e.driver_name = Convert.ToString(leerFilas["driver_name"]) + " " + Convert.ToString(leerFilas["plates"]);
                    lista.Add(e);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<Direccion> obtener_direcciones_select() {
            List<Direccion> lista = new List<Direccion>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select id_direccion,nombre from direcciones_envio";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    Direccion e = new Direccion();
                    e.id_direccion = Convert.ToInt32(leerFilas["id_direccion"]);
                    e.nombre = Convert.ToString(leerFilas["nombre"]);
                    lista.Add(e);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<Tarima> obtener_lista_tarimas_estilos(int pedido) {
            List<Tarima> lista = new List<Tarima>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct id_tarima from shipping_ids where id_pedido='" + pedido + "' and id_tarima!=0 and used=0";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    Tarima e = new Tarima();
                    e.id_tarima = Convert.ToInt32(leerFilas["id_tarima"]);
                    e.lista_estilos = buscar_lista_estilos_tarima(e.id_tarima);
                    //e.id_direccion = Convert.ToInt32(leerFilas["id_direccion"]);
                    //e.nombre = Convert.ToString(leerFilas["nombre"]);
                    lista.Add(e);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<estilos> buscar_lista_estilos_tarima(int tarima) {
            List<estilos> lista = new List<estilos>();
            Link conn = new Link(); try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select id_estilo,number_po,cantidad,dc from shipping_ids where id_tarima='" + tarima + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    estilos e = new estilos();
                    e.id_estilo = Convert.ToInt32(leerFilas["id_estilo"]);
                    e.estilo = consultas.obtener_estilo(e.id_estilo);
                    e.descripcion = consultas.buscar_descripcion_estilo(e.id_estilo);
                    e.number_po = Convert.ToString(leerFilas["number_po"]);
                    e.boxes = Convert.ToInt32(leerFilas["cantidad"]);
                    e.dc = Convert.ToString(leerFilas["dc"]);
                    lista.Add(e);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public string obtener_ultimo_pk() {
            string lista = "";
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select top 1 pk from packing_list order by id_packing_list desc ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    lista = Convert.ToString(leerFilas["pk"]);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }



        public int buscar_year_packing_configuracion(){
            int lista = 0;
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select year_packing from configuracion  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista = Convert.ToInt32(leerFilas["year_packing"]);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public int buscar_contador_packing_normal_configuracion(){
            int lista = 0;
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select contador_packing from configuracion  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista = Convert.ToInt32(leerFilas["contador_packing"]);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public int buscar_contador_packing_ext_configuracion(){
            int lista = 0;
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select contador_packing_ext from configuracion  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista = Convert.ToInt32(leerFilas["contador_packing_ext"]);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public void reset_configuracion(){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE configuracion set year_packing='" + DateTime.Now.Year + "',contador_packing=0,contador_packing_ext=0 ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void reset_year_configuracion(){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE configuracion set year_packing=year_packing+1 ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void incrementar_contador_packing_normal(){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE configuracion set contador_packing=(contador_packing+1) ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void incrementar_contador_packing_ext(){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE configuracion set contador_packing_ext=(contador_packing_ext+1) ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }




        public void guardar_pk_nuevo(int cliente,int cliente_final,int pedido, string pk, string address, string driver, string container, string seal, string replacement, string manager, string tipo, int usuario, string num_envio,string packing_type, string parte)
        {
            Link con_c = new Link();
            try {
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO packing_list(pk,id_direccion_envio,id_driver,id_container,shipping_manager,seal,replacement,fecha," +
                    "tipo,id_usuario,envio,id_pedido,id_customer,id_customer_po,parte,id_packing_type) VALUES " +
                    " ('" + pk + "','" + address + "','" + driver + "','" + container + "','" + manager + "','" + seal + "','" + replacement + "'," +
                    "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                    "'" + tipo + "','" + usuario + "','" + num_envio + "','"+pedido+"','"+cliente+"','"+cliente_final+"','"+parte+"','"+ packing_type + "')";
                com_c.ExecuteNonQuery();
            } finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }

        public int obtener_ultimo_pk_registrado() {
            int lista = 0;
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select top 1 id_packing_list from packing_list order by id_packing_list desc ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    lista = Convert.ToInt32(leerFilas["id_packing_list"]);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public int obtener_ultimo_shipping_registrado() {
            int lista = 0;
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select top 1 id_shipping_id from shipping_ids order by id_shipping_id desc ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    lista = Convert.ToInt32(leerFilas["id_shipping_id"]);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public void guardar_pk_tarima(string tarima, int pk) {
            Link con_s = new Link();
            try {
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE shipping_ids SET used='" + pk + "' where id_tarima='" + tarima + "' ";
                com_s.ExecuteNonQuery();
            } finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void guardar_pk_labels(string label, int pk, string type_labels) {
            Link con_s = new Link();
            try {
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "INSERT INTO ucc_labels(label,id_packing,tipo) VALUES('" + label + "','" + pk + "','" + type_labels + "') ";
                com_s.ExecuteNonQuery();
            } finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        //*******************************************************************************************************************************************************************************
        //*******************************************************************************************************************************************************************************
        public int summary, id_pedido;
        public List<Pk> obtener_packing_list(int pk) {
            List<Pk> lista = new List<Pk>();
            Link connx = new Link();
            try {
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "select nombre_archivo,id_pedido,id_packing_list,pk,id_customer,id_customer_po,id_direccion_envio,id_driver,id_container,shipping_manager,seal,replacement,fecha,tipo,id_packing_type,parte from packing_list where id_packing_list='" + pk + "' ";
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()) {
                    Pk p = new Pk(); //Regex.Replace(color, @"\s+", " ");
                    p.id_packing_list = Convert.ToInt32(leerFilasx["id_packing_list"]);
                    p.id_packing_type = Convert.ToInt32(leerFilasx["id_packing_type"]);
                    p.packing = Convert.ToString(leerFilasx["pk"]);
                    p.nombre_archivo = Convert.ToString(leerFilasx["nombre_archivo"]);
                    p.id_customer = Convert.ToInt32(leerFilasx["id_customer"]);
                    p.id_customer_po = Convert.ToInt32(leerFilasx["id_customer_po"]);
                    p.customer = Regex.Replace(consultas.obtener_customer_id(Convert.ToString(leerFilasx["id_customer"])), @"\s+", " ");
                    p.customer_po = Regex.Replace(consultas.obtener_customer_final_id(Convert.ToString(leerFilasx["id_customer_po"])), @"\s+", " ");
                    p.destino = obtener_direccion(Convert.ToInt32(leerFilasx["id_direccion_envio"]));                   
                    p.conductor = obtener_driver(Convert.ToInt32(leerFilasx["id_driver"]));
                    p.contenedor = obtener_contenedor(Convert.ToInt32(leerFilasx["id_container"]));
                    p.shipping_manager = Convert.ToString(leerFilasx["shipping_manager"]);
                    p.seal = Convert.ToString(leerFilasx["seal"]);
                    p.replacement = Convert.ToString(leerFilasx["replacement"]);
                    p.fecha = (Convert.ToDateTime(leerFilasx["fecha"])).ToString("MM/dd/yyyy");
                    //p.part = Convert.ToInt32(leerFilasx["parte"]);
                    //p.parte = consultas.AddOrdinal(Convert.ToInt32(leerFilasx["parte"])) + " Part"; 
                    p.id_pedido = Convert.ToInt32(leerFilasx["id_pedido"]);
                    p.id_tipo = Convert.ToInt32(leerFilasx["id_packing_type"]);
                    if (p.id_tipo != 8 && p.id_tipo != 4){
                        p.pedido = consultas.obtener_po_id((p.id_pedido).ToString());
                        //id_customer_po = consultas.obtener_customer_final_po(p.id_pedido);
                    }else{
                        p.pedido = consultas.obtener_po_id_fantasy((p.id_pedido).ToString());
                       // id_customer_po = Convert.ToInt32(leerFilasx["id_customer_po"]);
                    }
                    p.number_po = buscar_number_po_pedido(Convert.ToInt32(leerFilasx["id_pedido"]));
                    p.tipo = leerFilasx["tipo"].ToString();//TIPO DE PACKING LIST                   
                    //p.siglas_cliente = consultas.obtener_siglas_cliente(Convert.ToString(leerFilasx["id_customer_po"]));                    
                    //p.pedido = consultas.obtener_po_id(Convert.ToString(p.id_pedido));
                    p.id_tipo = Convert.ToInt32(leerFilasx["id_packing_type"]);
                    switch (p.id_tipo) {
                        case 1:
                        case 2:
                        case 7:
                        case 9:
                            p.lista_tarimas = obtener_tarimas(p.id_packing_list);                       
                            break;
                        case 3:
                            p.lista_samples = obtener_lista_samples_tarima(p.id_packing_list);
                            break;
                        case 5:
                            p.lista_tarimas = obtener_tarimas_returns(p.id_packing_list);                            
                            break;
                        case 6: 
                            p.lista_tarimas = obtener_tarimas_extras_fantasy(p.id_packing_list);                            
                            break;
                        case 8:
                            p.lista_estilos = obtener_lista_estilos_tarima(0, pk);                            
                            break;
                        case 4:
                            p.lista_tarimas = obtener_tarimas(p.id_packing_list);                            
                            break;
                    }                    
                    if (p.id_tipo != 3 && p.id_tipo != 8) {  if (p.lista_tarimas.Count != 0) {lista.Add(p); }  }else { lista.Add(p); }
                } leerFilasx.Close();
            } finally { connx.CerrarConexion(); connx.Dispose(); }
            return lista;
        }
        public string buscar_consecutivo(int pk){
            string tempo = "";
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select parte from packing_list where id_packing_list='" + pk + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo = Convert.ToString(leer["parte"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }
        public Direccion obtener_direccion(int direccion) {
            Direccion d = new Direccion();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select nombre,direccion,ciudad,codigo_postal from direcciones_envio where id_direccion='" + direccion + "'";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    d.nombre = Convert.ToString(leerFilas["nombre"]);
                    d.direccion = Convert.ToString(leerFilas["direccion"]);
                    d.ciudad = Convert.ToString(leerFilas["ciudad"]);
                    d.zip = Convert.ToString(leerFilas["codigo_postal"]);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return d;
        }
        public Drivers obtener_driver(int driver) {
            Drivers d = new Drivers();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select carrier,driver_name,plates,scac,caat,tractor from drivers where id_driver='" + driver + "'";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    d.carrier = Convert.ToString(leerFilas["carrier"]);
                    d.driver_name = Convert.ToString(leerFilas["driver_name"]);
                    d.plates = Convert.ToString(leerFilas["plates"]);
                    d.scac = Convert.ToString(leerFilas["scac"]);
                    d.caat = Convert.ToString(leerFilas["caat"]);
                    d.tractor = Convert.ToString(leerFilas["tractor"]);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return d;
        }
        public Container obtener_contenedor(int container) {
            Container d = new Container();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select eco,plates from containers where id_container='" + container + "'";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    d.eco = Convert.ToString(leerFilas["eco"]);
                    d.plates = Convert.ToString(leerFilas["plates"]);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return d;
        }
        public string buscar_dc_pk(int pk) {////TAL VEZ AQUI TENGA QUE ACOMODAR PARA LOS EXCEL
            string tempo = "";
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT dc from shipping_ids where used='" + pk + "' and dc!='0' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    tempo = Convert.ToString(leer["dc"]);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }

        public int contar_labels(int pk) {
            int tempo = 0;
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT tipo from ucc_labels where id_packing='" + pk + "'  ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    tempo = Convert.ToInt32(leer["tipo"]);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }

        public List<Labels> obtener_etiquetas(int pk) {
            List<Labels> lista = new List<Labels>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select id_label,label,tipo from ucc_labels where id_packing='" + pk + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    Labels e = new Labels();
                    e.id_label = Convert.ToInt32(leerFilas["id_label"]);
                    e.label = Convert.ToString(leerFilas["label"]);
                    e.tipo = Convert.ToString(leerFilas["tipo"]);
                    lista.Add(e);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<Tarima> obtener_tarimas(int pk) {
            List<Tarima> lista = new List<Tarima>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct id_tarima from shipping_ids where used='" + pk + "'  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    if (Convert.ToInt32(leerFilas["id_tarima"]) != 0) {
                        Tarima e = new Tarima();
                        e.id_tarima = Convert.ToInt32(leerFilas["id_tarima"]);
                        e.lista_estilos = obtener_lista_estilos_tarima(e.id_tarima,pk);
                        lista.Add(e);
                    }
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<Tarima> obtener_tarimas_samples(int pk){
            List<Tarima> lista = new List<Tarima>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct id_tarima from packing_samples where id_packing_list='" + pk + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    //if (Convert.ToInt32(leerFilas["id_tarima"]) != 0){
                        Tarima e = new Tarima();
                        e.id_tarima = Convert.ToInt32(leerFilas["id_tarima"]);
                        e.lista_samples = obtener_lista_samples_tarima(e.id_tarima);
                        lista.Add(e);
                   // }
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<Tarima> obtener_tarimas_returns(int pk){
            List<Tarima> lista = new List<Tarima>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct id_tarima from packing_returns where id_packing='" + pk + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    if (Convert.ToInt32(leerFilas["id_tarima"]) != 0){
                        Tarima e = new Tarima();
                        e.id_tarima = Convert.ToInt32(leerFilas["id_tarima"]);
                        e.lista_returns = obtener_lista_returns_tarima(e.id_tarima);
                        lista.Add(e);
                    }
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<Tarima> obtener_tarimas_extras_fantasy(int pk)
        {
            List<Tarima> lista = new List<Tarima>();
            Link conn = new Link();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct id_tarima from packing_fantasy where id_packing='" + pk + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read())
                {
                    if (Convert.ToInt32(leerFilas["id_tarima"]) != 0)
                    {
                        Tarima e = new Tarima();
                        e.id_tarima = Convert.ToInt32(leerFilas["id_tarima"]);
                        e.lista_fantasy = obtener_lista_extras_fantasy_tarima(e.id_tarima);
                        lista.Add(e);
                    }
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<Tarima> obtener_tarimas_assort(int pk, int tipo_empaque) {
            List<Tarima> lista = new List<Tarima>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct id_tarima from shipping_ids where used='" + pk + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    if (Convert.ToInt32(leerFilas["id_tarima"]) != 0) {
                        Tarima e = new Tarima();
                        e.id_tarima = Convert.ToInt32(leerFilas["id_tarima"]);
                        e.lista_assortments = obtener_lista_assort(e.id_tarima);
                        //e.lista_estilos = obtener_lista_estilos_tarima(e.id_tarima, tipo_empaque);
                        lista.Add(e);
                    }
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<Assortment> obtener_lista_assort(int tarima) {
            List<Assortment> lista = new List<Assortment>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select id_estilo,id_shipping_id,number_po,store,tipo,id_talla,cantidad,id_po_summary from shipping_ids where id_tarima='" + tarima + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    Assortment a = new Assortment();
                    a = obtener_assortment(Convert.ToInt32(leerFilas["id_estilo"]), tarima, Convert.ToInt32(leerFilas["id_shipping_id"]), Convert.ToInt32(leerFilas["number_po"]), Convert.ToString(leerFilas["store"]), Convert.ToString(leerFilas["tipo"]), Convert.ToInt32(leerFilas["id_talla"]), Convert.ToInt32(leerFilas["cantidad"]), Convert.ToInt32(leerFilas["id_po_summary"]));

                    lista.Add(a);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public Assortment obtener_assortment(int id, int tarima, int id_shipping, int number_po, string store, string tipo, int id_talla, int cantidad, int po_summary) {//OBTENER EL ASSORTMENT CON ESTILOS
            Assortment a = new Assortment();
            List<estilos> lista = new List<estilos>();
            string packing_name = buscar_packing_name(id);
            if (tipo != "NONE" && tipo != "INITIAL") {
                estilos e = new estilos();
                a.id_summary = po_summary;
                a.nombre = " ";
                a.cartones = 1;
                e.lista_ratio = obtener_lista_ratio_otros(id_shipping);
                e.id_estilo = consultas.obtener_estilo_summary(a.id_summary);
                e.id_po_summary = a.id_summary;
                e.id_color = consultas.obtener_color_id_item_cat(a.id_summary);
                e.color = Regex.Replace(consultas.obtener_color_id((e.id_color).ToString()), @"\s+", " ");
                e.estilo = Regex.Replace(consultas.obtener_estilo(e.id_estilo), @"\s+", " ");
                e.descripcion = Regex.Replace(consultas.buscar_descripcion_estilo(e.id_estilo), @"\s+", " ");
                e.number_po = (number_po).ToString();
                e.boxes = cantidad;//cantidad de cartones
                e.dc = "0";
                e.descripcion_final = Regex.Replace(buscar_descripcion_final_estilo(a.id_summary), @"\s+", " ");
                e.tipo = tipo;
                e.store = store;
                e.id_talla = id_talla;
                if (e.id_talla != 0) { e.talla = consultas.obtener_size_id(Convert.ToString(e.id_talla)); }
                lista.Add(e);
            } else {
                Link conna1 = new Link();
                try {
                    SqlCommand comandoa1 = new SqlCommand();
                    SqlDataReader leerFilasa1 = null;
                    comandoa1.Connection = conna1.AbrirConexion();
                    comandoa1.CommandText = "select distinct PTZ.ID_SUMMARY from PACKING_ASSORT PA,PACKING_TYPE_SIZE PTZ where " +
                        " PA.PACKING_NAME=PTZ.PACKING_NAME and PA.ID_PACKING_ASSORT='" + id + "'   ";
                    leerFilasa1 = comandoa1.ExecuteReader();
                    while (leerFilasa1.Read()) {
                        estilos e = new estilos();
                        a.id_summary = Convert.ToInt32(leerFilasa1["ID_SUMMARY"]);
                        a.nombre = buscar_nombre_assort(id);
                        a.cartones = cantidad;
                        e.lista_ratio = obtener_lista_ratio_assort(a.id_summary, e.id_estilo, packing_name);
                        e.id_estilo = consultas.obtener_estilo_summary(a.id_summary);
                        e.id_po_summary = a.id_summary;
                        e.id_color = consultas.obtener_color_id_item_cat(a.id_summary);
                        e.color = Regex.Replace(consultas.obtener_color_id((e.id_color).ToString()), @"\s+", " ");
                        e.estilo = Regex.Replace(consultas.obtener_estilo(e.id_estilo), @"\s+", " ");
                        e.descripcion = Regex.Replace(consultas.buscar_descripcion_estilo(e.id_estilo), @"\s+", " ");
                        e.number_po = (number_po).ToString();
                        e.boxes = cantidad;//cantidad de cartones
                        e.dc = "0";
                        e.descripcion_final = Regex.Replace(buscar_descripcion_final_estilo(a.id_summary), @"\s+", " ");
                        e.tipo = tipo;
                        e.store = store;
                        e.id_talla = id_talla;
                        if (e.id_talla != 0) { e.talla = consultas.obtener_size_id(Convert.ToString(e.id_talla)); }
                        lista.Add(e);
                    } leerFilasa1.Close();
                } finally { conna1.CerrarConexion(); conna1.Dispose(); }
            }
            a.lista_estilos = lista;
            return a;
        }
        public List<ratio_tallas> obtener_lista_ratio_assort(int posummary, int estilo, string packing_name) {
            List<ratio_tallas> lista = new List<ratio_tallas>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select ID_TALLA,RATIO from PACKING_TYPE_SIZE where ID_SUMMARY='" + posummary + "' and PACKING_NAME='" + packing_name + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    ratio_tallas e = new ratio_tallas();
                    e.id_estilo = estilo;
                    e.id_talla = Convert.ToInt32(leerFilas["ID_TALLA"]);
                    e.talla = Regex.Replace(consultas.obtener_size_id(Convert.ToString(leerFilas["ID_TALLA"])), @"\s+", " ");
                    e.ratio = Convert.ToInt32(leerFilas["RATIO"]);
                    lista.Add(e);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<estilos> obtener_lista_estilos_tarima(int tarima,int pk) {
            List<estilos> lista = new List<estilos>();
            Link conn = new Link();
            try { //Regex.Replace(color, @"\s+", " ");
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select repeticiones,id_shipping_id,id_estilo,number_po,cantidad,dc,id_po_summary,id_talla,store,tipo,tipo_empaque," +
                    "index_dc,dc,id_pedido,label,sobrantes,number_ppk,packing_name from shipping_ids where id_tarima='" + tarima + "' and used='"+pk+"' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    estilos e = new estilos();
                    e.id_estilo = Convert.ToInt32(leerFilas["id_estilo"]);
                    e.tipo_empaque = Convert.ToInt32(leerFilas["tipo_empaque"]);
                    //summary = consultas.obtener_po_summary(Convert.ToInt32(leerFilas["id_pedido"]), e.id_estilo);
                    e.id_po_summary = Convert.ToInt32(leerFilas["id_po_summary"]);
                    e.id_color = consultas.obtener_color_id_item_cat(e.id_po_summary);
                    e.color = Regex.Replace(consultas.obtener_color_id((e.id_color).ToString()), @"\s+", " ");
                    e.estilo = Regex.Replace(consultas.obtener_estilo(e.id_estilo), @"\s+", "")+buscar_terminacion_estilo(e.id_po_summary,e.tipo_empaque);
                    e.descripcion = Regex.Replace(consultas.buscar_descripcion_estilo(e.id_estilo), @"\s+", " ");
                    e.number_po = Convert.ToString(leerFilas["number_po"]);
                    e.boxes = Convert.ToInt32(leerFilas["cantidad"]);
                    e.dc = Convert.ToString(leerFilas["dc"]);
                    e.descripcion_final = Regex.Replace(buscar_descripcion_final_estilo(e.id_po_summary), @"\s+", " ");
                    e.tipo = Convert.ToString(leerFilas["tipo"]);                    
                    e.label = Convert.ToString(leerFilas["label"]);
                    e.pedido = Regex.Replace(consultas.obtener_po_id(Convert.ToString(consultas.obtener_id_pedido_summary(e.id_po_summary))), @"\s+", " ");
                    e.id_talla = Convert.ToInt32(leerFilas["id_talla"]);
                    e.usado = 0;
                    e.number_ppk = Convert.ToInt32(leerFilas["number_ppk"]);
                    e.sobrantes = Convert.ToInt32(leerFilas["sobrantes"]);
                    if (e.id_talla != 0) {
                         e.talla = consultas.obtener_size_id(Convert.ToString(e.id_talla));
                         e.piezas = buscar_cajas_talla_estilo(summary, e.id_talla);
                    }
                    e.store = Convert.ToString(leerFilas["store"]);
                    e.packing_name = Convert.ToString(leerFilas["packing_name"]);
                    e.dc = Convert.ToString(leerFilas["dc"]);
                   // e.ext = Convert.ToString(leerFilas["ext"]);
                    
                    e.index_dc = Convert.ToInt32(leerFilas["index_dc"]);
                    e.repeticiones = Convert.ToInt32(leerFilas["repeticiones"]);
                    if (e.tipo == "DMG" || e.tipo == "EXT" || e.tipo == "EXAMPLES") {
                        e.lista_ratio = obtener_lista_ratio_otros(Convert.ToInt32(leerFilas["id_shipping_id"]));
                    }else{
                        if (e.tipo_empaque ==1){
                            e.lista_ratio = obtener_lista_ratio(e.id_po_summary, e.id_estilo,1);
                        }
                        if (e.tipo_empaque ==2){
                            e.lista_ratio = obtener_lista_ratio(e.id_po_summary, e.id_estilo,2);
                        }
                        if (e.tipo_empaque == 3){
                            e.assort = assortment_id(e.id_po_summary, Convert.ToInt32(leerFilas["id_pedido"]));
                            e.assort_nombre = obtener_nombre_assort(e.id_po_summary);
                        }
                        if (e.tipo_empaque == 4){
                            e.lista_ratio = obtener_lista_ratio_ppks(e.id_po_summary, e.id_estilo, 4, e.number_ppk, e.packing_name);
                        }
                        if (e.tipo_empaque == 5){
                            e.lista_ratio = obtener_lista_ratio_bps(e.id_po_summary, e.id_estilo, 5, e.packing_name);
                        }
                    }
                    e.number_po_ht = buscar_number_po(e.id_po_summary);                    
                    lista.Add(e);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<estilos> buscar_lista_estilos_pedido(int pk){
            List<estilos> lista = new List<estilos>();
            Link conn = new Link();
            try{ //Regex.Replace(color, @"\s+", " ");
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select repeticiones,id_shipping_id,id_estilo,number_po,cantidad,dc,id_po_summary,id_talla,store,tipo,tipo_empaque," +
                    "index_dc,dc,id_pedido,label,sobrantes,number_ppk,id_tarima,packing_name from shipping_ids where used='" + pk + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    estilos e = new estilos();
                    e.id_estilo = Convert.ToInt32(leerFilas["id_estilo"]);
                    e.tipo_empaque = Convert.ToInt32(leerFilas["tipo_empaque"]);
                    e.id_tarima = Convert.ToInt32(leerFilas["id_tarima"]);
                    //summary = consultas.obtener_po_summary(Convert.ToInt32(leerFilas["id_pedido"]), e.id_estilo);
                    e.id_po_summary = Convert.ToInt32(leerFilas["id_po_summary"]);
                    summary = e.id_po_summary;
                    e.id_color = consultas.obtener_color_id_item_cat(e.id_po_summary);
                    e.color = Regex.Replace(consultas.obtener_color_id((e.id_color).ToString()), @"\s+", " ");
                    e.estilo = Regex.Replace(consultas.obtener_estilo(e.id_estilo), @"\s+", "") + buscar_terminacion_estilo(e.id_po_summary, e.tipo_empaque);
                    e.descripcion = Regex.Replace(consultas.buscar_descripcion_estilo(e.id_estilo), @"\s+", " ");
                    e.number_po = Convert.ToString(leerFilas["number_po"]);
                    e.boxes = Convert.ToInt32(leerFilas["cantidad"]);
                    e.dc = Convert.ToString(leerFilas["dc"]);
                    e.descripcion_final = Regex.Replace(buscar_descripcion_final_estilo(summary), @"\s+", " ");
                    e.tipo = Convert.ToString(leerFilas["tipo"]);
                    e.label = Convert.ToString(leerFilas["label"]);
                    e.packing_name = Convert.ToString(leerFilas["packing_name"]);
                    e.pedido = Regex.Replace(consultas.obtener_po_id(Convert.ToString(consultas.obtener_id_pedido_summary(e.id_po_summary))), @"\s+", " ");
                    e.id_talla = Convert.ToInt32(leerFilas["id_talla"]);
                    e.usado = 0;
                    //e.number_ppk = Convert.ToInt32(leerFilas["number_ppk"]);
                    e.sobrantes = Convert.ToInt32(leerFilas["sobrantes"]);
                    if (e.id_talla != 0){
                        e.talla = consultas.obtener_size_id(Convert.ToString(e.id_talla));
                        e.piezas = buscar_cajas_talla_estilo(summary, e.id_talla);
                    }
                    e.store = Convert.ToString(leerFilas["store"]);
                    e.dc = Convert.ToString(leerFilas["dc"]);
                    // e.ext = Convert.ToString(leerFilas["ext"]);
                    e.index_dc = Convert.ToInt32(leerFilas["index_dc"]);
                    e.repeticiones = Convert.ToInt32(leerFilas["repeticiones"]);
                    if (e.tipo == "DMG" || e.tipo == "EXT" || e.tipo == "EXAMPLES"){
                        e.lista_ratio = obtener_lista_ratio_otros(Convert.ToInt32(leerFilas["id_shipping_id"]));
                    }else{
                        if (e.tipo_empaque == 1){
                            e.lista_ratio = obtener_lista_ratio(e.id_po_summary, e.id_estilo, 1);
                        }
                        if (e.tipo_empaque == 2){
                            e.lista_ratio = obtener_lista_ratio(e.id_po_summary, e.id_estilo, 2);
                        }
                        if (e.tipo_empaque == 3){
                            e.assort = assortment_id(e.id_po_summary, Convert.ToInt32(leerFilas["id_pedido"]));
                            e.assort_nombre = obtener_nombre_assort(e.id_po_summary);
                        }
                        if (e.tipo_empaque == 4){
                            e.lista_ratio = obtener_lista_ratio_ppks(e.id_po_summary, e.id_estilo, 4, e.number_ppk, e.packing_name);
                        }
                        if (e.tipo_empaque == 5){
                            e.lista_ratio = obtener_lista_ratio_bps(e.id_po_summary, e.id_estilo, 5, e.packing_name);
                        }
                    }
                    //e.number_po_ht = buscar_number_po(e.id_po_summary);
                    lista.Add(e);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }




        int id_origen=0, id_porcentaje=0;
        string origen, porcentaje;
        public List<Sample> obtener_lista_samples_tarima(int pk){
            List<Sample> lista = new List<Sample>();
            Link conn = new Link();
            try{ //Regex.Replace(color, @"\s+", " ");
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_sample,id_packing_list,id_summary,talla_xs,talla_s,talla_m,talla_l,talla_xl,talla_2x,talla_3x," +
                    " cajas,attnto,id_customer,id_tarima,inicial,tipo FROM packing_samples WHERE id_packing_list='" + pk + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    Sample s = new Sample();   
                    s.tipo_sample= Convert.ToInt32(leerFilas["tipo"]);
                    if (s.tipo_sample == 0){
                        s.id_summary = Convert.ToInt32(leerFilas["id_summary"]);
                        int id_estilo = consultas.obtener_estilo_summary(s.id_summary);
                        s.id_color = consultas.obtener_color_id_item_cat(s.id_summary);
                        s.color = Regex.Replace(consultas.obtener_color_id((s.id_color).ToString()), @"\s+", " ");
                        List<string> tipo_empaque_temporal = consultas.buscar_tipo_empaque_extra(s.id_summary);
                        foreach (string empaque in tipo_empaque_temporal){
                            s.estilo = Regex.Replace(consultas.obtener_estilo(id_estilo), @"\s+", "") + buscar_terminacion_estilo(s.id_summary, Convert.ToInt32(empaque));
                        }
                        s.id_pedido = consultas.obtener_id_pedido_summary(s.id_summary);
                        s.pedido = consultas.obtener_po_id((s.id_pedido).ToString());
                        //s.number_po = buscar_number_po(s.id_pedido);
                        s.number_po = buscar_number_po_pedido(s.id_pedido);
                        s.descripcion = Regex.Replace(consultas.buscar_descripcion_estilo(id_estilo), @"\s+", " ");
                        buscar_origen_summary(s.id_summary);
                        s.origen = origen;
                        s.porcentaje = porcentaje;
                        s.id_genero = consultas.buscar_genero_summary(s.id_summary);
                        //s.genero = consultas.obtener_genero_id(Convert.ToString(s.id_genero));
                        s.genero = (buscar_descripcion_final_estilo(s.id_summary)).Trim();
                    }else {
                        s.nuevo_estilo = buscar_estilo_nuevo_ejemplo(Convert.ToInt32(leerFilas["id_summary"]));
                        s.id_summary= Convert.ToInt32(leerFilas["id_summary"]);
                        s.id_color = s.nuevo_estilo.id_color;
                        s.color = Regex.Replace(s.nuevo_estilo.color, @"\s+", " ");
                        s.estilo = Regex.Replace(s.nuevo_estilo.estilo, @"\s+", "");
                        s.id_pedido = 0;
                        s.pedido = s.nuevo_estilo.orden;
                        //s.number_po = buscar_number_po(s.id_pedido);
                        s.number_po = "N/A";
                        s.descripcion = Regex.Replace(s.nuevo_estilo.descripcion, @"\s+", " ");
                        s.origen = s.nuevo_estilo.origen;
                        s.porcentaje = s.nuevo_estilo.percent;
                        //s.id_genero = s.nuevo_estilo.id_genero;
                        s.genero = (s.nuevo_estilo.genero).Trim();
                    }
                    s.id_sample = Convert.ToInt32(leerFilas["id_sample"]);
                    s.talla_xs= Convert.ToInt32(leerFilas["talla_xs"]);
                    s.talla_s= Convert.ToInt32(leerFilas["talla_s"]);
                    s.talla_m= Convert.ToInt32(leerFilas["talla_m"]);
                    s.talla_l= Convert.ToInt32(leerFilas["talla_l"]);
                    s.talla_xl= Convert.ToInt32(leerFilas["talla_xl"]);
                    s.talla_2x= Convert.ToInt32(leerFilas["talla_2x"]);
                    s.talla_3x= Convert.ToInt32(leerFilas["talla_3x"]);
                    s.cajas= Convert.ToInt32(leerFilas["cajas"]);
                    s.total = s.talla_s + s.talla_xs + s.talla_m + s.talla_l + s.talla_xl + s.talla_2x + s.talla_3x;
                    s.id_customer = Convert.ToInt32(leerFilas["id_customer"]);
                    s.customer = consultas.obtener_customer_final_id(Convert.ToString(s.id_customer));
                    s.attnto = Convert.ToString(leerFilas["attnto"]);                    
                    s.inicial = Convert.ToInt32(leerFilas["inicial"]);
                    s.id_tarima= Convert.ToInt32(leerFilas["id_tarima"]);
                    
                    s.cols_extras = obtener_columnas_extras(pk);
                    s.total_extras = (s.cols_extras).Count();
                    s.cabeceras = buscar_cabeceras_sample(pk);
                    if (s.total_extras != 0){
                        s.lista_extras = obtener_extras_sample(s.id_sample);
                    }
                    lista.Add(s);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public Nuevo_estilo_ejemplo buscar_estilo_nuevo_ejemplo(int id){
            Nuevo_estilo_ejemplo e = new Nuevo_estilo_ejemplo();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_nuevo,orden,estilo,descripcion,color,porcentaje,genero,origen  " +
                    " from ejemplos_nuevos_fantasy where id_nuevo=" + id;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    e.id_nuevo = Convert.ToInt32(leer["id_nuevo"]);
                    e.orden = Convert.ToString(leer["orden"]);
                    e.estilo = Convert.ToString(leer["estilo"]);
                    e.descripcion = Convert.ToString(leer["descripcion"]);
                    e.color= Convert.ToString(leer["color"]);
                    e.percent= Convert.ToString(leer["porcentaje"]);
                    e.origen= Convert.ToString(leer["origen"]);
                    e.genero= Convert.ToString(leer["genero"]);                                     
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return e;
        }

        public List<Sample> obtener_lista_extras_fantasy_tarima(int tarima)
        {
            List<Sample> lista = new List<Sample>();
            Link conn = new Link();
            try
            { //Regex.Replace(color, @"\s+", " ");
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_fantasy,id_packing,id_summary,talla_s,talla_m,talla_l,talla_xl,talla_2x,cajas,tipo " +
                    " FROM packing_fantasy WHERE id_tarima='" + tarima + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    Sample s = new Sample();
                    s.id_fantasy = Convert.ToInt32(leerFilas["id_fantasy"]);
                    s.id_summary = Convert.ToInt32(leerFilas["id_summary"]);
                    s.tipo = Convert.ToString(leerFilas["tipo"]);
                    int id_estilo = consultas.obtener_estilo_summary(s.id_summary);
                    s.id_color = consultas.obtener_color_id_item_cat(s.id_summary);
                    s.color = Regex.Replace(consultas.obtener_color_id((s.id_color).ToString()), @"\s+", " ");
                    s.estilo = Regex.Replace(consultas.obtener_estilo(id_estilo), @"\s+", " ");
                    s.descripcion = Regex.Replace(consultas.buscar_descripcion_estilo(id_estilo), @"\s+", " ");                                  
                    s.talla_s = Convert.ToInt32(leerFilas["talla_s"]);
                    s.talla_m = Convert.ToInt32(leerFilas["talla_m"]);
                    s.talla_l = Convert.ToInt32(leerFilas["talla_l"]);
                    s.talla_xl = Convert.ToInt32(leerFilas["talla_xl"]);
                    s.talla_2x = Convert.ToInt32(leerFilas["talla_2x"]);
                    s.cajas = Convert.ToInt32(leerFilas["cajas"]);
                    s.total = s.talla_s + s.talla_m + s.talla_l + s.talla_xl + s.talla_2x;
                    s.id_customer = 2;
                    s.customer = consultas.obtener_customer_final_id(Convert.ToString(s.id_customer));
                    origen = ""; porcentaje = "";
                    buscar_origen_summary(s.id_summary);
                    s.origen = origen;
                    s.porcentaje = porcentaje;
                    s.id_pedido = consultas.obtener_id_pedido_summary(s.id_summary);
                    s.pedido = consultas.obtener_po_id((s.id_pedido).ToString());
                    //s.number_po = buscar_number_po(s.id_pedido);
                    s.number_po = buscar_number_po_pedido(s.id_pedido);
                    s.id_genero = consultas.buscar_genero_summary(s.id_summary);
                    //s.genero = consultas.obtener_genero_id(Convert.ToString(s.id_genero));
                    s.genero = Regex.Replace(buscar_descripcion_final_estilo(summary), @"\s+", " ");
                    lista.Add(s);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }


        public List<Return> obtener_lista_returns_tarima(int tarima){
            DatosTrim dtrim = new DatosTrim();
            List<Return> lista = new List<Return>();
            Link conn = new Link();
            try{ //Regex.Replace(color, @"\s+", " ");
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_return,id_packing,id_item,id_inventario,id_talla,id_categoria,id_summary,total,cajas" +
                    "  FROM packing_returns WHERE id_tarima='" + tarima + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    Return r = new Return();
                    r.id_return = Convert.ToInt32(leerFilas["id_return"]);
                    r.id_item = Convert.ToInt32(leerFilas["id_item"]);
                    r.item = dtrim.obtener_componente_item(r.id_item);//AMT
                    r.descripcion_item = dtrim.obtener_descripcion_item(r.id_item);
                    r.id_talla= Convert.ToInt32(leerFilas["id_talla"]);
                    r.talla = consultas.obtener_size_id((r.id_talla).ToString() );
                    r.amt = r.item;
                    r.id_categoria = Convert.ToInt32(leerFilas["id_categoria"]);
                    r.id_inventario = Convert.ToInt32(leerFilas["id_inventario"]);
                    r.id_summary = Convert.ToInt32(leerFilas["id_summary"]);
                    int id_estilo = consultas.obtener_estilo_summary(r.id_summary);
                    r.estilo = Regex.Replace(consultas.obtener_estilo(id_estilo), @"\s+", " ");
                    r.descripcion = Regex.Replace(consultas.buscar_descripcion_estilo(id_estilo), @"\s+", " ");
                    r.total = Convert.ToInt32(leerFilas["total"]);
                    r.cajas = Convert.ToInt32(leerFilas["cajas"]);
                    r.id_color = consultas.obtener_color_id_item_cat(r.id_summary);
                    r.color = Regex.Replace(consultas.obtener_color_id((r.id_color).ToString()), @"\s+", " ");                           
                    r.cajas = Convert.ToInt32(leerFilas["cajas"]);                    
                    r.id_pedido = consultas.obtener_id_pedido_summary(r.id_summary);
                    r.pedido = consultas.obtener_po_id((r.id_pedido).ToString());
                    r.id_genero = consultas.buscar_genero_summary(r.id_summary);
                    r.genero = consultas.obtener_genero_id(Convert.ToString(r.id_genero));
                    lista.Add(r);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public void buscar_origen_summary(int po_summary){            
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT distinct sc.id_pais,sc.id_porcentaje FROM staging_count sc,staging s,inventario i " +
                    " WHERE sc.id_staging=s.id_staging AND s.id_summary=" + po_summary;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    id_origen = Convert.ToInt32(leer["id_pais"]);
                    id_porcentaje = Convert.ToInt32(leer["id_porcentaje"]);
                    porcentaje =  " "+consultas.obtener_fabric_percent_id(id_porcentaje.ToString());
                    origen = " "+consultas.obtener_pais_id(id_origen.ToString());
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }            
        }

        public string obtener_nombre_assort(int po_summary){
            string cadena = "";
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT PTS.ASSORT_NAME from PACKING_ASSORT PA,PACKING_TYPE_SIZE PTS where PA.ID_PACKING_ASSORT='" + po_summary + "'" +
                    " and PA.PACKING_NAME=PTS.PACKING_NAME ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    cadena = Convert.ToString(leer["ASSORT_NAME"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return cadena;
        }
        public string buscar_descripcion_final_estilo(int estilo)
        {
            string cadena = "";
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT ID_GENDER,ID_TELA,ID_PRODUCT_TYPE from PO_SUMMARY where ID_PO_SUMMARY='" + estilo + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    cadena = (consultas.obtener_sigla_genero(Convert.ToString(leer["ID_GENDER"]))).Trim() + " " + (consultas.obtener_sigla_product_type(Convert.ToString(leer["ID_PRODUCT_TYPE"]))).Trim() + " " + (consultas.obtener_sigla_fabric(Convert.ToString(leer["ID_TELA"]))).Trim();
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return cadena;
        }
        public string buscar_terminacion_estilo(int summary,int tipo_empaque)
        {
            string cadena = "";
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT DESC_PACK from CAT_TYPE_PACK_STYLE where ID_SUMMARY='" + summary + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    if ((tipo_empaque == 1|| tipo_empaque == 5) && (Convert.ToString(leer["DESC_PACK"])).Contains("P")){
                        cadena = Convert.ToString(leer["DESC_PACK"]);
                    }else {
                        if ((Convert.ToString(leer["DESC_PACK"])).Contains("A") && tipo_empaque != 1&& tipo_empaque != 5){
                            cadena = Convert.ToString(leer["DESC_PACK"]);
                        }/*else {
                            cadena = Convert.ToString(leer["DESC_PACK"]);
                        }*/
                    }
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return cadena;
        }
        public List<ratio_tallas> obtener_lista_ratio(int posummary, int estilo,int tipo_empaque) {
            List<ratio_tallas> lista = new List<ratio_tallas>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select PTZ.ID_TALLA,PTZ.RATIO,PTZ.PIECES,CS.ORDEN,CS.TALLA from PACKING_TYPE_SIZE PTZ,CAT_ITEM_SIZE CS " +
                    " where PTZ.ID_SUMMARY='" + posummary + "' and PTZ.TYPE_PACKING='" + tipo_empaque+ "' AND " +
                    " CS.ID=PTZ.ID_TALLA ORDER BY CAST(CS.ORDEN AS int) ASC ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    ratio_tallas e = new ratio_tallas();
                    e.id_estilo = estilo; //Regex.Replace(color, @"\s+", " ");
                    e.id_talla = Convert.ToInt32(leerFilas["ID_TALLA"]);
                    e.talla = Regex.Replace(consultas.obtener_size_id(Convert.ToString(leerFilas["ID_TALLA"])), @"\s+", " ");
                    e.ratio = Convert.ToInt32(leerFilas["RATIO"]);
                    e.piezas = Convert.ToInt32(leerFilas["PIECES"]);
                    lista.Add(e);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<ratio_tallas> obtener_lista_ratio_hottopic(int posummary, string ponumero, string tipo_empaque)
        {
            List<ratio_tallas> lista = new List<ratio_tallas>();
            Link conn = new Link();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select ID_TALLA,RATIO,PIECES from PACKING_TYPE_SIZE where ID_SUMMARY='" + posummary + "' and TYPE_PACKING='" + tipo_empaque + "' AND NUMBER_PO='"+ponumero+"'";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read())
                {
                    ratio_tallas e = new ratio_tallas();                    
                    e.id_talla = Convert.ToInt32(leerFilas["ID_TALLA"]);                    
                    e.ratio = Convert.ToInt32(leerFilas["RATIO"]);
                    e.piezas = Convert.ToInt32(leerFilas["PIECES"]);
                    lista.Add(e);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<ratio_tallas> obtener_lista_ratio_assort_r(int posummary, int estilo,string packing_name){///***********************************************************        
            List<ratio_tallas> lista = new List<ratio_tallas>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();//obtener el packing name 
                comando.CommandText = "SELECT ID_PACKING_TYPE_SIZE,ID_TALLA,RATIO,PACKING_NAME,ASSORT_NAME " +
                    " FROM PACKING_TYPE_SIZE  where PACKING_NAME='" + packing_name + "'" +
                    "  AND ID_SUMMARY='" + posummary + "'";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    ratio_tallas e = new ratio_tallas();
                    e.id_estilo = estilo;
                    e.id_talla = Convert.ToInt32(leerFilas["ID_TALLA"]);
                    e.talla = consultas.obtener_size_id(Convert.ToString(leerFilas["ID_TALLA"]));
                    e.packing_name= Convert.ToString(leerFilas["PACKING_NAME"]);
                    e.assort_name= Convert.ToString(leerFilas["ASSORT_NAME"]);
                    e.ratio= Convert.ToInt32(leerFilas["RATIO"]);
                    lista.Add(e);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
             return lista;
        }

        public List<ratio_tallas> obtener_lista_ratio_otros(int shipping) {
            List<ratio_tallas> lista = new List<ratio_tallas>();
            Link conn = new Link();
            try { //Regex.Replace(color, @"\s+", " ");
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select id_talla,cantidad,id_shipping_id from shipping_ids where id_shipping_id='" + shipping + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    ratio_tallas e = new ratio_tallas();
                    //e.id_estilo = estilo;
                    e.id_talla = Convert.ToInt32(leerFilas["id_talla"]);
                    e.talla = Regex.Replace(consultas.obtener_size_id(Convert.ToString(leerFilas["id_talla"])), @"\s+", " ");
                    e.ratio = Convert.ToInt32(leerFilas["cantidad"]);
                    lista.Add(e);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public string buscar_pks_pedido(int pedido, int pk) {
            string cadena = "";
          /*  int parte = 0;
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_pedido from packing_list where id_pedido='" + pedido + "' and id_packing_list<='" + pk + "'  ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    parte++;
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            switch (parte) {
                case 1: cadena = "1st Part"; break;
                case 2: cadena = "2nd Part"; break;
                case 3: cadena = "3rd Part"; break;
                default: cadena = Convert.ToString(parte) + "st Part"; break;
            }*/
            return cadena;
        }
        public string buscar_number_po_pedido(int pedido){
            string cadena = "";
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT VPO from PEDIDO where ID_PEDIDO='" + pedido + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    cadena = Convert.ToString(leer["VPO"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return cadena;
        }
        public string buscar_number_po(int summary) {
            string cadena = "";
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                 com.CommandText = "SELECT NUMBER_PO from PACKING_TYPE_SIZE where ID_SUMMARY='" + summary + "' ";
                //com.CommandText = "SELECT P.VPO from PEDIDO P,PO_SUMMARU PS where P.ID_PEDIDO=PS.ID_PEDIDOS AND PS.ID_PO_SUMMARY='" + summary+ "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                   cadena = Convert.ToString(leer["NUMBER_PO"]);
                  //  cadena = Convert.ToString(leer["VPO"]);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return cadena;
        }
        //***************************************************************
        public List<Pk> lista_buscar_pk_inicio(string busqueda) {
            //List<Pk> lista = new List<Pk>();
            List<Pk> lista_final = new List<Pk>();
            List<int> ids = new List<int>();
            if (busqueda != "0"){
                //lista.AddRange(lista_buscar_pk_inicio_pedido(busqueda));
                ids.AddRange(lista_buscar_pk_inicio_pedido_ids(busqueda));
                //lista.AddRange(lista_buscar_pk_inicio_fecha(busqueda));
                ids.AddRange(lista_buscar_pk_inicio_fecha_ids(busqueda));
                //lista.AddRange(lista_buscar_pk_inicio_cliente(busqueda));
                ids.AddRange(lista_buscar_pk_inicio_cliente_ids(busqueda));
                //lista.AddRange(lista_buscar_pk_inicio_cliente_final(busqueda));
                ids.AddRange(lista_buscar_pk_inicio_cliente_final_ids(busqueda));
                //lista.AddRange(lista_buscar_pk_inicio_packing(busqueda));
                ids.AddRange(lista_buscar_pk_inicio_packing_ids(busqueda));
            }else {                
                Link conx = new Link();
                try {
                    SqlCommand comandox = new SqlCommand();
                    SqlDataReader leerFilasx = null;
                    comandox.Connection = conx.AbrirConexion();
                    comandox.CommandText = "select top 50 id_packing_list from packing_list order by id_packing_list desc ";
                    leerFilasx = comandox.ExecuteReader();
                    while (leerFilasx.Read()) {
                        Pk p = new Pk();
                        p = obtener_packing(Convert.ToInt32(leerFilasx["id_packing_list"]));
                        lista_final.Add(p);
                    } leerFilasx.Close();
                } finally { conx.CerrarConexion(); conx.Dispose(); }
            }            
            ids = ids.Distinct().ToList();
            string cadena = "select top 50 id_packing_list from packing_list where id_packing_list=0 ";
            foreach (int i in ids) { cadena += " or id_packing_list="+i+" "; }
            cadena += "  order by id_packing_list desc";
            Link connx = new Link();
            try{
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = cadena;
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()){
                    Pk p = new Pk();
                    p = obtener_packing(Convert.ToInt32(leerFilasx["id_packing_list"]));
                    lista_final.Add(p);
                }leerFilasx.Close();
            }finally { connx.CerrarConexion(); connx.Dispose(); }
            return lista_final;
        }
        public List<int> lista_buscar_pk_inicio_pedido_ids(string busqueda){
            List<int> lista = new List<int>();
            Link connx = new Link();
            try{
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "SELECT DISTINCT top 50 id_packing_list FROM packing_list " +
                    " WHERE nombre_archivo LIKE'%" + busqueda + "%'  order by id_packing_list desc   ";
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()){
                    lista.Add(Convert.ToInt32(leerFilasx["id_packing_list"]));
                }leerFilasx.Close();
            }finally { connx.CerrarConexion(); connx.Dispose(); }
            return lista;
        }
        public List<Pk> lista_buscar_pk_inicio_pedido(string busqueda){
            List<Pk> lista = new List<Pk>();
            Link connx = new Link();
            try{
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "SELECT DISTINCT top 50 id_packing_list FROM packing_list " +
                    " WHERE nombre_archivo LIKE'%" + busqueda + "%'  order by id_packing_list desc   ";
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()){
                    Pk p = new Pk();
                    p = obtener_packing(Convert.ToInt32(leerFilasx["id_packing_list"]));
                    lista.Add(p);
                }leerFilasx.Close();
            }finally { connx.CerrarConexion(); connx.Dispose(); }
            return lista;
        }
        public List<Pk> lista_buscar_pk_inicio_fecha(string busqueda){
            List<Pk> lista = new List<Pk>();
            Link connx = new Link();
            try{
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "SELECT DISTINCT top 50 id_packing_list FROM packing_list  WHERE fecha LIKE'%" + busqueda + "%'  order by id_packing_list desc  ";
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()){
                    Pk p = new Pk();
                    p = obtener_packing(Convert.ToInt32(leerFilasx["id_packing_list"]));
                    lista.Add(p);
                }leerFilasx.Close();
            }finally { connx.CerrarConexion(); connx.Dispose(); }
            return lista;
        }
        public List<int> lista_buscar_pk_inicio_fecha_ids(string busqueda){
            List<int> lista = new List<int>();
            Link connx = new Link();
            try{
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "SELECT DISTINCT top 50 id_packing_list FROM packing_list  WHERE fecha LIKE'%" + busqueda + "%'  order by id_packing_list desc  ";
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()){
                    lista.Add(Convert.ToInt32(leerFilasx["id_packing_list"]));
                }leerFilasx.Close();
            }finally { connx.CerrarConexion(); connx.Dispose(); }
            return lista;
        }
        public List<Pk> lista_buscar_pk_inicio_cliente(string busqueda){
            List<Pk> lista = new List<Pk>();
            Link connx = new Link();
            try{
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "SELECT DISTINCT top 50 p.id_packing_list FROM packing_list p,CAT_CUSTOMER c  WHERE " +
                    " p.id_customer=c.CUSTOMER AND c.NAME LIKE'%" + busqueda + "%'  order by p.id_packing_list desc  ";
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()){
                    Pk p = new Pk();
                    p = obtener_packing(Convert.ToInt32(leerFilasx["id_packing_list"]));
                    lista.Add(p);
                }leerFilasx.Close();
            }finally { connx.CerrarConexion(); connx.Dispose(); }
            return lista;
        }
        public List<int> lista_buscar_pk_inicio_cliente_ids(string busqueda){
            List<int> lista = new List<int>();
            Link connx = new Link();
            try{
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "SELECT DISTINCT top 50 p.id_packing_list FROM packing_list p,CAT_CUSTOMER c  WHERE " +
                    " p.id_customer=c.CUSTOMER AND c.NAME LIKE'%" + busqueda + "%'  order by p.id_packing_list desc  ";
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()) { 
                    lista.Add(Convert.ToInt32(leerFilasx["id_packing_list"]));
                }leerFilasx.Close();
            }finally { connx.CerrarConexion(); connx.Dispose(); }
            return lista;
        }
        public List<Pk> lista_buscar_pk_inicio_cliente_final(string busqueda){
            List<Pk> lista = new List<Pk>();
            Link connx = new Link();
            try{
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "SELECT DISTINCT top 50 p.id_packing_list FROM packing_list p,CAT_CUSTOMER_PO c  WHERE " +
                    " p.id_customer_po=c.CUSTOMER_FINAL AND c.NAME_FINAL LIKE'%" + busqueda + "%'  order by p.id_packing_list desc  ";
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()){
                    Pk p = new Pk();
                    p = obtener_packing(Convert.ToInt32(leerFilasx["id_packing_list"]));
                    lista.Add(p);
                }leerFilasx.Close();
            }finally { connx.CerrarConexion(); connx.Dispose(); }
            return lista;
        }
        public List<int> lista_buscar_pk_inicio_cliente_final_ids(string busqueda){
            List<int> lista = new List<int>();
            Link connx = new Link();
            try{
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "SELECT DISTINCT top 50 p.id_packing_list FROM packing_list p,CAT_CUSTOMER_PO c  WHERE " +
                    " p.id_customer_po=c.CUSTOMER_FINAL AND c.NAME_FINAL LIKE'%" + busqueda + "%'  order by p.id_packing_list desc  ";
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()){
                    lista.Add(Convert.ToInt32(leerFilasx["id_packing_list"]));
                }leerFilasx.Close();
            }finally { connx.CerrarConexion(); connx.Dispose(); }
            return lista;
        }
        public List<Pk> lista_buscar_pk_inicio_packing(string busqueda){
            List<Pk> lista = new List<Pk>();
            Link connx = new Link();
            try{
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "SELECT DISTINCT top 50 id_packing_list FROM packing_list WHERE pk LIKE'%" + busqueda + "%'  order by id_packing_list desc   "; 
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()){
                    Pk p = new Pk();
                    p = obtener_packing(Convert.ToInt32(leerFilasx["id_packing_list"]));
                    lista.Add(p);
                }leerFilasx.Close();
            }finally { connx.CerrarConexion(); connx.Dispose(); }
            return lista;
        }
        public List<int> lista_buscar_pk_inicio_packing_ids(string busqueda){
            List<int> lista = new List<int>();
            Link connx = new Link();
            try{
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "SELECT DISTINCT top 50 id_packing_list FROM packing_list WHERE pk LIKE'%" + busqueda + "%'  order by id_packing_list desc   ";
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()){
                    lista.Add(Convert.ToInt32(leerFilasx["id_packing_list"]));
                }leerFilasx.Close();
            }finally { connx.CerrarConexion(); connx.Dispose(); }
            return lista;
        }

        public Pk obtener_packing(int pk) {
            Pk p = new Pk();
            Link connx = new Link();
            try {
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "select nombre_archivo,id_pedido,id_packing_list,pk,id_customer_po,fecha,id_packing_type from packing_list where id_packing_list='" + pk + "' ";
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()) {
                    int id_customer_po = 0;
                    p.id_packing_list = Convert.ToInt32(leerFilasx["id_packing_list"]);
                    p.packing = Convert.ToString(leerFilasx["pk"]);
                    p.id_pedido= Convert.ToInt32(leerFilasx["id_pedido"]);
                    p.id_tipo= Convert.ToInt32(leerFilasx["id_packing_type"]);
                    if (p.id_tipo != 8 && p.id_tipo != 4){
                        p.pedido = consultas.obtener_po_id((p.id_pedido).ToString());
                        id_customer_po = consultas.obtener_customer_final_po(p.id_pedido);
                    }else {
                        p.pedido = consultas.obtener_po_id_fantasy((p.id_pedido).ToString());
                        id_customer_po = Convert.ToInt32(leerFilasx["id_customer_po"]);
                    }
                    p.nombre_archivo = Convert.ToString(leerFilasx["nombre_archivo"]);
                    p.customer_po = consultas.obtener_customer_final_id(Convert.ToString(id_customer_po));
                    p.fecha = (Convert.ToDateTime(leerFilasx["fecha"])).ToString("MM/dd/yyyy");
                    
                    p.tipo = buscar_tipo_packing(p.id_tipo);
                    /*switch (p.id_tipo){
                        case 1:                        
                        case 2:
                        case 7:
                            p.lista_tarimas = obtener_tarimas(p.id_packing_list);
                            break;
                        case 3:
                            p.lista_samples = obtener_lista_samples_tarima(p.id_packing_list);
                            break;
                        case 5:
                            p.lista_tarimas = obtener_tarimas_returns(p.id_packing_list);
                            break;
                        case 6:
                            p.lista_tarimas = obtener_tarimas_extras_fantasy(p.id_packing_list);
                            break;
                        case 8:
                            p.lista_estilos = obtener_lista_estilos_tarima(0, pk);
                            break;
                        case 4:
                            p.lista_tarimas = obtener_tarimas(p.id_packing_list);
                            break;
                    }*/



                } leerFilasx.Close();
            } finally { connx.CerrarConexion(); connx.Dispose(); }
            return p;
        }
        public string buscar_tipo_packing(int tipo){
            string tempo = "";
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT packing FROM tipos_packing_list WHERE id_tipo_packing='" + tipo + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo = Convert.ToString(leer["packing"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }
        public int obtener_id_staging(int pedido, int estilo, int talla) {
            int tempo = 0;
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT s.id_staging from staging s,staging_count sc " +
                    "where s.id_staging=sc.id_staging and  s.id_pedido='" + pedido + "' and s.id_estilo='" + estilo + "' and sc.id_talla='" + talla + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    tempo = Convert.ToInt32(leer["id_staging"]);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }
        public int obtener_id_pais_staging(int staging, int talla) {
            int tempo = 0;
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_pais from staging_count where id_staging='" + staging + "' and id_talla='" + talla + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    tempo = Convert.ToInt32(leer["id_pais"]);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }
        public int buscar_cajas_talla_estilo(int summary, int talla) {
            int tempo = 0;
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT PIECES from PACKING_TYPE_SIZE where ID_SUMMARY='" + summary + "' and ID_TALLA='" + talla + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    tempo = Convert.ToInt32(leer["PIECES"]);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }
        public List<Fabricantes> buscar_paises_estilos(List<estilos> lista_estilo) {
            List<Fabricantes> lista = new List<Fabricantes>();
            foreach (estilos e in lista_estilo) {
                Link con = new Link();
                try {
                    SqlCommand com = new SqlCommand();
                    SqlDataReader leer = null;
                    com.Connection = con.AbrirConexion();
                    com.CommandText = "SELECT sc.id_staging_count,sc.id_pais,sc.total,sc.id_porcentaje from staging_count sc,staging s where sc.id_staging=s.id_staging and s.id_summary='" + e.id_po_summary + "'  ";
                    leer = com.ExecuteReader();
                    while (leer.Read()) {
                        Fabricantes f = new Fabricantes();
                        f.id = Convert.ToInt32(leer["id_staging_count"]);
                        f.id_pais = Convert.ToInt32(leer["id_pais"]);
                        f.pais = consultas.obtener_pais_id(Convert.ToString(f.id_pais));
                        f.cantidad = Convert.ToInt32(leer["total"]);
                        f.percent = consultas.obtener_fabric_percent_id(Convert.ToString(leer["id_porcentaje"]));
                        lista.Add(f);
                    } leer.Close();
                } finally { con.CerrarConexion(); con.Dispose(); }
            }
            return lista;
        }

        public List<Fabricantes> buscar_paises_estilos_stag_recibos(List<int> lista_estilo){
            List<Fabricantes> lista = new List<Fabricantes>();
            foreach (int e in lista_estilo){
                Link con = new Link();
                try{
                    SqlCommand com = new SqlCommand();
                    SqlDataReader leer = null;
                    com.Connection = con.AbrirConexion();
                    com.CommandText = "SELECT distinct sc.id_pais, sc.id_porcentaje from staging_count sc,staging s where sc.id_staging=s.id_staging and s.id_summary='" + e + "'  ";
                    leer = com.ExecuteReader();
                    while (leer.Read()){
                        Fabricantes f = new Fabricantes();                        
                        f.id_pais = Convert.ToInt32(leer["id_pais"]);
                        f.pais = consultas.obtener_pais_id(Convert.ToString(f.id_pais));
                        f.cantidad = buscar_staging_porcentaje_pais(Convert.ToInt32(leer["id_pais"]), Convert.ToInt32(leer["id_porcentaje"]),e); 
                        //f.cantidad = Convert.ToInt32(leer["total"]);
                        f.percent = consultas.obtener_fabric_percent_id(Convert.ToString(leer["id_porcentaje"]));
                        lista.Add(f);
                    }leer.Close();
                }finally { con.CerrarConexion(); con.Dispose(); }
            }
            return lista;
        }
        public int buscar_staging_porcentaje_pais(int pais, int porcentaje,int estilo){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT sc.total from staging_count sc,staging s where sc.id_staging=s.id_staging and s.id_summary='" + estilo + "' and  sc.id_pais='" + pais + "'  and  sc.id_porcentaje='" + porcentaje + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo += Convert.ToInt32(leer["total"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }






        public List<Fabricantes> buscar_paises_returns(List<Return> lista_returns){
            List<Fabricantes> lista = new List<Fabricantes>();
            foreach (Return e in lista_returns) {
                if (e.id_categoria == 1){
                    Link con = new Link();
                    try{
                        SqlCommand com = new SqlCommand();
                        SqlDataReader leer = null;
                        com.Connection = con.AbrirConexion();
                        com.CommandText = "SELECT sc.id_pais,sc.total,sc.id_porcentaje from staging_count sc,staging s where sc.id_staging=s.id_staging and s.id_summary='" + e.id_summary + "'  ";
                        leer = com.ExecuteReader();
                        while (leer.Read()){
                            Fabricantes f = new Fabricantes();
                            f.id_pais = Convert.ToInt32(leer["id_pais"]);
                            f.pais = consultas.obtener_pais_id(Convert.ToString(f.id_pais));
                            f.cantidad = Convert.ToInt32(leer["total"]);
                            f.percent = consultas.obtener_fabric_percent_id(Convert.ToString(leer["id_porcentaje"]));
                            lista.Add(f);
                        }leer.Close();
                    }finally { con.CerrarConexion(); con.Dispose(); }
                }
            }
            return lista;
        }
        public int obtener_id_porcentaje_staging(int staging, int talla) {
            int tempo = 0;
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_porcentaje from staging_count where id_staging='" + staging + "' and id_talla='" + talla + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    tempo = Convert.ToInt32(leer["id_porcentaje"]);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }
        //obtener_lista_po_summarys
        public List<Breakdown> obtener_lista_po_shipping() {
            List<Breakdown> lista = new List<Breakdown>();
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT DISTINCT ID_PEDIDO,PO FROM PEDIDO WHERE ID_STATUS!=6 AND ID_STATUS!=7 AND ID_STATUS!=5 ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    Breakdown b = new Breakdown();
                    b.id_pedido = Convert.ToInt32(leer["ID_PEDIDO"]);
                    b.po = Convert.ToString(leer["PO"]);
                    lista.Add(b);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Breakdown> obtener_lista_po_shipping_fantasy(){
            List<Breakdown> lista = new List<Breakdown>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT DISTINCT ID_PEDIDO,PO FROM PEDIDO WHERE CUSTOMER=2 AND ID_STATUS!=6 AND ID_STATUS!=7 AND ID_STATUS!=5 ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Breakdown b = new Breakdown();
                    b.id_pedido = Convert.ToInt32(leer["ID_PEDIDO"]);
                    b.po = Convert.ToString(leer["PO"]);
                    lista.Add(b);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<estilos> lista_estilos_packing(int pk) {
            List<estilos> lista = new List<estilos>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select id_shipping_id,id_estilo,number_po,cantidad,dc,id_po_summary,tipo_empaque,index_dc,tipo,id_pedido,id_talla,store," +
                    "sobrantes,number_ppk,repeticiones,id_tarima,packing_name from shipping_ids where used='" + pk + "'  "; 
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    estilos e = new estilos();
                    e.id_shipping_id = Convert.ToInt32(leerFilas["id_shipping_id"]);
                    e.id_tarima = Convert.ToInt32(leerFilas["id_tarima"]);
                    e.id_estilo = Convert.ToInt32(leerFilas["id_estilo"]);
                    e.id_po_summary=Convert.ToInt32(leerFilas["id_po_summary"]);
                    e.id_color = consultas.obtener_color_id_item_cat(e.id_po_summary);
                    e.color = consultas.obtener_color_id((e.id_color).ToString());
                    e.estilo = consultas.obtener_estilo(e.id_estilo);
                    e.descripcion = consultas.buscar_descripcion_estilo(e.id_estilo);
                    e.number_po = Convert.ToString(leerFilas["number_po"]);
                    e.boxes = Convert.ToInt32(leerFilas["cantidad"]);
                    e.index_dc = Convert.ToInt32(leerFilas["index_dc"]);
                    e.tipo_empaque= Convert.ToInt32(leerFilas["tipo_empaque"]);
                    e.id_talla= Convert.ToInt32(leerFilas["id_talla"]);
                    e.talla = consultas.obtener_size_id((e.id_talla).ToString());
                    e.tipo= Convert.ToString(leerFilas["tipo"]);
                    e.store= Convert.ToString(leerFilas["store"]);
                    e.packing_name=Convert.ToString(leerFilas["packing_name"]);
                    e.sobrantes = Convert.ToInt32(leerFilas["sobrantes"]);
                    e.number_ppk = Convert.ToInt32(leerFilas["number_ppk"]);
                    if (e.tipo == "DMG" || e.tipo == "EXT" || e.tipo == "ECOM"){
                            e.lista_ratio = obtener_lista_ratio_otros(Convert.ToInt32(leerFilas["id_shipping_id"]));
                    }else{
                        if (e.tipo_empaque ==1){
                            e.lista_ratio = obtener_lista_ratio(e.id_po_summary, e.id_estilo,1);
                            e.piezas= buscar_piezas_empaque_bull(e.id_po_summary, e.id_talla);
                        }
                        if (e.tipo_empaque == 2){
                            e.lista_ratio = obtener_lista_ratio(e.id_po_summary, e.id_estilo, 2);
                        }
                        if (e.tipo_empaque == 3){
                            e.assort = assortment_id(e.id_po_summary, Convert.ToInt32(leerFilas["id_pedido"]));
                            e.assort_nombre = obtener_nombre_assort(e.id_po_summary);
                        }
                        if (e.tipo_empaque == 4){
                            e.lista_ratio = obtener_lista_ratio_ppks(e.id_po_summary, e.id_estilo, 4, e.number_ppk, e.packing_name);
                        }
                        if (e.tipo_empaque == 5){
                            e.lista_ratio = obtener_lista_ratio_bps(e.id_po_summary, e.id_estilo, 5, e.packing_name);
                        }
                    }                    
                    e.dc = Convert.ToString(leerFilas["dc"]);
                    e.pk = pk;
                    e.repeticiones = Convert.ToInt32(leerFilas["repeticiones"]);
                    e.id_pedido = buscar_pedido_pk(pk.ToString());
                    e.customer = consultas.obtener_customer_po(e.id_pedido);
                    e.customer_final = consultas.obtener_customer_final_po(e.id_pedido);
                    lista.Add(e);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<estilos> lista_estilos_packing_sin_tarima(int pk){
            List<estilos> lista = new List<estilos>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select id_shipping_id,id_estilo,number_po,cantidad,dc,id_po_summary,tipo_empaque,index_dc,tipo,id_pedido,id_talla,store," +
                    "sobrantes,number_ppk,repeticiones,id_tarima,packing_name from shipping_ids where used='" + pk + "' and id_tarima=0  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    estilos e = new estilos();
                    e.id_shipping_id = Convert.ToInt32(leerFilas["id_shipping_id"]);
                    e.id_tarima = Convert.ToInt32(leerFilas["id_tarima"]);
                    e.id_estilo = Convert.ToInt32(leerFilas["id_estilo"]);
                    e.id_po_summary = Convert.ToInt32(leerFilas["id_po_summary"]);
                    e.id_color = consultas.obtener_color_id_item_cat(e.id_po_summary);
                    e.color = consultas.obtener_color_id((e.id_color).ToString());
                    e.estilo = consultas.obtener_estilo(e.id_estilo);
                    e.descripcion = consultas.buscar_descripcion_estilo(e.id_estilo);
                    e.number_po = Convert.ToString(leerFilas["number_po"]);
                    e.boxes = Convert.ToInt32(leerFilas["cantidad"]);
                    e.index_dc = Convert.ToInt32(leerFilas["index_dc"]);
                    e.tipo_empaque = Convert.ToInt32(leerFilas["tipo_empaque"]);
                    e.id_talla = Convert.ToInt32(leerFilas["id_talla"]);
                    e.talla = consultas.obtener_size_id((e.id_talla).ToString());
                    e.tipo = Convert.ToString(leerFilas["tipo"]);
                    e.store = Convert.ToString(leerFilas["store"]);
                    e.packing_name = Convert.ToString(leerFilas["packing_name"]);
                    e.sobrantes = Convert.ToInt32(leerFilas["sobrantes"]);
                    e.number_ppk = Convert.ToInt32(leerFilas["number_ppk"]);
                    if (e.tipo == "DMG" || e.tipo == "EXT" || e.tipo == "ECOM")
                    {
                        e.lista_ratio = obtener_lista_ratio_otros(Convert.ToInt32(leerFilas["id_shipping_id"]));
                    }
                    else
                    {
                        if (e.tipo_empaque == 1)
                        {
                            e.lista_ratio = obtener_lista_ratio(e.id_po_summary, e.id_estilo, 1);
                            e.piezas = buscar_piezas_empaque_bull(e.id_po_summary, e.id_talla);
                        }
                        if (e.tipo_empaque == 2)
                        {
                            e.lista_ratio = obtener_lista_ratio(e.id_po_summary, e.id_estilo, 2);
                        }
                        if (e.tipo_empaque == 3)
                        {
                            e.assort = assortment_id(e.id_po_summary, Convert.ToInt32(leerFilas["id_pedido"]));
                            e.assort_nombre = obtener_nombre_assort(e.id_po_summary);
                        }
                        if (e.tipo_empaque == 4)
                        {
                            e.lista_ratio = obtener_lista_ratio_ppks(e.id_po_summary, e.id_estilo, 4, e.number_ppk, e.packing_name);
                        }
                        if (e.tipo_empaque == 5)
                        {
                            e.lista_ratio = obtener_lista_ratio_bps(e.id_po_summary, e.id_estilo, 5, e.packing_name);
                        }
                    }
                    e.dc = Convert.ToString(leerFilas["dc"]);
                    e.pk = pk;
                    e.repeticiones = Convert.ToInt32(leerFilas["repeticiones"]);
                    e.id_pedido = buscar_pedido_pk(pk.ToString());
                    e.customer = consultas.obtener_customer_po(e.id_pedido);
                    e.customer_final = consultas.obtener_customer_final_po(e.id_pedido);
                    lista.Add(e);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<estilos> lista_estilos_packing_edicion(int pk){
            List<estilos> lista = new List<estilos>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select label,repeticiones,id_shipping_id,id_estilo,number_po,cantidad,dc,id_po_summary,tipo_empaque,index_dc,tipo,id_pedido,id_talla,store,sobrantes,number_ppk,packing_name from shipping_ids where used='" + pk + "'   ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    estilos e = new estilos();
                    e.id_shipping_id = Convert.ToInt32(leerFilas["id_shipping_id"]);
                    e.id_estilo = Convert.ToInt32(leerFilas["id_estilo"]);
                    e.id_po_summary = Convert.ToInt32(leerFilas["id_po_summary"]);
                    e.id_color = consultas.obtener_color_id_item_cat(e.id_po_summary);
                    e.color = consultas.obtener_color_id((e.id_color).ToString());
                    e.estilo = consultas.obtener_estilo(e.id_estilo);
                    e.descripcion = consultas.buscar_descripcion_estilo(e.id_estilo);
                    e.number_po = Convert.ToString(leerFilas["number_po"]);
                    e.boxes = Convert.ToInt32(leerFilas["cantidad"]);
                    e.index_dc = Convert.ToInt32(leerFilas["index_dc"]);
                    e.tipo_empaque = Convert.ToInt32(leerFilas["tipo_empaque"]);
                    e.id_talla = Convert.ToInt32(leerFilas["id_talla"]);
                    e.talla = consultas.obtener_size_id(Convert.ToString(e.id_talla));
                    e.tipo = Convert.ToString(leerFilas["tipo"]);
                    e.label = Convert.ToString(leerFilas["label"]);
                    e.store = Convert.ToString(leerFilas["store"]);  
                    e.packing_name = Convert.ToString(leerFilas["packing_name"]);  
                    e.sobrantes= Convert.ToInt32(leerFilas["sobrantes"]);
                    e.number_ppk= Convert.ToInt32(leerFilas["number_ppk"]);
                    if (e.tipo == "DMG" || e.tipo == "EXT" ){
                        e.lista_ratio = obtener_lista_ratio_otros(Convert.ToInt32(leerFilas["id_shipping_id"]));
                    }else{
                        if (e.tipo_empaque == 1){
                            e.lista_ratio = obtener_lista_ratio(e.id_po_summary, e.id_estilo, 1);
                            e.piezas = buscar_piezas_empaque_bull(e.id_po_summary, e.id_talla);
                        }
                        if (e.tipo_empaque == 2){
                            e.lista_ratio = obtener_lista_ratio(e.id_po_summary, e.id_estilo, 2);
                        }
                        if (e.tipo_empaque == 4){
                            e.lista_ratio = obtener_lista_ratio_ppks(e.id_po_summary, e.id_estilo, 4, e.number_ppk, e.packing_name);
                        }
                        if (e.tipo_empaque == 3){
                            e.assort = assortment_id(e.id_po_summary, Convert.ToInt32(leerFilas["id_pedido"]));
                            e.assort_nombre = obtener_nombre_assort(e.id_po_summary);
                        }
                        if (e.tipo_empaque == 5){
                            e.lista_ratio = obtener_lista_ratio_bps(e.id_po_summary, e.id_estilo, 5, e.packing_name);
                        }
                    }
                    e.dc = Convert.ToString(leerFilas["dc"]);
                    e.pk = pk;
                    e.repeticiones= Convert.ToInt32(leerFilas["repeticiones"]);
                    lista.Add(e);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<Sample> obtener_lista_extras_fantasy_packing(int packing){
            List<Sample> lista = new List<Sample>();
            Link conn = new Link();
            try{ //Regex.Replace(color, @"\s+", " ");
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_fantasy,id_packing,id_summary,talla_s,talla_m,talla_l,talla_xl,talla_2x,cajas,tipo " +
                    " FROM packing_fantasy WHERE id_packing='" + packing + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    Sample s = new Sample();
                    s.id_fantasy = Convert.ToInt32(leerFilas["id_fantasy"]);
                    s.id_summary = Convert.ToInt32(leerFilas["id_summary"]);
                    s.tipo = Convert.ToString(leerFilas["tipo"]);
                    int id_estilo = consultas.obtener_estilo_summary(s.id_summary);
                    s.id_color = consultas.obtener_color_id_item_cat(s.id_summary);
                    s.color = Regex.Replace(consultas.obtener_color_id((s.id_color).ToString()), @"\s+", " ");
                    s.estilo = Regex.Replace(consultas.obtener_estilo(id_estilo), @"\s+", " ");
                    s.descripcion = Regex.Replace(consultas.buscar_descripcion_estilo(id_estilo), @"\s+", " ");
                    s.talla_s = Convert.ToInt32(leerFilas["talla_s"]);
                    s.talla_m = Convert.ToInt32(leerFilas["talla_m"]);
                    s.talla_l = Convert.ToInt32(leerFilas["talla_l"]);
                    s.talla_xl = Convert.ToInt32(leerFilas["talla_xl"]);
                    s.talla_2x = Convert.ToInt32(leerFilas["talla_2x"]);
                    s.cajas = Convert.ToInt32(leerFilas["cajas"]);
                    s.total = s.talla_s + s.talla_m + s.talla_l + s.talla_xl + s.talla_2x;
                    s.id_customer = 2;
                    s.customer = consultas.obtener_customer_final_id(Convert.ToString(s.id_customer));
                    buscar_origen_summary(s.id_summary);
                    s.origen = origen;
                    s.porcentaje = porcentaje;
                    s.id_pedido = consultas.obtener_id_pedido_summary(s.id_summary);
                    s.pedido = consultas.obtener_po_id((s.id_pedido).ToString());
                    s.number_po = buscar_number_po_pedido(s.id_pedido);
                    s.id_genero = consultas.buscar_genero_summary(s.id_summary);
                    s.genero = consultas.obtener_genero_id(Convert.ToString(s.id_genero));
                    lista.Add(s);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }


        public void guardar_ids_tarimas(string tarima, string shipping) {
            Link con_s = new Link();
            try {
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE shipping_ids SET id_tarima='" + tarima + "' where id_shipping_id='" + shipping + "' ";
                com_s.ExecuteNonQuery();
            } finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void guardar_ids_tarimas_bpdc(string tarima,string packing,string index){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE shipping_ids SET id_tarima='" + tarima + "' where used='" + packing + "'" +
                    " AND index_dc='"+index+"' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void guardar_ids_tarimas_samples(string tarima, string shipping){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE packing_samples SET id_tarima='" + tarima + "' where id_sample='" + shipping + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void guardar_ids_tarimas_returns(string tarima, string shipping){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE packing_returns SET id_tarima='" + tarima + "' where id_return='" + shipping + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }

        public void guardar_ids_tarimas_fantasy(string tarima, string shipping){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE packing_fantasy SET id_tarima='" + tarima + "' where id_fantasy='" + shipping + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }



        public List<Assortment> lista_assortments_pedido(int pedido) {
            List<Assortment> lista = new List<Assortment>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct PACKING_NAME from PACKING_ASSORT where ID_PEDIDO='" + pedido + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    Assortment a = new Assortment();
                    a = obtener_assortment_pedido(Convert.ToString(leerFilas["PACKING_NAME"]), pedido);
                    lista.Add(a);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public Assortment assortment_id(int id,int pedido){
            Assortment a = new Assortment();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct PACKING_NAME from PACKING_ASSORT where ID_PACKING_ASSORT='" + id + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    a = obtener_assortment_pedido(Convert.ToString(leerFilas["PACKING_NAME"]), pedido);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return a;
        }


        public Assortment obtener_assortment_pedido(string nombre_packing, int pedido) {
            Assortment aa = new Assortment();
            aa.cartones = 0;
            Link conna1 = new Link();
            try {
                SqlCommand comandoa1 = new SqlCommand();
                SqlDataReader leerFilasa1 = null;
                comandoa1.Connection = conna1.AbrirConexion();
                comandoa1.CommandText = "select ID_PACKING_ASSORT,ID_BLOCK,CANT_CARTONS,PACKING_NAME from PACKING_ASSORT where ID_PEDIDO='" + pedido + "' " +
                    "and PACKING_NAME='" + nombre_packing + "'";
                leerFilasa1 = comandoa1.ExecuteReader();
                while (leerFilasa1.Read()) {
                    aa.id_assortment = Convert.ToInt32(leerFilasa1["ID_PACKING_ASSORT"]);
                    aa.block = Convert.ToInt32(leerFilasa1["ID_BLOCK"]);
                    aa.cartones += Convert.ToInt32(leerFilasa1["CANT_CARTONS"]);
                    aa.nombre = Convert.ToString(leerFilasa1["PACKING_NAME"]);                    
                    aa.lista_estilos = obtener_lista_estilos_assort(nombre_packing, pedido);
                }leerFilasa1.Close();
            } finally { conna1.CerrarConexion(); conna1.Dispose(); }
            return aa;
        }
        public List<estilos> obtener_lista_estilos_assort(string nombre_packing, int pedido) {
            List<estilos> lista = new List<estilos>();
            Link conna2 = new Link();
            try {
                SqlCommand comandoa2 = new SqlCommand();
                SqlDataReader leerFilasa2 = null;
                comandoa2.Connection = conna2.AbrirConexion();
                comandoa2.CommandText = "select DISTINCT PTS.ID_SUMMARY,PTS.TYPE_PACKING FROM PACKING_TYPE_SIZE PTS,PO_SUMMARY PS WHERE PTS.PACKING_NAME='" + nombre_packing + "' " +
                    " AND PS.ID_PEDIDOS='" + pedido + "' AND PS.ID_PO_SUMMARY=PTS.ID_SUMMARY ";
                leerFilasa2 = comandoa2.ExecuteReader();
                while (leerFilasa2.Read()) {
                    estilos e = new estilos();
                    e.id_po_summary= Convert.ToInt32(leerFilasa2["ID_SUMMARY"]);
                    e.id_estilo = consultas.obtener_estilo_summary(e.id_po_summary);
                    e.estilo = (consultas.obtener_estilo(e.id_estilo) + buscar_terminacion_estilo(e.id_po_summary, e.tipo_empaque)).Trim();
                    e.tipo_empaque = Convert.ToInt32(leerFilasa2["TYPE_PACKING"]);
                    e.descripcion = (consultas.buscar_descripcion_estilo(e.id_estilo)).Trim();
                    e.id_color = consultas.obtener_color_id_item(pedido, e.id_estilo);
                    e.color = consultas.obtener_color_id((e.id_color).ToString());
                    e.descripcion_final = Regex.Replace(buscar_descripcion_final_estilo(e.id_po_summary), @"\s+", " ");
                    //e.lista_ratio = obtener_lista_ratio(e.id_po_summary, e.id_estilo);
                    e.lista_ratio = obtener_lista_ratio_assort_r(e.id_po_summary, e.id_estilo, nombre_packing);
                    lista.Add(e);
                }leerFilasa2.Close();
            } finally { conna2.CerrarConexion(); conna2.Dispose(); }
            return lista;
        }
        public int obtener_tipo_empaque_pk(int pk) {
            int tempo = 0;
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = " select id_packing_type from packing_list where id_packing_list='" + pk + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    tempo = Convert.ToInt32(leer["id_packing_type"]);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }
        public List<Assortment> lista_assortings_packing(int pk) {
            List<Assortment> lista = new List<Assortment>();
            Link conn = new Link();
            try {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select id_shipping_id,id_estilo,cantidad from shipping_ids where used='" + pk + "' and id_tarima=0";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    Assortment a = new Assortment();
                    a.id_assortment = Convert.ToInt32(leerFilas["id_shipping_id"]);
                    a.cartones = Convert.ToInt32(leerFilas["cantidad"]);
                    a.nombre = buscar_nombre_assort(Convert.ToInt32(leerFilas["id_estilo"]));
                    a.pk = pk;
                    lista.Add(a);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public string buscar_nombre_assort(int assort) {
            string tempo = "";
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = " select PTZ.ASSORT_NAME FROM PACKING_ASSORT PA,PACKING_TYPE_SIZE PTZ WHERE PA.PACKING_NAME=PTZ.PACKING_NAME and PA.ID_PACKING_ASSORT='" + assort + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    tempo = Convert.ToString(leer["ASSORT_NAME"]);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }
        
        public Pk obtener_informacion_editar_pk(int pk) {
            Pk p = new Pk();
            Link connx = new Link();
            try {
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "select id_packing_list,pk,id_driver,id_container,seal,replacement,id_direccion_envio from packing_list where id_packing_list='" + pk + "' ";
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()) {
                    p.id_packing_list = Convert.ToInt32(leerFilasx["id_packing_list"]);
                    p.packing = Convert.ToString(leerFilasx["pk"]);
                    p.id_driver = Convert.ToInt32(leerFilasx["id_driver"]);
                    p.id_container = Convert.ToInt32(leerFilasx["id_container"]);
                    p.seal = Convert.ToString(leerFilasx["seal"]);
                    p.replacement = Convert.ToString(leerFilasx["replacement"]);
                    p.id_direccion_envio = Convert.ToInt32(leerFilasx["id_direccion_envio"]);
                } leerFilasx.Close();
            } finally { connx.CerrarConexion(); connx.Dispose(); }
            return p;
        }
        public void actualizar_datos_pk(string id, string sello, string replacement, string conductor, string contenedor,string direccion,string num_envio) {
            Link con_s = new Link();
            try {
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE packing_list SET id_driver='" + conductor + "',id_container='" + contenedor + "'," +
                    " seal='" + sello + "',replacement='" + replacement + "',id_direccion_envio='"+direccion+"',envio='"+num_envio+"' " +
                    " where id_packing_list='" + id + "'  ";
                com_s.ExecuteNonQuery();
            } finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void actualizar_datos_pk_df(string customer,string id, string sello, string replacement, string conductor, string contenedor, string direccion, string num_envio){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE packing_list SET id_customer_po='"+customer+"',id_driver='" + conductor + "',id_container='" + contenedor + "'," +
                    " seal='" + sello + "',replacement='" + replacement + "',id_direccion_envio='" + direccion + "',envio='" + num_envio + "' " +
                    " where id_packing_list='" + id + "'  ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void editar_pedido_fantasy(string id,string pedido,string customer){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE pedidos_fantasy SET pedido='" + pedido + "',id_cliente='" + customer + "' where id_pedido='" + id + "'  ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }

        public void actualizar_packing_parte(int packing,string final){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE packing_list SET parte='"+final+"'  where id_packing_list='" + packing + "'  ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }

        public int obtener_ultimo_shipping_id() {
            int id = 0;
            Link con_u_r = new Link();
            try {
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT TOP 1 id_shipping_id FROM shipping_ids order by id_shipping_id desc ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()) {
                    id = Convert.ToInt32(leer_u_r["id_shipping_id"]);
                } leer_u_r.Close();
            } finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return id;
        }
       
        public string buscar_po_number_pk(string pk) {
            string id = "";
            Link con_u_r = new Link();
            try {
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT number_po FROM shipping_ids where used='" + pk + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()) {
                    id = Convert.ToString(leer_u_r["number_po"]);
                } leer_u_r.Close();
            } finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return id;
        }
        public int buscar_pedido_pk(string pk) {
            int id = 0;
            Link con_u_r = new Link();
            try {
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT id_pedido FROM shipping_ids where used='" + pk + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()) {
                    id = Convert.ToInt32(leer_u_r["id_pedido"]);
                } leer_u_r.Close();
            } finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return id;
        }
        public List<Assortment> obtener_assortment_by_id(int id) {
            List<Assortment> aa = new List<Assortment>();
            Link conna1 = new Link();
            try {
                SqlCommand comandoa1 = new SqlCommand();
                SqlDataReader leerFilasa1 = null;
                comandoa1.Connection = conna1.AbrirConexion();
                comandoa1.CommandText = "select PTZ.ID_SUMMARY,PTZ.ID_TALLA,PTZ.RATIO from PACKING_ASSORT PA,PACKING_TYPE_SIZE PTZ where " +
                    " PA.PACKING_NAME=PTZ.PACKING_NAME and PA.ID_PACKING_ASSORT='" + id + "'   ";
                leerFilasa1 = comandoa1.ExecuteReader();
                while (leerFilasa1.Read()) {
                    Assortment a = new Assortment();
                    a.id_summary = Convert.ToInt32(leerFilasa1["ID_SUMMARY"]);
                    a.id_talla = Convert.ToInt32(leerFilasa1["ID_TALLA"]);
                    a.ratio = Convert.ToInt32(leerFilasa1["RATIO"]);
                    aa.Add(a);
                } leerFilasa1.Close();
            } finally { conna1.CerrarConexion(); conna1.Dispose(); }
            return aa;
        }
        public string buscar_packing_name(int assort) {
            string tempo = "";
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = " select PACKING_NAME FROM PACKING_ASSORT WHERE ID_PACKING_ASSORT='" + assort + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    tempo = Convert.ToString(leer["PACKING_NAME"]);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }

        public string buscar_packing_name_pedido(int pedido) {
            string tempo = "";
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = " select PA.PACKING_NAME FROM PACKING_ASSORT PA,PACKING_TYPE_SIZE PTZ WHERE PA.PACKING_NAME=PTZ.PACKING_NAME AND PA.ID_PEDIDO='" + pedido + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    tempo = Convert.ToString(leer["PACKING_NAME"]);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }

        public Cantidades_Estilos obtener_cantidades_estilos(int pedido) {
            //List<Cantidades_Estilos> lista = new List<Cantidades_Estilos>();
            Cantidades_Estilos lista = new Cantidades_Estilos();
            /*List<string> assorts = buscar_assorts_pedido(pedido);
            bool isEmpty = !assorts.Any();
            if (isEmpty) {
                lista = obtener_lista_cantidades_estilos(pedido);
            } else {
                lista = obtener_lista_cantidades_estilos_assort(pedido, assorts);
            }*/
            lista.total_enviado = obtener_total_enviado_pedido(pedido);
            lista.total_pedido = obtener_total_pedido(pedido);
            return lista;
        }
        public int obtener_total_enviado_pedido(int pedido){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select ID_PO_SUMMARY from PO_SUMMARY where ID_PEDIDOS='" + pedido + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo += buscar_total_enviado_summary(Convert.ToInt32(leer["ID_PO_SUMMARY"]));
                }
                leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            //tempo += buscar_cantidades_ejemplos(pedido);
            return tempo;
        }
        public int buscar_cantidades_ejemplos(int pedido){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select sr.total from shipping_ratio sr,shipping_ids si where sr.id_shipping_id=si.id_shipping_id and si.id_pedido='" + pedido + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo += Convert.ToInt32(leer["total"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }
        public int obtener_total_enviado_pedido_exclusivo(int pedido,int packing){
            int total_enviado = obtener_total_enviado_pedido(pedido);
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                //com.CommandText = "select id_shipping_id from shipping_ids  where id_pedido='" + pedido + "' and used!='"+packing+"'";
                com.CommandText = "select id_shipping_id from shipping_ids  where used='"+packing+"'";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo += buscar_total_enviado_shipping(Convert.ToInt32(leer["id_shipping_id"]));                   
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            //tempo += buscar_cantidades_ejemplos_exclusivo(pedido, packing);
            return (total_enviado-tempo);
        }

        public int obtener_total_enviado_pk( int packing){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                //com.CommandText = "select id_shipping_id from shipping_ids  where id_pedido='" + pedido + "' and used!='"+packing+"'";
                com.CommandText = "select id_shipping_id from shipping_ids  where used='" + packing + "'";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo += buscar_total_enviado_shipping(Convert.ToInt32(leer["id_shipping_id"]));
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }

        public int buscar_total_enviado_summary(int summary){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select total from totales_envios  where id_summary='" + summary + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo += Convert.ToInt32(leer["total"]);
                } leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }            
            return tempo;
        }
        public int buscar_total_enviado_shipping(int shipping){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select total from totales_envios  where id_shipping_id='" + shipping + "' and " +
                    " tipo_packing!=3 and tipo_packing!=4 and tipo_packing!=5 and tipo_packing!=6 and tipo_packing!=8 and tipo_packing!=9 ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo += Convert.ToInt32(leer["total"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }


        public int buscar_cantidades_ejemplos_exclusivo(int pedido,int packing){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select sr.total from shipping_ratio sr,shipping_ids si where sr.id_shipping_id=si.id_shipping_id and si.id_pedido='" + pedido + "' and si.used!='" + packing + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo += Convert.ToInt32(leer["total"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }





        public int obtener_total_pedido(int pedido){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select TOTAL_UNITS FROM PEDIDO where id_pedido='" + pedido + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo = Convert.ToInt32(leer["TOTAL_UNITS"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }

        public int obtener_total_extras_pedido_summary(int summary){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select EXTRAS FROM ITEM_SIZE where ID_SUMMARY='" + summary + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo += Convert.ToInt32(leer["EXTRAS"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }

        public int obtener_total_samples_pedido_summary(int summary){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select EJEMPLOS FROM ITEM_SIZE where ID_SUMMARY='" + summary + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo += Convert.ToInt32(leer["EJEMPLOS"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }

        public int obtener_total_enviado_extras_pedido_summary(int summary){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select total FROM totales_envios where ID_SUMMARY='" + summary + "' AND (tipo='EXT' or tipo='DMG' )";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo+= Convert.ToInt32(leer["total"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }
        public int obtener_total_enviado_samples_pedido_summary(int summary){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select total FROM totales_envios where ID_SUMMARY='" + summary + "' AND  tipo='SAM' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo += Convert.ToInt32(leer["total"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }

        public int obtener_total_enviado_extras_pedido_summary_exclusivo(int summary,int packing){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select id_fantasy FROM packing_fantasy where id_packing!='" + packing + "' and id_summary='"+summary+"'";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo += obtener_total_extras_summary_packing(summary,Convert.ToInt32(leer["id_fantasy"]));
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }
        public int obtener_total_enviado_samples_pedido_summary_exclusivo(int summary, int packing){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select id_sample FROM packing_samples where id_packing_list!='" + packing + "' and id_summary='" + summary + "'";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo += obtener_total_samples_summary_packing(summary, Convert.ToInt32(leer["id_sample"]));
                }
                leer.Close();
            }
            finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }

        public int obtener_total_samples_summary_packing(int summary, int shipping){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select total FROM totales_envios where id_shipping_id='" + shipping + "' and id_summary='" + summary + "' and tipo_packing=3";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo += Convert.ToInt32(leer["total"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }

        public int obtener_total_extras_summary_packing(int summary, int shipping){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select total FROM totales_envios where id_shipping_id='" + shipping + "' and id_summary='" + summary + "' and tipo_packing=6";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo += Convert.ToInt32(leer["total"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }

        public List<Cantidades_Estilos> obtener_lista_cantidades_estilos(int pedido) {
            List<Cantidades_Estilos> lista = new List<Cantidades_Estilos>();
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = " select ID_PO_SUMMARY,ITEM_ID,QTY FROM PO_SUMMARY WHERE ID_PEDIDOS='" + pedido + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    Cantidades_Estilos ce = new Cantidades_Estilos();
                    ce.id_pedido = pedido;
                    ce.id_summary = Convert.ToInt32(leer["ID_PO_SUMMARY"]);
                    ce.id_estilo = Convert.ToInt32(leer["ITEM_ID"]);
                    ce.cantidad_pedido = Convert.ToInt32(leer["QTY"]);
                    ce.lista_tallas = obtener_tallas_cantidades_estilo(ce.id_summary);
                    //ce.id_assort=buscar_ass
                    lista.Add(ce);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Talla> obtener_tallas_cantidades_estilo(int summary) {
            List<Talla> lista = new List<Talla>();
            int total, extras, ejemplos;
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = " select TALLA_ITEM,CANTIDAD,\"1RST_CALIDAD\",EXTRAS,EJEMPLOS FROM ITEM_SIZE WHERE ID_SUMMARY='" + summary + "' and TALLA_ITEM IS NOT NULL ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    Talla t = new Talla();
                    t.id_talla = Convert.ToInt32(leer["TALLA_ITEM"]);
                    total = obtener_total_enviadas(summary, t.id_talla, "NONE");
                    total += obtener_total_enviadas(summary, t.id_talla, "INITIAL");
                    extras = obtener_total_enviadas(summary, t.id_talla, "EXT");
                    ejemplos = obtener_total_enviadas(summary, t.id_talla, "EXAMPLES");
                    t.talla = consultas.obtener_size_id(Convert.ToString(t.id_talla));
                    t.total = Convert.ToInt32(leer["CANTIDAD"]) -Convert.ToInt32(leer["EXTRAS"])- Convert.ToInt32(leer["EJEMPLOS"]) - total;
                    t.extras = Convert.ToInt32(leer["EXTRAS"]) - extras;
                    t.ejemplos = Convert.ToInt32(leer["EJEMPLOS"]) - ejemplos;
                    t.ratio = consultas.obtener_ratio_summary_talla(summary, t.id_talla);
                    lista.Add(t);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public int obtener_total_enviadas(int summary, int talla, string tipo) {
            int id = 0;
            Link con_u_r = new Link();
            try {
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT total FROM totales_envios where id_summary='" + summary + "' and id_talla='" + talla + "' and tipo='" + tipo + "' and assort=0";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()) {
                    id += Convert.ToInt32(leer_u_r["total"]);
                } leer_u_r.Close();
            } finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return id;
        }

        public List<string> buscar_assorts_pedido(int pedido) {
            List<string> lista = new List<string>();
            Link con01 = new Link();
            try {
                SqlCommand com01 = new SqlCommand();
                SqlDataReader leer01 = null;
                com01.Connection = con01.AbrirConexion();
                com01.CommandText = " select ID_PACKING_ASSORT,PACKING_NAME FROM PACKING_ASSORT WHERE ID_PEDIDO='" + pedido + "' ";
                leer01 = com01.ExecuteReader();
                while (leer01.Read()) {
                    lista.Add(Convert.ToString(leer01["PACKING_NAME"]) + "*" + Convert.ToString(leer01["ID_PACKING_ASSORT"]));
                } leer01.Close();
            } finally { con01.CerrarConexion(); con01.Dispose(); }
            return lista;
        }
        public List<Cantidades_Estilos> obtener_lista_cantidades_estilos_assort(int pedido, List<string> assorts) {
            List<Cantidades_Estilos> lista = new List<Cantidades_Estilos>();
            foreach (string a in assorts) {
                string[] datos = a.Split('*');
                Link con02 = new Link();
                try {
                    SqlCommand com02 = new SqlCommand();
                    SqlDataReader leer02 = null;
                    com02.Connection = con02.AbrirConexion();
                    com02.CommandText = " select distinct ID_SUMMARY FROM PACKING_TYPE_SIZE WHERE PACKING_NAME='" + datos[0] + "' ";
                    leer02 = com02.ExecuteReader();
                    while (leer02.Read()) {
                        Cantidades_Estilos ce = new Cantidades_Estilos();
                        ce.id_pedido = pedido;
                        ce.id_summary = Convert.ToInt32(leer02["ID_SUMMARY"]);
                        ce.id_estilo = consultas.obtener_estilo_summary(ce.id_summary);
                        ce.lista_tallas = obtener_tallas_cantidades_estilo_assort(ce.id_summary, datos[0], datos[1]);
                        ce.id_assort = Convert.ToInt32(datos[1]);
                        lista.Add(ce);
                    } leer02.Close();
                } finally { con02.CerrarConexion(); con02.Dispose(); }
            }
            return lista;
        }
        public List<Talla> obtener_tallas_cantidades_estilo_assort(int summary, string assorts, string id_assort) {
            List<Talla> lista = new List<Talla>();
            int total, extras, ejemplos;
            Link con03 = new Link();
            try {
                SqlCommand com03 = new SqlCommand();
                SqlDataReader leer03 = null;
                com03.Connection = con03.AbrirConexion();
                com03.CommandText = " select TALLA_ITEM,CANTIDAD,\"1RST_CALIDAD\",EXTRAS,EJEMPLOS FROM ITEM_SIZE WHERE ID_SUMMARY='" + summary + "'  and TALLA_ITEM IS NOT NULL  ";
                leer03 = com03.ExecuteReader();
                while (leer03.Read()) {
                    Talla t = new Talla();
                    t.id_talla = Convert.ToInt32(leer03["TALLA_ITEM"]);
                    total = obtener_total_enviadas_assortment(summary, t.id_talla, "NONE", id_assort);
                    total += obtener_total_enviadas_assortment(summary, t.id_talla, "INITIAL", id_assort);
                    extras = obtener_total_enviadas_assortment(summary, t.id_talla, "EXT", id_assort);
                    ejemplos = obtener_total_enviadas_assortment(summary, t.id_talla, "EXAMPLES", id_assort);
                    t.talla = consultas.obtener_size_id(Convert.ToString(t.id_talla));
                    t.total = Convert.ToInt32(leer03["CANTIDAD"])-Convert.ToInt32(leer03["EXTRAS"])-Convert.ToInt32(leer03["EJEMPLOS"]) - total;
                    t.extras = Convert.ToInt32(leer03["EXTRAS"]) - extras;
                    t.ejemplos = Convert.ToInt32(leer03["EJEMPLOS"]) - ejemplos;
                    t.ratio = consultas.obtener_ratio_summary_talla(summary, t.id_talla);
                    lista.Add(t);
                } leer03.Close();
            } finally { con03.CerrarConexion(); con03.Dispose(); }
            return lista;
        }

        public int obtener_total_enviadas_assortment(int summary, int talla, string tipo, string assort) {
            int id = 0;
            Link con_u_r04 = new Link();
            try {
                SqlCommand com_u_r04 = new SqlCommand();
                SqlDataReader leer_u_r04 = null;
                com_u_r04.Connection = con_u_r04.AbrirConexion();
                com_u_r04.CommandText = "SELECT total FROM totales_envios where id_summary='" + summary + "' and id_talla='" + talla + "' and tipo='" + tipo + "' and assort='" + assort + "' ";
                leer_u_r04 = com_u_r04.ExecuteReader();
                while (leer_u_r04.Read()) {
                    id += Convert.ToInt32(leer_u_r04["total"]);
                } leer_u_r04.Close();
            } finally { con_u_r04.CerrarConexion(); con_u_r04.Dispose(); }
            return id;
        }

        public string obtener_number_po_pedido(int pedido) {
            string id = "";
            Link con_u_r04 = new Link();
            try {
                SqlCommand com_u_r04 = new SqlCommand();
                SqlDataReader leer_u_r04 = null;
                com_u_r04.Connection = con_u_r04.AbrirConexion();
                com_u_r04.CommandText = "SELECT VPO FROM PEDIDO WHERE ID_PEDIDO='" + pedido + "' ";
                leer_u_r04 = com_u_r04.ExecuteReader();
                while (leer_u_r04.Read()) {
                    id = Convert.ToString(leer_u_r04["VPO"]);
                } leer_u_r04.Close();
            } finally { con_u_r04.CerrarConexion(); con_u_r04.Dispose(); }
            return id;
        }

        public void cerrar_pedido(int pedido) {
            Link con_s = new Link();
            try {
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE PEDIDO SET ID_STATUS=7 WHERE ID_PEDIDO='" + pedido + "' ";
                com_s.ExecuteNonQuery();
            } finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        /***************************************************************************************************************************************************/
        /***************************************************************************************************************************************************/
        /***************************************************************************************************************************************************/
        public List<Pk> obtener_pedido_cantidades(string inicio, string final) {
            List<Pk> lista = new List<Pk>();
            Link connx = new Link();
            try {
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "select ID_PEDIDO,PO,DATE_CANCEL,DATE_ORDER from PEDIDO where " +
                    " DATE_ORDER BETWEEN '" + inicio + "' and '" + final + " 23:59:59' AND ID_STATUS!=7 AND ID_STATUS!=6 AND ID_STATUS!=5 ";
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()) {
                    Pk p = new Pk(); //Regex.Replace(color, @"\s+", " ");   
                    p.id_pedido = Convert.ToInt32(leerFilasx["ID_PEDIDO"]);
                    p.pedido = Regex.Replace(Convert.ToString(leerFilasx["PO"]), @"\s+", " ");
                    p.cancel_date = (Convert.ToDateTime(leerFilasx["DATE_CANCEL"])).ToString("MM/dd/yyyy");
                    p.lista_estilos = obtener_lista_estilos_pedido(p.id_pedido);
                    lista.Add(p);
                } leerFilasx.Close();
            } finally { connx.CerrarConexion(); connx.Dispose(); }
            return lista;
        }
        public List<estilos> obtener_lista_estilos_pedido(int pedido) {
            List<estilos> lista = new List<estilos>();
            Link conn = new Link();
            try { //Regex.Replace(color, @"\s+", " ");
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select ID_PO_SUMMARY,ITEM_ID,ID_COLOR FROM PO_SUMMARY WHERE ID_PEDIDOS='" + pedido + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()) {
                    estilos e = new estilos();
                    e.id_estilo = Convert.ToInt32(leerFilas["ITEM_ID"]);
                    e.id_po_summary = Convert.ToInt32(leerFilas["ID_PO_SUMMARY"]);
                    e.id_color = Convert.ToInt32(leerFilas["ID_COLOR"]);
                    e.color = Regex.Replace(consultas.obtener_color_id((e.id_color).ToString()), @"\s+", " ");
                    e.estilo = Regex.Replace(consultas.obtener_estilo(e.id_estilo), @"\s+", " ");
                    e.descripcion = Regex.Replace(consultas.buscar_descripcion_estilo(e.id_estilo), @"\s+", " ");
                    e.descripcion_final = Regex.Replace(buscar_descripcion_final_estilo(summary), @"\s+", " ");
                    e.lista_ratio = buscar_cantidades_faltantes_estilo(e.id_po_summary);
                    lista.Add(e);
                } leerFilas.Close();
            } finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<ratio_tallas> buscar_cantidades_faltantes_estilo(int summary) {
            List<ratio_tallas> lista = new List<ratio_tallas>();
            int enviadas;
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = " select TALLA_ITEM,CANTIDAD,\"1RST_CALIDAD\",EXTRAS,EJEMPLOS FROM ITEM_SIZE WHERE ID_SUMMARY='" + summary + "'  and TALLA_ITEM IS NOT NULL  ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    Talla t = new Talla();
                    t.id_talla = Convert.ToInt32(leer["TALLA_ITEM"]);
                    enviadas = obtener_total_enviadas_talla(summary, t.id_talla);
                    t.total = Convert.ToInt32(leer["CANTIDAD"]);// + Convert.ToInt32(leer["EXTRAS"]) + Convert.ToInt32(leer["EJEMPLOS"]);

                    ratio_tallas rt = new ratio_tallas();
                    rt.id_talla = t.id_talla;
                    rt.talla = Regex.Replace(consultas.obtener_size_id((rt.id_talla).ToString()), @"\s+", " ");
                    rt.total_talla = t.total - enviadas;//total restante de enviar por talla

                    lista.Add(rt);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public int obtener_total_enviadas_talla(int summary, int talla) {
            int id = 0;
            Link con_u_r = new Link();
            try {
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT total FROM totales_envios where id_summary='" + summary + "' and id_talla='" + talla + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()) {
                    id += Convert.ToInt32(leer_u_r["total"]);
                } leer_u_r.Close();
            } finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return id;
        }
        /***************************************************************************************************************************************************/
        public List<Estilo_Pedido> obtener_estilos_pedido_status(int pedido) {
            List<Estilo_Pedido> lista = new List<Estilo_Pedido>();
            Link con = new Link();
            try {//Regex.Replace(, @"\s+", " ");
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select ID_PO_SUMMARY,ITEM_ID,ID_COLOR FROM PO_SUMMARY WHERE ID_PEDIDOS='" + pedido + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    Estilo_Pedido e = new Estilo_Pedido();
                    e.id_estilo = Convert.ToInt32(leer["ITEM_ID"]);
                    e.id_summary = Convert.ToInt32(leer["ID_PO_SUMMARY"]);
                    e.id_color = Convert.ToInt32(leer["ID_COLOR"]);
                    e.color = Regex.Replace(consultas.obtener_color_id((e.id_color).ToString()), @"\s+", " ");
                    e.estilo = Regex.Replace(consultas.obtener_estilo(e.id_estilo), @"\s+", " ");
                    e.descripcion = Regex.Replace(consultas.buscar_descripcion_estilo(e.id_estilo), @"\s+", " ");
                    e.totales_pedido = buscar_totales_summary(e.id_summary);
                    e.lista_pk = obtener_pk_estilos(e.id_summary);
                    lista.Add(e);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Talla> buscar_totales_summary(int summary) {
            List<Talla> lista = new List<Talla>();
            Link con = new Link();
            try {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = " select TALLA_ITEM,CANTIDAD,\"1RST_CALIDAD\",EXTRAS,EJEMPLOS FROM ITEM_SIZE WHERE ID_SUMMARY='" + summary + "'  and TALLA_ITEM IS NOT NULL  ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    Talla t = new Talla();
                    t.id_talla = Convert.ToInt32(leer["TALLA_ITEM"]);
                    t.talla = Regex.Replace(consultas.obtener_size_id((t.id_talla).ToString()), @"\s+", " ");
                    t.total = Convert.ToInt32(leer["CANTIDAD"]);// + Convert.ToInt32(leer["EJEMPLOS"]) + Convert.ToInt32(leer["EXTRAS"]);
                    lista.Add(t);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Packing_Estilo> obtener_pk_estilos(int summary) {
            List<Packing_Estilo> lista = new List<Packing_Estilo>();
            Link con = new Link();
            try
            {//Regex.Replace(, @"\s+", " ");
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_shipping_id,used,tipo from shipping_ids where  id_po_summary='" + summary + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    Packing_Estilo pe = new Packing_Estilo();
                    pe.id_shipping = Convert.ToInt32(leer["id_shipping_id"]);
                    pe.id_packing = Convert.ToInt32(leer["used"]);
                    pe.tipo = Convert.ToString(leer["tipo"]);
                    pe.package = obtener_clave_packing(pe.id_packing);
                    pe.fecha = obtener_fecha_packing(pe.id_packing);
                    pe.lista_enviados = obtener_cantidades_enviado(pe.id_shipping, pe.tipo);
                    lista.Add(pe);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public string obtener_clave_packing(int packing) {
            string lista = "";
            Link con = new Link();
            try {//Regex.Replace(, @"\s+", " ");
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT pk from packing_list where id_packing_list='" + packing + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    lista = Convert.ToString(leer["pk"]);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public string obtener_fecha_packing(int packing) {
            string lista = "";
            Link con = new Link();
            try {//Regex.Replace(, @"\s+", " ");
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT fecha from packing_list where id_packing_list='" + packing + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    lista = (Convert.ToDateTime(leer["fecha"])).ToString("MM/dd/yyyy");
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Talla> obtener_cantidades_enviado(int shipping, string tipo) {
            List<Talla> lista = new List<Talla>();
            Link con = new Link();
            try {//Regex.Replace(, @"\s+", " ");
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_talla,total from totales_envios where id_shipping_id='" + shipping + "' and tipo='" + tipo + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    Talla t = new Talla();
                    t.total = Convert.ToInt32(leer["total"]);
                    t.id_talla = Convert.ToInt32(leer["id_talla"]);
                    lista.Add(t);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        /***************************************************************************************************************************************************/
        public List<Estilo_PO> obtener_pedidos_po_estilo(int estilo) {
            List<Estilo_PO> lista = new List<Estilo_PO>();
            Link con = new Link();
            try {//Regex.Replace(, @"\s+", " ");
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT ID_PO_SUMMARY,ID_COLOR,QTY,ID_PEDIDOS FROM PO_SUMMARY WHERE ITEM_ID='" + estilo + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    Estilo_PO epo = new Estilo_PO();
                    //e.id_shipping = Convert.ToInt32(leer["id_shipping_id"]);
                    epo.id_pedido = Convert.ToInt32(leer["ID_PEDIDOS"]);
                    epo.pedido = Regex.Replace(consultas.obtener_po_id((epo.id_pedido).ToString()), @"\s+", " ");
                    epo.id_summary = Convert.ToInt32(leer["ID_PO_SUMMARY"]);
                    epo.id_color = Convert.ToInt32(leer["ID_COLOR"]);
                    epo.total = Convert.ToInt32(leer["QTY"]);
                    epo.color = Regex.Replace(consultas.obtener_color_id((epo.id_color).ToString()), @"\s+", " ");
                    epo.estilo = Regex.Replace(consultas.obtener_estilo(estilo), @"\s+", " ");
                    epo.descripcion = Regex.Replace(consultas.buscar_descripcion_estilo(estilo), @"\s+", " ");
                    int enviadas = buscar_totales_enviadas_summary(epo.id_summary);
                    if (enviadas >= epo.total) {
                        epo.estado = "COMPLETE";
                    } else {
                        epo.estado = "INCOMPLETE";
                    }
                    epo.total = epo.total - enviadas;
                    lista.Add(epo);
                } leer.Close();
            } finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public int buscar_totales_enviadas_summary(int summary) {
            int id = 0;
            Link con_u_r = new Link();
            try {
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT total FROM totales_envios where id_summary='" + summary + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()) {
                    id += Convert.ToInt32(leer_u_r["total"]);
                } leer_u_r.Close();
            } finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return id;
        }
        /***************************************************************************************************************************************************/
        public List<Shipping_pk> obtener_listado_packing_year(string year) {
            List<Shipping_pk> lista = new List<Shipping_pk>();
            Link con = new Link();
            try{//Regex.Replace(, @"\s+", " ");
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_packing_list,pk,id_direccion_envio,id_pedido,fecha,id_packing_type,envio  FROM packing_list  WHERE year(fecha)='" + year + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Shipping_pk s = new Shipping_pk();
                    s.id_packing= Convert.ToInt32(leer["id_packing_list"]);
                    s.id_pedido= Convert.ToInt32(leer["id_pedido"]);
                    string[] t = (Convert.ToString(leer["pk"])).Split('-');
                    s.packing= t[0];
                    s.fecha= (Convert.ToDateTime(leer["fecha"])).ToString("MM/dd/yyyy");
                    s.num_envio = Convert.ToInt32(leer["envio"]);
                    s.pedido= Regex.Replace(consultas.obtener_po_id((s.id_pedido).ToString()), @"\s+", " ");
                    Direccion d = obtener_direccion(Convert.ToInt32(leer["id_direccion_envio"]));
                    s.destino = d.nombre;
                    int tipo_packing= Convert.ToInt32(leer["id_packing_type"]);
                    if (tipo_packing == 1) {
                        string[] t2 = (obtener_cantidades_piezas_packing_bullpack(s.id_packing)).Split('*');
                        s.piezas = Convert.ToInt32(t2[0]);
                        s.cajas = Convert.ToInt32(t2[1]);
                    }
                    if (tipo_packing == 2) {
                        string[] t2 = (obtener_cantidades_piezas_packing_ppk(s.id_packing)).Split('*');
                        s.piezas = Convert.ToInt32(t2[0]);
                        s.cajas = Convert.ToInt32(t2[1]);
                    }
                    if (tipo_packing == 2){
                        string[] t2 = (obtener_cantidades_piezas_packing_assort(s.id_packing)).Split('*');
                        s.piezas = Convert.ToInt32(t2[0]);
                        s.cajas = Convert.ToInt32(t2[1]);
                    }
                    s.pallets = obtener_numero_pallets(s.id_packing);
                    lista.Add(s);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public int obtener_numero_pallets(int packing){
            int total = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT distinct id_tarima FROM shipping_ids where used='" + packing + "' and id_tarima!=0";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    total++;
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return total;
        }
        public string obtener_cantidades_piezas_packing_bullpack(int packing){
            int total = 0;
            int cajas = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT cantidad,id_po_summary,id_talla,tipo FROM shipping_ids where used='" + packing + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    if (Convert.ToString(leer_u_r["tipo"]) == "NONE" || Convert.ToString(leer_u_r["tipo"]) == "INITIAL"){
                        int cantidad = Convert.ToInt32(leer_u_r["cantidad"]);
                        total += cantidad;
                        int piezas_caja = buscar_cajas_talla_estilo(Convert.ToInt32(leer_u_r["id_po_summary"]), Convert.ToInt32(leer_u_r["id_talla"]));
                        if (piezas_caja == 0) { piezas_caja = 1; }
                        cajas += (cantidad / piezas_caja);
                    }else {
                        total++;
                        cajas++;
                    }                    
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return (total.ToString()+"*"+cajas.ToString());
        }
        public string obtener_cantidades_piezas_packing_ppk(int packing){
            int total = 0;
            int cajas = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT cantidad,id_po_summary,tipo FROM shipping_ids where used='" + packing + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    if (Convert.ToString(leer_u_r["tipo"]) == "NONE" || Convert.ToString(leer_u_r["tipo"]) == "INITIAL"){
                        cajas += Convert.ToInt32(leer_u_r["cantidad"]);
                        total += (buscar_piezas_ratio(Convert.ToInt32(leer_u_r["id_po_summary"]))) * Convert.ToInt32(leer_u_r["cantidad"]);
                    }else {
                        cajas++;
                        total++;
                    }
                                       
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return (total.ToString() + "*" + cajas.ToString());
        }
        public int buscar_piezas_ratio(int summary){
            int id = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT RATIO FROM PACKING_TYPE_SIZE where ID_SUMMARY='" + summary + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    id += Convert.ToInt32(leer_u_r["RATIO"]);
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return id;
        }
        public string obtener_cantidades_piezas_packing_assort(int packing){
            int total = 0;
            int cajas = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT cantidad,id_po_summary,id_estilo,tipo FROM shipping_ids where used='" + packing + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    /*a.nombre = buscar_nombre_assort(id);
                    a.cartones = cantidad;
                    e.lista_ratio = obtener_lista_ratio_assort(a.id_summary, e.id_estilo, packing_name);*/
                    if (Convert.ToString(leer_u_r["tipo"]) == "NONE" || Convert.ToString(leer_u_r["tipo"]) == "INITIAL"){
                        string nombre = buscar_packing_name_assort(Convert.ToInt32(leer_u_r["id_estilo"]));
                        cajas += Convert.ToInt32(leer_u_r["cantidad"]);
                        total += (buscar_piezas_ratio_assort(Convert.ToInt32(leer_u_r["id_po_summary"]), nombre)) * Convert.ToInt32(leer_u_r["cantidad"]);
                    }else {
                        cajas++;
                        total++;
                    }
                        
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return (total.ToString() + "*" + cajas.ToString());
        }
        public int buscar_piezas_ratio_assort(int summary,string name){
            int id = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT RATIO FROM PACKING_TYPE_SIZE where ID_SUMMARY='" + summary + "' and  PACKING_NAME='"+name+"' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    id += Convert.ToInt32(leer_u_r["RATIO"]);
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return id;
        }
        public string buscar_packing_name_assort(int assort){
            string tempo = "";
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = " select PACKING_NAME FROM PACKING_ASSORT  WHERE ID_PACKING_ASSORT='" + assort + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo = Convert.ToString(leer["PACKING_NAME"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }
        /***************************************************************************************************************************************************/
        public List<Shipping_pk> obtener_listado_packing_diario(){
            List<Shipping_pk> lista = new List<Shipping_pk>();
            Link con = new Link();
            try{//Regex.Replace(, @"\s+", " ");
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_packing_list,pk,id_direccion_envio,id_pedido,fecha,id_packing_type,envio FROM packing_list "+
                    " WHERE fecha between '" + DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00' and '"+ DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Shipping_pk s = new Shipping_pk();
                    s.id_packing = Convert.ToInt32(leer["id_packing_list"]);
                    s.id_pedido = Convert.ToInt32(leer["id_pedido"]);
                    string[] t = (Convert.ToString(leer["pk"])).Split('-');
                    s.packing = t[0];
                    s.fecha = (Convert.ToDateTime(leer["fecha"])).ToString("MM/dd/yyyy");
                    s.num_envio = Convert.ToInt32(leer["envio"]);
                    s.pedido = Regex.Replace(consultas.obtener_po_id((s.id_pedido).ToString()), @"\s+", " ");
                    Direccion d = obtener_direccion(Convert.ToInt32(leer["id_direccion_envio"]));
                    s.destino = d.nombre;
                    int tipo_packing = Convert.ToInt32(leer["id_packing_type"]);
                    if (tipo_packing == 1){
                        string[] t2 = (obtener_cantidades_piezas_packing_bullpack(s.id_packing)).Split('*');
                        s.piezas = Convert.ToInt32(t2[0]);
                        s.cajas = Convert.ToInt32(t2[1]);
                    }
                    if (tipo_packing == 2){
                        string[] t2 = (obtener_cantidades_piezas_packing_ppk(s.id_packing)).Split('*');
                        s.piezas = Convert.ToInt32(t2[0]);
                        s.cajas = Convert.ToInt32(t2[1]);
                    }
                    if (tipo_packing == 3){
                        string[] t2 = (obtener_cantidades_piezas_packing_assort(s.id_packing)).Split('*');
                        s.piezas = Convert.ToInt32(t2[0]);
                        s.cajas = Convert.ToInt32(t2[1]);
                    }
                    s.pallets = obtener_numero_pallets(s.id_packing);
                    lista.Add(s);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        /***************************************************************************************************************************************************/
        public List<Pk> obtener_pedido_cantidades_orden(int pedido){
            List<Pk> lista = new List<Pk>();
            Link connx = new Link();
            try{
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "select ID_PEDIDO,PO,DATE_CANCEL,DATE_ORDER from PEDIDO where ID_PEDIDO='" + pedido + "'   AND ID_STATUS!=7 AND ID_STATUS!=5  AND ID_STATUS!=6";
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()){
                    Pk p = new Pk(); //Regex.Replace(color, @"\s+", " ");   
                    p.id_pedido = Convert.ToInt32(leerFilasx["ID_PEDIDO"]);
                    p.pedido = Regex.Replace(Convert.ToString(leerFilasx["PO"]), @"\s+", " ");
                    p.cancel_date = (Convert.ToDateTime(leerFilasx["DATE_CANCEL"])).ToString("MM/dd/yyyy");
                    p.lista_estilos = obtener_lista_estilos_pedido(p.id_pedido);
                    p.total_po = buscar_totales(pedido);
                    p.total_enviado = buscar_totales_enviados(pedido);
                    lista.Add(p);
                }leerFilasx.Close();
            }finally { connx.CerrarConexion(); connx.Dispose(); }
            return lista;
        }
        public int buscar_totales(int pedido){
            int total = 0;            
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT ID_PO_SUMMARY,QTY FROM PO_SUMMARY where ID_PEDIDOS='" + pedido + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    total += Convert.ToInt32(leer_u_r["QTY"]);
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return total;
        }
        public int buscar_totales_enviados(int pedido){
            int enviado = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT ID_PO_SUMMARY,QTY FROM PO_SUMMARY where ID_PEDIDOS='" + pedido + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    enviado += buscar_totales_enviadas_summary(Convert.ToInt32(leer_u_r["ID_PO_SUMMARY"]));
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return enviado;
        }
        /***************************************************************************************************************************************************/
        public List<Po> obtener_lista_pedidos() {
            List<Po> lista = new List<Po>();
            Link con = new Link();
            try{//Regex.Replace(, @"\s+", " ");
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT ID_PEDIDO,PO,CUSTOMER,CUSTOMER_FINAL,DATE_CANCEL,TOTAL_UNITS,ID_STATUS FROM PEDIDO ORDER BY ID_PEDIDO ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Po p = new Po();
                    p.id_pedido = Convert.ToInt32(leer["ID_PEDIDO"]);
                    p.pedido= Regex.Replace(Convert.ToString(leer["PO"]), @"\s+", " ");
                    p.customer = Regex.Replace(consultas.obtener_customer_id(Convert.ToString(leer["CUSTOMER"])), @"\s+", " ");
                    p.customer_po = Regex.Replace(consultas.obtener_customer_final_id(Convert.ToString(leer["CUSTOMER_FINAL"])), @"\s+", " ");
                    p.fecha_cancelacion= (Convert.ToDateTime(leer["DATE_CANCEL"])).ToString("MM/dd/yyyy");
                    p.total= Convert.ToInt32(leer["TOTAL_UNITS"]);
                    p.estilos = buscar_total_estilos_pedido(p.id_pedido);
                    if (Convert.ToInt32(leer["ID_STATUS"]) == 6) { p.estado = "CANCELLED"; }
                    else {
                        if (Convert.ToInt32(leer["ID_STATUS"]) == 7) { p.estado = "COMPLETED"; }
                        else { p.estado = "INCOMPLETE"; }
                    }
                    lista.Add(p);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public int buscar_total_estilos_pedido(int pedido){
            int total = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT ID_PO_SUMMARY FROM PO_SUMMARY where ID_PEDIDOS='" + pedido + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    total++;
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return total;
        }
        /***************************************************************************************************************************************************/
        /***************************************************************************************************************************************************/
        /***************************************************************************************************************************************************/

        public void eliminar_inventario_pedido(int pedido) {
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_inventario FROM inventario WHERE id_pedido='"+pedido+"' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    vaciar_cajas_inventario(Convert.ToInt32(leer["id_inventario"]));
                    vaciar_inventario(Convert.ToInt32(leer["id_inventario"]));
                }
                leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
        }
        public void vaciar_cajas_inventario(int item){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                //com_s.CommandText = "UPDATE cajas_inventario SET cantidad_restante=0 where id_inventario='"+item+"' ";
                com_s.CommandText = "DELETE FROM cajas_inventario WHERE id_inventario='"+item+"' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void vaciar_inventario(int item){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                //com_s.CommandText = "DELETE FROM inventario WHERE id_inventario='" + item + "' ";
                com_s.CommandText = "UPDATE inventario SET total=0 WHERE id_inventario='" + item + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }

        public List<Talla> obtener_lista_tallas_pedido(List<estilo_shipping> estilos)
        {
            List<Talla> lista = new List<Talla>();
            List<Talla> lista2 = new List<Talla>();
            foreach (estilo_shipping e in estilos) {
                Link con = new Link();
                try{
                    SqlCommand com = new SqlCommand();
                    SqlDataReader leer = null;
                    com.Connection = con.AbrirConexion();
                    com.CommandText = "SELECT TALLA_ITEM FROM ITEM_SIZE WHERE ID_SUMMARY='" + e.id_summary + "' and TALLA_ITEM IS NOT NULL   ";
                    leer = com.ExecuteReader();
                    while (leer.Read()){
                        Talla t = new Talla();
                        t.id_talla = Convert.ToInt32(leer["TALLA_ITEM"]);
                        t.talla = consultas.obtener_size_id(Convert.ToString(leer["TALLA_ITEM"]));
                        t.total = 0; t.ratio = 0;
                        bool isEmpty = !lista.Any();
                        if (isEmpty){
                            lista.Add(t);
                        }else{
                            int existe = 0;
                            foreach (Talla size in lista) {
                                if (size.id_talla == t.id_talla) {
                                    existe++;
                                }
                            }
                            if (existe == 0) {
                                lista.Add(t);
                            }
                        }
                    }
                    leer.Close();
                }finally { con.CerrarConexion(); con.Dispose(); }
            }
            string query= "SELECT ID,TALLA FROM CAT_ITEM_SIZE WHERE ";
            int contador = 0;
            foreach (Talla t in lista) {
                if (contador == 0) {
                    query += " ID='"+t.id_talla+"' ";
                } else {
                    query += " or ID='" + t.id_talla + "'  ";
                }
                contador++;                
            }
            query += " ORDER by cast(ORDEN AS int) ASC ";
            Link conn = new Link();
            try{
                SqlCommand comn = new SqlCommand();
                SqlDataReader leern = null;
                comn.Connection = conn.AbrirConexion();
                comn.CommandText = query;
                leern = comn.ExecuteReader();
                while (leern.Read()){
                    Talla t = new Talla();
                    t.id_talla = Convert.ToInt32(leern["ID"]);
                    t.talla = (Convert.ToString(leern["TALLA"]));
                    t.total = 0; t.ratio = 0;
                    lista2.Add(t);
                }leern.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista2;
        }


        public List<Talla> obtener_tallas_pk(List<Talla> tallas) {
            List<Talla> lista = new List<Talla>();
            string query = "SELECT ID,TALLA FROM CAT_ITEM_SIZE WHERE ";
            int contador = 0;
            foreach (Talla t in tallas){
                if (contador == 0){
                    query += " ID='" + t.id_talla + "' ";
                }else{
                    query += " or ID='" + t.id_talla + "'  ";
                }
                contador++;
            }
            query += "  ORDER by cast(ORDEN AS int) ASC ";
            Link conn = new Link();
            try{
                SqlCommand comn = new SqlCommand();
                SqlDataReader leern = null;
                comn.Connection = conn.AbrirConexion();
                comn.CommandText = query;
                leern = comn.ExecuteReader();
                while (leern.Read()){
                    Talla t = new Talla();
                    t.id_talla = Convert.ToInt32(leern["ID"]);
                    t.talla = (Convert.ToString(leern["TALLA"]));
                    t.total = 0; t.ratio = 0;
                    lista.Add(t);
                }leern.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }




        public List<Pk> obtener_packing_list_bol(int pk)
        {
            List<Pk> lista = new List<Pk>();
            Link connx = new Link();
            try{
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "select nombre_archivo,id_packing_list,pk,id_customer,id_customer_po,id_direccion_envio,id_pedido,id_driver,id_container,shipping_manager," +
                    "seal,replacement,fecha,tipo,id_packing_type,parte from packing_list where id_packing_list='" + pk + "' ";
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()){
                    int id_customer_po;
                    Pk p = new Pk(); //Regex.Replace(color, @"\s+", " ");
                    p.id_packing_list = Convert.ToInt32(leerFilasx["id_packing_list"]);
                    p.packing = Convert.ToString(leerFilasx["pk"]);
                    p.customer = Regex.Replace(consultas.obtener_customer_id(Convert.ToString(leerFilasx["id_customer"])), @"\s+", " ");
                    p.customer_po = Regex.Replace(consultas.obtener_customer_final_id(Convert.ToString(leerFilasx["id_customer_po"])), @"\s+", " ");
                    p.destino = obtener_direccion(Convert.ToInt32(leerFilasx["id_direccion_envio"]));
                    p.id_pedido = Convert.ToInt32(leerFilasx["id_pedido"]);
                    p.id_tipo = Convert.ToInt32(leerFilasx["id_packing_type"]);
                    if (p.id_tipo != 8 && p.id_tipo != 4){
                        p.pedido = consultas.obtener_po_id((p.id_pedido).ToString());
                        id_customer_po = consultas.obtener_customer_final_po(p.id_pedido);
                    }else{
                        p.pedido = consultas.obtener_po_id_fantasy((p.id_pedido).ToString());
                        id_customer_po = Convert.ToInt32(leerFilasx["id_customer_po"]);
                    }
                    p.nombre_archivo = Convert.ToString(leerFilasx["nombre_archivo"]);
                    //p.pedido = consultas.obtener_po_id(Convert.ToString(leerFilasx["id_pedido"]));
                    //id_pedido = Convert.ToInt32(leerFilasx["id_pedido"]);
                    p.conductor = obtener_driver(Convert.ToInt32(leerFilasx["id_driver"]));
                    p.contenedor = obtener_contenedor(Convert.ToInt32(leerFilasx["id_container"]));
                    p.shipping_manager = Convert.ToString(leerFilasx["shipping_manager"]);
                    p.seal = Convert.ToString(leerFilasx["seal"]);
                    p.replacement = Convert.ToString(leerFilasx["replacement"]);
                    p.fecha = (Convert.ToDateTime(leerFilasx["fecha"])).ToString("MM/dd/yyyy");                   
                    //p.parte = buscar_pks_pedido(p.id_pedido, p.id_packing_list);                    
                    //p.parte = consultas.AddOrdinal(Convert.ToInt32(leerFilasx["parte"])) + " Part";
                    switch (p.id_tipo) {
                        case 1:                       
                        case 2:
                        case 9:
                            p.total_tarimas = obtener_total_tarimas_pk(pk);
                            p.total_piezas = obtener_total_piezas_pk(pk);// + obtener_total_piezas_pk_extras(pk);
                            p.total_cajas = obtener_total_cajas_pk(pk, p.id_tipo);
                            break;
                        case 3:
                            p.total_tarimas = 0;
                            p.total_cajas = obtener_total_cajas_pk_samples(pk);
                            p.total_piezas = obtener_total_piezas_pk_samples(pk);
                            p.pedido = "SAMPLES";                                  
                            break;
                        case 5:
                            p.total_tarimas = obtener_total_tarimas_pk_returns(pk);
                            p.total_cajas = obtener_total_cajas_pk_returns(pk);
                            p.total_piezas = obtener_total_piezas_pk_returns(pk);                            
                            break;
                        case 6:
                            p.total_tarimas = obtener_total_tarimas_pk_fantasy_extras(pk);
                            p.total_cajas = obtener_total_cajas_pk_fantasy_extras(pk);
                            p.total_piezas = obtener_total_piezas_pk_fantasy_extras(pk);
                            break;
                        case 7:
                            p.total_tarimas = obtener_total_tarimas_pk(pk);
                            p.total_piezas = obtener_total_piezas_pk(pk);// + obtener_total_piezas_pk_extras(pk);
                            p.total_cajas = obtener_total_cajas_pk_ht(pk, p.id_tipo);
                            break;
                        case 8:
                            p.lista_estilos = obtener_lista_estilos_tarima(0, pk);
                            p.total_tarimas = 0; p.total_piezas = 0;
                            List<int> indices = lista_indices_directo_fantasy(pk);
                            foreach (int i in indices) { p.total_tarimas++; }
                            foreach (estilos e in p.lista_estilos) { p.total_piezas += e.boxes; }
                            p.total_cajas = (p.total_piezas/6);                          
                            break;
                        case 4:
                            p.lista_tarimas = obtener_tarimas(p.id_packing_list);
                            //p.nombre_archivo = p.customer_po + " RPLN (PO#" + p.pedido + ") (With UCC Labels)";                            
                            foreach (Tarima t in p.lista_tarimas) {
                                p.total_tarimas++;
                                foreach (estilos e in t.lista_estilos) {
                                    p.total_piezas += e.boxes;
                                }
                            }
                            p.total_cajas = (p.total_piezas / 6);
                            break;
                    }                   
                    lista.Add(p);
                }leerFilasx.Close();
            }finally { connx.CerrarConexion(); connx.Dispose(); }
            return lista;
        }
        public int obtener_total_tarimas_pk(int packing){
            int total = 0;
            List<int> lista = new List<int>();
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT distinct id_tarima FROM shipping_ids WHERE used='" + packing + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    lista.Add(Convert.ToInt32(leer_u_r["id_tarima"]));
                    //total++;
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            int normales = 0, extras = 0;
            foreach (int i in lista) {
                normales = 0; extras = 0;
                List<string> tipos = obtener_tipos_tarimas(i, packing);
                foreach (string s in tipos) { if (s == "DMG" || s == "EXT") { extras++; } else { normales++; } }
                if (normales != 0) { total++; }
            }
            return total;
        }
        public List<string> obtener_tipos_tarimas(int tarima,int packing){
            List<string> lista = new List<string>();
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT tipo FROM shipping_ids WHERE id_tarima='" + tarima + "' and used='"+packing+"' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    lista.Add(Convert.ToString(leer_u_r["tipo"]));
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return lista;
        }
        public int obtener_total_tarimas_pk_samples(int packing){
            int total = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT DISTINCT id_tarima FROM packing_samples WHERE id_packing_list='" + packing + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    total++;
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return total;
        }
        public int obtener_total_tarimas_pk_fantasy_extras(int packing)
        {
            int total = 0;
            Link con_u_r = new Link();
            try
            {
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT DISTINCT id_tarima FROM packing_fantasy WHERE id_packing='" + packing + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read())
                {
                    total++;
                }
                leer_u_r.Close();
            }
            finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return total;
        }
        public int obtener_total_tarimas_pk_returns(int packing){
            int total = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT DISTINCT id_tarima FROM packing_returns WHERE id_packing='" + packing + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    total++;
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return total;
        }

        public int obtener_total_piezas_pk(int packing){
            int total = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "(SELECT te.total from totales_envios te,shipping_ids si where si.used='" + packing + "' and si.id_shipping_id=te.id_shipping_id)";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    total+=Convert.ToInt32(leer_u_r["total"]);
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return total;
        }

        public int obtener_total_piezas_pk_samples(int packing){
            int total = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT id_sample,talla_xs,talla_s,talla_m,talla_l,talla_xl,talla_2x,talla_3x FROM packing_samples WHERE id_packing_list='" + packing + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    Sample s = new Sample();
                    s.talla_xs = Convert.ToInt32(leer_u_r["talla_xs"]);
                    s.talla_s = Convert.ToInt32(leer_u_r["talla_s"]);
                    s.talla_m = Convert.ToInt32(leer_u_r["talla_m"]);
                    s.talla_l = Convert.ToInt32(leer_u_r["talla_l"]);
                    s.talla_xl = Convert.ToInt32(leer_u_r["talla_xl"]);
                    s.talla_2x = Convert.ToInt32(leer_u_r["talla_2x"]);
                    s.talla_3x = Convert.ToInt32(leer_u_r["talla_3x"]);                    
                    s.total = s.talla_s + s.talla_xs + s.talla_m + s.talla_l + s.talla_xl + s.talla_2x + s.talla_3x;
                    total += s.total;
                    total += obtener_total_ejemplos_extras(Convert.ToInt32(leer_u_r["id_sample"]));
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return total;
        }
        public int obtener_total_ejemplos_extras(int sample){
            int total = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT total from extras_samples where id_packing_sample='" + sample + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    total += Convert.ToInt32(leer_u_r["total"]);
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return total;
        }

        public int obtener_total_piezas_pk_fantasy_extras(int packing){
            int total = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT talla_s,talla_m,talla_l,talla_xl,talla_2x FROM packing_fantasy WHERE id_packing='" + packing + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    Sample s = new Sample();
                    s.talla_s = Convert.ToInt32(leer_u_r["talla_s"]);
                    s.talla_m = Convert.ToInt32(leer_u_r["talla_m"]);
                    s.talla_l = Convert.ToInt32(leer_u_r["talla_l"]);
                    s.talla_xl = Convert.ToInt32(leer_u_r["talla_xl"]);
                    s.talla_2x = Convert.ToInt32(leer_u_r["talla_2x"]);
                    s.total = s.talla_s + s.talla_m + s.talla_l + s.talla_xl + s.talla_2x;
                    total += s.total;
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return total;
        }

        public int obtener_total_piezas_pk_extras(int packing){
            int total = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT sr.total from shipping_ratio sr,shipping_ids si where si.used='" + packing + "' and si.id_shipping_id=sr.id_shipping_id";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    total += Convert.ToInt32(leer_u_r["total"]);
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return total;
        }

        public int obtener_total_cajas_pk(int packing,int tipo_packing){
            int total = 0;
            List<estilos> lista = new List<estilos>();
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT packing_name,id_tarima,cantidad,tipo_empaque,id_po_summary,id_talla,id_pedido,tipo,index_dc,repeticiones FROM shipping_ids WHERE used='" + packing + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    estilos e = new estilos();                    
                    e.tipo_empaque= Convert.ToInt32(leer_u_r["tipo_empaque"]);
                    e.id_tarima = Convert.ToInt32(leer_u_r["id_tarima"]);
                    e.id_talla= Convert.ToInt32(leer_u_r["id_talla"]);
                    e.boxes= Convert.ToInt32(leer_u_r["cantidad"]);
                    e.tipo= Convert.ToString(leer_u_r["tipo"]);
                    e.index_dc= Convert.ToInt32(leer_u_r["index_dc"]);
                    e.id_po_summary= Convert.ToInt32(leer_u_r["id_po_summary"]);
                    e.repeticiones= Convert.ToInt32(leer_u_r["repeticiones"]);
                    e.packing_name = Convert.ToString(leer_u_r["packing_name"]);
                    lista.Add(e);                  
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }

            List<int> index = new List<int>();//BUSCO LOS INDEX DE LOS DC
            foreach (estilos e in lista){ index.Add(e.index_dc); }
            index = index.Distinct().ToList();
            
            foreach (int indice in index) {
                int cajas = 0;
                foreach (estilos e in lista) {                    
                    if (e.index_dc == indice) {
                        if (e.tipo == "EXT" || e.tipo == "DMG"){
                            cajas = e.repeticiones;
                        }else {
                            if (e.tipo_empaque == 1) {
                                if (e.dc != "0"){
                                    cajas += e.boxes;
                                }else{
                                    cajas += (e.boxes / buscar_piezas_empaque_bull(e.id_po_summary, e.id_talla));
                                }
                            } else {
                                if (e.tipo_empaque == 5){
                                    if (e.dc != "0"){
                                        cajas += e.boxes;
                                    }else{
                                        cajas += (e.boxes / buscar_piezas_empaque_bulls(e.id_po_summary, e.id_talla, e.packing_name));
                                    }
                                }else{
                                    cajas = e.boxes;
                                }
                            }
                        }
                    }
                }
                total += cajas;
            }




            return total;
    }
        public int obtener_total_cajas_pk_ht(int packing, int tipo_packing){
            List<estilos> lista = buscar_estilos_packing_hottopic(packing);
            int total = 0;
            List<int> indices = new List<int>();
            foreach (estilos estilo in lista){
                bool isEmpty = !indices.Any();
                if (isEmpty) { indices.Add(estilo.index_dc); }
                else{
                    int existe = 0;
                    foreach (int i in indices) { if (i == estilo.index_dc) { existe++; } }
                    if (existe == 0) { indices.Add(estilo.index_dc); }
                }
            }
            foreach (int i in indices){
                int extras = 0,cajas_temporal=0;
                foreach (estilos e in lista){                   
                    if (e.index_dc == i){
                        if (e.tipo != "EXT" && e.tipo != "DMG"){
                            if (e.tipo_empaque == 1){
                                if (e.boxes >= 50) { total += e.boxes / 50; }
                                if ((e.boxes % 50) != 0) { total++; }
                            }else{
                                total += e.boxes;
                            }
                        }else{
                            if (extras == 0){
                                //total++;
                                cajas_temporal = e.repeticiones;
                                extras++;
                            }
                        }
                    }
                }
                total += cajas_temporal;
            }
            return total;
        }

        public List<estilos> buscar_estilos_packing_hottopic(int packing) {
                     List<estilos> lista = new List<estilos>();
                     int total = 0;
                     Link con_u_r = new Link();
                     try{
                         SqlCommand com_u_r = new SqlCommand();
                         SqlDataReader leer_u_r = null;
                         com_u_r.Connection = con_u_r.AbrirConexion();
                         com_u_r.CommandText = "SELECT id_po_summary,repeticiones,cantidad,tipo_empaque,tipo,number_po,id_talla,index_dc FROM shipping_ids WHERE used='" + packing + "' ";
                         leer_u_r = com_u_r.ExecuteReader();
                         while (leer_u_r.Read()){
                             estilos e = new estilos();
                             e.tipo_empaque = Convert.ToInt32(leer_u_r["tipo_empaque"]);
                             e.id_talla = Convert.ToInt32(leer_u_r["id_talla"]);
                             e.boxes = Convert.ToInt32(leer_u_r["cantidad"]);
                             e.repeticiones = Convert.ToInt32(leer_u_r["repeticiones"]);
                             e.id_po_summary = Convert.ToInt32(leer_u_r["id_po_summary"]);
                             e.number_po = Convert.ToString(leer_u_r["number_po"]);
                             e.tipo = Convert.ToString(leer_u_r["tipo"]);
                             e.index_dc = Convert.ToInt32(leer_u_r["index_dc"]);
                             lista.Add(e);
                         }
                         leer_u_r.Close();
                     }
                     finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
                     return lista;
                 }

                 public int buscar_piezas_bpht(int id_po_summary,int number_po, int talla,int empaque){
                     int total = 0;
                     Link con_u_r = new Link();
                     try{
                         SqlCommand com_u_r = new SqlCommand();
                         SqlDataReader leer_u_r = null;
                         com_u_r.Connection = con_u_r.AbrirConexion();
                         com_u_r.CommandText = "SELECT QTY,CARTONES FROM packing_type_size WHERE " +
                             " id_summary='" + id_po_summary + "' and number_po='" + number_po + "' and id_talla='" + talla + "' and type_packing='" + empaque + "' ";
                         leer_u_r = com_u_r.ExecuteReader();
                         while (leer_u_r.Read()){
                             int cartones = Convert.ToInt32(leer_u_r["CARTONES"]), qty = Convert.ToInt32(leer_u_r["QTY"]);
                             if (Convert.ToInt32(leer_u_r["CARTONES"]) != 0 && Convert.ToInt32(leer_u_r["QTY"]) != 0){
                                 total = Convert.ToInt32(leer_u_r["QTY"]) / Convert.ToInt32(leer_u_r["CARTONES"]);
                             }else {
                                 total++;
                             }
                         }leer_u_r.Close();
                     }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
                     return total;
                 }

                 public int obtener_total_cajas_pk_samples(int packing){
                     int total = 0;
                     Link con_u_r = new Link();
                     try{
                         SqlCommand com_u_r = new SqlCommand();
                         SqlDataReader leer_u_r = null;
                         com_u_r.Connection = con_u_r.AbrirConexion();
                         com_u_r.CommandText = "SELECT cajas FROM packing_samples WHERE id_packing_list='" + packing + "' ";
                         leer_u_r = com_u_r.ExecuteReader();
                         while (leer_u_r.Read()){
                            total +=Convert.ToInt32(leer_u_r["cajas"]);
                         }leer_u_r.Close();
                     }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
                     return total;
                 }
                 public int obtener_total_cajas_pk_fantasy_extras(int packing)
                 {
                     int total = 0;
                     Link con_u_r = new Link();
                     try
                     {
                         SqlCommand com_u_r = new SqlCommand();
                         SqlDataReader leer_u_r = null;
                         com_u_r.Connection = con_u_r.AbrirConexion();
                         com_u_r.CommandText = "SELECT cajas FROM packing_fantasy WHERE id_packing='" + packing + "' ";
                         leer_u_r = com_u_r.ExecuteReader();
                         while (leer_u_r.Read())
                         {
                             total += Convert.ToInt32(leer_u_r["cajas"]);
                         }
                         leer_u_r.Close();
                     }
                     finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
                     return total;
                 }

                 public int obtener_total_cajas_pk_returns(int packing){
                     int total = 0;
                     Link con_u_r = new Link();
                     try{
                         SqlCommand com_u_r = new SqlCommand();
                         SqlDataReader leer_u_r = null;
                         com_u_r.Connection = con_u_r.AbrirConexion();
                         com_u_r.CommandText = "SELECT cajas FROM packing_returns WHERE id_packing='" + packing + "' ";
                         leer_u_r = com_u_r.ExecuteReader();
                         while (leer_u_r.Read()){
                             total += Convert.ToInt32(leer_u_r["cajas"]);
                         }leer_u_r.Close();
                     }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
                     return total;
                 }
                 public int obtener_total_piezas_pk_returns(int packing){
                     int total = 0;
                     Link con_u_r = new Link();
                     try{
                         SqlCommand com_u_r = new SqlCommand();
                         SqlDataReader leer_u_r = null;
                         com_u_r.Connection = con_u_r.AbrirConexion();
                         com_u_r.CommandText = "SELECT total FROM packing_returns WHERE id_packing='" + packing + "' ";
                         leer_u_r = com_u_r.ExecuteReader();
                         while (leer_u_r.Read()){
                             total += Convert.ToInt32(leer_u_r["total"]);
                         }leer_u_r.Close();
                     }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
                     return total;
                 }

                 public List<Container> obtener_contenedores(){
                     List<Container> lista = new List<Container>();
                     Link conn = new Link();
                     try{
                         SqlCommand comando = new SqlCommand();
                         SqlDataReader leerFilas = null;
                         comando.Connection = conn.AbrirConexion();
                         comando.CommandText = "select id_container,eco,plates from containers ";
                         leerFilas = comando.ExecuteReader();
                         while (leerFilas.Read()){
                             Container e = new Container();
                             e.id_container = Convert.ToInt32(leerFilas["id_container"]);
                             e.eco = Convert.ToString(leerFilas["eco"]);
                             e.plates = Convert.ToString(leerFilas["plates"]);
                             lista.Add(e);
                         }leerFilas.Close();
                     }finally { conn.CerrarConexion(); conn.Dispose(); }
                     return lista;
                 }

                 public List<Container> obtener_contenedor_edicion(string id){
                     List<Container> lista = new List<Container>();
                     Link conn = new Link();
                     try{
                         SqlCommand comando = new SqlCommand();
                         SqlDataReader leerFilas = null;
                         comando.Connection = conn.AbrirConexion();
                         comando.CommandText = "select id_container,eco,plates from containers where id_container='"+id+"' ";
                         leerFilas = comando.ExecuteReader();
                         while (leerFilas.Read()){
                             Container e = new Container();
                             e.id_container = Convert.ToInt32(leerFilas["id_container"]);
                             e.eco = Convert.ToString(leerFilas["eco"]);
                             e.plates = Convert.ToString(leerFilas["plates"]);
                             lista.Add(e);
                         }leerFilas.Close();
                     }finally { conn.CerrarConexion(); conn.Dispose(); }
                     return lista;
                 }
                 public void borrar_contenedor(string id){
                     Link con_s = new Link();
                     try{
                         SqlCommand com_s = new SqlCommand();
                         com_s.Connection = con_s.AbrirConexion();
                         com_s.CommandText = "DELETE FROM containers where id_container='" + id + "' ";
                         com_s.ExecuteNonQuery();
                     }finally { con_s.CerrarConexion(); con_s.Dispose(); }
                 }
                
                 public void guardar_nuevo_contenedor(string eco, string plates){
                     Link con_s = new Link();
                     try{
                         SqlCommand com_s = new SqlCommand();
                         com_s.Connection = con_s.AbrirConexion();
                         com_s.CommandText = "INSERT INTO containers(eco,plates)values" +
                             "('" + eco + "','" + plates + "') ";
                         com_s.ExecuteNonQuery();
                     }finally { con_s.CerrarConexion(); con_s.Dispose(); }
                 }
                 public void guardar_contenedor_edicion(string id, string eco, string plates){
                     Link con_s = new Link();
                     try{
                         SqlCommand com_s = new SqlCommand();
                         com_s.Connection = con_s.AbrirConexion();
                         com_s.CommandText = "UPDATE containers SET eco='" + eco + "', plates='" + plates + "' " +
                             "  where id_container='" + id + "' ";
                         com_s.ExecuteNonQuery();
                     }finally { con_s.CerrarConexion(); con_s.Dispose(); }
                 }

                 public Pk obtener_informacion_editar_packing_completo_b(int pk){
                     Pk p = new Pk();
                     Link connx = new Link();
                     try{
                         SqlCommand comandox = new SqlCommand();
                         SqlDataReader leerFilasx = null;
                         comandox.Connection = connx.AbrirConexion();
                         comandox.CommandText = "select id_packing_list,id_direccion_envio,id_pedido,id_driver,id_container,seal,replacement,tipo,envio " +
                             "from packing_list where id_packing_list='" + pk + "' ";
                         leerFilasx = comandox.ExecuteReader();
                         while (leerFilasx.Read()){
                             p.id_packing_list = Convert.ToInt32(leerFilasx["id_packing_list"]);
                             p.id_direccion_envio = Convert.ToInt32(leerFilasx["id_direccion_envio"]);
                             p.id_pedido= Convert.ToInt32(leerFilasx["id_pedido"]);                   
                             p.id_driver = Convert.ToInt32(leerFilasx["id_driver"]);
                             p.id_container = Convert.ToInt32(leerFilasx["id_container"]);
                             p.seal = Convert.ToString(leerFilasx["seal"]);
                             p.replacement = Convert.ToString(leerFilasx["replacement"]);
                             p.tipo = Convert.ToString(leerFilasx["id_driver"]);
                             p.num_envio = Convert.ToInt32(leerFilasx["envio"]);
                             p.lista_labels = obtener_lista_labels(pk);
                         }
                         leerFilasx.Close();
                     }finally { connx.CerrarConexion(); connx.Dispose(); }
                     return p;
                 }
                 public List<Pk> obtener_informacion_editar_packing_completo(int pk){
                     List<Pk> lista = new List<Pk>();

                     Link connx = new Link();
                     try{
                         SqlCommand comandox = new SqlCommand();
                         SqlDataReader leerFilasx = null;
                         comandox.Connection = connx.AbrirConexion();
                         comandox.CommandText = "select id_packing_list,id_direccion_envio,id_pedido,id_driver,id_container,seal,replacement,tipo,envio,id_packing_type," +
                             "id_customer,id_customer_po,parte  from packing_list where id_packing_list='" + pk + "' ";
                         leerFilasx = comandox.ExecuteReader();
                         while (leerFilasx.Read()){
                            Pk p = new Pk();
                            p.id_tipo = Convert.ToInt32(leerFilasx["id_packing_type"]);
                            p.id_pedido = Convert.ToInt32(leerFilasx["id_pedido"]);
                            p.id_packing_list = Convert.ToInt32(leerFilasx["id_packing_list"]);
                            p.id_tipo = Convert.ToInt32(leerFilasx["id_packing_type"]);
                            p.parte = Convert.ToString(leerFilasx["parte"]);
                            if (p.id_tipo != 8 && p.id_tipo != 4){
                                p.pedido = consultas.obtener_po_id((p.id_pedido).ToString());
                            }else{
                                p.pedido = consultas.obtener_po_id_fantasy((p.id_pedido).ToString());
                            }
                             p.id_direccion_envio = Convert.ToInt32(leerFilasx["id_direccion_envio"]);
                             p.id_driver = Convert.ToInt32(leerFilasx["id_driver"]);
                             p.id_container = Convert.ToInt32(leerFilasx["id_container"]);
                             p.seal = Convert.ToString(leerFilasx["seal"]);
                             p.replacement = Convert.ToString(leerFilasx["replacement"]);
                             p.tipo = Convert.ToString(leerFilasx["tipo"]);
                             p.num_envio = Convert.ToInt32(leerFilasx["envio"]);
                             p.id_packing_type = Convert.ToInt32(leerFilasx["id_packing_type"]);
                             p.id_customer = Convert.ToInt32(leerFilasx["id_customer"]);
                             p.id_customer_po = Convert.ToInt32(leerFilasx["id_customer_po"]);
                             p.customer_po = consultas.obtener_customer_final_id(Convert.ToString(p.id_customer_po));
                             p.lista_labels = obtener_lista_labels(pk);
                             lista.Add(p);
                         }leerFilasx.Close();
                     }finally { connx.CerrarConexion(); connx.Dispose(); }
                     return lista;
                 }

                 public List<Labels> obtener_lista_labels(int pk){
                     List<Labels> lista = new List<Labels>();
                     Link conn = new Link();
                     try{
                         SqlCommand comando = new SqlCommand();
                         SqlDataReader leer = null;
                         comando.Connection = conn.AbrirConexion();
                         comando.CommandText = "select id_label,label,tipo from ucc_labels where id_packing='" + pk + "' ";
                         leer= comando.ExecuteReader();
                         while (leer.Read()){
                             Labels l = new Labels();
                             l.id_label = Convert.ToInt32(leer["id_label"]);
                             l.label = Convert.ToString(leer["label"]);
                             l.tipo = Convert.ToString(leer["tipo"]);
                             lista.Add(l);
                         }leer.Close();
                     }finally { conn.CerrarConexion(); conn.Dispose(); }
                     return lista;
                 }
                 public int obtener_pedido_packing(int packing){
                     int lista = 0;
                     Link con = new Link();
                     try{//Regex.Replace(, @"\s+", " ");
                         SqlCommand com = new SqlCommand();
                         SqlDataReader leer = null;
                         com.Connection = con.AbrirConexion();
                         com.CommandText = "SELECT id_pedido from packing_list where id_packing_list='" + packing + "' ";
                         leer = com.ExecuteReader();
                         while (leer.Read()){
                             lista = Convert.ToInt32(leer["id_pedido"]);
                         }leer.Close();
                     }finally { con.CerrarConexion(); con.Dispose(); }
                     return lista;
                 }
                public void eliminar_pedido_fantasy(int packing){
                    int pedido = obtener_pedido_packing(packing);
                    Link con_s = new Link();
                    try{
                        SqlCommand com_s = new SqlCommand();
                        com_s.Connection = con_s.AbrirConexion();
                        com_s.CommandText = "DELETE FROM pedidos_fantasy WHERE id_pedido='" + pedido + "'  ";
                        com_s.ExecuteNonQuery();
                    }finally { con_s.CerrarConexion(); con_s.Dispose(); }
                }
        public void cambiar_pk_packing_list(int packing,string pk){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE packing_list SET pk='" + pk + "' WHERE id_packing_list='" + packing + "'  ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }


        public void eliminar_estilos_packing_list(int packing){
            List<int> ids = lista_shipping_ids_packing(packing);
            foreach (int i in ids) {
                eliminar_totales_envios(i);
                //eliminar_totales_ejemplos(i);
                eliminar_estilo_shipping_id(i,3);
            }
         }
                 public List<int> lista_shipping_ids_packing(int packing) {
                     List<int> lista = new List<int>();
                     Link con = new Link();
                     try{//Regex.Replace(, @"\s+", " ");
                         SqlCommand com = new SqlCommand();
                         SqlDataReader leer = null;
                         com.Connection = con.AbrirConexion();
                         com.CommandText = "SELECT id_shipping_id from shipping_ids where used='" + packing + "' ";
                         leer = com.ExecuteReader();
                         while (leer.Read()){
                             int i = 0;
                             i = Convert.ToInt32(leer["id_shipping_id"]);
                             lista.Add(i);
                         }leer.Close();
                     }finally { con.CerrarConexion(); con.Dispose(); }
                     return lista;
                 }

                 public void eliminar_totales_envios(int id_shipping){
                     Link con_s = new Link();
                     try{
                         SqlCommand com_s = new SqlCommand();
                         com_s.Connection = con_s.AbrirConexion();
                         com_s.CommandText = "delete from totales_envios  where id_shipping_id='" + id_shipping + "' ";
                         com_s.ExecuteNonQuery();
                     }finally { con_s.CerrarConexion(); con_s.Dispose(); }
                 }
                 public void eliminar_totales_ejemplos(int id_shipping){
                     Link con_s = new Link();
                     try{
                         SqlCommand com_s = new SqlCommand();
                         com_s.Connection = con_s.AbrirConexion();
                         com_s.CommandText = "delete from shipping_ratio where id_shipping_id='" + id_shipping + "' ";
                         com_s.ExecuteNonQuery();
                     }finally { con_s.CerrarConexion(); con_s.Dispose(); }
                 }
                 public void eliminar_estilo_shipping_id(int id_shipping,int tipo){
                     Link con_s = new Link();
                     try{
                         SqlCommand com_s = new SqlCommand();
                         com_s.Connection = con_s.AbrirConexion();
                         com_s.CommandText = "delete from shipping_ids where id_shipping_id='" + id_shipping + "'  ";
                         com_s.ExecuteNonQuery();
                     }finally { con_s.CerrarConexion(); con_s.Dispose(); }
                 }
                 public void eliminar_labels(int packing)
                 {
                     Link con_s = new Link();
                     try
                     {
                         SqlCommand com_s = new SqlCommand();
                         com_s.Connection = con_s.AbrirConexion();
                         com_s.CommandText = "delete from ucc_labels where id_packing='" + packing + "' ";
                         com_s.ExecuteNonQuery();
                     }
                     finally { con_s.CerrarConexion(); con_s.Dispose(); }
                 }
                 public List<estilo_shipping> lista_estilos_extras(string id_pedido, string busqueda)
                 {
                     string cadena = "";
                     if (busqueda == "0")
                     {
                         cadena = "select top 10 ID_PO_SUMMARY,ID_PEDIDOS,ID_COLOR,ITEM_ID from PO_SUMMARY where ID_PEDIDOS='" + id_pedido + "' ";
                     }else{
                         cadena = "select top 10 PS.ID_PO_SUMMARY,PS.ID_PEDIDOS,PS.ID_COLOR,PS.ITEM_ID FROM PO_SUMMARY PS,ITEM_DESCRIPTION ITD,PEDIDO P WHERE" +
                             " ITD.ITEM_STYLE LIKE '%" + busqueda + "%' AND ITD.ITEM_ID=PS.ITEM_ID AND P.ID_STATUS!=7 AND P.ID_STATUS!=6" +
                             " AND P.ID_PEDIDO=PS.ID_PEDIDOS  ";
                     }
                     List<estilo_shipping> listar = new List<estilo_shipping>();
                     Link conn = new Link();
                     try{
                         SqlCommand comando = new SqlCommand();
                         SqlDataReader leerFilas = null;
                         comando.Connection = conn.AbrirConexion();
                         comando.CommandText = cadena;
                         leerFilas = comando.ExecuteReader();
                         while (leerFilas.Read()){
                             estilo_shipping l = new estilo_shipping();
                             /*l.id_estilo = Convert.ToInt32(leerFilas["ITEM_ID"]);
                             l.id_summary = consultas.obtener_po_summary(Convert.ToInt32(id_pedido), l.id_estilo);
                             l.id_color = consultas.obtener_color_id_item(Convert.ToInt32(id_pedido), l.id_estilo);*/
                                    l.id_summary = Convert.ToInt32(leerFilas["ID_PO_SUMMARY"]);
                    l.id_estilo = Convert.ToInt32(leerFilas["ITEM_ID"]);
                    l.id_color = Convert.ToInt32(leerFilas["ID_COLOR"]);
                    l.id_pedido = Convert.ToInt32(leerFilas["ID_PEDIDOS"]);
                    l.po = consultas.obtener_po_id(Convert.ToString(l.id_pedido));
                    l.color = consultas.obtener_color_id(Convert.ToString(l.id_color));
                    l.estilo = consultas.obtener_estilo(l.id_estilo);
                    l.descripcion = consultas.buscar_descripcion_estilo(l.id_estilo);
                    List<Empaque> lista_e = new List<Empaque>();
                    List<string> tipo_empaque_temporal = consultas.buscar_tipo_empaque(l.id_summary);
                    foreach (string s in tipo_empaque_temporal){
                        Empaque e = new Empaque();
                        e.tipo_empaque = Convert.ToInt32(s);
                        if (s == "1") { e.lista_ratio = obtener_lista_tallas_estilo(l.id_summary, l.id_estilo); }
                        if (s == "2") { e.lista_ratio = obtener_lista_ratio(l.id_summary, l.id_estilo, 2); }
                        lista_e.Add(e);
                    }
                    l.lista_empaque = lista_e;
                    listar.Add(l);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return listar;
        }
        


        public void guardar_ratio_otros(int id, string cantidad, string talla){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "INSERT INTO shipping_ratio(id_talla,total,id_shipping_id) VALUES " +
                                     " ('" + talla + "','" + cantidad + "','" + id + "')";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }

        public void revisar_totales_estilo(int pedido){            
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT ID_PO_SUMMARY,QTY FROM PO_SUMMARY where ID_PEDIDOS='" + pedido + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    int total_orden = Convert.ToInt32(leer_u_r["QTY"]);
                    int total_enviado = buscar_total_enviado_estilo(Convert.ToInt32(leer_u_r["ID_PO_SUMMARY"]));
                    if (total_enviado >= total_orden) {
                        cerrar_estilo(Convert.ToInt32(leer_u_r["ID_PO_SUMMARY"]));
                    }
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }            
        }
        public int buscar_total_enviado_estilo(int summary){
            int total = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT total FROM totales_envios WHERE id_summary='" + summary + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    total += Convert.ToInt32(leer_u_r["total"]);
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return total;
        }
        public int buscar_tipo_empaque_shipid(int id){
            int total = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT tipo_empaque FROM shipping_ids WHERE id_shipping_id='" + id + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    total += Convert.ToInt32(leer_u_r["tipo_empaque"]);
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return total;
        }
        public void cerrar_estilos_pedido(int pedido){
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT ID_PO_SUMMARY,QTY FROM PO_SUMMARY where ID_PEDIDOS='" + pedido + "' ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                   cerrar_estilo(Convert.ToInt32(leer_u_r["ID_PO_SUMMARY"]));
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
        }
        public void cerrar_estilo(int summary){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = " UPDATE PO_SUMMARY SET ID_ESTADO=7 WHERE ID_PO_SUMMARY='"+summary+"' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }


        public List<Pk> obtener_lista_shipping_summary(int summary,string tipo,List<Talla> lista_tallas){
            List<Pk> lista = new List<Pk>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT distinct p.id_packing_list,p.pk,p.fecha,p.id_direccion_envio  " +
                    " FROM  packing_list p,shipping_ids s,totales_envios t " +
                    " WHERE t.id_shipping_id=s.id_shipping_id  AND s.used=p.id_packing_list " +
                    " AND t.id_summary='" + summary + "'  ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Pk p = new Pk();
                    p.id_packing_list = Convert.ToInt32(leer["id_packing_list"]);
                    p.packing = Convert.ToString(leer["pk"]);
                    p.fecha = (Convert.ToDateTime(leer["fecha"])).ToString("MMM dd yyyy");
                    p.destino = obtener_direccion(Convert.ToInt32(leer["id_direccion_envio"]));
                    p.lista_tallas = obtener_lista_items_customer_staging(p.id_packing_list, summary,tipo,lista_tallas);
                    lista.Add(p);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }

        public List<Pk> obtener_lista_shipping_summary_samples(int summary, string tipo, List<Talla> lista_tallas){
            List<Pk> lista = new List<Pk>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT distinct p.id_packing_list,p.pk,p.fecha,p.id_direccion_envio FROM " +
                    " packing_list p,packing_samples s WHERE s.id_summary='" + summary + "' AND s.id_packing_list=p.id_packing_list";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Pk p = new Pk();
                    p.id_packing_list = Convert.ToInt32(leer["id_packing_list"]);
                    p.packing = Convert.ToString(leer["pk"]);
                    p.fecha = (Convert.ToDateTime(leer["fecha"])).ToString("MMM dd yyyy");
                    p.destino = obtener_direccion(Convert.ToInt32(leer["id_direccion_envio"]));
                    p.lista_tallas = obtener_lista_items_customer_staging(p.id_packing_list, summary, tipo, lista_tallas);
                    lista.Add(p);
                }
                leer.Close();
            }
            finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }

        public List<Talla> obtener_lista_items_customer_staging(int id_packing_list, int summary,string tipo,List<Talla> lista_tallas){
            string query = "";
            if (tipo != "DMG" && tipo != "EXT" && tipo != "SAM") {
                query= "SELECT t.id_talla,t.total,t.tipo FROM totales_envios t,shipping_ids s WHERE  " +
                    "  s.used='" + id_packing_list + "' AND s.id_shipping_id=t.id_shipping_id AND " +
                    " t.id_summary='" + summary + "' AND t.tipo!='DMG' AND t.tipo!='EXT'  AND t.tipo!='SAM'";
            } else {
                if (tipo == "SAM"){
                    query = "SELECT t.id_talla,t.total,t.tipo FROM totales_envios t,packing_samples s WHERE  " +
                        "  s.id_packing_list='" + id_packing_list + "' AND s.id_sample=t.id_shipping_id AND " +
                        " t.id_summary='" + summary + "'  ";                   
                }else{
                    query = "SELECT t.id_talla,t.total,t.tipo FROM totales_envios t,shipping_ids s WHERE " +
                        " s.used='" + id_packing_list + "' AND s.id_shipping_id=t.id_shipping_id AND  " +
                        " t.id_summary='" + summary + "' " +
                        " AND t.tipo!='ECOM' AND t.tipo!='INITIAL' AND t.tipo!='SAM' AND t.tipo!='NONE' AND t.tipo!='RPLN'  ";
                    //AND t.tipo='" + tipo + "'
                }
            }
            List<Talla> lista_temporal = new List<Talla>();
            List<Talla> lista= new List<Talla>();
            Link con = new Link();            
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = query;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Talla ts = new Talla();
                    ts.total = Convert.ToInt32(leer["total"]);
                    ts.id_talla = Convert.ToInt32(leer["id_talla"]);
                    ts.tipo = Convert.ToString(leer["tipo"]);
                    lista_temporal.Add(ts);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            foreach (Talla t in lista_tallas) {
                int total=0;
                foreach (Talla lt in lista_temporal){
                    if (t.id_talla == lt.id_talla) {
                        total += lt.total;
                    }
                }
                Talla ts = new Talla();
                ts.total = total;
                ts.id_talla =t.id_talla;
                ts.tipo =tipo;
                lista.Add(ts);
            }
            return lista;
        }

        public List<Talla> obtener_tallas_fantasy() {
            string[] tallas_arr = {"SM","MD","LG","XL","2XL" };
            List<Talla> lista = new List<Talla>();
            for (int i=0; i<5; i++) {
                Talla t = new Talla();
                t.id_talla = consultas.buscar_talla(tallas_arr[i]);
                t.talla = tallas_arr[i];
                lista.Add(t);
            }                
            return lista;
        }

        public void guardar_estilo_ejemplo(int packing,string summary,string xs,string sm,string md,string lg,string xl,string xxl,string xxxl,string cajas,string attnto,string customer,string indice,string inicial,string tipo){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO packing_samples(id_packing_list,id_summary,talla_xs,talla_s,talla_m,talla_l,talla_xl,talla_2x,talla_3x,cajas,attnto,id_customer,id_tarima,inicial,tipo) VALUES " +
                    " ('" + packing + "','" + summary + "','" + xs + "','" + sm + "','" + md + "','" + lg + "','" + xl + "','" + xxl + "','" + xxxl + "','" + cajas + "'," +
                    "'" + attnto + "','" + customer + "','"+ indice + "','"+ inicial + "','"+tipo+"')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }
        public void guardar_nuevo_estilo_ejemplo(string color,string percent,string genero,string orden,string estilo,string descripcion,string origen){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO ejemplos_nuevos_fantasy(orden,estilo,descripcion,color,porcentaje,genero,origen) VALUES " +
                    " ('" + orden + "','" + estilo + "','" + descripcion + "','" + color + "','" + percent + "','" + genero + "','"+origen+"')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }

        public void guardar_cabeceras_ejemplos(int packing,string cabeceras,string indice){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO tallas_cabecera_samples(id_packing_sample,cabeceras,indice) " +
                    " VALUES ('" + packing + "','" + cabeceras + "','"+indice+"')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }

       
        public int obtener_ultimo_nuevo_ejemplo_registrado(){
            int lista = 0;
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select top 1 id_nuevo from ejemplos_nuevos_fantasy order by id_nuevo desc ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista = Convert.ToInt32(leerFilas["id_nuevo"]);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public void guardar_estilo_fantasy_extras(int packing, string summary, string sm, string md, string lg, string xl, string xxl, string cajas, string tipo)
        {
            Link con_c = new Link();
            try
            {
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO packing_fantasy(id_packing,id_summary,talla_s,talla_m,talla_l,talla_xl,talla_2x,cajas,tipo) VALUES " +
                    " ('" + packing + "','" + summary + "','" + sm + "','" + md + "','" + lg + "','" + xl + "','" + xxl + "','" + cajas + "','" + tipo + "')";
                com_c.ExecuteNonQuery();
            }
            finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }

        public void editar_estilo_fantasy_extras(string id, string summary, string sm, string md, string lg, string xl, string xxl, string cajas, string tipo){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "UPDATE packing_fantasy SET talla_s='" + sm + "',talla_m='" + md + "',talla_l='" + lg + "',talla_xl='" + xl + "', " +
                    " talla_2x='" + xxl + "',cajas='" + cajas + "',tipo='" + tipo + "' WHERE id_fantasy='" + id + "' ";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }

        public string buscar_parte_packing(int pedido){
            int lista = 1;
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_packing_list FROM packing_list WHERE id_pedido="+pedido+ " " +
                    " AND id_packing_type!=4 AND id_packing_type!=5 AND id_packing_type!=6 AND id_packing_type!=8 AND id_packing_type!=9 AND id_packing_type!=3";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista++;
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista.ToString();
        }
        public string buscar_parte_packing_walmart(int pedido){
            int lista = 1;
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_packing_list FROM packing_list WHERE id_pedido=" + pedido + " " +
                    " AND id_packing_type!=4 AND id_packing_type!=5 AND id_packing_type!=6 AND id_packing_type!=8 AND id_packing_type!=9  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista++;
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista.ToString();
        }
        public int obtener_ultimo_examples_registrado(){
            int lista = 0;
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select top 1 id_sample from packing_samples order by id_sample desc ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista = Convert.ToInt32(leerFilas["id_sample"]);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public int obtener_ultimo_extra_fantasy_registrado()
        {
            int lista = 0;
            Link conn = new Link();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select top 1 id_fantasy from packing_fantasy order by id_fantasy desc ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read())
                {
                    lista = Convert.ToInt32(leerFilas["id_fantasy"]);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<Inventario> obtener_inventario_orden(int pedido) {
            List<Inventario> lista = new List<Inventario>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT distinct id_item,id_inventario,total,id_categoria_inventario,descripcion,id_item,id_size FROM inventario" +
                    " WHERE id_pedido='"+pedido+"' AND total>0 ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    Inventario i = new Inventario();
                    i.id_inventario = Convert.ToInt32(leerFilas["id_inventario"]);
                    i.id_categoria_inventario = Convert.ToInt32(leerFilas["id_categoria_inventario"]);
                    i.total = Convert.ToInt32(leerFilas["total"]);
                    //i.total = 0;
                    i.id_item = Convert.ToInt32(leerFilas["id_item"]);
                    i.descripcion = Convert.ToString(leerFilas["descripcion"]);
                    i.id_size = Convert.ToInt32(leerFilas["id_size"]);
                    i.size = consultas.obtener_size_id((i.id_size).ToString());
                    lista.Add(i);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public int buscar_summary_inventario(int inventario){
            int temp = 0;
            Link con_u_i = new Link();
            try{
                SqlCommand com_u_i = new SqlCommand();
                SqlDataReader leer_u_i = null;
                com_u_i.Connection = con_u_i.AbrirConexion();
                com_u_i.CommandText = "SELECT id_summary FROM inventario WHERE id_inventario='" + inventario + "' ";
                leer_u_i = com_u_i.ExecuteReader();
                while (leer_u_i.Read()){
                    temp += Convert.ToInt32(leer_u_i["id_summary"]);
                }leer_u_i.Close();
            }finally { con_u_i.CerrarConexion(); con_u_i.Dispose(); }
            return temp;
        }

        public void agregar_retorno_envio(string inventario,string cantidad,string categoria,string talla,string item,int summary,int packing,string cajas,string indice ){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "INSERT INTO packing_returns(id_packing,id_item,id_inventario,id_talla,id_categoria,id_summary,total,cajas,indice) VALUES " +
                    " ('" + packing + "','" + item + "','" + inventario + "','" + talla + "','" + categoria + "','" + summary + "','" + cantidad + "','"+cajas+"','"+indice+"')";
                com_c.ExecuteNonQuery();
            }finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }

        public void editar_retorno_envio(string id,string cantidad, string cajas){
            Link con_c = new Link();
            try{
                SqlCommand com_c = new SqlCommand();
                com_c.Connection = con_c.AbrirConexion();
                com_c.CommandText = "UPDATE packing_returns SET total='" + cantidad + "',cajas='" + cajas + "'  WHERE id_return='" + id + "' ";
                com_c.ExecuteNonQuery();}
            finally { con_c.CerrarConexion(); con_c.Dispose(); }
        }

        public List<Sample> lista_estilos_samples_pk(int packing){
            List<Sample> lista = new List<Sample>();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_sample,id_packing_list,id_summary,talla_xs,talla_s,talla_m,talla_l," +
                    " talla_xl,talla_2x,talla_3x,cajas,attnto,id_customer,id_tarima,inicial,tipo FROM packing_samples WHERE id_packing_list='" + packing + "'  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    Sample s = new Sample();
                    s.tipo_sample = Convert.ToInt32(leerFilas["tipo"]);
                    if (s.tipo_sample == 0){
                        s.id_summary = Convert.ToInt32(leerFilas["id_summary"]);
                        int id_estilo = consultas.obtener_estilo_summary(s.id_summary);
                        s.id_color = consultas.obtener_color_id_item_cat(s.id_summary);
                        s.color = Regex.Replace(consultas.obtener_color_id((s.id_color).ToString()), @"\s+", " ");
                        s.estilo = Regex.Replace(consultas.obtener_estilo(id_estilo), @"\s+", "") + buscar_terminacion_estilo(s.id_summary, 0);
                        s.id_pedido = consultas.obtener_id_pedido_summary(s.id_summary);
                        s.pedido = consultas.obtener_po_id((s.id_pedido).ToString());
                        //s.number_po = buscar_number_po(s.id_pedido);
                        s.number_po = buscar_number_po_pedido(s.id_pedido);
                        s.descripcion = Regex.Replace(consultas.buscar_descripcion_estilo(id_estilo), @"\s+", " ");
                        buscar_origen_summary(s.id_summary);
                        s.origen = origen;
                        s.porcentaje = porcentaje;
                        s.id_genero = consultas.buscar_genero_summary(s.id_summary);
                        //s.genero = consultas.obtener_genero_id(Convert.ToString(s.id_genero));
                        s.genero = (buscar_descripcion_final_estilo(s.id_summary)).Trim();
                    }else{
                        s.nuevo_estilo = buscar_estilo_nuevo_ejemplo(Convert.ToInt32(leerFilas["id_summary"]));
                        s.id_summary = Convert.ToInt32(leerFilas["id_summary"]);
                        s.id_color = s.nuevo_estilo.id_color;
                        s.color = Regex.Replace(s.nuevo_estilo.color, @"\s+", " ");
                        s.estilo = Regex.Replace(s.nuevo_estilo.estilo, @"\s+", "");
                        s.id_pedido = 0;
                        s.pedido = s.nuevo_estilo.orden;
                        //s.number_po = buscar_number_po(s.id_pedido);
                        s.number_po = "N/A";
                        s.descripcion = Regex.Replace(s.nuevo_estilo.descripcion, @"\s+", " ");
                        s.origen = s.nuevo_estilo.origen;
                        s.porcentaje = s.nuevo_estilo.percent;
                        //s.id_genero = s.nuevo_estilo.id_genero;
                        s.genero = (s.nuevo_estilo.genero).Trim();
                    }
                    s.id_sample = Convert.ToInt32(leerFilas["id_sample"]);
                    s.talla_xs = Convert.ToInt32(leerFilas["talla_xs"]);
                    s.talla_s = Convert.ToInt32(leerFilas["talla_s"]);
                    s.talla_m = Convert.ToInt32(leerFilas["talla_m"]);
                    s.talla_l = Convert.ToInt32(leerFilas["talla_l"]);
                    s.talla_xl = Convert.ToInt32(leerFilas["talla_xl"]);
                    s.talla_2x = Convert.ToInt32(leerFilas["talla_2x"]);
                    s.talla_3x = Convert.ToInt32(leerFilas["talla_3x"]);
                    s.tallaxs = buscar_talla_sample(Convert.ToInt32(leerFilas["id_sample"]), s.talla_xs);
                    s.tallas = buscar_talla_sample(Convert.ToInt32(leerFilas["id_sample"]), s.talla_s);
                    s.tallam = buscar_talla_sample(Convert.ToInt32(leerFilas["id_sample"]), s.talla_m);
                    s.tallal = buscar_talla_sample(Convert.ToInt32(leerFilas["id_sample"]), s.talla_l);
                    s.tallaxl = buscar_talla_sample(Convert.ToInt32(leerFilas["id_sample"]), s.talla_xl);
                    s.talla2x = buscar_talla_sample(Convert.ToInt32(leerFilas["id_sample"]), s.talla_2x);
                    s.talla3x = buscar_talla_sample(Convert.ToInt32(leerFilas["id_sample"]), s.talla_3x);
                    s.cajas = Convert.ToInt32(leerFilas["cajas"]);
                    s.total = s.talla_s + s.talla_xs + s.talla_m + s.talla_l + s.talla_xl + s.talla_2x + s.talla_3x;
                    s.id_customer = Convert.ToInt32(leerFilas["id_customer"]);
                    s.customer = consultas.obtener_customer_final_id(Convert.ToString(s.id_customer));
                    s.attnto = Convert.ToString(leerFilas["attnto"]);
                    s.inicial = Convert.ToInt32(leerFilas["inicial"]);
                    s.id_tarima = Convert.ToInt32(leerFilas["id_tarima"]);

                    s.cols_extras = obtener_columnas_extras(packing);
                    s.total_extras = (s.cols_extras).Count();
                    s.cabeceras = buscar_cabeceras_sample(packing);
                    if (s.total_extras!=0) {
                        s.lista_extras = obtener_extras_sample(s.id_sample);
                    }
                    s.lista_estilos = lista_estilos_samples((s.id_pedido).ToString(),Convert.ToInt32(leerFilas["id_packing_list"]));
                    lista.Add(s);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public string buscar_cabeceras_sample(int sample){
            string lista = "";
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT cabeceras,indice FROM tallas_cabecera_samples WHERE id_packing_sample='" + sample + "'  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista = Convert.ToString(leerFilas["cabeceras"])+"*"+ Convert.ToString(leerFilas["indice"]);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<int> buscar_total_extras(int sample){
            List<int> lista = new List<int>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT columna FROM extras_samples WHERE id_packing_sample='" + sample + "'  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista.Add(Convert.ToInt32(leerFilas["columna"]));
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<Extra_sample> obtener_extras_sample(int sample){
            List<Extra_sample> lista = new List<Extra_sample>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_extra,columna,total,id_talla FROM extras_samples WHERE id_packing_sample='" + sample + "'  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    Extra_sample x = new Extra_sample();
                    x.id_extra = Convert.ToInt32(leerFilas["id_extra"]);
                    x.columna = Convert.ToInt32(leerFilas["columna"]);
                    x.total = Convert.ToInt32(leerFilas["total"]);
                    x.id_talla = Convert.ToInt32(leerFilas["id_talla"]);
                    x.talla = consultas.obtener_size_id(Convert.ToString(leerFilas["id_talla"]));
                    lista.Add(x);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<int> obtener_columnas_extras(int packing){
            List<int> lista = new List<int>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_sample FROM packing_samples WHERE id_packing_list='" + packing + "'  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista.AddRange(buscar_total_extras(Convert.ToInt32(leerFilas["id_sample"])));                 
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            lista.Distinct().ToList();
            lista.Sort();
            return lista;
        }









        public List<estilo_shipping> lista_estilos_samples(string id_pedido,int packing){
            List<estilo_shipping> listar = new List<estilo_shipping>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct ITEM_ID from PO_SUMMARY where ID_PEDIDOS='" + id_pedido + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    estilo_shipping l = new estilo_shipping();
                    l.id_estilo = Convert.ToInt32(leerFilas["ITEM_ID"]);
                    l.id_summary = consultas.obtener_po_summary(Convert.ToInt32(id_pedido), l.id_estilo);
                    l.estilo = consultas.obtener_estilo(l.id_estilo);
                    l.descripcion = consultas.buscar_descripcion_estilo(l.id_estilo);
                    listar.Add(l);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }

            Link conn2 = new Link();
            try{
                SqlCommand comando2 = new SqlCommand();
                SqlDataReader leerFilas2 = null;
                comando2.Connection = conn2.AbrirConexion();
                comando2.CommandText = "select id_summary from packing_samples where id_packing_list='" + packing + "' and tipo=1";
                leerFilas2 = comando2.ExecuteReader();
                while (leerFilas2.Read()){
                    Nuevo_estilo_ejemplo e = buscar_estilo_nuevo_ejemplo(Convert.ToInt32(leerFilas2["id_summary"]));
                    estilo_shipping l = new estilo_shipping();
                    l.id_estilo = 0;
                    l.id_summary = e.id_nuevo;
                    l.estilo = "NEWSTYLE-"+ e.id_nuevo;
                    l.descripcion = "NEWSTYLE-" + e.id_nuevo;
                    listar.Add(l);
                }leerFilas2.Close();
            }finally { conn2.CerrarConexion(); conn2.Dispose(); }
            return listar;
        }




        public List<Sample> lista_indices_samples_pk(int packing){
            List<Sample> lista = new List<Sample>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT distinct id_tarima FROM packing_samples WHERE id_packing_list='" + packing + "'  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    Sample s = new Sample();                    
                    s.id_tarima = Convert.ToInt32(leerFilas["id_tarima"]);
                    lista.Add(s);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<Return> lista_indices_returns_pk(int packing){
            List<Return> lista = new List<Return>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT distinct indice FROM packing_returns WHERE id_packing='" + packing + "'  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    Return s = new Return();
                    s.indice = Convert.ToInt32(leerFilas["indice"]);
                    lista.Add(s);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public string buscar_talla_sample(int id,int total){
            string lista = "";
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_talla FROM totales_envios WHERE id_shipping_id='" + id + "' and tipo='SAM' AND tipo_packing=3 and total='"+total+"' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista = consultas.obtener_size_id(Convert.ToString(leerFilas["id_talla"]));
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<Sample> lista_estilos_extras_fantasy_pk(int packing)
        {
            List<Sample> lista = new List<Sample>();
            Link conn = new Link();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_fantasy,id_packing,id_summary,talla_s,talla_m,talla_l," +
                    " talla_xl,talla_2x,cajas,id_tarima FROM packing_fantasy WHERE id_packing='" + packing + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read())
                {
                    Sample s = new Sample();
                    s.id_fantasy = Convert.ToInt32(leerFilas["id_fantasy"]);
                    s.id_packing = Convert.ToInt32(leerFilas["id_packing"]);
                    s.id_summary = Convert.ToInt32(leerFilas["id_summary"]);
                    int id_estilo = consultas.obtener_estilo_summary(s.id_summary);
                    s.estilo = consultas.obtener_estilo((id_estilo)) + " " + consultas.buscar_descripcion_estilo(id_estilo);                    
                    s.talla_s = Convert.ToInt32(leerFilas["talla_s"]);
                    s.talla_m = Convert.ToInt32(leerFilas["talla_m"]);
                    s.talla_l = Convert.ToInt32(leerFilas["talla_l"]);
                    s.talla_xl = Convert.ToInt32(leerFilas["talla_xl"]);
                    s.talla_2x = Convert.ToInt32(leerFilas["talla_2x"]);
                    s.cajas = Convert.ToInt32(leerFilas["cajas"]);                    
                    s.total = s.talla_s + s.talla_m + s.talla_l + s.talla_xl + s.talla_2x ;
                    s.id_tarima = Convert.ToInt32(leerFilas["id_tarima"]);
                    lista.Add(s);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<Sample> lista_estilos_extras_fantasy_pk_sin_tarima(int packing)
        {
            List<Sample> lista = new List<Sample>();
            Link conn = new Link();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_fantasy,id_packing,id_summary,talla_s,talla_m,talla_l," +
                    " talla_xl,talla_2x,cajas,id_tarima FROM packing_fantasy WHERE id_packing='" + packing + "' and id_tarima=0 ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read())
                {
                    Sample s = new Sample();
                    s.id_fantasy = Convert.ToInt32(leerFilas["id_fantasy"]);
                    s.id_packing = Convert.ToInt32(leerFilas["id_packing"]);
                    s.id_summary = Convert.ToInt32(leerFilas["id_summary"]);
                    int id_estilo = consultas.obtener_estilo_summary(s.id_summary);
                    s.estilo = consultas.obtener_estilo((id_estilo)) + " " + consultas.buscar_descripcion_estilo(id_estilo);
                    s.talla_s = Convert.ToInt32(leerFilas["talla_s"]);
                    s.talla_m = Convert.ToInt32(leerFilas["talla_m"]);
                    s.talla_l = Convert.ToInt32(leerFilas["talla_l"]);
                    s.talla_xl = Convert.ToInt32(leerFilas["talla_xl"]);
                    s.talla_2x = Convert.ToInt32(leerFilas["talla_2x"]);
                    s.cajas = Convert.ToInt32(leerFilas["cajas"]);
                    s.total = s.talla_s + s.talla_m + s.talla_l + s.talla_xl + s.talla_2x;
                    s.id_tarima = Convert.ToInt32(leerFilas["id_tarima"]);
                    lista.Add(s);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public List<Return> lista_estilos_returns_pk(int packing){
            DatosTrim dtrim = new DatosTrim();
            List<Return> lista = new List<Return>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_return,id_packing,id_item,id_inventario,id_talla,id_categoria,id_summary," +
                    " total,id_tarima,cajas,indice FROM packing_returns WHERE id_packing='" + packing + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    Return r = new Return();
                    r.id_return = Convert.ToInt32(leerFilas["id_return"]);
                    r.id_packing = Convert.ToInt32(leerFilas["id_packing"]);
                    r.id_summary = Convert.ToInt32(leerFilas["id_summary"]);
                    int id_estilo = consultas.obtener_estilo_summary(r.id_summary);
                    r.estilo = consultas.obtener_estilo((id_estilo)) + " " + consultas.buscar_descripcion_estilo(id_estilo);
                    r.id_talla = Convert.ToInt32(leerFilas["id_talla"]);
                    r.id_categoria = Convert.ToInt32(leerFilas["id_categoria"]);
                    r.item = dtrim.obtener_descripcion_item(Convert.ToInt32(leerFilas["id_item"]));
                    r.id_inventario = Convert.ToInt32(leerFilas["id_inventario"]);
                    r.descripcion = consultas.buscar_descripcion_item(r.id_inventario);
                    r.cajas = Convert.ToInt32(leerFilas["cajas"]);                   
                    r.id_tarima = Convert.ToInt32(leerFilas["id_tarima"]);                   
                    r.total = Convert.ToInt32(leerFilas["total"]);                   
                    r.indice = Convert.ToInt32(leerFilas["total"]);                   
                    lista.Add(r);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<Return> lista_estilos_returns_pk_sin_tarima(int packing)
        {
            DatosTrim dtrim = new DatosTrim();
            List<Return> lista = new List<Return>();
            Link conn = new Link();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_return,id_packing,id_item,id_inventario,id_talla,id_categoria,id_summary," +
                    " total,id_tarima,cajas,indice FROM packing_returns WHERE id_packing='" + packing + "' and id_tarima=0 ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read())
                {
                    Return r = new Return();
                    r.id_return = Convert.ToInt32(leerFilas["id_return"]);
                    r.id_packing = Convert.ToInt32(leerFilas["id_packing"]);
                    r.id_summary = Convert.ToInt32(leerFilas["id_summary"]);
                    int id_estilo = consultas.obtener_estilo_summary(r.id_summary);
                    r.estilo = consultas.obtener_estilo((id_estilo)) + " " + consultas.buscar_descripcion_estilo(id_estilo);
                    r.id_talla = Convert.ToInt32(leerFilas["id_talla"]);
                    r.id_categoria = Convert.ToInt32(leerFilas["id_categoria"]);
                    r.item = dtrim.obtener_descripcion_item(Convert.ToInt32(leerFilas["id_item"]));
                    r.id_inventario = Convert.ToInt32(leerFilas["id_inventario"]);
                    r.descripcion = consultas.buscar_descripcion_item(r.id_inventario);
                    r.cajas = Convert.ToInt32(leerFilas["cajas"]);
                    r.id_tarima = Convert.ToInt32(leerFilas["id_tarima"]);
                    r.total = Convert.ToInt32(leerFilas["total"]);
                    r.indice = Convert.ToInt32(leerFilas["total"]);
                    lista.Add(r);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public int obtener_tipo_packing(int packing){
            int lista = 0;
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT top 1 id_packing_type FROM packing_list WHERE id_packing_list='"+packing+"' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista = Convert.ToInt32(leerFilas["id_packing_type"]);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public int buscar_bulpack_pedido(int pk){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT ptz.TYPE_PACKING FROM PACKING_TYPE_SIZE ptz,shipping_ids si " +
                    " WHERE si.used='" + pk + "' AND si.id_po_summary=ptz.ID_SUMMARY ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    if (Convert.ToInt32(leer["TYPE_PACKING"]) == 1) {
                        tempo++;
                    }                    
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }
        public int buscar_labels_pedido(int pk){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_label FROM ucc_labels  WHERE id_packing='" + pk + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){                    
                        tempo++;                    
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }
        public List<Pk> obtener_lista_packings(){
            List<Pk> lista = new List<Pk>();
            Link connx = new Link();
            try{
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "select top 100 id_pedido,id_packing_list,pk,id_customer,id_customer_po,fecha,id_packing_type from packing_list  ";
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()){
                    Pk p = new Pk(); //Regex.Replace(color, @"\s+", " ");
                    p.id_packing_list = Convert.ToInt32(leerFilasx["id_packing_list"]);
                    p.packing = Convert.ToString(leerFilasx["pk"]);
                    p.id_customer = Convert.ToInt32(leerFilasx["id_customer"]);
                    p.customer = (consultas.obtener_customer_id(Convert.ToString(leerFilasx["id_customer"]))).Trim();
                    p.customer_po =(consultas.obtener_customer_final_id(Convert.ToString(leerFilasx["id_customer_po"]))).Trim();
                    p.fecha = (Convert.ToDateTime(leerFilasx["fecha"])).ToString("MM/dd/yyyy");
                    p.id_pedido = Convert.ToInt32(leerFilasx["id_pedido"]);
                    p.pedido = consultas.obtener_po_id(Convert.ToString(p.id_pedido));
                    p.id_tipo = Convert.ToInt32(leerFilasx["id_packing_type"]);                    
                    lista.Add(p);
                }
                leerFilasx.Close();
            }
            finally { connx.CerrarConexion(); connx.Dispose(); }
            return lista;
        }
        public Pk obtener_packing_list_individual(int pk){
            // Pk lista = new Pk();
            Pk p = new Pk(); //Regex.Replace(color, @"\s+", " ");
            Link connx = new Link();
            try{
                SqlCommand comandox = new SqlCommand();
                SqlDataReader leerFilasx = null;
                comandox.Connection = connx.AbrirConexion();
                comandox.CommandText = "select nombre_archivo,id_pedido,id_packing_list,pk,id_customer,id_customer_po,id_direccion_envio,id_driver,id_container,shipping_manager,seal,replacement,fecha,tipo,id_packing_type,parte from packing_list where id_packing_list='" + pk + "' ";
                leerFilasx = comandox.ExecuteReader();
                while (leerFilasx.Read()){
                    //Pk p = new Pk(); //Regex.Replace(color, @"\s+", " ");
                    p.id_packing_list = Convert.ToInt32(leerFilasx["id_packing_list"]);
                    p.nombre_archivo = Convert.ToString(leerFilasx["nombre_archivo"]);
                    p.packing = Convert.ToString(leerFilasx["pk"]);
                    p.id_customer = Convert.ToInt32(leerFilasx["id_customer"]);
                    p.id_customer_po = Convert.ToInt32(leerFilasx["id_customer_po"]);
                    p.customer = Regex.Replace(consultas.obtener_customer_id(Convert.ToString(leerFilasx["id_customer"])), @"\s+", " ");
                    p.customer_po = Regex.Replace(consultas.obtener_customer_final_id(Convert.ToString(leerFilasx["id_customer_po"])), @"\s+", " ");
                    p.destino = obtener_direccion(Convert.ToInt32(leerFilasx["id_direccion_envio"]));
                    p.conductor = obtener_driver(Convert.ToInt32(leerFilasx["id_driver"]));
                    p.contenedor = obtener_contenedor(Convert.ToInt32(leerFilasx["id_container"]));
                    p.shipping_manager = Convert.ToString(leerFilasx["shipping_manager"]);
                    p.seal = Convert.ToString(leerFilasx["seal"]);
                    p.replacement = Convert.ToString(leerFilasx["replacement"]);
                    p.fecha = (Convert.ToDateTime(leerFilasx["fecha"])).ToString("MM/dd/yyyy");
                    p.part = Convert.ToString(leerFilasx["parte"]);
                    p.parte = (leerFilasx["parte"]).ToString();
                    p.id_pedido = Convert.ToInt32(leerFilasx["id_pedido"]);
                    p.number_po = buscar_number_po_pedido(Convert.ToInt32(leerFilasx["id_pedido"]));
                    p.tipo = leerFilasx["tipo"].ToString();//TIPO DE PACKING LIST 
                    p.id_tipo = Convert.ToInt32(leerFilasx["id_packing_type"]);
                    if (p.id_tipo != 8 && p.id_tipo != 4){
                        p.pedido = consultas.obtener_po_id((p.id_pedido).ToString());
                    }else{
                        p.pedido = consultas.obtener_po_id_fantasy((p.id_pedido).ToString());
                    }
                   
                    switch (p.id_tipo){
                        case 1:                       
                        case 2:
                        case 7:
                        case 9:
                            p.lista_tarimas = obtener_tarimas(p.id_packing_list);
                            break;
                        case 3:
                            p.lista_samples = obtener_lista_samples_tarima(p.id_packing_list);
                            break;
                        case 5:
                            p.lista_tarimas = obtener_tarimas_returns(p.id_packing_list);
                            break;
                        case 6:
                            p.lista_tarimas = obtener_tarimas_extras_fantasy(p.id_packing_list);
                            break;
                        case 8:
                            p.lista_estilos = obtener_lista_estilos_tarima(0, pk);
                            break;
                        case 4:
                            p.lista_tarimas = obtener_tarimas(p.id_packing_list);
                            break;
                    }
                }
                leerFilasx.Close();
            }finally { connx.CerrarConexion(); connx.Dispose(); }
            //return lista;
            return p;
        }

        public void eliminar_packing_list(int packing){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE FROM packing_list WHERE id_packing_list='" + packing + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        

        public void eliminar_packing_edicion_extras_fantasy(int packing) {            
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_fantasy FROM packing_fantasy WHERE id_packing='" + packing + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    eliminar_totales_extra_packing_fantasy(Convert.ToInt32(leer["id_fantasy"]));
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            eliminar_estilos_extra_packing_fantasy(packing);
        }
        public void eliminar_estilos_extra_packing_fantasy(int packing){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE FROM packing_fantasy WHERE id_packing='" + packing + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void eliminar_extras_packing_fantasy(int fantasy){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE FROM packing_fantasy WHERE id_fantasy='" + fantasy + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }

        public void eliminar_totales_extra_packing_fantasy(int id){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE FROM totales_envios WHERE id_shipping_id='" + id + "' and tipo_packing=6";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void eliminar_packing_edicion_samples(int packing){
            eliminar_cabeceras_sample_packing(packing);
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_sample,id_summary,tipo FROM packing_samples WHERE id_packing_list='" + packing + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    if (Convert.ToInt32(leer["tipo"]) == 1) {
                        eliminar_estilos_nuevos_samples(Convert.ToInt32(leer["id_summary"]));
                    }
                    eliminar_totales_samples(Convert.ToInt32(leer["id_sample"]));
                    eliminar_columnas_extras(Convert.ToInt32(leer["id_sample"]));
                }
                leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            eliminar_estilos_samples_packing(packing);
        }
        public void eliminar_columnas_extras(int id){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE FROM extras_samples WHERE id_packing_sample='" + id + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void eliminar_cabeceras_sample_packing(int id){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE FROM tallas_cabecera_samples WHERE id_packing_sample='" + id + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void eliminar_estilos_nuevos_samples(int id){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE FROM ejemplos_nuevos_fantasy WHERE id_nuevo='" + id + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void eliminar_totales_samples(int id){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE FROM totales_envios WHERE id_shipping_id='" + id + "' and tipo_packing=3";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void eliminar_estilos_samples_packing(int packing){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE FROM packing_samples WHERE id_packing_list='" + packing + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }

        public List<Return> obtener_lista_returns_pk(int packing){
            DatosTrim dtrim = new DatosTrim();
            DatosTransferencias dt = new DatosTransferencias();
            List<Return> lista = new List<Return>();
            Link conn = new Link();
            try{ //Regex.Replace(color, @"\s+", " ");
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_return,id_packing,id_item,id_inventario,id_talla,id_categoria,id_summary,total,cajas,id_tarima,indice" +
                    "  FROM packing_returns WHERE id_packing='" + packing + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    Return r = new Return();
                    r.id_return = Convert.ToInt32(leerFilas["id_return"]);
                    r.id_item = Convert.ToInt32(leerFilas["id_item"]);
                    r.item = dtrim.obtener_componente_item(r.id_item);//AMT
                    r.descripcion_item = dtrim.obtener_descripcion_item(r.id_item);
                    r.id_talla = Convert.ToInt32(leerFilas["id_talla"]);
                    r.talla = consultas.obtener_size_id((r.id_talla).ToString());
                    r.amt = r.item;
                    r.id_categoria = Convert.ToInt32(leerFilas["id_categoria"]);
                    r.id_inventario = Convert.ToInt32(leerFilas["id_inventario"]);
                    Inventario i = dt.consultar_item(r.id_inventario);
                    r.total_inventario = i.total;
                    r.id_summary = Convert.ToInt32(leerFilas["id_summary"]);
                    int id_estilo = consultas.obtener_estilo_summary(r.id_summary);
                    r.estilo = Regex.Replace(consultas.obtener_estilo(id_estilo), @"\s+", " ");
                    r.descripcion = Regex.Replace(consultas.buscar_descripcion_estilo(id_estilo), @"\s+", " ");
                    r.total = Convert.ToInt32(leerFilas["total"]);
                    r.id_color = consultas.obtener_color_id_item_cat(r.id_summary);
                    r.color = Regex.Replace(consultas.obtener_color_id((r.id_color).ToString()), @"\s+", " ");
                    r.cajas = Convert.ToInt32(leerFilas["cajas"]);
                    r.id_pedido = consultas.obtener_id_pedido_summary(r.id_summary);
                    r.id_tarima = Convert.ToInt32(leerFilas["id_tarima"]);
                    r.indice = Convert.ToInt32(leerFilas["indice"]);
                    r.pedido = consultas.obtener_po_id((r.id_pedido).ToString());
                    r.id_genero = consultas.buscar_genero_summary(r.id_summary);
                    r.genero = consultas.obtener_genero_id(Convert.ToString(r.id_genero));
                    lista.Add(r);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public void eliminar_packing_edicion_returns(int packing){
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_return,id_inventario,total FROM packing_returns WHERE id_packing='" + packing + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                   actualizar_inventario_returns(Convert.ToInt32(leer["id_inventario"]), Convert.ToInt32(leer["total"]));
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            eliminar_returns_packing(packing);
        }
        public void actualizar_inventario_returns(int inventario, int cantidad){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE inventario SET total=(total+'" + cantidad + "') WHERE id_inventario='" + inventario + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void eliminar_returns_packing(int packing){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE FROM packing_returns WHERE id_packing='" + packing + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }

        public void eliminar_packing_normal(int packing){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE FROM packing_list WHERE id_packing_list='" + packing + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }

        public List<int> obtener_shipping_ids_packing(int pk){
            List<int> lista = new List<int>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_shipping_id from shipping_ids  WHERE used='" + pk + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    lista.Add(Convert.ToInt32(leer["id_shipping_id"]));
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<int> obtener_shipping_ids_packing_return(int pk){
            List<int> lista = new List<int>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_return from packing_returns  WHERE id_packing='" + pk + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    lista.Add(Convert.ToInt32(leer["id_return"]));
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }

        public List<int> obtener_ids_packing_extras_fantasy(int pk){
            List<int> lista = new List<int>();
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_fantasy from packing_fantasy  WHERE id_packing='" + pk + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    lista.Add(Convert.ToInt32(leer["id_fantasy"]));
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }


        public int buscar_talla_shipping_id(string shipping){
            int temp = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_talla FROM shipping_ids  WHERE id_shipping_id='" + shipping + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temp=Convert.ToInt32(leer["id_talla"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }

        public int buscar_talla_shipping_id_returns(string id){
            int temp = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_talla FROM packing_returns WHERE id_return='" + id + "' ";
                leer = com.ExecuteReader();
                while (leer.Read())                {
                    temp = Convert.ToInt32(leer["id_talla"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }
        public int buscar_inventario_retornos(string pedido,string talla,string item){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_inventario FROM inventario  WHERE id_pedido='" + pedido + "' " +
                    " AND id_size='" + talla + "' AND id_item='" + item + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    tempo=Convert.ToInt32(leer["id_inventario"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }
        public void actualizar_inventario_retorno_id(int return_id){            
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_inventario,total FROM packing_returns  WHERE id_return='" + return_id + "'" ;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    int inv= Convert.ToInt32(leer["id_inventario"]);
                    int total= Convert.ToInt32(leer["total"]);
                    actualizar_inventario_returns(inv,total);
                }
                leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            
        }

        public int obtener_tipo_fila_estilo(string id){
            int tempo = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT dc,tipo,tipo_empaque FROM shipping_ids  WHERE id_shipping_id='" + id + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    int  tipo_empaque = Convert.ToInt32(leer["tipo_empaque"]);
                    string dc = Convert.ToString(leer["dc"]), tipo = Convert.ToString(leer["tipo"]);
                    if (tipo_empaque == 1 && dc == "0" && tipo != "DMG" && tipo != "EXT") {
                        tempo = 0;
                    } else {
                        tempo = 1;
                    }
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return tempo;
        }

        public void guardar_pedido_fantasy(string pedido, int cliente){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "INSERT INTO pedidos_fantasy(pedido,id_cliente)values ('" + pedido + "','" + cliente + "') ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public int obtener_ultimo_pedido_fantasy(){
            int id_recibo = 0;
            Link con_u_r = new Link();
            try{
                SqlCommand com_u_r = new SqlCommand();
                SqlDataReader leer_u_r = null;
                com_u_r.Connection = con_u_r.AbrirConexion();
                com_u_r.CommandText = "SELECT TOP 1 id_pedido FROM pedidos_fantasy order by id_pedido desc ";
                leer_u_r = com_u_r.ExecuteReader();
                while (leer_u_r.Read()){
                    id_recibo = Convert.ToInt32(leer_u_r["id_pedido"]);
                }leer_u_r.Close();
            }finally { con_u_r.CerrarConexion(); con_u_r.Dispose(); }
            return id_recibo;
        }
        public List<int> lista_indices_directo_fantasy(int packing){
            List<int> lista = new List<int>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT distinct index_dc FROM shipping_ids WHERE used='" + packing + "'  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){                    
                    lista.Add(Convert.ToInt32(leerFilas["index_dc"]));
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public void eliminar_packing_edicion_directo_fantasy(int packing){
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_shipping_id FROM shipping_ids WHERE used='" + packing + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    eliminar_totales_directo_fantasy(Convert.ToInt32(leer["id_shipping_id"]));
                    eliminar_estilo_shipping_id(Convert.ToInt32(leer["id_shipping_id"]),8);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            eliminar_estilos_extra_packing_fantasy(packing);
        }
        public void eliminar_totales_directo_fantasy(int id){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE FROM totales_envios WHERE id_shipping_id='" + id + "' and tipo_packing=8";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }

        public string obtener_color_estilo_fantasy(int estilo){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            string temp = "";
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_color from estilos_fantasy where id_estilo='" + estilo + "'";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    temp = consultas.obtener_color_id(Convert.ToString(leer["id_color"]));
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return temp;
        }

        public void guardar_nombre_packing_list(int id,string nombre){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "UPDATE packing_list SET nombre_archivo='" + nombre + "' where id_packing_list='" + id + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }

        public string buscar_parte_packing_anteriores(int pedido,int packing){
            int lista = 1;
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT id_packing_list FROM packing_list WHERE id_pedido=" + pedido + " and id_packing_list!='" + packing + "' AND id_packing_type!=4 AND id_packing_type!=5 AND id_packing_type!=6 AND id_packing_type!=8 AND id_packing_type!=9 AND id_packing_type!=3 ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista++;
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista.ToString();
        }
        public string generar_parte(string parte,int packing,int id_pedido) {
            string parte_nueva = "";
            int total_enviado = obtener_total_enviado_pedido(id_pedido);
            int total_pedido = obtener_total_pedido(id_pedido);
            int total_anterior = obtener_total_enviado_pedido_exclusivo(id_pedido, packing);
            int total_pk = obtener_total_enviado_pk(packing);
            switch (parte){
                case "0": parte_nueva = "0"; break;
                case "i":
                case "u":
                case "1":
                    if (total_pk >= total_pedido) { parte_nueva = "u"; }
                    else { parte_nueva = "1"; }
                    break;
                default:
                    if (total_enviado >= total_pedido) { parte_nueva = (buscar_parte_packing_anteriores(id_pedido, packing)).ToString() + ",f"; }
                    else { parte_nueva = buscar_parte_packing_anteriores(id_pedido, packing); }
                    break;
            }            
            actualizar_packing_parte(packing, parte_nueva);
            return parte_nueva;
        }

        public void editar_nombre_packing_list(int packing) {
            string archivo = "";
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "select id_pedido,id_packing_list,pk,id_customer,id_customer_po,id_direccion_envio,id_driver,id_container,shipping_manager,seal,replacement,fecha,tipo,id_packing_type,parte from packing_list where id_packing_list = '" + packing + "'";
                leer = com.ExecuteReader();
                while (leer.Read()) {
                    Pk p = new Pk(); //Regex.Replace(color, @"\s+", " ");
                    p.id_packing_list = Convert.ToInt32(leer["id_packing_list"]);
                    p.id_packing_type = Convert.ToInt32(leer["id_packing_type"]);
                    p.packing = Convert.ToString(leer["pk"]);
                    p.id_customer = Convert.ToInt32(leer["id_customer"]);
                    p.id_customer_po = Convert.ToInt32(leer["id_customer_po"]);
                    p.customer = Regex.Replace(consultas.obtener_customer_id(Convert.ToString(leer["id_customer"])), @"\s+", " ");
                    p.customer_po = Regex.Replace(consultas.obtener_customer_final_id(Convert.ToString(leer["id_customer_po"])), @"\s+", " ");                   
                    

                    p.id_pedido = Convert.ToInt32(leer["id_pedido"]);
                    p.id_tipo = Convert.ToInt32(leer["id_packing_type"]);
                    if (p.id_tipo != 8 && p.id_tipo != 4){
                        p.pedido = consultas.obtener_po_id((p.id_pedido).ToString());                        
                    }else{
                        p.pedido = consultas.obtener_po_id_fantasy((p.id_pedido).ToString());
                    }
                    if (p.id_tipo == 1 || p.id_tipo == 2 || p.id_tipo == 7) {
                        p.parte = generar_parte(Convert.ToString(leer["parte"]), p.id_packing_list, p.id_pedido);
                        
                        if ((p.parte).Contains(",")) {
                            string[] parte_temporal = (p.parte).Split(',');
                            p.parte = consultas.AddOrdinal(Convert.ToInt32(parte_temporal[0])) + " Part and Final shipment";
                        }else {
                            switch (p.parte) {
                                case "u":
                                case "0":
                                    p.parte = " ";
                                    break;
                                default:
                                    p.parte = consultas.AddOrdinal(Convert.ToInt32(p.parte)) + " Part";
                                    break;
                            }
                        }
                    }
                    p.number_po = buscar_number_po_pedido(Convert.ToInt32(leer["id_pedido"]));
                    p.tipo = leer["tipo"].ToString();//TIPO DE PACKING LIST                   
                    p.id_tipo = Convert.ToInt32(leer["id_packing_type"]);
                    switch (p.id_tipo){
                        case 1:
                        case 2:
                        case 9:
                            List<Tarima> lista_tarimas = obtener_tarimas(packing);
                            List<estilos> lista_e = new List<estilos>();
                            List<int> id_pedidos = new List<int>();
                            List<int> ecom = new List<int>();
                            int flag = 0;
                            foreach (Tarima t in lista_tarimas){
                                foreach (estilos e in t.lista_estilos){
                                    if (e.tipo_empaque != 3){
                                        id_pedidos.Add(consultas.obtener_id_pedido_summary(e.id_po_summary));
                                    }else{
                                        foreach (estilos ee in e.assort.lista_estilos){
                                            id_pedidos.Add(consultas.obtener_id_pedido_summary(ee.id_po_summary));
                                        }
                                    }
                                }
                                flag++;
                            }
                            if (flag == 0) {
                                lista_e = buscar_lista_estilos_pedido(packing);
                                foreach (estilos e in lista_e){
                                    if (e.tipo_empaque != 3){
                                        id_pedidos.Add(consultas.obtener_id_pedido_summary(e.id_po_summary));
                                    }else{
                                        foreach (estilos ee in e.assort.lista_estilos){
                                            id_pedidos.Add(consultas.obtener_id_pedido_summary(ee.id_po_summary));
                                        }
                                    }
                                }
                            }
                            id_pedidos = id_pedidos.Distinct().ToList();
                            int ecom_cont = 0;
                            foreach (int i in id_pedidos){
                                archivo += " " + consultas.obtener_po_id(Convert.ToString(i));
                                foreach (Tarima t in lista_tarimas){
                                    foreach (estilos e in t.lista_estilos){
                                        if (e.tipo_empaque != 3){
                                            int tempo = consultas.obtener_id_pedido_summary(e.id_po_summary);
                                            if (tempo == i && e.tipo == "ECOM"){ ecom_cont++; }
                                        }
                                    }
                                }
                            }
                            if (ecom_cont!=0) { archivo += " ECOM"; }
                            int ex_label = contar_labels(packing);
                            if (p.id_tipo != 9){
                                if (ex_label != 0){
                                    List<Labels> lista_etiquetas = obtener_etiquetas(packing);
                                    archivo += "(PO# ";
                                    foreach (Labels l in lista_etiquetas) { archivo += " " + l.label; }
                                    if (ex_label == 1) { archivo += " )" + " (With UCC Labels) " + p.parte; }
                                    else { archivo += " )" + " (With TPM Labels) " + p.parte; }
                                }else{
                                    if (p.tipo == "7") { archivo += " (Without TPM Labels) " + p.parte; }
                                    else { archivo += " (Without UCC Labels) " + p.parte; }
                                }
                            }
                            if (p.id_tipo == 9) { archivo += " (EXT-DMG) " ; }
                            archivo = Regex.Replace((archivo).Trim(), @"\s+", " ");
                            break;
                        case 7:
                            lista_tarimas = obtener_tarimas(packing);
                            List<int> id_pedidosht = new List<int>();
                            List<int> ecomht = new List<int>();
                            int flaght = 0;
                            List<estilos> lista_eht = new List<estilos>();
                            foreach (Tarima t in lista_tarimas){
                                foreach (estilos e in t.lista_estilos){
                                    id_pedidosht.Add(consultas.obtener_id_pedido_summary(e.id_po_summary));
                                }
                                flaght++;
                            }
                            if (flaght == 0){
                                lista_eht = buscar_lista_estilos_pedido(packing);
                                foreach (estilos e in lista_eht){
                                    id_pedidosht.Add(consultas.obtener_id_pedido_summary(e.id_po_summary));
                                }
                            }
                            id_pedidosht = id_pedidosht.Distinct().ToList();
                            foreach (int i in id_pedidosht) {
                                archivo += " "+ consultas.obtener_po_id(Convert.ToString(i));
                            }
                            archivo += " (With TPM Labels) " + p.parte;
                            archivo = Regex.Replace((archivo).Trim(), @"\s+", " ");
                            break;
                        case 3:
                            if (p.id_customer == 2) {
                                archivo = "SAMPLES FANTASY";
                            } else {
                                archivo = "SAMPLES ";
                                List<Sample> lista_samples = obtener_lista_samples_tarima(packing);
                                List<int> id_pedidos_samples = new List<int>();
                                foreach (Sample s in lista_samples) {
                                    if (s.tipo_sample == 0){ id_pedidos_samples.Add(consultas.obtener_id_pedido_summary(s.id_summary)); }
                                }
                                id_pedidos_samples = id_pedidos_samples.Distinct().ToList();
                                foreach (int i in id_pedidos_samples) { archivo += " " + consultas.obtener_po_id(Convert.ToString(i)); }
                                id_pedidos_samples.Clear();
                                foreach (Sample s in lista_samples){ if (s.tipo_sample == 1) { id_pedidos_samples.Add(s.id_summary); } }
                                id_pedidos_samples = id_pedidos_samples.Distinct().ToList();
                                foreach (int i in id_pedidos_samples) { archivo += " " + consultas.obtener_po_samples_id(Convert.ToString(i)); }
                                archivo = Regex.Replace((archivo).Trim(), @"\s+", " ");
                            }
                            break;
                        case 5:
                            lista_tarimas = obtener_tarimas_returns(packing);
                            int blanks = 0, trims = 0;
                            int flagr = 0;
                            List<int> id_pedidos_returns = new List<int>();
                            foreach (Tarima t in lista_tarimas){
                                foreach (Return r in t.lista_returns){
                                    id_pedidos_returns.Add(consultas.obtener_id_pedido_summary(r.id_summary));
                                    if (r.id_categoria == 1) { blanks++; } else { trims++; }
                                }
                                flagr++;
                            }
                            if (flagr == 0) {
                                List<Return> listar = lista_estilos_returns_pk(packing);
                                foreach (Return r in listar){
                                    id_pedidos_returns.Add(consultas.obtener_id_pedido_summary(r.id_summary));
                                    if (r.id_categoria == 1) { blanks++; } else { trims++; }
                                }
                            }                            
                            id_pedidos_returns = id_pedidos_returns.Distinct().ToList();
                            foreach (int i in id_pedidos_returns) { archivo += " " + consultas.obtener_po_id(Convert.ToString(i)); }
                            if (blanks != 0) { archivo += " BLANKS "; }
                            if (trims != 0) { archivo += " TRIMS "; }
                            archivo += " RETURNS ";
                            archivo = Regex.Replace((archivo).Trim(), @"\s+", " ");
                            break;
                        case 6:
                            //lista_tarimas = obtener_tarimas_extras_fantasy(packing);
                            archivo = " DAMAGES-EXTRAS FANTASY";
                            break;
                        case 8:
                            List<estilos> lista_estilos = obtener_lista_estilos_tarima(0, packing);
                            int ex_label_f = contar_labels(packing);
                            archivo = p.customer_po + " PO#" + p.pedido + " ";
                            if (ex_label_f != 0){
                                archivo += "( ";
                                if (p.customer_po.Contains("Kohl")) { archivo += " Kohls"; }
                                archivo += " PO # ";
                                List<Labels> lista_etiquetas_f = obtener_etiquetas(packing);
                                foreach (Labels l in lista_etiquetas_f) { archivo += " " + l.label; }
                                if (ex_label_f == 1) { archivo += " )" + " (With UCC Labels) "; }
                                else { archivo += " )" + " (With TPM Labels) "; }
                            }else{
                                if (p.id_tipo == 7) { archivo += " (Without TPM Labels)"; }
                                else { archivo += " (Without UCC Labels)"; }
                            }
                            break;
                        case 4:
                            //List<Tarima> lista_tarimas = obtener_tarimas(packing);
                            archivo = p.customer_po + " RPLN (PO#" + p.pedido + ") (With UCC Labels)";
                            break;
                    }
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            guardar_nombre_packing_list(packing, archivo);
        }
        public int obtener_tipo_packing_packing(int packing){
            int lista = 0;
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_packing_type from packing_list where id_packing_list='" + packing + "' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    lista = Convert.ToInt32(leer["id_packing_type"]);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }

        public void guardar_packing_borrado(string pk, string fecha,string tipo){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "INSERT INTO packings_borrados(pk,fecha,tipo_packing) VALUES  ('" + pk + "','" + fecha + "','" + tipo + "')";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public List<Pk_borrado> obtener_pks_borrados(string tipo){
            List<Pk_borrado> lista = new List<Pk_borrado>();
            string query;
            if (tipo == "9"){
                query = "SELECT id_pk,pk,fecha from packings_borrados WHERE tipo_packing=9";
            }else {
                query = "SELECT id_pk,pk,fecha from packings_borrados WHERE tipo_packing!=9";
            }
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = query;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Pk_borrado pb = new Pk_borrado();
                    pb.id_pk= Convert.ToInt32(leer["id_pk"]);
                    pb.fecha= (Convert.ToDateTime(leer["fecha"])).ToString("MM/dd/yyyy");
                    pb.pk= Convert.ToString(leer["pk"]);
                    lista.Add(pb);
                }
                leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public string obtener_pk_borrado(int id_pk_borrado){
            string lista = "";
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select  pk from packings_borrados where id_pk='"+id_pk_borrado+"' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista = Convert.ToString(leerFilas["pk"]);
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public void eliminar_packing_borrado(int id){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE from packings_borrados WHERE id_pk='" + id + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }


        //actualizar_packing_parte
        public List<Cantidades_pedido> obtener_cantidades_envio_anterior(int pedido, List<int> indices){
            List<Cantidades_pedido> lista = new List<Cantidades_pedido>();
            foreach (int i in indices){
                Cantidades_pedido c = new Cantidades_pedido();
                List<Talla> tallas = new List<Talla>();
                Link con = new Link();
                try{
                    SqlCommand com = new SqlCommand();
                    SqlDataReader leer = null;
                    com.Connection = con.AbrirConexion();
                    com.CommandText = "SELECT id_summary,id_talla,total,tipo,fecha FROM cantidades_envios_pedidos " +
                        " WHERE id_pedido='" + pedido + "' AND indice='" + i + "' ";
                    leer = com.ExecuteReader();
                    while (leer.Read()){
                        c.indice = i;
                        c.id_summary = Convert.ToInt32(leer["id_summary"]);
                        c.tipo = Convert.ToString(leer["tipo"]);
                        Talla t = new Talla();
                        t.id_talla = Convert.ToInt32(leer["id_talla"]);
                        t.total = Convert.ToInt32(leer["total"]);
                        c.fecha = (Convert.ToDateTime(leer["fecha"])).ToString("MM/dd/yyyy");
                        tallas.Add(t);
                    }leer.Close();
                }finally { con.CerrarConexion(); con.Dispose(); }
                c.lista_enviados = tallas;
                lista.Add(c);
            }
            return lista;
        }
        public List<int> buscar_indices_cantidades_enviadas(string pedido){
            List<int> lista = new List<int>();
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select distinct indice from cantidades_envios_pedidos where id_pedido='" + pedido + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista.Add(Convert.ToInt32(leerFilas["indice"]));
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public void eliminar_cantidades_enviadas_extra(string pedido){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "DELETE from cantidades_envios_pedidos WHERE id_pedido='" + pedido + "' ";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public void guardar_cantidades_enviadas_extra(string pedido, string estilo,string talla,string total,string tipo,string indice,string fecha){
            Link con_s = new Link();
            try{
                SqlCommand com_s = new SqlCommand();
                com_s.Connection = con_s.AbrirConexion();
                com_s.CommandText = "INSERT INTO cantidades_envios_pedidos(id_pedido,id_summary,id_talla,total,tipo,fecha,indice) VALUES" +
                    "   ('" + pedido + "','" + estilo + "','" + talla + "','" + total + "','" + tipo + "'," +
                    "'" +fecha + "','" + indice + "')";
                com_s.ExecuteNonQuery();
            }finally { con_s.CerrarConexion(); con_s.Dispose(); }
        }
        public List<Pk> obtener_lista_shipping_summary_cantidades_extra(int summary, string tipo, List<Talla> lista_tallas){
            List<Pk> lista = new List<Pk>();
            string query = "";
            if (tipo != "DMG" && tipo != "EXT" && tipo != "SAM"){
                query = "SELECT distinct indice FROM cantidades_envios_pedidos WHERE id_summary='" + summary + "'  " +
                    "   AND tipo!='DMG' AND tipo!='EXT' AND tipo!='SAM'";
            }else{
                if (tipo == "SAM"){
                    query = "SELECT distinct indice FROM cantidades_envios_pedidos WHERE id_summary='" + summary + "'   " +
                        " AND tipo='SAM'  ";
                }else{
                    query = "SELECT distinct indice FROM cantidades_envios_pedidos WHERE id_summary='" + summary + "'    " +    
                        " AND tipo!='ECOM' AND tipo!='INITIAL' AND tipo!='SAM' AND tipo!='NONE' AND tipo!='RPLN'  ";
                }
            }
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = query;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Pk p = new Pk();
                    p.id_packing_list = Convert.ToInt32(leer["indice"]);
                    p.packing = "TEMP"+Convert.ToString(leer["indice"]);
                    p.fecha = buscar_fecha_cantidades_extras_envios(Convert.ToInt32(leer["indice"]), summary);
                    p.destino = obtener_direccion(0);
                    p.lista_tallas = obtener_lista_items_customer_cantidades_extra(p.id_packing_list, summary, tipo, lista_tallas);
                    lista.Add(p);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Talla> obtener_lista_items_customer_cantidades_extra(int indice, int summary, string tipo, List<Talla> lista_tallas){
            List<Talla> lista = new List<Talla>();
            string query = "";
            if (tipo != "DMG" && tipo != "EXT" && tipo != "SAM"){
                query = "SELECT id_talla,total,tipo,indice FROM cantidades_envios_pedidos WHERE  " +
                    "  indice='" + indice + "' AND id_summary='" + summary + "' AND tipo!='DMG' AND tipo!='EXT' AND tipo!='SAM'";
            }else{
                if (tipo == "SAM"){
                    query = "SELECT id_talla,total,tipo,indice FROM cantidades_envios_pedidos WHERE  " +
                        "  indice='" + indice + "' AND id_summary='" + summary + "'AND tipo=='SAM'  ";                        
                }else{
                    query = "SELECT id_talla,total,tipo,indice FROM cantidades_envios_pedidos WHERE   " +
                        " indice='" + indice + "' AND   " +
                        " id_summary='" + summary + "' " +
                        " AND tipo!='ECOM' AND tipo!='INITIAL' AND tipo!='SAM' AND tipo!='NONE' AND tipo!='RPLN'  ";
                    //AND t.tipo='" + tipo + "'
                }
            }
            Link con = new Link();
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = query;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Talla ts = new Talla();
                    ts.total = Convert.ToInt32(leer["total"]);
                    ts.id_talla = Convert.ToInt32(leer["id_talla"]);
                    ts.tipo = Convert.ToString(leer["tipo"]);
                    lista.Add(ts);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }

            /* List<Talla> lista_temporal = new List<Talla>();            
             Link con = new Link();
             try{
                 SqlCommand com = new SqlCommand();
                 SqlDataReader leer = null;
                 com.Connection = con.AbrirConexion();
                 com.CommandText = query;
                 leer = com.ExecuteReader();
                 while (leer.Read()){
                     Talla ts = new Talla();
                     ts.total = Convert.ToInt32(leer["total"]);
                     ts.id_talla = Convert.ToInt32(leer["id_talla"]);
                     ts.tipo = Convert.ToString(leer["tipo"]);
                     lista_temporal.Add(ts);
                 }leer.Close();
             }
             finally { con.CerrarConexion(); con.Dispose(); }
             foreach (Talla t in lista_tallas){
                 int total = 0;
                 foreach (Talla lt in lista_temporal){
                     if (t.id_talla == lt.id_talla){
                         total += lt.total;
                     }
                 }
                 Talla ts = new Talla();
                 ts.total = total;
                 ts.id_talla = t.id_talla;
                 ts.tipo = tipo;
                 lista.Add(ts);
             }*/
            return lista;
        }
        public string buscar_fecha_cantidades_extras_envios(int indice,int summary){
            string lista = "";
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select fecha from cantidades_envios_pedidos where indice='" + indice + "' AND id_summary='" + summary + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista = (Convert.ToDateTime(leerFilas["fecha"])).ToString("MMM dd yyyy");
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
        public string buscar_fechas_envio_anterior_extra(string indice, string summary){
            string lista = "";
            Link conn = new Link();
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "select fecha from cantidades_envios_pedidos where indice='" + indice + "' AND id_summary='" + summary + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    lista = (Convert.ToString(leerFilas["fecha"]));
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }
















































    }
}






/*
 
     
     public virtual ActionResult PrintInvoice(int id) {
        var invoice = db.Invoices.Single(i => i.InvoiceId == id);
        var viewmodel = Mapper.Map<InvoiceViewModel>(invoice);

        var reportName = string.Format(@"Invoice {0:I-000000}.pdf", invoice.InvoiceNo);
        var switches = String.Format(@" --print-media-type --username {0} --password {1} ", 
                ConfigurationManager.AppSettings["PdfUserName"],
                ConfigurationManager.AppSettings["PdfPassword"]); 
        ViewBag.isPDF = true;
        return new ViewAsPdf("InvoiceDetails", viewmodel) {
            FileName = reportName,
            PageOrientation = Rotativa.Options.Orientation.Portrait,
            PageSize = Rotativa.Options.Size.A4,
            CustomSwitches = switches
        };
    }
     
     */




















































