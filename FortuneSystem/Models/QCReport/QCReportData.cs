using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models.QCReport
{
	public class QCReportData
	{
		//Permite crear un nuevo reporte
		public void AgregarReporte(QCReportGeneral reporte)
		{
			Conexion conn = new Conexion();
			try
			{
				SqlCommand comando = new SqlCommand
				{
					Connection = conn.AbrirConexion(),
					CommandText = "AgregarReporte",
					CommandType = CommandType.StoredProcedure
				};

				comando.Parameters.AddWithValue("@reporte", reporte.ReporteG);
				comando.Parameters.AddWithValue("@sacador", reporte.Sacador);
				comando.Parameters.AddWithValue("@cachador", reporte.Cachador);
				comando.Parameters.AddWithValue("@metedor", reporte.Metedor);
				comando.Parameters.AddWithValue("@inspector", reporte.QCInspector);
				comando.Parameters.AddWithValue("@AQL", reporte.AQL);
				comando.Parameters.AddWithValue("@turno", reporte.Turno);
				comando.Parameters.AddWithValue("@idUsr", reporte.IdUsuario);
				comando.Parameters.AddWithValue("@idMaq", reporte.IdMaquina);
				comando.Parameters.AddWithValue("@idSum", reporte.IdSummary);
				comando.Parameters.AddWithValue("@fecha", reporte.Fecha);
				

				comando.ExecuteNonQuery();
			}
			finally
			{
				conn.CerrarConexion();
				conn.Dispose();
			}

		}

		//Permite actualizar la información de un Reporte
		public void ActualizarReporte(QCReportGeneral reporte)
		{
			Conexion conn = new Conexion();
			try
			{
				SqlCommand comando = new SqlCommand
				{
					Connection = conn.AbrirConexion(),
					CommandText = "ActualizarReporte",
					CommandType = CommandType.StoredProcedure
				};

				comando.Parameters.AddWithValue("@idreporte", reporte.IdQCReport);
				comando.Parameters.AddWithValue("@reporte", reporte.ReporteG);
				comando.Parameters.AddWithValue("@sacador", reporte.Sacador);
				comando.Parameters.AddWithValue("@cachador", reporte.Cachador);
				comando.Parameters.AddWithValue("@metedor", reporte.Metedor);
				comando.Parameters.AddWithValue("@inspector", reporte.QCInspector);
				comando.Parameters.AddWithValue("@AQL", reporte.AQL);
				comando.Parameters.AddWithValue("@turno", reporte.Turno);
				comando.Parameters.AddWithValue("@idMaq", reporte.IdMaquina);
				comando.Parameters.AddWithValue("@fecha", reporte.Fecha);


				comando.ExecuteNonQuery();
			}
			finally
			{
				conn.CerrarConexion();
				conn.Dispose();
			}

		}

		public int Obtener_Utlimo_IdReporte()
		{
			SqlCommand cmd = new SqlCommand();
			SqlDataReader reader;
			Conexion conex = new Conexion();
			try
			{
				cmd.Connection = conex.AbrirConexion();
				cmd.CommandText = "SELECT Id_QC_Report FROM QC_REPORT WHERE Id_QC_Report = (SELECT MAX(Id_QC_Report) FROM QC_REPORT) ";
				cmd.CommandType = CommandType.Text;
				reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					return Convert.ToInt32(reader["Id_QC_Report"]);
				}

			}
			finally
			{
				conex.CerrarConexion();
				conex.Dispose();
			}
			return 0;
		}

		//Obtener informacion Report
		public QCReportGeneral ObtenerInformacionReportT1(int? idSummary, int? turno)
		{
			Conexion conexion = new Conexion();
			QCReportGeneral reporte = new QCReportGeneral();
			try
			{
				SqlCommand com = new SqlCommand();
				SqlDataReader leerF = null;

				com.Connection = conexion.AbrirConexion();
				com.CommandText = "select * from QC_REPORT where Id_Summary= '" + idSummary + "'and Turn= '"+turno+"' ";
				com.CommandType = CommandType.Text;

				leerF = com.ExecuteReader();
				while (leerF.Read())
				{
					reporte.IdQCReport = Convert.ToInt32(leerF["Id_QC_Report"]);
					reporte.ReporteG = leerF["Inf_Report"].ToString();
					reporte.Sacador = leerF["Sacador"].ToString();
					reporte.Cachador = leerF["Cachador"].ToString();
					reporte.Metedor = leerF["Metedor"].ToString();
					reporte.QCInspector = leerF["QC_Inspector"].ToString();
					reporte.AQL = Convert.ToBoolean(leerF["AQL"]);
					reporte.Turno = Convert.ToInt32(leerF["Turn"]);
					reporte.IdUsuario = Convert.ToInt32(leerF["Id_Usuario"]);
					reporte.IdSummary = Convert.ToInt32(leerF["Id_Summary"]);
					reporte.IdMaquina = Convert.ToInt32(leerF["Id_Maquina"]);
					reporte.Fecha = Convert.ToDateTime(leerF["FechaRegistro"]);

				}
				leerF.Close();
			}
			finally
			{
				conexion.CerrarConexion();
				conexion.Dispose();
			}

			return reporte;

		}
	}
}