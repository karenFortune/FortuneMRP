using FortuneSystem.Models.Item;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models.QCReport
{
	public class QCMisPrintsData
	{
		//Permite agregar las cantidades de misprints estilo
		public void AgregarMisPrints(QCMisPrints datoMP)
		{
			Conexion conn = new Conexion();
			try
			{
				SqlCommand comando = new SqlCommand
				{
					Connection = conn.AbrirConexion(),
					CommandText = "AgregarMisPrints",
					CommandType = CommandType.StoredProcedure
				};

				comando.Parameters.AddWithValue("@fecha", datoMP.FechaRegistro);
				comando.Parameters.AddWithValue("@talla", datoMP.IdTalla);
				comando.Parameters.AddWithValue("@mp1", datoMP.MisPrint1st);
				comando.Parameters.AddWithValue("@mp2", datoMP.MisPrint2nd);
				comando.Parameters.AddWithValue("@rp1", datoMP.Repairs1st);
				comando.Parameters.AddWithValue("@rp2", datoMP.Repairs2nd);
				comando.Parameters.AddWithValue("@sp1", datoMP.Sprayed1st);
				comando.Parameters.AddWithValue("@sp2", datoMP.Sprayed2nd);
				comando.Parameters.AddWithValue("@d1", datoMP.Defects1st);
				comando.Parameters.AddWithValue("@d2", datoMP.Defects2nd);
				comando.Parameters.AddWithValue("@idSummary", datoMP.IdSummary);




				comando.ExecuteNonQuery();
			}
			finally
			{
				conn.CerrarConexion();
				conn.Dispose();
			}

		}

		//Permite actualizar las cantidades de misprints estilo
		public void ActualizarMisPrints(QCMisPrints datoMP)
		{
			Conexion conn = new Conexion();
			try
			{
				SqlCommand comando = new SqlCommand
				{
					Connection = conn.AbrirConexion(),
					CommandText = "ActualizarMisPrints",
					CommandType = CommandType.StoredProcedure
				};

				comando.Parameters.AddWithValue("@idSummary", datoMP.IdSummary);
				comando.Parameters.AddWithValue("@fecha", datoMP.FechaRegistro);
				comando.Parameters.AddWithValue("@talla", datoMP.IdTalla);
				comando.Parameters.AddWithValue("@mp1", datoMP.MisPrint1st);
				comando.Parameters.AddWithValue("@mp2", datoMP.MisPrint2nd);
				comando.Parameters.AddWithValue("@rp1", datoMP.Repairs1st);
				comando.Parameters.AddWithValue("@rp2", datoMP.Repairs2nd);
				comando.Parameters.AddWithValue("@sp1", datoMP.Sprayed1st);
				comando.Parameters.AddWithValue("@sp2", datoMP.Sprayed2nd);
				comando.Parameters.AddWithValue("@d1", datoMP.Defects1st);
				comando.Parameters.AddWithValue("@d2", datoMP.Defects2nd);


				comando.ExecuteNonQuery();
			}
			finally
			{
				conn.CerrarConexion();
				conn.Dispose();
			}

		}


		//Muestra la lista de misPrints registrados por id Summary
		public IEnumerable<QCMisPrints> ListaMPRegistrados(int? idSummary)
		{
			Conexion conex = new Conexion();
			List<QCMisPrints> listMP = new List<QCMisPrints>();
			try
			{
				SqlCommand c = new SqlCommand();
				SqlDataReader leerF = null;
				c.Connection = conex.AbrirConexion();
				c.CommandText = "select * from QC_MISPRINTS where Id_Summary='" + idSummary + "'  ";
				leerF = c.ExecuteReader();

				while (leerF.Read())
				{
					QCMisPrints misPrints = new QCMisPrints()
					{

						FechaRegistro = Convert.ToDateTime(leerF["Fecha_Registro"]),
						IdTalla = Convert.ToInt32(leerF["Id_Talla"]),
						MisPrint1st = Convert.ToInt32(leerF["MisPrint1st"]),
						MisPrint2nd = Convert.ToInt32(leerF["MisPrint2nd"]),
						Repairs1st = Convert.ToInt32(leerF["Repairs1st"]),
						Repairs2nd = Convert.ToInt32(leerF["Repairs2nd"]),
						Sprayed1st = Convert.ToInt32(leerF["Sprayed1st"]),
						Sprayed2nd = Convert.ToInt32(leerF["Sprayed2nd"]),
						Defects1st = Convert.ToInt32(leerF["Defects1st"]),
						Defects2nd = Convert.ToInt32(leerF["Defects2nd"])
					};

					listMP.Add(misPrints);
				}
				leerF.Close();
			}
			catch (Exception)
			{
				conex.CerrarConexion();
				conex.Dispose();
			}

			return listMP;
		}

        //Muestra la lista de tallas TOTAL Misprints1 de QC por estilos
        public IEnumerable<int> ListaTotalMisPrintsQC(List<int> listadoEstilos, int mes, string year)
        {
            ItemTallaData objTallas = new ItemTallaData();
            Conexion conn = new Conexion();

            List<int> listTallas = new List<int>();
            string valores = "";
            for (int v = 0; v < listadoEstilos.Count; v++)
            {
                if (v > 0)
                {
                    valores += "," + listadoEstilos[v];
                }
                else
                {
                    valores += listadoEstilos[v];
                }

            }
            string query = valores;
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT distinct cast(S.ORDEN AS int) as codigo, S.TALLA FROM QC_MISPRINTS T  " +
                    "INNER JOIN CAT_ITEM_SIZE S ON S.ID=T.ID_TALLA " +
                    "WHERE T.ID_SUMMARY in(" + query + ") AND MONTH(T.Fecha_Registro)='" + mes + "' AND YEAR(T.Fecha_Registro) ='" + year + "'";
                leer = comando.ExecuteReader();
                int totalPrinted = 0;
                while (leer.Read())
                {
                    QCMisPrints tallas = new QCMisPrints()
                    {

                        Talla = leer["TALLA"].ToString()

                    };
                    tallas.IdTalla = objTallas.ObtenerIdTallas(tallas.Talla);
                    totalPrinted = SumaTotalMisPrintsTallaQC(query, tallas.IdTalla);
                    listTallas.Add(totalPrinted);

                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

            return listTallas;
        }

        //Muestra la lista de suma de MisPrint1 tallas reporte QC
        public int SumaTotalMisPrintsTallaQC(string query, int idTalla)
        {
            Conexion conex = new Conexion();
            int suma = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;
                com.Connection = conex.AbrirConexion();
                com.CommandText = "SELECT MisPrint1st  FROM QC_MISPRINTS WHERE ID_SUMMARY in(" + query + ") AND ID_TALLA='" + idTalla + "' ";
                leerF = com.ExecuteReader();
                while (leerF.Read())
                {
                    suma += Convert.ToInt32(leerF["MisPrint1st"]);

                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

            return suma;
        }

        //Muestra la lista de tallas TOTAL Misprints2 de QC por estilos
        public IEnumerable<int> ListaTotalMisPrints2QC(List<int> listadoEstilos, int mes, string year)
        {
            ItemTallaData objTallas = new ItemTallaData();
            Conexion conn = new Conexion();

            List<int> listTallas = new List<int>();
            string valores = "";
            for (int v = 0; v < listadoEstilos.Count; v++)
            {
                if (v > 0)
                {
                    valores += "," + listadoEstilos[v];
                }
                else
                {
                    valores += listadoEstilos[v];
                }

            }
            string query = valores;
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT distinct cast(S.ORDEN AS int) as codigo, S.TALLA FROM QC_MISPRINTS T  " +
                    "INNER JOIN CAT_ITEM_SIZE S ON S.ID=T.ID_TALLA " +
                    "WHERE T.ID_SUMMARY in(" + query + ") AND MONTH(T.Fecha_Registro)='" + mes + "' AND YEAR(T.Fecha_Registro) ='" + year + "'";
                leer = comando.ExecuteReader();
                int totalPrinted = 0;
                while (leer.Read())
                {
                    QCMisPrints tallas = new QCMisPrints()
                    {

                        Talla = leer["TALLA"].ToString()

                    };
                    tallas.IdTalla = objTallas.ObtenerIdTallas(tallas.Talla);
                    totalPrinted = SumaTotalMisPrints2TallaQC(query, tallas.IdTalla);
                    listTallas.Add(totalPrinted);

                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

            return listTallas;
        }

        //Muestra la lista de suma de MisPrint2 tallas reporte QC
        public int SumaTotalMisPrints2TallaQC(string query, int idTalla)
        {
            Conexion conex = new Conexion();
            int suma = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;
                com.Connection = conex.AbrirConexion();
                com.CommandText = "SELECT MisPrint2nd  FROM QC_MISPRINTS WHERE ID_SUMMARY in(" + query + ") AND ID_TALLA='" + idTalla + "' ";
                leerF = com.ExecuteReader();
                while (leerF.Read())
                {
                    suma += Convert.ToInt32(leerF["MisPrint2nd"]);

                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

            return suma;
        }

        //Muestra la lista de tallas TOTAL Repairs1st de QC por estilos
        public IEnumerable<int> ListaTotalRepairs1stQC(List<int> listadoEstilos, int mes, string year)
        {
            ItemTallaData objTallas = new ItemTallaData();
            Conexion conn = new Conexion();

            List<int> listTallas = new List<int>();
            string valores = "";
            for (int v = 0; v < listadoEstilos.Count; v++)
            {
                if (v > 0)
                {
                    valores += "," + listadoEstilos[v];
                }
                else
                {
                    valores += listadoEstilos[v];
                }

            }
            string query = valores;
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT distinct cast(S.ORDEN AS int) as codigo, S.TALLA FROM QC_MISPRINTS T  " +
                    "INNER JOIN CAT_ITEM_SIZE S ON S.ID=T.ID_TALLA " +
                    "WHERE T.ID_SUMMARY in(" + query + ") AND MONTH(T.Fecha_Registro)='" + mes + "' AND YEAR(T.Fecha_Registro) ='" + year + "'";
                leer = comando.ExecuteReader();
                int totalPrinted = 0;
                while (leer.Read())
                {
                    QCMisPrints tallas = new QCMisPrints()
                    {

                        Talla = leer["TALLA"].ToString()

                    };
                    tallas.IdTalla = objTallas.ObtenerIdTallas(tallas.Talla);
                    totalPrinted = SumaTotalRepairs1stTallaQC(query, tallas.IdTalla);
                    listTallas.Add(totalPrinted);

                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

            return listTallas;
        }

        //Muestra la lista de suma de Repairs1st tallas reporte QC
        public int SumaTotalRepairs1stTallaQC(string query, int idTalla)
        {
            Conexion conex = new Conexion();
            int suma = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;
                com.Connection = conex.AbrirConexion();
                com.CommandText = "SELECT Repairs1st  FROM QC_MISPRINTS WHERE ID_SUMMARY in(" + query + ") AND ID_TALLA='" + idTalla + "' ";
                leerF = com.ExecuteReader();
                while (leerF.Read())
                {
                    suma += Convert.ToInt32(leerF["Repairs1st"]);

                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

            return suma;
        }

        //Muestra la lista de tallas TOTAL Repairs2nd de QC por estilos
        public IEnumerable<int> ListaTotalRepairs2ndQC(List<int> listadoEstilos, int mes, string year)
        {
            ItemTallaData objTallas = new ItemTallaData();
            Conexion conn = new Conexion();

            List<int> listTallas = new List<int>();
            string valores = "";
            for (int v = 0; v < listadoEstilos.Count; v++)
            {
                if (v > 0)
                {
                    valores += "," + listadoEstilos[v];
                }
                else
                {
                    valores += listadoEstilos[v];
                }

            }
            string query = valores;
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT distinct cast(S.ORDEN AS int) as codigo, S.TALLA FROM QC_MISPRINTS T  " +
                    "INNER JOIN CAT_ITEM_SIZE S ON S.ID=T.ID_TALLA " +
                    "WHERE T.ID_SUMMARY in(" + query + ") AND MONTH(T.Fecha_Registro)='" + mes + "' AND YEAR(T.Fecha_Registro) ='" + year + "'";
                leer = comando.ExecuteReader();
                int totalPrinted = 0;
                while (leer.Read())
                {
                    QCMisPrints tallas = new QCMisPrints()
                    {

                        Talla = leer["TALLA"].ToString()

                    };
                    tallas.IdTalla = objTallas.ObtenerIdTallas(tallas.Talla);
                    totalPrinted = SumaTotalRepairs2ndTallaQC(query, tallas.IdTalla);
                    listTallas.Add(totalPrinted);

                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

            return listTallas;
        }

        //Muestra la lista de suma de Repairs2nd tallas reporte QC
        public int SumaTotalRepairs2ndTallaQC(string query, int idTalla)
        {
            Conexion conex = new Conexion();
            int suma = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;
                com.Connection = conex.AbrirConexion();
                com.CommandText = "SELECT Repairs2nd  FROM QC_MISPRINTS WHERE ID_SUMMARY in(" + query + ") AND ID_TALLA='" + idTalla + "' ";
                leerF = com.ExecuteReader();
                while (leerF.Read())
                {
                    suma += Convert.ToInt32(leerF["Repairs2nd"]);

                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

            return suma;
        }


        //Muestra la lista de tallas TOTAL Sprayed1st de QC por estilos
        public IEnumerable<int> ListaTotalSprayed1stQC(List<int> listadoEstilos, int mes, string year)
        {
            ItemTallaData objTallas = new ItemTallaData();
            Conexion conn = new Conexion();

            List<int> listTallas = new List<int>();
            string valores = "";
            for (int v = 0; v < listadoEstilos.Count; v++)
            {
                if (v > 0)
                {
                    valores += "," + listadoEstilos[v];
                }
                else
                {
                    valores += listadoEstilos[v];
                }

            }
            string query = valores;
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT distinct cast(S.ORDEN AS int) as codigo, S.TALLA FROM QC_MISPRINTS T  " +
                    "INNER JOIN CAT_ITEM_SIZE S ON S.ID=T.ID_TALLA " +
                    "WHERE T.ID_SUMMARY in(" + query + ") AND MONTH(T.Fecha_Registro)='" + mes + "' AND YEAR(T.Fecha_Registro) ='" + year + "'";
                leer = comando.ExecuteReader();
                int totalPrinted = 0;
                while (leer.Read())
                {
                    QCMisPrints tallas = new QCMisPrints()
                    {

                        Talla = leer["TALLA"].ToString()

                    };
                    tallas.IdTalla = objTallas.ObtenerIdTallas(tallas.Talla);
                    totalPrinted = SumaTotalSprayed1stTallaQC(query, tallas.IdTalla);
                    listTallas.Add(totalPrinted);

                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

            return listTallas;
        }

        //Muestra la lista de suma de Sprayed1st tallas reporte QC
        public int SumaTotalSprayed1stTallaQC(string query, int idTalla)
        {
            Conexion conex = new Conexion();
            int suma = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;
                com.Connection = conex.AbrirConexion();
                com.CommandText = "SELECT Sprayed1st  FROM QC_MISPRINTS WHERE ID_SUMMARY in(" + query + ") AND ID_TALLA='" + idTalla + "' ";
                leerF = com.ExecuteReader();
                while (leerF.Read())
                {
                    suma += Convert.ToInt32(leerF["Sprayed1st"]);

                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

            return suma;
        }

        //Muestra la lista de tallas TOTAL Sprayed2nd de QC por estilos
        public IEnumerable<int> ListaTotalSprayed2ndQC(List<int> listadoEstilos, int mes, string year)
        {
            ItemTallaData objTallas = new ItemTallaData();
            Conexion conn = new Conexion();

            List<int> listTallas = new List<int>();
            string valores = "";
            for (int v = 0; v < listadoEstilos.Count; v++)
            {
                if (v > 0)
                {
                    valores += "," + listadoEstilos[v];
                }
                else
                {
                    valores += listadoEstilos[v];
                }

            }
            string query = valores;
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT distinct cast(S.ORDEN AS int) as codigo, S.TALLA FROM QC_MISPRINTS T  " +
                    "INNER JOIN CAT_ITEM_SIZE S ON S.ID=T.ID_TALLA " +
                    "WHERE T.ID_SUMMARY in(" + query + ") AND MONTH(T.Fecha_Registro)='" + mes + "' AND YEAR(T.Fecha_Registro) ='" + year + "'";
                leer = comando.ExecuteReader();
                int totalPrinted = 0;
                while (leer.Read())
                {
                    QCMisPrints tallas = new QCMisPrints()
                    {

                        Talla = leer["TALLA"].ToString()

                    };
                    tallas.IdTalla = objTallas.ObtenerIdTallas(tallas.Talla);
                    totalPrinted = SumaTotalSprayed2ndTallaQC(query, tallas.IdTalla);
                    listTallas.Add(totalPrinted);

                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

            return listTallas;
        }

        //Muestra la lista de suma de Sprayed2nd tallas reporte QC
        public int SumaTotalSprayed2ndTallaQC(string query, int idTalla)
        {
            Conexion conex = new Conexion();
            int suma = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;
                com.Connection = conex.AbrirConexion();
                com.CommandText = "SELECT Sprayed2nd  FROM QC_MISPRINTS WHERE ID_SUMMARY in(" + query + ") AND ID_TALLA='" + idTalla + "' ";
                leerF = com.ExecuteReader();
                while (leerF.Read())
                {
                    suma += Convert.ToInt32(leerF["Sprayed2nd"]);

                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

            return suma;
        }

        //Muestra la lista de tallas TOTAL Defects1st de QC por estilos
        public IEnumerable<int> ListaTotalDefects1stQC(List<int> listadoEstilos, int mes, string year)
        {
            ItemTallaData objTallas = new ItemTallaData();
            Conexion conn = new Conexion();

            List<int> listTallas = new List<int>();
            string valores = "";
            for (int v = 0; v < listadoEstilos.Count; v++)
            {
                if (v > 0)
                {
                    valores += "," + listadoEstilos[v];
                }
                else
                {
                    valores += listadoEstilos[v];
                }

            }
            string query = valores;
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT distinct cast(S.ORDEN AS int) as codigo, S.TALLA FROM QC_MISPRINTS T  " +
                    "INNER JOIN CAT_ITEM_SIZE S ON S.ID=T.ID_TALLA " +
                    "WHERE T.ID_SUMMARY in(" + query + ") AND MONTH(T.Fecha_Registro)='" + mes + "' AND YEAR(T.Fecha_Registro) ='" + year + "'";
                leer = comando.ExecuteReader();
                int totalPrinted = 0;
                while (leer.Read())
                {
                    QCMisPrints tallas = new QCMisPrints()
                    {

                        Talla = leer["TALLA"].ToString()

                    };
                    tallas.IdTalla = objTallas.ObtenerIdTallas(tallas.Talla);
                    totalPrinted = SumaTotalDefects1stTallaQC(query, tallas.IdTalla);
                    listTallas.Add(totalPrinted);

                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

            return listTallas;
        }

        //Muestra la lista de suma de Defects1st tallas reporte QC
        public int SumaTotalDefects1stTallaQC(string query, int idTalla)
        {
            Conexion conex = new Conexion();
            int suma = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;
                com.Connection = conex.AbrirConexion();
                com.CommandText = "SELECT Defects1st  FROM QC_MISPRINTS WHERE ID_SUMMARY in(" + query + ") AND ID_TALLA='" + idTalla + "' ";
                leerF = com.ExecuteReader();
                while (leerF.Read())
                {
                    suma += Convert.ToInt32(leerF["Defects1st"]);

                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

            return suma;
        }

        //Muestra la lista de tallas TOTAL Defects2nd de QC por estilos
        public IEnumerable<int> ListaTotalDefects2ndQC(List<int> listadoEstilos, int mes, string year)
        {
            ItemTallaData objTallas = new ItemTallaData();
            Conexion conn = new Conexion();

            List<int> listTallas = new List<int>();
            string valores = "";
            for (int v = 0; v < listadoEstilos.Count; v++)
            {
                if (v > 0)
                {
                    valores += "," + listadoEstilos[v];
                }
                else
                {
                    valores += listadoEstilos[v];
                }

            }
            string query = valores;
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leer = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "SELECT distinct cast(S.ORDEN AS int) as codigo, S.TALLA FROM QC_MISPRINTS T  " +
                    "INNER JOIN CAT_ITEM_SIZE S ON S.ID=T.ID_TALLA " +
                    "WHERE T.ID_SUMMARY in(" + query + ") AND MONTH(T.Fecha_Registro)='" + mes + "' AND YEAR(T.Fecha_Registro) ='" + year + "'";
                leer = comando.ExecuteReader();
                int totalPrinted = 0;
                while (leer.Read())
                {
                    QCMisPrints tallas = new QCMisPrints()
                    {

                        Talla = leer["TALLA"].ToString()

                    };
                    tallas.IdTalla = objTallas.ObtenerIdTallas(tallas.Talla);
                    totalPrinted = SumaTotalDefects2ndTallaQC(query, tallas.IdTalla);
                    listTallas.Add(totalPrinted);

                }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

            return listTallas;
        }

        //Muestra la lista de suma de Defects2nd tallas reporte QC
        public int SumaTotalDefects2ndTallaQC(string query, int idTalla)
        {
            Conexion conex = new Conexion();
            int suma = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                SqlDataReader leerF = null;
                com.Connection = conex.AbrirConexion();
                com.CommandText = "SELECT Defects2nd  FROM QC_MISPRINTS WHERE ID_SUMMARY in(" + query + ") AND ID_TALLA='" + idTalla + "' ";
                leerF = com.ExecuteReader();
                while (leerF.Read())
                {
                    suma += Convert.ToInt32(leerF["Defects2nd"]);

                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }

            return suma;
        }
    }
}