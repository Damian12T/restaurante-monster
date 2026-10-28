using Microsoft.Data.SqlClient;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOStock
{
    public List<Stock> ListarTodo()
    {
        var lista = new List<Stock>();
        const string sqlPlatos  = @"SELECT s.*, p.nombre AS nombre_item, p.precio, 'PLATO' AS tipo_item
                                   FROM stock s JOIN plato p ON s.id_plato=p.id_plato
                                   WHERE s.id_plato IS NOT NULL ORDER BY p.nombre";
        const string sqlBebidas = @"SELECT s.*, b.nombre AS nombre_item, b.precio, 'BEBIDA' AS tipo_item
                                   FROM stock s JOIN bebida b ON s.id_bebida=b.id_bebida
                                   WHERE s.id_bebida IS NOT NULL ORDER BY b.nombre";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using (var cmd = new SqlCommand(sqlPlatos, cn))
        using (var r = cmd.ExecuteReader())
            while (r.Read()) lista.Add(Mapear(r));
        using (var cmd = new SqlCommand(sqlBebidas, cn))
        using (var r = cmd.ExecuteReader())
            while (r.Read()) lista.Add(Mapear(r));
        return lista;
    }

    public List<Stock> ListarStockBajo()
    {
        var lista = new List<Stock>();
        const string sql = @"SELECT s.*, p.nombre AS nombre_item, p.precio, 'PLATO' AS tipo_item
                             FROM stock s JOIN plato p ON s.id_plato=p.id_plato
                             WHERE s.id_plato IS NOT NULL AND s.cantidad<=s.stock_minimo
                             UNION
                             SELECT s.*, b.nombre AS nombre_item, b.precio, 'BEBIDA' AS tipo_item
                             FROM stock s JOIN bebida b ON s.id_bebida=b.id_bebida
                             WHERE s.id_bebida IS NOT NULL AND s.cantidad<=s.stock_minimo
                             ORDER BY nombre_item";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(Mapear(r));
        return lista;
    }

    public void ActualizarCantidad(int idStock, int cantidad, int stockMinimo)
    {
        const string sql = "UPDATE stock SET cantidad=@C,stock_minimo=@M WHERE id_stock=@Id";
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@C",  cantidad);
        cmd.Parameters.AddWithValue("@M",  stockMinimo);
        cmd.Parameters.AddWithValue("@Id", idStock);
        cmd.ExecuteNonQuery();
    }

    public void DescontarStock(int idPlato, int idBebida, int cantidad)
    {
        string sql;
        int itemId;
        if (idPlato > 0)
        {
            sql    = @"UPDATE stock SET cantidad = CASE WHEN cantidad > @C THEN cantidad - @C ELSE 0 END WHERE id_plato=@Id";
            itemId = idPlato;
        }
        else
        {
            sql    = @"UPDATE stock SET cantidad = CASE WHEN cantidad > @C THEN cantidad - @C ELSE 0 END WHERE id_bebida=@Id";
            itemId = idBebida;
        }
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@C",  cantidad);
        cmd.Parameters.AddWithValue("@Id", itemId);
        cmd.ExecuteNonQuery();
    }

    public int GetStockDisponible(int idPlato, int idBebida)
    {
        string sql;
        int itemId;
        if (idPlato > 0)
        {
            sql    = "SELECT cantidad FROM stock WHERE id_plato=@Id";
            itemId = idPlato;
        }
        else
        {
            sql    = "SELECT cantidad FROM stock WHERE id_bebida=@Id";
            itemId = idBebida;
        }
        using var cn = Conexion.GetConexion();
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Id", itemId);
        var result = cmd.ExecuteScalar();
        return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
    }

    private static Stock Mapear(SqlDataReader r) => new()
    {
        IdStock      = Convert.ToInt32(r["id_stock"]),
        IdPlato      = r["id_plato"]  == DBNull.Value ? 0 : Convert.ToInt32(r["id_plato"]),
        IdBebida     = r["id_bebida"] == DBNull.Value ? 0 : Convert.ToInt32(r["id_bebida"]),
        Cantidad     = Convert.ToInt32(r["cantidad"]),
        StockMinimo  = Convert.ToInt32(r["stock_minimo"]),
        NombreItem   = r["nombre_item"]?.ToString() ?? "",
        TipoItem     = r["tipo_item"]?.ToString() ?? "",
        Precio       = Convert.ToDouble(r["precio"])
    };
}
