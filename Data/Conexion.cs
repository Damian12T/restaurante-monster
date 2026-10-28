using Microsoft.Data.SqlClient;

namespace RestauranteMonster.Data;

public static class Conexion
{
    private static string _connectionString = string.Empty;

    public static void Configure(string connectionString)
    {
        _connectionString = connectionString;
    }

    public static SqlConnection GetConexion()
    {
        return new SqlConnection(_connectionString);
    }
}
