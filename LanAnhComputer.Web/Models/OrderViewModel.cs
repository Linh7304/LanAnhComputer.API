namespace LanAnhComputer.Web.Models
{
    public class OrderViewModel
    {
        public long OrderId { get; set; }

        public string? CheckoutUrl { get; set; }

        public string? PaymentStatus { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
