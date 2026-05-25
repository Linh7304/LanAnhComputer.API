namespace LanAnhComputer.Data.Entities
{
    public class Order
    {
        public long OrderId { get; set; }
        public string OrderCode { get; set; } = null!;
        public long UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; } = "Pending";
        public string PaymentMethod { get; set; } = "COD";
        public string PaymentStatus { get; set; } = "Pending";

        public string ShippingFullName { get; set; } = null!;
        public string ShippingPhone { get; set; } = null!;
        public string ShippingAddressLine { get; set; } = null!;
        public string? ShippingWard { get; set; }
        public string ShippingProvince { get; set; } = null!;
        public string? Note { get; set; }

        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public long? PayOSOrderCode { get; set; }

        public string? PaymentLinkId { get; set; }

        public string? CheckoutUrl { get; set; }

        public DateTime? PaidAt { get; set; }

        public string? TransactionId { get; set; }


        public User User { get; set; } = null!;
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    }
}
