using LanAnhComputer.Data;
using LanAnhComputer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LanAnhComputer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("revenue/daily")]
    public async Task<ActionResult<IEnumerable<RevenueByDateDto>>> GetDailyRevenue([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var fromDate = from?.Date ?? DateTime.UtcNow.Date.AddDays(-30);
        var toDate = to?.Date ?? DateTime.UtcNow.Date;

        var data = await dbContext.Orders
            .Where(x => x.OrderDate.Date >= fromDate && x.OrderDate.Date <= toDate && x.OrderStatus != "Cancelled")
            .GroupBy(x => x.OrderDate.Date)
            .Select(g => new RevenueByDateDto
            {
                Date = g.Key,
                TotalOrders = g.Count(),
                Revenue = g.Sum(x => x.TotalAmount)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("revenue/monthly")]
    public async Task<ActionResult<IEnumerable<object>>> GetMonthlyRevenue([FromQuery] int year = 0)
    {
        var selectedYear = year == 0 ? DateTime.UtcNow.Year : year;
        var data = await dbContext.Orders
            .Where(x => x.OrderDate.Year == selectedYear && x.OrderStatus != "Cancelled")
            .GroupBy(x => x.OrderDate.Month)
            .Select(g => new
            {
                Year = selectedYear,
                Month = g.Key,
                TotalOrders = g.Count(),
                Revenue = g.Sum(x => x.TotalAmount)
            })
            .OrderBy(x => x.Month)
            .ToListAsync();

        return Ok(data);
    }
}
