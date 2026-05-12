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
public class ProductsController(AppDbContext dbContext, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResultDto<ProductDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? productType = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = dbContext.Products.AsQueryable();
        if (categoryId.HasValue) query = query.Where(x => x.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(productType)) query = query.Where(x => x.ProductType == productType);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(x => x.ProductName.Contains(normalizedSearch));
        }

        var totalItems = await query.CountAsync();

        var result = await query
            .OrderBy(x => x.ProductId)
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

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductDto>> GetById(long id)
    {
        var entity = await dbContext.Products.FindAsync(id);
        if (entity is null) return NotFound();
        return Ok(mapper.Map<ProductDto>(entity));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> Create([FromBody] ProductUpsertDto dto)
    {
        var categoryExists = await dbContext.Categories.AnyAsync(x => x.CategoryId == dto.CategoryId);
        if (!categoryExists) return BadRequest("Category does not exist.");

        var entity = mapper.Map<Product>(dto);
        dbContext.Products.Add(entity);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.ProductId }, mapper.Map<ProductDto>(entity));
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(long id, [FromBody] ProductUpsertDto dto)
    {
        var entity = await dbContext.Products.FindAsync(id);
        if (entity is null) return NotFound();

        mapper.Map(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await dbContext.Products.FindAsync(id);
        if (entity is null) return NotFound();

        dbContext.Products.Remove(entity);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
