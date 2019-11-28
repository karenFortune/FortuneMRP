using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models.QCReport
{
	public class QCPruebaLavadoData
	{
		//Permite agregar las pruebas de lavado para un reporte
		public void AgregarPruebaLavado(QCPruebaLavado pruebaL)
		{
			Conexion conn = new Conexion();
			try
			{
				SqlCommand comando = new SqlCommand
				{
					Connection = conn.AbrirConexion(),
					CommandText = "AgregarPruebaLavado",
					CommandType = CommandType.StoredProcedure
				};

				comando.Parameters.AddWithValue("@hora", pruebaL.HoraLavado);
				comando.Parameters.AddWithValue("@talla", pruebaL.IdTalla);
				comando.Parameters.AddWithValue("@result", pruebaL.Results);
				comando.Parameters.AddWithValue("@idRep", pruebaL.IdQCReport);
	


				comando.ExecuteNonQuery();
			}
			finally
			{
				conn.CerrarConexion();
				conn.Dispose();
			}

		}

		//Permite actualizar las pruebas de lavado para un reporte
		public void ActualizarPruebaLavado(QCPruebaLavado pruebaL)
		{
			Conexion conn = new Conexion();
			try
			{
				SqlCommand comando = new SqlCommand
				{
					Connection = conn.AbrirConexion(),
					CommandText = "ActualizarPruebaLavado",
					CommandType = CommandType.StoredProcedure
				};
				
			    comando.Parameters.AddWithValue("@idPrueba", pruebaL.IdQCPruebasLavados);
				comando.Parameters.AddWithValue("@hora", pruebaL.HoraLavado);
				comando.Parameters.AddWithValue("@talla", pruebaL.IdTalla);
				comando.Parameters.AddWithValue("@result", pruebaL.Results);


				comando.ExecuteNonQuery();
			}
			finally
			{
				conn.CerrarConexion();
				conn.Dispose();
			}

		}

		//Muestra la lista de lavados
		public IEnumerable<QCPruebaLavado> ListaPruebasLavado(int? id)
		{
			Conexion conn = new Conexion();
			List<QCPruebaLavado> listLavados = new List<QCPruebaLavado>();
			try
			{
				SqlCommand comando = new SqlCommand();
				SqlDataReader leer = null;
				comando.Connection = conn.AbrirConexion();
				comando.CommandText = "select qpl.Id_QC_Pruebas_Lavados,qpl.Hora_Lavado,S.TALLA,QPL.Id_Talla,QPL.Results from QC_PRUEBAS_LAVADOS as qpl " +
									  "INNER JOIN CAT_ITEM_SIZE S ON S.ID=qpl.Id_Talla " +
									  "where Id_QC_Report='" +id+ "' ORDER by cast(S.ORDEN AS int) ASC";
				leer = comando.ExecuteReader();
				while (leer.Read())
				{
					QCPruebaLavado pruebaL = new QCPruebaLavado()
					{
						IdQCPruebasLavados = Convert.ToInt32(leer["Id_QC_Pruebas_Lavados"]),
						HoraLavado = Convert.ToDateTime(leer["Hora_Lavado"]),
						IdTalla = Convert.ToInt32(leer["Id_Talla"]),
						Results = Convert.ToInt32(leer["Results"]),
						Talla = leer["Talla"].ToString()
					};                    
                    pruebaL.Fecha = String.Format("{0:g}", pruebaL.HoraLavado);
                    listLavados.Add(pruebaL);
				}
				leer.Close();
			}
			finally
			{
				conn.CerrarConexion();
				conn.Dispose();
			}

			return listLavados;
		}
	}
}