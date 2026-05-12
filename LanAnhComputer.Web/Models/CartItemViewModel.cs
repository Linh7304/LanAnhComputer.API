namespace LanAnhComputer.Web.ViewModels
{
    public class CartItemViewModel
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public decimal TotalPrice => Price * Quantity;
        public string? ProductType { get; set; }
        public string? Brand { get; set; }
    }
}
