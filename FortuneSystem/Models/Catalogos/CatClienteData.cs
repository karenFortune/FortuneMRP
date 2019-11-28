using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models.Catalogos
{
    public class CatClienteData
    {


        //Muestra la lista de clientes
        public IEnumerable<CatCliente> ListaClientes()
        {
            Conexion conn = new Conexion();
            List<CatCliente> listClientes = new List<CatCliente>();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerCliente = null;

                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "Listar_Clientes";
                comando.CommandType = CommandType.StoredProcedure;
                leerCliente = comando.ExecuteReader();

                while (leerCliente.Read())
                {
                    CatCliente clientes = new CatCliente()
                    {
                        Customer = Convert.ToInt32(leerCliente["CUSTOMER"]),
                        Nombre = leerCliente["NAME"].ToString()
                    };

                    listClientes.Add(clientes);
                }
                leerCliente.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }
            return listClientes;
        }

        public IEnumerable<CatCliente> ListaClientes2()
        {
            Conexion conn = new Conexion();
            List<CatCliente> listClientes = new List<CatCliente>();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerClienteP = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "Listar_Clientes";
                comando.CommandType = CommandType.StoredProcedure;
                leerClienteP = comando.ExecuteReader();
                while (leerClienteP.Read())
                {
                    CatCliente clientes = new CatCliente()
                    {
                        Customer = Convert.ToInt32(leerClienteP["CUSTOMER"]),
                        Nombre = leerClienteP["NAME"].ToString()
                    };

                    listClientes.Add(clientes);
                }
                leerClienteP.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

            return listClientes;
        }

        //Permite crear un nuevo cliente
        public void AgregarClientes(CatCliente clientes)
        {
            Conexion conn = new Conexion();
            try
            {
                SqlCommand comando = new SqlCommand();
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "AgregarCliente";
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@Nombre", clientes.Nombre);
                comando.ExecuteNonQuery();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }

        }

        //Permite consultar los detalles de un cliente
        public CatCliente ConsultarListaClientes(int? id)
        {

            Conexion conn = new Conexion();
            CatCliente clientes = new CatCliente();
            try
            {
                SqlCommand comando = new SqlCommand();
                SqlDataReader leerCliente = null;
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "Listar_Cliente_Por_Id";
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@Id", id);
                leerCliente = comando.ExecuteReader();
                while (leerCliente.Read())
                {
                    clientes.Customer = Convert.ToInt32(leerCliente["CUSTOMER"]);
                    clientes.Nombre = leerCliente["NAME"].ToString();
                }
                leerCliente.Close();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }
            return clientes;

        }

        //Permite actualiza la informacion de un cliente
        public void ActualizarCliente(CatCliente clientes)
        {
            Conexion conn = new Conexion();
            try
            {
                SqlCommand comando = new SqlCommand();
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "Actualizar_Cliente";
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@Id", clientes.Customer);
                comando.Parameters.AddWithValue("@Nombre", clientes.Nombre);
                comando.ExecuteNonQuery();
            }
            finally
            {
                conn.CerrarConexion();
                conn.Dispose();
            }
        }

        //Permite eliminar la informacion de un cliente
        public void EliminarCliente(int? id)
        {
            Conexion conn = new Conexion();
            try
            {
                SqlCommand comando = new SqlCommand();
                comando.Connection = conn.AbrirConexion();
                comando.CommandText = "EliminarClientes";
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
        //Obtener numero cliente por pedido
        public int ObtenerNumeroCliente(int idPedido)
        {
            int cliente = 0;
            Conexion conex = new Conexion();
            try
            {
                SqlCommand coman = new SqlCommand();
                SqlDataReader leerF = null;
                coman.Connection = conex.AbrirConexion();
                coman.CommandText = "SELECT CUSTOMER FROM PEDIDO where ID_PEDIDO='" + idPedido + "' ";
                leerF = coman.ExecuteReader();
                while (leerF.Read())
                {
                    cliente = Convert.ToInt32(leerF["CUSTOMER"]);
                }
                leerF.Close();
            }
            finally
            {
                conex.CerrarConexion();
                conex.Dispose();
            }
            return cliente;
        }


    }
}


