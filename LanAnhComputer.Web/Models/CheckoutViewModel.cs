using LanAnhComputer.Web.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace LanAnhComputer.Web.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, ErrorMessage = "Họ và tên không được quá 100 ký tự")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số")]
        public string Phone { get; set; } = "";

        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        public string Email { get; set; } = "";

        public string Province { get; set; } = "";


        public string Ward { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ cụ thể")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được quá 255 ký tự")]
        public string Address { get; set; } = "";

        public string PaymentMethod { get; set; } = "COD";

        public string Note { get; set; } = "";

        public List<CartItemViewModel> CartItems { get; set; } = new();
        public decimal SubTotal => CartItems.Sum(x => x.TotalPrice);
    }
}
