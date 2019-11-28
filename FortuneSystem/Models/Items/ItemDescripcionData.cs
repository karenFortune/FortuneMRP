using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models.Items
{
    public class ItemDescripcionData
    {
        //Muestra la lista de Items
        public IEnumerable<ItemDescripcion> ListaItems()
        {
            Conexion conn = new Conexion();
            List<ItemDescripcion> listItems = new List<ItemDescripcion>();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;               
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "Listar_Item_Desc";
                comando.CommandType = CommandType.StoredProcedure;
                leerFilas = comando.ExecuteReader();

                while (leerFilas.Read())
                {
                    ItemDescripcion items = new ItemDescripcion();
                    items.ItemId = Convert.ToInt32(leerFilas["ITEM_ID"]);
                    items.ItemEstilo = leerFilas["ITEM_STYLE"].ToString().TrimEnd();
                    items.Descripcion = leerFilas["DESCRIPTION"].ToString().TrimEnd();
                    listItems.Add(items);

                }
                leerFilas.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }     
               

            return listItems;
        }


        //Permite crear un nuevo Item descripcion
        public void AgregarItemDescripcion(ItemDescripcion itemDesc)
        {
            Conexion conn = new Conexion();
            try
            {
                SqlCommand comando = new SqlCommand();

                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "Agregar_Item_Desc";
                comando.CommandType = CommandType.StoredProcedure;

                comando.Parameters.AddWithValue("@Style", itemDesc.ItemEstilo);
                comando.Parameters.AddWithValue("@Descripcion", itemDesc.Descripcion);

                comando.ExecuteNonQuery();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }         

        }

		//Permite actualiza la informacion de un estilo 
		public void ActualizarItemDesc(ItemDescripcion itemDesc)
		{
			Conexion conn = new Conexion();
			try
			{
				SqlCommand comando = new SqlCommand();

				comando.Connection = conn.AbrirConexion();
				comando.CommandText = "Actualizar_Items_Desc";
				comando.CommandType = CommandType.StoredProcedure;
				comando.Parameters.AddWithValue("@Id", itemDesc.ItemId);
				comando.Parameters.AddWithValue("@Item", itemDesc.ItemEstilo);
				comando.Parameters.AddWithValue("@ItemDesc", itemDesc.Descripcion);
				comando.ExecuteNonQuery();
			}
			finally
			{
				conn.CerrarConexion();
				conn.Dispose();
			}
		}

		//Permite eliminar la informacion de un estilo
		public void EliminarItemDesc(int? id)
		{
			Conexion conn = new Conexion();
			try
			{
				SqlCommand comando = new SqlCommand();
				comando.Connection = conn.AbrirConexion();
				comando.CommandText = "EliminarItemDesc";
				comando.CommandType = CommandType.StoredProcedure;
				comando.Parameters.AddWithValue("@Id", id);
				comando.ExecuteNonQuery();
			}
			finally
			{
				conn.CerrarConexion();
				conn.Dispose();
			}
		}

		//Permite consultar los detalles de un Item Desc
		public ItemDescripcion ConsultarListaItemDesc(int? id)
        {

            Conexion conex = new Conexion();
            ItemDescripcion itemDesc = new ItemDescripcion();
            try
            {
                SqlCommand coma = new SqlCommand();
                SqlDataReader leer = null;               

                coma.Connection = conex.AbrirConexion();
                coma.CommandText = "Listar_EstiloDesc_Por_Id";
                coma.CommandType = CommandType.StoredProcedure;
                coma.Parameters.AddWithValue("@Id", id);

                leer = coma.ExecuteReader();
                while (leer.Read())
                {
                    string descripcion = leer["DESCRIPTION"].ToString();
                    itemDesc.ItemId = Convert.ToInt32(leer["ITEM_ID"]);
                    itemDesc.ItemEstilo = leer["ITEM_STYLE"].ToString().TrimEnd();
                    itemDesc.Descripcion = descripcion.TrimEnd(' ');

                }
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
    
            return itemDesc;

        }
        //Obtener el id de un estilo por nombre
        public int ObtenerIdEstilo(string estilo)
        {
            int idEstilo = 0;
            Conexion conex = new Conexion();
            try
            {
                SqlCommand coman = new SqlCommand();
                SqlDataReader leerF = null;
                coman.Connection = conex.AbrirConexion();
                coman.CommandText = "SELECT ITEM_ID FROM ITEM_DESCRIPTION " +
                                     "WHERE ITEM_STYLE='" + estilo + "' ";
                leerF = coman.ExecuteReader();
                while (leerF.Read())
                {
                    idEstilo += Convert.ToInt32(leerF["ITEM_ID"]);
                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            
            return idEstilo;
        }
        //Obtener el nombre de un estilo por id
        public string ObtenerEstiloPorId(int?  idEstilo)
        {
            string Estilo = "";
            Conexion conex = new Conexion();
            try
            {
                SqlCommand coman = new SqlCommand();
                SqlDataReader leerF = null;
                coman.Connection = conex.AbrirConexion();
                coman.CommandText = "SELECT ITEM_STYLE FROM ITEM_DESCRIPTION " +
                                     "WHERE ITEM_ID='" + idEstilo + "' ";
                leerF = coman.ExecuteReader();
                while (leerF.Read())
                {
                    Estilo += leerF["ITEM_STYLE"].ToString();
                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

            return Estilo;
        }


        public string Verificar_Item_CD(string cadena)
        {
            Conexion conn = new Conexion();
            string texto = null;
            try
            {
                SqlCommand comando = new SqlCommand();                     
                cadena = cadena.TrimEnd();
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "VerificarItem";
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@Cadena", cadena);
                texto = (string)comando.ExecuteScalar();
                comando.Parameters.Clear();
                if (texto != null) texto = texto.TrimEnd();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }          
            
            return texto;
        }

        //Obtener el id de Estilo por Summary
        public int ObtenerIdEstiloPorsummary(int? idEstilo)
        {
            int numEstilo = 0;
            Conexion conex = new Conexion();
            try
            {
                SqlCommand coman = new SqlCommand();
                SqlDataReader leerF = null;
                coman.Connection = conex.AbrirConexion();
                coman.CommandText = "SELECT ITEM_ID FROM PO_SUMMARY " +
                                     "WHERE ID_PO_SUMMARY='" + idEstilo + "' ";
                leerF = coman.ExecuteReader();
                while (leerF.Read())
                {
                    numEstilo += Convert.ToInt32(leerF["ITEM_ID"]);
                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

            return numEstilo;
        }

        //Obtener el el nombre del estilo por Id
        public string ObtenerNombreEstiloId(string estilo)
        {
            string estiloDes = "";
            Conexion conex = new Conexion();
            try
            {
                SqlCommand coman = new SqlCommand();
                SqlDataReader leerF = null;
                coman.Connection = conex.AbrirConexion();
                coman.CommandText = "SELECT I.DESCRIPTION FROM ITEM_DESCRIPTION I " +
                        "where I.ITEM_STYLE='" + estilo + "'";
                leerF = coman.ExecuteReader();
                while (leerF.Read())
                {
                    estiloDes += leerF["DESCRIPTION"].ToString();
                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return estiloDes;
        }

        //Obtener id Summary por Id Estilo
        public int ObtenerIdSummaryPorIdEstilo(int? idEstilo, int? idPedido)
        {
            int numEstilo = 0;
            Conexion conex = new Conexion();
            try
            {
                SqlCommand coman = new SqlCommand();
                SqlDataReader leerF = null;
                coman.Connection = conex.AbrirConexion();
                coman.CommandText = "SELECT ID_PO_SUMMARY FROM PO_SUMMARY " +
                                     "WHERE ITEM_ID='" + idEstilo + "' AND ID_PEDIDOS='"+idPedido+"' ";
                leerF = coman.ExecuteReader();
                while (leerF.Read())
                {
                    numEstilo = Convert.ToInt32(leerF["ID_PO_SUMMARY"]);
                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

            return numEstilo;
        }

    }
}