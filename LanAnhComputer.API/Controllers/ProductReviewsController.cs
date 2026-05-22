using LanAnhComputer.API.Data.Entities;
using LanAnhComputer.Data;
using LanAnhComputer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LanAnhComputer.Controllers;

[ApiController]
[Route("api/products/{productId:long}/reviews")]
public class ProductReviewsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ProductReviewsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpsertReview(long productId, [FromBody] ProductReviewUpsertDto dto)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var productExists = await _dbContext.Products.AnyAsync(x => x.ProductId == productId && x.IsActive);
        if (!productExists)
        {
            return NotFound("Product not found.");
        }

        var review = await _dbContext.ProductReviews
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.UserId == userId);

        if (review == null)
        {
            review = new ProductReview
            {
                ProductId = (int)productId,
                UserId = (int)userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.ProductReviews.Add(review);
        }
        else
        {
            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.IsVisible = true;
            review.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        await RecalculateProductRatingAsync(productId);

        return Ok();
    }

    [HttpPatch("{reviewId:long}/visibility")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateVisibility(long productId, long reviewId, [FromBody] ProductReviewVisibilityDto dto)
    {
        var review = await _dbContext.ProductReviews
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.ProductReviewId == reviewId);

        if (review == null)
        {
            return NotFound();
        }

        review.IsVisible = dto.IsVisible;
        review.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        await RecalculateProductRatingAsync(productId);

        return NoContent();
    }

    [HttpDelete("{reviewId:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteReview(long productId, long reviewId)
    {
        var review = await _dbContext.ProductReviews
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.ProductReviewId == reviewId);

        if (review == null)
        {
            return NotFound();
        }

        _dbContext.ProductReviews.Remove(review);
        await _dbContext.SaveChangesAsync();
        await RecalculateProductRatingAsync(productId);

        return NoContent();
    }

    private async Task RecalculateProductRatingAsync(long productId)
    {
        var product = await _dbContext.Products.FindAsync(productId);
        if (product == null)
        {
            return;
        }

        var visibleReviews = _dbContext.ProductReviews
            .Where(x => x.ProductId == productId && x.IsVisible);

        product.TotalReviews = await visibleReviews.CountAsync();
        if (product.TotalReviews == 0)
        {
            product.AverageRating = 0;
        }
        else
        {
            var avgRating = await visibleReviews
                .AverageAsync(x => (double)x.Rating);

            product.AverageRating = Math.Round(avgRating, 2);
        }

        product.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }
}
