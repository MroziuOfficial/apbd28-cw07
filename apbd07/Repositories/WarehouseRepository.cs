using apbd07.Models;

namespace apbd07.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private IConfiguration _config;
    public WarehouseRepository(IConfiguration config)
    {
        _config = config;
    }

    public Task<int> addProduct(Warehouse wh)
    {
        throw new NotImplementedException();
    }
}