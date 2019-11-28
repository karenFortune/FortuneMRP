using FortuneSystem.Models.Roles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models.Usuarios
{
    public class CatUsuarioData
    {


        //Muestra la lista de Usuarios 
        public IEnumerable<CatUsuario> ListaUsuarios()
        {
            Conexion conn = new Conexion();
            List<CatUsuario> listUsuarios = new List<CatUsuario>();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
				comando.Connection = conn.AbrirConexion();
				comando.CommandText = "Listar_Usuarios";
                comando.CommandType = CommandType.StoredProcedure;
                leerFilas = comando.ExecuteReader();

                while (leerFilas.Read())
                {
                    CatUsuario usuarios = new CatUsuario();
                    CatRoles roles = new CatRoles();
                    usuarios.Id = Convert.ToInt32(leerFilas["Id"]);
                    usuarios.NoEmpleado = Convert.ToInt32(leerFilas["NoEmpleado"]);
                    usuarios.Nombres = leerFilas["Nombres"].ToString();
                    usuarios.Apellidos = leerFilas["Apellidos"].ToString();
                    usuarios.Cargo = Convert.ToInt32(leerFilas["Cargo"]);
                    usuarios.Email = leerFilas["Email"].ToString();
                    usuarios.Contrasena = leerFilas["Contrasena"].ToString();
                    usuarios.NombreCompleto = usuarios.Nombres + " " + usuarios.Apellidos;
					usuarios.TipoTurno = Convert.ToInt32(leerFilas["turno"]);
					roles.Rol = leerFilas["rol"].ToString();
                    usuarios.CatRoles = roles;
                    listUsuarios.Add(usuarios);

                }
                leerFilas.Close();
            }
            finally
            {

                conn.CerrarConexion();
                conn.Dispose();
            }


            return listUsuarios;
        }
        
        //Permite crear un nuevo usuario
        public void AgregarUsuarios(CatUsuario usuarios)
        {
            Conexion conn = new Conexion();
            try
            {
                SqlCommand comando = new SqlCommand();
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "AgregarUsuarios";
                comando.CommandType = CommandType.StoredProcedure;

                comando.Parameters.AddWithValue("@NoEmpleado", usuarios.NoEmpleado);
                comando.Parameters.AddWithValue("@Nombres", usuarios.Nombres);
                comando.Parameters.AddWithValue("@Apellidos", usuarios.Apellidos);
                comando.Parameters.AddWithValue("@Cargo", usuarios.Cargo);
                comando.Parameters.AddWithValue("@Email", usuarios.Email);
                comando.Parameters.AddWithValue("@Contrasena", usuarios.Contrasena);
                comando.Parameters.AddWithValue("@Sucursal", usuarios.IdSucursal);
                comando.Parameters.AddWithValue("@Turno", usuarios.TipoTurno);

                comando.ExecuteNonQuery();

            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }
            
            

        }

        //Permite consultar los detalles de un Usuario
        public CatUsuario ConsultarListaUsuarios (int? id)
        {
            CatUsuario usuarios = new CatUsuario();
            Conexion conn = new Conexion();

            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerFilas = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "Lista_Usuario_Por_Id";
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@Id", id);

                leerFilas = comando.ExecuteReader();
                while (leerFilas.Read())
                {

                    usuarios.Id = Convert.ToInt32(leerFilas["Id"]);
                    usuarios.NoEmpleado = Convert.ToInt32(leerFilas["NoEmpleado"]);
                    usuarios.Nombres = leerFilas["Nombres"].ToString();
                    usuarios.Apellidos = leerFilas["Apellidos"].ToString();
                    usuarios.Cargo = Convert.ToInt32(leerFilas["Cargo"]);
                    usuarios.Email = leerFilas["Email"].ToString();
                    usuarios.Contrasena = leerFilas["Contrasena"].ToString();
                    usuarios.IdSucursal = Convert.ToInt32(leerFilas["id_sucursal"]);
                    usuarios.TipoTurno = Convert.ToInt32(leerFilas["turno"]);

                }

            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }
            
            return usuarios;

        }

        //Permite actualiza la informacion de un usuario
        public void ActualizarUsuarios(CatUsuario usuarios)
        {
            Conexion conn = new Conexion();
            try
            {
                SqlCommand comando = new SqlCommand();
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "Actualizar_Usuarios";
                comando.CommandType = CommandType.StoredProcedure;

                comando.Parameters.AddWithValue("@Id", usuarios.Id);
                comando.Parameters.AddWithValue("@Nombres", usuarios.Nombres);
                comando.Parameters.AddWithValue("@NoEmpleado", usuarios.NoEmpleado);
                comando.Parameters.AddWithValue("@Apellidos", usuarios.Apellidos);
                comando.Parameters.AddWithValue("@Cargo", usuarios.Cargo);
                comando.Parameters.AddWithValue("@Email", usuarios.Email);
                comando.Parameters.AddWithValue("@Contrasena", usuarios.Contrasena);
                comando.Parameters.AddWithValue("@Sucursal", usuarios.IdSucursal);
                comando.Parameters.AddWithValue("@Turno", usuarios.TipoTurno);

                comando.ExecuteNonQuery();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

            
        }

        //Permite eliminar la informacion de un usuario
        public void EliminarUsuario(int? id)
        {
            Conexion conn = new Conexion();
            try
            {
                SqlCommand comando = new SqlCommand();
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "EliminarUsuarios";
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

        // Obtener datos de un usuario 
        public int Obtener_Datos_Usuarios(string noEmpleado)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "SELECT Id,CONCAT( Nombres,' ', Apellidos) AS Nombre  FROM Usuarios WHERE NoEmpleado='" + noEmpleado + "'";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    return Convert.ToInt32(reader["Id"]);
                }
                conex.CerrarConexion();
            }
            finally { conex.CerrarConexion(); conex.Dispose(); }
            return 0;
        }

        // Obtener contraseña de un usuario 
        public string Obtener_Contraseña_Usuario(string noEmpleado)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "SELECT contrasena FROM Usuarios WHERE NoEmpleado='" + noEmpleado + "'";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    return reader["contrasena"].ToString();
                }
             
            }
            finally { conex.CerrarConexion(); conex.Dispose(); }
            return "";
        }

		// Obtener sucursal de un usuario 
		public string Obtener_Sucursal_Usuario(string noEmpleado)
		{
			SqlCommand cmd = new SqlCommand();
			SqlDataReader reader;
			Conexion conex = new Conexion();
			string sucursal = "";
			try
			{
				cmd.Connection = conex.AbrirConexion();
				cmd.CommandText = "SELECT id_sucursal FROM Usuarios WHERE NoEmpleado='" + noEmpleado + "'";
				cmd.CommandType = CommandType.Text;
				reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					
					int idSucursal = Convert.ToInt32(reader["id_sucursal"]);
					if(idSucursal == 1)
					{
						sucursal = "FORTUNE";
					}
					else
					{
						sucursal = "LUCKY1";
					}
					
				}

			}
			finally { conex.CerrarConexion(); conex.Dispose(); }
			return sucursal;
		}

		// Obtenernombre de un usuario 
		public string Obtener_Nombre_Usuario(string noEmpleado)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "SELECT Id,CONCAT( Nombres,' ', Apellidos) AS Nombre  FROM Usuarios WHERE NoEmpleado='" + noEmpleado + "'";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    return reader["Nombre"].ToString();
                }
              
            }
            finally { conex.CerrarConexion(); conex.Dispose(); }
            return "";
        }

        // Obtenernombre de un usuario 
        public string Obtener_Nombre_Usuario_PorID(int idEmpleado)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "SELECT CONCAT( Nombres,' ', Apellidos) AS Nombre  FROM Usuarios WHERE Id='" + idEmpleado + "'";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    return reader["Nombre"].ToString();
                }
                
            }
            finally { conex.CerrarConexion(); conex.Dispose(); }
            return "";
        }

        // Obtener turno de un usuario 
        public int Obtener_Turno_Usuario_PorID(int idEmpleado)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            Conexion conex = new Conexion();
            try
            {
                cmd.Connection = conex.AbrirConexion();
                cmd.CommandText = "SELECT turno  FROM Usuarios WHERE Id='" + idEmpleado + "'";
                cmd.CommandType = CommandType.Text;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    return Convert.ToInt32(reader["turno"]);
                }

            }
            finally { conex.CerrarConexion(); conex.Dispose(); }
            return 0;
        }


    }
}