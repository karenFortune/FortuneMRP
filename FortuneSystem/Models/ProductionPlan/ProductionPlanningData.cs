using FortuneSystem.Models.Items;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models.ProductionPlan
{
	public class ProductionPlanningData
	{
		//Muestra la lista de hornos
		public IEnumerable<ProdOverMachine> ListaMaquinasHorno(int? IdHorno)
		{
			Conexion conn = new Conexion();
			List<ProdOverMachine> ListaMaqHornos = new List<ProdOverMachine>();
			try
			{
				SqlCommand comando = new SqlCommand();
				SqlDataReader leer = null;

				comando.Connection = conn.AbrirConexion();
				comando.CommandText = "select * from PROD_OVER_MACHINE where Id_Prod_Cat_Over='" + IdHorno + "'";
				comando.CommandType = CommandType.Text;
				leer = comando.ExecuteReader();

				while (leer.Read())
				{
					ProdOverMachine maquinasHornos = new ProdOverMachine()
					{
						IdProdCatOver = Convert.ToInt32(leer["Id_Prod_Cat_Over"]),
						IdProdOverMachine = Convert.ToInt32(leer["Id_Prod_Over_Machine"]),
						NoMaquina = Convert.ToInt32(leer["No_Machine"])
					};

					ListaMaqHornos.Add(maquinasHornos);
				}
				leer.Close();
			}
			finally
			{
				conn.CerrarConexion();
				conn.Dispose();
			}
			return ListaMaqHornos;
		}

		//Muestra la lista de maquinas
		public IEnumerable<ProdOverMachine> ListaMaquinas()
		{
			Conexion conn = new Conexion();
			List<ProdOverMachine> ListaMaq = new List<ProdOverMachine>();
			try
			{
				SqlCommand comando = new SqlCommand();
				SqlDataReader leer = null;

				comando.Connection = conn.AbrirConexion();
				comando.CommandText = "SELECT pom.No_Machine FROM PROD_OVER_MACHINE pom";
				comando.CommandType = CommandType.Text;
				leer = comando.ExecuteReader();

				while (leer.Read())
				{
					ProdOverMachine maquinas = new ProdOverMachine()
					{
						NoMaquina = Convert.ToInt32(leer["No_Machine"])
					};

					ListaMaq.Add(maquinas);
				}
				leer.Close();
			}
			finally
			{
				conn.CerrarConexion();
				conn.Dispose();
			}
			return ListaMaq;
		}

		public IEnumerable<ProductionPlanning> ListaPlaneacionGeneral()
		{
			Conexion conn = new Conexion();
			List<ProductionPlanning> ListaPlaneacion= new List<ProductionPlanning>();
			try
			{
				SqlCommand comando = new SqlCommand();
				SqlDataReader leer = null;

				comando.Connection = conn.AbrirConexion();
				comando.CommandText = "SELECT pp.Id_Production_Plan,p.PO,  pco.Name, pom.No_Machine, pp.Fecha, id.DESCRIPTION, pp.Metedor, pp.Sacador, pp.Cachador, pp.Estado " +
									  "FROM PRODUCTION_PLAN pp, PROD_OVER_MACHINE pom, PROD_CAT_OVER pco, PO_SUMMARY ps, ITEM_DESCRIPTION id, PEDIDO p  " +
									  "WHERE pp.Id_Prod_Over_Machine=pom.Id_Prod_Over_Machine AND pom.Id_Prod_Cat_Over=pco.Id_Prod_Cat_Over AND ps.ID_PO_SUMMARY=pp.Id_Summary " +
									  "AND ps.ITEM_ID=id.ITEM_ID AND ps.ID_PEDIDOS=P.ID_PEDIDO " +
									  "ORDER by cast(pom.No_Machine AS int) ASC ";
				comando.CommandType = CommandType.Text;
				leer = comando.ExecuteReader();

				while (leer.Read())
				{
					ItemDescripcion itemDesc = new ItemDescripcion
					{
						Descripcion = leer["DESCRIPTION"].ToString().TrimEnd()
					};
					ProdOverMachine maquinasHornos = new ProdOverMachine() 
				    {
						NoMaquina = Convert.ToInt32(leer["No_Machine"])
					};
					CatProdOver catProdOver = new CatProdOver
					{
						Horno = leer["Name"].ToString()
					};
					ProductionPlanning planning = new ProductionPlanning()
					{
						IdProductionPlan = Convert.ToInt32(leer["Id_Production_Plan"]),
						Fecha = Convert.ToDateTime(leer["Fecha"]),
						Metedor = leer["Metedor"].ToString().TrimEnd(),
						Sacador = leer["Sacador"].ToString().TrimEnd(),
						Cachador = leer["Cachador"].ToString().TrimEnd(),
						Pedido = leer["PO"].ToString().TrimEnd(),
						Status = leer["Estado"].ToString().TrimEnd()
					};

					planning.DiaFecha = String.Format("{0:dddd}", planning.Fecha);
					planning.FechaPlan = String.Format("{0:dd/MMM/yyyy}", planning.Fecha);
					maquinasHornos.CatProdOver = catProdOver;
					planning.ItemDescripcion = itemDesc;
					planning.ProdOverMachine = maquinasHornos;

					ListaPlaneacion.Add(planning);
				}
				leer.Close();
			}
			finally
			{
				conn.CerrarConexion();
				conn.Dispose();
			}
			return ListaPlaneacion;
		}


		//Obtener informacion de planeacion
		public ProductionPlanning ObtenerInformacionPlaneacionPorId(int? id, int? idMaquina)
		{
			Conexion conexion = new Conexion();
			ProductionPlanning reporte = new ProductionPlanning();
			try
			{
				SqlCommand com = new SqlCommand();
				SqlDataReader leerF = null;

				com.Connection = conexion.AbrirConexion();
				com.CommandText = "select  pp.Id_Production_Plan, pp.Id_Prod_Over_Machine, pp.Id_Summary, pp.Cachador, pp.Estado, "+
								  "pp.Fecha,pp.Id_Turn,pp.Id_User,pp.Metedor,pp.Prioridad, pp.Sacador, pom.Id_Prod_Cat_Over, ps.ID_PEDIDOS, ps.ITEM_ID " +
								  "from PRODUCTION_PLAN pp, PROD_OVER_MACHINE pom, PO_SUMMARY ps where pp.Id_Production_Plan= '" + id + "' and ps.ID_PO_SUMMARY=pp.Id_Summary and pom.Id_Prod_Over_Machine='"+ idMaquina + "' ";
				com.CommandType = CommandType.Text;

				leerF = com.ExecuteReader();
				while (leerF.Read())
				{
				
					reporte.IdProductionPlan = Convert.ToInt32(leerF["Id_Production_Plan"]);
					reporte.IdProdOverMachine = Convert.ToInt32(leerF["Id_Prod_Over_Machine"]);					
					reporte.IdSummary = Convert.ToInt32(leerF["Id_Summary"]);
					reporte.FechaAct = Convert.ToDateTime(leerF["Fecha"]);
					reporte.IdTurno = Convert.ToInt32(leerF["Id_Turn"]);
					reporte.IdUser = Convert.ToInt32(leerF["Id_User"]);
					reporte.MetedorAct = leerF["Metedor"].ToString();
					reporte.SacadorAct = leerF["Sacador"].ToString();
					reporte.CachadorAct = leerF["Cachador"].ToString();
					reporte.Status = leerF["Estado"].ToString();		
					reporte.IdPrioridad = Convert.ToInt32(leerF["Prioridad"]);
					reporte.IdHorno = Convert.ToInt32(leerF["Id_Prod_Cat_Over"]);
					reporte.IdPedido = Convert.ToInt32(leerF["ID_PEDIDOS"]);
					reporte.IdEstilo = Convert.ToInt32(leerF["ITEM_ID"]);

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

		//Permite agregar la planeacion de produccion por estilo
		public void AgregarPlaneacion(ProductionPlanning planning)
		{
			Conexion conn = new Conexion();
			try
			{
				SqlCommand comando = new SqlCommand
				{
					Connection = conn.AbrirConexion(),
					CommandText = "AgregarPlaneacion",
					CommandType = CommandType.StoredProcedure
				};

				comando.Parameters.AddWithValue("@IdHorno", planning.IdProdOverMachine);
				comando.Parameters.AddWithValue("@IdEstilo", planning.IdSummary);
				comando.Parameters.AddWithValue("@Fecha", planning.Fecha);
				comando.Parameters.AddWithValue("@Idturno", planning.IdTurno);
				comando.Parameters.AddWithValue("@IdUser", planning.IdUser);
				comando.Parameters.AddWithValue("@metedor", planning.Metedor);
				comando.Parameters.AddWithValue("@sacador", planning.Sacador);
				comando.Parameters.AddWithValue("@cachador", planning.Cachador);
				comando.Parameters.AddWithValue("@status", planning.Status);
				comando.Parameters.AddWithValue("@prioridad", planning.Prioridades);


				comando.ExecuteNonQuery();
			}
			finally
			{
				conn.CerrarConexion();
				conn.Dispose();
			}

		}

		//Permite actualizar la planeacion de produccion por estilo
		public void ActualizarPlaneacion(ProductionPlanning planning)
		{
			Conexion conn = new Conexion();
			try
			{
				SqlCommand comando = new SqlCommand
				{
					Connection = conn.AbrirConexion(),
					CommandText = "ActualizarPlaneacion",
					CommandType = CommandType.StoredProcedure
				};
				comando.Parameters.AddWithValue("@IdPlan", planning.IdProductionPlan);
				comando.Parameters.AddWithValue("@IdHorno", planning.IdProdOverMachine);
				comando.Parameters.AddWithValue("@IdEstilo", planning.IdSummary);
				comando.Parameters.AddWithValue("@Fecha", planning.FechaAct);
				comando.Parameters.AddWithValue("@Idturno", planning.IdTurno);
				comando.Parameters.AddWithValue("@IdUser", planning.IdUser);
				comando.Parameters.AddWithValue("@metedor", planning.MetedorAct);
				comando.Parameters.AddWithValue("@sacador", planning.SacadorAct);
				comando.Parameters.AddWithValue("@cachador", planning.CachadorAct);
				comando.Parameters.AddWithValue("@status", planning.Status);
				comando.Parameters.AddWithValue("@prioridad", planning.IdPrioridad);


				comando.ExecuteNonQuery();
			}
			finally
			{
				conn.CerrarConexion();
				conn.Dispose();
			}

		}



	}
}