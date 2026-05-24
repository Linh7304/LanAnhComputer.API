namespace LanAnhComputer.Web.Models
{
    public class PayOSResponseViewModel
    {
        public long OrderId { get; set; }
        public string CheckoutUrl { get; set; } = "";
        public string QrCode { get; set; } = "";
        public long OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string Bin { get; set; } = "";

        public string AccountNumber { get; set; } = "";

        public string AccountName { get; set; } = "";

        public string Description { get; set; } = "";
    }
}
