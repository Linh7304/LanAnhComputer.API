namespace LanAnhComputer.Web.Models
{
    public class RegisterResponseViewModel
    {
        public bool Success { get; set; }
        public string? EmailError { get; set; }

       public string? PhoneError { get; set; }
        public AuthResponseViewModel? User { get; set; }
    }
}
