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
public class ChatbotHistoriesController(AppDbContext dbContext, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatbotHistoryDto>>> GetAll([FromQuery] string? sessionId)
    {
        var query = dbContext.ChatbotHistories.AsQueryable();
        if (!string.IsNullOrWhiteSpace(sessionId))
            query = query.Where(x => x.SessionId == sessionId);

        var rows = await query
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<ChatbotHistoryDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(rows);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ChatbotHistoryDto>> GetById(long id)
    {
        var row = await dbContext.ChatbotHistories.FindAsync(id);
        if (row is null) return NotFound();
        return Ok(mapper.Map<ChatbotHistoryDto>(row));
    }

    [HttpPost]
    public async Task<ActionResult<ChatbotHistoryDto>> Create([FromBody] ChatbotHistoryCreateDto dto)
    {
        var row = mapper.Map<ChatbotHistory>(dto);
        row.CreatedAt = DateTime.UtcNow;

        dbContext.ChatbotHistories.Add(row);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = row.ChatHistoryId }, mapper.Map<ChatbotHistoryDto>(row));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var row = await dbContext.ChatbotHistories.FindAsync(id);
        if (row is null) return NotFound();

        dbContext.ChatbotHistories.Remove(row);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
