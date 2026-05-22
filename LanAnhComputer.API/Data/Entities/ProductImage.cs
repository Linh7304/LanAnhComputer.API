namespace LanAnhComputer.Data.Entities
{
    public class ProductImage
    {
        public long ProductImageId { get; set; }
        public long ProductId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string? AltText { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }

        public Product Product { get; set; } = null!;
    }
}
