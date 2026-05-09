using System.ComponentModel.DataAnnotations;

namespace LanAnhComputer.Dtos;

public class ProductDto
{
    public long ProductId { get; set; }
    public int CategoryId { get; set; }
    public string ProductCode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string ProductType { get; set; } = null!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Specifications { get; set; }
    public int WarrantyMonths { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
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
    public string? Specifications { get; set; }
    public int WarrantyMonths { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public string? ImageUrl { get; set; }
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
