using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models
{
    public class Link
    {
        private static readonly object objectLock = new object();
        private bool _disposed;
        //PRUEBAS
        //private SqlConnection conn = new SqlConnection("Server=tcp:fortunesp.database.windows.net,1433;Initial Catalog=FortuneTest;Persist Security Info=False;User ID=AdminFB;Password=Admin@2019;MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=False;Connection Timeout=10000;Trusted_Connection=False;");

        //PRODUCCION
        //private SqlConnection conn = new SqlConnection("Server=tcp:72.3.145.49,1433;Initial Catalog=FortuneTest;User ID=guest4723;Password=vG5Ix6SqI5zZ;Connection Timeout=10000;");

        private SqlConnection conn = new SqlConnection("Server=tcp:72.3.145.49,1433;Initial Catalog=FortuneTest;User ID=Admin;Password=Cx3VkjQG5opB;Connection Timeout=36000;");



        public SqlConnection AbrirConexion()
        {
            if (conn.State == ConnectionState.Closed)
            {               
                    conn.Open();             
            }
            return conn;
        }
        public SqlConnection CerrarConexion()
        {
            if(conn.State == ConnectionState.Closed)
            {
                conn.Close();
                SqlConnection.ClearPool(conn);
            }
            return conn;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (objectLock)
            {
                if (_disposed == false)
                {
                    if (disposing == true)
                    {
                        if (conn != null)
                        {
                            conn.Dispose();
                            conn = null;
                        }              
                        _disposed = true;
                    }
                }
            }
        }


    }
}