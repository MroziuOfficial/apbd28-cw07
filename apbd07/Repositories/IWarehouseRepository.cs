using apbd07.Models;

namespace apbd07.Repositories;

public interface IWarehouseRepository
{
    Task<int> addProduct(Warehouse wh);
}