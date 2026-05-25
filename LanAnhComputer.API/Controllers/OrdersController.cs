using AutoMapper;
using LanAnhComputer.API.Dtos;
using LanAnhComputer.Data;
using LanAnhComputer.Data.Entities;
using LanAnhComputer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LanAnhComputer.Services;

namespace LanAnhComputer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController(AppDbContext dbContext, IMapper mapper, IInventoryService inventoryService) : ControllerBase
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
            ShippingProvince = dto.ShippingProvince,
            Note = dto.Note
        };

        var details = new List<OrderDetail>();
        foreach (var detail in dto.Details)
        {
            var stockCheck = await inventoryService.ValidateStockAsync(detail.ProductId, detail.Quantity);
            if (!stockCheck.IsValid) return BadRequest(stockCheck.Error);

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

        await inventoryService.DeductStockAsync(details.Select(x => (x.ProductId, x.Quantity)));
        foreach (var detail in details)
        {
            var product = await dbContext.Products.FindAsync(detail.ProductId);
            if (product != null)
            {
                product.SoldQuantity += detail.Quantity;
            }
        }

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
        order.ShippingProvince = dto.ShippingProvince;
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

    [HttpPatch("{id:long}/status")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateOrderStatusDto dto)
    {
        var allowedStatuses = new[] { "Pending","Shipped", "Delivered", "Cancelled" };
        if (!allowedStatuses.Contains(dto.OrderStatus))
        {
            return BadRequest("Invalid order status.");
        }

        var order = await dbContext.Orders.FindAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        order.OrderStatus = dto.OrderStatus;
        // COD giao thành công => đã thanh toán
        if (
            order.PaymentMethod == "COD" &&
            dto.OrderStatus == "Delivered"
        )
        {
            order.PaymentStatus = "Paid";
        }
        order.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }
    [HttpPost("checkout")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<OrderDto>> Checkout([FromBody] CheckoutDtos dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!long.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        // Lấy Cart của user

        var cart = await dbContext.Carts
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart == null)
            return BadRequest("Cart not found");

        // Lấy CartItem + Product để kiểm tra tồn kho và tính giá
        var cartItems = await dbContext.CartItems
            .Where(x => x.CartId == cart.CartId)
            .Include(x => x.Product)
            .ToListAsync();

        if (cartItems.Count == 0)
            return BadRequest("Cart is empty");

        var stockCheck = await inventoryService.ValidateCartStockAsync(cart.CartId);
        if (!stockCheck.IsValid)
            return BadRequest(stockCheck.Error);
        //tạo order
        var order = new Order
        {
            // ❗ FIX: KHÔNG dùng string OrderCode nữa
            // OrderCode nên = OrderId sau khi save

            UserId = userId,
            OrderDate = DateTime.UtcNow,

            OrderStatus = "Pending",
            PaymentMethod = dto.PaymentMethod,
            PaymentStatus = "Pending",

            ShippingFullName = dto.ShippingFullName,
            ShippingPhone = dto.ShippingPhone,
            ShippingAddressLine = dto.ShippingAddressLine,
            ShippingWard = dto.ShippingWard,    
            ShippingProvince = dto.ShippingProvince,
            Note = dto.Note,

            OrderDetails = new List<OrderDetail>()
        };
        // Convert CartItems to OrderDetails
        foreach (var item in cartItems)
        {
            var unitPrice = item.Product.SalePrice;

            order.OrderDetails.Add(new OrderDetail
            {
                ProductId = item.ProductId,
                UnitPrice = unitPrice,
                Quantity = item.Quantity,
                DiscountPercent = 0,
                LineTotal = unitPrice * item.Quantity
            });
        }
        // Tính tiền sau khi có OrderDetails
        order.SubTotal = order.OrderDetails.Sum(x => x.LineTotal);
        order.DiscountAmount = dto.DiscountAmount;
        order.ShippingFee = dto.ShippingFee;
        order.TotalAmount = order.SubTotal - order.DiscountAmount + order.ShippingFee;

        // Save Order
        await inventoryService.DeductStockAsync( cartItems.Select(x => (x.ProductId, x.Quantity)));

   

        dbContext.Orders.Add(order);
      

        await dbContext.SaveChangesAsync();

        // ❗ FIX QUAN TRỌNG: set OrderCode = OrderId sau khi save
        order.OrderCode = order.OrderId.ToString();
        if (dto.PaymentMethod == "COD")
        {
            // tăng sold quantity
            foreach (var item in cartItems)
            {
                item.Product.SoldQuantity += item.Quantity;
            }

            // xoá cart
            dbContext.CartItems.RemoveRange(cartItems);
        }
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = order.OrderId },
            mapper.Map<OrderDto>(order)
        );
    }
    [HttpPost("cancel-expired")]
    public async Task<IActionResult> CancelExpiredOrders()
    {
        var expiredOrders = await dbContext.Orders
            .Include(x => x.OrderDetails)
            .Where(x =>
                x.PaymentStatus == "Pending" &&
                x.OrderDate < DateTime.UtcNow.AddMinutes(-15))
            .ToListAsync();

        foreach (var order in expiredOrders)
        {
            order.PaymentStatus = "Cancelled";
            order.OrderStatus = "Cancelled";

            // hoàn kho
            foreach (var detail in order.OrderDetails)
            {
                var product = await dbContext.Products
                    .FirstOrDefaultAsync(x => x.ProductId == detail.ProductId);

                if (product != null)
                {
                    product.StockQuantity += detail.Quantity;
                }
            }
        }

        await dbContext.SaveChangesAsync();

        return Ok();
    }
    [HttpGet("my")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> MyOrders()  // lấy đơn hàng của user
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!long.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var orders = await dbContext.Orders.Include(x => x.OrderDetails).ThenInclude(x => x.Product).Where(x => x.UserId == userId).OrderByDescending(x => x.OrderDate).ToListAsync();
        return Ok(mapper.Map<List<OrderDto>>(orders));
    }

}
