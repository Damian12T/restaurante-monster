using Microsoft.Data.SqlClient;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOPedido
{
    public int Crear(int idUsuario, int idMesero, string tipoPedido)
    {
        const string sql = @"INSERT INTO pedido(id_usuario,id_mesero,tipo_pedido,estado)
                             VALUES(@U,@M,@T,'PENDIENTE'); SELECT SCOPE_IDENTITY();";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@U", idUsuario > 0 ? (object)idUsuario : DBNull.Value);
        cmd.Parameters.AddWithValue("@M", idMesero  > 0 ? (object)idMesero  : DBNull.Value);
        cmd.Parameters.AddWithValue("@T", tipoPedido);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void AgregarDetalle(DetallePedido d)
    {
        const string sql = @"INSERT INTO detalle_pedido(id_pedido,id_plato,id_bebida,cantidad,precio_unitario,subtotal)
                             VALUES(@P,@Pl,@B,@C,@Pu,@S)";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@P",  d.IdPedido);
        cmd.Parameters.AddWithValue("@Pl", d.IdPlato  > 0 ? (object)d.IdPlato  : DBNull.Value);
        cmd.Parameters.AddWithValue("@B",  d.IdBebida > 0 ? (object)d.IdBebida : DBNull.Value);
        cmd.Parameters.AddWithValue("@C",  d.Cantidad);
        cmd.Parameters.AddWithValue("@Pu", d.PrecioUnitario);
        cmd.Parameters.AddWithValue("@S",  d.PrecioUnitario * d.Cantidad);
        cmd.ExecuteNonQuery();
    }

    public void ActualizarEstado(int idPedido, string estado)
    {
        const string sql = "UPDATE pedido SET estado=@E WHERE id_pedido=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@E",  estado);
        cmd.Parameters.AddWithValue("@Id", idPedido);
        cmd.ExecuteNonQuery();
    }

    public Pedido? BuscarPorId(int id)
    {
        const string sql = @"SELECT p.*,
                             CONCAT(COALESCE(uc.nombre,'Invitado'),' ',COALESCE(uc.apellido,'')) AS cliente_nombre,
                             CONCAT(COALESCE(um.nombre,''),' ',COALESCE(um.apellido,'')) AS mesero_nombre
                             FROM pedido p
                             LEFT JOIN usuario uc ON p.id_usuario=uc.id_usuario
                             LEFT JOIN usuario um ON p.id_mesero=um.id_usuario
                             WHERE p.id_pedido=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        var ped = Mapear(r);
        r.Close();
        ped.Detalles = ObtenerDetalles(id);
        return ped;
    }

    public List<Pedido> ListarTodos()
    {
        return Listar(null, -1);
    }

    public List<Pedido> ListarPorMesero(int idMesero)
    {
        return Listar("AND p.id_mesero=@Param AND CAST(p.fecha AS DATE)=CAST(GETDATE() AS DATE)", idMesero);
    }

    private List<Pedido> Listar(string? filtro, int paramId)
    {
        var lista = new List<Pedido>();
        var sql = @"SELECT p.*,
                    CONCAT(COALESCE(uc.nombre,'Invitado'),' ',COALESCE(uc.apellido,'')) AS cliente_nombre,
                    CONCAT(COALESCE(um.nombre,''),' ',COALESCE(um.apellido,'')) AS mesero_nombre
                    FROM pedido p
                    LEFT JOIN usuario uc ON p.id_usuario=uc.id_usuario
                    LEFT JOIN usuario um ON p.id_mesero=um.id_usuario
                    WHERE 1=1 " + (filtro ?? "") + " ORDER BY p.fecha DESC";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        if (paramId > 0) cmd.Parameters.AddWithValue("@Param", paramId);
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(Mapear(r));
        return lista;
    }

    public List<DetallePedido> ObtenerDetalles(int idPedido)
    {
        var lista = new List<DetallePedido>();
        const string sql = @"SELECT dp.*,
                             COALESCE(p.nombre, b.nombre) AS nombre_item,
                             COALESCE(p.imagen, b.imagen) AS imagen_item,
                             CASE WHEN dp.id_plato IS NOT NULL THEN 'PLATO' ELSE 'BEBIDA' END AS tipo_item
                             FROM detalle_pedido dp
                             LEFT JOIN plato p   ON dp.id_plato=p.id_plato
                             LEFT JOIN bebida b  ON dp.id_bebida=b.id_bebida
                             WHERE dp.id_pedido=@IdP";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@IdP", idPedido);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            lista.Add(new DetallePedido
            {
                IdDetalle       = Convert.ToInt32(r["id_detalle"]),
                IdPedido        = Convert.ToInt32(r["id_pedido"]),
                IdPlato         = r["id_plato"]  == DBNull.Value ? 0 : Convert.ToInt32(r["id_plato"]),
                IdBebida        = r["id_bebida"] == DBNull.Value ? 0 : Convert.ToInt32(r["id_bebida"]),
                Cantidad        = Convert.ToInt32(r["cantidad"]),
                PrecioUnitario  = Convert.ToDouble(r["precio_unitario"]),
                Subtotal        = Convert.ToDouble(r["subtotal"]),
                NombreItem      = r["nombre_item"]?.ToString() ?? "",
                Imagen          = r["imagen_item"]?.ToString(),
                TipoItem        = r["tipo_item"]?.ToString()
            });
        }
        return lista;
    }

    private static Pedido Mapear(SqlDataReader r) => new()
    {
        IdPedido      = Convert.ToInt32(r["id_pedido"]),
        IdUsuario     = r["id_usuario"] == DBNull.Value ? 0 : Convert.ToInt32(r["id_usuario"]),
        IdMesero      = r["id_mesero"]  == DBNull.Value ? 0 : Convert.ToInt32(r["id_mesero"]),
        Fecha         = r["fecha"]?.ToString(),
        Estado        = r["estado"]?.ToString() ?? "",
        TipoPedido    = r["tipo_pedido"]?.ToString() ?? "",
        ClienteNombre = r["cliente_nombre"]?.ToString(),
        MeseroNombre  = r["mesero_nombre"]?.ToString()
    };
}
