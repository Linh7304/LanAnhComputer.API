using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace LanAnhComputer.Dtos;

public class ProductDto
{
    public int ProductId { get; set; }
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
    public long ViewCount { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int SoldQuantity { get; set; }
    public bool IsActive { get; set; }
}

public class ProductDetailsDto : ProductDto
{
    public List<ProductImageDto> Images { get; set; } = [];
    public List<ProductSpecificationDto> DynamicSpecifications { get; set; } = [];
    public List<ProductReviewDto> ReviewsList { get; set; } = [];
}

public class ProductImageDto
{
    public long ProductImageId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}

public class ProductImageUpsertDto
{
    public IFormFile? Image { get; set; }

    public string? ImageUrl { get; set; }

    public string? AltText { get; set; }

    public bool IsPrimary { get; set; }

    public int SortOrder { get; set; }
}

public class ProductSpecificationDto
{
    public long ProductSpecificationId { get; set; }
    public string GroupName { get; set; } = "General";
    public string SpecKey { get; set; } = null!;
    public string SpecValue { get; set; } = null!;
    public int SortOrder { get; set; }
}

public class ProductSpecificationUpsertDto
{
    [MaxLength(100)]
    public string GroupName { get; set; } = "General";

    [Required]
    [MaxLength(100)]
    public string SpecKey { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string SpecValue { get; set; } = null!;

    public int SortOrder { get; set; }
}

public class ProductReviewDto
{
    public long ProductReviewId { get; set; }
    public long ProductId { get; set; }
    public long UserId { get; set; }
    public string UserFullName { get; set; } = null!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProductReviewUpsertDto
{
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }
}

public class ProductReviewVisibilityDto
{
    public bool IsVisible { get; set; }
}

public class ProductUpsertDto
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ProductCode { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string ProductName { get; set; } = null!;

    [Required]
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

    // đường dẫn ảnh lưu DB
    public string? ImageUrl { get; set; }

    // file upload thật
    public IFormFile? Image { get; set; }

    public bool IsActive { get; set; } = true;
}

public class PagedResultDto<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public IReadOnlyList<T> Items { get; set; } = [];

}
