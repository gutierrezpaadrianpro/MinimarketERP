using System;
using System.Data.SqlClient;

namespace MinimarketERP
{
    public class Conexion
    {
        // Conexión local a SQL Server y la base de datos MinimarketDB
        private static string cadenaConexion = "Data Source=.;Initial Catalog=MinimarketDB;Integrated Security=True;";

        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(cadenaConexion);
        }
    }
}