namespace LanAnhComputer.API.Dtos
{
    public class CheckoutDtos
    {
        public string PaymentMethod { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }

        public string ShippingFullName { get; set; }
        public string ShippingPhone { get; set; }
        public string ShippingAddressLine { get; set; }
        public string ShippingWard { get; set; }
        public string ShippingProvince { get; set; }
        public string? Note { get; set; }
    }
}
