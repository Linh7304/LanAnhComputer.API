namespace LanAnhComputer.Web.Models
{
    public class PayOSResponseViewModel
    {
        public long OrderId { get; set; }
        public string CheckoutUrl { get; set; } = "";
        public string QrCode { get; set; } = "";
        public long OrderCode { get; set; }
        public decimal Amount { get; set; }
    }
}
