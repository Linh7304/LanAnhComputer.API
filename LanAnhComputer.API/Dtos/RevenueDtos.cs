namespace LanAnhComputer.Dtos;

public class RevenueByDateDto
{
    public DateTime Date { get; set; }
    public int TotalOrders { get; set; }
    public decimal Revenue { get; set; }
}

public class DashboardOverviewDto
{
    public int TotalCustomers { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalProducts { get; set; }
    public int PendingOrders { get; set; }
    public List<RevenueByDateDto> DailyRevenue { get; set; } = [];
}
