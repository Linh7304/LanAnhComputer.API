using LanAnhComputer.Constants;
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
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardOverviewDto>> GetDashboardOverview([FromQuery] int days = 30)
    {
        var periodDays = Math.Clamp(days, 7, 90);
        var toDate = DateTime.UtcNow.Date;
        var fromDate = toDate.AddDays(-(periodDays - 1));

        var validOrders = dbContext.Orders
            .Where(x => x.OrderStatus != OrderStatuses.Cancelled);

        var totalCustomers = await dbContext.Users
            .CountAsync(x => x.Role == "Customer" && x.IsActive);

        var totalOrders = await validOrders.CountAsync();

        var totalRevenue = await validOrders.SumAsync(x => (decimal?)x.TotalAmount) ?? 0;

        var totalProducts = await dbContext.Products
            .CountAsync(x => x.IsActive);

        var pendingOrders = await dbContext.Orders
            .CountAsync(x => x.OrderStatus == OrderStatuses.Pending);

        var revenueByDate = await validOrders
            .Where(x => x.OrderDate.Date >= fromDate && x.OrderDate.Date <= toDate)
            .GroupBy(x => x.OrderDate.Date)
            .Select(g => new RevenueByDateDto
            {
                Date = g.Key,
                TotalOrders = g.Count(),
                Revenue = g.Sum(x => x.TotalAmount)
            })
            .ToListAsync();

        var revenueLookup = revenueByDate.ToDictionary(x => x.Date.Date);

        var dailyRevenue = new List<RevenueByDateDto>();
        for (var date = fromDate; date <= toDate; date = date.AddDays(1))
        {
            if (revenueLookup.TryGetValue(date, out var item))
            {
                dailyRevenue.Add(item);
            }
            else
            {
                dailyRevenue.Add(new RevenueByDateDto
                {
                    Date = date,
                    TotalOrders = 0,
                    Revenue = 0
                });
            }
        }

        return Ok(new DashboardOverviewDto
        {
            TotalCustomers = totalCustomers,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            TotalProducts = totalProducts,
            PendingOrders = pendingOrders,
            DailyRevenue = dailyRevenue
        });
    }

    [HttpGet("revenue/daily")]
    public async Task<ActionResult<IEnumerable<RevenueByDateDto>>> GetDailyRevenue([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var fromDate = from?.Date ?? DateTime.UtcNow.Date.AddDays(-30);
        var toDate = to?.Date ?? DateTime.UtcNow.Date;

        var data = await dbContext.Orders
            .Where(x => x.OrderDate.Date >= fromDate && x.OrderDate.Date <= toDate && x.OrderStatus != LanAnhComputer.Constants.OrderStatuses.Cancelled)
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
            .Where(x => x.OrderDate.Year == selectedYear && x.OrderStatus != LanAnhComputer.Constants.OrderStatuses.Cancelled)
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
