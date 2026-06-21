using LanAnhComputer.Dtos;

namespace LanAnhComputer.Web.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalProducts { get; set; }
        public int PendingOrders { get; set; }
        public List<RevenueByDateDto> DailyRevenue { get; set; } = new();
        public int ChartDays { get; set; } = 30;
    }
}
