using LanAnhComputer.Data.Entities;

namespace LanAnhComputer.API.Data.Entities
{
    public class Cart
    {
        public long CartId { get; set; }
        public long UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public User User { get; set; } = null!;
        public List<CartItem> CartItems { get; set; } = new();
    }
}
