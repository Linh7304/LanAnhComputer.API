using AutoMapper;
using System.Security.Claims;
using LanAnhComputer.Data;
using LanAnhComputer.Data.Entities;
using LanAnhComputer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LanAnhComputer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController(AppDbContext dbContext, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        var orders = await dbContext.Orders
            .Include(x => x.OrderDetails)
            .OrderByDescending(x => x.OrderDate)
            .ToListAsync();

        return Ok(mapper.Map<List<OrderDto>>(orders));
    }

    [HttpGet("{id:long}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<ActionResult<OrderDto>> GetById(long id)
    {
        var order = await dbContext.Orders
            .Include(x => x.OrderDetails)
            .FirstOrDefaultAsync(x => x.OrderId == id);

        if (order is null) return NotFound();

        var isCustomer = User.IsInRole("Customer");
        if (isCustomer)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdClaim, out var currentUserId) || order.UserId != currentUserId)
                return Forbid();
        }

        return Ok(mapper.Map<OrderDto>(order));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<ActionResult<OrderDto>> Create([FromBody] OrderUpsertDto dto)
    {
        var isCustomer = User.IsInRole("Customer");
        if (isCustomer)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdClaim, out var currentUserId) || dto.UserId != currentUserId)
                return Forbid();
        }

        var userExists = await dbContext.Users.AnyAsync(x => x.UserId == dto.UserId);
        if (!userExists) return BadRequest("User does not exist.");

        if (dto.Details.Count == 0) return BadRequest("Order must have at least 1 detail.");

        var order = new Order
        {
            OrderCode = dto.OrderCode,
            UserId = dto.UserId,
            OrderDate = DateTime.UtcNow,
            OrderStatus = dto.OrderStatus,
            PaymentMethod = dto.PaymentMethod,
            PaymentStatus = dto.PaymentStatus,
            ShippingFullName = dto.ShippingFullName,
            ShippingPhone = dto.ShippingPhone,
            ShippingAddressLine = dto.ShippingAddressLine,
            ShippingWard = dto.ShippingWard,
            ShippingDistrict = dto.ShippingDistrict,
            ShippingCity = dto.ShippingCity,
            Note = dto.Note
        };

        var details = new List<OrderDetail>();
        foreach (var detail in dto.Details)
        {
            var productExists = await dbContext.Products.AnyAsync(x => x.ProductId == detail.ProductId);
            if (!productExists) return BadRequest($"Product {detail.ProductId} does not exist.");

            var lineTotal = detail.UnitPrice * detail.Quantity * (1 - detail.DiscountPercent / 100m);
            details.Add(new OrderDetail
            {
                ProductId = detail.ProductId,
                UnitPrice = detail.UnitPrice,
                Quantity = detail.Quantity,
                DiscountPercent = detail.DiscountPercent,
                LineTotal = lineTotal
            });
        }

        order.SubTotal = details.Sum(x => x.LineTotal);
        order.DiscountAmount = dto.DiscountAmount;
        order.ShippingFee = dto.ShippingFee;
        order.TotalAmount = order.SubTotal - order.DiscountAmount + order.ShippingFee;
        order.OrderDetails = details;

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, mapper.Map<OrderDto>(order));
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(long id, [FromBody] OrderUpsertDto dto)
    {
        var order = await dbContext.Orders
            .Include(x => x.OrderDetails)
            .FirstOrDefaultAsync(x => x.OrderId == id);

        if (order is null) return NotFound();
        if (dto.Details.Count == 0) return BadRequest("Order must have at least 1 detail.");

        order.OrderCode = dto.OrderCode;
        order.UserId = dto.UserId;
        order.OrderStatus = dto.OrderStatus;
        order.PaymentMethod = dto.PaymentMethod;
        order.PaymentStatus = dto.PaymentStatus;
        order.ShippingFullName = dto.ShippingFullName;
        order.ShippingPhone = dto.ShippingPhone;
        order.ShippingAddressLine = dto.ShippingAddressLine;
        order.ShippingWard = dto.ShippingWard;
        order.ShippingDistrict = dto.ShippingDistrict;
        order.ShippingCity = dto.ShippingCity;
        order.Note = dto.Note;
        order.UpdatedAt = DateTime.UtcNow;

        dbContext.OrderDetails.RemoveRange(order.OrderDetails);
        var details = dto.Details.Select(x => new OrderDetail
        {
            ProductId = x.ProductId,
            UnitPrice = x.UnitPrice,
            Quantity = x.Quantity,
            DiscountPercent = x.DiscountPercent,
            LineTotal = x.UnitPrice * x.Quantity * (1 - x.DiscountPercent / 100m)
        }).ToList();

        order.OrderDetails = details;
        order.SubTotal = details.Sum(x => x.LineTotal);
        order.DiscountAmount = dto.DiscountAmount;
        order.ShippingFee = dto.ShippingFee;
        order.TotalAmount = order.SubTotal - order.DiscountAmount + order.ShippingFee;

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(long id)
    {
        var order = await dbContext.Orders.FindAsync(id);
        if (order is null) return NotFound();

        dbContext.Orders.Remove(order);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
