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
using FortuneSystem.Models.Almacen;

namespace FortuneSystem.Models.Staging
{
    public class DatosStaging
    {
        FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
        StagingGeneral stag = new StagingGeneral();
        /*******************************************************************************************************************/

       

        public List<pedido_staging> lista_papeleta(int estilo,int pedido,int turno){
            List<pedido_staging> lista = new List<pedido_staging>();
             Link con = new Link() ;
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                /* if (tipo == "r") {
                     com.CommandText = "SELECT r.id_inventario,r.total, i.id_pedido,i.id_estilo,i.descripcion,rg.fecha from recibos_items r,inventario i,recibos rg where " +
                    "  r.id_inventario=i.id_inventario and i.id_inventario='" + inventario + "' and r.id_recibo=rg.id_recibo order by rg.fecha desc";
                 }
                 if (tipo == "s") {*/
                //com.CommandText = "SELECT i.id_inventario, i.id_pedido,i.id_estilo,i.descripcion from inventario i where " +
                //"   i.id_inventario='" + inventario + "' ";
                //}
                com.CommandText = "select ID_PEDIDOS,ITEM_ID from PO_SUMMARY WHERE ID_PEDIDOS='"+pedido+"' AND ITEM_ID='"+estilo+"'";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    pedido_staging ps = new pedido_staging();
                    ps.id_pedido = Convert.ToInt32(leer["ID_PEDIDOS"]);
                    ps.id_estilo = Convert.ToInt32(leer["ITEM_ID"]);
                    ps.descripcion = consultas.buscar_descripcion_estilo(ps.id_estilo);
                    ps.estilo_nombre = consultas.buscar_descripcion_estilo(ps.id_estilo);
                    ps.po = consultas.obtener_po_id((ps.id_pedido).ToString());
                    //ps.total = Convert.ToInt32(leer["total"]);
                    ps.estilo = consultas.obtener_estilo(ps.id_estilo);
                    //ps.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    //ps.fecha = (Convert.ToDateTime(leer["fecha"])).ToString("MMM dd yyyy");
                    if (turno == 1) { ps.turno = "PRIMER TURNO"; } else { ps.turno = "SEGUNDO TURNO"; }                    
                    lista.Add(ps);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }

        public int buscar_id_summary(string po,string estilo){
            int temp = 0;
             Link conx = new Link() ;
            try{
                SqlCommand comx = new SqlCommand();
                SqlDataReader leerx = null;
                comx.Connection = conx.AbrirConexion();
                comx.CommandText = "SELECT ID_PO_SUMMARY FROM PO_SUMMARY WHERE ID_PEDIDOS='" + po + "' AND ITEM_ID='"+estilo+"'  ";
                leerx = comx.ExecuteReader();
                while (leerx.Read()){
                    temp = Convert.ToInt32(leerx["ID_PO_SUMMARY"]);
                }
                leerx.Close();
            }finally{ conx.CerrarConexion(); conx.Dispose(); }
            return temp;
        }

        public void guardar_stag_bd(string pedido,string estilo,int total,int usuario, int summary,string comentarios,string fecha){
            // DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
             Link con_r = new Link() ;
            try{
                SqlCommand com_r = new SqlCommand();
                com_r.Connection = con_r.AbrirConexion();
                com_r.CommandText = "INSERT INTO staging(id_pedido,id_estilo,total,id_usuario_captura,id_summary,comentarios,fecha) values('" + 
                    pedido + "','" + estilo + "','" + total + "','" + usuario + "','" + summary + "','" + comentarios + "','" +fecha + "') ";
                com_r.ExecuteNonQuery();
            }finally{
                con_r.CerrarConexion(); con_r.Dispose();
            }
        }

        public int obtener_ultimo_stag(){
            int temporal = 0;
             Link con_u_r_i = new Link() ;
            try{
                SqlCommand com_u_r_i = new SqlCommand();
                SqlDataReader leer_u_r_i = null;
                com_u_r_i.Connection = con_u_r_i.AbrirConexion();
                com_u_r_i.CommandText = "SELECT TOP 1 id_staging FROM staging order by id_staging desc ";
                leer_u_r_i = com_u_r_i.ExecuteReader();
                while (leer_u_r_i.Read()){
                    temporal = Convert.ToInt32(leer_u_r_i["id_staging"]);
                }
                leer_u_r_i.Close();
            }finally{
                con_u_r_i.CerrarConexion();
                con_u_r_i.Dispose();
            }
            return temporal;
        }

