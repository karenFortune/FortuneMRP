using FortuneSystem.Models.Usuarios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models.Roles
{
    public class CatRolesData
    { 
   

        //Muestra la lista de Roles
        public IEnumerable<CatRoles> ListaRoles()
        {
            List<CatRoles> listRoles = new List<CatRoles>();
            Conexion conn = new Conexion();
            SqlCommand comando = new SqlCommand();
            SqlDataReader leer = null;
            try
            {
            
            comando.Connection = conn.AbrirConexion();
            comando.CommandText = "Listar_Roles";
            comando.CommandType = CommandType.StoredProcedure;
            leer = comando.ExecuteReader();

            while (leer.Read())
            {
                CatRoles roles = new CatRoles();
                roles.Id= Convert.ToInt32(leer["id_Rol"]);
                roles.Rol = leer["rol"].ToString();



                listRoles.Add(roles);
            }
            leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

            return listRoles;
        }

        //Permite crear un nuevo rol
        public void AgregarRoles(CatRoles roles)
        {
            Conexion conn = new Conexion();
            try
            {
                SqlCommand comando = new SqlCommand();
            comando.Connection = conn.AbrirConexion();
            comando.CommandText = "AgregarRoles";
            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@Rol", roles.Rol);
 
            comando.ExecuteNonQuery();
        }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

}

        //Permite consultar los detalles de un rol
        public CatRoles ConsultarListaRoles(int? id)
        {
            Conexion conn = new Conexion();
            CatRoles roles = new CatRoles();
            try
            {
                SqlCommand comando = new SqlCommand();
            SqlDataReader leer = null;

            comando.Connection = conn.AbrirConexion();
            comando.CommandText = "Listar_Roles_Por_Id";
            comando.CommandType = CommandType.StoredProcedure;
            comando.Parameters.AddWithValue("@Id", id);

            leer = comando.ExecuteReader();
            while (leer.Read())
            {
                roles.Id = Convert.ToInt32(leer["id_Rol"]);
                roles.Rol= leer["rol"].ToString();

            }
                leer.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }
            return roles;

        }

        //Permite actualiza la informacion de un rol
        public void ActualizarRoles(CatRoles roles)
        {
            Conexion conn = new Conexion();
            try
            {
                SqlCommand comando = new SqlCommand();
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "Actualizar_Roles";
                comando.CommandType = CommandType.StoredProcedure;

                comando.Parameters.AddWithValue("@Id", roles.Id);
                comando.Parameters.AddWithValue("@Rol", roles.Rol);

                comando.ExecuteNonQuery();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }
        }

        //Permite eliminar la informacion de un rol
        public void EliminarRol(int? id)
        {
            Conexion conn = new Conexion();
            try
            {
                SqlCommand comando = new SqlCommand();
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "EliminarRoles";
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


    }
}