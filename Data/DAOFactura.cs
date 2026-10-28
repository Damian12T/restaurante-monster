using Microsoft.Data.SqlClient;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOFactura
{
    public int GenerarFactura(int idPedido, List<DetallePedido> detalles)
    {
        double subtotal = detalles.Sum(d => d.PrecioUnitario * d.Cantidad);
        double cargoServicio = subtotal * 0.10;
        double iva = subtotal * 0.12;
        double total = subtotal + cargoServicio + iva;

        const string sql = @"INSERT INTO factura(id_pedido,subtotal,cargo_servicio,iva,total,estado)
                             VALUES(@P,@S,@CS,@I,@T,'EMITIDA'); SELECT SCOPE_IDENTITY();";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@P",  idPedido);
        cmd.Parameters.AddWithValue("@S",  subtotal);
        cmd.Parameters.AddWithValue("@CS", cargoServicio);
        cmd.Parameters.AddWithValue("@I",  iva);
        cmd.Parameters.AddWithValue("@T",  total);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public Factura? BuscarPorId(int id)
    {
        const string sql = @"SELECT f.*, p.id_usuario, p.id_mesero,
                             CONCAT(COALESCE(uc.nombre,'Invitado'),' ',COALESCE(uc.apellido,'')) AS cliente_nombre,
                             CONCAT(COALESCE(um.nombre,''),' ',COALESCE(um.apellido,'')) AS mesero_nombre
                             FROM factura f
                             JOIN pedido p ON f.id_pedido=p.id_pedido
                             LEFT JOIN usuario uc ON p.id_usuario=uc.id_usuario
                             LEFT JOIN usuario um ON p.id_mesero=um.id_usuario
                             WHERE f.id_factura=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Mapear(r) : null;
    }

    public Factura? BuscarPorPedido(int idPedido)
    {
        const string sql = @"SELECT f.*, p.id_usuario, p.id_mesero,
                             CONCAT(COALESCE(uc.nombre,'Invitado'),' ',COALESCE(uc.apellido,'')) AS cliente_nombre,
                             CONCAT(COALESCE(um.nombre,''),' ',COALESCE(um.apellido,'')) AS mesero_nombre
                             FROM factura f
                             JOIN pedido p ON f.id_pedido=p.id_pedido
                             LEFT JOIN usuario uc ON p.id_usuario=uc.id_usuario
                             LEFT JOIN usuario um ON p.id_mesero=um.id_usuario
                             WHERE f.id_pedido=@IdP";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@IdP", idPedido);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Mapear(r) : null;
    }

    public List<Factura> Listar()
    {
        var lista = new List<Factura>();
        const string sql = @"SELECT f.*, p.id_usuario, p.id_mesero,
                             CONCAT(COALESCE(uc.nombre,'Invitado'),' ',COALESCE(uc.apellido,'')) AS cliente_nombre,
                             CONCAT(COALESCE(um.nombre,''),' ',COALESCE(um.apellido,'')) AS mesero_nombre
                             FROM factura f
                             JOIN pedido p ON f.id_pedido=p.id_pedido
                             LEFT JOIN usuario uc ON p.id_usuario=uc.id_usuario
                             LEFT JOIN usuario um ON p.id_mesero=um.id_usuario
                             ORDER BY f.fecha DESC";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(Mapear(r));
        return lista;
    }

    public Dictionary<string, double> EstadisticasHoy()
    {
        var stats = new Dictionary<string, double>();
        const string sql = @"SELECT COUNT(*) AS pedidos, COALESCE(SUM(total),0) AS ventas
                             FROM factura WHERE CAST(fecha AS DATE)=CAST(GETDATE() AS DATE) AND estado='EMITIDA'";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        using var r = cmd.ExecuteReader();
        if (r.Read())
        {
            stats["pedidos"] = Convert.ToDouble(r["pedidos"]);
            stats["ventas"]  = Convert.ToDouble(r["ventas"]);
        }
        return stats;
    }

    private static Factura Mapear(SqlDataReader r) => new()
    {
        IdFactura      = Convert.ToInt32(r["id_factura"]),
        IdPedido       = Convert.ToInt32(r["id_pedido"]),
        Fecha          = r["fecha"]?.ToString(),
        Subtotal       = Convert.ToDouble(r["subtotal"]),
        CargoServicio  = Convert.ToDouble(r["cargo_servicio"]),
        Iva            = Convert.ToDouble(r["iva"]),
        Total          = Convert.ToDouble(r["total"]),
        Estado         = r["estado"]?.ToString() ?? "",
        ClienteNombre  = r["cliente_nombre"]?.ToString(),
        MeseroNombre   = r["mesero_nombre"]?.ToString()
    };
}
