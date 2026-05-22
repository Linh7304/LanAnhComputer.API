using LanAnhComputer.Data.Entities;

namespace LanAnhComputer.API.Data.Entities
{
    public class ProductReview
    {
        public long ProductReviewId { get; set; }

        public long ProductId { get; set; }

        public long UserId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public bool IsVisible { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Product Product { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
