using AutoMapper;
using LanAnhComputer.API.Data.Entities;
using LanAnhComputer.API.Dtos;
using LanAnhComputer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace LanAnhComputer.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController(AppDbContext dbContext, IMapper mapper) : ControllerBase
    {
        // GET CART
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(long userId)
        {
            var cart = await dbContext.Carts
                .Include(x => x.CartItems)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart == null)
                return Ok(new List<CartItem>());

            var result = cart.CartItems.Select(x => new
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
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(CartDto dto)
        {
            var cart = await dbContext.Carts
                .FirstOrDefaultAsync(x => x.UserId == dto.UserId);

            if (cart == null)
            {
                cart = new Cart { UserId = dto.UserId };
                dbContext.Carts.Add(cart);
                await dbContext.SaveChangesAsync();
            }

            var item = await dbContext.CartItems
                .FirstOrDefaultAsync(x => x.CartId == cart.CartId && x.ProductId == dto.ProductId);

            if (item != null)
            {
                item.Quantity += dto.Quantity;
                item.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var newItem = mapper.Map<CartItem>(dto);

                newItem.CartId = cart.CartId;
                newItem.CreatedAt = DateTime.UtcNow; // thêm nếu entity có
                newItem.UpdatedAt = DateTime.UtcNow;

                dbContext.CartItems.Add(newItem);
            }

            await dbContext.SaveChangesAsync();

            return Ok(new { message = "Added to cart successfully" });
        }
        // UPDATE QUANTITY
        // =========================
        [HttpPut("update")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartDto dto)
        {
            var item = await dbContext.CartItems
                .FirstOrDefaultAsync(x => x.ProductId == dto.ProductId);

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
        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveItem(long productId)
        {
            var item = await dbContext.CartItems
                .FirstOrDefaultAsync(x => x.ProductId == productId);

            if (item == null)
                return NotFound();

            dbContext.CartItems.Remove(item);
            await dbContext.SaveChangesAsync();

            return Ok(new { message = "Removed" });
        }


    }
}