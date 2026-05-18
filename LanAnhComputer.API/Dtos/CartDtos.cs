namespace LanAnhComputer.API.Dtos
{
    public class CartDto
    {
        public long ProductId { get; set; }
        public int Quantity { get; set; }
    }
    public class CartItemDto
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;

        public string ImageUrl { get; set; } = "";
        public string Brand { get; set; } = "";
        public string ProductType { get; set; } = "";
    }

        public class UpdateCartDto
        {
            public long ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
