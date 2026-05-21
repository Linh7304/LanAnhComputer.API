namespace LanAnhComputer.Web.ViewModels
{
    public class ProductViewModel
    {
        public long ProductId { get; set; }
        public int CategoryId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public decimal SalePrice { get; set; }
        public decimal? OriginalPrice { get; set; }
        public double Rating { get; set; }
        public int Reviews { get; set; }
        public string? Badge { get; set; }
        public string? ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string Model { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string Specifications { get; set; } = string.Empty;
        public int WarrantyMonths { get; set; }
        public decimal CostPrice { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public long ViewCount { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int SoldQuantity { get; set; }
        public bool IsActive { get; set; }
        public List<ProductImageViewModel> Images { get; set; } = new();
        public List<ProductSpecificationViewModel> DynamicSpecifications { get; set; } = new();
        public List<ProductReviewViewModel> ReviewsList { get; set; } = new();
    }

    public class ProductImageViewModel
    {
        public long ProductImageId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }

    public class ProductSpecificationViewModel
    {
        public long ProductSpecificationId { get; set; }
        public string GroupName { get; set; } = "General";
        public string SpecKey { get; set; } = string.Empty;
        public string SpecValue { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    public class ProductReviewViewModel
    {
        public long ProductReviewId { get; set; }
        public long ProductId { get; set; }
        public long UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsVisible { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
