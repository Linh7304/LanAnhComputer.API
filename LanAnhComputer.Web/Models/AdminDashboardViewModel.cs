using LanAnhComputer.Dtos;

namespace LanAnhComputer.Web.Models
{
    public class AdminDashboardViewModel
    {
        public int OutOfStockCount { get; set; }
        public int LowStockCount { get; set; }
        public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();
    }
}
