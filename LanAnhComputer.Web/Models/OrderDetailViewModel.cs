using System.Text.Json.Serialization;

namespace LanAnhComputer.Web.Models
{
    public class OrderDetailViewModel
    {
        public long ProductId { get; set; }

        public string ProductName { get; set; }

        public string ImageUrl { get; set; }

        public int Quantity { get; set; }

        [JsonPropertyName("unitPrice")]
        public decimal Price { get; set; }
    }
}