        public void guardar_stag_conteos(int staging,int talla,int pais,int color, int porcentaje, int total,string usuario){
             Link con_r = new Link() ;
            try{
                SqlCommand com_r = new SqlCommand();
                com_r.Connection = con_r.AbrirConexion();
                com_r.CommandText = "INSERT INTO staging_count(id_staging,id_talla,id_pais,id_color,id_porcentaje,total,id_empleado) values('" +
                    staging + "','" + talla + "','" + pais + "','" + color + "','" +porcentaje + "','" + total + "','"+usuario+"') ";
                com_r.ExecuteNonQuery();
            }finally{con_r.CerrarConexion(); con_r.Dispose();}
        }
        //lista_papeleta_staging
        public List<stag_conteo> lista_papeleta_staging(int stag,int turno)
        {
            List<stag_conteo> lista = new List<stag_conteo>();
             Link con = new Link() ;
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT s.id_pedido,s.id_estilo,s.comentarios,s.fecha,s.id_usuario_captura,sc.id_talla,sc.id_pais,sc.id_color,sc.id_porcentaje,sc.total,sc.id_empleado  " +
                    " from staging_count sc,staging s where sc.id_staging=s.id_staging and sc.id_staging='"+stag+"' ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    stag_conteo ps = new stag_conteo();
                    ps.estilo_nombre = consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    ps.po = consultas.obtener_po_id(leer["id_pedido"].ToString());
                    ps.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"]));
                    ps.fecha = (Convert.ToDateTime(leer["fecha"])).ToString("MMM dd yyyy");
                    ps.color= consultas.obtener_color_id(Convert.ToString(leer["id_color"]))+"-"+consultas.obtener_descripcion_color_id(Convert.ToString(leer["id_color"]));
                    ps.talla= consultas.obtener_size_id(Convert.ToString(leer["id_talla"]));
                    ps.porcentaje = consultas.obtener_fabric_percent_id(Convert.ToString(leer["id_porcentaje"]));
                    ps.pais= consultas.obtener_pais_id(Convert.ToString(leer["id_pais"]));
                    ps.cantidad = Convert.ToString(leer["total"]);
                    ps.usuario_conteo = Convert.ToString(leer["id_empleado"]);
                    ps.observaciones = leer["comentarios"].ToString();
                    ps.usuario = (consultas.buscar_nombre_usuario(leer["id_usuario_captura"].ToString())).ToUpper();
                    if (turno == 1) { ps.turno = "PRIMER TURNO"; } else { ps.turno = "SEGUNDO TURNO"; }
                    lista.Add(ps);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public string obtener_nombre_empleado(int cadena)
        {
            string temp = "";
             Link con = new Link() ;
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT nombre_empleado from empleados where id_empleado ='" + cadena + "'";
                leer = com.ExecuteReader();
                while (leer.Read())
                {
                    temp = Convert.ToString(leer["nombre_empleado"]).ToUpper();
                }
                leer.Close();
            }
            finally
            {
                con.CerrarConexion(); con.Dispose();
            }
            return temp;
        }

        public List<stag_conteo> obtener_staging_inicio(){
            /*string query = "";
            if (busqueda == "0"){
                query = "SELECT top 20 s.id_staging,s.id_pedido,s.id_estilo,s.comentarios,s.fecha,s.id_usuario_captura,s.total  " +
                    " from staging s order by s.id_staging desc";
            }else {
                query = "SELECT top 20 s.id_staging,s.id_pedido,s.id_estilo,s.comentarios,s.fecha,s.id_usuario_captura,s.total  " +
                    " from staging s " +
                    "" +
                    "order by s.id_staging desc";
            }*/
            List<stag_conteo> lista = new List<stag_conteo>();
             Link con = new Link() ;
            int i = 0;
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT  s.id_staging,s.id_pedido,s.id_estilo,s.comentarios,s.fecha,s.id_usuario_captura,s.total  " +
                  " from staging s order by s.id_staging desc";
                //com.CommandText = query;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    stag_conteo ps = new stag_conteo();
                    ps.id_staging = Convert.ToInt32(leer["id_staging"]);
                    ps.estilo_nombre = consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    ps.po = consultas.obtener_po_id(leer["id_pedido"].ToString());
                    ps.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"]));
                    ps.fecha = ((Convert.ToDateTime(leer["fecha"])).ToString("MMM dd yyyy")).ToUpper();
                    ps.cantidad = Convert.ToString(leer["total"]);
                    ps.usuario = (consultas.buscar_nombre_usuario(leer["id_usuario_captura"].ToString())).ToUpper();
                    lista.Add(ps);
                }
                leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<stag_conteo> obtener_staging_inicio_busqueda(string busqueda){
            string query = "";
            if (busqueda == "0"){
                query = "SELECT top 20 s.id_staging,s.id_pedido,s.id_estilo,s.comentarios,s.fecha,s.id_usuario_captura,s.total  " +
                    " from staging s order by s.id_staging desc";
            }else {
                query = "SELECT top 20 s.id_staging,s.id_pedido,s.id_estilo,s.comentarios,s.fecha,s.id_usuario_captura,s.total  " +
                    " from staging s,PEDIDO p WHERE p.ID_PEDIDO=s.id_pedido AND p.PO like'%"+busqueda+"%' " +
                    "order by s.id_staging desc";
            }
            List<stag_conteo> lista = new List<stag_conteo>();
             Link con = new Link() ;
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                //com.CommandText = "SELECT  s.id_staging,s.id_pedido,s.id_estilo,s.comentarios,s.fecha,s.id_usuario_captura,s.total  " +
                //  " from staging s order by s.id_staging desc";
                com.CommandText = query;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    stag_conteo ps = new stag_conteo();
                    ps.id_staging = Convert.ToInt32(leer["id_staging"]);
                    ps.estilo_nombre = consultas.buscar_descripcion_estilo(Convert.ToInt32(leer["id_estilo"]));
                    ps.po = consultas.obtener_po_id(leer["id_pedido"].ToString());
                    ps.estilo = consultas.obtener_estilo(Convert.ToInt32(leer["id_estilo"]));
                    ps.fecha = ((Convert.ToDateTime(leer["fecha"])).ToString("MMM dd yyyy")).ToUpper();
                    ps.cantidad = Convert.ToString(leer["total"]);
                    ps.usuario = (consultas.buscar_nombre_usuario(leer["id_usuario_captura"].ToString())).ToUpper();
                    lista.Add(ps);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }


