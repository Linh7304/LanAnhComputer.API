namespace LanAnhComputer.Web.ViewModels
{
    public class ProductViewModel
    {
        public long ProductId { get; set; }

        public int CategoryId { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string ProductType { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public string Specifications { get; set; }

        public int WarrantyMonths { get; set; }

        public decimal CostPrice { get; set; }

        public decimal SalePrice { get; set; }

        public int StockQuantity { get; set; }

        public int ReorderLevel { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; }

        // Additional properties for frontend display
        public decimal? OriginalPrice { get; set; }

        public double Rating { get; set; }

        public int Reviews { get; set; }

        public string? Badge { get; set; }
    }
}