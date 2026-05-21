using LanAnhComputer.Data.Entities;

namespace LanAnhComputer.API.Data.Entities
{
    public class ProductReview
    {
        public int ProductReviewId { get; set; }

        public int ProductId { get; set; }

        public int UserId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public bool IsVisible { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Product Product { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
