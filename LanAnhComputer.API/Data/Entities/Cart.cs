using LanAnhComputer.Data.Entities;

namespace LanAnhComputer.API.Data.Entities
{
    public class Cart
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public User User { get; set; }
        public List<CartItem> CartItems { get; set; }
    }
}
