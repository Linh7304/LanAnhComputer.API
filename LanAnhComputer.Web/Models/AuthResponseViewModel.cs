namespace LanAnhComputer.Web.Models
{
    //hứng và lưu trữ thông tin kết quả trả về từ Backend API sau khi người dùng đăng nhập thành công.
    public class AuthResponseViewModel
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string Role { get; set; } = string.Empty;
        public long UserId { get; set; }
        public string FullName { get; set; } 
    }
}
