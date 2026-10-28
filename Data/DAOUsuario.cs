using Microsoft.Data.SqlClient;
using RestauranteMonster.Helpers;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOUsuario
{
    public Usuario? Autenticar(string correo, string contrasena)
    {
        const string sql = @"SELECT u.*, r.nombre AS rol_nombre FROM usuario u
                             JOIN rol r ON u.id_rol = r.id_rol
                             WHERE u.correo=@Correo AND u.contrasena=@Contrasena AND u.estado=1";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Correo", correo);
        cmd.Parameters.AddWithValue("@Contrasena", Encriptador.Sha256(contrasena));
        using var r = cmd.ExecuteReader();
        return r.Read() ? Mapear(r) : null;
    }

    public List<Usuario> Listar()
    {
        var lista = new List<Usuario>();
        const string sql = @"SELECT u.*, r.nombre AS rol_nombre FROM usuario u
                             JOIN rol r ON u.id_rol = r.id_rol ORDER BY u.nombre";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(Mapear(r));
        return lista;
    }

    public Usuario? BuscarPorId(int id)
    {
        const string sql = @"SELECT u.*, r.nombre AS rol_nombre FROM usuario u
                             JOIN rol r ON u.id_rol = r.id_rol WHERE u.id_usuario=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Mapear(r) : null;
    }

    public bool ExisteCorreo(string correo)
    {
        const string sql = "SELECT COUNT(*) FROM usuario WHERE correo=@Correo";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Correo", correo);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public int Insertar(Usuario u)
    {
        const string sql = @"INSERT INTO usuario(nombre,apellido,correo,contrasena,telefono,id_rol,estado)
                             VALUES(@N,@A,@C,@P,@T,@R,1); SELECT SCOPE_IDENTITY();";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@N", u.Nombre);
        cmd.Parameters.AddWithValue("@A", u.Apellido);
        cmd.Parameters.AddWithValue("@C", u.Correo);
        cmd.Parameters.AddWithValue("@P", Encriptador.Sha256(u.Contrasena));
        cmd.Parameters.AddWithValue("@T", (object?)u.Telefono ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@R", u.IdRol);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Actualizar(Usuario u)
    {
        const string sql = @"UPDATE usuario SET nombre=@N,apellido=@A,correo=@C,telefono=@T,id_rol=@R
                             WHERE id_usuario=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@N", u.Nombre);
        cmd.Parameters.AddWithValue("@A", u.Apellido);
        cmd.Parameters.AddWithValue("@C", u.Correo);
        cmd.Parameters.AddWithValue("@T", (object?)u.Telefono ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@R", u.IdRol);
        cmd.Parameters.AddWithValue("@Id", u.IdUsuario);
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

    public void CambiarContrasena(int id, string nuevaContrasena)
    {
        const string sql = "UPDATE usuario SET contrasena=@P WHERE id_usuario=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@P", Encriptador.Sha256(nuevaContrasena));
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();
    }

    private static Usuario Mapear(SqlDataReader r) => new()
    {
        IdUsuario  = Convert.ToInt32(r["id_usuario"]),
        Nombre     = r["nombre"]?.ToString() ?? "",
        Apellido   = r["apellido"]?.ToString() ?? "",
        Correo     = r["correo"]?.ToString() ?? "",
        Telefono   = r["telefono"]?.ToString(),
        IdRol      = Convert.ToInt32(r["id_rol"]),
        RolNombre  = r["rol_nombre"]?.ToString() ?? "",
        Estado     = Convert.ToInt32(r["estado"])
    };
}
