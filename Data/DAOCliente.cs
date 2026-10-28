using Microsoft.Data.SqlClient;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOCliente
{
    public List<Cliente> Listar()
    {
        var lista = new List<Cliente>();
        const string sql = @"SELECT c.*, u.nombre, u.apellido, u.correo, u.telefono, u.estado
                             FROM cliente c JOIN usuario u ON c.id_usuario=u.id_usuario
                             ORDER BY u.nombre";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(Mapear(r));
        return lista;
    }

    public Cliente? BuscarPorId(int id)
    {
        const string sql = @"SELECT c.*, u.nombre, u.apellido, u.correo, u.telefono, u.estado
                             FROM cliente c JOIN usuario u ON c.id_usuario=u.id_usuario
                             WHERE c.id_cliente=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Mapear(r) : null;
    }

    public Cliente? BuscarPorUsuario(int idUsuario)
    {
        const string sql = @"SELECT c.*, u.nombre, u.apellido, u.correo, u.telefono, u.estado
                             FROM cliente c JOIN usuario u ON c.id_usuario=u.id_usuario
                             WHERE c.id_usuario=@IdU";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@IdU", idUsuario);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Mapear(r) : null;
    }

    public void Insertar(Cliente c)
    {
        const string sql = @"INSERT INTO cliente(id_usuario,direccion,fecha_nacimiento)
                             VALUES(@IdU,@Dir,@FN)";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@IdU", c.IdUsuario);
        cmd.Parameters.AddWithValue("@Dir", (object?)c.Direccion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FN",  string.IsNullOrEmpty(c.FechaNacimiento)
            ? DBNull.Value : (object)c.FechaNacimiento);
        cmd.ExecuteNonQuery();
    }

    public void Actualizar(Cliente c)
    {
        const string sql = @"UPDATE usuario SET nombre=@N,apellido=@A,correo=@Co,telefono=@T
                             WHERE id_usuario=@IdU;
                             UPDATE cliente SET direccion=@Dir,fecha_nacimiento=@FN
                             WHERE id_usuario=@IdU;";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@N",   c.Nombre);
        cmd.Parameters.AddWithValue("@A",   c.Apellido);
        cmd.Parameters.AddWithValue("@Co",  c.Correo);
        cmd.Parameters.AddWithValue("@T",   (object?)c.Telefono ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Dir", (object?)c.Direccion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FN",  string.IsNullOrEmpty(c.FechaNacimiento)
            ? DBNull.Value : (object)c.FechaNacimiento);
        cmd.Parameters.AddWithValue("@IdU", c.IdUsuario);
        cmd.ExecuteNonQuery();
    }

    public bool TienePedidosActivos(int idUsuario)
    {
        const string sql = @"SELECT COUNT(*) FROM pedido
                             WHERE id_usuario=@IdU AND estado='PENDIENTE'";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@IdU", idUsuario);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    private static Cliente Mapear(SqlDataReader r) => new()
    {
        IdCliente       = Convert.ToInt32(r["id_cliente"]),
        IdUsuario       = Convert.ToInt32(r["id_usuario"]),
        Nombre          = r["nombre"]?.ToString() ?? "",
        Apellido        = r["apellido"]?.ToString() ?? "",
        Correo          = r["correo"]?.ToString() ?? "",
        Telefono        = r["telefono"]?.ToString(),
        Direccion       = r["direccion"]?.ToString(),
        FechaNacimiento = r["fecha_nacimiento"] == DBNull.Value
            ? null : Convert.ToDateTime(r["fecha_nacimiento"]).ToString("yyyy-MM-dd"),
        Estado          = Convert.ToInt32(r["estado"])
    };
}
