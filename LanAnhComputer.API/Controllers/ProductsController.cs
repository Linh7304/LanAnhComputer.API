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
    public async Task<ActionResult<ProductDto>> GetById(long id)
    {
        var entity = await dbContext.Products.FindAsync(id);

        if (entity == null)
            return NotFound();

        return Ok(mapper.Map<ProductDto>(entity));
    }

    // =========================
    // CREATE
    // =========================
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> Create(
        [FromForm] ProductUpsertDto dto)
    {
        var categoryExists = await dbContext.Categories
            .AnyAsync(x => x.CategoryId == dto.CategoryId);

        if (!categoryExists)
        {
            return BadRequest("Category does not exist.");
        }

        string? imageUrl = null;

        // =========================
        // UPLOAD IMAGE
        // =========================
        if (dto.Image != null)
        {
            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "products"
            );

            // tạo folder nếu chưa có
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // tạo tên file random
            var fileName =
                $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";

            var filePath = Path.Combine(
                uploadsFolder,
                fileName
            );

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(stream);
            }

            imageUrl = $"/uploads/products/{fileName}";
        }

        var entity = mapper.Map<Product>(dto);

        entity.ImageUrl = imageUrl;

        entity.CreatedAt = DateTime.UtcNow;

        dbContext.Products.Add(entity);

        await dbContext.SaveChangesAsync();

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

        mapper.Map(dto, entity);

        // =========================
        // UPLOAD NEW IMAGE
        // =========================
        if (dto.Image != null)
        {
            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "products"
            );

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName =
                $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";

            var filePath = Path.Combine(
                uploadsFolder,
                fileName
            );

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(stream);
            }

            entity.ImageUrl = $"/uploads/products/{fileName}";
        }

        entity.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    // =========================
    // DELETE
    // =========================
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(long id)
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
}