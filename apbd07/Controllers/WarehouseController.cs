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
    public async Task<IActionResult> addProduct([FromBody] Warehouse wh)
    {
        int idWarehouse;
        try
        {
            idWarehouse = await _whRep.addProduct(wh);
        }
        catch (Exception e)
        {
            return NotFound(e.Message);
        }

        return Ok();
    }
}