using System.ComponentModel.DataAnnotations;

namespace LanAnhComputer.Dtos;

public class InventoryItemDto
{
    public long ProductId { get; set; }
    public string ProductCode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string ProductType { get; set; } = null!;
    public string? Brand { get; set; }
    public decimal SalePrice { get; set; }
    public decimal ImportPrice { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public bool IsActive { get; set; }
    public string StockStatus { get; set; } = null!;
}

public class UpdateStockDto
{
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Range(0, int.MaxValue)]
    public int LowStockThreshold { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ImportPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal SalePrice { get; set; }
}

public class ImportStockDto
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ImportPrice { get; set; }
}

public class InventorySummaryDto
{
    public int OutOfStockCount { get; set; }
    public int LowStockCount { get; set; }
    public List<TopSellingProductDto> TopSellingProducts { get; set; } = [];
}

public class TopSellingProductDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int SoldQuantity { get; set; }
    public decimal Revenue { get; set; }
}
