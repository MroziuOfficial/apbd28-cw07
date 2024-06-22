using apbd07.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc;

namespace apbd07.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly IConfiguration _config;
    public WarehouseRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IActionResult> AddProduct(WarehouseDTO wh)
    {
        var productQuery = "SELECT 1 FROM Product WHERE IdProduct = @IdProduct";
        var warehouseQuery = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
        var orderQuery = @"SELECT IdOrder, Amount, CreatedAt FROM [Order] 
                           WHERE IdProduct = @IdProduct AND Amount = @Amount";
        var dateCheckQuery = @"SELECT IdOrder FROM [Order]
                               WHERE IdOrder = @IdOrder AND CreatedAt <= @CreatedAt";
        var checkFulfilledQuery = "SELECT 1 FROM Product_Warehouse WHERE IdOrder = @IdOrder";
        var updateOrderQuery = "UPDATE [Order] SET FulfilledAt = @Now WHERE IdOrder = @IdOrder";
        var insertProductWarehouseQuery = @"INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) 
                                            VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @Now); 
                                            SELECT CAST(SCOPE_IDENTITY() as int)";

        using SqlConnection connection = new SqlConnection(_config.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;

        await connection.OpenAsync();
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = (SqlTransaction)transaction;

        try
        {
            command.Parameters.Clear();
            command.CommandText = productQuery;
            command.Parameters.AddWithValue("@IdProduct", wh.IdProduct);
            var productExists = await command.ExecuteScalarAsync();
            if (productExists == null)
                return new NotFoundObjectResult("Product does not exist");

            command.Parameters.Clear();
            command.CommandText = warehouseQuery;
            command.Parameters.AddWithValue("@IdWarehouse", wh.IdWarehouse);
            var warehouseExists = await command.ExecuteScalarAsync();
            if (warehouseExists == null)
                return new NotFoundObjectResult("Warehouse does not exist");

            command.Parameters.Clear();
            command.CommandText = orderQuery;
            command.Parameters.AddWithValue("@IdProduct", wh.IdProduct);
            command.Parameters.AddWithValue("@Amount", wh.Amount);

            var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                await reader.CloseAsync();
                return new NotFoundObjectResult("Order does not exist");
            }

            var idOrder = reader.GetInt32(reader.GetOrdinal("IdOrder"));
            var orderCreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));
            await reader.CloseAsync();

            if (orderCreatedAt >= wh.CreatedAt)
                return new BadRequestObjectResult("Order creation date is not earlier than the creation date in the request");

            var priceQuery = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.Clear();
            command.CommandText = priceQuery;
            command.Parameters.AddWithValue("@IdProduct", wh.IdProduct);

            var price = (decimal)await command.ExecuteScalarAsync();
            var totalPrice = price * wh.Amount;

            command.Parameters.Clear();
            command.CommandText = checkFulfilledQuery;
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            var isFulfilled = await command.ExecuteScalarAsync();
            if (isFulfilled != null)
                return new BadRequestObjectResult("Order is already fulfilled");

            command.Parameters.Clear();
            command.CommandText = updateOrderQuery;
            command.Parameters.AddWithValue("@Now", DateTime.Now);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            await command.ExecuteNonQueryAsync();

            command.Parameters.Clear();
            command.CommandText = insertProductWarehouseQuery;
            command.Parameters.AddWithValue("@IdWarehouse", wh.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", wh.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@Amount", wh.Amount);
            command.Parameters.AddWithValue("@Price", totalPrice);
            command.Parameters.AddWithValue("@Now", DateTime.Now);

            var idProductWarehouse = await command.ExecuteScalarAsync();
            await transaction.CommitAsync();
            return new OkObjectResult(new { IdProductWarehouse = Convert.ToInt32(idProductWarehouse) });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
    public async Task<IActionResult> AddProductUsingProcedure(WarehouseDTO wh)
    {
        using SqlConnection connection = new SqlConnection(_config.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand("AddProductToWarehouse", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@IdProduct", wh.IdProduct);
        command.Parameters.AddWithValue("@IdWarehouse", wh.IdWarehouse);
        command.Parameters.AddWithValue("@Amount", wh.Amount);
        command.Parameters.AddWithValue("@CreatedAt", wh.CreatedAt);

        var newIdOutput = new SqlParameter("@NewId", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };
        command.Parameters.Add(newIdOutput);

        try
        {
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return new OkObjectResult(new { IdProductWarehouse = (int)newIdOutput.Value });
        }
        catch (SqlException sqlEx)
        {
            Console.WriteLine($"SQL Error: {sqlEx.Message}");
            return new ObjectResult(new { error = sqlEx.Message }) { StatusCode = StatusCodes.Status500InternalServerError };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return new ObjectResult(new { error = ex.Message }) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }
}