using apbd07.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;

namespace apbd07.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private IConfiguration _config;
    public WarehouseRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> addProduct(Warehouse wh)
    {
        using var conn = new SqlConnection(_config.GetConnectionString("Default"));
        using var command = new SqlCommand();
        command.Connection = conn;
        await conn.OpenAsync();
        //command.CommandText

        command.Parameters.AddWithValue("IdProduct", wh.IdProduct);
        command.Parameters.AddWithValue("Amount", wh.Amount);
        command.Parameters.AddWithValue("CreatedAt", wh.CreatedAt);

        var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        int idOrder = int.Parse(reader["IdOrder"].ToString());
        await reader.CloseAsync();
        command.Parameters.Clear();
        //command.CommandText

        command.Parameters.AddWithValue("IdProduct", wh.IdProduct);
        
        reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        double price = double.Parse(reader["Price"].ToString());
        await reader.CloseAsync();
        command.Parameters.Clear();
        //command.CommandText
        return 0;
    }
}