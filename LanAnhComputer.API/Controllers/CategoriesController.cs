// C�c truy v?n ???c th?c hi?n th�ng qua Entity Framework Core (EF Core) ,  EF Core s? t? ??ng chuy?n c�c c�u l?nh C# n�y th�nh SQL t??ng ?ng.

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
[Authorize(Roles = "Admin")]
public class CategoriesController(AppDbContext dbContext, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        var result = await dbContext.Categories
            .OrderBy(x => x.CategoryName)
            .ProjectTo<CategoryDto>(mapper.ConfigurationProvider)
            .ToListAsync(); // SELECT * FROM Categories ORDER BY CategoryName
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var category = await dbContext.Categories.FindAsync(id); // SELECT * FROM Categories WHERE CategoryId = id
        if (category is null) return NotFound();
        return Ok(mapper.Map<CategoryDto>(category));
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryUpsertDto dto)
    {
        var entity = mapper.Map<Category>(dto);
        dbContext.Categories.Add(entity);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.CategoryId }, mapper.Map<CategoryDto>(entity));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryUpsertDto dto)
    {
        var entity = await dbContext.Categories.FindAsync(id);
        if (entity is null) return NotFound();

        mapper.Map(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await dbContext.Categories.FindAsync(id);
        if (entity is null) return NotFound();

        dbContext.Categories.Remove(entity);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
