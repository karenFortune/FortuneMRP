using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models.Catalogos
{
	public class CatTypeFormPackData
	{
		//Listado del catalogo de tipo de forma de empaque
		public IEnumerable<CatTypeFormPack> ListaTipoFormaPack()
		{
			List<CatTypeFormPack> listTipoFormPack = new List<CatTypeFormPack>();
			Conexion conn = new Conexion();
			try
			{
				SqlCommand comando = new SqlCommand();
				SqlDataReader leer = null;
				comando.Connection = conn.AbrirConexion();
				comando.CommandText = "SELECT * FROM CAT_TYPE_FORM_PACK ";
				leer = comando.ExecuteReader();
				while (leer.Read())
				{
					CatTypeFormPack packing = new CatTypeFormPack()
					{
						IdTipoFormPack = Convert.ToInt32(leer["ID_TYPE_FORM_PACK"]),
						TipoFormPack = leer["TYPE_FORM_PACK"].ToString()

					};

					listTipoFormPack.Add(packing);
				}
				leer.Close();
			}
			finally
			{
				conn.CerrarConexion();
				conn.Dispose();
			}

			return listTipoFormPack;
		}

		//Permite consultar los detalles en el catalogo de formas de empaque
		public CatTypeFormPack ConsultarListatipoFormPack(int? id)
		{
			CatTypeFormPack TipoForn = new CatTypeFormPack();
			Conexion conn = new Conexion();
			try
			{
				SqlCommand comando = new SqlCommand();
				SqlDataReader leer = null;
				comando.Connection = conn.AbrirConexion();
				comando.CommandText = "SELECT * FROM CAT_TYPE_FORM_PACK WHERE ID_TYPE_FORM_PACK='" + id + "'";
				leer = comando.ExecuteReader();
				while (leer.Read())
				{
					TipoForn.IdTipoFormPack = Convert.ToInt32(leer["ID_TYPE_FORM_PACK"]);
					TipoForn.TipoFormPack = leer["TYPE_FORM_PACK"].ToString();

				}
			}
			finally
			{
				conn.CerrarConexion();
				conn.Dispose();
			}
			return TipoForn;

		}

	}
}