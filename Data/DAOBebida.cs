using Microsoft.Data.SqlClient;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOBebida
{
    public List<Bebida> Listar()
    {
        var lista = new List<Bebida>();
        const string sql = @"SELECT b.*, ISNULL(s.cantidad,0) AS stock
                             FROM bebida b
                             LEFT JOIN stock s ON s.id_bebida=b.id_bebida
                             ORDER BY b.nombre";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(Mapear(r));
        return lista;
    }

    public List<Bebida> ListarDisponibles()
    {
        var lista = new List<Bebida>();
        const string sql = @"SELECT b.*, ISNULL(s.cantidad,0) AS stock
                             FROM bebida b
                             LEFT JOIN stock s ON s.id_bebida=b.id_bebida
                             WHERE b.disponible=1 ORDER BY b.nombre";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(Mapear(r));
        return lista;
    }

    public Bebida? BuscarPorId(int id)
    {
        const string sql = @"SELECT b.*, ISNULL(s.cantidad,0) AS stock
                             FROM bebida b
                             LEFT JOIN stock s ON s.id_bebida=b.id_bebida
                             WHERE b.id_bebida=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Mapear(r) : null;
    }

    public void Insertar(Bebida b)
    {
        const string sql = @"INSERT INTO bebida(nombre,descripcion,precio,imagen,disponible)
                             VALUES(@N,@D,@P,@I,1);
                             INSERT INTO stock(id_bebida,cantidad,stock_minimo)
                             VALUES(SCOPE_IDENTITY(),80,10);";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@N", b.Nombre);
        cmd.Parameters.AddWithValue("@D", (object?)b.Descripcion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@P", b.Precio);
        cmd.Parameters.AddWithValue("@I", (object?)b.Imagen ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public void Actualizar(Bebida b)
    {
        var sql = b.Imagen != null
            ? "UPDATE bebida SET nombre=@N,descripcion=@D,precio=@P,imagen=@I WHERE id_bebida=@Id"
            : "UPDATE bebida SET nombre=@N,descripcion=@D,precio=@P WHERE id_bebida=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@N", b.Nombre);
        cmd.Parameters.AddWithValue("@D", (object?)b.Descripcion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@P", b.Precio);
        if (b.Imagen != null) cmd.Parameters.AddWithValue("@I", b.Imagen);
        cmd.Parameters.AddWithValue("@Id", b.IdBebida);
        cmd.ExecuteNonQuery();
    }

    public void CambiarDisponibilidad(int id, bool disponible)
    {
        const string sql = "UPDATE bebida SET disponible=@D WHERE id_bebida=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@D", disponible ? 1 : 0);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();
    }

    private static Bebida Mapear(SqlDataReader r) => new()
    {
        IdBebida    = Convert.ToInt32(r["id_bebida"]),
        Nombre      = r["nombre"]?.ToString() ?? "",
        Descripcion = r["descripcion"]?.ToString(),
        Precio      = Convert.ToDecimal(r["precio"]),
        Imagen      = r["imagen"]?.ToString(),
        Disponible  = Convert.ToBoolean(r["disponible"]),
        Stock       = Convert.ToInt32(r["stock"])
    };
}
