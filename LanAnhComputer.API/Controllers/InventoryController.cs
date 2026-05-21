using LanAnhComputer.Dtos;
using LanAnhComputer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanAnhComputer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<InventoryItemDto>>> GetInventory([FromQuery] string? status = null)
    {
        return Ok(await _inventoryService.GetInventoryAsync(status));
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<List<InventoryItemDto>>> GetLowStockProducts()
    {
        return Ok(await _inventoryService.GetLowStockProductsAsync());
    }

    [HttpGet("summary")]
    public async Task<ActionResult<InventorySummaryDto>> GetSummary()
    {
        return Ok(await _inventoryService.GetSummaryAsync());
    }

    [HttpPut("{productId:long}/stock")]
    public async Task<IActionResult> UpdateStock(long productId, [FromBody] UpdateStockDto dto)
    {
        var updated = await _inventoryService.UpdateStockAsync(productId, dto);

        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{productId:long}/import")]
    public async Task<IActionResult> ImportStock(long productId, [FromBody] ImportStockDto dto)
    {
        var updated = await _inventoryService.ImportStockAsync(productId, dto);

        return updated ? NoContent() : NotFound();
    }
}
