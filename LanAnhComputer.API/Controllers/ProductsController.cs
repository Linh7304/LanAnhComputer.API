using AutoMapper;
using AutoMapper.QueryableExtensions;
using LanAnhComputer.Data;
using LanAnhComputer.Data.Entities;
using LanAnhComputer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LanAnhComputer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext dbContext;
    private readonly IMapper mapper;

    public ProductsController(
        AppDbContext dbContext,
        IMapper mapper)
    {
        this.dbContext = dbContext;
        this.mapper = mapper;
    }

    // =========================
    // GET ALL
    // =========================
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResultDto<ProductDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? productType = null,
        [FromQuery] string? sort = null)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = dbContext.Products.AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(x =>
                x.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(productType))
        {
            query = query.Where(x =>
                x.ProductType == productType);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();

            query = query.Where(x =>
                x.ProductName.Contains(normalizedSearch));
        }

        var totalItems = await query.CountAsync();

        query = sort switch
        {
            "price_asc" => query.OrderBy(x => x.SalePrice),

            "price_desc" => query.OrderByDescending(x => x.SalePrice),

            "newest" => query.OrderByDescending(x => x.CreatedAt),

            _ => query.OrderBy(x => x.ProductId)
        };

        var result = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<ProductDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(new PagedResultDto<ProductDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            Items = result
        });
    }

    // =========================
    // GET BY ID
    // =========================
    [HttpGet("{id:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDetailsDto>> GetById(long id)
    {
        var entity = await dbContext.Products.FirstOrDefaultAsync(x => x.ProductId == id);

        if (entity == null)
            return NotFound();

        entity.ViewCount += 1;
        await dbContext.SaveChangesAsync();

        var dto = await dbContext.Products
            .AsNoTracking()
            .Where(x => x.ProductId == id)
            .Select(x => new ProductDetailsDto
            {
                ProductId = x.ProductId,
                CategoryId = x.CategoryId,
                ProductCode = x.ProductCode,
                ProductName = x.ProductName,
                ProductType = x.ProductType,
                Brand = x.Brand,
                Model = x.Model,
                ShortDescription = x.ShortDescription,
                Description = x.Description,
                ThumbnailUrl = x.ThumbnailUrl,
                Specifications = x.Specifications,
                WarrantyMonths = x.WarrantyMonths,
                CostPrice = x.CostPrice,
                SalePrice = x.SalePrice,
                StockQuantity = x.StockQuantity,
                ReorderLevel = x.ReorderLevel,
                ImageUrl = x.ImageUrl,
                ViewCount = x.ViewCount,
                AverageRating = x.AverageRating,
                TotalReviews = x.TotalReviews,
                SoldQuantity = x.SoldQuantity,
                IsActive = x.IsActive,
                Images = x.ProductImages
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.SortOrder)
                    .Select(i => new ProductImageDto
                    {
                        ProductImageId = i.ProductImageId,
                        ImageUrl = i.ImageUrl,
                        AltText = i.AltText,
                        IsPrimary = i.IsPrimary,
                        SortOrder = i.SortOrder
                    })
                    .ToList(),
                DynamicSpecifications = x.ProductSpecifications
                    .OrderBy(s => s.GroupName)
                    .ThenBy(s => s.SortOrder)
                    .Select(s => new ProductSpecificationDto
                    {
                        ProductSpecificationId = s.ProductSpecificationId,
                        GroupName = s.GroupName,
                        SpecKey = s.SpecKey,
                        SpecValue = s.SpecValue,
                        SortOrder = s.SortOrder
                    })
                    .ToList(),
                ReviewsList = x.ProductReviews
                    .Where(r => r.IsVisible)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new ProductReviewDto
                    {
                        ProductReviewId = r.ProductReviewId,
                        ProductId = r.ProductId,
                        UserId = r.UserId,
                        UserFullName = r.User.FullName,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        IsVisible = r.IsVisible,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt
                    })
                    .ToList()
            })
            .FirstAsync();

        if (!dto.Images.Any())
        {
            var fallbackImage = dto.ThumbnailUrl ?? dto.ImageUrl;
            if (!string.IsNullOrWhiteSpace(fallbackImage))
            {
                dto.Images.Add(new ProductImageDto
                {
                    ImageUrl = fallbackImage,
                    AltText = dto.ProductName,
                    IsPrimary = true
                });
            }
        }

        return Ok(dto);
    }

    // =========================
    // CREATE
    // =========================
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> Create([FromForm] ProductUpsertDto dto)
    {
        var productType = await ResolveProductTypeFromCategoryAsync(dto.CategoryId);

        if (productType == null)
        {
            return BadRequest("Danh mục không tồn tại");
        }

        var imageUrl = await SaveUploadedProductImageAsync(dto.Image);

        var entity = mapper.Map<Product>(dto);

        entity.ProductType = productType;
        entity.ImageUrl = imageUrl;
        entity.ThumbnailUrl = dto.ThumbnailUrl ?? imageUrl;

        entity.CreatedAt = DateTime.UtcNow;

        dbContext.Products.Add(entity);

        await dbContext.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            dbContext.ProductImages.Add(new ProductImage
            {
                ProductId = entity.ProductId,
                ImageUrl = imageUrl,
                AltText = entity.ProductName,
                IsPrimary = true,
                SortOrder = 0,
                CreatedAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = entity.ProductId },
            mapper.Map<ProductDto>(entity)
        );
    }

    // =========================
    // UPDATE
    // =========================
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        long id,
        [FromForm] ProductUpsertDto dto)
    {
        var entity = await dbContext.Products.FindAsync(id);

        if (entity == null)
        {
            return NotFound();
        }

        var productType = await ResolveProductTypeFromCategoryAsync(dto.CategoryId);

        if (productType == null)
        {
            return BadRequest("Danh mục không tồn tại");
        }

        mapper.Map(dto, entity);
        entity.ProductType = productType;

        if (dto.Image != null)
        {
            entity.ImageUrl = await SaveUploadedProductImageAsync(dto.Image);
            entity.ThumbnailUrl = entity.ImageUrl;

            var currentPrimary = await dbContext.ProductImages
                .Where(x => x.ProductId == id && x.IsPrimary)
                .ToListAsync();
            currentPrimary.ForEach(x => x.IsPrimary = false);

            dbContext.ProductImages.Add(new ProductImage
            {
                ProductId = id,
                ImageUrl = entity.ImageUrl,
                AltText = entity.ProductName,
                IsPrimary = true,
                SortOrder = 0,
                CreatedAt = DateTime.UtcNow
            });
        }

        entity.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id:long}/images")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductImageDto>> AddImage(long id, [FromForm] ProductImageUpsertDto dto)
    {
        var product = await dbContext.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        var imageUrl = await SaveUploadedProductImageAsync(dto.Image);
        imageUrl ??= dto.ImageUrl;

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return BadRequest("Image file or image URL is required.");
        }

        if (dto.IsPrimary)
        {
            var currentPrimary = await dbContext.ProductImages
                .Where(x => x.ProductId == id && x.IsPrimary)
                .ToListAsync();
            currentPrimary.ForEach(x => x.IsPrimary = false);
            product.ImageUrl = imageUrl;
            product.ThumbnailUrl = imageUrl;
        }

        var image = new ProductImage
        {
            ProductId = id,
            ImageUrl = imageUrl,
            AltText = dto.AltText ?? product.ProductName,
            IsPrimary = dto.IsPrimary,
            SortOrder = dto.SortOrder,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.ProductImages.Add(image);
        await dbContext.SaveChangesAsync();

        return Ok(new ProductImageDto
        {
            ProductImageId = image.ProductImageId,
            ImageUrl = image.ImageUrl,
            AltText = image.AltText,
            IsPrimary = image.IsPrimary,
            SortOrder = image.SortOrder
        });
    }

    [HttpPut("{id:long}/images/{imageId:long}/primary")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetPrimaryImage(long id, long imageId)
    {
        var product = await dbContext.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        var image = await dbContext.ProductImages
            .FirstOrDefaultAsync(x => x.ProductImageId == imageId && x.ProductId == id);
        if (image == null)
        {
            return NotFound();
        }

        var currentPrimary = await dbContext.ProductImages
            .Where(x => x.ProductId == id && x.IsPrimary)
            .ToListAsync();
        currentPrimary.ForEach(x => x.IsPrimary = false);

        image.IsPrimary = true;
        product.ImageUrl = image.ImageUrl;
        product.ThumbnailUrl = image.ImageUrl;
        product.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:long}/images/{imageId:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteImage(long id, long imageId)
    {
        var image = await dbContext.ProductImages
            .FirstOrDefaultAsync(x => x.ProductImageId == imageId && x.ProductId == id);
        if (image == null)
        {
            return NotFound();
        }

        dbContext.ProductImages.Remove(image);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id:long}/specifications")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSpecifications(long id, [FromBody] List<ProductSpecificationUpsertDto> dto)
    {
        var productExists = await dbContext.Products.AnyAsync(x => x.ProductId == id);
        if (!productExists)
        {
            return NotFound();
        }

        var currentSpecifications = await dbContext.ProductSpecifications
            .Where(x => x.ProductId == id)
            .ToListAsync();
        dbContext.ProductSpecifications.RemoveRange(currentSpecifications);

        var newSpecifications = dto
            .Where(x => !string.IsNullOrWhiteSpace(x.SpecKey) && !string.IsNullOrWhiteSpace(x.SpecValue))
            .Select(x => new ProductSpecification
            {
                ProductId = id,
                GroupName = string.IsNullOrWhiteSpace(x.GroupName) ? "General" : x.GroupName.Trim(),
                SpecKey = x.SpecKey.Trim(),
                SpecValue = x.SpecValue.Trim(),
                SortOrder = x.SortOrder
            });

        dbContext.ProductSpecifications.AddRange(newSpecifications);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    // =========================
    // DELETE
    // =========================
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await dbContext.Products.FindAsync(id);

        if (entity == null)
        {
            return NotFound();
        }

        dbContext.Products.Remove(entity);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private static async Task<string?> SaveUploadedProductImageAsync(IFormFile? image)
    {
        if (image == null || image.Length == 0)
        {
            return null;
        }

        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "products");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await image.CopyToAsync(stream);

        return $"/uploads/products/{fileName}";
    }

    private async Task<string?> ResolveProductTypeFromCategoryAsync(int categoryId)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .ToListAsync();

        var category = categories.FirstOrDefault(x => x.CategoryId == categoryId);
        if (category == null)
        {
            return null;
        }

        var rootCategory = category;
        while (rootCategory.ParentCategoryId.HasValue)
        {
            var parent = categories.FirstOrDefault(x => x.CategoryId == rootCategory.ParentCategoryId.Value);
            if (parent == null)
            {
                break;
            }

            rootCategory = parent;
        }

        var normalizedRoot = NormalizeCategoryText($"{rootCategory.CategoryCode} {rootCategory.CategoryName}");

        if (normalizedRoot.Contains("LINHKIEN") || normalizedRoot.Contains("COMPONENT"))
        {
            return "Component";
        }

        if (normalizedRoot.Contains("MAYTINH") || normalizedRoot.Contains("COMPUTER") || normalizedRoot.Contains("LAPTOP") || normalizedRoot.Contains("PC"))
        {
            return "Computer";
        }

        return "Computer";
    }

    private static string NormalizeCategoryText(string value)
    {
        var normalized = value.Normalize(System.Text.NormalizationForm.FormD);
        var chars = normalized
            .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            .Where(char.IsLetterOrDigit)
            .Select(char.ToUpperInvariant);

        return new string(chars.ToArray());
    }
}
