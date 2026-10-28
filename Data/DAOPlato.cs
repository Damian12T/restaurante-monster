using Microsoft.Data.SqlClient;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOPlato
{
    public List<Plato> Listar()
    {
        var lista = new List<Plato>();
        const string sql = @"SELECT p.*, t.nombre AS tipo_nombre, ISNULL(s.cantidad,0) AS stock
                             FROM plato p
                             LEFT JOIN tipo_plato t ON p.id_tipo=t.id_tipo
                             LEFT JOIN stock s ON s.id_plato=p.id_plato
                             ORDER BY t.nombre, p.nombre";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(Mapear(r, true));
        return lista;
    }

    public List<Plato> ListarDisponibles()
    {
        var lista = new List<Plato>();
        const string sql = @"SELECT p.*, t.nombre AS tipo_nombre, ISNULL(s.cantidad,0) AS stock
                             FROM plato p
                             LEFT JOIN tipo_plato t ON p.id_tipo=t.id_tipo
                             LEFT JOIN stock s ON s.id_plato=p.id_plato
                             WHERE p.disponible=1 ORDER BY t.nombre, p.nombre";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(Mapear(r, true));
        return lista;
    }

    public List<Plato> ListarPorTipo(int idTipo)
    {
        var lista = new List<Plato>();
        const string sql = @"SELECT p.*, t.nombre AS tipo_nombre, ISNULL(s.cantidad,0) AS stock
                             FROM plato p
                             LEFT JOIN tipo_plato t ON p.id_tipo=t.id_tipo
                             LEFT JOIN stock s ON s.id_plato=p.id_plato
                             WHERE p.id_tipo=@IdTipo AND p.disponible=1 ORDER BY p.nombre";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@IdTipo", idTipo);
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(Mapear(r, true));
        return lista;
    }

    public Plato? BuscarPorId(int id)
    {
        const string sql = @"SELECT p.*, t.nombre AS tipo_nombre, ISNULL(s.cantidad,0) AS stock
                             FROM plato p
                             LEFT JOIN tipo_plato t ON p.id_tipo=t.id_tipo
                             LEFT JOIN stock s ON s.id_plato=p.id_plato
                             WHERE p.id_plato=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Mapear(r, true) : null;
    }

    public void Insertar(Plato p)
    {
        const string sql = @"INSERT INTO plato(nombre,descripcion,precio,imagen,id_tipo,disponible)
                             VALUES(@N,@D,@P,@I,@T,1);
                             INSERT INTO stock(id_plato,cantidad,stock_minimo)
                             VALUES(SCOPE_IDENTITY(),50,5);";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@N", p.Nombre);
        cmd.Parameters.AddWithValue("@D", (object?)p.Descripcion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@P", p.Precio);
        cmd.Parameters.AddWithValue("@I", (object?)p.Imagen ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@T", p.IdTipo);
        cmd.ExecuteNonQuery();
    }

    public void Actualizar(Plato p)
    {
        var sql = p.Imagen != null
            ? "UPDATE plato SET nombre=@N,descripcion=@D,precio=@P,imagen=@I,id_tipo=@T WHERE id_plato=@Id"
            : "UPDATE plato SET nombre=@N,descripcion=@D,precio=@P,id_tipo=@T WHERE id_plato=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@N", p.Nombre);
        cmd.Parameters.AddWithValue("@D", (object?)p.Descripcion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@P", p.Precio);
        if (p.Imagen != null) cmd.Parameters.AddWithValue("@I", p.Imagen);
        cmd.Parameters.AddWithValue("@T", p.IdTipo);
        cmd.Parameters.AddWithValue("@Id", p.IdPlato);
        cmd.ExecuteNonQuery();
    }

    public void CambiarDisponibilidad(int id, bool disponible)
    {
        const string sql = "UPDATE plato SET disponible=@D WHERE id_plato=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@D", disponible ? 1 : 0);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();
    }

    private static Plato Mapear(SqlDataReader r, bool conTipo) => new()
    {
        IdPlato     = Convert.ToInt32(r["id_plato"]),
        Nombre      = r["nombre"]?.ToString() ?? "",
        Descripcion = r["descripcion"]?.ToString(),
        Precio      = Convert.ToDecimal(r["precio"]),
        Imagen      = r["imagen"]?.ToString(),
        IdTipo      = Convert.ToInt32(r["id_tipo"]),
        TipoNombre  = conTipo ? r["tipo_nombre"]?.ToString() : null,
        Disponible  = Convert.ToBoolean(r["disponible"]),
        Stock       = Convert.ToInt32(r["stock"])
    };
}
