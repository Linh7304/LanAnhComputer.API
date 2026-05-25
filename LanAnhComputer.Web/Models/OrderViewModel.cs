using System.Text.Json.Serialization;

namespace LanAnhComputer.Web.Models
{
    public class OrderViewModel
    {
        public long OrderId { get; set; }

        public string? CheckoutUrl { get; set; }

        public string? PaymentStatus { get; set; }
        public decimal TotalAmount { get; set; }
    }
    public class OrderItemViewModel  // danh sách đơn hàng của người dùng
    {
        public long OrderId { get; set; }
        public string OrderCode { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = "";
        public string PaymentStatus { get; set; } = "";
        public string PaymentMethod { get; set; } = "";
        [JsonPropertyName("orderDetails")]
        public List<OrderDetailViewModel> Items { get; set; } = new();
    }
}
