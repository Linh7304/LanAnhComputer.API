using System.ComponentModel.DataAnnotations;

namespace LanAnhComputer.Dtos;

public class OrderDetailDto  // chi tiết đơn hàng
{
    public long OrderDetailId { get; set; }
    public long ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal LineTotal { get; set; }
    public string ProductName { get; set; } = "";
    public string? ImageUrl { get; set; }

}

public class OrderDetailUpsertDto // chi tiết đơn hàng khi tạo hoặc cập nhật
{
    [Range(1, long.MaxValue)]
    public long ProductId { get; set; }
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
    public int Quantity { get; set; }
    [Range(0, 100)]
    public decimal DiscountPercent { get; set; }
}

public class OrderDto
{
    public long OrderId { get; set; }
    public string OrderCode { get; set; } = null!;
    public long UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public string OrderStatus { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
    public string PaymentStatus { get; set; } = null!;
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
    public List<OrderDetailDto> OrderDetails { get; set; } = [];
    public string? CheckoutUrl { get; set; }


}

public class OrderUpsertDto // dữ liệu để tạo hoặc cập nhật đơn hàng
{
    [Required]
    [MaxLength(30)]
    public string OrderCode { get; set; } = null!;
    [Range(1, long.MaxValue)]
    public long UserId { get; set; }
    [Required]
    public string OrderStatus { get; set; } = "Pending";
    [Required]
    public string PaymentMethod { get; set; } = "COD";
    [Required]
    public string PaymentStatus { get; set; } = "Unpaid";
    [Required]
    [MaxLength(150)]
    public string ShippingFullName { get; set; } = null!;
    [Required]
    [RegularExpression(@"^\d+$", ErrorMessage = "ShippingPhone must contain digits only.")]
    public string ShippingPhone { get; set; } = null!;
    [Required]
    [MaxLength(255)]
    public string ShippingAddressLine { get; set; } = null!;
    public string? ShippingWard { get; set; }
    [Required]
    [MaxLength(100)]
    public string ShippingProvince { get; set; } = null!;
    public string? Note { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingFee { get; set; }
    [MinLength(1, ErrorMessage = "Order must have at least 1 detail.")]
    public List<OrderDetailUpsertDto> Details { get; set; } = [];
}

public class UpdateOrderStatusDto
{
    [Required]
    public string OrderStatus { get; set; } = null!;
}
