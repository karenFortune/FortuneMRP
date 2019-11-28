using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models.ProductionPlan
{
	public class CatProdOverData
	{
		//Muestra la lista de hornos
		public IEnumerable<CatProdOver> ListaHornos()
		{
			Conexion conn = new Conexion();
			List<CatProdOver> ListaHornos = new List<CatProdOver>();
			try
			{
				SqlCommand comando = new SqlCommand();
				SqlDataReader leer = null;

				comando.Connection = conn.AbrirConexion();
				comando.CommandText = "SELECT * FROM PROD_CAT_OVER";
				comando.CommandType = CommandType.Text;
				leer = comando.ExecuteReader();

				while (leer.Read())
				{
					CatProdOver hornos = new CatProdOver()
					{
						IdProdCatOver = Convert.ToInt32(leer["Id_Prod_Cat_Over"]),
						Horno = leer["Name"].ToString()
					};

					ListaHornos.Add(hornos);
				}
				leer.Close();
			}
			finally
			{
				conn.CerrarConexion();
				conn.Dispose();
			}
			return ListaHornos;
		}
	}
}