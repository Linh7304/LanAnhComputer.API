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
public class UsersController(AppDbContext dbContext, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll([FromQuery] string? search = null)
    {
        var query = dbContext.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(x => x.FullName.Contains(keyword) || x.Email.Contains(keyword));
        }

        var users = await query
            .OrderBy(x => x.FullName)
            .ProjectTo<UserDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserDto>> GetById(long id)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null) return NotFound();
        return Ok(mapper.Map<UserDto>(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] UserUpsertDto dto)
    {
        if (await dbContext.Users.AnyAsync(x => x.Email == dto.Email))
            return BadRequest("Email already exists.");

        var entity = mapper.Map<User>(dto);
        dbContext.Users.Add(entity);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.UserId }, mapper.Map<UserDto>(entity));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UserUpsertDto dto)
    {
        var entity = await dbContext.Users.FindAsync(id);
        if (entity is null) return NotFound();

        mapper.Map(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null) return NotFound();

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id:long}/active")]
    public async Task<IActionResult> UpdateActive(long id, [FromBody] UpdateUserActiveDto dto)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.IsActive = dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id:long}/role")]
    public async Task<IActionResult> UpdateRole(long id, [FromBody] UpdateUserRoleDto dto)
    {
        var allowedRoles = new[] { "Admin", "Customer" };
        if (!allowedRoles.Contains(dto.Role))
        {
            return BadRequest("Invalid role.");
        }

        var user = await dbContext.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.Role = dto.Role;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
