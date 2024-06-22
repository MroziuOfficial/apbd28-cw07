using apbd07.Models;
using Microsoft.AspNetCore.Mvc;

namespace apbd07.Repositories;

public interface IWarehouseRepository
{
    Task<IActionResult> AddProduct(WarehouseDTO wh);
    //Task<int> AddProductUsingProcedure(WarehouseDTO wh);
}