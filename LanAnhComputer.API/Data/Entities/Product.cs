using LanAnhComputer.API.Data.Entities;

namespace LanAnhComputer.Data.Entities
{
    public class Product
    {
        public long ProductId { get; set; }
        public int CategoryId { get; set; }
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string ProductType { get; set; } = null!;
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Specifications { get; set; }
        public int WarrantyMonths { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public string? ImageUrl { get; set; }
        public int ViewCount { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int SoldQuantity { get; set; }
        public int LowStockThreshold { get; set; }

        public Category Category { get; set; } = null!;
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<ChatbotHistory> ChatbotHistories { get; set; } = new List<ChatbotHistory>();
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public ICollection<ProductSpecification> ProductSpecifications { get; set; } = new List<ProductSpecification>();
        public ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    }
}
