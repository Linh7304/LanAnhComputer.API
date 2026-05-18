namespace LanAnhComputer.Web.Models
{
    public class CheckoutViewModel
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public string Province { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string Address { get; set; }

        public string PaymentMethod { get; set; }

        public string Note { get; set; }
    }
}
