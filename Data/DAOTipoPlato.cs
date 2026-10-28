using Microsoft.Data.SqlClient;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOTipoPlato
{
    public List<TipoPlato> Listar()
    {
        var lista = new List<TipoPlato>();
        const string sql = "SELECT * FROM tipo_plato ORDER BY nombre";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(new TipoPlato
        {
            IdTipo = Convert.ToInt32(r["id_tipo"]),
            Nombre = r["nombre"]?.ToString() ?? ""
        });
        return lista;
    }

    public TipoPlato? BuscarPorId(int id)
    {
        const string sql = "SELECT * FROM tipo_plato WHERE id_tipo=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new TipoPlato { IdTipo = Convert.ToInt32(r["id_tipo"]), Nombre = r["nombre"]?.ToString() ?? "" };
    }

    public void Insertar(string nombre)
    {
        const string sql = "INSERT INTO tipo_plato(nombre) VALUES(@N)";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@N", nombre);
        cmd.ExecuteNonQuery();
    }

    public void Actualizar(int id, string nombre)
    {
        const string sql = "UPDATE tipo_plato SET nombre=@N WHERE id_tipo=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@N", nombre);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();
    }

    public bool TienePlatos(int idTipo)
    {
        const string sql = "SELECT COUNT(*) FROM plato WHERE id_tipo=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Id", idTipo);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public void Eliminar(int id)
    {
        const string sql = "DELETE FROM tipo_plato WHERE id_tipo=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();
    }
}