        public List<pedido_staging> buscar_pedidos_recibo(int sucursal, string busqueda)
        {
            //string query = "";

            List<pedido_staging> lista = new List<pedido_staging>();
            if (busqueda == "0")
            {
                List<pedido_staging> lista_inicial= obtener_stag_inicial ();
                foreach (pedido_staging ps in lista_inicial) {
                    lista.Add(ps);
                }

            }else{
                List<pedido_staging> lista_stag_pedido = obtener_stag_pedido(busqueda);
                foreach (pedido_staging ps in lista_stag_pedido){
                    lista.Add(ps);
                }
                List<pedido_staging> lista_stag_estilos= obtener_stag_estilos(busqueda);
                foreach (pedido_staging ps in lista_stag_estilos){
                    lista.Add(ps);
                }
         
            }

            return lista;
        }

        public List<pedido_staging> obtener_stag_inicial()
        {
            List<pedido_staging> lista = new List<pedido_staging>();
           
             Link conn = new Link() ;
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT top 25 'p' as 'tipo',PS.ID_PEDIDOS,PS.ITEM_ID,PS.ID_COLOR,PS.ID_PO_SUMMARY FROM PO_SUMMARY PS,PEDIDO P WHERE P.ID_PEDIDO=PS.ID_PEDIDOS AND P.ID_STATUS!=6 AND P.ID_STATUS!=7 ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read())
                {
                    pedido_staging ps = new pedido_staging();
                    ps.id_summary = Convert.ToInt32(leerFilas["ID_PO_SUMMARY"]);
                    ps.id_pedido = Convert.ToInt32(leerFilas["ID_PEDIDOS"]);
                    ps.id_estilo = Convert.ToInt32(leerFilas["ITEM_ID"]);
                    ps.color = consultas.obtener_descripcion_color_id(Convert.ToString(leerFilas["ID_COLOR"]));
                    ps.descripcion = consultas.buscar_descripcion_estilo(ps.id_estilo);
                    /*ps.id_customer= Convert.ToInt32(leer["id_customer"]);
                    ps.id_customer_final= Convert.ToInt32(leer["id_customer_final"]);*/
                    ps.estilo_nombre = consultas.buscar_descripcion_estilo(ps.id_estilo);
                    ps.po = consultas.obtener_po_id((ps.id_pedido).ToString());
                    //ps.total= Convert.ToInt32(leer["total"]);
                    ps.estilo = consultas.obtener_estilo(ps.id_estilo);
                    //ps.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    ps.id_inventario = 0;
                    //ps.fecha = (Convert.ToDateTime(leer["fecha"])).ToString("MMM dd yyyy");
                    ps.tipo_stag = Convert.ToString(leerFilas["tipo"]);
                    lista.Add(ps);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<pedido_staging> obtener_stag_pedido(string busqueda)
        {
            List<pedido_staging> lista = new List<pedido_staging>();

             Link conn = new Link() ;
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT DISTINCT 'p' as 'tipo',PS.ID_PEDIDOS,PS.ITEM_ID,PS.ID_COLOR,PS.ID_PO_SUMMARY FROM PO_SUMMARY PS,PEDIDO P,ITEM_DESCRIPTION ITD WHERE P.ID_PEDIDO=PS.ID_PEDIDOS AND P.ID_STATUS!=6 AND P.ID_STATUS!=7 " +
                    " AND ITD.ITEM_ID=PS.ITEM_ID AND ITD.DESCRIPTION LIKE '%" + busqueda + "%'  ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read())
                {
                    pedido_staging ps = new pedido_staging();
                    ps.id_summary = Convert.ToInt32(leerFilas["ID_PO_SUMMARY"]);
                    ps.id_pedido = Convert.ToInt32(leerFilas["ID_PEDIDOS"]);
                    ps.id_estilo = Convert.ToInt32(leerFilas["ITEM_ID"]);
                    ps.color = consultas.obtener_descripcion_color_id(Convert.ToString(leerFilas["ID_COLOR"]));
                    ps.descripcion = consultas.buscar_descripcion_estilo(ps.id_estilo);
                    /*ps.id_customer= Convert.ToInt32(leer["id_customer"]);
                    ps.id_customer_final= Convert.ToInt32(leer["id_customer_final"]);*/
                    ps.estilo_nombre = consultas.buscar_descripcion_estilo(ps.id_estilo);
                    ps.po = consultas.obtener_po_id((ps.id_pedido).ToString());
                    //ps.total= Convert.ToInt32(leer["total"]);
                    ps.estilo = consultas.obtener_estilo(ps.id_estilo);
                    //ps.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    ps.id_inventario = 0;
                    //ps.fecha = (Convert.ToDateTime(leer["fecha"])).ToString("MMM dd yyyy");
                    ps.tipo_stag = Convert.ToString(leerFilas["tipo"]);
                    lista.Add(ps);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<pedido_staging> obtener_stag_estilos(string busqueda)
        {
            List<pedido_staging> lista = new List<pedido_staging>();

             Link conn = new Link() ;
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT DISTINCT 'p' as 'tipo',PS.ID_PEDIDOS,PS.ITEM_ID,PS.ID_COLOR,PS.ID_PO_SUMMARY FROM PO_SUMMARY PS,PEDIDO P,ITEM_DESCRIPTION ITD WHERE P.ID_PEDIDO=PS.ID_PEDIDOS AND P.ID_STATUS!=6 AND P.ID_STATUS!=7  " +
                    " AND P.PO LIKE '%" + busqueda + "%'";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read())
                {
                    pedido_staging ps = new pedido_staging();
                    ps.id_summary = Convert.ToInt32(leerFilas["ID_PO_SUMMARY"]);
                    ps.id_pedido = Convert.ToInt32(leerFilas["ID_PEDIDOS"]);
                    ps.id_estilo = Convert.ToInt32(leerFilas["ITEM_ID"]);
                    ps.color = consultas.obtener_descripcion_color_id(Convert.ToString(leerFilas["ID_COLOR"]));
                    ps.descripcion = consultas.buscar_descripcion_estilo(ps.id_estilo);
                    /*ps.id_customer= Convert.ToInt32(leer["id_customer"]);
                    ps.id_customer_final= Convert.ToInt32(leer["id_customer_final"]);*/
                    ps.estilo_nombre = consultas.buscar_descripcion_estilo(ps.id_estilo);
                    ps.po = consultas.obtener_po_id((ps.id_pedido).ToString());
                    //ps.total= Convert.ToInt32(leer["total"]);
                    ps.estilo = consultas.obtener_estilo(ps.id_estilo);
                    //ps.id_inventario = Convert.ToInt32(leer["id_inventario"]);
                    ps.id_inventario = 0;
                    //ps.fecha = (Convert.ToDateTime(leer["fecha"])).ToString("MMM dd yyyy");
                    ps.tipo_stag = Convert.ToString(leerFilas["tipo"]);
                    lista.Add(ps);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }



        public List<Talla_staging> obtener_cantidades_tallas_estilo(int posummary){
            List<Talla_staging> lista = new List<Talla_staging>();
             Link conn = new Link() ;
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT CIS.TALLA,IZ.TALLA_ITEM,IZ.CANTIDAD,IZ.\"1RST_CALIDAD\",IZ.EXTRAS,IZ.EJEMPLOS FROM ITEM_SIZE IZ,CAT_ITEM_SIZE CIS WHERE IZ.ID_SUMMARY='" + posummary + "' " +
                    " AND IZ.TALLA_ITEM IS NOT NULL AND CIS.ID=IZ.TALLA_ITEM ORDER by cast(CIS.ORDEN AS int) ASC";//ORDER BY CIS.ORDEN+0 "; //,\"IZ.1RST_CALIDAD\"
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                    Talla_staging ts = new Talla_staging();
                    ts.id_talla = Convert.ToInt32(leerFilas["TALLA_ITEM"]);
                    ts.talla = consultas.obtener_size_id(Convert.ToString(leerFilas["TALLA_ITEM"]));
                    ts.total = Convert.ToInt32(leerFilas["CANTIDAD"]);// + Convert.ToInt32(leerFilas["EXTRAS"]) + Convert.ToInt32(leerFilas["EJEMPLOS"]);
                    lista.Add(ts);
                }
                leerFilas.Close();
            }
            finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }

        public List<recibos_item> obtener_lista_recibos_staging(int summary, List<Talla_staging> tallas){
            DatosTransferencias dt = new DatosTransferencias();
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<recibos_item> lista = new List<recibos_item>();
            foreach(Talla_staging t in tallas) {
                int inventario = buscar_inventario_stagging(summary,t.id_talla);
                int total = 0;
                 Link conn = new Link() ;
                try{
                    SqlCommand comando = new SqlCommand();
                    SqlDataReader leerFilas = null;
                    comando.Connection = conn.AbrirConexion();
                    //comando.CommandText = "SELECT total FROM recibos_items WHERE id_inventario='" + inventario + "' ";                    
                    comando.CommandText = "SELECT total FROM inventario WHERE  id_summary='" + summary + "' and id_size='" + t.id_talla + "'";
                    leerFilas = comando.ExecuteReader();
                    while (leerFilas.Read()){
                        total += Convert.ToInt32(leerFilas["total"]);
                    }leerFilas.Close();
                }finally { conn.CerrarConexion(); conn.Dispose(); }
                Inventario i = dt.consultar_item(inventario);
                recibos_item ri = new recibos_item();
                ri.total = total;
                ri.id_talla = t.id_talla;
                ri.porcentaje = consultas.obtener_fabric_percent_id(Convert.ToString(i.id_fabric_percent));
                ri.pais = consultas.obtener_pais_id(Convert.ToString(i.id_pais));
                               
                lista.Add(ri);
            }
            return lista;
        }
        /*public string obtener_cantidades_estilo_staging(int summary){
            List<stag_conteo> lista = new List<stag_conteo>();            
             Link conn = new Link() ;
            try{
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT fecha,id_staging,total FROM staging WHERE id_summary='" + summary + "' ";
                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read()){
                   
                }leerFilas.Close();
            }finally { conn.CerrarConexion(); conn.Dispose(); }
            return lista;
        }*/

        public int buscar_inventario_stagging(int summary,int talla){
            int temporal = 0;
             Link con_u_r_i = new Link() ;
            try{
                SqlCommand com_u_r_i = new SqlCommand();
                SqlDataReader leer_u_r_i = null;
                com_u_r_i.Connection = con_u_r_i.AbrirConexion();
                com_u_r_i.CommandText = "SELECT id_inventario FROM inventario WHERE id_summary='"+summary+"' AND id_size='"+talla+"' and id_categoria_inventario=1  ";
                leer_u_r_i = com_u_r_i.ExecuteReader();
                while (leer_u_r_i.Read()){
                    temporal = Convert.ToInt32(leer_u_r_i["id_inventario"]);
                }leer_u_r_i.Close();
            }finally{con_u_r_i.CerrarConexion();con_u_r_i.Dispose();}
            return temporal;
        }
        public recibos_item buscar_recibo_stag(int inventario){
            recibos_item r = new recibos_item();
              Link con_u_r_i = new Link() ;
            try{
                SqlCommand com_u_r_i = new SqlCommand();
                SqlDataReader leer_u_r_i = null;
                com_u_r_i.Connection = con_u_r_i.AbrirConexion();
                com_u_r_i.CommandText = "SELECT TOP 1 id_recibo_item,id_recibo,total FROM recibos_items " +
                    " WHERE id_inventario='" + inventario + "' ORDER BY id_recibo_item DESC  ";
                leer_u_r_i = com_u_r_i.ExecuteReader();
                while (leer_u_r_i.Read()){
                    r.id_recibo= Convert.ToInt32(leer_u_r_i["id_recibo"]);
                    r.id_recibo_item= Convert.ToInt32(leer_u_r_i["id_recibo_item"]);
                    r.total = Convert.ToInt32(leer_u_r_i["total"]);
                }leer_u_r_i.Close();
            }finally { con_u_r_i.CerrarConexion(); con_u_r_i.Dispose(); }
            return r;
        }
        public void actualizar_cantidad_inventario(int inventario,int total_stag,int total_anterior){
             Link con_r = new Link() ;
            try{
                SqlCommand com_r = new SqlCommand();
                com_r.Connection = con_r.AbrirConexion();
                com_r.CommandText = "UPDATE inventario SET total=(total-"+total_anterior+")+"+total_stag+" WHERE id_inventario="+inventario+" ";                   
                com_r.ExecuteNonQuery();
            }finally{ con_r.CerrarConexion(); con_r.Dispose(); }
        }
        public void actualizar_cantidad_recibos_item(int recibo_item, int total_stag, int total_anterior){
             Link con_r = new Link() ;
            try{
                SqlCommand com_r = new SqlCommand();
                com_r.Connection = con_r.AbrirConexion();
                com_r.CommandText = "UPDATE recibos_items SET total=(total-" + total_anterior + ")+" + total_stag + " WHERE id_recibo_item=" + recibo_item + " ";
                com_r.ExecuteNonQuery();
            }finally { con_r.CerrarConexion(); con_r.Dispose(); }
        }

        public void actualizar_cantidad_recibos(int recibo){
             Link con_r = new Link() ;
            try{
                SqlCommand com_r = new SqlCommand();
                com_r.Connection = con_r.AbrirConexion();
                com_r.CommandText = "UPDATE recibos SET total=(SELECT SUM(total) FROM recibos_items WHERE id_recibo="+recibo+" ) WHERE id_recibo=" + recibo + " ";
                com_r.ExecuteNonQuery();
            }
            finally { con_r.CerrarConexion(); con_r.Dispose(); }
        }

        /****************************************************************************************************************************************/
        public List<stag_conteo> obtener_lista_staging_summary(int summary){
            List<stag_conteo> lista = new List<stag_conteo>();
             Link con = new Link() ;
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_staging,id_pedido,id_estilo,id_summary,total,id_usuario_captura,comentarios,fecha FROM " +
                    " staging WHERE id_summary='" + summary + "'" ;
                leer = com.ExecuteReader();
                while (leer.Read()){
                    stag_conteo s = new stag_conteo();
                    s.id_staging = Convert.ToInt32(leer["id_staging"]);
                    s.fecha = (Convert.ToDateTime(leer["fecha"])).ToString("MMM dd yyyy");
                    s.usuario = consultas.buscar_nombre_usuario(Convert.ToString(leer["id_usuario_captura"]));
                    s.total = Convert.ToInt32(leer["total"]);                   
                    s.id_pedido = Convert.ToInt32(leer["id_pedido"]);                   
                    s.id_summary = Convert.ToInt32(leer["id_summary"]);                   
                    s.id_estilo = Convert.ToInt32(leer["id_estilo"]);                   
                    s.lista_staging = obtener_lista_items_customer_staging(s.id_staging);
                    s.observaciones = Convert.ToString(leer["comentarios"]);
                    lista.Add(s);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public List<Talla_staging> obtener_lista_items_customer_staging(int id_staging){
            List<Talla_staging> lista = new List<Talla_staging>();
             Link con = new Link() ;
            try{
                SqlCommand com = new SqlCommand();
                SqlDataReader leer = null;
                com.Connection = con.AbrirConexion();
                com.CommandText = "SELECT id_staging_count,id_talla,total,id_pais,id_color,id_porcentaje,id_empleado FROM staging_count  " +
                    " WHERE id_staging='" + id_staging + "'  ";
                leer = com.ExecuteReader();
                while (leer.Read()){
                    Talla_staging ts = new Talla_staging();
                    ts.id_staging_count= Convert.ToInt32(leer["id_staging_count"]);
                    ts.total = Convert.ToInt32(leer["total"]);
                    ts.id_talla = Convert.ToInt32(leer["id_talla"]);
                    ts.talla = consultas.obtener_size_id(Convert.ToString(leer["id_talla"]));
                    ts.id_pais = Convert.ToInt32(leer["id_pais"]);
                    ts.pais = consultas.obtener_pais_id(Convert.ToString(leer["id_pais"]));
                    ts.id_color = Convert.ToInt32(leer["id_color"]);
                    ts.color = consultas.obtener_descripcion_color_id(Convert.ToString(leer["id_color"]));
                    ts.id_porcentaje= Convert.ToInt32(leer["id_porcentaje"]);
                    ts.porcentaje = consultas.obtener_fabric_percent_id(Convert.ToString(leer["id_porcentaje"]));
                    ts.empleado= Convert.ToString(leer["id_empleado"]);
                    lista.Add(ts);
                }leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
        }
        public void eliminar_conteos_staging(int staging) {
             Link con_r = new Link() ;
            try{
                SqlCommand com_r = new SqlCommand();
                com_r.Connection = con_r.AbrirConexion();
                com_r.CommandText = "DELETE FROM staging_count WHERE id_staging=" + staging + " ";
                com_r.ExecuteNonQuery();
            }finally { con_r.CerrarConexion(); con_r.Dispose(); }
        }
        public void eliminar_staging(int staging)
        {
             Link con_r = new Link() ;
            try
            {
                SqlCommand com_r = new SqlCommand();
                com_r.Connection = con_r.AbrirConexion();
                com_r.CommandText = "DELETE FROM staging WHERE id_staging=" + staging + " ";
                com_r.ExecuteNonQuery();
            }
            finally { con_r.CerrarConexion(); con_r.Dispose(); }
        }
        public void editar_staging(int staging,int total,string comentario){
             Link con_r = new Link() ;
            try{
                SqlCommand com_r = new SqlCommand();
                com_r.Connection = con_r.AbrirConexion();
                com_r.CommandText = "UPDATE staging SET total='"+total+"',comentarios='"+comentario+"'  WHERE id_staging=" + staging + " ";
                com_r.ExecuteNonQuery();
            }finally { con_r.CerrarConexion(); con_r.Dispose(); }
        }

         public List<pedido_staging> obtener_informacion_pedido_summary(int pedido,int estilo,int summary) {
            List<pedido_staging> lista = new List<pedido_staging>();
             Link con = new Link() ;
            try{
               SqlCommand com = new SqlCommand();
               SqlDataReader leer = null;
               com.Connection = con.AbrirConexion();
               com.CommandText = "SELECT P.DATE_CANCEL,P.TOTAL_UNITS,P.PO,P.VPO,PS.ID_GENDER,PS.ID_COLOR,PS.ID_TELA,PS.ID_PRODUCT_TYPE,PS.QTY FROM PEDIDO P, PO_SUMMARY PS WHERE " +
                    " P.ID_PEDIDO='" + pedido + "' AND PS.ID_PO_SUMMARY='"+summary+"' and PS.ID_PEDIDOS=P.ID_PEDIDO ";
               leer = com.ExecuteReader();
               while (leer.Read()){
                    pedido_staging p = new pedido_staging();
                    p.po = Convert.ToString(leer["PO"]);
                    p.vpo = Convert.ToString(leer["VPO"]);
                    p.total = Convert.ToInt32(leer["TOTAL_UNITS"]);
                    p.id_summary = summary;
                    p.id_pedido = pedido;
                    p.id_estilo = estilo;
                    p.total_estilo = Convert.ToInt32(leer["QTY"]);
                    p.fecha_cancelacion = (Convert.ToDateTime(leer["DATE_CANCEL"])).ToString("MMM dd yyyy");
                    p.estilo = consultas.obtener_estilo(estilo);
                    p.descripcion = consultas.buscar_descripcion_estilo(estilo);
                    p.color = consultas.obtener_descripcion_color_id(Convert.ToString(leer["ID_COLOR"]));
                    p.genero = consultas.obtener_sigla_genero(Convert.ToString(leer["ID_GENDER"]));
                    p.tela = consultas.obtener_sigla_fabric(Convert.ToString(leer["ID_TELA"]));
                    p.manga = consultas.obtener_sigla_product_type(Convert.ToString(leer["ID_PRODUCT_TYPE"]));
                    lista.Add(p);
               }
                leer.Close();
            }finally { con.CerrarConexion(); con.Dispose(); }
            return lista;
       }
        public string obtener_fecha_staging(string id){
            string temporal = "";
             Link con_u_r_i = new Link() ;
            try{
                SqlCommand com_u_r_i = new SqlCommand();
                SqlDataReader leer_u_r_i = null;
                com_u_r_i.Connection = con_u_r_i.AbrirConexion();
                com_u_r_i.CommandText = "SELECT fecha FROM staging WHERE id_staging='" + id + "'  ";
                leer_u_r_i = com_u_r_i.ExecuteReader();
                while (leer_u_r_i.Read()){
                    temporal = (Convert.ToDateTime(leer_u_r_i["fecha"])).ToString("yyyy-MM-dd HH:mm:ss");
                }leer_u_r_i.Close();
            }finally { con_u_r_i.CerrarConexion(); con_u_r_i.Dispose(); }
            return temporal;
        }


        public List<Pedido_customer> obtener_pedidos_reportes(string busqueda){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<Pedido_customer> lista = new List<Pedido_customer>();
            string query = "";
            if (busqueda == "0"){
                query = "select top 30 ID_PEDIDO,PO,VPO,CUSTOMER,CUSTOMER_FINAL,DATE_CANCEL,DATE_ORDER,TOTAL_UNITS,ID_STATUS " +
                    " from PEDIDO where ID_STATUS!=6 and ID_STATUS!=7 AND ID_STATUS!=5  order by ID_PEDIDO DESC";
            }else{
                query = "select top 30 ID_PEDIDO,PO,VPO,CUSTOMER,CUSTOMER_FINAL,DATE_CANCEL,DATE_ORDER,TOTAL_UNITS,ID_STATUS " +
                    "from PEDIDO where PO like'%" + busqueda + "%' or VPO like'%" + busqueda + "%' AND ID_STATUS!=5 AND ID_STATUS!=6 AND ID_STATUS!=7  order by ID_PEDIDO DESC  ";
            }
             Link con_ltd = new Link() ;
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = query;
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Pedido_customer p = new Pedido_customer();
                    p.id_pedido = Convert.ToInt32(leer_ltd["ID_PEDIDO"]);
                    p.pedido = (Convert.ToString(leer_ltd["PO"])).Trim();
                    p.vpo = Convert.ToString(leer_ltd["VPO"]);
                    p.id_customer = Convert.ToInt32(leer_ltd["CUSTOMER"]);
                    p.customer = consultas.obtener_customer_id(Convert.ToString(leer_ltd["CUSTOMER"]));
                    p.id_customer_final = Convert.ToInt32(leer_ltd["CUSTOMER_FINAL"]);
                    p.customer_final = consultas.obtener_customer_final_id(Convert.ToString(leer_ltd["CUSTOMER_FINAL"]));
                    p.date_cancel = (Convert.ToDateTime(leer_ltd["DATE_CANCEL"])).ToString("MMM dd yyyy");
                    p.date_order = (Convert.ToDateTime(leer_ltd["DATE_ORDER"])).ToString("MMM dd yyyy");
                    p.total = Convert.ToInt32(leer_ltd["TOTAL_UNITS"]);
                    p.lista_estilos = obtener_estilos_customer((p.id_pedido).ToString());
                    lista.Add(p);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }
        public List<Estilo_customer> obtener_estilos_customer(string pedido){
            FuncionesInventarioGeneral consultas = new FuncionesInventarioGeneral();
            List<Estilo_customer> lista = new List<Estilo_customer>();
             Link con_ltd = new Link() ;
            try{
                SqlCommand com_ltd = new SqlCommand();
                SqlDataReader leer_ltd = null;
                com_ltd.Connection = con_ltd.AbrirConexion();
                com_ltd.CommandText = "select ID_PO_SUMMARY,ITEM_ID,ID_COLOR,QTY,ID_GENDER from PO_SUMMARY where ID_PEDIDOS=" + pedido + " ";
                leer_ltd = com_ltd.ExecuteReader();
                while (leer_ltd.Read()){
                    Estilo_customer e = new Estilo_customer();
                    e.id_estilo = Convert.ToInt32(leer_ltd["ITEM_ID"]);
                    e.id_summary = Convert.ToInt32(leer_ltd["ID_PO_SUMMARY"]);
                    e.id_genero = Convert.ToInt32(leer_ltd["ID_GENDER"]);
                    e.total = Convert.ToInt32(leer_ltd["QTY"]);
                    e.id_color = Convert.ToInt32(leer_ltd["ID_COLOR"]);
                    e.id_genero = Convert.ToInt32(leer_ltd["ID_GENDER"]);
                    e.estilo = (consultas.obtener_estilo(e.id_estilo)).Trim();
                    e.descripcion = (consultas.buscar_descripcion_estilo(e.id_estilo)).Trim();
                    e.color = (consultas.obtener_color_id(Convert.ToString(e.id_color)) + " - " + consultas.obtener_descripcion_color_id(Convert.ToString(e.id_color))).Trim();
                    e.genero = consultas.obtener_genero_id(Convert.ToString(e.id_genero));
                    lista.Add(e);
                }leer_ltd.Close();
            }finally { con_ltd.CerrarConexion(); con_ltd.Dispose(); }
            return lista;
        }

        public List<int> buscar_lista_pedidos_estilos(string[] lista_estilos){
            List<int> lista = new List<int>();
            for(int i = 1; i<lista_estilos.Length; i++) {
                 Link con_u_r_i = new Link() ;
                try{
                    SqlCommand com_u_r_i = new SqlCommand();
                    SqlDataReader leer_u_r_i = null;
                    com_u_r_i.Connection = con_u_r_i.AbrirConexion();
                    com_u_r_i.CommandText = "SELECT ID_PEDIDOS FROM PO_SUMMARY WHERE ID_PO_SUMMARY='" + lista_estilos[i] + "'  ";
                    leer_u_r_i = com_u_r_i.ExecuteReader();
                    while (leer_u_r_i.Read()){
                        lista.Add((Convert.ToInt32(leer_u_r_i["ID_PEDIDOS"])));
                    }leer_u_r_i.Close();
                }finally { con_u_r_i.CerrarConexion(); con_u_r_i.Dispose(); }
            }
            lista = lista.Distinct().ToList();
            return lista;
        }




















































































    }//No
}//No