using Microsoft.Data.SqlClient;
using RestauranteMonster.Helpers;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOEmpleado
{
    public List<Empleado> Listar()
    {
        var lista = new List<Empleado>();
        const string sql = @"SELECT id_usuario,nombre,apellido,correo,telefono,estado
                             FROM usuario WHERE id_rol=2 ORDER BY nombre";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(Mapear(r));
        return lista;
    }

    public Empleado? BuscarPorId(int id)
    {
        const string sql = @"SELECT id_usuario,nombre,apellido,correo,telefono,estado
                             FROM usuario WHERE id_usuario=@Id AND id_rol=2";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Mapear(r) : null;
    }

    public void Insertar(Empleado e, string contrasena)
    {
        const string sql = @"INSERT INTO usuario(nombre,apellido,correo,contrasena,telefono,id_rol,estado)
                             VALUES(@N,@A,@C,@P,@T,2,1)";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@N", e.Nombre);
        cmd.Parameters.AddWithValue("@A", e.Apellido);
        cmd.Parameters.AddWithValue("@C", e.Correo);
        cmd.Parameters.AddWithValue("@P", Encriptador.HashContrasena(contrasena));
        cmd.Parameters.AddWithValue("@T", (object?)e.Telefono ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public void Actualizar(Empleado e)
    {
        const string sql = @"UPDATE usuario SET nombre=@N,apellido=@A,correo=@C,telefono=@T
                             WHERE id_usuario=@Id AND id_rol=2";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@N", e.Nombre);
        cmd.Parameters.AddWithValue("@A", e.Apellido);
        cmd.Parameters.AddWithValue("@C", e.Correo);
        cmd.Parameters.AddWithValue("@T", (object?)e.Telefono ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Id", e.IdUsuario);
        cmd.ExecuteNonQuery();
    }

    public void CambiarEstado(int id, int estado)
    {
        const string sql = "UPDATE usuario SET estado=@E WHERE id_usuario=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@E", estado);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();
    }

    private static Empleado Mapear(SqlDataReader r) => new()
    {
        IdUsuario = Convert.ToInt32(r["id_usuario"]),
        Nombre    = r["nombre"]?.ToString() ?? "",
        Apellido  = r["apellido"]?.ToString() ?? "",
        Correo    = r["correo"]?.ToString() ?? "",
        Telefono  = r["telefono"]?.ToString(),
        Estado    = Convert.ToInt32(r["estado"])
    };
}
