using apbd07.Models;
using apbd07.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace apbd07.Controllers;

[ApiController]
[Route("api/warehouse")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseRepository _whRep;

    public WarehouseController(IWarehouseRepository whRep)
    {
        this._whRep = whRep;
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct([FromBody] WarehouseDTO wh)
    {
        if (wh == null || !IsValidWarehouseDTO(wh))
        {
            return BadRequest("Invalid data.");
        }

        return await _whRep.AddProduct(wh);
    }
    
    [HttpPost("procedure")]
    public async Task<IActionResult> AddProductUsingStoredProcedure([FromBody] WarehouseDTO wh)
    {
        if (wh == null || !IsValidWarehouseDTO(wh))
        {
            return BadRequest("Invalid data.");
        }

        return await _whRep.AddProductUsingProcedure(wh);
    }
    
    private bool IsValidWarehouseDTO(WarehouseDTO wh)
    {
        if (wh.IdProduct <= 0 || wh.IdWarehouse <= 0 || wh.Amount <= 0 || wh.CreatedAt == default)
        {
            return false;
        }
        return true;
    }
}