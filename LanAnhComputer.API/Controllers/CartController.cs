using AutoMapper;
using LanAnhComputer.API.Data.Entities;
using LanAnhComputer.API.Dtos;
using LanAnhComputer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace LanAnhComputer.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController(AppDbContext dbContext, IMapper mapper) : ControllerBase
    {
        // GET CART
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!long.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var cart = await dbContext.Carts
                .Include(x => x.CartItems)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart == null)
                return Ok(new List<object>());

            var result = cart.CartItems.Select(x => new  //(entity->dto ) 
            {
                productId = x.ProductId,
                productName = x.Product.ProductName,
                price = x.Product.SalePrice,
                quantity = x.Quantity,
                totalPrice = x.Product.SalePrice * x.Quantity,
                brand = x.Product.Brand,
                imageUrl = x.Product.ImageUrl
            });

            return Ok(result);

        }

        // ADD TO CART
        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartDto dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!long.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var cart = await dbContext.Carts
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId
                };

                dbContext.Carts.Add(cart);
                await dbContext.SaveChangesAsync();
            }

            var item = await dbContext.CartItems
                .FirstOrDefaultAsync(x =>
                    x.CartId == cart.CartId &&
                    x.ProductId == dto.ProductId);

            if (item != null)
            {
                item.Quantity += dto.Quantity;
                item.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var newItem = mapper.Map<CartItem>(dto); // (DTO -> entity) 

                newItem.CartId = cart.CartId;
                newItem.CreatedAt = DateTime.UtcNow;
                newItem.UpdatedAt = DateTime.UtcNow;

                dbContext.CartItems.Add(newItem);
            }

            await dbContext.SaveChangesAsync();
            Console.WriteLine(dto.ProductId);
            Console.WriteLine(dto.Quantity);
            return Ok(new
            {
                message = "Added to cart successfully"
            });
           
        }
        // UPDATE QUANTITY
        // =========================
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateQuantity(UpdateCartDto dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!long.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var cart = await dbContext.Carts
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart == null)
                return NotFound();

            var item = await dbContext.CartItems
                .FirstOrDefaultAsync(x =>
                    x.CartId == cart.CartId &&
                    x.ProductId == dto.ProductId);

            if (item == null)
                return NotFound();

            item.Quantity = dto.Quantity;
            item.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            return Ok(new { message = "Updated" });
        }
        // =========================
        // REMOVE ITEM
        // =========================
        [Authorize]
        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveItem(long productId)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!long.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var cart = await dbContext.Carts
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart == null)
                return NotFound();

            var item = await dbContext.CartItems
                .FirstOrDefaultAsync(x =>
                    x.CartId == cart.CartId &&
                    x.ProductId == productId);

            if (item == null)
                return NotFound();

            dbContext.CartItems.Remove(item);

            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Removed"
            });
        }


    }
}