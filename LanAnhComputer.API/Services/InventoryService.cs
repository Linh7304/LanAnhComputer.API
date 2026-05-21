using LanAnhComputer.Data;
using LanAnhComputer.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LanAnhComputer.Services;

public interface IInventoryService
{
    Task<List<InventoryItemDto>> GetInventoryAsync(string? status = null);
    Task<List<InventoryItemDto>> GetLowStockProductsAsync();
    Task<InventorySummaryDto> GetSummaryAsync();
    Task<bool> UpdateStockAsync(long productId, UpdateStockDto dto);
    Task<bool> ImportStockAsync(long productId, ImportStockDto dto);
    Task<(bool IsValid, string? Error)> ValidateStockAsync(long productId, int requestedQuantity);
    Task<(bool IsValid, string? Error)> ValidateCartStockAsync(long cartId);
    Task DeductStockAsync(IEnumerable<(long ProductId, int Quantity)> items);
}

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _dbContext;

    public InventoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<InventoryItemDto>> GetInventoryAsync(string? status = null)
    {
        var query = _dbContext.Products.AsNoTracking().AsQueryable();

        query = status switch
        {
            "out" => query.Where(x => x.StockQuantity <= 0),
            "low" => query.Where(x => x.StockQuantity > 0 && x.StockQuantity < x.ReorderLevel),
            _ => query
        };

        return await query
            .OrderBy(x => x.StockQuantity)
            .ThenBy(x => x.ProductName)
            .Select(x => new InventoryItemDto
            {
                ProductId = x.ProductId,
                ProductCode = x.ProductCode,
                ProductName = x.ProductName,
                ProductType = x.ProductType,
                Brand = x.Brand,
                SalePrice = x.SalePrice,
                ImportPrice = x.CostPrice,
                StockQuantity = x.StockQuantity,
                LowStockThreshold = x.ReorderLevel,
                IsActive = x.IsActive,
                StockStatus = x.StockQuantity <= 0
                    ? "OutOfStock"
                    : x.StockQuantity < x.ReorderLevel ? "LowStock" : "InStock"
            })
            .ToListAsync();
    }

    public async Task<List<InventoryItemDto>> GetLowStockProductsAsync()
    {
        return await GetInventoryAsync("low");
    }

    public async Task<InventorySummaryDto> GetSummaryAsync()
    {
        var outOfStockCount = await _dbContext.Products.CountAsync(x => x.StockQuantity <= 0);
        var lowStockCount = await _dbContext.Products.CountAsync(x => x.StockQuantity > 0 && x.StockQuantity < x.ReorderLevel);

        var topSellingProducts = await _dbContext.OrderDetails
            .AsNoTracking()
            .GroupBy(x => new { x.ProductId, x.Product.ProductName })
            .Select(x => new TopSellingProductDto
            {
                ProductId = x.Key.ProductId,
                ProductName = x.Key.ProductName,
                SoldQuantity = x.Sum(i => i.Quantity),
                Revenue = x.Sum(i => i.LineTotal)
            })
            .OrderByDescending(x => x.SoldQuantity)
            .Take(5)
            .ToListAsync();

        return new InventorySummaryDto
        {
            OutOfStockCount = outOfStockCount,
            LowStockCount = lowStockCount,
            TopSellingProducts = topSellingProducts
        };
    }

    public async Task<bool> UpdateStockAsync(long productId, UpdateStockDto dto)
    {
        var product = await _dbContext.Products.FindAsync(productId);
        if (product == null)
        {
            return false;
        }

        product.StockQuantity = dto.StockQuantity;
        product.ReorderLevel = dto.LowStockThreshold;
        product.CostPrice = dto.ImportPrice;
        product.SalePrice = dto.SalePrice;
        product.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ImportStockAsync(long productId, ImportStockDto dto)
    {
        var product = await _dbContext.Products.FindAsync(productId);
        if (product == null)
        {
            return false;
        }

        product.StockQuantity += dto.Quantity;
        if (dto.ImportPrice.HasValue)
        {
            product.CostPrice = dto.ImportPrice.Value;
        }

        product.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<(bool IsValid, string? Error)> ValidateStockAsync(long productId, int requestedQuantity)
    {
        if (requestedQuantity <= 0)
        {
            return (false, "Quantity must be greater than 0.");
        }

        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.IsActive);

        if (product == null)
        {
            return (false, "Product not found or inactive.");
        }

        if (requestedQuantity > product.StockQuantity)
        {
            return (false, $"Product '{product.ProductName}' only has {product.StockQuantity} item(s) in stock.");
        }

        return (true, null);
    }

    public async Task<(bool IsValid, string? Error)> ValidateCartStockAsync(long cartId)
    {
        var cartItems = await _dbContext.CartItems
            .Where(x => x.CartId == cartId)
            .Include(x => x.Product)
            .ToListAsync();

        foreach (var item in cartItems)
        {
            if (item.Product == null || !item.Product.IsActive)
            {
                return (false, $"Product {item.ProductId} not found or inactive.");
            }

            if (item.Quantity > item.Product.StockQuantity)
            {
                return (false, $"Product '{item.Product.ProductName}' only has {item.Product.StockQuantity} item(s) in stock.");
            }
        }

        return (true, null);
    }

    public async Task DeductStockAsync(IEnumerable<(long ProductId, int Quantity)> items)
    {
        foreach (var item in items)
        {
            var product = await _dbContext.Products.FindAsync(item.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException($"Product {item.ProductId} not found.");
            }

            if (item.Quantity > product.StockQuantity)
            {
                throw new InvalidOperationException($"Product '{product.ProductName}' only has {product.StockQuantity} item(s) in stock.");
            }

            product.StockQuantity -= item.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
        }
    }
}
